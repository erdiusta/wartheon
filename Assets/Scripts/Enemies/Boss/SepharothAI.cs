using System;
using System.Collections;
using UnityEngine;
using Color = UnityEngine.Color;
using Random = UnityEngine.Random;

public class SepharothAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSSES
    [SerializeField] Transform swordHoldingTransform;
    [SerializeField] float smearCircleRadius = 0.5f;

    SepharothPhase currentSepharothPhase;
    SepharothPhase previousSepharothPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.2f;  // Adjust this to control how long each phase lasts

    Vector3 lockedPosition;
    bool chargeProcessStarted;
    Health playerHealth;
    SpriteRenderer spriteRenderer;

    Coroutine sepharothAttackMoveRoutine;

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
        currentSepharothPhase = SepharothPhase.Wait;
        previousSepharothPhase = SepharothPhase.Wait;

        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    protected override void OnEnable()
    {
        player = GameManager.Instance.GetPlayer();
        currentRoom = GameManager.Instance.GetCurrentRoom();
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

        Vector3 direction = new Vector3();

        if (player != null)
        {
            direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() - transform.position).normalized :
            (player.GetPlayerPosition() - transform.position).normalized;
        }

        lockedVector = direction;

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
            // Check if the player is on stealth
            if (GameManager.Instance.GetPlayer().isStealthActive)
            {
                PlayerStealthCheck();
            }

            // Check if the enemy is a Sepharoth boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Sepharoth)
            {
                // Handle phases based on currentPhase
                switch (currentSepharothPhase)
                {
                    case SepharothPhase.Wait:
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

                    case SepharothPhase.InvisibleAndMine:
                        HandleSmearProjectile();
                        break;

                    case SepharothPhase.SmearAttack:
                        HandleSmearAttack();
                        break;

                    case SepharothPhase.LaserBeam:
                        HandleLaserBeam();
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

    private void HandleSmearProjectile()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (sepharothAttackMoveRoutine == null)
        {
            sepharothAttackMoveRoutine = StartCoroutine(AttackRoutine(SepharothPhase.InvisibleAndMine));
        }
    }

    private void HandleSmearAttack()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (sepharothAttackMoveRoutine == null)
        {
            sepharothAttackMoveRoutine = StartCoroutine(AttackRoutine(SepharothPhase.SmearAttack));
        }
    }

    private void HandleLaserBeam()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (sepharothAttackMoveRoutine == null)
        {
            sepharothAttackMoveRoutine = StartCoroutine(AttackRoutine(SepharothPhase.LaserBeam));
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

        if (GameManager.Instance.GetPlayer() != null)
        {
            if (Vector3.Distance(transform.position, player.transform.position) < 4f)
            {
                // If player is too close to boss, automatically next phase will be smear attack most probably
                int randomNum = Random.Range(0, 101);

                if (randomNum < 40)
                {
                    currentSepharothPhase = SepharothPhase.SmearAttack;
                    return;
                }
                else if (randomNum < 70)
                {
                    currentSepharothPhase = SepharothPhase.LaserBeam;
                    return;
                }
                else
                {
                    currentSepharothPhase = (SepharothPhase)Random.Range(2, Enum.GetValues(typeof(SepharothPhase)).Length);
                }
            }
            else if (Vector3.Distance(transform.position, player.transform.position) > 12f)
            {
                currentSepharothPhase = SepharothPhase.LaserBeam;
                return;
            }
        }

        if (currentSepharothPhase == SepharothPhase.SmearAttack || currentSepharothPhase == SepharothPhase.LaserBeam ||
            currentSepharothPhase == SepharothPhase.InvisibleAndMine)
        {
            // If boss made a move then next phase will be wait
            currentSepharothPhase = SepharothPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            if(previousSepharothPhase == SepharothPhase.LaserBeam)
            {
                currentSepharothPhase = (SepharothPhase)Random.Range(2, Enum.GetValues(typeof(SepharothPhase)).Length - 1);
            }
            else
            {
                currentSepharothPhase = (SepharothPhase)Random.Range(2, Enum.GetValues(typeof(SepharothPhase)).Length);
            }
        }
    }

    IEnumerator AttackRoutine(SepharothPhase sepharothPhase)
    {
        if (sepharothPhase == SepharothPhase.InvisibleAndMine)
        {
            if (enemy.health.hasDied) yield break;

            #region Invisibility
            // BEING INVISIBLE
            enemyPhase = EnemyPhase.Chase;

            float completeInvisibleDuration = 1.5f;
            float invisibleTimer = 0f;
            enemy.GetComponent<PolygonCollider2D>().enabled = false;
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

            // Become invisible
            while (invisibleTimer < completeInvisibleDuration)
            {
                if (enemy.health.hasDied) yield break;

                invisibleTimer += Time.deltaTime;

                float newAlpha = 0.8f; // Default to start value

                if (invisibleTimer > 1.2f) newAlpha = 0f;
                else if (invisibleTimer > 0.9f) newAlpha = 0.2f;
                else if (invisibleTimer > 0.6f) newAlpha = 0.4f;
                else if (invisibleTimer > 0.3f) newAlpha = 0.6f;

                // Apply alpha change
                Color color = spriteRenderer.color;
                color.a = newAlpha;
                spriteRenderer.color = color;

                yield return null;
            }

            yield return null;
            #endregion

            #region Teleport
            // TELEPORT TO NEW POSITON DURING INVISIBLE
            Grid grid = currentRoom.instantiatedRoom.grid;

            // Selected second mine' position
            int selectedIndexNum = Random.Range(0, currentRoom.spawnPositionArray.Length);
            Vector3Int selectedSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[selectedIndexNum].x, currentRoom.spawnPositionArray[selectedIndexNum].y, 0);

            // Convert the cell position to world position
            transform.position = grid.CellToWorld(selectedSpawnPoint);

            float minePlantDuration = 5f;
            float mineTimer = 0f;

            yield return null;
            #endregion

            #region MinePlanting
            // MINE PLANTING
            while (mineTimer < minePlantDuration)
            {
                if (enemy.health.hasDied) yield break;

                mineTimer += Time.deltaTime;

                // Interval Timer
                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.deltaTime;
                        enemy.animateEnemy.SetAttackAnimationParameters();
                        FireWeapon(false, 0, 0, 0, SepharothPhase.InvisibleAndMine);

                    }
                    else
                    {
                        // Reset timers and animation
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();
                        enemy.animateEnemy.SetIdleAnimationParameters();
                    }
                }

                yield return null; // Keep invisible issues active
            }

            yield return null;
            #endregion

            #region BeingVisibleAgain

            // BE VISIBLE AGAIN
            enemy.GetComponent<PolygonCollider2D>().enabled = true;

            invisibleTimer = 0f;
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

            // Apply alpha change
            while (invisibleTimer < completeInvisibleDuration)
            {
                if (enemy.health.hasDied) yield break;

                invisibleTimer += Time.deltaTime;

                float newAlpha = 0.2f; // Default to start value

                if (invisibleTimer > 1.2f) newAlpha = 1f;
                else if (invisibleTimer > 0.9f) newAlpha = 0.8f;
                else if (invisibleTimer > 0.6f) newAlpha = 0.6f;
                else if (invisibleTimer > 0.3f) newAlpha = 0.4f;

                // Apply alpha change
                Color color = spriteRenderer.color;
                color.a = newAlpha;
                spriteRenderer.color = color;

                yield return null;
            }

            #endregion  

            previousSepharothPhase = SepharothPhase.InvisibleAndMine;

        }
        else if (sepharothPhase == SepharothPhase.SmearAttack)
        {
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

                            // Evasiveness - dodge check
                            if (100 - (player.currentEvasivenessValue + blindPenalty) * 100 > Random.Range(1, 101))
                            {
                                playerHealth.TakeDamage(25, transform.position, player.transform.position, false);

                                //SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                // Apply knockback
                                player.movementByVelocity.TriggerKnockback((player.transform.position - transform.position).normalized);
                            }
                            else
                            {
                                player.health.isDodging = true;
                                player.healthEvent.CallDodgeEvent();
                                player.health.PostHitImmunity(true);
                                player.health.TakeDamage(0, transform.position, enemy.health.transform.position, false);
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

            previousSepharothPhase = SepharothPhase.SmearAttack;
        }
        else if (sepharothPhase == SepharothPhase.LaserBeam)
        {
            if (enemy.health.hasDied) yield break;

            enemyPhase = EnemyPhase.Chase;

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 1f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, true);
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.chargeSoundEffect);

            yield return null;

            // Locked Player direction vector
            Vector3 playerDirectionVector = new Vector3();

            if (GameManager.Instance.GetPlayer() != null)
            {
                playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;
            }

            // Locked enemy to player angle
            float enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);

            // Locked enemy aim direction
            AimDirection enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);

            // Locked enemy attack direction
            AttackDirection enemyAttackDirection = HelperUtilities.GetAttackDirection(enemyAngleDegrees);

            while (chargeTimer < prechargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                yield return null;
            }

            chargeTimer = 0f;

            yield return null;  // Wait for the animation to start

            // START CHARGE PHASE

            float fireTimer = 0f;
            float fireProjectileDuration = enemy.enemyDetails.enemyWeapon.weaponCooldownDuration;
            GetComponent<Enemy>().isFiring = false; // Reset firing before laser shot

            // **Fire laser once and hold it for the full duration**
            FireWeapon(playerDirectionVector, enemyAngleDegrees, enemyAimDirection, enemyAttackDirection, true, 0, 0, 0, SepharothPhase.LaserBeam);

            while (fireTimer < fireProjectileDuration)
            {
                if (enemy.health.hasDied) yield break;

                fireTimer += Time.deltaTime;

                yield return null; // Keep laser active
            }

            yield return null;

            previousSepharothPhase = SepharothPhase.LaserBeam;
        }

        chargeProcessStarted = false;
        sepharothAttackMoveRoutine = null;

        enemy.animator.SetBool(Settings.cast, false);
        enemy.animateEnemy.SetIdleAnimationParameters();

        TransitionToNextPhase();
    }

    public void PlayerStealthCheck()
    {
        // Check if the player is on stealth
        currentSepharothPhase = SepharothPhase.Wait;
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(swordHoldingTransform.position, smearCircleRadius);
    }
}
