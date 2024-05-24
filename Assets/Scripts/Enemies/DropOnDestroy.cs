using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropOnDestroy : MonoBehaviour
{
    public GameObject chestItemPrefab;

    List<SpawnableObjectsByLevel<WeaponDetailsSO>> enemyWeaponDropList;
    List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> enemyPassiveItemDropList;
    List<SpawnableObjectsByLevel<ActiveItemDetailsSO>> enemyActiveItemDropList;
    int ammoPercent;

    int dropSpawnChanceMin;
    int dropSpawnChanceMax;

    int numberOfItemsToSpawnMin;
    int numberOfItemsToSpawnMax;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    ActiveItemDetailsSO activeItemDetails;
    GameObject chestItemGameObject;
    ChestItem chestItem;
    Enemy enemy;

    private void Start()
    {
        chestItemPrefab = GameResources.Instance.chestItemPrefab;
        enemy = GetComponent<Enemy>();

        SetDropList();

        // Instantiate container
        InstantiateChestItem();
    }

    public void DropProcess()
    {
        // Should drop be spawned based on specified chance? If not return.
        if (!RandomDropCheck())
        {
            Destroy(chestItemGameObject);
            return;
        }
            
        // Get number of Ammo & Passive & Weapon Items To Spawn (max 3 of each)
        GetItemsToSpawn(out int activeItemNum, out int passiveItemNum, out int weaponNum);

        // Initialize drops
        weaponDetails = GetWeaponDetailsToSpawn(weaponNum);
        passiveItemDetails = GetPassiveItemDetailsToSpawn(passiveItemNum);
        activeItemDetails = GetActiveItemDetailsToSpawn(activeItemNum);

        //ammoPercent = GetAmmoPercentToSpawn(activeItemNum, enemy.enemyDetails.ammoPercent);

        // Instantiate items if not null
        if (weaponDetails != null)
        {
            InstantiateWeaponItem(weaponDetails);
        }

        if (passiveItemDetails != null)
        {
            InstantiatePassiveItem(passiveItemDetails);
        }

        if (activeItemDetails != null)
        {
            InstantiateActiveItem(activeItemDetails);
        }

        // Break drop free from parent
        chestItem.transform.SetParent(null);
    }

    private void SetDropList()
    {
        enemyWeaponDropList = enemy.enemyDetails.weaponsByLevelList;
        enemyPassiveItemDropList = enemy.enemyDetails.passiveItemsByLevelList;
        enemyActiveItemDropList = enemy.enemyDetails.activeItemsByLevelList;
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
    private void GetItemsToSpawn(out int actives, out int passives, out int weapons)
    {
        actives = 0;
        passives = 0;
        weapons = 0;

        int numberofItemsToSpawn = Random.Range(numberOfItemsToSpawnMin, numberOfItemsToSpawnMax + 1);

        int choice;

        if (numberofItemsToSpawn == 1)
        {
            choice = Random.Range(0, 10);

            if (choice >= 0 && choice <= 2) { weapons++; return; }
            if (choice >= 3 && choice <= 5) { actives++; return; }
            if (choice > 5 && choice <= 9) { passives++; return; }

            return;
        }
        else if (numberofItemsToSpawn == 2)
        {
            choice = Random.Range(0, 10);
            if (choice >= 0 && choice <= 2) { weapons++; return; }
            if (choice >= 3 && choice <= 5) { actives++; return; }
            if (choice > 5 && choice <= 9) { passives++; return; }
        }
        else if (numberofItemsToSpawn >= 3)
        {
            actives++;
            passives++;
            weapons++;

            return;
        }
    }

    /// <summary>
    /// Instantiate a chest item
    /// </summary>
    private void InstantiateChestItem()
    {
        chestItemGameObject = Instantiate(chestItemPrefab, transform.position, Quaternion.identity);
        chestItemGameObject.GetComponent<BoxCollider2D>().enabled = true;
        chestItem = chestItemGameObject.GetComponent<ChestItem>();
        chestItem.droppedByPlayer = false;
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails)
    {
        chestItem.hasWeaponDrop = true;
        chestItem.Initialize(weaponDetails, null, null, weaponDetails.weaponFrontSprite, weaponDetails.weaponName, transform.position);
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails)
    {
        chestItem.hasPassiveDrop = true;
        chestItem.Initialize(null, null, passiveItemDetails, passiveItemDetails.passiveItemSprite, passiveItemDetails.passiveItemName, transform.position);
    }

    /// <summary>
    /// Instantiate an active item for the player to collect
    /// </summary>
    private void InstantiateActiveItem(ActiveItemDetailsSO activeItemDetails)
    {
        int ammoPercent = 0;

        chestItem.hasActiveDrop = true;

        //chestItem.Initialize(null, GameResources.Instance.ammoDropIcon, ammoPercent.ToString() + "%", transform.position);
        chestItem.Initialize(null, activeItemDetails, null, activeItemDetails.activeItemSprite, activeItemDetails.activeItemName, transform.position);
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNumber)
    {
        if (weaponNumber == 0) return null;

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
        if (passiveItemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(enemyPassiveItemDropList);

        PassiveItemDetailsSO passiveItemDetails = passiveItemRandom.GetItem();

        return passiveItemDetails;
    }

    /// <summary>
    /// Get the active item details to spawn - return null if no active item is to be spawned
    /// </summary>
    private ActiveItemDetailsSO GetActiveItemDetailsToSpawn(int activeItemNumber)
    {
        if (activeItemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<ActiveItemDetailsSO> activeItemRandom = new RandomSpawnableObject<ActiveItemDetailsSO>(enemyActiveItemDropList);

        ActiveItemDetailsSO activeItemDetails = activeItemRandom.GetItem();

        return activeItemDetails;
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