using UnityEngine;

[System.Serializable]
public class Item
{
    public ItemData ItemData { get; private set; }
    public int Count { get; private set; }

    public Item(ItemData data, int count)
    {
        ItemData = data;
        Count = count;
    }

    public Item Clone()
    {
        return new Item(this.ItemData, this.Count);
    }

    public void UpdateCount(int newCount)
    {
        Count = Mathf.Max(newCount, 0); // Ensure count doesn't go negative
        Debug.Log($"Item Updated: {ItemData.item_name}, New Count: {Count}");
    }

    public void AddCount(int amount)
    {
        Count += amount;
        Debug.Log($"Item Added: {ItemData.item_name}, Amount Added: {amount}, New Count: {Count}");
    }

    public void SubtractCount(int amount)
    {
        Count = Mathf.Max(Count - amount, 0);
        Debug.Log($"Item Subtracted: {ItemData.item_name}, Amount Subtracted: {amount}, New Count: {Count}");
    }
}

