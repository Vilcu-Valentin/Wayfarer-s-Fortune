using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int length = 8;
    public int height = 4;
    [SerializeField] private float cellSize = 1f;

    private Vector3 gridOrigin;

    void Start()
    {
        InitializeGrid();
    }

    void InitializeGrid()
    {
        gridOrigin = transform.position - new Vector3(width * cellSize / 2, 0, length * cellSize / 2);
    }

    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        Vector3 localPosition = worldPosition - gridOrigin;
        return new Vector3Int(
            Mathf.FloorToInt(localPosition.x / cellSize),
            Mathf.FloorToInt(localPosition.y / cellSize),
            Mathf.FloorToInt(localPosition.z / cellSize)
        );
    }

    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        return gridOrigin + new Vector3(
            gridPosition.x * cellSize + cellSize / 2,
            gridPosition.y * cellSize + cellSize / 2,
            gridPosition.z * cellSize + cellSize / 2
        );
    }

    public bool IsValidPosition(Vector3Int gridPosition)
    {
        return gridPosition.x >= 0 && gridPosition.x < width &&
               gridPosition.y >= 0 && gridPosition.y < height &&
               gridPosition.z >= 0 && gridPosition.z < length;
    }

    // Checks if the module is inside the build volume // Later it will also check if there is another module in the way
    public bool CanPlaceModule(Vector3Int? gridPosition, Vector3Int moduleSize)
    {
        if(gridPosition != null)
        {
            if (gridPosition.Value.x + moduleSize.x > width)
                return false;
            if( gridPosition.Value.y + moduleSize.y > height)
                return false;
            if(gridPosition.Value.z + moduleSize.z > length)
                return false;
            return true;
        }
        return false;
    }

    // Optional: Visualize the grid (this can stay in the GridManager)
    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;

        Gizmos.color = Color.white;
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                for (int z = 0; z < length; z++)
                {
                    Vector3 cellCenter = GridToWorldPosition(new Vector3Int(x, y, z));
                    Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize * 0.9f);
                }
            }
        }
    }
}
