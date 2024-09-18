using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

[System.Serializable]
public class SpawnableObjectsByCharacterIndex
{
    public Character characterName;
    public List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByLevelList;
}

public class Counter : MonoBehaviour
{
    [SerializeField] List<SpawnableObjectsByCharacterIndex> weaponSpawnByCharacterList;
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

    private void Start()
    {
        // Activate price infos for chest items
        firstChestItem.transform.GetChild(3).gameObject.SetActive(true);
        secondChestItem.transform.GetChild(3).gameObject.SetActive(true);
        thirdChestItem.transform.GetChild(3).gameObject.SetActive(true);
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
        Weapon weapon = new Weapon();
        weapon.weaponDetails = weaponDetails;

        chestItem.Initialize(weapon, weaponDetails.weaponFrontSprite, chestItem.transform.position);

        chestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + weaponDetails.price.ToString();
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn()
    {
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(null);

        foreach (SpawnableObjectsByCharacterIndex item in weaponSpawnByCharacterList)
        {
            if (item.characterName == GameManager.Instance.GetPlayer().playerDetails.playerCharacterIndex)
            {
                weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(item.weaponSpawnByLevelList);
            }
        }

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        return weaponDetails;
    }
}
