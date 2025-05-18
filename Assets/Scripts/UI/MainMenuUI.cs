using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using System;

public class MainMenuUI : MonoBehaviour
{
    public static int currentDungeonLevelListIndex = 0;

    [SerializeField] GameObject playButton;
    [SerializeField] GameObject settingsButton;
    [SerializeField] GameObject quitButton;
    [SerializeField] GameObject cheatCodeObject;
    [SerializeField] SoundEffectSO buttonClickSound;

    [Space(10)]
    [Header("SETTINGS")]
    [Space(10)]
    [SerializeField] GameObject settingsMenuUI;
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider soundVolumeSlider;
    [SerializeField] Toggle postProcessingToggle;
    [SerializeField] Image  postProcessingCheckmarkImage;
    [SerializeField] Toggle vsyncToggle;
    [SerializeField] Image vysncCheckmarkImage;
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown screenModeDropDown;
    [SerializeField] TMP_Dropdown refreshRateDropdown;

    Resolution[] resolutions;
    Dictionary<string, List<int>> resolutionToHzMap;
    List<string> resolutionOptions;


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

        playButton.SetActive(true);
        settingsButton.SetActive(true);
        quitButton.SetActive(true);

        playButton.GetComponent<Button>().interactable = true;
        settingsButton.GetComponent<Button>().interactable = true;
        quitButton.GetComponent<Button>().interactable = true;
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

        //  Now that resolutions and dropdown are ready, load saved settings
        LoadPlayerPrefs();

        // Populate screen modes
        screenModeDropDown.ClearOptions();
        screenModeDropDown.AddOptions(new List<string> { "Exclusive Fullscreen", "Borderless Window", "Windowed" });
        if (PlayerPrefs.HasKey("ScreenModeIndex")) screenModeDropDown.value = PlayerPrefs.GetInt("ScreenModeIndex");
        else screenModeDropDown.value = (int)Screen.fullScreenMode;
        screenModeDropDown.RefreshShownValue();

        // Play music
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        // Add other listeners
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeSliderChanged);
        postProcessingToggle.onValueChanged.AddListener(OnPostProcessingToggleChanged);
        vsyncToggle.onValueChanged.AddListener(OnVsyncToggleChanged);
        refreshRateDropdown.onValueChanged.AddListener(index => OnRefreshRateChanged(index, false));
        screenModeDropDown.onValueChanged.AddListener(SetScreenMode);
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

        playButton.GetComponent<Button>().interactable = false;
        settingsButton.GetComponent<Button>().interactable = false;
        quitButton.GetComponent<Button>().interactable = false;

        playButton.SetActive(false);
        settingsButton.SetActive(false);
        quitButton.SetActive(false);

        // Save player prefs
        SavePlayerPrefs();

        // Load character selector scene additively
        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Settings Button
    /// </summary>
    public void OpenSettings()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        settingsMenuUI.SetActive(true);

        playButton.GetComponent<Button>().interactable = false;
        settingsButton.GetComponent<Button>().interactable = false;
        quitButton.GetComponent<Button>().interactable = false;

        playButton.SetActive(false);
        settingsButton.SetActive(false);
        quitButton.SetActive(false);
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

    private void OnMusicVolumeSliderChanged(float newValue)
    {
        MusicManager.Instance.SetVolume((int)newValue);
    }

    private void OnSoundVolumeSliderChanged(float newValue)
    {
        SoundEffectManager.Instance.SetVolume((int)newValue);
    }

    public void ExitSettingMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Save player prefs
        SavePlayerPrefs();

        settingsMenuUI.SetActive(false);

        playButton.SetActive(true);
        settingsButton.SetActive(true);
        quitButton.SetActive(true);

        playButton.GetComponent<Button>().interactable = true;
        settingsButton.GetComponent<Button>().interactable = true;
        quitButton.GetComponent<Button>().interactable = true;
    }

    /// <summary>
    /// Save player prefs
    /// </summary>
    private void SavePlayerPrefs()
    {
        // Save settings to PlayerPrefs
        PlayerPrefs.SetFloat("MusicVolume", musicVolumeSlider.value);
        PlayerPrefs.SetFloat("SoundVolume", soundVolumeSlider.value);
        PlayerPrefs.SetInt("PostProcessing", postProcessingToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("Vsync", vsyncToggle.isOn ? 1 : 0);
        PlayerPrefs.SetInt("ResolutionIndex", resolutionDropdown.value);
        PlayerPrefs.SetInt("ScreenModeIndex", screenModeDropDown.value);
        PlayerPrefs.SetInt("RefreshRateIndex", refreshRateDropdown.value);

        PlayerPrefs.Save();
    }

    /// <summary>
    /// Load player prefs
    /// </summary>
    private void LoadPlayerPrefs()
    {
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
            screenModeDropDown.value = screenModeIndex;
            SetScreenMode(screenModeIndex); // Applies the setting
        }

        refreshRateDropdown.RefreshShownValue();
        resolutionDropdown.RefreshShownValue();
        screenModeDropDown.RefreshShownValue();
    }


    /// <summary>
    /// Called from the Exit Game Button
    /// </summary>
    public void ExitGame()
    {
        Application.Quit();
    }
}
