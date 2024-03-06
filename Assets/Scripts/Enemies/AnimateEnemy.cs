using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class AnimateEnemy : MonoBehaviour
{
    Enemy enemy;

    [HideInInspector] public int baseLayerIndex;
    [HideInInspector] public int attackLayerIndex;
    [HideInInspector] public int getHitLayerIndex;
    [HideInInspector] public int deathLayerIndex;
    [HideInInspector] public bool isDying = false;

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

    private void Start()
    {
        baseLayerIndex = enemy.animator.GetLayerIndex("Base Layer");
        attackLayerIndex = enemy.animator.GetLayerIndex("Attack Layer");
        getHitLayerIndex = enemy.animator.GetLayerIndex("Get Hit Layer");
        deathLayerIndex = enemy.animator.GetLayerIndex("Death Layer");

        // Adjust animator layer weights
        enemy.animator.SetLayerWeight(baseLayerIndex, 1f);
        enemy.animator.SetLayerWeight(getHitLayerIndex, 0f);
        enemy.animator.SetLayerWeight(attackLayerIndex, 0f);
        enemy.animator.SetLayerWeight(deathLayerIndex, 0f);
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
    public void InitializeAimAnimationParameters()
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
        if (!isDying)
        {
            // Set idle
            enemy.animator.SetLayerWeight(baseLayerIndex, 1f);
            enemy.animator.SetLayerWeight(attackLayerIndex, 0f);
            enemy.animator.SetLayerWeight(getHitLayerIndex, 0f);
            enemy.animator.SetLayerWeight(deathLayerIndex, 0f);

            enemy.animator.SetBool(Settings.isMoving, false);
            enemy.animator.SetBool(Settings.isIdle, true);
            enemy.animator.SetBool(Settings.getHit, false);
            enemy.animator.SetBool(Settings.attackMotion, false);
            enemy.animator.SetBool(Settings.block, false);
        }
        else
        {
            enemy.animator.SetBool(Settings.isIdle, false);
            enemy.animator.SetBool(Settings.isMoving, false);
            enemy.animator.SetBool(Settings.getHit, false);
            enemy.animator.SetBool(Settings.attackMotion, false);
            enemy.animator.SetBool(Settings.preAttackMotion, false);
            enemy.animator.SetBool(Settings.getHit, false);
        }
    }

    /// <summary>
    /// Set movement animation parameters
    /// </summary>
    public void SetMovementAnimationParameters()
    {
        if (!isDying)
        {
            // Set Moving
            enemy.animator.SetLayerWeight(baseLayerIndex, 1f);
            enemy.animator.SetLayerWeight(attackLayerIndex, 0f);
            enemy.animator.SetLayerWeight(getHitLayerIndex, 0f);
            enemy.animator.SetLayerWeight(deathLayerIndex, 0f);

            enemy.animator.SetBool(Settings.isIdle, false);
            enemy.animator.SetBool(Settings.isMoving, true);
            enemy.animator.SetBool(Settings.getHit, false);
            enemy.animator.SetBool(Settings.attackMotion, false);
            enemy.animator.SetBool(Settings.preAttackMotion, false);
            enemy.animator.SetBool(Settings.death, false);
        }
        else
        {
            enemy.animator.SetBool(Settings.isIdle, false);
            enemy.animator.SetBool(Settings.isMoving, false);
            enemy.animator.SetBool(Settings.getHit, false);
            enemy.animator.SetBool(Settings.attackMotion, false);
            enemy.animator.SetBool(Settings.preAttackMotion, false);
            enemy.animator.SetBool(Settings.death, true);
        }
    }

    /// <summary>
    /// Play attack animation
    /// </summary>
    public void SetAttackAnimationParameters()
    {
        // Adjust animator layer weights
        enemy.animator.SetLayerWeight(baseLayerIndex, 0.3f);
        enemy.animator.SetLayerWeight(attackLayerIndex, 0.7f);
        enemy.animator.SetLayerWeight(getHitLayerIndex, 0f);
        enemy.animator.SetLayerWeight(deathLayerIndex, 0f);

        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Play get hit animation
    /// </summary>
    public void SetGetHitAnimationParameters()
    {
        // Adjust animator layer weights
        enemy.animator.SetLayerWeight(baseLayerIndex, 0f);
        enemy.animator.SetLayerWeight(attackLayerIndex, 0f);
        enemy.animator.SetLayerWeight(getHitLayerIndex, 1f);
        enemy.animator.SetLayerWeight(deathLayerIndex, 0f);

        enemy.animator.SetBool(Settings.attackMotion, false);
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.death, false);
    }

    /// <summary>
    /// Play death animation
    /// </summary>
    public void SetDeathAnimationParameters()
    {
        // Adjust animator layer weights
        enemy.animator.SetLayerWeight(baseLayerIndex, 0f);
        enemy.animator.SetLayerWeight(attackLayerIndex, 0f);
        enemy.animator.SetLayerWeight(getHitLayerIndex, 0f);
        enemy.animator.SetLayerWeight(deathLayerIndex, 1f);

        isDying = true;

        enemy.animator.SetBool(Settings.attackMotion, false);
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
        enemy.animator.SetBool(Settings.attackMotion, false);
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.death, false);
        enemy.animator.SetBool(Settings.block, false);
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
                break;

            case AimDirection.UpRight:
                enemy.animator.SetBool(Settings.aimUpRight, true);
                break;

            case AimDirection.UpLeft:
                enemy.animator.SetBool(Settings.aimUpLeft, true);
                break;

            case AimDirection.Right:
                enemy.animator.SetBool(Settings.aimRight, true);
                break;

            case AimDirection.Left:
                enemy.animator.SetBool(Settings.aimLeft, true);
                break;

            case AimDirection.Down:
                enemy.animator.SetBool(Settings.aimDown, true);
                break;
        }
    }
}
