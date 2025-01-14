using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Inv_ModuleItemUI : ItemUI, IPointerClickHandler
{
    // If there are additional UI elements specific to module items, handle them here
    public int amount;

    public override void Initialize(Item itemData)
    {
        base.Initialize(itemData);
        // Initialize additional UI elements if necessary
    }

    // Override UpdateUI if additional UI updates are needed
    protected override void UpdateUI()
    {
        ItemName.text = item.ItemData.item_name;
        Icon.sprite = item.ItemData.icon;
        Quantity.text = "Quantity: " + item.Count.ToString();
        GoodType.text = item.ItemData.store_type.ToString();
    }

    public void UpdateAmount(string amount)
    {
        this.amount = int.Parse(amount);   
    }

    public void SellItem()
    {
        Inventory.Instance.SellItem(item, amount);
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
