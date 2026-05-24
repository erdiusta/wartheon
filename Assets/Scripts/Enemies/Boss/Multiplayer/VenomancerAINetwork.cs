using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class VenomancerAINetwork : EnemyAINetwork, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-4, 6);
    readonly Vector2Int cellMax = new Vector2Int(8, 14);

    // BOSSES
    [SerializeField] Transform swordHoldingTransform;

    [Header("Seismic Slam")]
    [SerializeField] float seismicSlamCircleRadius = 2.2f;
    [SerializeField] int seismicSlamDamage = 25;
    [SerializeField] float seismicSlamChargeSpeed = 28f;
    [SerializeField] float seismicSlamChargeDuration = 0.65f;
    [SerializeField] float seismicSlamAttackDuration = 0.8f;

    [Header("Boss Behaviour")]
    [SerializeField] float waitPhase = 0.15f;

    [Header("Difficulty")]
    [SerializeField] float phase2Threshold = 0.7f;
    [SerializeField] float phase3Threshold = 0.4f;

    VenomancerPhase currentVenomancerPhase;

    Coroutine venomancerPhase;

    bool smearDamageActive;

    ParticleSystem specialMoveParticlesSystem;
    float shakeIntensity = 3f;
    float shakeDuration = 1.2f;

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
    public UnityEvent slamStartEvent;
    public UnityEvent sludgeReleaseEvent;
    public UnityEvent stoneRainReleaseEvent;
    public UnityEvent toxicPoolReleaseEvent;

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

        currentVenomancerPhase = VenomancerPhase.Wait;
    }

    protected override void OnEnable()
    {
        resetAnimationEvent.AddListener(ResetAnimations);
        slamStartEvent.AddListener(EnableSlamDamage);
        sludgeReleaseEvent.AddListener(OnSludgeReleased);
        stoneRainReleaseEvent.AddListener(OnStoneRainReleased);
        toxicPoolReleaseEvent.AddListener(OnToxicPoolReleased);
    }

    protected override void OnDisable()
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
        slamStartEvent.RemoveListener(EnableSlamDamage);
        sludgeReleaseEvent.RemoveListener(OnSludgeReleased);
        stoneRainReleaseEvent.RemoveListener(OnStoneRainReleased);
        toxicPoolReleaseEvent.RemoveListener(OnToxicPoolReleased);
    }

    public void ResetAnimationEvent() => resetAnimationEvent?.Invoke();
    public void SlamStartEvent() => slamStartEvent?.Invoke();
    public void SludgeReleaseEvent() => sludgeReleaseEvent?.Invoke();
    public void StoneRainReleaseEvent() => stoneRainReleaseEvent?.Invoke();
    public void ToxicPoolReleaseEvent() => toxicPoolReleaseEvent?.Invoke();

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

            Debug.LogWarning("Venomancer was outside bounds. Snapped back.");
        }

        UpdateBossDifficulty();

        HandleAim();

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            if (targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            switch (currentVenomancerPhase)
            {
                case VenomancerPhase.Wait:

                    PassedToWait = true;
                    phaseTimer += Time.fixedDeltaTime;

                    if (phaseTimer >= waitPhase)
                    {
                        phaseTimer = 0;
                        TransitionToNextPhase();
                    }
                    break;
                case VenomancerPhase.SludgeThrow:
                    if (venomancerPhase == null)
                    {
                        venomancerPhase = StartCoroutine(SludgeThrowRoutine());
                    }
                    break;
                case VenomancerPhase.SlamGround:
                    if (venomancerPhase == null)
                    {
                        venomancerPhase = StartCoroutine(SlamGroundRoutine());
                    }
                    break;
                case VenomancerPhase.StoneRain:
                    if (venomancerPhase == null)
                    {
                        venomancerPhase = StartCoroutine(StoneRainRoutine());
                    }
                    break;
                case VenomancerPhase.ToxicPool:
                    if (venomancerPhase == null)
                    {
                        venomancerPhase = StartCoroutine(ToxicPoolRoutine());
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

        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Cryothar);

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
            currentVenomancerPhase = VenomancerPhase.Wait;
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.GetPlayerPosition());

        // CLOSE RANGE
        if (distance < 5f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 70) currentVenomancerPhase = VenomancerPhase.SlamGround;
            else currentVenomancerPhase = VenomancerPhase.SludgeThrow;

            return;
        }

        // LONG RANGE
        if (distance > 10f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 35) currentVenomancerPhase = VenomancerPhase.StoneRain;
            else if (roll < 45) currentVenomancerPhase = VenomancerPhase.ToxicPool;
            else currentVenomancerPhase = VenomancerPhase.SludgeThrow;

            return;
        }

        // MID RANGE
        int midRoll = Random.Range(0, 100);

        if (midRoll < 75) currentVenomancerPhase = VenomancerPhase.SludgeThrow;
        else if (midRoll < 85) currentVenomancerPhase = VenomancerPhase.StoneRain;
        else currentVenomancerPhase = VenomancerPhase.ToxicPool;
    }

    // SLUDGE THROW
    IEnumerator SludgeThrowRoutine()
    {
        BeginRoutine();

        enemy.debugDisplay.text = "SLUDGE THROW";

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
                Vector2 shotDirection = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

                pendingProjectileRequest = new PendingProjectileRequest
                {
                    projectileKind = ProjectileKind.Default,
                    attackContext = new AttackContext { venomancerPhase = VenomancerPhase.SludgeThrow },
                    lockedAimVector = shotDirection
                };

                hasPendingProjectile = true;

            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetSummonAnimation(false);
        enemy.enemyAnimSync?.SetBossSummonAnimation(false);

        venomancerPhase = null;

        currentVenomancerPhase = VenomancerPhase.Wait;
        phaseTimer = 0;
    }

    // SLAM GROUND
    IEnumerator SlamGroundRoutine()
    {
        BeginRoutine();

        enemy.debugDisplay.text = "SLAM GROUND";

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
            float prepareDuration = 1f;

            while (prepareTimer < prepareDuration)
            {
                if (ShouldCancelRoutine()) yield break;

                prepareTimer += Time.fixedDeltaTime;
                rb2D.linearVelocity *= 0.25f;

                yield return waitForFixedUpdate;
            }

            // START CHARGE
            Vector2 chargeDirection = (lockedPosition - rb2D.position).normalized;

            float chargeTimer = 0f;

            while (chargeTimer < seismicSlamChargeDuration)
            {
                if (ShouldCancelRoutine()) yield break;

                chargeTimer += Time.fixedDeltaTime;

                float chargeSpeed = seismicSlamChargeSpeed * aggressiveMultiplier;
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
        enemy.animateEnemy.SetHealAnimation(true);
        enemy.enemyAnimSync?.SetBossHealAnimation(true);

        float smearTimer = 0f;

        while (smearTimer < seismicSlamAttackDuration)
        {
            if (ShouldCancelRoutine()) yield break;

            smearTimer += Time.fixedDeltaTime;

            // DAMAGE WINDOW CONTROLLED BY ANIMATION EVENTS
            if (smearDamageActive)
            {
                SeismicSlam();
            }

            yield return waitForFixedUpdate;
        }

        yield return new WaitForSeconds(0.4f);

        smearDamageActive = false;

        enemy.animateEnemy.SetHealAnimation(false);
        enemy.enemyAnimSync?.SetBossHealAnimation(false);

        isAttacking = false;

        CleanupRoutine();

        enemy.animateEnemy.SetIdleAnimationParameters();
        enemy.enemyAnimSync?.UpdateAnimationStateServer(moving: false, enemy.LastAim);
    }

    // TOXIC POOL
    IEnumerator ToxicPoolRoutine()
    {
        if (enemy.health.hasDied) yield break;

        BeginRoutine();

        enemy.debugDisplay.text = "TOXIC POOL";

        float timer = 0f;
        float duration = 2f;

        enemy.animateEnemy.SetCastAnimation(true);
        enemy.enemyAnimSync?.SetBossCastAnimation(true);

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
                    attackContext = new AttackContext { venomancerPhase = VenomancerPhase.ToxicPool },
                    lockedAimVector = shotDirection
                };
            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetCastAnimation(false);
        enemy.enemyAnimSync?.SetBossCastAnimation(false);

        CleanupRoutine();
    }

    // STONE RAIN
    IEnumerator StoneRainRoutine()
    {
        if (enemy.health.hasDied) yield break;

        BeginRoutine();

        enemy.debugDisplay.text = "STONE RAIN";

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

        CleanupRoutine();
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
        venomancerPhase = null;
        currentVenomancerPhase = VenomancerPhase.Wait;

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
    public void EnableSlamDamage()
    {
        if (!isServer) return;

        alreadyHitTargets.Clear();
        smearDamageActive = true;
    }

    private void OnSludgeReleased()
    {
        if (!isServer) return;
        if (!hasPendingProjectile) return;

        attackLockedVector = pendingProjectileRequest.lockedAimVector;
        lockAttackVector = true;

        FireWeapon(false, pendingProjectileRequest.projectileKind, pendingProjectileRequest.attackContext);

        pendingProjectileRequest = default;
        hasPendingProjectile = false;
    }

    private void OnStoneRainReleased()
    {
        if (!isServer) return;

        FireWeapon(false, pendingProjectileRequest.projectileKind, new AttackContext { venomancerPhase = VenomancerPhase.StoneRain }, targetPlayer.NetAuth.netId);

        pendingProjectileRequest = default;
        hasPendingProjectile = false;
    }

    private void OnToxicPoolReleased()
    {
        if (!isServer) return;

        FireWeapon(false, pendingProjectileRequest.projectileKind, new AttackContext { venomancerPhase = VenomancerPhase.ToxicPool }, targetPlayer.NetAuth.netId);

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

    /// <summary>
    /// Execute Seismic Slam special move
    /// </summary>
    private void SeismicSlam()
    {
        // Get all colliders within the radius of the seismic slam
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, seismicSlamCircleRadius);

        if (specialMoveParticlesSystem != null)
        {
            specialMoveParticlesSystem.Play();

            //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);
        }

        StaticEventHandler.CallCameraShakeEvent(shakeIntensity, shakeDuration);

        foreach (Collider2D col in colliders)
        {
            // Check if the collider belongs to an enemy or any other object you want to affect
            if (col.CompareTag(Settings.playerTag))
            {
                // Apply damage to the enemy
                Player player = col.GetComponent<Player>();
                ReceiveProjectileDamage receiveProjectileDamage = col.GetComponent<ReceiveProjectileDamage>();

                // Apply knockback
                ApplyKnockbackToPlayer(player);

                DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = player.health.transform.position };

                if (receiveProjectileDamage != null)
                {
                    receiveProjectileDamage.TakeProjectileDamage(15, ctx);
                }
            }
        }
    }

    public void PlayerStealthCheck()
    {
        currentVenomancerPhase = VenomancerPhase.Wait;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, seismicSlamCircleRadius);
    }
}
