using System.Collections;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.EventSystems.EventTrigger;
using System.Collections.Generic;

[DisallowMultipleComponent]
public class Projectile : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("Populate with child TrailRenderer component")]
    #endregion Tooltip
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] Animator projectileAnimator;
    [SerializeField] LayerMask layerMask;
    [SerializeField] Transform belongingParent;
    [SerializeField] SpriteRenderer spriteRenderer;

    [HideInInspector] public Coroutine playerBlockCoroutine;

    public ProjectileDetailsSO projectileDetails;

    Player player;
    Enemy belongingEnemy;
    float projectileRange = 0f;
    float projectileSpeed;
    Vector3 fireDirectionVector;
    Vector3 stoppedPosition;
    bool isStopped;
    float fireDirectionAngle;
    float projectileChargeTimer;
    bool isProjectileMaterialSet;
    bool overrideProjectileMovement;
    bool isColliding;
    Vector3 velocity;
    PolygonCollider2D polygonCollider2D;

    ContactFilter2D filter = new ContactFilter2D();

    DropOnAxeThrow dropOnAxeThrow;

    // NYVERAN ARROWS
    bool isPenetrationArrow;
    bool isTripleThreat;
    bool isBindingArrow;
    bool isArrowOfTheSeven;

    // MYCARA SPELLS
    bool isIceBreaker;

    // KYNARA SPELLS
    bool isFireBlast;
    bool isBlazingCyclone;

    // KARNAG SPELLS
    bool isThrowingAxe;

    // NYXA SPELLS
    bool isShiruken;

    // NYMARA SPELLS
    bool isChainLightning;
    [HideInInspector] public float chainJumpRadius = 6f;
    [HideInInspector] public float chainDelay = 0.05f;       // small delay feels nice; set 0 for instant
    [HideInInspector] public float chainDamageFalloff = 0.8f; // 20% less each jump (tune as you like)
    [HideInInspector] public ChainLightningPhase chainLightningPhase;
    // Chain state
    [HideInInspector] public HashSet<Enemy> chainHitSet; // carries across jumps

    float countDown = 3f;
    [SerializeField] float blastRadius = 5f;
    Coroutine explosionRoutine;
    int damageDone = 0;
    bool lightningStroke;
    bool isHittingWall; // Flag is for wall hit check for penetration arrow

    // Guided features
    bool isGuided;

    // Laser features
    [SerializeField] float angleSpeed = 10f;  // How fast the laser extends

    // Laser
    bool isLaserBeam;
    float laserDuration;
    float laserDurationOffset = 0.4f;
    Transform laserStartPoint;

    // Grapple Hook
    [Space(10)]
    [Header("Grapple Settings")]
    [SerializeField] Transform ropeTransform;
    [SerializeField] SpriteRenderer ropeSpriteRenderer;
    [SerializeField] Transform grappleHeadTransform;
    bool isGrappleHook;
    Transform grappleStartPoint;
    bool hasGrappled = false;
    float currentRopeLength = 0f;
    float grapplePullSpeed = 0f;
    bool grappleHasDealtDamage;
    bool isHookedToTheWall;
    bool isGrappleReleased;
    bool missRetractStarted = false;
    LayerMask playerLayer;
    LayerMask poolLayer;

    // Throwing axe
    [HideInInspector] public float maxDamage;

    Vector3 targetPosition;

    // Locked aim field
    [HideInInspector] public Vector3 lockedTargetVector;
    [HideInInspector] public float lockedAngle;
    bool isLocked = false;

    // Lifetime countdown
    float lifeTimeCountdownTimer = 0f;
    Transform target;
    float updateGuidedMissleTimer = 0f;

    // Guided missle features
    Vector2 straightDirection; // Cache this once
    bool directionInitialized = false;

    private void Awake()
    {
        playerLayer = LayerMask.NameToLayer("Player");
        poolLayer = LayerMask.NameToLayer("Pool");

        belongingParent = transform.parent;

        polygonCollider2D = GetComponent<PolygonCollider2D>();

        if(polygonCollider2D == null) polygonCollider2D = GetComponentInChildren<PolygonCollider2D>(); // Grapple head
    }

    private void OnEnable()
    {
        velocity = fireDirectionVector.normalized * projectileSpeed;

        if (isFireBlast)
        {
            blastRadius = projectileDetails.blastRadius;
        }

        if (isGrappleHook)
        {
            grappleStartPoint = transform;

            isGrappleReleased = false;
        }
        else if (isLaserBeam)
        {
            laserStartPoint = transform;
        }

        if (projectileDetails != null && projectileDetails.hasLifeTime)
        {
            lifeTimeCountdownTimer = projectileDetails.lifeDuration;
        }
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();

        // GUIDED MISSLE CHECK
        isGuided = projectileDetails.isGuided;

        if (isChainLightning)
        {
            if (chainLightningPhase == ChainLightningPhase.Second || chainLightningPhase == ChainLightningPhase.Third) isGuided = true;
        }

        damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);

        if (projectileDetails.isPlayerProjectile)
        {
            if (isGuided && target == null)
            {
                if (updateGuidedMissleTimer <= 0f)
                {
                    updateGuidedMissleTimer = 1f;

                    Transform t = FindClosestEnemy();
                    if (t != null && Vector3.Distance(transform.position, t.position) < 5f)
                    {
                        target = t;
                    }
                    else
                    {
                        DisableProjectile();
                    }
                }
            }
        }
        else
        {
            if (isGuided)
            {
                target = GameManager.Instance.GetPlayer().transform;
            }
        }

        if (isLaserBeam && !isGrappleHook)
        {
            laserDuration = belongingEnemy.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration - laserDurationOffset;
        }
    }

    private void Update()
    {
        // Life time time reduces over time
        lifeTimeCountdownTimer -= Time.deltaTime;

        if (projectileDetails != null && projectileDetails.hasLifeTime && lifeTimeCountdownTimer < 0)
        {
            DisableProjectile();
        }

        if (updateGuidedMissleTimer > 0f) updateGuidedMissleTimer -= Time.deltaTime;

        // Projectile charge effect
        if (projectileChargeTimer > 0f)
        {
            projectileChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isProjectileMaterialSet)
        {
            SetProjectileMaterial(projectileDetails.projectileMaterial);
            isProjectileMaterialSet = true;
        }

        if (isGrappleHook)
        {
            player.playersGrapple = this;

            if (player.isHuntersReachActive && player.springJoint2D.enabled)
            {
                InputManager.dodgeRollDisabled = true;

                Vector2 playerPos = player.GetPlayerPosition();
                Vector2 hookPos = player.springJoint2D.connectedAnchor;

                float dist = Vector2.Distance(playerPos, hookPos);

                if (dist <= 2f) // Lowered threshold
                {
                    ReleaseGrapple();
                }
            }

            if (!isGrappleReleased)
            {
                UpdateGrappleHook();
            }
        }
        else if (isLaserBeam)
        {
            UpdateLaser();
        }
        else
        {
            if (isGuided && target != null)
            {
                MoveGuidedProjectile();
            }
            else
            {
                MoveStandardProjectile();
            }
        }     
    }

    private void MoveStandardProjectile(VenomancerPhase venomancerPhase = VenomancerPhase.None)
    {
        // Don't move projectile if movement has been overriden - e.g. this projectile is part of an projectile pattern
        if (!overrideProjectileMovement)
        {
            // Move the projectile based on its velocity
            transform.position += velocity * Time.deltaTime;

            // Disable after max range reached
            projectileRange -= velocity.magnitude * Time.deltaTime;

            if (projectileRange < 0f)
            {
                if (!projectileDetails.isTrap && venomancerPhase != VenomancerPhase.ToxicPool)
                {
                    DisableProjectile();
                }
            }
            if (lightningStroke)
            {
                lightningStroke = false;
                StartCoroutine(DisableProcess(3.5f));
            }
        }
        else
        {
            if (isStopped)
            {
                transform.position = stoppedPosition;
            }
        }
    }

    private void MoveGuidedProjectile()
    {
        if (!overrideProjectileMovement)
        {
            projectileRange -= velocity.magnitude * Time.deltaTime;

            Vector2 directionToTarget;

            if (player.isStealthActive)
            {
                if (!directionInitialized)
                {
                    straightDirection = (target.position + new Vector3(0f, 0.5f, 0f) - transform.position).normalized;
                    directionInitialized = true;
                }

                directionToTarget = straightDirection; // Keep going straight
            }
            else
            {
                directionToTarget = (target.position + new Vector3(0f, 0.5f, 0f) - transform.position).normalized;
                directionInitialized = false; // Reset to re-guide next time
            }

            float targetAngle = HelperUtilities.GetAngleFromVector(directionToTarget);

            transform.rotation = Quaternion.Euler(0f, 0f, targetAngle);

            // Update velocity in the new direction
            fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(targetAngle);
            velocity = fireDirectionVector.normalized * projectileSpeed;

            transform.position += velocity * Time.deltaTime;

            if (projectileRange < 0f)
            {
                DisableProjectile();
            }
        }
    }

    // This is for bouncing projectiles
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // BOUNCING PROCESS
        if (projectileDetails != null && projectileDetails.isBouncing && GetComponentInParent<ProjectilePattern>() == null)
        {
            if (collision.collider.tag == Settings.collisionTilemap || collision.collider.tag == Settings.environment)
            {
                // Reflect off wall/prop
                Vector2 normal = collision.contacts[0].normal;
                velocity = Vector2.Reflect(velocity, normal);

                float angle = HelperUtilities.GetAngleFromVector(velocity);
                transform.rotation = Quaternion.Euler(0f, 0f, angle);
            }
        }

        // If this is a laser, don't disable it on collision
        if (projectileDetails != null && isLaserBeam)
        {
            return;  // Let the laser continue without being disabled
        }

        // If already colliding with something return
        if (isColliding) return;

        int inflictedDamage = 0;

        if (collision.collider.tag == Settings.playerTag)
        {
            Player player = collision.collider.GetComponent<Player>();

            if (player.isValorActive)
            {
                //SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.activeSkillTwoSoundEffect);
                player.health.PostHitImmunity(true);
            }
            else
            {
                if (projectileDetails.isTrap)
                {
                    if (explosionRoutine == null)
                    {
                        explosionRoutine = StartCoroutine(ExplosionRoutine(true));
                        return;
                    }
                }

                int diceRoll = Random.Range(1, 101);
                bool isProjectileDodged = 100 - player.currentEvasivenessValue * 100 < diceRoll ? true : false;

                // Dodge check
                if (isProjectileDodged || player.playerControl.isPlayerRolling)
                {
                    if (isProjectileDodged && player.isShiftingStanceActive)
                    {
                        if (!player.shiftingStanceOnProcess)
                        {
                            player.shiftingStanceOnProcess = true;
                            StartCoroutine(ShiftingStanceRoutine());
                        }
                    }

                    player.health.isDodging = true;
                    player.healthEvent.CallDodgeEvent();
                    player.health.PostHitImmunity(true);
                    //player.health.TakeDamage(0, transform.position, player.health.transform.position, false);
                }
                else
                {
                    if (player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
                    {
                        // Get enemy projectile direction
                        Vector2 enemyProjectileDirection = (player.transform.position - transform.position).normalized;

                        // Get weapon pointer direction
                        Vector2 cursorPosition = InputManager.Instance.pointerPosition.action.ReadValue<Vector2>();
                        Vector2 cursorWorldPosition = Camera.main.ScreenToWorldPoint(cursorPosition);

                        Vector2 pointerDirection = (cursorWorldPosition - new Vector2(player.transform.position.x, player.transform.position.y)).normalized;

                        // Calculate the dot product between the shield's forward direction and the projectile direction
                        float dotProduct = Vector2.Dot(pointerDirection, enemyProjectileDirection);

                        float blockingThreshold = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.blockRate;

                        // Check if the dot product is greater than the threshold, block fails
                        if (dotProduct > blockingThreshold - 1f && player.health.GetCurrentHealth() > 0)
                        {
                            // Deal Damage To Collision Object
                            DealDamage(collision.collider, ref inflictedDamage);

                            if (player.health.GetCurrentHealth() > 0)
                            {
                                // Status checks
                                CheckBleedingStatus(player);
                                CheckStunStatus(player);
                                CheckSlowStatus(player);
                                CheckWarmStatus(player);
                                CheckBurnStatus(player);
                                CheckPoisonStatus(player);
                                CheckAcidStatus(player);
                                CheckChillStatus(player);
                                CheckFrostStatus(player);
                                CheckStaticStatus(player);
                                CheckParalyzeStatus(player);
                                CheckRootStatus(player);
                                CheckCurseStatus(player);
                                CheckFearStatus(player);
                                CheckBlindStatus(player);
                            }
                        }
                        else
                        {
                            // If the player is attacking, guard is down so block is disabled
                            if (player.meleeAttackMainHand.IsAttacking)
                            {
                                // Deal Damage To Collision Object
                                DealDamage(collision.collider, ref inflictedDamage);

                                if (player.health.GetCurrentHealth() > 0)
                                {
                                    // Status checks
                                    CheckBleedingStatus(player);
                                    CheckStunStatus(player);
                                    CheckSlowStatus(player);
                                    CheckWarmStatus(player);
                                    CheckBurnStatus(player);
                                    CheckPoisonStatus(player);
                                    CheckAcidStatus(player);
                                    CheckChillStatus(player);
                                    CheckFrostStatus(player);
                                    CheckStaticStatus(player);
                                    CheckParalyzeStatus(player);
                                    CheckRootStatus(player);
                                    CheckCurseStatus(player);
                                    CheckFearStatus(player);
                                    CheckBlindStatus(player);
                                }
                            }
                            else
                            {
                                if (playerBlockCoroutine == null)
                                {
                                    // The projectile is within the blocking angle
                                    playerBlockCoroutine = StartCoroutine(PlayerBlockAnimRoutine(collision.collider));
                                }
                            }
                        }
                    }
                    else
                    {
                        // Deal Damage To Collision Object
                        DealDamage(collision.collider, ref inflictedDamage);

                        if (player.health.GetCurrentHealth() > 0)
                        {
                            // Status checks
                            CheckBleedingStatus(player);
                            CheckStunStatus(player);
                            CheckSlowStatus(player);
                            CheckWarmStatus(player);
                            CheckBurnStatus(player);
                            CheckPoisonStatus(player);
                            CheckAcidStatus(player);
                            CheckChillStatus(player);
                            CheckFrostStatus(player);
                            CheckStaticStatus(player);
                            CheckParalyzeStatus(player);
                            CheckRootStatus(player);
                            CheckCurseStatus(player);
                            CheckFearStatus(player);
                            CheckBlindStatus(player);
                        }
                    }
                }
            }

            // Show ammo hit effect
            ProjectileHitEffect();

            DisableProjectile();
        }
        else if (collision.collider.tag == Settings.playerWeapon) { }
        else if (collision.collider.tag == Settings.enemyTag)
        {
            Enemy enemy = collision.collider.GetComponent<Enemy>();

            if (collision.collider.GetComponent<Enemy>() != null)
            {
                if (enemy.enemyDetails.hasShield)
                {
                    int diceRoll = Random.Range(0, 100);
                    bool deflectHappened = 100 - enemy.enemyDetails.deflectionValue * 100 < diceRoll ? true : false;

                    if (deflectHappened)
                    {
                        enemy.health.isBlocking = true;
                        enemy.healthEvent.CallDodgeEvent();
                        enemy.health.TakeDamage(0, transform.position, enemy.health.transform.position, collision.collider, MeleeHand.None);
                    }
                    else
                    {
                        // Deal Damage To Collision Object
                        DealDamage(collision.collider, ref inflictedDamage);

                        if (enemy.health.GetCurrentHealth() > 0)
                        {
                            if (isArrowOfTheSeven)
                            {
                                int rng = Random.Range(1, 8);

                                switch (rng)
                                {
                                    case 1: CheckSlowStatus(enemy, true); break;
                                    case 2: CheckBleedingStatus(enemy, true); break;
                                    case 3: CheckPoisonStatus(enemy, true); break;
                                    case 4: CheckBurnStatus(enemy, true); break;
                                    case 5: CheckFrostStatus(enemy, true); break;
                                    case 6: CheckFearStatus(enemy, true); break;
                                    case 7: CheckBlindStatus(enemy, true); break;
                                    default: break;
                                }
                            }
                            else
                            {
                                // Status checks - PROJECTILE
                                CheckBleedingStatus(enemy);
                                CheckStunStatus(enemy);
                                CheckSlowStatus(enemy);
                                CheckWarmStatus(enemy);
                                CheckBurnStatus(enemy);
                                CheckPoisonStatus(enemy);
                                CheckAcidStatus(enemy);
                                CheckChillStatus(enemy);
                                CheckFrostStatus(enemy);
                                CheckShatterStatus(enemy, ref inflictedDamage, isIceBreaker);
                                CheckStaticStatus(enemy);
                                CheckParalyzeStatus(enemy);
                                CheckRootStatus(enemy, isBindingArrow);
                                CheckCurseStatus(enemy);
                                CheckFearStatus(enemy);
                                CheckBlindStatus(enemy);
                            }
                        }
                    }
                }
                else
                {
                    // Deal Damage To Collision Object
                    DealDamage(collision.collider, ref inflictedDamage);

                    if (enemy.health.GetCurrentHealth() > 0)
                    {
                        if (isArrowOfTheSeven)
                        {
                            int rng = Random.Range(1, 8);

                            switch (rng)
                            {
                                case 1: CheckSlowStatus(enemy, true); break;
                                case 2: CheckBleedingStatus(enemy, true); break;
                                case 3: CheckPoisonStatus(enemy, true); break;
                                case 4: CheckBurnStatus(enemy, true); break;
                                case 5: CheckFrostStatus(enemy, true); break;
                                case 6: CheckFearStatus(enemy, true); break;
                                case 7: CheckBlindStatus(enemy, true); break;
                                default: break;
                            }
                        }
                        else
                        {
                            // Status checks - PROJECTILE
                            CheckBleedingStatus(enemy);
                            CheckStunStatus(enemy);
                            CheckSlowStatus(enemy);
                            CheckWarmStatus(enemy);
                            CheckBurnStatus(enemy);
                            CheckPoisonStatus(enemy);
                            CheckAcidStatus(enemy);
                            CheckChillStatus(enemy);
                            CheckFrostStatus(enemy);
                            CheckStaticStatus(enemy);
                            CheckParalyzeStatus(enemy);
                            CheckShatterStatus(enemy, ref inflictedDamage, isIceBreaker);
                            CheckRootStatus(enemy, isBindingArrow);
                            CheckCurseStatus(enemy);
                            CheckFearStatus(enemy);
                            CheckBlindStatus(enemy);
                        }
                    }
                }
            }
            else
            {
                // Deal Damage To Collision Object
                DealDamage(collision.collider, ref inflictedDamage);

                if (enemy.health.GetCurrentHealth() > 0)
                {
                    if (isArrowOfTheSeven)
                    {
                        int rng = Random.Range(1, 8);

                        switch (rng)
                        {
                            case 1: CheckSlowStatus(enemy, true); break;
                            case 2: CheckBleedingStatus(enemy, true); break;
                            case 3: CheckPoisonStatus(enemy, true); break;
                            case 4: CheckBurnStatus(enemy, true); break;
                            case 5: CheckFrostStatus(enemy, true); break;
                            case 6: CheckFearStatus(enemy, true); break;
                            case 7: CheckBlindStatus(enemy, true); break;
                            default: break;
                        }
                    }
                    else
                    {
                        // Status checks - PROJECTILE
                        CheckBleedingStatus(enemy);
                        CheckStunStatus(enemy);
                        CheckSlowStatus(enemy);
                        CheckWarmStatus(enemy);
                        CheckBurnStatus(enemy);
                        CheckPoisonStatus(enemy);
                        CheckAcidStatus(enemy);
                        CheckChillStatus(enemy);
                        CheckFrostStatus(enemy);
                        CheckStaticStatus(enemy);
                        CheckParalyzeStatus(enemy);
                        CheckShatterStatus(enemy, ref inflictedDamage, isIceBreaker);
                        CheckRootStatus(enemy, isBindingArrow);
                        CheckCurseStatus(enemy);
                        CheckFearStatus(enemy);
                        CheckBlindStatus(enemy);
                    }
                }
            }

            // Show ammo hit effect
            ProjectileHitEffect();

            // Projectile behaviour on contact (replace with:)
            if (enemy != null)
            {
                OnHitEnemy(enemy);
            }
            else
            {
                // Non-enemy contact behavior (walls, ground, etc.)
                DisableProjectile();
            }
        }
        else if (collision.collider.tag == Settings.decoyTag)
        {
            // Deal Damage To Collision Object
            DealDamage(collision.collider, ref inflictedDamage);

            // Show ammo hit effect
            ProjectileHitEffect();

            if (isFireBlast)
            {
                if (explosionRoutine == null)
                {
                    velocity = Vector3.zero;
                    explosionRoutine = StartCoroutine(ExplosionRoutine());
                }
            }
            else
            {
                DisableProjectile();
            }
        }
        else if (collision.collider.tag == Settings.practiceDummy)
        {
            DummyCheck(collision.collider);

            if (isFireBlast)
            {
                if (explosionRoutine == null)
                {
                    velocity = Vector3.zero;
                    explosionRoutine = StartCoroutine(ExplosionRoutine());
                }
            }
            else
            {
                DisableProjectile();
            }
        }
        else if (collision.collider.tag == Settings.playerWeapon)
        {
            return;
        }
        else // HIT WALL CHECK
        {
            isHittingWall = true;

            // Deal Damage To Collision Object
            DealDamage(collision.collider, ref inflictedDamage);

            // Show ammo hit effect
            ProjectileHitEffect();

            if (isFireBlast)
            {
                if (explosionRoutine == null)
                {
                    velocity = new Vector3(0f, 0f, 1f);
                    explosionRoutine = StartCoroutine(ExplosionRoutine());
                }
            }
            else if (isShiruken)
            {

            }
            else
            {
                DisableProjectile();
            }
        }
    }

    private bool HasEnemyNegativeStatusEffect(Enemy enemy)
    {
        if (enemy.enemyAI.moveStatus != MoveStatus.Idle) return true;

        if (enemy.healthStatus != HealthStatus.Normal) return true;

        if (enemy.isCursed || enemy.isFeared || enemy.isRevealed || enemy.isStatic || enemy.isWarmed || enemy.isChilled || enemy.isBlind || enemy.isSlowed) return true;

        return false;
    }

    IEnumerator PlayerBlockAnimRoutine(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        // Adjust animator layer weights
        player.transform.GetChild(2).GetComponent<Animator>().SetTrigger(Settings.block);
        SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponSwingSoundEffect);
        player.healthEvent.CallBlockEvent();

        yield return new WaitForSeconds(0.6f);

        playerBlockCoroutine = null;
        player.animatePlayer.ResetAnimatonParameters();
        player.animator.SetBool(Settings.isIdle, true);
    }

    IEnumerator ShiftingStanceRoutine()
    {
        player.CurrentAgilityValue += 3;

        yield return new WaitForSeconds(3f);

        player.shiftingStanceOnProcess = false;
        player.CurrentAgilityValue -= 3;
    }

    private void DealDamage(Collider2D collision, ref int inflictedDamage, bool isLaser = false)
    {
        Health health = collision.GetComponent<Health>();

        if (health != null)
        {
            // Set isColliding to prevent ammo dealing damage multiple times
            if (isPenetrationArrow || isBlazingCyclone)
            {
                StartCoroutine(ColliderTimeThreshold());
            }
            else if(!isLaser) // Laser do not need this
            {
                isColliding = true;
            }

            if (collision != null && collision.GetComponent<Enemy>() != null)
            {
                // Caster is player
                damageDone = Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);
            }
            else
            {
                damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);
            }

            if (isPenetrationArrow)
            {
                float increasedDamage = damageDone * 1.15f;
                damageDone = (int)(increasedDamage * (1 + player.additionalPenetrationSkillDamageModifier));
            }

            if (collision != null && collision.GetComponent<Enemy>() != null)
            {
                Enemy enemy = collision.GetComponent<Enemy>();

                if (projectileDetails.isPlayerProjectile)
                {
                    // LOWER DAMAGE IF PLAYER IS CURSED - PROJECTILE
                    damageDone = player.isCursed ? player.currentMainHandMinDamageValue : Random.Range(player.currentMainHandMinDamageValue,
                        player.currentMainHandMaxDamageValue);

                    float punishersWillModifier = 0.05f;

                    float totalDamageModifiers = 0f;

                    float enemyCurrrentHealth = enemy.health.GetCurrentHealth();
                    float enemyMaximumHealth = enemy.health.GetMaximumHealth();

                    // Punisher's Will Check
                    if (player.isPunishersWillActive && enemyCurrrentHealth / enemyMaximumHealth < 0.5f)
                        totalDamageModifiers += punishersWillModifier;

                    damageDone = (int)(damageDone * (1 + totalDamageModifiers)); // Add additional damage modifiers

                    // Critical Hit Check
                    bool criticalHitHappened = CriticalHitHappened(enemy);

                    if (criticalHitHappened)
                    {
                        enemy.healthEvent.CallCriticalHitEvent();

                        float critMultiplier = player.currentMainHandCriticalHitDamage;

                        if ((enemy != null && enemy.isBlind)) critMultiplier += player.additionalCriticalDamageOnCloakedPrecision;

                        if (player.playerDetails.playerCharacterIndex == Character.Nyveran && HasEnemyNegativeStatusEffect(enemy)) //Deadeye's Quiver
                        {
                            damageDone = (int)(damageDone * (player.currentMainHandCriticalHitDamage + 0.25f));
                            player.playerControl.activeSkillTypeOneAnimator.SetTrigger("deadeye");
                        }
                        else damageDone = (int)(damageDone * critMultiplier);
                    }
                }

                // Segregate elemental and non-elemental damage
                int elementalDamage = 0;
                int nonElementalDamage = 0;
                int inflictedNonElementalDamage = 0;
                int additionalElementalDamage = 0;

                if (isIceBreaker || isFireBlast || isBlazingCyclone || isShiruken || isChainLightning)
                {
                    elementalDamage = (int)(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.elementalForgeRate * damageDone);
                    additionalElementalDamage = (int)(elementalDamage * (1 + player.additionalElementalDamageModifier));
                    elementalDamage += additionalElementalDamage;
                }
                else if (isThrowingAxe)
                {
                    elementalDamage = (int)(DropItem.droppedThrowingAxe.elementalForgeRate * damageDone);
                    additionalElementalDamage = (int)(elementalDamage * (1 + player.additionalElementalDamageModifier));
                    elementalDamage += additionalElementalDamage;
                }
                else
                {
                    elementalDamage = (int)(projectileDetails.belongingWeaponDetails.elementalForgeRate * damageDone);
                    additionalElementalDamage = (int)(elementalDamage * (1 + player.additionalElementalDamageModifier));
                    elementalDamage += additionalElementalDamage;
                }

                nonElementalDamage = damageDone - elementalDamage + additionalElementalDamage;

                // ARMOR DEDUCTIONS
                float effectiveArmor = enemy.currentArmor;

                if (isPenetrationArrow) effectiveArmor *= 0.7f; // 20% Armor Penetration
                if (player.isShatterCryActive) effectiveArmor *= 0.8f;

                inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - effectiveArmor));

                int inflictedElementalDamage = 0;

                // Damage inflicted to enemy after deducting enemy armor
                WeaponDetailsSO weaponDetails;

                if (isIceBreaker || isFireBlast || isBlazingCyclone || isShiruken || isChainLightning)
                {
                    weaponDetails = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails;
                }
                else if (isThrowingAxe)
                {
                    weaponDetails = DropItem.droppedThrowingAxe;
                }
                else
                {
                    weaponDetails = projectileDetails.belongingWeaponDetails;
                }

                // Calculate inflicted elemental damage
                switch (weaponDetails.elementalBias)
                {
                    case ElementalBias.None:
                        break;
                    case ElementalBias.Fire:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.fireResistance));
                        break;
                    case ElementalBias.Water:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.waterResistance));
                        break;
                    case ElementalBias.Earth:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.earthResistance));
                        break;
                    case ElementalBias.Air:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.airResistance));
                        break;
                    case ElementalBias.Dark:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.darkResistance));
                        break;
                    case ElementalBias.Light:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.lightResistance));
                        break;
                    default:
                        break;
                }

                inflictedDamage = inflictedElementalDamage + inflictedNonElementalDamage;
            }
            else if (collision != null && collision.GetComponent<Player>() != null)
            {
                Player player = collision.GetComponent<Player>();

                inflictedDamage = 0;

                // Segregate elemental and non-elemental damage
                int elementalDamage = (int)(projectileDetails.belongingWeaponDetails.elementalForgeRate * damageDone);
                int nonElementalDamage = damageDone - elementalDamage;

                int inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - player.currentArmorValue));

                int inflictedElementalDamage = 0;
                // Calculate inflicted elemental damage
                switch (projectileDetails.belongingWeaponDetails.elementalBias)
                {
                    case ElementalBias.None:
                        break;
                    case ElementalBias.Fire:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - player.currentFireResistanceValue));
                        break;
                    case ElementalBias.Water:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - player.currentWaterResistanceValue));
                        break;
                    case ElementalBias.Earth:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - player.currentEarthResistanceValue));
                        break;
                    case ElementalBias.Air:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - player.currentAirResistanceValue));
                        break;
                    case ElementalBias.Dark:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - player.currentDarkResistanceValue));
                        break;
                    case ElementalBias.Light:
                        inflictedElementalDamage = (int)(elementalDamage * (1 - player.currentLightResistanceValue));
                        break;
                    default:
                        break;
                }

                // Damage inflicted to player after deducting player armor
                inflictedDamage = inflictedElementalDamage + inflictedNonElementalDamage;
            }
            else if (collision != null && collision.GetComponent<Environment>() != null)
            {
                // Damage inflicted equals damage done for environment objects
                inflictedDamage = damageDone;
            }

            // **Apply Time.deltaTime Scaling ONLY for Laser Beams**
            if (isLaserBeam)
            {
                inflictedDamage = Mathf.Max(1, inflictedDamage); // Ensure at least 1 damage per frame
            }

            health.TakeDamage(inflictedDamage, transform.position, health.transform.position, collision, MeleeHand.None);
        }
    }

    /// <summary>
    /// Critical hit check
    /// </summary>
    /// <returns></returns>
    private bool CriticalHitHappened(Enemy enemy)
    {
        bool criticalHitHappened = false;

        float randomCriticalDice = Random.Range(0f, 1f);

        if (player.isBlind)
        {
            criticalHitHappened = false;
        }
        else if (enemy != null && player.playerDetails.playerCharacterIndex == Character.Nyveran)
        {
            criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance + 0.15f; // Nyveran - Deadeye's Quiver Passive
        }
        else
        {
            criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance;
        }

        return criticalHitHappened;
    }

    /// <summary>
    /// Dummy hit interactions
    /// </summary>
    private void DummyCheck(Collider2D collider)
    {
        Health health = collider.GetComponent<Health>();

        // Damage produced by player
        int damageDone = player.isCursed ? player.currentMainHandMinDamageValue : Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);
        int offHandDamageDone = player.isCursed ? player.currentOffHandMinDamageValue : Random.Range(player.currentOffHandMinDamageValue, player.currentOffHandMinDamageValue);
        damageDone += offHandDamageDone;

        bool criticalHitHappened = false;

        // Calculate damage after critical hit check
        if (player.isStealthActive)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier + player.additionalCriticalDamageOnCloakedPrecision)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        health.PostHitImmunity();
        health.TakeDamage(damageDone, transform.position, health.transform.position, collider, MeleeHand.None);
        collider.GetComponent<HealthEvent>().CallHealthChangedEvent(1000000000, damageDone, MeleeHand.None);
    }

    IEnumerator ColliderTimeThreshold()
    {
        yield return new WaitForSeconds(0.04f);

        isColliding = false;
    }

    /// <summary>
    /// Initialize the projectile being fired - using the projectileDetails, the aimangle, weaponAngle, and weaponAimDirectionVector. If this 
    /// projectile is part of a pattern the projectile movement can be overriden by setting overrideAmmoMovement to true - PROJECTILE
    /// </summary>
    public void InitializeProjectile(Enemy belongingEnemy, bool isIceBreaker, ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle,
        float projectileSpeed, Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false, bool fallingFromSkies = false,
        bool isPenetrationArrow = false, int projectileCounter = 0, int projectilesPerShot = 0, MoravellePhase moravellePhase = MoravellePhase.None,
        SylvarokPhase treantPhase = SylvarokPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None,
        FrostWrymPhase frostWrymPhase = FrostWrymPhase.None, VenomancerPhase venomancerPhase = VenomancerPhase.None, FireWrymPhase fireWrymPhase = FireWrymPhase.None,
        MoldranPhase moldranPhase = MoldranPhase.None, bool isTripleThreat = false, bool isBindingArrow = false, bool isArrowOfTheSeven = false,
        ProjectileDetailsSO grappleDetails = null, ProjectileDetailsSO iceBreakerDetails = null, bool isFireBlast = false, ProjectileDetailsSO fireBlastDetails = null,
        bool isBlazingCyclone = false, ProjectileDetailsSO blazingCyclone = null, bool isThrowingAxe = false, ProjectileDetailsSO throwingAxeDetails = null,
        bool isShiruken = false, ProjectileDetailsSO shirukenDetails = null, bool isChainLightning = false, ProjectileDetailsSO chainLightningDetails = null,
        ChainLightningPhase chainLightningPhase = ChainLightningPhase.None)
    {
        #region Projectile

        this.projectileDetails = projectileDetails;

        // Set penetration arrow bool
        this.isPenetrationArrow = isPenetrationArrow;

        // Set triple threat arrow bool
        this.isTripleThreat = isTripleThreat;

        // Set binding arrow bool
        this.isBindingArrow = isBindingArrow;

        // Set arrow of the seven
        this.isArrowOfTheSeven = isArrowOfTheSeven;

        // Set ice breaker
        this.isIceBreaker = isIceBreaker;

        // Set fire blast
        this.isFireBlast = isFireBlast;

        // Set blazing cyclone
        this.isBlazingCyclone = isBlazingCyclone;

        // Set throwing axe
        this.isThrowingAxe = isThrowingAxe;

        // Set shiruken
        this.isShiruken = isShiruken;

        // Set chain lightning
        this.isChainLightning = isChainLightning;
        this.chainLightningPhase = chainLightningPhase;

        if (isThrowingAxe) dropOnAxeThrow = GetComponent<DropOnAxeThrow>();

        // Grapple hook
        isGrappleHook = grappleDetails != null;

        isLaserBeam = projectileDetails.isLaser;   

        // Initialize isColliding
        isColliding = false;

        // Set belonging enemy if it is
        this.belongingEnemy = belongingEnemy;

        // Set fire direction
        SetFireDirection(projectileDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileCounter, projectilesPerShot, moravellePhase, 
            treantPhase, galvanusPhase, sepharothPhase, frostWrymPhase, venomancerPhase, fireWrymPhase, moldranPhase, isTripleThreat, isBindingArrow,
            isArrowOfTheSeven, isIceBreaker, isFireBlast, isBlazingCyclone, isThrowingAxe, isShiruken, isChainLightning, chainLightningPhase);

        // Play sound if it is a unique projectile
        if (galvanusPhase == GalvanusPhase.Lightning)
        {

            lightningStroke = true;
        }
        else if (frostWrymPhase == FrostWrymPhase.Icicle || fireWrymPhase == FireWrymPhase.FirePillar || moldranPhase == MoldranPhase.Spike)
        {
            SoundEffectManager.Instance.PlaySoundEffect(projectileDetails.projectileFireSoundEffect);
        }

        // Set initial projectile material depending on whether there is an projectile charge period
        if (projectileDetails.projectileChargeTime > 0f)
        {
            // Set ammo charge timer
            projectileChargeTimer = projectileDetails.projectileChargeTime;
            SetProjectileMaterial(projectileDetails.projectileChargeMaterial);
            isProjectileMaterialSet = false;
        }
        else
        {
            projectileChargeTimer = 0f;
            SetProjectileMaterial(projectileDetails.projectileMaterial);
            isProjectileMaterialSet = true;
        }

        // Set projectile range
        if (isPenetrationArrow)
        {
            projectileRange = 100;
            spriteRenderer.material = GameManager.Instance.GetPlayer().playerDetails.penetrateMaterial;
        }
        else
        {
            projectileRange = projectileDetails.projectileRange;
        }

        // Set projectile speed
        this.projectileSpeed = projectileSpeed;

        // Override projectile movement
        this.overrideProjectileMovement = overrideProjectileMovement;

        // Activate projectile gameObject
        gameObject.SetActive(true);

        #endregion

        #region Trail

        if (projectileDetails.isProjectileTrail)
        {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = projectileDetails.projectileTrailMaterial;
            trailRenderer.startWidth = projectileDetails.projectileTrailStartWidth;
            trailRenderer.endWidth = projectileDetails.projectileTrailEndWidth;
            trailRenderer.time = projectileDetails.projectileTrailTime;
        }
        else
        {
            trailRenderer.emitting = false;
            trailRenderer.gameObject.SetActive(false);
        }

        #endregion
    }

    /// <summary>
    /// Set projectile fire direction and angle based on the input angle and direction adjusted by the
    /// random spread - PROJECTILE
    private void SetFireDirection(ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, 
        int projectileCounter = 0, int totalProjectiles = 0, MoravellePhase moravellePhase = MoravellePhase.None, SylvarokPhase treantPhase = SylvarokPhase.None, 
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None, FrostWrymPhase frostWrymPhase = FrostWrymPhase.None,
        VenomancerPhase venomancerPhase = VenomancerPhase.None, FireWrymPhase fireWrymPhase = FireWrymPhase.None, MoldranPhase moldranPhase = MoldranPhase.None,
        bool isTripleThreat = false, bool isBindingArrow = false, bool isArrowOfTheSeven = false, bool isIceBreaker = false, bool isFireBlast = false,
        bool isBlazingCyclone = false, bool isThrowingAxe = false, bool isShiruken = false, bool isChainLightning = false, ChainLightningPhase chainLightningPhase
        = ChainLightningPhase.None)
    {
        if (isTripleThreat)
        {
            // Define the total angle spread (e.g., 30 degrees spread)
            float totalSpreadAngle = 30f;

            // Calculate the angle increment between projectiles
            float angleIncrement = (totalProjectiles > 1) ? totalSpreadAngle / (totalProjectiles - 1) : 0f;

            // Adjust the starting angle to center the spread
            float startAngle = aimAngle - (totalSpreadAngle / 2);

            // Set the fire direction angle based on the projectile index
            fireDirectionAngle = startAngle + (angleIncrement * projectileCounter);
        }
        else if (moravellePhase == MoravellePhase.SpreadArrowShot)
        {
            // Define the total angle spread (e.g., 45 degrees spread)
            float totalSpreadAngle = 45f;

            // Calculate the angle increment between projectiles
            float angleIncrement = (totalProjectiles > 1) ? totalSpreadAngle / (totalProjectiles - 1) : 0f;

            // Adjust the starting angle to center the spread
            float startAngle = aimAngle - (totalSpreadAngle / 2);

            // Set the fire direction angle based on the projectile index
            fireDirectionAngle = startAngle + (angleIncrement * projectileCounter);
        }
        else if (frostWrymPhase == FrostWrymPhase.IceProjectile || fireWrymPhase == FireWrymPhase.FireProjectile || moldranPhase == MoldranPhase.Projectile)
        {
            // Define the total angle spread (e.g., 45 degrees spread)
            float totalSpreadAngle = 30f;

            // Calculate the angle increment between projectiles
            float angleIncrement = (totalProjectiles > 1) ? totalSpreadAngle / (totalProjectiles - 1) : 0f;

            // Adjust the starting angle to center the spread
            float startAngle = aimAngle - (totalSpreadAngle / 2);

            // Set the fire direction angle based on the projectile index
            fireDirectionAngle = startAngle + (angleIncrement * projectileCounter);
        }
        else if (venomancerPhase == VenomancerPhase.SludgeThrow)
        {
            // Define the total angle spread (e.g., 60 degrees spread)
            float totalSpreadAngle = 360f;

            // Calculate the total weight for the decreasing intervals
            float weightSum = 0f;
            for (int i = 0; i < totalProjectiles; i++)
            {
                weightSum += (float)Math.Pow(1f, -i); // Exponential decrease
            }

            // Determine the incremental angle for each projectile
            float cumulativeAngle = 0f;
            for (int i = 0; i < totalProjectiles; i++)
            {
                float weight = (float)Math.Pow(1f, -i) / weightSum; // Normalize weight
                float angle = totalSpreadAngle * weight;

                if (i == projectileCounter)
                {
                    fireDirectionAngle = aimAngle - (totalSpreadAngle / 2) + cumulativeAngle + (angle / 2);
                    break;
                }

                cumulativeAngle += angle;
            }
        }
        else if (treantPhase == SylvarokPhase.RazorLeaf)
        {
            // Define the total angle spread (e.g., 60 degrees spread)
            float totalSpreadAngle = 145f;

            // Calculate the total weight for the decreasing intervals
            float weightSum = 0f;
            for (int i = 0; i < totalProjectiles; i++)
            {
                weightSum += (float)Math.Pow(1.2f, -i); // Exponential decrease
            }

            // Determine the incremental angle for each projectile
            float cumulativeAngle = 0f;
            for (int i = 0; i < totalProjectiles; i++)
            {
                float weight = (float)Math.Pow(1.2f, -i) / weightSum; // Normalize weight
                float angle = totalSpreadAngle * weight;

                if (i == projectileCounter)
                {
                    fireDirectionAngle = aimAngle - (totalSpreadAngle / 2) + cumulativeAngle + (angle / 2);
                    break;
                }

                cumulativeAngle += angle;
            }
        }
        else
        {
            // Calculate random spread angle between min and max
            float randomSpread = Random.Range(projectileDetails.projectileSpreadMin, projectileDetails.projectileSpreadMax);

            if (projectileDetails.isPlayerProjectile)
            {
                if (projectileDetails.belongingWeaponDetails != null)
                {
                    if (projectileDetails.belongingWeaponDetails.weaponClass == WeaponClass.Bow ||
                        projectileDetails.belongingWeaponDetails.weaponClass == WeaponClass.Crossbow)
                    {
                        float arrowSpreadReduction = randomSpread * (GameManager.Instance.GetPlayer().additionalBowAccuracyModifier);
                        randomSpread = randomSpread - arrowSpreadReduction;
                    }
                }
            }

            // Get a random spread toggle of 1 or -1
            int spreadToggle = Random.Range(0, 2) * 2 - 1;

            if (weaponAimDirectionVector.magnitude < Settings.useAimAngleDistance)
            {
                fireDirectionAngle = aimAngle;
            }
            else
            {
                fireDirectionAngle = weaponAimAngle;
            }

            // Adjust projectile fire angle by random spread
            fireDirectionAngle += spreadToggle * randomSpread;
        }

        // Set projectile rotation
        if (galvanusPhase == GalvanusPhase.Lightning || frostWrymPhase == FrostWrymPhase.Icicle || venomancerPhase == VenomancerPhase.StoneRain ||
            fireWrymPhase == FireWrymPhase.FirePillar || moldranPhase == MoldranPhase.Spike || venomancerPhase == VenomancerPhase.ToxicPool)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
        }
        else if (isIceBreaker)
        {
            transform.eulerAngles = new Vector3(0f, 0f, 0f);
            // Set projectile fire direction
            fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
        }
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);
            // Set projectile fire direction
            fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
        }
    }

    /// <summary>
    /// Disable the projectile - thus returning it to the object pool
    /// </summary>
    private void DisableProjectile()
    {
        if (!isPenetrationArrow && !isBlazingCyclone )
        {
            velocity = Vector2.zero;
            isStopped = true;
            stoppedPosition = transform.position;
        }
        else
        {
            if (isHittingWall)
            {
                velocity = Vector2.zero;
                isStopped = true;
                stoppedPosition = transform.position;
            }
        }

        if (transform.GetComponentInParent<ProjectilePattern>() != null || isGrappleHook || isIceBreaker || isFireBlast ) { }
        else GetComponent<Animator>().SetTrigger("impact");

        if (!isPenetrationArrow && !isBlazingCyclone)
        {
            StartCoroutine(DisableProcess(0.2f));
        }
        else
        {
            if (isHittingWall)
            {
                StartCoroutine(DisableProcess(0.2f));
            }
        }
    }

    private void UpdateLaser()
    {
        laserDuration -= Time.deltaTime;

        if (laserDuration <= 0)
        {
            DisableProjectile();
            return;
        }

        Vector3 startPos = laserStartPoint.position;
        Vector3 direction = new Vector3();
        float startAngle = 0f;

        if (!isLocked)
        {
            direction = lockedTargetVector;
            startAngle = lockedAngle;
            isLocked = true;
        }
        else
        {
            // Update direction for raycast
            direction = HelperUtilities.GetDirectionVectorFromAngle(lockedAngle);
        }

        // Check if the laser collides with something
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direction, Mathf.Infinity, layerMask);

        float maxDistance = hit.collider != null ? Vector2.Distance(startPos, hit.point + new Vector2(0f, 0.5f)) : Vector2.Distance(startPos, targetPosition);

        // If the player is inside the beam, apply continuous damage**
        if (hit.collider != null && hit.collider.CompareTag(Settings.playerTag))
        {
            // Status checks
            CheckPoisonStatus(player);
            CheckAcidStatus(player);
            CheckFrostStatus(player);
            CheckStunStatus(player);
            CheckCurseStatus(player);
            CheckBlindStatus(player);

            DealLaserDamage(hit.collider);
        }

        // **Smoothly Extend Laser Length**
        spriteRenderer.transform.localScale = new Vector3(maxDistance * 5, spriteRenderer.transform.localScale.y, 1f);

        // Update laser start points transform
        laserStartPoint = transform;

        // Check Player's location for beam
        bool isBeamAbovePlayer = IsBeamAbovePlayer(ref direction);

        // Change angle towards player position
        if (isBeamAbovePlayer)
        {
            lockedAngle -= angleSpeed * Time.deltaTime;
        }
        else
        {
            lockedAngle += angleSpeed * Time.deltaTime;
        }

        // **Rotate Laser Sprite**
        spriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, lockedAngle);

        // Tweak with sorting order
        Vector3 playerDirectionVector = Vector3.zero;

        if (GameManager.Instance.GetPlayer() != null)
        {
            playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - belongingEnemy.transform.position;
        }

        float enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);
        AimDirection enemyAimDirection = HelperUtilities.GetAimDirection(lockedAngle);

        if (enemyAimDirection == AimDirection.Up)
        {
            spriteRenderer.sortingOrder = -1;
        }
        else
        {
            spriteRenderer.sortingOrder = 3;
        }
    }

    private void UpdateGrappleHook()
    {
        Vector3 startPos = grappleStartPoint.position;
        Vector2 direction = fireDirectionVector.normalized;
        float angle = HelperUtilities.GetAngleFromVector(direction);
        float maxGrappleRange = projectileDetails.projectileRange;

        RaycastHit2D hit = Physics2D.Raycast(startPos, direction, maxGrappleRange, layerMask);
        float targetRopeLength = hit.collider != null ? Vector2.Distance(startPos, hit.point) : maxGrappleRange;

        if (!hasGrappled)
        {
            // Extend rope toward max range or hit point
            currentRopeLength = Mathf.MoveTowards(currentRopeLength, targetRopeLength, projectileSpeed * Time.deltaTime);

            // Update rope sprite
            ropeSpriteRenderer.size = new Vector2(currentRopeLength, ropeSpriteRenderer.size.y);
            ropeSpriteRenderer.transform.position = startPos;
            ropeSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Move grapple head to rope tip
            Vector3 hookTip = startPos + (Vector3)(direction * currentRopeLength);
            grappleHeadTransform.position = hookTip;
            grappleHeadTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            // --- Missed-grapple check ---
            if (!missRetractStarted && hit.collider == null && currentRopeLength >= maxGrappleRange - 0.01f)
            {
                missRetractStarted = true;
                StartCoroutine(RetractMissedGrapple());
                return;                                    // stop further processing this frame
            }
        }
        // ----------  PHASE 2 : grapple latched  ----------
        else
        {
            Vector3 endPos = grappleHeadTransform.position;   // fixed latch point
            Vector3 ropeVector = endPos - startPos;

            float ropeLength = ropeVector.magnitude;
            float ropeAngle = Mathf.Atan2(ropeVector.y, ropeVector.x) * Mathf.Rad2Deg;

            // Stretch rope between player and fixed head
            ropeSpriteRenderer.size = new Vector2(ropeLength, ropeSpriteRenderer.size.y);
            ropeSpriteRenderer.transform.position = startPos;
            ropeSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, ropeAngle);
        }

        // Handle damage if enemy hit
        if (hit.collider != null)
        {
            if (hit.collider.CompareTag(Settings.enemyTag) && !grappleHasDealtDamage && currentRopeLength >= targetRopeLength - 0.01f)
            {
                grappleHasDealtDamage = true;

                CheckPoisonStatus(player);
                CheckAcidStatus(player);
                CheckFrostStatus(player);
                CheckStunStatus(player);
                CheckCurseStatus(player);
                CheckBlindStatus(player);

                damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);
                int inflictedDamage = (int)(damageDone * (1 - hit.transform.GetComponent<Enemy>().currentArmor));

                hit.transform.GetComponent<Health>().TakeDamage(inflictedDamage, grappleHeadTransform.position, hit.transform.position,
                    hit.transform.GetComponent<PolygonCollider2D>(), MeleeHand.None);

                player.health.PostHitImmunity(true); // Temporary inviciblity
                player.meleeAttackMainHand.CheckStunStatus(hit.transform.GetComponent<Enemy>(), false, true);

                Physics2D.IgnoreLayerCollision(playerLayer, poolLayer, false); // re-enable
                return;
            }
            else if (hit.collider.CompareTag(Settings.collisionTilemap) && !isHookedToTheWall)
            {
                // Disable pool layer
                Physics2D.IgnoreLayerCollision(playerLayer, poolLayer, true); // disable

                isHookedToTheWall = true;

                SoundEffectSO hookHangSound = player.playersAllActiveUniqueSkills[4].activeUniqueSkillSoundEffectTwo;
                SoundEffectManager.Instance.PlaySoundEffect(hookHangSound);
            }

            // GRAPPLE - PULL PLAYER PHASE
            // Lock joint only when rope fully extended
            if (!hasGrappled && currentRopeLength >= targetRopeLength - 0.01f)
            {
                if(player.HasPlayerStrongNegativeStatusEffectExcludingHealth() && !missRetractStarted)
                {
                    missRetractStarted = true;
                    StartCoroutine(RetractMissedGrapple());
                    return;
                }

                hasGrappled = true;

                Vector2 grapplePoint = grappleHeadTransform.position;

                player.springJoint2D.enabled = true;
                player.springJoint2D.connectedAnchor = grapplePoint;
                player.springJoint2D.distance = 0f; // Pulls player to exact point
                player.springJoint2D.enableCollision = false;

                // Control pull strength and smoothness
                player.springJoint2D.frequency = 2f;      // Pulling strength — lower = slower pull
                player.springJoint2D.dampingRatio = 0.1f;   // How much it resists overshooting/bouncing

                player.isHuntersReachActive = true;
            }
        }
    }

    public void ReleaseGrapple()
    {
        ResetGrapple();

        DisableProjectile();

        InputManager.dodgeRollDisabled = false;
    }

    private void ResetGrapple()
    {
        Physics2D.IgnoreLayerCollision(playerLayer, poolLayer, false); // re-enable

        player.springJoint2D.enabled = false;
        player.isHuntersReachActive = false;
        hasGrappled = false;
        currentRopeLength = 0f;
        isHookedToTheWall = false;
        missRetractStarted = false;
        isGrappleReleased = true;
        grappleHasDealtDamage = false;

        transform.SetParent(belongingParent);

        grappleHeadTransform.GetComponent<SpriteRenderer>().enabled = false;
        PoolManager.Instance.ResetObject(Vector3.zero, Quaternion.identity, transform, gameObject);
        grappleHeadTransform.localPosition = Vector3.zero;
        grappleHeadTransform.localEulerAngles = Vector3.zero;
        ropeTransform.localPosition = Vector3.zero;
        ropeTransform.localEulerAngles = Vector3.zero;
    }

    IEnumerator RetractMissedGrapple()
    {
        float retractSpeed = 50f;

        while (currentRopeLength > 0.5f)
        {
            currentRopeLength = Mathf.MoveTowards(currentRopeLength, 0f, retractSpeed * Time.deltaTime);

            // Visually shrink rope
            ropeSpriteRenderer.size = new Vector2(currentRopeLength, ropeSpriteRenderer.size.y);

            // Rope direction still same
            Vector3 startPos = grappleStartPoint.position;
            Vector2 direction = fireDirectionVector.normalized;
            float angle = HelperUtilities.GetAngleFromVector(direction);
            ropeSpriteRenderer.transform.position = startPos;
            ropeSpriteRenderer.transform.rotation = Quaternion.Euler(0f, 0f, angle);

            // Move hook head along with rope tip
            Vector3 hookTip = startPos + (Vector3)(direction * currentRopeLength);
            grappleHeadTransform.position = hookTip;
            grappleHeadTransform.rotation = Quaternion.Euler(0f, 0f, angle);

            yield return null;
        }

        // After fully retracted
        ReleaseGrapple(); // Safely reset and return to pool
    }

    private bool IsBeamAbovePlayer(ref Vector3 direction)
    {
        Vector3 playerPos = Vector3.zero;

        if (GameManager.Instance.GetPlayer() != null)
        {
            playerPos = GameManager.Instance.GetPlayer().transform.position + new Vector3(0f, 0.5f, 0f);
        }

        Vector3 laserPos = laserStartPoint.position;
        Vector3 laserDirection = direction.normalized; // Laser's firing direction

        // Calculate a perpendicular vector to the laser's direction
        Vector3 perpendicularVector = new Vector2(-laserDirection.y, laserDirection.x);

        // Check if the player is above or below the laser line
        float positionCheck = Vector2.Dot(playerPos - laserPos, perpendicularVector);

        if (positionCheck > 0)
        {
            return false;
        }
        else
        {
            return true;
        }
    }

    private void DealLaserDamage(Collider2D collider)
    {
        int inflictedDamage = 0;

        DealDamage(collider, ref inflictedDamage, true);
    }

    IEnumerator DisableProcess(float disableDuration)
    {
        yield return new WaitForSeconds(disableDuration);

        if (isGrappleHook) grappleHeadTransform.GetComponent<SpriteRenderer>().enabled = true;

        if (isThrowingAxe)
        {
            dropOnAxeThrow.DropProcess();
            player.isAxeThrowActive = false;
        }

        isHittingWall = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Display the ammo hit effect
    /// </summary>
    private void ProjectileHitEffect()
    {
        // Process if a hit effect has been specified
        if (projectileDetails.projectileHitEffect != null && projectileDetails.projectileHitEffect.projectileHitEffectPrefab != null)
        {
            // Get ammo hit effect gameobject from the pool (with particle system component)
            ProjectileHitEffect projectileHitEffect = (ProjectileHitEffect)PoolManager.Instance.ReuseComponent(projectileDetails.projectileHitEffect.
                projectileHitEffectPrefab, transform.position, Quaternion.identity);

            // Set Hit Effect
            projectileHitEffect.SetHitEffect(projectileDetails.projectileHitEffect);

            // Set gameobject active (the particle system is set to automatically disable the gameobject once finished)
            projectileHitEffect.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Check bleeding status - Player
    /// </summary>
    private void CheckBleedingStatus(Player player)
    {
        if (player.isImmunetoBleeding) return;

        if (projectileDetails.hasBleedingDamage)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.bleedingChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.healthEvent.CallGetBleedingEvent();
                player.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
            }
        }
    }

    /// <summary>
    /// Check bleeding status - Enemy
    /// </summary>
    private void CheckBleedingStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (projectileDetails.hasBleedingDamage || arrowOfTheSeven)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.bleedingChance || arrowOfTheSeven)
            {
                enemy.healthEvent.CallGetBleedingEvent();
                enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
            }
        }
    }

    /// <summary>
    /// Check warmed status - Player
    /// </summary>
    public void CheckWarmStatus(Player player)
    {
        if (player.isImmunetoBurn) return;

        bool isWarmed = (player.healthStatus & HealthStatus.Burned) != 0;

        if (projectileDetails.hasWarmDamage && !isWarmed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.warmChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                if (player.isWarmed && !isWarmed)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.healthStatus |= HealthStatus.Burned;
                    player.healthEvent.CallGetBurnEvent();
                }
                else if (!player.isWarmed && !isWarmed)
                {
                    player.isWarmed = true;
                    player.healthEvent.CallGetWarmedEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check warmed status - Enemy
    /// </summary>
    public void CheckWarmStatus(Enemy enemy)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isWarmed = (enemy.healthStatus & HealthStatus.Burned) != 0;

        if (projectileDetails.hasWarmDamage && !isWarmed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.warmChance)
            {
                if (enemy.isWarmed && !isWarmed)
                {
                    enemy.healthEvent.CallGetBurnEvent();
                    enemy.healthStatus |= HealthStatus.Burned; // Add Burned status

                    enemy.healthEvent.CallWarmCuredEvent();
                }
                else if (!enemy.isWarmed && !isWarmed)
                {
                    enemy.isWarmed = true;
                    enemy.healthEvent.CallGetWarmedEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check burn status - Player
    /// </summary>
    private void CheckBurnStatus(Player player)
    {
        if (player.isImmunetoBurn) return;

        if (projectileDetails.hasBurnDamage)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.burnChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.healthEvent.CallGetBurnEvent();
                player.healthStatus |= HealthStatus.Burned; // Add Burned status
            }
        }
    }

    /// <summary>
    /// Check burn status - Enemy
    /// </summary>
    private void CheckBurnStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (projectileDetails.hasBurnDamage || arrowOfTheSeven)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.burnChance || arrowOfTheSeven)
            {
                enemy.healthEvent.CallGetBurnEvent();
                enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
            }
        }
    }

    /// <summary>
    /// Check slow status - Player
    /// </summary>
    private void CheckSlowStatus(Player player)
    {
        if (player.isImmunetoSlow) return;

        if (projectileDetails.hasSlowDamage && !player.isSlowed)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.slowChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.healthEvent.CallGetSlowEvent();
                player.isSlowed = true;
            }
        }
    }

    /// <summary>
    /// Check slow status - Enemy
    /// </summary>
    private void CheckSlowStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if ((projectileDetails.hasSlowDamage && !enemy.isSlowed) || arrowOfTheSeven)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.slowChance || arrowOfTheSeven)
            {
                enemy.healthEvent.CallGetSlowEvent();
                enemy.isSlowed = true;
            }
        }
    }

    /// <summary>
    /// Check poison status - Player
    /// </summary>
    private void CheckPoisonStatus(Player player)
    {
        if (player.isImmunetoPoison) return;

        if (projectileDetails.hasPoisonDamage)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.poisonChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.healthEvent.CallGetPoisonedEvent();
                player.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
            }
        }
    }

    /// <summary>
    /// Check poison status - Enemy
    /// </summary>
    private void CheckPoisonStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (projectileDetails.hasPoisonDamage || arrowOfTheSeven)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.poisonChance || arrowOfTheSeven)
            {
                enemy.healthEvent.CallGetPoisonedEvent();
                enemy.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
            }
        }
    }

    /// <summary>
    /// Check acid status - Player
    /// </summary>
    private void CheckAcidStatus(Player player)
    {
        if (projectileDetails.hasAcidDamage && player.armorStatus != ArmorStatus.Acid)
        {
            // Check get acid
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.acidEfficiency - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.armorStatus = ArmorStatus.Acid;

                player.acidArmorDebuffModifier = (float)Math.Round(projectileDetails.acidEfficiency, 2);

                // Deduct armor from acid
                player.currentArmorValue = Mathf.Clamp(player.currentArmorValue * (1 - player.acidArmorDebuffModifier), -0.5f,
                    player.currentArmorValue * (1 - player.acidArmorDebuffModifier));

                player.currentArmorValue = (float)Math.Round(projectileDetails.acidEfficiency * player.currentArmorValue, 2);
                player.healthEvent.CallGetAcidEvent();
            }
        }
    }

    /// <summary>
    /// Check acid status - Enemy
    /// </summary>
    private void CheckAcidStatus(Enemy enemy)
    {
        if (projectileDetails.hasAcidDamage && enemy.armorStatus != ArmorStatus.Acid && enemy.health.currentHealth > 0)
        {
            float randomAcidNum = Random.Range(0f, 1f);
            if (randomAcidNum < projectileDetails.acidEfficiency)
            {
                enemy.armorStatus = ArmorStatus.Acid;
                enemy.currentArmor = (float)Math.Round(projectileDetails.acidEfficiency * enemy.currentArmor, 2);
                enemy.healthEvent.CallGetAcidEvent();
            }
        }
    }

    /// <summary>
    /// Check stun status - Player
    /// </summary>
    private void CheckStunStatus(Player player)
    {
        bool isStunned = (player.moveStatus & MoveStatus.Stun) != 0;

        if (projectileDetails.hasStunDamage && !isStunned)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.stunChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Stun;
                player.healthEvent.CallGetStunEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                player.animator.SetBool(Settings.isStunned, true);
            }
        }
    }

    /// <summary>
    /// Check stun status - Enemy
    /// </summary>
    private void CheckStunStatus(Enemy enemy)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isStunned = (enemyAI.moveStatus & MoveStatus.Stun) != 0;

        if (projectileDetails.hasStunDamage && !isStunned)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.stunChance)
            {
                StartCoroutine(StunRoutine(enemy));
            }
        }
    }

    /// <summary>
    /// Check root status - Player
    /// </summary>
    private void CheckRootStatus(Player player)
    {
        bool isRooted = (player.moveStatus & MoveStatus.Root) != 0;

        if (projectileDetails.hasRootDamage && !isRooted)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.rootChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Root;
                player.healthEvent.CallGetRootEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
            }
        }
    }

    /// <summary>
    /// Check root status - Enemy
    /// </summary>
    private void CheckRootStatus(Enemy enemy, bool isBindingArrow = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isRooted = (enemyAI.moveStatus & MoveStatus.Root) != 0;

        EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

        if ((projectileDetails.hasRootDamage && !isRooted && enemy.health.currentHealth > 0) || isBindingArrow)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.rootChance || isBindingArrow)
            {
                StartCoroutine(RootRoutine(enemy));
            }
        }
    }

    /// <summary>
    /// Check chill status - Player
    /// </summary>
    private void CheckChillStatus(Player player)
    {
        if (player.isImmunetoFrost) return;

        bool isFrozen = (player.moveStatus & MoveStatus.Frozen) != 0;

        if (projectileDetails.hasChillDamage && !isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.chillChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                if (player.isChilled && !isFrozen)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus |= MoveStatus.Frozen;
                    player.healthEvent.CallGetFrostEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animatePlayer.ResetAnimatonParameters();
                    player.animator.SetBool(Settings.isFrozen, true);

                    player.healthEvent.CallChillCuredEvent();
                }
                else if (!player.isChilled && !isFrozen)
                {
                    player.isChilled = true;
                    player.healthEvent.CallGetChillEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check chill status - Enemy
    /// </summary>
    public void CheckChillStatus(Enemy enemy, bool isIceBreaker = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isFrozen = (enemyAI.moveStatus & MoveStatus.Frozen) != 0;

        if (projectileDetails.hasChillDamage && !isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.chillChance)
            {
                if (enemy.isChilled && !isFrozen)
                {
                    StartCoroutine(FrostRoutine(enemy)); // Second chill

                    enemy.healthEvent.CallChillCuredEvent();
                }
                else if (!enemy.isChilled && !isFrozen)
                {
                    enemy.isChilled = true;
                    enemy.healthEvent.CallGetChillEvent();
                }
            }
        }
    }


    /// <summary>
    /// Check static status - Player
    /// </summary>
    private void CheckStaticStatus(Player player)
    {
        if (player.isImmunetoParalyze) return;

        bool isParalyzed = (player.moveStatus & MoveStatus.Paralyze) != 0;

        if (projectileDetails.hasStaticDamage && !isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.staticChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                if (player.isStatic && !isParalyzed)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus |= MoveStatus.Paralyze;
                    player.healthEvent.CallGetParalyzedEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animatePlayer.ResetAnimatonParameters();

                    player.healthEvent.CallStaticCuredEvent();
                }
                else if (!player.isStatic && !isParalyzed)
                {
                    player.isStatic = true;
                    player.healthEvent.CallGetStaticEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check static status - Enemy
    /// </summary>
    public void CheckStaticStatus(Enemy enemy, bool isIceBreaker = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isParalyzed = (enemyAI.moveStatus & MoveStatus.Paralyze) != 0;

        if (projectileDetails.hasStaticDamage && !isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < projectileDetails.staticChance)
            {
                if (enemy.isStatic && !isParalyzed)
                {
                    StartCoroutine(ParalyzeRoutine(enemy)); // Second static

                    enemy.healthEvent.CallChillCuredEvent();
                }
                else if (!enemy.isChilled && !isParalyzed)
                {
                    enemy.isChilled = true;
                    enemy.healthEvent.CallGetChillEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check frost status - Player
    /// </summary>
    private void CheckFrostStatus(Player player)
    {
        if (player.isImmunetoFrost) return;

        bool isFrozen = (player.moveStatus & MoveStatus.Frozen) != 0;

        if (projectileDetails.hasFrostDamage && !isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.frostChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Frozen;
                player.healthEvent.CallGetFrostEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                player.animatePlayer.ResetAnimatonParameters();
                player.animator.SetBool(Settings.isFrozen, true);
            }
        }
    }

    /// <summary>
    /// Check frost status - Enemy
    /// </summary>
    private void CheckFrostStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isFrozen = (enemyAI.moveStatus & MoveStatus.Frozen) != 0;

        if ((!isFrozen && projectileDetails.hasFrostDamage) || arrowOfTheSeven)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.frostChance || arrowOfTheSeven)
            {
                StartCoroutine(FrostRoutine(enemy));
            }
        }
    }

    /// <summary>
    /// Check paralyze status - Player
    /// </summary>
    private void CheckParalyzeStatus(Player player)
    {
        if (player.isImmunetoParalyze) return;

        bool isParalyzed = (player.moveStatus & MoveStatus.Paralyze) != 0;

        if (projectileDetails.hasParalyzeDamage && !isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.paralyzeChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Paralyze;
                player.healthEvent.CallGetParalyzedEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                player.animatePlayer.ResetAnimatonParameters();
            }
        }
    }

    /// <summary>
    /// Check paralyze status - Enemy
    /// </summary>
    private void CheckParalyzeStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();

        bool isParalyzed = (enemyAI.moveStatus & MoveStatus.Paralyze) != 0;

        if ((!isParalyzed && projectileDetails.hasParalyzeDamage) || arrowOfTheSeven)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.paralyzeChance || arrowOfTheSeven)
            {
                StartCoroutine(ParalyzeRoutine(enemy));
            }
        }
    }

    /// <summary>
    /// Check shatter status - Enemy
    /// </summary>
    private void CheckShatterStatus(Enemy enemy, ref int inflictedDamage, bool isIceBreaker = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        Health enemyHealth = enemy.GetComponent<Health>();

        bool isFrozen = (enemyAI.moveStatus & MoveStatus.Frozen) != 0;

        float randomDice = Random.Range(0f, 1f);

        if (isFrozen && (randomDice < 0.1f || isIceBreaker))
        {
            inflictedDamage *= 2;
            enemy.isShattered = true;
            enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, null, MeleeHand.MainHand, true);
            enemy.healthEvent.CallGetShatteredEvent();
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
        }
    }

    /// <summary>
    /// Check blind status - Player
    /// </summary>
    private void CheckBlindStatus(Player player)
    {
        if (player.isImmunetoBlind) return;

        if (projectileDetails.hasBlindDamage)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.blindChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.healthEvent.CallGetBlindEvent();
            }
        }
    }

    /// <summary>
    /// Check blind status - Enemy
    /// </summary>
    private void CheckBlindStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if (projectileDetails.hasBlindDamage || arrowOfTheSeven)
        {
            float randomDice = Random.Range(0f, 1f);
            if ((randomDice < projectileDetails.blindChance + GameManager.Instance.GetPlayer().additionalBlindMakerModifier) || arrowOfTheSeven)
            {
                enemy.healthEvent.CallGetBlindEvent();
            }
        }
    }

    /// <summary>
    /// Check curse status - Player
    /// </summary>
    private void CheckCurseStatus(Player player)
    {
        if (projectileDetails.hasCurseDamage && !player.isCursed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.curseChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.isCursed = true;
                player.healthEvent.CallGetCurseEvent();
            }
        }
    }

    /// <summary>
    /// Check curse status - Enemy
    /// </summary>
    private void CheckCurseStatus(Enemy enemy)
    {
        if (projectileDetails.hasCurseDamage && !enemy.isCursed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.curseChance)
            {
                enemy.isCursed = true;
                enemy.healthEvent.CallGetCurseEvent();
            }
        }
    }

    /// <summary>
    /// Check fear status - Player
    /// </summary>
    private void CheckFearStatus(Player player)
    {
        if (projectileDetails.hasFearDamage && !player.isFeared)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.fearChance - player.additionalNegativeStatusEffectNegatorModifier)
            {
                player.isFeared = true;
                player.healthEvent.CallGetFearEvent();
            }
        }
    }

    /// <summary>
    /// Check curse status - Enemy
    /// </summary>
    private void CheckFearStatus(Enemy enemy, bool arrowOfTheSeven = false)
    {
        if ((projectileDetails.hasFearDamage && !enemy.isFeared) || arrowOfTheSeven)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < projectileDetails.fearChance || arrowOfTheSeven)
            {
                enemy.isFeared = true;
                enemy.healthEvent.CallGetFearEvent();
            }
        }
    }

    IEnumerator StunRoutine(Enemy enemy)
    {
        enemy.enemyAI.moveStatus |= MoveStatus.Stun;
        enemy.healthEvent.CallGetStunEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);

        yield return new WaitForFixedUpdate();
    }

    IEnumerator RootRoutine(Enemy enemy)
    {
        enemy.enemyAI.moveStatus |= MoveStatus.Root;
        enemy.healthEvent.CallGetRootEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForFixedUpdate();
    }

    IEnumerator FrostRoutine(Enemy enemy)
    {
        enemy.enemyAI.moveStatus |= MoveStatus.Frozen;
        enemy.healthEvent.CallGetFrostEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);

        yield return new WaitForFixedUpdate();
    }

    IEnumerator ParalyzeRoutine(Enemy enemy)
    {
        enemy.enemyAI.moveStatus |= MoveStatus.Paralyze;
        enemy.healthEvent.CallGetParalyzedEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForFixedUpdate();
    }

    IEnumerator ExplosionRoutine(bool isEnemy = false)
    {
        Animator animator = GetComponent<Animator>();

        if (isFireBlast)
        {
            GetComponent<Animator>().SetTrigger("impact");
            SoundEffectManager.Instance.PlaySoundEffect(projectileDetails.projectileImpactSoundEffect);
            Explosion();

            yield return new WaitForSeconds(0.5f);

            DisableProjectile();
        }
        else if (isEnemy)
        {
            animator.SetTrigger("burst");
            SoundEffectManager.Instance.PlaySoundEffect(projectileDetails.projectileImpactSoundEffect);
            Explosion(true);

            yield return new WaitForSeconds(0.5f);

            DisableProjectile();
        }
    }

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Explosion(bool isEnemy = false)
    {
        StaticEventHandler.CallCameraShakeEvent(4, 0.6f);

        filter.SetLayerMask(layerMask);

        Collider2D[] results = new Collider2D[20];
        int hitCount = Physics2D.OverlapCircle(transform.position, blastRadius, filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D collider = results[i];
            if (collider == null) continue;

            if (collider is PolygonCollider2D)
            {
                if (isEnemy)
                {
                    // Don't hit yourself if player is also in the collider list
                    if (collider.tag == Settings.enemyTag) continue;

                    if (collider.tag == Settings.playerTag)
                    {
                        Player player = collider.GetComponent<Player>();

                        int inflictedDamage = CalculateDamageAmount(null, true);
                        player.health.TakeDamage(inflictedDamage, transform.position, player.transform.position, collider, MeleeHand.None);

                        CheckAcidStatus(player);
                        CheckStunStatus(player);

                        // Apply knockback
                        Vector2 knockbackDir = (player.transform.position - transform.position).normalized;
                        float knockbackForce = 0.5f; // Adjust force to your liking

                        player.movementByForce.ApplyKnockback(knockbackDir, knockbackForce, 1);
                    }
                }
                else
                {
                    // Don't hit yourself if player is also in the collider list
                    if (collider.tag == Settings.playerTag) continue;

                    if (collider.tag == Settings.enemyTag)
                    {
                        Enemy enemy = collider.GetComponent<Enemy>();

                        int inflictedDamage = CalculateDamageAmount(enemy);
                        enemy.health.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, collider, MeleeHand.None);

                        if (isFireBlast)
                        {
                            CheckBurnStatus(enemy);
                        }

                        if (!enemy.enemyDetails.hasKnockbackResistance && enemy.GetComponent<Health>().currentHealth > 0)
                        {
                            // Knockback
                            Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                            float knockbackForce = 5f;
                            float dealDamageMass = 1f;

                            enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                        }
                    }
                }
            }
            else
            {
                if (isEnemy)
                {
                    collider.GetComponent<Health>().TakeDamage(Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax),
                        transform.position, collider.transform.position, collider, MeleeHand.None);
                }
                else
                {
                    if (isFireBlast)
                    {
                        collider.GetComponent<Health>().TakeDamage(Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax),
                            transform.position, collider.transform.position, collider, MeleeHand.None);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    private int CalculateDamageAmount(Enemy enemy, bool isEnemy = false)
    {
        int damageDone = 0;
        int inflictedDamage = 0;
        Health health;

        if (isEnemy)
        {
            // Damage produced by enemy
            damageDone = Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax);

            // Damage inflicted to player after deducting player armor
            health = player.GetComponent<Health>();
            inflictedDamage = (int)(damageDone * (1 - player.currentArmorValue));
        }
        else
        {
            if (isFireBlast)
            {
                // Damage produced by player
                damageDone = Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax);
            }

            // Damage inflicted to enemy after deducting enemy armor
            health = enemy.GetComponent<Health>();

            inflictedDamage = (int)(damageDone * (1 - enemy.currentArmor));
        }

        return inflictedDamage;
    }

    private Transform FindClosestEnemy()
    {
        float closestDistance = Mathf.Infinity;
        Transform closestEnemy = null;

        Enemy[] enemyArrayInRoom = EnemySpawner.Instance.GetComponentsInChildren<Enemy>();

        for (int i = 0; i < enemyArrayInRoom.Length; i++)
        {
            float distance = Vector3.Distance(transform.position, enemyArrayInRoom[i].GetEnemyPosition());

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestEnemy = enemyArrayInRoom[i].transform;
            }
        }

        return closestEnemy;
    }

    private void OnHitEnemy(Enemy hitEnemy)
    {
        // Record this hit for the chain
        if (chainHitSet == null) chainHitSet = new HashSet<Enemy>();
        chainHitSet.Add(hitEnemy);

        if (isFireBlast)
        {
            if (explosionRoutine == null)
            {
                velocity = new Vector3(0f, 0f, 1f);
                explosionRoutine = StartCoroutine(ExplosionRoutine());
            }
            return;
        }

        // Chain logic: First -> Second -> Third
        if (isChainLightning && (chainLightningPhase == ChainLightningPhase.First || chainLightningPhase == ChainLightningPhase.Second))
        {
            Vector2 origin = hitEnemy.transform.position;

            // Find next target excluding those already hit
            Enemy next = FindClosestEnemyFrom(origin, chainHitSet, chainJumpRadius, hitEnemy);

            if (next != null)
            {
                // Spawn next hop at the hit position
                GameObject projectilePrefab = projectileDetails.projectilePrefabArray[0];
                IFireable projIFireable = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, origin, Quaternion.identity);
                Projectile nextProj = projIFireable as Projectile;

                // Compute direction/angle from origin to next
                Vector3 toNext = (next.transform.position + new Vector3(0f, 0.7f, 0f) - (Vector3)origin).normalized;
                float aimAngle = HelperUtilities.GetAngleFromVector(toNext);

                // Carry state forward
                nextProj.isChainLightning = true;
                nextProj.chainLightningPhase = chainLightningPhase + 1;
                nextProj.isGuided = true;
                nextProj.target = next.transform;
                nextProj.chainHitSet = chainHitSet;       // share the same set
                nextProj.projectileDetails = projectileDetails;

                // Initialize with the incremented phase (important so Start() doesn’t clobber it)
                nextProj.InitializeProjectile(null, false, projectileDetails, aimAngle, aimAngle, projectileSpeed: 35f, toNext, false, false, false, projectileCounter: 1, 
                    projectilesPerShot: 1, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false, null, null, false, null, false, null, false, null, false, null, true, 
                    nextProj.projectileDetails, nextProj.chainLightningPhase);  // pass the incremented phase

                // Optional damage falloff
                nextProj.damageDone = Mathf.RoundToInt(damageDone * chainDamageFalloff);

                // Optional small hop delay
                if (chainDelay > 0f) nextProj.StartCoroutine(nextProj.DelayArming(chainDelay));
            }
        }

        // Current projectile is done after resolving the hit/chain
        DisableProjectile();
    }

    private Enemy FindClosestEnemyFrom(Vector2 origin, HashSet<Enemy> exclude, float radius, Enemy ignoreEnemy)
    {
        // Use your layer mask/filter
        Collider2D[] hits = Physics2D.OverlapCircleAll(origin, radius, LayerMask.GetMask("Enemy"));
        Enemy best = null;
        float bestDist = float.MaxValue;

        for (int i = 0; i < hits.Length; i++)
        {
            // Make sure this collider belongs to an Enemy
            Enemy e = hits[i].GetComponent<Enemy>();
            if (e == null || !e.isActiveAndEnabled) continue; // Ignore the same enemy we just hit
            if (exclude != null && exclude.Contains(e)) continue; // Skip any already-hit enemies in this chain
            if (!(hits[i] is PolygonCollider2D)) continue; // Only accept polygon colliders

            // Distance check
            float d = Vector2.SqrMagnitude((Vector2)e.transform.position - origin);
            if (d < bestDist)
            {
                bestDist = d;
                best = e;
            }
        }

        return best;
    }

    // Optional small arming delay so the guided step feels like a hop
    private IEnumerator DelayArming(float delay)
    {
        // Prevent immediate movement if you want (e.g., zero velocity)
        Vector3 cachedVel = velocity;
        velocity = Vector3.zero;
        yield return new WaitForSeconds(delay);
        velocity = cachedVel;
    }

    public void ResetProjectileState()
    {
        isStopped = false;
        isColliding = false;
        isProjectileMaterialSet = false;
        directionInitialized = false;
        isHittingWall = false;
        lightningStroke = false;
        hasGrappled = false;
        isGrappleReleased = false;
        grappleHasDealtDamage = false;

        velocity = Vector3.zero;
        projectileRange = 0f;
        projectileSpeed = 0f;

        if (trailRenderer != null)
        {
            trailRenderer.Clear();
            trailRenderer.emitting = false;
        }
    }

    public void SetProjectileMaterial(Material material)
    {
        if (!isGrappleHook) spriteRenderer.material = material;
        else ropeSpriteRenderer.material = material;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
    }

    public void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = this == null ? Vector3.zero : transform.position;
        Gizmos.DrawWireSphere(position, blastRadius);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(trailRenderer), trailRenderer);
    }
#endif
    #endregion Validation
}
