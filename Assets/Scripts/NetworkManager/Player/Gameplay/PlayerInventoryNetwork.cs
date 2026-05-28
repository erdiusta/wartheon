using Mirror;
using System.Collections;
using UnityEngine;

public class PlayerInventoryNetwork : NetworkBehaviour
{
    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    #region PICK UP PROCESS
    public void RequestPickUp(Character character, DropItemNetwork drop, bool isPrimaryPassive = false)
    {
        if (!isLocalPlayer) return;

        InventoryItemData data = StaticSlotHandler.ConvertDropToData(drop);

        if (data.Equals(default))
        {
            Debug.LogError("PICK UP FAILED BECAUSE PICK UP DATA NOT RETRIEVED.");
            return;
        }

        CmdPickUpItem(character, data, drop.netIdentity, isPrimaryPassive);
    }

    [Command]
    void CmdPickUpItem(Character character, InventoryItemData data, NetworkIdentity dropNetId, bool isPrimaryPassive)
    {
        var player = GetComponent<Player>();
        var drop = dropNetId?.GetComponent<DropItemNetwork>();

        if (drop == null || drop.isPickedUp) return;

        if (isPrimaryPassive)
        {
            drop.PickUpPrimaryPassive_Server(player);
            return;
        }

        if (drop.itemGeneric is Weapon && !drop.MeetsRequirements(player))
        {
            int shardGain = 0;

            if (player.IsLocal)
            {
                shardGain = StaticSlotHandler.ShardGainProcess(drop.itemGeneric, player);

                GameManager.Instance.OpenPopUpLog(PopUpReason.DontMeetRequiredCharacter, shardGain);

                NetworkSoundManager.Instance.ServerPlaySound(SoundName.PickUpWeapon, transform.position);
            }

            RpcDontMeetRequirements(player.NetAuth.netIdentity, shardGain, drop.netIdentity);

            StartCoroutine(DestroyRoutine(1f, drop));

            return;
        }

        ItemGeneric item = ConvertToItem(data);

        // Decide target slot
        SlotType targetSlot;
        int targetIndex;
        int setIndex;

        StaticSlotHandler.ResolvePickUpTarget(player, character, item, out targetSlot, out targetIndex, out setIndex);

        // Check if slot is occupied
        ItemGeneric existing = StaticSlotHandler.ResolveItemFromState(player, StaticSlotHandler.GetSlotStatusFromSlotType(targetSlot), targetIndex, setIndex, data);

        InventoryItemData swappedData = default;
        SlotType swappedToSlot = SlotType.None;
        int swappedToIndex = -1;

        if (existing != null)
        {
            swappedData = StaticSlotHandler.ConvertToData(existing);

            if (!player.playerInventory.IsInventoryFull())
            {
                swappedToSlot = SlotType.Inventory;
                swappedToIndex = player.playerInventory.GetFirstEmptyIndex();
            }
            else
            {
                swappedToSlot = SlotType.Drop;
                swappedToIndex = -1;
            }
        }
 
        PickUpResultMP result = new PickUpResultMP
        {
            newItem = data,
            swappedItem = swappedData,

            toSlot = targetSlot,
            toIndex = targetIndex,

            oldItemToSlot = swappedToSlot,
            oldItemToIndex = swappedToIndex,

            setIndex = setIndex,
            wasSwap = existing != null
        };

        drop.isPickedUp = true;

        RpcSyncPickUp(result, drop.netIdentity);

        StartCoroutine(DestroyRoutine(1f, drop));
    }

    [ClientRpc] 
    void RpcDontMeetRequirements(NetworkIdentity playerNetId, int shardGain, NetworkIdentity dropNetIdentity)
    {
        DropItemNetwork drop = dropNetIdentity.GetComponent<DropItemNetwork>();
        drop.pickUpAnimator.SetTrigger("pickUp");

        drop.animator.runtimeAnimatorController = null;
        drop.spriteRenderer.sprite = null;

        Player player = playerNetId.GetComponent<Player>();

        if (!player.IsLocal) return;

        shardGain = StaticSlotHandler.ShardGainProcess(drop.itemGeneric, player);

        GameManager.Instance.OpenPopUpLog(PopUpReason.DontMeetRequiredCharacter, shardGain);
    }

