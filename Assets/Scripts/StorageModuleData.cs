using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Storage Module", menuName = "Storage Module")]
public class StorageModuleData : ScriptableObject
{
    public string id;
    [Tooltip("The size that will take up in the wagon")]
    public Vector3Int size;
    public StorageType storage_type;
    [Tooltip("The capacity of the storage component, in the respective units (either count or gallons)")]
    public int capacity;

    [Tooltip("This will be the model that will be displayed when added to the caravan")]
    public GameObject graphics;
    [Tooltip("This will be the icon in the build mode")]
    public Sprite icon;

    // Add a steal_protection variable;
}
