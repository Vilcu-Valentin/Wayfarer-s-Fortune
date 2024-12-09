using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaravanUIManager : MonoBehaviour
{
    [SerializeField] private Upgrade_CaravanManager caravanManager;

    [Header("Caravan modes UI Groups")]
    [SerializeField] private GameObject buildMode;
    [SerializeField] private GameObject wagonMode;
    [SerializeField] private GameObject caravanMode;

    [SerializeField] private GameObject removeWagonButton;

    private void Update()
    {
        if(caravanManager != null)
        {
            if(caravanManager.CurrentState == CaravanUpgradeState.CaravanMode)
            {
                caravanMode.SetActive(true);
                wagonMode.SetActive(false);
                buildMode.SetActive(false);
            }

            if (caravanManager.CurrentState == CaravanUpgradeState.WagonMode)
            {
                caravanMode.SetActive(false);
                wagonMode.SetActive(true);
                buildMode.SetActive(false);
            }

            if (caravanManager.CurrentState == CaravanUpgradeState.BuildMode)
            {
                caravanMode.SetActive(false);
                wagonMode.SetActive(false);
                buildMode.SetActive(true);
            }

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
        caravanManager.ToggleWagonBuildMode(false);
        caravanManager.SetCaravanState(CaravanUpgradeState.WagonMode);
    }

    public void EnterBuildMode()
    {
        caravanManager.ToggleWagonBuildMode(true);
        caravanManager.SetCaravanState(CaravanUpgradeState.BuildMode);
    }
}
