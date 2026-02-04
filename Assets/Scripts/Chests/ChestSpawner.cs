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

    #region Header CHEST CONTENT DETAILS
    [Space(10)]
    [Header("CHEST CONTENT DETAILS")]
    #endregion Header CHEST CONTENT DETAILS
    #region Tooltip
    [Tooltip("The weapons to spawn for each dungeon level and their spawn ratios")]
    #endregion Tooltip
    [SerializeField] List<ChestsBasedOnSpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByChestBasedOnLevelList;

    #region Tooltip
    [Tooltip("The passive items to spawn for each dungeon level and their spawn ratios")]
    #endregion Tooltip
    [SerializeField] List<ChestsBasedOnSpawnableObjectsByLevel<PassiveItemDetailsSO>> passiveItemsSpawnByChestBasedOnLevelList;

    [HideInInspector] public static float rareChestLocateModifier = 0f;
    [HideInInspector] public static float legendaryChestLocateModifier = 0f;

    GameObject selectedChestPrefab;
    bool chestSpawned = false;
    Room chestRoom;

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // Get the room the chest is in if we don't already have it
        if (chestRoom == null)
        {
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        // If the chest is spawned on room entry then spawn chest
        if (!chestSpawned && chestRoom == roomChangedEventArgs.room)
        {
            SpawnChest();
        }
    }

    /// <summary>
    /// Handle room enemies defeated event
    /// </summary>
    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        // Get the room the chest is in if we don't already have it
        if (chestRoom == null)
        {
            chestRoom = GetComponentInParent<InstantiatedRoom>().room;
        }

        // If the chest is spawned when enemies are defeated and the chest is in the room that the
        // enemies have been defeated
        if (!chestSpawned && chestRoom == roomEnemiesDefeatedArgs.room)
        {
            SpawnChest();
        }
    }

    /// <summary>
    /// Spawn the chest prefab
    /// </summary>
    private void SpawnChest()
    {
        chestSpawned = true;

        // Should chest be spawned based on specified chance? If not return.
        if (!RandomSpawnChest()) return;

        // Get Weapon Item To Spawn
        GetItemsToSpawn(out int passiveItemNum, out int weaponNum);

        int randomNumberForChest = Random.Range(0, 100);

        // Instantiate chest
        if (randomNumberForChest < 70 - (rareChestLocateModifier * 100))
        {
            selectedChestPrefab = chestPrefabs[0];
        }
        else if (randomNumberForChest < 90 - (legendaryChestLocateModifier * 100))
        {
            selectedChestPrefab = chestPrefabs[1];
        }
        else
        {
            selectedChestPrefab = chestPrefabs[2];
        }

        // Instantiate chest
        GameObject chestGameObject = Instantiate(selectedChestPrefab, transform);

        // Position chest
        chestGameObject.transform.position = transform.position;

        // Get Chest component
        Chest chest = chestGameObject.GetComponent<Chest>();

        // Initialize items
        chest.Initialize(GetWeaponDetailsToSpawn(weaponNum));
        chest.Initialize(GetPassiveItemDetailsToSpawn(passiveItemNum));
    }

    /// <summary>
    /// Check if a chest should be spawned based on the chest spawn chance - returns true if chest should be spawned false otherwise
    /// </summary>
    private bool RandomSpawnChest()
    {
        int chancePercent = Random.Range(chestSpawnChanceMin, chestSpawnChanceMax + 1);

        // Check if an override chance percent has been set for the current level
        foreach (RangeByLevel rangeByLevel in chestSpawnChanceByLevelList)
        {
            if (rangeByLevel.dungeonLevel == GameManager.Instance.GetCurrentDungeonLevel())
            {
                chancePercent = Random.Range(rangeByLevel.min, rangeByLevel.max + 1);
                break;
            }
        }

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
    private void GetItemsToSpawn(out int passiveItems, out int weapons)
    {
        passiveItems = 0;
        weapons = 0;

        int choice = Random.Range(0, 50);

        if (choice >= 0 && choice <= 25) { weapons++; return; }
        if (choice > 25 && choice <= 50) { passiveItems++; return; }
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNumber)
    {
        if (weaponNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the
        // relative 'ratios' of the items specified
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(new List<SpawnableObjectsByLevel<WeaponDetailsSO>>());

        if (selectedChestPrefab.Equals(chestPrefabs[0]))
        {
            weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByChestBasedOnLevelList[0].spawnableObjectByLevelList);
        }
        else if (selectedChestPrefab.Equals(chestPrefabs[1]))
        {
            weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByChestBasedOnLevelList[1].spawnableObjectByLevelList);
        }
        else if (selectedChestPrefab.Equals(chestPrefabs[2]))
        {
            weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByChestBasedOnLevelList[2].spawnableObjectByLevelList);
        }

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        return weaponDetails;
    }

    /// <summary>
    /// Get the passive item details to spawn - return null if no item is to be spawned or the player already has the weapon
    /// </summary>
    private PassiveItemDetailsSO GetPassiveItemDetailsToSpawn(int itemNumber)
    {
        if (itemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the
        // relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(new List<SpawnableObjectsByLevel<PassiveItemDetailsSO>>());

        if (selectedChestPrefab.Equals(chestPrefabs[0]))
        {
            passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(passiveItemsSpawnByChestBasedOnLevelList[0].spawnableObjectByLevelList);
        }
        else if (selectedChestPrefab.Equals(chestPrefabs[1]))
        {
            passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(passiveItemsSpawnByChestBasedOnLevelList[1].spawnableObjectByLevelList);
        }
        else if (selectedChestPrefab.Equals(chestPrefabs[2]))
        {
            passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(passiveItemsSpawnByChestBasedOnLevelList[2].spawnableObjectByLevelList);
        }

        PassiveItemDetailsSO passiveItemDetails = passiveItemRandom.GetItem();

        return passiveItemDetails;
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
