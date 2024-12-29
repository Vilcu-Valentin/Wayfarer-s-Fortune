using UnityEngine;

[CreateAssetMenu(fileName = "New Frame", menuName = "Upgrades/Frame")]
public class FrameData : ScriptableObject
{
    public Vector2 spacing;
    public int health;

    [Tooltip("This is a frame dependent list of positions for stuff like the frame, engine, wheels etc." +
        " 0 is always the frame graphics itself, 1 is wheels, 2 is either engine or canopy")]
    public Vector3[] position;
    public GameObject graphics;

    [Tooltip("The possible wheels that you can add on this frame, this is just the database")]
    public WheelData[] wheels;

    [Header("Allowed Upgrades")]
    [Tooltip("Allowed engines for this frame (only applicable if vehicle is Locomotive)")]
    public EngineData[] allowedEngines;

    [Tooltip("Allowed canopies for this frame (only applicable if vehicle is Wagon)")]
    public CanopyData[] allowedCanopies;
}
