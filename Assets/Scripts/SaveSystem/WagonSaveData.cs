// Serializable class to hold wagon data
using System.Collections.Generic;
using System;

[Serializable]
public class WagonSaveData
{
    public int wagonIndex;
    public List<StorageModuleSaveData> modules = new List<StorageModuleSaveData>();
}