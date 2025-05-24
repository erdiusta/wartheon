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

    public ItemGeneric[] inventoryArray = new ItemGeneric[5];

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

    public ItemGeneric GetInventoryItem(int indexNumber) => inventoryArray[indexNumber];

    public GameObject GetDropButtonObject() => dropButton;

    public void SetOriginalSlotIndex(int index)
    {
        originalSlotIndex = index;
    }

    public int GetOriginalSlotIndex() => originalSlotIndex;
}
