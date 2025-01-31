using System;
using UnityEngine;

public class SetPassiveItemEvent : MonoBehaviour
{
    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnEquippedPassiveItem;

    public void CallEquipPassiveItem(PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName)
    {
        OnEquippedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { passiveItem = passiveItem, passiveItemSlotName = passiveItemSlotName });
    }

    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnRemovedPassiveItem;

    public void CallRemovePassiveItem(PassiveItemSlotName passiveItemSlotName)
    {
        OnRemovedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { passiveItemSlotName = passiveItemSlotName });
    }
}

public class SetPassiveItemEventArgs : EventArgs
{
    public PassiveItem passiveItem;
    public PassiveItemSlotName passiveItemSlotName;
}
