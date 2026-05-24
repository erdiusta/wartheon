using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MoldranAINetwork : EnemyAINetwork, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSSES
    [SerializeField] Transform swordHoldingTransform;

    [Header("Combat")]
    [SerializeField] float preferredDistance = 6f;
    [SerializeField] float movementForce = 5f;
    [SerializeField] float retreatForce = 6f;
    [SerializeField] float strafeForce = 8f;

    [Header("Swing Attack")]
    [SerializeField] float smearCircleRadius = 2.2f;
    [SerializeField] int smearDamage = 25;
    [SerializeField] float smearChargeSpeed = 28f;
    [SerializeField] float smearChargeDuration = 0.65f;
    [SerializeField] float smearAttackDuration = 0.8f;

    [Header("Boss Behaviour")]
    [SerializeField] float waitPhase = 0.15f;

    [Header("Difficulty")]
    [SerializeField] float phase2Threshold = 0.7f;
    [SerializeField] float phase3Threshold = 0.4f;

    MoldranPhase currentMoldanPhase;
    MoldranPhase lastPhase;

    Coroutine moldranRoutine;

    bool smearDamageActive;

    // STATE
    private float phaseTimer;

    bool phase2Active;
    bool phase3Active;

    bool lockAttackVector;
    Vector2 lockedPosition;

    float aggressiveMultiplier = 1f;

    readonly HashSet<uint> alreadyHitTargets = new HashSet<uint>();

    // EVENTS
    public UnityEvent resetAnimationEvent;
    public UnityEvent projectileReleaseEvent;
    public UnityEvent smearStartEvent;
    public UnityEvent smearStopEvent;
    public UnityEvent spikeReleaseEvent;

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
    }

    protected override void Start()
    {
        base.Start();

        currentMoldanPhase = MoldranPhase.Wait;
    }

    protected override void OnEnable()
    {
        resetAnimationEvent.AddListener(ResetAnimations);
        projectileReleaseEvent.AddListener(OnProjectileReleased);
        smearStartEvent.AddListener(EnableSmearDamage);
        smearStopEvent.AddListener(DisableSmearDamage);
        spikeReleaseEvent.AddListener(OnSpikeReleased);
    }

    protected override void OnDisable()
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
        projectileReleaseEvent.RemoveListener(OnProjectileReleased);
        smearStartEvent.RemoveListener(EnableSmearDamage);
        smearStopEvent.RemoveListener(DisableSmearDamage);
        spikeReleaseEvent.RemoveListener(OnSpikeReleased);
    }

    public void ResetAnimationEvent() => resetAnimationEvent?.Invoke();
    public void ProjectileReleaseEvent() => projectileReleaseEvent?.Invoke();
    public void SmearStartEvent() => smearStartEvent?.Invoke();
    public void SmearStopEvent() => smearStopEvent?.Invoke();
    public void SpikeReleaseEvent() => spikeReleaseEvent?.Invoke();

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

            Debug.LogWarning("Moldran was outside bounds. Snapped back.");
        }

        UpdateBossDifficulty();

        HandleAim();

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            if (targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            switch (currentMoldanPhase)
            {
                case MoldranPhase.Wait:

                    PassedToWait = true;
                    phaseTimer += Time.fixedDeltaTime;

                    if (phaseTimer >= waitPhase)
                    {
                        phaseTimer = 0;
                        TransitionToNextPhase();
                    }
                    break;
                case MoldranPhase.Projectile:
                    if (moldranRoutine == null)
                    {
                        moldranRoutine = StartCoroutine(ProjectileRoutine());
                    }
                    break;
                case MoldranPhase.SwingAttack:
                    if (moldranRoutine == null)
                    {
                        moldranRoutine = StartCoroutine(SwingAttackRoutine());
                    }
                    break;
                case MoldranPhase.Spike:
                    if (moldranRoutine == null)
                    {
                        moldranRoutine = StartCoroutine(SpikeRoutine());
                    }
                    break;
                case MoldranPhase.Heal:
                    if (moldranRoutine == null)
                    {
                        moldranRoutine = StartCoroutine(HealRoutine());
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

        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Moldran);

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
        if (targetPlayer == null)
        {
            currentMoldanPhase = MoldranPhase.Wait;
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.GetPlayerPosition());

        if (lastPhase != MoldranPhase.Heal)
        {
            int roll = Random.Range(0, 100);

            if (enemy.health.GetCurrentHealth() / enemy.health.GetMaximumHealth() < 0.2f)
            {
                if (roll < 75) currentMoldanPhase = MoldranPhase.Heal;
                else goto skipHeal;
            }
            else if (enemy.health.GetCurrentHealth() / enemy.health.GetMaximumHealth() < 0.4f)
            {
                if (roll < 55) currentMoldanPhase = MoldranPhase.Heal;
                else goto skipHeal;
            }
            else
            {
                goto skipHeal;
            }
        }

    skipHeal:

        // CLOSE RANGE
        if (distance < 3f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 70) currentMoldanPhase = MoldranPhase.Projectile;
            else currentMoldanPhase = MoldranPhase.SwingAttack;

            return;
        }

        // LONG RANGE
        if (distance > 8f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 65) currentMoldanPhase = MoldranPhase.Spike;
            else currentMoldanPhase = MoldranPhase.Projectile;

            return;
        }

        // MID RANGE
        int midRoll = Random.Range(0, 100);

        if (midRoll < 45) currentMoldanPhase = MoldranPhase.SwingAttack;
        else if (midRoll < 75) currentMoldanPhase = MoldranPhase.Projectile;
        else currentMoldanPhase = MoldranPhase.Spike;
    }

    // PROJECTILE
    IEnumerator ProjectileRoutine()
    {
        BeginRoutine();

        lastPhase = MoldranPhase.Projectile;

        float timer = 0f;
        float duration = 2f;

        enemy.animateEnemy.SetSummonAnimation(true);
        enemy.enemyAnimSync?.SetBossSummonAnimation(true);

        while (timer < duration)
        {
            if (enemy.health.hasDied) yield break;

            timer += Time.fixedDeltaTime;

            if (!hasPendingProjectile)
            {
                hasPendingProjectile = true;

                Vector2 shotDirection = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

                pendingProjectileRequest = new PendingProjectileRequest
                {
                    projectileKind = ProjectileKind.Default,
                    attackContext = new AttackContext { moldranPhase = MoldranPhase.Projectile },
                    lockedAimVector = shotDirection
                };
            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetSummonAnimation(false);
        enemy.enemyAnimSync?.SetBossSummonAnimation(false);

        CleanupRoutine();
    }

    // SWING ATTACK
    IEnumerator SwingAttackRoutine()
    {
        BeginRoutine();

        lastPhase = MoldranPhase.SwingAttack;

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
        enemy.animateEnemy.SetCastAnimation(true);
        enemy.enemyAnimSync?.SetBossCastAnimation(true);

        float smearTimer = 0f;

        while (smearTimer < smearAttackDuration)
        {
            if (ShouldCancelRoutine()) yield break;

            smearTimer += Time.fixedDeltaTime;

            // DAMAGE WINDOW CONTROLLED BY ANIMATION EVENTS
            if (smearDamageActive)
            {
                PerformSmearDamage(smearCircleRadius);
            }

            yield return waitForFixedUpdate;
        }

        smearDamageActive = false;

        enemy.animateEnemy.SetCastAnimation(false);
        enemy.enemyAnimSync?.SetBossCastAnimation(false);

        isAttacking = false;

        CleanupRoutine();

        enemy.animateEnemy.SetIdleAnimationParameters();
        enemy.enemyAnimSync?.UpdateAnimationStateServer(moving: false, enemy.LastAim);
    }

    // SPIKE
    IEnumerator SpikeRoutine()
    {
        if (enemy.health.hasDied) yield break;

        lastPhase = MoldranPhase.Spike;

        float timer = 0f;
        float duration = 2.5f;

        uint lockedTargetNetId = targetPlayer.NetAuth.netId;

        enemy.animateEnemy.SetFocusedAnimation(true);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(true);

        while (timer < duration)
        {
            if (enemy.health.hasDied) yield break;

            timer += Time.fixedDeltaTime;

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetFocusedAnimation(false);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(false);

        moldranRoutine = null;

        currentMoldanPhase = MoldranPhase.Wait;
        phaseTimer = 0f;
    }

    // HEAL
    IEnumerator HealRoutine()
    {
        BeginRoutine();

        lastPhase = MoldranPhase.Heal;

        enemy.animateEnemy.SetHealAnimation(true);
        enemy.enemyAnimSync?.SetBossHealAnimation(true);

        IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(gameObject);
        healthAuthority.ApplyDamage(-30, default);

        //SoundEffectManager.Instance.PlaySoundEffect(enemyDetails.chargeSoundEffect);

        yield return new WaitForSeconds(1.5f);

        enemy.animateEnemy.SetHealAnimation(false);
        enemy.enemyAnimSync?.SetBossHealAnimation(false);

        currentMoldanPhase = MoldranPhase.Wait;
        phaseTimer = 0;

        moldranRoutine = null;

        enemy.animateEnemy.SetIdleAnimationParameters();
        enemy.enemyAnimSync?.UpdateAnimationStateServer(moving: false, enemy.LastAim);
    }

    void PerformSmearDamage(float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(swordHoldingTransform.position, radius);

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
        moldranRoutine = null;
        currentMoldanPhase = MoldranPhase.Wait;

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

    private void OnProjectileReleased()
    {
        if (!isServer) return;
        if (!hasPendingProjectile) return;

        attackLockedVector = pendingProjectileRequest.lockedAimVector;
        lockAttackVector = true;

        FireWeapon(false, pendingProjectileRequest.projectileKind, pendingProjectileRequest.attackContext);

        hasPendingProjectile = false;
    }

    private void OnSpikeReleased()
    {
        if (!isServer) return;

        FireWeapon(false, pendingProjectileRequest.projectileKind, new AttackContext { moldranPhase = MoldranPhase.Spike }, targetPlayer.NetAuth.netId);

        pendingProjectileRequest = default;
        hasPendingProjectile = false;
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
        currentMoldanPhase = MoldranPhase.Wait;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, smearCircleRadius);
    }
}
