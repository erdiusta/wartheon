using Mirror;
using System.Collections;
using UnityEngine;

public class EnemySpawnerNetwork : SingletonNetworkBehaviour<EnemySpawnerNetwork>
{
    [HideInInspector] public Enemy bossEnemy;
    [HideInInspector] public bool isBossInstantiated;

    int enemiesToSpawn;
    int currentEnemyCount;
    int enemiesSpawnedSoFar;
    int enemyMaxConcurrentSpawnNumber;
    int spawnPositionIndex = 0;
    RoomNetData currentRoomNetData;
    RoomEnemySpawnParametersNet roomEnemySpawnParameters;

    public override void OnStartServer()
    {
        base.OnStartServer();

        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChangedServer;
        StaticEventHandler.OnEnemyKilled -= StaticEventHandler_OnEnemyKilledServer;

        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChangedServer;
        StaticEventHandler.OnEnemyKilled += StaticEventHandler_OnEnemyKilledServer;
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChangedServer;
        StaticEventHandler.OnEnemyKilled -= StaticEventHandler_OnEnemyKilledServer;
    }

    /// <summary>
    /// Process a change in room
    /// </summary>
    private void StaticEventHandler_OnRoomChangedServer(RoomChangedEventArgs args)
    {
        //// If player is null add it to dynamicGameObjectsInScene
        //if (player == null)
        //{
        //    player = GameManager.Instance.GetPlayer();
        //    SceneObjectsManager.dynamicGameObjectsInScene.Add(player.gameObject);
        //}

        currentRoomNetData = args.roomNetData;

        // If room is cleared, don't go further.
        if (!CanRoomSpawnEnemies(currentRoomNetData)) return;

        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;
        spawnPositionIndex = 0;

        //// Update music for room
        //MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

        // Tutorial check - Lock door for a while
        if (InputManager.TutorialEnabled && currentRoomNetData.isEntrance) goto tutorialEntranceRoomCheck;

        // If the room is a corridor or the entrance then return
        if (currentRoomNetData.isCorridorEW || currentRoomNetData.isCorridorNS || currentRoomNetData.isEntrance) return;

        // If the room has already been defeated then return
        if (currentRoomNetData.isClearedOfEnemies) return;

        int dungeonLevelIndex = GameSessionManager.Instance.selectedDungeonLevelIndex + 1;

        // Get random number of enemies to spawn
        if (!currentRoomNetData.TryGetEnemySpawnParameters(dungeonLevelIndex, out roomEnemySpawnParameters))
        {
            // No spawn parameters - room is auto-cleared
            currentRoomNetData.isClearedOfEnemies = true;
            return;
        }

        // Server-Only randomization
        enemiesToSpawn = Random.Range(roomEnemySpawnParameters.minTotalEnemiesToSpawn, roomEnemySpawnParameters.maxTotalEnemiesToSpawn + 1);

        // Get concurrent number of enemies to spawn
        enemyMaxConcurrentSpawnNumber = Random.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies + 1);

    //// Update music for room
    //MusicManager.Instance.PlayMusic(currentRoom.battleMusic, 0.2f, 0.5f);

    tutorialEntranceRoomCheck:

        // Lock doors
        InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
        instantiatedRoom.LockDoors();

