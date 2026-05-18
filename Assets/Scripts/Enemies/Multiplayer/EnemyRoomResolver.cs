using Mirror;
using UnityEngine;

public static class EnemyRoomResolver
{
    public static bool TryGetRoom(out InstantiatedRoom instantiatedRoom, out Vector2Int[] spawnPositions)
    {
        instantiatedRoom = null;
        spawnPositions = null;

        if (NetworkServer.active)
        {
            var roomNetData = GameSessionManager.Instance.GetCurrentRoomNetData();

            if (roomNetData.roomId != null) return false;

            instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(roomNetData.roomId);
            spawnPositions = roomNetData.spawnPositions;

            return instantiatedRoom != null;
        }
        else
        {
            // SP PATH
            Room room = GameManager.Instance.GetCurrentRoom();

            if (room == null || room.instantiatedRoom == null) return false;

            instantiatedRoom = room.instantiatedRoom;
            spawnPositions = room.spawnPositionArray;

            return true;
        }
    }
}
