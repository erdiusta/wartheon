using Mirror;
using Pathfinding.Serialization;
using System.Collections.Generic;
using UnityEngine;

public class ChestSpawner : MonoBehaviour
{
    [System.Serializable]
    private struct RangeByLevel
    {
        public DungeonLevelSO dungeonLevel;
        [Range(0, 100)] public int min;
        [Range(0, 100)] public int max;
    }

    #region Header CHEST PREFAB
    [Space(10)]
    [Header("CHEST PREFAB")]
    #endregion Header CHEST PREFAB
    #region Tooltip
    [Tooltip("Populate with the chest prefab")]
    #endregion Tooltip
    [SerializeField] GameObject[] chestPrefabs;

    #region Header CHEST SPAWN CHANCE
    [Space(10)]
    [Header("CHEST SPAWN CHANCE")]
    #endregion Header CHEST SPAWN CHANCE
    #region Tooltip
    [Tooltip("The minimum probability for spawning a chest")]
    #endregion Tooltip
    [SerializeField][Range(0, 100)] int chestSpawnChanceMin;
    #region Tooltip
    [Tooltip("The maximum probability for spawning a chest")]
    #endregion Tooltip
    [SerializeField][Range(0, 100)] int chestSpawnChanceMax;
    #region Tooltip
    [Tooltip(" You can override the chest spawn chance by dungeon level")]
    #endregion Tooltip
    [SerializeField] List<RangeByLevel> chestSpawnChanceByLevelList;

    [HideInInspector] public static float rareChestLocateModifier = 0f;
    [HideInInspector] public static float legendaryChestLocateModifier = 0f;

    GameObject selectedChestPrefab;
    bool chestSpawned = false;

    Room chestRoom;
    RoomNetData chestRoomNetData;
    int selectedChestIndex;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            // Get the room the chest is in if we don't already have it
            if (chestRoom == null)
            {
                chestRoom = GetComponentInParent<InstantiatedRoom>().room;
            }

