using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;

public class MapCameraMovement : MonoBehaviour
{
    [Header("Camera Movement Settings")]
    public float moveSpeed = 10f;
    public float zoomSpeed = 10f;

    [Header("Zoom Settings")]
    public float minZoom = 10f;
    public float maxZoom = 50f;
    public float zoomPadding = 10f;

    [Header("Zoom-In Bounds")]
    public float minXZoomIn = -50f;
    public float maxXZoomIn = 50f;
    public float minZZoomIn = -50f;
    public float maxZZoomIn = 50f;

    [Header("Zoom-Out Bounds")]
    public float minXZoomOut = -100f;
    public float maxXZoomOut = 100f;
    public float minZZoomOut = -100f;
    public float maxZZoomOut = 100f;

    private float currentZoom;


    void Start()
    {
        currentZoom = Mathf.Clamp(transform.position.y, minZoom, maxZoom);
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            moveSpeed = 20f;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            moveSpeed = 10f;
    }

    void HandleMovement()
    {
        // Get WASD input
        float horizontal = Input.GetAxis("Horizontal"); // A/D or Left/Right
        float vertical = Input.GetAxis("Vertical");   // W/S or Up/Down

        Vector3 move = new Vector3(horizontal, 0, vertical) * moveSpeed * Time.deltaTime;
        transform.Translate(move, Space.World);

        // Calculate zoom factor (normalized)
        float zoomFactor = (currentZoom - minZoom) / (maxZoom - minZoom); // Normalized zoom between 0 and 1

        // Lerp bounds based on zoom level
        float adjustedMinX = Mathf.Lerp(minXZoomIn, minXZoomOut, zoomFactor);
        float adjustedMaxX = Mathf.Lerp(maxXZoomIn, maxXZoomOut, zoomFactor);
        float adjustedMinZ = Mathf.Lerp(minZZoomIn, minZZoomOut, zoomFactor);
        float adjustedMaxZ = Mathf.Lerp(maxZZoomIn, maxZZoomOut, zoomFactor);

        // Clamp position within dynamically adjusted bounds
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

    public void MoveCamereTo(Vector3 position)
    {
        position.z -= 5f;
        // Calculate zoom factor (normalized)
        float zoomFactor = (currentZoom - minZoom) / (maxZoom - minZoom); // Normalized zoom between 0 and 1

        // Lerp bounds based on zoom level
        float adjustedMinX = Mathf.Lerp(minXZoomIn, minXZoomOut, zoomFactor);
        float adjustedMaxX = Mathf.Lerp(maxXZoomIn, maxXZoomOut, zoomFactor);
        float adjustedMinZ = Mathf.Lerp(minZZoomIn, minZZoomOut, zoomFactor);
        float adjustedMaxZ = Mathf.Lerp(maxZZoomIn, maxZZoomOut, zoomFactor);

        // Clamp position within dynamically adjusted bounds
        Vector3 clampedPosition = position;
        clampedPosition.x = Mathf.Clamp(clampedPosition.x, adjustedMinX, adjustedMaxX);
        clampedPosition.z = Mathf.Clamp(clampedPosition.z, adjustedMinZ, adjustedMaxZ);
        transform.position = clampedPosition;
    }

}
