using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Item", menuName = "Item")]
public class ItemData : ScriptableObject
{
    public string item_name;
    [Tooltip("The size that will take up in the storage module")]
    public Vector2Int size;
    public StorageType store_type;

    [Tooltip("This will be the icon in the build mode")]
    public Sprite icon;
}