using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class SettingsUI : MonoBehaviour
{
    [SerializeField] TMP_Text settingsHeaderText;
    [SerializeField] TMP_Text backText;

    [Space(10)]
    [Header("Video")]
    [SerializeField] TMP_Text videoText;
    [SerializeField] TMP_Text resolutionText;
    [SerializeField] TMP_Text screenModeText;
    [SerializeField] TMP_Text refreshRateText;
    [SerializeField] TMP_Text postProcessingText;
    [SerializeField] TMP_Text vsyncText;

    [Space(10)]
    [Header("Audio")]
    [SerializeField] TMP_Text audioText;
    [SerializeField] TMP_Text musicVolumeText;
    [SerializeField] TMP_Text soundVolumeText;

    [Space(10)]
    [Header("Game")]
    [SerializeField] TMP_Text gameText;
    [SerializeField] TMP_Text languageText;
    [SerializeField] TMP_Text dynamicCameraText;

    private void OnEnable()
    {
        RefreshLocalizedTexts();
    }

    private void OnDisable()
    {
        
    }

    private void RefreshLocalizedTexts()
    {
        settingsHeaderText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_SETTINGS");
        backText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_BACK");

        videoText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_VIDEO");
        resolutionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_RESOLUTION");
        screenModeText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_SCREEN_MODE");
        vsyncText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_VSYNC");
        refreshRateText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_REFRESH_RATE");
        postProcessingText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_POST_PROCESSING");

        audioText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_AUDIO");
        musicVolumeText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_MUSIC_VOLUME");
        soundVolumeText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_SOUND_VOLUME");

        gameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_GAME");
        languageText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_LANGUAGE");
        dynamicCameraText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_DYNAMIC_CAMERA_FOLLOW");
    }
}
