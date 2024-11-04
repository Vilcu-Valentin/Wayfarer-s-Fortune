// Serializable class to hold wagon data
using System.Collections.Generic;
using System;
using UnityEngine;

[Serializable]
public class WagonSaveData
{
    public int wagonIndex;
    public string wagonPrefabPath; // Store the prefab path instead of GameObject
    public List<StorageModuleSaveData> modules = new List<StorageModuleSaveData>();
}