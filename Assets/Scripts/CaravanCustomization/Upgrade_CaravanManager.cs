using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;
<<<<<<< HEAD
=======

public enum CaravanUpgradeState
{
    CaravanMode,
    WagonMode,
    BuildMode
}
>>>>>>> develop

public enum CaravanUpgradeState
{
    CaravanMode,
    WagonMode
}

// TODO, RIGHT NOW: Add state manager between caravanMode <-> wagonMode <-> moduleMode
public class Upgrade_CaravanManager : CaravanManager
{
    [SerializeField] private UnityEvent<float> onUpdateDollyLength; // Unity Event to call UpdateDollyLength

<<<<<<< HEAD
    public Wagon CurrentWagon { get; private set; }
    public CaravanUpgradeState CurrentState { get; private set; } = CaravanUpgradeState.CaravanMode;

    [SerializeField] private float dragThreshold = 1.0f; // Threshold for z-axis movement to trigger reorder


    private bool isDragging = false;
    private Wagon draggedWagon = null;
    private float dragStartZ;
    private int initialIndex;
    private float clickStartTime;
    private bool potentialDrag = false;
    private float dragTimer = 0.1f;

=======
    public CaravanBody CurrentWagon { get; private set; }
    public CaravanUpgradeState CurrentState { get; private set; } = CaravanUpgradeState.CaravanMode;

>>>>>>> develop
    void Update()
    {
        if (CurrentState == CaravanUpgradeState.CaravanMode)
        {
<<<<<<< HEAD
            // Left Mouse Button Down
            if (Input.GetMouseButtonDown(0))
            {
                clickStartTime = Time.time;
                potentialDrag = true;
            }

            // Left Mouse Button Held
            if (Input.GetMouseButton(0))
            {
                if (potentialDrag && Time.time - clickStartTime > dragTimer)
                {
                    // If held longer than threshold, start dragging
                    StartDragging();
                    potentialDrag = false; // Prevent further actions
                    isDragging = true;
                }
            }

            // Dragging handling
            if (isDragging && Input.GetMouseButton(0))
            {
                HandleDragging();
            }

            // Left Mouse Button Up
            if (Input.GetMouseButtonUp(0))
            {
                if (potentialDrag && Time.time - clickStartTime <= dragTimer)
                {
                    // Short click - perform wagon selection
                    Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                    if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                    {
                        Wagon selectedWagon = hit.transform.gameObject.GetComponent<Wagon>();
                        if (selectedWagon == null)
                            selectedWagon = hit.transform.gameObject.GetComponentInParent<Wagon>();
                        if (selectedWagon != null)
                            SetActiveWagon(selectedWagon, true);
                    }
                }

                // Stop dragging if it was active
                if (isDragging)
                {
                    StopDragging();
                    isDragging = false;
                }

                // Reset drag potential
                potentialDrag = false;
            }

            // Right Mouse Button handling remains the same
            if (Input.GetMouseButtonDown(1))
                if (CurrentWagon != null)
                    CurrentWagon.GetComponent<ModuleManager>().DeFocusCamera();
        }
    }

    private void StartDragging()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            draggedWagon = hit.transform.gameObject.GetComponent<Wagon>();
            if (draggedWagon == null)
                draggedWagon = hit.transform.gameObject.GetComponentInParent<Wagon>();

            if (draggedWagon != null)
            {
                isDragging = true;
                dragStartZ = draggedWagon.transform.position.z;
                initialIndex = Wagons.IndexOf(draggedWagon);
=======
            // Left Mouse Button Up
            if (Input.GetMouseButtonDown(0))
            {
                // Short click - perform wagon selection
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
                {
                    CaravanBody selectedWagon = hit.transform.gameObject.GetComponent<CaravanBody>();
                    if (selectedWagon == null)
                        selectedWagon = hit.transform.gameObject.GetComponentInParent<CaravanBody>();
                    if (selectedWagon != null)
                    {
                        SetActiveWagon(selectedWagon, true);
                        SetCaravanState(CaravanUpgradeState.WagonMode);
                    }
                }
            }
        }

