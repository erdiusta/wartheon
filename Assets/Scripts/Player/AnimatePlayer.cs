using System;
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
        // Subscribe to movement to position event
        player.movementToPositionEvent.OnMovementToPosition += MovementToPositionEvent_OnMovementToPosition;
    }

    private void OnDisable()
    {
        // Unsubscribe from movement to position event
        player.movementToPositionEvent.OnMovementToPosition -= MovementToPositionEvent_OnMovementToPosition;
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
    }

    /// <summary>
    /// Set aim animation parameters
    /// </summary>
    public void SetAimWeaponAnimationParameters(AimDirection aimDirection, AttackDirection attackDirection)
    {
        // Set aim direction
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

        // Set attack aim direction
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

    // Method to check if the Animator contains the specified parameter
    bool HasParameter(Animator animator, int paramHashCode)
    {
        foreach (AnimatorControllerParameter param in animator.parameters)
        {
            if (param.GetHashCode() == paramHashCode)
            {
                return true;
            }
        }
        return false;
    }
}
