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
    public CanopyData canopies;

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

        _frameTransform.localPosition = transform.position + _currentFrame.position[0];
        _wheelsTransform.localPosition = transform.position + _currentFrame.position[1];
        if (_vehicleType == VehicleType.Locomotive)
            _customAddonTransform.localPosition = transform.position + _currentFrame.position[2];
        if (_vehicleType == VehicleType.Wagon)
            _customAddonTransform.localPosition = transform.position + _currentFrame.position[2];
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
        UpdateVehicleUpgrades();
    }

    public void SetWheels(WheelData wheels)
    {
        _currentWheels = wheels;
        UpdateVehicleUpgrades();
    }

    public void SetEngine(EngineData engine)
    {
        if (_vehicleType != VehicleType.Locomotive)
        {
            Debug.LogWarning("Cannot set engine on a non-Locomotive vehicle!");
            return;
        }
        _currentEngine = engine;
        UpdateVehicleUpgrades();
    }

    public void SetCanopy(CanopyData canopy)
    {
        if (_vehicleType != VehicleType.Wagon)
        {
            Debug.LogWarning("Cannot set canopy on a non-Wagon vehicle!");
            return;
        }
        _currentCanopy = canopy;
        UpdateVehicleUpgrades();
    }

    // Optional: Methods to retrieve current upgrades
    public FrameData GetCurrentFrame() => _currentFrame;
    public WheelData GetCurrentWheels() => _currentWheels;
    public EngineData GetCurrentEngine() => _currentEngine;
    public CanopyData GetCurrentCanopy() => _currentCanopy;
}
