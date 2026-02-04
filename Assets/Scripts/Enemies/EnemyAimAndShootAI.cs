using UnityEngine;

public class EnemyAimAndShootAI : EnemyAI
{
    float enemyShotCooldownTimer = 0f;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();
    }

    protected override void Update()
    {
        base.Update();
    }

    protected override void FixedUpdate() 
    {
        if (!enemy.initializationCompleted) return;

        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.fixedDeltaTime;
        enemyShotCooldownTimer = Mathf.Max(0f, enemyShotCooldownTimer - Time.fixedDeltaTime);

        // AIM
        Vector3 unitVector = Vector3.zero; Vector3 weaponDirection; float weaponAngleDegrees; float enemyAngleDegrees;
        AimDirection enemyAimDirection; AttackDirection enemyAttackDirection;

        if (HasNegativeMoveStatusEffect()) return;
        else SecondaryStatusEffectsCheck();

        if (enemy.moveStatus == MoveStatus.Idle)
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
                    enemy.ResetEnemySpeed();

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
                    enemy.ResetEnemySpeed();
                    enemy.animator.SetBool(Settings.isAttack, false);

                    enemy.animateEnemy.ResetAnimatonParameters();
                    enemy.animateEnemy.SetMovementAnimationParameters();
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
