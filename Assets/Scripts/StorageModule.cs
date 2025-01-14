using System.Collections;
using System.Collections.Generic;
using System.Threading;
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

    public int GetCurrentCapacity()
    {
        int capacity = 0;
        foreach (var item in items)
        {
            capacity += item.ItemData.size * item.Count;
        }
        return capacity;
    }

    public bool AddItem(Item item)
    {
        var existingItem = items.Find(i => i.ItemData == item.ItemData);
        if (existingItem != null)
        {
            existingItem.AddCount(item.Count);
        }
        else
        {
            items.Add(item);
        }
        return true;
    }

    public bool RemoveItem(Item item)
    {
        var existingItem = items.Find(i => i.ItemData == item.ItemData);
        if (existingItem != null)
        {
            existingItem.SubtractCount(item.Count);
            if (existingItem.Count <= 0)
            {
                items.Remove(existingItem);
            }
            return true;
        }
        return false;
    }
}