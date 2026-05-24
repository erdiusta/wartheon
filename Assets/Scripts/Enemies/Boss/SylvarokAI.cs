using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class SylvarokAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-4, 6);
    readonly Vector2Int cellMax = new Vector2Int(8, 14);

    // BOSS
    SylvarokPhase currentSylvarokPhase;

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
    public UnityEvent razorLeafRelease;

    // ANIMATION EVENT STATES
    bool pendingChargeRelease;
    bool chargeReleased;

    bool chargeProcessStarted;
    int enemiesToSpawn = 2;

    CapsuleCollider2D movementCollider;

    Coroutine sylvarokRoutine;

    bool passedToWait;

    readonly List<Enemy> summonedMinions = new List<Enemy>();

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

        currentSylvarokPhase = SylvarokPhase.Wait;
    }

    protected override void OnEnable()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom();

        resetAnimationEvent.AddListener(ResetAnimations);
        chargeStartEvent.AddListener(OnChargeReleaseFrame);
        razorLeafRelease.AddListener(OnRazorLeafRelease);
    }

    protected override void OnDisable()
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
        chargeStartEvent.RemoveListener(OnChargeReleaseFrame);
        razorLeafRelease.RemoveListener(OnRazorLeafRelease);
    }

    public void ResetAnimationEvent() => resetAnimationEvent?.Invoke();
    public void ChargeReleaseEvent() => chargeStartEvent?.Invoke();
    public void RazorLeafReleaseEvent() => razorLeafRelease?.Invoke();

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

            Debug.LogWarning("Sylvarok was outside bounds. Snapped back.");
        }

        // AIM
        Vector2 activeAimVector = attackLockedVector;
        float unitAngle = HelperUtilities.GetAngleFromVector(activeAimVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);

        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Sylvarok);

        if (unitAimDirection != enemy.LastAim)
        {
            enemy.LastAim = unitAimDirection;

            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimParameters(unitAimDirection);
        }

        // STATE
        if (enemy.moveStatus != MoveStatus.Idle) return;

        if (targetPlayer.isStealthActive)
        {
            PlayerStealthCheck();
        }

        switch (currentSylvarokPhase)
        {
            case SylvarokPhase.Wait:
                PassedToWait = true;
                phaseTimer += Time.fixedDeltaTime;
                MaintainDistance();

                if (phaseTimer >= waitPhase)
                {
                    phaseTimer = 0;
                    TransitionToNextPhase();
                }
                break;
            case SylvarokPhase.Charge:
                if (sylvarokRoutine == null)
                {
                    sylvarokRoutine = StartCoroutine(ChargeRoutine());
                }
                break;
            case SylvarokPhase.RazorLeaf:
                if (sylvarokRoutine == null)
                {
                    sylvarokRoutine = StartCoroutine(RazorLeafRoutine());
                }
                break;
            case SylvarokPhase.Summon:
                if (sylvarokRoutine == null)
                {
                    sylvarokRoutine = StartCoroutine(SummonRoutine());
                }
                break;
            case SylvarokPhase.Heal:
                if (sylvarokRoutine == null)
                {
                    sylvarokRoutine = StartCoroutine(HealRoutine());
                }
                break;
            default:
                break;
        }
    }

    public void HandleWaitPhase()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();
    }

    // CHARGE
    IEnumerator ChargeRoutine()
    {
        enemy.debugDisplay.text = "Current Phase: CHARGE";

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

        enemy.animateEnemy.SetChargeAnimation(true);

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
        sylvarokRoutine = null;

        enemy.animateEnemy.SetChargeAnimation(false);

        currentSylvarokPhase = SylvarokPhase.Wait;
        phaseTimer = 0;
    }

    // RAZOR LEAF
    IEnumerator RazorLeafRoutine()
    {
        if (enemy.health.hasDied) yield break;

        enemy.debugDisplay.text = "Current Phase: RAZOR LEAF";

        float timer = 0f;
        float duration = 1.2f;

        enemy.animateEnemy.SetFocusedAnimation(true);

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
                    attackContext = new AttackContext { sylvarokPhase = SylvarokPhase.RazorLeaf },
                    lockedAimVector = shotDirection
                };
            }

            yield return waitForFixedUpdate;
        }

        enemy.animateEnemy.SetFocusedAnimation(false);

        sylvarokRoutine = null;

        currentSylvarokPhase = SylvarokPhase.Wait;
        phaseTimer = 0;
    }

    // ANIMATION EVENTS
    public void OnRazorLeafRelease()
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

    // SUMMON
    IEnumerator SummonRoutine()
    {
        if (enemy.health.hasDied) yield break;

        enemy.debugDisplay.text = "Current Phase: SUMMON";

        enemy.animateEnemy.SetSummonAnimation(true);

        Grid grid = enemy.owningSpawner.instantiatedRoom.GetComponentInChildren<Grid>();

        //SoundEffectManager.Instance.PlaySoundEffect(enemyDetails.attackSoundEffect);

        summonedMinions.RemoveAll(x => x == null || x.health.hasDied);

        for (int i = 0; i < enemiesToSpawn; i++)
        {
            bool validSpawnFound = false;

            for (int attempt = 0; attempt < 15; attempt++)
            {
                int randomX = Random.Range(cellMin.x, cellMax.y + 1);
                int randomY = Random.Range(cellMin.x, cellMax.y + 1);

                Vector3Int randomCell = new Vector3Int(randomX, randomY, 0);

                Vector3 spawnWorldPos = grid.GetCellCenterWorld(randomCell);

                Collider2D hit = Physics2D.OverlapCircle(spawnWorldPos, 0.4f, avoidLayerMask);

                if (hit != null) continue;

                enemy.owningSpawner.CreateEnemySP(enemyDetails.enemyMinionDetails, spawnWorldPos, out Enemy minionEnemy);

                if (minionEnemy != null)
                {
                    summonedMinions.Add(minionEnemy);
                }

                validSpawnFound = true;
                break;
            }

            if (!validSpawnFound)
            {
                Debug.LogWarning("Sylvarok failed to find valid summon position");
            }
        }

        yield return new WaitForSeconds(1.5f);

        enemy.animateEnemy.SetSummonAnimation(false);

        currentSylvarokPhase = SylvarokPhase.Wait;
        phaseTimer = 0;

        sylvarokRoutine = null;
    }

    // HEAL
    IEnumerator HealRoutine()
    {
        if (enemy.health.hasDied) yield break;

        enemy.debugDisplay.text = "Current Phase: HEAL";

        enemy.animateEnemy.SetHealAnimation(true);

        IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(gameObject);
        healthAuthority.ApplyDamage(-20, default);

        //SoundEffectManager.Instance.PlaySoundEffect(enemyDetails.chargeSoundEffect);

        yield return new WaitForSeconds(1.5f);

        enemy.animateEnemy.SetHealAnimation(false);

        currentSylvarokPhase = SylvarokPhase.Wait;
        phaseTimer = 0;

        sylvarokRoutine = null;
    }

    private void TransitionToNextPhase()
    {
        // Check if the player is on stealth
        if (targetPlayer != null && targetPlayer.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        if (targetPlayer != null && Vector3.Distance(transform.position, targetPlayer.GetPlayerPosition()) < 4f)
        {
            // If player is too close to treant, automatically next phase will be chargeAndRetreat or razorLeaf
            currentSylvarokPhase = (SylvarokPhase)Random.Range(2, 4);
            return;
        }

        if (currentSylvarokPhase == SylvarokPhase.Charge || currentSylvarokPhase == SylvarokPhase.RazorLeaf ||
            currentSylvarokPhase == SylvarokPhase.Heal || currentSylvarokPhase == SylvarokPhase.Summon)
        {
            // If treant made a move then next phase will be wait
            currentSylvarokPhase = SylvarokPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentSylvarokPhase = (SylvarokPhase)Random.Range(2, Enum.GetValues(typeof(SylvarokPhase)).Length);

            // If health is not low enough, switch heal phase
            if (currentSylvarokPhase == SylvarokPhase.Heal && enemy.health.GetCurrentHealth() > (int)(enemy.health.GetMaximumHealth() * 0.65f))
            {
                currentSylvarokPhase = (SylvarokPhase)Random.Range(2, Enum.GetValues(typeof(SylvarokPhase)).Length - 1);
            }
        }
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

        sylvarokRoutine = null;

        currentSylvarokPhase = SylvarokPhase.Wait;
        phaseTimer = 0f;
    }

    private void ResetAnimations()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetBossAnimationParameters();
    }

    public void PlayerStealthCheck()
    {
        currentSylvarokPhase = SylvarokPhase.Wait;
    }
}
