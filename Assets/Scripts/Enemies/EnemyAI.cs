using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Pathfinding;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyAI : MonoBehaviour
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

    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public int updateFrameNumber = 1; // default value.  This is set by the enemy spawner
    [HideInInspector] public EnemyPhase enemyPhase;

    protected Enemy enemy;
    protected Coroutine attackAnimationRoutine;
    protected Coroutine stunEnemyRoutine;
    protected Coroutine rootEnemyRoutine;
    protected Coroutine chillEnemyRoutine;
    protected Coroutine frostEnemyRoutine;
    protected Coroutine shatterEnemyRoutine;
    protected Coroutine slowEnemyRoutine;
    protected Coroutine fearEnemyRoutine;
    protected Vector3 lockedVector;
    protected Room currentRoom;

    protected float dashTimer;
    protected WaitForFixedUpdate waitForFixedUpdate;
    protected float attackMoveTimer;

    Vector3 referencePosition;
    GameObject selectedTargetEnemy;
    Vector3 knockbackVector;
    float knockbackForce;
    float knockbackTimeWeight;
    EnemyPhase enemyPhaseAtPreviousFrame;

    // PHYSICS
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isDashing;
    protected bool isTargetLocked;
    protected Vector3 lockedTargetPosition;

    // FIRING
    protected float firingIntervalTimer;
    protected float firingDurationTimer;

    // PATROLLED POINTS
    [SerializeField] Transform patrolPointsParentContainer;
    protected Coroutine updatePhaseRoutine;

    protected Player player;
    protected bool deathAnimationStarted;

    // AVOID
    protected Vector3 referenceIntentionVector = Vector3.zero;
    protected const float IntentionValue = 0.35f;
    protected float avoidSuppressionTimer = 0f;
    protected float avoidToPatrolTimer = 0f;

    // FLANK
    protected float flankDecisionTimer;
    protected const float FLANK_COOLDOWN = 1.5f; // Seconds between flank attempts

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        currentRoom = GameManager.Instance.GetCurrentRoom();

        waitForFixedUpdate = new WaitForFixedUpdate();

        // Cache player target reference
        player = GameManager.Instance.GetPlayer();
        enemy.currentMoveSpeed = enemyDetails.movementDetails.GetBaseMoveSpeed();

        // Set enemy aiLerp speed dynamically based on related enemy's moveSpeed
        if (!enemyDetails.isEnemyBoss)
        {
            enemy.aiRigidbody2D.speed = enemy.currentMoveSpeed;
        }
    }

    protected virtual void OnEnable()
    {
        // Patrol points array cache
        enemy.patrol.targets = currentRoom.GetPatrolTargets(currentRoom.spawnPositionArray, currentRoom.instantiatedRoom.grid, patrolPointsParentContainer);

        Patrol.OnRequestSpawnPositions += Patrol_OnRequestSpawnPositions;
    }

    protected virtual void OnDisable()
    {
        Patrol.OnRequestSpawnPositions -= Patrol_OnRequestSpawnPositions;
    }

    private Vector2Int[] Patrol_OnRequestSpawnPositions() => currentRoom.spawnPositionArray;

    protected virtual void Start()
    {
        if (player != null) enemy.aiDestinationSetter.target = player.transform;

        // Reset attack move timer
        attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown;

        // Default enemy phase
        enemyPhase = EnemyPhase.Patrol;
        enemyPhaseAtPreviousFrame = EnemyPhase.Patrol;
    }

    protected virtual void FixedUpdate()
    {
        dashTimer += Time.fixedDeltaTime;
        attackMoveTimer -= Time.fixedDeltaTime;

        avoidToPatrolTimer = Mathf.Max(0f, avoidToPatrolTimer - Time.fixedDeltaTime);
        avoidSuppressionTimer = Mathf.Max(0f, avoidSuppressionTimer - Time.fixedDeltaTime);

        if (isDashing)
        {
            // Move towards the locked target position
            enemy.movementToPosition.AttackMoveRigidbodyByPosition(lockedVector, enemy.currentMoveSpeed * 1.6f);
            return;
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

        if (moveStatus == MoveStatus.Idle)
        {
            if (player == null || player.health.hasDied) return;

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

                    enemy.animateEnemy.ResetAnimatonParameters();
                    enemy.animateEnemy.SetMovementAnimationParameters();
                    break;

                case EnemyPhase.Chase:
                    // Disable patrol during chase and enable aiDestinationSetter
                    enemy.aiRigidbody2D.enabled = true;
                    enemy.patrol.enabled = false;
                    enemy.aiDestinationSetter.enabled = true;

                    // Reset animation and dashing flag
                    ResetEnemySpeed();
                    enemy.animator.SetBool(Settings.isAttack, false);

                    enemy.animateEnemy.ResetAnimatonParameters();
                    enemy.animateEnemy.SetMovementAnimationParameters();
                    break;

                case EnemyPhase.Avoid:
                    // Disable patrol during chase and enable aiDestinationSetter
                    enemy.aiRigidbody2D.enabled = false;
                    enemy.patrol.enabled = false;
                    enemy.aiDestinationSetter.enabled = false;

                    // Reset animation and dashing flag
                    ResetEnemySpeed();

                    //Vector3 intentionVector = GetMovementIntention().normalized;
                    enemy.movementToPosition.AttackMoveRigidbodyByPosition(referenceIntentionVector.normalized, enemy.currentMoveSpeed);

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
                                enemy.GetEnemyPosition()).normalized : (GameManager.Instance.GetPlayer().GetPlayerPosition() - enemy.GetEnemyPosition()).normalized;
                            lockedVector = direction;
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

                fearEnemyRoutine = StartCoroutine(FearRoutine(5f));
            }
        }
        if (enemy.isChilled)
        {
            if (chillEnemyRoutine == null)
            {
                chillEnemyRoutine = StartCoroutine(ChillRoutine(5f));
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
                slowEnemyRoutine = StartCoroutine(SlowRoutine(3f));
            }
        }
    }

    protected virtual void Update() { }

    protected void UpdatePhaseStatus(bool isAimAttackBehaviour = false)
    {
        Vector3 intention = avoidToPatrolTimer > 0f ? Vector3.zero : GetMovementIntention(); // Bypass when patrol triggered during stuck in the walls

        // If neighbored enemies are close enough and not in attack motion, so avoid
        if (intention.sqrMagnitude > IntentionValue * IntentionValue && enemy.enemyDetails.enemyBehaviour != EnemyBehaviour.Roaming &&
            !isDashing && !isAttacking && !enemy.isFiring)
        {
            enemyPhase = EnemyPhase.Avoid;
            avoidSuppressionTimer = 0.6f;
            referenceIntentionVector = intention.normalized;
        }
        else if (avoidSuppressionTimer > 0f)
        {
            enemyPhase = EnemyPhase.Avoid;
        }
        else
        {
            referenceIntentionVector = Vector3.zero; // Reset reference intention vector

            if (isAimAttackBehaviour && enemyPhase != EnemyPhase.Flank)
            {
                // AIM & ATTACK BEHAVIOUR
                if (tag == Settings.enemyTag)
                {
                    // Check if there is any decoy(dummy)
                    if (GameManager.Instance.GetDecoy() != null)
                    {
                        // Switch to attack if chase distance is lower than trigger distance
                        if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetDecoy().GetDecoyPosition()) <
                            enemy.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileRange)
                        {
                            enemyPhase = EnemyPhase.Attack;

                            enemy.animateEnemy.ResetAnimatonParameters();
                            enemy.animateEnemy.SetMovementAnimationParameters();
                            enemy.idle.StopVelocity(); // Stop velocity during attack
                        }
                        else
                        {
                            // Switch to change if it in chase distance but not in trigger distance
                            if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetDecoy().GetDecoyPosition()) <
                                enemy.enemyDetails.chaseDistance)
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
                    }
                    else
                    {
                        if (!player.isStealthActive)
                        {
                            // Switch to attack if chase distance is lower than trigger distance
                            if (Vector3.Distance(enemy.GetEnemyPosition(), player.GetPlayerPosition()) <
                                enemy.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileSpeed *
                                enemy.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.lifeDuration)
                            {
                                enemyPhase = EnemyPhase.Attack;

                                enemy.animateEnemy.ResetAnimatonParameters();
                                enemy.animateEnemy.SetAttackAnimationParameters();
                                enemy.idle.StopVelocity(); // Stop velocity during attack
                            }
                            else
                            {
                                // Switch to change if it in chase distance but not in trigger distance
                                if (Vector3.Distance(enemy.GetEnemyPosition(), player.GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
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
                        }
                        else
                        {
                            enemyPhaseAtPreviousFrame = enemyPhase;
                            enemyPhase = EnemyPhase.Patrol;
                        }
                    }
                }
                else if (tag == Settings.summonedEnemyTag)
                {
                    SummonedAllyEnemyBehaviour();
                }
            }
            else
            {
                if (tag == Settings.enemyTag)
                {
                    if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Roaming)
                    {
                        enemyPhase = EnemyPhase.Patrol;
                    }
                    else if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Pursuit)
                    {
                        enemyPhase = EnemyPhase.Chase;
                    }
                    else
                    {
                        // DASH & ATTACK BEHAVIOUR
                        // Check if there is any decoy(dummy)
                        if (GameManager.Instance.GetDecoy() != null)
                        {
                            // Switch to attack if chase distance is lower than trigger distance
                            if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetDecoy().GetDecoyPosition()) <
                                enemy.enemyDetails.attackMoveTriggerDistance)
                            {
                                if (attackMoveTimer <= 0f && !player.isStealthActive)
                                {
                                    enemyPhaseAtPreviousFrame = enemyPhase;
                                    enemyPhase = EnemyPhase.Attack;
                                    enemy.animateEnemy.SetAttackAnimationParameters();
                                }
                            }
                            else
                            {
                                // Switch to change if it in chase distance but not in trigger distance
                                if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetDecoy().GetDecoyPosition()) < enemy.enemyDetails.chaseDistance)
                                {
                                    enemyPhaseAtPreviousFrame = enemyPhase;
                                    enemyPhase = EnemyPhase.Chase;
                                }
                            }
                        }
                        else
                        {
                            // Switch to attack if chase distance is lower than trigger distance
                            if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.attackMoveTriggerDistance)
                            {
                                if (attackMoveTimer <= 0f && !player.isStealthActive)
                                {
                                    enemyPhaseAtPreviousFrame = enemyPhase;
                                    enemyPhase = EnemyPhase.Attack;
                                    enemy.animateEnemy.SetAttackAnimationParameters();
                                }
                            }
                            else
                            {
                                // Switch to change if it in chase distance but not in trigger distance
                                if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
                                {
                                    if (!player.isStealthActive)
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
                        }
                    }
                }
                else if (tag == Settings.summonedEnemyTag)
                {
                    SummonedAllyEnemyBehaviour();
                }
            }
        }
    }

    public Vector3 GetMovementIntention()
    {
        // Initialize intention
        Vector3 intention = Vector3.zero;

        if (isDashing) return Vector3.zero; // During dash don't get involved in avoid calculation

        // Wall avoidance
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, 1f, avoidLayerMask);

        foreach (var hit in hits)
        {
            if (hit.gameObject == gameObject) continue;

            Vector3 direction = GetDirection(gameObject, hit.gameObject);
            float distance = GetDistance(gameObject, hit.gameObject);

            float springStrength = 1f / (1f + distance * distance * distance);
            intention -= direction * springStrength; // Push away from the walls or pools

            //if (springStrength > 0.3f)
            //{
            //    enemyPhase = EnemyPhase.Patrol; // Transition to patrol
            //    avoidToPatrolTimer = 1f;
            //    return Vector3.zero;
            //}
        }

        // Avoid obstacles
        foreach (GameObject obstacle in GameManager.Instance.GetCurrentRoom().instantiatedRoom.roomObstaclesList)
        {
            Vector3 direction = GetDirection(gameObject, obstacle.gameObject);
            float distance = GetDistance(gameObject, obstacle.gameObject);

            float springStrength = 1f / (1f + distance * distance * distance); // Inverse cube of distance
            intention -= direction * springStrength * 2; // Push away from the obstacle
        }

        // Spread out
        foreach (GameObject otherEnemy in SceneObjectsManager.Instance.dynamicGameObjectsInScene)
        {
            if (otherEnemy.TryGetComponent<Player>(out Player player)) continue; // Player check is done above, don't need to do it agian here

            if (otherEnemy.TryGetComponent(out Decoy decoy)) continue; // Decoy check

            if (otherEnemy.GetComponent<Enemy>().enemyAI.isDashing) continue; // Don't involve dashing enemies

            if (otherEnemy.GetComponent<Enemy>().isFiring) continue; // Don't involve firing enemies

            if (enemy.gameObject == otherEnemy) continue; // Don't repel yourself

            if (enemy.GetComponent<Enemy>().enemyDetails.enemyBehaviour == EnemyBehaviour.Roaming) continue;

            Vector3 direction = GetDirection(gameObject, otherEnemy);
            float distance = GetDistance(gameObject, otherEnemy);

            float springStrength = 1f / (1f + distance * distance * distance); // Inverse cube of distance

            intention -= direction * springStrength; // Push away from the other monsters

            if (springStrength > 0.3f)
            {
                Debug.Log(enemy.enemyDetails.enemyName + "'s spring strength vs " + otherEnemy.GetComponent<Enemy>().enemyDetails.enemyName
                    + " is " + "(" + intention.x + ", " + intention.y + ") with " + intention.sqrMagnitude + " sqr magnitude");
            }
        }

        // Check above cutoff threshold
        if (intention.sqrMagnitude < IntentionValue * IntentionValue) return Vector3.zero;

        // Return final movement intention
        return intention;
    }


    protected void IdleProcess()
    {
        enemy.idle.StopVelocity();
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    IEnumerator DashProcess()
    {
        isAttacking = true;
        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(lockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
        enemy.animateEnemy.SetAttackAnimationParameters();
        enemy.idle.StopVelocity();

        // Calculate the locked target position if not already locked
        if (!isTargetLocked)
        {
            lockedTargetPosition = enemy.GetEnemyPosition() + lockedVector * enemyDetails.attackMoveEfficentDistance;
            isTargetLocked = true;
        }

        yield return waitForFixedUpdate; // Pass aim and dash lock phase

        // Wait until dash starts
        yield return new WaitForSeconds(enemyDetails.countdownDurationBeforeDashAttack);

        isDashing = true;
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

        // Reset flags after the dash is complete
        IdleProcess();
        enemyPhaseAtPreviousFrame = enemyPhase;
        enemyPhase = EnemyPhase.Patrol;

        attackAnimationRoutine = null;
    }

    /// <summary>   
    /// Fire the weapon - laser
    /// </summary>
    protected void FireWeapon(Vector3 lockedPlayerVector, float lockedEnemyAngle, AimDirection lockedAimDirection, AttackDirection lockedAttackDirection, 
        bool isLaser = false, MoravellePhase moravellePhase = MoravellePhase.None, TreantPhase treantPhase = TreantPhase.None,
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None)
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
            enemy.aimWeapon.Aim(enemyAimDirection, lockedAttackDirection, enemyAngleDegrees);
            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimWeaponAnimationParameters(enemyAimDirection);

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

                enemy.fireWeaponEvent.CallFireWeaponEvent(true, false, enemy, isLaser, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection, false,
                    false, false, moravellePhase, treantPhase, galvanusPhase, sepharothPhase);
            }
        }
    }

    /// <summary>   
    /// Fire the weapon - ordinary aim
    /// </summary>
    protected void FireWeapon(bool isLaser = false, MoravellePhase moravellePhase = MoravellePhase.None, TreantPhase treantPhase = TreantPhase.None, 
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None, FrostWrymPhase frostWrymPhase = FrostWrymPhase.None,
        VenomancerPhase venomancerPhase = VenomancerPhase.None, FireWrymPhase fireWrymPhase = FireWrymPhase.None, MoldranPhase moldranPhase = MoldranPhase.None)
    {
        Vector3 playerDirectionVector, weaponDirection;
        float weaponAngleDegrees, enemyAngleDegrees;
        AimDirection enemyAimDirection;
        AttackDirection enemyAttackDirection;

        Aim(out playerDirectionVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);

        // Only fire if enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            // Get projectile range
            float enemyProjectileRange = enemyDetails.enemyWeapon.weaponCurrentProjectile.projectileRange;

            // Is the player in range
            if (playerDirectionVector.magnitude <= enemyProjectileRange)
            {
                // Does this enemy require line of sight to the player before firing
                if (enemyDetails.firingLineOfSightRequired)
                {
                    if (!IsPlayerInLineOfSight(weaponDirection, enemyProjectileRange))
                    {
                        // Attempt flank if cooldown allows
                        if (Time.time - flankDecisionTimer > FLANK_COOLDOWN)
                        {
                            flankDecisionTimer = Time.time;
                            enemyPhaseAtPreviousFrame = enemyPhase;
                            enemyPhase = EnemyPhase.Flank;
                        }

                        return;
                    }
                }

                enemy.fireWeaponEvent.CallFireWeaponEvent(true, false, enemy, isLaser, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection,
                    false, false, false, moravellePhase, treantPhase, galvanusPhase, sepharothPhase, frostWrymPhase, venomancerPhase, fireWrymPhase, moldranPhase);
            }
        }
    }

    public void Aim(out Vector3 playerDirectionVector, out Vector3 weaponDirection, out float weaponAngleDegrees, out float enemyAngleDegrees,
        out AimDirection enemyAimDirection, out AttackDirection enemyAttackDirection)
    {
        if (player == null || player.health.hasDied)
        {
            enemy.enemyAI.enemyPhase = EnemyPhase.Patrol;
            playerDirectionVector = Vector3.zero;
            weaponDirection = Vector3.zero;
            weaponAngleDegrees = 0f;
            enemyAngleDegrees = 0f;
            enemyAimDirection = 0;
            enemyAttackDirection = 0;
            return;
        }

        // Player distance
        playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;

        // Calculate direction vector of player from weapon shoot position
        weaponDirection = GameManager.Instance.GetPlayer().GetPlayerPosition() - weaponShootPosition.position;

        // Get weapon to player angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get enemy to player angle
        enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);

        // Set enemy aim direction
        enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);

        // Set enemy attack direction
        enemyAttackDirection = HelperUtilities.GetAttackDirection(enemyAngleDegrees);

        // Trigger weapon aim methods
        enemy.aimWeapon.Aim(enemyAimDirection, enemyAttackDirection, enemyAngleDegrees);
        enemy.animateEnemy.SetAimWeaponAnimationParameters(enemyAimDirection);
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
            float distance = Vector3.Distance(currentPosition, enemy.GetEnemyPosition(enemy.enemyDetails.isEnemyBoss));

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
                if (!GameManager.Instance.GetPlayer().isStealthActive)
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
            referencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
            enemyPhaseAtPreviousFrame = enemyPhase;
            enemyPhase = EnemyPhase.Patrol;
        }
    }

    public IEnumerator StunRoutine()
    {
        enemy.idle.StopVelocity();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForSeconds(3f);

        enemy.healthEvent.CallStunCuredEvent();
        enemy.animator.SetBool(Settings.isStunned, false);
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        ResetEnemySpeed();
        moveStatus &= ~MoveStatus.Root;
        stunEnemyRoutine = null;
    }

    public IEnumerator ChillRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        enemy.isChilled = false;
        enemy.healthEvent.CallChillCuredEvent();
        chillEnemyRoutine = null;
    }

    public IEnumerator SlowRoutine(float duration)
    {
        enemy.currentMoveSpeed = enemy.enemyDetails.movementDetails.GetBaseMoveSpeed() * 0.5f;

        yield return new WaitForSeconds(duration);

        enemy.currentMoveSpeed = enemy.enemyDetails.movementDetails.GetBaseMoveSpeed();
        enemy.isSlowed = false;
        enemy.healthEvent.CallChillCuredEvent();
        chillEnemyRoutine = null;
    }

    public IEnumerator ShatterRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        // Reset All Cold Status
        moveStatus &= ~MoveStatus.Frozen;
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
        enemy.rootAnimator.SetBool("root", true);
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.rootSoundEffect);

        yield return new WaitForSeconds(4f);

        enemy.rootAnimator.SetBool("root", false);
        enemy.healthEvent.CallRootCuredEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        ResetEnemySpeed();
        moveStatus &= ~MoveStatus.Root;
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

        yield return new WaitForSeconds(3f);

        enemy.healthEvent.CallFrostCuredEvent();
        enemy.animateEnemy.SetIdleAnimationParameters();
        enemy.animator.SetBool(Settings.isFrozen, false);
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        ResetEnemySpeed();
        // Remove Frozen
        moveStatus &= ~MoveStatus.Frozen;
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

        enemy.aiRigidbody2D.enabled = true;
        enemy.aiDestinationSetter.enabled = true;
        enemy.patrol.enabled = true;

        enemy.isFeared = false;
        enemy.healthEvent.CallFearCuredEvent();
        fearEnemyRoutine = null;
    }

    public void MoveAwayFromPlayer()
    {
        if (player == null) return;

        Vector2 direction = (transform.position - player.transform.position).normalized;
        float fearMoveSpeed = enemy.currentMoveSpeed * 0.5f;

        transform.position += (Vector3)(direction * fearMoveSpeed * Time.deltaTime);
    }

    public IEnumerator KnockbackRoutine()
    {
        enemy.rb2D.linearVelocity = CalculateKnockback();

        yield return waitForFixedUpdate;

    }

    public void TriggerKnockback(Vector3 vector)
    {
        if (moveStatus == MoveStatus.Idle)
        {
            StartCoroutine(Stagger(vector));
        }
    }

    IEnumerator Stagger(Vector3 vector)
    {
        moveStatus = MoveStatus.Stagger;

        knockbackVector = vector;
        knockbackForce = enemy.knockback.knockbackForce;
        knockbackTimeWeight = enemy.knockback.knockbackTimeWeight;

        yield return new WaitForSeconds(knockbackTimeWeight);

        ResetEnemySpeed();
        moveStatus = MoveStatus.Idle;
    }

    private Vector2 CalculateKnockback()
    {
        Vector2 updatedKnockbackVector = new Vector2();

        if (knockbackTimeWeight > 0f)
        {
            knockbackTimeWeight -= Time.deltaTime;
            updatedKnockbackVector = knockbackVector * knockbackForce * (knockbackTimeWeight > 0f ? knockbackTimeWeight : 0f);
        }
        else
        {
            NullifySpeedForDeathRoutine();
        }

        return updatedKnockbackVector;
    }

    IEnumerator NullifySpeedForDeathRoutine()
    {
        yield return waitForFixedUpdate;

        enemy.idle.StopVelocity();
        knockbackForce = 0f;
    }

    protected bool HasNegativeMoveStatusEffect()
    {
        bool negativeStatusEffect = false;

        // Frozen
        if ((moveStatus & MoveStatus.Frozen) != 0)
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
        if ((moveStatus & MoveStatus.Stun) != 0)
        {
            enemy.idle.StopVelocity();
            negativeStatusEffect = true;

            if (stunEnemyRoutine == null)
            {
                stunEnemyRoutine = StartCoroutine(StunRoutine());
            }
        }

        // Root
        if ((moveStatus & MoveStatus.Root) != 0)
        {
            enemy.idle.StopVelocity();
            negativeStatusEffect = true;

            if (rootEnemyRoutine == null)
            {
                rootEnemyRoutine = StartCoroutine(RootRoutine());
            }
        }

        // Stagger
        if ((moveStatus & MoveStatus.Stagger) != 0)
        {
            negativeStatusEffect = true;

            StartCoroutine(KnockbackRoutine());
        }

        // If no negative effects were detected, revert to Idle
        if (!negativeStatusEffect)
        {
            moveStatus = MoveStatus.Idle;
        }

        return negativeStatusEffect;
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

    private Vector3 GetDirection(GameObject currentObject, GameObject targetObject)
    {
        return ((targetObject.transform.position + new Vector3(0f, 0.7f, 0f)) - (currentObject.transform.position + new Vector3(0f, 0.7f, 0f))).normalized;
    }

    private float GetDistance(GameObject currentObject, GameObject targetObject)
    {
        return Vector3.Distance(targetObject.transform.position + new Vector3(0f, 0.7f, 0f), currentObject.transform.position + new Vector3(0f, 0.7f, 0f));
    }

    // Method to stop dashing
    private void StopDashing()
    {
        // Set dashing flag to false, you can also add any additional logic you want to run when stopping the dash
        isDashing = false;
    }

    public void ResetEnemySpeed()
    {
        enemy.currentMoveSpeed = enemyDetails.movementDetails.GetBaseMoveSpeed() + enemy.addionalSpeedModifier;
        enemy.aiRigidbody2D.canMove = true;
        enemy.aiRigidbody2D.speed = enemy.currentMoveSpeed;
    }


    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyDetails), enemyDetails);
    }
#endif
    #endregion Validation
}
