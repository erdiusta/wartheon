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
        DeathAnimation();
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
    /// Set movement animation parameters
    /// </summary>
    public void SetMovementAnimationParameters()
    {
        // Set Moving
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.isMoving, true);
    }

    /// <summary>
    /// Set idle animation parameters
    /// </summary>
    public void SetIdleAnimationParameters()
    {
        // Set idle
        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, true);
    }

    /// <summary>
    /// Play death animation
    /// </summary>
    public void DeathAnimation()
    {
        // Adjust animator layer weights
        enemy.animator.SetLayerWeight(baseLayerIndex, 0f);
        enemy.animator.SetLayerWeight(attackLayerIndex, 0f);
        enemy.animator.SetLayerWeight(getHitLayerIndex, 0f);
        enemy.animator.SetLayerWeight(deathLayerIndex, 1f);

        enemy.animator.SetBool(Settings.isMoving, false);
        enemy.animator.SetBool(Settings.isIdle, false);
        enemy.animator.SetBool(Settings.death, true);
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
