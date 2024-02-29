using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSelectionUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject RightWeaponAnchorPosition/WeaponRotationPoint/Hand")]
    #endregion
    public SpriteRenderer playerRightHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject RightWeaponAnchorPosition/WeaponRotationPoint/Weapon")]
    #endregion
    public SpriteRenderer playerWeaponRightHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject LeftWeaponAnchorPosition/WeaponRotationPoint/Hand")]
    #endregion
    public SpriteRenderer playerLeftHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject LeftWeaponAnchorPosition/WeaponRotationPoint/Weapon")]
    #endregion
    public SpriteRenderer playerWeaponLeftHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Animator component")]
    #endregion
    public Animator animator;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerRightHandSpriteRenderer), playerRightHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerLeftHandSpriteRenderer), playerLeftHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerWeaponRightHandSpriteRenderer), playerWeaponRightHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerWeaponLeftHandSpriteRenderer), playerWeaponLeftHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(animator), animator);
    }
#endif
    #endregion
}
