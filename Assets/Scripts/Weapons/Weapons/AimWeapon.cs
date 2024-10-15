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

    Transform mainHandWeaponRotationPointTransform;
    Transform offHandWeaponRotationPointTransform;
    Player player;
    Enemy enemy;

    Transform mainHandShootPosition;
    Transform offHandShootPosition;

    private void Start()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();

        mainHandWeaponRotationPointTransform = mainHandWeaponAnchorPointTransform.GetChild(0);
        offHandWeaponRotationPointTransform = offHandWeaponAnchorPointTransform.GetChild(0);

        if (tag == Settings.enemyTag)
        {
            mainHandShootPosition = enemy.enemyAI.weaponShootPosition;
        }
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    public void Aim(AimDirection aimDirection, float aimAngle)
    {
        if (tag == Settings.enemyTag)
        {
            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

            if (enemy.enemyDetails.enemyWeapon != null && enemy.enemyDetails.enemyWeapon.weaponClass == WeaponClass.Staff)
            {
                switch (aimDirection)
                {
                    case AimDirection.Left:
                    case AimDirection.UpLeft:

                        mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.Up:
                    case AimDirection.UpRight:
                    case AimDirection.Right:
                    case AimDirection.Down:

                        mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                }
            }

            if (enemy.enemyDetails.enemyWeapon != null)
            {
                // Adjust weapon shoot position if weapon shoot position is not relative to the rotation point
                switch (aimDirection)
                {
                    case AimDirection.Up:
                        mainHandShootPosition.localPosition = enemy.enemyDetails.enemyWeapon.weaponUpShootPosition;
                        break;
                    case AimDirection.Down:
                        mainHandShootPosition.localPosition = enemy.enemyDetails.enemyWeapon.weaponDownShootPosition;
                        break;
                    case AimDirection.Right:
                    case AimDirection.UpRight:
                        mainHandShootPosition.localPosition = enemy.enemyDetails.enemyWeapon.weaponRightShootPosition;
                        break;
                    case AimDirection.Left:
                    case AimDirection.UpLeft:
                        mainHandShootPosition.localPosition = enemy.enemyDetails.enemyWeapon.weaponLeftShootPosition;
                        break;
                    default:
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
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow)
                {
                    // Set angle of the weapon transform
                    if (aimAngle > 0f && aimAngle < 22f) // RIGHT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.1f, 0f, 0f);
                    }
                    else if (aimAngle >= 22f && aimAngle <= 45f) // UPRIGHT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle >= 45f && aimAngle <= 67f) // UPRIGHT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle > 67f && aimAngle <= 90f) // UP
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.05f, 0.3f, 0f);
                    }
                    else if (aimAngle > 90f && aimAngle <= 112f) // UP
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.05f, 0.3f, 0f);
                    }
                    else if (aimAngle > 112f && aimAngle <= 135f) // UPLEFT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle > 135f && aimAngle <= 158f) // UPLEFT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.05f, 0.15f, 0f);
                    }
                    else if (aimAngle <= 180f && aimAngle > 158f) // LEFT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.1f, 0.05f, 0f);
                    }
                    else if (aimAngle > -180f && aimAngle <= -158f) // LEFT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.1f, -0.25f, 0f);
                    }
                    else if (aimAngle > -158f && aimAngle <= -135f) // LEFT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(0f, -0.5f, 0f);
                    }
                    else if (aimAngle > -135f && aimAngle <= -112f) // DOWN
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.8f, -0.6f, 0f);
                    }
                    else if (aimAngle > -112f && aimAngle <= -90f) // DOWN
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.45f, -0.7f, 0f);
                    }
                    else if (aimAngle > -90f && aimAngle <= -67f) // DOWN
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.35f, -0.7f, 0f);
                    }
                    else if (aimAngle > -67f && aimAngle <= -45f) // DOWN
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.2f, -0.6f, 0f);
                    }
                    else if (aimAngle > -45f && aimAngle <= -22f) // RIGHT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.15f, -0.5f, 0f);
                    }
                    else if (aimAngle > -22f && aimAngle <= 0f) // RIGHT
                    {
                        mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.1f, -0.3f, 0f);
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

                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                            break;

                        case AimDirection.Up:
                        case AimDirection.UpRight:
                        case AimDirection.Right:
                        case AimDirection.Down:

                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                            break;
                    }
                }
                else
                {
                    mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

                    if (offHandWeaponAnchorPointTransform.gameObject.activeSelf)
                    {
                        offHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);
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

                        offHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.UpLeft:

                        // Disable child weapon animator
                        shieldAnimator.runtimeAnimatorController = null;
                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponRearSprite;

                        offHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.Up:
                    case AimDirection.UpRight:

                        // Disable child weapon animator
                        shieldAnimator.runtimeAnimatorController = null;
                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponRearSprite;

                        offHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;

                    case AimDirection.Right:
                    case AimDirection.Down:

                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponFrontSprite;
                        // Re-enable child weapon animator
                        shieldAnimator.runtimeAnimatorController = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponAnimatorController;

                        offHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                }
            }      
        }

        // Flip weapon transform based on player direction
        switch (aimDirection)
        {
            case AimDirection.Left:
            case AimDirection.UpLeft:

                mainHandWeaponRotationPointTransform.localScale = new Vector3(1f, -1f, 1f);
                offHandWeaponRotationPointTransform.localScale = new Vector3(1f, -1f, 1f);
                break;

            case AimDirection.Up:
            case AimDirection.UpRight:
            case AimDirection.Right:
            case AimDirection.Down:

                mainHandWeaponRotationPointTransform.localScale = new Vector3(1f, 1f, 1f);
                offHandWeaponRotationPointTransform.localScale = new Vector3(1f, 1f, 1f);
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
