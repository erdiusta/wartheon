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

    public int PlaceItemToInventoryIndexSlot(ItemGeneric itemGeneric, bool placeToLowestIndex = true, int specificIndex = -1)
    {
        if (itemGeneric == null) return -1;

        int existingIndex = FindIndexOfItem(itemGeneric);

        if (existingIndex != -1)
        {
            inventoryArray[existingIndex] = null;
        }

        itemGeneric.itemSlotStatus = ItemSlotStatus.Inventory;

        if (placeToLowestIndex)
        {
            for (int i = 0; i < inventoryArray.Length; i++)
            {
                if (inventoryArray[i] == null)
                {
                    inventoryArray[i] = itemGeneric;
                    return i;
                }
            }
        }
        else
        {
            if (specificIndex < 0 || specificIndex >= inventoryArray.Length)
            {
                Debug.LogWarning($"Invalid inventory index: {specificIndex}");
                return -1;
            }

            inventoryArray[specificIndex] = itemGeneric;
            return specificIndex;
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
        if (index < 0 || index >= inventoryArray.Length)
        {
            Debug.LogWarning($"EmptyItemFromInventory: index {index} out of range");
            return;
        }

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

    public void ReplaceItemAt(int index, ItemGeneric item)
    {
        inventoryArray[index] = item;
        //StaticEventHandler.Instance.CallInventorySlotChanged(index);
    }

    public int GetOriginalSlotIndex() => originalSlotIndex;
}
