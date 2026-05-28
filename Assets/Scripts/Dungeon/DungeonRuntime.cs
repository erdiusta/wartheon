using System.Collections.Generic;

public static class DungeonRuntime
{
    public static Dictionary<string, RoomNetData> RoomNetDataDict = new Dictionary<string, RoomNetData>();

    private static readonly Dictionary<string, InstantiatedRoom> roomsById = new Dictionary<string, InstantiatedRoom>();

    public static void MarkRoomVisited(string roomId)
    {
        if (!RoomNetDataDict.TryGetValue(roomId, out var roomNet)) return;

        roomNet.isLit = true;
        RoomNetDataDict[roomId] = roomNet;
    }

    public static void RegisterRoom(string roomId, InstantiatedRoom room)
    {
        roomsById[roomId] = room;
    }

    public static void UnregisterRoom(string roomId, InstantiatedRoom room)
    {
        if (string.IsNullOrEmpty(roomId)) return;

        roomsById.Remove(roomId);
    }

    public static InstantiatedRoom GetInstantiatedRoom(string roomId)
    {
        roomsById.TryGetValue(roomId, out var room);
        return room;
    }

    public static RoomNetData GetRoomNetData(string roomId)
    {
        return RoomNetDataDict[roomId];
    }

    public static RoomNetData ConvertRoomToRoomNetData(Room room)
    {
        List<DoorwayNetData> doorwayData = new();

        foreach (Doorway doorway in room.doorwayList)
        {
            doorwayData.Add(DoorwayNetMapper.ToNetData(doorway));
        }

        RoomNetData data = new RoomNetData
        {
            roomId = room.id,
            templateId = room.templateID,
            lowerBounds = room.lowerBounds,
            upperBounds = room.upperBounds,
            templateLowerBounds = room.templateLowerBounds,
            templateUpperBounds = room.templateUpperBounds,
            spawnPositions = room.spawnPositionArray,
            doorways = doorwayData.ToArray(),
            isPositioned = room.isPositioned,
            isShopRoom = room.roomNodeType.isShopRoom,
            isChestRoom = room.roomNodeType.isChestRoom,
            isCombatRoom = room.roomNodeType.isCombatRoom,
            isEntrance = room.roomNodeType.isEntrance,
            isBossFoyer = room.roomNodeType.isBossFoyer,
            isBossRoom = room.roomNodeType.isBossRoom,
            isCorridor = room.roomNodeType.isCorridor,
            isCorridorEW = room.roomNodeType.isCorridorEW,
            isCorridorNS = room.roomNodeType.isCorridorNS,
            isLit = room.isLit,
            shopRoomGoodsCreated = room.shopRoomGoodsCreated,
        };

        if (room.roomNodeType.isCorridor || room.roomNodeType.isChestRoom || room.roomNodeType.isShopRoom || room.roomNodeType.isEntrance || room.roomNodeType.isBossFoyer) data.roomCombatState = RoomCombatState.Cleared;
        else data.roomCombatState = RoomCombatState.Idle;

        data.enemiesByLevel = ConvertEnemiesByLevel(room.enemiesByLevelList);
        data.enemySpawnParameters = ConvertEnemySpawnParameters(room.roomLevelEnemySpawnParametersList);

        return data;
    }

    public static EnemiesByLevelNet[] ConvertEnemiesByLevel(List<SpawnableObjectsByLevel<EnemyDetailsSO>> source)
    {
        if (source == null || source.Count == 0) return System.Array.Empty<EnemiesByLevelNet>();

        List<EnemiesByLevelNet> result = new();

        foreach (var byLevel in source)
        {
            List<EnemySpawnRatioNet> ratios = new();

            foreach (var ratio in byLevel.spawnableObjectRatioList)
            {
                ratios.Add(new EnemySpawnRatioNet
                {
                    enemyCategory = ratio.dungeonObject.enemyCategory,
                    ratio = ratio.ratio
                });
            }

            result.Add(new EnemiesByLevelNet
            {
                dungeonlevelIndex = byLevel.dungeonLevel.levelNumber,
                enemyRatios = ratios.ToArray()
            });
        }

        return result.ToArray();
    }

    public static RoomEnemySpawnParametersNet[] ConvertEnemySpawnParameters(List<RoomEnemySpawnParameters> source)
    {
        if (source == null || source.Count == 0) return System.Array.Empty<RoomEnemySpawnParametersNet>();

        List<RoomEnemySpawnParametersNet> result = new();

        foreach (var param in source)
        {
            result.Add(new RoomEnemySpawnParametersNet
            {
                dungeonLevelIndex = param.dungeonLevel.levelNumber,
                minTotalEnemiesToSpawn = param.minTotalEnemiesToSpawn,
                maxTotalEnemiesToSpawn = param.maxTotalEnemiesToSpawn,
                minConcurrentEnemies = param.minConcurrentEnemies,
                maxConcurrentEnemies = param.maxConcurrentEnemies,
                minSpawnInterval = param.minSpawnInterval,
                maxSpawnInterval = param.maxSpawnInterval,
            });
        }

        return result.ToArray();
    }

    public static bool HasRooms() => roomsById.Count > 0;
}
