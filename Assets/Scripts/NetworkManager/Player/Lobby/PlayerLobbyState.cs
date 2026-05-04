using Mirror;
using UnityEngine;

public class PlayerLobbyState : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnNameChanged))]
    [HideInInspector] public string playerName;

    [SyncVar(hook = nameof(OnCharacterChanged))]
    [HideInInspector] public int selectedCharacterIndex = -1;

    [SyncVar(hook = nameof(OnReadyChanged))]
    [HideInInspector] public bool isReady;

    CharacterSelectorUIMultiplayer cachedSelector;

    public override void OnStartClient()
    {
        base.OnStartClient();

        var lobby = MultiplayerLobbyUI.Instance;

        if (!GamePhase.IsInLobby || !lobby || !lobby.isActiveAndEnabled) return;

        lobby.RegisterPlayer(this);

        if (isLocalPlayer)
        {
            cachedSelector = FindFirstObjectByType<CharacterSelectorUIMultiplayer>();
            cachedSelector?.SetLocalLobbyState(this);
        }
    }
    public override void OnStopClient()
    {
        MultiplayerLobbyUI.Instance?.UnregisterPlayer(this);
    }

    public override void OnStopServer()
    {
        ReleaseCharacter();
    }

    [Server]
    void ReleaseCharacter()
    {
        if (selectedCharacterIndex < 0) return;

        WartheonNetworkManager.Instance.UnlockCharacter(selectedCharacterIndex, netId);
        selectedCharacterIndex = -1;
    }

    public override void OnStartLocalPlayer()
    {
        // Send name ONCE when local player is ready
        CmdSetPlayerName(PlayerProfile.DisplayName);
    }

    #region Commands
    [Command]
    public void CmdSetPlayerName(string name)
    {
        playerName = name;
    }

    [Command]
    public void CmdSelectCharacter(int index)
    {
        if (isReady) return; // Character locked

        var manager = WartheonNetworkManager.Instance;

        // Deselect
        if (index < 0)
        {
            manager.ClearCharacterForConnection(connectionToClient);
            ReleaseCharacter();
            RpcCharacterConfirmed(-1);
            return;
        }

        // Reject if taken by someone else
        if (manager.IsCharacterTaken(index)) return;

        // Release old character
        ReleaseCharacter();

        // Populate connect id
        manager.SetCharacterForConnection(connectionToClient, index);

        // Lock new character
        manager.LockCharacter(index, netId);
        selectedCharacterIndex = index;

        // Confirm to client
        RpcCharacterConfirmed(index);
    }

    [ClientRpc]
    private void RpcCharacterConfirmed(int index)
    {
        // This tells the local UI: server accepted the character
        if (isLocalPlayer)
        {
            CharacterSelectorUIMultiplayer selector = FindFirstObjectByType<CharacterSelectorUIMultiplayer>();
            selector?.OnServerCharacterConfirmed(index);
        }
    }

    [Command]
    public void CmdRequestReadyChange()
    {
        // Cannot ready without a character
        if (!isReady && selectedCharacterIndex < 0) return;

        isReady = !isReady;
    }

    [Command]
    public void CmdRequestStartGame()
    {
        // Only host can start
        if (connectionToClient != NetworkServer.localConnection) return;

        WartheonNetworkManager.Instance.StartGameFromLobby();
    }
    #endregion

    // HOOKS
    void OnNameChanged(string oldValue, string newValue)
    {
        MultiplayerLobbyUI.Instance?.RefreshPlayer(this);
    }

    void OnCharacterChanged(int oldValue, int newValue)
    {
        if (!GamePhase.IsInLobby) return;

        MultiplayerLobbyUI.Instance?.RefreshPlayer(this);
    }

    void OnReadyChanged(bool oldValue, bool newValue)
    {
        if (!GamePhase.IsInLobby) return;

        MultiplayerLobbyUI.Instance?.RefreshPlayer(this);

        if (isLocalPlayer)
        {
            cachedSelector ??= FindFirstObjectByType<CharacterSelectorUIMultiplayer>();
            cachedSelector?.SetInteractionAllowed(!newValue);
        }
    }
}
