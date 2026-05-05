using UnityEngine;

[System.Serializable]
public struct InventoryItemData
{
    public ItemType itemType;
    public WeaponStats weaponStats;
    public PassiveItemStats passiveStats;
    public Rarity rarity;
    public ItemSlotStatus itemSlotStatus;
    public int inventoryIndexNum;
}
