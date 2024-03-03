using UnityEngine;

[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the Transform from the child RightHandWeaponRotationPoint gameobject")]
    #endregion
    [SerializeField] Transform rightHandWeaponRotationPointTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform from the child LeftHandWeaponRotationPoint gameobject")]
    #endregion
    [SerializeField] Transform leftHandWeaponRotationPointTransform;

    Player player;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    public void Aim(AimDirection aimDirection, float aimAngle)
    {
        if (gameObject.tag == Settings.playerTag && (player.meleeAttackRightHand.IsAttackingAtRightHand || 
            player.meleeAttackLeftHand.IsAttackingAtLeftHand)) return;

        // Set angle of the weapon transform
        rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);
        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

        // If left hand has a shield, fix the shield position
        if (tag == Settings.playerTag)
        {
            Animator shieldAnimator = player.transform.GetChild(1).GetComponent<Animator>();
            SpriteRenderer shieldSpriteRenderer = player.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

            if (player.activeWeapon.GetCurrentLeftHandWeapon() == null)
            {
                shieldAnimator.runtimeAnimatorController = null;
                shieldSpriteRenderer.sprite = null;
            }

            if (player.activeWeapon.GetCurrentLeftHandWeapon() != null &&
                player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
            {

                switch (aimDirection)
                {
                    case AimDirection.Left:

                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponFrontSprite;
                        // Re-enable child weapon animator
                        shieldAnimator.runtimeAnimatorController = player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponAnimatorController;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.UpLeft:

                        // Disable child weapon animator
                        shieldAnimator.runtimeAnimatorController = null;
                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponRearSprite;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.Up:
                    case AimDirection.UpRight:

                        // Disable child weapon animator
                        shieldAnimator.runtimeAnimatorController = null;
                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponRearSprite;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;

                    case AimDirection.Right:
                    case AimDirection.Down:

                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponFrontSprite;
                        // Re-enable child weapon animator
                        shieldAnimator.runtimeAnimatorController = player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponAnimatorController;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                }
            }

            //if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponName == "Bow" && aimDirection == AimDirection.Down)
            //{
            //    rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.5f, -0.2f, 0f);
            //}
            //else
            //{
            //    rightHandWeaponRotationPointTransform.localPosition = new Vector3(0f, 0f, 0f);
            //}
        }

        // Flip weapon transform based on player direction
        switch (aimDirection)
        {
            case AimDirection.Left:
            case AimDirection.UpLeft:

                rightHandWeaponRotationPointTransform.localScale = new Vector3(1f, -1f, 0f);
                leftHandWeaponRotationPointTransform.localScale = new Vector3(1f, -1f, 0f);
                break;

            case AimDirection.Up:
            case AimDirection.UpRight:
            case AimDirection.Right:
            case AimDirection.Down:

                rightHandWeaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                leftHandWeaponRotationPointTransform.localScale = new Vector3(1f, 1f, 0f);
                break;
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(rightHandWeaponRotationPointTransform), rightHandWeaponRotationPointTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(leftHandWeaponRotationPointTransform), leftHandWeaponRotationPointTransform);
    }
#endif
    #endregion
}
