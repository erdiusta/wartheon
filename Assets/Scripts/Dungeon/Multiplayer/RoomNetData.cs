using System;
using UnityEngine;

[System.Serializable]
public struct RoomNetData : IEquatable<RoomNetData>
{
    public string roomId;
    public string templateId;
    public RoomCombatState roomCombatState;
    public Vector2Int lowerBounds;
    public Vector2Int upperBounds;
    public Vector2Int templateLowerBounds;
    public Vector2Int templateUpperBounds;
    public Vector2Int[] spawnPositions;
    public EnemiesByLevelNet[] enemiesByLevel;
    public RoomEnemySpawnParametersNet[] enemySpawnParameters;
    public DoorwayNetData[] doorways;
    public bool isPositioned;
    public bool isShopRoom;
    public bool isCombatRoom;
    public bool isChestRoom;
    public bool isBossRoom;
    public bool isBossFoyer;
    public bool isEntrance;
    public bool isCorridor;
    public bool isCorridorEW;
    public bool isCorridorNS;
    public bool isLit;
    public bool isClearedOfEnemies;
    public bool isPreviouslyVisited;
    public bool shopRoomGoodsCreated;
    public EnemyCategory currentBossCategory;
    public bool bossSpawned;

    public bool Equals(RoomNetData other) => roomId == other.roomId;

    public override bool Equals(object obj)
    {
        return obj is RoomNetData other && Equals(other);
    }
    public override int GetHashCode()
    {
        return roomId != null ? roomId.GetHashCode() : 0;
    }

    public static bool operator ==(RoomNetData left, RoomNetData right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(RoomNetData left, RoomNetData right)
    {
        return !left.Equals(right);
    }

    public bool TryGetEnemySpawnParameters(int dungeonLevelIndex, out RoomEnemySpawnParametersNet parameters)
    {
        for (int i = 0; i < enemySpawnParameters.Length; i++)
        {
            if (enemySpawnParameters[i].dungeonLevelIndex == dungeonLevelIndex)
            {
                parameters = enemySpawnParameters[i];
                return true;
            }
        }

        parameters = default;
        return false;
    }
}
