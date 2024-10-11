using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CentaurAI : EnemyAI
{
    // BOSSES
    CentaurPhase currentCentaurPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.5f;  // Adjust this to control how long each phase lasts

    Vector3 lockedPosition;
    bool chargeProcessStarted;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start() 
    {
        currentCentaurPhase = CentaurPhase.Wait;
    }

    protected override void FixedUpdate() { }

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
            // Check if the enemy is a Centaur boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Centaur)
            {
                // Handle phases based on currentPhase
                switch (currentCentaurPhase)
                {
                    case CentaurPhase.Wait:
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

                    case CentaurPhase.StraightArrowShot:
                        HandleStraightArrowShot();
                        break;

                    case CentaurPhase.ChargeAndRetreat:
                        HandleChargeAndRetreat();
                        break;

                    case CentaurPhase.SpreadArrowShot:
                        HandleSpreadArrowShot();
                        break;

                    default:
                        break;
                }
            }
        }
    }

    private void HandleWaitPhase()
    {
        // Logic for waiting phase (maybe the Centaur just moves or idles here)
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    private void HandleStraightArrowShot()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (attackMoveEnemyRoutine == null)
        {
            attackMoveEnemyRoutine = StartCoroutine(AttackRoutine(CentaurPhase.StraightArrowShot));
        }
    }

    private void HandleChargeAndRetreat()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (attackMoveEnemyRoutine == null)
        {
            attackMoveEnemyRoutine = StartCoroutine(AttackRoutine(CentaurPhase.ChargeAndRetreat));
        }
    }

    private void HandleSpreadArrowShot()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (attackMoveEnemyRoutine == null)
        {
            attackMoveEnemyRoutine = StartCoroutine(AttackRoutine(CentaurPhase.SpreadArrowShot));
        }
    }

    private void TransitionToNextPhase()
    {
        if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().transform.position) < 6f)
        {
            // If player is too close to centaur, automatically next phase will be chargeAndRetreat
            currentCentaurPhase = CentaurPhase.ChargeAndRetreat;
            return;
        }

        if (currentCentaurPhase == CentaurPhase.StraightArrowShot || currentCentaurPhase == CentaurPhase.ChargeAndRetreat || currentCentaurPhase == CentaurPhase.SpreadArrowShot)
        {
            // If centaur made a move then next phase will be wait
            currentCentaurPhase = CentaurPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentCentaurPhase = (CentaurPhase)Random.Range(2, Enum.GetValues(typeof(CentaurPhase)).Length);
        }
    }

    IEnumerator AttackRoutine(CentaurPhase centaurPhase)
    {
        if (centaurPhase == CentaurPhase.StraightArrowShot)
        {
            float fireTimer = 0f;
            float fireProjectileDuration = 5f;
            enemy.animator.SetBool(Settings.isAttacking, true);

            yield return null;

            while (fireTimer < fireProjectileDuration)
            {
                fireTimer += Time.deltaTime;

                // Interval Timer
                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.deltaTime;
                        FireWeapon();
                    }
                    else
                    {
                        // Reset timers and animation
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();
                        enemy.animateEnemy.SetIdleAnimationParameters();
                    }
                }

                yield return null;

            }

            enemy.animator.SetBool(Settings.isAttacking, false);

            yield return null;

        }
        else if (centaurPhase == CentaurPhase.ChargeAndRetreat)
        {
            // PREPARE PRECHARGE PHASE
            // Lock-on player position during the start of precharge
            if (!chargeProcessStarted)
            {
                lockedPosition = GameManager.Instance.GetPlayer().transform.position;
            }

            chargeProcessStarted = true;

            float prehargeDuration = 1.5f;
            float chargeTimer = 0f;
            enemy.animateEnemy.SetAttackAnimationParameters();
            enemy.animator.SetBool(Settings.isAttacking, false);
            enemy.animator.SetTrigger(Settings.isPrecharging);
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

            while (chargeTimer < prehargeDuration)
            {
                chargeTimer += Time.deltaTime;

                yield return null;
            }

            chargeTimer = 0f;
            float chargeDuration = 2f;

            yield return null;  // Wait for the animation to start

            // START CHARGE PHASE
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animateEnemy.SetMovementAnimationParameters();

            Vector3 direction = (lockedPosition - transform.position).normalized;
            float chargeSpeed = 20f;

            while (chargeTimer < chargeDuration)
            {
                chargeTimer += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, lockedPosition, chargeSpeed * Time.deltaTime);

                // Check if boss has reached the destination before the desired duration
                if (Vector3.Distance(transform.position, lockedPosition) < 0.1f)  // Small threshold for accuracy
                {
                    // Exit the loop early if boss has reached the destination
                    break;
                }

                yield return null;
            }

            // Revert to the idle state after charge completed
            enemy.animateEnemy.SetIdleAnimationParameters();
            chargeTimer = 0f;

            yield return null;
        }
        else if (centaurPhase == CentaurPhase.SpreadArrowShot)
        {
            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 1.3f;
            float chargeTimer = 0f;
            enemy.animateEnemy.SetAttackAnimationParameters();
            enemy.animator.SetBool(Settings.isAttacking, false);
            enemy.animator.SetTrigger(Settings.isPrechargingProjectile);

            yield return null;

            while (chargeTimer < prechargeDuration)
            {
                chargeTimer += Time.deltaTime;

                yield return null;
            }

            chargeTimer = 0f;

            yield return null;  // Wait for the animation to start

            // START CHARGE PHASE
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
                        FireWeapon(CentaurPhase.SpreadArrowShot);
                    }
                    else
                    {
                        // Reset timers
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();
                    }
                }

                yield return null;
            }

            yield return null;
        }

        chargeProcessStarted = false;
        attackMoveEnemyRoutine = null;

        TransitionToNextPhase();
    }
}
