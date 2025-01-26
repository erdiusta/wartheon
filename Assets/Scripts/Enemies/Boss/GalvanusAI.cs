using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GalvanusAI : EnemyAI
{
    // BOSSES
    GalvanusPhase currentGalvanusPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.5f;  // Adjust this to control how long each phase lasts

    Vector3 lockedPosition;
    bool chargeProcessStarted;

    Coroutine galvanusAttackMoveRoutine;

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start() 
    {
        currentGalvanusPhase = GalvanusPhase.Wait;
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
            // Check if the enemy is a Galvanus boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Galvanus)
            {
                // Handle phases based on currentPhase
                switch (currentGalvanusPhase)
                {
                    case GalvanusPhase.Wait:
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

                    case GalvanusPhase.LightningBolt:
                        HandleLightningBolt();
                        break;

                    case GalvanusPhase.DashAttack:
                        HandleDashAttack();
                        break;

                    case GalvanusPhase.Lightning:
                        HandleLightning();
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

    private void HandleLightningBolt()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (galvanusAttackMoveRoutine == null)
        {
            galvanusAttackMoveRoutine = StartCoroutine(AttackRoutine(GalvanusPhase.LightningBolt));
        }
    }

    private void HandleDashAttack()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (galvanusAttackMoveRoutine == null)
        {
            galvanusAttackMoveRoutine = StartCoroutine(AttackRoutine(GalvanusPhase.DashAttack));
        }
    }

    private void HandleLightning()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (galvanusAttackMoveRoutine == null)
        {
            galvanusAttackMoveRoutine = StartCoroutine(AttackRoutine(GalvanusPhase.Lightning));
        }
    }

    private void TransitionToNextPhase()
    {
        if (Vector3.Distance(transform.position, GameManager.Instance.GetPlayer().transform.position) < 4f)
        {
            // If player is too close to centaur, automatically next phase will be chargeAndRetreat
            currentGalvanusPhase = GalvanusPhase.DashAttack;
            return;
        }

        if (currentGalvanusPhase == GalvanusPhase.DashAttack || currentGalvanusPhase == GalvanusPhase.Lightning ||
            currentGalvanusPhase == GalvanusPhase.LightningBolt)
        {
            // If centaur made a move then next phase will be wait
            currentGalvanusPhase = GalvanusPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentGalvanusPhase = (GalvanusPhase)Random.Range(2, Enum.GetValues(typeof(CentaurPhase)).Length);
        }
    }

    IEnumerator AttackRoutine(GalvanusPhase galvanusPhase)
    {
        if (galvanusPhase == GalvanusPhase.LightningBolt)
        {
            enemyPhase = EnemyPhase.Chase;

            float fireTimer = 0f;
            float fireProjectileDuration = 5f;

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
                        enemy.animateEnemy.SetAttackAnimationParameters();
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
        else if (galvanusPhase == GalvanusPhase.DashAttack)
        {
            enemyPhase = EnemyPhase.Attack;
            isAttacking = true;

            // PREPARE PRECHARGE PHASE
            // Lock-on player position during the start of precharge
            if (!chargeProcessStarted)
            {
                lockedPosition = GameManager.Instance.GetPlayer().transform.position;
            }

            chargeProcessStarted = true;

            float prehargeDuration = 1.5f;
            float chargeTimer = 0f;

            enemy.animator.SetFloat(Settings.motionType, 1f); // charge trigger to blend tree

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
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.attackSoundEffect);

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

            isAttacking = false;
        }
        else if (galvanusPhase == GalvanusPhase.Lightning)
        {
            enemyPhase = EnemyPhase.Chase;

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 1.3f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, true);
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

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
                        FireWeapon(0, 0, GalvanusPhase.Lightning);
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

        chargeProcessStarted = false;
        galvanusAttackMoveRoutine = null;

        TransitionToNextPhase();
    }
}
