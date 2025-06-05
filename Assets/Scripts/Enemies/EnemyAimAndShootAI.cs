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

    protected override void Start()
    {
        base.Start();
    }

    protected override void FixedUpdate() 
    {
        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.fixedDeltaTime;

        // AIM
        Vector3 unitVector = Vector3.zero; Vector3 weaponDirection; float weaponAngleDegrees; float enemyAngleDegrees;
        AimDirection enemyAimDirection; AttackDirection enemyAttackDirection;

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
            /// STOP CODE
        }
        else
        {
            if (moveStatus == MoveStatus.Idle)
            {
                if (GameManager.Instance.GetPlayer() == null || GameManager.Instance.GetPlayer().isDead) return;

                if (updatePhaseRoutine == null)
                {
                    updatePhaseRoutine = StartCoroutine(UpdatePhaseStatus(true));
                }

                switch (enemyPhase)
                {
                    case EnemyPhase.Patrol:

                        Aim(out unitVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);

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

                        Aim(out unitVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);

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
                        enemy.isFiring = false;

                        enemy.patrol.enabled = false;
                        enemy.aiDestinationSetter.enabled = true;

                        enemy.idle.StopVelocity();
                        break;

                    case EnemyPhase.Attack:
                        if (enemy.isDead) return;

                        // Disable patrol bot aiDestination setter for attack phase
                        enemy.patrol.enabled = false;
                        enemy.aiDestinationSetter.enabled = false;
                        enemy.aiLerp.canMove = false; // Disable normal attack behaviour during dash or firing

                        enemy.animateEnemy.ResetAnimatonParameters();

                        // Interval timer
                        if (firingIntervalTimer < 0f)
                        {
                            if (firingDurationTimer >= 0)
                            {
                                firingDurationTimer -= Time.deltaTime;

                                FireWeapon();
                            }
                            else
                            {
                                // Reset timers
                                firingIntervalTimer = WeaponShootInterval();
                                firingDurationTimer = WeaponShootDuration();
                                enemy.isFiring = false;
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
