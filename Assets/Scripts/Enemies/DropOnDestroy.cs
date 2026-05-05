using Mirror;
using System.Collections;
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
    DropItemNetwork dropItemNetwork;
    Enemy enemy;
    InstantiatedRoom instantiatedRoom;

    bool isMultiplayer = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();

        isMultiplayer = NetworkServer.active || NetworkClient.active;
    }

    public void InitializeDropList()
    {
        if (!isMultiplayer)
        {
            Room currentRoom = GameManager.Instance.GetCurrentRoom();
            instantiatedRoom = currentRoom.instantiatedRoom;
        }
        else
        {
            RoomNetData currentRoomNetData = GameSessionManager.Instance.GetCurrentRoomNetData();
            instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
        }

        enemySecondaryPassiveItemDropList = enemy.enemyDetails.secondaryPassiveItemsByLevelList;
        enemyWeaponDropList = enemy.enemyDetails.weaponsByLevelList;
        enemyPrimaryPassiveItemDropList = enemy.enemyDetails.primaryPassiveItemsByLevelList;
        dropSpawnChanceMin = enemy.enemyDetails.dropSpawnChanceMin;
        dropSpawnChanceMax = enemy.enemyDetails.dropSpawnChanceMax;
    }

    public void DropProcess()
    {
        // PRIMARY PASSIVE DROP PHASE
        // Get primary passive items
        if (enemy.isMinion) return; // If it is a minion, no drop happens

        if (isMultiplayer && !NetworkServer.active) return;

        int seed = Random.Range(int.MinValue, int.MaxValue);

        EnemyDropData dropData = new EnemyDropData
        {
            primaryPassiveDropChanceMax = enemy.enemyDetails.primaryPassiveDropChanceMax
        };

        ExecuteDropProcess(seed, dropData);
    }

    // SERVER CODE
    public void ExecuteDropProcess(int seed, EnemyDropData dropData)
    {
        WartheonRNG rng = new WartheonRNG(seed);

        int primaryPassiveItemNum = rng.Range(0, dropData.primaryPassiveDropChanceMax + 1);

        CreatePrimaryPassiveItems(seed, primaryPassiveItemNum);

        // OTHER DROPS PHASE IF HAS
        // Should drop be spawned based on specified chance? If not return.
        if (!RandomDropCheck(out int chancePercent, out int randomPercent, rng))
        {
            Destroy(dropItemGameObject);
            return;
        }

        // Instantiate container
        InstantiateDropItem();

        // Get number of Passive & Weapon Items To Spawn (max 2 of each)
        GetItemsToSpawn(out int secondaryPassiveItemNum, out int weaponNum, out int choice, rng);

        // Initialize drops
        weaponDetails = GetWeaponDetailsToSpawn(weaponNum, rng);
        secondaryPassiveItemDetails = GetSecondaryPassiveItemDetailsToSpawn(secondaryPassiveItemNum, rng);

        InstantiateWeaponAndPassiveItems(seed);
    }

    private void InstantiateWeaponAndPassiveItems(int seed)
    {
        if (weaponDetails != null)
        {
            InstantiateWeaponItem(weaponDetails, seed);

            if (isMultiplayer) dropItemNetwork.transform.SetParent(instantiatedRoom.transform);
            else dropItem.transform.SetParent(instantiatedRoom.transform);
        }

        if (secondaryPassiveItemDetails != null)
        {
            InstantiatePassiveItem(secondaryPassiveItemDetails, seed);

            if (isMultiplayer) dropItemNetwork.transform.SetParent(instantiatedRoom.transform);
            else dropItem.transform.SetParent(instantiatedRoom.transform);
        }
    }

    private void CreatePrimaryPassiveItems(int seed, int primaryPassiveItemNum)
    {
        for (int i = 0; i < primaryPassiveItemNum; i++)
        {
            WartheonRNG rng = new WartheonRNG(seed);

            // Instantiate item container
            InstantiateDropItem();

            // Retrieve item details
            primaryPassiveItemDetails = GetPrimaryPassiveItemDetailsToSpawn(primaryPassiveItemNum, rng);

            InstantiatePassiveItem(primaryPassiveItemDetails, seed);

            if (isMultiplayer) dropItemNetwork.transform.SetParent(null);
            else dropItem.transform.SetParent(null);

            Vector3 spawnPointDeviation = new Vector3(rng.Range(-2, 2), rng.Range(-2, 2), 0);

            if (isMultiplayer) dropItemNetwork.transform.position += spawnPointDeviation;
            else dropItem.transform.position += spawnPointDeviation;

            if (isMultiplayer) dropItemNetwork.passiveStats.passiveItemType = primaryPassiveItemDetails.passiveItemType;
        }
    }

    /// <summary>
    /// Check if a drop should be spawned based on the drop spawn chance - returns true if drop should be spawned false otherwise
    /// </summary>
    private bool RandomDropCheck(out int chancePercent, out int randomPercent, WartheonRNG rng)
    {
        chancePercent = 100 - rng.Range(dropSpawnChanceMin, dropSpawnChanceMax + 1);

        //int passiveItemModifier = (int)(player.additionalDropChanceModifier * 100);

        // get random value between 1 and 100
        randomPercent = rng.Range(1, 101);

        //randomPercent += passiveItemModifier;
        randomPercent = randomPercent >= 100 ? 100 : randomPercent;

        if (randomPercent >= chancePercent) return true;
        else return false;
    }

    /// <summary>
    /// Get the number of items to spawn - max 1 of each - max 2 in total
    /// </summary>
    private void GetItemsToSpawn(out int secondaryPassives, out int weapons, out int choice, WartheonRNG rng)
    {
        secondaryPassives = 0;
        weapons = 0;

        choice = rng.Range(0, 50);

        if (choice >= 0 && choice <= 25) { weapons++; return; }
        if (choice > 25 && choice <= 50) { secondaryPassives++; return; }
    }

    /// <summary>
    /// Instantiate a drop item
    /// </summary>
    private void InstantiateDropItem()
    {
        if (!isMultiplayer)
        {
            dropItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);

            dropItem = dropItemGameObject.GetComponent<DropItem>();
            dropItem.dropSourceType = DropSourceType.Enemy;

            // Set collider to true
            dropItemGameObject.GetComponent<BoxCollider2D>().enabled = true;
        }        
        else
        {
            if (NetworkServer.active)
            {
                dropItemGameObject = Instantiate(GameResources.Instance.chestItemNetworkPrefab, transform);
                NetworkServer.Spawn(dropItemGameObject);

                dropItemNetwork = dropItemGameObject.GetComponent<DropItemNetwork>();
                dropItemNetwork.dropSourceType = DropSourceType.Enemy;
                
                // Set collider to true
                dropItemGameObject.GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails, int seed)
    {
        WartheonRNG rng = new WartheonRNG(seed);

        if (isMultiplayer)
        {
            if (dropItemNetwork == null) return;

            Weapon weapon = new Weapon(Rarity.Basic);

            dropItemNetwork.hasWeaponDrop = true;
            dropItemNetwork.dropSourceType = DropSourceType.Enemy;

            weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails, rng);

            NetworkTransformUnreliable nt = dropItemNetwork.GetComponent<NetworkTransformUnreliable>();
            nt.ServerTeleport(transform.position, Quaternion.identity);

            dropItemNetwork.weaponTitle = weapon.weaponStats.weaponTitle;
            dropItemNetwork.weaponClass = weapon.weaponStats.weaponClass;
            dropItemNetwork.weaponStats = weapon.weaponStats;
            dropItemNetwork.dropCompleted = true;

            StartCoroutine(InitializeRoutine(dropItemNetwork));
        }
        else
        {
            if (dropItem == null) return;

            dropItem.hasWeaponDrop = true;
            dropItem.dropCompleted = true;
            dropItem.dropSourceType = DropSourceType.Enemy;

            // Create a weapon instance with rolled modifiers
            Weapon weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails, rng);

            dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position, null);
        }
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails, int seed)
    {
        WartheonRNG rng = new WartheonRNG(seed);

        if (isMultiplayer)
        {
            if (dropItemNetwork == null) return;

            PassiveItem passiveItem = new PassiveItem(Rarity.Basic);

            if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.None) return;

            if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Primary)
            {
                dropItemNetwork.hasPrimaryPassiveDrop = true;
                dropItemNetwork.dropSourceType = DropSourceType.Enemy;
                passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng, false, Rarity.Basic, isPrimaryPassive: true);
            }
            else if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Secondary)
            {
                dropItemNetwork.hasSecondaryPassiveDrop = true;
                dropItemNetwork.dropSourceType = DropSourceType.Enemy;
                passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng);
            }

            NetworkTransformUnreliable nt = dropItemNetwork.GetComponent<NetworkTransformUnreliable>();
            nt.ServerTeleport(transform.position, Quaternion.identity);

            dropItemNetwork.passiveItemType = passiveItem.passiveStats.passiveItemType;
            dropItemNetwork.passiveItemSlotName = passiveItem.passiveStats.passiveItemSlotName;
            dropItemNetwork.passiveStats = passiveItem.passiveStats;
            dropItemNetwork.dropCompleted = true;

            StartCoroutine(InitializeRoutine(dropItemNetwork));
        }
        else
        {
            if (dropItem == null) return;

            PassiveItem passiveItem = new PassiveItem(Rarity.Basic);

            if (passiveItemDetails?.passiveItemCategory == PassiveItemCategory.Primary)
            {
                dropItem.hasPrimaryPassiveDrop = true;
                dropItem.dropSourceType = DropSourceType.Enemy;
                passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng, false, Rarity.Basic, isPrimaryPassive: true);
            }
            else if (passiveItemDetails?.passiveItemCategory == PassiveItemCategory.Secondary)
            {
                dropItem.hasSecondaryPassiveDrop = true;
                dropItem.dropSourceType = DropSourceType.Enemy;
                passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng);
            }

            dropItem.dropCompleted = true;
            dropItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, transform.position, null);
        }
    }

    IEnumerator InitializeRoutine(DropItemNetwork drop)
    {
        yield return null;

        drop.canInitialize = true;
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNumber, WartheonRNG rng)
    {
        if (weaponNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(enemyWeaponDropList);

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem(rng);

        return weaponDetails;
    }

    /// <summary>
    /// Get the secondary passive item details to spawn - return null if no passive item is to be spawned
    /// </summary>
    private PassiveItemDetailsSO GetSecondaryPassiveItemDetailsToSpawn(int passiveItemNumber, WartheonRNG rng)
    {
        if (passiveItemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> secondaryPassiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(enemySecondaryPassiveItemDropList);

        PassiveItemDetailsSO passiveItemDetails = secondaryPassiveItemRandom.GetItem(rng);

        return passiveItemDetails;
    }

    /// <summary>
    /// Get the primary passive item details to spawn - return null if no passive item is to be spawned
    /// </summary>
    private PassiveItemDetailsSO GetPrimaryPassiveItemDetailsToSpawn(int passiveItemNumber, WartheonRNG rng)
    {
        if (passiveItemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> primaryPassiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(enemyPrimaryPassiveItemDropList);

        PassiveItemDetailsSO passiveItemDetails = primaryPassiveItemRandom.GetItem(rng);

        return passiveItemDetails;
    }
}