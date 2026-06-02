using Steamworks;
using Mirror;

public class SteamLobbyManager : SingletonMonobehaviour<SteamLobbyManager>
{
    Callback<LobbyCreated_t> lobbyCreatedCallback;
    Callback<LobbyMatchList_t> lobbyMatchListCallback;
    Callback<LobbyEnter_t> lobbyEnterCallback;

    public CSteamID CurrentLobbyId { get; private set; }

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        if (!SteamManager.Initialized) return;

        lobbyCreatedCallback = Callback<LobbyCreated_t>.Create(OnLobbyCreated);
        lobbyMatchListCallback = Callback<LobbyMatchList_t>.Create(OnLobbyMatchList);
        lobbyEnterCallback = Callback<LobbyEnter_t>.Create(OnLobbyEntered);
    }

    public void CreateLobby()
    {
        SteamMatchmaking.CreateLobby(ELobbyType.k_ELobbyTypePublic, 4);
    }

    public void JoinLobby(CSteamID lobbyId)
    {
        SteamMatchmaking.JoinLobby(lobbyId);
    }

    public void RefreshLobbies()
    { 
        SteamMatchmaking.AddRequestLobbyListStringFilter("Game", "Wartheon", ELobbyComparison.k_ELobbyComparisonEqual);
        SteamMatchmaking.RequestLobbyList();
    }

    private void OnLobbyCreated(LobbyCreated_t callback)
    {
        if (callback.m_eResult != EResult.k_EResultOK) return;

        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);

        CurrentLobbyId = lobbyId;

        SteamMatchmaking.SetLobbyData(lobbyId, "HostAddress", SteamUser.GetSteamID().ToString());
        SteamMatchmaking.SetLobbyData(lobbyId, "LobbyName", $"{PlayerProfile.DisplayName}'s Lobby");
        SteamMatchmaking.SetLobbyData(lobbyId, "Game", "Wartheon");
        SteamMatchmaking.SetLobbyData(lobbyId, "HostName", PlayerProfile.DisplayName);

        NetworkManager.singleton.StartHost();
    }

    private void OnLobbyMatchList(LobbyMatchList_t result)
    {
        MultiplayerEntryUI.Instance.ClearFoundGames();

        for (int i = 0; i < result.m_nLobbiesMatching; i++)
        {
            CSteamID lobbyId = SteamMatchmaking.GetLobbyByIndex(i);

            string lobbyName = SteamMatchmaking.GetLobbyData(lobbyId, "LobbyName");
            string hostName = SteamMatchmaking.GetLobbyData(lobbyId, "HostName");
            int currentPlayers = SteamMatchmaking.GetNumLobbyMembers(lobbyId);
            int maxPlayers = SteamMatchmaking.GetLobbyMemberLimit(lobbyId);

            MultiplayerEntryUI.Instance.AddSteamSession(lobbyName, $"{currentPlayers} / {maxPlayers} Players", true, lobbyId);
        }
    }

    private void OnLobbyEntered(LobbyEnter_t callback)
    {
        CSteamID lobbyId = new CSteamID(callback.m_ulSteamIDLobby);
        string hostAddress = SteamMatchmaking.GetLobbyData(lobbyId, "HostAddress");

        if (NetworkServer.active) return;

        NetworkManager.singleton.networkAddress = hostAddress;
        NetworkManager.singleton.StartClient();
    }
}
