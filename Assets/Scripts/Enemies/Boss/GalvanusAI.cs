using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class GalvanusAI : EnemyAI, IMutualBossBehaviour
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
        player = GameManager.Instance.GetPlayer();
        currentGalvanusPhase = GalvanusPhase.Wait;
    }

    protected override void OnEnable() { }

    protected override void OnDisable() { }

    protected override void FixedUpdate() { }

    protected override void Update()
    {
        if (enemy.enemyAI.enemyPhase == EnemyPhase.Death)
        {
            if (attackAnimationRoutine != null)
            {
                StopCoroutine(attackAnimationRoutine);
            }

            return;
        }

        if (player != null)
        {
            Vector3 direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() - transform.position).normalized :
                (GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position).normalized;
            lockedVector = direction;
        }

        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(lockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(unitAimDirection);

        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.deltaTime;

        HasNegativeMoveStatusEffect();

        if (moveStatus == MoveStatus.Idle)
        {
            if (player != null)
            {
                // Check if the player is on stealth
                if (player.isStealthActive)
                {
                    PlayerStealthCheck();
                }
            }

            // Check if the enemy is a Galvanus boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Galvanus)
            {
                // Handle phases based on currentPhase
                switch (currentGalvanusPhase)
                {
                    case GalvanusPhase.Wait:
                        PassedToWait = true;

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
        if (player == null) return;
        if (player.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        float distance = Vector3.Distance(transform.position + new Vector3(0f, 0.8f, 0f), player.GetPlayerPosition());

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

                fireTimer += Time.deltaTime;

                // Interval Timer
                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.deltaTime;
                        enemy.animateEnemy.SetAttackAnimationParameters();
                        FireWeapon(false, 0, 0, GalvanusPhase.LightningBolt);
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

            // PREPARE PRECHARGE PHASE
            // Lock-on player position during the start of precharge
            if (!chargeProcessStarted)
            {
                if (GameManager.Instance.GetPlayer() != null)
                {
                    lockedPosition = GameManager.Instance.GetPlayer().transform.position + new Vector3(0f, 0.5f, 0f);
                }

            }

            chargeProcessStarted = true;

            float prehargeDuration = 1.5f;
            float chargeTimer = 0f;
            enemy.animator.SetFloat(Settings.motionType, 3f); // charge trigger to blend tree

            while (chargeTimer < prehargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                yield return null;
            }

            chargeTimer = 0f;
            float chargeDuration = 2f;

            yield return null;  // Wait for the animation to start

            // START CHARGE PHASE
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animateEnemy.SetMovementAnimationParameters();

            // Clamp lockedPosition
            Grid grid = GameManager.Instance.GetBossRoom().instantiatedRoom.grid;

            Vector3Int cell = grid.WorldToCell(lockedPosition);
            cell.x = Mathf.Clamp(cell.x, cellMin.x, cellMax.x);
            cell.y = Mathf.Clamp(cell.y, cellMin.y, cellMax.y);
            Vector3 clampedPosition = grid.GetCellCenterWorld(cell);

            Vector3 direction = (clampedPosition - transform.position).normalized;
            float chargeSpeed = 20f;
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.attackSoundEffect);

            while (chargeTimer < chargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                transform.position = Vector3.MoveTowards(transform.position, clampedPosition, chargeSpeed * Time.deltaTime);

                // Check if boss has reached the destination before the desired duration
                if (Vector3.Distance(transform.position, clampedPosition) < 0.02f)  // Small threshold for accuracy
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
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

            yield return null;

            while (chargeTimer < prechargeDuration)
            {
                if (enemy.health.hasDied) yield break;

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
                if (enemy.health.hasDied) yield break;

                fireTimer += Time.deltaTime;

                // Interval Timer
                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.deltaTime;
                        FireWeapon(false, 0, 0, GalvanusPhase.Lightning);
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

        currentGalvanusPhase = GalvanusPhase.Wait;
        phaseTimer = 0f;
        passedToWait = false; // Ensure wait phase triggers again
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
