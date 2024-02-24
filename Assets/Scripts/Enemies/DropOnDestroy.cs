using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropOnDestroy : MonoBehaviour
{
    public GameObject chestItemPrefab;

    List<SpawnableObjectsByLevel<WeaponDetailsSO>> enemyWeaponDropList;
    List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> enemyPassiveItemDropList;
    int ammoPercent;

    int dropSpawnChanceMin;
    int dropSpawnChanceMax;

    int numberOfItemsToSpawnMin;
    int numberOfItemsToSpawnMax;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    GameObject chestItemGameObject;
    ChestItem chestItem;
    Enemy enemy;

    private void Start()
    {
        chestItemPrefab = GameResources.Instance.chestItemPrefab;
        enemy = GetComponent<Enemy>();

        SetDropList();

        // Instantiate container
        InstantiateItem();
    }

    public void DropProcess()
    {
        // Should drop be spawned based on specified chance? If not return.
        if (!RandomDropCheck()) return;

        // Get number of Ammo & Passive & Weapon Items To Spawn (max 3 of each)
        GetItemsToSpawn(out int ammoNum, out int passiveItemNum, out int weaponNum);

        // Initialize drops
        weaponDetails = GetWeaponDetailsToSpawn(weaponNum);
        passiveItemDetails = GetPassiveItemDetailsToSpawn(passiveItemNum);
        ammoPercent = GetAmmoPercentToSpawn(ammoNum, enemy.enemyDetails.ammoPercent);

        // Instantiate items if not null
        if (weaponDetails != null)
        {
            InstantiateWeaponItem(weaponDetails);
        }

        if (passiveItemDetails != null)
        {
            InstantiatePassiveItem(passiveItemDetails);
        }

        if (ammoPercent != 0)
        {
            InstantiateAmmoItem(ammoPercent);
        }

        // Break drop free from parent
        chestItem.transform.SetParent(null);
    }

    private void SetDropList()
    {
        enemyWeaponDropList = enemy.enemyDetails.weaponsByLevelList;
        enemyPassiveItemDropList = enemy.enemyDetails.passiveItemsByLevelList;
        dropSpawnChanceMin = enemy.enemyDetails.dropSpawnChanceMin;
        dropSpawnChanceMax = enemy.enemyDetails.dropSpawnChanceMax;
        numberOfItemsToSpawnMin = enemy.enemyDetails.numberOfItemsToSpawnMin;
        numberOfItemsToSpawnMax = enemy.enemyDetails.numberOfItemsToSpawnMax;
    }

    /// <summary>
    /// Check if a drop should be spawned based on the drop spawn chance - returns true if drop should be spawned false otherwise
    /// </summary>
    private bool RandomDropCheck()
    {
        int chancePercent = Random.Range(dropSpawnChanceMin, dropSpawnChanceMax + 1);

        // get random value between 1 and 100
        int randomPercent = Random.Range(1, 100 + 1);

        if (randomPercent <= chancePercent)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    /// <summary>
    /// Get the number of items to spawn - max 1 of each - max 3 in total
    /// </summary>
    private void GetItemsToSpawn(out int ammo, out int passives, out int weapons)
    {
        ammo = 0;
        passives = 0;
        weapons = 0;

        int numberofItemsToSpawn = Random.Range(numberOfItemsToSpawnMin, numberOfItemsToSpawnMax + 1);

        int choice;

        if (numberofItemsToSpawn == 1)
        {
            choice = Random.Range(0, 10);

            if (choice >= 0 && choice <= 2) { weapons++; return; }
            if (choice == 3) { ammo++; return; }
            if (choice > 3 && choice <= 9) { passives++; return; }

            return;
        }
        else if (numberofItemsToSpawn == 2)
        {
            choice = Random.Range(0, 10);
            if (choice >= 0 && choice <= 2) { weapons++; return; }
            if (choice == 3) { ammo++; return; }
            if (choice > 3 && choice <= 9) { passives++; return; }
        }
        else if (numberofItemsToSpawn >= 3)
        {
            ammo++;
            passives++;
            weapons++;

            return;
        }
    }

    /// <summary>
    /// Instantiate a chest item
    /// </summary>
    private void InstantiateItem()
    {
        chestItemGameObject = Instantiate(chestItemPrefab, transform.position, Quaternion.identity);
        chestItem = chestItemGameObject.GetComponent<ChestItem>();
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails)
    {
        chestItem.Initialize(weaponDetails, weaponDetails.weaponSprite, weaponDetails.weaponName, transform.position);
        chestItem.hasWeaponDrop = true;
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails)
    {
        chestItem.Initialize(null, passiveItemDetails.passiveItemSprite, passiveItemDetails.passiveItemName, transform.position);
        chestItem.hasPassiveDrop = true;
    }

    /// <summary>
    /// Instantiate an ammo item for the player to collect
    /// </summary>
    private void InstantiateAmmoItem(int ammoPercent)
    {
        chestItem.Initialize(null, GameResources.Instance.ammoDropIcon, ammoPercent.ToString() + "%", transform.position);
        chestItem.hasAmmoDrop = true;
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNumber)
    {
        if (weaponNumber == 0)
            return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(enemyWeaponDropList);

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        return weaponDetails;
    }

    /// <summary>
    /// Get the passive item details to spawn - return null if no passive item is to be spawned
    /// </summary>
    private PassiveItemDetailsSO GetPassiveItemDetailsToSpawn(int passiveItemNumber)
    {
        if (passiveItemNumber == 0)
            return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(enemyPassiveItemDropList);

        PassiveItemDetailsSO passiveItemDetails = passiveItemRandom.GetItem();

        return passiveItemDetails;
    }

    /// <summary>
    /// Get ammo percent to spawn
    /// </summary>
    private int GetAmmoPercentToSpawn(int ammoNumber, int ammoPercent)
    {
        if (ammoNumber == 0)
            return 0;

        return ammoPercent;
    }
}