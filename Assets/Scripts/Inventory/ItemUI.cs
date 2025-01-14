using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class ItemUI : MonoBehaviour
{
    [SerializeField] protected TextMeshProUGUI ItemName;
    [SerializeField] protected Image Icon;
    [SerializeField] protected TextMeshProUGUI Quantity;
    [SerializeField] protected TextMeshProUGUI GoodType;

    protected Item item;
    public Button sellButton;
    // Method to initialize the UI with item data
    public virtual void Initialize(Item itemData)
    {
        item = itemData;
        UpdateUI();
    }

    // Updates the UI elements based on the item data
    protected virtual void UpdateUI()
    {
        ItemName.text = item.ItemData.item_name;
        Icon.sprite = item.ItemData.icon;
        Quantity.text = "Quantity\n" + item.Count.ToString();
        GoodType.text = item.ItemData.store_type.ToString();
    }

    // Provides access to the Item data
    public Item GetItem()
    {
        return item;
    }
}
