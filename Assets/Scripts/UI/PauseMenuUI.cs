using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
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
