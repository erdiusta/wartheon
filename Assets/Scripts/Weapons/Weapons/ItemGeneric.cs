
public abstract class ItemGeneric
{
    public abstract ItemType ItemType { get; set; }
    public abstract Rarity Rarity { get; set; }
    public abstract ItemSlotStatus ItemSlotStatus { get; set; }
    public abstract int InventoryIndex { get; set; }
}
