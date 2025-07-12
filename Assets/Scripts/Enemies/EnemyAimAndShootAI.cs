using System.Collections;
using UnityEngine;

public class EnemyAimAndShootAI : EnemyAI
{
    Coroutine waitAfterFiringRoutine;

    bool isFired;
    float enemyShotCooldownTimer = 0f;

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

        enemyShotCooldownTimer = Mathf.Max(0f, enemyShotCooldownTimer - Time.fixedDeltaTime);
        avoidSuppressionTimer = Mathf.Max(0f, avoidSuppressionTimer - Time.fixedDeltaTime);

        // AIM
        Vector3 unitVector = Vector3.zero; Vector3 weaponDirection; float weaponAngleDegrees; float enemyAngleDegrees;
        AimDirection enemyAimDirection; AttackDirection enemyAttackDirection;

        if (HasNegativeMoveStatusEffect()) return;
        else SecondaryStatusEffectsCheck();

        if (moveStatus == MoveStatus.Idle)
        {
            if (player == null || player.health.hasDied) return;

            UpdatePhaseStatus(true);

            switch (enemyPhase)
            {
                case EnemyPhase.Patrol:

                    Aim(out unitVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);

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

                    Aim(out unitVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection, out enemyAttackDirection);

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
                    enemy.movementToPosition.AttackMoveRigidbodyByPosition(referenceIntentionVector, enemy.currentMoveSpeed);

                    break;

                case EnemyPhase.Attack:
                    if (enemy.health.hasDied) return;

                    // Disable patrol bot aiDestination setter for attack phase
                    enemy.aiRigidbody2D.enabled = true;
                    enemy.patrol.enabled = false;
                    enemy.aiDestinationSetter.enabled = false;
                    enemy.aiRigidbody2D.canMove = false; // Disable normal attack behaviour during dash or firing

                    enemy.animateEnemy.ResetAnimatonParameters();

                    // Interval timer
                    if (firingIntervalTimer < 0f)
                    {
                        if (firingDurationTimer >= 0 && enemyShotCooldownTimer == 0f)
                        {
                            firingDurationTimer -= Time.fixedDeltaTime;
                            FireWeapon();
                            enemyShotCooldownTimer = WeaponShootDuration();
                        }
                        else
                        {
                            // Reset timers
                            firingIntervalTimer = WeaponShootInterval();
                            firingDurationTimer = WeaponShootDuration();
                        }
                    }
                    break;

                case EnemyPhase.Flank:
                    // Disable pathfinder classes
                    enemy.aiRigidbody2D.enabled = false;
                    enemy.patrol.enabled = false;
                    enemy.aiDestinationSetter.enabled = false;

                    Aim(out Vector3 _, out Vector3 weaponDir, out float _, out float _, out AimDirection _, out AttackDirection _);

                    if (IsPlayerInLineOfSight(weaponDir, enemyDetails.enemyWeapon.weaponCurrentProjectile.projectileRange))
                    {
                        enemyPhase = EnemyPhase.Attack;
                        enemy.aiRigidbody2D.canMove = false;
                        return;
                    }

                    // NEED TO ADD
                    Vector3 flankDirection = Quaternion.Euler(0f, 0f, 90f) * (player.transform.position - enemy.transform.position).normalized;
                    enemy.movementToPosition.AttackMoveRigidbodyByPosition(flankDirection, enemy.currentMoveSpeed);

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
