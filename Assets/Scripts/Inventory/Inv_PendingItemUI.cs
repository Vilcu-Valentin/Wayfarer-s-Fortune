using UnityEngine;
using UnityEngine.EventSystems;

public class Inv_PendingItemUI : ItemUI, IPointerClickHandler
{
    // If there are additional UI elements specific to pending items, handle them here

    public override void Initialize(Item itemData)
    {
        base.Initialize(itemData);
        // Initialize additional UI elements if necessary
    }

    // Override UpdateUI if additional UI updates are needed
    protected override void UpdateUI()
    {
        base.UpdateUI();
        // Update additional UI elements if necessary
    }

    public void SellItem()
    {
        Inventory.Instance.SellFromPendingItem(item, item.Count);
    }

    public void DropItem()
    {
        Inventory.Instance.DropItem(item, item.Count);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (eventData.button == PointerEventData.InputButton.Right)
        {
            // Optionally, display a custom context menu or ignore
            Debug.Log("Right-click detected on Pending Item. No action taken.");
            // Prevent further propagation if necessary
            eventData.Use();
        }
    }
}
