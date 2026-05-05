using System.Collections.Generic;
using UnityEngine;

public class ChestLootDatabase : SingletonMonobehaviour<ChestLootDatabase>
{
    #region Header CHEST CONTENT DETAILS
    [Space(10)]
    [Header("CHEST CONTENT DETAILS")]
    #endregion Header CHEST CONTENT DETAILS
    #region Tooltip
    [Tooltip("The weapons to spawn for each dungeon level and their spawn ratios")]
    #endregion Tooltip
    public List<ChestsBasedOnSpawnableObjectsByLevel<WeaponDetailsSO>> weaponSpawnByChestBasedOnLevelList;

    #region Tooltip
    [Tooltip("The passive items to spawn for each dungeon level and their spawn ratios")]
    #endregion Tooltip
    public List<ChestsBasedOnSpawnableObjectsByLevel<PassiveItemDetailsSO>> passiveItemsSpawnByChestBasedOnLevelList;


    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    public WeaponDetailsSO GetWeaponDetailsToSpawn(int weaponNumber, WartheonRNG rng, int selectedChestIndex)
    {
        if (weaponNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the
        // relative 'ratios' of the items specified
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(new List<SpawnableObjectsByLevel<WeaponDetailsSO>>());

        weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(weaponSpawnByChestBasedOnLevelList[selectedChestIndex].spawnableObjectByLevelList);
        WeaponDetailsSO weaponDetails = weaponRandom.GetItem(rng);

        return weaponDetails;
    }

    /// <summary>
    /// Get the passive item details to spawn - return null if no item is to be spawned or the player already has the weapon
    /// </summary>
    public PassiveItemDetailsSO GetPassiveItemDetailsToSpawn(int itemNumber, WartheonRNG rng, int selectedChestIndex)
    {
        if (itemNumber == 0) return null;

        // Create an instance of the class used to select a random item from a list based on the
        // relative 'ratios' of the items specified
        RandomSpawnableObject<PassiveItemDetailsSO> passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(new List<SpawnableObjectsByLevel<PassiveItemDetailsSO>>());

        passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(passiveItemsSpawnByChestBasedOnLevelList[selectedChestIndex].spawnableObjectByLevelList);
        PassiveItemDetailsSO passiveItemDetails = passiveItemRandom.GetItem(rng);

        return passiveItemDetails;
    }
}
