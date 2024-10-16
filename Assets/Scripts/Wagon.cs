using System.Collections.Generic;
using UnityEngine;

public class Wagon : MonoBehaviour
{
    private List<StorageModule> storageModules = new List<StorageModule>();

    public void AddStorageModule(StorageModule module)
    {
        storageModules.Add(module);
        // Additional logic for handling modules can go here
    }

    public void RemoveStorageModule(StorageModule module)
    {
        storageModules.Remove(module);
        // Additional removal logic
    }

    // Method for getting wagon status (if needed)
    public List<StorageModule> GetStorageModules()
    {
        return storageModules;
    }
}
