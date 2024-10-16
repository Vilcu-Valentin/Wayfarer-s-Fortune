using UnityEngine;

public class CaravanCustomizer : MonoBehaviour
{
    [SerializeField] private StorageModuleData selectModule;
    [SerializeField] private Wagon currentWagon;
    [SerializeField] private GridManager gridManager;
    private GameObject selectedModulePreview = null;

    void Update()
    {
        Vector3Int? selectionPosition = GetGridPositionFromMouseRay();

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            if (selectedModulePreview != null)
            {
                Destroy(selectedModulePreview);
            }
            selectedModulePreview = Instantiate(selectModule.graphics, currentWagon.transform.position, Quaternion.identity);
        }

        if (selectedModulePreview != null && selectionPosition.HasValue)
        {
            Vector3Int adjustedPosition = AdjustPositionForPlacement(selectionPosition.Value, selectModule.size);
            Vector3Int? stackPosition = GetStackPosition(adjustedPosition, selectModule.size);

            if (stackPosition.HasValue && gridManager.CanPlaceModule(stackPosition.Value, selectModule.size))
            {
                Vector3 finalPos = gridManager.GridToWorldPosition(stackPosition.Value);
                finalPos += new Vector3(selectModule.size.x / 2f - 0.5f, 0, selectModule.size.z / 2f - 0.5f);
                finalPos.y = (stackPosition.Value.y + selectModule.size.y / 2f) * gridManager.cellSize;
                selectedModulePreview.transform.position = finalPos;

                SetPreviewColor(Color.green);

                if (Input.GetMouseButtonDown(0)) // Left mouse button
                {
                    PlaceModule(stackPosition.Value);
                }
            }
            else
            {
                // Move the preview even if placement is invalid
                Vector3 invalidPos = gridManager.GridToWorldPosition(adjustedPosition);
                invalidPos += new Vector3(selectModule.size.x / 2f - 0.5f, 0, selectModule.size.z / 2f - 0.5f);
                invalidPos.y = (adjustedPosition.y + selectModule.size.y / 2f) * gridManager.cellSize;
                selectedModulePreview.transform.position = invalidPos;

                SetPreviewColor(Color.red);
            }
        }

        if (Input.GetMouseButtonDown(1)) // Right mouse button
        {
            CancelPlacement();
        }
    }

    Vector3Int? GetStackPosition(Vector3Int basePosition, Vector3Int moduleSize)
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
        return null; // Cannot stack at this position
    }

    bool CanStackAt(Vector3Int position, Vector3Int moduleSize)
    {
        // Check if the space is free
        if (currentWagon.IsSpaceOccupied(position, moduleSize))
        {
            return false;
        }

        // If it's on the ground level, we can place it
        if (position.y == 0)
        {
            return true;
        }

        // Check if there's a full base underneath
        for (int x = position.x; x < position.x + moduleSize.x; x++)
        {
            for (int z = position.z; z < position.z + moduleSize.z; z++)
            {
                if (!currentWagon.IsSpaceOccupied(new Vector3Int(x, position.y - 1, z), Vector3Int.one))
                {
                    return false;
                }
            }
        }

        return true;
    }

    void PlaceModule(Vector3Int position)
    {
        GameObject placedModule = Instantiate(selectModule.graphics, selectedModulePreview.transform.position, Quaternion.identity);
        StorageModule newModule = new StorageModule
        {
            moduleData = selectModule,
            currentPosition = position,
            objectRef = placedModule
        };
        currentWagon.AddStorageModule(newModule);
        Destroy(selectedModulePreview);
        selectedModulePreview = null;
    }

    void CancelPlacement()
    {
        if (selectedModulePreview != null)
        {
            Destroy(selectedModulePreview);
            selectedModulePreview = null;
        }
    }

    void SetPreviewColor(Color color)
    {
        Renderer[] renderers = selectedModulePreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    Vector3Int? GetGridPositionFromMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int gridPosition = gridManager.WorldToGridPosition(hit.point);
            return gridPosition;
        }
        return null;
    }

    Vector3Int AdjustPositionForPlacement(Vector3Int position, Vector3Int moduleSize)
    {
        Vector3Int gridSize = new Vector3Int(gridManager.width, gridManager.height, gridManager.length);
        Vector3Int adjustedPosition = position;

        // Adjust X position
        if (position.x + moduleSize.x > gridSize.x)
        {
            adjustedPosition.x = gridSize.x - moduleSize.x;
        }

        // Adjust Y position
        if (position.y + moduleSize.y > gridSize.y)
        {
            adjustedPosition.y = gridSize.y - moduleSize.y;
        }

        // Adjust Z position
        if (position.z + moduleSize.z > gridSize.z)
        {
            adjustedPosition.z = gridSize.z - moduleSize.z;
        }

        // Ensure the adjusted position is not negative
        adjustedPosition.x = Mathf.Max(0, adjustedPosition.x);
        adjustedPosition.y = Mathf.Max(0, adjustedPosition.y);
        adjustedPosition.z = Mathf.Max(0, adjustedPosition.z);

        return adjustedPosition;
    }
}