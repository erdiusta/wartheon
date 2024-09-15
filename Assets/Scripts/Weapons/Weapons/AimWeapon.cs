using UnityEngine;

[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the Transform from the child RightHandWeaponAnchorPoint gameobject")]
    #endregion
    public Transform mainHandWeaponAnchorPointTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform from the child LeftHandWeaponAnchorPoint gameobject")]
    #endregion
    public Transform offHandWeaponAnchorPointTransform;

    Transform rightHandWeaponRotationPointTransform;
    Transform leftHandWeaponRotationPointTransform;
    Player player;
    Enemy enemy;

    private void Start()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();

        rightHandWeaponRotationPointTransform = mainHandWeaponAnchorPointTransform.GetChild(0);
        leftHandWeaponRotationPointTransform = offHandWeaponAnchorPointTransform.GetChild(0);
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    public void Aim(AimDirection aimDirection, float aimAngle)
    {
        if (tag == Settings.enemyTag)
        {
            rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

            if (enemy.enemyDetails.enemyWeapon != null && enemy.enemyDetails.enemyWeapon.weaponClass == WeaponClass.Staff)
            {
                switch (aimDirection)
                {
                    case AimDirection.Left:
                    case AimDirection.UpLeft:

                        rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.Up:
                    case AimDirection.UpRight:
                    case AimDirection.Right:
                    case AimDirection.Down:

                        rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                }
            }

        }
        else if (tag == Settings.playerTag)
        {
            if (player.activeWeapon.isSwitching) return;

            // No weapon
            if (player.activeWeapon.GetCurrentMainHandWeapon() == null)
            {

            }
            else
            {
                // Bow aim
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow && player.activeWeapon.
                    GetCurrentMainHandWeapon().weaponDetails.weaponName != "Crossbow")
                {
                    // Set angle of the weapon transform
                    if (aimAngle > 0f && aimAngle < 22f) // RIGHT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(0.1f, 0f, 0f);
                    }
                    else if (aimAngle >= 22f && aimAngle <= 45f) // UPRIGHT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle >= 45f && aimAngle <= 67f) // UPRIGHT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle > 67f && aimAngle <= 90f) // UP
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(0.05f, 0.3f, 0f);
                    }
                    else if (aimAngle > 90f && aimAngle <= 112f) // UP
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(0.05f, 0.3f, 0f);
                    }
                    else if (aimAngle > 112f && aimAngle <= 135f) // UPLEFT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle > 135f && aimAngle <= 158f) // UPLEFT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle <= 180f && aimAngle > 158f) // LEFT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.1f, 0.05f, 0f);
                    }
                    else if (aimAngle > -180f && aimAngle <= -158f) // LEFT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.1f, -0.25f, 0f);
                    }
                    else if (aimAngle > -158f && aimAngle <= -135f) // LEFT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(0f, -0.5f, 0f);
                    }
                    else if (aimAngle > -135f && aimAngle <= -112f) // DOWN
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.8f, -0.6f, 0f);
                    }
                    else if (aimAngle > -112f && aimAngle <= -90f) // DOWN
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.45f, -0.7f, 0f);
                    }
                    else if (aimAngle > -90f && aimAngle <= -67f) // DOWN
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.35f, -0.7f, 0f);
                    }
                    else if (aimAngle > -67f && aimAngle <= -45f) // DOWN
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.2f, -0.6f, 0f);
                    }
                    else if (aimAngle > -45f && aimAngle <= -22f) // RIGHT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(-0.15f, -0.5f, 0f);
                    }
                    else if (aimAngle > -22f && aimAngle <= 0f) // RIGHT
                    {
                        rightHandWeaponRotationPointTransform.localPosition = new Vector3(0.1f, -0.3f, 0f);
                    }
                }
            }

            // No weapon
            if (player.activeWeapon.GetCurrentMainHandWeapon() == null)
            {

            }
            else
            {
                // Staff case
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                {
                    switch (aimDirection)
                    {
                        case AimDirection.Left:
                        case AimDirection.UpLeft:

                            rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                            break;

                        case AimDirection.Up:
                        case AimDirection.UpRight:
                        case AimDirection.Right:
                        case AimDirection.Down:

                            rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                            break;
                    }
                }
                else
                {
                    rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

                    if (offHandWeaponAnchorPointTransform.gameObject.activeSelf)
                    {
                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);
                    }
                }
            }


            // In case of shield
            Animator shieldAnimator = player.transform.GetChild(1).GetComponent<Animator>();
            SpriteRenderer shieldSpriteRenderer = player.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

            if (player.activeWeapon.GetCurrentOffHandWeapon() == null)
            {
                shieldAnimator.runtimeAnimatorController = null;
                shieldSpriteRenderer.sprite = null;
            }

            if (player.activeWeapon.GetCurrentOffHandWeapon() != null &&
                player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
            {
                switch (aimDirection)
                {
                    case AimDirection.Left:

                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponFrontSprite;
                        // Re-enable child weapon animator
                        shieldAnimator.runtimeAnimatorController = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponAnimatorController;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.UpLeft:

                        // Disable child weapon animator
                        shieldAnimator.runtimeAnimatorController = null;
                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponRearSprite;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.Up:
                    case AimDirection.UpRight:

                        // Disable child weapon animator
                        shieldAnimator.runtimeAnimatorController = null;
                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponRearSprite;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;

                    case AimDirection.Right:
                    case AimDirection.Down:

                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponFrontSprite;
                        // Re-enable child weapon animator
                        shieldAnimator.runtimeAnimatorController = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponAnimatorController;

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                }
            }      
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
        HelperUtilities.ValidateCheckNullValue(this, nameof(mainHandWeaponAnchorPointTransform), mainHandWeaponAnchorPointTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(offHandWeaponAnchorPointTransform), offHandWeaponAnchorPointTransform);
    }
#endif
    #endregion
}
