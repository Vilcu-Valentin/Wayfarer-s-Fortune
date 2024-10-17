using System.Collections.Generic;
using UnityEngine;

public class CaravanManager : MonoBehaviour
{
    [SerializeField] private GameObject wagonPrefab; // Temporary it will be selected from the UI
    [SerializeField] private Vector3 wagonOffset; // Temporary it will be handled differently in the future
    
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

    private Vector3 CalculateNextWagonPosition()
    {
        if (wagons.Count == 0)
        {
            return transform.position;
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
