using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using TMPro.EditorUtilities;

public class Item_UIManager : MonoBehaviour
{
    [Header("Momentum Sprites")]
    public Sprite noMomentum;
    public Sprite upMomentum;
    public Sprite up2Momentum;
    public Sprite downMomentum;
    public Sprite down2Momentum;

    [Header("UI Elements")]
    public Image mainPanel;
    public Image Momentum;
    public Color[] colors;
    public TextMeshProUGUI itemName;
    public Image itemSprite;
    public TextMeshProUGUI priceRange;
    public TextMeshProUGUI typeName;

    private void Start()
    {
        colors[0] = mainPanel.color;
    }

    // Update is called once per frame
    public void UpdateUI(OutputPriceData priceData)
    {
        Momentum.sprite = noMomentum;
        mainPanel.color = colors[0];
        if(priceData.minPrice != -1)
        {
            if (priceData.realPrice >= priceData.basePrice * 1.2)
            {
                Momentum.sprite = upMomentum;
                mainPanel.color = colors[1];
            }
            if (priceData.realPrice >= priceData.basePrice * 2)
            {
                Momentum.sprite = up2Momentum;
                mainPanel.color = colors[2];
            }
            if (priceData.realPrice < priceData.basePrice * 0.8)
            {
                Momentum.sprite = downMomentum;
                mainPanel.color = colors[3];
            }
            if (priceData.realPrice < priceData.basePrice / 2)
            {
                Momentum.sprite = down2Momentum;
                mainPanel.color = colors[4];
            }
        }

        itemName.text = priceData.itemData.item_name;
        itemSprite.sprite = priceData.itemData.icon;
        typeName.text = priceData.itemData.store_type.ToString();

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
