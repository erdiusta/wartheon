using UnityEngine;
using Mirror;

public class PlayerAnimationSync : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnIsMovingChanged))] 
    public bool isMoving;

    [SyncVar(hook = nameof(OnAimChanged))] 
    public AimDirection aimDirection;

    [SyncVar(hook = nameof(OnAttackDirChanged))] 
    public AttackDirection attackDirection;

    AnimatePlayer animatePlayer;

    // Cached states
    AimDirection cachedAim;
    AttackDirection cachedAttackDir;

    public override void OnStartClient()
    {
        base.OnStartClient();

        animatePlayer = GetComponent<AnimatePlayer>();
    }

    #region Hooks
    private void OnIsMovingChanged(bool _, bool value)
    {
        if (isLocalPlayer) return;

        animatePlayer?.ApplyMovement(value);
    }

    private void OnAimChanged(AimDirection _, AimDirection value)
    {
        cachedAim = value;

        if (isLocalPlayer) return;

        animatePlayer?.ApplyAim(value);
    }

    private void OnAttackDirChanged(AttackDirection _, AttackDirection value)
    {
        cachedAttackDir = value;
    }



    #endregion
    #region Command
    public void UpdateLocalAnimationState(bool moving, AimDirection aim, AttackDirection attack)
    {
        if (!NetworkClient.active) return;

        if (!isLocalPlayer) return;

        CmdUpdateAnimationState(moving, aim, attack);
    }

    [Command]
    public void CmdUpdateAnimationState(bool moving, AimDirection aimDir, AttackDirection atkDir)
    {
        isMoving = moving;

        aimDirection = aimDir;
        attackDirection = atkDir;
    }

    // ATTACK
    [Command]
    public void CmdPlayAttack(AimDirection aim, AttackDirection attackDir)
    {
        RpcPlayAttack(aim, attackDir);
    }

    [ClientRpc]
    private void RpcPlayAttack(AimDirection aim, AttackDirection attackDir)
    {
        // Local player already played it
        if (isLocalPlayer) return;

        animatePlayer.ApplyAttack(aim, attackDir);
    }

    // ROLL
    [Command]
    public void CmdPlayRoll(RollDirection dir)
    {
        RpcPlayRoll(dir);
    }

    [ClientRpc]
    private void RpcPlayRoll(RollDirection dir)
    {
        if (isLocalPlayer) return;
        animatePlayer.ApplyRoll(dir);
    }

    [Command]
    public void CmdEndRoll()
    {
        RpcEndRoll();
    }

    [ClientRpc]
    public void RpcEndRoll()
    {
        if (isLocalPlayer) return;
        animatePlayer.EndRoll();
    }

    // PARRY
    [Command]
    public void CmdPlayParry(AttackDirection attackDir)
    {
        RpcPlayParry(attackDir);
    }

    [ClientRpc]
    private void RpcPlayParry(AttackDirection attackDir)
    {
        if (isLocalPlayer) return;
        animatePlayer.ApplyParry(attackDir);
    }

    [Command]
    public void CmdEndParry(AimDirection aim, AttackDirection attackDir)
    {
        RpcEndParry(aim, attackDir);
    }

    [ClientRpc]
    private void RpcEndParry(AimDirection aim, AttackDirection attackDir)
    {
        if (isLocalPlayer) return;
        animatePlayer.EndParry(aim, attackDir);
    }
    #endregion
}
