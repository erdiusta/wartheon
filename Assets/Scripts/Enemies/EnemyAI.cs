using System.Collections;
using System.Collections.Generic;
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

    public UnityEvent OnReadyForDashAttack;

    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public float moveSpeed;
    [HideInInspector] public int updateFrameNumber = 1; // default value.  This is set by the enemy spawner
    [HideInInspector] public Coroutine attackMoveEnemyRoutine;
    [HideInInspector] public Coroutine chaseMoveEnemyRoutine;
    [HideInInspector] public Coroutine patrolMoveEnemyRoutine;
    [HideInInspector] public float enemyStartingSpeed;
    [HideInInspector] public EnemyPhase enemyPhase;

    protected Enemy enemy;
    protected Coroutine attackAnimationRoutine;
    protected Coroutine dashRoutine;
    protected Coroutine stunEnemyRoutine;
    protected Vector3 lockedVector;
    protected Room currentRoom;
    protected Stack<Vector3> movementSteps = new Stack<Vector3>();
    protected Stack<Vector3> patrolSteps = new Stack<Vector3>();
    protected float currentEnemyChasePathRebuildCooldown;
    protected float currentEnemyPatrolPathRebuildCooldown;

    int currentPatrolIndex = 0;
    Vector3 referencePosition;
    GameObject selectedTargetEnemy;
    float attackMoveTimer;
    float dashTimer;
    WaitForFixedUpdate waitForFixedUpdate;
    Vector3 knockbackVector;
    float knockbackForce;
    float knockbackTimeWeight;
    bool patrolPathFound;
    EnemyPhase enemyPhaseAtPreviousFrame;

    // PHYSICS
    bool isDashing;
    bool isTargetLocked;
    Vector3 lockedTargetPosition;

    // FIRING
    protected float firingIntervalTimer;
    protected float firingDurationTimer;

    protected virtual void Awake()
    {
        enemy = GetComponent<Enemy>();
        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
        enemyStartingSpeed = enemyDetails.movementDetails.moveSpeed;
    }

    protected virtual void OnEnable()
    {
        currentRoom = GameManager.Instance.GetCurrentRoom();
        OnReadyForDashAttack.AddListener(CompleteDashAttackProcess);
    }

    protected virtual void OnDisable()
    {
        OnReadyForDashAttack.RemoveListener(CompleteDashAttackProcess);
    }

    public void CallReadyForDashAttack()
    {
        OnReadyForDashAttack?.Invoke();
    }

    protected virtual void Start()
    {
        // Create waitforfixed update for use in coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();

        // Reset player reference position
        referencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

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
            // Move towards the target position
            enemy.movementToPosition.AttackMoveRigidbodyByPosition(lockedVector, moveSpeed * 1.5f);
        }
    }

    protected virtual void Update()
    {
        attackMoveTimer -= Time.deltaTime;

        // Movement cooldown timer
        currentEnemyChasePathRebuildCooldown -= Time.deltaTime;
        currentEnemyPatrolPathRebuildCooldown -= Time.deltaTime;

        // If enemy hits wall, stop all coroutines
        Vector3Int enemyCellPosition = new Vector3Int(currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).x,
            currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).y);
        Vector3Int enemyZeroBasedCellPosition = new Vector3Int(enemyCellPosition.x - currentRoom.templateLowerBounds.x,
            enemyCellPosition.y - currentRoom.templateLowerBounds.y);

        //// PHASE CHANGE CHECK DEBUG
        //if (enemyPhase != enemyPhaseAtPreviousFrame && enemyPhase == EnemyPhase.Attack)
        //{
        //    Debug.Log(enemy.enemyDetails.enemyName + "'s " + enemyPhaseAtPreviousFrame.ToString() + " phase changed to " + enemyPhase.ToString());
        //}

        // If enemy is in wall or pool tile, make enemy go away from there
        if (currentRoom.instantiatedRoom?.GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) == 0 || currentRoom.instantiatedRoom?.
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
                enemyPhaseAtPreviousFrame = enemyPhase;
                enemyPhase = EnemyPhase.Chase;
            }
        }

        // Second check if enemy is on stun status
        if (moveStatus == MoveStatus.Stun)
        {
            IdleProcess();

            if (stunEnemyRoutine == null)
            {
                stunEnemyRoutine = StartCoroutine(StunRoutine());
            }
        }
        // Third check if enemy is on knockback status
        else if (moveStatus == MoveStatus.Stagger)
        {
            StartCoroutine(KnockbackRoutine());
        }

        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.deltaTime;

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

                        Patrol();

                        break;

                    case EnemyPhase.Chase:

                        if (patrolMoveEnemyRoutine != null)
                        {
                            StopCoroutine(patrolMoveEnemyRoutine);
                            patrolMoveEnemyRoutine = null;
                            patrolSteps.Clear();
                        }

                        if (currentEnemyChasePathRebuildCooldown <= 0f)
                        {

                            Chase();
                        }

                        break;

                    case EnemyPhase.GetHit:

                        enemy.animateEnemy.ResetAnimatonParameters();
                        enemy.animateEnemy.SetGetHitAnimationParameters();
                        attackMoveEnemyRoutine = null;

                        if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.PrepareAndDash)
                        {
                            enemy.animator.SetBool(Settings.isAttacking, false);
                        }

                        break;

                    case EnemyPhase.Attack:

                        enemy.animateEnemy.SetAttackAnimationParameters();

                        if (attackMoveTimer > 0f) return; // Prevent executing if still in attack timer

                        if (enemy.enemyDetails.enemyBehaviour != EnemyBehaviour.AimAndShoot)
                        {
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
                        }

                        if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.PrepareAndDash)
                        {
                            if (dashRoutine == null)
                            {
                                attackMoveEnemyRoutine = StartCoroutine(AttackMoveRoutine());
                            }
                        }           
                        else
                        {
                            if (attackMoveEnemyRoutine == null)
                            {
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

    protected void Perform()
    {
        // Only process A Star path rebuild on certain frames to spread the load between enemies
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathfindingOver != updateFrameNumber) return;

        if (dashRoutine != null) return;

        if (tag == Settings.enemyTag)
        {
            // Check if there is any decoy(dummy)
            if (GameManager.Instance.GetDecoy() != null)
            {
                if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
                {
                    enemyPhaseAtPreviousFrame = enemyPhase;
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
                        if (enemyPhase != EnemyPhase.Attack)
                        {
                            enemyPhaseAtPreviousFrame = enemyPhase;
                            enemyPhase = EnemyPhase.Patrol;
                            Patrol();
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
        attackMoveEnemyRoutine = null;

        // Check distance is too close to the player, reset patrol state
        if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) < enemy.enemyDetails.chaseDistance)
        {
            enemyPhaseAtPreviousFrame = enemyPhase;
            enemyPhase = EnemyPhase.Chase;
            return;
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

        //// Check distance is too far away from player, reset patrol state
        //if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) >= enemy.enemyDetails.chaseDistance)
        //{
        //    enemyPhaseAtPreviousFrame = enemyPhase;
        //    enemyPhase = EnemyPhase.Patrol;
        //    Patrol();
        //    return;
        //}

        if (tag == Settings.enemyTag)
        {
            // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
            Vector3 updatedTargetPosition = GameManager.Instance.GetDecoy() != null ? GameManager.Instance.GetDecoy().GetDecoyPosition() :
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
                referencePosition = GameManager.Instance.GetDecoy() == null ? GameManager.Instance.GetPlayer().GetPlayerPosition() : 
                    GameManager.Instance.GetDecoy().GetDecoyPosition();

                // Move the enemy using AStar pathfinding - Trigger rebuild of path to player
                CreatePath();

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
                if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.AimAndShoot || (enemy.enemyDetails.hasAttackMove && Vector3.Distance(updatedTargetPosition, transform.position) < 
                    enemy.enemyDetails.attackMoveTriggerDistance))
                {
                    enemyPhaseAtPreviousFrame = enemyPhase;
                    enemyPhase = EnemyPhase.Attack;
                    //enemy.animateEnemy.ResetAnimatonParameters();
                }
            }
            else if ((Vector3.Distance(transform.position, referencePosition) < enemy.enemyDetails.chaseDistance))
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
        else if (tag == Settings.summonedEnemyTag)
        {
            // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
            Vector3 updatedTargetPosition = selectedTargetEnemy != null ? selectedTargetEnemy.transform.position :
                GameManager.Instance.GetPlayer().GetPlayerPosition();

            if (Vector3.Distance(referencePosition, updatedTargetPosition) > Settings.playerMoveDistanceToRebuildPath)
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
        Vector3Int enemyGridPosition = grid.WorldToCell(transform.position);

        // Build a path for the enemy to move on
        movementSteps = AStar.BuildPath(currentRoom, enemyGridPosition, playerGridPosition);

        // Take off first step on path - this is the grid square the enemy is already on
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
            enemy.animateEnemy.SetMovementAnimationParameters();

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

                enemy.movementToPosition.MoveRigidbodyByPosition(unitVector, moveSpeed);

                // Moving the enemy using 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate;

                if (enemy.health.getHitCoroutine != null || attackMoveEnemyRoutine != null)
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
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                Vector2 unitVector = Vector3.Normalize(nextPosition - transform.position);

                // Initialize vectors, angles, directions and aim
                float unitAngle = HelperUtilities.GetAngleFromVector(unitVector);
                AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
                enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
                enemy.animateEnemy.ResetAimAnimationParameters();
                enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);

                // Trigger movement and animations
                enemy.movementToPosition.MoveRigidbodyByPosition(unitVector, moveSpeed);

                // Moving the enemy using 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate;

                if (enemy.health.getHitCoroutine != null || attackMoveEnemyRoutine != null)
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

    /// <summary>
    /// Make special move - Attack phase 
    /// </summary>
    IEnumerator AttackMoveRoutine()
    {
        patrolPathFound = false;

        // Skip aim and shoot because, there is more detail for aim and shoot behaviour below
        if (enemy.enemyDetails.enemyBehaviour != EnemyBehaviour.AimAndShoot)
        {
            // Initialize vectors, angles, directions and aim
            float unitAngle = HelperUtilities.GetAngleFromVector(lockedVector);
            AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
            enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
            enemy.animateEnemy.SetAttackAnimationParameters();
        }

        if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.PrepareAndDash)
        {
            if (attackAnimationRoutine == null)
            {
                if (dashRoutine == null)
                {
                    attackAnimationRoutine = StartCoroutine(AttackAnimation());
                }
            }

            attackMoveEnemyRoutine = null;

            // Wait for the attack preparation animation to finish (handled via Unity Event)
            yield return new WaitUntil(() => dashRoutine != null);  // Wait until dashRoutine starts
        }
    }

    IEnumerator AttackAnimation()
    {
        // Set the animator's isAttacking parameter to true to start the attack animation
        enemy.animateEnemy.SetAttackAnimationParameters();
        enemy.animator.SetBool(Settings.isAttacking, true);
        enemy.rb2D.velocity = Vector2.zero;

        // Ensure correct animation state
        yield return new WaitForEndOfFrame();

        // Wait until the attack animation ends
        AnimatorStateInfo stateInfo = enemy.animator.GetCurrentAnimatorStateInfo(enemy.animateEnemy.attackLayerIndex);
        float animationDuration = stateInfo.length; // Get the length of the current animation

        float durationOffset = 1f;
        yield return new WaitForSeconds(animationDuration + durationOffset);

        // Set the animator's isAttacking parameter to true to start the attack animation
        attackAnimationRoutine = null;
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
    }

    IEnumerator DashRoutine()
    {
        dashTimer = 0f;
        isDashing = true;

        yield return null;

        // minDistance used to decide when to exit coroutine loop
        float minDistance = 0.1f;

        if (!isTargetLocked)
        {
            lockedTargetPosition = transform.position + lockedVector * enemyDetails.attackMoveEfficentDistance;
            isTargetLocked = true;
        }

        // Attack animation attack phase
        while (Vector3.Distance(transform.position, lockedTargetPosition) > minDistance && dashTimer <= 0.4f)
        {
            //// Check if the current tile is an obstacle (penalty value = 0)
            //Vector3Int enemyCellPosition = new Vector3Int(currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).x,
            //    currentRoom.instantiatedRoom.grid.WorldToCell(transform.position).y);
            //Vector3Int enemyZeroBasedCellPosition = new Vector3Int(enemyCellPosition.x - currentRoom.templateLowerBounds.x,
            //    enemyCellPosition.y - currentRoom.templateLowerBounds.y);

            //// Break out of the loop if the stepped tile is not a preferred tile
            //if (currentRoom.instantiatedRoom.GetRoomTilePenaltyValue(enemyZeroBasedCellPosition) != 1)
            //{
            //    // If the enemy collides with an obstacle or second-choice tile, transition to the appropriate phase
            //    enemyPhase = EnemyPhase.Chase;
            //    attackMoveEnemyRoutine = null;
            //    isDashing = false;
            //    break;
            //}

            // yield and wait for fixed update
            yield return waitForFixedUpdate;
        }

        // Set cooldown timer
        attackMoveTimer = enemy.enemyDetails.attackMoveBaseCooldown;

        yield return waitForFixedUpdate;

        // Check the previous phase and transition accordingly
        enemyPhaseAtPreviousFrame = enemyPhase;
        enemyPhase = EnemyPhase.Patrol;

        // Reset the coroutine reference
        enemy.animator.SetBool(Settings.isAttacking, false);

        isDashing = false;
        isTargetLocked = false;
        attackMoveEnemyRoutine = null;
        dashRoutine = null;
    }

    /// <summary>   
    /// Fire the weapon
    /// </summary>
    protected void FireWeapon(CentaurPhase centaurPhase = CentaurPhase.None)
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

                enemy.fireWeaponEvent.CallFireWeaponEvent(true, false, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection, false,
                    false, false, centaurPhase);
            }
        }
    }

    public void Aim(out Vector3 playerDirectionVector, out Vector3 weaponDirection, out float weaponAngleDegrees, out float enemyAngleDegrees,
        out AimDirection enemyAimDirection)
    {
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

    public IEnumerator StunRoutine()
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

    public IEnumerator KnockbackRoutine()
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

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyDetails), enemyDetails);
    }
#endif
    #endregion Validation
}
