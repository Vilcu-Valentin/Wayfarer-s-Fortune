using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorageModuleGridUI : MonoBehaviour
{
    public StorageModuleData module;

    public GameObject gridCell; // Prefab for a single grid cell
    public int cellSize; // Size of each cell
    public RectTransform gridContainer; // Container for the grid cells

    private void Start()
    {
        InitializeGrid();
    }

    public void InitializeGrid()
    {
        // Get the grid dimensions
        Vector2Int size = module.inventorySize;

        // Set the grid container size based on the total grid dimensions
        gridContainer.sizeDelta = new Vector2(size.x * cellSize, size.y * cellSize);
        gridContainer.GetComponent<GridLayoutGroup>().cellSize = new Vector2Int(cellSize - 1, cellSize - 1);
        gridContainer.GetComponent<GridLayoutGroup>().spacing = new Vector2Int(1, 1);

        // Clear any existing cells in the container (optional)
        foreach (Transform child in gridContainer)
        {
            Destroy(child.gameObject);
        }

        // Loop through each cell position in the grid
        for (int x = 0; x < size.x; x++)
        {
            for (int y = 0; y < size.y; y++)
            {
                // Instantiate a new cell
                GameObject newCell = Instantiate(gridCell, gridContainer);
            }
        }
    }
}
