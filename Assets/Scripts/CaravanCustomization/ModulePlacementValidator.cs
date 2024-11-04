using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulePlacementValidator
{
    private readonly GridManager gridManager;
    private readonly Wagon wagon;

    public ModulePlacementValidator(GridManager gridManager, Wagon wagon)
    {
        this.gridManager = gridManager;
        this.wagon = wagon;
    }

    // Get's the y position for an object
    // REMOVE IT FROM HERE AND PLACE IT IN A POSITION VALIDATOR OR SOMETHING
    public Vector3Int GetStackPosition(Vector3Int basePosition, Vector3Int moduleSize)
    {
        Vector3Int currentPosition = basePosition;
        while (currentPosition.y + moduleSize.y <= gridManager.height)
        {
            if (CanStackAt(currentPosition, moduleSize))
            {
                return currentPosition;
            }
            currentPosition.y++;
        }
        Debug.Log("Object out of build volume!");
        return currentPosition;
    }

    public bool IsPlacementValid(Vector3Int position, Vector3Int size)
    {
        return CheckFullBase(position, size) && gridManager.CanPlaceModule(position, size);
    }

    // If it's not in an occupied space it means you can stack it here
    public bool CanStackAt(Vector3Int position, Vector3Int moduleSize)
    {
        return !IsSpaceOccupied(position, moduleSize);
    }

    // Checks if there is a full base below the object
    public bool CheckFullBase(Vector3Int position, Vector3Int moduleSize)
    {
        if (position.y == 0) return true;

        for (int x = position.x; x < position.x + moduleSize.x; x++)
            for (int z = position.z; z < position.z + moduleSize.z; z++)
            {
                if (!IsSpaceOccupied(new Vector3Int(x, position.y - 1, z), Vector3Int.one))
                {
                    Debug.Log("Object base is not full!");
                    return false;
                }
            }
        return true;
    }

    // Returns true if the any space above the current module is occupied
    public bool IsSpaceOccupiedAbove(StorageModule module)
    {
        Vector3Int virtualModulePosition = module.currentPosition;
        Vector3Int virtualModuleSize = module.rotated
            ? new Vector3Int(module.moduleData.size.z, 1, module.moduleData.size.x)
            : module.moduleData.size;

        virtualModulePosition.y += module.moduleData.size.y;

        return IsSpaceOccupied(virtualModulePosition, virtualModuleSize);
    }

    // It takes a virtual object (position and size), and for each cell in that it checks if it intersects with a module
    public bool IsSpaceOccupied(Vector3Int position, Vector3Int size)
    {
        for (int x = position.x; x < position.x + size.x; x++)
            for (int y = position.y; y < position.y + size.y; y++)
                for (int z = position.z; z < position.z + size.z; z++)
                    if (IsPositionOccupied(new Vector3Int(x, y, z))) return true;

        return false;
    }

    // Actually checks if a gridPosition (cell) is occupied by an already placed module
    private bool IsPositionOccupied(Vector3Int position)
    {
        foreach (StorageModule module in wagon.storageModules)
        {
            Vector3Int moduleSize = module.rotated
                ? new Vector3Int(module.moduleData.size.z, module.moduleData.size.y, module.moduleData.size.x)
                : module.moduleData.size;

            if (IsPositionWithinModule(position, module.currentPosition, moduleSize)) return true;
        }
        return false;
    }

    // Performs some calculations to check if a cell is within the bounds of a module
    public bool IsPositionWithinModule(Vector3Int position, Vector3Int modulePosition, Vector3Int moduleSize)
    {
        return position.x >= modulePosition.x && position.x < modulePosition.x + moduleSize.x &&
               position.y >= modulePosition.y && position.y < modulePosition.y + moduleSize.y &&
               position.z >= modulePosition.z && position.z < modulePosition.z + moduleSize.z;
    }

    // Adjust position to update position on normally illegal origin points (like last column or row for multi tile objects)
    public Vector3Int AdjustPositionForPlacement(Vector3Int position, Vector3Int moduleSize)
    {
        Vector3Int gridSize = new Vector3Int(gridManager.width, gridManager.height, gridManager.length);
        Vector3Int adjustedPosition = position;

        if (position.x + moduleSize.x > gridSize.x)
            adjustedPosition.x = gridSize.x - moduleSize.x;
        if (position.y + moduleSize.y > gridSize.y)
            adjustedPosition.y = gridSize.y - moduleSize.y;
        if (position.z + moduleSize.z > gridSize.z)
            adjustedPosition.z = gridSize.z - moduleSize.z;

        adjustedPosition.x = Mathf.Max(0, adjustedPosition.x);
        adjustedPosition.y = Mathf.Max(0, adjustedPosition.y);
        adjustedPosition.z = Mathf.Max(0, adjustedPosition.z);

        return adjustedPosition;
    }
}