        if(CurrentState != CaravanUpgradeState.BuildMode)
        {
            // Right Mouse Button handling remains the same
            if (Input.GetMouseButtonDown(1))
            {
                if (CurrentWagon != null)
                    CurrentWagon.GetComponent<WagonCamera>().DeFocusCamera();
                SetCaravanState(CaravanUpgradeState.CaravanMode);
                CurrentWagon = null;
>>>>>>> develop
            }
        }
    }

<<<<<<< HEAD
    private void HandleDragging()
    {
        if (draggedWagon == null) return;

        // Cast a ray from the mouse position to detect the world position
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity))
        {
            // Calculate the offset along the caravan's local z-axis
            Vector3 localHitPoint = transform.InverseTransformPoint(hit.point); // Convert hit point to caravan's local space
            float currentZ = localHitPoint.z; // Use the local z-position of the hit
            float dragOffset = currentZ - dragStartZ;

            Debug.Log($"Local Mouse Z: {currentZ}, Drag Offset: {dragOffset}");

            // Check if the offset exceeds the threshold
            if (Mathf.Abs(dragOffset) > dragThreshold)
            {
                int direction = dragOffset > 0 ? -1 : 1; // Determine direction (-1 = forward, +1 = backward)
                int newIndex = initialIndex + direction;

                // Ensure the new index is valid
                if (newIndex >= 0 && newIndex < Wagons.Count)
                {
                    // Swap wagons in the list
                    Wagons.RemoveAt(initialIndex);
                    Wagons.Insert(newIndex, draggedWagon);

                    // Update positions and reset drag data
                    UpdateWagonPosition();
                    dragStartZ = currentZ; // Reset drag start for smooth snapping
                    initialIndex = newIndex; // Update index
                }
            }
        }
    }

    private void StopDragging()
    {
        isDragging = false;
        draggedWagon = null;
    }


