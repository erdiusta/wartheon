using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

public class DropOnDestroy : MonoBehaviour
{
    [HideInInspector] public GameObject chestItemGameObject;

    List<SpawnableObjectsByLevel<WeaponDetailsSO>> enemyWeaponDropList;
    List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> enemyPassiveItemDropList;
    List<SpawnableObjectsByLevel<ActiveItemDetailsSO>> enemyActiveItemDropList;
    int ammoPercent;
    int characterIndexNo;

    int dropSpawnChanceMin;
    int dropSpawnChanceMax;

    int numberOfItemsToSpawnMin;
    int numberOfItemsToSpawnMax;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    ActiveItemDetailsSO activeItemDetails;
    ChestItem chestItem;
    Enemy enemy;
    Player player;

    private void Start()
    {
        enemy = GetComponent<Enemy>();
        player = GameManager.Instance.GetPlayer();

        SetDropList();
    }

    public void DropProcess()
    {
        // Should drop be spawned based on specified chance? If not return.
        if (!RandomDropCheck())
        {
            Destroy(chestItemGameObject);
            return;
        }

        // Instantiate container
        InstantiateChestItem();

        // Set collider to true
        chestItemGameObject.GetComponent<BoxCollider2D>().enabled = true;
            
        // Get number of Ammo & Passive & Weapon Items To Spawn (max 3 of each)
        GetItemsToSpawn(out int activeItemNum, out int passiveItemNum, out int weaponNum);

        // Initialize drops
        weaponDetails = GetWeaponDetailsToSpawn(weaponNum);
        passiveItemDetails = GetPassiveItemDetailsToSpawn(passiveItemNum);
        activeItemDetails = GetActiveItemDetailsToSpawn(activeItemNum);

        // Instantiate items if not null
        if (weaponDetails != null)
        {
            InstantiateWeaponItem(weaponDetails);
            chestItem.transform.SetParent(null);
        }

        if (passiveItemDetails != null)
        {
            InstantiatePassiveItem(passiveItemDetails);
            chestItem.transform.SetParent(null);
        }

        if (activeItemDetails != null)
        {
            InstantiateActiveItem(activeItemDetails);
            chestItem.transform.SetParent(null);
        }
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
        int chancePercent = 100 - Random.Range(dropSpawnChanceMin, dropSpawnChanceMax + 1);

        int passiveItemModifier;

        if (GameManager.Instance.GetPlayer().playerDetails.passiveItemsList.Any(item => item.passiveItemType == PassiveItemType.RingOfFortune))
        {
            // The player has a passive item of type RingOfFortune
            passiveItemModifier = 15;
        }
        else
        {
            passiveItemModifier = 0;
        }

        // get random value between 1 and 100
        int randomPercent = Random.Range(1, 100 + 1);

        randomPercent += passiveItemModifier;
        randomPercent = randomPercent >= 100 ? 100 : randomPercent;

        if (randomPercent >= chancePercent)
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
            choice = Random.Range(0, 50);

            if (choice >= 0 && choice <= 35) { weapons++; return; }
            if (choice > 35 && choice <= 40) { actives++; return; }
            if (choice > 40 && choice <= 50) { passives++; return; }

            return;
        }
        else if (numberofItemsToSpawn == 2)
        {
            choice = Random.Range(0, 50);
            if (choice >= 0 && choice <= 2) { weapons++; return; }
            if (choice >= 3 && choice <= 5) { actives++; return; }
            if (choice > 5 && choice <= 50) { passives++; return; }
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
        chestItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        chestItemGameObject.GetComponent<BoxCollider2D>().enabled = false;
        chestItem = chestItemGameObject.GetComponent<ChestItem>();
        chestItem.droppedByPlayer = false;
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails)
    {
        if (chestItem == null) return;

        chestItem.hasWeaponDrop = true;
        Weapon weapon = new Weapon();
        weapon.weaponDetails = weaponDetails;

        chestItem.Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position);
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails)
    {
        if (chestItem == null) return;

        if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Primary)
        {
            chestItem.hasPrimaryPassiveDrop = true;
        }
        else if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Secondary)
        {
            chestItem.hasSecondaryPassiveDrop = true;
        }

        PassiveItem passiveItem = new PassiveItem();
        passiveItem.passiveItemDetails = passiveItemDetails;

        chestItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, transform.position);
    }

    /// <summary>
    /// Instantiate an active item for the player to collect
    /// </summary>
    private void InstantiateActiveItem(ActiveItemDetailsSO activeItemDetails)
    {
        if (chestItem == null) return;

        chestItem.hasActiveDrop = true;
        ActiveItem activeItem = new ActiveItem();
        activeItem.activeItemDetails = activeItemDetails;

        chestItem.Initialize(activeItem, activeItemDetails.activeItemSprite, transform.position);
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

        if (IsWeaponAvailableForTheCharacter(weaponDetails))
        {
            return weaponDetails;
        }
        else
        {
            Destroy(chestItemGameObject);
            return null;
        }
    }

    private bool IsWeaponAvailableForTheCharacter(WeaponDetailsSO weaponDetails)
    {
        for (int i = 0; i < player.playerDetails.collectibleWeaponsArray.Length; i++)
        {
            if (player.playerDetails.collectibleWeaponsArray[i].weaponName == weaponDetails.weaponName)
            {
                return true;
            }
        }

        return false;
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
}