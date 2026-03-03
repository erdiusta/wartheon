using Mirror;
using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetworkAuthority : NetworkBehaviour
{
    [HideInInspector] public Player player;

    private void Awake()
    {
        player = GetComponent<Player>();

    }
    public override void OnStartServer()
    {
        base.OnStartServer();
    }

    public override void OnStopServer()
    {
        base.OnStopServer();

        if (player != null) GameSessionManager.Instance.ServerPlayers.Remove(player);
    }

#region Client
    public override void OnStartClient()
    {
        base.OnStartClient();
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        if (SceneManager.GetActiveScene().buildIndex != 2) return;

        // Assign camera if not already assigned
        player.cameraManager.ShowGameplay();

        player.MarkLocalAuthorityReady(player);

        CmdRegisterPlayer();

        CmdSyncAnimSpeed(player.movementByForce.moveSpeed);
    }

    [Command]
    private void CmdRegisterPlayer()
    {
        if (player != null) GameSessionManager.Instance.CallPlayerRegisteredEvent(player);
    }

    [Command]
    public void CmdEnteredRoom(string roomId, Vector2 entryDirection)
    {
        DungeonNetworkController.Instance.ServerRoomEntered(roomId, netIdentity, entryDirection);
    }

    [Command]
    private void CmdSyncAnimSpeed(float moveSpeed)
    {
        player.animSync.SetAnimationSpeed(moveSpeed);
    }

    [Command]
    public void CmdRequestMeleeHit(AttackShape attackType, MeleeHand hand, bool isBloodDrain, bool shieldBash, bool isSheerCold, bool isDontBlink, bool isWhisperSlice, bool isThrowingAxe)
    {
        ServerMeleeCombat.ResolveMeleeHit(connectionToClient, netId, attackType, hand, isBloodDrain, shieldBash, isSheerCold, isDontBlink, isWhisperSlice, isThrowingAxe);
    }

    [Command]
    public void CmdRequestTogglePause()
    {
        GameSessionManager.Instance.ServerTogglePause();
    }

    [ClientRpc]
    public void RpcTeleportTo(Vector3 targetPos)
    {
        player.rb2D.linearVelocity = Vector3.zero;
        player.rb2D.angularVelocity = 0f;
        player.rb2D.position = targetPos;

        Physics2D.SyncTransforms();
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }
#endregion
}
