using UnityEngine;

public class CaravanCustomizer : MonoBehaviour
{
    [SerializeField] private StorageModuleData selectModule;
    [SerializeField] private Wagon currentWagon;
    [SerializeField] private GridManager gridManager;
    private GameObject selectedModule = null;

    void Update()
    {
        Vector3Int? selectionPosition = GetGridPositionFromMouseRay();

        if (Input.GetKeyDown("1"))
        {
            selectedModule = Instantiate(selectModule.graphics, currentWagon.transform.position, Quaternion.identity);
        }

        if (selectedModule != null && selectionPosition.HasValue)
        {
            Vector3Int adjustedPosition = AdjustPositionForPlacement(selectionPosition.Value, selectModule.size);

            if (gridManager.CanPlaceModule(adjustedPosition, selectModule.size))
            {
                Vector3 finalPos = gridManager.GridToWorldPosition(adjustedPosition);
                finalPos += new Vector3(selectModule.size.x / 2f - 0.5f, selectModule.size.y / 2f, selectModule.size.z / 2f - 0.5f);
                finalPos.y = selectModule.size.y /(float) 2f;
                selectedModule.transform.position = finalPos;
                // Optionally handle adding to the wagon's state
                // if the placement is confirmed (e.g., on mouse click)
            }
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