using Mirror;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class DungeonNetworkController : NetworkBehaviour
{
    public static DungeonNetworkController Instance { get; private set; }

    private readonly HashSet<string> processedRooms = new();

    private void Awake()
    {
        if (Instance != null) return; 

        Instance = this;
    }

    [Server]
    public void ServerRoomEntered(string roomId, NetworkIdentity initiator, Vector2 entryDirection)
    {
        // Hard Guard
        if (processedRooms.Contains(roomId)) return;
        processedRooms.Add(roomId);

        DungeonRuntime.MarkRoomVisited(roomId);
        RoomNetData roomNetData = DungeonRuntime.GetRoomNetData(roomId);

        RpcRoomChanged(roomNetData);

        if (roomNetData.roomCombatState == RoomCombatState.Idle) TeleportAllPlayersNearInitiator(roomNetData, initiator, entryDirection);
    }

    [ClientRpc]
    private void RpcRoomChanged(RoomNetData roomNetData)
    {
        StaticEventHandler.CallRoomChangedEvent(null, roomNetData);
    }

    [Server] private void TeleportAllPlayersNearInitiator(RoomNetData roomNetData, NetworkIdentity initiator, Vector2 entryDirection)
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

            player.RpcTeleportTo(targetPos);
        }
    }

    [Server]
    public void SpawnDungeon()
    {
        foreach (RoomNetData data in DungeonRuntime.RoomNetDataDict.Values)
        {
            Vector3 roomPosition = new Vector3(data.lowerBounds.x - data.templateLowerBounds.x, data.lowerBounds.y - data.templateLowerBounds.y, 0f);

            var rootPrefab = NetworkManager.singleton.spawnPrefabs.First(p => p.name == "RoomNetworkRoot");
            GameObject rootObject = Instantiate(rootPrefab, roomPosition, Quaternion.identity);

            RoomNetworkRoot root = rootObject.GetComponent<RoomNetworkRoot>();
            root.templateId = data.templateId;
            root.roomNetData = data;
            root.randomNum = Random.Range(1, 101);

            NetworkServer.Spawn(rootObject);

            if (data.isEntrance) StaticEventHandler.CallRoomChangedEvent(null, data);
        }
    }
}
