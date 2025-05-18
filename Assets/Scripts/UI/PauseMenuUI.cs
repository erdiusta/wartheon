using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    [SerializeField] GameObject settingsMenuUI;

    private void OnEnable()
    {
        Time.timeScale = 0f;

        if (GameManager.Instance.glossaryBookOpen)
        {
            GameManager.Instance.CloseBookInCasePauseClick();
        }
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }
}
