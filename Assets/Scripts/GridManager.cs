// Core grid system functionality
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int length = 8;
    public int height = 4;
    public float cellSize = 1f;
    private Vector3 gridOrigin;

    void Start() => InitializeGrid();

    public void InitializeGrid()
    {
        gridOrigin = transform.position - new Vector3(width * cellSize / 2, 0, length * cellSize / 2);
    }

    public void InitializeGrid(Vector3 origin, float cellSize, Vector3Int gridSize)
    {
        this.cellSize = cellSize;
        width = gridSize.x;
        length = gridSize.z;
        height = gridSize.y;
        gridOrigin = origin - new Vector3(width * cellSize / 2, 0, length * cellSize / 2);
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

    public bool CanPlaceModule(Vector3Int gridPosition, Vector3Int moduleSize)
    {
        return IsValidPosition(gridPosition) &&
               IsValidPosition(gridPosition + moduleSize - Vector3Int.one);
    }

    void OnDrawGizmos()
    {
        if (!Application.isPlaying) return;
        DrawGridGizmos();
    }

    private void DrawGridGizmos()
    {
        Gizmos.color = Color.white;
        for (int x = 0; x < width; x++)
            for (int y = 0; y < height; y++)
                for (int z = 0; z < length; z++)
                {
                    Vector3 cellCenter = GridToWorldPosition(new Vector3Int(x, y, z));
                    Gizmos.DrawWireCube(cellCenter, Vector3.one * cellSize * 0.9f);
                }
    }
}