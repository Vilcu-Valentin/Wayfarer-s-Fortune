using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways] // Ensures the script runs in edit mode
public class AdjustChildrenToGround : MonoBehaviour
{
    [Tooltip("Offset added to the Y position after aligning to the terrain.")]
    public float yOffset = 0f;

    // Stores the last known position to detect changes
    private Vector3 lastPosition;

    private void OnEnable()
    {
        // Initialize the last position and perform the initial adjustment
        lastPosition = transform.position;
        AdjustChildren();
    }

    private void Update()
    {
        // Only execute in the Unity Editor
#if UNITY_EDITOR
        if (!Application.isPlaying)
        {
            // Check if the parent has moved
            if (transform.position != lastPosition)
            {
                lastPosition = transform.position;
                AdjustChildren();
            }
        }
#endif
    }

    /// <summary>
    /// Adjusts all child objects to align their Y positions with the terrain.
    /// </summary>
    private void AdjustChildren()
    {
        Terrain terrain = Terrain.activeTerrain;

        if (terrain == null)
        {
            Debug.LogWarning($"[AdjustChildrenToGround] No active terrain found in the scene.");
            return;
        }

        foreach (Transform child in transform)
        {
            // Get the world position of the child
            Vector3 worldPos = child.position;

            // Sample the terrain height at the child's X and Z position
            float terrainHeight = terrain.SampleHeight(worldPos) + terrain.transform.position.y;

            // Set the child's Y position to the terrain height plus the offset
            Vector3 newPos = worldPos;
            newPos.y = terrainHeight + yOffset;

            // Apply the new position
            child.position = newPos;
        }
    }

    // Called when the script is loaded or a value is changed in the Inspector
    private void OnValidate()
    {
        AdjustChildren();
    }
}
