using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemEffect_UI : MonoBehaviour
{
    public Image icon;
    public TextMeshProUGUI amount;


    public void Initialize(Sprite icon, int amount, bool reverseAmountColor)
    {
        this.icon.sprite = icon;
        if (amount == 0)
        {
            this.amount.text = "Nothing happened!";
            return;
        }

        if(reverseAmountColor)
        {
            if (amount > 0)
                this.amount.text = "<color=#FABEA8>+" + amount.ToString() + "</color>";
            else
                this.amount.text = "<color=#D7FAA8>" + amount.ToString() + "</color>";
        }
        else
        {
            if (amount < 0)
                this.amount.text = "<color=#FABEA8>" + amount.ToString() + "</color>";
            else
                this.amount.text = "<color=#D7FAA8>+" + amount.ToString() + "</color>";
        }
    }
}
