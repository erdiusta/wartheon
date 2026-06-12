using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class MultiplayerLobbyUI : SingletonMonobehaviour<MultiplayerLobbyUI>
{
    [Header("Buttons")]
    [SerializeField] Button startGameButton;
    [SerializeField] CanvasGroup startButtonCanvasGroup;
    [SerializeField] Button leaveLobbyButton;

    [Header("Texts")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text selectYourCharacterText;
    [SerializeField] TMP_InputField chatInput;
    [SerializeField] TMP_Text chatPlaceholderText;
    [SerializeField] TMP_Text chatLog;
    [SerializeField] TMP_Text sendText;
    [SerializeField] TMP_Text startGameText;
    [SerializeField] TMP_Text leaveLobbyText;
    [SerializeField] ScrollRect scrollRect;

    [Header("Player list")]
    [SerializeField] Transform playerListRoot;
    [SerializeField] LobbyPlayerEntryUI playerEntryPrefab;

    [Header("Selected char's sprite")]
    public Sprite[] characterSpritesArray;

    // Runtime mapping
    Dictionary<uint, LobbyPlayerEntryUI> playerEntries = new();

    public GameObject DefaultSelectable => leaveLobbyButton.gameObject;

    bool isHost;
    bool isAlive = true;

    private void Start()
    {
        chatInput.onSubmit.AddListener(_ => OnChatSubmit());
    }

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        RefreshLocalizedTexts();
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
    }

    public void Initialize(bool host)
    {
        isHost = host;

        RefreshStartGameState();
        ClearChatWindows();

        // Register already-spawned players (host case)
        PlayerLobbyState[] players = FindObjectsByType<PlayerLobbyState>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            RegisterPlayer(player);
        }

        EventSystem.current.SetSelectedGameObject(DefaultSelectable);
    }

    private bool IsHost(PlayerLobbyState p)
    {
        var conn = p.netIdentity.connectionToClient;
        return conn != null && conn.connectionId == 0; // ConnectionID 0 means Host
    }

    public void RegisterPlayer(PlayerLobbyState player)
    {
        if (!GamePhase.IsInLobby || !enabled) return;

        if (playerEntries.ContainsKey(player.netId)) return;

        LobbyPlayerEntryUI newPlayerEntry = Instantiate(playerEntryPrefab, playerListRoot);
        newPlayerEntry.Bind(player);

        playerEntries[player.netId] = newPlayerEntry;

        RefreshHeader();
        RefreshStartGameState();
    }

    public void UnregisterPlayer(PlayerLobbyState player)
    {
        if (!playerEntries.TryGetValue(player.netId, out var entry)) return;

        // Remove from data structures FIRST
        playerEntries.Remove(player.netId);

        // Update UI state while references are still valid
        RefreshHeader();
        RefreshStartGameState();

        // Destroy LAST
        if (entry != null && entry.gameObject != null)
        {
            entry.gameObject.SetActive(false);
        }
    }

    public void RefreshPlayer(PlayerLobbyState player)
    {
        if (!playerEntries.TryGetValue(player.netId, out var entry))
        {
            RegisterPlayer(player);
            return;
        }

        entry.Bind(player);

        RefreshHeader();
        RefreshStartGameState();
    }

    private void RefreshHeader()
    {
        PlayerLobbyState[] players = FindObjectsByType<PlayerLobbyState>(FindObjectsSortMode.None);

        foreach (PlayerLobbyState player in players)
        {
            if (IsHost(player))
            {
                headerText.text = $"Multiplayer Lobby: {player.playerName}'s Game";
                return;
            }
        }

        headerText.text = "Multiplayer Lobby";
    }

    public void ShutdownLobby()
    {
        // Stop future UI logic first
        isAlive = false;

        ClearAllEntries();

        // Safety: prevent late callbacks
        enabled = false;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        isAlive = false;
    }

    #region Buttons
    public void OnStartGamePressed()
    {
        if (!isHost) return;

        PlayerLobbyState local = GetLocalPlayer();

        if (local == null)
        {
            Debug.LogWarning("Local PlayerLobbyState not found.");
            return;
        }

        local.CmdRequestStartGame();
    }
    #endregion

    #region Chat
    public void OnChatSubmit()
    {
        if (string.IsNullOrWhiteSpace(chatInput.text)) return;

        PlayerLobbyState localPlayer = GetLocalPlayer();

        if (localPlayer != null)
        {
            localPlayer.CmdSendChatMessage(chatInput.text);
        }

        chatInput.SetTextWithoutNotify("");
        chatInput.ActivateInputField();
    }

    public void ReceiveChatMessage(string sender, string message)
    {
        chatLog.text += $"{sender}: {message}\n";

        chatLog.ForceMeshUpdate();
        Canvas.ForceUpdateCanvases();

        LayoutRebuilder.ForceRebuildLayoutImmediate(chatLog.rectTransform);

        scrollRect.verticalNormalizedPosition = 0f;
    }
    #endregion

    #region Start Button
    private void RefreshStartGameState()
    {
        if (!isHost)
        {
            SetStartButton(false);
            return;
        }

        PlayerLobbyState[] players = FindObjectsByType<PlayerLobbyState>(FindObjectsSortMode.None);

        if (players.Length < 2)
        {
            SetStartButton(false);
            return;
        }

        foreach (var p in players)
        {
            if (!p.isReady)
            {
                SetStartButton(false);
                return;
            }
        }

        SetStartButton(true);
    }

    private void SetStartButton(bool enabled)
    {
        if (!isAlive) return;
        if (!startButtonCanvasGroup) return; // Unity fake-null safe

        startGameButton.interactable = enabled;
        startButtonCanvasGroup.blocksRaycasts = enabled;
        startButtonCanvasGroup.alpha = enabled ? 1f : 0.4f;
    }
    #endregion

    #region Helpers
    private PlayerLobbyState GetLocalPlayer()
    {
        PlayerLobbyState[] players = FindObjectsByType<PlayerLobbyState>(FindObjectsSortMode.None);

        foreach (var player in players)
        {
            if (player != null && player.isLocalPlayer) return player;
        }

        return null;
    }

    private void ClearChatWindows()
    {
        chatLog.SetText("");
        chatInput.SetTextWithoutNotify("");
    }

    public void ClearAllEntries()
    {
        foreach (var entry in playerEntries.Values)
        {
            Destroy(entry.gameObject);
        }

        playerEntries.Clear();
    }
    #endregion

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalizedTexts();
    }

    private void RefreshLocalizedTexts()
    {
        headerText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_HEADER",
            arguments: new object[] { new { PlayerName = PlayerProfile.DisplayName } });

        selectYourCharacterText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_CHARACTER");
        chatPlaceholderText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_PLACEHOLDER");
        sendText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_SEND");
        startGameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_START_GAME");
        leaveLobbyText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_LEAVE_LOBBY");
    }

    public void ExitMultiplayerLobby()
    {
        GameSessionState.ResetSession();

        if (Settings.Backend == MultiplayerBackend.Steam)
        {
            SteamLobbyManager.Instance.LeaveCurrentLobby();
        }

        // IMPORTANT: delegate close to MainMenuUI
        MainMenuUI.Instance.ExitMultiplayerLobby();
        ClearAllEntries();
    }
}
