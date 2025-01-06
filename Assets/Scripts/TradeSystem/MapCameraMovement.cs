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

    [Header("DOF Settings")]
    [Tooltip("X value is the start offset from the focus distance, Y value is the end offset from the focus distance")]
    public Vector2 nearRange;
    [Tooltip("X value is the start offset from the focus distance, Y value is the end offset from the focus distance")]
    public Vector2 farRange;

    private float currentZoom;

    [Header("Volume")]
    public VolumeProfile profile;
    private DepthOfField dof;
    private Transform mainCamera;


    void Start()
    {
        currentZoom = Mathf.Clamp(transform.position.y, minZoom, maxZoom);
        mainCamera = Camera.main.transform;
        profile.TryGet<DepthOfField>(out dof);
    }

    void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleFocus();

        if (Input.GetKeyDown(KeyCode.LeftShift))
            moveSpeed = 20f;
        if (Input.GetKeyUp(KeyCode.LeftShift))
            moveSpeed = 10f;
    }

    void HandleFocus()
    {
        if (dof == null) return;

        RaycastHit hit;
        if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit, Mathf.Infinity))
        {
            float dist = Vector3.Distance(mainCamera.position, hit.point);
            dof.nearFocusStart.value = dist - nearRange.x;
            dof.nearFocusEnd.value = dist - nearRange.y;
            dof.farFocusStart.value = dist + farRange.x;
            dof.farFocusEnd.value = dist + farRange.y;
        }
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

        // Optional: Debugging logs
        Debug.Log($"Zoom Factor: {zoomFactor}, Bounds: X({adjustedMinX}, {adjustedMaxX}), Z({adjustedMinZ}, {adjustedMaxZ})");
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

        // Optionally log zoom level for debugging
        Debug.Log($"Zoom Level: {currentZoom}");
    }

}
