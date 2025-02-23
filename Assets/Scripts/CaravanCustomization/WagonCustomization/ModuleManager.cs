using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModuleManager : MonoBehaviour 
{
    public bool IsActive = false;

    [SerializeField] private Wagon wagon;
    [SerializeField] private LayerMask buildMask;
    [SerializeField] private GridManager grid;


    private ModulePlacementValidator placementValidator;
    private ModulePositionValidator positionValidator;
    private ModulePreviewValidator previewValidator;

    private StorageModuleData selectedModule = null;

    public void Start()
    {
        placementValidator = new ModulePlacementValidator(grid, wagon);
        positionValidator = new ModulePositionValidator();
        previewValidator = new ModulePreviewValidator();
    }

    public void Update()
    {
        if (!IsActive)
        {
            previewValidator.ClearPreview();
            return;
        }

        if(Input.GetMouseButtonDown(1))
            previewValidator.ClearPreview();

        if (previewValidator.IsActive)
        {
            if (Input.GetKeyDown(KeyCode.R))
            {
                positionValidator.Rotate();
                previewValidator.Rotate();
            }

            UpdateModulePosition();
            previewValidator.UpdateTransform(10f, 15f);
        }
        else
        {
            if(Input.GetMouseButtonDown(0))
            {
                SelectModule();
            }
        }
    }

    // This is currently removing the module instead of just selecting it
    public void SelectModule()
    {
        Vector3Int? gridCell = GetModulePositionFromMouseRay();
        if (gridCell == null) return;

        StorageModule selectedModule = GetModuleByGridCoords(gridCell.Value);
        if (selectedModule == null) return;

        if (placementValidator.IsSpaceOccupiedAbove(selectedModule))
        {
            Debug.Log("There is a module above! Can't remove!");
            return;
        }    

        wagon.RemoveStorageModule(selectedModule);
    }

    // Maybe rename this to something better ?
    public void UpdateModulePosition()
    {
        // We make sure that the target position is on the grid and not outside
        Vector3Int? targetPosition = GetGridPositionFromMouseRay();
        if (!targetPosition.HasValue) return;

        // We get the adjusted size of the module (the PreviewManager already knows if it's rotated or not)
        Vector3Int moduleSize = positionValidator.GetSize(selectedModule.size);
        // We adjust the position for placement (this includes edge cases like last row or column)
        Vector3Int adjustedPosition = placementValidator.AdjustPositionForPlacement(targetPosition.Value, moduleSize);
        // We now get the stack position of the module 
        Vector3Int stackPosition = placementValidator.GetStackPosition(adjustedPosition, moduleSize);

        previewValidator.UpdatePosition(grid.GridToAdjustedWorldPosition(stackPosition, moduleSize));

        // Checks to see if it's a valid position
        bool canPlace = placementValidator.IsPlacementValid(stackPosition, moduleSize);
        // Change the preivew color
        previewValidator.SetColor(canPlace ? Color.green : Color.red);

        // If the position is valid and we press LMB we can place the module
        if (canPlace && Input.GetMouseButtonDown(0))
        {
            CreateModule(stackPosition);
        }
    }

    public void CreateModule(Vector3Int stackPosition)
    {
        StorageModule newModule = new StorageModule
        {
            moduleData = selectedModule,
            currentPosition = stackPosition,
            rotated = positionValidator.IsRotated,
            rotation = previewValidator.GetCurrentRotation()
        };

        wagon.AddStorageModule(newModule);
    }

    // Maybe better naming ?
    public void StartModule(StorageModuleData module)
    {
        selectedModule = module;
        positionValidator.ResetRotation();
        previewValidator.CreatePreview(module.graphics, transform.position);
    }

    // Returns a module from a grid position (cell)
    public StorageModule GetModuleByGridCoords(Vector3Int gridCoords)
    {
        foreach (StorageModule module in wagon.storageModules)
            if (placementValidator.IsPositionWithinModule(gridCoords, module.currentPosition, module.Size)) return module;

        return null;
    }

    // Returns the position of a selected grid cell
    private Vector3Int? GetGridPositionFromMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildMask)
            ? grid.WorldToGridPosition(hit.point)
            : null;
    }

    private Vector3Int? GetModulePositionFromMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildMask)
            ? grid.WorldToGridPosition(hit.transform.position)
            : null;
    }
}
