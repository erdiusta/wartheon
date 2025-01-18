using UnityEngine;

[RequireComponent(typeof(Enemy))]
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

    /// <summary>
    /// Initialise aim animation parameters
    /// </summary>
    public void ResetAimAnimationParameters()
    {
        enemy.animator.SetBool(Settings.aimUp, false);
        enemy.animator.SetBool(Settings.aimUpRight, false);
        enemy.animator.SetBool(Settings.aimUpLeft, false);
        enemy.animator.SetBool(Settings.aimRight, false);
        enemy.animator.SetBool(Settings.aimLeft, false);
        enemy.animator.SetBool(Settings.aimDown, false);
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
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.isAttacking, false);
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
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.isAttacking, false);
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
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.isAttacking, true);
        enemy.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Play death animation
    /// </summary>
    public void SetDeathAnimationParameters()
    {
        enemy.animator.SetBool(Settings.isAttacking, false);
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.death, true);
    }

    /// <summary>
    /// Reset all animation parameters
    /// </summary>
    public void ResetAnimatonParameters()
    {
        enemy.animator.SetBool(Settings.isAttacking, false);
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isFrozen, false);

        enemy.animator.SetBool(Settings.dash, true);

        if (HasParameter(enemy.animator, Settings.getHit))
        {
            enemy.animator.SetBool(Settings.getHit, false);
        }

        if (HasParameter(enemy.animator, Settings.block))
        {
            enemy.animator.SetBool(Settings.block, false);
        }

        enemy.animator.SetBool(Settings.death, false);
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
                enemy.animator.SetBool(Settings.aimUp, true);
                enemy.animator.SetFloat(Settings.axisX, 0f);
                enemy.animator.SetFloat(Settings.axisY, 1f);
                break;

            case AimDirection.Down:
                enemy.animator.SetBool(Settings.aimDown, true);
                enemy.animator.SetFloat(Settings.axisX, 0f);
                enemy.animator.SetFloat(Settings.axisY, -1f);
                break;

            case AimDirection.Right:
                enemy.animator.SetBool(Settings.aimRight, true);
                enemy.animator.SetFloat(Settings.axisX, 1f);
                enemy.animator.SetFloat(Settings.axisY, 0f);
                break;

            case AimDirection.Left:
                enemy.animator.SetBool(Settings.aimLeft, true);
                enemy.animator.SetFloat(Settings.axisX, -1f);
                enemy.animator.SetFloat(Settings.axisY, 0f);
                break;

            case AimDirection.UpRight:
                enemy.animator.SetBool(Settings.aimUpRight, true);
                enemy.animator.SetFloat(Settings.axisX, 0.7f);
                enemy.animator.SetFloat(Settings.axisY, 0.7f);
                break;

            case AimDirection.UpLeft:
                enemy.animator.SetBool(Settings.aimUpLeft, true);
                enemy.animator.SetFloat(Settings.axisX, -0.7f);
                enemy.animator.SetFloat(Settings.axisY, 0.7f);
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
