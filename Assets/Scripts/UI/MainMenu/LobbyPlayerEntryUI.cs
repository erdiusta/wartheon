using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class LobbyPlayerEntryUI : MonoBehaviour
{
    [Header("TEXTS")]
    [SerializeField] TextMeshProUGUI playerNameText;
    [SerializeField] TextMeshProUGUI characterText;

    [Header("CHARACTER IMAGE")]
    [SerializeField] Image characterImage;
    [SerializeField] Sprite noSprite;

    [Header("READY TOGGLE")]
    [SerializeField] Toggle readyToggle;
    [SerializeField] CanvasGroup readyToggleCanvasGroup;
    [SerializeField] TextMeshProUGUI readyStatusText;
    [SerializeField] Color readyColor = Color.green;
    [SerializeField] Color notReadyColor = Color.red;
    [SerializeField] SoundEffectSO clickSound;

    PlayerLobbyState boundPlayer;

    public void Bind(PlayerLobbyState player)
    {
        boundPlayer = player;
        Refresh();
    }

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged += OnLanguageChanged;
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
    }

    public void Refresh()
    {
        if (boundPlayer == null) return;

        // PLAYER NAME
        playerNameText.text = string.IsNullOrEmpty(boundPlayer.playerName) ? "Connecting..." : boundPlayer.playerName;

        // CHARACTER
        if (boundPlayer.selectedCharacterIndex >= 0)
        {
            characterText.text = ((Character)boundPlayer.selectedCharacterIndex).ToString();
            characterImage.sprite = MultiplayerLobbyUI.Instance.characterSpritesArray[boundPlayer.selectedCharacterIndex];
        }
        else
        {
            characterText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_NO_CHARACTER");
            characterImage.sprite = noSprite;
        }

        readyToggle.SetIsOnWithoutNotify(boundPlayer.isReady);
        //SoundEffectManager.Instance.PlaySoundEffect(clickSound);

        // Ready toggle rules
        readyToggle.interactable = boundPlayer.isLocalPlayer && boundPlayer.selectedCharacterIndex >= 0;

        if (boundPlayer.isLocalPlayer)
        {
            if (boundPlayer.selectedCharacterIndex >= 0) readyToggleCanvasGroup.alpha = 1f;
            else readyToggleCanvasGroup.alpha = 0.4f;
        }
        else
        {
            readyToggleCanvasGroup.alpha = 0.4f;
        }

        readyStatusText.text = boundPlayer.isReady ? LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_READY")
            : LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_NOT_READY");
        readyStatusText.color = boundPlayer.isReady ? readyColor : notReadyColor;
    }

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalizedTexts();
    }

    private void RefreshLocalizedTexts()
    {
        // CHARACTER
        if (boundPlayer.selectedCharacterIndex >= 0) characterText.text = ((Character)boundPlayer.selectedCharacterIndex).ToString();
        else characterText.text = LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_NO_CHARACTER");

        readyStatusText.text = boundPlayer.isReady ? LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_READY")
            : LocalizationSettings.StringDatabase.GetLocalizedString("MultiplayerLobby", "MULTIPLAYER_NOT_READY");
        readyStatusText.color = boundPlayer.isReady ? readyColor : notReadyColor;
    }

    public void OnReadyToggled(bool value)
    {
        if (!boundPlayer.isLocalPlayer) return;

        // Ask server to toggle, NOT set
        boundPlayer.CmdRequestReadyChange();
    }
}
