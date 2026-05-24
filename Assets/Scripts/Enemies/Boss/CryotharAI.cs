using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class CryotharAI : EnemyAI, IMutualBossBehaviour
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

    [Header("Tail Attack")]
    [SerializeField] float smearCircleRadius = 2.2f;
    [SerializeField] int smearDamage = 25;
    [SerializeField] float smearChargeSpeed = 28f;
    [SerializeField] float smearChargeDuration = 0.65f;
    [SerializeField] float smearAttackDuration = 0.8f;

    [Header("Frost Breathe")]
    [SerializeField] float frostBreatheCircleRadius = 2.2f;

    [Header("Boss Behaviour")]
    [SerializeField] float waitPhase = 0.15f;

    [Header("Difficulty")]
    [SerializeField] float phase2Threshold = 0.7f;
    [SerializeField] float phase3Threshold = 0.4f;

    CryotharPhase currentCryotharPhase;

    Coroutine cryotharRoutine;

    bool smearDamageActive;

    // STATE
    private float phaseTimer;

    bool phase2Active;
    bool phase3Active;

    bool lockAttackVector;
    Vector2 lockedPosition;

    float aggressiveMultiplier = 1f;

    // EVENTS
    public UnityEvent resetAnimationEvent;
    public UnityEvent smearStartEvent;
    public UnityEvent smearStopEvent;
    public UnityEvent breatheReleaseEvent;
    public UnityEvent icicleReleaseEvent;

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

        currentCryotharPhase = CryotharPhase.Wait;
        targetPlayer = GameManager.Instance.GetLocalPlayer();
    }

    protected override void OnEnable()
    {
        resetAnimationEvent.AddListener(ResetAnimations);
        smearStartEvent.AddListener(EnableSmearDamage);
        smearStopEvent.AddListener(DisableSmearDamage);
        breatheReleaseEvent.AddListener(OnBreatheReleased);
        icicleReleaseEvent.AddListener(OnIcicleReleased);
    }

    protected override void OnDisable()
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
        smearStartEvent.RemoveListener(EnableSmearDamage);
        smearStopEvent.RemoveListener(DisableSmearDamage);
        breatheReleaseEvent.RemoveListener(OnBreatheReleased);
        icicleReleaseEvent.RemoveListener(OnIcicleReleased);
    }

    public void ResetAnimationEvent() => resetAnimationEvent?.Invoke();
    public void SmearStartEvent() => smearStartEvent?.Invoke();
    public void SmearStopEvent() => smearStopEvent?.Invoke();
    public void BreatheReleaseEvent() => breatheReleaseEvent?.Invoke();
    public void IcicleReleaseEvent() => icicleReleaseEvent?.Invoke();

    protected override void Update()
    {
        if (currentRoom == null) currentRoom = enemy.owningSpawner.instantiatedRoom.room;

        if (enemyPhase == EnemyPhase.Death) return;

        if (targetPlayer != null)
        {
            trackingVector = (targetPlayer.GetPlayerPosition() - transform.position).normalized;
        }
    }

    protected override void FixedUpdate()
    {
        if (targetPlayer == null) return;
        if (enemyPhase == EnemyPhase.Death) return;

        // Emergency pullback if Sylvarok drifts outside bounds
        if (IsOutsideBossRoom(transform.position, cellMin, cellMax))
        {
            Vector3 safePos = ClampToBossRoom(transform.position, cellMin, cellMax);
            rb2D.position = safePos;
            rb2D.linearVelocity = Vector2.zero;

            Debug.LogWarning("Cryothar was outside bounds. Snapped back.");
        }

        UpdateBossDifficulty();

        HandleAim();

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            if (targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            switch (currentCryotharPhase)
            {
                case CryotharPhase.Wait:

                    PassedToWait = true;
                    phaseTimer += Time.fixedDeltaTime;

                    if (phaseTimer >= waitPhase)
                    {
                        phaseTimer = 0;
                        TransitionToNextPhase();
                    }
                    break;
                case CryotharPhase.IceProjectile:
                    if (cryotharRoutine == null)
                    {
                        cryotharRoutine = StartCoroutine(IceProjectileRoutine());
                    }
                    break;
                case CryotharPhase.TailAttack:
                    if (cryotharRoutine == null)
                    {
                        cryotharRoutine = StartCoroutine(TailAttackRoutine());
                    }
                    break;
                case CryotharPhase.Icicle:
                    if (cryotharRoutine == null)
                    {
                        cryotharRoutine = StartCoroutine(IcicleRoutine());
                    }
                    break;
                case CryotharPhase.FrostBreath:
                    if (cryotharRoutine == null)
                    {
                        cryotharRoutine = StartCoroutine(FrostBreatheRoutine());
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
        }
    }

    public void HandleWaitPhase()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();
    }

    private void TransitionToNextPhase()
    {
        if (targetPlayer == null)
        {
            currentCryotharPhase = CryotharPhase.Wait;
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.GetPlayerPosition());

        // CLOSE RANGE
        if (distance < 3f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 70) currentCryotharPhase = CryotharPhase.FrostBreath;
            else currentCryotharPhase = CryotharPhase.TailAttack;

            return;
        }

        // LONG RANGE
        if (distance > 8f)
        {
            int roll = Random.Range(0, 100);

            if (roll < 65) currentCryotharPhase = CryotharPhase.Icicle;
            else currentCryotharPhase = CryotharPhase.IceProjectile;

            return;
        }

        // MID RANGE
        int midRoll = Random.Range(0, 100);

        if (midRoll < 45) currentCryotharPhase = CryotharPhase.TailAttack;
        else if (midRoll < 75) currentCryotharPhase = CryotharPhase.IceProjectile;
        else currentCryotharPhase = CryotharPhase.Icicle;
    }

    // ICE PROJECTILE
    IEnumerator IceProjectileRoutine()
    {
        BeginRoutine();

        float timer = 0f;
        float duration = 2f;

        enemy.animateEnemy.SetHealAnimation(true);

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
                    attackContext = new AttackContext { cryotharPhase = CryotharPhase.IceProjectile },
                    lockedAimVector = shotDirection
                };
            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetHealAnimation(false);

        cryotharRoutine = null;

        currentCryotharPhase = CryotharPhase.Wait;
        phaseTimer = 0;
    }

    // TAIL ATTACK
    IEnumerator TailAttackRoutine()
    {
        BeginRoutine();

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

        smearDamageActive = false;

        // SMEAR ATTACK START
        enemy.animateEnemy.SetSummonAnimation(true);

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

        enemy.animateEnemy.SetSummonAnimation(false);

        isAttacking = false;

        CleanupRoutine();

        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    // FROST BREATHE
    IEnumerator FrostBreatheRoutine()
    {
        BeginRoutine();

        isAttacking = true;
        rb2D.linearVelocity = Vector2.zero;

        // ONLY USE AT CLOSE RANGE
        if (targetPlayer == null || Vector2.Distance(rb2D.position, targetPlayer.GetPlayerPosition()) > 3f)
        {
            CleanupRoutine();
            yield break;
        }

        // LOCK AIM ONCE
        attackLockedVector = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

        lockAttackVector = true;

        smearDamageActive = false;

        // SMEAR ATTACK START
        enemy.animateEnemy.SetCastAnimation(true);

        float breathTimer = 0f;
        float breathDuration = 2f;

        while (breathTimer < breathDuration)
        {
            if (ShouldCancelRoutine()) yield break;

            breathTimer += Time.fixedDeltaTime;

            // DAMAGE WINDOW CONTROLLED BY ANIMATION EVENTS
            if (smearDamageActive)
            {
                PerformSmearDamage(frostBreatheCircleRadius);
            }

            yield return waitForFixedUpdate;
        }

        smearDamageActive = false;

        enemy.animateEnemy.SetCastAnimation(false);

        isAttacking = false;

        CleanupRoutine();

        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    // ICICLE
    IEnumerator IcicleRoutine()
    {
        if (enemy.health.hasDied) yield break;

        float timer = 0f;
        float duration = 2.5f;

        enemy.animateEnemy.SetFocusedAnimation(true);

        while (timer < duration)
        {
            if (enemy.health.hasDied) yield break;

            timer += Time.fixedDeltaTime;

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetFocusedAnimation(false);

        cryotharRoutine = null;

        currentCryotharPhase = CryotharPhase.Wait;
        phaseTimer = 0f;
    }

    void PerformSmearDamage(float radius)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(swordHoldingTransform.position, radius);

        foreach (Collider2D collider in colliders)
        {
            if (!collider.CompareTag(Settings.playerTag)) continue;

            Player player = collider.GetComponent<Player>();
            if (player == null) continue;

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
        cryotharRoutine = null;
        currentCryotharPhase = CryotharPhase.Wait;

        passedToWait = false;
        phaseTimer = 0f;
    }

    bool ShouldCancelRoutine()
    {
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
        smearDamageActive = true;
    }

    public void DisableSmearDamage()
    {
        smearDamageActive = false;
    }

    private void OnBreatheReleased()
    {
        if (!hasPendingProjectile) return;

        attackLockedVector = pendingProjectileRequest.lockedAimVector;
        lockAttackVector = true;

        FireWeapon(false, pendingProjectileRequest.projectileKind, pendingProjectileRequest.attackContext);

        hasPendingProjectile = false;
    }

    private void OnIcicleReleased()
    {
        FireWeapon(false, pendingProjectileRequest.projectileKind, new AttackContext { cryotharPhase = CryotharPhase.Icicle });

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
    }

    public void PlayerStealthCheck()
    {
        currentCryotharPhase = CryotharPhase.Wait;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, smearCircleRadius);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, frostBreatheCircleRadius);
    }
}
