using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct EventEffectData
{
    public ItemData item;
    
    [Range(-10, 10)]
    public int strength;
}

[CreateAssetMenu(fileName = "NewEvent", menuName = "Event")]
public class EventData : ScriptableObject, IEquatable<EventData>
{
    public new string name;
    public string description;

    [Tooltip("Measured in hours.")]
    public int leadTime;
    [Tooltip("Measured in hours.")]
    public int duration;
    [Tooltip("The time it takes for the effects to completely dissapear after the event is over. Measured in hours.")]
    public int deathTime;

    [Tooltip("Spring is 0, Summer is 1, Autumn is 2 and Winter is 3. For seasonal events set values between 0 and 1 " +
        "for the chance to occur at the beggining of each season.")]
    public float[] seasonalModifiers = { 1f, 1f, 1f, 1f };
    
    public List<EventEffectData> affectedGoods;

    public bool Equals(EventData other)
    {
        if (other == null) return false;
        if (System.Object.ReferenceEquals(this, other)) return true;
        if (this.GetType() != other.GetType()) return false;

        return this.name.Equals(other.name);
    }

    public override bool Equals(object other) { return this.Equals(other as EventData); }
    public override int GetHashCode()  { return name.GetHashCode(); }
}
