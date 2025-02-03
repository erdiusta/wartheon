using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class TreantAI : EnemyAI
{
    // BOSSES
    TreantPhase currentTreantPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.5f;  // Adjust this to control how long each phase lasts

    Vector3 lockedPosition;
    bool chargeProcessStarted;
    int enemiesToSpawn = 2;

    Coroutine treantAttackMoveRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start() 
    {
        currentTreantPhase = TreantPhase.Wait;
    }

    protected override void FixedUpdate() 
    {
        base.FixedUpdate();
    }

    protected override void Update()
    {
        Vector3 direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() - transform.position).normalized :
            (GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position).normalized;
        lockedVector = direction;

        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(lockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);

        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.deltaTime;

        // Second check if enemy is on frost status
        if (moveStatus == MoveStatus.Frozen)
        {
            enemy.animateEnemy.SetIdleAnimationParameters();

            if (frostEnemyRoutine == null)
            {
                frostEnemyRoutine = StartCoroutine(FrostRoutine());
            }
        }
        // Third check if enemy is on stun status
        if (moveStatus == MoveStatus.Stun)
        {
            enemy.animateEnemy.SetIdleAnimationParameters();

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
        else if (moveStatus == MoveStatus.Idle)
        {
            // Check if the enemy is Treant boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Treant)
            {
                // Handle phases based on currentPhase
                switch (currentTreantPhase)
                {
                    case TreantPhase.Wait:
                        HandleWaitPhase();

                        // Reset timers
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();

                        // Optionally handle phase transitions based on a timer
                        phaseTimer += Time.deltaTime;
                        if (phaseTimer >= waitPhase)
                        {
                            TransitionToNextPhase();
                            phaseTimer = 0f;  // Reset the timer for the next phase
                        }
                        break;

                    case TreantPhase.StraightAttack:
                        HandleStraightAttack();
                        break;

                    case TreantPhase.Summon:
                        HandleSummon();
                        break;

                    case TreantPhase.RazorLeaf:
                        HandleRazorLeaf();
                        break;

                    case TreantPhase.Heal:
                        HandleHeal();
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private void HandleWaitPhase()
    {
        // Logic for waiting phase (maybe the Treant just moves or idles here)
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    private void HandleStraightAttack()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (treantAttackMoveRoutine == null)
        {
            treantAttackMoveRoutine = StartCoroutine(AttackRoutine(TreantPhase.StraightAttack));
        }
    }

    private void HandleSummon()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (treantAttackMoveRoutine == null)
        {
            treantAttackMoveRoutine = StartCoroutine(AttackRoutine(TreantPhase.Summon));
        }
    }

    private void HandleHeal()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (treantAttackMoveRoutine == null)
        {
            treantAttackMoveRoutine = StartCoroutine(AttackRoutine(TreantPhase.Heal));
        }
    }

    private void HandleRazorLeaf()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (treantAttackMoveRoutine == null)
        {
            treantAttackMoveRoutine = StartCoroutine(AttackRoutine(TreantPhase.RazorLeaf));
        }
    }

    private void TransitionToNextPhase()
    {
        if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().transform.position) < 4f)
        {
            // If player is too close to treant, automatically next phase will be chargeAndRetreat
            currentTreantPhase = (TreantPhase)Random.Range(2, 4);
            return;
        }
        
        if (currentTreantPhase == TreantPhase.StraightAttack || currentTreantPhase == TreantPhase.RazorLeaf ||
            currentTreantPhase == TreantPhase.Heal || currentTreantPhase == TreantPhase.Summon)
        {
            // If treant made a move then next phase will be wait
            currentTreantPhase = TreantPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentTreantPhase = (TreantPhase)Random.Range(2, Enum.GetValues(typeof(TreantPhase)).Length);

            // If health is not low enough, switch heal phase
            if (currentTreantPhase == TreantPhase.Heal && enemy.health.GetCurrentHealth() > (int)(enemy.health.GetMaximumHealth() * 0.5f))
            {
                currentTreantPhase = (TreantPhase)Random.Range(2, Enum.GetValues(typeof(TreantPhase)).Length - 1);
            }
        }
    }

    IEnumerator AttackRoutine(TreantPhase treantPhase)
    {
        if (treantPhase == TreantPhase.StraightAttack)
        {
            isAttacking = true;
            // Initialize vectors, angles, directions and aim
            float unitAngle = HelperUtilities.GetAngleFromVector(lockedVector);
            AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
            enemy.aimWeapon.Aim(unitAimDirection, unitAngle);
            enemy.animateEnemy.ResetAimAnimationParameters();
            enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);
            enemy.animateEnemy.SetAttackAnimationParameters();
            enemy.idle.StopVelocity();

            // Wait until dash starts
            yield return new WaitForSeconds(0.6f);

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
            enemyPhase = EnemyPhase.Patrol;
            currentEnemyPatrolPathRebuildCooldown = -0.1f;

            yield return null;
        }
        else if (treantPhase == TreantPhase.RazorLeaf)
        {
            enemyPhase = EnemyPhase.Chase;

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 1f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, true);

            yield return null;

            while (chargeTimer < prechargeDuration)
            {
                chargeTimer += Time.deltaTime;

                yield return null;
            }

            chargeTimer = 0f;

            yield return null;  // Wait for the animation to start

            // START CHARGE PHASE
            enemy.animator.SetBool(Settings.cast, false);
            enemy.animateEnemy.SetIdleAnimationParameters();

            float fireTimer = 0f;
            float fireProjectileDuration = enemy.enemyDetails.enemyWeapon.weaponCooldownDuration;

            while (fireTimer < fireProjectileDuration)
            {
                fireTimer += Time.deltaTime;

                // Interval Timer
                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.deltaTime;
                        FireWeapon(0, TreantPhase.RazorLeaf);
                    }
                    else
                    {
                        // Reset timers
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();
                        enemy.animateEnemy.SetIdleAnimationParameters();
                    }
                }

                yield return null;
            }

            yield return null;
        }
        else if (treantPhase == TreantPhase.Summon)
        {
            enemy.animator.SetFloat(Settings.motionType, 3f);

            Grid grid = currentRoom.instantiatedRoom.grid;

            // Create an instance of the helper class used to select a random enemy
            RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

            // Check we have somewhere to spawn the enemies
            if (currentRoom.spawnPositionArray.Length > 0)
            {
                // Loop through to create all the enemeies
                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];

                    // Create Enemy - Get next enemy type to spawn 
                    EnemySpawner.Instance.CreateEnemy(enemyDetails.enemyMinionDetails, grid.CellToWorld(cellPosition));
                }
            }

            yield return new WaitForSeconds(2f);
        }
        else if (treantPhase == TreantPhase.Heal)
        {
            enemy.animator.SetFloat(Settings.motionType, 4f);

            enemy.health.AddHealth((int)(20f / enemy.health.GetMaximumHealth() * 100));

            yield return new WaitForEndOfFrame();

            yield return new WaitForSeconds(2f);
        }

        chargeProcessStarted = false;
        treantAttackMoveRoutine = null;

        TransitionToNextPhase();
    }
}
