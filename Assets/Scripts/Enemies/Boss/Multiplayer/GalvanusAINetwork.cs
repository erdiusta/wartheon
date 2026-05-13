using Mirror;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GalvanusAINetwork : EnemyAINetwork, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSSES
    GalvanusPhase currentGalvanusPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.5f;  // Adjust this to control how long each phase lasts

    Vector3 lockedPosition;
    bool chargeProcessStarted;

    Coroutine galvanusAttackMoveRoutine;

    bool passedToWait;

    public bool PassedToWait
    {
        get => passedToWait;
        set
        {
            if (!passedToWait && value)
            {
                passedToWait = true;
                HandleWaitPhase();
            }
        }
    }

    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start() 
    {

    }

    protected override void OnEnable()
    {
        currentGalvanusPhase = GalvanusPhase.Wait;
    }

    protected override void OnDisable() { }

    protected override void FixedUpdate() 
    {
        if (!isServer || !enemyFullyInitialized) return;

        prevVel = rb2D.linearVelocity;

        if (enemyPhase == EnemyPhase.Death)
        {
            if (attackAnimationRoutine != null)
            {
                StopCoroutine(attackAnimationRoutine);
            }

            return;
        }

        healTimer += Time.deltaTime;
        targetRefreshTimer += Time.deltaTime;

        if (targetRefreshTimer >= Settings.targetRefreshInterval)
        {
            targetPlayer = HelperUtilities.GetClosestPlayer(transform.position);
            targetRefreshTimer = 0;
        }

        // Emergency pullback if Moravelle drifts outside bounds
        if (IsOutsideBossRoom(transform.position, cellMin, cellMax, true))
        {
            Vector3 safePos = ClampToBossRoom(transform.position, cellMin, cellMax);
            transform.position = safePos;
            rb2D.linearVelocity = Vector2.zero;
        }

        if (targetPlayer != null)
        {
            Vector3 direction = (targetPlayer.GetPlayerPosition() - transform.position).normalized;
            attackLockedVector = direction;
        }

        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(attackLockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Galvanus);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimParameters(unitAimDirection);

        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.fixedDeltaTime;

        HasNegativeMoveStatusEffect();

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            if (targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            if (targetPlayer != null && targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            // Check if the enemy is a Galvanus boss
            switch (currentGalvanusPhase)
            {
                case GalvanusPhase.None:
                    break;
                case GalvanusPhase.Wait:
                    PassedToWait = true;

                    // Reset timers
                    firingIntervalTimer = WeaponShootInterval();
                    firingDurationTimer = WeaponShootDuration();

                    // Optionally handle phase transitions based on a timer
                    phaseTimer += Time.fixedDeltaTime;
                    if (phaseTimer >= waitPhase)
                    {
                        TransitionToNextPhase();
                        phaseTimer = 0f;  // Reset the timer for the next phase
                    }
                    break;
                case GalvanusPhase.DashAttack:
                    HandleDashAttack();
                    break;
                case GalvanusPhase.LightningBolt:
                    HandleLightningBolt();
                    break;
                case GalvanusPhase.Lightning:
                    HandleLightning();
                    break;
                default:
                    break;
            }
        }
    }

    protected override void Update() 
    {
        if (!isServer || !enemyFullyInitialized) return;

        healTimer += Time.deltaTime;
        targetRefreshTimer += Time.deltaTime;

        if (targetRefreshTimer >= Settings.targetRefreshInterval)
        {
            targetPlayer = HelperUtilities.GetClosestPlayer(transform.position);
            targetRefreshTimer = 0;
        }
    }

    public void HandleWaitPhase()
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
        if (targetPlayer == null) return;

        if (targetPlayer.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        float distance = Vector3.Distance(transform.position + new Vector3(0f, 0.8f, 0f), targetPlayer.GetPlayerPosition());

        if (distance < 4f)
        {
            currentGalvanusPhase = (GalvanusPhase)Random.Range(2, 4); // Dash or Bolt
        }
        else if (distance <= 12f)
        {
            float rng = Random.value;
            if (rng < 0.33f) currentGalvanusPhase = GalvanusPhase.LightningBolt;
            else if (rng < 0.66f) currentGalvanusPhase = GalvanusPhase.DashAttack;
            else currentGalvanusPhase = GalvanusPhase.Lightning;
        }
        else
        {
            currentGalvanusPhase = GalvanusPhase.Lightning;
        }
    }

    IEnumerator AttackRoutine(GalvanusPhase galvanusPhase)
    {
        if (galvanusPhase == GalvanusPhase.LightningBolt)
        {
            if (enemy.health.hasDied) yield break;

            enemyPhase = EnemyPhase.Chase;

            enemy.animator.SetBool(Settings.cast, false);

            float fireTimer = 0f;
            float fireProjectileDuration = 5f;

            yield return null;

            while (fireTimer < fireProjectileDuration)
            {
                if (enemy.health.hasDied) yield break;

                fireTimer += Time.fixedDeltaTime;

                // Interval Timer
                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.fixedDeltaTime;
                        enemy.animateEnemy.SetAttackAnimationParameters();
                        FireWeapon(isLaser: false, ProjectileKind.Default, new AttackContext { galvanusPhase = GalvanusPhase.LightningBolt });
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

            enemy.animator.SetBool(Settings.isAttack, false);

            yield return null;

            currentGalvanusPhase = GalvanusPhase.Wait;
            phaseTimer = 0f;
            passedToWait = false; // Ensure wait phase triggers again
        }
        else if (galvanusPhase == GalvanusPhase.DashAttack)
        {
            if (enemy.health.hasDied) yield break;

            enemyPhase = EnemyPhase.Attack;

            isAttacking = true;

            if (!chargeProcessStarted && targetPlayer != null)
            {
                lockedPosition = ClampToBossRoom(targetPlayer.GetPlayerPosition(), cellMin, cellMax);
            }

            chargeProcessStarted = true;

            float prechargeDuration = 0.4f;
            float chargeTimer = 0f;
            enemy.animator.SetFloat(Settings.motionType, 3f); // charge trigger to blend tree

            while (chargeTimer < prechargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.fixedDeltaTime;
                yield return waitForFixedUpdate;
            }

            isCharging = true;

            chargeTimer = 0f;
            float chargeDuration = 1f;
            Vector3 clampedPosition = ClampToBossRoom(lockedPosition, cellMin, cellMax);

            Vector3 currentPos = ClampToBossRoom(transform.position, cellMin, cellMax);
            transform.position = currentPos; // Optional but safe snap-in
            Vector2 moveDir = (clampedPosition - currentPos).normalized;

            float distance = Vector2.Distance(transform.position, clampedPosition);
            float requiredVelocity = distance / chargeDuration;
            Vector2 force = moveDir * requiredVelocity * rb2D.mass;

            rb2D.linearVelocity = Vector2.zero;
            rb2D.linearDamping = 0;
            rb2D.AddForce(force, ForceMode2D.Impulse);

            yield return waitForFixedUpdate;

            float timer = 0f;
            while (timer < chargeDuration)
            {
                if (enemy.health.hasDied) yield break;
                timer += Time.fixedDeltaTime;
                yield return waitForFixedUpdate;
            }

            rb2D.linearDamping = 3;
            rb2D.linearVelocity = Vector2.zero;
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animateEnemy.SetIdleAnimationParameters();
            isCharging = false;
            isAttacking = false;

            currentGalvanusPhase = GalvanusPhase.Wait;
            phaseTimer = 0f;
            passedToWait = false; // Ensure wait phase triggers again
        }
        else if (galvanusPhase == GalvanusPhase.Lightning)
        {
            if (enemy.health.hasDied) yield break;

            enemyPhase = EnemyPhase.Chase;

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 1.3f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, true);
            //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

            yield return null;

            while (chargeTimer < prechargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.fixedDeltaTime;

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
                if (enemy.health.hasDied) yield break;

                fireTimer += Time.fixedDeltaTime;

                // Interval Timer
                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.fixedDeltaTime;
                        FireWeapon(isLaser: false, ProjectileKind.Default, new AttackContext { galvanusPhase = GalvanusPhase.Lightning });
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

    public void PlayerStealthCheck()
    {
        currentGalvanusPhase = GalvanusPhase.Wait;
    }

    void IMutualBossBehaviour.HandleWaitPhase()
    {
        HandleWaitPhase();
    }
}
