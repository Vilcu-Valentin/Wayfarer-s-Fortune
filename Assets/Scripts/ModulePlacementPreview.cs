// Module placement preview and validation
using UnityEngine;

public class ModulePlacementPreview
{
    private GameObject previewObject;
    private bool isRotated;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    // If we have a previewObject spawned it means this script is active
    public bool IsActive => previewObject != null;

    // Create the preview using the gameObject prefab, TODO add the blueprint shader here as well
    public void CreatePreview(GameObject prefab, Vector3 position)
    {
        ClearPreview();
        previewObject = Object.Instantiate(prefab, position, Quaternion.identity);
        isRotated = false;
        targetPosition = previewObject.transform.position;
        targetRotation = previewObject.transform.rotation;
    }

    // Rotate the preview 90 degrees
    public void Rotate()
    {
        if (IsActive)
        {
            isRotated = !isRotated;
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

        previewObject.transform.position = Vector3.Lerp(
            previewObject.transform.position,
            targetPosition,
            positionLerpSpeed * Time.deltaTime
        );

        previewObject.transform.rotation = Quaternion.Lerp(
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
        isRotated = false;
    }

    // We get the gridSize of the object if it's rotated, we flip x and z sizes.
    public Vector3Int GetRotatedSize(Vector3Int originalSize)
    {
        return isRotated ? new Vector3Int(originalSize.z, originalSize.y, originalSize.x) : originalSize;
    }


    // Get the target rotation or final rotation of the placed object
    public Quaternion GetCurrentRotation() => targetRotation;
    // Get the final position of the placed object
    public Vector3 GetTargetPosition() => targetPosition;
    public bool IsRotated => isRotated;
}
