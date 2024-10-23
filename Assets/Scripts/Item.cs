using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData ItemData { get; }
    public Vector3Int Position { get; set; }
    public GameObject ObjectReference { get; set; }
    public bool IsRotated { get; set; }
    public Vector3Int Size => IsRotated ?
        new Vector3Int(ItemData.size.z, ItemData.size.y, ItemData.size.x) :
        ItemData.size;

    public Item(ItemData data, Vector3Int position, GameObject objRef, bool isRotated = false)
    {
        ItemData = data;
        Position = position;
        ObjectReference = objRef;
        IsRotated = isRotated;
    }
}