=======
>>>>>>> develop
    // Public method used by UI
    public void AddWagon(GameObject wagonPrefab)
    {
        AddWagonInternal(wagonPrefab, null);
    }
    // Public method used for loading from save
    public override bool AddWagon(GameObject wagonPrefab, List<StorageModule> storageModules)
    {
        return AddWagonInternal(wagonPrefab, storageModules);
    }
    // Private helper to handle shared logic
    private bool AddWagonInternal(GameObject wagonPrefab, List<StorageModule> storageModules)
    {
        if (wagonPrefab == null)
            return false;

        try
        {
            Vector3 position = CalculateNextWagonPosition(wagonPrefab.GetComponent<Wagon>().spacing);
            GameObject wagonObject = Instantiate(wagonPrefab, position, Quaternion.identity);
            Wagon newWagon = wagonObject.GetComponent<Wagon>();

            if (newWagon == null)
            {
                Destroy(wagonObject);
                return false;
            }

            if (storageModules != null)
            {
                foreach (StorageModule storageModule in storageModules)
                {
                    newWagon.AddStorageModule(storageModule);
                }
            }

            Wagons.Add(newWagon);
<<<<<<< HEAD
            SetActiveWagon(newWagon, false);
=======
>>>>>>> develop
            UpdateDollyLength(position.z);

            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding wagon: {e.Message}");
            return false;
        }
    }

    public void RemoveWagon()
    {
<<<<<<< HEAD
        if (CurrentWagon == null) return;
        Wagon wagonToRemove = CurrentWagon;
=======
        if (CurrentWagon == null || CurrentWagon.GetType() != typeof(Wagon)) return;
        Wagon wagonToRemove = (Wagon)CurrentWagon;
>>>>>>> develop

        Wagons.Remove(wagonToRemove);
        Destroy(wagonToRemove.gameObject);

<<<<<<< HEAD
=======
        SetCaravanState(CaravanUpgradeState.CaravanMode);

>>>>>>> develop
        UpdateWagonPosition();
    }
    // Clears all current wagons.
    public override void ResetCaravan()
    {
        foreach (var wagon in Wagons)
        {
            Destroy(wagon.gameObject);
        }
        Wagons.Clear();
        CurrentWagon = null;
    }

    public void StartCurrentWagonModule(StorageModuleData module)
    {
        CurrentWagon.GetComponent<ModuleManager>().StartModule(module);
    }


    private void UpdateDollyLength(float zPos)
    {
        onUpdateDollyLength.Invoke(zPos);
<<<<<<< HEAD
    }

    // Make SetActiveWagon public so it can be called by SaveManager
    public void SetActiveWagon(Wagon wagon, bool focusCamera)
    {
        if (CurrentWagon != null)
        {
            CurrentWagon.GetComponent<ModuleManager>().IsActive = false;
            CurrentWagon.GetComponent<ModuleManager>().DeFocusCamera();
        }

        CurrentWagon = wagon;
        if(focusCamera)
            CurrentWagon.GetComponent<ModuleManager>().FocusCamera();
    }

    public void ToggleWagonBuildMode(bool mode)
    {
        CurrentWagon.GetComponent<ModuleManager>().IsActive = mode;
=======
    }

    // Make SetActiveWagon public so it can be called by SaveManager
    public void SetActiveWagon(CaravanBody wagon, bool focusCamera)
    {
        if (CurrentWagon != null)
        {
            if(CurrentWagon.GetType() == typeof(Wagon))
                CurrentWagon.GetComponent<ModuleManager>().IsActive = false;
            CurrentWagon.GetComponent<WagonCamera>().DeFocusCamera();
        }

        CurrentWagon = wagon;
        if(focusCamera)
            CurrentWagon.GetComponent<WagonCamera>().FocusCamera();
    }

    public void ToggleWagonBuildMode(bool mode)
    {
        if(CurrentWagon.GetType() == typeof(Wagon))
            CurrentWagon.GetComponent<ModuleManager>().IsActive = mode;
>>>>>>> develop
    }


    public void SetCaravanState(CaravanUpgradeState newState)
    {
        CurrentState = newState;
    }

    // Calculates and individual wagon's position
    private Vector3 CalculateWagonPosition(Vector3 basePosition, Vector2 spacing, Vector2 previousSpacing)
    {
        return basePosition + new Vector3(0, 0, -previousSpacing.x / 2 - spacing.x / 2 + spacing.y);
    }

    private Vector3 CalculateNextWagonPosition(Vector2 spacing)
    {
        if (Wagons.Count == 0)
        {
<<<<<<< HEAD
            // TEMPORARY, the locomotive will also have a spacing parameter, but for now, use this
            return transform.position + new Vector3(0, 0, spacing.y - 5);
=======
            return CalculateWagonPosition(locomotive.transform.position, spacing, locomotive.spacing);
>>>>>>> develop
        }

        Wagon lastWagon = Wagons[^1];
        return CalculateWagonPosition(lastWagon.transform.position, spacing, lastWagon.spacing);
    }
<<<<<<< HEAD

=======
>>>>>>> develop
    // Re-calculates wagon position according to their current order in the list
    private void UpdateWagonPosition()
    {
        List<Vector3> targetPositions = new List<Vector3>();
        Vector3 currentPosition = transform.position;

        for (int i = 0; i < Wagons.Count; i++)
        {
            Vector3 targetPosition;
            if (i == 0)
            {
<<<<<<< HEAD
                targetPosition = currentPosition + new Vector3(0, 0, Wagons[i].spacing.y - 5);
=======

                targetPosition = CalculateWagonPosition(locomotive.transform.position, Wagons[i].spacing, locomotive.spacing);
>>>>>>> develop
            }
            else
            {
                targetPosition = CalculateWagonPosition(targetPositions[i - 1], Wagons[i].spacing, Wagons[i - 1].spacing);
            }

            targetPositions.Add(targetPosition);
        }

        if (targetPositions.Count > 0)
        {
            UpdateDollyLength(targetPositions[^1].z);
        }

        // Start movement for all wagons
        for (int i = 0; i < Wagons.Count; i++)
        {
            var mover = Wagons[i].GetComponent<WagonSmoothMover>();
            mover?.StartMoving(targetPositions[i]);
        }
    }

}
