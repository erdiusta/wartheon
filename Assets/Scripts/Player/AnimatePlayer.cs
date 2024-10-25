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
        player.animator.SetBool(Settings.aimUp, false);
        player.animator.SetBool(Settings.aimUpRight, false);
        player.animator.SetBool(Settings.aimUpLeft, false);
        player.animator.SetBool(Settings.aimRight, false);
        player.animator.SetBool(Settings.aimLeft, false);
        player.animator.SetBool(Settings.aimDown, false);
    }

    /// <summary>
    /// Initialize roll animation parameters
    /// </summary>
    public void InitializeRollAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, false);

        player.animator.SetBool(Settings.rollDown, false);
        player.animator.SetBool(Settings.rollRight, false);
        player.animator.SetBool(Settings.rollLeft, false);
        player.animator.SetBool(Settings.rollUp, false);
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
    /// Set movement animation parameters
    /// </summary>
    public void SetMovementAnimationParameters()
    {
        if (player.meleeAttackRightHand.playerAttackMotionRoutine == null || !player.isDead)
        {
            player.animator.SetBool(Settings.isMoving, true);
            player.animator.SetBool(Settings.isIdle, false);
            player.animator.SetBool(Settings.getHit, false);
            player.animator.SetBool(Settings.death, false);
        }
    }

    /// <summary>
    /// Reset all animation parameters
    /// </summary>
    public void ResetAnimatonParameters()
    {
        player.animator.SetBool(Settings.isAttacking, false);
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, false);

        if (HasParameter(player.animator, Settings.getHit))
        {
            player.animator.SetBool(Settings.getHit, false);
        }

        if (HasParameter(player.animator, Settings.block))
        {
            player.animator.SetBool(Settings.block, false);
        }

        player.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Set aim animation parameters
    /// </summary>
    public void SetAimWeaponAnimationParameters(AimDirection aimDirection)
    {
        // Set aim direction
        switch (aimDirection)
        {
            case AimDirection.Up:
                player.animator.SetBool(Settings.aimUp, true);
                break;

            case AimDirection.UpRight:
                player.animator.SetBool(Settings.aimUpRight, true);
                break;

            case AimDirection.UpLeft:
                player.animator.SetBool(Settings.aimUpLeft, true);
                break;

            case AimDirection.Right:
                player.animator.SetBool(Settings.aimRight, true);
                break;

            case AimDirection.Left:
                player.animator.SetBool(Settings.aimLeft, true);
                break;

            case AimDirection.Down:
                player.animator.SetBool(Settings.aimDown, true);
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
