using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class StorageModule
{
    public StorageModuleData moduleData;

    public Vector3Int currentPosition;
    public bool rotated;
    public GameObject objectRef;

    public List<Item> items;
}
