using Mirror;
using UnityEngine;

public class PlayerAnimationSync : NetworkBehaviour
{
    [SerializeField] Animator animator;

    [SyncVar(hook = nameof(OnAnimSpeedChanged))]
    float animSpeed = 1f;

    [SyncVar(hook = nameof(OnIsMovingChanged))] 
    public bool isMoving;

    [SyncVar(hook = nameof(OnAimChanged))] 
    public AimDirection aimDirection;

    [SyncVar] 
    public AttackDirection attackDirection;

    AnimatePlayer animatePlayer;

    public override void OnStartClient()
    {
        base.OnStartClient();

        animatePlayer = GetComponent<AnimatePlayer>();
    }

    [Server]
    public void SetAnimationSpeed(float moveSpeed)
    {
        animSpeed = moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    #region Hooks
    private void OnAnimSpeedChanged(float oldValue, float newValue)
    {
        animator.speed = newValue;
    }

    private void OnIsMovingChanged(bool _, bool value)
    {
        if (isLocalPlayer) return;

        animatePlayer?.ApplyMovement(value);
    }

    private void OnAimChanged(AimDirection _, AimDirection value)
    {
        if (isLocalPlayer) return;

        animatePlayer?.ApplyAim(value);
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
