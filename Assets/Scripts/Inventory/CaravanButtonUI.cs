using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CaravanButtonUI : MonoBehaviour
{
    public Button Button;
    public Image WagonImage;

    private InventoryWagon wagon;

    public void Setup(InventoryWagon wagonData, bool highlight)
    {
        wagon = wagonData;

        // Assuming wagonGraphics is a sprite name or path
        // You might need to load the sprite accordingly
        // For simplicity, let's assume you have a reference
        WagonImage.sprite = wagon.icon;

        if (highlight)
            WagonImage.color = new Color(65, 65, 65, 100);

        Button.onClick.AddListener(OnSelectWagon);
    }

    private void OnSelectWagon()
    {
        Inventory.Instance.SelectWagon(wagon);
        Setup(wagon, true);
    }
}
