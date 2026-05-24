using Mirror;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class GalvanusAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSS
    GalvanusPhase currentGalvanusPhase;

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

    bool lockAttackVector;
    Vector3 lockedPosition;

    float aggressiveMultiplier = 1f;

    // EVENTS
    public UnityEvent resetAnimationEvent;
    public UnityEvent chargeStartEvent;
    public UnityEvent lightningBoltRelease;
    public UnityEvent lightningRelease;

    // ANIMATION EVENT STATES
    bool pendingChargeRelease;
    bool chargeReleased;

    bool chargeProcessStarted;

    uint pendingLightningTargetNetId = 0;

    CapsuleCollider2D movementCollider;

    Coroutine galvanusRoutine;

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
    }

    protected override void Start()
    {
        base.Start();

        targetPlayer = GameManager.Instance.GetLocalPlayer();

        currentGalvanusPhase = GalvanusPhase.Wait;
    }

    protected override void OnEnable()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom();

        resetAnimationEvent.AddListener(ResetAnimations);
        chargeStartEvent.AddListener(OnChargeReleaseFrame);
        lightningBoltRelease.AddListener(OnLightningBoltRelease);
        lightningRelease.AddListener(OnLightningRelease);
    }

    protected override void OnDisable()
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
        chargeStartEvent.RemoveListener(OnChargeReleaseFrame);
        lightningBoltRelease.RemoveListener(OnLightningBoltRelease);
        lightningRelease.RemoveListener(OnLightningRelease);
    }

    public void ResetAnimationEvent() => resetAnimationEvent?.Invoke();
    public void ChargeReleaseEvent() => chargeStartEvent?.Invoke();
    public void LightningBoltReleaseEvent() => lightningBoltRelease?.Invoke();
    public void LightningReleaseEvent() => lightningRelease?.Invoke();

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

    protected override void Update()
    {
        prevVel = rb2D.linearVelocity;

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

        prevVel = rb2D.linearVelocity;

        if (enemyPhase == EnemyPhase.Death)
        {
            if (attackAnimationRoutine != null)
            {
                StopCoroutine(attackAnimationRoutine);
            }

            return;
        }

        UpdateBossDifficulty();

        healTimer += Time.deltaTime;

        // Emergency pullback if Sylvarok drifts outside bounds
        if (IsOutsideBossRoom(transform.position, cellMin, cellMax))
        {
            Vector3 safePos = ClampToBossRoom(transform.position, cellMin, cellMax);
            rb2D.position = safePos;
            rb2D.linearVelocity = Vector2.zero;

            Debug.LogWarning("Galvanus was outside bounds. Snapped back.");
        }

        // AIM
        Vector2 activeAimVector = attackLockedVector;
        float unitAngle = HelperUtilities.GetAngleFromVector(activeAimVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);

        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Galvanus);

        if (unitAimDirection != enemy.LastAim)
        {
            enemy.LastAim = unitAimDirection;

            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimParameters(unitAimDirection);
        }

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            if (targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            switch (currentGalvanusPhase)
            {
                case GalvanusPhase.Wait:
                    PassedToWait = true;
                    phaseTimer += Time.fixedDeltaTime;
                    MaintainDistance();

                    if (phaseTimer >= waitPhase)
                    {
                        phaseTimer = 0;
                        TransitionToNextPhase();
                    }
                    break;
                case GalvanusPhase.LightningBolt:
                    if (galvanusRoutine == null)
                    {
                        galvanusRoutine = StartCoroutine(LightningBoltRoutine());
                    }
                    break;
                case GalvanusPhase.Charge:
                    if (galvanusRoutine == null)
                    {
                        galvanusRoutine = StartCoroutine(ChargeRoutine());
                    }
                    break;
                case GalvanusPhase.Lightning:
                    if (galvanusRoutine == null)
                    {
                        galvanusRoutine = StartCoroutine(LightningRoutine());
                    }
                    break;
                default:
                    break;
            }
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

        if (targetPlayer.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        float distance = Vector3.Distance(transform.position + new Vector3(0f, 0.8f, 0f), targetPlayer.GetPlayerPosition());

        float rng = Random.value;

        if (distance < 4f)
        {
            currentGalvanusPhase = (GalvanusPhase)Random.Range(2, 4); // Dash or Bolt
        }
        else if (distance <= 8f)
        {
            if (rng < 0.4f) currentGalvanusPhase = GalvanusPhase.LightningBolt;
            else if (rng < 0.7f) currentGalvanusPhase = GalvanusPhase.Charge;
            else currentGalvanusPhase = GalvanusPhase.Lightning;
        }
        else
        {
            if (rng < 0.3f) currentGalvanusPhase = GalvanusPhase.Charge;
            else currentGalvanusPhase = GalvanusPhase.Lightning;
        }
    }

    // LIGHTNING BOLT
    IEnumerator LightningBoltRoutine()
    {
        float attackDuration = 5f;
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
                    attackContext = new AttackContext { galvanusPhase = GalvanusPhase.LightningBolt },
                    lockedAimVector = shotDirection
                };
            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetCastAnimation(false);
        enemy.enemyAnimSync?.SetBossCastAnimation(false);

        galvanusRoutine = null;

        currentGalvanusPhase = GalvanusPhase.Wait;
        phaseTimer = 0;
    }

    // CHARGE
    IEnumerator ChargeRoutine()
    {
        lockAttackVector = true;
        isAttacking = true;
        chargeGraceTimer = 1f;

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

        // RESET BEFORE CHARGE
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();
        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.ResetAllBossAnimations();

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
        galvanusRoutine = null;

        enemy.animateEnemy.SetChargeAnimation(false);
        enemy.enemyAnimSync?.SetBossChargeAnimation(false);

        currentGalvanusPhase = GalvanusPhase.Wait;
        phaseTimer = 0;
    }


    // LIGHTNING
    IEnumerator LightningRoutine()
    {
        if (enemy.health.hasDied) yield break;

        float timer = 0f;
        float duration = 2.5f;

        // Arm Only Once

        enemy.animateEnemy.SetFocusedAnimation(true);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(true);

        while (timer < duration)
        {
            if (enemy.health.hasDied) yield break;

            timer += Time.fixedDeltaTime;

            yield return waitForFixedUpdate;
        }

        pendingLightningTargetNetId = 0;

        enemy.animateEnemy.SetFocusedAnimation(false);
        enemy.enemyAnimSync?.SetBossFocusedAnimation(false);

        galvanusRoutine = null;

        currentGalvanusPhase = GalvanusPhase.Wait;
        phaseTimer = 0f;
    }

    // ANIMATION EVENTS
    public void OnLightningBoltRelease()
    {
        if (!hasPendingProjectile) return;

        FireWeapon(false, pendingProjectileRequest.projectileKind, pendingProjectileRequest.attackContext);

        pendingProjectileRequest = default;
        hasPendingProjectile = false;
    }

    public void OnChargeReleaseFrame()
    {
        if (!pendingChargeRelease) return;

        pendingChargeRelease = false;
        chargeReleased = true;

        StartChargeMovement();
    }

    public void OnLightningRelease()
    {
        FireWeapon(false, pendingProjectileRequest.projectileKind, new AttackContext { galvanusPhase = GalvanusPhase.Lightning });

        pendingProjectileRequest = default;
        hasPendingProjectile = false;
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

    void StrafeAroundPlayer()
    {
        if (targetPlayer == null) return;

        Vector2 toPlayer = (targetPlayer.GetPlayerPosition() - transform.position).normalized;

        Vector2 perpendicular = Random.value > 0.5f ? Vector2.Perpendicular(toPlayer) : -Vector2.Perpendicular(toPlayer);
        rb2D.AddForce(perpendicular * strafeForce);
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

        galvanusRoutine = null;

        currentGalvanusPhase = GalvanusPhase.Wait;
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
        currentGalvanusPhase = GalvanusPhase.Wait;
    }
}
