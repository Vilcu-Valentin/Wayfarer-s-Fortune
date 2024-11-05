using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Upgrade_CaravanManager : CaravanManager
{
    [SerializeField] private Vector3 wagonOffset; // Temporary it will be handled differently in the future
    [SerializeField] private float moveDuration = 0.5f; // Duration for the movement

    private int currentWagonIndex = -1;
    public int CurrentWagonIndex => currentWagonIndex; // Read-only access to the current wagon index
    public Wagon CurrentWagon { get; private set; }

    public void AddWagon(GameObject wagonPrefab)
    {
        Vector3 position = CalculateNextWagonPosition();
        GameObject wagonObject = Instantiate(wagonPrefab, position, Quaternion.identity);
        Wagon newWagon = wagonObject.GetComponent<Wagon>();

        Wagons.Add(newWagon);
        SetActiveWagon(Wagons.Count - 1);  // Set this new wagon as active
    }
    public override bool AddWagon(GameObject wagonPrefab, List<StorageModule> storageModules)
    {
        try
        {
            if (wagonPrefab == null) return false;

            Vector3 position = CalculateNextWagonPosition();
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
            SetActiveWagon(Wagons.Count - 1);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Error adding wagon: {e.Message}");
            return false;
        }
    }

    // Clears all current wagons.
    public override void ResetCaravan()
    {
        foreach (var wagon in Wagons)
        {
            Destroy(wagon.gameObject);
        }
        Wagons.Clear();
        currentWagonIndex = -1;
        CurrentWagon = null;
    }
    public void RemoveWagon()
    {
        Wagon wagonToRemove = CurrentWagon;

        Wagons.Remove(wagonToRemove);
        Destroy(wagonToRemove.gameObject);

        if (currentWagonIndex == 0)
            FocusNextWagon();
        FocusPreviousWagon();

        UpdateWagonPosition();
    }

    private Vector3 CalculateNextWagonPosition()
    {
        if (Wagons.Count == 0)
        {
            return transform.position + wagonOffset;
        }

        Wagon lastWagon = Wagons[Wagons.Count - 1];
        return lastWagon.transform.position + wagonOffset;
    }

    public void FocusNextWagon()
    {
        if (Wagons.Count == 0) return;
        SetActiveWagon((currentWagonIndex + 1) % Wagons.Count);

    }

    public void FocusPreviousWagon()
    {
        if (Wagons.Count == 0) return;
        SetActiveWagon((currentWagonIndex - 1 + Wagons.Count) % Wagons.Count);
    }

    public void StartCurrentWagonModule(StorageModuleData module)
    {
        CurrentWagon.GetComponent<ModuleManager>().StartModule(module);
    }

    // Make SetActiveWagon public so it can be called by SaveManager
    public void SetActiveWagon(int index)
    {
        if (index < 0 || index >= Wagons.Count) return;

        if (CurrentWagon != null)
        {
            CurrentWagon.GetComponent<ModuleManager>().IsActive = false;
            CurrentWagon.GetComponent<ModuleManager>().DeFocusCamera();
        }

        currentWagonIndex = index;
        CurrentWagon = Wagons[currentWagonIndex];
        CurrentWagon.GetComponent<ModuleManager>().IsActive = true;
        CurrentWagon.GetComponent<ModuleManager>().FocusCamera();
    }

    private void UpdateWagonPosition()
    {
        // Collect all the target positions for each wagon first
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < Wagons.Count; i++)
        {
            Vector3 targetPosition;
            if (i == 0)
                targetPosition = transform.position + wagonOffset;
            else
                targetPosition = targetPositions[i - 1] + wagonOffset;

            targetPositions.Add(targetPosition);
        }

        // Now move all wagons simultaneously
        StartCoroutine(MoveWagonsSmoothly(targetPositions));
    }
    private IEnumerator MoveWagonsSmoothly(List<Vector3> targetPositions)
    {
        float elapsedTime = 0f;
        List<Vector3> startingPositions = new List<Vector3>();

        // Store the starting positions of all wagons
        foreach (var wagon in Wagons)
        {
            startingPositions.Add(wagon.transform.position);
        }

        while (elapsedTime < moveDuration)
        {
            // Update the position of each wagon during the lerp
            for (int i = 0; i < Wagons.Count; i++)
            {
                Vector3 intermediatePosition = Vector3.Slerp(startingPositions[i], targetPositions[i], elapsedTime / moveDuration);
                intermediatePosition.y = Mathf.Lerp(startingPositions[i].y, targetPositions[i].y, elapsedTime / moveDuration);
                Wagons[i].transform.position = intermediatePosition;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure all wagons are exactly at their final positions at the end
        for (int i = 0; i < Wagons.Count; i++)
        {
            Wagons[i].transform.position = targetPositions[i];
            Wagons[i].GetComponent<GridManager>().InitializeGrid();
        }
    }
}
