using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Represents a road between two settlements with an associated distance.
/// </summary>
[System.Serializable]
public struct Roads
{
    public SettlementData road1;
    public SettlementData road2;

    [Tooltip("Distance between 2 cities, measured in hours")]
    public int distance;

    public override string ToString()
    {
        return "Road: " + road1 + " <-> " + road2 + " (Distance: " + distance + " hours)";
    }
}

[CreateAssetMenu(fileName = "New Map", menuName = "TradeSystem/Map")]
[Serializable]
public class MapData : ScriptableObject
{
    [SerializeField]
    [Tooltip("Roads are implicitly bidirectional.")]
    public List<Roads> roads;
}
