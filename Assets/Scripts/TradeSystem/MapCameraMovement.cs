using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapCameraMovement : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 10f;

    [Header("Zoom Settings")]
    public float minZoom = 10f;
    public float maxZoom = 50f;

    [Header("Map Bounds")]
    public float minX = -50f;
    public float maxX = 50f;
    public float minZ = -50f;
    public float maxZ = 50f;

    private float currentZoom;

    void Start()
    {

        currentZoom = Mathf.Clamp(transform.position.y, minZoom, maxZoom);
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
    }

    void HandleMovement()
    {
        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");   // W/S or Up/Down

        Vector3 move = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Adjust bounds dynamically based on zoom level
        float adjustedMinX = minX + currentZoom;
        float adjustedMaxX = maxX - currentZoom;
        float adjustedMinZ = minZ + currentZoom;
        float adjustedMaxZ = maxZ - currentZoom;

        // Clamp position within bounds
        Vector3 clampedPosition = transform.position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, adjustedMinX, adjustedMaxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, adjustedMinZ, adjustedMaxZ);
        transform.position = clampedPosition;
    }

    void HandleZoom()
    {
        // Get scroll input
        float scroll = Input.GetAxis("Mouse ScrollWheel");

        // Adjust zoom level
        currentZoom -= scroll * zoomSpeed;
        currentZoom = Mathf.Clamp(currentZoom, minZoom, maxZoom);

        // Apply zoom to camera target
        Vector3 targetPosition = transform.position;
        targetPosition.y = currentZoom;
        transform.position = targetPosition;
    }
}
