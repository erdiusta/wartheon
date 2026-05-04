using System.Collections.Generic;
using Mirror;
using Mirror.Discovery;
using TMPro;
using UnityEngine;
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

        if (discovery == null)
        {
            Debug.LogError("WartheonNetworkDiscovery not found in scene!");
        }
    }

    private void OnEnable()
    {
        if (PlayerProfile.IsValid) welcomeText.text = $"Welcome {PlayerProfile.DisplayName}! Finding Games...";

        ClearFoundGames();
        ResetSelection();

        if (!NetworkServer.active)
        {
            discovery.OnServerFound.AddListener(OnServerFound);
            discovery.StartDiscovery();
        }
    }

    private void OnDisable()
    {
        if (!NetworkServer.active)
        {
            discovery.OnServerFound.RemoveListener(OnServerFound);
            discovery.StopDiscovery();
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
    private void ClearFoundGames()
    {
        foreach (Transform child in foundGamesContent) 
        {
            Destroy(child.gameObject);
        }

        discoveredServers.Clear();
    }

    private void ResetSelection()
    {
        selectedSession = null;
        UpdateJoinButtonState(false);
    }

    public void AddSession(string title, string info, bool joinable, ServerResponse response)
    {
        SessionEntryUI entry = Instantiate(sessionEntryPrefab, foundGamesContent);
        entry.Initialize(title, info, joinable);
        entry.BindResponse(response);
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
        NetworkManager.singleton.StartHost();
    }

    public void OnJoinPressed()
    {
        if (selectedSession == null || !selectedSession.isJoinable) return;

        if (!selectedSession.HasResponse)
        {
            Debug.LogError("Selected session has no ServerResponse bound.");
            return;
        }

        //NetworkManager.singleton.StartClient(selectedSession.BoundResponse.uri);

        NetworkManager.singleton.networkAddress = "127.0.0.1";
        NetworkManager.singleton.StartClient();
    }

    public void OnRefreshPressed()
    {
        discovery.StopDiscovery();
        discovery.StartDiscovery();

        ClearFoundGames();
        ResetSelection();

        if (discovery != null) discovery.StartDiscovery();
    }

    public void ExitMultiplayerEntry()
    {
        // IMPORTANT: delegate close to MainMenuUI
        MainMenuUI.Instance.ExitMultiplayerEntry();
    }
    #endregion
}
