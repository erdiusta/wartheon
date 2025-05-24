using System;
using UnityEngine;

public class SetPassiveItemEvent : MonoBehaviour
{
    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnEquippedPassiveItem;

    public void CallEquipPassiveItem(PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName)
    {
        OnEquippedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { passiveItem = passiveItem, passiveItemSlotName = passiveItemSlotName});
    }

    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnRemovedPassiveItem;

    public void CallRemovePassiveItem(PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName, bool dragIntoInventory)
    {
        OnRemovedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { passiveItem = passiveItem, passiveItemSlotName = passiveItemSlotName, dragIntoInventory = dragIntoInventory });
    }
}

public class SetPassiveItemEventArgs : EventArgs
{
    public PassiveItem passiveItem;
    public PassiveItemSlotName passiveItemSlotName;
    public bool dragIntoInventory;
}
