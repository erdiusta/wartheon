using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class SepharothAINetwork : EnemyAINetwork, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSS
    [Header("References")]
    [SerializeField] Transform swordHoldingTransform;

    [Header("Combat")]
    [SerializeField] float preferredDistance = 6f;
    [SerializeField] float movementForce = 5f;
    [SerializeField] float retreatForce = 6f;
    [SerializeField] float strafeForce = 8f;

    [Header("Laser")]
    [SerializeField] float laserPrepareDuration = 1f;

    [Header("Smear")]
    [SerializeField] float smearCircleRadius = 2f;
    [SerializeField] int smearDamage = 25;
    [SerializeField] float smearChargeSpeed = 28f;
    [SerializeField] float smearChargeDuration = 0.65f;
    [SerializeField] float smearAttackDuration = 0.8f;

    [Header("Invisible")]
    [SerializeField] float invisibleFadeDuration = 0.6f;
    [SerializeField] float invisibleMineDuration = 4f;

    [Header("Boss Behaviour")]
    [SerializeField] float waitPhase = 0.15f;

    [Header("Difficulty")]
    [SerializeField] float phase2Threshold = 0.7f;
    [SerializeField] float phase3Threshold = 0.4f;

    [SyncVar(hook = nameof(OnAlphaChanged))] float syncedAlpha = 1f;

    SepharothPhase currentSepharothPhase;

    Coroutine sepharothRoutine;

    bool smearDamageActive;

    // STATE
    private float phaseTimer;

    bool phase2Active;
    bool phase3Active;

    bool lockAttackVector;
    Vector2 lockedPosition;

    float aggressiveMultiplier = 1f;

    bool isInvisible;

    SpriteRenderer spriteRenderer;
    CapsuleCollider2D movementCollider;
    PolygonCollider2D hitCollider;

    readonly HashSet<uint> alreadyHitTargets = new HashSet<uint>();

    // EVENTS
    public UnityEvent resetAnimationEvent;
    public UnityEvent smearStartEvent;
    public UnityEvent smearStopEvent;

    bool passedToWait;

    public bool PassedToWait
    {
        get => passedToWait;
        set
        {
            if (!passedToWait && value)
            {
                passedToWait = true;
                HandleWaitPhase();
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();

        movementCollider = GetComponent<CapsuleCollider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        hitCollider = GetComponent<PolygonCollider2D>();
    }

    protected override void Start()
    {
        base.Start();

        currentSepharothPhase = SepharothPhase.Wait;
    }

    protected override void OnEnable()
    {
        resetAnimationEvent.AddListener(ResetAnimations);
        smearStartEvent.AddListener(EnableSmearDamage);
        smearStopEvent.AddListener(DisableSmearDamage);
    }

    protected override void OnDisable()
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
        smearStartEvent.RemoveListener(EnableSmearDamage);
        smearStopEvent.RemoveListener(DisableSmearDamage);
    }

    void OnAlphaChanged(float oldAlpha, float newAlpha)
    {
        SetAlpha(newAlpha);
    }

    public void ResetAnimationEvent() => resetAnimationEvent?.Invoke();
    public void SmearStartEvent() => smearStartEvent?.Invoke();
    public void SmearStopEvent() => smearStopEvent?.Invoke();

    protected override void Update()
    {
        if (!isServer || !enemyFullyInitialized) return;
        if (currentRoomNetData == default) currentRoomNetData = enemy.owningSpawner.instantiatedRoom.roomNetData;

        if (enemyPhase == EnemyPhase.Death) return;

        RefreshTargetAndLockedVector();

        if (targetPlayer != null)
        {
            trackingVector = (targetPlayer.GetPlayerPosition() - transform.position).normalized;
        }
    }

    protected override void FixedUpdate()
    {
        if (!isServer || !enemyFullyInitialized) return;
        if (targetPlayer == null) return;
        if (enemyPhase == EnemyPhase.Death) return;

        // Emergency pullback if Sylvarok drifts outside bounds
        if (IsOutsideBossRoom(transform.position, cellMin, cellMax, isMultiplayer: true))
        {
            Vector3 safePos = ClampToBossRoom(transform.position, cellMin, cellMax);
            rb2D.position = safePos;
            rb2D.linearVelocity = Vector2.zero;

            Debug.LogWarning("Sepharoth was outside bounds. Snapped back.");
        }

        UpdateBossDifficulty();

        HandleAim();

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            if (targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            switch (currentSepharothPhase)
            {
                case SepharothPhase.Wait:
                    enemy.debugDisplay.text = "WAIT";

                    PassedToWait = true;
                    phaseTimer += Time.fixedDeltaTime;
                    MaintainDistance();

                    if (phaseTimer >= waitPhase)
                    {
                        phaseTimer = 0;
                        TransitionToNextPhase();
                    }
                    break;
                case SepharothPhase.LaserBeam:
                    if (sepharothRoutine == null)
                    {
                        sepharothRoutine = StartCoroutine(LaserBeamRoutine());
                    }
                    break;
                case SepharothPhase.SmearAttack:
                    if (sepharothRoutine == null)
                    {
                        sepharothRoutine = StartCoroutine(SmearAttackRoutine());
                    }
                    break;
                case SepharothPhase.InvisibleAndMine:
                    if (sepharothRoutine == null)
                    {
                        sepharothRoutine = StartCoroutine(InsivibleAndMineRoutine());
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void HandleAim()
    {
        Vector2 activeAimVector = lockAttackVector ? attackLockedVector : trackingVector;
        float unitAngle = HelperUtilities.GetAngleFromVector(activeAimVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);

        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Sepharoth);

        if (unitAimDirection != enemy.LastAim)
        {
            enemy.LastAim = unitAimDirection;

            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimParameters(unitAimDirection);

            enemy.enemyAnimSync?.ResetAimAnimations();
            enemy.enemyAnimSync?.UpdateAnimationStateServer(wasMoving, unitAimDirection);
        }
    }

    public void HandleWaitPhase()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();

        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.ResetAllBossAnimations();
    }

    private void TransitionToNextPhase()
    {
        if (targetPlayer == null) return;

        // Check if the player is on stealth
        if (targetPlayer.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.transform.position);

        // CLOSE RANGE
        if (distance < 4f)
        {
            int randomNum = Random.Range(0, 100);

            if (randomNum < 70) currentSepharothPhase = SepharothPhase.SmearAttack;
            else currentSepharothPhase = SepharothPhase.LaserBeam;

            return;
        }

        // MID & LONG RANGE
        int midLongRangeRandom = Random.Range(0, 100);

        if (midLongRangeRandom < 35) currentSepharothPhase = SepharothPhase.LaserBeam;
        else if (midLongRangeRandom < 70) currentSepharothPhase = SepharothPhase.SmearAttack;
        else currentSepharothPhase = SepharothPhase.InvisibleAndMine;
    }

    // LASER BEAM
    IEnumerator LaserBeamRoutine()
    {
        BeginRoutine();

        enemy.debugDisplay.text = "LASER BEAM";

        // Set the motion type for the precharge phase
        enemy.animateEnemy.SetCastAnimation(true);
        enemy.enemyAnimSync?.SetBossCastAnimation(true);

        float timer = 0f;

        //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.chargeSoundEffect);

        while (timer < laserPrepareDuration)
        {
            if (ShouldCancelRoutine()) yield break;

            timer += Time.fixedDeltaTime;
            rb2D.linearVelocity = Vector2.zero;

            yield return waitForFixedUpdate;
        }

        FireWeapon(true, ProjectileKind.Default, new AttackContext { sepharothPhase = SepharothPhase.LaserBeam });

        float fireDuration = enemy.enemyDetails.enemyWeapon.weaponCooldownDuration;
        timer = 0f;

        while (timer < fireDuration)
        {
            if (ShouldCancelRoutine()) yield break;

            timer += Time.fixedDeltaTime;
            rb2D.linearVelocity = Vector2.zero;

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetCastAnimation(false);
        enemy.enemyAnimSync?.SetBossCastAnimation(false);

        CleanupRoutine();
    }

    // SMEAR ATTACK
    IEnumerator SmearAttackRoutine()
    {
        BeginRoutine();

        enemy.debugDisplay.text = "SMEAR ATTACK";

        isAttacking = true;


        // LOCK TARGET POSITION ONCE
        if (targetPlayer != null)
        {
            lockedPosition = targetPlayer.GetPlayerPosition();
            lockAttackVector = true;
        }

        // PRE-CHECK
        bool closeEnough = Vector2.Distance(rb2D.position, lockedPosition) < 1.2f;

        // PREPARE CHARGE
        if (!closeEnough)
        {
            float prepareTimer = 0f;
            float prepareDuration = 0.35f;

            while (prepareTimer < prepareDuration)
            {
                if (ShouldCancelRoutine()) yield break;

                prepareTimer += Time.fixedDeltaTime;
                rb2D.linearVelocity = Vector2.zero;

                yield return waitForFixedUpdate;
            }

            // START CHARGE
            Vector2 chargeDirection = (lockedPosition - rb2D.position).normalized;

            float chargeTimer = 0f;

            while (chargeTimer < smearChargeDuration)
            {
                if (ShouldCancelRoutine()) yield break;

                chargeTimer += Time.fixedDeltaTime;

                float chargeSpeed = smearChargeSpeed * aggressiveMultiplier;
                rb2D.linearVelocity = chargeDirection * chargeSpeed;

                // EARLY STOP IF CLOSE ENOUGH
                if (Vector2.Distance(rb2D.position, lockedPosition) < 1.2f)
                {
                    break;
                }

                yield return waitForFixedUpdate;
            }

            rb2D.linearVelocity = Vector2.zero; 
        }

        // SMALL BUFFER BEFORE SMEAR
        yield return waitForFixedUpdate;

        alreadyHitTargets.Clear();

        smearDamageActive = false;

        // SMEAR ATTACK START
        enemy.animateEnemy.SetFocusedAnimation(true);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(true);

        float smearTimer = 0f;

        while (smearTimer < smearAttackDuration)
        {
            if (ShouldCancelRoutine()) yield break;

            smearTimer += Time.fixedDeltaTime;

            // DAMAGE WINDOW CONTROLLED BY ANIMATION EVENTS
            if (smearDamageActive)
            {
                PerformSmearDamage();
            }

            yield return waitForFixedUpdate;
        }

        smearDamageActive = false;

        enemy.animateEnemy.SetFocusedAnimation(false);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(false);

        isAttacking = false;

        CleanupRoutine();

        enemy.animateEnemy.SetIdleAnimationParameters();
        enemy.enemyAnimSync?.UpdateAnimationStateServer(moving: false, enemy.LastAim);
    }

    // INVISIBLE AND MINE
    IEnumerator InsivibleAndMineRoutine()
    {
        BeginRoutine();

        enemy.debugDisplay.text = "INVISIBLE AND MINE";

        rb2D.linearVelocity = Vector2.zero;

        // FADE OUT
        yield return FadeRoutine(1f, 0f);

        isInvisible = true;

        hitCollider.enabled = false;

        // SMALL INVISIBLE DELAY
        yield return new WaitForSeconds(0.4f);

        int mineCount = Random.Range(3, 5);

        for (int i = 0; i < mineCount; i++)
        {
            if (ShouldCancelRoutine()) yield break;

            // TELEPORT TO RANDOM POSITION
            TeleportToRandomSpawn();

            yield return new WaitForSeconds(0.25f);

            // PLACE MINE
            if (!hasPendingProjectile)
            {
                pendingProjectileRequest = new PendingProjectileRequest
                {
                    projectileKind = ProjectileKind.Default,
                    attackContext = new AttackContext { sepharothPhase = SepharothPhase.InvisibleAndMine }
                };

                hasPendingProjectile = true;

                FireWeapon(false, pendingProjectileRequest.projectileKind, pendingProjectileRequest.attackContext);

                hasPendingProjectile = false;

                // WAIT BETWEEN MINES
                yield return new WaitForSeconds(1f);
            }
        }

        // FINAL INVISIBLE DELAY
        yield return new WaitForSeconds(0.5f);

        TeleportNearPlayer();

        hitCollider.enabled = true;
        isInvisible = false;

        yield return FadeRoutine(0f, 1f);

        CleanupRoutine();
    }

    void TeleportNearPlayer()
    {
        if (targetPlayer == null) return;

        Vector2 offset = Random.insideUnitCircle.normalized * 3f;

        Vector2 targetPos = (Vector2)targetPlayer.transform.position + offset;
        targetPos = ClampToBossRoom(targetPos, cellMin, cellMax);

        rb2D.position = targetPos;
    }

    IEnumerator FadeRoutine(float startAlpha, float endAlpha)
    {
        float timer = 0f;

        while (timer < invisibleFadeDuration)
        {
            timer += Time.fixedDeltaTime;
            float t = timer / invisibleFadeDuration;

            float alpha = Mathf.Lerp(startAlpha, endAlpha, t);

            syncedAlpha = alpha;

            yield return waitForFixedUpdate;
        }

        syncedAlpha = endAlpha;
    }

    void SetAlpha(float alpha)
    {
        Color color = spriteRenderer.color;
        color.a = alpha;
        spriteRenderer.color = color;
    }

    void PerformSmearDamage()
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(swordHoldingTransform.position, smearCircleRadius);

        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag(Settings.playerTag)) continue;

            Player player = collider.GetComponent<Player>();
            if (player == null) continue;

            if (alreadyHitTargets.Contains(player.NetAuth.netId)) continue;

            alreadyHitTargets.Add(player.NetAuth.netId);

            ReceiveProjectileDamage damageReceiver = collider.GetComponent<ReceiveProjectileDamage>();

            if (damageReceiver == null) continue;

            DamageContext ctx = new()
            {
                dealerPosition = transform.position,
                receiverPosition = player.transform.position
            };

            damageReceiver.TakeProjectileDamage(smearDamage, ctx);
            ApplyKnockbackToPlayer(player);
        }
    }

    void BeginRoutine()
    {
        ResetAnimations();

        rb2D.linearVelocity = Vector2.zero;

        enemy.isFiring = false;
    }

    void CleanupRoutine()
    {
        ResetAnimations();

        rb2D.linearVelocity = Vector2.zero;

        enemy.isFiring = false;

        lockAttackVector = false;
        hasPendingProjectile = false;
        sepharothRoutine = null;
        currentSepharothPhase = SepharothPhase.Wait;

        passedToWait = false;
        phaseTimer = 0f;
    }

    bool ShouldCancelRoutine()
    {
        if (!isServer)
        {
            CleanupRoutine();
            return true;
        }

        if (enemy.health.hasDied)
        {
            CleanupRoutine();
            return true;
        }

        if (targetPlayer == null)
        {
            CleanupRoutine();
            return true;
        }

        return false;
    }

    // ANIMATION EVENTS
    public void EnableSmearDamage()
    {
        if (!isServer) return;

        alreadyHitTargets.Clear();
        smearDamageActive = true;
    }

    public void DisableSmearDamage()
    {
        if (!isServer) return;

        smearDamageActive = false;
    }

    void UpdateBossDifficulty()
    {
        float hpPercent = (float)enemy.health.GetCurrentHealth() / enemy.health.GetMaximumHealth();

        if (!phase2Active && hpPercent <= phase2Threshold)
        {
            phase2Active = true;
            aggressiveMultiplier = 1.25f;
            waitPhase = 0.8f;
        }

        if (!phase3Active && hpPercent <= phase3Threshold)
        {
            phase3Active = true;
            aggressiveMultiplier = 1.5f;
            waitPhase = 0.6f;
        }
    }

    void MaintainDistance()
    {
        if (targetPlayer == null) return;

        float distance = Vector2.Distance(transform.position, targetPlayer.GetPlayerPosition());
        Vector2 dir = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

        if (distance < preferredDistance - 1f)
        {
            rb2D.AddForce(-dir * retreatForce);
        }
        else if (distance > preferredDistance + 2f)
        {
            rb2D.AddForce(dir * movementForce);
        }
    }

    void TeleportToRandomSpawn()
    {
        InstantiatedRoom room = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
        Grid grid = room.grid;

        int randomIndex = Random.Range(0, currentRoomNetData.spawnPositions.Length);

        Vector3Int spawnCell = new Vector3Int(currentRoomNetData.spawnPositions[randomIndex].x, currentRoomNetData.spawnPositions[randomIndex].y, 0);

        rb2D.position = grid.GetCellCenterWorld(spawnCell);
    }

    private void ResetAnimations()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();

        enemy.enemyAnimSync?.ResetAllBossAnimations();
        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.ResetAllBossAnimations();
    }

    private void RefreshTargetAndLockedVector()
    {
        targetRefreshTimer += Time.deltaTime;

        if (targetRefreshTimer >= Settings.targetRefreshInterval)
        {
            targetPlayer = HelperUtilities.GetClosestPlayer(transform.position);
            targetRefreshTimer = 0f;

            if (targetPlayer != null)
            {
                attackLockedVector = (targetPlayer.GetPlayerPosition() - transform.position).normalized;
            }
        }
    }

    public void PlayerStealthCheck()
    {
        currentSepharothPhase = SepharothPhase.Wait;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, smearCircleRadius);
    }
}
