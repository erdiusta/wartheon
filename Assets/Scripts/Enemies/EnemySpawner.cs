using Mirror;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class EnemySpawner : SingletonMonobehaviour<EnemySpawner>
{
    [HideInInspector] public Enemy bossEnemy;
    [HideInInspector] public bool isBossInstantiated;

    int enemiesToSpawn;
    int currentEnemyCount;
    int enemiesSpawnedSoFar;
    int enemyMaxConcurrentSpawnNumber;
    int spawnPositionIndex = 0;
    Room currentRoom;
    RoomEnemySpawnParameters roomEnemySpawnParameters;
    Player player;

    protected override void Awake()
    {
        base.Awake();

        // Disable this script in Multiplayer
        if(NetworkServer.active || NetworkClient.active)
        {
            enabled = false;
            return;
        }
    }

    private void OnEnable()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnEnemyKilled += StaticEventHandler_OnEnemyKilled;

            Debug.Log("LEAAK!!!");
        }
    }

    private void OnDisable()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnEnemyKilled -= StaticEventHandler_OnEnemyKilled;
        }
    }

    private void StaticEventHandler_OnEnemyKilled(EnemyKilledArgs enemyKilledArgs)
    {
        if (enemyKilledArgs.enemy.enemyDetails.enemyCategory == EnemyCategory.MainSlime)
        {
            Grid grid = currentRoom.instantiatedRoom.grid;

            // Create three minions from the dead main slime
            CreateEnemy(enemyKilledArgs.enemy.enemyDetails.enemyMinionDetails, enemyKilledArgs.enemy.transform.position + new Vector3(1f, 0f, 0f));
            CreateEnemy(enemyKilledArgs.enemy.enemyDetails.enemyMinionDetails, enemyKilledArgs.enemy.transform.position + new Vector3(-1, 0f, 0f));
            CreateEnemy(enemyKilledArgs.enemy.enemyDetails.enemyMinionDetails, enemyKilledArgs.enemy.transform.position);
        }
    }

    /// <summary>
    /// Process a change in room
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        // If player is null add it to dynamicGameObjectsInScene
        if (player == null)
        {
            player = GameManager.Instance.GetPlayer();
            SceneObjectsManager.dynamicGameObjectsInScene.Add(player.gameObject);
        }

        enemiesSpawnedSoFar = 0;
        currentEnemyCount = 0;
        spawnPositionIndex = 0;

        currentRoom = roomChangedEventArgs.room;

        // Update music for room
        MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

        // Tutorial check - Lock door for a while
        if (InputManager.TutorialEnabled && currentRoom.roomNodeType.isEntrance) goto tutorialEntranceRoomCheck;

        // If the room is a corridor or the entrance then return
        if (currentRoom.roomNodeType.isCorridorEW || currentRoom.roomNodeType.isCorridorNS || currentRoom.roomNodeType.isEntrance) return;

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
        enemyMaxConcurrentSpawnNumber = GetConcurrentEnemies();

        // Update music for room
        MusicManager.Instance.PlayMusic(currentRoom.battleMusic, 0.2f, 0.5f);

    tutorialEntranceRoomCheck:

        // Lock doors
        currentRoom.instantiatedRoom.LockDoors();

        // Spawn enemies
        SpawnEnemies(roomChangedEventArgs.room);
    }

    /// <summary>
    /// Spawn the enemies
    /// </summary>
    private void SpawnEnemies(Room room)
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

        StartCoroutine(SpawnEnemiesRoutine());
    }

    /// <summary>
    /// Spawn the enemies coroutine
    /// </summary>
    public IEnumerator SpawnEnemiesRoutine()
    {
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Create an instance of the helper class used to select a random enemy
        RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

        // Check we have somewhere to spawn the enemies
        if (currentRoom.spawnPositionArray.Length > 0)
        {
            // Create Enemy - Get next enemy type to spawn 
            if (InputManager.TutorialEnabled)
            {
                Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[spawnPositionIndex++];
                spawnPositionIndex %= currentRoom.spawnPositionArray.Length;

                if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Combat)
                {
                    CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));
                    yield break;
                }

                if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry)
                {
                    CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));
                    yield break;
                }

                if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.DodgeRoll)
                {
                    CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition - new Vector3Int(4, 4, 0)));
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

                if (randomEnemyHelperClass.GetItem().isEnemyBoss)
                {
                    cellPosition = (Vector3Int)currentRoom.spawnPositionArray[0];
                }
                else
                {
                    cellPosition = (Vector3Int)currentRoom.spawnPositionArray[spawnPositionIndex++];
                    spawnPositionIndex %= currentRoom.spawnPositionArray.Length;
                }

                CreateEnemy(randomEnemyHelperClass.GetItem(), grid.CellToWorld(cellPosition));

                yield return new WaitForSeconds(GetEnemySpawnInterval());
            }
        }
    }

    /// <summary>
    /// Get a random spawn interval between the minimum and maximum values
    /// </summary>
    private float GetEnemySpawnInterval()
    {
        return Random.Range(roomEnemySpawnParameters.minSpawnInterval, roomEnemySpawnParameters.maxSpawnInterval);
    }

    /// <summary>
    /// Get a random number of concurrent enemies between the minimum and maximum values
    /// </summary>
    private int GetConcurrentEnemies()
    {
        return (Random.Range(roomEnemySpawnParameters.minConcurrentEnemies, roomEnemySpawnParameters.maxConcurrentEnemies));
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
        DungeonLevelSO dungeonLevel = GameManager.Instance.GetCurrentDungeonLevel();

        // Instantiate enemy
        GameObject enemy = Instantiate(enemyDetails.enemyPrefab, position, Quaternion.identity, transform);

        // Initialize Enemy
        enemy.GetComponent<Enemy>().EnemyInitialization(enemyDetails, enemiesSpawnedSoFar, dungeonLevel, isMultiplayer: false);

        // Set boss 
        if (currentRoom.roomNodeType.isBossRoom)
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

    /// <summary>
    /// Process enemy destroyed
    /// </summary>
    public void Enemy_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        // Unsubscribe from event
        destroyedEvent.OnDestroyed -= Enemy_OnDestroyed;

        // Reduce current enemy count
        currentEnemyCount--;

        if (currentEnemyCount <= 0 && enemiesSpawnedSoFar == enemiesToSpawn)
        {
            currentRoom.isClearedOfEnemies = true;
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
                currentRoom.instantiatedRoom.UnlockDoors(Settings.doorUnlockDelay);
            }

            // Update music for room
            MusicManager.Instance.PlayMusic(currentRoom.ambientMusic, 0.2f, 2f);

            // Trigger room enemies defeated event
            StaticEventHandler.CallRoomEnemiesDefeatedEvent(currentRoom, GameManager.Instance.GetPlayer().summonedEnemies);
        }
    }
}
