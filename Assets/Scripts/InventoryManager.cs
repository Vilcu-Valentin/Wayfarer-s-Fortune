using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryScene;
    [SerializeField] private GameObject normalVolume;
    [SerializeField] private GameObject outsideLight;
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
    private ExplodedViewController explodedViewController;

    private GameObject hoveredObject;

    public StorageModule CurrentStorage { get; private set; }
    public GameObject InventoryScene => inventoryScene;
    public bool HasActiveStorage => CurrentStorage != null;

    private void Start()
    {
        previewManager = new ModulePlacementPreview();
        visualizer = new InventoryVisualizer(Camera.main, normalVolume, outsideLight, moduleMask, buildMask);
        explodedViewController = new ExplodedViewController();
    }

    private void Update()
    {
        HandleModuleInput();

        if (!previewManager.IsActive)
            HandleModuleSelection(); 
        else if (!explodedViewController.IsExploded)
        {
            UpdateModulePlacement();
            previewManager.UpdateTransform(positionLerpSpeed, rotationLerpSpeed);
        }
        else
        {
            previewManager.ClearPreview();
        }

        if (explodedViewController.IsTransitioning)
        {
            explodedViewController.UpdateTransition();
        }
    }

    private void HandleModuleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        { 
            previewManager.CreatePreview(selectedItem.graphics, InventoryScene.transform.position);
            if(explodedViewController.IsExploded)
                explodedViewController.ToggleExplodedView(CurrentStorage.items, gridManager.cellSize);
        }

        if (Input.GetKeyDown(KeyCode.R))
            previewManager.Rotate();

        if (Input.GetMouseButtonDown(1))
            previewManager.ClearPreview();

        if (Input.GetKeyDown(KeyCode.Escape))
            ExitStorage();
        if (Input.GetKeyDown(KeyCode.V))
        {
            explodedViewController.ToggleExplodedView(CurrentStorage.items, gridManager.cellSize);
        }
    }

    // When you don't have a module to place selected, you will be able to select an already placed module
    private void HandleModuleSelection()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, moduleMask)) return;

        if (!Input.GetMouseButtonDown(1)) return;

        Debug.Log(hit.transform + " at position" + gridManager.WorldToGridPosition(hit.transform.position));

        Vector3 position = hit.transform.position;
        if (explodedViewController.IsExploded)
        {
            position = explodedViewController.TranslateExplodedToGridPosition(hit.transform.gameObject, gridManager.cellSize);
            Debug.Log("It was in exploded view, so tranformed position is: " + gridManager.WorldToGridPosition(position));
        }

        Vector3Int? selectedCell = gridManager.WorldToGridPosition(position);
        if (!selectedCell.HasValue) return;

        Item itemToDelete = storageManager.GetModuleAtPosition(selectedCell.Value);
        if (itemToDelete != null)
            storageManager.RemoveItem(itemToDelete);
    }

    // TEMPORARY
    private void HandleHover(GameObject currentHover)
    {
        if (currentHover != hoveredObject)
        {
            if(hoveredObject != null)
                hoveredObject.GetComponent<Renderer>().material.color = Color.yellow;
            hoveredObject = currentHover;
            hoveredObject.GetComponent<Renderer>().material.color = Color.blue;
        }
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
            (moduleSize.y / 2f) * gridManager.cellSize - 0.5f,
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
        explodedViewController.Reset(); // This will handle resetting positions if in exploded view
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
