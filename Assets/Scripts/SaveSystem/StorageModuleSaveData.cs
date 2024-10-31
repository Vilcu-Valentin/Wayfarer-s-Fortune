// Serializable class to hold module data
using System;
using UnityEngine;

[Serializable]
public class StorageModuleSaveData
{
    public string moduleId; // Identifier for the module prefab
    public Vector3Int position;
    public bool isRotated;
}