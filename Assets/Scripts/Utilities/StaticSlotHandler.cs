using Mirror;

public static class StaticSlotHandler
{
    public const int INVENTORY_SIZE = 12;

    public static bool IsSlotValidated(Character character, uint playerNetId, InventoryItemData data, ItemSlotStatus fromStatus, SlotType toSlot, int fromIndex, int toIndex, int setIndex, 
        ItemGeneric item, out bool nyveranDualWield)
    {
        nyveranDualWield = false;

        // Inventory bounds
        if (fromStatus == ItemSlotStatus.Inventory)
        {
            if (toSlot == SlotType.Inventory && !IsValidIndex(toIndex, INVENTORY_SIZE))
            {
                StaticLogHandler.LogValidationFailure($"Invalid fromIndex: {fromIndex}", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
                return false;
            }
        }

        // TYPE VALIDATION
        // Passive to Weapon Slots
        if (item is PassiveItem && (toSlot == SlotType.WeaponMainHand || toSlot == SlotType.WeaponOffHand))
        {
            StaticLogHandler.LogValidationFailure($"Passive item cannot be placed into weapon slot", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
            return false;
        }

        // Weapon to Passive Slots
        if (item is Weapon && toSlot == SlotType.Passive)
        {
            StaticLogHandler.LogValidationFailure($"Weapon cannot be placed into passive slot", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
            return false;
        }

        // PASSIVE SLOT MATCH VALIDATION
        if (item is PassiveItem passive && toSlot == SlotType.Passive)
        {
            PassiveItemSlotName targetSlot = data.passiveStats.passiveItemSlotName;

            if (passive.passiveStats.passiveItemSlotName != targetSlot)
            {
                StaticLogHandler.LogValidationFailure($"Passive item placed into wrong slot", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
                return false;
            }
        }

        // TWO-HANDED WEAPON RULES
        if (item is Weapon weapon)
        {
            Weapon[] currentSet;

            if (playerNetId == 0) currentSet = GameManager.Instance.GetLocalPlayer().weaponSlotSetArray[setIndex - 1]; // 0 NetID means Single Player
            else
            {
                if (!NetworkServer.spawned.TryGetValue(playerNetId, out NetworkIdentity playerIdentity) || playerIdentity == null)
                {
                    StaticLogHandler.LogValidationFailure("Player not found on server", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
                    return false;
                }

                currentSet = playerIdentity.GetComponent<Player>().weaponSlotSetArray[setIndex - 1];
            }

            Weapon mainHand = currentSet?[0];
            Weapon offHand = currentSet?[1];

            // To MainHand
            if (toSlot == SlotType.WeaponMainHand)
            {
                if (weapon.weaponStats.wieldType == WieldType.TwoHanded)
                {
                    if (offHand != null)
                    {
                        StaticLogHandler.LogValidationFailure($"Cannot equip two-handed weapon while off-hand occupied", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
                        return false;
                    }
                }
            }

            // To Off-hand
            if (toSlot == SlotType.WeaponOffHand)
            {
                // Two-handed cannot go off-hand
                if (weapon.weaponStats.wieldType == WieldType.TwoHanded)
                {
                    StaticLogHandler.LogValidationFailure($"Two-handed weapon cannot be placed in off-hand", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
                    return false;
                }

                // Main hand has two-handed
                if (mainHand != null && mainHand.weaponStats.wieldType == WieldType.TwoHanded)
                {
                    StaticLogHandler.LogValidationFailure($"Cannot equip off-hand while main hand has two-hand weapon", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
                    return false;
                }

                if(character == Character.Nyveran)
                {
                    nyveranDualWield = true;
                }
            }
        }

        // FROM PASSIVE RESTRICTIONS
        if (fromStatus == ItemSlotStatus.Passive && (toSlot == SlotType.WeaponMainHand || toSlot == SlotType.WeaponOffHand))
        {
            StaticLogHandler.LogValidationFailure($"Passive item cannot be moved to weapon slot", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
            return false;
        }

        if (fromStatus == ItemSlotStatus.Passive && toSlot == SlotType.Passive)
        {
            if (item is PassiveItem p)
            {
                //PassiveItemSlotName targetSlot = data.passiveStats.passiveItemSlotName;

                PassiveItemSlotName targetSlot = p.passiveStats.passiveItemSlotName;

                if (p.passiveStats.passiveItemSlotName != targetSlot)
                {
                    StaticLogHandler.LogValidationFailure($"Passive cannot move to different passive slot", playerNetId, fromStatus, fromIndex, toIndex, toSlot);
                    return false;
                }
            }
        }

        return true;
    }


    public static bool IsValidIndex(int index, int inventorySize)
    {
        return index >= 0 && index < inventorySize;
    }

    public static int ShardGainProcess(ItemGeneric item, Player player)
    {
        int shardGain = 0;

        switch (item.Rarity)
        {
            case Rarity.Basic: shardGain = 10; break;
            case Rarity.Enchanted: shardGain = 35; break;
            case Rarity.Mythic: shardGain = 100; break;
            case Rarity.Legendary: shardGain = 250; break;
            default:
                break;
        }

        player.coinsAndShards.AddShard(shardGain);
        return shardGain;
    }

    public static InventoryItemData ConvertDropToData(DropItem drop)
    {
        InventoryItemData data = new InventoryItemData();

        data.itemType = drop.itemGeneric.ItemType;

        if (drop.itemGeneric is Weapon w)
        {
            data.weaponStats = w.weaponStats;
            data.passiveStats = default;
        }
        else if (drop.itemGeneric is PassiveItem p)
        {
            data.weaponStats = default;
            data.passiveStats = p.passiveStats;
        }

        data.rarity = drop.itemGeneric.Rarity;

        // Include the slot status so receivers have that context
        data.itemSlotStatus = drop.itemGeneric.ItemSlotStatus;

        return data;
    }

    public static InventoryItemData ConvertDropToData(DropItemNetwork drop)
    {
        InventoryItemData data = new InventoryItemData();

        if (drop.itemGeneric == null) return default;

        data.itemType = drop.itemGeneric.ItemType;

        if (drop.itemGeneric is Weapon w)
        {
            data.weaponStats = w.weaponStats;
            data.passiveStats = default;
        }
        else if (drop.itemGeneric is PassiveItem p)
        {
            data.weaponStats = default;
            data.passiveStats = p.passiveStats;
        }

        data.rarity = drop.itemGeneric.Rarity;

        // Include the slot status so receivers have that context
        data.itemSlotStatus = drop.itemGeneric.ItemSlotStatus;

        return data;
    }

    public static InventoryItemData ConvertToData(ItemGeneric item)
    {
        InventoryItemData data = new InventoryItemData();

        data.itemType = item.ItemType;

        if (item is Weapon w)
        {
            data.weaponStats = w.weaponStats;
            data.passiveStats = default;
        }
        else if (item is PassiveItem p)
        {
            data.weaponStats = default;
            data.passiveStats = p.passiveStats;
        }

        data.rarity = item.Rarity;

        // Include the slot status so receivers have that context
        data.itemSlotStatus = item.ItemSlotStatus;

        return data;
    }

    public static void ResolvePickUpTarget(Player player, Character character, ItemGeneric item, out SlotType targetSlot, out int targetIndex, out int setIndex)
    {
        setIndex = player.currentWeaponSlotSetIndex;
        targetIndex = -1;
        targetSlot = SlotType.None;

        // WEAPON LOGIC
        if (item is Weapon weapon)
        {
            Weapon main = player.weaponSlotSetArray[setIndex - 1]?[0];
            Weapon off = player.weaponSlotSetArray[setIndex - 1]?[1];

            var weaponStats = weapon.weaponStats;

            // 1 - SHIELD LOGIC
            if (weaponStats.weaponClass == WeaponClass.Shield)
            {
                if (main != null && main.weaponStats.wieldType == WieldType.TwoHanded)
                {
                    targetSlot = SlotType.Inventory;
                    targetIndex = player.playerInventory.GetFirstEmptyIndex();
                    return;
                }

                targetSlot = SlotType.WeaponOffHand;
                return;
            }

            // 2 - TWO HANDED
            if (weaponStats.wieldType == WieldType.TwoHanded)
            {
                targetSlot = SlotType.WeaponMainHand;
                return;
            }

            // 3 - MAIN HAND EMPTY
            if (main == null)
            {
                targetSlot = SlotType.WeaponMainHand;
                return;
            }

            // 4 - TRY OFF-HAND (DUAL WIELD LOGIC)
            bool canDualWield = (weaponStats.wieldType == WieldType.OneHanded && weaponStats.weaponClass != WeaponClass.Spear &&
                main.weaponStats.wieldType == WieldType.OneHanded && main.weaponStats.weaponClass != WeaponClass.Spear) && character != Character.Nyveran;

            if (canDualWield && off == null)
            {
                targetSlot = SlotType.WeaponOffHand;
                return;
            }

            // 5 - DEFAULT - REPLACE MAIN (SWAP WILL HANDLE IT)
            targetSlot = SlotType.WeaponMainHand;
            return;
        }

        // PASSIVE ITEM LOGIC
        if (item is PassiveItem passive)
        {
            targetSlot = SlotType.Passive;
            targetIndex = (int)passive.passiveStats.passiveItemSlotName;
            return;
        }

        // FALLBACK - INVENTORY
        int emptyIndex = player.playerInventory.GetFirstEmptyIndex();

        targetSlot = SlotType.Inventory;
        targetIndex = emptyIndex; // Maybe -1 if inventory is full
    }

    public static ItemGeneric ResolveItemFromState(Player player, ItemSlotStatus status, int index, int setIndex, InventoryItemData data)
    {
        switch (status)
        {
            case ItemSlotStatus.Inventory:
                if (index >= 0 && index < player.playerInventory.inventoryArray.Length) return player.playerInventory.inventoryArray[index];
                return null;
            case ItemSlotStatus.MainHand: return player.weaponSlotSetArray[setIndex - 1]?[0];
            case ItemSlotStatus.OffHand: return player.weaponSlotSetArray[setIndex - 1]?[1];
            case ItemSlotStatus.Passive:
                if (player.equippedPassiveItems.TryGetValue(data.passiveStats.passiveItemSlotName, out PassiveItem passive))
                {
                    return passive;
                }
                return null;
            default: break;
        }

        return null;
    }

    public static ItemGeneric ResolveItemFromState(Player player, ItemSlotStatus status, int index, int setIndex, PassiveItemStats passiveStats)
    {
        switch (status)
        {
            case ItemSlotStatus.Inventory:
                if (index >= 0 && index < player.playerInventory.inventoryArray.Length) return player.playerInventory.inventoryArray[index];
                return null;
            case ItemSlotStatus.MainHand: return player.weaponSlotSetArray[setIndex - 1]?[0];
            case ItemSlotStatus.OffHand: return player.weaponSlotSetArray[setIndex - 1]?[1];
            case ItemSlotStatus.Passive:
                if (player.equippedPassiveItems.TryGetValue(passiveStats.passiveItemSlotName, out PassiveItem passive))
                {
                    return passive;
                }
                return null;
            default: break;
        }

        return null;
    }

    public static ItemSlotStatus GetSlotStatusFromSlotType(SlotType slotType)
    {
        switch (slotType)
        {
            case SlotType.Passive:
                return ItemSlotStatus.Passive;
            case SlotType.WeaponMainHand:
                return ItemSlotStatus.MainHand;
            case SlotType.WeaponOffHand:
                return ItemSlotStatus.OffHand;
            case SlotType.Inventory:
                return ItemSlotStatus.Inventory;
            default:
                return ItemSlotStatus.None;
        }
    }
    public static SlotType GetSlotTypeFromStatus(ItemSlotStatus slotStatus)
    {
        switch (slotStatus)
        {
            case ItemSlotStatus.MainHand: return SlotType.WeaponMainHand;
            case ItemSlotStatus.OffHand: return SlotType.WeaponOffHand;
            case ItemSlotStatus.Passive: return SlotType.Passive;
            case ItemSlotStatus.Inventory: return SlotType.Inventory;
            default: return SlotType.None;
        }
    }
}

public struct MoveResultSP
{
    public ItemGeneric item;

    public ItemSlotStatus fromStatus;
    public SlotType toSlot;

    public int fromIndex;
    public int toIndex;
    public int setIndex;
}

public struct MoveResultMP
{
    public InventoryItemData data;

    public ItemSlotStatus fromStatus;
    public SlotType toSlot;

    public int fromIndex;
    public int toIndex;
    public int setIndex;
}

public struct SwapResultSP
{
    public ItemGeneric oldItemGeneric;
    public ItemGeneric newItemGeneric;

    public ItemSlotStatus fromOldItem;
    public ItemSlotStatus fromNewItem;

    public int fromIndexA;
    public int fromIndexB;

    public SlotType toSlotA;
    public SlotType toSlotB;

    public int toIndexA;
    public int toIndexB;

    public int setIndex;
}

public struct SwapResultMP
{
    public InventoryItemData oldItem;
    public InventoryItemData newItem;

    public ItemSlotStatus fromOldItem;
    public ItemSlotStatus fromNewItem;

    public int fromIndexA;
    public int fromIndexB;

    public SlotType toSlotA;
    public SlotType toSlotB;

    public int toIndexA;
    public int toIndexB;

    public int setIndex;
}

public struct DropResultSP
{
    public ItemType itemType;
    public ItemGeneric itemGeneric;

    public ItemSlotStatus fromStatus;
    public int fromIndex;
    public int setIndex;

    public WeaponStats weaponStats;
    public PassiveItemStats passiveStats;

    public PassiveItemSlotName passiveSlotName;
    public bool wasEquipped;
}

public struct DropResultMP
{
    public ItemType itemType;
    public InventoryItemData data;

    public ItemSlotStatus fromStatus;
    public int fromIndex;
    public int setIndex;

    public WeaponStats weaponStats;
    public PassiveItemStats passiveStats;

    public PassiveItemSlotName passiveSlotName;
    public bool wasEquipped;
}

public struct PickUpResultSP
{
    public ItemGeneric newItemGeneric;

    public bool wasSwap;

    // TARGET
    public SlotType toSlot;
    public int toIndex;
    public int setIndex;

    // OLD ITEM
    public InventoryItemData oldItem;

    public SlotType oldItemToSlot;
    public int oldItemToIndex;

    public ItemGeneric swappedItemGeneric;
}

public struct PickUpResultMP
{
    public InventoryItemData newItem;

    public bool wasSwap;

    // TARGET
    public SlotType toSlot;
    public int toIndex;
    public int setIndex;

    // OLD ITEM
    public InventoryItemData oldItem;

    public SlotType oldItemToSlot;
    public int oldItemToIndex;

    public InventoryItemData swappedItem;
}
