// Manages storage operations and item placement
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StorageManager : MonoBehaviour
{
    [SerializeField] private InventoryManager inventoryManager;
    [SerializeField] private GridManager gridManager;
    private List<GameObject> spawnedItems = new List<GameObject>();

    public void AddItem(Item item)
    {
        inventoryManager.CurrentStorage.items.Add(item);
        spawnedItems.Add(item.ObjectReference);
    }

    public void RemoveItem(Item item)
    {
        if (item == null) return;

        inventoryManager.CurrentStorage.items.Remove(item);
        Destroy(item.ObjectReference);
        spawnedItems.Remove(item.ObjectReference);
    }

    public void SpawnStoredItems()
    {
        ClearSpawnedItems();

        foreach (var item in inventoryManager.CurrentStorage.items)
        {
            if (item?.ItemData == null) continue;

            Vector3 worldPos = CalculateItemWorldPosition(item);
            Quaternion rotation = item.IsRotated ? Quaternion.Euler(0, 90, 0) : Quaternion.identity;

            GameObject spawnedObj = Instantiate(
                item.ItemData.graphics,
                worldPos,
                rotation,
                inventoryManager.InventoryScene.transform
            );

            item.ObjectReference = spawnedObj;
            spawnedItems.Add(spawnedObj);
        }
    }

    public void ClearSpawnedItems()
    {
        foreach (var item in spawnedItems)
        {
            if (item != null) Destroy(item);
        }
        spawnedItems.Clear();
    }

    private Vector3 CalculateItemWorldPosition(Item item)
    {
        Vector3 gridPos = gridManager.GridToWorldPosition(item.Position);
        return gridPos + new Vector3(
            item.Size.x / 2f - 0.5f,
            item.Size.y / 2f * gridManager.cellSize,
            item.Size.z / 2f - 0.5f
        );
    }

    public Item GetModuleAtPosition(Vector3Int position)
    {
        return inventoryManager.CurrentStorage.items.FirstOrDefault(item =>
            IsPositionWithinBounds(position, item.Position, item.Size));
    }

    public bool IsSpaceAvailable(Vector3Int position, Vector3Int size)
    {
        for (int x = position.x; x < position.x + size.x; x++)
            for (int y = position.y; y < position.y + size.y; y++)
                for (int z = position.z; z < position.z + size.z; z++)
                {
                    if (IsPositionOccupied(new Vector3Int(x, y, z)))
                        return false;
                }
        return true;
    }

    private bool IsPositionOccupied(Vector3Int position)
    {
        return inventoryManager.CurrentStorage.items.Any(item =>
            IsPositionWithinBounds(position, item.Position, item.Size));
    }

    private bool IsPositionWithinBounds(Vector3Int position, Vector3Int itemPos, Vector3Int itemSize)
    {
        return position.x >= itemPos.x && position.x < itemPos.x + itemSize.x &&
               position.y >= itemPos.y && position.y < itemPos.y + itemSize.y &&
               position.z >= itemPos.z && position.z < itemPos.z + itemSize.z;
    }

    public Vector3Int GetStackPosition(Vector3Int basePosition, Vector3Int size)
    {
        Vector3Int currentPos = basePosition;
        while (currentPos.y + size.y <= gridManager.height)
        {
            if (IsSpaceAvailable(currentPos, size))
                return currentPos;
            currentPos.y++;
        }
        return currentPos;
    }

    public Vector3Int AdjustPositionForBounds(Vector3Int position, Vector3Int size)
    {
        Vector3Int gridSize = new Vector3Int(gridManager.width, gridManager.height, gridManager.length);
        return new Vector3Int(
            Mathf.Clamp(position.x, 0, gridSize.x - size.x),
            Mathf.Clamp(position.y, 0, gridSize.y - size.y),
            Mathf.Clamp(position.z, 0, gridSize.z - size.z)
        );
    }
}