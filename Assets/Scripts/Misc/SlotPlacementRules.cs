public static class SlotPlacementRules
{
    public static bool IsPlacementAllowed(ItemGeneric item, SlotType targetSlotType, Weapon mainHandWeapon, int weaponSetIndexNumber)
    {
        if (item is Weapon && (targetSlotType == SlotType.Active || targetSlotType == SlotType.Passive)) return false;

        if (item is PassiveItem && (targetSlotType == SlotType.WeaponMainHand || targetSlotType == SlotType.WeaponOffHand ||
            targetSlotType == SlotType.Active)) return false;

        if (NotPossibleToPlaceToOffHand(item, mainHandWeapon) && targetSlotType == SlotType.WeaponOffHand) return false; // Two-handed cant't be placed on off-hand

        if (NotPossibleToPlaceMainHandWeaponToInventoryOrDrop(item, weaponSetIndexNumber) && (targetSlotType == SlotType.None || targetSlotType == SlotType.Drop)) return false;

        return true;
    }

    // Overload method for pick-up swap
    public static bool IsSwapAllowed(Weapon toBePickedUpWeapon, Weapon toBeDroppedWeapon, Weapon mainHandWeapon,Weapon offHandWeapon, bool isDropOffHand) 
    {
        if (isDropOffHand)
        {
            if (toBePickedUpWeapon.weaponDetails.weaponClass == WeaponClass.Spear) return false; // Can swap spear to off-hand

            if (toBePickedUpWeapon.weaponDetails.wieldType == WieldType.OneHanded) return true; // Can swap for one-hand at offhand - offhhand

            if (toBePickedUpWeapon.weaponDetails.wieldType == WieldType.TwoHanded) return false; // Can't place two-hand to off-hand
        }
        else
        {
            if (toBePickedUpWeapon.weaponDetails.weaponClass == WeaponClass.Shield) return false; // Can't place shield to main hand

            if (toBePickedUpWeapon.weaponDetails.wieldType == WieldType.TwoHanded)
            {
                if (offHandWeapon != null) return false; // Can't place two thanded when off-hand is full

                else return true;
            }

            if (toBePickedUpWeapon.weaponDetails.wieldType == WieldType.OneHanded) return true;
        }

        return false;
    }

    public static bool IsSwapAllowed(DraggableItem draggedItem, DraggableItem targetDraggableItem, ItemGeneric draggingItem, ItemGeneric targetItem, Weapon mainHandWeapon, 
        Weapon offHandWeapon, Weapon peekedWeaponSetsOffHandWeapon, out ItemSwapPos itemSwapPos)
    {
        itemSwapPos = ItemSwapPos.None;

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
