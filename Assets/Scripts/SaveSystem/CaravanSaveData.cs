// Serializable class to hold entire caravan data
using System.Collections.Generic;
using System;

[Serializable]
public class CaravanSaveData
{
    public List<WagonSaveData> wagons = new List<WagonSaveData>();
    public int activeWagonIndex;
}