using System.Collections;
using TMPro;
using UnityEngine;

public class PauseMenuUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the music volume level")]
    #endregion
    [SerializeField] TextMeshProUGUI musiclevelText;
    #region Tooltip
    [Tooltip("Populate with the sound volume level")]
    #endregion
    [SerializeField] TextMeshProUGUI soundlevelText;

    private void Start()
    {
        // Initially hide the pause menu
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Initialize the UI text
    /// </summary>
    IEnumerator InitializeUI()
    {
        // Wait a frame to ensure the previous music and sound levels have been set
        yield return null;

        // Initialize UI text
        soundlevelText.SetText(SoundEffectManager.Instance.soundVolume.ToString());
        musiclevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    private void OnEnable()
    {
        Time.timeScale = 0f;

        if (GameManager.Instance.glossaryBookOpen)
        {
            GameManager.Instance.CloseBookInCasePauseClick();
        }

        // Initialize UI text
        StartCoroutine(InitializeUI());
    }

    private void OnDisable()
    {
        Time.timeScale = 1f;
    }

    public void IncreaseMusicVolume()
    {
        MusicManager.Instance.IncreaseMusicVolume();
        musiclevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    public void DecreaseMusicVolume()
    {
        MusicManager.Instance.DecreaseMusicVolume();
        musiclevelText.SetText(MusicManager.Instance.musicVolume.ToString());
    }

    public void IncreaseSoundVolume()
    {
        SoundEffectManager.Instance.IncreaseSoundVolume();
        soundlevelText.SetText(SoundEffectManager.Instance.soundVolume.ToString());
    }

    public void DecreaseSoundVolume()
    {
        SoundEffectManager.Instance.DecreaseSoundVolume();
        soundlevelText.SetText(SoundEffectManager.Instance.soundVolume.ToString());
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(musiclevelText), musiclevelText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(soundlevelText), soundlevelText);
    }
#endif
    #endregion
}
