using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaravanManager : MonoBehaviour
{
    [SerializeField] private GameObject wagonPrefab; // Temporary it will be selected from the UI
    [SerializeField] private Vector3 wagonOffset; // Temporary it will be handled differently in the future
    [SerializeField] private float moveDuration = 0.5f; // Duration for the movement

    private GridManager currentGridManager;  // Add GridManager reference
    private List<Wagon> wagons = new List<Wagon>();
    private int currentWagonIndex = -1;

    public Wagon CurrentWagon { get; private set; }
    public bool HasActiveWagon => CurrentWagon != null;
    public GridManager GridManager => currentGridManager;  // Expose GridManager

    void Update()
    {
        HandleWagonManagement();
    }

    private void HandleWagonManagement()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            AddWagon();
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            FocusPreviousWagon();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            FocusNextWagon();
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            RemoveWagon();
        }
    }

    private void AddWagon()
    {
        Vector3 position = CalculateNextWagonPosition();
        GameObject wagonObject = Instantiate(wagonPrefab, position, Quaternion.identity);
        Wagon newWagon = wagonObject.GetComponent<Wagon>();

        wagons.Add(newWagon);
        SetActiveWagon(wagons.Count - 1);  // Set this new wagon as active
        UpdateWagonHighlights();
    }

    private void RemoveWagon()
    {
        Wagon wagonToRemove = CurrentWagon;

        wagons.Remove(wagonToRemove);
        Destroy(wagonToRemove.gameObject);

        if (currentWagonIndex == 0)
            FocusNextWagon();
        FocusPreviousWagon();

        UpdateWagonPosition();
    }

    private void UpdateWagonPosition()
    {
        // Collect all the target positions for each wagon first
        List<Vector3> targetPositions = new List<Vector3>();

        for (int i = 0; i < wagons.Count; i++)
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
        foreach (var wagon in wagons)
        {
            startingPositions.Add(wagon.transform.position);
        }

        while (elapsedTime < moveDuration)
        {
            // Update the position of each wagon during the lerp
            for (int i = 0; i < wagons.Count; i++)
            {
                Vector3 intermediatePosition = Vector3.Slerp(startingPositions[i], targetPositions[i], elapsedTime / moveDuration);
                intermediatePosition.y = Mathf.Lerp(startingPositions[i].y, targetPositions[i].y, elapsedTime / moveDuration);
                wagons[i].transform.position = intermediatePosition;
            }

            elapsedTime += Time.deltaTime;
            yield return null;
        }

        // Ensure all wagons are exactly at their final positions at the end
        for (int i = 0; i < wagons.Count; i++)
        {
            wagons[i].transform.position = targetPositions[i];
            wagons[i].GetComponent<GridManager>().InitializeGrid();
        }
    }


    private Vector3 CalculateNextWagonPosition()
    {
        if (wagons.Count == 0)
        {
            return transform.position + wagonOffset;
        }

        Wagon lastWagon = wagons[wagons.Count - 1];
        return lastWagon.transform.position + wagonOffset;
    }

    private void FocusNextWagon()
    {
        if (wagons.Count == 0) return;
        CurrentWagon.DeFocusCamera();
        SetActiveWagon((currentWagonIndex + 1) % wagons.Count);
        CurrentWagon.FocusCamera();
    }

    private void FocusPreviousWagon()
    {
        if (wagons.Count == 0) return;
        CurrentWagon.DeFocusCamera();
        SetActiveWagon((currentWagonIndex - 1 + wagons.Count) % wagons.Count);
        CurrentWagon.FocusCamera();
    }

    private void SetActiveWagon(int index)
    {
        if (index < 0 || index >= wagons.Count) return;

        currentWagonIndex = index;
        CurrentWagon = wagons[currentWagonIndex];
        currentGridManager = CurrentWagon.GetComponent<GridManager>();
        currentGridManager.InitializeGrid();
        UpdateWagonHighlights();
    }

    private void UpdateWagonHighlights()
    {
        foreach (var wagon in wagons)
        {
            wagon.SetHighlight(wagon == CurrentWagon);
        }
    }
}
