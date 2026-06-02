using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CharacterSelectorUI : MonoBehaviour
{
    [Header("REFERENCES")]
    [SerializeField] Canvas parentCanvas;
    [SerializeField] GameObject firstSelectedButton;
    [SerializeField] GameObject startGameButton;

    [Space(10)]
    [Header("CHARACTER DETAIL POPUPS")]
    [SerializeField] GameObject[] characterDetailsPopups;

    [Space(10)]
    [Header("TUTORIAL")]
    [SerializeField] Toggle tutorialToggle;
    [SerializeField] Image tutorialToggleCheckmarkImage;
    [SerializeField] SoundEffectSO buttonClickSound;

    [Space(10)]
    [Header("TEXTS")]
    [SerializeField] TMP_Text backButtonText;
    [SerializeField] TMP_Text startJourneyText;
    [SerializeField] TMP_Text tutorialText;
    [SerializeField] TMP_Text selectCharacterText;

    [SerializeField] float selectionCooldown = 0.15f;
    float lastSelectionTime;

    PlayerDetailsSO[] playerDetailsList;
    CurrentPlayerSO currentPlayer;

    int selectedCharacterIndex = 1;
    int currentOpenIndex = -1;

    Button startButton;
    CanvasGroup startButtonGroup;

    #region Unity Lifecycle
    private void Awake()
    {
        playerDetailsList = GameResources.Instance.playerDetailsArray;
        currentPlayer = GameResources.Instance.currentPlayer;

        startButton = startGameButton.GetComponent<Button>();
        startButtonGroup = startGameButton.GetComponent<CanvasGroup>();
    }
    void OnEnable()
    {
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        RefreshLocalizedTexts();

        // Delay selection until the next frame to ensure UI is ready
        StartCoroutine(SetFirstSelected());

        // Load Tutorial toggle
        LoadTutorialState();

        StaticEventHandler.OnCharacterButtonSelected += StaticEventHandler_OnCharacterButtonSelected;
        StaticEventHandler.OnCharacterButtonDeselected += StaticEventHandler_OnCharacterButtonDeselected;

        // UI Element listeners
        tutorialToggle.onValueChanged.AddListener(OnTutorialToggleChanged);

        SetStartButtonState(false);
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;

        StaticEventHandler.OnCharacterButtonSelected -= StaticEventHandler_OnCharacterButtonSelected;
        StaticEventHandler.OnCharacterButtonDeselected -= StaticEventHandler_OnCharacterButtonDeselected;

        // UI Element listeners
        tutorialToggle.onValueChanged.RemoveListener(OnTutorialToggleChanged);
    }
    #endregion

    #region Button Input (PC + Gamepad)
    // Called from Button.onClick(int)
    public void OnCharacterClicked(int index)
    {
        if (Time.unscaledTime - lastSelectionTime < selectionCooldown) return; // To prevent undesired double click issues

        lastSelectionTime = Time.unscaledTime;

        if (index == currentOpenIndex)
        {
            StaticEventHandler.CallCharacterButtonDeselectedEvent();
            return;
        }

        StaticEventHandler.CallCharacterButtonSelectedEvent((Character)index);
    }

    private void StaticEventHandler_OnCharacterButtonSelected(CharacterButtonArgs args)
    {
        if (Time.unscaledTime - lastSelectionTime < selectionCooldown) return; // To prevent undesired double click issues

        SelectCharacter((int)args.charIndex);
    }

    private void StaticEventHandler_OnCharacterButtonDeselected()
    {
        if (Time.unscaledTime - lastSelectionTime < selectionCooldown) return; // To prevent undesired double click issues

        ClearSelection();
    }
    #endregion

    #region Selection logic
    private void SelectCharacter(int index)
    {
        if (index < 0 || index >= playerDetailsList.Length) return;

        DisableAllPopUps();

        selectedCharacterIndex = index;
        currentOpenIndex = index;

        currentPlayer.playerDetails = playerDetailsList[index];
        characterDetailsPopups[index].SetActive(true);

        SetStartButtonState(true);
    }

    private void ClearSelection()
    {
        selectedCharacterIndex = -1;
        currentOpenIndex = -1;

        DisableAllPopUps();
        SetStartButtonState(false);
    }

    private void DisableAllPopUps()
    {
        foreach (var popup in characterDetailsPopups)
        {
            popup.SetActive(false);
        }
    }
    #endregion

    #region Start Game
    public void StartGame()
    {
        if(selectedCharacterIndex < 0)
        {
            Debug.LogWarning("StartGame called with no character selected.");
            return;
        }

        // Save the selected character immediately before any scene transition
        currentPlayer.playerDetails = playerDetailsList[selectedCharacterIndex];
        PlayerPrefs.SetInt("SelectedCharacterIndex", selectedCharacterIndex);
        PlayerPrefs.Save();

        // Safe loading call
        if (LoadingManager.SafeInstance != null)
        {
            LoadingManager.SafeInstance.StartCoroutine(LoadingManager.SafeInstance.LoadGameScene(2));
        }
        else
        {
            Debug.LogError("LoadingManager instance missing! Loading directly...");

            StaticEventHandler.ClearAll(); // Clear all events before start (to prevent holding old data in case of restarts)
            SceneManager.LoadScene(2);
        }
    }
    #endregion

    #region Tutorial
    private void LoadTutorialState()
    {
        bool enabled = PlayerPrefs.GetInt("Tutorial", 1) == 1;
        tutorialToggle.SetIsOnWithoutNotify(enabled);
        InputManager.TutorialEnabled = enabled;
    }

    private void OnTutorialToggleChanged(bool isOn)
    {
        if (InputManager.TutorialEnabled != isOn) SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        InputManager.TutorialEnabled = isOn;
        PlayerPrefs.SetInt("Tutorial", isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
    #endregion

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalizedTexts();
    }

    private void RefreshLocalizedTexts()
    {
        backButtonText.text = LocalizationSettings.StringDatabase.GetLocalizedString("SinglePlayer", "SINGLEPLAYER_BACK");
        startJourneyText.text = LocalizationSettings.StringDatabase.GetLocalizedString("SinglePlayer", "SINGLEPLAYER_START_JOURNEY");
        tutorialText.text = LocalizationSettings.StringDatabase.GetLocalizedString("SinglePlayer", "SINGLEPLAYER_PLAY_TUTORIAL");
        selectCharacterText.text = LocalizationSettings.StringDatabase.GetLocalizedString("SinglePlayer", "SINGLEPLAYER_SELECT");
    }

    #region Helpers
    private void SetStartButtonState(bool enabled)
    {
        startButton.interactable = enabled;
        startButtonGroup.interactable = enabled;
        startButtonGroup.blocksRaycasts = enabled;
        startButtonGroup.alpha = enabled ? 1f : 0.4f; // Visual feedback
    }

    private IEnumerator SetFirstSelected()
    {
        yield return null; // wait 1 frame

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }
    #endregion
}
