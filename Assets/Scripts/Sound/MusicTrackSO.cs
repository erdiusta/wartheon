using UnityEngine;

[CreateAssetMenu(fileName = "MusicTrack_", menuName = "Scriptable Objects/Sounds/Music Track")]
public class MusicTrackSO : ScriptableObject
{
    #region Header MUSIC TRACK DETAILS
    [Space(10)]
    [Header("MUSIC TRACK DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("The name for the music track")]
    #endregion
    public string musicName;
    #region Tooltip
    [Tooltip("The audio clip for the music track")]
    #endregion
    public AudioClip musicClip;
    #region Tooltip
    [Tooltip("The volume for the music track")]
    #endregion
    [Range(0f, 1f)] public float musicVolume = 1f;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(musicName), musicName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(musicClip), musicClip);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(musicVolume), musicVolume, true);
    }
#endif
    #endregion
}