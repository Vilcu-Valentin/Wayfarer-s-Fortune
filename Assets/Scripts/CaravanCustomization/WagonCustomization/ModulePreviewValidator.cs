using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulePreviewValidator 
{
    private GameObject previewObject;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // If we have a previewObject spawned it means this script is active
    public bool IsActive => previewObject != null;

    // Create the preview using the gameObject prefab, TODO add the blueprint shader here as well
    public void CreatePreview(GameObject prefab, Vector3 position)
    {
        ClearPreview();
        previewObject = Object.Instantiate(prefab, position, Quaternion.identity);
        targetPosition = previewObject.transform.position;
        targetRotation = previewObject.transform.rotation;
    }

    public void Rotate()
    {
        if (IsActive)
        {
            targetRotation *= Quaternion.Euler(0, 90, 0);
        }
    }

    // Update the position of the preview (this is already transformed from gridPosition to worldPosition)
    public void UpdatePosition(Vector3 newPosition)
    {
        targetPosition = newPosition;
    }

    // Smoothly update the transform using Lerp
    public void UpdateTransform(float positionLerpSpeed, float rotationLerpSpeed)
    {
        if (!IsActive) return;

        previewObject.transform.position = Vector3.Slerp(
            previewObject.transform.position,
            targetPosition,
            positionLerpSpeed * Time.deltaTime
        );

        previewObject.transform.rotation = Quaternion.Slerp(
            previewObject.transform.rotation,
            targetRotation,
            rotationLerpSpeed * Time.deltaTime
        );
    }

    // Change preview color, TODO change it so it works with the blueprint shader instead
    public void SetColor(Color color)
    {
        if (!IsActive) return;

        foreach (Renderer renderer in previewObject.GetComponentsInChildren<Renderer>())
        {
            renderer.material.color = color;
        }
    }

    // Remove the preview
    public void ClearPreview()
    {
        if (previewObject != null)
        {
            Object.Destroy(previewObject);
            previewObject = null;
        }
    }

    public float GetCurrentRotation() => previewObject.transform.eulerAngles.y;
}
