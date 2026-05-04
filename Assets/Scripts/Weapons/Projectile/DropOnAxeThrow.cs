using UnityEngine;

public class DropOnAxeThrow : MonoBehaviour
{
    [HideInInspector] public static GameObject dropItemGameObject;

    public Weapon throwingAxe;

    DropItem dropItem;
    Player player;
    Room currentRoom;

    private void Start()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom();
        player = GameManager.Instance.GetLocalPlayer();
    }

    public void DropProcess()
    {
        throwingAxe = DropItem.droppedThrowingAxe;

        // Instantiate item container
        InstantiateDropItem();
        dropItem.transform.SetParent(null);
        dropItemGameObject = dropItem.gameObject;

        // Base drop point
        Vector3 dropPoint = transform.position;

        // Room center fallback direction
        Vector3 roomCenter = currentRoom.instantiatedRoom.transform.position;
        Vector3 centerDirection = (roomCenter - dropPoint).normalized;

        // Base forward offset
        float forwardOffset = 2.5f;

        // Detect if axe was thrown toward north wall (Y direction mostly upwards)
        if (centerDirection.y > 0.7f && Mathf.Abs(centerDirection.x) < 0.5f)
        {
            forwardOffset = 6f; // Increase offset if near north wall
        }

        // Apply random offset, but bias it toward room center
        Vector3 spawnPointDeviation = centerDirection * forwardOffset;
        dropItem.transform.position = dropPoint + spawnPointDeviation;       

        if (throwingAxe != null)
        {
            InstantiateWeaponItem(throwingAxe);
            dropItem.transform.SetParent(currentRoom.instantiatedRoom.transform);
        }
    }

    /// <summary>
    /// Instantiate a drop item
    /// </summary>
    private void InstantiateDropItem()
    {
        dropItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        dropItem = dropItemGameObject.GetComponent<DropItem>();
        dropItem.droppedByPlayer = false;

        // Set collider to true
        dropItemGameObject.GetComponent<BoxCollider2D>().enabled = true;
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(Weapon weapon)
    {
        if (dropItem == null) return;

        dropItem.hasWeaponDrop = true;

        dropItem.Initialize(weapon, weapon.weaponDetails.weaponFrontSprite, transform.position);
    }
}