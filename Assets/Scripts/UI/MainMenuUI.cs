using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.Localization.Settings;
using System.Linq;

public class MainMenuUI : SingletonMonobehaviour<MainMenuUI>
{
    public static int currentDungeonLevelListIndex = 0;

    public Button singlePlayerButton;
    public Button multiplayerButton;
    [SerializeField] GameObject characterSelectorUI;
    [SerializeField] Button settingsButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button profileButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject cheatCodeObject;

    [Space(10)]
    [Header("MULTIPLAYER")]
    [SerializeField] GameObject multiplayerEntryUI;
    [SerializeField] GameObject multiplayerLobbyUI;
    [SerializeField] GameObject hostButton;

    [Space(10)]
    [Header("SETTINGS")]
    [SerializeField] GameObject settingsMenuUI;
    [SerializeField] Button settingsBackButton;
    [Header("Video")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown screenModeDropdown;
    [SerializeField] TMP_Dropdown refreshRateDropdown;
    [SerializeField] Toggle postProcessingToggle;
    [SerializeField] Image postProcessingCheckmarkImage;
    [SerializeField] Toggle vsyncToggle;
    [SerializeField] Image vysncCheckmarkImage;

    [Space(10)]
    [Header("Audio")]
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider soundVolumeSlider;

    [Space(10)]
    [Header("Game")]
    [SerializeField] TMP_Dropdown languageDropdown;
    [SerializeField] Toggle dynamicCameraToggle;
    [SerializeField] Image dynamicCameraCheckmarkImage;

    [Space(10)]
    [Header("CONTROLS")]
    [SerializeField] GameObject controlsMenuUI;
    [SerializeField] Button keyboardMouseButton;
    [SerializeField] Button gamepadButton;
    [SerializeField] Button controlsBackButton;
     
    [Header("Keyboard&Mouse Rebindings Menu")]
    [SerializeField] GameObject keyboardRebindingsMenuUI;
    [SerializeField] Button keyboardRebindingsBackButton;

    [Header("Gamepad Rebindings Menu")]
    [SerializeField] GameObject gamepadRebindingsMenuUI;
    [SerializeField] Button gamepadRebindingsBackButton;

    [Space(10)]
    [Header("PROFILE")]
    [SerializeField] GameObject profileMenuUI;

    Resolution[] resolutions;
    Dictionary<string, List<int>> resolutionToHzMap;
    List<string> resolutionOptions;

    InputActionAsset actions;

    protected override void Awake()
    {
        base.Awake();

        PlayerProfile.Load();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnCheatActivated += StaticEventHandler_OnCheatActivated;
        StaticEventHandler.OnAdditiveSceneRemoved += StaticEventHandler_OnAdditiveSceneRemoved;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnCheatActivated -= StaticEventHandler_OnCheatActivated;
        StaticEventHandler.OnAdditiveSceneRemoved -= StaticEventHandler_OnAdditiveSceneRemoved;
    }

    private void StaticEventHandler_OnAdditiveSceneRemoved()
    {
        ActivateAllPrimaryMainMenuButtons();
    }

    private void StaticEventHandler_OnCheatActivated()
    {
        StartCoroutine(FlashCheatCodePrompt());
    }

    IEnumerator FlashCheatCodePrompt()
    {
        cheatCodeObject.SetActive(true);

        yield return new WaitForSeconds(3f);

        cheatCodeObject.SetActive(false);
    }

    void Start()
    {
        RefreshLocalizedTexts();

        actions = InputManager.Instance.actions;

        // Initialize and categorize resolutions
        resolutions = Screen.resolutions;
        resolutionToHzMap = new Dictionary<string, List<int>>();
        resolutionOptions = new List<string>();

        foreach (Resolution res in resolutions)
        {
            string key = $"{res.width} x {res.height}";
            int hz = (int)res.refreshRateRatio.value;

            if (!resolutionToHzMap.ContainsKey(key))
            {
                resolutionToHzMap[key] = new List<int>();
                resolutionOptions.Add(key);
            }

            if (!resolutionToHzMap[key].Contains(hz)) resolutionToHzMap[key].Add(hz);
        }

        // Sort Hz lists
        foreach (KeyValuePair<string, List<int>> kvp in resolutionToHzMap) kvp.Value.Sort();

        // Populate resolution dropdown
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.onValueChanged.AddListener(index => OnResolutionDropdownChanged(index, false));

        // If has no player pref select max resolution in default
        bool resolutionPrefSeemsValid = false;

        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int savedIndex = PlayerPrefs.GetInt("ResolutionIndex");
            if (savedIndex > 0 && savedIndex < resolutionOptions.Count)
            {
                resolutionPrefSeemsValid = true;
            }
        }

        if (!resolutionPrefSeemsValid)
        {
            int highestIndex = 0;
            int maxPixels = 0;

            for (int i = 0; i < resolutionOptions.Count; i++)
            {
                string[] parts = resolutionOptions[i].Split('x');
                int width = int.Parse(parts[0].Trim());
                int height = int.Parse(parts[1].Trim());

                int totalPixels = width * height;
                if (totalPixels > maxPixels)
                {
                    maxPixels = totalPixels;
                    highestIndex = i;
                }
            }

            resolutionDropdown.value = highestIndex;
            OnResolutionDropdownChanged(highestIndex, true); // make sure Hz list is also updated
        }

        // Populate language options
        PopulateLanguageDropdown();

        // For first launch default dynamiceCameraFollowToggle is false
        dynamicCameraToggle.isOn = false;

        //  Now that resolutions and dropdown are ready, load saved settings
        LoadPlayerPrefs();

        // Populate screen modes
        screenModeDropdown.ClearOptions();
        screenModeDropdown.AddOptions(new List<string> { "Exclusive Fullscreen", "Borderless Window", "Windowed" });
        if (PlayerPrefs.HasKey("ScreenModeIndex")) screenModeDropdown.value = PlayerPrefs.GetInt("ScreenModeIndex");
        else screenModeDropdown.value = (int)Screen.fullScreenMode;
        screenModeDropdown.RefreshShownValue();

        // Play music
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        // Add other listeners
        postProcessingToggle.onValueChanged.AddListener(OnPostProcessingToggleChanged);
        vsyncToggle.onValueChanged.AddListener(OnVsyncToggleChanged);
        refreshRateDropdown.onValueChanged.AddListener(index => OnRefreshRateChanged(index, false));
        screenModeDropdown.onValueChanged.AddListener(SetScreenMode);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeSliderChanged);
        languageDropdown.onValueChanged.AddListener(OnLanguageChanged);
        dynamicCameraToggle.onValueChanged.AddListener(OnDynamicCameraFollowToggleChanged);
    }
  
    private void Update()
    {
        if (InputManager.Instance.escapeButton.action.WasPressedThisFrame())
        {
            if (multiplayerEntryUI.activeSelf)
            {
                ExitMultiplayerEntry();
            }

            if (settingsMenuUI.activeSelf)
            {
                ExitSettingMenu();
            }

            if (controlsMenuUI.activeSelf)
            {
                if (keyboardRebindingsMenuUI.activeSelf)
                {
                    ReturnToControlsMenu();
                    EventSystem.current.SetSelectedGameObject(keyboardMouseButton.gameObject);
                }
                else if (gamepadRebindingsMenuUI.activeSelf)
                {
                    ReturnToControlsMenu();
                    EventSystem.current.SetSelectedGameObject(gamepadButton.gameObject);
                }
                else
                {
                    ExitControlsMenu();
                }
            }

            if (profileMenuUI.activeSelf)
            {
                ExitProfileMenu();
            }

            if (characterSelectorUI.activeSelf)
            {
                if (InputManager.Instance.escapeButton.action.WasPressedThisFrame())
                {
                    BackButtonForCharacterSelection();
                }
            }
        }
    }

    public void BackButtonForCharacterSelection()
    {
        // Set Play Button as selected
        StartCoroutine(HandleReturnFromCharacterScene());

        // Unload the current additive scene
        StaticEventHandler.CallAdditiveSceneRemoveEvent();

        characterSelectorUI.SetActive(false);
    }

    /// <summary>
    /// Called from the Single Player Button
    /// </summary>
    public void OpenSingleplayer()
    {
        DeactivateAllPrimaryMainMenuButtons();

        // Save player prefs
        SavePlayerPrefs();

        // Load character selector scene additively
        characterSelectorUI.SetActive(true);
    }

    /// <summary>
    /// Called from the Multiplayer Button
    /// </summary>
    public void OpenMultiplayer()
    {
        if (!PlayerProfile.IsValid)
        {
            OpenProfile();   // reuse Profile popup
            return;
        }

        multiplayerEntryUI.SetActive(true);
        DeactivateAllPrimaryMainMenuButtons();
    }

    /// <summary>
    /// Called from the join or host button
    /// </summary>
    public void OpenMultiplayerLobby(bool isHost)
    {
        multiplayerEntryUI.SetActive(false);
        multiplayerLobbyUI.SetActive(true);

        MultiplayerLobbyUI.Instance.Initialize(isHost);
    }

    /// <summary>
    /// Called from the Controls Button
    /// </summary>
    public void OpenControls()
    {
        controlsMenuUI.SetActive(true);

        DeactivateAllPrimaryMainMenuButtons();
    }

    /// <summary>
    /// Called from the Settings Button
    /// </summary>
    public void OpenSettings()
    {
        settingsMenuUI.SetActive(true);

        DeactivateAllPrimaryMainMenuButtons();
    }

    /// <summary>
    /// Called from the Profile Button
    /// </summary>
    public void OpenProfile()
    {
        profileMenuUI.SetActive(true);

        DeactivateAllPrimaryMainMenuButtons();
    }


    /// <summary>
    /// Called from the Keyboard&Mouse Button
    /// </summary>
    public void OpenKeyboardMouseRebindingsMenu()
    {
        keyboardRebindingsMenuUI.SetActive(true);

        keyboardMouseButton.interactable = false;
        gamepadButton.interactable = false;
        controlsBackButton.interactable = false;

        keyboardMouseButton.gameObject.SetActive(false);
        gamepadButton.gameObject.SetActive(false);
        controlsBackButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called from the Gamepad Button
    /// </summary>
    public void OpenGamepadRebindingsMenu()
    {
        gamepadRebindingsMenuUI.SetActive(true);

        keyboardMouseButton.interactable = false;
        gamepadButton.interactable = false;
        controlsBackButton.interactable = false;

        keyboardMouseButton.gameObject.SetActive(false);
        gamepadButton.gameObject.SetActive(false);
        controlsBackButton.gameObject.SetActive(false);
    }

    private void OnPostProcessingToggleChanged(bool isOn)
    {
        PostProcessingEnabler.Instance.isOn = isOn;
        UpdatePostProcessingCheckmarkVisibility(isOn);
    }

    private void OnResolutionDropdownChanged(int index, bool onStart)
    {
        // Force dropdown's actual value to match the selected index
        if (onStart)
        {
            if (resolutionDropdown.value != index) resolutionDropdown.SetValueWithoutNotify(index);
        }

        string selectedRes = resolutionOptions[index];
        List<int> hzOptions = resolutionToHzMap[selectedRes];

        // Convert Hz list to readable labels
        List<string> hzLabels = hzOptions.ConvertAll(hz => hz + " Hz");

        refreshRateDropdown.ClearOptions();
        refreshRateDropdown.AddOptions(hzLabels);

        // Select refresh rate properly
        int targetHzIndex;

        if (onStart)
        {
            targetHzIndex = PlayerPrefs.HasKey("RefreshRateIndex") 
                ? Mathf.Clamp(PlayerPrefs.GetInt("RefreshRateIndex"), 0, hzOptions.Count - 1)
                : hzOptions.Count - 1; // default to highest Hz
        }
        else
        {
            targetHzIndex = Mathf.Clamp(refreshRateDropdown.value, 0, hzOptions.Count - 1);
        }

        refreshRateDropdown.value = targetHzIndex;
        refreshRateDropdown.RefreshShownValue();

        // Immediately apply resolution with new Hz
        ApplyResolution(onStart);
    }

    private void OnRefreshRateChanged(int hzIndex, bool onStart)
    {
        ApplyResolution(onStart);
    }

    private void ApplyResolution(bool onStart)
    {
        string selectedRes = resolutionOptions[resolutionDropdown.value];
        string[] parts = selectedRes.Split('x');
        int width = int.Parse(parts[0].Trim());
        int height = int.Parse(parts[1].Trim());

        List<int> hzList = resolutionToHzMap[selectedRes];
        int safeHzIndex = Mathf.Clamp(refreshRateDropdown.value, 0, hzList.Count - 1);
        int selectedHz = hzList[safeHzIndex];

        // Create the RefreshRate struct directly
        RefreshRate refreshRate = new RefreshRate
        {
            numerator = (uint)selectedHz,
            denominator = 1
        };

        Screen.SetResolution(width, height, Screen.fullScreenMode, refreshRate);
    }

    private void SetScreenMode(int modeIndex)
    {
        switch (modeIndex)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            default:
                break;
        }
    }

    private void UpdatePostProcessingCheckmarkVisibility(bool show)
    {
        postProcessingCheckmarkImage.enabled = show;
    }

    private void OnVsyncToggleChanged(bool isOn)
    {
        QualitySettings.vSyncCount = isOn ? 1 : 0;
        UpdateVysncCheckmarkVisibility(isOn);
    }

    private void UpdateVysncCheckmarkVisibility(bool show)
    {
        vysncCheckmarkImage.enabled = show;
    }

    private void OnDynamicCameraFollowToggleChanged(bool isOn)
    {
        InterScenesSingleton.dynamicCameraFollowEnabled = isOn ? true : false;
        UpdateDynamicCameraFollowCheckmarkVisibility(isOn);
    }

    private void UpdateDynamicCameraFollowCheckmarkVisibility(bool show)
    {
        dynamicCameraCheckmarkImage.enabled = show;
    }

    private void OnMusicVolumeSliderChanged(float newValue)
    {
        MusicManager.Instance.SetVolume((int)newValue);
    }

    private void OnSoundVolumeSliderChanged(float newValue)
    {
        SoundEffectManager.Instance.SetVolume((int)newValue);
    }

    private void PopulateLanguageDropdown()
    {
        languageDropdown.ClearOptions();

        List<string> languageOptions = Enum.GetNames(typeof(Language)).ToList();
        languageDropdown.AddOptions(languageOptions);
    }

    private void OnLanguageChanged(int languageIndex)
    {
        Language selectedLanguage = (Language)languageIndex;

        switch (selectedLanguage)
        {
            case Language.English:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("en");
                break;
            case Language.German:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("de");
                break;
            case Language.French:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("fr");
                break;
            case Language.Spanish:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("es");
                break;
            case Language.Portuguese:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("pt");
                break;
            case Language.Turkish:
                LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.GetLocale("tr");
                break;
            default:
                break;
        }

        //LocalizationManager.RefreshAllTexts();

        RefreshLocalizedTexts();
        LocalizationManager.LanguageChanged?.Invoke(selectedLanguage);
    }

    public void OpenKeyboardControls()
    {
        // Save player prefs
        SavePlayerPrefs();
    }

    public void ExitMultiplayerEntry()
    {
        multiplayerEntryUI.SetActive(false);
        ActivateAllPrimaryMainMenuButtons();
        EventSystem.current.SetSelectedGameObject(multiplayerButton.gameObject);
    }

    public void ExitMultiplayerLobby()
    {
        // Stop networking properly
        if (NetworkClient.isConnected)
        {
            // Close lobby ui
            multiplayerLobbyUI.SetActive(false);

            if (NetworkServer.active) NetworkManager.singleton.StopHost(); // Host case
            else NetworkManager.singleton.StopClient(); // Client-only case
        }

        // Restore main menu
        ActivateAllPrimaryMainMenuButtons();
        EventSystem.current.SetSelectedGameObject(hostButton.gameObject);
    }

    public void ExitSettingMenu()
    {
        // Save player prefs
        SavePlayerPrefs();

        settingsMenuUI.SetActive(false);
        ActivateAllPrimaryMainMenuButtons();
        EventSystem.current.SetSelectedGameObject(settingsButton.gameObject);
    }

    public void ExitProfileMenu()
    {
        // Save player prefs
        SavePlayerPrefs();

        profileMenuUI.SetActive(false);
        ActivateAllPrimaryMainMenuButtons();
        EventSystem.current.SetSelectedGameObject(profileButton.gameObject);
    }

    public void ExitControlsMenu()
    {
        // Save player prefs
        SavePlayerPrefs();

        controlsMenuUI.SetActive(false);
        ActivateAllPrimaryMainMenuButtons();
        EventSystem.current.SetSelectedGameObject(controlsButton.gameObject);
    }

    public void ReturnToControlsMenu()
    {
        // Save player prefs
        SavePlayerPrefs();

        keyboardRebindingsMenuUI.SetActive(false);
        gamepadRebindingsMenuUI.SetActive(false);

        keyboardMouseButton.interactable = true;
        gamepadButton.interactable = true;
        controlsBackButton.interactable = true;

        keyboardMouseButton.gameObject.SetActive(true);
        gamepadButton.gameObject.SetActive(true);
        controlsBackButton.gameObject.SetActive(true);
    }

    private void ActivateAllPrimaryMainMenuButtons()
    {
        if (SceneManager.GetActiveScene().buildIndex == 2) return;

        singlePlayerButton.gameObject.SetActive(true);
        multiplayerButton.gameObject.SetActive(true);
        settingsButton.gameObject.SetActive(true);
        controlsButton.gameObject.SetActive(true);
        profileButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        singlePlayerButton.interactable = true;
        multiplayerButton.interactable = true;
        settingsButton.interactable = true;
        controlsButton.interactable = true;
        profileButton.interactable = true;
        quitButton.interactable = true;
    }

    private void DeactivateAllPrimaryMainMenuButtons()
    {
        singlePlayerButton.gameObject.SetActive(false);
        multiplayerButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        controlsButton.gameObject.SetActive(false);
        profileButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

        singlePlayerButton.interactable = false;
        multiplayerButton.interactable = false;
        settingsButton.interactable = false;
        controlsButton.interactable = false;
        profileButton.interactable = false;
        quitButton.interactable = false;
    }

    /// <summary>
    /// Save player prefs
    /// </summary>
    private void SavePlayerPrefs()
    {
        // Save settings to PlayerPrefs
        // SETTINGS
        // Video
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("ScreenModeIndex", screenModeDropdown.value);
        PlayerPrefs.SetInt("RefreshRateIndex", refreshRateDropdown.value);
        PlayerPrefs.SetInt("PostProcessing", postProcessingToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Vsync", vsyncToggle.isOn ? 1 : 0);
        // Audio
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", soundVolumeSlider.value);
        // Game
        PlayerPrefs.SetInt("Language", languageDropdown.value);
        PlayerPrefs.SetInt("DynamicCamera", dynamicCameraToggle.isOn ? 1 : 0);
        // CONTROLS
        var rebinds = actions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString("Rebinds", rebinds);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load player prefs
    /// </summary>
    private void LoadPlayerPrefs()
    {
        // VIDEO
        // Load post-processing toggle
        if (PlayerPrefs.HasKey("PostProcessing"))
        {
            bool pp = PlayerPrefs.GetInt("PostProcessing") == 1;
            postProcessingToggle.SetIsOnWithoutNotify(pp);
            PostProcessingEnabler.Instance.isOn = pp;
            UpdatePostProcessingCheckmarkVisibility(pp);
        }

        // Load VSync toggle
        if (PlayerPrefs.HasKey("Vsync"))
        {
            bool vsync = PlayerPrefs.GetInt("Vsync") == 1;
            vsyncToggle.SetIsOnWithoutNotify(vsync);
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            UpdateVysncCheckmarkVisibility(vsync);
        }

        // Load resolution & refresh rate
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            int resIndex = PlayerPrefs.GetInt("ResolutionIndex");
            if (resIndex >= 0 && resIndex < resolutionOptions.Count)
            {
                OnResolutionDropdownChanged(resIndex, true); // populate Hz dropdown

                int hzIndex = PlayerPrefs.GetInt("RefreshRateIndex", resolutionToHzMap[resolutionOptions[resIndex]].Count - 1);
                hzIndex = Mathf.Clamp(hzIndex, 0, resolutionToHzMap[resolutionOptions[resIndex]].Count - 1);
                refreshRateDropdown.value = hzIndex;

                ApplyResolution(true);
            }
        }

        // Load screen mode index
        if (PlayerPrefs.HasKey("ScreenModeIndex"))
        {
            int screenModeIndex = PlayerPrefs.GetInt("ScreenModeIndex");
            screenModeDropdown.value = screenModeIndex;
            SetScreenMode(screenModeIndex); // Applies the setting
        }

        refreshRateDropdown.RefreshShownValue();
        resolutionDropdown.RefreshShownValue();
        screenModeDropdown.RefreshShownValue();

        // AUDIO
        // Load music volume
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume");
            musicVolumeSlider.value = musicVol;
            MusicManager.Instance.SetVolume((int)musicVol);
        }

        // Load sound volume
        if (PlayerPrefs.HasKey("SoundVolume"))
        {
            float soundVol = PlayerPrefs.GetFloat("SoundVolume");
            soundVolumeSlider.value = soundVol;
            SoundEffectManager.Instance.SetVolume((int)soundVol);
        }

        // GAME
        if (PlayerPrefs.HasKey("Language"))
        {
            int savedLanguage = PlayerPrefs.GetInt("Language");
            languageDropdown.value = savedLanguage;
            OnLanguageChanged(savedLanguage);
        }
        else
        {
            languageDropdown.value = (int)Language.English;
            OnLanguageChanged((int)Language.English);
        }

        // Load Language
        languageDropdown.RefreshShownValue();

        // Load Dynamic camera toggle
        if (PlayerPrefs.HasKey("DynamicCamera"))
        {
            bool dynamicCamera = PlayerPrefs.GetInt("DynamicCamera") == 1;
            dynamicCameraToggle.SetIsOnWithoutNotify(dynamicCamera);
            InterScenesSingleton.dynamicCameraFollowEnabled = dynamicCamera;
            UpdateDynamicCameraFollowCheckmarkVisibility(dynamicCamera);
        }

        // CONTROLS
        if (PlayerPrefs.HasKey("Rebinds"))
        {
            var rebinds = PlayerPrefs.GetString("Rebinds");
            if (!string.IsNullOrEmpty(rebinds)) actions.LoadBindingOverridesFromJson(rebinds);
        }
    }

    private async void RefreshLocalizedTexts()
    {
        await LocalizationSettings.InitializationOperation.Task;

        singlePlayerButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.GetText("MainMenu", "MENU_SINGLE_PLAYER");
        multiplayerButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.GetText("MainMenu", "MENU_MULTIPLAYER");
        controlsButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.GetText("MainMenu", "MENU_CONTROLS");  
        settingsButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.GetText("MainMenu", "MENU_SETTINGS");
        profileButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.GetText("MainMenu", "MENU_PROFILE");
        quitButton.GetComponentInChildren<TMP_Text>().text = LocalizationManager.GetText("MainMenu", "MENU_QUIT");
    }

    public IEnumerator HandleReturnFromCharacterScene()
    {
        // Set Play Button as selected
        yield return null; // Wait 1 more frame just to be sure

        EventSystem.current.SetSelectedGameObject(singlePlayerButton.gameObject);
    }

    /// <summary>
    /// Called from the Exit Game Button
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
