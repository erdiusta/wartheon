using Mirror;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class EnemySpawner : MonoBehaviour
{
    public static Enemy ActiveBoss { get; set; }

    Enemy bossEnemy;

    public EnemyCategory bossEnemyName;

    int enemiesToSpawn;
    int currentEnemyCount;
    int enemiesSpawnedSoFar;
    int enemyMaxConcurrentSpawnNumber;
    int spawnPositionIndex = 0;

    Room currentRoom;
    RoomNetData currentRoomNetData;

    RoomNetworkRoot currentRoomNetworkRoot;

    RoomEnemySpawnParameters roomEnemySpawnParameters;
    RoomEnemySpawnParametersNet roomEnemySpawnParametersNet;

    Player player;

    [HideInInspector] public InstantiatedRoom instantiatedRoom;

    private void Awake()
    {
        instantiatedRoom = GetComponentInParent<InstantiatedRoom>();
    }

    private void OnEnable()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnEnemyKilled += StaticEventHandler_OnEnemyKilled; // Spawn minions
        }
    }

    private void OnDisable()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnEnemyKilled -= StaticEventHandler_OnEnemyKilled; // Spawn minions
        }
    }

    #region SP - OnRoomChanged
    /// <summary>
    /// Process a change in room
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs args)
    {
        if (!NetworkServer.active && !NetworkClient.active) HandleRoomEnteredSP(args);
    }

    void HandleRoomEnteredSP(RoomChangedEventArgs args)
    {
        if (args.room != instantiatedRoom.room) return;

        // If player is null add it to dynamicGameObjectsInScene
        if (player == null)
        {
            player = GameManager.Instance.GetLocalPlayer();
            SceneObjectsManager.dynamicGameObjectsInScene.Add(player.gameObject);
        }

        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;
        spawnPositionIndex = 0;

        currentRoom = args.room;

        // Update music for room
        MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

        // Create seed
        int seed = Random.Range(int.MinValue, int.MaxValue);
        WartheonRNG rng = new WartheonRNG(seed);

        // Tutorial check - Lock door for a while
        if (InputManager.TutorialEnabled && currentRoom.roomNodeType.isEntrance) goto tutorialEntranceRoomCheck;

        // If the room has already been defeated then return
        if (currentRoom.isClearedOfEnemies) return;

        // Get random number of enemies to spawn
        enemiesToSpawn = currentRoom.GetNumberOfEnemiesToSpawn(GameManager.Instance.GetCurrentDungeonLevel());

        // Get room enemy spawn parameters
        roomEnemySpawnParameters = currentRoom.GetRoomEnemySpawnParameters(GameManager.Instance.GetCurrentDungeonLevel());

        // If no enemies to spawn return
        if (enemiesToSpawn == 0)
        {
            // Mark the room as cleared
            currentRoom.isClearedOfEnemies = true;
            return;
        }

        // Get concurrent number of enemies to spawn
        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies(rng);

        // Update music for room
        MusicManager.Instance.PlayMusic(currentRoom.battleMusic, 0.2f, 0.5f);

    tutorialEntranceRoomCheck:

        // Lock door
        currentRoom.instantiatedRoom.LockDoors();

        // Spawn enemies
        SpawnEnemiesSP(args.room, rng);
    }

    /// <summary>
    /// Spawn the enemies
    /// </summary>
    private void SpawnEnemiesSP(Room room, WartheonRNG rng)
    {
        // Set gamestate engaging enemies
        if (GameManager.Instance.gameState == GameState.playingLevel)
        {
            if (room.roomNodeType.isBossRoom)
            {
                GameManager.Instance.previousGameState = GameState.playingLevel;
                GameManager.Instance.gameState = GameState.engagingBoss;
            }
            else
            {
                GameManager.Instance.previousGameState = GameState.playingLevel;
                GameManager.Instance.gameState = GameState.engagingEnemies;
            }
        }

        StartCoroutine(SpawnEnemiesRoutineSP(rng));
    }

    /// <summary>
    /// Spawn the enemies coroutine
    /// </summary>
    public IEnumerator SpawnEnemiesRoutineSP(WartheonRNG rng)
    {
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Create an instance of the helper class used to select a random enemy
        RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

        // Check we have somewhere to spawn the enemies
        if (currentRoom.spawnPositionArray.Length > 0)
        {
            Enemy enemy;

            // Create Enemy - Get next enemy type to spawn 
            if (InputManager.TutorialEnabled)
            {
                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[spawnPositionIndex++];
                spawnPositionIndex %= currentRoom.spawnPositionArray.Length;

                if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Combat)
                {
                    CreateEnemySP(randomEnemyHelperClass.GetItem(rng), grid.CellToWorld(cellPosition), out enemy);
                    yield break;
                }

                if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry)
                {
                    CreateEnemySP(randomEnemyHelperClass.GetItem(rng), grid.CellToWorld(cellPosition), out enemy);
                    yield break;
                }

                if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.DodgeRoll)
                {
                    CreateEnemySP(randomEnemyHelperClass.GetItem(rng), grid.CellToWorld(cellPosition - new Vector3Int(4, 4, 0)), out enemy);
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

                if (randomEnemyHelperClass.GetItem(rng).isEnemyBoss)
                {
                    cellPosition = (Vector3Int)currentRoom.spawnPositionArray[0];
                }
                else
                {
                    cellPosition = (Vector3Int)currentRoom.spawnPositionArray[spawnPositionIndex++];
                    spawnPositionIndex %= currentRoom.spawnPositionArray.Length;
                }

                CreateEnemySP(randomEnemyHelperClass.GetItem(rng), grid.CellToWorld(cellPosition), out enemy);

                yield return new WaitForSeconds(GetEnemySpawnInterval(rng));
            }
        }
    }

    /// <summary>
    /// Create an enemy in the specified position
    /// </summary>
    public void CreateEnemySP(EnemyDetailsSO enemyDetails, Vector3 position, out Enemy enemy)
    {
        // Keep track of the number of enemies spawned so far
        if (!enemyDetails.isSummonedMinion)
        {
            enemiesSpawnedSoFar++;
        }

        // Add one to the current enemy count - this is reduced when an enemy is destroyed
        currentEnemyCount++;

        // Get current dungeon level
        DungeonLevelSO dungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();

        // Instantiate enemy
        GameObject enemyObject = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity, transform);
        enemy = enemyObject.GetComponent<Enemy>();

        // Initialize Enemy
        enemy.GetComponent<Enemy>().EnemyInitialization(enemyDetails, enemiesSpawnedSoFar, dungeonLevel, isMultiplayer: false);

        // Set boss 
        if (instantiatedRoom.room.roomNodeType.isBossRoom)
        {
            if (enemy.Isboss)
            {
                SetEnemyAsBoss(enemy.GetComponent<Enemy>());
            }
        }

        // Add enemy to dynamic game objects in scene
        SceneObjectsManager.dynamicGameObjectsInScene.Add(enemy.gameObject);

        // Call mob & boss unlock event
        StaticEventHandler.CallMobUnlockedEvent(enemy.GetComponent<Enemy>().enemyDetails.enemyCategory, bossEnemy != null);

        // Enemy belonging room
        enemy.GetComponent<Enemy>().belongingRoom = instantiatedRoom.room;

        // Subscribe to enemy destroyed event
        enemy.GetComponent<DestroyedEvent>().OnDestroyed += Enemy_OnDestroyed;
    }
    #endregion

    #region SP - OnEnemyKilled
    private void StaticEventHandler_OnEnemyKilled(EnemyKilledArgs enemyKilledArgs)
    {
        if (enemyKilledArgs.enemy == null) return;

        if (enemyKilledArgs.enemy.belongingRoom != currentRoom) return;

        if (enemyKilledArgs.enemy.enemyDetails.enemyCategory == EnemyCategory.MainSlime)
        {
            Enemy enemy;

            // Create three minions from the dead main slime
            CreateEnemySP(enemyKilledArgs.enemy.enemyDetails.enemyMinionDetails, enemyKilledArgs.enemy.transform.position + new Vector3(1f, 0f, 0f), out enemy);
            CreateEnemySP(enemyKilledArgs.enemy.enemyDetails.enemyMinionDetails, enemyKilledArgs.enemy.transform.position + new Vector3(-1, 0f, 0f), out enemy);
            CreateEnemySP(enemyKilledArgs.enemy.enemyDetails.enemyMinionDetails, enemyKilledArgs.enemy.transform.position, out enemy);
        }
    }
    #endregion

    /// <summary>
    /// Process enemy destroyed
    /// </summary>
    public void Enemy_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        if (!NetworkServer.active && !NetworkClient.active) EnemyDestroyProcessSP(destroyedEvent);
        else if (NetworkServer.active) EnemyDestroyProcessMP(destroyedEvent);
    }

    #region SP - OnEnemyDestroyed
    private void EnemyDestroyProcessSP(DestroyedEvent destroyedEvent)
    {
        // Unsubscribe from event
        destroyedEvent.OnDestroyed -= Enemy_OnDestroyed;

        // Reduce current enemy count
        currentEnemyCount--;

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn)
        {
            instantiatedRoom.room.isClearedOfEnemies = true;
            StaticEventHandler.CallEnemiesClearedEvent();

            // Set game state
            if (GameManager.Instance.gameState == GameState.engagingEnemies)
            {
                GameManager.Instance.gameState = GameState.playingLevel;
                GameManager.Instance.previousGameState = GameState.engagingEnemies;
            }
            else if (GameManager.Instance.gameState == GameState.engagingBoss)
            {
                // Are there more dungeon levels then
                if (GameManager.Instance.currentDungeonLevelListIndex < GameManager.Instance.dungeonLevelList.Count - 1)
                {
                    GameManager.Instance.gameState = GameState.levelCompleted;
                }
                else
                {
                    GameManager.Instance.gameState = GameState.gameWon;
                }
            }

            // Unlock doors
            if (!InputManager.TutorialEnabled)
            {
                instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);
            }

            // Update music for room
            MusicManager.Instance.PlayMusic(instantiatedRoom.room.ambientMusic, 0.2f, 2f);

            // Trigger room enemies defeated event - For summons
            StaticEventHandler.CallRoomEnemiesDefeatedEvent(instantiatedRoom.room, default, GameManager.Instance.GetLocalPlayer().summonedEnemies);
        }
    }
    #endregion

    #region MP - OnRoomChanged
    public void RoomChangedMP(RoomNetData roomNetData)
    {
        currentRoomNetworkRoot = instantiatedRoom.GetComponentInParent<RoomNetworkRoot>();
        currentRoomNetData = roomNetData;

        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;
        spawnPositionIndex = 0;

        // Tutorial check - Lock door for a while
        if (currentRoomNetData.isEntrance) goto entranceRoomCheck;

        // If the room is a corridor or the entrance then return
        if (currentRoomNetData.isCorridorEW || currentRoomNetData.isCorridorNS || currentRoomNetData.isEntrance) return;

        // If the room has already been defeated then return
        if (currentRoomNetData.roomCombatState == RoomCombatState.Cleared) return;

        currentRoomNetworkRoot.roomNetData = roomNetData;
        currentRoomNetData = roomNetData;

        int dungeonLevelIndex = GameSessionManager.Instance.selectedDungeonLevelIndex + 1;

        // Get random number of enemies to spawn
        if (!currentRoomNetData.TryGetEnemySpawnParameters(dungeonLevelIndex, out roomEnemySpawnParametersNet))
        {
            // No spawn parameters - room is auto-cleared
            roomNetData.roomCombatState = RoomCombatState.Cleared;
            currentRoomNetworkRoot.roomNetData.roomCombatState = RoomCombatState.Cleared;
            currentRoomNetData.roomCombatState = RoomCombatState.Cleared;

            currentRoomNetworkRoot.roomNetData = roomNetData;
            currentRoomNetData = roomNetData;

            return;
        }

        // Server-Only randomization
        enemiesToSpawn = Random.Range(roomEnemySpawnParametersNet.minTotalEnemiesToSpawn, roomEnemySpawnParametersNet.maxTotalEnemiesToSpawn + 1);

        // Get concurrent number of enemies to spawn
        enemyMaxConcurrentSpawnNumber = Random.Range(roomEnemySpawnParametersNet.minConcurrentEnemies, roomEnemySpawnParametersNet.maxConcurrentEnemies + 1);

    entranceRoomCheck:
        // Spawn enemies
        SpawnEnemiesMP(currentRoomNetData);
    }

    [Server]
    private void SpawnEnemiesMP(RoomNetData roomNetData)
    {
        // Set gamestate engaging enemies
        if (GameSessionManager.Instance.gameState == GameState.playingLevel)
        {
            if (roomNetData.isBossRoom)
            {
                GameSessionManager.Instance.SetGameState(GameState.engagingBoss);
            }
            else
            {
                GameSessionManager.Instance.SetGameState(GameState.engagingEnemies);
            }
        }

        StartCoroutine(SpawnEnemiesRoutineMP());
    }

    /// <summary>
    /// Spawn the enemies coroutine
    /// </summary>
    public IEnumerator SpawnEnemiesRoutineMP()
    {
        InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
        Grid grid = instantiatedRoom.grid;

        // Create an instance of the helper class used to select a random enemy
        RandomSpawnableObjectNet randomEnemySelector = new RandomSpawnableObjectNet(currentRoomNetData.enemiesByLevel, GameSessionManager.Instance.selectedDungeonLevelIndex + 1);

        if (currentRoomNetData.spawnPositions.Length == 0) yield break;

        // Create Enemy - Get next enemy type to spawn 
        if (InputManager.TutorialEnabled)
        {
            Vector3Int cellPosition = Vector3Int.zero;

            try
            {
                cellPosition = (Vector3Int)currentRoomNetData.spawnPositions[spawnPositionIndex++];
                spawnPositionIndex %= currentRoomNetData.spawnPositions.Length;
            }
            catch (System.IndexOutOfRangeException)
            {
                Debug.Log("Spawn position count is " + currentRoomNetData.spawnPositions.Length);
                Debug.Log("Spawn position index is " + spawnPositionIndex);
            }


            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Combat)
            {
                CreateEnemyMP(WartheonDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory()), grid.CellToWorld(cellPosition));
                yield break;
            }

            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry)
            {
                CreateEnemyMP(WartheonDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory()), grid.CellToWorld(cellPosition));
                yield break;
            }

            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.DodgeRoll)
            {
                CreateEnemyMP(WartheonDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory()), grid.CellToWorld(cellPosition - new Vector3Int(4, 4, 0)));
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

            EnemyDetailsSO enemyDetails = WartheonDatabase.Instance.GetEnemy(randomEnemySelector.GetRandomCategory());

            if (enemyDetails.isEnemyBoss)
            {
                cellPosition = (Vector3Int)currentRoomNetData.spawnPositions[0];
            }
            else
            {
                try
                {
                    cellPosition = (Vector3Int)currentRoomNetData.spawnPositions[spawnPositionIndex++];
                    spawnPositionIndex %= currentRoomNetData.spawnPositions.Length;
                }
                catch (System.IndexOutOfRangeException)
                {
                    Debug.Log("Spawn position count is " + currentRoomNetData.spawnPositions.Length);
                    Debug.Log("Spawn position index is " + spawnPositionIndex);
                }

                cellPosition = (Vector3Int)currentRoomNetData.spawnPositions[spawnPositionIndex++];
                spawnPositionIndex %= currentRoomNetData.spawnPositions.Length;
            }

            CreateEnemyMP(enemyDetails, grid.CellToWorld(cellPosition));

            yield return new WaitForSeconds(Random.Range(roomEnemySpawnParametersNet.minSpawnInterval, roomEnemySpawnParametersNet.maxSpawnInterval));
        }

    }

    /// <summary>
    /// Create an enemy in the specified position
    /// </summary>
    public GameObject CreateEnemyMP(EnemyDetailsSO enemyDetails, Vector3 position)
    {
        // Keep track of the number of enemies spawned so far
        if (!enemyDetails.isSummonedMinion) enemiesSpawnedSoFar++;

        // Add one to the current enemy count - this is reduced when an enemy is destroyed
        currentEnemyCount++;

        // Get current dungeon level
        DungeonLevelSO dungeonLevel = GameSessionManager.Instance.GetCurrentDungeonLevel();

        GameObject enemyObject = InstantiateEnemyPrefab(enemyDetails.enemyCategory, position);
        Enemy enemy = enemyObject.GetComponent<Enemy>();

        InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);

        NetworkServer.Spawn(enemyObject);

        // Initialize Enemy
        enemy.GetComponent<EnemyNetwork>().ServerInitialize(enemyDetails, enemiesSpawnedSoFar, dungeonLevel);
        enemy.GetComponent<EnemyNetwork>().InitializeRoom(instantiatedRoom, currentRoomNetData.spawnPositions);

        // Set boss
        if (enemy.GetComponent<EnemyNetwork>().Isboss)
        {
            SetEnemyAsBoss(enemy);

            DungeonNetworkController.Instance.activeBossNetId = enemy.GetComponent<EnemyNetwork>().netId;
        }

        // Add enemy to dynamic game objects in scene
        SceneObjectsManager.dynamicGameObjectsInScene.Add(enemyObject.gameObject);

        // Call mob & boss unlock event
        StaticEventHandler.CallMobUnlockedEvent(enemyObject.GetComponent<Enemy>().enemyDetails.enemyCategory,
            enemyObject.GetComponent<Enemy>().enemyDetails.isEnemyBoss);

        // Add room to enemy
        enemy.belongingRoomData = currentRoomNetData;
        enemy.owningSpawner = this;

        // Subscribe to enemy destroyed event
        enemyObject.GetComponent<DestroyedEvent>().OnDestroyed += Enemy_OnDestroyed;

        return enemyObject;
    }

    [Server]
    private void EnemyDestroyProcessMP(DestroyedEvent destroyedEvent)
    {
        // Unsubscribe from event
        destroyedEvent.OnDestroyed -= Enemy_OnDestroyed;

        // Reduce current enemy count
        currentEnemyCount--;

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn)
        {
            currentRoomNetworkRoot = instantiatedRoom.GetComponentInParent<RoomNetworkRoot>();

            RoomNetData data = currentRoomNetworkRoot.roomNetData;
            data.isClearedOfEnemies = true;
            data.roomCombatState = RoomCombatState.Cleared;
            currentRoomNetworkRoot.roomNetData.roomCombatState = RoomCombatState.Cleared;
            currentRoomNetData.roomCombatState = RoomCombatState.Cleared;

            currentRoomNetworkRoot.roomNetData = data;
            instantiatedRoom.roomNetData = data;
            currentRoomNetData = data;

            StartCoroutine(UpdateRoomAfterStateRoutine());
        }
    }

    IEnumerator UpdateRoomAfterStateRoutine()
    {
        yield return null;

        currentRoomNetworkRoot.Server_UnlockDoors();

        StaticEventHandler.CallEnemiesClearedEvent();

        // Set game state
        if (GameSessionManager.Instance.gameState == GameState.engagingEnemies)
        {
            GameSessionManager.Instance.SetGameState(GameState.playingLevel);
        }
        else if (GameSessionManager.Instance.gameState == GameState.engagingBoss)
        {
            // Are there more dungeon levels then
            if (GameSessionManager.Instance.selectedDungeonLevelIndex < GameSessionManager.Instance.dungeonLevelList.Count - 1)
            {
                GameSessionManager.Instance.SetGameState(GameState.levelCompleted);
            }
            else
            {
                GameSessionManager.Instance.SetGameState(GameState.gameWon);
            }
        }

        currentRoomNetworkRoot.Server_RefreshMusic();
    }

    private GameObject InstantiateEnemyPrefab(EnemyCategory category, Vector3 position)
    {
        foreach (var enemyPrefab in NetworkManager.singleton.spawnPrefabs)
        {
            if (enemyPrefab.TryGetComponent(out EnemyPrefabIdentity identity))
            {
                if (identity.category == category)
                {
                    GameObject enemyObject = Instantiate(enemyPrefab, position, Quaternion.identity);
                    return enemyObject;
                }
            }
        }

        return null;
    }
    #endregion

    #region HELPERS
    /// <summary>
    /// Get a random spawn interval between the minimum and maximum values
    /// </summary>
    private float GetEnemySpawnInterval(WartheonRNG rng)
    {
        return rng.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval);
    }

    /// <summary>
    /// Get a random number of concurrent enemies between the minimum and maximum values
    /// </summary>
    private int GetConcurrentEnemies(WartheonRNG rng)
    {
        return rng.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies);
    }
    #endregion

    #region BOSS
    public void SetEnemyAsBoss(Enemy enemy)
    {
        bossEnemy = enemy;
        ActiveBoss = enemy;
    }

    public Enemy GetBoss() => bossEnemy;
    #endregion
}