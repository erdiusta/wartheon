using UnityEngine;

[CreateAssetMenu(fileName = "PassiveItem_", menuName = "Scriptable Objects/Passive Items")]
public class PassiveItemDetailsSO : ScriptableObject
{
    #region Header PASSIVE BASE DETAILS
    [Space(10)]
    [Header("PASSIVE BASE DETAILS")]
    #endregion 
    #region Tooltip
    [Tooltip("Passive item name")]
    #endregion Tooltip
    public string passiveItemName;
    #region Tooltip
    [Tooltip("The sprite for the item - the sprite should have the 'generate physics shape' option selected ")]
    #endregion Tooltip
    public Sprite passiveItemSprite;
    #region Tooltip
    [Tooltip("The animatior controller for the item")]
    #endregion Tooltip
    public RuntimeAnimatorController passiveItemAnimatorController;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(passiveItemName), passiveItemName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(passiveItemSprite), passiveItemSprite);
        HelperUtilities.ValidateCheckNullValue(this, nameof(passiveItemAnimatorController), passiveItemAnimatorController);
    }
#endif
    #endregion Validation
}
