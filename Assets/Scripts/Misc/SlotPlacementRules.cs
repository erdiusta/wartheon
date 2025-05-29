public static class SlotPlacementRules
{
    public static bool IsPlacementAllowed(ItemGeneric item, Slot targetSlot, Weapon mainHandWeapon, int weaponSetIndexNumber)
    {
        if (item is ActiveItem) return targetSlot.slotType == SlotType.Drop;

        if (item is Weapon && (targetSlot.slotType == SlotType.Active || targetSlot.slotType == SlotType.Passive)) return false;

        if (item is PassiveItem && (targetSlot.slotType == SlotType.WeaponMainHand || targetSlot.slotType == SlotType.WeaponOffHand ||
            targetSlot.slotType == SlotType.Active)) return false;

        if (NotPossibleToPlaceToOffHand(item, mainHandWeapon) && targetSlot.slotType == SlotType.WeaponOffHand) return false; // Two-handed cant't be placed on off-hand

        if (NotPossibleToPlaceMainHandWeaponToInventoryOrDrop(item, weaponSetIndexNumber) && (targetSlot.slotType == SlotType.None || targetSlot.slotType == SlotType.Drop)) return false;

        return true;
    }

    public static bool IsSwapAllowed(DraggableItem draggedItem, DraggableItem targetDraggableItem, ItemGeneric draggingItem, ItemGeneric targetItem, Weapon mainHandWeapon, 
        Weapon offHandWeapon, Weapon peekedWeaponSetsOffHandWeapon, out ItemSwapPos itemSwapPos)
    {
        itemSwapPos = ItemSwapPos.None;

        if (draggingItem is ActiveItem || targetItem is ActiveItem) return false;

        if (draggingItem is PassiveItem && targetItem is not PassiveItem) return false;

        if (draggingItem is Weapon && targetItem is not Weapon) return false;

        if (draggingItem.itemSlotStatus == ItemSlotStatus.Inventory && targetItem.itemSlotStatus == ItemSlotStatus.Inventory) return false; 

            if (draggingItem is PassiveItem && targetItem is PassiveItem)
        {
            PassiveItem draggingPassiveItem = (PassiveItem)draggingItem;
            PassiveItem targetPassiveItem = (PassiveItem)targetItem;

            // SWAP FAILS
            if (draggingPassiveItem.passiveItemDetails.passiveItemSlotName != targetPassiveItem.passiveItemDetails.passiveItemSlotName) return false;

            // SWAP TYPE
            if (draggingPassiveItem.itemSlotStatus == ItemSlotStatus.Inventory && targetItem.itemSlotStatus != ItemSlotStatus.Inventory) itemSwapPos = ItemSwapPos.DragPassiveInventorySlotPassive;

            if (draggingPassiveItem.itemSlotStatus != ItemSlotStatus.Inventory && targetItem.itemSlotStatus == ItemSlotStatus.Inventory) itemSwapPos = ItemSwapPos.DragPassiveSlotPassiveInventory;
        }

        if (draggingItem is Weapon && targetItem is Weapon)
        {
            Weapon draggingWeapon = (Weapon)draggingItem;
            Weapon targetWeapon = (Weapon)targetItem;

            // SWAP FAILS
            // Can't place shield to main hand
            if (draggingWeapon.weaponDetails.weaponClass == WeaponClass.Shield && targetItem.itemSlotStatus == ItemSlotStatus.MainHand) return false;
            if (draggingItem.itemSlotStatus == ItemSlotStatus.MainHand && targetWeapon.weaponDetails.weaponClass == WeaponClass.Shield) return false;


            // Can't swap one-hand with two-hand if off-hand is full
            if (draggingWeapon.weaponDetails.wieldType == WieldType.OneHanded && offHandWeapon != null && targetWeapon.weaponDetails.wieldType == WieldType.TwoHanded) return false;

            // Can't swap your two hand weapon with another set if this set's off hand is full
            if (draggingWeapon.weaponDetails.wieldType == WieldType.TwoHanded && targetWeapon.weaponDetails.wieldType == WieldType.OneHanded && peekedWeaponSetsOffHandWeapon != null) return false;

            // Can't swap between main hand and off-hand weapon if off-hand is shield
            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.MainHand && targetWeapon.itemSlotStatus == ItemSlotStatus.OffHand && 
                targetWeapon.weaponDetails.weaponClass == WeaponClass.Shield) return false;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.OffHand && draggingWeapon.weaponDetails.weaponClass == WeaponClass.Shield &&
                targetItem.itemSlotStatus == ItemSlotStatus.MainHand) return false;

            // Can't swap between main hand and off-hand weapon if main hand is one-hand spear
            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.MainHand && draggingWeapon.weaponDetails.weaponClass == WeaponClass.Spear  &&
                targetWeapon.itemSlotStatus == ItemSlotStatus.OffHand) return false;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.OffHand && targetWeapon.itemSlotStatus == ItemSlotStatus.MainHand && 
                targetWeapon.weaponDetails.weaponClass == WeaponClass.Shield) return false;


            // SWAP TYPE
            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.MainHand && targetItem.itemSlotStatus == ItemSlotStatus.MainHand) itemSwapPos = ItemSwapPos.DragMainSlotMain;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.MainHand && targetWeapon.itemSlotStatus == ItemSlotStatus.OffHand) itemSwapPos = ItemSwapPos.DragMainSlotOff;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.OffHand && targetItem.itemSlotStatus == ItemSlotStatus.MainHand) itemSwapPos = ItemSwapPos.DragOffSlotMain;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.OffHand && targetWeapon.itemSlotStatus == ItemSlotStatus.OffHand) itemSwapPos = ItemSwapPos.DragOffSlotOff;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.MainHand && targetWeapon.itemSlotStatus == ItemSlotStatus.Inventory) itemSwapPos = ItemSwapPos.DragMainSlotInventory;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.OffHand && targetWeapon.itemSlotStatus == ItemSlotStatus.Inventory) itemSwapPos = ItemSwapPos.DragOffSlotInventory;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.Inventory && targetItem.itemSlotStatus == ItemSlotStatus.MainHand) itemSwapPos = ItemSwapPos.DragInventorySlotMain;

            if (draggingWeapon.itemSlotStatus == ItemSlotStatus.Inventory && targetItem.itemSlotStatus == ItemSlotStatus.OffHand) itemSwapPos = ItemSwapPos.DragInventorySlotOff;
        }

        return true;
    }

    private static bool NotPossibleToPlaceToOffHand(ItemGeneric item, Weapon mainHandWeapon)
    {
        if (item is Weapon)
        {
            Weapon weapon = (Weapon)item;

            if (weapon.weaponDetails.wieldType == WieldType.TwoHanded) return true; // Weapon is two handed

            if (mainHandWeapon == null) return true; // Char has no weapon on main hand
        }

        return false;
    }

    private static bool NotPossibleToPlaceMainHandWeaponToInventoryOrDrop(ItemGeneric item, int weaponSetIndexNumber)
    {
        if (item is Weapon)
        {
            Weapon weapon = (Weapon)item;

            if (weaponSetIndexNumber == 1 && weapon.itemSlotStatus == ItemSlotStatus.MainHand) return true;
        }

        return false;
    }
}
