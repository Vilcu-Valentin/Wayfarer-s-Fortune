using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryScene;
    [SerializeField] private GameObject normalVolume;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private StorageManager storageManager;
    [SerializeField] private GameObject buildSpace;
    [SerializeField] private ItemData selectedItem;
    [SerializeField] private float positionLerpSpeed = 15f;
    [SerializeField] private float rotationLerpSpeed = 20f;
    [SerializeField] private LayerMask buildMask;
    [SerializeField] private LayerMask moduleMask;

    private ModulePlacementPreview previewManager;
    private InventoryVisualizer visualizer;

    public StorageModule CurrentStorage { get; private set; }
    public GameObject InventoryScene => inventoryScene;
    public bool HasActiveStorage => CurrentStorage != null;

    private void Start()
    {
        previewManager = new ModulePlacementPreview();
        visualizer = new InventoryVisualizer(Camera.main, normalVolume, moduleMask, buildMask);
    }

    private void Update()
    {
        HandleModuleInput();

        if (!previewManager.IsActive)
            HandleModuleSelection();
        else
        {
            UpdateModulePlacement();
            previewManager.UpdateTransform(positionLerpSpeed, rotationLerpSpeed);
        }
    }

    private void HandleModuleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
            previewManager.CreatePreview(selectedItem.graphics, InventoryScene.transform.position);

        if (Input.GetKeyDown(KeyCode.R))
            previewManager.Rotate();

        if (Input.GetMouseButtonDown(1))
            previewManager.ClearPreview();

        if (Input.GetKeyDown(KeyCode.Escape))
            ExitStorage();
    }

    // When you don't have a module to place selected, you will be able to select an already placed module
    private void HandleModuleSelection()
    {
        if (!Input.GetMouseButtonDown(1)) return;

        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, moduleMask)) return;

        Vector3Int? selectedCell = gridManager.WorldToGridPosition(hit.transform.position);
        if (!selectedCell.HasValue) return;

        Item itemToDelete = storageManager.GetModuleAtPosition(selectedCell.Value);
        if (itemToDelete != null)
            storageManager.RemoveItem(itemToDelete);
    }

    private void UpdateModulePlacement()
    {
        // Seems unnecessary since we already check it update ?
        if (!previewManager.IsActive) return;

        // We get the selected cell position (if it exists - meaning is withing the build volume)
        Vector3Int? selectionPosition = GetGridPositionFromMouseRay();
        if (!selectionPosition.HasValue) return;

        // We get the module size from the previewManager (this script knows what kind of module we have selected) TODO change the name of the function to GetSize
        Vector3Int moduleSize = previewManager.GetRotatedSize(selectedItem.size);
        // We get the adjusted position (with the correct origin point for edge cases)
        Vector3Int adjustedPosition = storageManager.AdjustPositionForBounds(selectionPosition.Value, moduleSize);
        // Now we also adjust the y coordinate for the final placement position
        Vector3Int stackPosition = storageManager.GetStackPosition(adjustedPosition, moduleSize);

        // We also update the preview position
        UpdatePreviewPosition(stackPosition, moduleSize);

        // Checks to see if it's a valid position
        bool canPlace = gridManager.CanPlaceModule(stackPosition, moduleSize);
        // Change the preivew color
        previewManager.SetColor(canPlace ? Color.green : Color.red);

        // Handle placement on click
        if (canPlace && Input.GetMouseButtonDown(0))
        {
            PlaceModule(stackPosition);
        }
    }

    // Perform the calculations to transform from gridPosition to worldPosition, normalise it and then update the preview 
    private void UpdatePreviewPosition(Vector3Int stackPosition, Vector3Int moduleSize)
    {
        Vector3 newPosition = gridManager.GridToWorldPosition(stackPosition);
        newPosition += new Vector3(
            moduleSize.x / 2f - 0.5f,
            (moduleSize.y / 2f) * gridManager.cellSize,
            moduleSize.z / 2f - 0.5f
        );
        previewManager.UpdatePosition(newPosition);
    }

    // We get the correct final position, instantiate a gameobject and also update the storageModule list with the new module
    private void PlaceModule(Vector3Int position)
    {
        GameObject placedModule = Instantiate(
            selectedItem.graphics,
            previewManager.GetTargetPosition(),
            previewManager.GetCurrentRotation(),
            InventoryScene.transform
        );

        var newItem = new Item(
            selectedItem,
            position,
            placedModule,
            previewManager.IsRotated
        );

        placedModule.layer = 9;
        storageManager.AddItem(newItem);
        previewManager.ClearPreview();
    }

    // Uses a raycast to get the grid position from the wagon)
    private Vector3Int? GetGridPositionFromMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildMask)
            ? gridManager.WorldToGridPosition(hit.point)
            : null;
    }


    public void EnterStorage(StorageModule storage)
    {
        CurrentStorage = storage;
        storageManager.ClearSpawnedItems();

        InitializeStorageView(storage);
        storageManager.SpawnStoredItems();

        visualizer.SetInventoryView(true);
        inventoryScene.SetActive(true);
    }

    public void ExitStorage()
    {
        storageManager.ClearSpawnedItems();
        CurrentStorage = null;
        visualizer.SetInventoryView(false);
        inventoryScene.SetActive(false);
    }

    private void InitializeStorageView(StorageModule storage)
    {
        if (storage.items == null)
            storage.items = new List<Item>();

        Vector3 storagePos = storage.objectRef.transform.position;
        buildSpace.transform.localScale = new Vector3(
            storage.moduleData.inventorySize.x,
            0.05f,
            storage.moduleData.inventorySize.z
        );

        inventoryScene.transform.position = storagePos;
        gridManager.InitializeGrid(
            storagePos,
            1f,
            storage.moduleData.inventorySize
        );
    }
}
