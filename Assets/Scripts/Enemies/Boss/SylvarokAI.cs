using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

public class SylvarokAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-4, 6);
    readonly Vector2Int cellMax = new Vector2Int(8, 14);

    // BOSS
    SylvarokPhase currentSylvarokPhase;
    SylvarokPhase previousSylvarokPhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.5f;  // Adjust this to control how long each phase lasts

    Vector3 lockedPosition;
    bool chargeProcessStarted;
    int enemiesToSpawn = 2;

    Coroutine sylvarokAttackMoveRoutine;

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
        currentSylvarokPhase = SylvarokPhase.Wait;
        previousSylvarokPhase = SylvarokPhase.Wait;
    }

    protected override void OnEnable() 
    {
        player = GameManager.Instance.GetPlayer();
        currentRoom = GameManager.Instance.GetCurrentRoom();
    }

    protected override void OnDisable() { }

    protected override void FixedUpdate()
    {

    }

    protected override void Update()
    {
        prevVel = rb2D.linearVelocity;

        if (enemy.enemyAI.enemyPhase == EnemyPhase.Death)
        {
            if (attackAnimationRoutine != null)
            {
                StopCoroutine(attackAnimationRoutine);
            }

            return;
        }

        // Emergency pullback if Moravelle drifts outside bounds
        if (IsOutsideBossRoom(transform.position, cellMin, cellMax))
        {
            Vector3 safePos = ClampToBossRoom(transform.position, cellMin, cellMax);
            transform.position = safePos;
            rb2D.linearVelocity = Vector2.zero;

            Debug.LogWarning("Sylvarok was outside bounds. Snapped back.");
        }

        Vector3 direction = Vector3.zero;

        if (player != null)
        {
            direction = GameManager.Instance.GetDecoy() != null ? (GameManager.Instance.GetDecoy().GetDecoyPosition() - transform.position).normalized :
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

        if (moveStatus == MoveStatus.Idle)
        {
            if (player.isStealthActive)
            {
                PlayerStealthCheck();
            }

            switch (currentSylvarokPhase)
            {
                case SylvarokPhase.None:
                    break;
                case SylvarokPhase.Wait:
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
                case SylvarokPhase.StraightAttack:
                    HandleStraightAttack();
                    break;
                case SylvarokPhase.RazorLeaf:
                    HandleRazorLeaf();
                    break;
                case SylvarokPhase.Summon:
                    HandleSummon();
                    break;
                case SylvarokPhase.Heal:
                    HandleHeal();
                    break;
                default:
                    break;
            }
        }
    }

    public void HandleWaitPhase()
    {
        // Logic for waiting phase (maybe the Centaur just moves or idles here)
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    private void HandleStraightAttack()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (sylvarokAttackMoveRoutine == null)
        {
            sylvarokAttackMoveRoutine = StartCoroutine(AttackRoutine(SylvarokPhase.StraightAttack));
        }
    }

    private void HandleSummon()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (sylvarokAttackMoveRoutine == null)
        {
            sylvarokAttackMoveRoutine = StartCoroutine(AttackRoutine(SylvarokPhase.Summon));
        }
    }

    private void HandleHeal()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (sylvarokAttackMoveRoutine == null)
        {
            sylvarokAttackMoveRoutine = StartCoroutine(AttackRoutine(SylvarokPhase.Heal));
        }
    }

    private void HandleRazorLeaf()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (sylvarokAttackMoveRoutine == null)
        {
            sylvarokAttackMoveRoutine = StartCoroutine(AttackRoutine(SylvarokPhase.RazorLeaf));
        }
    }

    private void TransitionToNextPhase()
    {
        // Check if the player is on stealth
        if (player.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        if (player != null && Vector3.Distance(transform.position, player.GetPlayerPosition()) < 4f)
        {
            // If player is too close to treant, automatically next phase will be chargeAndRetreat or razorLeaf
            currentSylvarokPhase = (SylvarokPhase)Random.Range(2, 4);
            return;
        }
        
        if (currentSylvarokPhase == SylvarokPhase.StraightAttack || currentSylvarokPhase == SylvarokPhase.RazorLeaf ||
            currentSylvarokPhase == SylvarokPhase.Heal || currentSylvarokPhase == SylvarokPhase.Summon)
        {
            // If treant made a move then next phase will be wait
            currentSylvarokPhase = SylvarokPhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentSylvarokPhase = (SylvarokPhase)Random.Range(2, Enum.GetValues(typeof(SylvarokPhase)).Length);

            // If health is not low enough, switch heal phase
            if (currentSylvarokPhase == SylvarokPhase.Heal && enemy.health.GetCurrentHealth() > (int)(enemy.health.GetMaximumHealth() * 0.65f))
            {
                currentSylvarokPhase = (SylvarokPhase)Random.Range(2, Enum.GetValues(typeof(SylvarokPhase)).Length - 1);
            }
        }
    }

    IEnumerator AttackRoutine(SylvarokPhase treantPhase)
    {
        if (treantPhase == SylvarokPhase.StraightAttack)
        {
            if (enemy.health.hasDied) yield break;

            enemyPhase = EnemyPhase.Attack;
            isAttacking = true;

            // PREPARE PRECHARGE PHASE
            // Lock-on player position during the start of precharge
            if (!chargeProcessStarted)
            {
                if (player != null)
                {
                    lockedPosition = player.GetPlayerPosition();
                }
            }

            chargeProcessStarted = true;

            float prehargeDuration = 1.5f;
            float chargeTimer = 0f;
            enemy.animator.SetFloat(Settings.motionType, 3f); // charge trigger to blend tree
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

            while (chargeTimer < prehargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                yield return null;
            }

            chargeTimer = 0f;
            float chargeDuration = 1.4f;

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
            float chargeSpeed = 14f;

            while (chargeTimer < chargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                transform.position = Vector3.MoveTowards(transform.position, clampedPosition, chargeSpeed * Time.deltaTime);

                // Check if boss has reached the destination before the desired duration
                if (Vector3.Distance(transform.position, clampedPosition) < 0.1f)  // Small threshold for accuracy
                {
                    if (enemy.health.hasDied) yield break;

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

            yield return null;

            previousSylvarokPhase = SylvarokPhase.StraightAttack;
        }
        else if (treantPhase == SylvarokPhase.RazorLeaf)
        {
            if (enemy.health.hasDied) yield break;

            enemyPhase = EnemyPhase.Chase;

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 1f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.cast, true);

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
                        FireWeapon(false, 0, SylvarokPhase.RazorLeaf);
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

            previousSylvarokPhase = SylvarokPhase.RazorLeaf;
        }
        else if (treantPhase == SylvarokPhase.Summon)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetBool("summon", true);

            Grid grid = currentRoom.instantiatedRoom.grid;

            // Create an instance of the helper class used to select a random enemy
            RandomSpawnableObject<EnemyDetailsSO> randomEnemyHelperClass = new RandomSpawnableObject<EnemyDetailsSO>(currentRoom.enemiesByLevelList);

            SoundEffectManager.Instance.PlaySoundEffect(enemyDetails.attackSoundEffect);

            // Check we have somewhere to spawn the enemies
            if (currentRoom.spawnPositionArray.Length > 0)
            {
                // Loop through to create all the enemeies
                for (int i = 0; i < enemiesToSpawn; i++)
                {
                    Vector3Int cellPosition = (Vector3Int)currentRoom.spawnPositionArray[Random.Range(0, currentRoom.spawnPositionArray.Length)];

                    // Create Enemy - Get next enemy type to spawn 
                    EnemySpawner.Instance.CreateEnemy(enemyDetails.enemyMinionDetails, grid.CellToWorld(cellPosition));
                }
            }

            yield return new WaitForSeconds(1.5f);

            enemy.animator.SetBool("summon", false);
            previousSylvarokPhase = SylvarokPhase.Summon;
        }
        else if (treantPhase == SylvarokPhase.Heal)
        {
            if (enemy.health.hasDied) yield break;

            enemy.animator.SetBool("heal", true);

            enemy.health.AddHealth(20);

            SoundEffectManager.Instance.PlaySoundEffect(enemyDetails.chargeSoundEffect);

            yield return new WaitForSeconds(1.5f);

            enemy.animator.SetBool("heal", false);
        }

        chargeProcessStarted = false;
        sylvarokAttackMoveRoutine = null;

        TransitionToNextPhase();

        previousSylvarokPhase = SylvarokPhase.Heal;
    }

    public void PlayerStealthCheck()
    {
        currentSylvarokPhase = SylvarokPhase.Wait;
    }
}
