// Base wagon class with storage and highlighting functionality
using System.Collections.Generic;
using Cinemachine;
using UnityEngine;
using UnityEngine.UIElements;

public class Wagon : MonoBehaviour
{
    [Tooltip("This list will be used for the inventory and other calculations like weight etc.")]
    [SerializeField] public List<StorageModule> storageModules = new List<StorageModule>();
    [Tooltip("Temporary, is used to highlight the currently selected wagon. Will be removed in the future in favor of just changing the cinemachine camera")]
    [SerializeField] private MeshRenderer[] highlightMeshes;
    [Tooltip("The hover camera")]
    [SerializeField] private CinemachineFreeLook lookCamera;

    //Temporary as well will be removed with the highlightMeshes;
    private Color defaultColor;
    private Color highlightColor = new Color(1f, 0.92f, 0.016f, 1f);

    // Temporary will be removed with the highlightMeshses;
    private void Awake()
    {
        if (highlightMeshes.Length > 0)
        {
            defaultColor = highlightMeshes[0].material.color;
        }
    }

    // Add the storageModule we just placed to the List
    public void AddStorageModule(StorageModule module)
    {
        storageModules.Add(module);
    }

    // Remove a selected storage module from the wagon
    public void RemoveStorageModule(StorageModule module)
    {
        // A virtual module the same area as our storage module, but with a height of one
        Vector3Int virtualModuleSize = module.moduleData.size;
        if(module.rotated)
            virtualModuleSize = new Vector3Int(module.moduleData.size.z, 1, module.moduleData.size.x);
        Vector3Int virtualModulePosition = module.currentPosition;
        virtualModulePosition.y += module.moduleData.size.y;

        Debug.Log($"Trying to remove module at: {module.currentPosition}, with virtual position {virtualModulePosition} and virtual size {virtualModuleSize}");

        // We check if there is any module above the one we want to delete
        if (IsSpaceOccupied(virtualModulePosition, virtualModuleSize))
        {
            Debug.Log("Cannot remove object! There is a module above");
        }
        else
        {
            storageModules.Remove(module);
            Destroy(module.objectRef);
        }
    }

    // Takes an virtual object (it's origin and size) and checks for each cell of that object if it intersects with an already existing one
    public bool IsSpaceOccupied(Vector3Int position, Vector3Int size)
    {
        for (int x = position.x; x < position.x + size.x; x++)
            for (int y = position.y; y < position.y + size.y; y++)
                for (int z = position.z; z < position.z + size.z; z++)
                {
                    if (IsPositionOccupied(new Vector3Int(x, y, z)))
                    {
                        return true;
                    }
                }
        return false;
    }

    // Performs the actual check if the cell occupies an already full position (by going through each storageModule already placed)
    private bool IsPositionOccupied(Vector3Int position)
    {
        foreach (StorageModule module in storageModules)
        {
            Vector3Int moduleSize = module.moduleData.size;
            // We make sure that we get the rotated size (with the flipped x and z sizes)
            if (module.rotated)
            {
                moduleSize = new Vector3Int(moduleSize.z, moduleSize.y, moduleSize.x);
            }

            // If the cell intersects with a module we return true -> occupiedPosition
            if (IsPositionWithinModule(position, module.currentPosition, moduleSize))
            {
                return true;
            }
        }
        return false;
    }

    #nullable enable
    public StorageModule? GetModuleByGridCoords(Vector3Int gridCoords)
    {
        foreach(StorageModule module in storageModules)
        {
            Vector3Int moduleSize = module.moduleData.size;
            // We make sure that we get the rotated size (with the flipped x and z sizes)
            if (module.rotated)
            {
                moduleSize = new Vector3Int(moduleSize.z, moduleSize.y, moduleSize.x);
            }

            // If the gridCoords intersect with a module, we return the module
            if (IsPositionWithinModule(gridCoords, module.currentPosition, moduleSize))
            {
                return module;
            }
        }
        return null; //No module could be found at those coordinates
    }    


    // We check if a position, is within the bounding box of a module
    private bool IsPositionWithinModule(Vector3Int position, Vector3Int modulePosition, Vector3Int moduleSize)
    {
        return position.x >= modulePosition.x && position.x < modulePosition.x + moduleSize.x &&
               position.y >= modulePosition.y && position.y < modulePosition.y + moduleSize.y &&
               position.z >= modulePosition.z && position.z < modulePosition.z + moduleSize.z;
    }

    // Used to set a high priority on this camera
    public void FocusCamera()
    {
        lookCamera.Priority = 100;
    }

    // Used to set a low priority on this camera
    public void DeFocusCamera()
    {
        lookCamera.Priority = 1;
    }

    // Will be removed with the highlightMeshes
    public void SetHighlight(bool isHighlighted)
    {
        foreach (var mesh in highlightMeshes)
        {
            mesh.material.color = isHighlighted ? highlightColor : defaultColor;
        }
    }
}