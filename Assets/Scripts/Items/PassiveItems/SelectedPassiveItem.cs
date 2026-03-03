using UnityEngine;

public class SelectedPassiveItem : MonoBehaviour
{
    Player player;
    SetPassiveItemEvent setPassiveItemEvent;

    private void Awake()
    {
        setPassiveItemEvent = GetComponent<SetPassiveItemEvent>();
    }

    private void OnEnable()
    {
        setPassiveItemEvent.OnEquippedPassiveItem += SetPassiveItemEvent_OnEquippedPassiveItem;
        setPassiveItemEvent.OnRemovedPassiveItem += SetPassiveItemEvent_OnRemovedPassiveItem;
    }

    private void OnDisable()
    {
        setPassiveItemEvent.OnEquippedPassiveItem -= SetPassiveItemEvent_OnEquippedPassiveItem;
        setPassiveItemEvent.OnRemovedPassiveItem -= SetPassiveItemEvent_OnRemovedPassiveItem;
    }

    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    private void SetPassiveItemEvent_OnEquippedPassiveItem(SetPassiveItemEvent _, SetPassiveItemEventArgs args)
    {
        TryEquipPassiveItem(args.passiveItem, args.isSwap);

        // Update stats values after weapon switch
        player.RecalculateSecondaryStats();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    private void SetPassiveItemEvent_OnRemovedPassiveItem(SetPassiveItemEvent _, SetPassiveItemEventArgs args)
    {
        if (player.equippedPassiveItems.TryGetValue(args.passiveItemSlotName, out PassiveItem equippedItem) && equippedItem != null)
        {
            RemovePassiveEffects(equippedItem);
            player.equippedPassiveItems[args.passiveItemSlotName] = null; // Item removed from passive slot
        }

        if (!InventoryManager.Instance.IsInventoryFull() && !args.isSwap && !args.dropButton)
        {
            args.passiveItem.itemSlotStatus = ItemSlotStatus.Inventory;
            InventoryManager.Instance.PlaceItemToInventoryIndexSlot(args.passiveItem); // Item added to inventory slot
        }

        // Rebuild once and update stats/UI once
        player.RecalculateSecondaryStats();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    private void TryEquipPassiveItem(PassiveItem newItem, bool isSwap)
    {
        PassiveItemSlotName slot = newItem.passiveItemDetails.passiveItemSlotName;

        // If already equipped and not a swap, move to inventory
        if (player.equippedPassiveItems[slot] != null && !isSwap)
        {
            // Swap: move currently equipped item to inventory
            if (!InventoryManager.Instance.IsInventoryFull())
            {
                // Place current item to inventory
                InventoryManager.Instance.PlaceItemToInventoryIndexSlot(player.equippedPassiveItems[slot]);
            }

            // Remove old item effects
            RemovePassiveEffects(player.equippedPassiveItems[slot]);
        }

        // Equip new item and apply effects
        player.equippedPassiveItems[slot] = newItem;
        ApplyPassiveEffects(newItem);
    }

    public PassiveItem GetCurrentPassiveItem(PassiveItemSlotName slot)
    {
        player.equippedPassiveItems.TryGetValue(slot, out PassiveItem item);
        return item;
    }

    private void ApplyPassiveEffects(PassiveItem item)
    {
        if (item == null || item.passiveItemDetails == null) return;

        // Optional gameplay flag (non-stat behavior)
        if (item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak) player.shadowCloakEquipped = true;
    }

    private void RemovePassiveEffects(PassiveItem item)
    {
        if (item == null) return;

        // Optional gameplay flag (non-stat behavior)
        if (item.passiveItemDetails != null && item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
        {
            player.shadowCloakEquipped = false;
        }
    }
}
