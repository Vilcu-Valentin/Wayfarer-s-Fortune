using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.VisualScripting;

public class Inventory_UI_Manager : MonoBehaviour
{
    [HideInInspector] public bool canCloseButton = true;

    [Header("Parents")]
    public Transform PendingListParent;
    public Transform CaravanListParent;
    public Transform ModuleListParent;
    public Transform InventoryListParent;

    [Header("Prefabs")]
    public GameObject PendingItemPrefab;
    public GameObject CaravanPrefab;
    public GameObject ModulePrefab;
    public GameObject ModuleItemPrefab; // Prefab for items inside modules

    [Header("Misc")]
    public TextMeshProUGUI currentlySelectedModuleName;
    public TextMeshProUGUI currentlySelectedModuleCapacity;
    public GameObject inventoryPanel;
    public Button closeButton;
    public SimpleHoverPopUp closeButtonPopUp;

    private Inventory inventory;

    private void Start()
    {
        // Subscribe to inventory updates
        inventory = FindObjectOfType<Inventory>();
        if (inventory != null)
        {
            inventory.OnInventoryUpdated += RefreshUI;
            UpdateUI(inventory.Wagons, inventory.PendingItems);
        }
    }

    public void Update()
    {
        if (canCloseButton)
        {
            closeButton.interactable = true;
            closeButtonPopUp.enabled = false;
        }
        else
        {
            closeButton.interactable = false;
            closeButtonPopUp.enabled = true;
        }
    }

    private void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryUpdated -= RefreshUI;
        }
    }

    // Initial UI update
    public void UpdateUI(List<InventoryWagon> wagons, List<Item> pendingData)
    {
        PopulateCaravans(wagons);
        PopulatePendingItems(pendingData);
    }

    // Refreshes the entire UI based on current inventory data
    private void RefreshUI()
    {
        if (inventory != null)
        {
            UpdateUI(inventory.Wagons, inventory.PendingItems);
            if (inventory.SelectedWagon.IsUnityNull())
            {
                DisplayModules(inventory.SelectedWagon.Modules);
            }
            if (inventory.SelectedModule != null)
            {
                DisplayModuleItems(inventory.SelectedModule);
            }
        }
    }

    private void PopulateCaravans(List<InventoryWagon> wagons)
    {
        ClearChildren(CaravanListParent);
        foreach (var wagon in wagons)
        {
            GameObject go = Instantiate(CaravanPrefab, CaravanListParent);
            if (go.TryGetComponent<CaravanButtonUI>(out CaravanButtonUI component))
            {
                if(Inventory.Instance.SelectedWagon.Equals(wagon))
                    component.Setup(wagon, true);
                else
                    component.Setup(wagon, false);
            }
        }
    }

    private void PopulatePendingItems(List<Item> pendingData)
    {
        ClearChildren(PendingListParent);
        foreach (var item in pendingData)
        {
            GameObject go = Instantiate(PendingItemPrefab, PendingListParent);
            if (go.TryGetComponent<Inv_PendingItemUI>(out Inv_PendingItemUI component))
            {
                component.Initialize(item);

                // Add DraggableItem component if not already present
                DraggableItem draggable = go.GetComponent<DraggableItem>();
                if (draggable == null)
                {
                    draggable = go.AddComponent<DraggableItem>();
                }

                // Ensure the DraggableItem script has access to the Item data
                draggable.SetItem(item);
            }
        }
    }

    public void DisplayModules(List<StorageModule> modules)
    {
        ClearChildren(ModuleListParent);
        foreach (var module in modules)
        {
            GameObject go = Instantiate(ModulePrefab, ModuleListParent);
            if (go.TryGetComponent<ModuleButtonUI>(out ModuleButtonUI component))
            {
                if(Inventory.Instance.SelectedModule == module)
                    component.Setup(module, true);
                else
                    component.Setup(module, false);
            }
        }
    }

    public void DisplayModuleItems(StorageModule module)
    {
        currentlySelectedModuleName.text = module.moduleData.name;
        currentlySelectedModuleCapacity.text = module.GetCurrentCapacity().ToString() + " / " + module.moduleData.capacity;

        IReadOnlyList<Item> items = module.items;

        ClearChildren(InventoryListParent);
        foreach (var item in items)
        {
            GameObject go = Instantiate(ModuleItemPrefab, InventoryListParent);
            if (go.TryGetComponent<Inv_ModuleItemUI>(out Inv_ModuleItemUI component))
            {
                component.Initialize(item);

                // Add DraggableItem component if not already present
                DraggableItem draggable = go.GetComponent<DraggableItem>();
                if (draggable == null)
                {
                    draggable = go.AddComponent<DraggableItem>();
                }

                // Ensure the DraggableItem script has access to the Item data
                draggable.SetItem(item);
            }
        }
    }

    private void ClearChildren(Transform parent)
    {
        foreach (Transform child in parent)
        {
            Destroy(child.gameObject);
        }
    }

    public void ToggleInventory()
    {
        if (inventoryPanel.activeSelf)
            inventoryPanel.SetActive(false);
        else
            inventoryPanel.SetActive(true);
    }
}
