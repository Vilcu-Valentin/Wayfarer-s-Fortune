using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalEventManager : MonoBehaviour
{
    [SerializeField] private List<ZoneData> zonesData;

    private Dictionary<SettlementData, Settlement> settlementObjDict = null;

    private int currentDay;
    private int currentHour;

    private void Awake()
    {
        settlementObjDict = new Dictionary<SettlementData, Settlement>();
        TimeMaster.Instance().timeChangedEvent += updateTime;
        updateTime(TimeMaster.Instance().day, TimeMaster.Instance().hour);
    }

    private void updateTime(int newDay, int newHour)
    {
        if (newDay != currentDay)
        {
            foreach (ZoneData zone in zonesData)
            {
                foreach (EventSettlementContext eventContext in zone.eventContexts)
                {
                    float chance = eventContext.frequency / TimeMaster.daysPerYear //* ((newDay - currentDay) + (newHour - currentHour) / 24f)
                                    * eventContext.eventData.seasonalModifiers[(int)TimeMaster.Instance().getCurrentSeason()];
                    if (Random.Range(0f, 1f) < chance)
                    {
                        foreach (SettlementData settlementData in zone.settlements)
                            if (settlementObjDict.ContainsKey(settlementData))
                            {
                                settlementObjDict[settlementData].activateGolbalEvent(eventContext.eventData);
                            }
                        Debug.Log(eventContext.eventData.name + " has started in zone " + zone.name);
                    }

                    Debug.Log("Rolling event for " + zone.name);
                }
            }
        }

        currentDay = newDay;
        currentHour = newHour;
    }

    public void registerSettlement(Settlement settlement)
    {
        if (settlementObjDict.ContainsKey(settlement.data))
            return;
        settlementObjDict.Add(settlement.data, settlement);
        Debug.Log(settlement + " has been registered in the GlobalEventManager");
    }

    public void unregisterSettlement(Settlement settlement) { settlementObjDict.Remove(settlement.data); }
}
