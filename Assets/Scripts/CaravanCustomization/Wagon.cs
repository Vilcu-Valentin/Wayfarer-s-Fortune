// Base wagon class with storage and highlighting functionality
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class Wagon : MonoBehaviour
{
    [Tooltip("This list will be used for the inventory and other calculations like weight etc.")]
    [SerializeField] public List<StorageModule> storageModules = new List<StorageModule>();
    [SerializeField] private GridManager grid;

    // Add the storageModule we just placed to the List
    public void AddStorageModule(StorageModule module)
    {
        storageModules.Add(module);
        InitModuleGFX();
    }

    // Remove a selected storage module from the wagon
    public void RemoveStorageModule(StorageModule module)
    {
        storageModules.Remove(module);
        InitModuleGFX();
    }

    // TODO: Also add an updateStorageModule so you can move already placed modules

    // Creates the module graphics based on the list of modules
    public void InitModuleGFX()
    {
        foreach (var module in storageModules)
        {
            if (module.objectRef != null)
                Destroy(module.objectRef);
            PlaceModule(module);
        }
    }

    private void PlaceModule(StorageModule selectedModule)
    {
        float yRotation = selectedModule.rotated ? 90f : 0f;

        Vector3 placementPosition = grid.GridToAdjustedWorldPosition(selectedModule.currentPosition, selectedModule.Size);

        // Instantiate the module and set its properties
        GameObject placedModule = Instantiate(
            selectedModule.moduleData.graphics,
            placementPosition,
            Quaternion.Euler(0, yRotation, 0),
            transform
        );

        selectedModule.objectRef = placedModule;
        placedModule.layer = 7;
    }
}