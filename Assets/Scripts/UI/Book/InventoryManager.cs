using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager> 
{
    public Transform mainHandBackgroundSlot;
    public Transform mainHandEquippedSlot;
    public Transform offHandBackgroundSlot;
    public Transform offHandEquippedSlot;
    public Transform activeItemBackgroundSlot;
    public Transform activeItemEquippedSlot;
    public GameObject dropButton;
    public int originalSlotIndex = 1;
    public bool mainHandDropped;

    public ItemGeneric[] inventoryArray = new ItemGeneric[12];

    public int CurrentWeaponSlotSetIndex { get { return currentWeaponSlotSetIndex; } set { currentWeaponSlotSetIndex = value; } }

    int currentWeaponSlotSetIndex = 1;

    /// <summary>
    /// Check if inventory is full or not
    /// </summary>
    public bool IsInventoryFull()
    {
        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i] == null) return false;
        }

        return true;
    }

    public int PlaceItemToLowestPossibleIndexSlot(ItemGeneric receivable)
    {
        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i] == null)
            {
                receivable.itemSlotStatus = ItemSlotStatus.Inventory;

                inventoryArray[i] = receivable;
                return i;
            }
        }

        return -1;
    }

    public int FindIndexOfItem(ItemGeneric item)
    {
        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i] == item) return i;
        }

        return -1;
    }

    public void EmptyItemFromInventory(int index)
    {
        inventoryArray[index] = null;
    }

    public int GetFlatInventoryIndex(int x, int y) => y * 6 + x; 

    public Vector2Int GetXYFromInventoryIndex(int index) => new Vector2Int(index % 6, index / 6);

    public ItemGeneric GetInventoryItem(int indexNumber) => inventoryArray[indexNumber];

    public GameObject GetDropButtonObject() => dropButton;

    public void SetOriginalSlotIndex(int index)
    {
        originalSlotIndex = index;
    }

    public ItemGeneric ReplaceItemAt(int index, ItemGeneric replacement, bool setStatusToInventory = true)
    {
        if (index < 0 || index >= inventoryArray.Length)
        {
            Debug.LogWarning($"InventoryManager.ReplaceItemAt: index {index} is out of range.");
            return null;
        }

        // Preserve previous item so the caller can handle it if necessary
        ItemGeneric previous = inventoryArray[index];

        // Write the new reference
        inventoryArray[index] = replacement;

        // Ensure upgraded items are marked as inventory items (optional toggle)
        if (replacement != null && setStatusToInventory)
        {
            replacement.itemSlotStatus = ItemSlotStatus.Inventory;
        }

        return previous;
    }

    public int GetOriginalSlotIndex() => originalSlotIndex;
}
