using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyMovementAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion
    [SerializeField] MovementDetailsSO movementDetails;

    Enemy enemy;
    Stack<Vector3> movementSteps = new Stack<Vector3>();
    Vector3 playerReferencePosition;
    Coroutine moveEnemyRoutine;
    float currentEnemyPathRebuildCooldown;
    WaitForFixedUpdate waitForFixedUpdate;
    bool chasePlayer = false;

    Vector3 knockbackVector;
    float knockbackForce;
    float knockbackTimeWeight;
    Status status = Status.Idle;

    [HideInInspector] public float moveSpeed;
    [HideInInspector] public int updateFrameNumber = 1; // default value.  This is set by the enemy spawner

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
<<<<<<< Updated upstream
        moveSpeed = movementDetails.GetMoveSpeed();
=======
        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
>>>>>>> Stashed changes
    }

    private void Start()
    {
        // Create waitforfixed update for use in coroutine
        waitForFixedUpdate = new WaitForFixedUpdate();

        // Reset player reference position
        playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();        
    }

    private void Update()
    {
        if (status != Status.Stagger)
        {
            MoveEnemy();
        }
    }

    /// <summary>
    /// Use AStar pathfinding to build a path to the player - and then move the enemy to each grid location on the path
    /// </summary>
    private void MoveEnemy()
    {
<<<<<<< Updated upstream
        // Movement cooldown timer
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        // Check distance to player to see if enemy should start chasing
        if (!chasePlayer && Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) <
            enemy.enemyDetails.chaseDistance)
        {
            chasePlayer = true;
        }

        // If not close enough to chase player then return
        if (!chasePlayer)
            return;

        // Only process A Star path rebuild on certain frames to spread the load between enemies
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathfindingOver != updateFrameNumber) 
            return;

        // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
        if (currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) > 
            Settings.playerMoveDistanceToRebuildPath))
        {
            // Reset path rebuild cooldown timer
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            // Reset player reference position
            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            // Move the enemy using AStar pathfinding - Trigger rebuild of path to player
            CreatePath();

            // If a path has been found move the enemy
            if (movementSteps != null)
            {
=======
        // First check if enemy is dead
        if (enemy.health.currentHealth <= 0f)
        {
            StopAllCoroutines();
            StartCoroutine(NullifySpeedForDeathRoutine());
        }
        // Second check if enemy is on knockback status
        else if (status == Status.Stagger)
        {

            StartCoroutine(KnockbackRoutine());
        }
        // If enemy is neither dead or knockedback, start move process based on enemy's move behaviour
        else
        {
            if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.AimAndShoot)
            {
                if (enemy.isFiring)
                {
                    StartCoroutine(WeaponFiredRoutine());
                }
            }

            PathfindMove();
        }
    }

    private void PathfindMove()
    {
        if (enemy.isFiring) return;

        // Movement cooldown timer
        currentEnemyPathRebuildCooldown -= Time.deltaTime;

        // Check distance to player to see if enemy should start chasing
        if (!chasePlayer && Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().GetPlayerPosition()) <
            enemy.enemyDetails.chaseDistance)
        {
            chasePlayer = true;
        }

        // If not close enough to chase player then return
        if (!chasePlayer)
            return;

        // Only process A Star path rebuild on certain frames to spread the load between enemies
        if (Time.frameCount % Settings.targetFrameRateToSpreadPathfindingOver != updateFrameNumber)
            return;

        // If the movement cooldown timer reached or player has moved more than required distance then rebuild the enemy path and move the enemy
        if (currentEnemyPathRebuildCooldown <= 0f || (Vector3.Distance(playerReferencePosition, GameManager.Instance.GetPlayer().GetPlayerPosition()) >
            Settings.playerMoveDistanceToRebuildPath))
        {
            // Reset path rebuild cooldown timer
            currentEnemyPathRebuildCooldown = Settings.enemyPathRebuildCooldown;

            // Reset player reference position
            playerReferencePosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

            // Move the enemy using AStar pathfinding - Trigger rebuild of path to player
            CreatePath();

            // If a path has been found move the enemy
            if (movementSteps != null)
            {
>>>>>>> Stashed changes
                if (moveEnemyRoutine != null)
                {
                    // Trigger idle event
                    enemy.idle.StopVelocity();
                    enemy.animateEnemy.SetIdleAnimationParameters();
                    StopCoroutine(moveEnemyRoutine);
                }

                // Move enemy along the path using a coroutine
<<<<<<< Updated upstream
                moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps)); 
=======
                moveEnemyRoutine = StartCoroutine(MoveEnemyRoutine(movementSteps));
