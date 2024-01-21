using UnityEngine;

[RequireComponent(typeof(AimWeaponEvent))]
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

    AimWeaponEvent aimWeaponEvent;
    Player player;

    private void Awake()
    {
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
    }

    /// <summary>
    /// Aim the weapon
    /// </summary>
    private void Aim(AimDirection aimDirection, float aimAngle)
    {
        if (gameObject.tag == "Player" && (player.meleeAttack.IsAttackingAtRightHand || player.meleeAttack.IsAttackingAtLeftHand))
            return;

        // Set angle of the weapon transform
        rightHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);
        leftHandWeaponRotationPointTransform.eulerAngles = new Vector3(0f, 0f, aimAngle);

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
