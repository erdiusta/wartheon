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
        if (eventData.pointerDrag != null)
        {
            DraggableItem draggableItem = eventData.pointerDrag.GetComponent<DraggableItem>();

            if (draggableItem != null)
            {
                if (draggableItem.receivable is Weapon)
                {
                    Weapon weapon = (Weapon)draggableItem.receivable;

                    if (ChestItem.toBeDroppedChestItem != null)
                    {
                        player.playerControl.DropProcess(ChestItem.toBeDroppedChestItem, DropType.Weapon, weapon);
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                    }
                }
                else if (draggableItem.receivable is ActiveItem)
                {
                    ActiveItem activeItem = (ActiveItem)draggableItem.receivable;

                    if (ChestItem.toBeDroppedChestItem != null)
                    {
                        player.playerControl.DropProcess(ChestItem.toBeDroppedChestItem, DropType.ActiveItem);
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                    }
                }
                else if (draggableItem.receivable is PassiveItem)
                {
                    PassiveItem passiveItem = (PassiveItem)draggableItem.receivable;

                    if (ChestItem.toBeDroppedChestItem != null)
                    {
                        player.playerControl.DropProcess(ChestItem.toBeDroppedChestItem, DropType.PassiveItem, passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName);
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                    }
                }
            }
        }
    }
}
