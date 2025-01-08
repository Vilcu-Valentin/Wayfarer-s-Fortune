using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Rendering;

[System.Serializable]
public struct PriceData
{
    public float basePricePerUnit;
    public float pricePerUnit;
    public float eventsMultiplier;
    public float todaysMultiplier;
    public float tomorrowsMultiplier;

    public PriceData(float basePricePerUnit, float pricePerUnit=0, float eventsMultiplier = 1, float todaysMultiplier = 1, float tomorrowsMultiplier = 1)
    {
        this.basePricePerUnit = basePricePerUnit;
        this.pricePerUnit = pricePerUnit;
        this.eventsMultiplier = eventsMultiplier;
        this.todaysMultiplier = todaysMultiplier;
        this.tomorrowsMultiplier = tomorrowsMultiplier;
    }
}

[System.Serializable]
public struct ActiveEventData
{
    public static readonly float minRandMod = 0.8f;
    public static readonly float maxRandMod = 1.2f;

    public int startDay;
    public int startHour;
    public float randModifier;
    public bool visible;

    public ActiveEventData(int startDay, int startHour=0)
    {
        this.startDay = startDay;
        this.startHour = startHour;
        randModifier = Random.Range(minRandMod, maxRandMod);
        visible = false;
    }
}

public struct OutputPriceData
{
    public ItemData itemData;
    public float minPrice;
    public float maxPrice;

    public OutputPriceData(ItemData itemData, float minPrice, float maxPrice)
    {
        this.itemData = itemData;
        this.minPrice = minPrice;
        this.maxPrice = maxPrice;
    }
}

public class Settlement : MonoBehaviour
{
    public SettlementData data;
    [SerializeField] private SerializableDictionary<ItemData, MutableHolder<PriceData>> pricesData = null;
    [SerializeField] private SerializableDictionary<EventData, MutableHolder<ActiveEventData>> activeEventsData = null;
    [SerializeField] private SerializableDictionary<EventData, MutableHolder<ActiveEventData>> dyingEventsData = null;

    [SerializeField]
    private int currentDay;
    [SerializeField]
    private int currentHour;

    private GlobalEventManager globalEventManager = null;
    private MapMaster mapMaster = null;

    public bool permanentPerfectPriceInfoFlag { get; private set; } = false;
    public bool perfectPriceInfoFlag { get; private set; } = false;
    public int perfectPriceInfoEndDay { get; private set; }
    public int perfectPriceInfoEndHour { get; private set; }

    public bool permanentPerfectEventInfoFlag { get; private set; } = false;
    public bool perfectEventInfoFlag { get; private set; }  = false;
    public int perfectEventInfoEndDay { get; private set; }
    public int perfectEventInfoEndHour { get; private set; }

    private void Start()
    {
        // Initialize late to get data from MapMaster
        // Initialize the dictionary of item prices
        pricesData = new SerializableDictionary<ItemData, MutableHolder<PriceData>>();
        for (int i = 0; i < data.goodsPool.Count; i++)
            pricesData.Add(data.goodsPool[i].itemData, new MutableHolder<PriceData>(new PriceData(data.goodsPool[i].basePricePerUnit)));

        // Initialize the dictionary of active events and dead events
        activeEventsData = new SerializableDictionary<EventData, MutableHolder<ActiveEventData>>();
        dyingEventsData = new SerializableDictionary<EventData, MutableHolder<ActiveEventData>>();

        // Find the TimeMaster script, register the callback and set the current time
        TimeMaster.Instance().timeChangedEvent += updateTime;
        updateTime(TimeMaster.Instance().day, TimeMaster.Instance().hour);

        // Late Global Event Manager search
        globalEventManager = GameObject.Find("GlobalEventManager").GetComponent<GlobalEventManager>();
        globalEventManager.registerSettlement(this);

        // Late MapMaster search
        mapMaster = GameObject.Find("MapMaster").GetComponent<MapMaster>();

        transform.name = data.name;
    }

