using System;
using UnityEngine;

public class SetPassiveItemEvent : MonoBehaviour
{
    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnEquippedPassiveItem;

    public void CallEquipPassiveItem(Player player, PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName, bool isSwap = false)
    {
        OnEquippedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { player = player, passiveItem = passiveItem, passiveItemSlotName = passiveItemSlotName, isSwap = isSwap});
    }

    public event Action<SetPassiveItemEvent, SetPassiveItemEventArgs> OnRemovedPassiveItem;

    public void CallRemovePassiveItem(Player player, PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName, bool isSwap = false, bool dropButton = false)
    {
        OnRemovedPassiveItem?.Invoke(this, new SetPassiveItemEventArgs { player = player, passiveItem = passiveItem, passiveItemSlotName = passiveItemSlotName, isSwap = isSwap, dropButton = dropButton });
    }
}

public class SetPassiveItemEventArgs : EventArgs
{
    public Player player;
    public PassiveItem passiveItem;
    public PassiveItem targetPassiveItem;
    public PassiveItemSlotName passiveItemSlotName;
    public bool dragIntoInventory;
    public bool isSwap;
    public bool dropButton;
}
