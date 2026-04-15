using NUnit.Framework.Constraints;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

public class Counter : MonoBehaviour
{
    [Header("WEAPON LIST")]
    [Space(10)]
    [SerializeField] List<SpawnableObjectsByLevel<WeaponDetailsSO>> vendorWeaponSpawnByLevelList;
    [SerializeField] List<SpawnableObjectsByLevel<WeaponDetailsSO>> blackMarketWeaponSpawnByLevelList;

    [Header("PASSIVE ITEM LIST")]
    [Space(10)]
    [SerializeField] List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> vendorPassiveItemSpawnByLevelList;
    [SerializeField] List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> blackMarketPassiveItemSpawnByLevelList;

    [SerializeField] DropItem firstChestItem;
    [SerializeField] DropItem secondChestItem;
    [SerializeField] DropItem thirdChestItem;

    Player player;
    List<int> gambleValuesList = new List<int>();

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnGambleCompleted += StaticEventHandler_OnGambleCompleted;
        StaticDialogueHandler.OnGambleLost += StaticDialogueHandler_OnGambleLost;
        StaticDialogueHandler.OnGambleWon += StaticDialogueHandler_OnGambleWon;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnGambleCompleted -= StaticEventHandler_OnGambleCompleted;
        StaticDialogueHandler.OnGambleLost -= StaticDialogueHandler_OnGambleLost;
        StaticDialogueHandler.OnGambleWon -= StaticDialogueHandler_OnGambleWon;
    }

    private void Start()
    {
        // Activate price infos for chest items
        firstChestItem.transform.GetChild(3).gameObject.SetActive(true);
        secondChestItem.transform.GetChild(3).gameObject.SetActive(true);
        thirdChestItem.transform.GetChild(3).gameObject.SetActive(true);

        player = GameManager.Instance.GetLocalPlayer();
    }

    /// <summary>
    /// Handle the room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        bool isMultiplayer = roomChangedEventArgs.room == null;

        InstantiatedRoom instantiatedRoom = isMultiplayer ? DungeonRuntime.GetInstantiatedRoom(roomChangedEventArgs.roomNetData.roomId) : roomChangedEventArgs.room.instantiatedRoom;

        int seed = Random.Range(int.MinValue, int.MaxValue);
        WartheonRNG rng = new WartheonRNG(seed);

        // If the room is shop room then start spawning chest items
        if (isMultiplayer)
        {
            IntantiateCounterItems(roomChangedEventArgs.roomNetData.isShopRoom, roomChangedEventArgs.roomNetData.shopRoomGoodsCreated, instantiatedRoom, rng);
            roomChangedEventArgs.roomNetData.shopRoomGoodsCreated = true;
        }
        else
        {
            IntantiateCounterItems(roomChangedEventArgs.room.roomNodeType.isShopRoom, roomChangedEventArgs.room.shopRoomGoodsCreated, instantiatedRoom, rng);
            roomChangedEventArgs.room.shopRoomGoodsCreated = true;
        }
    }

    private void IntantiateCounterItems(bool isShopRoom, bool shopRoomGoodsCreated, InstantiatedRoom ir, WartheonRNG rng)
    {
        if (isShopRoom && !shopRoomGoodsCreated)
        {
           // Get npc type of the shop room
           NpcType npcType = ir.GetComponentInChildren<NPC>().npcType;

            int firstRandomNum = rng.Range(1, 3);
            int secondRandomNum = rng.Range(1, 3);
            int thirdRandomNum = rng.Range(1, 3);

            switch (npcType)
            {
                case NpcType.Vendor:
                    if (firstRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(false, rng), firstChestItem, rng);
                    }
                    else if (firstRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(false, rng), firstChestItem, rng);
                    }

                    if (secondRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(false, rng), secondChestItem, rng);
                    }
                    else if (secondRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(false, rng), secondChestItem, rng);
                    }

                    if (thirdRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(false, rng), thirdChestItem, rng);
                    }
                    else if (thirdRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(false, rng), thirdChestItem, rng);
                    }

                    break;
                case NpcType.BlackMarketSeller:
                    if (firstRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(true, rng), firstChestItem, rng);
                    }
                    else if (firstRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(true, rng), firstChestItem, rng);
                    }

                    if (secondRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(true, rng), secondChestItem, rng);
                    }
                    else if (secondRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(true, rng), secondChestItem, rng);
                    }

                    if (thirdRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(true, rng), thirdChestItem, rng);
                    }
                    else if (thirdRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(true, rng), thirdChestItem, rng);
                    }
                    break;
                case NpcType.Gambler:
                    GambleTransaction(rng);

                    break;
                default:
                    break;
            }
        }
    }

    private void StaticEventHandler_OnGambleCompleted()
    {
        int seed = Random.Range(int.MinValue, int.MaxValue);
        WartheonRNG rng = new WartheonRNG(seed);

        GambleTransaction(rng);
    }

    private void StaticDialogueHandler_OnGambleWon()
    {
        firstChestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + gambleValuesList[0];
        secondChestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + gambleValuesList[1];
        thirdChestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + gambleValuesList[2];
    }

    private void StaticDialogueHandler_OnGambleLost()
    {
        firstChestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + gambleValuesList[0];
        secondChestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + gambleValuesList[1];
        thirdChestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + gambleValuesList[2];
    }

    private void GambleTransaction(WartheonRNG rng)
    {
        gambleValuesList.Clear();

        int variationNumber = rng.Range(1, 3);

        firstChestItem.isGambleDropItem = true;
        secondChestItem.isGambleDropItem = true;
        thirdChestItem.isGambleDropItem = true;

        int gambleValueOne = 0;
        int gambleValueTwo = 0;
        int gambleValueThree = 0;

        if (variationNumber == 1)
        {
            gambleValueOne = -10;
            gambleValueTwo = -10;
            gambleValueThree = 20;
        }
        else if (variationNumber == 2)
        {
            gambleValueOne = -10;
            gambleValueTwo = -40;
            gambleValueThree = 50;
        }

        gambleValuesList.Add(gambleValueOne);
        gambleValuesList.Add(gambleValueTwo);
        gambleValuesList.Add(gambleValueThree);

        // Shuffle the list
        gambleValuesList = gambleValuesList.OrderBy(x => Random.value).ToList();

        RetrieveGambleTableValue(ref firstChestItem, gambleValuesList[0]);
        RetrieveGambleTableValue(ref secondChestItem, gambleValuesList[1]);
        RetrieveGambleTableValue(ref thirdChestItem, gambleValuesList[2]);
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails, DropItem dropItem, WartheonRNG rng)
    {
        if (dropItem == null) return;

        dropItem.hasWeaponDrop = true;

        // Create a weapon instance with rolled modifiers
        Weapon weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails, rng);

        weapon.weaponStats.activePrice = (int)(weaponDetails.price * (1 + player.additinalNPCCostModifier));

        dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, dropItem.transform.position);

        dropItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + weapon.weaponStats.activePrice.ToString();
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(bool isBlackMarket, WartheonRNG rng)
    {
        RandomSpawnableObject<WeaponDetailsSO> weaponRandom;

        if (isBlackMarket)
        {
            weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(blackMarketWeaponSpawnByLevelList);
        }
        else
        {
            weaponRandom = new RandomSpawnableObject<WeaponDetailsSO>(vendorWeaponSpawnByLevelList);
        }

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem(rng);

        return weaponDetails;
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails, DropItem dropItem, WartheonRNG rng)
    {
        if (dropItem == null) return;

        dropItem.hasSecondaryPassiveDrop = true;

        PassiveItem passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng);

        passiveItem.passiveStats.activePrice = (int)(passiveItemDetails.price * (1 + player.additinalNPCCostModifier));
        dropItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, dropItem.transform.position);

        dropItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + passiveItem.passiveStats.activePrice.ToString();
    }

    /// <summary>
    /// Get the passive item details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private PassiveItemDetailsSO GetPassiveItemDetailsToSpawn(bool isBlackMarket, WartheonRNG rng)
    {
        RandomSpawnableObject<PassiveItemDetailsSO> passiveItemRandom;

        if (isBlackMarket)
        {
            passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(blackMarketPassiveItemSpawnByLevelList);
        }
        else
        {
            passiveItemRandom = new RandomSpawnableObject<PassiveItemDetailsSO>(vendorPassiveItemSpawnByLevelList);
        }

        PassiveItemDetailsSO passiveItemDetails = passiveItemRandom.GetItem(rng);

        return passiveItemDetails;
    }

    /// <summary>
    /// Gamble reset
    /// </summary>
    private void RetrieveGambleTableValue(ref DropItem dropItem, int gambleValue)
    {
        if (dropItem == null) return;

        ResetGambleTable(ref dropItem);
        dropItem.gambleValue = gambleValue;
    }

    /// <summary>
    /// Gamble reset
    /// </summary>
    private void ResetGambleTable(ref DropItem dropItem)
    {
        dropItem.gambleValue = 0;
        dropItem.isColliding = false;
        dropItem.animator.runtimeAnimatorController = GameResources.Instance.gambleDiceAnimatorController;
    }
}
