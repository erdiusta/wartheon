using Mirror;

public class EnemyAnimationSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnIsMovingChanged))]
    public bool isMoving;

    [SyncVar(hook = nameof(OnAimChanged))]
    public AimDirection aimDirection;

    AnimateEnemy animateEnemy;

    public override void OnStartClient()
    {
        base.OnStartClient();

        animateEnemy = GetComponent<AnimateEnemy>();
    }

    #region HOOKS
    private void OnIsMovingChanged(bool _, bool value)
    {
        animateEnemy?.ApplyMovement(value);
    }

    private void OnAimChanged(AimDirection _, AimDirection value)
    {
        animateEnemy?.ApplyAim(value);
    }
    #endregion

    [Server]
    public void UpdateAnimationStateServer(bool moving, AimDirection aimDir)
    {
        isMoving = moving;
        aimDirection = aimDir;
    }

    [Server]
    public void PlayAttackServer(AimDirection aim)
    {
        if (!isServer) return;

        RpcPlayAttack(aim);
    }

    [ClientRpc]
    private void RpcPlayAttack(AimDirection aim)
    {
        animateEnemy?.ApplyAttack(aim);
    }

    [Server]
    public void ResetAllAnimations()
    {
        RpcResetAnimations();
    }

    [ClientRpc]
    private void RpcResetAnimations()
    {
        animateEnemy?.ResetAnimatonParameters();
    }
}
