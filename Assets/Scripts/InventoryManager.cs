using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;

public class InventoryManager : MonoBehaviour
{
    [SerializeField] private GameObject inventoryScene;
    [SerializeField] private GameObject normalVolume;
    [SerializeField] private GridManager gridManager;
    [SerializeField] private Storage storageManager;
    [SerializeField] private GameObject buildSpace;

    public StorageModule CurrentStorage { get; private set; }
    public GameObject InventoryScene { get; private set; }
    public bool HasActiveStorage => CurrentStorage != null;

    [SerializeField] private ItemData selectedItem;
    [SerializeField] private float positionLerpSpeed = 15f;
    [SerializeField] private float rotationLerpSpeed = 20f;

    private ModulePlacementPreview previewManager;

    [SerializeField] private LayerMask buildMask;
    [SerializeField] private LayerMask moduleMask;

    void Start()
    {
        previewManager = new ModulePlacementPreview();
        InventoryScene = inventoryScene;
    }


    void Update()
    {
        HandleModuleInput();

        // If there is no preview active we'll be able to select an already existing item
        if (!previewManager.IsActive)
        {
            ModuleSelector();
        }
        else
        {
            // If we do have a preview, we'll continue
            UpdateModulePlacement();
            previewManager.UpdateTransform(positionLerpSpeed, rotationLerpSpeed);
        }
    }

    private void UpdateCameraMask(bool inventoryCamera)
    {
        // Get the HDRP camera component
        HDAdditionalCameraData hdCamera = Camera.main.GetComponent<HDAdditionalCameraData>();

        if (hdCamera != null)
        {
            if (inventoryCamera)
            {
                // Set background type for HDRP camera
                hdCamera.backgroundColorHDR = new Color(17f / 255f, 5f / 255f, 39f / 255f) * Mathf.Pow(2, 1); // Or whatever color you want
                hdCamera.clearColorMode = HDAdditionalCameraData.ClearColorMode.Color;
                // Update culling mask using binary OR operation for combining masks
                Camera.main.cullingMask = moduleMask | buildMask;
                normalVolume.SetActive(false);
            }
            else
            {
                // Reset to show skybox
                hdCamera.clearColorMode = HDAdditionalCameraData.ClearColorMode.Sky;
                // Set culling mask to everything (-1 represents all layers)
                Camera.main.cullingMask = -1;
                normalVolume.SetActive(true);
            }
        }

    }

    // When you don't have a module to place selected, you will be able to select an already placed module
    private void ModuleSelector()
    {
        if (Input.GetMouseButtonDown(1))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, moduleMask))
            {
                Vector3Int? selectedCell = gridManager.WorldToGridPosition(hit.transform.position);
                if (selectedCell != null)
                {
                    // temporary this will just be used to select a module, but for now we'll delete it
                    Item itemToDelete = storageManager.GetModuleByGridCoords(selectedCell.Value);
                    if(itemToDelete != null)
                        storageManager.RemoveStorageModule(itemToDelete); 
                }
            }
        }
    }

    private void HandleModuleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Start preview 
            previewManager.CreatePreview(selectedItem.graphics, InventoryScene.transform.position);
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            previewManager.Rotate();
        }
        if (Input.GetMouseButtonDown(1))
        {
            previewManager.ClearPreview();
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ExitModule();
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
        Vector3Int adjustedPosition = storageManager.AdjustPositionForPlacement(selectionPosition.Value, moduleSize);
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
        newPosition += new Vector3(moduleSize.x / 2f - 0.5f, 0, moduleSize.z / 2f - 0.5f);
        newPosition.y = (newPosition.y + moduleSize.y / 2f) * gridManager.cellSize;
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

        Item newItem = new Item
        {
            ItemData = selectedItem,
            currentPosition = position,
            objectRef = placedModule,
            rotated = previewManager.IsRotated
        };

        storageManager.AddItemReference(placedModule);

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


    public void EnterModule(StorageModule currentStorage)
    {
        this.CurrentStorage = currentStorage;

        // Clear any previously spawned objects
        storageManager.ClearSpawnedItems();

        buildSpace.transform.localScale = new Vector3(currentStorage.moduleData.inventorySize.x, 0.05f, currentStorage.moduleData.inventorySize.z);

        // Initialize items list if null
        if (CurrentStorage.items == null)
        {
            CurrentStorage.items = new List<Item>();
        }
        else
        {
            // Spawn any existing items
            storageManager.SpawnItems();
        }

        InventoryScene.transform.position = currentStorage.objectRef.transform.position;
        gridManager.InitializeGrid(InventoryScene.transform.position, 1f, currentStorage.moduleData.inventorySize);
        UpdateCameraMask(true);
        InventoryScene.SetActive(true);
    }

    // Update ExitModule to clear spawned items
    public void ExitModule()
    {
        storageManager.ClearSpawnedItems();
        CurrentStorage = null;
        UpdateCameraMask(false);
        InventoryScene.SetActive(false);
    }
}
