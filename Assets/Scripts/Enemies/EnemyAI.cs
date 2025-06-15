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
    [SerializeField] LayerMask layerMask;

    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public int updateFrameNumber = 1; // default value.  This is set by the enemy spawner
    [HideInInspector] public EnemyPhase enemyPhase;

    protected Enemy enemy;
    protected Coroutine attackAnimationRoutine;
    protected Coroutine stunEnemyRoutine;
    protected Coroutine frostEnemyRoutine;
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

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        currentRoom = GameManager.Instance.GetCurrentRoom();

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

        // Create waitforfixed update for use in coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();

        // Reset player reference position
        if (GameManager.Instance.GetPlayer() != null)
        {
            referencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
        }

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

        // Second check if enemy is on frozen status
        if (moveStatus == MoveStatus.Frozen)
        {
            enemy.idle.StopVelocity();

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
        // Third check if enemy is on stun status
        else if (moveStatus == MoveStatus.Stun)
        {
            enemy.idle.StopVelocity();

            if (stunEnemyRoutine == null)
            {
                stunEnemyRoutine = StartCoroutine(StunRoutine());
            }
        }
        // Fourth check if enemy is on knockback status
        else if (moveStatus == MoveStatus.Stagger)
        {
            StartCoroutine(KnockbackRoutine());
        }

        // If enemy is at the time of other animations, don't move
        if (enemy.health.getHitCoroutine != null)
        {
            // STOP MOVE CODE !!!
        }
        else
        {
            if (moveStatus == MoveStatus.Idle)
            {
                if (player == null || player.health.hasDied) return;

                // If enemy is attacking process and in attack phase, don't get involved in AStar calculations
                if (enemyPhase == EnemyPhase.Attack && isAttacking) return;

                if (updatePhaseRoutine == null)
                {
                    updatePhaseRoutine = StartCoroutine(UpdatePhaseStatus());
                }

                switch (enemyPhase)
                {
                    case EnemyPhase.Patrol:
                        // Disable aiDestinationSetter and enable patrol
                        enemy.aiDestinationSetter.enabled = false;
                        enemy.patrol.enabled = true;

                        // Reset animation and dashing flag
                        ResetEnemySpeed();

                        enemy.animateEnemy.ResetAnimatonParameters();
                        enemy.animateEnemy.SetMovementAnimationParameters();

                        //ApplySeparation();
                        break;

                    case EnemyPhase.Chase:
                        // Disable patrol during chase and enable aiDestinationSetter
                        enemy.patrol.enabled = false;
                        enemy.aiDestinationSetter.enabled = true;

                        // Reset animation and dashing flag
                        ResetEnemySpeed();
                        enemy.animator.SetBool(Settings.isAttack, false);

                        enemy.animateEnemy.ResetAnimatonParameters();
                        enemy.animateEnemy.SetMovementAnimationParameters();

                        //ApplySeparation();
                        break;

                    case EnemyPhase.GetHit:
                        // Disable patrol during get hit and enable aiDestinationSetter
                        enemy.patrol.enabled = false;
                        enemy.aiDestinationSetter.enabled = false;
                        enemy.aiRigidbody2D.canMove = false; // Disable normal attack behaviour during dash or firing

                        enemy.idle.StopVelocity();

                        break;

                    case EnemyPhase.Attack:
                        // Disable patrol bot aiDestination setter for attack phase
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
    }

    protected virtual void Update() { }

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
    protected void FireWeapon(Vector3 lockedPlayerVector, float lockedEnemyAngle, AimDirection lockedAimDirection, AttackDirection lockedAttackDirection, bool isLaser = false, 
        CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None, 
        SepharothPhase sepharothPhase = SepharothPhase.None)
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
                    false, false, centaurPhase, treantPhase, galvanusPhase, sepharothPhase);
            }
        }
    }

    protected IEnumerator UpdatePhaseStatus(bool isAimAttackBehaviour = false)
    {
        if (isAimAttackBehaviour)
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
                    if (!player.onStealth)
                    {
                        // Switch to attack if chase distance is lower than trigger distance
                        if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetPlayer().GetPlayerPosition()) <
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
                            if (Vector3.Distance(enemy.GetEnemyPosition(), GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
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
                            if (attackMoveTimer <= 0f && !player.onStealth)
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
                            if (attackMoveTimer <= 0f && !player.onStealth)
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
                                if (!player.onStealth)
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

        yield return waitForFixedUpdate; // Wait a fixed update time after behaviour change.

        updatePhaseRoutine = null;
    }

    /// <summary>   
    /// Fire the weapon - ordinary aim
    /// </summary>
    protected void FireWeapon(bool isLaser = false, CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None, 
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
                // Does this enemy require line of sight to the player before firing?
                if (enemyDetails.firingLineOfSightRequired && !IsPlayerInLineOfSight(weaponDirection, enemyProjectileRange))
                {
                    enemy.enemyAI.enemyPhase = EnemyPhase.Chase;
                    return;
                }

                enemy.fireWeaponEvent.CallFireWeaponEvent(true, false, enemy, isLaser, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection,
                    false, false, false, centaurPhase, treantPhase, galvanusPhase, sepharothPhase, frostWrymPhase, venomancerPhase, fireWrymPhase, moldranPhase);
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

    private bool IsPlayerInLineOfSight(Vector3 weaponDirection, float enemyProjectileRange)
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(weaponShootPosition.position, (Vector2)weaponDirection, enemyProjectileRange, layerMask);

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
                if (!GameManager.Instance.GetPlayer().onStealth)
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
        moveStatus = MoveStatus.Idle;
        stunEnemyRoutine = null;
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
        moveStatus = MoveStatus.Idle;
        enemyPhase = EnemyPhase.Patrol;
        frostEnemyRoutine = null;
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
