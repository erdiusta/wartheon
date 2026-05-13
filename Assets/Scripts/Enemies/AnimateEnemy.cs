using UnityEngine;

[DisallowMultipleComponent]
public class AnimateEnemy : MonoBehaviour
{
    Enemy enemy;

    private void Awake()
    {
        // Load components
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        enemy.destroyedEvent.OnDestroyed += DestroyedEvent_OnDestroyed;
    }

    private void OnDisable()
    {
        enemy.destroyedEvent.OnDestroyed -= DestroyedEvent_OnDestroyed;
    }

    /// <summary>
    /// OnDestryed event handler
    /// </summary>
    private void DestroyedEvent_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        SetDeathAnimationParameters();
    }

    public void ApplyMovement(bool moving)
    {
        if (moving) SetMovementAnimationParameters();
        else SetIdleAnimationParameters();
    }

    public void ApplyAim(AimDirection aim)
    {
        if (enemy.animator.GetBool(Settings.isAttack)) return;

        SetAimParameters(aim);
    }

    public void ApplyAttack(AimDirection aim)
    {
        ResetAnimatonParameters();
        SetAimParameters(aim);
        SetAttackAnimationParameters();
    }

    /// <summary>
    /// Initialise aim animation parameters
    /// </summary>
    public void ResetAimAnimationParameters()
    {
        //enemy.animator.SetFloat(Settings.axisX, -1f);
        //enemy.animator.SetFloat(Settings.axisY, -1f);

        enemy.animator.SetBool(Settings.aimUp, false);
        enemy.animator.SetBool(Settings.aimUpRight, false);
        enemy.animator.SetBool(Settings.aimRight, false);
        enemy.animator.SetBool(Settings.aimDownRight, false);
        enemy.animator.SetBool(Settings.aimDown, false);
        enemy.animator.SetBool(Settings.aimDownLeft, false);
        enemy.animator.SetBool(Settings.aimLeft, false);
        enemy.animator.SetBool(Settings.aimUpLeft, false);
    }

    /// <summary>
    /// Set idle animation parameters
    /// </summary>
    public void SetIdleAnimationParameters()
    {
        enemy.animator.SetFloat(Settings.motionType, 0f);

        // Set idle
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, true);
        enemy.animator.SetBool(Settings.isAttack, false);
        enemy.animator.SetBool(Settings.block, false);
    }

    /// <summary>
    /// Set movement animation parameters
    /// </summary>
    public void SetMovementAnimationParameters()
    {
        enemy.animator.SetFloat(Settings.motionType, 1f);

        // Set Moving
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isMoving, true);
        enemy.animator.SetBool(Settings.isAttack, false);
        enemy.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Set attack animation parameters
    /// </summary>
    public void SetAttackAnimationParameters()
    {
        enemy.animator.SetFloat(Settings.motionType, 2f);

        // Set Moving
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isAttack, true);
        enemy.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Play death animation
    /// </summary>
    public void SetDeathAnimationParameters()
    {
        enemy.animator.SetBool(Settings.isAttack, false);
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.death, true);
    }

    /// <summary>
    /// Reset all animation parameters
    /// </summary>
    public void ResetAnimatonParameters()
    {
        enemy.animator.SetBool(Settings.isAttack, false);
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isFrozen, false);
        enemy.animator.SetBool(Settings.dash, false);
        enemy.animator.SetBool(Settings.block, false);
        enemy.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Reset boss animation parameters
    /// </summary>
    public void ResetBossAnimationParameters()
    {
        enemy.animator.SetFloat(Settings.motionType, 0);

        SetCastAnimation(false);
        SetFocusedAnimation(false);
        SetChargeAnimation(false);
    }

    /// <summary>
    /// Set aim animation parameters
    /// </summary>
    public void SetAimParameters(AimDirection aimDirection)
    {
        // Set aim direction
        switch (aimDirection)
        {
            case AimDirection.Up:
                enemy.animator.SetBool(Settings.aimUp, true);
                enemy.animator.SetFloat(Settings.axisX, 0f);
                enemy.animator.SetFloat(Settings.axisY, 1f);
                break;

            case AimDirection.UpRight:
                enemy.animator.SetBool(Settings.aimUpRight, true);
                enemy.animator.SetFloat(Settings.axisX, 0.5f);
                enemy.animator.SetFloat(Settings.axisY, 0.5f);
                break;

            case AimDirection.Right:
                enemy.animator.SetBool(Settings.aimRight, true);
                enemy.animator.SetFloat(Settings.axisX, 1f);
                enemy.animator.SetFloat(Settings.axisY, 0f);
                break;

            case AimDirection.DownRight:
                enemy.animator.SetBool(Settings.aimDownRight, true);
                enemy.animator.SetFloat(Settings.axisX, 0.5f);
                enemy.animator.SetFloat(Settings.axisY, -0.5f);
                break;

            case AimDirection.Down:
                enemy.animator.SetBool(Settings.aimDown, true);
                enemy.animator.SetFloat(Settings.axisX, 0f);
                enemy.animator.SetFloat(Settings.axisY, -1f);
                break;

            case AimDirection.DownLeft:
                enemy.animator.SetBool(Settings.aimDownLeft, true);
                enemy.animator.SetFloat(Settings.axisX, -0.5f);
                enemy.animator.SetFloat(Settings.axisY, -0.5f);
                break;

            case AimDirection.Left:
                enemy.animator.SetBool(Settings.aimLeft, true);
                enemy.animator.SetFloat(Settings.axisX, -1f);
                enemy.animator.SetFloat(Settings.axisY, 0f);
                break;

            case AimDirection.UpLeft:
                enemy.animator.SetBool(Settings.aimUpLeft, true);
                enemy.animator.SetFloat(Settings.axisX, -0.5f);
                enemy.animator.SetFloat(Settings.axisY, 0.5f);
                break;
        }
    }

    public void SetCastAnimation(bool isEnabled)
    {

        enemy.animator.SetBool(Settings.cast, isEnabled);
    }

    public void SetFocusedAnimation(bool isEnabled)
    {
        enemy.animator.SetBool(Settings.focused, isEnabled);
    }

    public void SetChargeAnimation(bool isEnabled)
    {
        enemy.animator.SetBool(Settings.charge, isEnabled);
    }
}
