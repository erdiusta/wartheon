using System;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class AnimatePlayer : MonoBehaviour
{
    Player player;

    [HideInInspector] public int baseLayerIndex;
    [HideInInspector] public int attackLayerIndex;
    [HideInInspector] public int getHitLayerIndex;
    [HideInInspector] public int deathLayerIndex;

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

    private void Start()
    {
        baseLayerIndex = player.animator.GetLayerIndex("Base Layer");
        attackLayerIndex = player.animator.GetLayerIndex("Attack Layer");
        getHitLayerIndex = player.animator.GetLayerIndex("Get Hit Layer");
        deathLayerIndex = player.animator.GetLayerIndex("Death Layer");

        // Adjust animator layer weights
        player.animator.SetLayerWeight(baseLayerIndex, 1f);
        player.animator.SetLayerWeight(getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(attackLayerIndex, 0f);
        player.animator.SetLayerWeight(deathLayerIndex, 0f);
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
            // Adjust animator layer weights
            player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 1f);
            player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 0f);
            player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
            player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

            player.animator.SetBool(Settings.isMoving, true);
            player.animator.SetBool(Settings.isIdle, false);
            player.animator.SetBool(Settings.getHit, false);
            player.animator.SetBool(Settings.death, false);
        }
    }

    /// <summary>
    /// Set movement animation parameters
    /// </summary>
    public void SetIdleAnimationParameters()
    {
        if (player.meleeAttackRightHand.playerAttackMotionRoutine == null)
        {
            // Adjust animator layer weights
            player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 1f);
            player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 0f);
            player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
            player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

            player.animator.SetBool(Settings.isMoving, false);
            player.animator.SetBool(Settings.isIdle, true);
            player.animator.SetBool(Settings.getHit, false);
            player.animator.SetBool(Settings.death, false);
        }
    }

    /// <summary>
    /// Play get hit animation
    /// </summary>
    public void SetGetHitAnimationParameters()
    {
        // Adjust animator layer weights
        player.animator.SetLayerWeight(baseLayerIndex, 0f);
        player.animator.SetLayerWeight(attackLayerIndex, 0f);
        player.animator.SetLayerWeight(getHitLayerIndex, 1f);
        player.animator.SetLayerWeight(deathLayerIndex, 0f);

        player.animator.SetBool(Settings.isAttacking, false);
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, false);
        player.animator.SetBool(Settings.getHit, true);
        player.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Play death animation
    /// </summary>
    public void SetDeathAnimationParameters()
    {
        // Adjust animator layer weights
        player.animator.SetLayerWeight(baseLayerIndex, 0f);
        player.animator.SetLayerWeight(attackLayerIndex, 0f);
        player.animator.SetLayerWeight(getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(deathLayerIndex, 1f);

        player.animator.SetBool(Settings.isAttacking, false);
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, false);
        player.animator.SetBool(Settings.getHit, false);
        player.animator.SetBool(Settings.death, true);
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
}
