using UnityEngine;

[CreateAssetMenu(fileName = "BuildDetails_", menuName = "Scriptable Objects/Player/Build Details")]
public class BuildDetailsSO : ScriptableObject
{
    public Character belongingCharacter;
    public Sprite characterBuildImage;
    public string characterBuildName;
    public string characterBuildDetails;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(characterBuildName), characterBuildName);
        HelperUtilities.ValidateCheckEmptyString(this, nameof(characterBuildDetails), characterBuildDetails);
    }
#endif
    #endregion
}
