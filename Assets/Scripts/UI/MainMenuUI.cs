using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class MainMenuUI : SingletonMonobehaviour<MainMenuUI>
{
    public static int currentDungeonLevelListIndex = 0;

    public Button playButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button quitButton;
    [SerializeField] GameObject cheatCodeObject;
    [SerializeField] SoundEffectSO buttonClickSound;

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

    Resolution[] resolutions;
    Dictionary<string, List<int>> resolutionToHzMap;
    List<string> resolutionOptions;

    InputActionAsset actions;

    protected override void Awake()
    {
        base.Awake();
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
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        CanvasGroup canvasGroup = playButton.GetComponentInParent<CanvasGroup>();
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;

        playButton.gameObject.SetActive(true);
        settingsButton.gameObject.SetActive(true);
        controlsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        playButton.interactable = true;
        settingsButton.interactable = true;
        controlsButton.interactable = true;
        quitButton.interactable = true;
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
        dynamicCameraToggle.onValueChanged.AddListener(OnDynamicCameraFollowToggleChanged);
    }

    
    private void Update()
    {
        if (InputManager.Instance.escapeButton.action.WasPressedThisFrame())
        {
            if (settingsMenuUI.activeSelf)
            {
                ExitSettingMenu();
                EventSystem.current.SetSelectedGameObject(playButton.gameObject);
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
                    EventSystem.current.SetSelectedGameObject(playButton.gameObject);
                }
            }
        }
    }

    /// <summary>
    /// Called from the Play Game Button
    /// </summary>
    public void PlayGame()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        CanvasGroup canvasGroup = playButton.GetComponentInParent<CanvasGroup>();
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        playButton.interactable = false;
        settingsButton.interactable = false;
        controlsButton.interactable = false;
        quitButton.interactable = false;

        playButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        controlsButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);

        // Save player prefs
        SavePlayerPrefs();

        // Load character selector scene additively
        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Controls Button
    /// </summary>
    public void OpenControls()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        controlsMenuUI.SetActive(true);

        playButton.interactable = false;
        settingsButton.interactable = false;
        controlsButton.interactable = false;
        quitButton.interactable = false;

        playButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        controlsButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called from the Settings Button
    /// </summary>
    public void OpenSettings()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        settingsMenuUI.SetActive(true);

        playButton.interactable = false;
        settingsButton.interactable = false;
        controlsButton.interactable = false;
        quitButton.interactable = false;

        playButton.gameObject.SetActive(false);
        settingsButton.gameObject.SetActive(false);
        controlsButton.gameObject.SetActive(false);
        quitButton.gameObject.SetActive(false);
    }

    /// <summary>
    /// Called from the Keyboard&Mouse Button
    /// </summary>
    public void OpenKeyboardMouseRebindingsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

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
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

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
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

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

        if (!onStart) SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);
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
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        QualitySettings.vSyncCount = isOn ? 1 : 0;
        UpdateVysncCheckmarkVisibility(isOn);
    }

    private void UpdateVysncCheckmarkVisibility(bool show)
    {
        vysncCheckmarkImage.enabled = show;
    }

    private void OnDynamicCameraFollowToggleChanged(bool isOn)
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

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

    public void OpenKeyboardControls()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Save player prefs
        SavePlayerPrefs();
    }

    public void ExitControlsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Save player prefs
        SavePlayerPrefs();

        controlsMenuUI.SetActive(false);

        playButton.gameObject.SetActive(true);
        settingsButton.gameObject.SetActive(true);
        controlsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        playButton.interactable = true;
        settingsButton.interactable = true;
        controlsButton.interactable = true;
        quitButton.interactable = true;
    }


    public void ReturnToControlsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

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

    public void ExitSettingMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Save player prefs
        SavePlayerPrefs();

        settingsMenuUI.SetActive(false);

        playButton.gameObject.SetActive(true);
        settingsButton.gameObject.SetActive(true);
        controlsButton.gameObject.SetActive(true);
        quitButton.gameObject.SetActive(true);

        playButton.interactable = true;
        settingsButton.interactable = true;
        controlsButton.interactable = true;
        quitButton.interactable = true;
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
            postProcessingToggle.isOn = pp;
            PostProcessingEnabler.Instance.isOn = pp;
            UpdatePostProcessingCheckmarkVisibility(pp);
        }

        // Load VSync toggle
        if (PlayerPrefs.HasKey("Vsync"))
        {
            bool vsync = PlayerPrefs.GetInt("Vsync") == 1;
            vsyncToggle.isOn = vsync;
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
        // Load Dynamic amera toggle
        if (PlayerPrefs.HasKey("DynamicCamera"))
        {
            bool dynamicCamera = PlayerPrefs.GetInt("DynamicCamera") == 1;
            dynamicCameraToggle.isOn = dynamicCamera;
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

    public IEnumerator HandleReturnFromCharacterScene()
    {
        // Wait until the scene is actually unloaded
        while (SceneManager.GetSceneByName("CharacterSelectorScene").isLoaded)
        {
            yield return null;
        }

        // Set Play Button as selected
        yield return null; // Wait 1 more frame just to be sure

        EventSystem.current.SetSelectedGameObject(playButton.gameObject);
    }

    /// <summary>
    /// Called from the Exit Game Button
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
