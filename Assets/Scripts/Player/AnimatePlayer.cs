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

    /// <summary>
    /// Initialise aim animation parameters
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
    /// Set movement animation parameters
    /// </summary>
    public void SetMovementAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, true);
        player.animator.SetBool(Settings.isIdle, false);
    }

    /// <summary>
    /// Set movement animation parameters
    /// </summary>
    public void SetIdleAnimationParameters()
    {
        player.animator.SetBool(Settings.isMoving, false);
        player.animator.SetBool(Settings.isIdle, true);
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