    [ClientRpc]
    void RpcSyncPickUp(PickUpResultMP result, NetworkIdentity dropNetIdentity)
    {
        DropItemNetwork drop = dropNetIdentity.GetComponent<DropItemNetwork>();
        drop.pickUpAnimator.SetTrigger("pickUp");

        drop.animator.runtimeAnimatorController = null;
        drop.spriteRenderer.sprite = null;

        var player = GetComponent<Player>();

        ItemGeneric item = ConvertToItem(result.newItem);

        // 1 - REMOVE & APPLY LOGIC
        if (!result.wasSwap)
        {
            // MOVE
            ApplyToTarget(result.toSlot, result.toIndex, result.setIndex, item);
        }
        else
        {
            // SWAP
            ItemGeneric oldItem = ConvertToItem(result.swappedItem);
            HandlePickUpSwap(player, item, oldItem, result.toSlot, result.toIndex, result.setIndex);
        }

        // 2 - WEAPON VISUAL UPDATE
        if(item is Weapon) ActivationForPickUp(player, result.setIndex);

        // 3 - UPDATE STATS
        player.RecalculateSecondaryStats();

        // 4 - UI (OWNER ONLY)
        if (player.IsLocal)
        {
            if (!result.wasSwap)
            {
                MoveResultMP moveResult = new MoveResultMP
                {
                    data = result.newItem,
                    fromStatus = ItemSlotStatus.None,
                    toSlot = result.toSlot,
                    fromIndex = -1,
                    toIndex = result.toIndex,
                    setIndex = result.setIndex
                };

                ApplyOwnerUI(item, moveResult);
            }
            else
            {
                ItemGeneric oldItem = ConvertToItem(result.swappedItem);

                SwapResultMP swapResult = new SwapResultMP
                {
                    oldItem = result.swappedItem,
                    newItem = result.newItem,

                    fromOldItem = StaticSlotHandler.GetSlotStatusFromSlotType(result.toSlot), // Old item was in target slot
                    fromNewItem = ItemSlotStatus.None, // New item comes from ground

                    fromIndexA = result.toIndex, // Old item was at target index
                    fromIndexB = -1, // Ground - no index

                    toSlotA = result.oldItemToSlot,
                    toSlotB = result.toSlot,

                    toIndexA = result.oldItemToIndex,
                    toIndexB = result.toIndex,

                    setIndex = result.setIndex
                };

                ApplyOwnerUI_Swap(swapResult, oldItem, item);
            }
        }
    }

    void HandlePickUpSwap(Player player, ItemGeneric newItem, ItemGeneric oldItem, SlotType targetSlot, int targetIndex, int setIndex)
    {
        if (oldItem == null) return;
        if (newItem == null) return;

        bool canGoInventory = !player.playerInventory.IsInventoryFull();

        if (canGoInventory)
        {
            // Remove old item from existing slot
            RemoveFromSource(oldItem.ItemSlotStatus, -1, setIndex, oldItem);

            // Place old item into inventory at automatically the lowest index slot
            ApplyToTarget(SlotType.Inventory, -1, setIndex, oldItem);
        }
        else
        {
            // Inventory full so old item is dropped onto the ground

            if (oldItem is Weapon weapon) SpawnDropWeapon_Server(weapon.weaponStats, weapon.weaponStats.weaponTitle);
            else if (oldItem is PassiveItem passive) SpawnDropPassiveItem_Server(passive.passiveStats);
        }

        // Place New Item
        ApplyToTarget(targetSlot, targetIndex, setIndex, newItem);
    }

    void ActivationForPickUp(Player player, int setIndex)
    {
        Weapon main = player.weaponSlotSetArray[setIndex - 1]?[0];
        Weapon off = player.weaponSlotSetArray[setIndex - 1]?[1];

        // Main hand
        player.ApplyWeaponActivationEvents(main, ItemSlotStatus.MainHand, setIndex, false, player.IsLocal, false, false, false);

        // Off-hand
        player.ApplyWeaponActivationEvents(off, ItemSlotStatus.OffHand, setIndex, false, player.IsLocal, false, false, false);
    }

    #endregion

    #region SLOT MOVE PROCESS
    public void RequestMoveItem(Character character, ItemGeneric item, ItemSlotStatus fromStatus, SlotType toSlot, int fromIndex, int toIndex, int setIndex, ItemSwapPos swapPos, bool isInventoryFull, bool transactionOnSameSet)
    {
        if (!isLocalPlayer) return;

        InventoryItemData data = StaticSlotHandler.ConvertToData(item);

        CmdMoveItem(character, data, fromStatus, toSlot, fromIndex, toIndex, setIndex, swapPos, isInventoryFull, transactionOnSameSet);
    }

