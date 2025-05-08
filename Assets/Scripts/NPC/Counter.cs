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

    [SerializeField] ChestItem firstChestItem;
    [SerializeField] ChestItem secondChestItem;
    [SerializeField] ChestItem thirdChestItem;

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

        player = GameManager.Instance.GetPlayer();
    }

    /// <summary>
    /// Handle the room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // If the room is shop room then start spawning chest items
        if (roomChangedEventArgs.room.roomNodeType.isShopRoom && !roomChangedEventArgs.room.shopRoomGoodsCreated)
        {
            // Get npc type of the shop room
            NpcType npcType = roomChangedEventArgs.room.instantiatedRoom.GetComponentInChildren<NPC>().npcType;

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

            roomChangedEventArgs.room.shopRoomGoodsCreated = true;
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

        firstChestItem.isGambleChestItem = true;
        secondChestItem.isGambleChestItem = true;
        thirdChestItem.isGambleChestItem = true;

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
    private void InstantiateWeaponItem(WeaponDetailsSO weaponDetails, ChestItem chestItem)
    {
        if (chestItem == null) return;

        chestItem.hasWeaponDrop = true;
        Weapon weapon = new Weapon();
        weapon.weaponDetails = weaponDetails;
        weapon.activePrice = (int)(weaponDetails.price * (1 + player.additinalNPCCostModifier));

        chestItem.Initialize(weapon, weaponDetails.weaponFrontSprite, chestItem.transform.position);

        if (weaponDetails.weaponClass == WeaponClass.Bow)
        {
            chestItem.transform.localPosition += new Vector3(0f, 1f, 0f);
        }

        chestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + weapon.activePrice.ToString();
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
    private void InstantiatePassiveItem(PassiveItemDetailsSO passiveItemDetails, ChestItem chestItem)
    {
        if (chestItem == null) return;

        chestItem.hasSecondaryPassiveDrop = true;
        PassiveItem passiveItem = new PassiveItem();
        passiveItem.passiveItemDetails = passiveItemDetails;
        passiveItem.activePrice = (int)(passiveItemDetails.price * (1 + player.additinalNPCCostModifier));

        chestItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, chestItem.transform.position);

        chestItem.transform.GetChild(3).GetComponentInChildren<TextMeshPro>().text = "x " + passiveItem.activePrice.ToString();
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
    private void RetrieveGambleTableValue(ref ChestItem chestItem, int gambleValue)
    {
        if (chestItem == null) return;

        ResetGambleTable(ref chestItem);
        chestItem.gambleValue = gambleValue;
    }

    /// <summary>
    /// Gamble reset
    /// </summary>
    private void ResetGambleTable(ref ChestItem chestItem)
    {
        chestItem.gambleValue = 0;
        chestItem.isColliding = false;
        chestItem.animator.runtimeAnimatorController = GameResources.Instance.gambleDiceAnimatorController;
    }
}

public class CounterChestItem
{
    public ChestItem chestItem;
    public int counterChestItemIndexNumber;
    public int indexGambleValue;
}
