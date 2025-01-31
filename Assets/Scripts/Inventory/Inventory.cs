using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

[System.Serializable]
public class InventoryWagon
{
    public List<StorageModule> Modules;
    public Sprite icon;
}

[System.Serializable]
public class CaravanStatistics
{
    public float speedModifier;
    public Sprite locomotiveIcon;
}

public class Inventory : MonoBehaviour
{
    public CaravanStatistics caravanStatistics;
    public static Inventory Instance { get; private set; }
    public List<InventoryWagon> Wagons = new List<InventoryWagon>();
    public List<Item> PendingItems = new List<Item>();

    public InventoryWagon SelectedWagon { get; private set; }
    public StorageModule SelectedModule { get; private set; }

    public Inventory_UI_Manager UiManager;
    public TradeManager tradeManager;

    // Reference to the WagonIconDatabase
    [SerializeField] private WagonIconDatabase wagonIconDatabase;

    // Reference to the StorageModuleDatabase
    [SerializeField] private StorageModuleDatabase storageModuleDatabase;

    // Events
    public event Action OnInventoryUpdated;

    // Save file details
    [SerializeField] private string saveFileName = "caravan_save.json";
    private string SavePath => Path.Combine(Application.persistentDataPath, saveFileName);

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }
        Instance = this;

        // Initialize the WagonIconDatabase
        if (wagonIconDatabase != null)
        {
            wagonIconDatabase.Initialize();
        }
        else
        {
            Debug.LogError("WagonIconDatabase reference is missing!");
        }
    }

    private void Start()
    {
        LoadInventoryWagons();

        if (Wagons.Count > 0 && Wagons[0].Modules.Count > 0)
        {
            SelectedWagon = Wagons[0];
            SelectedModule = SelectedWagon.Modules[0];
        }

        // Optionally, notify UI of the loaded data
        NotifyInventoryUpdated();
    }

    private void LoadInventoryWagons()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning($"No save file found at {SavePath}!");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            CaravanSaveData saveData = JsonUtility.FromJson<CaravanSaveData>(json);

            if (saveData == null || saveData.wagons == null)
            {
                Debug.LogError("Failed to parse save data.");
                return;
            }

            foreach (var wagonData in saveData.wagons.OrderBy(w => w.wagonIndex))
            {
                // Get the icon from the database
                Sprite wagonIcon = wagonIconDatabase.GetIcon(wagonData.wagonPrefabPath);
                if (wagonIcon == null)
                {
                    Debug.LogError($"Icon not found for prefab path: {wagonData.wagonPrefabPath}");
                    continue;
                }

                // Create InventoryWagon instance
                InventoryWagon inventoryWagon = new InventoryWagon
                {
                    Modules = new List<StorageModule>(),
                    icon = wagonIcon
                };

                // Populate modules
                foreach (var moduleData in wagonData.modules)
                {
                    StorageModule module = CreateStorageModuleFromSaveData(moduleData);
                    if (module != null)
                    {
                        inventoryWagon.Modules.Add(module);
                    }
                }

                Wagons.Add(inventoryWagon);
            }

            Debug.Log("Inventory wagons loaded successfully.");
        }
        catch (Exception e)
        {
            Debug.LogError($"Error loading caravan save data: {e.Message}");
        }
    }

    private StorageModule CreateStorageModuleFromSaveData(StorageModuleSaveData moduleData)
    {
        // Retrieve the module data from the database
        StorageModuleData moduleDefinition = storageModuleDatabase.GetModuleById(moduleData.moduleId);
        if (moduleDefinition == null)
        {
            Debug.LogError($"Module definition not found for module ID: {moduleData.moduleId}");
            return null;
        }

        // Create a new StorageModule instance
        StorageModule module = new StorageModule
        {
            moduleData = moduleDefinition,
            currentPosition = moduleData.position,
            rotated = moduleData.isRotated,
            rotation = moduleData.rotation,
            items = new List<Item>(), // Assuming no items are loaded here
            objectRef = null // Assuming objectRef is not needed in Inventory
        };

        return module;
    }


    public void Update()
    {
        if (PendingItems.Count > 0)
            UiManager.canCloseButton = false;
        else
            UiManager.canCloseButton = true;
    }

    public void OpenInventory()
    {
        UiManager.UpdateUI(Wagons, PendingItems);
    }

    public bool AddItemToModule(StorageModule module, Item item)
    {
        if (module == null)
        {
            Debug.LogError("AddItemToModule failed: Module is null.");
            return false;
        }

        if (item == null)
        {
            Debug.LogError("AddItemToModule failed: Item is null.");
            return false;
        }

        if (item.Count <= 0)
        {
            Debug.LogWarning("AddItemToModule failed: Item count is zero or negative.");
            return false;
        }

        // Check for storage type compatibility
        if (module.moduleData.storage_type != item.ItemData.store_type)
        {
            Debug.LogWarning($"Cannot add {item.ItemData.item_name} to module. Storage type mismatch: " +
                             $"{item.ItemData.store_type} vs {module.moduleData.storage_type}.");
            return false;
        }

        // **New Constraint: Only one type of fluid or livestock per container**
        // Assuming StorageType is an enum with values like Fluid, Livestock, etc.
        if (module.moduleData.storage_type == StorageType.Fluid || module.moduleData.storage_type == StorageType.Livestock)
        {
            if (module.items.Count > 0)
            {
                // Retrieve the first item's ItemData to compare types
                var existingItem = module.items[0].ItemData;

                // Check if the existing item's type differs from the new item's type
                if (existingItem != item.ItemData)
                {
                    if (module.moduleData.storage_type == StorageType.Fluid)
                    {
                        FloatingTextManager.Show("You can only have one type of fluid in a tank!", 2f);
                    }
                    else if (module.moduleData.storage_type == StorageType.Livestock)
                    {
                        FloatingTextManager.Show("You can only have one type of livestock in a cage!", 2f);
                    }
                    Debug.LogWarning($"Cannot add {item.ItemData.item_name} to module. Only one type is allowed.");
                    return false;
                }
            }
        }
        // **End of New Constraint**

        // Check if there's enough capacity
        if (module.moduleData.capacity >= module.GetCurrentCapacity() + (item.Count * item.ItemData.size))
        {
            // Check if the item already exists in the module
            var existingItem = module.items.Find(i => i.ItemData == item.ItemData);
            if (existingItem != null)
            {
                existingItem.AddCount(item.Count);
            }
            else
            {
                module.AddItem(item.Clone());
            }

            RemoveFromPendingItems(item);
            Debug.Log($"Successfully added {item.Count}x {item.ItemData.item_name} to module.");
            NotifyInventoryUpdated();
            return true;
        }
        else
        {
            int availableSize = module.moduleData.capacity - module.GetCurrentCapacity();
            if (availableSize <= 0)
            {
                Debug.LogWarning("Module has no available capacity.");
                FloatingTextManager.Show("This module has no capacity left!", 2f);
                return false;
            }

            int transferableCount = availableSize / item.ItemData.size;
            if (transferableCount <= 0)
            {
                Debug.LogWarning("Not enough capacity to transfer any items.");
                return false;
            }

            // Prevent transferring zero items
            if (transferableCount > item.Count)
            {
                transferableCount = item.Count;
            }

            Item transferableItem = new Item(item.ItemData, transferableCount);

            // Check if the item already exists in the module
            var existingTransferableItem = module.items.Find(i => i.ItemData == transferableItem.ItemData);
            if (existingTransferableItem != null)
            {
                existingTransferableItem.AddCount(transferableItem.Count);
            }
            else
            {
                module.AddItem(transferableItem);
            }

            int remainingCount = item.Count - transferableCount;
            if (remainingCount > 0)
            {
                Item remainingItem = new Item(item.ItemData, remainingCount);
                RemoveFromPendingItems(item);
                AddToPendingItems(remainingItem);
                Debug.Log($"Remaining {remainingCount}x {item.ItemData.item_name} stayed in pending list.");
            }
            else
            {
                RemoveFromPendingItems(item);
                Debug.Log($"All {transferableCount}x {item.ItemData.item_name} transferred to module.");
            }

            NotifyInventoryUpdated();
            return true;
        }
    }

    public bool RemoveItemFromModule(StorageModule module, Item item, bool destroy)
    {
        if (module == null)
        {
            Debug.LogError("RemoveItemFromModule failed: Module is null.");
            return false;
        }

        if (item == null)
        {
            Debug.LogError("RemoveItemFromModule failed: Item is null.");
            return false;
        }

        var existingItem = module.items.Find(i => i.ItemData == item.ItemData);
        if (existingItem != null)
        {
            if (existingItem.Count >= item.Count)
            {
                existingItem.SubtractCount(item.Count);
                if (existingItem.Count == 0)
                {
                    module.items.Remove(existingItem);
                }

                if (destroy)
                {
                    NotifyInventoryUpdated();
                    return true;
                }

                AddToPendingItems(item.Clone());
                Debug.Log($"Successfully removed {item.Count}x {item.ItemData.item_name} from module to pending.");
                NotifyInventoryUpdated();
                return true;
            }
            else
            {
                Debug.LogWarning($"Module has only {existingItem.Count}x {item.ItemData.item_name}, cannot remove {item.Count}x.");
                return false;
            }
        }
        else
        {
            Debug.LogWarning($"Module does not contain {item.ItemData.item_name}.");
            return false;
        }
    }


    public void SelectWagon(InventoryWagon wagon)
    {
        SelectedWagon = wagon;
        UiManager.DisplayModules(wagon.Modules);
    }

    public void SelectModule(StorageModule module)
    {
        SelectedModule = module;
        UiManager.DisplayModuleItems(module);
    }

    public bool AddToPendingItems(Item item)
    {
        var existingItem = PendingItems.Find(i => i.ItemData == item.ItemData);
        if (existingItem != null)
        {
            existingItem.AddCount(item.Count);
            Debug.Log($"Updated PendingItem: {existingItem.ItemData.item_name}, Count: {existingItem.Count}");
        }
        else
        {
            PendingItems.Add(new Item(item.ItemData, item.Count));
            Debug.Log($"Added new PendingItem: {item.ItemData.item_name}, Count: {item.Count}");
        }

        NotifyInventoryUpdated();
        return true;
    }

    private void RemoveFromPendingItems(Item item)
    {
        var existingItem = PendingItems.Find(i => i.ItemData == item.ItemData);
        if (existingItem != null)
        {
            existingItem.SubtractCount(item.Count);
            Debug.Log($"Updated PendingItem: {existingItem.ItemData.item_name}, Count: {existingItem.Count}");

            if (existingItem.Count <= 0)
            {
                PendingItems.Remove(existingItem);
                Debug.Log($"Removed PendingItem: {existingItem.ItemData.item_name}");
            }
        }
        else
        {
            Debug.LogWarning($"Attempted to remove {item.ItemData.item_name} from PendingItems, but it was not found.");
        }

        NotifyInventoryUpdated();
    }


    private void NotifyInventoryUpdated()
    {
        OnInventoryUpdated?.Invoke();
        UiManager.UpdateUI(Wagons, PendingItems);
    }

    public void BuyItem(Item item, float currentPrice)
    {
        if (PlayerMaster.Instance().removeMoney(currentPrice))
        {
            AddToPendingItems(item);
            Debug.Log("Bought: " + item.ItemData.item_name + " for: " + currentPrice);
        }
    }

    public void SellItem(Item item, int amount)
    {
        var _item = SelectedModule.items.Find(i => i == item);
        int removeAmount = Mathf.Max(Mathf.Min(_item.Count, amount), 1);

        float itemPrice = tradeManager.getCurrentSettlement().getPrices().Find(i => i.itemData == item.ItemData).realPrice;
        if (itemPrice <= 0)
        {
            FloatingTextManager.Show("You can't sell this item here!", 2f);
            return;
        }
        Debug.Log("Sold: " + item.ItemData.item_name + " for: " + itemPrice);
        Debug.Log("Player trading location is in: " + tradeManager.getCurrentSettlement());

        RemoveItemFromModule(SelectedModule, new Item(_item.ItemData, removeAmount), true);

        PlayerMaster.Instance().addMoney(removeAmount * itemPrice);
    }

    public void SellFromPendingItem(Item item, int amount)
    {
        float itemPrice = tradeManager.getCurrentSettlement().getPrices().Find(i => i.itemData == item.ItemData).realPrice;
        if (itemPrice <= 0)
        {
            FloatingTextManager.Show("You can't sell this item here!", 2f);
            return;
        }
        Debug.Log("Sold: " + item.ItemData.item_name + " for: " + itemPrice);
        Debug.Log("Player trading location is in: " + tradeManager.getCurrentSettlement());

        RemoveFromPendingItems(new Item(item.ItemData, amount));

        PlayerMaster.Instance().addMoney(amount * itemPrice);
    }

    public void DropItem(Item item, int amount)
    {
        RemoveFromPendingItems(new Item(item.ItemData, amount));
    }


    /// <summary>
    /// Automatically sorts pending items into available storage modules across all wagons.
    /// This method attempts to distribute items efficiently, respecting module capacities and storage types.
    /// </summary>
    public void AutoSort()
    {
        Debug.Log("AutoSort initiated.");

        bool itemsTransferred;

        // Continue sorting as long as items are being transferred and there are items left to sort
        do
        {
            itemsTransferred = false;

            // Create a fresh copy of PendingItems to iterate over
            List<Item> itemsToSort = new List<Item>(PendingItems);

            foreach (Item pendingItem in itemsToSort)
            {
                Debug.Log($"Attempting to sort {pendingItem.Count}x {pendingItem.ItemData.item_name} (Type: {pendingItem.ItemData.store_type}).");

                // Collect all modules compatible with the item's storage type
                List<StorageModule> compatibleModules = new List<StorageModule>();

                foreach (InventoryWagon wagon in Wagons)
                {
                    foreach (StorageModule module in wagon.Modules)
                    {
                        if (module.moduleData.storage_type == pendingItem.ItemData.store_type)
                        {
                            compatibleModules.Add(module);
                        }
                    }
                }

                if (compatibleModules.Count == 0)
                {
                    Debug.LogWarning($"No compatible modules found for {pendingItem.ItemData.item_name}.");
                    continue; // Move to the next pending item
                }

                // Sort compatible modules: prioritize empty modules, then by least available capacity
                compatibleModules.Sort((a, b) =>
                {
                    bool aEmpty = a.GetCurrentCapacity() == 0;
                    bool bEmpty = b.GetCurrentCapacity() == 0;

                    if (aEmpty && !bEmpty)
                        return -1; // a comes before b
                    if (!aEmpty && bEmpty)
                        return 1; // b comes before a

                    // If both are empty or both are not empty, sort by available capacity ascending
                    float aAvailable = a.moduleData.capacity - a.GetCurrentCapacity();
                    float bAvailable = b.moduleData.capacity - b.GetCurrentCapacity();
                    return aAvailable.CompareTo(bAvailable);
                });

                // Attempt to add the pending item to each compatible module in sorted order
                foreach (StorageModule module in compatibleModules)
                {
                    if (pendingItem.Count <= 0)
                        break; // All items have been sorted

                    // Attempt to add as much as possible to the current module
                    bool added = AddItemToModule(module, pendingItem);

                    if (added)
                    {
                        Debug.Log($"Added items to module in wagon.");
                        itemsTransferred = true; // Indicate that a transfer occurred
                    }
                }
            }

        } while (itemsTransferred && PendingItems.Count > 0); // Continue if transfers occurred and items remain

        // Final update to UI and notify listeners
        NotifyInventoryUpdated();
        Debug.Log("AutoSort completed.");
    }

}

