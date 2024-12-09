using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UIElements;

public enum CaravanUpgradeState
{
    CaravanMode,
    WagonMode,
    BuildMode
}

// TODO, RIGHT NOW: Add state manager between caravanMode <-> wagonMode <-> moduleMode
public class Upgrade_CaravanManager : CaravanManager
{
    [SerializeField] private UnityEvent<float> onUpdateDollyLength; // Unity Event to call UpdateDollyLength

    public CaravanBody CurrentWagon { get; private set; }
    public CaravanUpgradeState CurrentState { get; private set; } = CaravanUpgradeState.CaravanMode;

    void Update()
    {
        if (CurrentState == CaravanUpgradeState.CaravanMode)
        {
            // TEMPORARY JUST FOR SHOW TOMMOROW ^^^^
            if(Input.GetKeyDown(KeyCode.Escape))
            {
                Application.Quit();
            }

            if (Input.GetMouseButtonDown(0) && Input.GetKey(KeyCode.X))
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
                        SetActiveWagon(selectedWagon, false);
                    }
                }
                return;
            }
            // TEMPORARY ^^^^

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
            }
        }
    }

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
        if (CurrentWagon == null || CurrentWagon.GetType() != typeof(Wagon)) return;
        Wagon wagonToRemove = (Wagon)CurrentWagon;

        Wagons.Remove(wagonToRemove);
        Destroy(wagonToRemove.gameObject);

        SetCaravanState(CaravanUpgradeState.CaravanMode);

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
            return CalculateWagonPosition(locomotive.transform.position, spacing, locomotive.spacing);
        }

        Wagon lastWagon = Wagons[^1];
        return CalculateWagonPosition(lastWagon.transform.position, spacing, lastWagon.spacing);
    }
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

                targetPosition = CalculateWagonPosition(locomotive.transform.position, Wagons[i].spacing, locomotive.spacing);
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
