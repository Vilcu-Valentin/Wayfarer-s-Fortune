// Serializable class to hold module data
using System;
using UnityEngine;

[Serializable]
public class StorageModuleSaveData
{
    public string moduleId;
    public Vector3Int position;
    public bool isRotated;
    public float rotation;
}