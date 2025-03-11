using System.Collections;
using UnityEngine;

public class EnemyAimAndShootAI : EnemyAI
{
    Coroutine waitAfterFiringRoutine;

    bool isFired;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FixedUpdate() { }

    protected override void Update()
    {
        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.deltaTime;

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
                Perform();

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
                        enemy.animator.SetBool(Settings.isAttack, false);

                        //debugText.text = "CHASE";

                        ClearPatrolPath();
                        Chase();

                        break;

                    case EnemyPhase.GetHit:

                        break;

                    case EnemyPhase.Attack:

                        if (enemy.isDead) return;

                        //debugText.text = "ATTACK";

                        // Interval Timer
                        if (firingIntervalTimer < 0f)
                        {
                            if (firingDurationTimer >= 0)
                            {
                                firingDurationTimer -= Time.deltaTime;
                                enemy.idle.StopVelocity();
                                ClearChasePath();
                                ClearPatrolPath();
                                enemy.animateEnemy.ResetAnimatonParameters();
                                enemy.animateEnemy.SetAttackAnimationParameters();

                                if (!GameManager.Instance.GetPlayer().onStealth && !isFired)
                                {
                                    FireWeapon();
                                    isFired = true; // Make sure it doesn't fire consecutive projectiles
                                }

                                if (waitAfterFiringRoutine == null)
                                {
                                    waitAfterFiringRoutine = StartCoroutine(WaitAfterFiringRoutine());
                                }
                            }
                            else
                            {
                                // Reset timers
                                firingIntervalTimer = WeaponShootInterval();
                                firingDurationTimer = WeaponShootDuration();
                                isFired = false;
                            }
                        }

                        break;

                    default:
                        break;
                }
            }
        }
    }

    IEnumerator WaitAfterFiringRoutine()
    {
        yield return new WaitForSeconds(enemy.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration);

        // Transition to patrol phase
        if (GameManager.Instance.GetPlayer() != null)
        {
            if ((Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().transform.position) >= enemy.enemyDetails.chaseDistance))
            {
                enemyPhase = EnemyPhase.Patrol;
            }
        }

        waitAfterFiringRoutine = null;
    }
}
