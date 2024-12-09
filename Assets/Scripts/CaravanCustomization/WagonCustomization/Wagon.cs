using System.Collections.Generic;
using UnityEngine;

public class Wagon : CaravanBody
{
    [Tooltip("This list will be used for the inventory and other calculations like weight etc.")]
    public List<StorageModule> storageModules = new List<StorageModule>();
    [SerializeField] private GridManager grid;

    public void AddStorageModule(StorageModule module)
    {
        storageModules.Add(module);
        InitModuleGFX();
    }
    // Remove a selected storage module from the wagon
    public void RemoveStorageModule(StorageModule module)
    {
        Destroy(module.objectRef);
        storageModules.Remove(module);
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
        float yRotation = selectedModule.rotation;

        // Get the world position based on grid rotation and position
        Vector3 placementPosition = grid.GridToAdjustedWorldPosition(selectedModule.currentPosition, selectedModule.Size);

        // Combine grid rotation with the module's intended rotation
        Quaternion gridRotation = grid.gridRotation;
        Quaternion moduleRotation = Quaternion.Euler(0, yRotation, 0);

        // Instantiate the module with combined rotation
        GameObject placedModule = Instantiate(
            selectedModule.moduleData.graphics,
            placementPosition,
            gridRotation * moduleRotation, // Apply grid rotation as base, then module's relative rotation
            transform
        );

        // Additional settings
        selectedModule.objectRef = placedModule;
        placedModule.layer = 7;
    }

}