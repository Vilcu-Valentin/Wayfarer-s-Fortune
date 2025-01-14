using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Cinemachine;

public class TradeUIManager : MonoBehaviour
{
    [SerializeField] private Vector2 item_content_sized = new Vector2(207, 620);
    [HideInInspector] public bool cityMode { get; private set; } = false;
    private bool isShop = false;

    [Header("Prefabs")]
    public GameObject tradeItem_Prefab;
    public GameObject tradeItem_BuyMode_Prefab;
    public GameObject tradeEvent_Prefab;
    public List<Sprite> settlementSizePrefabs;

    [Header("SettlementInfo")]
    public TMP_Text distanceToSettlement;
    public Button travelButton;
    public TMP_Text settlementName;
    public Image settlementSize;
    public TMP_Text settlementOccupation;
    public GameObject tradeItemList;

    [Header("Events")]
    public TMP_Text eventName;
    public TMP_Text eventDescription;
    public GameObject eventPopUp;
    public GameObject tradeEventList;
    public GameObject noEventsMarker;

    [Header("Cameras")]
    public CinemachineFreeLook cityViewCamera;
    public TradeCameraFocus focus;

    [Header("Canvases")]
    public GameObject cityNameCanvas;
    public GameObject infoCanvas;

    [Header("Sorting")]
    public TMP_Dropdown sortDropdown;

    private List<OutputPriceData> currentPrices;

    private void Start()
    {
        infoCanvas.SetActive(false);
        if (sortDropdown != null)
        {
            sortDropdown.onValueChanged.AddListener(OnSortOptionChanged);
        }
    }

    public void CloseInfoCanvas()
    {
        infoCanvas.SetActive(false);
        cityMode = false;
        ChangeToMainCamera();
    }

    public void OpenInfoCanvas(Settlement settlement, int distance)
    {
        infoCanvas.SetActive(true);
        cityMode = true;
        RemoveChildGameObjects(tradeItemList);

        settlementName.text = settlement.data.name;
        if (distance == 0)
        {
            distanceToSettlement.text = "You are here!";
            travelButton.interactable = false;
        }
        else
        {
            distanceToSettlement.text = "Travel\r\n<color=#ddd><size=25>(Distance: " + Mathf.RoundToInt(distance / Inventory.Instance.caravanStatistics.speedModifier).ToString() + " hours)</size></color>";
            travelButton.interactable = true;
        }

        if (tradeItemList.TryGetComponent(out GridLayoutGroup gridLayout))
        {
            if (distance != 0)
            {
                gridLayout.cellSize = new Vector2(item_content_sized.x, 132);
                isShop = false;
            }
            else
            { 
                gridLayout.cellSize = new Vector2(item_content_sized.y, 132);
                isShop = true;
            }
        }

        if (settlement.data.size == "City")
            settlementSize.sprite = settlementSizePrefabs[2];
        else if (settlement.data.size == "Town")
            settlementSize.sprite = settlementSizePrefabs[1];
        else
            settlementSize.sprite = settlementSizePrefabs[0];

        settlementOccupation.text = settlement.data.occupation;

        ChangeToCityCamera(settlement.transform);

        currentPrices = settlement.getPrices();
        PopulateEvents(settlement);
        SortAndPopulateTradeItems();
    }

    private void RemoveChildGameObjects(GameObject parent)
    {
        Debug.Log(parent.transform.childCount);
        int i = 0;

        // Array to hold all child objects
        GameObject[] allChildren = new GameObject[parent.transform.childCount];

        // Find all child objects and store them in the array
        foreach (Transform child in parent.transform)
        {
            allChildren[i] = child.gameObject;
            i += 1;
        }

        // Now unsubscribe and destroy them
        foreach (GameObject child in allChildren)
        {
            if (child.TryGetComponent(out Event_UIManager uiManager))
            {
                uiManager.OnHoverEnter -= HandleItemHoverEnter;
                uiManager.OnHoverExit -= HandleItemHoverExit;
            }

            Destroy(child);
        }

        Debug.Log(parent.transform.childCount);
    }

    public void ChangeToCityCamera(Transform target)
    {
        cityNameCanvas.SetActive(false);

        cityViewCamera.LookAt = target;
        cityViewCamera.Follow = target;

        cityViewCamera.Priority = 100;
        focus.FocusOnObject(target);
    }

