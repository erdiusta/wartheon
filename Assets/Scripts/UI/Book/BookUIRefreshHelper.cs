public static class BookUIRefreshHelper
{
    public static void RefreshBookUIAfterItemPlacement(ItemGeneric item, Slot sourceSlot, Slot targetSlot)
    {
        Player player = GameManager.Instance.GetPlayer();

        // Weapon placed into inventory
        if (item is Weapon weapon)
        {
            if (weapon.itemSlotStatus == ItemSlotStatus.Inventory)
            {
                int index = InventoryManager.Instance.FindIndexOfItem(weapon);
                StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, index);
            }
            else
            {
                StaticEventHandler.CallInventoryWeaponDroppedEventForBook(sourceSlot.inventoryIndexNumber);
            }

            StaticEventHandler.CallWeaponSwitchedEventForBook(); // Always call switch to refresh view
        }

        // Passive item placed into inventory
        else if (item is PassiveItem passive)
        {
            if (passive.itemSlotStatus == ItemSlotStatus.Inventory)
            {
                int index = InventoryManager.Instance.FindIndexOfItem(passive);
                StaticEventHandler.CallPassiveItemAddedToInventorySlot(passive, index);
            }
            else
            {
                StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(sourceSlot.inventoryIndexNumber);
            }
        }

        // Active item: no need to do anything — UI events for active items already fire on equip
    }
}
