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
                Weapon weapon = draggableItem.weapon;

                // Handle the logic for dropping the item
                player.playerControl.DropProcess(player.CreateChestItemForWeapon(weapon), true, weapon);
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
            }
        }
    }
}
