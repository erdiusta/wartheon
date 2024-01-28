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

<<<<<<< Updated upstream
    AimWeaponEvent aimWeaponEvent;
=======
>>>>>>> Stashed changes
    Player player;

    private void Start()
    {
<<<<<<< Updated upstream
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
    }

    private void OnEnable()
    {
        aimWeaponEvent.OnWeaponAim += AimWeaponEvent_OnWeaponAim;
    }

    private void OnDisable()
    {
        aimWeaponEvent.OnWeaponAim -= AimWeaponEvent_OnWeaponAim;
    }

    private void Start()
    {
        player = GetComponent<Player>();
    }

    /// <summary>
    /// Aim weapon event handler
    /// </summary>
    private void AimWeaponEvent_OnWeaponAim(AimWeaponEvent aimWeaponEvent, AimWeaponEventArgs aimWeaponEventArgs)
    {
        Aim(aimWeaponEventArgs.aimDirection, aimWeaponEventArgs.aimAngle);
=======
        player = GetComponent<Player>();
>>>>>>> Stashed changes
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    public void Aim(AimDirection aimDirection, float aimAngle)
    {
<<<<<<< Updated upstream
        if (gameObject.tag == "Player" && (player.meleeAttack.IsAttackingAtRightHand || player.meleeAttack.IsAttackingAtLeftHand))
=======
        if (gameObject.tag == "Player" && (player.meleeAttackRightHand.IsAttackingAtRightHand || player.meleeAttackLeftHand.IsAttackingAtLeftHand))
>>>>>>> Stashed changes
            return;

        // Set angle of the weapon transform
        rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);
        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);
<<<<<<< Updated upstream
=======

        // If left hand has a shield, fix the shield position
        if (tag == "Player")
        {
            if (player.activeWeapon.GetCurrentLeftHandWeapon() != null &&
                player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
            {
                switch (aimDirection)
                {
                    case AimDirection.Left:
                    case AimDirection.UpLeft:

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 180f);
                        break;

                    case AimDirection.Up:
                    case AimDirection.UpRight:
                    case AimDirection.Right:
                    case AimDirection.Down:

                        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, 0f);
                        break;
                }
            }
        }
>>>>>>> Stashed changes

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
