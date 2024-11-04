using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEditor.MPE;

public class ModuleManager : MonoBehaviour 
{
    public bool IsActive = false;

    [SerializeField] private CinemachineFreeLook lookCamera;

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
        if(!IsActive) return;

        if (!selectedModule) return;

        if (Input.GetKeyDown(KeyCode.R))
        { 
            positionValidator.Rotate();
            previewValidator.Rotate();
        }

        if(Input.GetMouseButtonDown(1))
            previewValidator.ClearPreview();

        if (previewValidator.IsActive)
        {
            UpdateModulePosition();
            previewValidator.UpdateTransform(10f, 15f);
        }
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

    // Returns the position of a selected grid cell
    private Vector3Int? GetGridPositionFromMouseRay()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        return Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, buildMask)
            ? grid.WorldToGridPosition(hit.point)
            : null;
    }

    //TEMPORARY PLACED HERE
    // Used to set a high priority on this camera
    public void FocusCamera()
    {
        lookCamera.Priority = 100;
    }

    // Used to set a low priority on this camera
    public void DeFocusCamera()
    {
        lookCamera.Priority = 1;
    }
}
