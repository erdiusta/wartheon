using Mirror;
using UnityEngine;

public class EnemyAnimationSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnIsMovingChanged))]
    public bool isMoving;

    [SyncVar(hook = nameof(OnAimChanged))]
    public AimDirection aimDirection;

    AnimateEnemy animateEnemy;
    Animator animator;

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
        RpcPlayAttack(aim);
    }

    [ClientRpc]
    private void RpcPlayAttack(AimDirection aim)
    {
        if (isServer) return;

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
        if (isServer) return;

        animateEnemy?.ResetAnimatonParameters();
    }

    [Server]
    public void ResetAllBossAnimations()
    {
        RpcResetAllBossAnimations();
    }

    [Server]
    public void ResetAimAnimations()
    {
        RpcResetAimAnimations();
    }

    [ClientRpc]
    private void RpcResetAimAnimations()
    {
        if (isServer) return;

        animateEnemy?.ResetAimAnimationParameters();
    }

    [ClientRpc]
    private void RpcResetAllBossAnimations()
    {
        if (isServer) return;

        animateEnemy?.ResetBossAnimationParameters();
    }

    [Server]
    public void SetBossCastAnimation(bool isEnabled)
    {
        RpcSetBossCastAnimation(isEnabled);
    }

    [ClientRpc]
    public void RpcSetBossCastAnimation(bool isEnabled)
    {
        if (isServer) return;

        animateEnemy?.SetCastAnimation(isEnabled);
    }

    [Server]
    public void SetBossChargeAnimation(bool isEnabled)
    {
        RpcSetBossChargeAnimation(isEnabled);
    }

    [ClientRpc]
    public void RpcSetBossChargeAnimation(bool isEnabled)
    {
        if (isServer) return;

        animateEnemy?.SetChargeAnimation(isEnabled);
    }

    [Server]
    public void SetBossFocusedAnimation(bool isEnabled)
    {
        RpcSetBossFocusedAnimation(isEnabled);
    }

    [ClientRpc]
    public void RpcSetBossFocusedAnimation(bool isActive)
    {
        if (isServer) return;

        animateEnemy?.SetFocusedAnimation(isActive);
    }
}
