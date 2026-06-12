using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Localization.Settings;

public class ResetDeviceBindings : MonoBehaviour
{
    [SerializeField] InputActionAsset _inputActions;
    [SerializeField] string _targetControlScheme;

    [Space(10)]
    [Header("TEXTS")]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text backText;
    [SerializeField] TMP_Text resetAllText;

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
        if (_targetControlScheme == "Keyboard&Mouse")
        {
            headerText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_KEYBOARD_MOUSE");
        }
        else if (_targetControlScheme == "Gamepad")
        {
            headerText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_GAMEPAD");
        }

        backText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_BACK");
        resetAllText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Controls", "CONTROLS_RESET_ALL");
    }

    public void ResetAllBindings()
    {
        foreach (InputActionMap map in _inputActions.actionMaps)
        {
            map.RemoveAllBindingOverrides();
        }
    }

    public void ResetControlSchemeBinding()
    {
        foreach (InputActionMap map in _inputActions.actionMaps)
        {
            foreach (InputAction action in map.actions)
            {
                action.RemoveBindingOverride(InputBinding.MaskByGroup(_targetControlScheme));
            }
        }
    }
}