        // Spawn enemies
        SpawnEnemies(args.roomNetData);
    }

    [Server]
    private bool CanRoomSpawnEnemies(RoomNetData roomNetData)
    {
        if(roomNetData.roomCombatState != RoomCombatState.Idle) Debug.LogWarning("ROOM COMBAT STATE IS NOT IDLE, SO CAN'T SPAWN ENEMIES!");

        if (roomNetData.roomCombatState != RoomCombatState.Idle) return false;

        roomNetData.roomCombatState = RoomCombatState.CombatActive;
        return true;
    }

    private void StaticEventHandler_OnEnemyKilledServer(EnemyKilledArgs enemyKilledArgs)
    {
        if (enemyKilledArgs.enemy.enemyDetails.enemyCategory == EnemyCategory.MainSlime)
        {
            InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
            Grid grid = instantiatedRoom.grid;

            EnemyCategory minionCategory = enemyKilledArgs.enemy.enemyDetails.enemyCategory;
            EnemyDetailsSO minionDetails = EnemyDatabase.Instance.GetEnemy(minionCategory);

            // Create three minions from the dead main slime
            CreateEnemy(minionDetails, enemyKilledArgs.enemy.transform.position + new Vector3(1f, 0f, 0f));
            CreateEnemy(minionDetails, enemyKilledArgs.enemy.transform.position + new Vector3(-1, 0f, 0f));
            CreateEnemy(minionDetails, enemyKilledArgs.enemy.transform.position);
        }
    }

    [Server]
    private void SpawnEnemies(RoomNetData roomNetData)
    {
        // Set gamestate engaging enemies
        if (GameSessionManager.Instance.gameState == GameState.playingLevel)
        {
            if (roomNetData.isBossRoom)
            {
                GameSessionManager.Instance.previousGameState = GameState.playingLevel;
                GameSessionManager.Instance.gameState = GameState.engagingBoss;
            }
            else
            {
                GameSessionManager.Instance.previousGameState = GameState.playingLevel;
                GameSessionManager.Instance.gameState = GameState.engagingEnemies;
            }
        }

        StartCoroutine(SpawnEnemiesRoutine());
    }

    /// <summary>
    /// Spawn the enemies coroutine
    /// </summary>
    public IEnumerator SpawnEnemiesRoutine()
    {
        InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
        Grid grid = instantiatedRoom.grid;

        // Create an instance of the helper class used to select a random enemy
        RandomSpawnableObjectNet randomEnemySelector = new RandomSpawnableObjectNet(currentRoomNetData.enemiesByLevel, GameSessionManager.Instance.selectedDungeonLevelIndex + 1);

        if (currentRoomNetData.spawnPositions.Length == 0) yield break;

        // Create Enemy - Get next enemy type to spawn 
        if (InputManager.TutorialEnabled)
        {
            Vector3Int cellPosition = (Vector3Int)currentRoomNetData.spawnPositions[spawnPositionIndex++];
            spawnPositionIndex %= currentRoomNetData.spawnPositions.Length;

            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Combat)
            {
                CreateEnemy(EnemyDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory()), grid.CellToWorld(cellPosition));
                yield break;
            }

            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry)
            {
                CreateEnemy(EnemyDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory()), grid.CellToWorld(cellPosition));
                yield break;
            }

            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.DodgeRoll)
            {
                CreateEnemy(EnemyDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory()), grid.CellToWorld(cellPosition - new Vector3Int(4, 4, 0)));
                yield break;
            }
        }

        // Loop through to create all the enemeies
        for (int i = 0; i < enemiesToSpawn; i++)
        {
            // Wait until current enemy count is less than max concurrent enemies
            while (currentEnemyCount >= enemyMaxConcurrentSpawnNumber)
            {
                yield return null;
            }

            Vector3Int cellPosition;

            EnemyDetailsSO enemyDetails = EnemyDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory());

            if (enemyDetails.isEnemyBoss)
            {
                cellPosition = (Vector3Int)currentRoomNetData.spawnPositions[0];
            }
            else
            {
                cellPosition = (Vector3Int)currentRoomNetData.spawnPositions[spawnPositionIndex++];
                spawnPositionIndex %= currentRoomNetData.spawnPositions.Length;
            }

            CreateEnemy(enemyDetails, grid.CellToWorld(cellPosition));

            yield return new WaitForSeconds(Random.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval));
        }

        currentRoomNetData.isRoomReady = true;
    }

    /// <summary>
    /// Create an enemy in the specified position
    /// </summary>
    public void CreateEnemy(EnemyDetailsSO enemyDetails, Vector3 position)
    {
        // Keep track of the number of enemies spawned so far
        if (!enemyDetails.isSummonedMinion)
        {
            enemiesSpawnedSoFar++;
        }

        // Add one to the current enemy count - this is reduced when an enemy is destroyed
        currentEnemyCount++;

        // Get current dungeon level
        DungeonLevelSO dungeonLevel = GameSessionManager.Instance.GetCurrentDungeonLevel();
        
        GameObject enemy = InstantiateEnemyPrefab(enemyDetails.enemyCategory, position);
        InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);

        NetworkServer.Spawn(enemy);

        // Initialize Enemy
        enemy.GetComponent<EnemyNetwork>().ServerInitialize(enemyDetails, enemiesSpawnedSoFar, dungeonLevel);
        enemy.GetComponent<EnemyNetwork>().InitializeRoom(instantiatedRoom, currentRoomNetData.spawnPositions);

        // Set boss 
        if (currentRoomNetData.isBossRoom)
        {
            isBossInstantiated = true;
            if (enemy.GetComponent<Enemy>().enemyDetails.isEnemyBoss)
            {
                SetEnemyAsBoss(enemy.GetComponent<Enemy>());
            }
        }
        else
        {
            isBossInstantiated = false;
        }

        // Add enemy to dynamic game objects in scene
        SceneObjectsManager.dynamicGameObjectsInScene.Add(enemy.gameObject);

        // Call mob & boss unlock event
        StaticEventHandler.CallMobUnlockedEvent(enemy.GetComponent<Enemy>().enemyDetails.enemyCategory, isBossInstantiated);

        // Subscribe to enemy destroyed event
        enemy.GetComponent<DestroyedEvent>().OnDestroyed += Enemy_OnDestroyed;
    }

    private void SetEnemyAsBoss(Enemy enemy)
    {
        bossEnemy = enemy;
    }

    public Enemy GetBoss() => bossEnemy;

    public RoomNetData GetCurrentRoomNetData() => currentRoomNetData;

    /// <summary>
    /// Process enemy destroyed
    /// </summary>
    public void Enemy_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        EnemyDestroyProcess(destroyedEvent);
    }

    [Server]
    private void EnemyDestroyProcess(DestroyedEvent destroyedEvent)
    {
        // Unsubscribe from event
        destroyedEvent.OnDestroyed -= Enemy_OnDestroyed;

        // Reduce current enemy count
        currentEnemyCount--;

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn)
        {
            currentRoomNetData.isClearedOfEnemies = true;
            StaticEventHandler.CallEnemiesClearedEvent();

            // Set game state
            if (GameSessionManager.Instance.gameState == GameState.engagingEnemies)
            {
                GameSessionManager.Instance.gameState = GameState.playingLevel;
                GameSessionManager.Instance.previousGameState = GameState.engagingEnemies;
            }
            else if (GameSessionManager.Instance.gameState == GameState.engagingBoss)
            {
                // Are there more dungeon levels then
                if (GameSessionManager.Instance.selectedDungeonLevelIndex < GameSessionManager.Instance.dungeonLevelList.Count - 1)
                {
                    GameSessionManager.Instance.gameState = GameState.levelCompleted;
                }
                else
                {
                    GameSessionManager.Instance.gameState = GameState.gameWon;
                }
            }

            // Unlock doors
            if (!InputManager.TutorialEnabled)
            {
                InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
                instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);
            }

            //// Update music for room
            //MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

            currentRoomNetData.roomCombatState = RoomCombatState.Cleared;

            // Trigger room enemies defeated event
            StaticEventHandler.CallRoomEnemiesDefeatedEventMP(currentRoomNetData, GameSessionManager.Instance.summonedEnemies);
        }
    }

    private GameObject InstantiateEnemyPrefab(EnemyCategory category, Vector3 position)
    {
        foreach (var enemyPrefab in NetworkManager.singleton.spawnPrefabs)
        {
            if (enemyPrefab.TryGetComponent(out EnemyPrefabIdentity identity))
            {
                if (identity.category == category)
                {
                    GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity, transform);
                    return enemyObject;
                }
            }
        }

        return null;
    }
}
