using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaravanUIManager : MonoBehaviour
{
    [SerializeField] private Upgrade_CaravanManager caravanManager;

<<<<<<< HEAD
    [Header("Wagon navigation and management")]
    [SerializeField] private GameObject addWagonButton;
    [SerializeField] private GameObject removeWagonButton;
    [SerializeField] private GameObject customizeButton;

    [Header("Caravan modes UI Groups")]
    [SerializeField] private GameObject buildMode;
    [SerializeField] private GameObject wagonMode;

    [Header("Storage Module UI")]
    [SerializeField] private GameObject moduleCustomUi;
=======
    [Header("Caravan modes UI Groups")]
    [SerializeField] private GameObject buildMode;
    [SerializeField] private GameObject wagonMode;
    [SerializeField] private GameObject caravanMode;
>>>>>>> develop


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

<<<<<<< HEAD
=======
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

>>>>>>> develop


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
<<<<<<< HEAD
        buildMode.SetActive(false);
        wagonMode.SetActive(true);

        caravanManager.ToggleWagonBuildMode(false);
        caravanManager.SetCaravanState(CaravanUpgradeState.CaravanMode);
=======
        caravanManager.ToggleWagonBuildMode(false);
        caravanManager.SetCaravanState(CaravanUpgradeState.WagonMode);
>>>>>>> develop
    }

    public void EnterBuildMode()
    {
<<<<<<< HEAD
        buildMode.SetActive(true);
        wagonMode.SetActive(false);

        caravanManager.ToggleWagonBuildMode(true);
        caravanManager.SetCaravanState(CaravanUpgradeState.WagonMode);
=======
        caravanManager.ToggleWagonBuildMode(true);
        caravanManager.SetCaravanState(CaravanUpgradeState.BuildMode);
>>>>>>> develop
    }
}
