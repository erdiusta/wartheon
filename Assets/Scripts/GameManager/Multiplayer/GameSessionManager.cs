using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameSessionManager : NetworkBehaviour
{
    public static GameSessionManager Instance;

    public static bool isDemo = false;

    public static event Action<PlayerReadyEventArgs> OnPlayerRegistered;

    public List<GameObject> summonedEnemies = new List<GameObject>();

    public void CallPlayerRegisteredEvent(Player player)
    {
        OnPlayerRegistered?.Invoke(new PlayerReadyEventArgs { player = player });
    }

    [HideInInspector] public HashSet<Player> ServerPlayers = new HashSet<Player>();

    // ==========================
    // SERVER STATE
    // ==========================

    [SyncVar(hook = nameof(OnGameStateChanged))] 
    public GameState gameState = GameState.lobby;

    [SyncVar]
    public GameState previousGameState;

    [SyncVar(hook = nameof(OnDungeonBuiltChanged))] 
    bool dungeonBuilt;
    public bool OnDungeonBuilt => dungeonBuilt;

    // ==========================
    // DATA / REFERENCES
    // ==========================

    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS
    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion Tooltip
    public List<DungeonLevelSO> dungeonLevelList;
    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for testing , first level = 0")]
    #endregion Tooltip
    [SyncVar] public int selectedDungeonLevelIndex;
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the MessageText textmeshpro component in the FadeScreenUI")]
    #endregion
    [SerializeField] TextMeshProUGUI messageTextTMP;
    #region Tooltip
    [Tooltip("Populate with the fade image component in the FadeScreenUI")]
    #endregion
    [SerializeField] Image fadeImage;

    // ==========================
    // RUN-TIME
    // ==========================
    [HideInInspector] public Dummy decoy;

    // ROOMS
    InstantiatedRoom bossRoom;
    Room currentRoom;
    Room previousRoom;
    [SyncVar] public RoomNetData currentRoomNetData;
    RoomNetData previousRoomNetData;

    HashSet<Room> visitedRooms = new HashSet<Room>();
    [HideInInspector] public Queue<InstantiatedRoom> lastThreeRooms = new Queue<InstantiatedRoom>();

    [SyncVar] public bool levelTransitionInProgress;

    // ==========================
    // UNITY LIFECYCLE
    // ==========================

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnStartServer()
    {
        Subscribe();

        Debug.Log("On start server, player count is " + ServerPlayers.Count);
        ServerPlayers.Clear();
    }

    public override void OnStopServer()
    {
        Unsubscribe();
    }


    [Server]
    private void Subscribe()
    {
        OnPlayerRegistered += GameSessionManager_OnPlayerRegistered;
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnDecoySpawned += StaticEventHandler_OnDecoySpawned;
    }

    [Server]
    private void Unsubscribe()
    {
        OnPlayerRegistered -= GameSessionManager_OnPlayerRegistered;
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnDecoySpawned -= StaticEventHandler_OnDecoySpawned;
    }

    private void GameSessionManager_OnPlayerRegistered(PlayerReadyEventArgs args)
    {
        if (args.player != null) AddServerPlayer(args.player);

        if (ServerPlayers.Count == NetworkServer.connections.Count) StartGame(); // Server players 2 - Connection count 2
    }

    [Server]
    private void AddServerPlayer(Player player)
    {
        ServerPlayers.Add(player);
    }

    private void OnGameStateChanged(GameState oldState, GameState newState)
    {
        if (!isClient) return;

        // Ignore initial SyncVar delivery
        if (oldState == GameState.lobby && previousGameState == GameState.lobby) return;

        HandleClientState(newState);
    }

    /// <summary>
    /// Client-side fire
    /// </summary>
    private void OnDungeonBuiltChanged(bool oldVal, bool newVal)
    {
        if (!isClient || !newVal) return;

        StaticEventHandler.CallDungeonBuiltEvent();
    }

    [Server]
    public void SetGameState(GameState newState)
    {
        if (gameState == newState) return;

        previousGameState = gameState;
        gameState = newState;

        HandleServerState(newState);
    }

    // ==========================
    // SERVER FLOW
    // ==========================

    [Server]
    public void StartGame()
    {
        if (gameState == GameState.gameStarted) return;

        SetGameState(GameState.gameStarted);
    }

    [Server]
    public void HandleServerState(GameState newState)
    {
        switch (newState)
        {
            case GameState.lobby:
                break;
            case GameState.gameStarted:
                PlayDungeonLevel();
                SetGameState(GameState.dungeonAndPlayersGenerated);
                break;
            case GameState.dungeonAndPlayersGenerated:
                SetGameState(GameState.playingLevel);
                break;
            case GameState.playingLevel:
                break;
            case GameState.engagingEnemies:
                break;
            case GameState.engagingBoss:
                break;
            case GameState.levelCompleted:
                StartCoroutine(ServerLevelCompletedRoutine());
                break;
            case GameState.gameWon:
                break;
            case GameState.gameLost:
                break;
            case GameState.gamePaused:
                break;
            case GameState.restartGame:
                SceneManager.LoadScene("MainMenuScene");
                break;
            default:
                break;
        }
    }

    private void HandleClientState(GameState newState)
    {
        // Manage pause in server
        bool paused = newState == GameState.gamePaused;
        Time.timeScale = paused ? 0f : 1f;
        GameManager.Instance.ApplyPauseUI(paused, singlePlayer: false);

        switch (newState)
        {
            case GameState.lobby:
                break;
            case GameState.gameStarted:
                break;
            case GameState.dungeonAndPlayersGenerated:
                break;
            case GameState.playingLevel:
                break;
            case GameState.engagingEnemies:
                break;
            case GameState.engagingBoss:
                break;
            case GameState.levelCompleted:
                StartCoroutine(LevelCompletedUI());
                break;
            case GameState.gameWon:
                StartCoroutine(GameWonUI());
                break;
            case GameState.gameLost:
                StartCoroutine(GameLostUI());
                break;
            case GameState.gamePaused:
                break;
            case GameState.restartGame:
                break;
            default:
                break;
        }
    }

    public void ApplyPauseClient(bool paused)
    {
        Time.timeScale = paused ? 0f : 1f;
        GameManager.Instance.ApplyPauseUI(paused, singlePlayer: false);
    }

    // ==========================
    // SERVER ACTIONS
    // ==========================

    [Server]
    private void PlayDungeonLevel()
    {
        levelTransitionInProgress = true;

        //Build dungeon for level
        bool success = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[selectedDungeonLevelIndex], tutorialEnabled: false);

        if (!success)
        {
            Debug.LogError("Dungeon generation failed.");
            return;
        }

        // Instantiate rooms
        DungeonNetworkController.Instance.SpawnDungeon(selectedDungeonLevelIndex);

        // Bake nav meshes when dungeon build is successful
        AstarPath.active?.Scan();

        // Reset Phoenix Rising - Kynara specific skill
        foreach (Player player in ServerPlayers)
        {
            if (!player.isInitialized) continue;
            if (player.playerDetails.playerCharacterIndex != Character.Kynara) continue;
            player.phoenixRisingUsed = false;
        }

        // Client ready
        StartCoroutine(ServerLevelStartRoutine());
    }

    [Server]
    IEnumerator ServerLevelStartRoutine()
    {
        // Wait until room visuals built
        yield return new WaitForSeconds(0.1f);

        // Spawn players safely
        SpawnPlayers();

        // Allow player transforms/network sync
        yield return null;

        // Initialize entrance room properly
        InitializeEntranceRoom();

        // Allow player transforms/network sync
        yield return null;

        levelTransitionInProgress = false;

        // Now clients are allowed to proceed
        RpcClientGameplayReady(selectedDungeonLevelIndex);
    }

    void SpawnPlayers()
    {
        Vector3 roomCenter = new Vector3((currentRoomNetData.lowerBounds.x + currentRoomNetData.upperBounds.x) * 0.5f, (currentRoomNetData.lowerBounds.y + currentRoomNetData.upperBounds.y) * 0.5f, 0f);

        int index = 0;

        foreach (Player player in ServerPlayers)
        {
            Vector3 spawnPos = roomCenter;

            var nt = player.GetComponent<NetworkTransformReliable>();           
            nt.ServerTeleport(spawnPos, Quaternion.identity);

            player.transform.localPosition = Vector3.zero; // Fix synced positions between player

            index++;
        }
    }

    [Server]
    void InitializeEntranceRoom()
    {
        RoomNetData entranceRoom = default;

        foreach (RoomNetData room in DungeonRuntime.RoomNetDataDict.Values)
        {
            if (!room.isEntrance) continue;

            entranceRoom = room;
            break;
        }

        Player initiator = ServerPlayers.FirstOrDefault();

        if (initiator == null) return;

        DungeonNetworkController.Instance.ServerRoomEntered(entranceRoom.roomId, initiator.NetAuth.netIdentity, Vector2.zero);
    }

    [Server]
    public void ServerTogglePause()
    {
        if (gameState == GameState.gamePaused)
        {
            SetGameState(previousGameState == GameState.gamePaused ? GameState.playingLevel : previousGameState);
        }
        else
        {
            previousGameState = gameState;
            SetGameState(GameState.gamePaused);
        }
    }

    // ==========================
    // SERVER RPCS (UI ONLY)
    // ==========================
    [ClientRpc]
    private void RpcClientGameplayReady(int levelIndex)
    {
        if (!NetworkClient.active) return;

        StartCoroutine(ClientEnterGameplayRoutine(levelIndex));
    }

    // ==========================
    // CLIENT UI COROUTINES
    // ==========================
    IEnumerator ClientEnterGameplayRoutine(int levelIndex)
    {
        // Wait for local player
        while (NetworkClient.localPlayer == null) yield return null;

        Player player = NetworkClient.localPlayer.GetComponent<Player>();
        if (player == null) yield break;

        // Fade out
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 1f, Color.black));

        // Show intro text
        messageTextTMP.SetText("LEVEL " + (levelIndex + 1).ToString() + "\n\n" + dungeonLevelList[levelIndex].levelName.ToUpper());
        messageTextTMP.color = Color.yellow;

        //SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.nextLevelSoundEffect);

        yield return new WaitForSeconds(1f);

        // Fade In
        yield return StartCoroutine(GameManager.Instance.Fade(1f, 0f, 1f, Color.black));

        player.EnablePlayer();
        InputManager.Instance.EnableGameplayInput();
    }

    [Server]
    IEnumerator ServerLevelCompletedRoutine()
    {
        EnemySpawner.ActiveBoss = null;

        DungeonNetworkController.Instance.activeBossNetId = 0;

        yield return new WaitForSeconds(5f);

        DestroyEnemies();

        // Allow enemy despawns to propagate
        yield return null;

        DestroyDungeon(selectedDungeonLevelIndex);

        DungeonNetworkController.Instance.processedRooms.Clear();

        selectedDungeonLevelIndex++;

        // Allow despawns to propagate to clients
        yield return new WaitForSeconds(0.2f);

        if (selectedDungeonLevelIndex >= dungeonLevelList.Count)
        {
            SetGameState(GameState.gameWon);
            yield break;
        }

        PlayDungeonLevel();

        SetGameState(GameState.dungeonAndPlayersGenerated);
    }

    IEnumerator LevelCompletedUI()
    {
        messageTextTMP.SetText("WELL DONE WARTHEON TEAM! YOU'VE SURVIVED\n\nTHIS DUNGEON LEVEL! PRESS OK FOR NEXT LEVEL!");

        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 1.5f, Color.black));

        messageTextTMP.color = Color.yellow;

        yield return new WaitForSeconds(2f);

        CmdRequestNextLevel();
    }

    IEnumerator GameWonUI()
    {
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 2f, Color.black));

        if (isDemo)
        {
            messageTextTMP.SetText("WELL DONE TEAM!\nYOU HAVE COMPLETED DEMO!");
            messageTextTMP.color = Color.green;
        }
        else
        {
            messageTextTMP.SetText("WELL DONE TEAM!\nYOU HAVE SECURED THE WARTHEON");
            messageTextTMP.color = Color.green;
        }

        yield return new WaitForSeconds(3f);

        CmdRequestRestart();
    }

    IEnumerator GameLostUI()
    {
        yield return StartCoroutine(GameManager.Instance.Fade(0f, 1f, 2f, Color.black));

        messageTextTMP.SetText("BAD LUCK TEAM! YOU HAVE\nDIED SOMEWHERE IN WARTHEON.");
        messageTextTMP.color = Color.red;

        yield return new WaitForSeconds(2f);

        messageTextTMP.SetText("PRESS ENTER TO RESTART THE GAME");
        messageTextTMP.color = Color.yellow;

        CmdRequestRestart();
    }

    // ==========================
    // CLIENT -> SERVER REQUESTS
    // ==========================
    [Command]
    private void CmdRequestNextLevel()
    {
        // Increase index to next level
        selectedDungeonLevelIndex++;

        // DEMO CASE
        if (selectedDungeonLevelIndex >= 2 && isDemo)
        {
            gameState = GameState.gameWon;
        }
        else if (selectedDungeonLevelIndex >= dungeonLevelList.Count)
        {
            gameState = GameState.gameWon;
        }
        else
        {
            gameState = GameState.gameStarted;
        }
    }

    [Command]
    private void CmdRequestRestart()
    {
        gameState = GameState.restartGame;
    }

    // ==========================
    // EVENT HANDLERS
    // ==========================

    [Server]
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room, roomChangedEventArgs.roomNetData);

        if (decoy != null)
        {
            Destroy(decoy.gameObject);
        }

        if (roomChangedEventArgs.roomNetData.isCorridor) return;

        visitedRooms.Add(currentRoom);

    }

    [Server]
    private void StaticEventHandler_OnDecoySpawned(DecoySpawnedArgs decoySpawnedArgs)
    {
        SetDecoy(decoySpawnedArgs.decoy);
    }

    [Server]
    private void SetDecoy(Dummy decoy)
    {
        this.decoy = decoy;
    }

    public Dummy GetDecoy()
    {
        return decoy;
    }

    [Server]
    private void DestroyDungeon(int generationToDestroy)
    {
        RoomNetworkRoot[] roomRoots = FindObjectsByType<RoomNetworkRoot>(FindObjectsSortMode.None);

        foreach (RoomNetworkRoot root in roomRoots)
        {
            if (root == null) continue;

            if (root.dungeonGenerationId != generationToDestroy) continue;

            NetworkServer.Destroy(root.gameObject);
        }
    }

    [Server]
    private void DestroyEnemies()
    {
        Enemy[] remainingEnemies = FindObjectsByType<Enemy>(FindObjectsSortMode.None);

        foreach (Enemy enemy in remainingEnemies)
        {
            if (enemy == null) continue;
            if (enemy.Isboss) continue;

            NetworkServer.Destroy(enemy.gameObject);
        }
    }

    [Server]
    public void SetCurrentRoom(Room room, RoomNetData roomNetData = default)
    {
        previousRoom = currentRoom;
        currentRoom = room;

        previousRoomNetData = currentRoomNetData;
        currentRoomNetData = roomNetData;
    }

    public Room GetCurrentRoom() => currentRoom;
    public RoomNetData GetCurrentRoomNetData() => currentRoomNetData;

    public string GetCurrentRoomNetDataID() => currentRoomNetData.roomId;

    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[selectedDungeonLevelIndex];
    }

    public RoomTemplateSO GetRoomTemplateSO(string roomId)
    {
        foreach (var template in dungeonLevelList[selectedDungeonLevelIndex].roomTemplateList)
        {
            if (template.guid == roomId) return template;
            else continue;
        }

        return null;
    }

    [Server]
    public void ClearAllDropItemsInScene()
    {
        DropItemNetwork[] dropItems = FindObjectsByType<DropItemNetwork>(FindObjectsInactive.Include, FindObjectsSortMode.None);

        foreach (DropItemNetwork item in dropItems)
        {
            if (item == null) continue;

            if (item.currentLocation != DropItemLocation.World) continue;

            NetworkServer.Destroy(item.gameObject);
        }
    }
}

public class PlayerArgs : EventArgs
{
    public Player player;
}
