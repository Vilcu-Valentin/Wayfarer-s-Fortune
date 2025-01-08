using UnityEngine;
using TMPro;
using UnityEngine.UI;

[RequireComponent(typeof(Transform))]
public class WorldSpaceLabel : MonoBehaviour
{
    // Reference to the Canvas where the label will be displayed
    [Tooltip("Assign the Canvas where the city label should be displayed.")]
    public Canvas canvas;

    // Prefab for the label UI
    [Tooltip("Assign the label prefab to be used for displaying the city name.")]
    public GameObject labelPrefab;

    // The name to display. If left empty, it will use the GameObject's name
    [Tooltip("Name of the city to display. If empty, the GameObject's name is used.")]
    public string cityName;

    // Offset for fine-tuning the label's position relative to the city
    [Tooltip("Offset of the label's position relative to the city in screen space.")]
    public Vector2 positionOffset = Vector2.zero;

    // Reference to the instantiated label's GameObject
    private GameObject labelInstance;

    // Reference to the instantiated label's TextMeshProUGUI component
    private TextMeshProUGUI labelText;

    // Reference to the main camera
    private Camera mainCamera;

    // Reference to the Canvas's RectTransform
    private RectTransform canvasRectTransform;

    // Reference to the instantiated label's RectTransform
    private RectTransform labelRectTransform;

    void Start()
    {
        // Ensure a Canvas is assigned
        if (canvas == null)
        {
            Debug.LogError("Canvas reference is missing in WorldSpaceLabel script attached to " + gameObject.name);
            enabled = false;
            return;
        }

        // Ensure a Label Prefab is assigned
        if (labelPrefab == null)
        {
            Debug.LogError("Label Prefab is missing in WorldSpaceLabel script attached to " + gameObject.name);
            enabled = false;
            return;
        }

        // Get the main camera
        mainCamera = Camera.main;
        if (mainCamera == null)
        {
            Debug.LogError("Main Camera not found in the scene.");
            enabled = false;
            return;
        }

        // Get the Canvas's RectTransform
        canvasRectTransform = canvas.GetComponent<RectTransform>();
        if (canvasRectTransform == null)
        {
            Debug.LogError("Canvas does not have a RectTransform component.");
            enabled = false;
            return;
        }

        // Instantiate the label prefab
        labelInstance = Instantiate(labelPrefab, canvas.transform);
        labelInstance.name = "CityLabel_" + gameObject.name; // Optional: Unique naming

        // Get the RectTransform component
        labelRectTransform = labelInstance.GetComponent<RectTransform>();
        if (labelRectTransform == null)
        {
            Debug.LogError("Label Prefab does not have a RectTransform component.");
            Destroy(labelInstance);
            enabled = false;
            return;
        }

        // Get the TextMeshProUGUI component
        labelText = labelInstance.GetComponentInChildren<TextMeshProUGUI>();
        if (labelText == null)
        {
            Debug.LogError("Label Prefab does not have a TextMeshProUGUI component in its children.");
            Destroy(labelInstance);
            enabled = false;
            return;
        }

        // Set the text
        labelText.text = string.IsNullOrEmpty(cityName) ? gameObject.name : cityName;

        // Initially disable the label until it's updated in the first frame
        labelInstance.SetActive(false);
    }

    void Update()
    {
        if (labelInstance == null || canvas == null || mainCamera == null || canvasRectTransform == null || labelRectTransform == null)
            return;

        // Convert the world position of the city to screen position
        Vector3 screenPos = mainCamera.WorldToScreenPoint(transform.position);

        // Check if the city is in front of the camera
        bool isVisible = screenPos.z > 0;

        // Check if the position is within the screen bounds
        bool withinScreen = screenPos.x > 0 && screenPos.x < Screen.width &&
                            screenPos.y > 0 && screenPos.y < Screen.height;

        // Determine if the label should be visible
        bool shouldBeVisible = isVisible && withinScreen;

        // Toggle label visibility based on visibility
        if (labelInstance.activeSelf != shouldBeVisible)
        {
            labelInstance.SetActive(shouldBeVisible);
        }

        if (shouldBeVisible)
        {
            // Convert screen position to Canvas local position
            Vector2 anchoredPos;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRectTransform,
                screenPos + new Vector3(positionOffset.x, positionOffset.y, 0),
                canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out anchoredPos
            );

            // Set the label's anchored position
            labelRectTransform.anchoredPosition = anchoredPos;
        }
    }

    void OnDestroy()
    {
        // Clean up the label when the city GameObject is destroyed
        if (labelInstance != null)
        {
            Destroy(labelInstance);
        }
    }
}
