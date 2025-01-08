using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Item_UIManager : MonoBehaviour
{
    [Header("Momentum Sprites")]
    public Sprite noMomentum;
    public Sprite upMomentum;
    public Sprite up2Momentum;
    public Sprite downMomentum;
    public Sprite down2Momentum;

    [Header("UI Elements")]
    public Image Momentum;
    public TextMeshProUGUI itemName;
    public Image itemSprite;
    public TextMeshProUGUI priceRange;

    // Update is called once per frame
    public void UpdateUI(OutputPriceData priceData)
    {
        //For now we don't calculate momentum;
        Momentum.sprite = noMomentum;

        itemName.text = priceData.itemData.item_name;
        itemSprite.sprite = priceData.itemData.icon;

        if(priceData.minPrice == priceData.maxPrice)
            priceRange.text = FormatNumber(priceData.minPrice);
        else
            priceRange.text = FormatNumber(priceData.minPrice) + " - " + FormatNumber(priceData.maxPrice);
    }

    public static string FormatNumber(float number)
    {
        if (number == -1)
            return "???";

        if (number >= 1000000) // For numbers in the millions
            return (number / 1000000).ToString("0.#") + "M";
        else if (number >= 1000) // For numbers in the thousands
            return (number / 1000).ToString("0.#") + "k";
        else
            return number.ToString("0.##"); // For numbers less than 1000
    }

}
