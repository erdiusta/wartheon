using UnityEngine;

public class InventoryManager : SingletonMonobehaviour<InventoryManager> 
{
    public Transform mainHandEquippedSlot;
    public Transform offHandEquippedSlot;
    public GameObject dropButton;
    public int originalSlotIndex = 1;
    public bool mainHandDropped;

    public void ClearMainHandEquippedSlot()
    {
        for (int i = 0; i < mainHandEquippedSlot.childCount; i++)
        {
            Destroy(mainHandEquippedSlot.GetChild(i).gameObject);
        }
    }

    public void ClearOffHandEquippedSlot()
    {
        for (int i = 0; i < offHandEquippedSlot.childCount; i++)
        {
            Destroy(offHandEquippedSlot.GetChild(i).gameObject);
        }
    }

    public void ClearIntendedElementInMainHandEquippedSlot(int childNum)
    {
        Destroy(mainHandEquippedSlot.GetChild(childNum).gameObject);
    }

    public void ClearIntendedElementInOffHandEquippedSlot(int childNum)
    {
        Destroy(offHandEquippedSlot.GetChild(childNum).gameObject);
    }

    public int GetMainHandEquippedChildCounts() => mainHandEquippedSlot.childCount;

    public int GetOffHandEquippedChildCounts() => offHandEquippedSlot.childCount;

    public GameObject GetDropButtonObject() => dropButton;

    public void SetOriginalSlotIndex(int index)
    {
        originalSlotIndex = index;
    }

    public int GetOriginalSlotIndex() => originalSlotIndex;
}
