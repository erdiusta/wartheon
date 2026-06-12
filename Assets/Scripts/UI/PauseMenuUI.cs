using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class PauseMenuUI : SingletonMonobehaviour<MonoBehaviour>
{
    [Header("Texts")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text resumeGameText;
    [SerializeField] TMP_Text controlsText;
    [SerializeField] TMP_Text settingsText;
    [SerializeField] TMP_Text quitGameText;
    [SerializeField] TMP_Text exitText;

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        RefreshLocalizedTexts();

        Time.timeScale = 0f;

        if (GameManager.Instance.glossaryBookOpen)
        {
            GameManager.Instance.CloseBookInCasePauseClick();
        }
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;

        Time.timeScale = 1f;
    }

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalizedTexts();
    }

    private void RefreshLocalizedTexts()
    {
        headerText.text = LocalizationSettings.StringDatabase.GetLocalizedString("PauseMenu", "PAUSE_MENU_HEADER");
        resumeGameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("PauseMenu", "PAUSE_MENU_RESUME_GAME");
        controlsText.text = LocalizationSettings.StringDatabase.GetLocalizedString("PauseMenu", "PAUSE_CONTROLS");
        settingsText.text = LocalizationSettings.StringDatabase.GetLocalizedString("PauseMenu", "PAUSE_MENU_SETTINGS");
        quitGameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("PauseMenu", "PAUSE_MENU_QUIT_GAME");
        exitText.text = LocalizationSettings.StringDatabase.GetLocalizedString("PauseMenu", "PAUSE_MENU_EXIT");
    }
}
