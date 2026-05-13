using Mirror;
using Pathfinding;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyAINetwork : NetworkBehaviour
{
    #region Tooltip
    [Tooltip("EnemyDetailsSO scriptable object")]
    #endregion
    public EnemyDetailsSO enemyDetails;
    #region Tooltip
    [Tooltip("Populate this with the WeaponShootPosition child gameobject transform")]
    #endregion Tooltip
    public Transform weaponShootPosition;
    #region Tooltip
    [Tooltip("Select the layers that the enemy bullets will hit")]
    #endregion Tooltip
    public LayerMask shootLayerMask;
    #region Tooltip
    [Tooltip("Select the layers that the enemy will avoid static objects during avoid")]
    #endregion Tooltip
    public LayerMask avoidLayerMask;

    [HideInInspector] public int updateFrameNumber = 1; // default value.  This is set by the enemy spawner
    [HideInInspector] [SyncVar] public EnemyPhase enemyPhase;

    protected Enemy enemy;
    protected Coroutine attackAnimationRoutine;
    protected Coroutine stunEnemyRoutine;
    protected Coroutine paralyzeEnemyRoutine;
    protected Coroutine rootEnemyRoutine;
    protected Coroutine chillEnemyRoutine;
    protected Coroutine curseEnemyRoutine;
    protected Coroutine frostEnemyRoutine;
    protected Coroutine shatterEnemyRoutine;
    protected Coroutine slowEnemyRoutine;
    protected Coroutine fearEnemyRoutine;
    protected Vector2 trackingVector;
    protected Vector3 attackLockedVector;
    protected RoomNetData currentRoomNetData;
    protected Rigidbody2D rb2D;
    protected Vector2 prevVel;

    protected bool isCharging;
    protected float dashTimer;
    protected float healTimer;
    protected bool healTriggered;
    protected WaitForFixedUpdate waitForFixedUpdate;
    protected float attackMoveTimer;

    Vector3 referencePosition;
    GameObject selectedTargetEnemy;
    EnemyPhase enemyPhaseAtPreviousFrame;

    // PHYSICS
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isDashing;

    protected bool hasAppliedDashImpulse;
    protected const float dashSpeedMultiplier = 1.6f; // Or tweak based on testing

    protected bool isTargetLocked;
    protected Vector3 lockedTargetPosition;

    // FIRING
    protected float firingIntervalTimer;
    protected float firingDurationTimer;

    // PATROLLED POINTS
    [SerializeField] Transform patrolPointsParentContainer;
    protected Coroutine updatePhaseRoutine;

    protected Player targetPlayer;
    protected Dummy targetDecoy;
    protected bool deathAnimationStarted;

    protected bool isSlowTriggered;
    protected float speedReducer;

    // MULTIPLAYER
    protected bool isMultiplayer;
    protected bool enemyFullyInitialized;
    protected IEnemyCombatData enemyCombatData;
    protected IEnemyMovementData enemyMovementData;
    protected bool wasMoving = false;
    protected uint enemyNetId = 0;
    protected float targetRefreshTimer;

    protected EnemySpawnerNetwork spawnerNetwork;

    protected PendingProjectileRequest pendingProjectileRequest;
    protected bool hasPendingProjectile;

    protected Vector2 chargeStartPosition;
    protected Vector2 chargeMoveDirection;
    protected bool chargeRecoveryActive;
    protected Coroutine chargeRecoveryRoutine;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(gameObject);
        enemyMovementData = EnemyDataResolver.Resolve<IEnemyMovementData>(gameObject);
        rb2D = GetComponent<Rigidbody2D>();
        waitForFixedUpdate = new WaitForFixedUpdate();
    }

    protected virtual void OnEnable()
    {
        if (!isServer) return;
    }

    protected virtual void OnDisable()
    {
        PatrolRigidbody2D.OnRequestSpawnPositions -= Patrol_OnRequestSpawnPositions;
    }

    private Vector2Int[] Patrol_OnRequestSpawnPositions()
    {
        if (!isServer) return System.Array.Empty<Vector2Int>();

        if (EnemyRoomResolver.TryGetRoom(out _, out var spawnPositions, spawnerNetwork)) return spawnPositions;
        return System.Array.Empty<Vector2Int>();
    }

    protected virtual void Start() 
    {
        spawnerNetwork = FindFirstObjectByType<EnemySpawnerNetwork>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        isMultiplayer = true;

        // Cache player target reference
        targetPlayer = HelperUtilities.GetClosestPlayer(transform.position);

        enemy.currentMoveSpeed = enemyDetails.movementDetails.GetBaseMaxMoveSpeed();

        enemyNetId = enemy.enemyNetwork.netId;

        // Set enemy aiLerp speed dynamically based on related enemy's moveSpeed
        if (!enemy.Isboss)
        {
            if (targetPlayer != null) enemy.aiDestinationSetter.target = targetPlayer.transform;

            enemy.aiRigidbody2D.speed = enemy.currentMoveSpeed;
        }

        StartCoroutine(Server_WaitRoomAndInit());
    }

    IEnumerator Server_WaitRoomAndInit()
    {
        while (enemy.enemyNetwork.InstantiatedRoom == null) yield return null;

        // Safe to initialize
        enemy.patrol.targets = enemy.enemyNetwork.InstantiatedRoom.CreatePatrolTargets(enemy.enemyNetwork.SpawnPositions, patrolPointsParentContainer, true);

        PatrolRigidbody2D.OnRequestSpawnPositions += Patrol_OnRequestSpawnPositions;
        StartCoroutine(WaitForInitialization());
    }

    IEnumerator WaitForInitialization()
    {
        while (!enemy.initializationCompleted) yield return null;

        // Reset attack move timer
        attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown;

        // Default enemy phase
        enemyPhase = EnemyPhase.Patrol;
        enemyPhaseAtPreviousFrame = EnemyPhase.Patrol;

        enemyFullyInitialized = true;
    }

    protected virtual void Update()
    {
        if (!isServer || !enemyFullyInitialized) return;

        healTimer += Time.deltaTime;
        targetRefreshTimer += Time.deltaTime;

        if (targetRefreshTimer >= Settings.targetRefreshInterval)
        {
            targetPlayer = HelperUtilities.GetClosestPlayer(transform.position);
            targetRefreshTimer = 0;
        }
    }

    [ServerCallback]
    protected virtual void FixedUpdate()
    {
        if (!enemyFullyInitialized) return;

        dashTimer += Time.fixedDeltaTime;
        attackMoveTimer -= Time.fixedDeltaTime;

        if ((enemy.moveStatus & MoveStatus.KnockedBack) != 0) return;

        enemy.currentMoveSpeed = enemy.enemyDetails.movementDetails.GetBaseMaxMoveSpeed() - speedReducer;

        bool isMovingNow = rb2D.linearVelocity.sqrMagnitude > 0.01f;

        if (isMovingNow != wasMoving)
        {
            wasMoving = isMovingNow;
            enemy.enemyAnimSync.UpdateAnimationStateServer(wasMoving, enemy.LastAim);
        }

        if (isDashing)
        {
            if ((enemy.moveStatus & MoveStatus.KnockedBack) != 0)
            {
                CancelDash();
            }
            else
            {
                if (!hasAppliedDashImpulse)
                {
                    Vector2 force = attackLockedVector.normalized * enemy.currentMoveSpeed * dashSpeedMultiplier * rb2D.mass;
                    rb2D.AddForce(force, ForceMode2D.Impulse);
                    hasAppliedDashImpulse = true;
                }

                return;
            }
        }

        // AIM
        Vector3 unitVector = Vector3.zero; Vector3 weaponDirection; float weaponAngleDegrees; float enemyAngleDegrees;
        AimDirection enemyAimDirection; AttackDirection enemyAttackDirection;

        Aim(out unitVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);

        if (isAttacking) return;

        if (HasNegativeMoveStatusEffect()) return;
        else
        {
            SecondaryStatusEffectsCheck();
        }

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            // If enemy is attacking process and in attack phase, don't get involved in AStar calculations
            if (enemyPhase == EnemyPhase.Attack && isAttacking) return;

            UpdatePhaseStatus();

            switch (enemyPhase)
            {
                case EnemyPhase.Patrol:
                    // Disable aiDestinationSetter and enable patrol
                    enemy.aiRigidbody2D.enabled = true;
                    enemy.aiDestinationSetter.enabled = false;
                    enemy.patrol.enabled = true;

                    // Reset animation and dashing flag
                    ResetEnemySpeed();

                    enemy.enemyAnimSync?.UpdateAnimationStateServer(moving: true, enemy.LastAim);
                    break;

                case EnemyPhase.Chase:
                    // Disable patrol during chase and enable aiDestinationSetter
                    enemy.aiRigidbody2D.enabled = true;
                    enemy.patrol.enabled = false;
                    enemy.aiDestinationSetter.enabled = true;

                    // Reset animation and dashing flag
                    ResetEnemySpeed();

                    enemy.enemyAnimSync?.ResetAllAnimations();
                    enemy.enemyAnimSync?.UpdateAnimationStateServer(moving: true, enemy.LastAim);
                    break;

                case EnemyPhase.Attack:
                    // Disable patrol bot aiDestination setter for attack phase
                    enemy.aiRigidbody2D.enabled = true;
                    enemy.patrol.enabled = false;
                    enemy.aiDestinationSetter.enabled = false;
                    enemy.aiRigidbody2D.canMove = false; // Disable normal attack behaviour during dash or firing

                    if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.PrepareAndDash)
                    {
                        if (tag == Settings.enemyTag && !isAttacking) // isAttacking flag added for fixed locked vectort
                        {
                            Vector3 direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() -
                                enemy.GetEnemyPosition()).normalized : (targetPlayer.GetPlayerPosition() - enemy.GetEnemyPosition()).normalized;
                            attackLockedVector = direction;
                        }

                        // Check if cooldown has expired
                        if (attackMoveTimer <= 0f)
                        {
                            // Trigger the attack if not already attacking
                            attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown; // Reset cooldown

                            if (attackAnimationRoutine == null)
                            {
                                attackAnimationRoutine = StartCoroutine(DashProcess());
                            }
                        }
                        else
                        {
                            // Cooldown is active, switch to Chase phase to avoid awkward waiting
                            enemyPhaseAtPreviousFrame = enemyPhase;
                            enemyPhase = EnemyPhase.Chase;
                        }
                    }

                    break;

                case EnemyPhase.Death:
                    if (attackAnimationRoutine != null)
                    {
                        StopCoroutine(attackAnimationRoutine);
                    }

                    break;

                default:
                    break;
            }
        }
    }

    private void CancelDash()
    {
        isAttacking = false;
        isDashing = false;
        hasAppliedDashImpulse = false;

        // Re-enable enemy layer collisions and force interaction
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

        attackLockedVector = Vector2.zero;
        enemy.rb2D.linearVelocity = Vector2.zero;

        dashTimer = 0f;
        SwitchToPatrol();
    }

    protected void SecondaryStatusEffectsCheck()
    {
        if (enemy.isFeared)
        {
            if (fearEnemyRoutine == null)
            {
                if (attackAnimationRoutine != null)
                {
                    StopCoroutine(attackAnimationRoutine);
                    isAttacking = false;
                }

                fearEnemyRoutine = StartCoroutine(FearRoutine(enemy.fearDuration));
            }
        }
        if (enemy.isChilled)
        {
            if (chillEnemyRoutine == null)
            {
                chillEnemyRoutine = StartCoroutine(ChillRoutine(enemy.chillDuration));
            }
        }
        if (enemy.isShattered)
        {
            if (shatterEnemyRoutine == null)
            {
                shatterEnemyRoutine = StartCoroutine(ShatterRoutine(1f));
            }
        }
        if (enemy.isSlowed)
        {
            if (slowEnemyRoutine == null)
            {
                slowEnemyRoutine = StartCoroutine(SlowRoutine(enemy.slowDuration));
            }
        }
        if (enemy.isCursed)
        {
            if (curseEnemyRoutine == null)
            {
                curseEnemyRoutine = StartCoroutine(CurseRoutine(enemy.curseDuration));
            }
        }
    }

    protected void UpdatePhaseStatus(bool isAimAttackBehaviour = false)
    {
        if (!isServer) return;

        Enemy[] checkedEnemies = EnemyRegistry.ActiveEnemies.ToArray();

        // Ensure the aiDestinationSetter target is valid
        if (enemy.aiDestinationSetter.enabled)
        {
            if (enemy.aiDestinationSetter.target == null ||  enemy.aiDestinationSetter.target.gameObject == null)
            {
                // Fallback to player if exists
                if (targetPlayer != null && !targetPlayer.health.hasDied) enemy.aiDestinationSetter.target = targetPlayer.transform;
                else
                {
                    // Optional: fallback to first patrol point
                    if (enemy.patrol.targets != null && enemy.patrol.targets.Length > 0) enemy.aiDestinationSetter.target = enemy.patrol.targets[0];
                }
            }
        }

        if (enemy.isDisoriented)
        {
            SwitchToPatrol();
            return;
        }

        if(tag == Settings.summonedEnemyTag)
        {
            SummonedAllyEnemyBehaviour();
            return;
        }

        if (targetPlayer == null) return;

        float distanceToTarget = Vector3.Distance(enemy.GetEnemyPosition(), targetPlayer.transform.position);
        float chaseDistance = enemy.enemyDetails.chaseDistance;

        if (isAimAttackBehaviour)
        {
            bool isNexarion = enemy.enemyDetails.enemyCategory == EnemyCategory.Nexarion || enemy.enemyDetails.enemyCategory == EnemyCategory.IceNexarion ||
                enemy.enemyDetails.enemyCategory == EnemyCategory.NexarionLord;

            if (isNexarion)
            {
                int rng = 1;

                rng = HealCheck(rng);

                if (rng == 1) // Ranged Attack
                {
                    RangedAttackProcess(targetPlayer.transform, distanceToTarget, chaseDistance);
                }
                else if(rng == 2) // Heal
                {
                    foreach (Enemy checkedEnemy in checkedEnemies)
                    {
                        float healthRatio = (float)checkedEnemy.health.GetCurrentHealth() / (float)checkedEnemy.health.GetMaximumHealth();

                        if (healthRatio < 0.5f)
                        {
                            if (enemy.enemyDetails.enemyCategory == EnemyCategory.Nexarion || enemy.enemyDetails.enemyCategory == EnemyCategory.IceNexarion)
                            {
                                checkedEnemy.health.AddHealth(15);
                            }
                            else if (enemy.enemyDetails.enemyCategory == EnemyCategory.NexarionLord)
                            {
                                checkedEnemy.health.AddHealth(30);
                            }

                            checkedEnemy.healAnimator.SetTrigger("heal");
                            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healSoundEffect);
                            break;
                        }
                    }
                }
            }
            else
            {
                RangedAttackProcess(targetPlayer.transform, distanceToTarget, chaseDistance);
            }
        }
        else
        {
            if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Roaming)
            {
                enemyPhase = EnemyPhase.Patrol;
            }
            else if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Pursuit)
            {
                enemyPhase = EnemyPhase.Chase;
            }
            else // Dash & Attack
            {
                float attackMoveTriggerDistance = enemy.enemyDetails.attackMoveTriggerDistance;

                if (distanceToTarget < attackMoveTriggerDistance && attackMoveTimer <= 0f && !targetPlayer.isStealthActive)
                {
                    enemyPhaseAtPreviousFrame = enemyPhase;
                    enemyPhase = EnemyPhase.Attack;

                    // Sync
                    enemy.enemyAnimSync?.PlayAttackServer(enemy.LastAim);
                }
                else if (distanceToTarget < chaseDistance)
                {
                    if (!targetPlayer.isStealthActive) SwitchToChase();
                    else SwitchToPatrol();
                }
                else
                {
                    SwitchToPatrol();
                }
            }
        }
    }

    private int HealCheck(int rng)
    {
        if (healTimer < 5 && !healTriggered)
        {
            healTriggered = true;
            rng = Random.Range(1, 3);
        }
        else if (healTimer >= 5)
        {
            healTimer = 0;
            healTriggered = false;
        }

        return rng;
    }

    private void RangedAttackProcess(Transform target, float distanceToTarget, float chaseDistance)
    {
        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(enemy.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);

        float attackRange = weaponDetails.weaponCurrentProjectile.projectileSpeed * weaponDetails.weaponCurrentProjectile.lifeDuration;

        if (target.CompareTag(Settings.decoyTag))
        {
            attackRange = weaponDetails.weaponCurrentProjectile.projectileRange;
        }

        if (distanceToTarget < attackRange) SwitchToAttack();
        else if (distanceToTarget < chaseDistance) SwitchToChase();
        else SwitchToPatrol();
    }

    private void SwitchToAttack()
    {
        enemyPhaseAtPreviousFrame = enemyPhase;
        enemyPhase = EnemyPhase.Attack;

        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.PlayAttackServer(enemy.LastAim);

        enemy.idle.StopVelocity();
    }

    private void SwitchToChase()
    {
        enemyPhaseAtPreviousFrame = enemyPhase;
        enemyPhase = EnemyPhase.Chase;
    }

    private void SwitchToPatrol()
    {
        enemyPhaseAtPreviousFrame = enemyPhase;
        enemyPhase = EnemyPhase.Patrol;
    }

    protected void IdleProcess(AimDirection aimDirection)
    {
        enemy.idle.StopVelocity();

        enemy.enemyAnimSync?.ResetAllAnimations();
        enemy.enemyAnimSync?.UpdateAnimationStateServer(moving: false, aimDirection);
    }

    IEnumerator DashProcess()
    {
        isAttacking = true;
        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(attackLockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);

        // Sync
        if (unitAimDirection != enemy.LastAim)
        {
            enemy.LastAim = unitAimDirection;

            enemy.enemyAnimSync?.UpdateAnimationStateServer(wasMoving, unitAimDirection);
            enemy.enemyAnimSync?.PlayAttackServer(unitAimDirection);
        }

        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, enemy.EnemyCategory);
        enemy.idle.StopVelocity();

        // Calculate the locked target position if not already locked
        if (!isTargetLocked)
        {
            lockedTargetPosition = enemy.GetEnemyPosition() + attackLockedVector * enemyDetails.attackMoveEfficentDistance;
            isTargetLocked = true;
        }

        yield return waitForFixedUpdate; // Pass aim and dash lock phase

        // Wait until dash starts
        yield return new WaitForSeconds(enemyDetails.countdownDurationBeforeDashAttack);

        isDashing = true;
        hasAppliedDashImpulse = false;

        // Disable physical collision and force interaction with other enemies
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), true);

        dashTimer = 0f;

        enemy.aiRigidbody2D.canMove = true;
        enemy.aiRigidbody2D.speed = enemy.currentMoveSpeed * 2;

        yield return waitForFixedUpdate;

        // Let FixedUpdate() handle movement during dashing, just wait for the dash duration to complete
        while (dashTimer <= enemy.enemyDetails.attackMoveDashDuration)
        {
            //EnemyObstacleCheck(out obstacleFound);

            //if (obstacleFound) break;

            yield return waitForFixedUpdate;
        }

        // Set cooldown timer
        attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown;

        // Always reset the attacking state
        isAttacking = false;

        isDashing = false;
        isTargetLocked = false;

        // Re-enable enemy layer collisions and force interaction
        Physics2D.IgnoreLayerCollision(gameObject.layer, LayerMask.NameToLayer("Enemy"), false);

        // Reset flags after the dash is complete
        IdleProcess(unitAimDirection);
        enemyPhaseAtPreviousFrame = enemyPhase;
        enemyPhase = EnemyPhase.Patrol;

        attackAnimationRoutine = null;
    }

    /// <summary>   
    /// Fire the weapon - ordinary aim
    /// </summary>
    protected void FireWeapon(bool isLaser, ProjectileKind kind, AttackContext ctx)
    {
        Vector3 playerDirectionVector, weaponDirection;
        float weaponAngleDegrees, enemyAngleDegrees;
        AimDirection enemyAimDirection;
        AttackDirection enemyAttackDirection;

        if (ctx.useOverrideDirection)
        {
            weaponDirection = ctx.overrideDirection.normalized;

            playerDirectionVector = weaponDirection;
            weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);
            enemyAngleDegrees = weaponAngleDegrees;

            enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);
            enemyAttackDirection = HelperUtilities.GetAttackDirection(enemyAngleDegrees);
        }
        else
        {
            Aim(out playerDirectionVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);
        }

        // Only fire if enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            // Get projectile range
            float enemyProjectileRange = enemyDetails.enemyWeapon.weaponCurrentProjectile.projectileRange;

            // Is the player in range
            if (playerDirectionVector.magnitude <= enemyProjectileRange)
            {
                enemy.fireWeaponEvent.CallFireWeaponEvent(true, false, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection, isLaser, kind, ctx, enemyNetId, enemy);
            }
        }
    }

    /// <summary>   
    /// Fire the weapon - laser
    /// </summary>
    protected void FireWeapon(Vector3 lockedPlayerVector, float lockedEnemyAngle, AimDirection lockedAimDirection, AttackDirection lockedAttackDirection,
        bool isLaser, ProjectileKind kind, AttackContext ctx)
    {
        Vector3 playerDirectionVector, weaponDirection;
        float weaponAngleDegrees, enemyAngleDegrees;
        AimDirection enemyAimDirection;
        AttackDirection enemyAttackDirection;

        if (isLaser)
        {
            playerDirectionVector = lockedPlayerVector;
            weaponDirection = lockedPlayerVector;
            weaponAngleDegrees = lockedEnemyAngle;
            enemyAngleDegrees = lockedEnemyAngle;
            enemyAimDirection = lockedAimDirection;

            // Trigger weapon aim methods
            enemy.aimWeapon.Aim(enemyAimDirection, lockedAttackDirection, enemyAngleDegrees, enemy.EnemyCategory);

            // Sync
            if (enemyAimDirection != enemy.LastAim)
            {
                enemy.LastAim = enemyAimDirection;

                enemy.enemyAnimSync?.UpdateAnimationStateServer(wasMoving, enemyAimDirection);
            }

            enemy.fireWeaponEvent.CallFocusedAimEvent(lockedPlayerVector, lockedEnemyAngle);

            goto laserJump;
        }

        Aim(out playerDirectionVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);

        // Skip ordinary aim procedures for locked shots like laser
        laserJump:

        // Only fire if enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            // Get projectile range
            float enemyProjectileRange = enemyDetails.enemyWeapon.weaponCurrentProjectile.projectileRange;

            // Is the player in range
            if (playerDirectionVector.magnitude <= enemyProjectileRange)
            {
                // Does this enemy require line of sight to the player before firing?
                if (enemyDetails.firingLineOfSightRequired && !IsPlayerInLineOfSight(weaponDirection, enemyProjectileRange)) return;

                enemy.fireWeaponEvent.CallFireWeaponEvent(true, false, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection, isLaser, kind, ctx, enemyNetId, enemy);
            }
        }
    }

    public void Aim(out Vector3 playerDirectionVector, out Vector3 weaponDirection, out float weaponAngleDegrees, out float enemyAngleDegrees,
        out AimDirection enemyAimDirection, out AttackDirection enemyAttackDirection)
    {
        if (targetPlayer == null || targetPlayer.health.hasDied)
        {
            enemyPhase = EnemyPhase.Patrol;
            playerDirectionVector = Vector3.zero;
            weaponDirection = Vector3.zero;
            weaponAngleDegrees = 0f;
            enemyAngleDegrees = 0f;
            enemyAimDirection = 0;
            enemyAttackDirection = 0;
            return;
        }

        // Player distance
        playerDirectionVector = targetPlayer.GetPlayerPosition() - transform.position;

        // Calculate direction vector of player from weapon shoot position
        weaponDirection = targetPlayer.GetPlayerPosition() - weaponShootPosition.position;

        // Get weapon to player angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get enemy to player angle
        enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);

        // Set enemy aim direction
        enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);

        // Set enemy attack direction
        enemyAttackDirection = HelperUtilities.GetAttackDirection(enemyAngleDegrees);

        // Sync
        if (enemyAimDirection != enemy.LastAim)
        {
            enemy.LastAim = enemyAimDirection;

            enemy.enemyAnimSync?.UpdateAnimationStateServer(wasMoving, enemyAimDirection);
        }

        // Trigger weapon aim methods
        enemy.aimWeapon.Aim(enemyAimDirection, enemyAttackDirection, enemyAngleDegrees, enemy.enemyNetwork.EnemyCategory);
    }

    protected bool IsPlayerInLineOfSight(Vector3 weaponDirection, float enemyProjectileRange)
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(weaponShootPosition.position, (Vector2)weaponDirection, enemyProjectileRange, shootLayerMask);

        if (raycastHit2D && raycastHit2D.transform.CompareTag(Settings.playerTag))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Summoned ally mob's behaviour
    /// </summary>
    private void SummonedAllyEnemyBehaviour()
    {
        List<Enemy> nonSummonedEnemyList = new List<Enemy>();

        foreach (Transform child in GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform)
        {
            Enemy enemyObject = child.GetComponent<Enemy>();

            // Retrieve all non-summoned enemies in the room
            if (enemyObject != null && enemyObject.tag == Settings.enemyTag)
            {
                nonSummonedEnemyList.Add(enemyObject);
            }
        }

        float closestDistance = Mathf.Infinity;
        GameObject closestEnemy = null;
        Vector3 currentPosition = transform.position;

        foreach (Enemy enemy in nonSummonedEnemyList)
        {
            float distance = Vector3.Distance(currentPosition, enemy.GetEnemyPosition(enemy.Isboss));

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemy.gameObject;
            }
        }

        selectedTargetEnemy = closestEnemy;

        if (selectedTargetEnemy != null)
        {
            referencePosition = selectedTargetEnemy.transform.position;

            float distanceToTarget = Vector3.Distance(transform.position, referencePosition);

            if (distanceToTarget < enemy.enemyDetails.chaseDistance)
            {
                if (!GameManager.Instance.GetLocalPlayer().isStealthActive)
                {
                    enemyPhaseAtPreviousFrame = enemyPhase;
                    enemyPhase = EnemyPhase.Chase;
                }
                else
                {
                    enemyPhaseAtPreviousFrame = enemyPhase;
                    enemyPhase = EnemyPhase.Patrol;
                }
            }
            else
            {
                enemyPhaseAtPreviousFrame = enemyPhase;
                enemyPhase = EnemyPhase.Patrol;
            }
        }
        else
        {
            // No target found, fall back to player as reference position
            referencePosition = GameManager.Instance.GetLocalPlayer().GetPlayerPosition();
            enemyPhaseAtPreviousFrame = enemyPhase;
            enemyPhase = EnemyPhase.Patrol;
        }
    }

    #region CC Routines
    public IEnumerator StunRoutine()
    {
        enemy.idle.StopVelocity();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

        enemy.enemyNetwork.RpcPlayStunEffect(undo: false);

        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForSeconds(enemy.stunDuration);

        enemy.enemyNetwork.RpcPlayStunEffect(undo: true);

        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        ResetEnemySpeed();
        enemy.stunDuration = 2; // Reset to default value
        enemy.moveStatus &= ~MoveStatus.Stun;
        stunEnemyRoutine = null;
    }

    public IEnumerator ParalyzeRoutine()
    {
        enemy.idle.StopVelocity();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForSeconds(enemy.paralyzeDuration);

        enemy.healthEvent.CallParalyzeCuredEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        ResetEnemySpeed();
        enemy.paralyzeDuration = 2; // Reset to default value
        enemy.moveStatus &= ~MoveStatus.Paralyze;
        stunEnemyRoutine = null;
    }

    public IEnumerator ChillRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        enemy.isChilled = false;
        enemy.healthEvent.CallChillCuredEvent();
        chillEnemyRoutine = null;
    }

    public IEnumerator CurseRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        enemy.curseDuration = 4; // Reset to default value
        enemy.isCursed = false;
        enemy.healthEvent.CallCurseCuredEvent();
        curseEnemyRoutine = null;
    }

    public IEnumerator SlowRoutine(float duration)
    {
        if (!isSlowTriggered)
        {
            isSlowTriggered = true;
            speedReducer = enemy.currentMoveSpeed * 0.8f;
        }

        yield return new WaitForSeconds(duration);

        speedReducer = 0;
        isSlowTriggered = false;
        enemy.isSlowed = false;
        enemy.slowDuration = 2; // Reset to default value
        enemy.healthEvent.CallChillCuredEvent();
        chillEnemyRoutine = null;
    }

    public IEnumerator ShatterRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Reset All Cold Status
        enemy.moveStatus &= ~MoveStatus.Frozen;
        enemy.isChilled = false;
        enemy.isShattered = false;
        enemy.healthEvent.CallChillCuredEvent();
        enemy.healthEvent.CallFrostCuredEvent();
        enemy.healthEvent.CallShatterCuredEvent();

        shatterEnemyRoutine = null;
    }

    public IEnumerator RootRoutine()
    {
        enemy.idle.StopVelocity();

        // Notify All Clients
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.rootSoundEffect);

        enemy.enemyNetwork.RpcPlayRootEffect(undo: false);

        yield return new WaitForSeconds(enemy.rootDuration);

        enemy.rootDuration = 2; // Reset to default value

        enemy.enemyNetwork.RpcPlayRootEffect(undo: true);
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        ResetEnemySpeed();
        enemy.moveStatus &= ~MoveStatus.Root;
        rootEnemyRoutine = null;
    }

    public IEnumerator FrostRoutine()
    {
        enemy.idle.StopVelocity();
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.ResetAimAnimationParameters();

        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.healthEvent.CallGetFrostEvent();
        enemy.animator.SetBool(Settings.isFrozen, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForSeconds(enemy.freezeDuration);

        enemy.freezeDuration = 2; // Reset to default value

        enemy.healthEvent.CallFrostCuredEvent();

        enemy.animateEnemy.SetIdleAnimationParameters();

        enemy.animator.SetBool(Settings.isFrozen, false);
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        ResetEnemySpeed();
        // Remove Frozen
        enemy.moveStatus &= ~MoveStatus.Frozen;
        enemyPhase = EnemyPhase.Patrol;
        frostEnemyRoutine = null;
    }

    public IEnumerator FearRoutine(float duration)
    {
        float elapsed = 0f;

        enemy.aiRigidbody2D.enabled = false;
        enemy.aiDestinationSetter.enabled = false;
        enemy.patrol.enabled = false;

        while (elapsed < duration)
        {
            elapsed += Time.fixedDeltaTime;

            MoveAwayFromPlayer(); // Move away from player every frame
            yield return null;
        }

        enemy.fearDuration = 2; // Reset to default value
        enemy.aiRigidbody2D.enabled = true;
        enemy.aiDestinationSetter.enabled = true;
        enemy.patrol.enabled = true;

        enemy.isFeared = false;
        enemy.healthEvent.CallFearCuredEvent();
        fearEnemyRoutine = null;
    }

    public void MoveAwayFromPlayer()
    {
        if (!isServer) return;
        if (targetPlayer == null) return;

        Vector2 direction = (transform.position - targetPlayer.transform.position).normalized;
        float fearMoveSpeed = enemy.currentMoveSpeed * 0.5f;

        rb2D.MovePosition(rb2D.position + direction * fearMoveSpeed * Time.fixedDeltaTime);

        //transform.position += (Vector3)(direction * fearMoveSpeed * Time.deltaTime);
    }
    #endregion

    protected bool HasNegativeMoveStatusEffect()
    {
        bool negativeStatusEffect = false;

        // Frozen
        if ((enemy.moveStatus & MoveStatus.Frozen) != 0)
        {
            enemy.idle.StopVelocity();
            negativeStatusEffect = true;

            if (frostEnemyRoutine == null)
            {
                if (attackAnimationRoutine != null)
                {
                    StopCoroutine(attackAnimationRoutine);
                    isAttacking = false;
                }

                frostEnemyRoutine = StartCoroutine(FrostRoutine());
            }
        }

        // Stun
        if ((enemy.moveStatus & MoveStatus.Stun) != 0)
        {
            enemy.idle.StopVelocity();
            negativeStatusEffect = true;

            if (stunEnemyRoutine == null)
            {
                stunEnemyRoutine = StartCoroutine(StunRoutine());
            }
        }

        // Paralyze
        if ((enemy.moveStatus & MoveStatus.Paralyze) != 0)
        {
            enemy.idle.StopVelocity();
            negativeStatusEffect = true;

            if (paralyzeEnemyRoutine == null)
            {
                paralyzeEnemyRoutine = StartCoroutine(ParalyzeRoutine());
            }
        }

        // Root
        if ((enemy.moveStatus & MoveStatus.Root) != 0)
        {
            enemy.idle.StopVelocity();
            negativeStatusEffect = true;

            if (rootEnemyRoutine == null)
            {
                rootEnemyRoutine = StartCoroutine(RootRoutine());
            }
        }

        // Knocked Back
        if ((enemy.moveStatus & MoveStatus.KnockedBack) != 0)
        {
            negativeStatusEffect = true;
        }

        // If no negative effects were detected, revert to Idle
        if (!negativeStatusEffect)
        {
            enemy.moveStatus = MoveStatus.Idle;
        }

        return negativeStatusEffect;
    }

    public void ApplyKnockbackToPlayer(Player player, bool overrideDefaultKnockbackValue = false, float newKnockbackForce = 0f)
    {
        Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
        float knockbackForce = overrideDefaultKnockbackValue ? newKnockbackForce : 1f;
        float attackerMass = enemyCombatData.Isboss ? enemy.rb2D.mass /300 : enemy.rb2D.mass;

        player.movementByForce.ApplyKnockback(knockbackDir, knockbackForce, attackerMass);
    }

    /// <summary>
    /// Calculate a random weapon shoot duration between the min and max values
    /// </summary>
    protected float WeaponShootDuration()
    {
        // Calculate a random weapon shoot duration
        return Random.Range(enemyDetails.firingDurationMin, enemyDetails.firingDurationMax);
    }

    /// <summary>
    /// Calculate a random weapon shoot interval between the min and max values
    /// </summary>
    protected float WeaponShootInterval()
    {
        // Calculate a random weapon shoot interval
        return Random.Range(enemyDetails.firingIntervalMin, enemyDetails.firingIntervalMax);
    }

    protected Vector2 GetValidPosition(Vector2 desiredPosition, CapsuleCollider2D movementCollider)
    {
        if (movementCollider == null) return desiredPosition;

        Vector2 currentPos = transform.position;
        Vector2 dir = (desiredPosition - currentPos).normalized;

        float distance = Vector2.Distance(currentPos, desiredPosition); 

        // Too close
        if (distance <= 0.01f) return currentPos;

        ContactFilter2D filter = new ContactFilter2D();

        filter.useLayerMask = true;
        filter.useTriggers = false;

        filter.layerMask = GetObstacleMask();

        RaycastHit2D[] hits = new RaycastHit2D[16];

        int hitCount = movementCollider.Cast(dir, filter, hits, distance);
        bool validHitFound = false;

        float closestDistance = float.MaxValue;

        // FIND CLOSEST VALID OBSTACLE
        for (int i = 0; i < hitCount; i++)
        {
            Collider2D hitCol = hits[i].collider;

            if (hitCol == null) continue;

            // Door collision control
            if (hitCol.GetComponentInParent<Door>() != null) continue;

            // Ignore self
            if (hitCol.transform.root == transform.root) continue;

            // Ignore triggers
            if (hitCol.isTrigger) continue;

            // Ignore zero distance overlap artifacts
            if (hits[i].distance <= 0.03f) continue;

            if (hits[i].distance < closestDistance)
            {
                closestDistance = hits[i].distance;

                validHitFound = true;
            }
        }

        // VALID OBSTACLE FOUND
        if (validHitFound)
        {
            // Small safety padding
            float safeDistance = Mathf.Max(0, closestDistance - 0.05f);

            if (safeDistance < 1.25f)
            {
                safeDistance = 1.25f;

                return currentPos + dir * safeDistance;
            }
        }

        // PATH IS VALID
        return desiredPosition;
    }

    protected LayerMask GetObstacleMask() => LayerMask.GetMask("Wall", "Pool", "Environment");

    protected Vector3 ClampToBossRoom(Vector3 worldPosition, Vector2Int cellMin, Vector2Int cellMax)
    {
        Grid grid;

        if (isMultiplayer)
        {
            InstantiatedRoom ir = DungeonRuntime.GetInstantiatedRoom(GameSessionManager.Instance.GetCurrentRoomNetData().roomId);
            grid = ir.grid;
        }
        else
        {
            grid = GameManager.Instance.GetBossRoom().instantiatedRoom.grid;
        }

        Vector3Int cell = grid.WorldToCell(worldPosition);
        cell.x = Mathf.Clamp(cell.x, cellMin.x, cellMax.x);
        cell.y = Mathf.Clamp(cell.y, cellMin.y, cellMax.y);
        return grid.GetCellCenterWorld(cell);
    }

    protected bool IsOutsideBossRoom(Vector3 worldPosition, Vector2Int cellMin, Vector2Int cellMax, bool isMultiplayer)
    {
        Grid grid;

        if (isMultiplayer)
        {
            InstantiatedRoom ir = GameManager.Instance.FindBossRoom();
            grid = ir.grid;
        }
        else
        {
            grid = GameManager.Instance.GetBossRoom().instantiatedRoom.grid;
        }

        Vector3Int cell = grid.WorldToCell(worldPosition);
        return cell.x < cellMin.x || cell.x > cellMax.x || cell.y < cellMin.y || cell.y > cellMax.y;
    }

    public void ResetEnemySpeed()
    {
        enemy.currentMoveSpeed = enemyMovementData.MaxBaseMoveSpeed + enemy.additionalSpeedModifier - speedReducer;
        enemy.aiRigidbody2D.canMove = true;
        enemy.aiRigidbody2D.speed = enemy.currentMoveSpeed;
    }
}
