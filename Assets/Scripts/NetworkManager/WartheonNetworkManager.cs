using Mirror;
using Mirror.Discovery;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class WartheonNetworkManager : NetworkManager
{
    public static WartheonNetworkManager Instance { get; private set; }

    public WartheonNetworkDiscovery networkDiscovery;

    public Dictionary<int, uint> characterLocks = new Dictionary<int, uint>();

    // SERVER ONLY
    public Dictionary<int, int> connectionToCharacterIndex = new Dictionary<int, int>();

    [SerializeField] GameObject lobbyPlayerPrefab;

    public override void Awake()
    {
        if(Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        base.Awake();

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public override void Start()
    {
        base.Start();

        if (SteamManager.Initialized) Settings.Backend = MultiplayerBackend.Steam;
        else Settings.Backend = MultiplayerBackend.Lan;
    }

    // Starts at the lobby phase (Main Menu Scene)
    public override void OnStartServer()
    {
        base.OnStartServer();

        if (NetworkRPCExecuter.Instance != null) return;

        foreach (var prefab in spawnPrefabs)
        {
            if (prefab.TryGetComponent<NetworkRPCExecuter>(out _))
            {
                NetworkServer.Spawn(Instantiate(prefab));
                return;
            }
        }
    }

    // Starts at the lobby phase (Main Menu Scene)
    public override void OnStartHost()
    {
        base.OnStartHost();
        GamePhase.IsInLobby = true;

        switch (Settings.Backend)
        {
            case MultiplayerBackend.Lan:
                networkDiscovery?.AdvertiseServer();
                break;
            case MultiplayerBackend.Steam:
                break;
            default:
                break;
        }
    }

    // It should start at lobby phase after players are added (Main Menu Scene)
    public override void OnClientConnect()
    {
        base.OnClientConnect();

        // ONLY request a player here (client-only)
        if (!NetworkClient.ready) NetworkClient.Ready();

        // Add lobby prefab (not gameplay one yet)
        if (NetworkClient.localPlayer == null) NetworkClient.AddPlayer();

        // Host and clients both come through here
        bool isHost = NetworkServer.active;

        // Only open lobby if we are NOT already in game
        if (!GameSessionState.IsGameRunning)
        {
            GamePhase.IsInLobby = true;
            MainMenuUI.Instance.OpenMultiplayerLobby(isHost);
        }
    }

    // Starts at the lobby phase after OnStartClient (Main Menu Scene)
    public override void OnServerAddPlayer(NetworkConnectionToClient conn)
    {
        GameObject lobbyPlayer = Instantiate(lobbyPlayerPrefab);
        NetworkServer.AddPlayerForConnection(conn, lobbyPlayer);
    }

    // Starts at the beginning of MainGame scene
    public override void OnServerSceneChanged(string sceneName)
    {
        // Don't jump to MainGame until lobby phase is completed and scene change from NetworkManager
        if (sceneName != "MainGameScene") return;

        // Create pool
        PoolManager.Instance.InitializePoolMP();

        foreach (NetworkConnectionToClient conn in NetworkServer.connections.Values)
        {
            if (!connectionToCharacterIndex.TryGetValue(conn.connectionId, out int charIndex)) continue;

            // New gameplay player
            GameObject gameplayPlayer = Instantiate(spawnPrefabs[charIndex]);

            // Initialize after replacement
            var state = gameplayPlayer.GetComponent<PlayerNetworkState>();
            state.SetCharacterIndex(charIndex);

            NetworkServer.ReplacePlayerForConnection(conn, gameplayPlayer, ReplacePlayerOptions.KeepActive);
        }
    }

    public override void OnClientSceneChanged() 
    {
        base.OnClientSceneChanged();

        if (SceneManager.GetActiveScene().buildIndex != 2) return;

        // If we are host, server has already initialized the pool
        if (NetworkServer.active) return;

        // Create pool
        PoolManager.Instance.InitializePoolMP();
    }

    public override void OnStopHost()
    {
        switch (Settings.Backend)
        {
            case MultiplayerBackend.Lan:
                if (networkDiscovery != null) networkDiscovery.StopDiscovery();
                break;
            case MultiplayerBackend.Steam:
                SteamLobbyManager.Instance.LeaveCurrentLobby();
                break;
            default:
                break;
        }

        characterLocks.Clear();

        Debug.Log("ON STOP HOST!");
        base.OnStopHost();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        Debug.Log("ON STOP SERVER");
        GameSessionState.ResetSession();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        switch (Settings.Backend)
        {
            case MultiplayerBackend.Lan:
                if (networkDiscovery != null) networkDiscovery.StopDiscovery();
                break;
            case MultiplayerBackend.Steam:
                SteamLobbyManager.Instance.LeaveCurrentLobby();
                break;
            default:
                break;
        }

        Debug.Log("ON STOP CLIENT");
    }

    public override void OnServerDisconnect(NetworkConnectionToClient conn)
    {
        connectionToCharacterIndex.Remove(conn.connectionId);
        base.OnServerDisconnect(conn);
    }

    public override void OnClientDisconnect()
    {
        base.OnClientDisconnect();

        GamePhase.IsInLobby = false;
        MainMenuUI.Instance?.ExitMultiplayerLobby();
    }

    [Server]
    public void SetCharacterForConnection(NetworkConnectionToClient conn, int characterIndex)
    {
        connectionToCharacterIndex[conn.connectionId] = characterIndex;
    }

    [Server]
    public void ClearCharacterForConnection(NetworkConnectionToClient conn)
    {
        connectionToCharacterIndex.Remove(conn.connectionId);
    }

    #region Lobby - Game
    [Server]
    public void StartGameFromLobby()
    {
        if (!NetworkServer.active) return;

        // Not in the lobby anymore
        GamePhase.IsInLobby = false;

        // Tell clients to clean lobby UI NOW
        NetworkRPCExecuter.Instance.RpcPrepareForSceneChange();

        // Change scene (MainGameScene)
        ServerChangeScene("MainGameScene");
    }
    #endregion

    #region Character Locking
    [Server]
    public bool IsCharacterTaken(int index)
    {
        return characterLocks.ContainsKey(index);
    }

    [Server]
    public void LockCharacter(int index, uint playerNetId)
    {
        characterLocks[index] = playerNetId;
    }

    [Server]
    public void UnlockCharacter(int index, uint playerNetId)
    {
        if(characterLocks.TryGetValue(index, out uint owner) && owner == playerNetId)
        {
            characterLocks.Remove(index);
        }
    }
    #endregion
}

public static class GamePhase
{
    public static bool IsInLobby = true;
}
