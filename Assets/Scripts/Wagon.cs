using System.Collections.Generic;
using UnityEngine;

public class Wagon : MonoBehaviour
{
    [SerializeField] private List<StorageModule> storageModules = new List<StorageModule>();

    public void AddStorageModule(StorageModule module)
    {
        storageModules.Add(module);
        // Additional logic for handling modules can go here
    }

    public void RemoveStorageModule(StorageModule module)
    {
        storageModules.Remove(module);
        Destroy(module.objectRef);
        // Additional removal logic
    }

    public List<StorageModule> GetStorageModules()
    {
        return storageModules;
    }

    public bool IsSpaceOccupied(Vector3Int position, Vector3Int size)
    {
        for (int x = position.x; x < position.x + size.x; x++)
        {
            for (int y = position.y; y < position.y + size.y; y++)
            {
                for (int z = position.z; z < position.z + size.z; z++)
                {
                    Vector3Int checkPos = new Vector3Int(x, y, z);
                    if (IsPositionOccupied(checkPos))
                    {
                        return true;
                    }
                }
            }
        }
        return false;
    }

    private bool IsPositionOccupied(Vector3Int position)
    {
        foreach (StorageModule module in storageModules)
        {
            if (IsPositionWithinModule(position, module.currentPosition, module.moduleData.size))
            {
                return true;
            }
        }
        return false;
    }

    private bool IsPositionWithinModule(Vector3Int position, Vector3Int modulePosition, Vector3Int moduleSize)
    {
        return position.x >= modulePosition.x && position.x < modulePosition.x + moduleSize.x &&
               position.y >= modulePosition.y && position.y < modulePosition.y + moduleSize.y &&
               position.z >= modulePosition.z && position.z < modulePosition.z + moduleSize.z;
    }
}