using System.Collections.Generic;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [SerializeField] List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByLevelList;
    [SerializeField] ChestItem firstChestItem;
    [SerializeField] ChestItem secondChestItem;
    [SerializeField] ChestItem thirdChestItem;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    /// <summary>
    /// Handle the room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // If the room is shop room then start spawning chest items
        if (roomChangedEventArgs.room.roomNodeType.isShopRoom)
        {
            InstantiateWeaponItem(GetWeaponDetailsToSpawn(), firstChestItem);
            InstantiateWeaponItem(GetWeaponDetailsToSpawn(), secondChestItem);
            InstantiateWeaponItem(GetWeaponDetailsToSpawn(), thirdChestItem);
        }
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails, ChestItem chestItem)
    {
        if (chestItem == null) return;

        chestItem.hasWeaponDrop = true;
        chestItem.Initialize(weaponDetails, null, null, weaponDetails.weaponFrontSprite, weaponDetails.weaponName, chestItem.transform.position);
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn()
    {
        // Create an instance of the class used to select a random item from a list based on the
        // relative 'ratios' of the items specified
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByLevelList);

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        return weaponDetails;
    }
}
