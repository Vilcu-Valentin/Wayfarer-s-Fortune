using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public Image Icon;
    public enum DropType { Pending, Module };
    public DropType type;
    private CanvasGroup canvasGroup;
    private RectTransform rectTransform;
    private Canvas canvas;

    private Vector3 originalPosition;
    private Transform originalParent;

    private ItemUI itemUI; // Reference to the ItemUI component

    // Reference to the source module (if any)
    private StorageModule sourceModule;

    // Flag to track if the drag was initiated with the Left Mouse Button
    private bool isDraggingWithLeftMouse = false;

    // Static reference to track the currently dragged item
    public static DraggableItem currentlyDraggedItem;

    private void Awake()
    {
        // Ensure CanvasGroup is attached
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
            Debug.LogWarning($"CanvasGroup was missing on {gameObject.name}. Added one automatically.");
        }

        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();

        // Get the ItemUI component
        itemUI = GetComponent<ItemUI>();
        if (itemUI == null)
        {
            Debug.LogError($"ItemUI component is missing on {gameObject.name}. Please add an ItemUI-derived component.");
        }
    }

    // Method to set the source module
    public void SetSourceModule(StorageModule module)
    {
        sourceModule = module;
    }

    public StorageModule GetSourceModule()
    {
        return sourceModule;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check if the drag was initiated with the Left Mouse Button
        if (eventData.button != PointerEventData.InputButton.Left)
            return;

        isDraggingWithLeftMouse = true;

        // Set the currently dragged item
        currentlyDraggedItem = this;

        if (itemUI == null)
            return;

        originalPosition = rectTransform.position;
        originalParent = rectTransform.parent;

        // Move the item to the DragLayer to ensure it's on top
        if (DragLayerManager.Instance != null && DragLayerManager.Instance.GetDragLayer() != null)
        {
            rectTransform.SetParent(DragLayerManager.Instance.GetDragLayer(), true);
        }
        else
        {
            Debug.LogError("DragLayerManager or DragLayerTransform is not set.");
        }

        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDraggingWithLeftMouse)
            return;

        if (canvas != null)
        {
            // Convert the screen point to canvas space
            Vector2 position;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                eventData.position,
                eventData.pressEventCamera,
                out position);
            rectTransform.anchoredPosition = position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!isDraggingWithLeftMouse)
            return;

        // Clear the currently dragged item
        currentlyDraggedItem = null;

        canvasGroup.alpha = 1f;
        canvasGroup.blocksRaycasts = true;

        // Check if the item was dropped onto a valid DropZone
        // If not, revert to original parent and position
        if (transform.parent == DragLayerManager.Instance.GetDragLayer())
        {
            // Item was not dropped onto a DropZone, revert
            rectTransform.SetParent(originalParent, true);
            rectTransform.position = originalPosition;
        }

        // Reset the drag initiation flag
        isDraggingWithLeftMouse = false;
    }

    // Method to set item data (if needed)
    public void SetItem(Item item)
    {
        if (itemUI != null)
        {
            itemUI.Initialize(item);
        }
    }

    // Provides access to the Item data
    public Item GetItem()
    {
        if (itemUI != null)
        {
            return itemUI.GetItem();
        }
        return null;
    }
}
