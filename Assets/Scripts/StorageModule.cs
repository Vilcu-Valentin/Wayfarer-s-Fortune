using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorageModule
{
    public StorageModuleData moduleData;

    public Vector3Int currentPosition;
    public Vector3Int Size => rotated ?
       new Vector3Int(moduleData.size.z, moduleData.size.y, moduleData.size.x) :
       moduleData.size;

    public bool rotated;
    public float rotation;
    public GameObject objectRef;

    public List<Item> items;
}
