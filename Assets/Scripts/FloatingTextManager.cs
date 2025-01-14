using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FloatingTextManager : MonoBehaviour
{
    // Singleton instance
    private static FloatingTextManager _instance;
    public static FloatingTextManager Instance
    {
        get
        {
            if (_instance == null)
            {
                // Attempt to find an existing instance in the scene
                _instance = FindObjectOfType<FloatingTextManager>();

                if (_instance == null)
                {
                    // If none exists, create a new GameObject with the manager
                    GameObject go = new GameObject("FloatingTextManager");
                    _instance = go.AddComponent<FloatingTextManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    [Header("Floating Text Settings")]
    public GameObject floatingTextPrefab; // Assign your prefab in the Inspector
    public Canvas canvas; // Assign your Canvas in the Inspector (ensure it's set to Screen Space - Overlay)
    public int sortingOrder = 1000; // High sorting order to ensure it's on top

    private void Awake()
    {
        // Ensure singleton instance
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);

            if (canvas == null)
            {
                // Create a new Canvas if not assigned
                GameObject canvasGO = new GameObject("FloatingTextCanvas");
                canvas = canvasGO.AddComponent<Canvas>();
                canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                canvas.sortingOrder = sortingOrder;

                canvasGO.AddComponent<CanvasScaler>();
                canvasGO.AddComponent<GraphicRaycaster>();
            }

            // Load prefab from Resources if not assigned
            if (floatingTextPrefab == null)
            {
                floatingTextPrefab = Resources.Load<GameObject>("FloatingTextPrefab");
                if (floatingTextPrefab == null)
                {
                    Debug.LogError("FloatingTextPrefab not found in Resources!");
                }
            }
        }
        else
        {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Displays floating text at the current mouse position.
    /// </summary>
    /// <param name="text">The text to display.</param>
    /// <param name="duration">Duration in seconds before the text is destroyed.</param>
    public static void Show(string text, float duration)
    {
        Instance.StartCoroutine(Instance.ShowFloatingText(text, duration));
    }

    private IEnumerator ShowFloatingText(string text, float duration)
    {
        if (floatingTextPrefab == null || canvas == null)
        {
            yield break;
        }

        // Instantiate the prefab as a child of the canvas
        GameObject floatingTextGO = Instantiate(floatingTextPrefab, canvas.transform);

        // Attempt to find the Text component in the children
        Text txt = floatingTextGO.GetComponentInChildren<Text>();
        if (txt != null)
        {
            txt.text = text;
        }
        else
        {
            // If using TextMeshPro
            TMPro.TextMeshProUGUI tmp = floatingTextGO.GetComponentInChildren<TMPro.TextMeshProUGUI>();
            if (tmp != null)
            {
                tmp.text = text;
            }
            else
            {
                Debug.LogWarning("No Text or TextMeshProUGUI component found in FloatingTextPrefab children.");
            }
        }

        // Convert mouse position to Canvas space
        Vector2 mousePos = Input.mousePosition;
        floatingTextGO.GetComponent<RectTransform>().position = mousePos;

        // Optionally, you can add animations or movement here

        // Wait for the specified duration
        yield return new WaitForSeconds(duration);

        // Destroy the floating text
        Destroy(floatingTextGO);
    }
}
