using Mirror;
using UnityEngine;

public class SelectedPassiveItem : MonoBehaviour
{
    SetPassiveItemEvent setPassiveItemEvent;
    Player player;

    private void Awake()
    {
        setPassiveItemEvent = GetComponent<SetPassiveItemEvent>();
        player = GetComponent<Player>();
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

    private void SetPassiveItemEvent_OnEquippedPassiveItem(SetPassiveItemEvent _, SetPassiveItemEventArgs args)
    {
        TryEquipPassiveItem(args.player, args.passiveItem, args.passiveItemSlotName, args.isSwap);

        // Update stats values after weapon switch
        args.player.RecalculateSecondaryStats();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    private void SetPassiveItemEvent_OnRemovedPassiveItem(SetPassiveItemEvent _, SetPassiveItemEventArgs args)
    {
        if (args.player.equippedPassiveItems.TryGetValue(args.passiveItemSlotName, out PassiveItem equippedItem) && equippedItem != null)
        {
            args.player.equippedPassiveItems[args.passiveItemSlotName] = null; // Item removed from passive slot
        }

        if (!NetworkServer.active && !NetworkClient.active)
        {
            if (!args.player.playerInventory.IsInventoryFull() && !args.isSwap && !args.dropButton)
            {
                args.passiveItem.ItemSlotStatus = ItemSlotStatus.Inventory;
                player.playerInventory.PlaceItemToInventoryIndexSlot(args.passiveItem); // Item added to inventory slot
            }
        }
        else
        {
            if (!args.player.playerInventory.IsInventoryFull() && !args.isSwap && !args.dropButton)
            {
                args.passiveItem.ItemSlotStatus = ItemSlotStatus.Inventory;
                player.playerInventory.PlaceItemToInventoryIndexSlot(args.passiveItem); // Item added to inventory slot
            }
        }

        // Rebuild once and update stats/UI once
        player.RecalculateSecondaryStats();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    private void TryEquipPassiveItem(Player player, PassiveItem newItem, PassiveItemSlotName passiveItemSlotName, bool isSwap)
    {
        PassiveItemSlotName slot = passiveItemSlotName;

        if (isSwap) return;

        if (!player.playerInventory.IsInventoryFull())
        {
            // Place current item to inventory
            player.playerInventory.PlaceItemToInventoryIndexSlot(player.equippedPassiveItems[slot]);
        }

        // Equip new item and apply effects
        player.equippedPassiveItems[slot] = newItem;
    }
}
