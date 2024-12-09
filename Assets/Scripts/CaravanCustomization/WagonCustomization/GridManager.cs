// Core grid system functionality
using UnityEngine;

public class GridManager : MonoBehaviour
{
    public int width = 5;
    public int length = 8;
    public int height = 4;
    public float cellSize = 1f;
    private Vector3 gridOrigin;

    public Transform buildVolume;

    [HideInInspector] public Quaternion gridRotation;
    private Matrix4x4 rotationMatrix;

    void Awake() => InitializeGrid();

    public void InitializeGrid()
    {
        // Use transform's global rotation directly
        gridRotation = transform.rotation;
        rotationMatrix = Matrix4x4.Rotate(gridRotation);

        // Adjust grid origin based on global rotation and center point
        gridOrigin = buildVolume.position - rotationMatrix.MultiplyVector(new Vector3(width * cellSize / 2, -cellSize / 2, length * cellSize / 2));
    }

    public void InitializeGrid(Vector3 origin, float cellSize, Vector3Int gridSize)
    {
        this.cellSize = cellSize;
        width = gridSize.x;
        length = gridSize.z;
        height = gridSize.y;

        // Use transform's global rotation directly
        gridRotation = transform.rotation * buildVolume.transform.rotation;
        rotationMatrix = Matrix4x4.Rotate(gridRotation);

        gridOrigin = origin - rotationMatrix.MultiplyVector(new Vector3(width * cellSize / 2, -cellSize / 2, length * cellSize / 2));
    }

    public Vector3Int WorldToGridPosition(Vector3 worldPosition)
    {
        // Convert world to grid position using inverse of the global rotation
        Vector3 localPosition = Quaternion.Inverse(gridRotation) * (worldPosition - gridOrigin);

        return new Vector3Int(
            Mathf.FloorToInt(localPosition.x / cellSize),
            Mathf.FloorToInt(localPosition.y / cellSize),
            Mathf.FloorToInt(localPosition.z / cellSize)
        );
    }

    public Vector3 GridToWorldPosition(Vector3Int gridPosition)
    {
        Vector3 localPosition = new Vector3(
            gridPosition.x * cellSize + cellSize / 2,
            gridPosition.y * cellSize + cellSize / 2,
            gridPosition.z * cellSize + cellSize / 2
        );

        // Apply global rotation to calculate world position
        return gridOrigin + rotationMatrix.MultiplyPoint3x4(localPosition);
    }

    public Vector3 GridToAdjustedWorldPosition(Vector3Int position, Vector3Int size)
    {
        Vector3 adjustedPosition = GridToWorldPosition(position);
        adjustedPosition += rotationMatrix.MultiplyVector(new Vector3(size.x / 2f - 0.5f, size.y / 2f - 0.5f, size.z / 2f - 0.5f)) * cellSize;
        return adjustedPosition;
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
