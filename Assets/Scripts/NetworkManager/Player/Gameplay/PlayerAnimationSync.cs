using Mirror;
using UnityEngine;
using System.Collections;
using Mirror.BouncyCastle.Asn1.Cmp;

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

    Player player;
    AnimatePlayer animatePlayer;
    AnimateSkillManager animateSkillManager;

    Coroutine teleportParticleRoutine;

    public override void OnStartClient()
    {
        base.OnStartClient();

        player = GetComponent<Player>();
        animatePlayer = GetComponent<AnimatePlayer>();
        animateSkillManager = GetComponentInChildren<AnimateSkillManager>();
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

    #region Generic Actions
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

    #region Caelion Skills
    // SEISMIC SLAM
    [Command]
    public void CmdPlaySeismicSlam()
    {
        RpcPlaySeismicSlam();
    }

    [ClientRpc]
    private void RpcPlaySeismicSlam()
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplySeismicSlam();
    }

    // VALOR
    [Command]
    public void CmdPlayValor(bool undo)
    {
        RpcPlayValor(undo);
    }

    [ClientRpc]
    private void RpcPlayValor(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyValor(undo);
    }

    // SHIELD BASH
    [Command]
    public void CmdPlayShieldBash(bool undo)
    {
        RpcPlayShieldBash(undo);
    }

    [ClientRpc]
    private void RpcPlayShieldBash(bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyShieldBash(undo);
    }

    // GUARDED OATH
    [Command]
    public void CmdPlayGuardedOath(bool undo)
    {
        RpcPlayGuardedOath(undo);
    }

    [ClientRpc]
    private void RpcPlayGuardedOath(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyGuardedOath(undo);
    }
    #endregion

    #region Morven Skills
    // UMBRAL MIST
    [Command]
    public void CmdPlayUmbralMist(float duration, int slotIndex, uint ownerNetId)
    {
        GameObject mist = Instantiate(GameResources.Instance.umbralMistNetworkPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(mist);

        var mistNet = mist.GetComponent<UmbralMistNetwork>();
        mistNet.Initialize(GetComponent<Player>(), duration, slotIndex, ownerNetId);
    }

    // BLOOD DRAIN
    [Command]
    public void CmdPlayBloodDrain(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        RpcPlayBloodDrain(aim, attackDir, undo);
    }

    [ClientRpc]
    private void RpcPlayBloodDrain(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyBloodDrain(aim, attackDir, undo);
    }

    // SHADOW STEP
    [Command]
    public void CmdPlayShadowStep(bool undo)
    {
        RpcPlayShadowStep(undo);
    }

    [ClientRpc]
    private void RpcPlayShadowStep(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyShadowStep(undo);
    }

    // CULL THE MEEK
    [Command]
    public void CmdPlayCullTheMeek(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        RpcPlayCullTheMeek(aim, attackDir, undo);
    }

    [ClientRpc]
    private void RpcPlayCullTheMeek(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyCullTheMeek(aim, attackDir, undo);
    }
    #endregion

    #region Nyveran Skills
    // PENETRATE
    [Command]
    public void CmdPlayPenetrate()
    {
        RpcPlayPenetrate();
    }

    [ClientRpc]
    private void RpcPlayPenetrate()
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyPenetrate();
    }

    // TRIPLE THREAT
    [Command]
    public void CmdPlayTripleThreat()
    {
        RpcPlayTripleThreat();
    }

    [ClientRpc]
    private void RpcPlayTripleThreat()
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyTripleThreat();
    }
    #endregion

    #region Mycara Skills
    // BLIZZARD
    [Command]
    public void CmdPlayBlizzard(float duration, int slotIndex, uint ownerNetId)
    {
        GameObject blizzard = Instantiate(GameResources.Instance.blizzardNetworkPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(blizzard);

        var blizzardNet = blizzard.GetComponent<BlizzardNetwork>();
        blizzardNet.Initialize(GetComponent<Player>(), duration, slotIndex, ownerNetId);
    }

    // MYCARA'S SEAL
    [Command]
    public void CmdPlayMycarasSeal(bool undo)
    {
        RpcPlayMycarasSeal(undo);
    }

    [ClientRpc]
    private void RpcPlayMycarasSeal(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyMycarasSeal(undo);
    }

    // SHEER COLD
    [Command]
    public void CmdPlaySheerCold(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        RpcPlaySheerCold(aim, attackDir, undo);
    }

    [ClientRpc]
    private void RpcPlaySheerCold(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplySheerCold(aim, attackDir, undo);
    }

    // ABSOLUTE ZERO
    [Command]
    public void CmdPlayAbsoluteZero(float duration, int slotIndex, uint ownerNetId)
    {
        GameObject absoluteZero = Instantiate(GameResources.Instance.absoluteZeroPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(absoluteZero);

        var absNetwork = absoluteZero.GetComponent<AbsoluteZeroNetwork>();
        absNetwork.Initialize(GetComponent<Player>(), duration, slotIndex, ownerNetId);
    }
    #endregion

    #region Kynara Skills
    // MOLTEN RIFT
    [Command]
    public void CmdMoltenRiftTeleport(Vector3 targetPosition)
    {
        // Validate position on server
        RoomNetData roomNetData = GameSessionManager.Instance.GetCurrentRoomNetData();

        Vector3Int cellPos = DungeonRuntime.GetInstantiatedRoom(roomNetData.roomId).grid.WorldToCell(targetPosition);

        if (!player.playerSkillController.IsObstacleTile(null, roomNetData, cellPos))
        {
            // Move on server
            player.rb2D.position = targetPosition;

            RpcOnMoltenRiftTeleport(targetPosition);
        }
    }

    [ClientRpc]
    void RpcOnMoltenRiftTeleport(Vector3 pos)
    {
        player.rb2D.position = pos;
    }

    [Command]
    public void CmdPlayMoltenRiftParticles()
    {
        RpcPlayMoltenRiftParticles();
    }

    [ClientRpc]
    private void RpcPlayMoltenRiftParticles()
    {
        if (teleportParticleRoutine != null)
        {
            StopCoroutine(teleportParticleRoutine);
        }

        teleportParticleRoutine = StartCoroutine(ParticleSystemRoutine());
    }

    IEnumerator ParticleSystemRoutine()
    {
        player.specialMoveParticlesSystem.Play();

        yield return new WaitForSeconds(0.3f);

        player.playerSkillController.particlePlayed = true;
    }

    [Command]
    public void CmdStopParticle()
    {
        RpcStopParticle();
    }

    [ClientRpc]
    void RpcStopParticle()
    {
        player.specialMoveParticlesSystem.Stop();

        player.playerSkillController.particlePlayed = false;
    }

    [Command]
    public void CmdInvincibility()
    {
        RpcInvincibility();
    }

    [ClientRpc]
    void RpcInvincibility()
    {
        StartCoroutine(InvincibilityRoutine());
    }

    IEnumerator InvincibilityRoutine()
    {
        player.health.healthAuthority.IsDamageable = false;

        // 4 is duration of invincibility
        int iterations = Mathf.RoundToInt(4 / Health.spriteFlashInterval / 2);

        // Flash effect
        while (iterations > 0)
        {
            player.health.flashManager.WhiteFlashCharacter(player.spriteRenderer);
            yield return new WaitForSeconds(Health.spriteFlashInterval);

            player.health.flashManager.UnflashCharacter(player.spriteRenderer);
            yield return new WaitForSeconds(Health.spriteFlashInterval);

            iterations--;

            yield return null;
        }

        player.isMoltenRiftActive = false;
        player.health.healthAuthority.IsDamageable = true;
    }

    // FLAME LOTUS
    [Command]
    public void CmdPlayFlameLotus(float duration, int slotIndex, uint ownerNetId)
    {
        GameObject flameLotus = Instantiate(GameResources.Instance.flameLotusPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(flameLotus);

        var flameLotusNet = flameLotus.GetComponent<FlameLotusNetwork>();
        flameLotusNet.Initialize(GetComponent<Player>(), duration, slotIndex, ownerNetId);
    }

    // KYNARA'S EMBRACE
    [Command]
    public void CmdPlayKynarasEmbrace(bool undo)
    {
        RpcPlayKynarasEmbrace(undo);
    }

    [ClientRpc]
    private void RpcPlayKynarasEmbrace(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyKynarasEmbrace(undo);
    }
    #endregion

    #region Nyxa Skills

    // DON'T BLINK
    [Command]
    public void CmdDontBlink(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        RpcDontBlink(aim, attackDir, undo);
    }

    [ClientRpc]
    private void RpcDontBlink(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyDontBlink(aim, attackDir, undo);
    }

    // WHISPER SLICE
    [Command]
    public void CmdWhisperSlice(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        RpcWhisperSlice(aim, attackDir, undo);
    }

    [ClientRpc]
    private void RpcWhisperSlice(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyWhisperSlice(aim, attackDir, undo);
    }

    // VENOMOUS IVY
    [Command]
    public void CmdVenomousIvy(bool undo)
    {
        RpcVenomousIvy(undo);
    }

    [ClientRpc]
    private void RpcVenomousIvy(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyVenomousIvy(undo);
    }

    // FADE AND FEED
    [Command]
    public void CmdFadeAndFeed(bool undo)
    {
        RpcFadeAndFeed(undo);
    }

    [ClientRpc]
    private void RpcFadeAndFeed(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyFadeAndFeed(undo);
    }

    // BLADE DASH
    [Command]
    public void CmdBladeDash(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        RpcBladeDash(aim, attackDir, undo);
    }

    [ClientRpc]
    private void RpcBladeDash(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ResetAnimatonParameters();
        animatePlayer.SetAimWeaponAnimationParameters(aim, attackDirection);

        animateSkillManager.ApplyBladeDash(undo);
    }
    #endregion

    #region Karnag Skills
    // RAGE
    [Command]
    public void CmdRage(AimDirection aim, AttackDirection attackDir)
    {
        RpcRage(aim, attackDir);
    }

    [ClientRpc]
    private void RpcRage(AimDirection aim, AttackDirection attackDir)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyRage(aim, attackDir);
    }

    // SHATTER CRY
    [Command]
    public void CmdShatterCry(AimDirection aim, AttackDirection attackDir)
    {
        RpcShatterCry(aim, attackDir);
    }

    [ClientRpc]
    private void RpcShatterCry(AimDirection aim, AttackDirection attackDir)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyShatterCry(aim, attackDir);
    }

    // WHIRLREND
    [Command]
    public void CmdWhirlrend(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        RpcWhirlrend(aim, attackDir, undo);
    }

    [ClientRpc]
    private void RpcWhirlrend(AimDirection aim, AttackDirection attackDir, bool undo)
    {
        if (isLocalPlayer) return;

        animatePlayer.ApplyWhirlrend(aim, attackDir, undo);
    }
    #endregion

    #region Nymara Skills

    // MIST OF DISRUPTION
    [Command]
    public void CmdMistOfDisruption(float duration, int slotIndex, uint ownerNetId)
    {
        GameObject mist = Instantiate(GameResources.Instance.mistOfDisruptionPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(mist);

        var mistNet = mist.GetComponent<MistOfDisruptionNetwork>();
        mistNet.Initialize(GetComponent<Player>(), duration, slotIndex, ownerNetId);
    }

    // NYMARA'S WINDVEIL
    [Command]
    public void CmdPlayNymarasWindveil(bool undo)
    {
        RpcPlayNymarasWindveil(undo);
    }

    [ClientRpc]
    private void RpcPlayNymarasWindveil(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyNymarasWindveil(undo);
    }

    // EYE OF THE STORM
    [Command]
    public void CmdPlayEyeOfTheStorm(float duration, int slotIndex, uint ownerNetId)
    {
        GameObject eyeOfTheStorm = Instantiate(GameResources.Instance.eyeOfTheStormPrefab, transform.position, Quaternion.identity);
        NetworkServer.Spawn(eyeOfTheStorm);

        var eyeOfTheStormNetwork = eyeOfTheStorm.GetComponent<EyeOfTheStormNetwork>();
        eyeOfTheStormNetwork.Initialize(GetComponent<Player>(), duration, slotIndex, ownerNetId);
    }

    // IONIC REJUVENATION
    [Command]
    public void CmdIonicRejuvenation(bool undo)
    {
        RpcIonicRejuvenation(undo);
    }

    [ClientRpc]
    private void RpcIonicRejuvenation(bool undo)
    {
        if (isLocalPlayer) return;

        animateSkillManager.ApplyIonicRejuvenation(undo);
    }

    #endregion
}
