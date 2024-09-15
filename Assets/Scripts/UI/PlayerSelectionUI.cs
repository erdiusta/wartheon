using UnityEngine;

[DisallowMultipleComponent]
public class PlayerSelectionUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the RightWeaponAnchorTransform")]
    #endregion
    public Transform mainHandWeaponAnchorTransform;
    #region Tooltip
    [Tooltip("Populate with the LeftWeaponAnchorTransform")]
    #endregion
    public Transform offHandWeaponAnchorTransform;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject RightWeaponAnchorPosition/WeaponRotationPoint/Hand")]
    #endregion
    public SpriteRenderer playerMainHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject RightWeaponAnchorPosition/WeaponRotationPoint/Weapon")]
    #endregion
    public SpriteRenderer playerWeaponMainHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the right hand weapon animator on child gameObject")]
    #endregion
    public Animator playerMainHandWeaponAnimator;
    #region Tooltip
    [Tooltip("Populate with the left hand weapon animator on child gameObject")]
    #endregion
    public Animator playerOffHandWeaponAnimator;
    #region Tooltip
    [Tooltip("Populate with the thirdHandGameObject")]
    #endregion
    public GameObject thirdHandGameObject;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject LeftWeaponAnchorPosition/WeaponRotationPoint/Hand")]
    #endregion
    public SpriteRenderer playerOffHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Sprite Renderer on child gameObject LeftWeaponAnchorPosition/WeaponRotationPoint/Weapon")]
    #endregion
    public SpriteRenderer playerWeaponOffHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Animator component")]
    #endregion
    public Animator animator;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerMainHandSpriteRenderer), playerMainHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerOffHandSpriteRenderer), playerOffHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerWeaponMainHandSpriteRenderer), playerWeaponMainHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(playerWeaponOffHandSpriteRenderer), playerWeaponOffHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(animator), animator);
    }
#endif
    #endregion
}
