using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using Random = UnityEngine.Random;

public class MoravelleAI : EnemyAI, IMutualBossBehaviour
{
    // Define the cell boundaries in grid coordinates
    readonly Vector2Int cellMin = new Vector2Int(-8, 2);
    readonly Vector2Int cellMax = new Vector2Int(12, 18);

    // BOSSES
    MoravellePhase currentMoravellePhase;
    private float phaseTimer;  // Timer to control phase duration
    private float waitPhase = 0.5f;  // Adjust this to control how long each phase lasts

    Vector3 lockedPosition;
    bool chargeProcessStarted;

    public UnityEvent resetAnimationEvent;

    Coroutine moravelleAttackMoveRoutine;

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
        currentMoravellePhase = MoravellePhase.Wait;
    }

    protected override void OnEnable() 
    {
        resetAnimationEvent.AddListener(ResetAnimations);
    }

    protected override void OnDisable() 
    {
        resetAnimationEvent.RemoveListener(ResetAnimations);
    }

    public void ResetAnimationEvent()
    {
        resetAnimationEvent?.Invoke();
    }

    private void ResetAnimations()
    {
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animator.SetFloat(Settings.motionType, 0);
        enemy.animator.SetBool(Settings.focused, false);
        enemy.animator.SetBool(Settings.cast, false);
    }

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
            if (player.isStealthActive)
            {
                PlayerStealthCheck();
            }

            // Check if the enemy is a Moravelle boss
            if (enemyDetails.enemyBehaviour == EnemyBehaviour.Moravelle)
            {
                // Handle phases based on currentPhase
                switch (currentMoravellePhase)
                {
                    case MoravellePhase.Wait:

                        enemy.animator.SetBool(Settings.charge, false);
                        enemy.animator.SetBool(Settings.focused, false);
                        enemy.animator.SetBool(Settings.cast, false);

                        if (Time.frameCount % 20 == 0)
                        {
                            Debug.Log("Current phase is " + currentMoravellePhase.ToString());
                        }

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

                    case MoravellePhase.StraightArrowShot:
                        HandleStraightArrowShot();
                        break;

                    case MoravellePhase.ChargeAndRetreat:
                        HandleChargeAndRetreat();
                        break;

                    case MoravellePhase.SpreadArrowShot:
                        HandleSpreadArrowShot();
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
        enemy.animateEnemy.ResetAnimatonParameters();
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    private void HandleStraightArrowShot()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (moravelleAttackMoveRoutine == null)
        {
            moravelleAttackMoveRoutine = StartCoroutine(AttackRoutine(MoravellePhase.StraightArrowShot));
        }
    }

    private void HandleChargeAndRetreat()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (moravelleAttackMoveRoutine == null)
        {
            moravelleAttackMoveRoutine = StartCoroutine(AttackRoutine(MoravellePhase.ChargeAndRetreat));
        }
    }

    private void HandleSpreadArrowShot()
    {
        enemy.animateEnemy.ResetAnimatonParameters();

        if (moravelleAttackMoveRoutine == null)
        {
            moravelleAttackMoveRoutine = StartCoroutine(AttackRoutine(MoravellePhase.SpreadArrowShot));
        }
    }

    private void TransitionToNextPhase()
    {
        if (player == null) return;

        // Check if the player is on stealth
        if (player.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        if (Vector3.Distance(transform.position, player.GetPlayerPosition()) < 4f)
        {
            int rng = Random.Range(0, 101);

            if (rng > 65)
            {
                // If player is too close to Moravelle, automatically next phase will be chargeAndRetreat
                currentMoravellePhase = MoravellePhase.ChargeAndRetreat;
                return;
            }
        }

        if (currentMoravellePhase == MoravellePhase.StraightArrowShot || currentMoravellePhase == MoravellePhase.ChargeAndRetreat ||
            currentMoravellePhase == MoravellePhase.SpreadArrowShot)
        {
            // If Moravelle made a move then next phase will be wait
            currentMoravellePhase = MoravellePhase.Wait;
        }
        else
        {
            // Example of conditional or random phase transitions
            currentMoravellePhase = (MoravellePhase)Random.Range(2, Enum.GetValues(typeof(MoravellePhase)).Length);
        }
    }

    IEnumerator AttackRoutine(MoravellePhase moravellePhase)
    {
        if (moravellePhase == MoravellePhase.StraightArrowShot)
        {
            if (enemy.health.hasDied) yield break;

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
                        enemy.animator.SetBool(Settings.cast, true);

                        yield return null;

                        FireWeapon();
                    }
                    else
                    {
                        // Reset timers and animation
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();
                        enemy.animator.SetBool(Settings.cast, false);
                        enemy.animateEnemy.SetIdleAnimationParameters();
                    }
                }


                yield return null;
            }
        }
        else if (moravellePhase == MoravellePhase.ChargeAndRetreat)
        {
            if (enemy.health.hasDied) yield break;

            isAttacking = true;

            // Lock-on player position during the start of precharge
            if (!chargeProcessStarted)
            {
                if (player != null)
                {
                    lockedPosition = player.GetPlayerPosition();
                }
            }

            chargeProcessStarted = true;

            // ----- PRECHARGE PHASE -----
            float prechargeDuration = 0.4f;
            float chargeTimer = 0f;

            enemy.animator.SetBool(Settings.charge, true);
            //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.roarSoundEffect);

            while (chargeTimer < prechargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                yield return null;
            }

            // ----- CHARGE PHASE START -----
            chargeTimer = 0f;
            float chargeDuration = 1f;

            //// Reset animation, enter movement mode
            //enemy.animateEnemy.ResetAnimatonParameters();

            // Clamp destination inside boss room bounds
            Grid grid = GameManager.Instance.GetBossRoom().instantiatedRoom.grid;
            Vector3Int cellPosition = grid.WorldToCell(lockedPosition);
            cellPosition.x = Mathf.Clamp(cellPosition.x, cellMin.x, cellMax.x);
            cellPosition.y = Mathf.Clamp(cellPosition.y, cellMin.y, cellMax.y);
            Vector3 clampedPosition = grid.GetCellCenterWorld(cellPosition);

            float chargeSpeed = 20f;

            while (chargeTimer < chargeDuration)
            {
                if (enemy.health.hasDied) yield break;

                chargeTimer += Time.deltaTime;

                Vector3 moveDir = (clampedPosition - transform.position).normalized;
                transform.position = Vector3.MoveTowards(transform.position, clampedPosition, chargeSpeed * Time.deltaTime);
                float degree = HelperUtilities.GetAngleFromVector(moveDir);
                AimDirection aimDirection = HelperUtilities.GetAimDirection(degree);

                //// Update movement animation based on direction every frame
                //enemy.animateEnemy.SetAimWeaponAnimationParameters(aimDirection);
                //enemy.animateEnemy.SetMovementAnimationParameters();

                if (Vector3.Distance(transform.position, clampedPosition) < 0.02f)
                {
                    enemy.animateEnemy.ResetAnimatonParameters();
                    enemy.animateEnemy.SetIdleAnimationParameters();
                    break; // Stop early if destination reached
                }

                yield return null;
            }

            // Revert to the idle state after charge completed
            enemy.animateEnemy.SetIdleAnimationParameters();
            chargeTimer = 0f;

            yield return null;

            isAttacking = false;
        }
        else if (moravellePhase == MoravellePhase.SpreadArrowShot)
        {
            if (enemy.health.hasDied) yield break;

            // PREPARE PRECHARGE PHASE
            float prechargeDuration = 0.8f;
            float chargeTimer = 0f;

            // Set the motion type for the precharge phase
            enemy.animator.SetBool(Settings.focused, true);

            yield return null;

            while (chargeTimer < prechargeDuration)
            {
                chargeTimer += Time.deltaTime;

                yield return null;
            }

            chargeTimer = 0f;

            yield return null;  // Wait for the animation to start

            // START CHARGE PHASE

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
                        FireWeapon(false, MoravellePhase.SpreadArrowShot);
                    }
                    else
                    {
                        // Reset timers
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();
                        enemy.animator.SetBool(Settings.focused, false);
                        enemy.animateEnemy.SetIdleAnimationParameters();
                    }
                }

                yield return null;
            }

            yield return null;
        }

        chargeProcessStarted = false;
        moravelleAttackMoveRoutine = null;

        TransitionToNextPhase();
    }

    public void PlayerStealthCheck()
    {
        // Check if the player is on stealth
        currentMoravellePhase = MoravellePhase.Wait;
    }
}
