using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GridManager gridManager;
    private List<GameObject> itemsReference;

    // Add the storageModule we just placed to the List
    public void AddItem(Item item)
    {
        inventoryManager.CurrentStorage.items.Add(item);
    }

    // Remove a selected storage module from the wagon
    public void RemoveStorageModule(Item item)
    {
        Debug.Log($"Trying to remove module at: {item.currentPosition}");

        inventoryManager.CurrentStorage.items.Remove(item);
        Destroy(item.objectRef);
    }

    public void AddItemReference(GameObject reference)
    {
        itemsReference.Add(reference);
    }

    public void SpawnItems()
    {
        foreach (Item item in inventoryManager.CurrentStorage.items)
        {
            if (item == null || item.ItemData == null) continue;

            // Calculate the world position based on the grid position
            Vector3 worldPosition = CalculateWorldPosition(item);
            Quaternion rotation = CalculateRotation(item);

            // Spawn the item
            GameObject spawnedObject = Instantiate(
                item.ItemData.graphics,
                worldPosition,
                rotation,
                inventoryManager.InventoryScene.transform
            );

            // Update the object reference and add to our tracking list
            item.objectRef = spawnedObject;
            itemsReference.Add(spawnedObject);
        }
    }

    public void ClearSpawnedItems()
    {
        if (itemsReference != null)
        {
            foreach (GameObject item in itemsReference)
            {
                if (item != null)
                    Destroy(item);
            }
            itemsReference.Clear();
        }
        else
        {
            itemsReference = new List<GameObject>();
        }
    }

    private Vector3 CalculateWorldPosition(Item item)
    {
        Vector3Int moduleSize = item.rotated ?
            new Vector3Int(item.ItemData.size.z, item.ItemData.size.y, item.ItemData.size.x) :
            item.ItemData.size;

        // Convert grid position to world position
        Vector3 worldPosition = gridManager.GridToWorldPosition(item.currentPosition);

        // Adjust for center position like we do in placement
        worldPosition += new Vector3(
            moduleSize.x / 2f - 0.5f,
            moduleSize.y / 2f * gridManager.cellSize,
            moduleSize.z / 2f - 0.5f
        );

        return worldPosition;
    }

    private Quaternion CalculateRotation(Item item)
    {
        return item.rotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;
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
        foreach (Item item in inventoryManager.CurrentStorage.items)
        {
            Vector3Int moduleSize = item.ItemData.size;
            // We make sure that we get the rotated size (with the flipped x and z sizes)
            if (item.rotated)
            {
                moduleSize = new Vector3Int(moduleSize.z, moduleSize.y, moduleSize.x);
            }

            // If the cell intersects with a module we return true -> occupiedPosition
            if (IsPositionWithinModule(position, item.currentPosition, moduleSize))
            {
                return true;
            }
        }
        return false;
    }

#nullable enable
    public Item? GetModuleByGridCoords(Vector3Int gridCoords)
    {
        foreach (Item item in inventoryManager.CurrentStorage.items)
        {
            Vector3Int moduleSize = item.ItemData.size;
            // We make sure that we get the rotated size (with the flipped x and z sizes)
            if (item.rotated)
            {
                moduleSize = new Vector3Int(moduleSize.z, moduleSize.y, moduleSize.x);
            }

            // If the gridCoords intersect with a module, we return the module
            if (IsPositionWithinModule(gridCoords,item.currentPosition, moduleSize))
            {
                return item;
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
        return !IsSpaceOccupied(position, moduleSize);
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
