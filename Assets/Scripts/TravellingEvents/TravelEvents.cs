using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class EffectResult
{
    public Type EffectType { get; set; }
    public object Value { get; set; }
    public bool IsAddingItems { get; set; } // Additional field for ItemEffect
}


[CreateAssetMenu(fileName = "New Travel Event", menuName = "TradeSystem/Travel Events/Travel Event")]
public class TravelEvents : ScriptableObject
{
    public Sprite icon;
    public string event_name;
    [TextArea] public string description;
    [Tooltip("This is used to calculate how often it happens, lower numbers mean it happens less time")]
    public int ticket;

    [SerializeReference] // Enables polymorphic serialization
    public List<EventEffect> effects = new List<EventEffect>();

    public List<EffectResult> TriggerEvent()
    {
        List<EffectResult> results = new List<EffectResult>();

        foreach (var effect in effects)
        {
            if (effect is EventEffect eventEffect)
            {
                var method = eventEffect.GetType().GetMethod("ApplyTypedEffect");
                if (method != null)
                {
                    var result = method.Invoke(effect, null);
                    if (effect is ItemEffect itemEffect)
                    {
                        results.Add(new EffectResult
                        {
                            EffectType = effect.GetType(),
                            Value = result,
                            IsAddingItems = itemEffect.isAddingItems
                        });
                    }
                    else
                    {
                        results.Add(new EffectResult
                        {
                            EffectType = effect.GetType(),
                            Value = result
                        });
                    }
                }
            }
        }

        return results;
    }


}

[System.Serializable]
public abstract class EventEffect
{
    public abstract void ApplyEffect();
}

[System.Serializable]
public abstract class EventEffect<T> : EventEffect
{
    public abstract T ApplyTypedEffect();

    public override void ApplyEffect()
    {
        ApplyTypedEffect();
    }
}

[System.Serializable]
public class DurationEffect : EventEffect<int>
{
    [Tooltip("Negative numbers increase the speed, positive numbers make the journey take longer")]
    [Range(-8, 40)]
    public int duration;
    [Range(0, 5)]
    public int randomness;

    public override int ApplyTypedEffect()
    {
        Debug.Log($"Duration changed by {duration}.");
        return UnityEngine.Random.Range(duration - randomness, duration + randomness);
    }
}

[System.Serializable]
public class CoinEffect : EventEffect<float>
{
    [Tooltip("Negative values mean you lose up to 50% of your coins, positive values mean you can gain up to 50% more coins")]
    [Range(0.5f, 1.5f)]
    public float coin;

    public override float ApplyTypedEffect()
    {
        float amount = PlayerMaster.Instance().money * coin - PlayerMaster.Instance().money;
        if (coin >= 1)
            PlayerMaster.Instance().addMoney(amount);
        else 
            PlayerMaster.Instance().removeMoney(-amount);
        Debug.Log($"Coins changed by {coin}.");
        return amount;
    }
}

[System.Serializable]
public class NoEffect : EventEffect<bool>
{
    public override bool ApplyTypedEffect()
    {
        Debug.Log($"Nothing happened.");
        return true;
    }
}
