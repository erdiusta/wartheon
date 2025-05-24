using UnityEngine;
using UnityEngine.EventSystems;

public class DropButton : MonoBehaviour, IDropHandler
{
    Player player;

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    public void OnDrop(PointerEventData eventData)
    {
        // If this was a click-based drop (not drag), ignore
        if (Slot.selectedSlot != null && eventData.pointerDrag == null) return;

        bool dropFailed = false;

        if (eventData.pointerDrag != null)
        {
            DraggableItem draggableItem = eventData.pointerDrag?.GetComponentInParent<DraggableItem>() ?? eventData.pointerDrag?.GetComponent<DraggableItem>();
            dropFailed = DropProcess(dropFailed, draggableItem);
        }
    }

    public void DropSelectedItem()
    {
        if (Slot.selectedSlot != null && Slot.selectedSlot.selectedSlotDraggableItem != null)
        {
            bool dropFailed = DropProcess(false, Slot.selectedSlot.selectedSlotDraggableItem);

            if (!dropFailed)
            {
                // Clean up
                Slot.selectedSlot.selectedSlotDraggableItem = null;
                Slot.selectedSlot = null;
            }
        }
    }

    private bool DropProcess(bool dropFailed, DraggableItem draggableItem)
    {
        if (draggableItem != null)
        {
            if (draggableItem.itemGeneric is Weapon)
            {
                Weapon weapon = (Weapon)draggableItem.itemGeneric;

                bool dropOffhand = draggableItem.belongingSlot.slotType == SlotType.WeaponOffHand ? true : false;

                dropFailed = player.playerControl.DropProcess(DropType.Weapon, weapon, false, dropOffhand, weapon.onInventorySlot, draggableItem.belongingSlot.inventoryIndexNumber);

                if (!dropFailed)
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                }
            }
            else if (draggableItem.itemGeneric is ActiveItem)
            {
                ActiveItem activeItem = (ActiveItem)draggableItem.itemGeneric;

                player.playerControl.DropProcess(DropType.ActiveItem);
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
            }
            else if (draggableItem.itemGeneric is PassiveItem)
            {
                PassiveItem passiveItem = (PassiveItem)draggableItem.itemGeneric;

                player.playerControl.DropProcess(DropType.PassiveItem, passiveItem, false, false, passiveItem.onInventorySlot, draggableItem.belongingSlot.inventoryIndexNumber);
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
            }
        }

        return dropFailed;
    }
}
