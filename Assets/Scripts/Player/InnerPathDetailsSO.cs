using UnityEngine;

[CreateAssetMenu(fileName = "InnerPathDetails_", menuName = "Scriptable Objects/Player/Inner Path Details")]
public class InnerPathDetailsSO : ScriptableObject
{
    public string innerPathName;
    public InnerPathName innerPathSelectionName;
    public string innerPathDetails;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(innerPathName), innerPathName);
        HelperUtilities.ValidateCheckEmptyString(this, nameof(innerPathDetails), innerPathDetails);
    }
#endif
    #endregion
}
