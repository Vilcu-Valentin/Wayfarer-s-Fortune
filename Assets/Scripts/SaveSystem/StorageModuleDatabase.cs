// You'll need this ScriptableObject to manage module prefabs
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ModuleDatabase", menuName = "Caravan/Module Database")]
public class StorageModuleDatabase : ScriptableObject
{
    [SerializeField] private List<StorageModuleData> modules;

    public StorageModuleData GetModuleById(string id)
    {
        return modules.Find(m => m.id == id);
    }
}