using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum StorageType
{
    Goods,
    Fluid,
    Livestock
};

[CreateAssetMenu(fileName = "New Storage Module", menuName = "Storage Module")]
public class StorageModuleData : ScriptableObject
{
    public string id;
    [Tooltip("The size that will take up in the wagon")]
    public Vector3Int size;
    public StorageType storage_type;
    [Tooltip("How many inventory slots it has")]
    public Vector2Int inventorySize;

    [Tooltip("This will be the model that will be displayed when added to the caravan")]
    public GameObject graphics;
    [Tooltip("This will be the icon in the build mode")]
    public Sprite icon;
}
