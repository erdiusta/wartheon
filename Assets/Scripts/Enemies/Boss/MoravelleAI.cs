using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MoravelleAI : EnemyAI, IMutualBossBehaviour
{
    // ROOM LIMITS
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // PHASE
    MoravellePhase currentMoravellePhase;

    [Header("Boss Behaviour")]
    [SerializeField] float waitPhase = 0.15f;
    [SerializeField] float preferredDistance = 6f;

    [Header("Movement")]
    [SerializeField] float strafeForce = 8f;
    [SerializeField] float retreatForce = 6f;
    [SerializeField] float movementForce = 5f;

    [Header("Charge")]
    [SerializeField] float chargePrepareDuration = 0.55f;
    [SerializeField] float chargeDuration = 0.45f;
    [SerializeField] float chargePredictionMultiplier = 0.45f;
    [SerializeField] float chargeOvershootDistance = 1.5f;

    [Header("Boss Scaling")]
    [SerializeField] float phase2Threshold = 0.7f;
    [SerializeField] float phase3Threshold = 0.4f;

    // STATE
    private float phaseTimer;
    bool phase2Active;
    bool phase3Active;

    float aggressiveMultiplier = 1f;

    bool passedToWait;

    bool lockAttackVector;
    Vector3 lockedPosition;

    CapsuleCollider2D movementCollider;

    Coroutine moravelleRoutine;

    // ANIMATION EVENT STATES
    bool pendingChargeRelease;
    bool chargeReleased;

    // EVENTS
    public UnityEvent resetAnimationEvent;
    public UnityEvent chargeStartEvent;
    public UnityEvent bowReleaseEvent;

    // PROPERTIES
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
    }

    protected override void Start()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom();

        currentMoravellePhase = MoravellePhase.Wait;
    }

    protected override void OnEnable()
    {
        resetAnimationEvent.AddListener(ResetAnimations);
        chargeStartEvent.AddListener(OnChargeReleaseFrame);
        bowReleaseEvent.AddListener(OnBowReleaseFrame);
    }

    protected override void OnDisable()
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
        chargeStartEvent.RemoveListener(OnChargeReleaseFrame);
        bowReleaseEvent.RemoveListener(OnBowReleaseFrame);
    }

    public void ResetAnimationEvent() => resetAnimationEvent?.Invoke();
    public void ChargeReleaseEvent() => chargeStartEvent?.Invoke();
    public void BowReleaseEvent() => bowReleaseEvent?.Invoke();

    protected override void Update()
    {
        if (enemyPhase == EnemyPhase.Death)
        {
            if (attackAnimationRoutine != null)
            {
                StopCoroutine(attackAnimationRoutine);
            }

            return;
        }

        if (chargeGraceTimer > 0)
        {
            chargeGraceTimer -= Time.deltaTime;
        }

        healTimer += Time.deltaTime;

        if (targetPlayer != null)
        {
            trackingVector = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

            if (!lockAttackVector)
            {
                attackLockedVector = trackingVector;
            }
        }
    }

    protected override void FixedUpdate()
    {
        if (targetPlayer == null) return;

        if (enemyPhase == EnemyPhase.Death)
        {
            if (attackAnimationRoutine != null)
            {
                StopCoroutine(attackAnimationRoutine);
            }

            return;
        }

        UpdateBossDifficulty();

        // Emergency pullback if Sylvarok drifts outside bounds
        if (IsOutsideBossRoom(transform.position, cellMin, cellMax))
        {
            Vector3 safePos = ClampToBossRoom(transform.position, cellMin, cellMax);
            rb2D.position = safePos;
            rb2D.linearVelocity = Vector2.zero;

            Debug.LogWarning("Moravelle was outside bounds. Snapped back.");
        }

        // AIM
        Vector2 activeAimVector = attackLockedVector;
        float unitAngle = HelperUtilities.GetAngleFromVector(activeAimVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);

        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Moravelle);

        if (unitAimDirection != enemy.LastAim)
        {
            enemy.LastAim = unitAimDirection;

            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimParameters(unitAimDirection);
        }

        // STATE
        if (enemy.moveStatus != MoveStatus.Idle) return;

        switch (currentMoravellePhase)
        {
            case MoravellePhase.Wait:
                PassedToWait = true;
                phaseTimer += Time.fixedDeltaTime;
                MaintainDistance();

                if (phaseTimer >= waitPhase)
                {
                    phaseTimer = 0;
                    TransitionToNextPhase();
                }
                break;
            case MoravellePhase.StraightArrowShot:
                if (moravelleRoutine == null)
                {
                    moravelleRoutine = StartCoroutine(StraightArrowRoutine());
                }
                break;

            case MoravellePhase.SpreadArrowShot:
                if (moravelleRoutine == null)
                {
                    moravelleRoutine = StartCoroutine(SpreadArrowRoutine());
                }

                break;

            case MoravellePhase.Charge:
                if (moravelleRoutine == null)
                {
                    moravelleRoutine = StartCoroutine(ChargeRoutine());
                }

                break;

            default:
                break;
        }
    }

    // STRAIGHT SHOT
    IEnumerator StraightArrowRoutine()
    {
        float attackDuration = 4f;
        float timer = 0f;

        enemy.animateEnemy.SetCastAnimation(true);
        enemy.enemyAnimSync?.SetBossCastAnimation(true);

        while (timer < attackDuration)
        {
            if (enemy.health.hasDied) yield break;

            timer += Time.fixedDeltaTime;

            StrafeAroundPlayer();
            MaintainDistance();

            if (!hasPendingProjectile)
            {
                hasPendingProjectile = true;

                Vector2 shotDirection = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

                pendingProjectileRequest = new PendingProjectileRequest
                {
                    projectileKind = ProjectileKind.Default,
                    attackContext = default,
                    lockedAimVector = shotDirection
                };
            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetCastAnimation(false);
        enemy.enemyAnimSync?.SetBossCastAnimation(false);

        moravelleRoutine = null;

        currentMoravellePhase = MoravellePhase.Wait;
        phaseTimer = 0;
    }

    // SPREAD SHOT
    IEnumerator SpreadArrowRoutine()
    {
        float timer = 0f;
        float duration = 2f;

        enemy.animateEnemy.SetFocusedAnimation(true);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(true);

        while (timer < duration)
        {
            if (enemy.health.hasDied) yield break;

            timer += Time.fixedDeltaTime;

            MaintainDistance();

            if (!hasPendingProjectile)
            {
                hasPendingProjectile = true;

                Vector2 shotDirection = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

                pendingProjectileRequest = new PendingProjectileRequest
                {
                    projectileKind = ProjectileKind.Default,
                    attackContext = new AttackContext { moravellePhase = MoravellePhase.SpreadArrowShot },
                    lockedAimVector = shotDirection
                };
            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetFocusedAnimation(false);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(false);

        moravelleRoutine = null;

        currentMoravellePhase = MoravellePhase.Wait;
        phaseTimer = 0;
    }

    // CHARGE
    IEnumerator ChargeRoutine()
    {
        lockAttackVector = true;
        isAttacking = true;
        chargeGraceTimer = 1.5f;

        chargeStartPosition = transform.position;

        Rigidbody2D targetRB = targetPlayer.GetComponent<Rigidbody2D>();

        Vector2 currentPos = transform.position;
        Vector2 predictedPosition = targetPlayer.GetPlayerPosition();

        if (targetRB != null)
        {
            predictedPosition += targetRB.linearVelocity * chargePredictionMultiplier;
        }

        Vector2 chargeDirection = (predictedPosition - currentPos).normalized;

        chargeMoveDirection = chargeDirection;

        Vector2 overshootPosition = predictedPosition + chargeDirection * chargeOvershootDistance;
        Vector2 clampedPosition = ClampToBossRoom(overshootPosition, cellMin, cellMax);
        lockedPosition = GetValidPosition(clampedPosition, movementCollider);

        // PREPARE CHARGE
        pendingChargeRelease = true;
        chargeReleased = false;

        enemy.animateEnemy.SetChargeAnimation(true);
        enemy.enemyAnimSync?.SetBossChargeAnimation(true);

        float timer = 0f;

        while (!chargeReleased)
        {
            if (enemy.health.hasDied) yield break;

            timer += Time.fixedDeltaTime;

            if (timer >= chargePrepareDuration + 1f)
            {
                ResetChargeState();
                yield break;
            }

            yield return waitForFixedUpdate;
        }

        // CHARGE LOOP
        while (isCharging)
        {
            CapsuleCollider2D col = movementCollider;

            if (col != null)
            {
                ContactFilter2D filter = new ContactFilter2D();
                filter.useLayerMask = true;
                filter.useTriggers = false;
                filter.layerMask = GetObstacleMask();

                RaycastHit2D[] hits = new RaycastHit2D[10];

                int hitCount = col.Cast(chargeMoveDirection, filter, hits, 0.15f);

                bool validObstacleDetected = false;

                // VALIDATE HITS
                for (int i = 0; i < hitCount; i++)
                {
                    Collider2D hitCol = hits[i].collider;

                    if (hitCol == null) continue;

                    // Door collision control
                    if (hitCol.GetComponentInParent<Door>() != null) continue;

                    // Ignore self colliders
                    if (hitCol.transform.root == transform.root) continue;

                    // Ignore triggers
                    if (hitCol.isTrigger) continue;

                    // Ignore overlap artifacts
                    if (hits[i].distance <= 0.001f) continue;

                    validObstacleDetected = true;
                    break;
                }

                // STOP CHARGE ON VALID OBSTACLE
                if (validObstacleDetected)
                {
                    StopCharge();
                    break;
                }
            }

            // TARGET REACHED
            Vector2 toTarget = (Vector2)lockedPosition - (Vector2)transform.position;
            float remainingDistance = toTarget.magnitude;

            if (remainingDistance <= 0.6f)
            {
                StopCharge();
                break;
            }

            float speed = rb2D.linearVelocity.magnitude;

            if (speed <= 0.05f && chargeGraceTimer <= 0f)
            {
                StopCharge();
                break;
            }

            yield return waitForFixedUpdate;
        }

        lockAttackVector = false;
        isAttacking = false;
        moravelleRoutine = null;

        currentMoravellePhase = MoravellePhase.Wait;
        phaseTimer = 0;
    }

    // ANIMATION EVENTS
    public void OnBowReleaseFrame()
    {
        if (!hasPendingProjectile) return;

        attackLockedVector = pendingProjectileRequest.lockedAimVector;
        lockAttackVector = true;

        FireWeapon(false, pendingProjectileRequest.projectileKind, pendingProjectileRequest.attackContext);

        pendingProjectileRequest = default;
        hasPendingProjectile = false;

        StartCoroutine(DelayedAimUnlock());
    }

    IEnumerator DelayedAimUnlock()
    {
        yield return new WaitForSeconds(0.08f);

        lockAttackVector = false;
    }

    public void OnChargeReleaseFrame()
    {
        if (!pendingChargeRelease) return;

        pendingChargeRelease = false;
        chargeReleased = true;

        StartChargeMovement();
    }

    // START CHARGE
    void StartChargeMovement()
    {
        isCharging = true;

        Vector2 currentPos = transform.position;
        Vector2 moveDir = ((Vector2)lockedPosition - currentPos).normalized;

        float chargeDistance = Vector2.Distance(currentPos, lockedPosition);
        float requiredVelocity = chargeDistance / chargeDuration;

        rb2D.linearVelocity = Vector2.zero;
        rb2D.linearDamping = 0;

        rb2D.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb2D.linearVelocity = moveDir * requiredVelocity;
    }

    // STOP CHARGE
    void StopCharge()
    {
        isCharging = false;

        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();

        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.ResetAllBossAnimations();

        StartCoroutine(ReturnToCenterRoutine());
    }

    IEnumerator ReturnToCenterRoutine()
    {
        isRecoveringFromCharge = true;

        float recoveryDuration = 0.4f;
        float timer = 0f;

        rb2D.linearDamping = 4f;

        if (currentRoom == null) currentRoom = GameManager.Instance.GetCurrentRoom();

        while (timer < recoveryDuration)
        {
            timer += Time.fixedDeltaTime;

            Vector2 cellCenter = (currentRoom.lowerBounds + currentRoom.upperBounds) / 2;
            Vector2 dir = (cellCenter - (Vector2)transform.position).normalized;

            rb2D.linearVelocity = dir * 3f;

            yield return waitForFixedUpdate;
        }

        rb2D.linearVelocity = Vector2.zero;
        isRecoveringFromCharge = false;
    }

    // TRANSITIONS
    void TransitionToNextPhase()
    {
        if (targetPlayer == null) return;

        if (targetPlayer.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        float distance = Vector2.Distance(transform.position, targetPlayer.GetPlayerPosition());
        float random = Random.value;

        // HIGH AGGRESSION LOW HP
        float chargeChance = phase3Active ? 0.55f : phase2Active ? 0.4f : 0.3f;

        // CLOSE RANGE
        if (distance < 5f)
        {
            if (random < chargeChance) currentMoravellePhase = MoravellePhase.Charge;
            else currentMoravellePhase = MoravellePhase.SpreadArrowShot;

            return;
        }

        // MID RANGE
        if (distance < 8f)
        {
            if (random < chargeChance) currentMoravellePhase = MoravellePhase.Charge;
            else if (random < 0.45f) currentMoravellePhase = MoravellePhase.StraightArrowShot;
            else currentMoravellePhase = MoravellePhase.SpreadArrowShot;
        }

        // LONG RANGE
        if (random < 0.45f) currentMoravellePhase = MoravellePhase.Charge;
        else if (random < 0.65f) currentMoravellePhase = MoravellePhase.SpreadArrowShot;
        else currentMoravellePhase = MoravellePhase.StraightArrowShot;
    }

    // HELPERS
    public void HandleWaitPhase()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();

        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.ResetAllBossAnimations();
    }

    void UpdateBossDifficulty()
    {
        float hpPercent = (float)enemy.health.GetCurrentHealth() / enemy.health.GetMaximumHealth();

        if (!phase2Active && hpPercent <= phase2Threshold)
        {
            phase2Active = true;
            aggressiveMultiplier = 1.2f;
            waitPhase = 0.15f;
        }

        if (!phase3Active && hpPercent <= phase3Threshold)
        {
            phase3Active = true;
            aggressiveMultiplier = 1.5f;
            waitPhase = 0.1f;
        }
    }

    void StrafeAroundPlayer()
    {
        if (targetPlayer == null) return;

        Vector2 toPlayer = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

        Vector2 perpendicular = Random.value > 0.5f ? Vector2.Perpendicular(toPlayer) : -Vector2.Perpendicular(toPlayer);
        rb2D.AddForce(perpendicular * strafeForce);
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

    void ResetChargeState()
    {
        pendingChargeRelease = false;
        chargeReleased = false;

        isCharging = false;
        lockAttackVector = false;
        isAttacking = false;

        enemy.animateEnemy.SetChargeAnimation(false);
        enemy.enemyAnimSync?.SetBossChargeAnimation(false);

        moravelleRoutine = null;

        currentMoravellePhase = MoravellePhase.Wait;
        phaseTimer = 0f;
    }

    private void ResetAnimations()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();

        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.ResetAllBossAnimations();
    }

    public void PlayerStealthCheck()
    {
        currentMoravellePhase = MoravellePhase.Wait;
    }
}