    [Command]
    void CmdMoveItem(Character character, InventoryItemData data, ItemSlotStatus fromStatus, SlotType toSlot, int fromIndex, int toIndex, int setIndex, ItemSwapPos swapPos, bool isInventoryFull, bool transactionOnSameSet)
    {
        // 0 - SAFETY
        if (player == null) player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogWarning("CmdMoveItem: player reference is null on server. Aborting command.");
            return;
        }

        // Resolve item from Server State (must exist before validation continues)
        ItemGeneric item = StaticSlotHandler.ResolveItemFromState(player, fromStatus, fromIndex, setIndex, data);

        // 1 - VALIDATION
        if (item == null)
        {
            StaticLogHandler.LogValidationFailure($"Item not found on server", player.netId, data.itemSlotStatus, fromIndex, toIndex, toSlot);
            return;
        }

        // If slot move is illegal then it can't be validated
        if (!StaticSlotHandler.IsSlotValidated(character, player.netId, data, fromStatus, toSlot, fromIndex, toIndex, setIndex, item, out bool nyveranDualWieldFailed)) return;

        if(player.IsLocal && nyveranDualWieldFailed)
        {
            StaticLogHandler.LogValidationFailure($"Nyveran can't equip dual-wield.", player.NetAuth.netId, data.itemSlotStatus, fromIndex, toIndex, toSlot);
            return;
        }

        // 2 - SEND RESULTS TO CLIENTS
        MoveResultMP result = new MoveResultMP
        {
            data = data,
            fromStatus = fromStatus,
            toSlot = toSlot,
            fromIndex = fromIndex,
            toIndex = toIndex,
            setIndex = setIndex,
            nyveranDualWieldFailed = nyveranDualWieldFailed
        };

