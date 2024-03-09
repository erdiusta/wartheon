using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSelectionUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the RightWeaponAnchorTransform")]
    #endregion
    public Transform rightWeaponAnchorTransform;
    #region Tooltip
    [Tooltip("Populate with the LeftWeaponAnchorTransform")]
    #endregion
    public Transform leftWeaponAnchorTransform;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject RightWeaponAnchorPosition/WeaponRotationPoint/Hand")]
    #endregion
    public SpriteRenderer playerRightHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject RightWeaponAnchorPosition/WeaponRotationPoint/Weapon")]
    #endregion
    public SpriteRenderer playerWeaponRightHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the right hand weapon animator on child gameObject")]
    #endregion
    public Animator playerRightHandWeaponAnimator;
    #region Tooltip
    [Tooltip("Populate with the left hand weapon animator on child gameObject")]
    #endregion
    public Animator playerLeftHandWeaponAnimator;
    #region Tooltip
    [Tooltip("Populate with the thirdHandGameObject")]
    #endregion
    public GameObject thirdHandGameObject;
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