            // If the chest is spawned on room entry then spawn chest
            if (!chestSpawned && chestRoom == roomChangedEventArgs.room)
            {
                SpawnChest(isMultiplayer: false);
            }
        }
        else
        {
            if (!NetworkServer.active) return;

            if (chestRoomNetData.Equals(default))
            {
                InstantiatedRoom ir = GetComponentInParent<InstantiatedRoom>();
                chestRoomNetData = ir.roomNetData;
            }

            if (!chestSpawned && chestRoomNetData.roomId == roomChangedEventArgs.roomNetData.roomId)
            {
                SpawnChest(isMultiplayer: true);
            }
        }
    }

    /// <summary>
    /// Spawn the chest prefab
    /// </summary>
    private void SpawnChest(bool isMultiplayer)
    {
        if (isMultiplayer)
        {
            SpawnChest_Server();
        }
        else
        {
            chestSpawned = true;

            int seed = Random.Range(int.MinValue, int.MaxValue);
            WartheonRNG rng = new WartheonRNG(seed);

            // Should chest be spawned based on specified chance? If not return.
            if (!RandomSpawnChest(rng)) return;

            // Get Weapon Item To Spawn
            GetItemsToSpawn(out int passiveItemNum, out int weaponNum, rng);

            int randomNumberForChest = rng.Range(0, 100);

            // Instantiate chest
            if (randomNumberForChest < 70 - (rareChestLocateModifier * 100))
            {
                selectedChestIndex = 0;
                selectedChestPrefab = chestPrefabs[selectedChestIndex];
            }
            else if (randomNumberForChest < 90 - (legendaryChestLocateModifier * 100))
            {
                selectedChestIndex = 1;
                selectedChestPrefab = chestPrefabs[selectedChestIndex];
            }
            else
            {
                selectedChestIndex = 2;
                selectedChestPrefab = chestPrefabs[2];
            }

            // Instantiate chest
            GameObject chestGameObject = Instantiate(selectedChestPrefab, transform);

            // Position chest
            chestGameObject.transform.position = transform.position;

            // Get Chest component
            Chest chest = chestGameObject.GetComponent<Chest>();

            // Get Item Details
            WeaponDetailsSO weaponDetails = ChestLootDatabase.Instance.GetWeaponDetailsToSpawn(weaponNum, rng, selectedChestIndex);
            PassiveItemDetailsSO passiveItemDetails = ChestLootDatabase.Instance.GetPassiveItemDetailsToSpawn(passiveItemNum, rng, selectedChestIndex);

            // Initialize items
            chest.Initialize(weaponDetails, rng);
            chest.Initialize(passiveItemDetails, rng);
        }
    }

    [Server]
    private void SpawnChest_Server()
    {
        chestSpawned = true;

        int seed = Random.Range(int.MinValue, int.MaxValue);
        WartheonRNG rng = new WartheonRNG(seed);

        if (!RandomSpawnChest(rng)) return;

        GetItemsToSpawn(out int passiveItemNum, out int weaponNum, rng);

        int randomNumberForChest = rng.Range(0, 100);

        if (randomNumberForChest < 70 - (rareChestLocateModifier * 100)) selectedChestIndex = 3;    
        else if (randomNumberForChest < 90 - (legendaryChestLocateModifier * 100)) selectedChestIndex = 4;
        else selectedChestIndex = 5;

        // Instantiate on server
        GameObject chestGO = Instantiate(chestPrefabs[selectedChestIndex], transform.position, Quaternion.identity);

        // Get network script
        ChestNetwork chestNet = chestGO.GetComponent<ChestNetwork>();

        // Generate actual loot on Server
        WeaponDetailsSO weaponDetails = ChestLootDatabase.Instance.GetWeaponDetailsToSpawn(weaponNum, rng, selectedChestIndex);
        PassiveItemDetailsSO passiveItemDetails = ChestLootDatabase.Instance.GetPassiveItemDetailsToSpawn(passiveItemNum, rng, selectedChestIndex);

        // Title id
        WeaponTitle weaponTitle = weaponDetails != null ? weaponDetails.weaponTitle : WeaponTitle.None;
        PassiveItemType passiveItemType = passiveItemDetails != null ? passiveItemDetails.passiveItemType : PassiveItemType.None;

        // Send Only Deterministic Data
        chestNet.InitializeServer(seed, selectedChestIndex, weaponTitle, passiveItemType);

        // Spawn over network
        NetworkServer.Spawn(chestGO);
    }

    /// <summary>
    /// Check if a chest should be spawned based on the chest spawn chance - returns true if chest should be spawned false otherwise
    /// </summary>
    private bool RandomSpawnChest(WartheonRNG rng)
    {
        int chancePercent = rng.Range(chestSpawnChanceMin, chestSpawnChanceMax + 1);

        // Check if an override chance percent has been set for the current level
        foreach (RangeByLevel rangeByLevel in chestSpawnChanceByLevelList)
        {
            if (rangeByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                chancePercent = rng.Range(rangeByLevel.min, rangeByLevel.max + 1);
                break;
            }
        }

        // get random value between 1 and 100
        int randomPercent = rng.Range(1, 100 + 1);

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
    private void GetItemsToSpawn(out int passiveItems, out int weapons, WartheonRNG rng)
    {
        passiveItems = 0;
        weapons = 0;

        int choice = rng.Range(0, 50);

        if (choice >= 0 && choice <= 25) { weapons++; return; }
        if (choice > 25 && choice <= 50) { passiveItems++; return; }
    }

    #region Validation
#if UNITY_EDITOR
    // Validate prefab details enetered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(chestPrefabs), chestPrefabs);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(chestSpawnChanceMin), chestSpawnChanceMin, nameof(chestSpawnChanceMax), 
            chestSpawnChanceMax, true);

        if (chestSpawnChanceByLevelList != null && chestSpawnChanceByLevelList.Count > 0)
        {
            HelperUtilities.ValidateCheckEnumerableValues(this, nameof(chestSpawnChanceByLevelList), chestSpawnChanceByLevelList);

            foreach (RangeByLevel rangeByLevel in chestSpawnChanceByLevelList)
            {
                HelperUtilities.ValidateCheckNullValue(this, nameof(rangeByLevel.dungeonLevel), rangeByLevel.dungeonLevel);
                HelperUtilities.ValidateCheckPositiveRange(this, nameof(rangeByLevel.min), rangeByLevel.min, nameof(rangeByLevel.max), rangeByLevel.max, true);
            }
        }
    }
#endif
    #endregion
}
