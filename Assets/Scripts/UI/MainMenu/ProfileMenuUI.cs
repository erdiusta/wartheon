using TMPro;
using UnityEngine;

public class ProfileMenuUI : MonoBehaviour
{
    [SerializeField] TMP_InputField nameInput;

    private void OnEnable()
    {
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
}
