using Mirror;
using Mirror.Discovery;
using Steamworks;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MultiplayerEntryUI : SingletonMonobehaviour<MultiplayerEntryUI>
{
    [Header("Session List")]
    [SerializeField] Transform foundGamesContent;
    [SerializeField] SessionEntryUI sessionEntryPrefab;

    [Header("Controls")]
    [SerializeField] Button joinButton;
    [SerializeField] CanvasGroup joinButtonCanvasGroup;

    [Header("Texts")]   
    [SerializeField] TMP_Text welcomeText;
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text backText;
    [SerializeField] TMP_Text hostText;
    [SerializeField] TMP_Text joinText;
    [SerializeField] TMP_Text refreshText;

    const float JOIN_ENABLED_ALPHA = 1f;
    const float JOIN_DISABLED_ALPHA = 0.45f;

    SessionEntryUI selectedSession;

    WartheonNetworkDiscovery discovery;

    // Track discovered servers to avoid duplicates
    readonly Dictionary<long, ServerResponse> discoveredServers = new();

    #region Unity Lifecycle
    protected override void Awake()
    {
        base.Awake();

        discovery = WartheonNetworkManager.Instance.networkDiscovery;
    }

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        RefreshLocalizedTexts();

        ClearFoundGames();
        ResetSelection();

        switch (Settings.Backend)
        {
            case MultiplayerBackend.Lan:
                if (!NetworkServer.active)
                {
                    if (discovery != null)
                    {
                        discovery.OnServerFound.AddListener(OnServerFound);
                        discovery.StartDiscovery();
                    }
                }
                break;
            case MultiplayerBackend.Steam:
                RefreshSteamLobbies();
                break;
            default:
                break;
        }
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        if (!NetworkServer.active)
        {
            if (discovery != null)
            {
                discovery.OnServerFound.RemoveListener(OnServerFound);
                discovery.StopDiscovery();
            }
        }
    }
    #endregion

    #region Discovery Callbacks
    public void OnServerFound(ServerResponse response)
    {
        // Prevent duplicates
        if (discoveredServers.ContainsKey(response.serverId)) return;

        discoveredServers.Add(response.serverId, response);

        string title = $"{response.hostName}'s Lobby";

        string info = $"{response.playerCount} / {response.maxPlayers} Players";
        if (response.playerCount == response.maxPlayers) info += " (FULL)";

        AddSession(title, info: info, joinable: response.joinable, response: response);
    }
    #endregion

    #region Session List
    public void ClearFoundGames()
    {
        foreach (Transform child in foundGamesContent) 
        {
            Destroy(child.gameObject);
        }

        if(discoveredServers != null) discoveredServers.Clear();

        ResetSelection();
    }

    private void ResetSelection()
    {
        selectedSession = null;
        UpdateJoinButtonState(false);
    }

    // LAN
    public void AddSession(string title, string info, bool joinable, ServerResponse response)
    {
        SessionEntryUI entry = Instantiate(sessionEntryPrefab, foundGamesContent);
        entry.Initialize(title, info, joinable);
        entry.BindResponse(response);
    }

    // STEAM
    public void AddSteamSession(string title, string info, bool joinable, CSteamID lobbyId)
    {
        SessionEntryUI entry = Instantiate(sessionEntryPrefab, foundGamesContent);
        entry.Initialize(title, info, joinable);
        entry.BindSteamLobby(lobbyId);
    }
    #endregion

    #region Selection
    public void OnSessionSelected(SessionEntryUI entry)
    {
        if (selectedSession != null) selectedSession.SetSelected(false);

        selectedSession = entry;
        selectedSession.SetSelected(true);

        UpdateJoinButtonState(entry.isJoinable);
    }

    private void UpdateJoinButtonState(bool joinable)
    {
        joinButton.interactable = joinable;
        joinButtonCanvasGroup.alpha = joinable ? JOIN_ENABLED_ALPHA : JOIN_DISABLED_ALPHA;
    }
    #endregion

    #region Buttons
    public void OnHostPressed()
    {
        // Reset any selected join target
        ResetSelection();

        // Stop discovery before becoming host
        if (discovery != null)
        {
            discovery.StopDiscovery();
        }

        switch (Settings.Backend)
        {
            case MultiplayerBackend.Lan:
                NetworkManager.singleton.StartHost();
                break;
            case MultiplayerBackend.Steam:
                SteamLobbyManager.Instance.CreateLobby();
                break;
            default:
                break;
        }
    }

    public void OnJoinPressed()
    {
        if (selectedSession == null || !selectedSession.isJoinable) return;

        switch (Settings.Backend)
        {
            case MultiplayerBackend.Lan:
                if (!selectedSession.HasResponse)
                {
                    Debug.LogError("Selected session has no ServerResponse bound.");
                    return;
                }

                NetworkManager.singleton.networkAddress = "127.0.0.1";
                NetworkManager.singleton.StartClient();
                break;
            case MultiplayerBackend.Steam:
                if (!selectedSession.HasSteamLobby)
                {
                    Debug.LogError("Selected session has no Steam Lobby bound.");
                    return;
                }

                SteamLobbyManager.Instance.JoinLobby(selectedSession.SteamLobbyID);
                break;
            default:
                break;
        }
    }

    public void OnRefreshPressed()
    {
        // Reset any selected join target
        ResetSelection();

        switch (Settings.Backend)
        {
            case MultiplayerBackend.Lan:
                if (discovery != null) discovery.StopDiscovery();
                ClearFoundGames();
                if (discovery != null) discovery.StartDiscovery();
                break;
            case MultiplayerBackend.Steam:
                ClearFoundGames();
                RefreshSteamLobbies();
                break;
            default:
                break;
        }
    }

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalizedTexts();
    }

    private void RefreshLocalizedTexts()
    {
        if (PlayerProfile.IsValid)
        {
            welcomeText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Multiplayer", "MULTIPLAYER_WELCOME",
                arguments: new object[] { new { PlayerName = PlayerProfile.DisplayName } });
        }

        headerText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Multiplayer", "MULTIPLAYER_MULTIPLAYER");
        backText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Multiplayer", "MULTIPLAYER_BACK");
        hostText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Multiplayer", "MULTIPLAYER_HOST");
        joinText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Multiplayer", "MULTIPLAYER_JOIN");
        refreshText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Multiplayer", "MULTIPLAYER_REFRESH");
    }

    public void RefreshSteamLobbies()
    {
        SteamLobbyManager.Instance.RefreshLobbies();
    }

    public void ExitMultiplayerEntry()
    {
        // Reset any selected join target
        ResetSelection();

        // IMPORTANT: delegate close to MainMenuUI
        MainMenuUI.Instance.ExitMultiplayerEntry();
    }
    #endregion
}
