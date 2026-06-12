using TMPro;
using UnityEngine;
using UnityEngine.Localization.Settings;

public class ProfileMenuUI : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInput;

    [Space(10)]
    [Header("Texts")]
    [SerializeField] TMP_Text enterYourNameText;
    [SerializeField] TMP_Text okText;

    private void OnEnable()
    {
        RefreshLocalizedTexts();

        if (nameInput == null) return;

        if (!PlayerProfile.IsValid)
        {
            string randomName = PlayerProfile.GenerateRandomName();
            nameInput.SetTextWithoutNotify(randomName);
        }
        else
        {
            nameInput.SetTextWithoutNotify(PlayerProfile.DisplayName);
        }

        nameInput.caretPosition = nameInput.text.Length;
        nameInput.Select();
    }

    public void OnOkPressed()
    {
        string name = nameInput.text;

        if (string.IsNullOrWhiteSpace(name)) return;

        PlayerProfile.Save(name);

        // IMPORTANT: delegate close to MainMenuUI
        MainMenuUI.Instance.ExitProfileMenu();
    }

    private void RefreshLocalizedTexts()
    {
        enterYourNameText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_ENTER_YOUR_NAME");
        okText.text = LocalizationSettings.StringDatabase.GetLocalizedString("Settings", "SETTINGS_OK");
    }
}
