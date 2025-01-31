using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class TravelEvent_UI : MonoBehaviour
{
    [Header("References")]
    public Image icon;
    public TextMeshProUGUI title;
    public TextMeshProUGUI description;
    public GameObject effectsSpawn;

    public GameObject effectsPrefab;

    [Header("Icons")]
    public Sprite currency;
    public Sprite time;
    public Sprite nothingHappened;

    public void Initialize(TravelEvents tEvent, List<EffectResult> effects)
    {
        icon.sprite = tEvent.icon;
        title.text = tEvent.event_name;
        description.text = tEvent.description;

        foreach (var result in effects)
        {
            if (result.EffectType == typeof(ItemEffect))
            {
                List<Item> itemsAffected = (List<Item>)result.Value;
                foreach (var item in itemsAffected)
                {
                    ItemEffect_UI spawnedResult = Instantiate(effectsPrefab, effectsSpawn.transform).GetComponent<ItemEffect_UI>();
                    if (result.IsAddingItems) // Use the IsAddingItems property
                        spawnedResult.Initialize(item.ItemData.icon, item.Count, false);
                    else
                        spawnedResult.Initialize(item.ItemData.icon, -item.Count, false);
                }
            }
            else if (result.EffectType == typeof(CoinEffect))
            {
                ItemEffect_UI spawnedResult = Instantiate(effectsPrefab, effectsSpawn.transform).GetComponent<ItemEffect_UI>();
                float coinValue = (float)result.Value;
                spawnedResult.Initialize(currency, (int)coinValue, false);
            }
            else if (result.EffectType == typeof(DurationEffect))
            {
                ItemEffect_UI spawnedResult = Instantiate(effectsPrefab, effectsSpawn.transform).GetComponent<ItemEffect_UI>();
                int durationValue = (int)result.Value;
                spawnedResult.Initialize(time, durationValue, true);
            }
            else if (result.EffectType == typeof(NoEffect))
            {
                ItemEffect_UI spawnedResult = Instantiate(effectsPrefab, effectsSpawn.transform).GetComponent<ItemEffect_UI>();
                bool noEffectValue = (bool)result.Value;
                spawnedResult.Initialize(nothingHappened, 0, false);
                Destroy(this.gameObject);
            }
        }
    }

}
