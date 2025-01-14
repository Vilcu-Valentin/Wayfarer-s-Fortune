using System.Collections.Generic;
using UnityEngine;
using Modules.Rendering.Outline; // Ensure this matches your OutlineComponent's namespace

[RequireComponent(typeof(Collider))]
public class OutlineManager : MonoBehaviour
{
    [Header("Outline Settings")]
    [Tooltip("Whether to automatically add OutlineComponent to child GameObjects that don't have one.")]
    public bool addMissingOutlineComponents = true;

    // List to store all OutlineComponents of child GameObjects
    private List<OutlineComponent> outlineComponents = new List<OutlineComponent>();

    private void Awake()
    {
        InitializeOutlineComponents();
    }

    /// <summary>
    /// Initializes the list of OutlineComponents by iterating through all child GameObjects.
    /// Optionally adds OutlineComponent if missing.
    /// </summary>
    private void InitializeOutlineComponents()
    {
        // Get all child transforms (including nested children)
        foreach (Transform child in transform.GetComponentsInChildren<Transform>())
        {
            // Skip the parent itself
            if (child == this.transform)
                continue;

            // Try to get existing OutlineComponent
            OutlineComponent outline = child.GetComponent<OutlineComponent>();

            if (outline == null && addMissingOutlineComponents)
            {
                // Add OutlineComponent if missing and allowed
                outline = child.gameObject.AddComponent<OutlineComponent>();
                Debug.Log($"Added OutlineComponent to {child.gameObject.name}");
            }

            if (outline != null)
            {
                // Initially disable outlines
                outline.enabled = false;
                outlineComponents.Add(outline);
            }
            else
            {
                Debug.LogWarning($"No OutlineComponent found on {child.gameObject.name} and 'Add Missing Outline Components' is disabled.");
            }
        }

        if (outlineComponents.Count == 0)
        {
            Debug.LogWarning("No OutlineComponents found in child GameObjects.");
        }
    }

    /// <summary>
    /// Enables or disables outlines on all child GameObjects.
    /// </summary>
    /// <param name="enable">True to enable outlines, false to disable.</param>
    public void EnableOutlines(bool enable)
    {
        foreach (var outline in outlineComponents)
        {
            if (outline != null)
            {
                outline.enabled = enable;
            }
        }
    }

    /// <summary>
    /// Optional: Ensure outlines are disabled when the object is disabled.
    /// </summary>
    private void OnDisable()
    {
        EnableOutlines(false);
    }
}
