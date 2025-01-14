using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ItemShop : MonoBehaviour
{
    public Item_UIManager uiManager;
    public TextMeshProUGUI buyText;

    private ItemData item;
    private float currentPrice;
    private int currentAmount;

    private void Start()
    {
        UpdateUI("1");
    }
    // Update is called once per frame
    public void UpdateUI(string input)
    {
        int.TryParse(input, out int amount);

        if (amount <= 0)
            amount = 1;
        buyText.text = "Buy\n" + FormatNumber(amount * uiManager.getPriceData());
        currentPrice = amount * uiManager.getPriceData();
        currentAmount = amount;
    }

    public void SetItem(ItemData item) { this.item = item; }

    public void BuyItem()
    {
        Inventory.Instance.BuyItem(new Item(item, currentAmount), currentPrice);
    }

    private static string FormatNumber(float number)
    {
        if (number == -1)
            return "???";

        if (number >= 1000000) // For numbers in the millions
            return (number / 1000000).ToString("0.#") + "M";
        else if (number >= 100000)
            return (number / 1000).ToString("0") + "k";
        else if (number >= 1000) // For numbers in the thousands
            return (number / 1000).ToString("0.#") + "k";
        else if (number >= 100)
            return number.ToString("0");
        else
            return number.ToString("0.##"); // For numbers less than 1000
    }
}
