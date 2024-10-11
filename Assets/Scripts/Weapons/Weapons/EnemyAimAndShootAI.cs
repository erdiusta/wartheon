using System.Collections;
using UnityEngine;

public class EnemyAimAndShootAI : EnemyAI
{
    Coroutine waitAfterFiringRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void FixedUpdate() { }

    protected override void Update()
    {
        // Movement cooldown timer
        currentEnemyChasePathRebuildCooldown -= Time.deltaTime;
        currentEnemyPatrolPathRebuildCooldown -= Time.deltaTime;

        // Update timers
        firingIntervalTimer -= Time.deltaTime;

        Vector3 direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() - transform.position).normalized :
            (GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position).normalized;
        lockedVector = direction;

        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.deltaTime;

        // Second check if enemy is on stun status
        if (moveStatus == MoveStatus.Stun)
        {
            enemy.animateEnemy.SetIdleAnimationParameters();

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
        else if (moveStatus == MoveStatus.Idle)
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

                    break;

                case EnemyPhase.Attack:

                    if (enemy.isDead) return;

                    enemy.animateEnemy.SetAttackAnimationParameters();

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

                    // Interval Timer
                    if (firingIntervalTimer < 0f)
                    {
                        if (firingDurationTimer >= 0)
                        {
                            firingDurationTimer -= Time.deltaTime;
                            FireWeapon();

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
                        }
                    }
                    break;

                default:
                    break;
            }
        }
    }

    IEnumerator WaitAfterFiringRoutine()
    {
        // Wait for a while after firing
        IdleProcess();

        yield return new WaitForSeconds(enemy.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration / 2f);

        // Transition to patrol phase
        enemyPhase = EnemyPhase.Patrol;
        waitAfterFiringRoutine = null;
    }
}
