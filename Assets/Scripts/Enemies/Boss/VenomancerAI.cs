using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class VenomancerAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-4, 6);
    readonly Vector2Int cellMax = new Vector2Int(8, 14);

    // BOSSES
    [SerializeField] Transform swordHoldingTransform;
    [SerializeField] float smearCircleRadius = 0.5f;

    VenomancerPhase currentVenomancerPhase;
    VenomancerPhase previousVenomancerPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.2f;  // Adjust this to control how long each phase lasts

    ParticleSystem specialMoveParticlesSystem;
    float shakeIntensity = 3f;
    float shakeDuration = 1.2f;

    bool slamPerformed;
    Health playerHealth;
    Vector3 lockedPosition;
    bool chargeProcessStarted;
    float seismicSlamCircleRadius = 5f;

    Coroutine venomancerAttackMoveRoutine;

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
        specialMoveParticlesSystem = transform.GetChild(transform.childCount - 2).GetComponent<ParticleSystem>();

        currentVenomancerPhase = VenomancerPhase.Wait;
    }

    protected override void OnEnable() 
    {
        player = GameManager.Instance.GetPlayer();
    }

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
                (player.GetPlayerPosition() - transform.position).normalized;
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
            // Check if the player is on stealth
            if (GameManager.Instance.GetPlayer() != null && GameManager.Instance.GetPlayer().isStealthActive)
            {
                PlayerStealthCheck();
            }

            // Check if the enemy is a Venomancer boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Venomancer)
            {
                // Handle phases based on currentPhase
                switch (currentVenomancerPhase)
                {
                    case VenomancerPhase.Wait:
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

                    case VenomancerPhase.SludgeThrow:
                        HandleSludgeThrow();
                        break;

                    case VenomancerPhase.SlamGround:
                        HandleSlamGround();
                        break;

                    case VenomancerPhase.StoneRain:
                        HandleStoneRain();
                        break;

                    case VenomancerPhase.ToxicPool:
                        HandleToxicPool();
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

    private void HandleSludgeThrow()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (venomancerAttackMoveRoutine == null)
        {
            venomancerAttackMoveRoutine = StartCoroutine(AttackRoutine(VenomancerPhase.SludgeThrow));
        }
    }

    private void HandleSlamGround()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (venomancerAttackMoveRoutine == null)
        {
            venomancerAttackMoveRoutine = StartCoroutine(AttackRoutine(VenomancerPhase.SlamGround));
        }
    }

    private void HandleStoneRain()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (venomancerAttackMoveRoutine == null)
        {
            venomancerAttackMoveRoutine = StartCoroutine(AttackRoutine(VenomancerPhase.StoneRain));
        }
    }

    private void HandleToxicPool()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (venomancerAttackMoveRoutine == null)
        {
            venomancerAttackMoveRoutine = StartCoroutine(AttackRoutine(VenomancerPhase.ToxicPool));
        }
    }

    private void TransitionToNextPhase()
    {
        // Reset Slam Performed
        slamPerformed = false;

        // Check if the player is on stealth
        if (GameManager.Instance.GetPlayer().isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.GetPlayerPosition()) < 3f)
            {
                // If player is too close to boss, automatically next phase will be Slam Ground
                currentVenomancerPhase = VenomancerPhase.SlamGround;
                return;
            }
            else if (Vector3.Distance(transform.position, player.GetPlayerPosition()) > 12f)
            {
                int rng = Random.Range(0, 2);
                currentVenomancerPhase = rng == 0 ? VenomancerPhase.SludgeThrow : VenomancerPhase.StoneRain;
            }
        }

        if (currentVenomancerPhase == VenomancerPhase.SlamGround || currentVenomancerPhase == VenomancerPhase.ToxicPool ||
            currentVenomancerPhase == VenomancerPhase.SludgeThrow || currentVenomancerPhase == VenomancerPhase.StoneRain)
        {
            // If centaur made a move then next phase will be wait
            currentVenomancerPhase = VenomancerPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentVenomancerPhase = (VenomancerPhase)Random.Range(2, Enum.GetValues(typeof(VenomancerPhase)).Length);
        }
    }

    IEnumerator AttackRoutine(VenomancerPhase venomancerPhase)
    {
        if (venomancerPhase == VenomancerPhase.SludgeThrow)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 0.6f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, true);

            yield return null;

            enemy.animator.SetBool(Settings.isAttack, true);
            enemy.animator.SetInteger(Settings.attackType, 1);

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
                        FireWeapon(false, 0, 0, 0, 0, 0, VenomancerPhase.SludgeThrow);
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

            enemy.animateEnemy.SetIdleAnimationParameters();
            previousVenomancerPhase = VenomancerPhase.SludgeThrow;
            enemy.animator.SetInteger(Settings.attackType, 0);

        }
        else if (venomancerPhase == VenomancerPhase.SlamGround)
        {
            if (enemy.health.hasDied) yield break;

            enemyPhase = EnemyPhase.Attack;
            isAttacking = true;

            // PREPARE PRECHARGE PHASE
            // Lock-on player position during the start of precharge

            if (!chargeProcessStarted && GameManager.Instance.GetPlayer() != null)
            {
                lockedPosition = GameManager.Instance.GetPlayer().transform.position + new Vector3(0f, 0.5f, 0f);
            }

            chargeProcessStarted = true;

            // Pre-check if moving towards player is necessary 
            if (Vector3.Distance(transform.position, lockedPosition) < 1.5f)  // Small threshold for accuracy
            {
                // Exit the loop early if boss has reached the destination
                goto skipRun;
            }

            enemy.animator.SetFloat(Settings.motionType, 1f); // charge trigger to blend tree

            float chargeTimer = 0f;
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

            Vector3 direction = (lockedPosition - transform.position).normalized;
            float chargeSpeed = 20f;

            while (chargeTimer < chargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, clampedPosition, chargeSpeed * Time.deltaTime);

                // Check if boss has reached the destination before the desired duration
                if (Vector3.Distance(transform.position, clampedPosition) < 1.5f)  // Small threshold for accuracy
                {
                    // Exit the loop early if boss has reached the destination
                    break;
                }

                yield return null;
            }


        skipRun:

            yield return null;

            // SLAM PHASE
            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, false);
            enemy.animator.SetFloat(Settings.motionType, -1f);

            enemy.animator.SetInteger(Settings.attackType, 2);

            if (!slamPerformed)
            {
                yield return new WaitForSeconds(1f); // Wait for slam body animation

                SeismicSlam();
                slamPerformed = true;
            }

            //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.attackSoundEffect);

            yield return null;

            enemy.animator.SetBool(Settings.isAttack, false);
            enemy.animateEnemy.SetIdleAnimationParameters();

            isAttacking = false;

            previousVenomancerPhase = VenomancerPhase.SlamGround;
            enemy.animator.SetInteger(Settings.attackType, 0);
        }
        else if (venomancerPhase == VenomancerPhase.StoneRain)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 3);
            enemyPhase = EnemyPhase.Attack;

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
                        FireWeapon(false, 0, 0, 0, 0, 0, VenomancerPhase.StoneRain);
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

            previousVenomancerPhase = VenomancerPhase.StoneRain;
            enemy.animator.SetInteger(Settings.attackType, 0);
        }
        else if (venomancerPhase == VenomancerPhase.ToxicPool)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 4);

            enemyPhase = EnemyPhase.Chase;

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
                        FireWeapon(false, 0, 0, 0, 0, 0, VenomancerPhase.ToxicPool);
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

            isAttacking = false;
            previousVenomancerPhase = VenomancerPhase.ToxicPool;
            enemy.animator.SetInteger(Settings.attackType, 0);
        }

        chargeProcessStarted = false;
        venomancerAttackMoveRoutine = null;

        TransitionToNextPhase();
    }

    public void PlayerStealthCheck()
    {
        currentVenomancerPhase = VenomancerPhase.Wait;
    }


    /// <summary>
    /// Execute Seismic Slam special move
    /// </summary>
    private void SeismicSlam()
    {
        // Get all colliders within the radius of the seismic slam
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, seismicSlamCircleRadius);

        if (specialMoveParticlesSystem != null)
        {
            specialMoveParticlesSystem.Play();

            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);
        }

        StaticEventHandler.CallCameraShakeEvent(shakeIntensity, shakeDuration);

        foreach (Collider2D col in colliders)
        {
            // Check if the collider belongs to an enemy or any other object you want to affect
            if (col.CompareTag(Settings.playerTag))
            {
                // Apply damage to the enemy
                Player player = col.GetComponent<Player>();

                player.movementByVelocity.TriggerKnockback((player.transform.position - transform.position).normalized);

                if (player.health != null)
                {
                    player.health.TakeDamage(15, transform.position, player.health.transform.position, false);
                }
            }
        }
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, smearCircleRadius);
    }
}