    private void OnDestroy()
    {
        globalEventManager.unregisterSettlement(this);
    }

    private void updateTime(int newDay, int newHour)
    {
        if (newDay != currentDay)
        {
            // Update random multipliers
            if (newDay == currentDay + 1)
            {
                foreach (ItemMarketProfile imp in data.goodsPool)
                {
                    pricesData[imp.itemData].value.todaysMultiplier = pricesData[imp.itemData].value.tomorrowsMultiplier;
                    pricesData[imp.itemData].value.tomorrowsMultiplier = Random.Range(1f-imp.volatility, 1f+imp.volatility);
                }
            }
            else
            {
                foreach (ItemMarketProfile imp in data.goodsPool)
                {
                    pricesData[imp.itemData].value.todaysMultiplier = Random.Range(1f - imp.volatility, 1f + imp.volatility);
                    pricesData[imp.itemData].value.tomorrowsMultiplier = Random.Range(1f - imp.volatility, 1f + imp.volatility);
                }
            }
        }

        // Update active local events
        // Mark dead events for removal
        List<EventData> deadEvents = new List<EventData>();
        foreach( EventData eventData in dyingEventsData.Keys )
        {
            int startDay = dyingEventsData[eventData].value.startDay;
            int startHour = dyingEventsData[eventData].value.startHour;
            if ((newDay - startDay) * 24 + (newHour - startHour) >= eventData.leadTime + eventData.duration + eventData.deathTime)
                deadEvents.Add(eventData);
        }
        // Remove dead events
        foreach( EventData eventData in deadEvents )
            dyingEventsData.Remove(eventData);
        // Mark finished events for dying stage
        List<EventData> eventsToKill = new List<EventData>();
        foreach (EventData eventData in activeEventsData.Keys)
        {
            int startDay = activeEventsData[eventData].value.startDay;
            int startHour = activeEventsData[eventData].value.startHour;
            if (eventData.leadTime + eventData.duration <= (newDay - startDay) * 24 + newHour - startHour)
                eventsToKill.Add(eventData);
        }
        // Move finished events to dying dict
        foreach (EventData eventData in eventsToKill)
        {
            dyingEventsData.Add(eventData, new MutableHolder<ActiveEventData>(activeEventsData[eventData].value));
            activeEventsData.Remove(eventData);
        }
        // Roll for new local events
        if (newDay != currentDay)
        {
            
            foreach (EventSettlementContext eventContext in data.localEvents)
            {
                if (activeEventsData.ContainsKey(eventContext.eventData))
                    continue;
                float chance = eventContext.frequency / TimeMaster.daysPerYear //* ((newDay - currentDay) + (newHour - currentHour) / 24f)
                                * eventContext.eventData.seasonalModifiers[(int)TimeMaster.Instance().getCurrentSeason()];
                if (Random.Range(0f, 1f) < chance)
                    activeEventsData.Add(eventContext.eventData, new MutableHolder<ActiveEventData>(new ActiveEventData(newDay)));
            }
        }
        // Roll for new seasonal events
        if (TimeMaster.Instance().getCurrentSeason() != TimeMaster.getSeason(currentDay))
        {
            foreach (EventData seasonalEvent in data.seasonalEvents)
            {
                float chance = seasonalEvent.seasonalModifiers[(int)TimeMaster.Instance().getCurrentSeason()];
                if (Random.Range(0f, 1f) < chance)
                    activeEventsData.Add(seasonalEvent, new MutableHolder<ActiveEventData>(new ActiveEventData(newDay)));
            }
        }

        // Reset event multipliers
        foreach (ItemData item in pricesData.Keys)
            pricesData[item].value.eventsMultiplier = 1f;
        // Update event multipliers from active events
        foreach (EventData eventData in activeEventsData.Keys)
        {
            foreach (EventEffectData eventEffect in eventData.affectedGoods)
                if (pricesData.ContainsKey(eventEffect.item))
                {
                    float multiplier = 5318008;
                    int startDay = activeEventsData[eventData].value.startDay;
                    int startHour = activeEventsData[eventData].value.startHour;
                    int activeTime = (newDay - startDay) * 24 + newHour - startHour;
                    if (activeTime < eventData.leadTime) // lead phase
                        multiplier = ((float)activeTime / eventData.leadTime 
                                        * Mathf.Pow(2.05f, 0.3f * eventEffect.strength) * activeEventsData[eventData].value.randModifier
                                        + ((float)eventData.leadTime - activeTime) / eventData.leadTime) * Random.Range(0.95f, 1.05f);
                    else // active phase
                        multiplier = Mathf.Pow(2.05f, 0.3f * eventEffect.strength) * activeEventsData[eventData].value.randModifier;

                    pricesData[eventEffect.item].value.eventsMultiplier *= multiplier;
                }
        }
        // Update event multipliers from dying events
        foreach (EventData eventData in dyingEventsData.Keys)
        {
            if( eventData.deathTime > 0)
                foreach (EventEffectData eventEffect in eventData.affectedGoods)
                    if (pricesData.ContainsKey(eventEffect.item))
                    {
                        int startDay = dyingEventsData[eventData].value.startDay;
                        int startHour = dyingEventsData[eventData].value.startHour;
                        int dyingTime = (newDay - startDay) * 24 + newHour - startHour - eventData.leadTime - eventData.duration;
                        pricesData[eventEffect.item].value.eventsMultiplier
                                *= ((float)eventData.deathTime - dyingTime) / eventData.deathTime
                                    * Mathf.Pow(2.05f, 0.3f * eventEffect.strength) * dyingEventsData[eventData].value.randModifier
                                    + (float)dyingTime / eventData.deathTime;
                    }
        }


        // Update perfect price info flag
        if (perfectPriceInfoFlag && (newDay > perfectPriceInfoEndDay || (newDay == perfectPriceInfoEndDay && newHour >= perfectPriceInfoEndHour)))
            perfectPriceInfoFlag = false;

        // Update perfect event info flag
        if(perfectEventInfoFlag && (newDay > perfectEventInfoEndDay || (newDay == perfectEventInfoEndDay && newHour >= perfectEventInfoEndHour)) )
            perfectEventInfoFlag = false;

        // Update day and hour
        currentDay = newDay;
        currentHour = newHour;

        // Update prices
        foreach (ItemData item in pricesData.Keys)
        {
            pricesData[item].value.pricePerUnit = ((24 - currentHour) / 24f * pricesData[item].value.todaysMultiplier + 
                                            currentHour / 24f * pricesData[item].value.tomorrowsMultiplier)
                                           * pricesData[item].value.eventsMultiplier * pricesData[item].value.basePricePerUnit;
        }
    }

