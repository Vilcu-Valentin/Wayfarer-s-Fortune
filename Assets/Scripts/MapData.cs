using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Map", menuName = "Map")]
[Serializable]
public class MapData : ScriptableObject
{
    [SerializeField]
    [Tooltip("Roads are implicitly bidirectional.")]
    public List<SerializablePair<SettlementData, SettlementData>> roads;
}
