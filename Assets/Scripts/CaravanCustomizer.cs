using UnityEngine;
using UnityEngine.UIElements;

public class CaravanCustomizer : MonoBehaviour
{
    [SerializeField] private StorageModuleData selectModule;
    [SerializeField] private Wagon currentWagon;
    [SerializeField] private GridManager gridManager;

    [SerializeField] private float positionLerpSpeed = 15f;
    [SerializeField] private float rotationLerpSpeed = 20f;

    private GameObject selectedModulePreview = null;
    private bool isRotated = false;

    private Vector3 targetPosition;
    private Quaternion targetRotation;

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
            isRotated = false;
            targetPosition = selectedModulePreview.transform.position;
            targetRotation = selectedModulePreview.transform.rotation;
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            RotateModule();
        }

        if (selectedModulePreview != null && selectionPosition.HasValue)
        {
            Vector3Int moduleSize = GetRotatedSize(selectModule.size);
            Vector3Int adjustedPosition = AdjustPositionForPlacement(selectionPosition.Value, moduleSize);
            Vector3Int stackPosition = GetStackPosition(adjustedPosition, moduleSize);

            Vector3 newTargetPos = gridManager.GridToWorldPosition(stackPosition);
            newTargetPos += new Vector3(moduleSize.x / 2f - 0.5f, 0, moduleSize.z / 2f - 0.5f);
            newTargetPos.y = (stackPosition.y + moduleSize.y / 2f) * gridManager.cellSize;
            targetPosition = newTargetPos;

            if (CheckFullBase(stackPosition, moduleSize) && gridManager.CanPlaceModule(stackPosition, moduleSize))
            {
                SetPreviewColor(Color.green);

                if (Input.GetMouseButtonDown(0))
                {
                    PlaceModule(stackPosition, targetPosition, targetRotation);
                }
            }
            else
            {
                SetPreviewColor(Color.red);
            }
        }

        if (selectedModulePreview != null)
        {
            selectedModulePreview.transform.position = Vector3.Lerp(
                selectedModulePreview.transform.position,
                targetPosition,
                positionLerpSpeed * Time.deltaTime
            );

            selectedModulePreview.transform.rotation = Quaternion.Lerp(
                selectedModulePreview.transform.rotation,
                targetRotation,
                rotationLerpSpeed * Time.deltaTime
            );
        }

        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    void RotateModule()
    {
        if (selectedModulePreview != null)
        {
            isRotated = !isRotated;
            targetRotation *= Quaternion.Euler(0, 90, 0);
        }
    }

    // Adjust module size based on its rotation state
    Vector3Int GetRotatedSize(Vector3Int originalSize)
    {
        return isRotated ? new Vector3Int(originalSize.z, originalSize.y, originalSize.x) : originalSize;
    }

    // Get the stacking position for the module
    Vector3Int GetStackPosition(Vector3Int basePosition, Vector3Int moduleSize)
    {
        Vector3Int currentPosition = basePosition;

        // Iterate upwards in the grid to find a valid stacking position
        while (currentPosition.y + moduleSize.y <= gridManager.height)
        {
            if (CanStackAt(currentPosition, moduleSize))
            {
                return currentPosition;
            }
            currentPosition.y++;
        }
        return currentPosition; // Default to the highest valid position
    }

    // Check if the module can be stacked at a given position
    bool CanStackAt(Vector3Int position, Vector3Int moduleSize)
    {
        // Ensure the space is unoccupied
        return !currentWagon.IsSpaceOccupied(position, moduleSize);
    }

    // Ensure there is a full base underneath the module for placement
    bool CheckFullBase(Vector3Int position, Vector3Int moduleSize)
    {
        if (position.y == 0)
            return true; // If at ground level, no need for a base check

        // Check the base under each module grid cell
        for (int x = position.x; x < position.x + moduleSize.x; x++)
        {
            for (int z = position.z; z < position.z + moduleSize.z; z++)
            {
                // If any space beneath the module is unoccupied, placement is invalid
                if (!currentWagon.IsSpaceOccupied(new Vector3Int(x, position.y - 1, z), Vector3Int.one))
                {
                    return false;
                }
            }
        }
        return true;
    }

    // Modified PlaceModule to use target transforms
    void PlaceModule(Vector3Int position, Vector3 finalPosition, Quaternion finalRotation)
    {
        GameObject placedModule = Instantiate(selectModule.graphics, finalPosition, finalRotation);

        StorageModule newModule = new StorageModule
        {
            moduleData = selectModule,
            currentPosition = position,
            objectRef = placedModule,
            rotated = isRotated
        };

        currentWagon.AddStorageModule(newModule);

        Destroy(selectedModulePreview);
        selectedModulePreview = null;
        isRotated = false;
    }

    // Cancel the current module placement
    void CancelPlacement()
    {
        if (selectedModulePreview != null)
        {
            Destroy(selectedModulePreview);
            selectedModulePreview = null;
            isRotated = false;
        }
    }

    // Change the color of the preview model to indicate placement validity
    void SetPreviewColor(Color color)
    {
        Renderer[] renderers = selectedModulePreview.GetComponentsInChildren<Renderer>();
        foreach (Renderer renderer in renderers)
        {
            renderer.material.color = color;
        }
    }

    // Get the grid position from the mouse cursor using a raycast
    Vector3Int? GetGridPositionFromMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

        // Perform the raycast to find the grid position
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            Vector3Int gridPosition = gridManager.WorldToGridPosition(hit.point);
            return gridPosition;
        }
        return null; // Return null if no valid position is found
    }

    // Adjust the module position if it's out of bounds or overlaps with grid edges
    Vector3Int AdjustPositionForPlacement(Vector3Int position, Vector3Int moduleSize)
    {
        Vector3Int gridSize = new Vector3Int(gridManager.width, gridManager.height, gridManager.length);
        Vector3Int adjustedPosition = position;

        // Adjust for X axis if module exceeds grid boundaries
        if (position.x + moduleSize.x > gridSize.x)
        {
            adjustedPosition.x = gridSize.x - moduleSize.x;
        }

        // Adjust for Y axis if module exceeds grid height
        if (position.y + moduleSize.y > gridSize.y)
        {
            adjustedPosition.y = gridSize.y - moduleSize.y;
        }

        // Adjust for Z axis if module exceeds grid length
        if (position.z + moduleSize.z > gridSize.z)
        {
            adjustedPosition.z = gridSize.z - moduleSize.z;
        }

        // Ensure position values are not negative
        adjustedPosition.x = Mathf.Max(0, adjustedPosition.x);
        adjustedPosition.y = Mathf.Max(0, adjustedPosition.y);
        adjustedPosition.z = Mathf.Max(0, adjustedPosition.z);

        return adjustedPosition;
    }
}
