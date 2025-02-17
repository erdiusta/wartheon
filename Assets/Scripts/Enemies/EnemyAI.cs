using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;

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

    [HideInInspector] public TextMeshPro debugText;

    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public int updateFrameNumber = 1; // default value.  This is set by the enemy spawner
    [HideInInspector] public Coroutine chaseMoveEnemyRoutine;
    [HideInInspector] public Coroutine patrolMoveEnemyRoutine;
    [HideInInspector] public float enemyStartingSpeed;
    [HideInInspector] public EnemyPhase enemyPhase;

    protected Enemy enemy;
    protected Coroutine attackAnimationRoutine;
    protected Coroutine stunEnemyRoutine;
    protected Coroutine frostEnemyRoutine;
    protected Vector3 lockedVector;
    protected Room currentRoom;
    protected Stack<Vector3> movementSteps = new Stack<Vector3>();
    protected Stack<Vector3> patrolSteps = new Stack<Vector3>();
    protected float currentEnemyChasePathRebuildCooldown;
    protected float currentEnemyPatrolPathRebuildCooldown;
    protected float dashTimer;
    protected WaitForFixedUpdate waitForFixedUpdate;
    protected float attackMoveTimer;

    int currentPatrolIndex = 0;
    Vector3 referencePosition;
    GameObject selectedTargetEnemy;
    Vector3 knockbackVector;
    float knockbackForce;
    float knockbackTimeWeight;
    bool patrolPathFound;
    EnemyPhase enemyPhaseAtPreviousFrame;

    // PHYSICS
    [HideInInspector] public bool isAttacking;
    [HideInInspector] public bool isDashing;
    protected bool isTargetLocked;
    protected Vector3 lockedTargetPosition;

    // FIRING
    protected float firingIntervalTimer;
    protected float firingDurationTimer;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        moveSpeed = enemyDetails.movementDetails.GetBaseMoveSpeed();
        enemyStartingSpeed = enemyDetails.movementDetails.baseMoveSpeed;
    }

    protected virtual void OnEnable()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom();
    }

    protected virtual void Start()
    {
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

        if (isDashing)
        {
            // Move towards the locked target position
            enemy.movementToPosition.AttackMoveRigidbodyByPosition(lockedVector, moveSpeed * 2f);

            enemy.rb2D.mass = 4f;

            //// Check for collision with the player
            //if (IsCollidedWithPlayer())
            //{
            //    StopDashing(); // Stop dashing if colliding with player
            //}
        }
        else
        {
            enemy.rb2D.mass = 1f;
        }
    }

    protected virtual void Update()
    {
        if (isAttacking) return;

        attackMoveTimer -= Time.deltaTime;

        // Movement cooldown timer
        currentEnemyChasePathRebuildCooldown -= Time.deltaTime;
        currentEnemyPatrolPathRebuildCooldown -= Time.deltaTime;

        // If enemy hits wall, stop all coroutines
        Vector3Int enemyCellPosition = new Vector3Int(currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).x,
            currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).y);
        Vector3Int enemyZeroBasedCellPosition = new Vector3Int(enemyCellPosition.x - currentRoom.templateLowerBounds.x,
            enemyCellPosition.y - currentRoom.templateLowerBounds.y);

        // If enemy is in wall or pool tile, make enemy go away from there
        if (currentRoom.instantiatedRoom?.GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) == 0 || currentRoom.instantiatedRoom?.
            GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) > 1)
        {
            ClearPatrolPath();
            ClearChasePath();

            //if (enemyPhase != EnemyPhase.Attack)
            //{
            //    // For safe pathfind, set enemy phase to chase until enemy settles normal poisiton again
            //    enemy.animateEnemy.SetMovementAnimationParameters();
            //    enemyPhaseAtPreviousFrame = enemyPhase;
            //    enemyPhase = EnemyPhase.Chase;
            //}
        }

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
            ClearPatrolPath();
            ClearChasePath();
        }
        else
        {
            if (moveStatus == MoveStatus.Idle)
            {
                if (GameManager.Instance.GetPlayer() == null || GameManager.Instance.GetPlayer().isDead) return;

                // If enemy is attacking process, don't get involved in AStar calculations
                if (isAttacking || enemy.isFiring) return;

                Perform();

                if (GameManager.Instance.GetPlayer() == null) return;

                switch (enemyPhase)
                {
                    case EnemyPhase.Patrol:

                        //debugText.text = "PATROL";

                        // Reset animation and dashing flag
                        enemy.animateEnemy.ResetAnimatonParameters();

                        if (currentEnemyPatrolPathRebuildCooldown <= 0)
                        {
                            ClearChasePath();
                            Patrol();
                            currentEnemyPatrolPathRebuildCooldown = Settings.enemyPatrolPathRebuildCooldown; // Reset cooldown
                        }

                        break;

                    case EnemyPhase.Chase:

                        // Reset animation and dashing flag
                        enemy.animator.SetBool(Settings.isAttacking, false);

                        //debugText.text = "CHASE";

                        ClearPatrolPath();
                        Chase();

                        break;

                    case EnemyPhase.GetHit:

                        //debugText.text = "GETHIT";

                        //enemy.animateEnemy.ResetAnimatonParameters();
                        //enemy.animateEnemy.SetGetHitAnimationParameters();
                        //attackMoveEnemyRoutine = null;

                        break;

                    case EnemyPhase.Attack:

                        //debugText.text = "ATTACK";

                        if (enemy.enemyDetails.enemyBehaviour != EnemyBehaviour.AimAndShoot)
                        {
                            if (tag == Settings.enemyTag)
                            {
                                Vector3 direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() - transform.position).normalized :
                                    (GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position).normalized;
                                lockedVector = direction;
                            }
                        }

                        // Check if cooldown has expired
                        if (attackMoveTimer <= 0f)
                        {
                            // Trigger the attack if not already attacking
                            attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown; // Reset cooldown
                            attackAnimationRoutine = StartCoroutine(AttackAnimation());
                        }
                        else
                        {
                            // Cooldown is active, switch to Chase phase to avoid awkward waiting
                            enemyPhaseAtPreviousFrame = enemyPhase;
                            enemyPhase = EnemyPhase.Chase;
                        }

                        break;

                    default:
                        break;
                }
            }
        }
    }

    protected void Perform()
    {
        // Only process A Star path rebuild on certain frames to spread the load between enemies
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathfindingOver != updateFrameNumber) return;

        if (GameManager.Instance.GetPlayer() == null) return;

        if (tag == Settings.enemyTag)
        {
            // Check if there is any decoy(dummy)
            if (GameManager.Instance.GetDecoy() != null)
            {
                if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().rb2D.position) < enemy.enemyDetails.chaseDistance)
                {
                    enemyPhaseAtPreviousFrame = enemyPhase;
                    enemyPhase = EnemyPhase.Chase;
                }
            }
            else
            {
                // Check distance to player to see if enemy should start chasing
                if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().rb2D.position) < enemy.enemyDetails.chaseDistance)
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
            }
        }
        else if (tag == Settings.summonedEnemyTag)
        {
            Enemy[] allObjects = FindObjectsOfType<Enemy>();
            List<Enemy> nonSummonedEnemyList = new List<Enemy>();

            // Retrieve all non-summoned enemies in the room
            for (int i = 0; i < allObjects.Length; i++)
            {
                if (allObjects[i].tag == Settings.enemyTag)
                {
                    nonSummonedEnemyList.Add(allObjects[i]);
                }
            }

            selectedTargetEnemy = nonSummonedEnemyList.Count > 0 ? nonSummonedEnemyList[0].gameObject : null;

            referencePosition = nonSummonedEnemyList.Count > 0 ? selectedTargetEnemy.transform.position : GameManager.Instance.GetPlayer().GetPlayerPosition();

            // Chase enemy if it is in chase distance
            if (nonSummonedEnemyList.Count > 0)
            {
                // Check distance to player to see if enemy should start chasing
                if (Vector3.Distance(transform.position, nonSummonedEnemyList[0].transform.position) < enemy.enemyDetails.chaseDistance)
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
            }
            else
            {
                enemyPhaseAtPreviousFrame = enemyPhase;
                enemyPhase = EnemyPhase.Patrol;
            }
        }
    }

    /// <summary>
    /// Patrol 
    /// </summary>
    protected void Patrol()
    {
        // Check distance is too close to the player, reset patrol state
        if(GameManager.Instance.GetPlayer() != null)
        {
            if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
            {
                enemyPhaseAtPreviousFrame = enemyPhase;
                enemyPhase = EnemyPhase.Chase;
                return;
            }
        }

        // Reset path rebuild cooldown timer
        currentEnemyPatrolPathRebuildCooldown = Settings.enemyPatrolPathRebuildCooldown;

        // Clear chase stack
        if (movementSteps != null)
        {
            movementSteps.Clear();
        }

        if (!patrolPathFound)
        {
            // Move to the next patrol point for the next iteration randomly
            currentPatrolIndex = Random.Range(0, currentRoom.spawnPositionArray.Length);
        }

        // Create path for patrol
        CreatePath(currentPatrolIndex);
        PrintPathfindingPathForPatrol(); // It is for debugging

        // If a path has been found, move the enemy
        if (patrolSteps != null)
        {
            if (patrolMoveEnemyRoutine == null)
            {
                // Move enemy along the path using a coroutine
                patrolMoveEnemyRoutine = StartCoroutine(PatrolMoveEnemyRoutine());
            }
            else
            {
                StopCoroutine(patrolMoveEnemyRoutine);
                patrolMoveEnemyRoutine = StartCoroutine(PatrolMoveEnemyRoutine());
            }
        }
    }

    /// <summary>
    /// Chase the player
    /// </summary>
    protected void Chase()
    {
        patrolPathFound = false;

        if (tag == Settings.enemyTag)
        {
            // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
            Vector3 updatedTargetPosition = GameManager.Instance.GetDecoy() != null ? GameManager.Instance.GetDecoy().GetDecoyPosition() :
                GameManager.Instance.GetPlayer().rb2D.position;

            if (currentEnemyChasePathRebuildCooldown <= 0f || (Vector3.Distance(referencePosition, updatedTargetPosition) > Settings.playerMoveDistanceToRebuildPath))
            {
                // Reset path rebuild cooldown timer
                currentEnemyChasePathRebuildCooldown = Settings.enemyPatrolPathRebuildCooldown;

                // Clear patrol stack
                if (patrolSteps != null)
                {
                    patrolSteps.Clear();
                }

                // Reset player reference position
                referencePosition = GameManager.Instance.GetDecoy() == null ? GameManager.Instance.GetPlayer().GetPlayerPosition() : 
                    GameManager.Instance.GetDecoy().GetDecoyPosition();

                // Move the enemy using AStar pathfinding - Trigger rebuild of path to player
                CreatePath();
                PrintPathfindingPathForChase();

                if (movementSteps != null)
                {
                    // If a path has been found move the enemy
                    if (chaseMoveEnemyRoutine == null)
                    {
                        // Move enemy along the path using a coroutine
                        chaseMoveEnemyRoutine = StartCoroutine(ChaseMoveEnemyRoutine());
                    }
                }

                // Switch to attack if chase distance is lower than trigger distance
                if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.AimAndShoot || (enemy.enemyDetails.hasAttackMove && 
                    Vector3.Distance(referencePosition, transform.position) < enemy.enemyDetails.attackMoveTriggerDistance))
                {
                    if ((enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.AimAndShoot || attackMoveTimer <= 0f) && !GameManager.Instance.GetPlayer().onStealth)
                    {
                        enemyPhaseAtPreviousFrame = enemyPhase;
                        enemyPhase = EnemyPhase.Attack;
                        enemy.animateEnemy.SetAttackAnimationParameters();
                    }
                }
            }
            else if ((Vector3.Distance(transform.position, referencePosition) < enemy.enemyDetails.chaseDistance) && !GameManager.Instance.GetPlayer().onStealth)
            {
                enemyPhaseAtPreviousFrame = enemyPhase;
                enemyPhase = EnemyPhase.Chase;
                currentEnemyChasePathRebuildCooldown = -0.1f;
            }
            else
            {
                enemyPhaseAtPreviousFrame = enemyPhase;
                enemyPhase = EnemyPhase.Patrol;
                currentEnemyPatrolPathRebuildCooldown = -0.1f;
            }
        }
        else if (tag == Settings.summonedEnemyTag)
        {
            // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
            Vector3 updatedTargetPosition = selectedTargetEnemy != null ? selectedTargetEnemy.transform.position :
                GameManager.Instance.GetPlayer().GetPlayerPosition();

            if (currentEnemyChasePathRebuildCooldown <= 0f || (Vector3.Distance(referencePosition, updatedTargetPosition) > Settings.playerMoveDistanceToRebuildPath))
            {
                // Reset path rebuild cooldown timer
                currentEnemyChasePathRebuildCooldown = Settings.enemyPatrolPathRebuildCooldown;

                // Clear patrol stack
                if (patrolSteps != null)
                {
                    patrolSteps.Clear();
                }

                // Reset player reference position
                referencePosition = selectedTargetEnemy != null ? selectedTargetEnemy.transform.position : GameManager.Instance.GetPlayer().GetPlayerPosition();

                // Move the enemy using AStar pathfinding - Trigger rebuild of path to player
                CreatePath();

                if (movementSteps == null) return;

                // If a path has been found move the enemy
                if (chaseMoveEnemyRoutine == null)
                {
                    // Move enemy along the path using a coroutine
                    chaseMoveEnemyRoutine = StartCoroutine(ChaseMoveEnemyRoutine());
                }

                // Switch to attack if chase distance is lower than trigger distance
                if (enemy.enemyDetails.hasAttackMove && Vector3.Distance(updatedTargetPosition, transform.position) < enemy.enemyDetails.attackMoveTriggerDistance)
                {
                    enemyPhase = EnemyPhase.Attack;
                    enemy.animateEnemy.ResetAnimatonParameters();
                }
            }
        }
    }

    /// <summary>
    /// Use the AStar static class to create a path for the enemy - patrol
    /// </summary>
    private void CreatePath(int currentPatrolIndex)
    {
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Get enemy position on the grid
        Vector3Int enemyGridPosition = GetEnemyPositionWithinRoom(grid);

        Vector3Int currentPatrolledSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[currentPatrolIndex].x,
            currentRoom.spawnPositionArray[currentPatrolIndex].y, 0);

        // Build a path for the enemy to move on
        patrolSteps = AStar.BuildPath(currentRoom, enemyGridPosition, currentPatrolledSpawnPoint);

        // Take off first step on path - this is the grid square the enemy is already on
        if (patrolSteps != null)
        {
            patrolSteps.Pop();
        }

        patrolPathFound = true;
    }

    /// <summary>
    /// Use the AStar static class to create a path for the enemy - chase
    /// </summary>
    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instantiatedRoom.grid;

        // Get players position on the grid
        Vector3Int playerGridPosition = GetNearestNonObstacleTargetPosition(currentRoom);

        // Get enemy position on the grid
        Vector3Int enemyGridPosition = GetEnemyPositionWithinRoom(grid);

        // Build a path for the enemy to move on
        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        // Take off first step on path and the last step (which is the tile player is on) to stop a tile away
        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
    }

    /// <summary>
    /// Coroutine to move the enemy to the next location on the path - Patrol
    /// </summary>
    IEnumerator PatrolMoveEnemyRoutine()
    {
        while (patrolSteps != null && patrolSteps.Count > 0)
        {
            Vector3 nextPosition = patrolSteps.Pop();

            // Trigger movement and animations
            enemy.animateEnemy.ResetAnimatonParameters();

            // while not very close continue to move - when close move onto the next step
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                Vector2 unitVector = Vector3.Normalize(nextPosition - transform.position);

                // Initialize vectors, angles, directions and aim
                float unitAngle = HelperUtilities.GetAngleFromVector(unitVector);
                AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
                enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
                enemy.animateEnemy.ResetAimAnimationParameters();
                enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
                enemy.animateEnemy.SetMovementAnimationParameters();

                enemy.movementToPosition.MoveRigidbodyByPosition(unitVector, moveSpeed);

                // Moving the enemy using 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate;

                if (enemy.health.getHitCoroutine != null)
                {
                    nextPosition = transform.position;

                    if (patrolSteps != null)    
                    {
                        patrolSteps.Clear();
                    }
                }
            }

            yield return waitForFixedUpdate;
        }

        // End of path steps - trigger the enemy idle event
        IdleProcess();

        patrolMoveEnemyRoutine = null;
        patrolPathFound = false;
    }

    /// <summary>
    /// Coroutine to move the enemy to the next location on the path - Chase
    /// </summary>
    IEnumerator ChaseMoveEnemyRoutine()
    {
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            // Trigger movement and animations
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animateEnemy.SetMovementAnimationParameters();

            // while not very close continue to move - when close move onto the next step
            while (Vector3.Distance(nextPosition, transform.position) > 0.3f)
            {
                Vector2 unitVector = Vector3.Normalize(nextPosition - transform.position);

                // Initialize vectors, angles, directions and aim
                float unitAngle = HelperUtilities.GetAngleFromVector(unitVector);
                AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
                enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
                enemy.animateEnemy.ResetAimAnimationParameters();
                enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
                enemy.animateEnemy.SetMovementAnimationParameters();

                // Trigger movement and animations
                enemy.movementToPosition.MoveRigidbodyByPosition(unitVector, moveSpeed);

                // Moving the enemy using 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate;

                if (enemy.health.getHitCoroutine != null)
                {
                    nextPosition = transform.position;
                    if (movementSteps != null)
                    {
                        movementSteps.Clear();
                    }
                    chaseMoveEnemyRoutine = null;
                }
            }

            yield return waitForFixedUpdate;
        }

        // End of path steps - trigger the enemy idle event
        IdleProcess();

        chaseMoveEnemyRoutine = null;
    }

    protected void IdleProcess()
    {
        enemy.idle.StopVelocity();
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    IEnumerator AttackAnimation()
    {
        patrolPathFound = false;
        isAttacking = true;
        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(lockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
        enemy.animateEnemy.SetAttackAnimationParameters();
        enemy.idle.StopVelocity();

        ClearChasePath();
        ClearPatrolPath();

        // Wait until dash starts
        yield return new WaitForSeconds(enemyDetails.countdownDurationBeforeDashAttack);

        isDashing = true;
        dashTimer = 0f;

        // Calculate the locked target position if not already locked
        if (!isTargetLocked)
        {
            lockedTargetPosition = (Vector3)enemy.rb2D.position + lockedVector * enemyDetails.attackMoveEfficentDistance;
            isTargetLocked = true;
        }

        yield return waitForFixedUpdate;

        // Let FixedUpdate() handle movement during dashing, just wait for the dash duration to complete
        while (dashTimer <= 0.6f)
        {
            // Check for obstacles or invalid tiles, exit dash if needed
            Vector3Int enemyCellPosition = new Vector3Int(currentRoom.instantiatedRoom.grid.WorldToCell(enemy.rb2D.position).x,
                currentRoom.instantiatedRoom.grid.WorldToCell(enemy.rb2D.position).y);
            Vector3Int enemyZeroBasedCellPosition = new Vector3Int(enemyCellPosition.x - currentRoom.templateLowerBounds.x,
                enemyCellPosition.y - currentRoom.templateLowerBounds.y);

            if (currentRoom.instantiatedRoom.GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) != 1)
            {
                enemyPhase = EnemyPhase.Patrol;
                currentEnemyPatrolPathRebuildCooldown = -0.1f;
                isDashing = false;
                isAttacking = false;
                break;
            }

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
        currentEnemyPatrolPathRebuildCooldown = -0.1f;

        attackAnimationRoutine = null;
    }

    /// <summary>   
    /// Fire the weapon - laser
    /// </summary>
    protected void FireWeapon(Vector3 lockedPlayerVector, float lockedEnemyAngle, AimDirection lockedAimDirection, bool isLaser = false, 
        CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None, 
        SepharothPhase sepharothPhase = SepharothPhase.None)
    {
        Vector3 playerDirectionVector, weaponDirection;
        float weaponAngleDegrees, enemyAngleDegrees;
        AimDirection enemyAimDirection;

        if (isLaser)
        {
            playerDirectionVector = lockedPlayerVector;
            weaponDirection = lockedPlayerVector;
            weaponAngleDegrees = lockedEnemyAngle;
            enemyAngleDegrees = lockedEnemyAngle;
            enemyAimDirection = lockedAimDirection;

            // Trigger weapon aim methods
            enemy.aimWeapon.Aim(enemyAimDirection, enemyAngleDegrees);
            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimWeaponAnimationParameters(enemyAimDirection);

            enemy.fireWeaponEvent.CallFocusedAimEvent(lockedPlayerVector, lockedEnemyAngle);

            goto laserJump;
        }

        Aim(out playerDirectionVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection);

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

    /// <summary>   
    /// Fire the weapon - ordinary aim
    /// </summary>
    protected void FireWeapon(bool isLaser = false, CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None, 
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None)
    {
        Vector3 playerDirectionVector, weaponDirection;
        float weaponAngleDegrees, enemyAngleDegrees;
        AimDirection enemyAimDirection;

        Aim(out playerDirectionVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection);

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

    public void Aim(out Vector3 playerDirectionVector, out Vector3 weaponDirection, out float weaponAngleDegrees, out float enemyAngleDegrees,
        out AimDirection enemyAimDirection)
    {
        if (GameManager.Instance.GetPlayer().isDead || GameManager.Instance.GetPlayer() == null)
        {
            playerDirectionVector = Vector3.zero;
            weaponDirection = Vector3.zero;
            weaponAngleDegrees = 0f;
            enemyAngleDegrees = 0f;
            enemyAimDirection = 0;
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

        // Trigger weapon aim methods
        enemy.aimWeapon.Aim(enemyAimDirection, enemyAngleDegrees);
        enemy.animateEnemy.ResetAimAnimationParameters();
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
    /// Set the frame number that the enemy path will be recalculated on - to avoid performance spikes
    /// </summary>
    public void SetUpdateFrameNumber(int updateFrameNumber)
    {
        this.updateFrameNumber = updateFrameNumber;
    }

    /// <summary>
    /// Get the nearest position to the player that isn't on an obstacle
    /// </summary>
    private Vector3Int GetNearestNonObstacleTargetPosition(Room currentRoom)
    {
        Vector3 targetPosition = new Vector3();

        // If object is an enemy, firstly chase decoy, if not chase player
        if (tag == Settings.enemyTag)
        {
            if (GameManager.Instance.GetDecoy() != null)
            {
                targetPosition = GameManager.Instance.GetDecoy().GetDecoyPosition();
            }
            else
            {
                targetPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
            }
        }
        // If object is a summoned enemy by player, firstly chase selected enemy, if not roam around the player
        else if (tag == Settings.summonedEnemyTag)
        {
            if (selectedTargetEnemy != null)
            {
                targetPosition = selectedTargetEnemy.transform.position;
            }
            else
            {
                targetPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();
            }
        }

        Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(targetPosition);

        // Adjust the position to the room's lower bounds for indexing
        Vector2Int adjustedPlayerCellPosition = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x,
            playerCellPosition.y - currentRoom.templateLowerBounds.y);

        // Clamp the adjusted position within the room bounds
        adjustedPlayerCellPosition.x = Mathf.Clamp(adjustedPlayerCellPosition.x, 0, currentRoom.templateUpperBounds.x - currentRoom.templateLowerBounds.x);
        adjustedPlayerCellPosition.y = Mathf.Clamp(adjustedPlayerCellPosition.y, 0, currentRoom.templateUpperBounds.y - currentRoom.templateLowerBounds.y);

        // Now ensure the playerCellPosition is within bounds as well
        playerCellPosition.x = adjustedPlayerCellPosition.x + currentRoom.templateLowerBounds.x;
        playerCellPosition.y = adjustedPlayerCellPosition.y + currentRoom.templateLowerBounds.y;

        int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];

        // If the player isn't on a cell square marked as an obstacle then return that position
        if (obstacle != 0) return playerCellPosition;

        // Find a surrounding cell that isn't an obstacle - required because with the 'half collision' tiles the player can be on a grid
        // square that is marked as an obstacle
        else
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0) continue;

                    try
                    {
                        obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x + i, adjustedPlayerCellPosition.y + j];

                        if (obstacle != 0)
                            return new Vector3Int(playerCellPosition.x + i, playerCellPosition.y + j, 0);
                    }
                    catch
                    {
                        continue;
                    }
                }
            }

            // No non-obstacle cells surrounding the player so just return the player position
            return playerCellPosition;
        }
    }

    /// <summary>
    /// Get the enemy position inside the room bounds
    /// </summary>
    private Vector3Int GetEnemyPositionWithinRoom(Grid grid)
    {
        Vector3Int gridPosition = grid.WorldToCell(transform.position);

        // Adjust the position to the room's lower bounds for indexing
        Vector2Int adjustedPlayerCellPosition = new Vector2Int(gridPosition.x - currentRoom.templateLowerBounds.x,
            gridPosition.y - currentRoom.templateLowerBounds.y);

        // Clamp the adjusted position within the room bounds
        adjustedPlayerCellPosition.x = Mathf.Clamp(adjustedPlayerCellPosition.x, 0, currentRoom.templateUpperBounds.x - currentRoom.templateLowerBounds.x);
        adjustedPlayerCellPosition.y = Mathf.Clamp(adjustedPlayerCellPosition.y, 0, currentRoom.templateUpperBounds.y - currentRoom.templateLowerBounds.y);

        // Now ensure the playerCellPosition is within bounds as well
        gridPosition.x = adjustedPlayerCellPosition.x + currentRoom.templateLowerBounds.x;
        gridPosition.y = adjustedPlayerCellPosition.y + currentRoom.templateLowerBounds.y;

        return gridPosition;
    }

    public IEnumerator StunRoutine()
    {
        moveSpeed = 0f;
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForSeconds(3f);

        enemy.healthEvent.CallStunCuredEvent();
        enemy.animator.SetBool(Settings.isStunned, false);
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        moveSpeed = enemyDetails.movementDetails.GetBaseMoveSpeed();
        moveStatus = MoveStatus.Idle;
        stunEnemyRoutine = null;
    }

    public IEnumerator FrostRoutine()
    {
        moveSpeed = 0f;
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
        moveSpeed = enemyDetails.movementDetails.GetBaseMoveSpeed();
        moveStatus = MoveStatus.Idle;
        enemyPhase = EnemyPhase.Patrol;
        currentEnemyPatrolPathRebuildCooldown = -0.1f;
        frostEnemyRoutine = null;
    }

    public IEnumerator KnockbackRoutine()
    {
        enemy.rb2D.velocity = CalculateKnockback();

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

        moveSpeed = enemyDetails.movementDetails.GetBaseMoveSpeed();
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

        moveSpeed = 0f;
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

    protected void ClearChasePath()
    {
        if (chaseMoveEnemyRoutine != null)
        {
            StopCoroutine(chaseMoveEnemyRoutine);
            chaseMoveEnemyRoutine = null;
            movementSteps.Clear();
        }
    }

    protected void ClearPatrolPath()
    {
        if (patrolMoveEnemyRoutine != null)
        {
            StopCoroutine(patrolMoveEnemyRoutine);
            patrolMoveEnemyRoutine = null;
            patrolSteps.Clear();
        }
    }

    protected void PrintPathfindingPathForPatrol()
    {
        if (patrolSteps != null)
        {
            Vector3[] patrolStepArray = patrolSteps.ToArray();

            for (int i = 0; i < patrolStepArray.Length - 1; i++)
            {
                Debug.DrawLine(patrolStepArray[i], patrolStepArray[i + 1]);
            }
        }
    }

    protected void PrintPathfindingPathForChase()
    {
        if (movementSteps != null)
        {
            Vector3[] movementStepsArray = movementSteps.ToArray();

            for (int i = 0; i < movementStepsArray.Length - 1; i++)
            {
                Debug.DrawLine(movementStepsArray[i], movementStepsArray[i + 1], Color.red);
            }
        }
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
