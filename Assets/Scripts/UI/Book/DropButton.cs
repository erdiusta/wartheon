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
            DraggableItem draggableItem = eventData.pointerDrag?.GetComponentInParent<DraggableItem>() ?? eventData.pointerDrag?.GetComponent<DraggableItem>();

            if (draggableItem != null)
            {
                if (draggableItem.receivable is Weapon)
                {
                    Weapon weapon = (Weapon)draggableItem.receivable;

                    player.playerControl.DropProcess(DropType.Weapon, weapon);
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                }
                else if (draggableItem.receivable is ActiveItem)
                {
                    ActiveItem activeItem = (ActiveItem)draggableItem.receivable;

                    player.playerControl.DropProcess(DropType.ActiveItem);
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                }
                else if (draggableItem.receivable is PassiveItem)
                {
                    PassiveItem passiveItem = (PassiveItem)draggableItem.receivable;

                    player.playerControl.DropProcess(DropType.PassiveItem, passiveItem);
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                }
            }
        }
    }
}
