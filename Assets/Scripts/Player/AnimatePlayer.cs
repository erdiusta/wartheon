using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class AnimatePlayer : MonoBehaviour
{
    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        if (player == null) return;

        Subscribe();
    }

    private void OnDisable()
    {
        if (player == null) return;

        Unsubscribe();
    }

    private void Subscribe()
    {
        if (player == null) return;

        player.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;
    }

    private void Unsubscribe()
    {
        if (player == null) return;

        player.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;
    }

    public void ApplyMovement(bool moving)
    {
        if (moving) SetMovementAnimationParameters();
        else SetIdleAnimationParameters();
    }

    public void ApplyRoll(RollDirection dir)
    {
        ResetAnimatonParameters();
        InitializeRollAnimationParameters();

        switch (dir)
        {
            case RollDirection.Up:
                player.animator.SetBool(Settings.rollUp, true);
                break;
            case RollDirection.Right:
                player.animator.SetBool(Settings.rollRight, true);
                break;
            case RollDirection.Down:
                player.animator.SetBool(Settings.rollDown, true);
                break;
            case RollDirection.Left:
                player.animator.SetBool(Settings.rollLeft, true);
                break;
            default:
                break;
        }
    }

    #region CAELION
    public void ApplyShield(AimDirection aimDirection, Player player)
    {
        Weapon playerOffHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        Transform offHandHoldingHand = player.transform.GetChild(2).GetChild(0).GetChild(0).GetChild(0);

        Animator shieldAnimator = player.transform.GetChild(2).GetComponent<Animator>();
        SpriteRenderer shieldSpriteRenderer = player.transform.GetChild(2).GetChild(0).GetChild(0).GetComponent<SpriteRenderer>();

        if (playerOffHandWeapon != null && playerOffHandWeapon.weaponStats.weaponClass == WeaponClass.Shield)
        {
            var currentShieldDetails = WartheonDatabase.Instance.GetWeaponDetails(playerOffHandWeapon.weaponStats.weaponTitle);

            switch (aimDirection)
            {
                case AimDirection.Up:
                case AimDirection.UpRight:
                case AimDirection.UpLeft:
                    shieldAnimator.runtimeAnimatorController = null;
                    shieldSpriteRenderer.sprite = currentShieldDetails.weaponRearSprite;
                    offHandHoldingHand.gameObject.SetActive(true);
                    break;
                default:
                    shieldSpriteRenderer.sprite = currentShieldDetails.weaponFrontSprite;
                    shieldAnimator.runtimeAnimatorController = currentShieldDetails.weaponAnimatorController;
                    offHandHoldingHand.gameObject.SetActive(false);
                    break;
            }
        }
        else
        {
            if (!offHandHoldingHand.gameObject.activeSelf) offHandHoldingHand.gameObject.SetActive(true);
        }
    }

    public void ApplySeismicSlam()
    {
        player.animator.SetTrigger("seismicSlam");
    }

    public void ApplyShieldBash(bool undo)
    {
        player.animator.SetBool("shieldBash", !undo);
    }
    #endregion

    #region MORVEN

    public void ApplyBloodDrain(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetBool("blood", !undo);
    }

    public void ApplyCullTheMeek(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetBool("cullTheMeek", !undo);
    }
    #endregion

    #region MYCARA
    public void ApplySheerCold(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetBool("sheerCold", !undo);
    }
    #endregion

    #region NYXA
    public void ApplyDontBlink(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetBool("dontBlink", !undo);
    }

    public void ApplyWhisperSlice(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetBool("whisperSlice", !undo);
    }
    #endregion

    #region KARNAG
    public void ApplyRage(AimDirection aim, AttackDirection attackDir)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetTrigger("rage");
    }

    public void ApplyShatterCry(AimDirection aim, AttackDirection attackDir)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetTrigger("shatterCry");
    }

    public void ApplyWhirlrend(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);

        player.animator.SetBool("whirlrend", !undo);
    }
    #endregion

    public void EndRoll()
    {
        ResetAnimatonParameters();
        SetIdleAnimationParameters();
    }

    public void ApplyAttack(AimDirection aim, AttackDirection attackDir)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);
        SetAttackAnimationParameters();
    }

    public void ApplyParry(AttackDirection attackDir)
    {
        ResetAnimatonParameters();
        SetAttackDirectionParameters(attackDir);
        InitializeParryAnimationParameters();
    }

    public void EndParry(AimDirection aim, AttackDirection attackDir)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackDirectionParameters(attackDir);
        SetIdleAnimationParameters();
    }

    public void ApplyAim(AimDirection aim)
    {
        if (player.animator.GetBool(Settings.isAttack)) return;

        ApplyShield(aim, player);
        SetAimParameters(aim);
    }

    private void MovementToPositionEvent_OnMovementToPosition(MovementToPositionEvent movementToPositionEvent, MovementToPositionArgs movementToPositionArgs)
    {
        InitializeAimAnimationParameters();
        InitializeRollAnimationParameters();
        SetToMovementToPositionAnimationParameters(movementToPositionArgs);
    }

    /// <summary>
    /// Initialize aim animation parameters
    /// </summary>
    public void InitializeAimAnimationParameters()
    {
        // Reset aim parameters
        player.animator.SetBool(Settings.aimUp, false);
        player.animator.SetBool(Settings.aimRight, false);
        player.animator.SetBool(Settings.aimLeft, false);
        player.animator.SetBool(Settings.aimDown, false);

        // Reset attack aim parameters
        player.animator.SetBool(Settings.attackUp, false);
        player.animator.SetBool(Settings.attackUpRight, false);
        player.animator.SetBool(Settings.attackRight, false);
        player.animator.SetBool(Settings.attackDownRight, false);
        player.animator.SetBool(Settings.attackDown, false);
        player.animator.SetBool(Settings.attackDownLeft, false);
        player.animator.SetBool(Settings.attackLeft, false);
        player.animator.SetBool(Settings.attackUpLeft, false);
    }


    /// <summary>
    /// Set idle animation parameters
    /// </summary>
    public void SetIdleAnimationParameters()
    {
        // Set idle
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, true);
        player.animator.SetBool(Settings.isRoll, false);
        player.animator.SetBool(Settings.isAttack, false);
        player.animator.SetBool(Settings.isParry, false);

        player.animator.SetFloat(Settings.motionType, 0f);
    }

    /// <summary>
    /// Set attack animation parameters
    /// </summary>
    public void SetAttackAnimationParameters()
    {
        // Set attack
        player.animator.SetBool(Settings.isAttack, true);
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, false);
        player.animator.SetBool(Settings.isRoll, false);

    }

    /// <summary>
    /// Set movement animation parameters
    /// </summary>
    public void SetMovementAnimationParameters()
    {
        if (!player.health.hasDied)
        {
            player.animator.SetBool(Settings.isMoving, true);
            player.animator.SetBool(Settings.isIdle, false);
            player.animator.SetBool(Settings.isRoll, false);
            player.animator.SetBool(Settings.death, false);
            player.animator.SetBool(Settings.isAttack, false);
        }

        player.animator.SetFloat(Settings.motionType, 1f);
    }

    /// <summary>
    /// Initialize roll animation parameters
    /// </summary>
    public void InitializeRollAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isAttack, false);
        player.animator.SetBool(Settings.isIdle, false);
        player.animator.SetBool(Settings.isRoll, true);
        player.animator.SetBool(Settings.isParry, false);

        player.animator.SetBool(Settings.rollDown, false);
        player.animator.SetBool(Settings.rollRight, false);
        player.animator.SetBool(Settings.rollLeft, false);
        player.animator.SetBool(Settings.rollUp, false);
    }

    /// <summary>
    /// Initialize parry animation parameters
    /// </summary>
    public void InitializeParryAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isAttack, false);
        player.animator.SetBool(Settings.isIdle, false);
        player.animator.SetBool(Settings.isRoll, false);
        player.animator.SetBool(Settings.isParry, true);
    }


    /// <summary>
    /// Set movement to position animation parameters
    /// </summary>
    private void SetToMovementToPositionAnimationParameters(MovementToPositionArgs movementToPositionArgs)
    {
        // Animate roll
        if (movementToPositionArgs.isRolling)
        {
            if (movementToPositionArgs.moveDirection.x > 0f)
            {
                player.animator.SetBool(Settings.rollRight, true);
            }
            else if (movementToPositionArgs.moveDirection.x < 0f)
            {
                player.animator.SetBool(Settings.rollLeft, true);
            }
            else if (movementToPositionArgs.moveDirection.y > 0f)
            {
                player.animator.SetBool(Settings.rollUp, true);
            }
            else if (movementToPositionArgs.moveDirection.y < 0f)
            {
                player.animator.SetBool(Settings.rollDown, true);
            }
        }
    }

    /// <summary>
    /// Reset all animation parameters
    /// </summary>
    public void ResetAnimatonParameters()
    {
        player.animator.SetBool(Settings.isAttack, false);
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, false);
        player.animator.SetBool(Settings.isRoll, false);
        player.animator.SetBool(Settings.isParry, false);
        player.animator.SetBool(Settings.block, false);
        player.animator.SetBool(Settings.death, false);

        player.animator.SetBool(Settings.attackUp, false);
        player.animator.SetBool(Settings.attackUpRight, false);
        player.animator.SetBool(Settings.attackRight, false);
        player.animator.SetBool(Settings.attackDownRight, false);
        player.animator.SetBool(Settings.attackDown, false);
        player.animator.SetBool(Settings.attackDownLeft, false);
        player.animator.SetBool(Settings.attackLeft, false);
        player.animator.SetBool(Settings.attackUpLeft, false);
    }

    /// <summary>
    /// Set aim animation parameters
    /// </summary>
    public void SetAimWeaponAnimationParameters(AimDirection aimDirection, AttackDirection attackDirection)
    {
        // Set aim direction
        SetAimParameters(aimDirection);

        // Set attack aim direction
        SetAttackDirectionParameters(attackDirection);
    }

    public void SetAimParameters(AimDirection aimDirection)
    {
        switch (aimDirection)
        {
            case AimDirection.Up:
                player.animator.SetFloat(Settings.axisX, 0f);
                player.animator.SetFloat(Settings.axisY, 1f);
                break;

            case AimDirection.UpRight:
                player.animator.SetFloat(Settings.axisX, 0.5f);
                player.animator.SetFloat(Settings.axisY, 0.5f);
                break;

            case AimDirection.Right:
                player.animator.SetFloat(Settings.axisX, 1f);
                player.animator.SetFloat(Settings.axisY, 0f);
                break;

            case AimDirection.DownRight:
                player.animator.SetFloat(Settings.axisX, 0.5f);
                player.animator.SetFloat(Settings.axisY, -0.5f);
                break;

            case AimDirection.Left:
                player.animator.SetFloat(Settings.axisX, -1f);
                player.animator.SetFloat(Settings.axisY, 0f);
                break;

            case AimDirection.DownLeft:
                player.animator.SetFloat(Settings.axisX, -0.5f);
                player.animator.SetFloat(Settings.axisY, -0.5f);
                break;

            case AimDirection.Down:
                player.animator.SetFloat(Settings.axisX, 0f);
                player.animator.SetFloat(Settings.axisY, -1f);
                break;

            case AimDirection.UpLeft:
                player.animator.SetFloat(Settings.axisX, -0.5f);
                player.animator.SetFloat(Settings.axisY, 0.5f);
                break;
        }
    }

    public void SetAttackDirectionParameters(AttackDirection attackDirection)
    {
        switch (attackDirection)
        {
            case AttackDirection.Up:
                player.animator.SetBool(Settings.attackUp, true);
                break;
            case AttackDirection.UpRight:
                player.animator.SetBool(Settings.attackUpRight, true);
                break;
            case AttackDirection.Right:
                player.animator.SetBool(Settings.attackRight, true);
                break;
            case AttackDirection.DownRight:
                player.animator.SetBool(Settings.attackDownRight, true);
                break;
            case AttackDirection.Down:
                player.animator.SetBool(Settings.attackDown, true);
                break;
            case AttackDirection.DownLeft:
                player.animator.SetBool(Settings.attackDownLeft, true);
                break;
            case AttackDirection.Left:
                player.animator.SetBool(Settings.attackLeft, true);
                break;
            case AttackDirection.UpLeft:
                player.animator.SetBool(Settings.attackUpLeft, true);
                break;
            default:
                break;
        }
    }
}
