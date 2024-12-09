using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EventSettlementContext
{
    public EventData eventData;

    [Tooltip("Measured in days per year.")]
    [Range(0f, 56f)]
    public float frequency;
}

[System.Serializable]
public struct ItemMarketProfile
{
    public ItemData itemData;
    public float basePricePerUnit;
    [Tooltip("Absolute fractional price volatility, i.e. for a volatility of 0.25 the price will range between 0.75*basePrice and 1.25*basePrice, " +
        "not accounting other modifiers. Used in daily price fluctuations as a final modifier.")]
    [Range(0f, 0.99f)]
    public float volatility;
}

[CreateAssetMenu(fileName = "New Settlement", menuName = "TradeSystem/Settlement")]
public class SettlementData : ScriptableObject, IEquatable<SettlementData>
{
    public new string name;
    public string size;
    public string occupation;
    public List<ItemMarketProfile> goodsPool;
    public List<EventSettlementContext> localEvents;
    public List<EventData> globalEvents;
    public List<EventData> seasonalEvents;

    public bool Equals(SettlementData other)
    {
        if (other == null) return false;
        if (System.Object.ReferenceEquals(this, other)) return true;
        if (this.GetType() != other.GetType()) return false;

        return this.name == other.name;
    }

    public override bool Equals(object other) { return this.Equals(other as SettlementData); }
    public override int GetHashCode() { return this.name.GetHashCode(); }
}