    public void activateGolbalEvent(EventData globalEvent)
    {
        if (activeEventsData.ContainsKey(globalEvent))
            return;

        bool valid = false;
        foreach (EventData validEvent in data.globalEvents)
            if (validEvent.Equals(globalEvent))
            {
                valid = true;
                break;
            }
        if (!valid) return;

        activeEventsData.Add(globalEvent, 
            new MutableHolder<ActiveEventData>(new ActiveEventData(TimeMaster.Instance().day)));
    }
    

    public void setPerfectInfo(int duration)
    {
        if (duration < 0) return; // Invalid input.

        if (permanentPerfectPriceInfoFlag) return;
        if (duration == 0) { permanentPerfectPriceInfoFlag = true; return; }

        /// It is a bit unclear how this should behave when the flag is already set. For now, it will just add the duration.
        if(!perfectPriceInfoFlag)
        {
            perfectPriceInfoFlag = true;
            perfectPriceInfoEndDay = TimeMaster.Instance().day;
            perfectPriceInfoEndHour = TimeMaster.Instance().hour;
        }
        perfectPriceInfoEndDay += duration / 24 + (perfectPriceInfoEndHour + duration % 24) / 24;
        perfectPriceInfoEndHour = (perfectPriceInfoEndHour + duration % 24) % 24;
    }

