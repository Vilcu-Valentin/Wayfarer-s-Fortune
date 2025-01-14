using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections.Generic;
using TMPro;

public class TradeManager_HUD : MonoBehaviour
{
    [Header("UI Elements")]
    public TextMeshProUGUI coin_counter;


    private float previous_coint_counter;
    public void Update()
    {
        if(previous_coint_counter != PlayerMaster.Instance().money)
        {
            previous_coint_counter = PlayerMaster.Instance().money;
            UpdateCoinCounter();
        }
    }

    public void UpdateCoinCounter()
    {
        coin_counter.text = FormatNumber(PlayerMaster.Instance().money);
    }

    private static string FormatNumber(float number)
    {
        if (number == -1)
            return "???";

        if (number >= 1000000) // For numbers in the millions
            return (number / 1000000).ToString("0.#") + "M";
        else if (number >= 100000)
            return (number / 1000).ToString("0") + "k";
        else if (number >= 10000)
            return (number / 1000).ToString("0.#") + "k";
        else if (number >= 1000)
            return number.ToString("0");
        else if (number >= 100)
            return number.ToString("0.##");
        else
            return number.ToString("0.##"); // For numbers less than 1000
    }
}