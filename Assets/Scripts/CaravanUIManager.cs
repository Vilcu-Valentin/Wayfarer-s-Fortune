using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaravanUIManager : MonoBehaviour
{
    [SerializeField] private CaravanManager caravanManager;
    [SerializeField] private WagonCustomizer wagonCustomizer;
    [SerializeField] private SelectedModuleUI selectedModuleUI;

    [Header("Wagon navigation and management")]
    [SerializeField] private GameObject prevWagonButton;
    [SerializeField] private GameObject nextWagonButton;
    [SerializeField] private GameObject addWagonButton;
    [SerializeField] private GameObject removeWagonButton;
    [SerializeField] private GameObject customizeButton;

    [Header("Caravan modes UI Groups")]
    [SerializeField] private GameObject buildMode;
    [SerializeField] private GameObject wagonMode;

    [Header("Storage Module Custom UI")]
    [SerializeField] private GameObject moduleCustomUi;

    private void Update()
    {
        if(caravanManager != null)
        {
            if(caravanManager.WagonCount == 0)
                customizeButton.SetActive(false);
            else
                customizeButton.SetActive(true);

            if(caravanManager.CurrentWagonIndex == 0 || caravanManager.WagonCount == 0)
                prevWagonButton.SetActive(false);  
            else
                prevWagonButton.SetActive(true);
            if(caravanManager.CurrentWagonIndex == caravanManager.WagonCount - 1 || caravanManager.WagonCount == 0)
                nextWagonButton.SetActive(false);
            else
                nextWagonButton.SetActive(true);


            if(caravanManager.WagonCount == 0)
                removeWagonButton.SetActive(false);
            else
                removeWagonButton.SetActive(true);

        }
        else
        {
            Debug.LogError("No caravan manager has been assigned!");
        }
    }

    private void HandleModuleSelected(StorageModule selectedModule)
    {
        if(selectedModule != null)
        {
            selectedModuleUI.UpdateSelectedModule(selectedModule);
            moduleCustomUi.SetActive(true);
        }
        else
        {
            moduleCustomUi.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (wagonCustomizer != null)
        {
            wagonCustomizer.OnModuleSelected += HandleModuleSelected;  // Subscribe to the event
        }
        else
        {
            Debug.LogError("WagonCustomizer is not assigned!");
        }
    }

    private void OnDisable()
    {
        if (wagonCustomizer != null)
        {
            wagonCustomizer.OnModuleSelected -= HandleModuleSelected;  // Unsubscribe from the event
        }
    }

    public void ExitBuildMode()
    {
        buildMode.SetActive(false);
        wagonMode.SetActive(true);
    }

    public void EnterBuildMode()
    {
        buildMode.SetActive(true);
        wagonMode.SetActive(false);
    }
}
