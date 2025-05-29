using System;
using UnityEngine;

public class SetPassiveItemEvent : MonoBehaviour
{
    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnEquippedPassiveItem;

    public void CallEquipPassiveItem(PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName, EquipResult equipResult, bool isSwap = false)
    {
        OnEquippedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { passiveItem = passiveItem, passiveItemSlotName = passiveItemSlotName, 
            equipResult = equipResult, isSwap = isSwap});
    }

    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnRemovedPassiveItem;

    public void CallRemovePassiveItem(PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName,  EquipResult equipResult, bool isSwap = false)
    {
        OnRemovedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { passiveItem = passiveItem, passiveItemSlotName = passiveItemSlotName, 
            equipResult = equipResult, isSwap = isSwap});
    }
}

public class SetPassiveItemEventArgs : EventArgs
{
    public PassiveItem passiveItem;
    public PassiveItem targetPassiveItem;
    public PassiveItemSlotName passiveItemSlotName;
    public bool dragIntoInventory;
    public EquipResult equipResult;
    public bool isSwap;
}
