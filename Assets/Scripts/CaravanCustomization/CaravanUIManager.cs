using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class CaravanUIManager : MonoBehaviour
{
    [SerializeField] private Upgrade_CaravanManager caravanManager;
    [SerializeField] private UpgradesManager upgradesManager; // Reference to UpgradesManager

    [Header("Caravan Modes UI Groups")]
    [SerializeField] private GameObject buildMode;
    [SerializeField] private GameObject wagonMode;
    [SerializeField] private GameObject caravanMode;

    [SerializeField] private GameObject removeWagonButton;

    [Header("Upgrade Selection UI Elements")]
     [SerializeField] private TMP_Dropdown frameDropdown;
     [SerializeField] private TMP_Dropdown wheelsDropdown;
     [SerializeField] private TMP_Dropdown engineDropdown;
     [SerializeField] private TMP_Dropdown canopyDropdown;

    private bool isInitializing = true; // Flag to prevent triggering callbacks during initialization

    private void Start()
    {
        if (upgradesManager == null)
        {
            Debug.LogError("UpgradesManager reference is missing in CaravanUIManager!");
            return;
        }

        // Initialize Dropdowns
        InitializeDropdowns();

        // Add listeners to Dropdowns
        frameDropdown.onValueChanged.AddListener(OnFrameSelected);
        wheelsDropdown.onValueChanged.AddListener(OnWheelsSelected);
        engineDropdown.onValueChanged.AddListener(OnEngineSelected);
        canopyDropdown.onValueChanged.AddListener(OnCanopySelected);

        // Initially populate frame dropdown and set default selections
        PopulateFrameDropdown();

        isInitializing = false;
    }

    private void Update()
    {
        if (caravanManager != null)
        {
            // Manage UI Groups based on Caravan State
            switch (caravanManager.CurrentState)
            {
                case CaravanUpgradeState.CaravanMode:
                    caravanMode.SetActive(true);
                    wagonMode.SetActive(false);
                    buildMode.SetActive(false);
                    break;

                case CaravanUpgradeState.WagonMode:
                    caravanMode.SetActive(false);
                    wagonMode.SetActive(true);
                    buildMode.SetActive(false);
                    break;

                case CaravanUpgradeState.BuildMode:
                    caravanMode.SetActive(false);
                    wagonMode.SetActive(false);
                    buildMode.SetActive(true);
                    break;
            }

            // Manage Remove Wagon Button visibility
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

    /// <summary>
    /// Initializes Dropdowns by clearing existing options and setting initial states.
    /// </summary>
    private void InitializeDropdowns()
    {
        // Clear existing options
        frameDropdown.ClearOptions();
        wheelsDropdown.ClearOptions();
        engineDropdown.ClearOptions();
        canopyDropdown.ClearOptions();

        // No "Select" option. Dropdowns will always have a selected option after population.

        // Disable upgrade dropdowns initially until a frame is selected
        wheelsDropdown.interactable = false;
        engineDropdown.interactable = false;
        canopyDropdown.interactable = false;
    }

    /// <summary>
    /// Populates the Frame Dropdown with available frames from UpgradesManager.
    /// Automatically selects the first frame and triggers dependent dropdown population.
    /// </summary>
    private void PopulateFrameDropdown()
    {
        frameDropdown.ClearOptions();
        var frameOptions = upgradesManager.frames.Select(f => f.name).ToList();
        frameDropdown.AddOptions(frameOptions);

        if (frameOptions.Count > 0)
        {
            // Automatically select the first frame
            frameDropdown.value = 0;
            OnFrameSelected(0);
        }
        else
        {
            Debug.LogWarning("No frames available to populate the Frame Dropdown.");
        }
    }

    /// <summary>
    /// Callback when a frame is selected from the Frame Dropdown.
    /// Sets the selected frame and updates dependent dropdowns.
    /// </summary>
    /// <param name="index">Selected index.</param>
    private void OnFrameSelected(int index)
    {
        if (isInitializing) return;

        if (index < 0 || index >= upgradesManager.frames.Length)
        {
            Debug.LogError("Invalid frame index selected.");
            return;
        }

        var selectedFrame = upgradesManager.frames[index];
        upgradesManager.SetFrame(selectedFrame);

        // Automatically select the first available wheels for the new frame
        PopulateWheelsDropdown(autoSelect: true);

        // Populate Engine and Canopy Dropdowns based on vehicle type and frame
        PopulateUpgradeDropdowns();
    }

    /// <summary>
    /// Populates the Wheels Dropdown based on the selected frame.
    /// Optionally auto-selects the first available wheels.
    /// </summary>
    /// <param name="autoSelect">Whether to automatically select the first available wheel.</param>
    private void PopulateWheelsDropdown(bool autoSelect = false)
    {
        wheelsDropdown.ClearOptions();
        var availableWheels = upgradesManager.GetAvailableWheels();

        if (availableWheels != null && availableWheels.Length > 0)
        {
            wheelsDropdown.interactable = true;
            var wheelOptions = availableWheels.Select(w => w.name).ToList();
            wheelsDropdown.AddOptions(wheelOptions);

            if (autoSelect)
            {
                wheelsDropdown.value = 0;
                OnWheelsSelected(0);
            }
        }
        else
        {
            wheelsDropdown.interactable = false;
            wheelsDropdown.AddOptions(new List<string> { "No Wheels Available" });
        }
    }

    /// <summary>
    /// Populates the Engine and Canopy Dropdowns based on the selected frame and vehicle type.
    /// Automatically selects the first available option if applicable.
    /// </summary>
    private void PopulateUpgradeDropdowns()
    {
        // Populate Engine Dropdown if Locomotive
        if (upgradesManager.GetAvailableEngines() != null && upgradesManager.GetAvailableEngines().Length > 0)
        {
            engineDropdown.ClearOptions();
            engineDropdown.interactable = true;
            var engineOptions = upgradesManager.GetAvailableEngines().Select(e => e.name).ToList();
            engineDropdown.AddOptions(engineOptions);

            // Automatically select the first engine
            engineDropdown.value = 0;
            OnEngineSelected(0);
        }
        else
        {
            engineDropdown.ClearOptions();
            engineDropdown.interactable = false;
            engineDropdown.AddOptions(new List<string> { "No Engines Available" });
        }

        // Populate Canopy Dropdown if Wagon
        if (upgradesManager.GetAvailableCanopies() != null && upgradesManager.GetAvailableCanopies().Length > 0)
        {
            canopyDropdown.ClearOptions();
            canopyDropdown.interactable = true;
            var canopyOptions = upgradesManager.GetAvailableCanopies().Select(c => c.name).ToList();
            canopyDropdown.AddOptions(canopyOptions);

            // Automatically select the first canopy
            canopyDropdown.value = 0;
            OnCanopySelected(0);
        }
        else
        {
            canopyDropdown.ClearOptions();
            canopyDropdown.interactable = false;
            canopyDropdown.AddOptions(new List<string> { "No Canopies Available" });
        }
    }

    /// <summary>
    /// Callback when wheels are selected from the Wheels Dropdown.
    /// Sets the selected wheels in UpgradesManager.
    /// </summary>
    /// <param name="index">Selected index.</param>
    private void OnWheelsSelected(int index)
    {
        if (isInitializing) return;

        var availableWheels = upgradesManager.GetAvailableWheels();
        if (availableWheels != null && index < availableWheels.Length)
        {
            var selectedWheels = availableWheels[index];
            upgradesManager.SetWheels(selectedWheels);
        }
        else
        {
            Debug.LogWarning("Invalid wheels index selected.");
        }
    }

    /// <summary>
    /// Callback when engine is selected from the Engine Dropdown.
    /// Sets the selected engine in UpgradesManager.
    /// </summary>
    /// <param name="index">Selected index.</param>
    private void OnEngineSelected(int index)
    {
        if (isInitializing) return;

        var availableEngines = upgradesManager.GetAvailableEngines();
        if (availableEngines != null && index < availableEngines.Length)
        {
            var selectedEngine = availableEngines[index];
            upgradesManager.SetEngine(selectedEngine);
        }
        else
        {
            Debug.LogWarning("Invalid engine index selected.");
        }
    }

    /// <summary>
    /// Callback when canopy is selected from the Canopy Dropdown.
    /// Sets the selected canopy in UpgradesManager.
    /// </summary>
    /// <param name="index">Selected index.</param>
    private void OnCanopySelected(int index)
    {
        if (isInitializing) return;

        var availableCanopies = upgradesManager.GetAvailableCanopies();
        if (availableCanopies != null && index < availableCanopies.Length)
        {
            var selectedCanopy = availableCanopies[index];
            upgradesManager.SetCanopy(selectedCanopy);
        }
        else
        {
            Debug.LogWarning("Invalid canopy index selected.");
        }
    }

    /// <summary>
    /// Exits Build Mode and switches to Wagon Mode.
    /// </summary>
    public void ExitBuildMode()
    {
        caravanManager.ToggleWagonBuildMode(false);
        caravanManager.SetCaravanState(CaravanUpgradeState.WagonMode);
    }

    /// <summary>
    /// Enters Build Mode from Wagon Mode.
    /// </summary>
    public void EnterBuildMode()
    {
        caravanManager.ToggleWagonBuildMode(true);
        caravanManager.SetCaravanState(CaravanUpgradeState.BuildMode);
    }
}
