using System.Collections.Generic;
using System;
using System.IO;
using System.Linq;
using UnityEngine;

[Serializable]
public class WagonPrefabEntry
{
    public string key;
    public GameObject prefab;
}

public class CaravanSaveManager : MonoBehaviour
{
    [SerializeField] private CaravanManager caravanManager;
    [SerializeField] private StorageModuleDatabase storageModuleDatabase;
    [SerializeField] private string saveFileName = "caravan_save.json";
    [SerializeField] private List<WagonPrefabEntry> wagonPrefabEntries; // List for Inspector display

    private Dictionary<string, GameObject> wagonPrefabMap; // Dictionary for internal use
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        // Initialize the dictionary from the list
        wagonPrefabMap = wagonPrefabEntries.ToDictionary(entry => entry.key, entry => entry.prefab);
    }

    public void SaveCaravan()
    {
        try
        {
            CaravanSaveData saveData = new CaravanSaveData
            {
                activeWagonIndex = caravanManager.CurrentWagonIndex
            };

            // Get data from each wagon
            for (int i = 0; i < caravanManager.WagonCount; i++)
            {
                Wagon wagon = caravanManager.Wagons[i];
                string prefabPath = GetPrefabPath(wagon.gameObject);

                WagonSaveData wagonData = new WagonSaveData
                {
                    wagonIndex = i,
                    wagonPrefabPath = prefabPath,
                    modules = wagon.storageModules.Select(module => new StorageModuleSaveData
                    {
                        moduleId = module.moduleData.id,
                        position = module.currentPosition,
                        isRotated = module.rotated,
                        rotation = module.rotation
                    }).ToList()
                };
                saveData.wagons.Add(wagonData);
            }

            string json = JsonUtility.ToJson(saveData, true);
            File.WriteAllText(SavePath, json);
            Debug.Log($"Caravan saved successfully to: {SavePath}");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error saving caravan: {e.Message}");
        }
    }

    public void LoadCaravan()
    {
        try
        {
            if (!File.Exists(SavePath))
            {
                Debug.LogWarning("No save file found!");
                return;
            }

            string json = File.ReadAllText(SavePath);
            CaravanSaveData saveData = JsonUtility.FromJson<CaravanSaveData>(json);

            // Clear existing wagons before loading
            caravanManager.ResetCaravan();

            // Load each wagon from saved data
            foreach (WagonSaveData wagonData in saveData.wagons.OrderBy(w => w.wagonIndex))
            {
                GameObject wagonPrefab = GetWagonPrefab(wagonData.wagonPrefabPath);
                if (wagonPrefab == null)
                {
                    Debug.LogError($"Could not find wagon prefab: {wagonData.wagonPrefabPath}");
                    continue;
                }

                // Create list of storage modules from saved data
                List<StorageModule> storageModules = wagonData.modules.Select(moduleData =>
                    CreateStorageModule(moduleData)).ToList();

                // Add wagon with its modules
                caravanManager.AddWagon(wagonPrefab, storageModules);
            }

            // Restore active wagon
            if (saveData.activeWagonIndex >= 0 && saveData.activeWagonIndex < caravanManager.WagonCount)
            {
                caravanManager.SetActiveWagon(saveData.activeWagonIndex);
            }

            Debug.Log("Caravan loaded successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading caravan: {e.Message}");
        }
    }

    private string GetPrefabPath(GameObject wagonObject)
    {
        // Implementation depends on your project structure
        return wagonObject.name.Replace("(Clone)", "").Trim();
    }

    private GameObject GetWagonPrefab(string prefabPath)
    {
        // Look up the prefab in the dictionary
        if (wagonPrefabMap.TryGetValue(prefabPath, out GameObject prefab))
        {
            return prefab;
        }
        return null;
    }

    private StorageModule CreateStorageModule(StorageModuleSaveData moduleData)
    {
        // Create a new StorageModule instance with saved data
        StorageModule module = new StorageModule
        {
            moduleData = storageModuleDatabase.GetModuleById(moduleData.moduleId),
            currentPosition = moduleData.position,
            rotated = moduleData.isRotated,
            rotation = moduleData.rotation
        };
        return module;
    }
}