    public List<OutputPriceData> getPrices()
    {
        List<OutputPriceData> outPrices = new List<OutputPriceData>();
        int playerDistance = mapMaster.GetPlayerDistanceTo(data);
        Debug.Log("The player is here: " + mapMaster.playerLocation + "Wants to go here: " + data + " with distance: " + playerDistance);
        foreach( ItemData item in pricesData.Keys )
        {
            if (permanentPerfectPriceInfoFlag || perfectPriceInfoFlag)
                outPrices.Add(new OutputPriceData(item, pricesData[item].value.pricePerUnit, pricesData[item].value.pricePerUnit));
            else
            {
                (float, float) bounds = getPriceRange(pricesData[item].value.pricePerUnit, playerDistance);
                outPrices.Add(new OutputPriceData(item, bounds.Item1, bounds.Item2));
            }
        }
        return outPrices;
    }

    /// <summary>
    /// Applies the price range calculations and renturns a lower and upper bound tuple. If and only if the range_size is over 40, returns (-1, -1).
    /// </summary>
    /// <param name="truePrice"></param>
    /// <param name="playerDistance"></param>
    /// <returns></returns>
    private (float, float) getPriceRange(float truePrice, int playerDistance)
    {
        float scale_factor = Mathf.Pow((float)playerDistance, 2.5f) / (PlayerMaster.Instance().currentLvl*0.6f);
        float range_size = scale_factor * Mathf.Sqrt(truePrice);
        if (scale_factor > 40) 
            return (-1, -1);
        
        float half_range = range_size / 2;
        float random_offset = Random.Range(-half_range, half_range);
        
        float lower_bound = Mathf.Max(0, truePrice-half_range+random_offset);
        float upper_bound = truePrice + half_range + random_offset;
        return (lower_bound, upper_bound);
    }


    public Dictionary<EventData, ActiveEventData> getVisibleEvents()
    {
        Dictionary<EventData, ActiveEventData> visibles = new Dictionary<EventData, ActiveEventData>();
        foreach( EventData eventData in activeEventsData.Keys )
        {
            if (permanentPerfectEventInfoFlag || perfectEventInfoFlag || activeEventsData[eventData].value.visible)
                visibles.Add(eventData, activeEventsData[eventData].value);
        }
        return visibles;
    }


    public void makeEventVisible(EventData eventData = null)
    {
        if( eventData == null )
        {
            // Pick a random active event to make visible.
            eventData = activeEventsData.ElementAt(Random.Range((int)0, activeEventsData.Count)).Key;
            activeEventsData[eventData].value.visible = true;
        }
        else
        {
            if (activeEventsData.ContainsKey(eventData))
                activeEventsData[eventData].value.visible = true;
        }
    }

    public void setPerfectEventDataInfo(int duration)
    {
        if (duration < 0) return; // Invalid data

        if (permanentPerfectEventInfoFlag) return;
        if ( duration == 0 ) { permanentPerfectEventInfoFlag = true; return; }

        /// It is a bit unclear how this should behave when the flag is already set. For now, it will just add the duration.
        if (!perfectEventInfoFlag)
        {
            perfectEventInfoFlag = true;
            perfectEventInfoEndDay = TimeMaster.Instance().day;
            perfectEventInfoEndHour = TimeMaster.Instance().hour;
        }
        perfectEventInfoEndDay += duration / 24 + (perfectEventInfoEndHour + duration % 24) / 24;
        perfectEventInfoEndHour = (perfectEventInfoEndHour + duration % 24) % 24;
    }
}
