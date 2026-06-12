using Mirror;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonNetworkController : NetworkBehaviour
{
    public static DungeonNetworkController Instance { get; private set; }

    public readonly HashSet<string> processedRooms = new();

    [SyncVar(hook = nameof(OnBossNetIdChanged))] public uint activeBossNetId;

    public static uint CachedBossNetId;

    private void Awake()
    {
        if (Instance != null) return; 

        Instance = this;
    }

    private void OnBossNetIdChanged(uint oldNetId, uint newNetId)
    {
        if (isServer) return;

        CachedBossNetId = newNetId;
    }

    [Server]
    public void ServerRoomEntered(string roomId, NetworkIdentity initiator, Vector2 entryDirection)
    {
        DungeonRuntime.MarkRoomVisited(roomId);

        InstantiatedRoom ir = DungeonRuntime.GetInstantiatedRoom(roomId);

        if (ir == null) return;

        RoomNetworkRoot roomRoot = ir.GetComponentInParent<RoomNetworkRoot>();
        RoomNetData data = roomRoot.roomNetData;
     
        GameSessionManager.Instance.SetCurrentRoom(null, roomRoot.roomNetData);

        if(roomRoot.roomNetData.isCombatRoom && roomRoot.roomNetData.roomCombatState == RoomCombatState.Cleared) return;

        if (roomRoot.roomNetData.isCombatRoom && roomRoot.roomNetData.roomCombatState == RoomCombatState.Idle)
        {
            if (!processedRooms.Add(roomId)) return;

            data.roomCombatState = RoomCombatState.Engaged;
            roomRoot.roomNetData.roomCombatState = RoomCombatState.Engaged;
            roomRoot.roomNetData = data;

            ir.enemySpawner.RoomChangedMP(roomRoot.roomNetData);
        }

        if (!roomRoot.roomNetData.isEntrance && roomRoot.roomNetData.roomCombatState == RoomCombatState.Engaged && !roomRoot.roomNetData.isPreviouslyVisited)
        {
            StartCoroutine(TeleportAllPlayersNearInitiatorsRoutine(roomRoot.roomNetData, initiator, entryDirection));
        }

        if (roomRoot.roomNetData.isShopRoom)
        {
            WartheonRNG rng = new WartheonRNG(Random.Range(int.MinValue, int.MaxValue));
            Counter counter = ir.GetComponentInChildren<Counter>();

            NpcType npcType = ir.GetComponentInChildren<NPC>().npcType;

            if (counter != null)
            {
                if (npcType == NpcType.Gambler) RpcSpawnForClient(roomRoot.roomNetData, initiator);
                else
                {
                    if (!roomRoot.roomNetData.isPreviouslyVisited)
                    {
                        counter.SpawnItems(ir, rng, true);
                    }
                }
            }
        }

        data.isPreviouslyVisited = true;
        roomRoot.roomNetData.isPreviouslyVisited = true;
        roomRoot.roomNetData = data;

        RpcRoomChanged(roomId, roomRoot.roomNetData);

        // Lock interactions
        if (roomRoot.roomNetData.roomCombatState == RoomCombatState.Engaged) ir.LockDoors();
        else ir.UnlockDoors(0f);

        RpcLockDoors(roomRoot.roomNetData);
    }

    [ClientRpc]
    private void RpcRoomChanged(string roomId, RoomNetData roomNetData)
    {
        if (!isServer)
        {
            if (!processedRooms.Add(roomId)) return;
        }

        StaticEventHandler.CallRoomChangedEvent(null, roomNetData);
    }

    [ClientRpc]
    private void RpcLockDoors(RoomNetData roomNetData)
    {
        if (isServer) return;

        InstantiatedRoom ir = DungeonRuntime.GetInstantiatedRoom(roomNetData.roomId);

        if (roomNetData.roomCombatState == RoomCombatState.Engaged) ir.LockDoors();
        else ir.UnlockDoors(0f);
    }

    [ClientRpc]
    void RpcSpawnForClient(RoomNetData roomNetData, NetworkIdentity initiator)
    {
        if(!initiator.isLocalPlayer) return;

        InstantiatedRoom ir = DungeonRuntime.GetInstantiatedRoom(roomNetData.roomId);

        if (ir == null) return;

        if (roomNetData.isShopRoom && !roomNetData.shopRoomGoodsCreated)
        {
            roomNetData.shopRoomGoodsCreated = true;

            WartheonRNG rng = new WartheonRNG(Random.Range(int.MinValue, int.MaxValue));
            Counter counter = ir.GetComponentInChildren<Counter>();

            if (counter != null) counter.SpawnItems(ir, rng, true);
        }
    }

    [Server]
    IEnumerator TeleportAllPlayersNearInitiatorsRoutine(RoomNetData roomNetData, NetworkIdentity initiator, Vector2 entryDirection)
    {
        yield return new WaitForSeconds(1f);

        TeleportAllPlayersNearInitiator(roomNetData, initiator, entryDirection);
    }

    [Server] 
    private void TeleportAllPlayersNearInitiator(RoomNetData roomNetData, NetworkIdentity initiator, Vector2 entryDirection)
    {
        if (initiator == null) return;

        Vector3 anchorPos = initiator.transform.position;

        // We teleport players behind the initiator
        Vector3 backDirection = -new Vector3(entryDirection.x, entryDirection.y, 0f).normalized;

        const float baseDistance = 0.5f;
        const float sideSpacing = 1.5f;

        int sideIndex = 0;

        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null || conn.identity == initiator) continue;

            PlayerNetworkAuthority player = conn.identity.GetComponent<PlayerNetworkAuthority>();
            if (player == null) continue;

            // Zig-zag sideways (Left / Right)
            Vector3 sideDir = Vector3.Cross(backDirection, Vector3.back).normalized;
            Vector3 lateralOffset = sideDir * sideSpacing * ((sideIndex % 2 == 0) ? 1 : -1);
            sideIndex++;

            Vector3 targetPos = anchorPos + backDirection * baseDistance + lateralOffset;

            player.RpcTeleportTo(targetPos, roomNetData, initiator.netId);
        }
    }

    [Server]
    public void SpawnDungeon(int dungeonLevelIndex)
    {
        foreach (RoomNetData data in DungeonRuntime.RoomNetDataDict.Values)
        {
            Vector3 roomPosition = new Vector3(data.lowerBounds.x - data.templateLowerBounds.x, data.lowerBounds.y - data.templateLowerBounds.y, 0f);

            var rootPrefab = NetworkManager.singleton.spawnPrefabs.First(p => p.name == "RoomNetworkRoot");
            GameObject rootObject = Instantiate(rootPrefab, roomPosition, Quaternion.identity);

            RoomNetworkRoot root = rootObject.GetComponent<RoomNetworkRoot>();

            if (data.isEntrance) GameSessionManager.Instance.SetCurrentRoom(null, data);

            root.roomNetData = data;
            root.dungeonGenerationId = dungeonLevelIndex;
            root.randomNum = Random.Range(1, 101);

            NetworkServer.Spawn(rootObject);
        }
    }
}