    public void ChangeToMainCamera()
    {
        cityNameCanvas.SetActive(true);
        cityViewCamera.Priority = 1;
        focus.AutoFocus();
    }

    private void PopulateEvents(Settlement settlement)
    {
        RemoveChildGameObjects(tradeEventList);
        bool anyEvents = false;
        noEventsMarker.SetActive(true);

        foreach (var _event in settlement.getVisibleEvents())
        {
            GameObject spawnedEvent = Instantiate(tradeEvent_Prefab, tradeEventList.transform);
            if (spawnedEvent.TryGetComponent(out Event_UIManager uiManager))
            {
                uiManager.UpdateUI(_event.Key);

                // Subscribe to hover events
                uiManager.OnHoverEnter += HandleItemHoverEnter;
                uiManager.OnHoverExit += HandleItemHoverExit;
            }
            anyEvents = true; 
        }

        if(anyEvents)
            noEventsMarker.SetActive(false);
    }

    private void SortAndPopulateTradeItems()
    {
        if (currentPrices == null) return;

        int value = sortDropdown.value;
        if (currentPrices[0].minPrice == -1)
            if (value == 2 || value == 3)
                value = 0;

        // Sort currentPrices based on sortDropdown selection
        switch (value)
        {
            case 0: // Name Ascending
                currentPrices.Sort((a, b) => a.itemData.item_name.CompareTo(b.itemData.item_name));
                break;
            case 1: // Name Descending
                currentPrices.Sort((a, b) => b.itemData.item_name.CompareTo(a.itemData.item_name));
                break;
            case 2: // Price: Low to High
                currentPrices.Sort((a, b) =>
                    ((a.minPrice + a.maxPrice) / 2).CompareTo((b.minPrice + b.maxPrice) / 2));
                break;
            case 3: // Price: High to Low
                currentPrices.Sort((a, b) =>
                    ((b.minPrice + b.maxPrice) / 2).CompareTo((a.minPrice + a.maxPrice) / 2));
                break;
            case 4: // Type
                currentPrices.Sort((a, b) =>
                {
                    int typeComparison = a.itemData.store_type.CompareTo(b.itemData.store_type);
                    if (typeComparison == 0)
                        return a.itemData.item_name.CompareTo(b.itemData.item_name);
                    return typeComparison;
                });
                break;
            default:
                // Default to Name Ascending
                currentPrices.Sort((a, b) => a.itemData.item_name.CompareTo(b.itemData.item_name));
                break;
        }

        // After sorting, populate the trade item list
        PopulateTradeItems();
    }

    private void OnSortOptionChanged(int selectedOption)
    {
        // When the sort option changes, sort and repopulate the trade items
        SortAndPopulateTradeItems();
    }

    private void PopulateTradeItems()
    {
        RemoveChildGameObjects(tradeItemList);

        foreach (OutputPriceData price in currentPrices)
        {
            GameObject itemToSpawn;
            if (isShop)
                itemToSpawn = tradeItem_BuyMode_Prefab;
            else
                itemToSpawn = tradeItem_Prefab;
            GameObject item = Instantiate(itemToSpawn, tradeItemList.transform);
            if (item.TryGetComponent(out Item_UIManager uiManager))
            {
                uiManager.UpdateUI(price);
            }

            if(item.TryGetComponent(out ItemShop shop))
            {
                shop.SetItem(price.itemData);
            }
        }
    }

    // **Listener Methods for Hover Events**

    /// <summary>
    /// Called when the mouse pointer enters a UI element.
    /// </summary>
    /// <param name="item">The Event_UIManager instance that triggered the event.</param>
    public void HandleItemHoverEnter(Event_UIManager item)
    {
        eventPopUp.SetActive(true);
        EventData data = item.getEvent();
        eventName.text = data.name;
        eventDescription.text = data.description;
    }

    /// <summary>
    /// Called when the mouse pointer exits a UI element.
    /// </summary>
    /// <param name="item">The Event_UIManager instance that triggered the event.</param>
    public void HandleItemHoverExit(Event_UIManager item)
    {
        eventPopUp.SetActive(false);
        eventName.text = "";
        eventDescription.text = "";
    }
}
