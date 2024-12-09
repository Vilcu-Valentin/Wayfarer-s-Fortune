using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData ItemData { get; }
    public GameObject ObjectReference { get; set; }
    public Vector2Int Position { get; set; }
    public bool IsRotated { get; set; }
    public Item(ItemData data, Vector2Int position, GameObject objRef, bool isRotated = false)
    {
        ItemData = data;
        Position = position;
        ObjectReference = objRef;
        IsRotated = isRotated;
    }
}
