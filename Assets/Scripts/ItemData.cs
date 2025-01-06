using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
[System.Serializable]
public class ItemData : ScriptableObject, IEquatable<ItemData>
{
    public string item_name;
    [Tooltip("The size that will take up in the storage module")]
    public Vector2Int size;
    [Tooltip("Weight in lbs per unit")]
    public int weight;
    public StorageType store_type;

    [Tooltip("This will be the model that will be displayed when added to the caravan")]
    public GameObject graphics;
    [Tooltip("This will be the icon in the build mode")]
    public Sprite icon;


    public bool Equals(ItemData other)
    {
        if (other == null) return false;
        if (System.Object.ReferenceEquals(this, other)) return true;
        if (this.GetType() != other.GetType()) return false;

        return this.item_name.Equals(other.item_name);
    }

    public override bool Equals(object other) { return this.Equals(other as ItemData); }
    public override int GetHashCode() { return this.item_name.GetHashCode(); }
}
