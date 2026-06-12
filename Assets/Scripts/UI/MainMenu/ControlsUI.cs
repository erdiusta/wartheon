using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class ControlsUI : MonoBehaviour
{
    [Space(10)]
    [Header("TEXTS")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text keyboardMouseText;
    [SerializeField] TMP_Text gamepadText;
    [SerializeField] TMP_Text backText;

    private void OnEnable()
    {
        LocalizationManager.LanguageChanged += OnLanguageChanged;

        RefreshLocalizedTexts();
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= OnLanguageChanged;
    }

    private void OnLanguageChanged(Language language)
    {
        RefreshLocalizedTexts();
    }

    private void RefreshLocalizedTexts()
    {
        headerText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_HEADER");
        keyboardMouseText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_KEYBOARD_MOUSE");
        gamepadText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_GAMEPAD");
        backText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_BACK");
    }
}
