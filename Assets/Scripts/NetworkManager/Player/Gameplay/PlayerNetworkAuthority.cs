using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerNetworkAuthority : NetworkBehaviour
{
    [HideInInspector] public Player player;

    [SyncVar(hook = nameof(OnStealthChanged))]
    public bool isStealthActive;

    [SyncVar] public bool isDamageable;

    SpriteRenderer spriteRenderer;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        player = GetComponent<Player>();
    }

    private void OnStealthChanged(bool oldValue, bool newValue)
    {
        if(spriteRenderer != null)
        {
            var c = spriteRenderer.color;
            c.a = newValue ? 0.3f : 1f;
            spriteRenderer.color = c;
        }

        // Owner-only UI
        if (isLocalPlayer && player != null)
        {
            if (newValue) player.healthEvent.CallStealthSpecialMoveEvent();
            else player.healthEvent.CallStealthWoreOffEvent();
        }
    }

    [Server]
    public void ServerSetStealth(bool active)
    {
        isStealthActive = active;
        isDamageable = !active;
    }

    [Server]
    public void ServerFinishStealth(int slotIndex)
    {
        isStealthActive = false;

        TargetFinishStealth(connectionToClient, slotIndex);
    }

    [TargetRpc]
    void TargetFinishStealth(NetworkConnection target, int slotIndex)
    {
        var p = GetComponent<Player>();
        if (p == null) return;

        p.isStealthActive = false;

        p.specialMovesCooldownCheckArray[slotIndex - 1] = true;
        p.healthEvent.CallStealthWoreOffEvent();
    }

    [Command]
    public void CmdStartStealth(int slotIndex, float duration)
    {
        ServerSetStealth(true);

        // Start server-timed end
        StartCoroutine(ServerStealthLifetime(slotIndex, duration));
    }

    [Server]
    IEnumerator ServerStealthLifetime(int slotIndex, float duration)
    {
        yield return new WaitForSeconds(duration);

        ServerFinishStealth(slotIndex);
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
        player.cameraManager.ShowGameplay(onStart: true);

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

    [ClientRpc]
    public void RpcGambleCompleted(RoomNetData roomNetData)
    {
        roomNetData.shopRoomGoodsCreated = true;
    }

    [Command]
    public void CmdRequestOpenChest(NetworkIdentity chestNetId)
    {
        Chest chest = chestNetId.GetComponent<Chest>();

        if (chest != null)
        {
            chest.Server_StartChestProcess(netId);
        }
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

    [Command]
    public void CmdDeactivatePassiveItemAfterDrop(PassiveItemStats passiveStats, Rarity rarity, ItemSlotStatus slotStatus, bool isSwap, bool dropButton)
    {
        player.playerControl.DeactivatePassiveItem(passiveStats, rarity, slotStatus, isSwap, dropButton);
    }


    [Command]
    public void CmdDeactivateWeaponAfterDrop(int currentWeaponIndex, ItemSlotStatus slotStatus)
    {
        if (slotStatus == ItemSlotStatus.MainHand)
        {
            player.weaponState.mainHandWeaponRarity = Rarity.Basic;
            player.weaponState.mainHandWeaponTitle = WeaponTitle.None;

            DeactivateMainHandWeapon_Server(currentWeaponIndex);
        }
        else if (slotStatus == ItemSlotStatus.OffHand)
        {
            player.weaponState.offHandWeaponRarity = Rarity.Basic;
            player.weaponState.offhandWeaponTitle = WeaponTitle.None;

            DeactivateOffhandWeapon_Server(currentWeaponIndex);
        }

        RpcSyncWeaponVisuals(currentWeaponIndex, slotStatus);
    }

    [Server]
    public void DeactivateMainHandWeapon_Server(int index)
    {
        player.weaponState.mainHandWeaponTitle = WeaponTitle.None;
    }

    [Server]
    public void DeactivateOffhandWeapon_Server(int index)
    {
        player.weaponState.offhandWeaponTitle = WeaponTitle.None;
    }

    [ClientRpc]
    private void RpcSyncWeaponVisuals(int index, ItemSlotStatus slotStatus)
    {
        if (isServer) return; // Prevent double run on host

        if (slotStatus == ItemSlotStatus.MainHand)
        {
            player.weaponSlotSetArray[index - 1][0] = null;
            player.playerControl.DeactivateMainHandWeapon_Client(index);
        }
           
        else if (slotStatus == ItemSlotStatus.OffHand)
        {
            player.weaponSlotSetArray[index - 1][1] = null;
            player.playerControl.DeactivateOffhandWapon_Client(index);
        }
    }

    [Command]
    public void CmdValueAndBookUpdate(int index, ItemSlotStatus slotStatus)
    {
        RpcValueAndBookUpdate(index, slotStatus);
    }

    [TargetRpc]
    private void RpcValueAndBookUpdate(int currentWeaponIndex, ItemSlotStatus slotStatus)
    {
        player.UpdateDamageValues();
        player.UpdateArmorValues();
        player.UpdateAttackRatingAndCriticalValues();
        player.UpdateBlockAndDodgeValues();

        if (slotStatus == ItemSlotStatus.MainHand)
        {
            StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponMainHand);
        }
        else if (slotStatus == ItemSlotStatus.OffHand)
        {
            StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponOffHand);
        }
    }   


    [ClientRpc]
    public void RpcTeleportTo(Vector3 targetPos)
    {
        player.rb2D.linearVelocity = Vector3.zero;
        player.rb2D.angularVelocity = 0f;
        player.rb2D.position = targetPos;

        Physics2D.SyncTransforms();
    }

    [TargetRpc]
    public void TargetUmbralMistFinished(NetworkConnection target, int slotIndex, uint ownerNetId)
    {
        // Match owner Net ID; if player is not owner return
        if (GetComponent<Player>().NetAuth.netId != ownerNetId) return;

        player.isUmbralMistActive = false;
        player.healthEvent.CallUmbralMistWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }

    [TargetRpc]
    public void TargetBlizzardFinished(NetworkConnection target, int slotIndex, uint ownerNetId)
    {
        // Match owner Net ID; if player is not owner return
        if (GetComponent<Player>().NetAuth.netId != ownerNetId) return;

        player.isBlizzardActive = false;
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }

    [TargetRpc]
    public void TargetAbsoluteZeroFinished(NetworkConnection target, int slotIndex, uint ownerNetId)
    {
        // Match owner Net ID; if player is not owner return
        if (GetComponent<Player>().NetAuth.netId != ownerNetId) return;

        player.isAbsoluteZeroActive = false;
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }

    [TargetRpc]
    public void TargetFlameLotusFinished(NetworkConnection target, int slotIndex, uint ownerNetId)
    {
        // Match owner Net ID; if player is not owner return
        if (GetComponent<Player>().NetAuth.netId != ownerNetId) return;

        player.isFlameLotusActive = false;
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }


    [TargetRpc]
    public void TargetMistOfDisruptionFinished(NetworkConnection target, int slotIndex, uint ownerNetId)
    {
        // Match owner Net ID; if player is not owner return
        if (GetComponent<Player>().NetAuth.netId != ownerNetId) return;

        player.isMistOfDisruptionActive = false;
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }

    [TargetRpc]
    public void TargetEyeOfTheStormFinished(NetworkConnection target, int slotIndex, uint ownerNetId)
    {
        // Match owner Net ID; if player is not owner return
        if (GetComponent<Player>().NetAuth.netId != ownerNetId) return;

        player.isEyeOfTheStormActive = false;
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();
    }

    #endregion

    #region STATUS EFFECTS
    // BLEEDING
    [Command]
    public void CmdApplyBleeding(NetworkIdentity enemyNetId, uint projectileOwnerNetId, int projectileIndex, bool arrowOfTheSeven, bool isThrowingAxe)
    {
        if (!enemyNetId) return;

        Enemy enemy = enemyNetId.GetComponent<Enemy>();
        if (enemy == null) return;

        if (!NetworkServer.spawned.TryGetValue(projectileOwnerNetId, out var identity)) return;

        Player owner = identity.GetComponent<Player>();
        if (owner == null) return;

        var projectileDetails = WartheonDatabase.Instance.GetProjectile(projectileIndex);
        if (projectileDetails == null) return;

        ApplyBleedingServer(enemy, owner, projectileDetails, arrowOfTheSeven, isThrowingAxe);
    }

    [Server]
    private void ApplyBleedingServer(Enemy enemy, Player owner, ProjectileDetailsSO projectileDetails, bool arrowOfTheSeven, bool isThrowingAxe)
    {
        if (enemy.enemyDetails.isImmuneToBleeding) return;

        if (arrowOfTheSeven)
        {
            if (player != null)
            {
                enemy.bleedDuration = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 6,
                    2 => 7,
                    3 => 8,
                    _ => 2 // Default
                };
            }
        }

        float bleedingChance = 0f;

        if (isThrowingAxe)
        {
            if (player != null)
            {
                bleedingChance = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 0.2f,
                    2 => 0.3f,
                    3 => 0.45f,
                    _ => 0f // Default
                };
            }
        }

        // Check get bleeding
        float randomDice = Random.Range(0f, 1f);

        if (randomDice < projectileDetails.bleedingChance + bleedingChance + player.additionalStatusEffectInflictModifier + player.additionalBleedChance
            || arrowOfTheSeven)
        {
            enemy.statusEffectAnimators.bleedAnimator.SetTrigger(Settings.activateVFX);
            enemy.healthEvent.CallGetBleedingEvent();
            enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
        }
    }

    // STUN
    [Command]
    public void CmdApplyStun(NetworkIdentity enemyNetId, uint projectileOwnerNetId, int projectileIndex)
    {
        if (!enemyNetId) return;

        Enemy enemy = enemyNetId.GetComponent<Enemy>();
        if (enemy == null) return;

        if (!NetworkServer.spawned.TryGetValue(projectileOwnerNetId, out var identity)) return;

        Player owner = identity.GetComponent<Player>();
        if (owner == null) return;

        var projectileDetails = WartheonDatabase.Instance.GetProjectile(projectileIndex);
        if (projectileDetails == null) return;

        ApplyStunServer(enemy, owner, projectileDetails);
    }

    [Server]
    private void ApplyStunServer(Enemy enemy, Player owner, ProjectileDetailsSO projectileDetails)
    {
        if (enemy.enemyDetails.isImmuneToStun) return;

        bool isStunned = (enemy.moveStatus & MoveStatus.Stun) != 0;

        if (!isStunned)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.stunChance + player.additionalStatusEffectInflictModifier + player.additionalStunChance)
            {
                enemy.moveStatus |= MoveStatus.Stun;
            }
        }
    }

    // ROOT
    [Command]
    public void CmdApplyRoot(NetworkIdentity enemyNetId, uint projectileOwnerNetId, int projectileIndex, bool isBindingArrow)
    {
        if (!enemyNetId) return;

        Enemy enemy = enemyNetId.GetComponent<Enemy>();
        if (enemy == null) return;

        if (!NetworkServer.spawned.TryGetValue(projectileOwnerNetId, out var identity)) return;

        Player owner = identity.GetComponent<Player>();
        if (owner == null) return;

        var projectileDetails = WartheonDatabase.Instance.GetProjectile(projectileIndex);
        if (projectileDetails == null) return;

        ApplyRootServer(enemy, owner, projectileDetails, isBindingArrow);
    }

    [Server]
    private void ApplyRootServer(Enemy enemy, Player owner, ProjectileDetailsSO projectileDetails, bool isBindingArrow)
    {
        if (enemy.enemyDetails.isImmuneToRoot) return;

        bool isRooted = (enemy.moveStatus & MoveStatus.Root) != 0;

        if ((!isRooted && enemy.health.currentHealth > 0) || isBindingArrow)
        {
            if (isBindingArrow)
            {
                if (player != null)
                {
                    enemy.rootDuration = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 3,
                        2 => 4,
                        3 => 5,
                        _ => 2 // Default
                    };
                }
            }

            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.rootChance + player.additionalStatusEffectInflictModifier + player.additionalRootChance || isBindingArrow)
            {
                enemy.moveStatus |= MoveStatus.Root;
            }
        }
    }

    #endregion
}
