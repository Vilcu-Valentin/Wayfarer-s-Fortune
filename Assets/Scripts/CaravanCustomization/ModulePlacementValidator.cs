// Module placement validation and rules
using UnityEngine;

public class ModulePlacementValidator
{
    private readonly GridManager gridManager;
    private readonly Wagon currentWagon;

    public ModulePlacementValidator(GridManager gridManager, Wagon wagon)
    {
        this.gridManager = gridManager;
        this.currentWagon = wagon;
    }

    // Get's the y position for an object
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

    // If it's not in an occupied space it means you can stack it here
    public bool CanStackAt(Vector3Int position, Vector3Int moduleSize)
    {
        return !currentWagon.IsSpaceOccupied(position, moduleSize);
    }

    // Checks if there is a full base below the object
    public bool CheckFullBase(Vector3Int position, Vector3Int moduleSize)
    {
        if (position.y == 0) return true;

        for (int x = position.x; x < position.x + moduleSize.x; x++)
            for (int z = position.z; z < position.z + moduleSize.z; z++)
            {
                if (!currentWagon.IsSpaceOccupied(new Vector3Int(x, position.y - 1, z), Vector3Int.one))
                {
                    Debug.Log("Object base is not full!");
                    return false;
                }
            }
        return true;
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