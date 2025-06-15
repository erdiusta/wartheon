using System.Collections;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

[DisallowMultipleComponent]
public class Projectile : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("Populate with child TrailRenderer component")]
    #endregion Tooltip
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] LayerMask layerMask;

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
    SpriteRenderer spriteRenderer;
    ActiveItemDetailsSO activeItemDetails;
    float projectileChargeTimer;
    bool isProjectileMaterialSet;
    bool overrideProjectileMovement;
    bool isColliding;
    Vector3 velocity;
    PolygonCollider2D polygonCollider2D;
    bool headShotHappened;
    bool isPenetrationArrow;
    float countDown = 3f;
    float blastRadius = 5f;
    Coroutine explosionRoutine;
    int damageDone = 0;
    bool lightningStroke;
    bool isHittingWall; // Flag is for wall hit check for penetration arrow

    // Laser features
    [SerializeField] float angleSpeed = 10f;  // How fast the laser extends

    bool isLaserBeam;
    float laserDuration;
    float laserDurationOffset = 0.4f;
    Transform laserStartPoint;
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
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
    }

    private void OnEnable()
    {
        velocity = fireDirectionVector.normalized * projectileSpeed;

        if (activeItemDetails != null)
        {
            if (activeItemDetails.activeItemType == ActiveItemType.Bomb || activeItemDetails.activeItemType == ActiveItemType.Incendiary)
            {
                countDown = activeItemDetails.countDown;
                blastRadius = activeItemDetails.blastRadius;
            }
            else if (activeItemDetails.activeItemType == ActiveItemType.Pentagram)
            {
                blastRadius = activeItemDetails.blastRadius;
            }
        }

        if (tag == "meteor")
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }

        if (isLaserBeam)
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

        if (projectileDetails != null)
        {
            damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);

            if (projectileDetails.isPlayerProjectile)
            {
                if (projectileDetails.isGuided && updateGuidedMissleTimer <= 0f)
                {
                    updateGuidedMissleTimer = 1f;
                    target = FindClosestEnemy(); // Your own method to get a target enemy
                }
            }
            else
            {
                if (projectileDetails.isGuided)
                {
                    target = GameManager.Instance.GetPlayer().transform;
                }
            }
        }

        if (isLaserBeam)
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

        // Projectile charge effect
        if (projectileChargeTimer > 0f)
        {
            projectileChargeTimer -= Time.deltaTime;
            return;
        }
        else if (!isProjectileMaterialSet)
        {
            if (activeItemDetails == null)
            {
                SetProjectileMaterial(projectileDetails.projectileMaterial);
            }
            else
            {
                SetProjectileMaterial(activeItemDetails.projectileMaterial);
            }
            isProjectileMaterialSet = true;
        }

        if (activeItemDetails != null)
        {
            // Start countdown until explosion if this is an active item bomb
            if (activeItemDetails.activeItemType == ActiveItemType.Bomb || activeItemDetails.activeItemType == ActiveItemType.Incendiary)
            {
                countDown -= Time.deltaTime;

                if (countDown < 0f)
                {
                    if (explosionRoutine == null)
                    {
                        explosionRoutine = StartCoroutine(ExplosionRoutine());
                    }
                }
            }
        }

        if (isLaserBeam)
        {
            UpdateLaser();
        }
        else
        {
            if (projectileDetails != null && projectileDetails.isGuided && target != null)
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
                if (activeItemDetails == null)
                {
                    if (!projectileDetails.isTrap && venomancerPhase != VenomancerPhase.ToxicPool)
                    {
                        DisableProjectile();
                    }
                }
                else if (activeItemDetails != null && activeItemDetails.activeItemType != ActiveItemType.Bomb &&
                    activeItemDetails.activeItemType != ActiveItemType.Incendiary && activeItemDetails.activeItemType != ActiveItemType.Decoy)
                {
                    DisableProjectile();
                }
                else
                {
                    velocity = Vector3.zero;
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

            if (player.onStealth)
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

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // **If this is a laser, don't disable it on collision**
        if (projectileDetails != null && isLaserBeam)
        {
            return;  // Let the laser continue without being disabled
        }

        // If already colliding with something return
        if (isColliding) return;

        if (activeItemDetails != null)
        {
            if (activeItemDetails.activeItemType == ActiveItemType.Bomb) return;
        }

        if (collision.tag == Settings.playerTag)
        {
            Player player = collision.GetComponent<Player>();

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

                    float blockingThreshold = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.projectileDeflectRatio;

                    // Check if the dot product is greater than the threshold, block fails
                    if (dotProduct > blockingThreshold - 1f)
                    {
                        // Status checks
                        CheckBurnStatus(player);
                        CheckPoisonStatus(player);
                        CheckAcidStatus(player);
                        CheckFrostStatus(player);
                        CheckStunStatus(player);
                        CheckCurseStatus(player);
                        CheckBlindStatus(player);

                        // Deal Damage To Collision Object
                        DealDamage(collision);
                    }
                    else
                    {
                        // If the player is attacking, guard is down so block is disabled
                        if (player.meleeAttackMainHand.IsAttacking)
                        {
                            // Status checks
                            CheckBurnStatus(player);
                            CheckPoisonStatus(player);
                            CheckAcidStatus(player);
                            CheckFrostStatus(player);
                            CheckStunStatus(player);
                            CheckCurseStatus(player);
                            CheckBlindStatus(player);

                            // Deal Damage To Collision Object
                            DealDamage(collision);
                        }
                        else
                        {
                            if (playerBlockCoroutine == null)
                            {
                                // The projectile is within the blocking angle
                                playerBlockCoroutine = StartCoroutine(PlayerBlockAnimRoutine(collision));
                            }
                        }
                    }
                }
                else
                {
                    // Status checks
                    CheckBurnStatus(player);
                    CheckPoisonStatus(player);
                    CheckAcidStatus(player);
                    CheckFrostStatus(player);
                    CheckStunStatus(player);
                    CheckCurseStatus(player);
                    CheckBlindStatus(player);

                    // Deal Damage To Collision Object
                    DealDamage(collision);
                }
            }

            // Show ammo hit effect
            ProjectileHitEffect();

            DisableProjectile();
        }
        else if (collision.tag == Settings.playerWeapon)
        {

        }
        else if(collision.tag == Settings.enemyTag)
        {
            Enemy enemy = collision.GetComponent<Enemy>();

            if (collision.GetComponent<Enemy>() != null)
            {
                if (activeItemDetails != null)
                {
                    if (activeItemDetails.activeItemType == ActiveItemType.Boomerang)
                    {
                        ProjectilePattern projectilePattern = GetComponentInParent<ProjectilePattern>();
                        projectilePattern.boomerangPhase = BoomerangPhase.Return;
                    }
                    else if (activeItemDetails.activeItemType == ActiveItemType.Pentagram)
                    {
                        if (explosionRoutine == null)
                        {
                            explosionRoutine = StartCoroutine(ExplosionRoutine());
                            return;
                        }
                    }
                }

                if (enemy.enemyDetails.hasShield && !headShotHappened)
                {
                    int diceRoll = Random.Range(0, 100);
                    bool deflectHappened = 100 - enemy.enemyDetails.deflectionValue * 100 < diceRoll ? true : false;
                  
                    if (deflectHappened)
                    {
                        enemy.health.isBlocking = true;
                        enemy.healthEvent.CallDodgeEvent();
                        enemy.health.TakeDamage(0, transform.position, enemy.health.transform.position, false, collision, MeleeHand.None);
                    }
                    else
                    {
                        if (activeItemDetails == null)
                        {
                            // Status checks - PROJECTILE
                            CheckBurnStatus(enemy);
                            CheckPoisonStatus(enemy);
                            CheckAcidStatus(enemy);
                            CheckFrostStatus(enemy);
                            CheckStunStatus(enemy);
                            CheckCurseStatus(enemy);
                            CheckBlindStatus(enemy);

                            // Deal Damage To Collision Object
                            DealDamage(collision);
                        }
                        else
                        {
                            // Status checks - ACTIVE ITEM
                            CheckBurnStatus(enemy, true);
                            CheckPoisonStatus(enemy, true);
                            CheckAcidStatus(enemy, true);
                            CheckFrostStatus(enemy, true);
                            CheckStunStatus(enemy, true);
                            CheckCurseStatus(enemy, true);
                            CheckBlindStatus(enemy, true);

                            // Deal Damage To Collision Object
                            DealDamage(collision, true);
                        }
                    }
                }
                else
                {
                    if (activeItemDetails == null)
                    {
                        // Status checks - PROJECTILE
                        CheckBurnStatus(enemy);
                        CheckPoisonStatus(enemy);
                        CheckAcidStatus(enemy);
                        CheckFrostStatus(enemy);
                        CheckStunStatus(enemy);
                        CheckCurseStatus(enemy);
                        CheckBlindStatus(enemy);

                        // Deal Damage To Collision Object
                        DealDamage(collision);
                    }
                    else
                    {
                        // Status checks - ACTIVE ITEM
                        CheckBurnStatus(enemy, true);
                        CheckPoisonStatus(enemy, true);
                        CheckAcidStatus(enemy, true);
                        CheckFrostStatus(enemy, true);
                        CheckStunStatus(enemy, true);
                        CheckCurseStatus(enemy, true);
                        CheckBlindStatus(enemy, true);

                        // Deal Damage To Collision Object
                        DealDamage(collision, true);
                    }
                }
            }
            else
            {
                if (activeItemDetails == null)
                {
                    // Status checks - PROJECTILE
                    CheckBurnStatus(enemy);
                    CheckPoisonStatus(enemy);
                    CheckAcidStatus(enemy);
                    CheckFrostStatus(enemy);
                    CheckStunStatus(enemy);
                    CheckCurseStatus(enemy);
                    CheckBlindStatus(enemy);

                    // Deal Damage To Collision Object
                    DealDamage(collision);
                }
                else
                {
                    // Status checks - ACTIVE ITEM
                    CheckBurnStatus(enemy, true);
                    CheckPoisonStatus(enemy, true);
                    CheckAcidStatus(enemy, true);
                    CheckFrostStatus(enemy, true);
                    CheckStunStatus(enemy, true);
                    CheckCurseStatus(enemy, true);
                    CheckBlindStatus(enemy, true);

                    // Deal Damage To Collision Object
                    DealDamage(collision, true);
                }
            }

            // Show ammo hit effect
            ProjectileHitEffect();

            DisableProjectile();
        }
        else if (collision.tag == Settings.decoyTag)
        {
            // Deal Damage To Collision Object
            DealDamage(collision);
            // Show ammo hit effect
            ProjectileHitEffect();

            DisableProjectile();
        }
        else if (collision.tag == "PracticeDummy")
        {
            DummyCheck(collision);

            DisableProjectile();
        }
        else if (collision.tag == Settings.playerWeapon)
        {
            return;
        }
        else // HIT WALL CHECK
        {
            isHittingWall = true;

            if (activeItemDetails != null)
            {

                if (activeItemDetails.activeItemType == ActiveItemType.Boomerang)
                {
                    ProjectilePattern projectilePattern = GetComponentInParent<ProjectilePattern>();
                    projectilePattern.boomerangPhase = BoomerangPhase.Return;
                }
                //else if (activeItemDetails.activeItemType == ActiveItemType.Shiruken)
                //{
                //    ProjectilePattern projectilePattern = GetComponentInParent<ProjectilePattern>();
                //    projectilePattern.shirukenPhase = ShirukenPhase.Ricochet;
                //    DoProjectileHitEffect();
                //    return;
                //}
            }

            // Deal Damage To Collision Object
            if (activeItemDetails != null)
            {
                DealDamage(collision, true);
            }
            else
            {
                DealDamage(collision);
            }

            // Show ammo hit effect
            ProjectileHitEffect();

            DisableProjectile();
        }
    }

    // This is for bouncing projectiles
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (projectileDetails != null && projectileDetails.isBouncing)
        {
            // Reflect off wall/prop
            Vector2 normal = collision.contacts[0].normal;
            velocity = Vector2.Reflect(velocity, normal);

            float angle = HelperUtilities.GetAngleFromVector(velocity);
            transform.rotation = Quaternion.Euler(0f, 0f, angle);

            return;
        }

        DisableProjectile();
    }


    IEnumerator PlayerBlockAnimRoutine(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        // Adjust animator layer weights
        player.transform.GetChild(1).GetComponent<Animator>().SetTrigger(Settings.block);
        SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponSwingSoundEffect);
        player.healthEvent.CallBlockEvent();

        yield return new WaitForSeconds(0.6f);

        playerBlockCoroutine = null;
        player.animatePlayer.ResetAnimatonParameters();
        player.animator.SetBool(Settings.isIdle, true);
    }

    private void DealDamage(Collider2D collision, bool activeItem = false, bool isLaser = false)
    {
        Health health = collision.GetComponent<Health>();

        if (health != null)
        {
            // Set isColliding to prevent ammo dealing damage multiple times
            if (isPenetrationArrow)
            {
                StartCoroutine(ColliderTimeThreshold());
            }
            else if(!isLaser) // Laser do not need this
            {
                isColliding = true;
            }

            if (!activeItem)
            {
                if (collision != null && collision.GetComponent<Enemy>() != null)
                {
                    // Caster is player
                    damageDone = Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);
                }
                else
                {
                    damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);
                }
            }
            else
            { 
                damageDone = Random.Range(activeItemDetails.projectileDamageMin, activeItemDetails.projectileDamageMax);
            }

            if (isPenetrationArrow)
            {
                float increasedDamage = damageDone * 1.25f;
                damageDone = (int)(increasedDamage * (1 + player.additionalPenetrationSkillDamageModifier));
            }

            int inflictedDamage = 0;

            if (collision != null && collision.GetComponent<Enemy>() != null)
            {
                Enemy enemy = collision.GetComponent<Enemy>();

                if (!activeItem)
                {
                    if (projectileDetails.isPlayerProjectile)
                    {
                        // LOWER DAMAGE IF PLAYER IS CURSED - PROJECTILE
                        damageDone = player.isCursed ? player.currentMainHandMinDamageValue : Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);
                    }
                }
                else
                {
                    // LOWER DAMAGE IF PLAYER IS CURSED - ACTIVE ITEM
                    damageDone = player.isCursed ? activeItemDetails.projectileDamageMin : Random.Range(activeItemDetails.projectileDamageMin, activeItemDetails.projectileDamageMax);
                }

                if (headShotHappened)
                {
                    // x2.5 damage if used headshot and add additional modifier if has
                    damageDone = (int)(2.5f * damageDone * (1 + player.additionalHeadShotDamageModifier));
                }

                // Segregate elemental and non-elemental damage
                int elementalDamage = 0;
                int nonElementalDamage = 0;
                int inflictedNonElementalDamage = 0;
                int additionalElementalCataclysmDamage = 0;
                int additionalElementalDamage = 0;

                if (!activeItem)
                {
                    if (projectileDetails.isCataclysmProjectile)
                    {
                        elementalDamage = (int)(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.elementalForgeRate * damageDone);
                    }
                    else
                    {
                        elementalDamage = (int)(projectileDetails.belongingWeaponDetails.elementalForgeRate * damageDone);
                        additionalElementalDamage = (int)(elementalDamage * (player.additionalStaffElementalDamageModifier + player.additionalElementalDamageModifier));
                        elementalDamage += additionalElementalDamage;
                    }

                    // Check if projectile is cataclysm projectile
                    if (projectileDetails.isCataclysmProjectile)
                    {
                        additionalElementalCataclysmDamage = (int)(elementalDamage * (player.additionalCataclysmElementalDamageModifier + player.additionalElementalDamageModifier));
                        elementalDamage += additionalElementalCataclysmDamage;
                    }

                    if (projectileDetails.isCataclysmProjectile)
                    {
                        nonElementalDamage = damageDone - elementalDamage + additionalElementalCataclysmDamage;
                    }
                    else
                    {
                        nonElementalDamage = damageDone - elementalDamage + additionalElementalDamage;
                    }

                    inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - enemy.currentPhysicalResistance));
                }
                else
                {
                    inflictedDamage = (int)(damageDone * (1 - enemy.currentPhysicalResistance));
                }

                int inflictedElementalDamage = 0;

                // Damage inflicted to enemy after deducting enemy armor
                if (!activeItem)
                {
                    // Retrieve weapon details if projectile cataclysm or not
                    WeaponDetailsSO weaponDetails = projectileDetails.isCataclysmProjectile ? player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails :
                        projectileDetails.belongingWeaponDetails;

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
            }
            else if (collision != null && collision.GetComponent<Player>() != null)
            {
                Player player = collision.GetComponent<Player>();

                inflictedDamage = 0;

                // Segregate elemental and non-elemental damage
                int elementalDamage = (int)(projectileDetails.belongingWeaponDetails.elementalForgeRate * damageDone);
                int nonElementalDamage = damageDone - elementalDamage;

                int inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - player.currentPhysicalResistanceValue));

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

            health.TakeDamage(inflictedDamage, transform.position, health.transform.position, headShotHappened, collision, MeleeHand.None);
        }
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
        if (player.onStealth)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier + player.additionalCriticalDamageOnStealth)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        health.PostHitImmunity();
        health.TakeDamage(damageDone, transform.position, health.transform.position, false, collider, MeleeHand.None);
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
    public void InitializeProjectile(Enemy belongingEnemy, bool headShotHappened, ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle,
        float projectileSpeed, Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false, bool fallingFromSkies = false,
        bool isPenetrationArrow = false, int projectileCounter = 0, int projectilesPerShot = 0,CentaurPhase centaurPhase = CentaurPhase.None,
        TreantPhase treantPhase = TreantPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None,
        FrostWrymPhase frostWrymPhase = FrostWrymPhase.None, VenomancerPhase venomancerPhase = VenomancerPhase.None, FireWrymPhase fireWrymPhase = FireWrymPhase.None,
        MoldranPhase moldranPhase = MoldranPhase.None)
    {
        #region Projectile

        this.projectileDetails = projectileDetails;

        // Set head shot bool
        this.headShotHappened = headShotHappened;

        // Set penetration arrow bool
        this.isPenetrationArrow = isPenetrationArrow;

        // Set laser status
        isLaserBeam = projectileDetails.isLaser;

        // Initialize isColliding
        isColliding = false;

        // Set belonging enemy if it is
        this.belongingEnemy = belongingEnemy;

        // Set fire direction
        SetFireDirection(projectileDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileCounter, projectilesPerShot, centaurPhase, treantPhase, galvanusPhase,
            sepharothPhase, frostWrymPhase, venomancerPhase, fireWrymPhase, moldranPhase);

        //// Set projectile sprite
        //spriteRenderer.sprite = projectileDetails.projectileSprite;

        // Play sound if it is a unique projectile
        if (galvanusPhase == GalvanusPhase.Lightning)
        {

            lightningStroke = true;
        }
        else if (frostWrymPhase == FrostWrymPhase.Icicle || fireWrymPhase == FireWrymPhase.FirePillar || moldranPhase == MoldranPhase.Spike)
        {
            SoundEffectManager.Instance.PlaySoundEffect(projectileDetails.projectileImpactSoundEffect);
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

        if (headShotHappened)
        {
            spriteRenderer.material = GameManager.Instance.GetPlayer().playerDetails.headShotMaterial;
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
    /// Initialize the projectile being fired - using the projectileDetails, the aimangle, weaponAngle, and weaponAimDirectionVector. If this 
    /// projectile is part of a pattern the projectile movement can be overriden by setting overrideAmmoMovement to true - ACTIVEITEM
    /// </summary>
    public void InitializeProjectile(bool headShotHappened, ActiveItemDetailsSO activeItemDetails, float aimAngle, 
        float weaponAimAngle, float projectileSpeed, Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false)
    {
        #region Projectile - Active Item

        this.activeItemDetails = activeItemDetails;

        // Set head shot bool
        this.headShotHappened = headShotHappened;

        // Initialize isColliding
        isColliding = false;

        // Set fire direction
        SetFireDirection(activeItemDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // Set projectile sprite
        spriteRenderer.sprite = activeItemDetails.activeItemSprite;

        // Set initial projectile material depending on whether there is an projectile charge period
        if (activeItemDetails.projectileChargeTime > 0f)
        {
            // Set ammo charge timer
            projectileChargeTimer = activeItemDetails.projectileChargeTime;
            SetProjectileMaterial(activeItemDetails.projectileChargeMaterial);
            isProjectileMaterialSet = false;
        }
        else
        {
            projectileChargeTimer = 0f;
            SetProjectileMaterial(activeItemDetails.projectileMaterial);
            isProjectileMaterialSet = true;
        }

        // Set projectile range
        projectileRange = activeItemDetails.projectileRange;

        // Set projectile speed
        this.projectileSpeed = projectileSpeed;

        // Override projectile movement
        this.overrideProjectileMovement = overrideProjectileMovement;

        // Activate projectile gameObject
        gameObject.SetActive(true);

        #endregion

        #region Trail

        if (activeItemDetails.isProjectileTrail)
        {
            trailRenderer.gameObject.SetActive(true);
            trailRenderer.emitting = true;
            trailRenderer.material = activeItemDetails.projectileTrailMaterial;
            trailRenderer.startWidth = activeItemDetails.projectileTrailStartWidth;
            trailRenderer.endWidth = activeItemDetails.projectileTrailEndWidth;
            trailRenderer.time = activeItemDetails.projectileTrailTime;
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
        int projectileCounter = 0, int totalProjectiles = 0, CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None, 
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None, FrostWrymPhase frostWrymPhase = FrostWrymPhase.None,
        VenomancerPhase venomancerPhase = VenomancerPhase.None, FireWrymPhase fireWrymPhase = FireWrymPhase.None, MoldranPhase moldranPhase = MoldranPhase.None)
    {
        if (centaurPhase == CentaurPhase.SpreadArrowShot)
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
        else if (treantPhase == TreantPhase.RazorLeaf)
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
        else
        {
            transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

            // Set projectile fire direction
            fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
        }
    }

    /// <summary>
    /// Set projectile fire direction and angle based on the input angle and direction adjusted by the
    /// random spread - ACTIVE ITEM
    private void SetFireDirection(ActiveItemDetailsSO activeItemDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        // Calculate random spread angle between min and max
        float randomSpread = Random.Range(activeItemDetails.projectileSpreadMin, activeItemDetails.projectileSpreadMax);

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

        // Set projectile rotation
        transform.eulerAngles = new Vector3(0f, 0f, fireDirectionAngle);

        // Set projectile fire direction
        fireDirectionVector = HelperUtilities.GetDirectionVectorFromAngle(fireDirectionAngle);
    }

    /// <summary>
    /// Disable the projectile - thus returning it to the object pool
    /// </summary>
    private void DisableProjectile()
    {
        if (activeItemDetails != null)
        {
            switch (activeItemDetails.activeItemType)
            {
                case ActiveItemType.Boomerang:
                case ActiveItemType.Bomb:
                case ActiveItemType.Decoy:
                    return;
                case ActiveItemType.Generic:
                case ActiveItemType.Shiruken:
                case ActiveItemType.Pentagram:
                    break;
                default:
                    break;
            }
        }

        if (!isPenetrationArrow )
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

        if (tag == "meteor")
        {
            GetComponentInChildren<Animator>().SetTrigger("impact");
            StaticEventHandler.CallCameraShakeEvent(GameManager.Instance.GetPlayer().playerDetails.shakeIntensity, GameManager.Instance.GetPlayer().playerDetails.shakeDuration);
            SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.specialMoveThreeSoundEffect);
            StartCoroutine(DisableProcess(0.2f));
        }
        else
        {
            if (transform.GetComponentInParent<ProjectilePattern>() != null)
            {

            }
            GetComponent<Animator>().SetTrigger("impact");
        }

        if (!isPenetrationArrow)
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

        // **If the player is inside the beam, apply continuous damage**
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
        DealDamage(collider, false, true);
    }

    IEnumerator DisableProcess(float disableDuration)
    {
        yield return new WaitForSeconds(disableDuration);

        isHittingWall = false;
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Display the ammo hit effect
    /// </summary>
    private void ProjectileHitEffect()
    {
        if (activeItemDetails == null)
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
    }

    /// <summary>
    /// Check burn status - Player
    /// </summary>
    private void CheckBurnStatus(Player player, bool isActiveItem = false)
    {
        if (player.isImmunetoBurn) return;

        if (!isActiveItem)
        {
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
        else
        {
            if (activeItemDetails.hasBurnDamage)
            {
                // Check get poisoned
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.burnChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.healthEvent.CallGetBurnEvent();
                    player.healthStatus |= HealthStatus.Burned; // Add Burned status
                }
            }
        }
    }

    /// <summary>
    /// Check burn status - Enemy
    /// </summary>
    private void CheckBurnStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.hasBurnDamage)
            {
                // Check get bleeding
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.burnChance)
                {
                    enemy.healthEvent.CallGetBurnEvent();
                    enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
                }
            }
        }
        else
        {
            if (activeItemDetails.hasBurnDamage)
            {
                // Check get bleeding
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.burnChance)
                {
                    enemy.healthEvent.CallGetBurnEvent();
                    enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
                }
            }
        }
    }

    /// <summary>
    /// Check poison status - Player
    /// </summary>
    private void CheckPoisonStatus(Player player, bool isActiveItem = false)
    {
        if (player.isImmunetoPoison) return;

        if (!isActiveItem)
        {
            if (projectileDetails.isPoisonous)
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
        else
        {
            if (activeItemDetails.isPoisonous)
            {
                // Check get poisoned
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.poisonChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.healthEvent.CallGetPoisonedEvent();
                    player.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
                }
            }
        }
    }

    /// <summary>
    /// Check poison status - Enemy
    /// </summary>
    private void CheckPoisonStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.isPoisonous)
            {
                // Check get bleeding
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.poisonChance)
                {
                    enemy.healthEvent.CallGetPoisonedEvent();
                    enemy.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
                }
            }
        }
        else
        {
            if (activeItemDetails.isPoisonous)
            {
                // Check get bleeding
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.poisonChance)
                {
                    enemy.healthEvent.CallGetPoisonedEvent();
                    enemy.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
                }
            }
        }
    }

    /// <summary>
    /// Check acid status - Player
    /// </summary>
    private void CheckAcidStatus(Player player, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.hasAcid && player.armorStatus != ArmorStatus.Acid)
            {
                // Check get acid
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.acidEfficiency - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    if (player.armorStatus == ArmorStatus.SilverArmor)
                    {
                        player.healthEvent.CallArmorWoreOffEvent();
                    }

                    player.armorStatus = ArmorStatus.Acid;
                    player.currentPhysicalResistanceValue = (float)Math.Round(projectileDetails.acidEfficiency * player.currentPhysicalResistanceValue, 2);
                    player.healthEvent.CallGetAcidEvent();
                }
            }
        }
        else
        {
            if (activeItemDetails.hasAcid && player.armorStatus != ArmorStatus.Acid)
            {
                // Check get acid
                float randomDice = Random.Range(0f, 1f - player.additionalNegativeStatusEffectNegatorModifier);
                if (randomDice < activeItemDetails.acidEfficiency)
                {
                    if (player.armorStatus == ArmorStatus.SilverArmor)
                    {
                        player.healthEvent.CallArmorWoreOffEvent();
                    }

                    player.armorStatus = ArmorStatus.Acid;
                    player.currentPhysicalResistanceValue = (float)Math.Round(activeItemDetails.acidEfficiency * player.currentPhysicalResistanceValue, 2);
                    player.healthEvent.CallGetAcidEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check acid status - Enemy
    /// </summary>
    private void CheckAcidStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.hasAcid && enemy.armorStatus != ArmorStatus.Acid && enemy.health.currentHealth > 0)
            {
                float randomAcidNum = Random.Range(0f, 1f);
                if (randomAcidNum < projectileDetails.acidEfficiency)
                {
                    enemy.armorStatus = ArmorStatus.Acid;
                    enemy.currentPhysicalResistance = (float)Math.Round(projectileDetails.acidEfficiency * enemy.currentPhysicalResistance, 2);
                    enemy.healthEvent.CallGetAcidEvent();
                }
            }
        }
        else
        {
            if (activeItemDetails.hasAcid && enemy.armorStatus != ArmorStatus.Acid && enemy.health.currentHealth > 0)
            {
                float randomAcidNum = Random.Range(0f, 1f);
                if (randomAcidNum < activeItemDetails.acidEfficiency)
                {
                    enemy.armorStatus = ArmorStatus.Acid;
                    enemy.currentPhysicalResistance = (float)Math.Round(activeItemDetails.acidEfficiency * enemy.currentPhysicalResistance, 2);
                    enemy.healthEvent.CallGetAcidEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check stun status - Player
    /// </summary>
    private void CheckStunStatus(Player player, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.hasStunDamage && player.moveStatus != MoveStatus.Stun)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.stunChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus = MoveStatus.Stun;
                    player.healthEvent.CallGetStunEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animator.SetBool(Settings.isStunned, true);
                }
            }
        }
        else
        {
            if (activeItemDetails.hasStunDamage && player.moveStatus != MoveStatus.Stun)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.stunChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus = MoveStatus.Stun;
                    player.healthEvent.CallGetStunEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animator.SetBool(Settings.isStunned, true);
                }
            }
        }
    }

    /// <summary>
    /// Check stun status - Enemy
    /// </summary>
    private void CheckStunStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

            if (projectileDetails.hasStunDamage && enemyMovementAI.moveStatus != MoveStatus.Stun && enemy.health.currentHealth > 0)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.stunChance)
                {
                    StartCoroutine(StunRoutine(enemy));
                }
            }
        }
        else
        {
            EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

            if (activeItemDetails.hasStunDamage && enemyMovementAI.moveStatus != MoveStatus.Stun && enemy.health.currentHealth > 0)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.stunChance)
                {
                    StartCoroutine(StunRoutine(enemy));
                }
            }
        }
    }

    /// <summary>
    /// Check frost status - Player
    /// </summary>
    private void CheckFrostStatus(Player player, bool isActiveItem = false)
    {
        if (player.isImmunetoFrost) return;

        if (!isActiveItem)
        {
            if (projectileDetails.hasFrostDamage && player.moveStatus != MoveStatus.Frozen)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.frostChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus = MoveStatus.Frozen;
                    player.healthEvent.CallGetFrostEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animatePlayer.ResetAnimatonParameters();
                    player.animator.SetBool(Settings.isFrozen, true);
                }
            }
        }
        else
        {
            if (activeItemDetails.hasStunDamage && player.moveStatus != MoveStatus.Frozen)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.frostChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.playerControl.isPlayerRolling = false;

                    player.moveStatus = MoveStatus.Stun;
                    player.healthEvent.CallGetFrostEvent();
                    player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                    player.animatePlayer.ResetAnimatonParameters();
                    player.animator.SetBool(Settings.isFrozen, true);
                }
            }
        }
    }

    /// <summary>
    /// Check frost status - Enemy
    /// </summary>
    private void CheckFrostStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

            if (projectileDetails.hasFrostDamage && enemyMovementAI.moveStatus != MoveStatus.Frozen && enemy.health.currentHealth > 0)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.frostChance)
                {
                    StartCoroutine(FrostRoutine(enemy));
                }
            }
        }
        else
        {
            EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

            if (activeItemDetails.hasFrostDamage && enemyMovementAI.moveStatus != MoveStatus.Frozen && enemy.health.currentHealth > 0)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.frostChance)
                {
                    StartCoroutine(FrostRoutine(enemy));
                }
            }
        }
    }

    /// <summary>
    /// Check blind status - Player
    /// </summary>
    private void CheckBlindStatus(Player player, bool isActiveItem = false)
    {
        if (player.isImmunetoBlind) return;

        if (!isActiveItem)
        {
            if (projectileDetails.hasBlindDamage)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.blindChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.healthEvent.CallGetBlindEvent();
                }
            }
        }
        else
        {
            if (activeItemDetails.hasBlindDamage)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.blindChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.healthEvent.CallGetBlindEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check blind status - Enemy
    /// </summary>
    private void CheckBlindStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.hasBlindDamage)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.blindChance + GameManager.Instance.GetPlayer().additionalBlindMakerModifier)
                {
                    enemy.healthEvent.CallGetBlindEvent();
                }
            }
        }
        else
        {
            if (activeItemDetails.hasBlindDamage)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.blindChance + GameManager.Instance.GetPlayer().additionalBlindMakerModifier)
                {
                    enemy.healthEvent.CallGetBlindEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check curse status - Player
    /// </summary>
    private void CheckCurseStatus(Player player, bool isActiveItem = false)
    {
        if (!isActiveItem)
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
        else
        {
            if (activeItemDetails.hasCurseDamage && player.isCursed)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.curseChance - player.additionalNegativeStatusEffectNegatorModifier)
                {
                    player.isCursed = true;
                    player.healthEvent.CallGetCurseEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check curse status - Enemy
    /// </summary>
    private void CheckCurseStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (!isActiveItem)
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
        else
        {
            if (activeItemDetails.hasCurseDamage && enemy.isCursed)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.curseChance)
                {
                    enemy.isCursed = true;
                    enemy.healthEvent.CallGetCurseEvent();
                }
            }
        }
    }

    IEnumerator StunRoutine(Enemy enemy)
    {
        enemy.enemyAI.moveStatus = MoveStatus.Stun;
        enemy.healthEvent.CallGetStunEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForFixedUpdate();
    }

    IEnumerator FrostRoutine(Enemy enemy)
    {
        enemy.enemyAI.moveStatus = MoveStatus.Frozen;
        enemy.healthEvent.CallGetFrostEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForFixedUpdate();
    }

    IEnumerator ExplosionRoutine(bool isEnemy = false)
    {
        Animator animator = GetComponent<Animator>();
        animator.SetTrigger("burst");

        if (isEnemy)
        {
            SoundEffectManager.Instance.PlaySoundEffect(projectileDetails.projectileImpactSoundEffect);
            Explosion(true);

            yield return new WaitForSeconds(0.5f);

            DisableProjectile();
        }
        else
        {
            SoundEffectManager.Instance.PlaySoundEffect(activeItemDetails.activeItemImpactSoundEffect);
            Explosion();

            if (activeItemDetails.activeItemType != ActiveItemType.Incendiary)
            {
                yield return new WaitForSeconds(0.5f);

                DisableProjectile();
            }
            else
            {
                BlastArea blastArea = transform.GetChild(1).GetComponentInChildren<BlastArea>();
                blastArea.SetBlastAreaTiling(); // Adjust the scale of the blast area
                blastArea.TriggerBurnAnimation(); // Trigger burn animation on separate object

                yield return new WaitForSeconds(5f);

                DisableProjectile();
            }
        }
    }

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Explosion(bool isEnemy = false)
    {
        StaticEventHandler.CallCameraShakeEvent(4, 0.6f);

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, blastRadius, layerMask))
        {
            if (collider.GetType() == typeof(PolygonCollider2D))
            {
                if (isEnemy)
                {
                    // Don't hit yourself if player is also in the collider list
                    if (collider.tag == Settings.enemyTag) continue;

                    if (collider.tag == Settings.playerTag)
                    {
                        Player player = collider.GetComponent<Player>();

                        int inflictedDamage = CalculateDamageAmount(null, true);
                        player.health.TakeDamage(inflictedDamage, transform.position, player.transform.position, false, collider, MeleeHand.None);

                        CheckAcidStatus(player);
                        CheckStunStatus(player);

                        // Apply knockback
                        player.movementByVelocity.TriggerKnockback((player.transform.position - transform.position).normalized);
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
                        enemy.health.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false, collider, MeleeHand.None);

                        CheckAcidStatus(enemy, true);
                        CheckStunStatus(enemy, true);

                        if (!enemy.enemyDetails.hasKnockbackResistance && enemy.GetComponent<Health>().currentHealth > 0)
                        {
                            enemy.enemyAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                        }
                    }
                }
            }
            else
            {
                if (isEnemy)
                {
                    collider.GetComponent<Health>().TakeDamage(Random.Range(projectileDetails.burstDamageMin, projectileDetails.burstDamageMax),
                        transform.position, collider.transform.position, false, collider, MeleeHand.None);
                }
                else
                {
                    collider.GetComponent<Health>().TakeDamage(Random.Range(activeItemDetails.burstDamageMin, activeItemDetails.burstDamageMax),
                        transform.position, collider.transform.position, false, collider, MeleeHand.None);
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
            inflictedDamage = (int)(damageDone * (1 - player.currentPhysicalResistanceValue));
        }
        else
        {
            // Damage produced by player
            damageDone = Random.Range(activeItemDetails.burstDamageMin, activeItemDetails.burstDamageMax);

            // Damage inflicted to enemy after deducting enemy armor
            health = enemy.GetComponent<Health>();

            inflictedDamage = (int)(damageDone * (1 - enemy.currentPhysicalResistance));
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

    public void ResetProjectileState()
    {
        isStopped = false;
        isColliding = false;
        isProjectileMaterialSet = false;
        directionInitialized = false;
        isHittingWall = false;
        lightningStroke = false;

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
        spriteRenderer.material = material;
    }

    public GameObject GetGameObject()
    {
        return gameObject;
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
