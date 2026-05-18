using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Counter : NetworkBehaviour
{
    [Header("WEAPON LIST")]
    [Space(10)]
    [SerializeField] List<SpawnableObjectsByLevel<WeaponDetailsSO>> vendorWeaponSpawnByLevelList;
    [SerializeField] List<SpawnableObjectsByLevel<WeaponDetailsSO>> blackMarketWeaponSpawnByLevelList;

    [Header("PASSIVE ITEM LIST")]
    [Space(10)]
    [SerializeField] List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> vendorPassiveItemSpawnByLevelList;
    [SerializeField] List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> blackMarketPassiveItemSpawnByLevelList;

    [Header("SPAWN POINTS")]
    [SerializeField] Transform spawn1;
    [SerializeField] Transform spawn2;
    [SerializeField] Transform spawn3;

    Transform[] spawnPoints;

    Player player;

    List<DropItem> spawnedGambleItems_SP = new List<DropItem>();
    List<DropItemNetwork> spawnedGambleItems_MP = new List<DropItemNetwork>();
    List<int> currentGambleValues = new List<int>();

    Room belongingRoom;
    RoomNetData belongingRoomNetData;

    [HideInInspector] public bool isGambleLocked;

    private void Awake()
    {
        spawnPoints = new[] { spawn1, spawn2, spawn3 };
    }

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
        player = GameManager.Instance.GetLocalPlayer();
    }

    // ROOM ENTRY
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs args)
    {
        bool isMultiplayer = args.room == null;

        belongingRoom = args.room;
        belongingRoomNetData = args.roomNetData;

        InstantiatedRoom ir = isMultiplayer ? DungeonRuntime.GetInstantiatedRoom(args.roomNetData.roomId) : args.room.instantiatedRoom;
        if (ir == null) return;

        // If the room is shop room then start spawning chest items
        if (!isMultiplayer)
        {
            HandleRoom_SP(args, ir);
        }
    }

    private void HandleRoom_SP(RoomChangedEventArgs args, InstantiatedRoom ir)
    {
        if (!args.room.roomNodeType.isShopRoom || args.room.shopRoomGoodsCreated) return;

        WartheonRNG rng = new WartheonRNG(Random.Range(int.MinValue, int.MaxValue));
        SpawnItems(ir, rng, false);
        args.room.shopRoomGoodsCreated = true;
    }

    // SPAWN CORE
    public void SpawnItems(InstantiatedRoom ir, WartheonRNG rng, bool isMultiplayer)
    {
        NpcType npcType = ir.GetComponentInChildren<NPC>().npcType;

        for (int i = 0; i < spawnPoints.Length; i++)
        {
            int roll = rng.Range(1, 3);

            switch (npcType)
            {
                case NpcType.Vendor:
                    SpawnVendorItem(i, roll, false, rng, isMultiplayer);
                    break;
                case NpcType.BlackMarketSeller:
                    SpawnVendorItem(i, roll, true, rng, isMultiplayer);
                    break;
                case NpcType.Gambler:
                    if (i != 0) continue;
                    SpawnGambleSet(rng, isMultiplayer);
                    break;
                default:
                    break;
            }
        }
    }

    // SPAWN HELPERS
    private GameObject SpawnObject(int index, bool isMultiplayer, bool isGamble, out Vector3 spawnPosition)
    {
        Transform point = spawnPoints[index];
        spawnPosition = point.position;

        GameObject prefab = isMultiplayer && !isGamble ? GameResources.Instance.chestItemNetworkPrefab : GameResources.Instance.chestItemPrefab;

        GameObject obj = Instantiate(prefab, point.position, point.rotation);
        if (isMultiplayer && !isGamble) NetworkServer.Spawn(obj);

        return obj;
    }

    private void SpawnVendorItem(int index, int roll, bool isBlackMarket, WartheonRNG rng, bool isMultiplayer)
    {
        if (roll == 1) SpawnWeapon(index, GetWeaponDetailsToSpawn(isBlackMarket, rng), rng, isMultiplayer);
        else SpawnPassive(index, GetPassiveItemDetailsToSpawn(isBlackMarket, rng), rng, isMultiplayer);
    }

    private void SpawnWeapon(int index, WeaponDetailsSO details, WartheonRNG rng, bool isMultiplayer)
    {
        GameObject obj = SpawnObject(index, isMultiplayer, isGamble: false, out Vector3 spawnPosition);

        Weapon weapon = WeaponDropGenerator.CreateRolledInstance(details, rng);
        weapon.weaponStats.activePrice = (int)(details.price * (1 + player.additionalNPCCostModifier));

        if (isMultiplayer)
        {
            DropItemNetwork dropItemNetwork = obj.GetComponent<DropItemNetwork>();

            dropItemNetwork.currentLocation = DropItemLocation.Counter;
            dropItemNetwork.hasWeaponDrop = true;

            dropItemNetwork.weaponTitle = weapon.weaponStats.weaponTitle;
            dropItemNetwork.weaponClass = weapon.weaponStats.weaponClass;

            dropItemNetwork.weaponStats = weapon.weaponStats;

            dropItemNetwork.dropCompleted = true;

            StartCoroutine(InitializeRoutine(dropItemNetwork));
        }
        else
        {
            var drop = obj.GetComponent<DropItem>();
            obj.GetComponent<DropItem>().dropCompleted = true;
            drop.hasWeaponDrop = true;
            drop.Initialize(weapon, details.weaponFrontSprite, spawnPosition, this);
        }
    }

    private void SpawnPassive(int index, PassiveItemDetailsSO details, WartheonRNG rng, bool isMultiplayer)
    {
        GameObject obj = SpawnObject(index, isMultiplayer, isGamble: false, out Vector3 spawnPosition);

        PassiveItem item = PassiveDropGenerator.CreateRolledInstance(details, rng);
        item.passiveStats.activePrice = (int)(details.price * (1 + player.additionalNPCCostModifier));
        item.passiveStats.passiveItemType = details.passiveItemType;

        if (isMultiplayer)
        {
            DropItemNetwork dropItemNetwork = obj.GetComponent<DropItemNetwork>();

            if (details.passiveItemCategory == PassiveItemCategory.Primary) dropItemNetwork.hasPrimaryPassiveDrop = true;
            else if(details.passiveItemCategory == PassiveItemCategory.Secondary) dropItemNetwork.hasSecondaryPassiveDrop = true;

            dropItemNetwork.currentLocation = DropItemLocation.Counter;
            dropItemNetwork.passiveItemType = item.passiveStats.passiveItemType;
            dropItemNetwork.passiveItemSlotName = item.passiveStats.passiveItemSlotName;

            dropItemNetwork.passiveStats = item.passiveStats;

            dropItemNetwork.dropCompleted = true;

            StartCoroutine(InitializeRoutine(dropItemNetwork));
        }
        else
        {
            var drop = obj.GetComponent<DropItem>();
            obj.GetComponent<DropItem>().dropCompleted = true;
            drop.hasSecondaryPassiveDrop = true;
            drop.Initialize(item, details.passiveItemSprite, spawnPosition, this);
        }
    }

    IEnumerator InitializeRoutine(DropItemNetwork drop)
    {
        yield return null;

        drop.canInitialize = true;
    }

    // GAMBLE
    private void StaticEventHandler_OnGambleCompleted(GambleArgs args)
    {
        StartCoroutine(GambleResetRoutine());
    }

    IEnumerator GambleResetRoutine()
    {
        isGambleLocked = true;

        yield return new WaitForSeconds(3.5f);

        WartheonRNG rng = new WartheonRNG(Random.Range(int.MinValue, int.MaxValue));
        UpdateGambleSet(rng);

        isGambleLocked = false;
    }

    private void UpdateGambleSet(WartheonRNG rng)
    {
        currentGambleValues = GenerateGambleValues(rng);

        for (int i = 0; i < spawnedGambleItems_SP.Count; i++)
        {
            DropItem drop = spawnedGambleItems_SP[i];
            if (drop == null) continue;

            drop.gambleValue = currentGambleValues[i];

            // Reset to hidden state every method
            var text = drop.priceContainer.GetChild(1).GetComponent<TextMeshPro>();
            text.text = "x -10";
        }
    }

    private void SpawnGambleSet(WartheonRNG rng, bool isMultiplayer)
    {
        currentGambleValues = GenerateGambleValues(rng);

        // Clear previous
        spawnedGambleItems_SP.Clear();

        for (int i = 0; i < 3; i++)
        {
            GameObject obj = SpawnObject(i, isMultiplayer, isGamble: true, out Vector3 spawnPosition);

            var drop = obj.GetComponent<DropItem>();

            drop.isGambleDropItem = true;
            drop.gambleValue = currentGambleValues[i];

            drop.priceContainer.gameObject.SetActive(true);

            drop.InitializeGamble(this, drop);

            // Default hidden until result
            drop.priceContainer.GetChild(1).GetComponent<TextMeshPro>().text = "x -10";

            spawnedGambleItems_SP.Add(drop);
        }
    }

    List<int> GenerateGambleValues(WartheonRNG rng)
    {
        List<int> values = new List<int>();

        if (rng.Range(1, 3) == 1) values.AddRange(new[] { -10, -10, 20 });
        else values.AddRange(new[] { -10, -40, 50 });

        // Fisher-Yates Shuffle
        for (int i = values.Count - 1; i > 0; i--)
        {
            int j = rng.Range(0, i + 1);
            (values[i], values[j]) = (values[j], values[i]);
        }

        return values;
    }

    #region Gamble Win&Lost
    private void StaticDialogueHandler_OnGambleWon()
    {
        RevealGambleValues();
    }

    private void StaticDialogueHandler_OnGambleLost()
    {
        RevealGambleValues();
    }

    void RevealGambleValues()
    {
        // SP
        for (int i = 0; i < spawnedGambleItems_SP.Count; i++)
        {
            var drop = spawnedGambleItems_SP[i];
            if (drop == null) continue;

            drop.priceContainer.GetChild(1).GetComponent<TextMeshPro>().text = "x " + currentGambleValues[i];
        }

        // MP
        if (player != null && player.IsLocal)
        {
            for (int i = 0; i < spawnedGambleItems_MP.Count; i++)
            {
                var net = spawnedGambleItems_MP[i];
                if (net == null) continue;

                net.priceContainer.GetChild(1).GetComponent<TextMeshPro>().text = "x " + currentGambleValues[i];
            }
        }
    }
    #endregion

    /// <summary>
    /// Get the weapon details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private WeaponDetailsSO GetWeaponDetailsToSpawn(bool isBlackMarket, WartheonRNG rng)
    {
        var pool = isBlackMarket ? new RandomSpawnableObject<WeaponDetailsSO>(blackMarketWeaponSpawnByLevelList) : new RandomSpawnableObject<WeaponDetailsSO>(vendorWeaponSpawnByLevelList);

        return pool.GetItem(rng);
    }

    /// <summary>
    /// Get the passive item details to spawn - return null if no weapon is to be spawned or the player already has the weapon
    /// </summary>
    private PassiveItemDetailsSO GetPassiveItemDetailsToSpawn(bool isBlackMarket, WartheonRNG rng)
    {
        var pool = isBlackMarket ? new RandomSpawnableObject<PassiveItemDetailsSO>(blackMarketPassiveItemSpawnByLevelList) : new RandomSpawnableObject<PassiveItemDetailsSO>(vendorPassiveItemSpawnByLevelList);

        return pool.GetItem(rng);
    }
}
