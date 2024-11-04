using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ModulePositionValidator 
{
    private bool isRotated;
    private Vector3 targetPosition;
    private Quaternion targetRotation;

    public void ResetRotation()
    {
        isRotated = false;
        targetRotation = Quaternion.Euler(0, 0, 0);
    }

    // Rotate the preview 90 degrees
    public void Rotate()
    {
            isRotated = !isRotated;
            targetRotation *= Quaternion.Euler(0, 90, 0);
    }

    // We get the gridSize of the object if it's rotated, we flip x and z sizes.
    public Vector3Int GetSize(Vector3Int originalSize)
    {
        return isRotated ? new Vector3Int(originalSize.z, originalSize.y, originalSize.x) : originalSize;
    }

    // Get the target rotation or final rotation of the placed object
    public Quaternion GetCurrentRotation() => targetRotation;
    // Get the final position of the placed object
    public Vector3 GetTargetPosition() => targetPosition;
    public bool IsRotated => isRotated;
}