        RpcSyncMove(result);
    }

    [ClientRpc]
    void RpcSyncMove(MoveResultMP result)
    {
        if (player == null) player = GetComponent<Player>();
        if (player == null) return;

        ApplyState(result);
    }

    void ApplyState(MoveResultMP r)
    {
        if (r.nyveranDualWieldFailed && player.IsLocal)
        {
            // Nyveran can't wield dual dagger or claw
            StaticLogHandler.LogValidationFailure($"Nyveran can't equip dual-wield.", player.NetAuth.netId, r.data.itemSlotStatus, r.fromIndex, r.toIndex, r.toSlot);
            return;
        }

        ItemGeneric item = ConvertToItem(r.data);

        if (item == null) return;

        // 1 - REMOVE
        RemoveFromSource(r.fromStatus, r.fromIndex, r.setIndex, item);

        // 2 - APPLY
        ApplyToTarget(r.toSlot, r.toIndex, r.setIndex, item);

        // 3 - ACTIVATION
        Activation(item, r.fromStatus, r.toSlot, r.setIndex);

        // 4 - STATS
        player.RecalculateSecondaryStats();

        // 5 - UI (OWNER ONLY)
        if (player.IsLocal)
        {
            ApplyOwnerUI(item, r);
        }
    }

    void ApplyOwnerUI(ItemGeneric item, MoveResultMP result)
    {
        HudUpdate(result.setIndex);
        BookUpdate(result, item);
    }

    void HudUpdate(int setIndex)
    {
        if (player == null) return;
        if (player.weaponSlotSetArray == null || setIndex <= 0 || setIndex - 1 >= player.weaponSlotSetArray.Length) return;

        Weapon main = player.weaponSlotSetArray[setIndex - 1]?[0];
        Weapon off = player.weaponSlotSetArray[setIndex - 1]?[1];

        // Main Hand Hud
        if (main != null)
        {
            player.setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEventForHud(main.weaponStats, main.Rarity, setIndex, onStart: false, onStartWeaponIndex: 0);
        }
        else
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEventForHud();
        }

        // Off-hand Hud
        if (off != null)
        {
            player.setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEventForHud(off.weaponStats, off.Rarity, setIndex, onStart: false);
        }
        else
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEventForHud();
        }

        // Lock Icon (One/Two Hand Logic)
        player.UpdateWeaponHudLockStateMP(main, off);
    }

    void BookUpdate(MoveResultMP r, ItemGeneric item)
    {
        // 1 - CLEAR SOURCE VISUAL
        ClearSourceBookUI_Move(r, item);

        // 2 - APPLY TARGET VISUAL
        ApplyTargetUI(r, item);

        // 3 - STATS TEXT
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    void ApplyTargetUI(MoveResultMP r, ItemGeneric item)
    {
        switch (r.toSlot)
        {
            case SlotType.Inventory:
                if (item is Weapon w) StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(w, r.toIndex);
                else if (item is PassiveItem p) StaticEventHandler.CallPassiveItemAddedToInventorySlot(p, r.toIndex);
                else StaticEventHandler.CallGenericItemPlacedToEmptyInInventory(item, r.fromIndex, r.toIndex);
                break;

            case SlotType.WeaponMainHand:
            case SlotType.WeaponOffHand:
                StaticEventHandler.CallWeaponSwitchedEventForBook();
                break;

            case SlotType.Passive:
                if (item is PassiveItem passive)
                {
                    StaticEventHandler.CallItemAddedToPassiveItemSlot(passive, passive.passiveStats.passiveItemSlotName);
                }
                break;
            default:
                break;
        }
    }
    #endregion

    #region SWAP PROCESS
    public void RequestSwapItem(Character character, ItemGeneric draggedItem, ItemGeneric targetItem, ItemSlotStatus draggedItemSlotStatus, ItemSlotStatus targetItemSlotStatus, int fromIndex, int toIndex, int setIndex,
        ItemSwapPos swapPos, bool isInventoryFull, bool transactionOnSameSet)
    {
        if (!isLocalPlayer) return;

        InventoryItemData draggedItemData = StaticSlotHandler.ConvertToData(draggedItem);
        InventoryItemData targetItemData = StaticSlotHandler.ConvertToData(targetItem);

        CmdSwapItem(character, draggedItemData, targetItemData, draggedItemSlotStatus, targetItemSlotStatus, fromIndex, toIndex, setIndex, swapPos, isInventoryFull, transactionOnSameSet);
    }

    [Command]
    void CmdSwapItem(Character character, InventoryItemData draggedItemData, InventoryItemData targetItemData, ItemSlotStatus draggedItemSlotStatus, ItemSlotStatus targetItemSlotStatus, int fromIndex, int toIndex, 
        int setIndex, ItemSwapPos swapPos, bool isInventoryFull, bool transactionOnSameSet)
    {
        // 0 - SAFETY
        if (player == null) player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogWarning("CmdSwapItem: player reference is null on server. Aborting command.");
            return;
        }

        // Resolve items from server state
        ItemGeneric itemA = StaticSlotHandler.ResolveItemFromState(player, draggedItemSlotStatus, fromIndex, setIndex, draggedItemData);
        ItemGeneric itemB = StaticSlotHandler.ResolveItemFromState(player, targetItemSlotStatus, toIndex, setIndex, targetItemData);

        // 1 - VALIDATION
        if (itemA == null || itemB == null)
        {
            StaticLogHandler.LogValidationFailure("Swap item null", player.netId, draggedItemData.itemSlotStatus, fromIndex, toIndex, SlotType.None);
            return;
        }

        // Validate A going to B
        if (!StaticSlotHandler.IsSlotValidated(character, player.netId, draggedItemData, draggedItemSlotStatus, StaticSlotHandler.GetSlotTypeFromStatus(targetItemSlotStatus), fromIndex, toIndex, setIndex, 
            itemB, out bool nyveranDualWield)) return;

        // Validate B going to A
        if (!StaticSlotHandler.IsSlotValidated(character, player.netId, targetItemData, targetItemSlotStatus, StaticSlotHandler.GetSlotTypeFromStatus(draggedItemSlotStatus), toIndex, fromIndex, setIndex, 
            itemB, out nyveranDualWield)) return;

        //// 2 - REMOVE FROM SOURCE
        //RemoveFromSource(draggedItemSlotStatus, fromIndex, setIndex, itemA);
        //RemoveFromSource(targetItemSlotStatus, toIndex, setIndex, itemB);

        //// 3 - APPLY TO TARGET
        //ApplyToTarget(GetSlotTypeFromStatus(targetItemSlotStatus), toIndex, setIndex, itemA);
        //ApplyToTarget(GetSlotTypeFromStatus(draggedItemSlotStatus), fromIndex, setIndex, itemB);

        //// 4 - RECALCULATE STATS
        //player.RecalculateSecondaryStats();

        // 5 - SEND RESULTS TO CLIENTS
        SwapResultMP result = new SwapResultMP
        {
            oldItem = draggedItemData,
            newItem = targetItemData,

            fromOldItem = draggedItemSlotStatus,
            fromNewItem = targetItemSlotStatus,

            fromIndexA = fromIndex,
            fromIndexB = toIndex,

            toSlotA = StaticSlotHandler.GetSlotTypeFromStatus(targetItemSlotStatus),
            toSlotB = StaticSlotHandler.GetSlotTypeFromStatus(draggedItemSlotStatus),

            toIndexA = toIndex,
            toIndexB = fromIndex,

            setIndex = setIndex
        };

        RpcSyncSwap(result);
    }

    [ClientRpc]
    void RpcSyncSwap(SwapResultMP result)
    {
        if (player == null) player = GetComponent<Player>();
        if (player == null) return;

        ApplyState(result);
    }

    void ApplyState(SwapResultMP s)
    {
        ItemGeneric itemA = ConvertToItem(s.oldItem);
        ItemGeneric itemB = ConvertToItem(s.newItem);

        if (itemA == null || itemB == null)
        {
            Debug.LogWarning("Apply State(Swap): item reconstruction failed.");
        }

        // 1 - REMOVE
        RemoveFromSource(s.fromOldItem, s.fromIndexA, s.setIndex, itemA);
        RemoveFromSource(s.fromNewItem, s.fromIndexB, s.setIndex, itemB);

        // 2 - APPLY
        ApplyToTarget(s.toSlotA, s.toIndexA, s.setIndex, itemA);
        ApplyToTarget(s.toSlotB, s.toIndexB, s.setIndex, itemB);

        // 3 - ACTIVATION
        Activation(itemA, s.fromOldItem, s.toSlotA, s.setIndex);
        Activation(itemB, s.fromNewItem, s.toSlotB, s.setIndex);

        // 4 - STATS
        player.RecalculateSecondaryStats();

        // 5 - UI (OWNER ONLY)
        if (player.IsLocal)
        {
            ApplyOwnerUI_Swap(s, itemA, itemB);
        }
    }

    void ApplyOwnerUI_Swap(SwapResultMP result, ItemGeneric itemA, ItemGeneric itemB)
    {
        if(itemA is Weapon) HudUpdate(result.setIndex);
        BookUpdateSwap(result, itemA, itemB);
    }

    void BookUpdateSwap(SwapResultMP s, ItemGeneric itemA, ItemGeneric itemB)
    {
        // 1 - CLEAR SOURCE VISUAL
        ClearSourceBookUI_Swap(s, itemA, itemB, isSlotA: true);
        ClearSourceBookUI_Swap(s, itemA, itemB, isSlotA: false);

        // 2 - APPLY TARGET VISUAL
        ApplyTargetUI_Swap(s, itemA, itemB, isSlotA: true);
        ApplyTargetUI_Swap(s, itemA, itemB, isSlotA: false);

        // 3 - STATS TEXT
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    void ApplyTargetUI_Swap(SwapResultMP s, ItemGeneric itemA, ItemGeneric itemB, bool isSlotA)
    {
        ItemGeneric item;
        SlotType toSlot;
        int toIndex;
        int fromIndex;

        if (isSlotA)
        {
            // A Goes To B Slot
            item = itemA;
            toSlot = s.toSlotA;
            toIndex = s.toIndexA;
            fromIndex = s.fromIndexA;
        }
        else
        {
            // B Goes To A Slot
            item = itemB;
            toSlot = s.toSlotB;
            toIndex = s.toIndexB;
            fromIndex = s.fromIndexB;
        }

        switch (toSlot)
        {
            case SlotType.Inventory:
                if(item is Weapon w) StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(w, toIndex);
                else if (item is PassiveItem p) StaticEventHandler.CallPassiveItemAddedToInventorySlot(p, toIndex);
                break;
            case SlotType.WeaponMainHand:
            case SlotType.WeaponOffHand:
                // Swap still results in "weapon changed"
                if (isSlotA)
                {
                    StaticEventHandler.CallWeaponSwitchedEventForBook(); // Updating once is enough for this event handler
                }
                break;
            case SlotType.Passive:
                if (item is PassiveItem passive)
                {
                    StaticEventHandler.CallItemAddedToPassiveItemSlot(passive, passive.passiveStats.passiveItemSlotName);
                }
                break;

            default:
                break;
        }
    }
    #endregion

    #region DROP PROCESS
    [Command]
    public void CmdDropWeapon(WeaponStats weaponStats, WeaponTitle weaponTitle, ItemSlotStatus fromStatus, int fromIndex, int setIndex)
    {
        var player = GetComponent<Player>();

        // 1 - RESOLVE
        Weapon weapon = ResolveWeapon(player, fromStatus, fromIndex, setIndex);
        if (weapon == null) return;

        // 2 - SPAWN DROP
        SpawnDropWeapon_Server(weapon.weaponStats, weapon.weaponStats.weaponTitle);

        // 3 - SEND RESULT
        DropResultMP result = new DropResultMP
        {
            itemType = ItemType.Weapon,
            fromStatus = fromStatus,
            fromIndex = fromIndex,
            setIndex = setIndex,
            weaponStats = weaponStats,
            wasEquipped = fromStatus != ItemSlotStatus.Inventory
        };

        RpcSyncDropMP(result);
    }

    [Command]
    public void CmdDropPassive(PassiveItemStats passiveStats, PassiveItemType passiveItemType, ItemSlotStatus fromStatus, int fromIndex, PassiveItemSlotName slotName)
    {
        var player = GetComponent<Player>();

        // 1 - RESOLVE
        PassiveItem item = ResolvePassive(player, fromStatus, fromIndex, slotName);
        if (item == null) return;

        // 2 - SPAWN DROP
        SpawnDropPassiveItem_Server(item.passiveStats);

        // 3 - SEND RESULT
        DropResultMP result = new DropResultMP
        {
            itemType = ItemType.PassiveItem,
            fromStatus = fromStatus,
            fromIndex = fromIndex,
            passiveStats = passiveStats,
            passiveSlotName = slotName,
            wasEquipped = fromStatus != ItemSlotStatus.Inventory
        };

        RpcSyncDropMP(result);
    }

    [ClientRpc]
    void RpcSyncDropMP(DropResultMP d)
    {
        if (player == null) player = GetComponent<Player>();
        if (player == null) return;

        ItemGeneric item = StaticSlotHandler.ResolveItemFromState(player, d.fromStatus, d.fromIndex, d.setIndex, d.data);

        ApplyDropState(d, item);
    }

    void ApplyDropState(DropResultMP d, ItemGeneric item)
    {
        if (item == null) return;

        // 1 - Remove From Source
        RemoveFromSource(d.fromStatus, d.fromIndex, d.setIndex, item);

        // 2 - Activation
        Activation(item, d.fromStatus, SlotType.Drop, d.setIndex);

        // 3 - Stats
        player.RecalculateSecondaryStats();

        // 4 - Apply UI
        if (player.IsLocal)
        {
            ApplyOwnerUI(item, d);
        }
    }

    void ApplyOwnerUI(ItemGeneric item, DropResultMP result)
    {
        HudUpdate(result.setIndex);
        BookUpdate(result, item);
    }

    void BookUpdate(DropResultMP d, ItemGeneric item)
    {
        // 1 - CLEAR SOURCE VISUAL
        ClearSourceBookUI_Drop(d, item);

        // 2 - STATS TEXT
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    #region CLEAR SLOT
    void ClearSourceBookUI_Move(MoveResultMP r, ItemGeneric item)
    {
        switch (r.fromStatus)
        {
            case ItemSlotStatus.Inventory:
                StaticEventHandler.CallInventoryItemRemovedForBook(r.fromIndex);
                break;
            case ItemSlotStatus.MainHand:
                StaticEventHandler.CallWeaponRemovedFromEquippedSlot(SlotType.WeaponMainHand);
                StaticEventHandler.CallWeaponSwitchedEventForBook();
                break;
            case ItemSlotStatus.OffHand:
                StaticEventHandler.CallWeaponRemovedFromEquippedSlot(SlotType.WeaponOffHand);
                StaticEventHandler.CallWeaponSwitchedEventForBook();
                break;
            case ItemSlotStatus.Passive:
                StaticEventHandler.CallItemRemovedFromPassiveItemSlot(r.data.passiveStats.passiveItemSlotName);
                break;
            default:
                break;
        }
    }

    void ClearSourceBookUI_Swap(SwapResultMP s, ItemGeneric itemA, ItemGeneric itemB, bool isSlotA)
    {
        ItemSlotStatus fromStatus;
        int fromIndex;
        ItemGeneric item;

        if (isSlotA)
        {
            fromStatus = s.fromOldItem;
            fromIndex = s.fromIndexA;
            item = itemA;
        }
        else
        {
            fromStatus = s.fromNewItem;
            fromIndex = s.fromIndexB;
            item = itemB;
        }

        switch (fromStatus)
        {
            case ItemSlotStatus.Inventory:
                StaticEventHandler.CallInventoryItemRemovedForBook(fromIndex);
                break;
            case ItemSlotStatus.MainHand:
                if (item is Weapon wMain)
                {
                    StaticEventHandler.CallWeaponRemovedFromEquippedSlot(SlotType.WeaponMainHand);
                    StaticEventHandler.CallWeaponSwitchedEventForBook();
                }
                break;
            case ItemSlotStatus.OffHand:
                if (item is Weapon wOff)
                {
                    StaticEventHandler.CallWeaponRemovedFromEquippedSlot(SlotType.WeaponOffHand);
                    StaticEventHandler.CallWeaponSwitchedEventForBook();
                }
                break;
            case ItemSlotStatus.Passive:
                if (item is PassiveItem passive)
                {
                    StaticEventHandler.CallItemRemovedFromPassiveItemSlot(s.oldItem.passiveStats.passiveItemSlotName);
                }
                break;

            default:
                break;
        }
    }

    void ClearSourceBookUI_Drop(DropResultMP d, ItemGeneric item)
    {
        switch (d.fromStatus)
        {
            case ItemSlotStatus.Inventory:
                StaticEventHandler.CallInventoryItemRemovedForBook(d.fromIndex);
                break;
            case ItemSlotStatus.MainHand:
                StaticEventHandler.CallWeaponRemovedFromEquippedSlot(SlotType.WeaponMainHand);
                StaticEventHandler.CallWeaponSwitchedEventForBook();
                break;
            case ItemSlotStatus.OffHand:
                StaticEventHandler.CallWeaponRemovedFromEquippedSlot(SlotType.WeaponOffHand);
                StaticEventHandler.CallWeaponSwitchedEventForBook();
                break;
            case ItemSlotStatus.Passive:
                StaticEventHandler.CallItemRemovedFromPassiveItemSlot(d.passiveSlotName);
                break;
            default:
                break;
        }
    }
    #endregion

    [Server]
    public void SpawnDropWeapon_Server(WeaponStats weaponStats, WeaponTitle weaponTitle)
    {
        GameObject obj = Instantiate(GameResources.Instance.chestItemNetworkPrefab, transform.position, Quaternion.identity);

        DropItemNetwork drop = obj.GetComponent<DropItemNetwork>();
        obj.GetComponent<BoxCollider2D>().enabled = true;

        drop.hasWeaponDrop = true;
        drop.dropCompleted = true;
        drop.dropSourceType = DropSourceType.Player;

        NetworkServer.Spawn(obj);

        drop.weaponTitle = weaponTitle;
        drop.weaponClass = weaponStats.weaponClass;
        drop.weaponStats = weaponStats;
    }

    [Server]
    public void SpawnDropPassiveItem_Server(PassiveItemStats passiveItemStats)
    {
        GameObject obj = Instantiate(GameResources.Instance.chestItemNetworkPrefab, transform.position, Quaternion.identity);
        DropItemNetwork drop = obj.GetComponent<DropItemNetwork>();

        drop.hasSecondaryPassiveDrop = true;
        drop.dropCompleted = true;
        drop.dropSourceType = DropSourceType.Player;

        NetworkServer.Spawn(obj);

        drop.passiveItemType = passiveItemStats.passiveItemType;
        drop.passiveItemSlotName = passiveItemStats.passiveItemSlotName;
        drop.passiveStats = passiveItemStats; // Here initialization starts in DropItemNetwork
    }
    #endregion

    #region HELPERS
    ItemGeneric ConvertToItem(InventoryItemData data)
    {
        ItemGeneric item;

        if (data.itemType == ItemType.Weapon)
        {
            Weapon weapon = WeaponDropGenerator.GetWeaponWithStats(data.weaponStats, data.rarity, data.itemSlotStatus, data.inventoryIndexNum);
            item = weapon;
        }
        else
        {
            PassiveItem passiveItem = PassiveDropGenerator.GetPassiveWithStats(data.passiveStats, data.rarity, data.itemSlotStatus, data.inventoryIndexNum);
            item = passiveItem;
        }

        return item;
    }
    private bool IsWeaponRelevantChange(ItemGeneric item, ItemSlotStatus fromStatus, SlotType toSlot)
    {
        // If the moved item itself is a weapon - always relevant
        if (item is Weapon) return true;

        // If something moved out of a weapon slot - relevant
        if (fromStatus == ItemSlotStatus.MainHand || fromStatus == ItemSlotStatus.OffHand) return true;

        // If something moved into a weapon slot - relevant
        if (toSlot == SlotType.WeaponMainHand || toSlot == SlotType.WeaponOffHand) return true;

        return false;
    }
    private void RemoveFromSource(ItemSlotStatus fromStatus, int fromIndex, int setIndex, ItemGeneric item)
    {
        if (item == null) return;

        switch (fromStatus)
        {
            case ItemSlotStatus.Inventory:
                item.InventoryIndex = -1;
                player.playerInventory.inventoryArray[fromIndex] = null;
                break;

            case ItemSlotStatus.MainHand:
                player.weaponSlotSetArray[setIndex - 1][0] = null;
                break;

            case ItemSlotStatus.OffHand:
                player.weaponSlotSetArray[setIndex - 1][1] = null;
                break;

            case ItemSlotStatus.Passive:
                if (item is PassiveItem passive)
                {
                    var slot = passive.passiveStats.passiveItemSlotName;
                    player.equippedPassiveItems[slot] = null;
                }
                break;
            default:
                break;
        }
    }
    private void ApplyToTarget(SlotType toSlot, int toIndex, int setIndex, ItemGeneric item)
    {
        if (item == null) return;

        switch (toSlot)
        {
            case SlotType.Inventory:
                item.InventoryIndex = -1;
                player.playerInventory.PlaceItemToInventoryIndexSlot(item, toIndex);
                break;

            case SlotType.WeaponMainHand:
                Weapon mainHandWeapon = item as Weapon;

                player.weaponSlotSetArray[setIndex - 1][0] = mainHandWeapon;

                item.ItemSlotStatus = ItemSlotStatus.MainHand;
                item.InventoryIndex = -1;

                if (mainHandWeapon != null) mainHandWeapon.weaponStats.weaponBelongingToWhichMainHandSet = setIndex;
                break;

            case SlotType.WeaponOffHand:
                Weapon offhandWeapon = item as Weapon;

                player.weaponSlotSetArray[setIndex - 1][1] = offhandWeapon;

                item.ItemSlotStatus = ItemSlotStatus.OffHand;
                item.InventoryIndex = -1;

                if (offhandWeapon != null) offhandWeapon.weaponStats.weaponBelongingToWhichOffHandSet = setIndex;
                break;

            case SlotType.Passive:
                if (item is PassiveItem passive)
                {
                    var slot = passive.passiveStats.passiveItemSlotName;

                    player.equippedPassiveItems[slot] = passive;

                    passive.ItemSlotStatus = ItemSlotStatus.Passive;
                    passive.InventoryIndex = -1;
                }
                break;

            case SlotType.Drop:
                break;
            case SlotType.Upgrade:
                break;
            case SlotType.Dismantle:
                break;
            default:
                break;
        }
    }
    private void Activation(ItemGeneric item, ItemSlotStatus fromStatus, SlotType toSlot, int setIndex)
    {
        if (!IsWeaponRelevantChange(item, fromStatus, toSlot) || fromStatus == ItemSlotStatus.Inventory) return;

        Weapon main = player.weaponSlotSetArray[setIndex - 1]?[0];
        Weapon off = player.weaponSlotSetArray[setIndex - 1]?[1];

        player.ApplyWeaponActivationEvents(main, ItemSlotStatus.MainHand, setIndex, onStart: false, isOwnerContext: false, allowHudEvents: false, allowLockIconUpdate: false, isStatUpdateAllowed: false);
        player.ApplyWeaponActivationEvents(off, ItemSlotStatus.OffHand, setIndex, onStart: false, isOwnerContext: false, allowHudEvents: false, allowLockIconUpdate: false, isStatUpdateAllowed: false);
    }
    Weapon ResolveWeapon(Player player, ItemSlotStatus fromStatus, int fromIndex, int setIndex)
    {
        if (player == null) return null;

        switch (fromStatus)
        {
            case ItemSlotStatus.Inventory:
                if (fromIndex < 0 || fromIndex >= player.playerInventory.inventoryArray.Length) return null;

                return player.playerInventory.inventoryArray[fromIndex] as Weapon;

            case ItemSlotStatus.MainHand:
                if (setIndex <= 0 || setIndex > player.weaponSlotSetArray.Length) return null;

                return player.weaponSlotSetArray[setIndex - 1][0];
            case ItemSlotStatus.OffHand:
                if (setIndex <= 0 || setIndex > player.weaponSlotSetArray.Length) return null;

                return player.weaponSlotSetArray[setIndex - 1][1];

            default: return null;
               
        }
    }
    PassiveItem ResolvePassive(Player player, ItemSlotStatus fromStatus, int fromIndex, PassiveItemSlotName slotName)
    {
        if (player == null) return null;

        switch (fromStatus)
        {
            case ItemSlotStatus.Inventory:
                if (fromIndex < 0 || fromIndex >= player.playerInventory.inventoryArray.Length) return null;

                return player.playerInventory.inventoryArray[fromIndex] as PassiveItem;
            case ItemSlotStatus.Passive:
                return player.equippedPassiveItems[slotName];

            default:
                return null;
        }
    }
    IEnumerator DestroyRoutine(float duration, DropItemNetwork drop)
    {
        drop.animator.runtimeAnimatorController = null;
        drop.spriteRenderer.sprite = null;

        yield return new WaitForSeconds(duration);

        NetworkServer.Destroy(drop.gameObject);
    }
    #endregion
}