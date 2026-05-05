using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Transform mainHandBackgroundSlot;
    public Transform mainHandEquippedSlot;
    public Transform offHandBackgroundSlot;
    public Transform offHandEquippedSlot;
    public int originalSlotIndex = 1;
    public bool mainHandDropped;

    public ItemGeneric[] inventoryArray = new ItemGeneric[12];

    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    #region PICK UP PROCESS
    public void PickUpProcess(Character character, DropItem drop, bool isPrimaryPassive = false)
    {
        if (drop == null || drop.isPickedUp) return;

        if (isPrimaryPassive)
        {
            drop.PickUpPrimaryPassive(player);
            return;
        }

        if (drop.itemGeneric is Weapon && !drop.MeetsRequirements())
        {
            int shardGain = StaticSlotHandler.ShardGainProcess(drop.itemGeneric, player);

            GameManager.Instance.OpenPopUpLog(PopUpReason.DontMeetRequiredCharacter, shardGain);
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

            drop.pickUpAnimator.SetTrigger("pickUp");

            drop.animator.runtimeAnimatorController = null;
            drop.spriteRenderer.sprite = null;

            Destroy(drop.gameObject, 1f);

            return;
        }

        ItemGeneric item = drop.itemGeneric;

        // Decide target slot
        SlotType targetSlot;
        int targetIndex;
        int setIndex;

        StaticSlotHandler.ResolvePickUpTarget(player, character, item, out targetSlot, out targetIndex, out setIndex);

        PassiveItemStats passiveStats = item is PassiveItem p ? p.passiveStats : default;

        // Check if slot is occupied
        ItemGeneric existing = StaticSlotHandler.ResolveItemFromState(player, StaticSlotHandler.GetSlotStatusFromSlotType(targetSlot), targetIndex, setIndex, passiveStats);

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

        PickUpResultSP result = new PickUpResultSP
        {
            newItemGeneric = drop.itemGeneric,
            swappedItemGeneric = existing,

            toSlot = targetSlot,
            toIndex = targetIndex,

            oldItemToSlot = swappedToSlot,
            oldItemToIndex = swappedToIndex,

            setIndex = setIndex,
            wasSwap = existing != null
        };

        drop.isPickedUp = true;

        ExecutePickUp(result, drop);

        Destroy(drop.gameObject, 1f);
    }

    void ExecutePickUp(PickUpResultSP result, DropItem drop)
    {
        drop.pickUpAnimator.SetTrigger("pickUp");

        drop.animator.runtimeAnimatorController = null;
        drop.spriteRenderer.sprite = null;

        ItemGeneric item = drop.itemGeneric;

        // 1 - REMOVE & APPLY LOGIC
        if (!result.wasSwap)
        {
            // MOVE
            ApplyToTarget(result.toSlot, result.toIndex, result.setIndex, item);
        }
        else
        {
            // SWAP
            ItemGeneric oldItem = result.swappedItemGeneric;
            HandlePickUpSwap(player, item, oldItem, result.toSlot, result.toIndex, result.setIndex);
        }

        // 2 - WEAPON VISUAL UPDATE
        if (item is Weapon) ActivationForPickUp(player, result.setIndex);

        // 3 - UPDATE STATS
        player.RecalculateSecondaryStats();

        // 4 - UI (OWNER ONLY)
        if (player.IsLocal)
        {
            if (!result.wasSwap)
            {
                MoveResultSP moveResult = new MoveResultSP
                {
                    item = result.newItemGeneric,
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
                ItemGeneric oldItem = result.swappedItemGeneric;

                SwapResultSP swapResult = new SwapResultSP
                {
                    oldItemGeneric = result.swappedItemGeneric,
                    newItemGeneric = result.newItemGeneric,

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

            if (oldItem is Weapon weapon) SpawnWeaponDropItem(weapon, weapon.weaponStats.weaponTitle);
            else if (oldItem is PassiveItem passive) SpawnPassiveDropItem(passive);
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
    public void MoveItem(Character character, ItemGeneric item, ItemSlotStatus fromStatus, SlotType toSlot, int fromIndex, int toIndex, int setIndex, bool isInventoryFull, bool transactionOnSameSet)
    {
        // 0 - SAFETY
        if (player == null) player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogWarning("CmdMoveItem: player reference is null on server. Aborting command.");
            return;
        }

        // 1 - VALIDATION
        if (item == null)
        {
            StaticLogHandler.LogValidationFailure($"Item not found on server", player.netId, item.ItemSlotStatus, fromIndex, toIndex, toSlot);
            return;
        }

        // If slot move is illegal then it can't be validated
        if (!StaticSlotHandler.IsSlotValidated(character, player.netId, default, fromStatus, toSlot, fromIndex, toIndex, setIndex, item, out bool nyveranDualWieldFailed)) return;

        if (player.IsLocal && nyveranDualWieldFailed)
        {
            StaticLogHandler.LogValidationFailure($"Nyveran can't equip dual-wield.", player.NetAuth.netId, item.ItemSlotStatus, fromIndex, toIndex, toSlot);
            return;
        }

        // 2 - SEND RESULTS TO CLIENTS
        MoveResultSP result = new MoveResultSP
        {
            item = item,
            fromStatus = fromStatus,
            toSlot = toSlot,
            fromIndex = fromIndex,
            toIndex = toIndex,
            setIndex = setIndex,
            nyveranDualWieldFailed = nyveranDualWieldFailed
        };

        ExecuteMove(result);
    }


    void ExecuteMove(MoveResultSP result)
    {
        ApplyState(result);
    }

    void ApplyState(MoveResultSP r)
    {
        if (r.nyveranDualWieldFailed && player.IsLocal)
        {
            // Nyveran can't wield dual dagger or claw
            StaticLogHandler.LogValidationFailure($"Nyveran can't equip dual-wield.", player.NetAuth.netId, r.item.ItemSlotStatus, r.fromIndex, r.toIndex, r.toSlot);
            return;
        }

        ItemGeneric item = r.item;

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

    void ApplyOwnerUI(ItemGeneric item, MoveResultSP result)
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

    void BookUpdate(MoveResultSP r, ItemGeneric item)
    {
        // 1 - CLEAR SOURCE VISUAL
        ClearSourceBookUI_Move(r, item);

        // 2 - APPLY TARGET VISUAL
        ApplyTargetUI(r, item);

        // 3 - STATS TEXT
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    void ApplyTargetUI(MoveResultSP r, ItemGeneric item)
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
    public void SwapItem(Character character, ItemGeneric draggedItem, ItemGeneric targetItem, ItemSlotStatus draggedItemSlotStatus, ItemSlotStatus targetItemSlotStatus, int fromIndex, int toIndex, int setIndex,
        ItemSwapPos swapPos, bool isInventoryFull, bool transactionOnSameSet)
    {
        // 0 - SAFETY
        if (player == null) player = GetComponent<Player>();
        if (player == null)
        {
            Debug.LogWarning("SwapItem: player reference is null on server. Aborting command.");
            return;
        }

        // Resolve items from server state
        ItemGeneric itemA = draggedItem;
        ItemGeneric itemB = targetItem;

        // 1 - VALIDATION
        if (itemA == null || itemB == null)
        {
            StaticLogHandler.LogValidationFailure("Swap item null", player.netId, draggedItem.ItemSlotStatus, fromIndex, toIndex, SlotType.None);
            return;
        }

        // Validate A going to B
        if (!StaticSlotHandler.IsSlotValidated(character, player.netId, default, draggedItemSlotStatus, StaticSlotHandler.GetSlotTypeFromStatus(targetItemSlotStatus), fromIndex, toIndex, setIndex,
            itemB, out bool nyveranDualWield)) return;

        // Validate B going to A
        if (!StaticSlotHandler.IsSlotValidated(character, player.netId, default, targetItemSlotStatus, StaticSlotHandler.GetSlotTypeFromStatus(draggedItemSlotStatus), toIndex, fromIndex, setIndex,
             itemB, out nyveranDualWield)) return;

        // 2 - SEND RESULTS
        SwapResultSP result = new SwapResultSP
        {
            oldItemGeneric = draggedItem,
            newItemGeneric = targetItem,

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

        ExecuteSwap(result);
    }

    void ExecuteSwap(SwapResultSP result)
    {
        ApplyState(result);
    }

    void ApplyState(SwapResultSP s)
    {
        ItemGeneric itemA = s.oldItemGeneric;
        ItemGeneric itemB = s.newItemGeneric;

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

    void ApplyOwnerUI_Swap(SwapResultSP result, ItemGeneric itemA, ItemGeneric itemB)
    {
        if (itemA is Weapon) HudUpdate(result.setIndex);
        BookUpdateSwap(result, itemA, itemB);
    }

    void BookUpdateSwap(SwapResultSP s, ItemGeneric itemA, ItemGeneric itemB)
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

    void ApplyTargetUI_Swap(SwapResultSP s, ItemGeneric itemA, ItemGeneric itemB, bool isSlotA)
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
                if (item is Weapon w) StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(w, toIndex);
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
    public void DropWeapon(WeaponStats weaponStats, WeaponTitle weaponTitle, ItemSlotStatus fromStatus, int fromIndex, int setIndex)
    {
        var player = GetComponent<Player>();

        // 1 - RESOLVE
        Weapon weapon = ResolveWeapon(player, fromStatus, fromIndex, setIndex);
        if (weapon == null) return;

        // 2 - SPAWN DROP
        SpawnWeaponDropItem(weapon, weapon.weaponStats.weaponTitle);

        // 3 - SEND RESULT
        DropResultSP result = new DropResultSP
        {
            itemType = ItemType.Weapon,
            fromStatus = fromStatus,
            fromIndex = fromIndex,
            setIndex = setIndex,
            weaponStats = weaponStats,
            wasEquipped = fromStatus != ItemSlotStatus.Inventory
        };

        ExecuteDrop(result);
    }

    public void DropPassive(PassiveItemStats passiveStats, PassiveItemType passiveItemType, ItemSlotStatus fromStatus, int fromIndex, PassiveItemSlotName slotName)
    {
        var player = GetComponent<Player>();

        // 1 - RESOLVE
        PassiveItem item = ResolvePassive(player, fromStatus, fromIndex, slotName);
        if (item == null) return;

        // 2 - SPAWN DROP
        SpawnPassiveDropItem(item);

        // 3 - SEND RESULT
        DropResultSP result = new DropResultSP
        {
            itemType = ItemType.PassiveItem,
            fromStatus = fromStatus,
            fromIndex = fromIndex,
            passiveStats = passiveStats,
            passiveSlotName = slotName,
            wasEquipped = fromStatus != ItemSlotStatus.Inventory
        };

        ExecuteDrop(result);
    }

    void ExecuteDrop(DropResultSP r)
    {
        if (player == null) player = GetComponent<Player>();
        if (player == null) return;

        ItemGeneric item = r.itemGeneric;

        ApplyDropState(r, item);
    }

    void ApplyDropState(DropResultSP d, ItemGeneric item)
    {
        if (item == null) return;

        // 1 - Remove From Source
        RemoveFromSource(d.fromStatus, d.fromIndex, d.setIndex, item);

        // 2 - Activation
        Activation(item, d.fromStatus, SlotType.Drop, d.setIndex);

        // 3 - Stats
        player.RecalculateSecondaryStats();

        // 4 - Apply UI
        ApplyOwnerUI(item, d);
    }

    void ApplyOwnerUI(ItemGeneric item, DropResultSP result)
    {
        HudUpdate(result.setIndex);
        BookUpdate(result, item);
    }

    void BookUpdate(DropResultSP d, ItemGeneric item)
    {
        // 1 - CLEAR SOURCE VISUAL
        ClearSourceBookUI_Drop(d, item);

        // 2 - STATS TEXT
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    #region CLEAR SLOT
    void ClearSourceBookUI_Move(MoveResultSP r, ItemGeneric item)
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
                PassiveItemStats passiveStats = r.item is PassiveItem p ? p.passiveStats : default;
                StaticEventHandler.CallItemRemovedFromPassiveItemSlot(passiveStats.passiveItemSlotName);
                break;
            default:
                break;
        }
    }

    void ClearSourceBookUI_Swap(SwapResultSP s, ItemGeneric itemA, ItemGeneric itemB, bool isSlotA)
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
                    PassiveItemStats passiveStats = s.oldItemGeneric is PassiveItem p ? p.passiveStats : default;
                    StaticEventHandler.CallItemRemovedFromPassiveItemSlot(passiveStats.passiveItemSlotName);
                }
                break;

            default:
                break;
        }
    }

    void ClearSourceBookUI_Drop(DropResultSP d, ItemGeneric item)
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

    public void SpawnWeaponDropItem(Weapon weapon, WeaponTitle weaponTitle)
    {
        GameObject dropItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        DropItem dropItem = dropItemObject.GetComponent<DropItem>();

        dropItem.hasWeaponDrop = true;
        dropItem.dropSourceType = DropSourceType.Player;

        dropItem.isColliding = true;

        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

        dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position, null);

        // Break free from the player object
        dropItem.spriteRenderer.enabled = true;
        dropItem.animator.enabled = true;
        dropItem.animator.runtimeAnimatorController = weaponDetails.weaponHoverAnimatorController;

        dropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);
        dropItem.isPickedUp = false;
        dropItem.dropCompleted = true;

        // Make sure drop completed
        dropItem.boxCollider2D.enabled = true;
        dropItem.isColliding = false;
    }

    public void SpawnPassiveDropItem(PassiveItem passiveItem)
    {
        GameObject dropItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        DropItem dropItem = dropItemObject.GetComponent<DropItem>();

        dropItem.dropSourceType = DropSourceType.Player;
        dropItem.hasSecondaryPassiveDrop = true;
        dropItem.isColliding = true;

        PassiveItemDetailsSO passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);

        dropItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, transform.position, null);
        dropItem.spriteRenderer.enabled = true;
        dropItem.animator.enabled = true;
        dropItem.animator.runtimeAnimatorController = passiveItemDetails.passiveItemAnimatorController;
        dropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);

        dropItem.isPickedUp = false;
        dropItem.boxCollider2D.enabled = true;
        dropItem.isColliding = false;
        dropItem.dropCompleted = true;
    }
    #endregion

    #region HELPERS
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

    #endregion

    #region INVENTORY HELPERS

    /// <summary>
    /// Check if inventory is full or not
    /// </summary>
    public bool IsInventoryFull()
    {
        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i] == null) return false;
        }

        return true;
    }

    public int GetFirstEmptyIndex()
    {
        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i] == null)
            {
                return i;
            }
        }

        return -1;
    }

    public int PlaceItemToInventoryIndexSlot(ItemGeneric itemGeneric, int specificIndex = -1)
    {
        if (itemGeneric == null) return -1;

        int existingIndex = FindIndexOfItem(itemGeneric);

        if (existingIndex != -1) inventoryArray[existingIndex] = null;

        itemGeneric.ItemSlotStatus = ItemSlotStatus.Inventory;

        if (specificIndex >= 0)
        {
            if (specificIndex >= inventoryArray.Length)
            {
                Debug.LogWarning($"Invalid index {specificIndex}");
                return -1;
            }

            if (inventoryArray[specificIndex] != null)
            {
                Debug.LogWarning($"Slot {specificIndex} already occupied!");
                return -1;
            }

            inventoryArray[specificIndex] = itemGeneric;
            itemGeneric.InventoryIndex = specificIndex;
                
            return specificIndex;
        }
        else
        {
            int index = GetFirstEmptyIndex();

            if (index == -1)
            {
                Debug.LogWarning("Inventory full");
                return -1;
            }

            inventoryArray[index] = itemGeneric;
            itemGeneric.InventoryIndex = index;

            return index;
        }
    }

    public int FindIndexOfItem(ItemGeneric item)
    {
        for (int i = 0; i < inventoryArray.Length; i++)
        {
            if (inventoryArray[i] == item) return i;
        }

        return -1;
    }

    /// <summary>
    /// Empties the inventory slot at the given index (keeps signature for compatibility).
    /// Prefer using RemoveItemFromInventory when you need the removed item.
    /// </summary>
    public void EmptyItemFromInventory(int index)
    {
        // Delegate to RemoveItemFromInventory and ignore return value
        if (!IsValidIndex(index))
        {
            Debug.LogWarning($"RemoveItemFromInventory: index {index} out of range");
        }

        ItemGeneric removed = inventoryArray[index];

        inventoryArray[index] = null;
        removed.InventoryIndex = -1;
    }

    public bool IsAllMainWeaponSetsFull()
    {
        for (int i = 0; i < player.weaponSlotSetArray.Length; i++)
        {
            if (player.weaponSlotSetArray[i][0] == null) return false;
        }

        return true;
    }

    public bool IsAllOffhandWeaponSetsFull()
    {
        for (int i = 0; i < player.weaponSlotSetArray.Length; i++)
        {
            if (player.weaponSlotSetArray[i][1] == null) return false;
        }

        return true;
    }

    /// <summary>
    /// Returns the inventory item at index, or null if index is invalid.
    /// </summary>
    public ItemGeneric GetInventoryItem(int indexNumber)
    {
        if (!IsValidIndex(indexNumber))
        {
            Debug.LogWarning($"GetInventoryItem: index {indexNumber} out of range");
            return null;
        }

        return inventoryArray[indexNumber];
    }

    public void SetOriginalSlotIndex(int index)
    {
        originalSlotIndex = index;
    }

    public int GetOriginalSlotIndex() => originalSlotIndex;

    // Utility: centralize index validation
    bool IsValidIndex(int index)
    {
        return index >= 0 && index < inventoryArray.Length;
    }

    #endregion
}