using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyMovementAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("EnemyDetailsSO scriptable object")]
    #endregion
    public EnemyDetailsSO enemyDetails;

    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public int updateFrameNumber = 1; // default value.  This is set by the enemy spawner
    [HideInInspector] public bool chasePlayer = false;
    [HideInInspector] public Coroutine attackMoveEnemyRoutine;
    [HideInInspector] public Coroutine chaseMoveEnemyRoutine;
    [HideInInspector] public Coroutine patrolMoveEnemyRoutine;
    [HideInInspector] public float enemyStartingSpeed;
    [HideInInspector] public EnemyPhase enemyPhase;
    [HideInInspector] public UnityEvent OnReadyForDashAttack;

    Enemy enemy;
    Stack<Vector3> movementSteps = new Stack<Vector3>();
    Stack<Vector3> patrolSteps = new Stack<Vector3>();
    int currentPatrolIndex = 0;
    Vector3 referencePosition;
    Coroutine stunEnemyRoutine;
    Coroutine aimAndShootRoutine;
    Coroutine dashRoutine;
    GameObject selectedTargetEnemy;
    float attackMoveTimer;
    float dashTimer;
    float currentEnemyChasePathRebuildCooldown;
    float currentEnemyPatrolPathRebuildCooldown;
    WaitForFixedUpdate waitForFixedUpdate;
    Vector3 knockbackVector;
    float knockbackForce;
    float knockbackTimeWeight;
    Room currentRoom;
    Vector3 lockedVector;

    // PHYSICS
    bool isDashing;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
        enemyStartingSpeed = enemyDetails.movementDetails.moveSpeed;
    }

    private void OnEnable()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom();
        OnReadyForDashAttack.AddListener(CompleteDashAttackProcess);
    }

    private void OnDisable()
    {
        OnReadyForDashAttack.RemoveListener(CompleteDashAttackProcess);
    }

    public void CallReadyForDashAttack()
    {
        OnReadyForDashAttack?.Invoke();
    }

    private void Start()
    {
        // Create waitforfixed update for use in coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();

        // Reset player reference position
        referencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        // Reset attack move timer
        attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown;

        // Default enemy phase
        enemyPhase = EnemyPhase.Patrol;
    }

    private void FixedUpdate()
    {
        if (isDashing)
        {
            // Move towards the target position
            enemy.movementToPosition.AttackMoveRigidbodyByPosition(lockedVector, moveSpeed * 1.5f);
        }
    }

    private void Update()
    {
        attackMoveTimer -= Time.deltaTime;
        dashTimer += Time.deltaTime;

        // Movement cooldown timer
        currentEnemyChasePathRebuildCooldown -= Time.deltaTime;
        currentEnemyPatrolPathRebuildCooldown -= Time.deltaTime;

        // If enemy hits wall, stop all coroutines
        Vector3Int enemyCellPosition = new Vector3Int(currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).x,
            currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).y);
        Vector3Int enemyZeroBasedCellPosition = new Vector3Int(enemyCellPosition.x - currentRoom.templateLowerBounds.x,
            enemyCellPosition.y - currentRoom.templateLowerBounds.y);

        // If enemy is in wall or pool tile, make enemy go away from there
        if (currentRoom.instantiatedRoom.GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) == 0 || currentRoom.instantiatedRoom.
            GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) > 1)
        {
            if (chaseMoveEnemyRoutine != null)
            {
                StopCoroutine(chaseMoveEnemyRoutine);
                chaseMoveEnemyRoutine = null;
            }
            if (patrolMoveEnemyRoutine != null)
            {
                StopCoroutine(patrolMoveEnemyRoutine);
                patrolMoveEnemyRoutine = null;
            }
            if (enemyPhase != EnemyPhase.Attack)
            {
                // For safe pathfind, set enemy phase to chase until enemy settles normal poisiton again
                enemy.animateEnemy.SetMovementAnimationParameters();
                enemyPhase = EnemyPhase.Chase;
            }
        }

        // Second check if enemy is on stun status
        if (moveStatus == MoveStatus.Stun)
        {
            enemy.animateEnemy.SetIdleAnimationParameters();


            if (stunEnemyRoutine == null)
            {
                enemy.enemyWeaponAI.StopAllCoroutines();
                stunEnemyRoutine = StartCoroutine(StunRoutine());
            }
        }
        // Third check if enemy is on knockback status
        else if (moveStatus == MoveStatus.Stagger)
        {
            StartCoroutine(KnockbackRoutine());
        }

        // If enemy is at the time of other animations, don't move
        if (enemy.health.getHitCoroutine != null)
        {
            if (patrolMoveEnemyRoutine != null)
            {
                StopCoroutine(patrolMoveEnemyRoutine);
                patrolMoveEnemyRoutine = null;
                patrolSteps.Clear();
            }
            if (chaseMoveEnemyRoutine != null)
            {
                StopCoroutine(chaseMoveEnemyRoutine);
                chaseMoveEnemyRoutine = null;
                movementSteps.Clear();
            }
            if (attackMoveEnemyRoutine != null)
            {
                StopCoroutine(attackMoveEnemyRoutine);
                attackMoveEnemyRoutine = null;
            }
            if (chaseMoveEnemyRoutine != null)
            {
                StopCoroutine(chaseMoveEnemyRoutine);
                chaseMoveEnemyRoutine = null;
            }
        }
        else
        {
            if (moveStatus == MoveStatus.Idle)
            {
                Perform();

                switch (enemyPhase)
                {
                    case EnemyPhase.Patrol:


                        if (chaseMoveEnemyRoutine != null)
                        {
                            StopCoroutine(chaseMoveEnemyRoutine);
                            chaseMoveEnemyRoutine = null;
                            movementSteps.Clear();
                        }

                        if (currentEnemyPatrolPathRebuildCooldown <= 0f)
                        {
                            enemy.isFiring = false;
                            Patrol();
                        }

                        break;

                    case EnemyPhase.Chase:

                        if (patrolMoveEnemyRoutine != null)
                        {
                            StopCoroutine(patrolMoveEnemyRoutine);
                            patrolMoveEnemyRoutine = null;
                            patrolSteps.Clear();
                        }

                        Chase();

                        break;

                    case EnemyPhase.GetHit:

                        enemy.animateEnemy.ResetAnimatonParameters();
                        enemy.animateEnemy.SetGetHitAnimationParameters();

                        if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.PrepareAndDash)
                        {
                            enemy.animator.SetBool(Settings.isAttacking, false);
                        }

                        break;

                    case EnemyPhase.Attack:

                        enemy.animateEnemy.ResetAnimatonParameters();

                        if (attackMoveTimer > 0f) return;

                        if (tag == Settings.enemyTag)
                        {
                            Vector3 direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() - transform.position).normalized :
                                (GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position).normalized;
                            lockedVector = direction;
                        }
                        else if (tag == Settings.summonedEnemyTag)
                        {
                            Vector3 direction = selectedTargetEnemy != null ? (selectedTargetEnemy.transform.position - transform.position).normalized :
                                (GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position).normalized;
                        }

                        if (enemyDetails.enemyBehaviour == EnemyBehaviour.AimAndShoot)
                        {
                            if (enemy.isFiring)
                            {
                                StopAllCoroutines();

                                if (patrolSteps != null)
                                {
                                    patrolSteps.Clear();
                                }
                                if (movementSteps != null)
                                {
                                    movementSteps.Clear();
                                }
                                patrolMoveEnemyRoutine = null;
                                chaseMoveEnemyRoutine = null;
                                enemy.idle.StopVelocity();
                                enemy.animateEnemy.SetIdleAnimationParameters();
                                enemy.animator.SetBool(Settings.isAttacking, true);

                                if (aimAndShootRoutine == null)
                                {
                                    aimAndShootRoutine = StartCoroutine(AimAndShootRoutine());
                                }
                            }
                            // Only start the attack move routine if it's not already in progress
                            else
                            {
                                moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
                                enemyPhase = EnemyPhase.Chase;
                            }
                        }
                        else
                        {
                            if (attackMoveEnemyRoutine == null)
                            {
                                attackMoveEnemyRoutine = StartCoroutine(AttackMoveRoutine());
                            }
                            else
                            {
                                StopCoroutine(attackMoveEnemyRoutine);
                                attackMoveEnemyRoutine = StartCoroutine(AttackMoveRoutine());
                            }
                        }
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private void Perform()
    {
        // Only process A Star path rebuild on certain frames to spread the load between enemies
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathfindingOver != updateFrameNumber) return;

        if (tag == Settings.enemyTag)
        {
            // Check if there is any decoy(dummy)
            if (GameManager.Instance.GetDecoy() != null)
            {
                if (Vector3.Distance(transform.position, GameManager.Instance.GetDecoy().GetDecoyPosition()) < enemy.enemyDetails.chaseDistance)
                {
                    enemyPhase = EnemyPhase.Chase;
                }
            }
            else
            {
                // Check distance to player to see if enemy should start chasing
                if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
                {
                    if (!GameManager.Instance.GetPlayer().onStealth)
                    {
                        enemyPhase = EnemyPhase.Chase;
                    }
                    else
                    {
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
                        enemyPhase = EnemyPhase.Chase;
                    }
                    else
                    {
                        enemyPhase = EnemyPhase.Patrol;
                    }
                }
            }
            else
            {
                enemyPhase = EnemyPhase.Patrol;
            }
        }
    }

    /// <summary>
    /// Patrol 
    /// </summary>
    private void Patrol()
    {
        // Reset path rebuild cooldown timer
        currentEnemyPatrolPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

        // Clear chase stack
        if (movementSteps != null)
        {
            movementSteps.Clear();
        }

        // Reset the current patrol index to 0 if it exceeds the array length
        if (currentPatrolIndex >= currentRoom.spawnPositionArray.Length)
        {
            currentPatrolIndex = 0;
        }

        // Move to the next patrol point for the next iteration randomly
        currentPatrolIndex = Random.Range(0, currentRoom.spawnPositionArray.Length);

        // Create path for patrol
        CreatePath(currentPatrolIndex);

        // If a path has been found, move the enemy
        if (patrolSteps != null)
        {
            if (patrolMoveEnemyRoutine == null)
            {
                // Move enemy along the path using a coroutine
                patrolMoveEnemyRoutine = StartCoroutine(PatrolMoveEnemyRoutine());
            }
        }
    }

    /// <summary>
    /// Chase the player
    /// </summary>
    private void Chase()
    {
        if (tag == Settings.enemyTag)
        {
            // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
            Vector3 updatedTargetPosition = GameManager.Instance.GetDecoy() != null ? GameManager.Instance.GetDecoy().GetDecoyPosition() :
                GameManager.Instance.GetPlayer().GetPlayerPosition();

            if (currentEnemyChasePathRebuildCooldown <= 0f || (Vector3.Distance(referencePosition, updatedTargetPosition) > Settings.playerMoveDistanceToRebuildPath))
            {
                // Reset path rebuild cooldown timer
                currentEnemyChasePathRebuildCooldown = Settings.enemyPathRebuildCooldown;

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
        else if (tag == Settings.summonedEnemyTag)
        {
            // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
            Vector3 updatedTargetPosition = selectedTargetEnemy != null ? selectedTargetEnemy.transform.position :
                GameManager.Instance.GetPlayer().GetPlayerPosition();

            if (currentEnemyChasePathRebuildCooldown <= 0f || (Vector3.Distance(referencePosition, updatedTargetPosition) > Settings.playerMoveDistanceToRebuildPath))
            {
                // Reset path rebuild cooldown timer
                currentEnemyChasePathRebuildCooldown = Settings.enemyPathRebuildCooldown;

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
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        Vector3Int currentPatrolledSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[currentPatrolIndex].x,
            currentRoom.spawnPositionArray[currentPatrolIndex].y, 0);

        // Build a path for the enemy to move on
        patrolSteps = AStar.BuildPath(currentRoom, enemyGridPosition, currentPatrolledSpawnPoint);

        // Take off first step on path - this is the grid square the enemy is already on
        if (patrolSteps != null)
        {
            patrolSteps.Pop();
        }
        else
        {
            // End of path steps - trigger the enemy idle event
            IdleProcess();
        }
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
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        // Build a path for the enemy to move on
        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        // Take off first step on path - this is the grid square the enemy is already on
        if (movementSteps != null)
        {
            movementSteps.Pop();
        }
        else
        {
            // End of path steps - trigger the enemy idle event
            IdleProcess();
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

            // while not very close continue to move - when close move onto the next step
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                // Trigger movement and animations
                enemy.animateEnemy.ResetAnimatonParameters();
                enemy.movementToPosition.PatrolMoveRigidbodyByPosition(nextPosition, transform.position, moveSpeed);
                enemy.animateEnemy.SetMovementAnimationParameters();

                // Moving the enemy using 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate;

                if (enemy.enemyWeaponAI.enemyAttackCoroutine != null || enemy.health.getHitCoroutine != null || attackMoveEnemyRoutine != null)
                {
                    //nextPosition = transform.position;

                    if (patrolSteps != null)    
                    {
                        patrolSteps.Clear();
                    }
                    patrolMoveEnemyRoutine = null;
                }
            }

            yield return waitForFixedUpdate;
        }

        // End of path steps - trigger the enemy idle event
        IdleProcess();

        patrolMoveEnemyRoutine = null;
    }

    /// <summary>
    /// Coroutine to move the enemy to the next location on the path - Chase
    /// </summary>
    IEnumerator ChaseMoveEnemyRoutine()
    {
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            // while not very close continue to move - when close move onto the next step
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                // Trigger movement and animations
                enemy.movementToPosition.ChaseMoveRigidbodyByPosition(nextPosition, transform.position, moveSpeed);
                enemy.animateEnemy.SetMovementAnimationParameters();

                // Moving the enemy using 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate;

                if (enemy.enemyWeaponAI.enemyAttackCoroutine != null || enemy.health.getHitCoroutine != null || attackMoveEnemyRoutine != null)
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

    private void IdleProcess()
    {
        enemy.idle.StopVelocity();
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    /// <summary>
    /// Make special move - Attack phase 
    /// </summary>
    IEnumerator AttackMoveRoutine()
    {
        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(lockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);

        if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.PrepareAndDash)
        {
            // Prepare for the attack
            enemy.animateEnemy.SetAttackAnimationParameters();
            enemy.animator.SetBool(Settings.isAttacking, true);

            // Wait for the attack preparation animation to finish (handled via Unity Event)
            yield return new WaitUntil(() => dashRoutine != null);  // Wait until dashRoutine starts
        }

        attackMoveEnemyRoutine = null;
    }

    /// <summary>
    /// After receving event the preparition is completed, start to dash attack
    /// </summary>
    private void CompleteDashAttackProcess()
    {
        if (dashRoutine == null)
        {
            dashRoutine = StartCoroutine(DashRoutine());
        }
        else
        {
            StopCoroutine(dashRoutine);
            dashRoutine = StartCoroutine(DashRoutine());
        }
    }

    IEnumerator DashRoutine()
    {
        dashTimer = 0f;
        isDashing = true;

        // Attack animation attack phase
        enemy.animateEnemy.SetAttackAnimationParameters();

        // minDistance used to decide when to exit coroutine loop
        float minDistance = 0.2f;
        Vector3 targetPosition = transform.position + lockedVector * enemyDetails.attackMoveEfficentDistance;

        // Attack animation attack phase
        while (Vector3.Distance(transform.position, targetPosition) > minDistance && dashTimer <= 0.3f)
        {
            // Check if the current tile is an obstacle (penalty value = 0)
            Vector3Int enemyCellPosition = new Vector3Int(currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).x,
                currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).y);
            Vector3Int enemyZeroBasedCellPosition = new Vector3Int(enemyCellPosition.x - currentRoom.templateLowerBounds.x,
                enemyCellPosition.y - currentRoom.templateLowerBounds.y);

            // Break out of the loop if the stepped tile is not a preferred tile
            if (currentRoom.instantiatedRoom.GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) != 1)
            {
                // If the enemy collides with an obstacle or second-choice tile, transition to the appropriate phase
                enemyPhase = EnemyPhase.Patrol;
                attackMoveEnemyRoutine = null;
                break;
            }

            // yield and wait for fixed update
            yield return waitForFixedUpdate;
        }

        // Set cooldown timer
        attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown;

        // Check the previous phase and transition accordingly
        enemyPhase = EnemyPhase.Chase;

        // Reset the coroutine reference
        enemy.animator.SetBool(Settings.isAttacking, false);
        attackMoveEnemyRoutine = null;
        dashRoutine = null;

        yield return null;

        isDashing = false;
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

        Vector2Int adjustedPlayerCellPosition = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x,
            playerCellPosition.y - currentRoom.templateLowerBounds.y);

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

    IEnumerator StunRoutine()
    {
        moveSpeed = 0f;

        yield return new WaitForSeconds(3f);

        enemy.healthEvent.CallStunCuredEvent();
        enemy.animator.SetBool(Settings.isStunned, false);
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;

        // Reset stun status and allow other stun coroutines to be started
        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
        moveStatus = MoveStatus.Idle;
        stunEnemyRoutine = null;
    }

    IEnumerator KnockbackRoutine()
    {
        yield return waitForFixedUpdate;

        enemy.rb2D.velocity = CalculateKnockback();
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

        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
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

    IEnumerator AimAndShootRoutine()
    {
        moveSpeed = 0f;

        yield return new WaitForSeconds(enemy.enemyDetails.enemyWeapon.weaponCooldownDuration);

        aimAndShootRoutine = null;
        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
    }

    IEnumerator NullifySpeedForDeathRoutine()
    {
        yield return waitForFixedUpdate;

        moveSpeed = 0f;
        knockbackForce = 0f;
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
