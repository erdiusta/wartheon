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

    protected override void FixedUpdate() 
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

            Debug.LogWarning("Moravelle was outside bounds. Snapped back.");
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
        firingIntervalTimer -= Time.fixedDeltaTime;

        HasNegativeMoveStatusEffect();

        if (moveStatus == MoveStatus.Idle)
        {
            if (player.isStealthActive)
            {
                PlayerStealthCheck();
            }

            switch (currentMoravellePhase)
            {
                case MoravellePhase.Wait:
                    enemy.animator.SetBool(Settings.charge, false);
                    enemy.animator.SetBool(Settings.focused, false);
                    enemy.animator.SetBool(Settings.cast, false);

                    PassedToWait = true;

                    firingIntervalTimer = WeaponShootInterval();
                    firingDurationTimer = WeaponShootDuration();

                    phaseTimer += Time.fixedDeltaTime;
                    if (phaseTimer >= waitPhase)
                    {
                        TransitionToNextPhase();
                        phaseTimer = 0f;
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
            }
        }
    }

    protected override void Update()
    {

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

        if (player.isStealthActive)
        {
            PlayerStealthCheck();
            return;
        }

        if (Vector3.Distance(transform.position, player.GetPlayerPosition()) < 4f)
        {
            if (Random.Range(0, 101) > 65)
            {
                currentMoravellePhase = MoravellePhase.ChargeAndRetreat;
                return;
            }
        }

        if (currentMoravellePhase == MoravellePhase.StraightArrowShot ||
            currentMoravellePhase == MoravellePhase.ChargeAndRetreat ||
            currentMoravellePhase == MoravellePhase.SpreadArrowShot)
        {
            currentMoravellePhase = MoravellePhase.Wait;
        }
        else
        {
            currentMoravellePhase = (MoravellePhase)Random.Range(2, Enum.GetValues(typeof(MoravellePhase)).Length);
        }
    }

    IEnumerator AttackRoutine(MoravellePhase moravellePhase)
    {
        if (enemy.health.hasDied) yield break;

        if (moravellePhase == MoravellePhase.StraightArrowShot)
        {
            float fireTimer = 0f;
            float fireProjectileDuration = 5f;

            yield return null;

            while (fireTimer < fireProjectileDuration)
            {
                if (enemy.health.hasDied) yield break;

                fireTimer += Time.fixedDeltaTime;

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
            isAttacking = true;

            if (!chargeProcessStarted && player != null)
            {
                lockedPosition = ClampToBossRoom(player.GetPlayerPosition(), cellMin, cellMax);
            }

            chargeProcessStarted = true;

            float prechargeDuration = 0.4f;
            float chargeTimer = 0f;
            enemy.animator.SetBool(Settings.charge, true);

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
        }
        else if (moravellePhase == MoravellePhase.SpreadArrowShot)
        {
            float prechargeDuration = 0.8f;
            float chargeTimer = 0f;
            enemy.animator.SetBool(Settings.focused, true);
            yield return null;

            while (chargeTimer < prechargeDuration)
            {
                chargeTimer += Time.fixedDeltaTime;
                yield return null;
            }

            float fireTimer = 0f;
            float fireProjectileDuration = enemy.enemyDetails.enemyWeapon.weaponCooldownDuration;

            while (fireTimer < fireProjectileDuration)
            {
                if (enemy.health.hasDied) yield break;

                fireTimer += Time.fixedDeltaTime;

                if (firingIntervalTimer < 0f)
                {
                    if (firingDurationTimer >= 0)
                    {
                        firingDurationTimer -= Time.fixedDeltaTime;
                        FireWeapon(false, MoravellePhase.SpreadArrowShot);
                    }
                    else
                    {
                        firingIntervalTimer = WeaponShootInterval();
                        firingDurationTimer = WeaponShootDuration();
                        enemy.animator.SetBool(Settings.focused, false);
                        enemy.animateEnemy.SetIdleAnimationParameters();
                    }
                }

                yield return null;
            }
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
