using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System.Collections;

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
    [SerializeField] Toggle fullscreenToggle;
    [SerializeField] Image fullScreenCheckmarkImage;
    [SerializeField] Toggle vsyncToggle;
    [SerializeField] Image vysncCheckmarkImage;

    private void OnEnable()
    {
        StaticEventHandler.OnCheatActivated += StaticEventHandler_OnCheatActivated;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnCheatActivated -= StaticEventHandler_OnCheatActivated;
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
        // Play music
        MusicManager.Instance.PlayMusic(GameResources.Instance.mainMenuMusic, 0f, 2f);

        // Sync toggle with current fullscreen state
        fullscreenToggle.isOn = Screen.fullScreen;
        UpdateFullScreenCheckmarkVisibility(fullscreenToggle.isOn);

        // Sync toggle with current V-Sync state (1 = on, 0 = off)
        bool vsyncEnabled = QualitySettings.vSyncCount > 0;
        vsyncToggle.isOn = vsyncEnabled;
        UpdateVysncCheckmarkVisibility(vsyncEnabled);

        // Music and sound sliders
        musicVolumeSlider.value = MusicManager.Instance.GetMusicVolume();
        soundVolumeSlider.value = SoundEffectManager.Instance.GetSoundVolume();

        // Add Listeners
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeSliderChanged);
        fullscreenToggle.onValueChanged.AddListener(OnFullScreenToggleChanged);
        vsyncToggle.onValueChanged.AddListener(OnVsyncToggleChanged);
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

        // Load character selector scene additively
        SceneManager.LoadScene("CharacterSelectorScene", LoadSceneMode.Additive);
    }

    /// <summary>
    /// Called from the Settings Button
    /// </summary>
    public void OpenSettings()
    {
        //CanvasGroup canvasGroup = playButton.GetComponentInParent<CanvasGroup>();
        //canvasGroup.interactable = false;
        //canvasGroup.blocksRaycasts = false;

        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        settingsMenuUI.SetActive(true);

        playButton.GetComponent<Button>().interactable = false;
        settingsButton.GetComponent<Button>().interactable = false;
        quitButton.GetComponent<Button>().interactable = false;

        playButton.SetActive(false);
        settingsButton.SetActive(false);
        quitButton.SetActive(false);
    }


    private void OnFullScreenToggleChanged(bool isOn)
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        Screen.fullScreen = isOn;
        UpdateFullScreenCheckmarkVisibility(isOn);
    }

    private void UpdateFullScreenCheckmarkVisibility(bool show)
    {
        fullScreenCheckmarkImage.enabled = show;
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

        settingsMenuUI.SetActive(false);

        playButton.SetActive(true);
        settingsButton.SetActive(true);
        quitButton.SetActive(true);

        playButton.GetComponent<Button>().interactable = true;
        settingsButton.GetComponent<Button>().interactable = true;
        quitButton.GetComponent<Button>().interactable = true;
    }

    /// <summary>
    /// Called from the Exit Game Button
    /// </summary>
    public void ExitGame()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        Application.Quit();
    }
}