>>>>>>> Stashed changes
            }
        }
    }

    /// <summary>
    /// Coroutine to move the enemy to the next location on the path
    /// </summary>
    IEnumerator MoveEnemyRoutine(Stack<Vector3> movementSteps)
    {
        while (movementSteps.Count > 0)
        {
            Vector3 nextPosition = movementSteps.Pop();

            // while not very close continue to move - when close move onto the next step
            while (Vector3.Distance(nextPosition, transform.position) > 0.2f)
            {
                // Trigger movement event
                enemy.movementToPositionEvent.CallMovementToPositionEvent(nextPosition, transform.position, moveSpeed,
                    (nextPosition - transform.position).normalized);

                // Moving the enemy using 2D physics so wait until the next fixed update
                yield return waitForFixedUpdate; 
            }

            yield return waitForFixedUpdate;
        }

        // End of path steps - trigger the enemy idle event
        enemy.idle.StopVelocity();
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    /// <summary>
    /// Use the AStar static class to create a path for the enemy
    /// </summary>
    private void CreatePath()
    {
        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Grid grid = currentRoom.instantiatedRoom.grid;

        // Get players position on the grid
        Vector3Int playerGridPosition = GetNearestNonObstaclePlayerPosition(currentRoom);

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
            // Trigger idle event - no path
            enemy.idle.StopVelocity();
            enemy.animateEnemy.SetIdleAnimationParameters();
        }
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
    private Vector3Int GetNearestNonObstaclePlayerPosition(Room currentRoom)
    {
        Vector3 playerPosition = GameManager.Instance.GetPlayer().GetPlayerPosition();

        Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(playerPosition);

        Vector2Int adjustedPlayerCellPosition = new Vector2Int(playerCellPosition.x - currentRoom.templateLowerBounds.x,
            playerCellPosition.y - currentRoom.templateLowerBounds.y);

        int obstacle = currentRoom.instantiatedRoom.aStarMovementPenalty[adjustedPlayerCellPosition.x, adjustedPlayerCellPosition.y];

        // If the player isn't on a cell square marked as an obstacle then return that position
        if (obstacle != 0)
            return playerCellPosition;

        // Find a surrounding cell that isn't an obstacle - required because with the 'half collision' tiles the player can be on a grid
        // square that is marked as an obstacle
        else
        {
            for (int i = -1; i <= 1; i++)
            {
                for (int j = -1; j <= 1; j++)
                {
                    if (j == 0 && i == 0) 
                        continue;

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
<<<<<<< Updated upstream
=======
    #endregion

    IEnumerator KnockbackRoutine()
    {
        enemy.movementToPosition.MoveRigidbodyByPosition(playerReferencePosition, transform.position, moveSpeed);
        enemy.animateEnemy.SetMovementAnimationParameters();

        if (enemy.health.currentHealth > 0f)
        {
            enemy.rb2D.velocity += CalculateKnockback();
        }
        else
        {
            enemy.rb2D.velocity = new Vector2(0f, 0f);
        }

        yield return waitForFixedUpdate;
    }
>>>>>>> Stashed changes

    public void Knockback(Vector3 vector, float force, float timeWeight)
    {
        knockbackVector = vector;
        knockbackForce = force;
        knockbackTimeWeight = timeWeight;
        StartCoroutine(Stagger());
    }

    IEnumerator Stagger()
    {
        status = Status.Stagger;
        float staggerTime = knockbackTimeWeight;
        moveSpeed = 0f;
        enemy.rb2D.velocity += CalculateKnockback();
        yield return new WaitForSeconds(staggerTime);

<<<<<<< Updated upstream
        moveSpeed = movementDetails.GetMoveSpeed();
=======
        knockbackTimeWeight = 0f;
        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
>>>>>>> Stashed changes
        status = Status.Idle;
    }

    private Vector2 CalculateKnockback()
    {
        Vector2 updatedKnockbackVector = new Vector2();

        if (knockbackTimeWeight > 0f)
        {
            knockbackTimeWeight -= Time.fixedDeltaTime;
            updatedKnockbackVector = knockbackVector * knockbackForce * (knockbackTimeWeight > 0f ? knockbackTimeWeight : 0f);

<<<<<<< Updated upstream
        return knockbackVector * knockbackForce * (knockbackTimeWeight > 0f ? 1f : 0f);
    }

=======
        }
        else
        {
            NullifySpeedForDeathRoutine();
        }

        return updatedKnockbackVector;
    }

    private void WeaponFiredWithoutRoutine()
    {
        enemy.isFiring = false;
    }

    IEnumerator WeaponFiredRoutine()
    {
        moveSpeed = 0f;

        yield return new WaitForSeconds(enemy.enemyDetails.enemyWeapon.weaponFireRate);

        moveSpeed = enemyDetails.movementDetails.GetMoveSpeed();
        enemy.isFiring = false;
    }

    IEnumerator NullifySpeedForDeathRoutine()
    {
        moveSpeed = 0f;
        enemy.knockback.knockbackForce = 0f;
        enemy.rb2D.velocity = new Vector2(0f, 0f);

        yield return null;
    }


>>>>>>> Stashed changes
    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion Validation
}
