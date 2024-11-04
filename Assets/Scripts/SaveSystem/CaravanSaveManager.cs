using System.IO;
using System.Linq;
using UnityEngine;

public class CaravanSaveManager : MonoBehaviour
{
    [SerializeField] private CaravanManager caravanManager;
    [SerializeField] private GameObject wagonPrefab;

    private string SavePath => Path.Combine(Application.persistentDataPath, "caravan_save.json");

    /*public void SaveCaravan()
    {
        CaravanSaveData saveData = new CaravanSaveData();

        // Get data from each wagon
        for (int i = 0; i < caravanManager.WagonCount; i++)
        {
            Wagon wagon = caravanManager.Wagons[i];
            WagonSaveData wagonData = new WagonSaveData
            {
                wagonIndex = i,
                modules = wagon.storageModules.Select(module => new StorageModuleSaveData
                {
                    moduleId = module.moduleData.id,
                    position = module.currentPosition,
                    isRotated = module.rotated
                }).ToList()
            };

            saveData.wagons.Add(wagonData);
        }

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"Caravan saved to: {SavePath}");
    }

    public void LoadCaravan()
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
            caravanManager.AddWagon(wagonPrefab);
        }

        Debug.Log("Caravan loaded successfully.");
    } */
}
