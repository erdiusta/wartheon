[System.Serializable]
public struct RoomEnemySpawnParametersNet
{
    public int dungeonLevelIndex;

    public int minTotalEnemiesToSpawn;
    public int maxTotalEnemiesToSpawn;

    public int minConcurrentEnemies;
    public int maxConcurrentEnemies;

    public float minSpawnInterval;
    public float maxSpawnInterval;
}
