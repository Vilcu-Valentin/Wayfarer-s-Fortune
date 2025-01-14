using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DropZone : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public enum DropType { Pending, Module }

    public DropType dropType;
    public Image backgroundImage; // Assign in Inspector for visual feedback

    private Inventory inventory;

    [Header("Highlihght")]
    public GameObject objectToHighlight;
    private Color defaultColor;
    public Color highlightColor;

    private void Start()
    {
        inventory = FindObjectOfType<Inventory>();
        if (objectToHighlight != null)
        {
            defaultColor = objectToHighlight.GetComponent<Image>().color;
        }
        else
        {
            Debug.LogWarning("objectToHighlight is not assigned in the inspector.");
        }
    }

    public void OnDrop(PointerEventData eventData)
    {
        DraggableItem draggedItem = eventData.pointerDrag.GetComponent<DraggableItem>();
        if (draggedItem != null)
        {
            Item item = draggedItem.GetItem();
            if (item == null)
            {
                Debug.LogError("Dragged item does not have valid Item data.");
                return;
            }

            // Capture the count and create a clone to prevent reference issues
            int originalCount = item.Count;
            string itemName = item.ItemData.item_name;

            // Clone the item to pass to inventory methods
            Item itemClone = item.Clone();

            bool success = false;

            switch (dropType)
            {
                case DropType.Module:
                    if(draggedItem.type != DraggableItem.DropType.Module)
                    if (inventory.SelectedModule != null)
                    {
                        success = inventory.AddItemToModule(inventory.SelectedModule, itemClone);
                        if (success)
                        {
                            Debug.Log($"Added {originalCount}x {itemName} to module.");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to add item to module due to insufficient capacity.");
                        }
                    }
                    break;

                case DropType.Pending:
                    if (draggedItem.type != DraggableItem.DropType.Pending)
                        if (inventory.SelectedModule != null)
                    {
                        success = inventory.RemoveItemFromModule(inventory.SelectedModule, itemClone, false);
                        if (success)
                        {
                            Debug.Log($"Removed {originalCount}x {itemName} from module to pending.");
                        }
                        else
                        {
                            Debug.LogWarning("Failed to remove item from module.");
                        }
                    }
                    break;
            }

            if (success)
            {
                // Destroy the dragged UI element as the inventory will update the UI
                Destroy(draggedItem.gameObject);
                // Optionally, you can add a sound effect or visual cue here
            }
            else
            {
                // Handle failed drop if necessary (e.g., show a message)
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Check if a DraggableItem is being dragged
        if (DraggableItem.currentlyDraggedItem != null)
        {
            // Check if the types match
            if (IsDropTypeMatching(DraggableItem.currentlyDraggedItem.type))
            {
                if (objectToHighlight != null)
                {
                    objectToHighlight.GetComponent<Image>().color = highlightColor;
                }
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Always reset the color when pointer exits
        if (objectToHighlight != null)
        {
            objectToHighlight.GetComponent<Image>().color = defaultColor;
        }
    }

    private bool IsDropTypeMatching(DraggableItem.DropType draggedType)
    {
        switch (dropType)
        {
            case DropType.Module:
                return draggedType == DraggableItem.DropType.Pending;
            case DropType.Pending:
                return draggedType == DraggableItem.DropType.Module;
            default:
                return false;
        }
    }
}
