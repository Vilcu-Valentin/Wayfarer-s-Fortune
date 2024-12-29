using UnityEngine;

public enum VehicleType
{
    Locomotive,
    Wagon
}

/// <summary>
/// Manages upgrades for a specific vehicle (Locomotive or Wagon)
/// </summary>
public class UpgradesManager : MonoBehaviour
{
    [Header("Vehicle Configuration")]
    [SerializeField] private VehicleType _vehicleType;

    [Header("Upgrade Transforms")]
    [SerializeField] private Transform _frameTransform;
    [SerializeField] private Transform _wheelsTransform;
    [SerializeField] private Transform _customAddonTransform;

    [Header("Current Upgrades")]
    [SerializeField] private FrameData _currentFrame;
    [SerializeField] private WheelData _currentWheels;
    [SerializeField] private EngineData _currentEngine;
    [SerializeField] private CanopyData _currentCanopy;

    [Header("Upgrade Database")]
    public FrameData[] frames;
    public EngineData[] engines;
    public CanopyData[] canopies;

    /// <summary>
    /// Updates the entire vehicle's graphics and potentially underlying data
    /// </summary>
    public void UpdateVehicleUpgrades()
    {
        // Clear existing graphics
        ClearTransformChildren(_frameTransform);
        ClearTransformChildren(_wheelsTransform);
        ClearTransformChildren(_customAddonTransform);

        // Reposition transforms based on current frame
        UpdateTransformPositions();

        // Instantiate new graphics
        InstantiateUpgradeGraphics();
    }

    /// <summary>
    /// Clears all children of a given transform
    /// </summary>
    private void ClearTransformChildren(Transform targetTransform)
    {
        foreach (Transform child in targetTransform)
        {
            Destroy(child.gameObject);
        }
    }

    /// <summary>
    /// Updates transform positions based on current frame's offset
    /// </summary>
    private void UpdateTransformPositions()
    {
        if (_currentFrame == null) return;

        // Maintain original positioning logic: localPosition = vehicle's position + frame's position offset
        _frameTransform.position = transform.position + _currentFrame.position[0];
        _wheelsTransform.position = transform.position + _currentFrame.position[1];
        _customAddonTransform.position = transform.position + _currentFrame.position[2];
    }

    /// <summary>
    /// Instantiates graphics for current upgrades based on vehicle type
    /// </summary>
    private void InstantiateUpgradeGraphics()
    {
        // Frame graphics
        if (_currentFrame?.graphics != null)
            Instantiate(_currentFrame.graphics, _frameTransform);

        // Wheels graphics
        if (_currentWheels?.graphics != null)
            Instantiate(_currentWheels.graphics, _wheelsTransform);

        // Custom addon graphics (different based on vehicle type)
        switch (_vehicleType)
        {
            case VehicleType.Locomotive:
                if (_currentEngine?.graphics != null)
                    Instantiate(_currentEngine.graphics, _customAddonTransform);
                break;
            case VehicleType.Wagon:
                if (_currentCanopy?.graphics != null)
                    Instantiate(_currentCanopy.graphics, _customAddonTransform);
                break;
        }
    }

    // Setter methods with validation
    public void SetFrame(FrameData frame)
    {
        _currentFrame = frame;

        // Validate current wheels
        if (_currentFrame.wheels != null && _currentWheels != null)
        {
            bool wheelsValid = false;
            foreach (var allowedWheel in _currentFrame.wheels)
            {
                if (allowedWheel == _currentWheels)
                {
                    wheelsValid = true;
                    break;
                }
            }

            if (!wheelsValid)
            {
                Debug.LogWarning("Current wheels are not compatible with the selected frame. Resetting wheels.");
                _currentWheels = null;
            }
        }

        // Validate current engine/canopy
        if (_vehicleType == VehicleType.Locomotive && _currentEngine != null)
        {
            bool engineValid = false;
            foreach (var allowedEngine in _currentFrame.allowedEngines)
            {
                if (allowedEngine == _currentEngine)
                {
                    engineValid = true;
                    break;
                }
            }

            if (!engineValid)
            {
                Debug.LogWarning("Current engine is not compatible with the selected frame. Resetting engine.");
                _currentEngine = null;
            }
        }

        if (_vehicleType == VehicleType.Wagon && _currentCanopy != null)
        {
            bool canopyValid = false;
            foreach (var allowedCanopy in _currentFrame.allowedCanopies)
            {
                if (allowedCanopy == _currentCanopy)
                {
                    canopyValid = true;
                    break;
                }
            }

            if (!canopyValid)
            {
                Debug.LogWarning("Current canopy is not compatible with the selected frame. Resetting canopy.");
                _currentCanopy = null;
            }
        }

        UpdateVehicleUpgrades();
    }

    public void SetWheels(WheelData wheels)
    {
        if (_currentFrame == null)
        {
            Debug.LogWarning("No frame selected. Cannot set wheels.");
            return;
        }

        bool isAllowed = false;
        foreach (var allowedWheel in _currentFrame.wheels)
        {
            if (allowedWheel == wheels)
            {
                isAllowed = true;
                break;
            }
        }

        if (isAllowed)
        {
            _currentWheels = wheels;
            UpdateVehicleUpgrades();
        }
        else
        {
            Debug.LogWarning("Selected wheels are not compatible with the current frame.");
        }
    }

    public void SetEngine(EngineData engine)
    {
        if (_vehicleType != VehicleType.Locomotive)
        {
            Debug.LogWarning("Cannot set engine on a non-Locomotive vehicle!");
            return;
        }

        if (_currentFrame == null)
        {
            Debug.LogWarning("No frame selected. Cannot set engine.");
            return;
        }

        bool isAllowed = false;
        foreach (var allowedEngine in _currentFrame.allowedEngines)
        {
            if (allowedEngine == engine)
            {
                isAllowed = true;
                break;
            }
        }

        if (isAllowed)
        {
            _currentEngine = engine;
            UpdateVehicleUpgrades();
        }
        else
        {
            Debug.LogWarning("Selected engine is not compatible with the current frame.");
        }
    }

    public void SetCanopy(CanopyData canopy)
    {
        if (_vehicleType != VehicleType.Wagon)
        {
            Debug.LogWarning("Cannot set canopy on a non-Wagon vehicle!");
            return;
        }

        if (_currentFrame == null)
        {
            Debug.LogWarning("No frame selected. Cannot set canopy.");
            return;
        }

        bool isAllowed = false;
        foreach (var allowedCanopy in _currentFrame.allowedCanopies)
        {
            if (allowedCanopy == canopy)
            {
                isAllowed = true;
                break;
            }
        }

        if (isAllowed)
        {
            _currentCanopy = canopy;
            UpdateVehicleUpgrades();
        }
        else
        {
            Debug.LogWarning("Selected canopy is not compatible with the current frame.");
        }
    }

    // Optional: Methods to retrieve current upgrades
    public FrameData GetCurrentFrame() => _currentFrame;
    public WheelData GetCurrentWheels() => _currentWheels;
    public EngineData GetCurrentEngine() => _currentEngine;
    public CanopyData GetCurrentCanopy() => _currentCanopy;

    // Optional: Methods to get available upgrades based on current frame
    public WheelData[] GetAvailableWheels()
    {
        return _currentFrame?.wheels;
    }

    public EngineData[] GetAvailableEngines()
    {
        if (_vehicleType != VehicleType.Locomotive) return null;
        return _currentFrame?.allowedEngines;
    }

    public CanopyData[] GetAvailableCanopies()
    {
        if (_vehicleType != VehicleType.Wagon) return null;
        return _currentFrame?.allowedCanopies;
    }
}
