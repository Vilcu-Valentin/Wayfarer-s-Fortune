using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CaravanUIManager : MonoBehaviour
{
    [SerializeField] private Upgrade_CaravanManager caravanManager;

    [Header("Wagon navigation and management")]
    [SerializeField] private GameObject addWagonButton;
    [SerializeField] private GameObject removeWagonButton;
    [SerializeField] private GameObject customizeButton;

    [Header("Caravan modes UI Groups")]
    [SerializeField] private GameObject buildMode;
    [SerializeField] private GameObject wagonMode;

    [Header("Storage Module UI")]
    [SerializeField] private GameObject moduleCustomUi;


    private void Update()
    {
        if(caravanManager != null)
        {
            if(caravanManager.WagonCount == 0)
                customizeButton.SetActive(false);
            else
                customizeButton.SetActive(true);


            if(caravanManager.WagonCount == 0)
                removeWagonButton.SetActive(false);
            else
                removeWagonButton.SetActive(true);

            if (caravanManager.CurrentWagon != null)
                removeWagonButton.SetActive(true);
            else
                removeWagonButton.SetActive(false);

        }
        else
        {
            Debug.LogError("No caravan manager has been assigned!");
        }
    }

    public void ExitBuildMode()
    {
        buildMode.SetActive(false);
        wagonMode.SetActive(true);

        caravanManager.ToggleWagonBuildMode(false);
        caravanManager.SetCaravanState(CaravanUpgradeState.CaravanMode);
    }

    public void EnterBuildMode()
    {
        buildMode.SetActive(true);
        wagonMode.SetActive(false);

        caravanManager.ToggleWagonBuildMode(true);
        caravanManager.SetCaravanState(CaravanUpgradeState.WagonMode);
    }
}
