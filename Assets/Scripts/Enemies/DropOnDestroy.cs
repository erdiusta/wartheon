using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropOnDestroy : MonoBehaviour
{
    [HideInInspector] public GameObject dropItemGameObject;

    // Consumables
    List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> enemyPrimaryPassiveItemDropList;

    // Equipped weapons and passive body items
    List<SpawnableObjectsByLevel<WeaponDetailsSO>> enemyWeaponDropList;
    List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> enemySecondaryPassiveItemDropList;

    int dropSpawnChanceMin;
    int dropSpawnChanceMax;

    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO primaryPassiveItemDetails;
    PassiveItemDetailsSO secondaryPassiveItemDetails;
    DropItem dropItem;
    Enemy enemy;
    Player player;
    Room currentRoom;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        player = GameManager.Instance.GetPlayer();
        currentRoom = GameManager.Instance.GetCurrentRoom();

        SetDropList();
    }

    public void DropProcess()
    {
        // PRIMARY PASSIVE DROP PHASE
        // Get primary passive items
        if (enemy.enemyDetails.enemyType == EnemyType.Minion) return; // If it is a minion, no drop happens

        int primaryPassiveItemNum = Random.Range(0, enemy.enemyDetails.primaryPassiveDropChanceMax + 1);

        for (int i = 0; i < primaryPassiveItemNum; i++)
        {
            // Instantiate item container
            InstantiateDropItem();

            // Retrieve item details
            primaryPassiveItemDetails = GetPrimaryPassiveItemDetailsToSpawn(primaryPassiveItemNum);

            InstantiatePassiveItem(primaryPassiveItemDetails);
            dropItem.transform.SetParent(null);

            Vector3 spawnPointDeviation = new Vector3(Random.Range(-2, 2), Random.Range(-2, 2), 0);
            dropItem.transform.position += spawnPointDeviation;
        }

        // OTHER DROPS PHASE IF HAS
        // Should drop be spawned based on specified chance? If not return.
        if (!RandomDropCheck())
        {
            Destroy(dropItemGameObject);
            return;
        }

        // Instantiate container
        InstantiateDropItem();
        
        // Get number of Passive & Weapon Items To Spawn (max 2 of each)
        GetItemsToSpawn(out int secondaryPassiveItemNum, out int weaponNum);

        // Initialize drops
        weaponDetails = GetWeaponDetailsToSpawn(weaponNum);
        secondaryPassiveItemDetails = GetSecondaryPassiveItemDetailsToSpawn(secondaryPassiveItemNum);

        if (weaponDetails != null)
        {
            InstantiateWeaponItem(weaponDetails);
            dropItem.transform.SetParent(currentRoom.instantiatedRoom.transform);
        }

        if (secondaryPassiveItemDetails != null)
        {
            InstantiatePassiveItem(secondaryPassiveItemDetails);
            dropItem.transform.SetParent(currentRoom.instantiatedRoom.transform);
        }
    }

    private void SetDropList()
    {
        enemySecondaryPassiveItemDropList = enemy.enemyDetails.secondaryPassiveItemsByLevelList;
        enemyWeaponDropList = enemy.enemyDetails.weaponsByLevelList;
        enemyPrimaryPassiveItemDropList = enemy.enemyDetails.primaryPassiveItemsByLevelList;
        dropSpawnChanceMin = enemy.enemyDetails.dropSpawnChanceMin;
        dropSpawnChanceMax = enemy.enemyDetails.dropSpawnChanceMax;
    }

    /// <summary>
    /// Check if a drop should be spawned based on the drop spawn chance - returns true if drop should be spawned false otherwise
    /// </summary>
    private bool RandomDropCheck()
    {
        int chancePercent = 100 - Random.Range(dropSpawnChanceMin, dropSpawnChanceMax + 1);

        //int passiveItemModifier = (int)(player.additionalDropChanceModifier * 100);

        // get random value between 1 and 100
        int randomPercent = Random.Range(1, 101);

        //randomPercent += passiveItemModifier;
        randomPercent = randomPercent >= 100 ? 100 : randomPercent;

        if (randomPercent >= chancePercent) return true;
        else return false;
    }

    /// <summary>
    /// Get the number of items to spawn - max 1 of each - max 2 in total
    /// </summary>
    private void GetItemsToSpawn(out int secondaryPassives, out int weapons)
    {
        secondaryPassives = 0;
        weapons = 0;

        int choice = Random.Range(0, 50);

        if (choice >= 0 && choice <= 25) { weapons++; return; }
        if (choice > 25 && choice <= 50) { secondaryPassives++; return; }
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
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails)
    {
        if (dropItem == null) return;

        dropItem.hasWeaponDrop = true;

        // Create a weapon instance with rolled modifiers
        Weapon weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails);

        dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position);
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails)
    {
        if (dropItem == null) return;

        if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Primary)
        {
            dropItem.hasPrimaryPassiveDrop = true;
        }
        else if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Secondary)
        {
            dropItem.hasSecondaryPassiveDrop = true;
        }

        PassiveItem passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails);

        dropItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, transform.position);
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
    /// Get the secondary passive item details to spawn - return null if no passive item is to be spawned
    /// </summary>
    private PassiveItemDetailsSO GetSecondaryPassiveItemDetailsToSpawn(int passiveItemNumber)
    {
        if (passiveItemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> secondaryPassiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(enemySecondaryPassiveItemDropList);

        PassiveItemDetailsSO passiveItemDetails = secondaryPassiveItemRandom.GetItem();

        return passiveItemDetails;
    }

    /// <summary>
    /// Get the primary passive item details to spawn - return null if no passive item is to be spawned
    /// </summary>
    private PassiveItemDetailsSO GetPrimaryPassiveItemDetailsToSpawn(int passiveItemNumber)
    {
        if (passiveItemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> primaryPassiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(enemyPrimaryPassiveItemDropList);

        PassiveItemDetailsSO passiveItemDetails = primaryPassiveItemRandom.GetItem();

        return passiveItemDetails;
    }
}