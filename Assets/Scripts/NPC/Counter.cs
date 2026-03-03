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

        // If the room is shop room then start spawning chest items
        if (isMultiplayer)
        {
            IntantiateCounterItems(roomChangedEventArgs.roomNetData.isShopRoom, roomChangedEventArgs.roomNetData.shopRoomGoodsCreated, instantiatedRoom);
            roomChangedEventArgs.roomNetData.shopRoomGoodsCreated = true;
        }
        else
        {
            IntantiateCounterItems(roomChangedEventArgs.room.roomNodeType.isShopRoom, roomChangedEventArgs.room.shopRoomGoodsCreated, instantiatedRoom);
            roomChangedEventArgs.room.shopRoomGoodsCreated = true;
        }
    }

    private void IntantiateCounterItems(bool isShopRoom, bool shopRoomGoodsCreated, InstantiatedRoom ir)
    {
        if (isShopRoom && !shopRoomGoodsCreated)
        {
           // Get npc type of the shop room
           NpcType npcType = ir.GetComponentInChildren<NPC>().npcType;

            int firstRandomNum = Random.Range(1, 3);
            int secondRandomNum = Random.Range(1, 3);
            int thirdRandomNum = Random.Range(1, 3);

            switch (npcType)
            {
                case NpcType.Vendor:
                    if (firstRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(), firstChestItem);
                    }
                    else if (firstRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(), firstChestItem);
                    }

                    if (secondRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(), secondChestItem);
                    }
                    else if (secondRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(), secondChestItem);
                    }

                    if (thirdRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(), thirdChestItem);
                    }
                    else if (thirdRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(), thirdChestItem);
                    }

                    break;
                case NpcType.BlackMarketSeller:
                    if (firstRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(true), firstChestItem);
                    }
                    else if (firstRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(true), firstChestItem);
                    }

                    if (secondRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(true), secondChestItem);
                    }
                    else if (secondRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(true), secondChestItem);
                    }

                    if (thirdRandomNum == 1)
                    {
                        InstantiateWeaponItem(GetWeaponDetailsToSpawn(true), thirdChestItem);
                    }
                    else if (thirdRandomNum == 2)
                    {
                        InstantiatePassiveItem(GetPassiveItemDetailsToSpawn(true), thirdChestItem);
                    }
                    break;
                case NpcType.Gambler:
                    GambleTransaction();

                    break;
                default:
                    break;
            }
        }
    }

    private void StaticEventHandler_OnGambleCompleted()
    {
        GambleTransaction();
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

    private void GambleTransaction()
    {
        gambleValuesList.Clear();

        int variationNumber = Random.Range(1, 3);

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
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails, DropItem dropItem)
    {
        if (dropItem == null) return;

        dropItem.hasWeaponDrop = true;

        // Create a weapon instance with rolled modifiers
        Weapon weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails);

        weapon.activePrice = (int)(weaponDetails.price * (1 + player.additinalNPCCostModifier));

        dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, dropItem.transform.position);

        dropItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + weapon.activePrice.ToString();
    }

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(bool isBlackMarket = false)
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

        WeaponDetailsSO weaponDetails = weaponRandom.GetItem();

        return weaponDetails;
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails, DropItem dropItem)
    {
        if (dropItem == null) return;

        dropItem.hasSecondaryPassiveDrop = true;

        PassiveItem passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails);

        passiveItem.activePrice = (int)(passiveItemDetails.price * (1 + player.additinalNPCCostModifier));
        dropItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, dropItem.transform.position);

        dropItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + passiveItem.activePrice.ToString();
    }

    /// <summary>
    /// Get the passive item details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private PassiveItemDetailsSO GetPassiveItemDetailsToSpawn(bool isBlackMarket = false)
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

        PassiveItemDetailsSO passiveItemDetails = passiveItemRandom.GetItem();

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
