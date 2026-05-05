using Mirror;

public static class BookUIRefreshHelper
{
    public static void RefreshBookUIAfterItemPlacement(ItemGeneric item, int inventoryIndex)
    {
        Player player = GameManager.Instance.GetLocalPlayer();

        bool isMultiplayer = NetworkServer.active || NetworkClient.active;

        if (isMultiplayer)
        {
            //player.playerInventoryNetwork.RequestRefreshUI(item, inventoryIndex);
        }
        else
        {
            // Weapon placed into inventory
            if (item is Weapon weapon)
            {
                if (weapon.ItemSlotStatus == ItemSlotStatus.Inventory)
                {
                    int index = player.playerInventory.FindIndexOfItem(weapon);
                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, index);
                }
                else
                {
                    StaticEventHandler.CallInventoryWeaponDroppedEventForBook(inventoryIndex);
                }

                StaticEventHandler.CallWeaponSwitchedEventForBook(); // Always call switch to refresh view
            }

            // Passive item placed into inventory
            else if (item is PassiveItem passive)
            {
                if (passive.ItemSlotStatus == ItemSlotStatus.Inventory)
                {
                    int index = player.playerInventory.FindIndexOfItem(passive);
                    StaticEventHandler.CallPassiveItemAddedToInventorySlot(passive, index);
                }
                else
                {
                    StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(inventoryIndex);
                }
            }
        }
    }
}
