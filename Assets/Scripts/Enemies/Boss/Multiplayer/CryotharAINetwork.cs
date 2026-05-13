using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class CryotharAINetwork : EnemyAINetwork, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSSES
    [SerializeField] Transform swordHoldingTransform;
    [SerializeField] float smearCircleRadius = 0.5f;

    CryotharPhase currentCryotharPhase;
    CryotharPhase previousCryotharPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.2f;  // Adjust this to control how long each phase lasts

    Health playerHealth;
    Vector3 lockedPosition;
    bool chargeProcessStarted;

    Coroutine cryotharAttackMoveRoutine;

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
        currentCryotharPhase = CryotharPhase.Wait;
    }

    protected override void OnEnable() 
    {
        currentRoomNetData = GameSessionManager.Instance.GetCurrentRoomNetData();
    }

    protected override void OnDisable() { }

    protected override void FixedUpdate() { }

    protected override void Update()
    {
        if (!isServer || !enemyFullyInitialized) return;

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

        if (targetPlayer != null)
        {
            Vector3 direction = (targetPlayer.GetPlayerPosition() - transform.position).normalized;
            attackLockedVector = direction;
        }

        // Initialize vectors, angles, directions and aim
        float unitAngle = HelperUtilities.GetAngleFromVector(attackLockedVector);
        AimDirection unitAimDirection = HelperUtilities.GetAimDirection(unitAngle);
        AttackDirection attackDirection = HelperUtilities.GetAttackDirection(unitAngle);
        enemy.aimWeapon.Aim(unitAimDirection, attackDirection, unitAngle, EnemyCategory.Cryothar);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimParameters(unitAimDirection);

        // Update timers - Fire Projectile
        firingIntervalTimer -= Time.deltaTime;

        HasNegativeMoveStatusEffect();

        if (enemy.moveStatus == MoveStatus.Idle)
        {
            // Check if the player is on stealth
            if (targetPlayer != null && targetPlayer.isStealthActive)
            {
                PlayerStealthCheck();
            }

            // Check if the enemy is a Frost Wrym boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Cryothar)
            {
                // Handle phases based on currentPhase
                switch (currentCryotharPhase)
                {
                    case CryotharPhase.Wait:
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

                    case CryotharPhase.IceProjectile:
                        HandleIceProjectile();
                        break;

                    case CryotharPhase.TailAttack:
                        HandleTailAttack();
                        break;

                    case CryotharPhase.Icicle:
                        HandleIcicle();
                        break;

                    case CryotharPhase.FrostBreath:
                        HandleFrostBreath();
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

    private void HandleIceProjectile()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (cryotharAttackMoveRoutine == null)
        {
            cryotharAttackMoveRoutine = StartCoroutine(AttackRoutine(CryotharPhase.IceProjectile));
        }
    }

    private void HandleTailAttack()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (cryotharAttackMoveRoutine == null)
        {
            cryotharAttackMoveRoutine = StartCoroutine(AttackRoutine(CryotharPhase.TailAttack));
        }
    }

    private void HandleIcicle()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (cryotharAttackMoveRoutine == null)
        {
            cryotharAttackMoveRoutine = StartCoroutine(AttackRoutine(CryotharPhase.Icicle));
        }
    }

    private void HandleFrostBreath()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (cryotharAttackMoveRoutine == null)
        {
            cryotharAttackMoveRoutine = StartCoroutine(AttackRoutine(CryotharPhase.FrostBreath));
        }
    }

    private void TransitionToNextPhase()
    {
        // Check if the player is on stealth
        if (targetPlayer != null && targetPlayer.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }


        if (targetPlayer != null)
        {
            if (Vector3.Distance(transform.position, targetPlayer.GetPlayerPosition()) < 2f)
            {
                // If player is too close to boss, automatically next phase will be TailAttack or FrostBreath
                currentCryotharPhase = (CryotharPhase)Random.Range(4, Enum.GetValues(typeof(CryotharPhase)).Length);
                return;
            }
            else if (Vector3.Distance(transform.position, targetPlayer.GetPlayerPosition()) > 10f)
            {
                // If player is too far to boss, automatically next phase will be Icicle,
                currentCryotharPhase = CryotharPhase.Icicle;
                return;
            }
        }

        if (currentCryotharPhase == CryotharPhase.TailAttack || currentCryotharPhase == CryotharPhase.Icicle ||
            currentCryotharPhase == CryotharPhase.IceProjectile || currentCryotharPhase == CryotharPhase.FrostBreath)
        {
            // If centaur made a move then next phase will be wait
            currentCryotharPhase = CryotharPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentCryotharPhase = (CryotharPhase)Random.Range(2, Enum.GetValues(typeof(CryotharPhase)).Length);
        }
    }

    IEnumerator AttackRoutine(CryotharPhase frostWrymPhase)
    {
        if (frostWrymPhase == CryotharPhase.IceProjectile)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 2);

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 0.6f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, true);

            yield return null;

            enemy.animator.SetBool(Settings.isAttack, true);

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
                        FireWeapon(isLaser: false, ProjectileKind.Default, new AttackContext { cryotharPhase = CryotharPhase.IceProjectile });
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
            previousCryotharPhase = CryotharPhase.IceProjectile;

        }
        else if (frostWrymPhase == CryotharPhase.TailAttack)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 0);

            enemyPhase = EnemyPhase.Attack;
            isAttacking = true;

            // PREPARE PRECHARGE PHASE
            // Lock-on player position during the start of precharge
            if (!chargeProcessStarted && targetPlayer != null)
            {
                lockedPosition = targetPlayer.transform.position + new Vector3(0f, 0.5f, 0f);
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
            InstantiatedRoom ir = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
            Grid grid = ir.grid;

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

            chargeTimer = 0f;

            yield return null;

            // Location change completed now starting sword smear process starts if player is close to the enemy
            float smearDuration = 1f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();

            Vector3 playerDirectionVector = new Vector3();

            if (GameManager.Instance.GetLocalPlayer() != null)
            {
                playerDirectionVector = GameManager.Instance.GetLocalPlayer().GetPlayerPosition() - transform.position;
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

                            DamageContext ctx = new DamageContext { dealerPosition = transform.position};
                            ReceiveProjectileDamage receiveProjectileDamage = collider.GetComponent<ReceiveProjectileDamage>();

                            // Evasiveness - dodge check
                            if (100 - (player.currentDodgeValue + blindPenalty) * 100 > Random.Range(1, 101))
                            {
                                ctx.receiverPosition = player.transform.position;
                                receiveProjectileDamage.TakeProjectileDamage(25, ctx);

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
                                receiveProjectileDamage.TakeProjectileDamage(0, ctx);
                            }
                        }
                    }
                }

                yield return null;

                //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.attackSoundEffect);
            }

            enemy.animator.SetBool(Settings.isAttack, false);
            enemy.animateEnemy.SetIdleAnimationParameters();

            isAttacking = false;

            previousCryotharPhase = CryotharPhase.TailAttack;
        }
        else if (frostWrymPhase == CryotharPhase.Icicle)
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

            //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.chargeSoundEffect);

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
                        FireWeapon(isLaser: false, ProjectileKind.Default, new AttackContext { cryotharPhase = CryotharPhase.Icicle });
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

            previousCryotharPhase = CryotharPhase.Icicle;
        }
        else if (frostWrymPhase == CryotharPhase.FrostBreath)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetFloat(Settings.motionType, -1f);
            enemy.animator.SetInteger(Settings.attackType, 0);

            enemyPhase = EnemyPhase.Attack;
            isAttacking = true;

            // PREPARE PRECHARGE PHASE
            // Lock-on player position during the start of precharge
            if (!chargeProcessStarted && GameManager.Instance.GetLocalPlayer() != null)
            {
                lockedPosition = GameManager.Instance.GetLocalPlayer().transform.position + new Vector3(0f, 0.5f, 0f);
            }

            chargeProcessStarted = true;

            // Pre-check if moving towards player is necessary 
            if (Vector3.Distance(transform.position, lockedPosition) < 3f)  // Small threshold for accuracy
            {
                // Exit the loop early if boss has reached the destination
                goto skipRun;
            }

            float prechargeDuration = 1.5f;
            float chargeTimer = 0f;

            while (chargeTimer < prechargeDuration)
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

            Vector3 direction = (lockedPosition - transform.position).normalized;
            float chargeSpeed = 20f;

            while (chargeTimer < chargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;
                transform.position = Vector3.MoveTowards(transform.position, lockedPosition, chargeSpeed * Time.deltaTime);

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
            float iceBreathDuration = 1f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();

            Vector3 playerDirectionVector = new Vector3();

            if (GameManager.Instance.GetLocalPlayer() != null)
            {
                playerDirectionVector = GameManager.Instance.GetLocalPlayer().GetPlayerPosition() - transform.position;
            }

            yield return null;

            while (chargeTimer < iceBreathDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                enemy.animator.SetBool(Settings.cast, true);

                foreach (Collider2D collider in Physics2D.OverlapCircleAll(swordHoldingTransform.position, smearCircleRadius))
                {
                    if (enemy.health.hasDied) yield break;

                    if (collider.GetType() == typeof(PolygonCollider2D))
                    {
                        // Don't hit yourself if player is also in the collider list
                        if (collider.tag == Settings.enemyTag) continue;

                        if (collider.tag == Settings.chestItemTag) continue;

                        if (playerHealth = collider.GetComponent<Health>())
                        {
                            Player player = collider.GetComponent<Player>();
                            float blindPenalty = enemy.isBlind ? 0.5f : 0f;

                            DamageContext ctx = new DamageContext { dealerPosition = transform.position };
                            ReceiveProjectileDamage receiveProjectileDamage = collider.GetComponent<ReceiveProjectileDamage>();

                            // Evasiveness - dodge check
                            if (100 - (player.currentDodgeValue + blindPenalty) * 100 > Random.Range(1, 101))
                            {
                                ctx.receiverPosition = player.transform.position;
                                receiveProjectileDamage.TakeProjectileDamage(25, ctx);

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
                                receiveProjectileDamage.TakeProjectileDamage(0, ctx);
                            }
                        }
                    }
                }

                yield return null;

                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);
            }

            enemy.animator.SetBool(Settings.cast, false);
            enemy.animateEnemy.SetIdleAnimationParameters();

            isAttacking = false;

            previousCryotharPhase = CryotharPhase.FrostBreath;
        }

        chargeProcessStarted = false;
        cryotharAttackMoveRoutine = null;

        TransitionToNextPhase();
    }

    public void PlayerStealthCheck()
    {
        currentCryotharPhase = CryotharPhase.Wait;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, smearCircleRadius);
    }
}
