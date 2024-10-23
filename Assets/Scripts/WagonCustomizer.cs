// Handles module placement and customization for individual wagons
using Unity.VisualScripting;
using UnityEngine;

public class WagonCustomizer : MonoBehaviour
{
    [SerializeField] CaravanManager caravanManager;
    [SerializeField] InventoryManager inventoryManager;
    [SerializeField] private StorageModuleData selectedModule;
    [SerializeField] private float positionLerpSpeed = 15f;
    [SerializeField] private float rotationLerpSpeed = 20f;

    private ModulePlacementPreview previewManager;
    private ModulePlacementValidator placementValidator;

    [SerializeField] private LayerMask buildMask;
    [SerializeField] private LayerMask moduleMask;

    void Start()
    {
        previewManager = new ModulePlacementPreview();
        placementValidator = null;  // Set this to null initially, only initialize when needed
    }


    void Update()
    {
        // If there is no active wagon, or if we are inside an inventory we make sure to clear the preview and return
        if (!caravanManager.HasActiveWagon || inventoryManager.HasActiveStorage)
        {
            previewManager.ClearPreview();
            ClearPlacementValidator();
            return;
        }

        UpdatePlacementValidator();
        HandleModuleInput();


        // If there is no preview active we'll be able to select an already existing item
        if (!previewManager.IsActive) {
            ModuleSelector();
        }
        else
        {
            // If we do have a preview, we'll continue
            UpdateModulePlacement();
            previewManager.UpdateTransform(positionLerpSpeed, rotationLerpSpeed);
        }
    }

    // When you don't have a module to place selected, you will be able to select an already placed module
    private void ModuleSelector()
    {
        if (Input.GetMouseButtonDown(0)) {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, moduleMask))
            {
                Vector3Int? selectedCell = caravanManager.GridManager.WorldToGridPosition(hit.transform.position);
                if (selectedCell != null)
                {
                    /* temporary this will just be used to select a module, but for now we'll delete it
                    StorageModule moduleToDelete = caravanManager.CurrentWagon.GetModuleByGridCoords(selectedCell.Value);
                    if(moduleToDelete != null)
                        caravanManager.CurrentWagon.RemoveStorageModule(moduleToDelete); */
                    StorageModule selectedModule = caravanManager.CurrentWagon.GetModuleByGridCoords(selectedCell.Value);
                    if (selectedModule != null)
                        inventoryManager.EnterModule(selectedModule);
                }
            }
        }
    }

    // Creates a placement Validator (basically the script that handles if a module can be placed on that position or not)
    private void UpdatePlacementValidator()
    {
        ClearPlacementValidator();
        if (placementValidator == null && caravanManager.HasActiveWagon)
        {
            placementValidator = new ModulePlacementValidator(caravanManager.GridManager, caravanManager.CurrentWagon);
        }
    }

    // Clears the placement validator (used when switching wagons or exiting customization)
    private void ClearPlacementValidator()
    {
        placementValidator = null;
    }

    private void HandleModuleInput()
    {
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            // Start preview on current wagon
            if (caravanManager.HasActiveWagon)
            {
                previewManager.CreatePreview(selectedModule.graphics, caravanManager.CurrentWagon.transform.position);
            }
        }
        if (Input.GetKeyDown(KeyCode.R))
        {
            previewManager.Rotate();
        }
        if (Input.GetMouseButtonDown(1))
        {
            previewManager.ClearPreview();
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
        Vector3Int moduleSize = previewManager.GetRotatedSize(selectedModule.size);
        // We get the adjusted position (with the correct origin point for edge cases)
        Vector3Int adjustedPosition = placementValidator.AdjustPositionForPlacement(selectionPosition.Value, moduleSize);
        // Now we also adjust the y coordinate for the final placement position
        Vector3Int stackPosition = placementValidator.GetStackPosition(adjustedPosition, moduleSize);

        // We also update the preview position
        UpdatePreviewPosition(stackPosition, moduleSize);

        // Checks to see if it's a valid position
        bool canPlace = placementValidator.CheckFullBase(stackPosition, moduleSize) &&
                        caravanManager.GridManager.CanPlaceModule(stackPosition, moduleSize);
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
        Vector3 newPosition = caravanManager.GridManager.GridToWorldPosition(stackPosition);
        newPosition += new Vector3(moduleSize.x / 2f - 0.5f, 0, moduleSize.z / 2f - 0.5f);
        newPosition.y = (newPosition.y + moduleSize.y / 2f) * caravanManager.GridManager.cellSize;
        previewManager.UpdatePosition(newPosition);
    }

    // We get the correct final position, instantiate a gameobject and also update the storageModule list with the new module
    private void PlaceModule(Vector3Int position)
    {
        GameObject placedModule = Instantiate(
            selectedModule.graphics,
            previewManager.GetTargetPosition(),
            previewManager.GetCurrentRotation(),
            caravanManager.CurrentWagon.transform
        );

        StorageModule newModule = new StorageModule
        {
            moduleData = selectedModule,
            currentPosition = position,
            objectRef = placedModule,
            rotated = previewManager.IsRotated
        };

        caravanManager.CurrentWagon.AddStorageModule(newModule);
        previewManager.ClearPreview();
    }

    // Uses a raycast to get the grid position from the wagon)
    private Vector3Int? GetGridPositionFromMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildMask)
            ? caravanManager.GridManager.WorldToGridPosition(hit.point)
            : null;
    }
}