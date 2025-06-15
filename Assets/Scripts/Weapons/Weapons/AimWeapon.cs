using UnityEngine;

[DisallowMultipleComponent]
public class AimWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the Transform from the child RightHandWeaponAnchorPoint gameobject")]
    #endregion
    public Transform mainHandWeaponAnchorPointTransform;

    Transform mainHandWeaponRotationPointTransform;
    Player player;
    Enemy enemy;

    Transform mainHandShootPosition;

    private void Start()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();

        mainHandWeaponRotationPointTransform = mainHandWeaponAnchorPointTransform.GetChild(0);

        if (tag == Settings.enemyTag)
        {

            mainHandShootPosition = enemy.enemyAI?.weaponShootPosition;
        }
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    public void Aim(AimDirection aimDirection, AttackDirection attackDirection, float aimAngle)
    {
        if (tag == Settings.enemyTag)
        {
            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

            if (enemy.enemyDetails.enemyWeapon != null && enemy.enemyDetails.enemyWeapon.weaponClass == WeaponClass.Staff)
            {
                switch (aimDirection)
                {
                    case AimDirection.Left:

                        mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.Up:
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
                        mainHandShootPosition.localPosition = enemy.enemyDetails.enemyWeapon.weaponRightShootPosition;
                        break;
                    case AimDirection.Left:
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

            // Bow aim
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null && (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow ||
                player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow))
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow)
                {
                    switch (aimDirection)
                    {
                        case AimDirection.Up:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.09375f, 0.5625f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 90f);
                            break;
                        case AimDirection.UpRight:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.4f, 0.38f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 45f);
                            break;
                        case AimDirection.Right:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.375f, 0.134f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                            break;
                        case AimDirection.DownRight:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.7f, -0.3125f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -45f);
                            break;
                        case AimDirection.Down:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.28125f, -0.8125f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -90f);
                            break;
                        case AimDirection.DownLeft:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.71875f, -0.28125f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -135f);
                            break;
                        case AimDirection.Left:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.38f, 0.1332785f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -180f);
                            break;
                        case AimDirection.UpLeft:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.21875f, 0.5f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 135f);
                            break;

                        default:
                            break;
                    }
                }
                else if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow)
                {
                    switch (aimDirection)
                    {
                        case AimDirection.Up:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.5f, 0.5f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 90f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(-1f, -1f, 1f);
                            break;
                        case AimDirection.UpRight:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.8f, 0f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 45f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(1f, 1f, 1f);
                            break;
                        case AimDirection.Right:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0f, 0f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(1f, 1f, 1f);
                            break;
                        case AimDirection.DownRight:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0f, -0.375f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -45f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(1f, 1f, 1f);
                            break;
                        case AimDirection.Down:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.5f, -0.25f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -90f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(-1f, -1f, 1f);
                            break;
                        case AimDirection.DownLeft:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0.03125f, -0.375f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -135f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(-1f, -1f, 1f);
                            break;
                        case AimDirection.Left:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(0f, 0f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, -180f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(-1f, -1f, 1f);
                            break;
                        case AimDirection.UpLeft:
                            mainHandWeaponRotationPointTransform.localPosition = new Vector3(-0.8f, 0f, 0f);
                            mainHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 135f);
                            mainHandWeaponRotationPointTransform.localScale = new Vector3(-1f, -1f, 1f);
                            break;

                        default:
                            break;
                    }
                }
            }

            // In case of shield
            Animator shieldAnimator = player.transform.GetChild(1).GetComponent<Animator>();
            SpriteRenderer shieldSpriteRenderer = player.transform.GetChild(1).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

            //if (player.activeWeapon.GetCurrentOffHandWeapon() == null)
            //{
            //    shieldAnimator.runtimeAnimatorController = null;
            //    shieldSpriteRenderer.sprite = null;
            //}

            if (player.activeWeapon.GetCurrentOffHandWeapon() != null &&
                player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
            {

                if (player.moveStatus == MoveStatus.Stun || player.moveStatus == MoveStatus.Frozen) return;

                switch (aimDirection)
                {
                    case AimDirection.Up:
                    case AimDirection.UpRight:
                    case AimDirection.UpLeft:

                        // Disable child weapon animator
                        shieldAnimator.runtimeAnimatorController = null;
                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponRearSprite;
                        break;

                    case AimDirection.Right:
                    case AimDirection.DownRight:
                    case AimDirection.Down:
                    case AimDirection.DownLeft:
                    case AimDirection.Left:

                        // Flip rear face if equipped weapon is a shield
                        shieldSpriteRenderer.sprite = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponFrontSprite;
                        // Re-enable child weapon animator
                        shieldAnimator.runtimeAnimatorController = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponAnimatorController;
                        break;

                    default:
                        break;
                }
            }
        }
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(mainHandWeaponAnchorPointTransform), mainHandWeaponAnchorPointTransform);
    }
#endif
    #endregion
}
