using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Represents an effect that adds or removes items from the inventory.
/// </summary>
[System.Serializable]
public class ItemEffect : EventEffect<List<Item>>
{
    [Tooltip("If true, adds items to the inventory; if false, removes items from the inventory.")]
    public bool isAddingItems;

    [Tooltip("A list of items to add or remove.")]
    public List<ItemData> items = new List<ItemData>();

    [Tooltip("The range for the number of different items to add/remove.")]
    public Vector2Int itemRange;

    [Tooltip("The range for the count of each item to add/remove.")]
    public Vector2Int itemCountRange;

    /// <summary>
    /// Applies the item effect by adding or removing items from the inventory.
    /// Returns a list of items that were added or removed.
    /// </summary>
    /// <returns>List of items added or removed with their respective counts.</returns>
    public override List<Item> ApplyTypedEffect()
    {
        List<Item> affectedItems = new List<Item>();

        if (isAddingItems)
        {
            affectedItems = AddItemsToInventory();
        }
        else
        {
            affectedItems = RemoveItemsFromInventory();
        }

        return affectedItems;
    }

    /// <summary>
    /// Adds items to the inventory based on the specified ranges.
    /// </summary>
    /// <returns>List of items that were added.</returns>
    private List<Item> AddItemsToInventory()
    {
        List<Item> addedItems = new List<Item>();

        if (items == null || items.Count == 0)
        {
            Debug.LogWarning("AddItemsToInventory failed: No items specified to add.");
            return addedItems;
        }

        // Determine the number of different items to add
        int maxItemsToAdd = Mathf.Min(UnityEngine.Random.Range(itemRange.x, itemRange.y + 1), items.Count);
        if (maxItemsToAdd <= 0)
        {
            Debug.LogWarning("AddItemsToInventory failed: itemRange resulted in zero items to add.");
            return addedItems;
        }

        // Shuffle the items list to ensure random selection
        List<ItemData> shuffledItems = new List<ItemData>(items);
        ShuffleList(shuffledItems);

        // Select the required number of items
        List<ItemData> selectedItems = shuffledItems.GetRange(0, maxItemsToAdd);

        foreach (ItemData itemData in selectedItems)
        {
            // Determine the quantity to add for this item
            int quantityToAdd = UnityEngine.Random.Range(itemCountRange.x, itemCountRange.y + 1);
            if (quantityToAdd <= 0)
                continue;

            Item newItem = new Item(itemData, quantityToAdd);

            // Find a suitable module to add the item
            bool itemAdded = false;
            foreach (InventoryWagon wagon in Inventory.Instance.Wagons)
            {
                bool added = Inventory.Instance.AddToPendingItems(newItem);
                if (added)
                {
                    addedItems.Add(newItem);
                    itemAdded = true;
                    break; // Move to the next item
                }

                if (itemAdded)
                    break; // Move to the next item
            }

            if (!itemAdded)
            {
                Debug.LogWarning($"Failed to add {quantityToAdd}x {itemData.item_name}: No suitable storage module found.");
                FloatingTextManager.Show($"Cannot add {itemData.item_name}: No suitable storage module.", 2f);
            }
        }

        return addedItems;
    }

    /// <summary>
    /// Removes items from the inventory based on the specified ranges.
    /// </summary>
    /// <returns>List of items that were removed.</returns>
    private List<Item> RemoveItemsFromInventory()
    {
        List<Item> removedItems = new List<Item>();

        // Collect all unique items from the inventory
        List<Item> allInventoryItems = new List<Item>();
        foreach (InventoryWagon wagon in Inventory.Instance.Wagons)
        {
            foreach (StorageModule module in wagon.Modules)
            {
                foreach (Item item in module.items)
                {
                    // Check if the item is already in the list
                    Item existingItem = allInventoryItems.Find(i => i.ItemData == item.ItemData);
                    if (existingItem != null)
                    {
                        existingItem.AddCount(item.Count);
                    }
                    else
                    {
                        allInventoryItems.Add(new Item(item.ItemData, item.Count));
                    }
                }
            }
        }

        if (allInventoryItems.Count == 0)
        {
            Debug.LogWarning("RemoveItemsFromInventory failed: Inventory is empty.");
            return removedItems;
        }

        // Determine the number of different items to remove
        int maxItemsToRemove = Mathf.Min(UnityEngine.Random.Range(itemRange.x, itemRange.y + 1), allInventoryItems.Count);
        if (maxItemsToRemove <= 0)
        {
            Debug.LogWarning("RemoveItemsFromInventory failed: itemRange resulted in zero items to remove.");
            return removedItems;
        }

        // Shuffle the inventory items list to ensure random selection
        List<Item> shuffledInventoryItems = new List<Item>(allInventoryItems);
        ShuffleList(shuffledInventoryItems);

        // Select the required number of items to remove
        List<Item> selectedItems = shuffledInventoryItems.GetRange(0, maxItemsToRemove);

        foreach (Item inventoryItem in selectedItems)
        {
            // Determine the quantity to remove for this item
            int quantityToRemove = UnityEngine.Random.Range(itemCountRange.x, itemCountRange.y + 1);
            if (quantityToRemove <= 0)
                continue;

            int actualRemoved = 0;

            // Traverse the inventory to remove the item
            foreach (InventoryWagon wagon in Inventory.Instance.Wagons)
            {
                foreach (StorageModule module in wagon.Modules)
                {
                    // Find the item in the module
                    Item moduleItem = module.items.Find(i => i.ItemData == inventoryItem.ItemData);
                    if (moduleItem != null)
                    {
                        int removeCount = Mathf.Min(quantityToRemove - actualRemoved, moduleItem.Count);
                        if (removeCount <= 0)
                            continue;

                        bool removed = Inventory.Instance.RemoveItemFromModule(module, new Item(inventoryItem.ItemData, removeCount), true);
                        if (removed)
                        {
                            actualRemoved += removeCount;
                            removedItems.Add(new Item(inventoryItem.ItemData, removeCount));
                            Debug.Log($"Removed {removeCount}x {inventoryItem.ItemData.item_name} from inventory.");

                            if (actualRemoved >= quantityToRemove)
                                break; // Move to the next item
                        }
                    }
                }

                if (actualRemoved >= quantityToRemove)
                    break; // Move to the next item
            }

            if (actualRemoved < quantityToRemove)
            {
                Debug.LogWarning($"Could only remove {actualRemoved}x {inventoryItem.ItemData.item_name} instead of {quantityToRemove}x.");
            }
        }

        return removedItems;
    }

    /// <summary>
    /// Shuffles a list in-place using the Fisher-Yates algorithm.
    /// </summary>
    /// <typeparam name="T">Type of list elements.</typeparam>
    /// <param name="list">List to shuffle.</param>
    private void ShuffleList<T>(List<T> list)
    {
        for (int i = list.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            T temp = list[i];
            list[i] = list[j];
            list[j] = temp;
        }
    }
}
