using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class MoldranAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSSES
    [SerializeField] Transform swordHoldingTransform;
    [SerializeField] float smearCircleRadius = 0.5f;

    MoldranPhase currentMoldranPhase;
    MoldranPhase previousMoldranPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.1f;  // Adjust this to control how long each phase lasts

    Health playerHealth;
    Vector3 lockedPosition;
    bool chargeProcessStarted;

    Coroutine moldranAttackMoveRoutine;

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
        currentMoldranPhase = MoldranPhase.Wait;
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

        HasNegativeMoveStatusEffect();

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            // Check if the player is on stealth
            if (GameManager.Instance.GetPlayer() != null && GameManager.Instance.GetPlayer().isStealthActive)
            {
                PlayerStealthCheck();
            }

            // Check if the enemy is a Moldran boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Moldran)
            {
                // Handle phases based on currentPhase
                switch (currentMoldranPhase)
                {
                    case MoldranPhase.Wait:
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

                    case MoldranPhase.Projectile:
                        HandleProjectile();
                        break;

                    case MoldranPhase.SwingAttack:
                        HandleSwingAttack();
                        break;

                    case MoldranPhase.Spike:
                        HandleSpike();
                        break;

                    case MoldranPhase.Heal:
                        HandleHeal();
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

    private void HandleProjectile()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (moldranAttackMoveRoutine == null)
        {
            moldranAttackMoveRoutine = StartCoroutine(AttackRoutine(MoldranPhase.Projectile));
        }
    }

    private void HandleSwingAttack()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (moldranAttackMoveRoutine == null)
        {
            moldranAttackMoveRoutine = StartCoroutine(AttackRoutine(MoldranPhase.SwingAttack));
        }
    }

    private void HandleSpike()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (moldranAttackMoveRoutine == null)
        {
            moldranAttackMoveRoutine = StartCoroutine(AttackRoutine(MoldranPhase.Spike));
        }
    }

    private void HandleHeal()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (moldranAttackMoveRoutine == null)
        {
            moldranAttackMoveRoutine = StartCoroutine(AttackRoutine(MoldranPhase.Heal));
        }
    }

    private void TransitionToNextPhase()
    {
        // Check if the player is on stealth
        if (GameManager.Instance.GetPlayer().isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        if (player != null)
        {
            if (Vector3.Distance(transform.position, player.GetPlayerPosition()) < 2f)
            {
                // If player is too close to boss, automatically next phase will be TailAttack or FrostBreath
                currentMoldranPhase = (MoldranPhase)Random.Range(4, Enum.GetValues(typeof(MoldranPhase)).Length);
                return;
            }
            else if (Vector3.Distance(transform.position, player.GetPlayerPosition()) > 10f)
            {
                // If player is too close to boss, automatically next phase will be TailAttack or FrostBreath
                int rng = Random.Range(0, 2);
                currentMoldranPhase = rng == 0 ? MoldranPhase.Projectile : MoldranPhase.Spike;
                return;
            }
        }



        if (currentMoldranPhase == MoldranPhase.SwingAttack || currentMoldranPhase == MoldranPhase.Spike ||
            currentMoldranPhase == MoldranPhase.Projectile || currentMoldranPhase == MoldranPhase.Heal)
        {
            // If centaur made a move then next phase will be wait
            currentMoldranPhase = MoldranPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            if (previousMoldranPhase == MoldranPhase.Heal) // Don't perform heal consecutively
            {
                currentMoldranPhase = (MoldranPhase)Random.Range(2, Enum.GetValues(typeof(MoldranPhase)).Length - 1);
            }
            else
            {
                currentMoldranPhase = (MoldranPhase)Random.Range(2, Enum.GetValues(typeof(MoldranPhase)).Length);
            }
        }
    }

    IEnumerator AttackRoutine(MoldranPhase moldranPhase)
    {
        if (moldranPhase == MoldranPhase.Projectile)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 2);

            //// PREPARE PRECHARGE PHASE
            //float prechargeDuration = 0.6f;
            //float chargeTimer = 0f;

            //// Set the motion type for the precharge phase
            //enemy.animateEnemy.ResetAnimatonParameters();
            //enemy.animator.SetBool(Settings.cast, true);

            //yield return null;

            //enemy.animator.SetBool(Settings.isAttack, true);

            //while (chargeTimer < prechargeDuration)
            //{
            //    chargeTimer += Time.deltaTime;

            //    yield return null;
            //}

            //chargeTimer = 0f;

            //yield return null;  // Wait for the animation to start

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
                        FireWeapon(false, 0, 0, 0, 0, 0, 0, 0, MoldranPhase.Projectile);
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
            previousMoldranPhase = MoldranPhase.Projectile;

        }
        else if (moldranPhase == MoldranPhase.SwingAttack)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 0);

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

            float prehargeDuration = 1.5f;
            float chargeTimer = 0f;

            enemy.animator.SetFloat(Settings.motionType, 1f); // charge trigger to blend tree

            //while (chargeTimer < prehargeDuration)
            //{
            //    chargeTimer += Time.deltaTime;

            //    yield return null;
            //}

            //chargeTimer = 0f;
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
                if (Vector3.Distance(transform.position, lockedPosition) < 1.5f)  // Small threshold for accuracy
                {
                    // Exit the loop early if boss has reached the destination
                    break;
                }

                yield return null;
            }


        skipRun:

            chargeTimer = 0f;

            yield return null;

            // Location change completed now starting sword smear process starts if player is close to the enemy
            float smearDuration = 1f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();

            Vector3 playerDirectionVector = new Vector3();

            if (GameManager.Instance.GetPlayer() != null)
            {
                playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;
            }

            yield return null;

            while (chargeTimer < smearDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                enemy.animator.SetBool(Settings.isAttack, true);

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(swordHoldingTransform.position, smearCircleRadius))
                {
                    if (collider.GetType() == typeof(PolygonCollider2D))
                    {
                        // Don't hit yourself if player is also in the collider list
                        if (collider.tag == Settings.enemyTag) continue;

                        if (collider.tag == Settings.chestItemTag) continue;

                        if (playerHealth = collider.GetComponent<Health>())
                        {
                            Player player = collider.GetComponent<Player>();

                            float blindPenalty = enemy.isBlind ? 0.5f : 0f;

                            DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position };

                            // Evasiveness - dodge check
                            if (100 - (player.currentDodgeValue + blindPenalty) * 100 > Random.Range(1, 101))
                            {
                                ctx.receiverPosition = player.transform.position;
                                HealthAuthorityResolver.GetAuthority(playerHealth.gameObject).ApplyDamage(25, ctx);

                                //SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                // Apply knockback
                                ApplyKnockbackToPlayer(player);
                            }
                            else
                            {
                                ctx.receiverPosition = enemy.health.transform.position;

                                player.health.isDodging = true;
                                player.healthEvent.CallDodgeEvent();
                                player.health.PostHitImmunity(true);
                                HealthAuthorityResolver.GetAuthority(player.gameObject).ApplyDamage(0, ctx);
                            }
                        }
                    }
                }

                yield return null;

                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.attackSoundEffect);
            }

            enemy.animator.SetBool(Settings.isAttack, false);
            enemy.animateEnemy.SetIdleAnimationParameters();

            isAttacking = false;

            previousMoldranPhase = MoldranPhase.SwingAttack;
        }
        else if (moldranPhase == MoldranPhase.Spike)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 1);
            enemyPhase = EnemyPhase.Attack;

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
                        FireWeapon(false, 0, 0, 0, 0, 0, 0, 0, MoldranPhase.Spike);
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

            previousMoldranPhase = MoldranPhase.Spike;
        }
        else if (moldranPhase == MoldranPhase.Heal)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetBool(Settings.cast, true);
            enemy.animator.SetInteger(Settings.attackType, 0);

            enemy.health.AddHealth(20);

            yield return new WaitForSeconds(2f);

            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.chargeSoundEffect);

            previousMoldranPhase = MoldranPhase.Heal;
            enemy.animator.SetBool(Settings.cast, false);
        }

        chargeProcessStarted = false;
        moldranAttackMoveRoutine = null;

        TransitionToNextPhase();
    }

    public void PlayerStealthCheck()
    {
        currentMoldranPhase = MoldranPhase.Wait;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, smearCircleRadius);
    }
}
