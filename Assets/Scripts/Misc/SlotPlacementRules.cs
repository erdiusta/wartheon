public static class SlotPlacementRules
{
    public static bool IsPlacementAllowed(ItemGeneric item, SlotType targetSlotType, Weapon mainHandWeapon, int weaponSetIndexNumber)
    {
        if (item is Weapon && (targetSlotType == SlotType.Active || targetSlotType == SlotType.Passive)) return false;

        if (item is PassiveItem && (targetSlotType == SlotType.WeaponMainHand || targetSlotType == SlotType.WeaponOffHand ||
            targetSlotType == SlotType.Active)) return false;

        if (NotPossibleToPlaceToOffHand(item, mainHandWeapon) && targetSlotType == SlotType.WeaponOffHand) return false; // Two-handed cant't be placed on off-hand

        if (NotPossibleToPlaceMainHandWeaponToInventoryOrDrop(item, weaponSetIndexNumber) && (targetSlotType == SlotType.Inventory || targetSlotType == SlotType.Drop)) return false;

        return true;
    }

    // Overload method for pick-up swap
    public static bool IsSwapAllowed(Weapon toBePickedUpWeapon, Weapon toBeDroppedWeapon, Weapon mainHandWeapon,Weapon offHandWeapon, bool isDropOffHand) 
    {
        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(toBeDroppedWeapon.weaponStats.weaponTitle);

        if (isDropOffHand)
        {
            if (weaponDetails.weaponClass == WeaponClass.Spear) return false; // Can swap spear to off-hand

            if (weaponDetails.wieldType == WieldType.OneHanded) return true; // Can swap for one-hand at offhand - offhhand

            if (weaponDetails.wieldType == WieldType.TwoHanded) return false; // Can't place two-hand to off-hand
        }
        else
        {
            if (weaponDetails.weaponClass == WeaponClass.Shield) return false; // Can't place shield to main hand

            if (weaponDetails.wieldType == WieldType.TwoHanded)
            {
                if (offHandWeapon != null) return false; // Can't place two thanded when off-hand is full

                else return true;
            }

            if (weaponDetails.wieldType == WieldType.OneHanded) return true;
        }

        return false;
    }

    public static bool IsSwapAllowed(DraggableItem draggedItem, DraggableItem targetDraggableItem, ItemGeneric draggingItem, ItemGeneric targetItem, Weapon mainHandWeapon, 
        Weapon offHandWeapon, Weapon peekedWeaponSetsOffHandWeapon, out ItemSwapPos itemSwapPos)
    {
        itemSwapPos = ItemSwapPos.None;

        if (draggingItem is PassiveItem && targetItem is not PassiveItem) return false;

        if (draggingItem is Weapon && targetItem is not Weapon) return false;

        if (draggingItem is PassiveItem dp && targetItem is PassiveItem tp)
        {
            // SWAP FAILS
            if (dp.passiveStats.passiveItemSlotName != tp.passiveStats.passiveItemSlotName) return false;

            // SWAP TYPE
            if (dp.ItemSlotStatus == ItemSlotStatus.Inventory && tp.ItemSlotStatus != ItemSlotStatus.Inventory) 
                itemSwapPos = ItemSwapPos.DragPassiveInventorySlotPassive;

            if (dp.ItemSlotStatus != ItemSlotStatus.Inventory && tp.ItemSlotStatus == ItemSlotStatus.Inventory) 
                itemSwapPos = ItemSwapPos.DragPassiveSlotPassiveInventory;
        }

        if (draggingItem is Weapon dw && targetItem is Weapon tw)
        {
            // SWAP FAILS
            // Can't place shield to main hand
            if (dw.weaponStats.weaponClass == WeaponClass.Shield && tw.ItemSlotStatus == ItemSlotStatus.MainHand) return false;
            if (dw.ItemSlotStatus == ItemSlotStatus.MainHand && tw.weaponStats.weaponClass == WeaponClass.Shield) return false;

            // Can't swap one-hand with two-hand if off-hand is full
            if (dw.weaponStats.wieldType == WieldType.OneHanded && offHandWeapon != null && tw.weaponStats.wieldType == WieldType.TwoHanded) return false;

            // Can't swap your two hand weapon with another set if this set's off hand is full
            if (dw.weaponStats.wieldType == WieldType.TwoHanded && tw.weaponStats.wieldType == WieldType.OneHanded && peekedWeaponSetsOffHandWeapon != null) return false;

            // Can't swap between main hand and off-hand weapon if off-hand is shield
            if (dw.ItemSlotStatus == ItemSlotStatus.MainHand && tw.ItemSlotStatus == ItemSlotStatus.OffHand && 
                tw.weaponStats.weaponClass == WeaponClass.Shield) return false;

            if (dw.ItemSlotStatus == ItemSlotStatus.OffHand && dw.weaponStats.weaponClass == WeaponClass.Shield &&
                tw.ItemSlotStatus == ItemSlotStatus.MainHand) return false;

            // Can't swap between main hand and off-hand weapon if main hand is one-hand spear
            if (dw.ItemSlotStatus == ItemSlotStatus.MainHand && dw.weaponStats.weaponClass == WeaponClass.Spear  &&
                tw.ItemSlotStatus == ItemSlotStatus.OffHand) return false;

            if (dw.ItemSlotStatus == ItemSlotStatus.OffHand && tw.ItemSlotStatus == ItemSlotStatus.MainHand && 
                tw.weaponStats.weaponClass == WeaponClass.Shield) return false;


            // SWAP TYPE
            if (dw.ItemSlotStatus == ItemSlotStatus.MainHand && tw.ItemSlotStatus == ItemSlotStatus.MainHand) itemSwapPos = ItemSwapPos.DragMainSlotMain;

            if (dw.ItemSlotStatus == ItemSlotStatus.MainHand && tw.ItemSlotStatus == ItemSlotStatus.OffHand) itemSwapPos = ItemSwapPos.DragMainSlotOff;

            if (dw.ItemSlotStatus == ItemSlotStatus.OffHand && tw.ItemSlotStatus == ItemSlotStatus.MainHand) itemSwapPos = ItemSwapPos.DragOffSlotMain;

            if (dw.ItemSlotStatus == ItemSlotStatus.OffHand && tw.ItemSlotStatus == ItemSlotStatus.OffHand) itemSwapPos = ItemSwapPos.DragOffSlotOff;

            if (dw.ItemSlotStatus == ItemSlotStatus.MainHand && tw.ItemSlotStatus == ItemSlotStatus.Inventory) itemSwapPos = ItemSwapPos.DragMainSlotInventory;

            if (dw.ItemSlotStatus == ItemSlotStatus.OffHand && tw.ItemSlotStatus == ItemSlotStatus.Inventory) itemSwapPos = ItemSwapPos.DragOffSlotInventory;

            if (dw.ItemSlotStatus == ItemSlotStatus.Inventory && tw.ItemSlotStatus == ItemSlotStatus.MainHand) itemSwapPos = ItemSwapPos.DragInventorySlotMain;

            if (dw.ItemSlotStatus == ItemSlotStatus.Inventory && tw.ItemSlotStatus == ItemSlotStatus.OffHand) itemSwapPos = ItemSwapPos.DragInventorySlotOff;
        }

        return true;
    }

    private static bool NotPossibleToPlaceToOffHand(ItemGeneric item, Weapon mainHandWeapon)
    {
        if (item is Weapon)
        {
            Weapon weapon = (Weapon)item;

            if (weapon.weaponStats.wieldType == WieldType.TwoHanded) return true; // Weapon is two handed
        }

        return false;
    }

    private static bool NotPossibleToPlaceMainHandWeaponToInventoryOrDrop(ItemGeneric item, int weaponSetIndexNumber)
    {
        if (item is Weapon)
        {
            Weapon weapon = (Weapon)item;

            if (weaponSetIndexNumber == 1 && weapon.ItemSlotStatus == ItemSlotStatus.MainHand) return true;
        }

        return false;
    }
}
