using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;

[DisallowMultipleComponent]
public class CharacterSelectorUIMultiplayer : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] GameObject firstSelectedButton;

    [Header("CHARACTER DETAIL POPUPS")]
    [SerializeField] GameObject[] characterDetailsPopups;
    [SerializeField] CanvasGroup characterButtonsGroup;

    [SerializeField] float selectionCooldown = 0.15f;

    // Multiplayer (assigned later, NOT in Awake)
    PlayerLobbyState localLobbyState;

    float lastSelectionTime;
    bool serverConfirmedCharacter = false;

    PlayerDetailsSO[] playerDetailsArray;

    #region Unity Lifecycle
    private void Awake()
    {
        // Local-only data
        playerDetailsArray = GameResources.Instance.playerDetailsArray;

        // Networking must NOT be touched here
        localLobbyState = null;

        // DO NOT touch networking here
        localLobbyState = null;
    }

    private void OnEnable()
    {
        // Delay selection until the next frame to ensure UI is ready
        StartCoroutine(SetFirstSelected());

        StaticEventHandler.OnCharacterButtonSelected += OnCharacterSelected;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnCharacterButtonSelected -= OnCharacterSelected;
    }
    #endregion

    #region UI Input
    // Called from Button.onClick(int)
    public void OnCharacterClicked(int index)
    {
        if (Time.unscaledTime - lastSelectionTime < selectionCooldown) return; // To prevent undesired double click issues

        lastSelectionTime = Time.unscaledTime;

        StaticEventHandler.CallCharacterButtonSelectedEvent((Character)index);
    }

    private void OnCharacterSelected(CharacterButtonArgs args)
    {
        if (Time.unscaledTime - lastSelectionTime < selectionCooldown) return; // To prevent undesired double click issues

        SelectCharacter((int)args.charIndex);
    }

    public void OnServerCharacterConfirmed(int index)
    {
        serverConfirmedCharacter = true;
    }

    public void OnReadyStateChanged(bool isReady)
    {
        // Disable character buttons when ready
        if (characterButtonsGroup != null)
        {
            SetInteractionAllowed(isReady);
        }
    }
    #endregion

    #region Selection logic
    private void SelectCharacter(int index)
    {
        if (index < 0 || index >= playerDetailsArray.Length) return;

        DisableAllPopups();

        characterDetailsPopups[index].SetActive(true);

        // Network Intent
        if (localLobbyState != null)
        {
            localLobbyState.CmdSelectCharacter(index);
        }
    }

    private void DisableAllPopups()
    {
        foreach (var popup in characterDetailsPopups) popup.SetActive(false);
    }
    #endregion

    #region Multiplayer Hook
    /// <summary>
    /// Called by MultiplayerLobbyUI AFTER PlayerLobbyState is available
    /// </summary>
    public void SetLocalLobbyState(PlayerLobbyState lobbyState)
    {
        localLobbyState = lobbyState;

        // Sync UI with already-selected character (late join / reconnect)
        if (localLobbyState != null && localLobbyState.selectedCharacterIndex >= 0)
        {
            SelectCharacter(localLobbyState.selectedCharacterIndex);
        }
    }
    #endregion

    #region Helpers
    public void SetInteractionAllowed(bool allowed)
    {
        characterButtonsGroup.interactable = allowed;
        characterButtonsGroup.blocksRaycasts = allowed;
        characterButtonsGroup.alpha = allowed ? 1f : 0.4f;
    }

    private IEnumerator SetFirstSelected()
    {
        yield return null;
        if (firstSelectedButton != null)
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
    }
    #endregion
}
