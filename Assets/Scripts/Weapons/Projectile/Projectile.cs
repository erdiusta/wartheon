using System.Collections;
using System;
using UnityEngine;
using Random = UnityEngine.Random;
using static UnityEngine.EventSystems.EventTrigger;

[DisallowMultipleComponent]
public class Projectile : MonoBehaviour, IFireable
{
    #region Tooltip
    [Tooltip("Populate with child TrailRenderer component")]
    #endregion Tooltip
    [SerializeField] TrailRenderer trailRenderer;
    [SerializeField] LayerMask layerMask;

    [HideInInspector] public Coroutine playerBlockCoroutine;

    float projectileRange = 0f;
    float projectileSpeed;
    Vector3 fireDirectionVector;
    Vector3 stoppedPosition;
    bool isStopped;
    float fireDirectionAngle;
    SpriteRenderer spriteRenderer;
    ProjectileDetailsSO projectileDetails;
    ActiveItemDetailsSO activeItemDetails;
    float projectileChargeTimer;
    bool isProjectileMaterialSet;
    bool overrideProjectileMovement;
    bool isColliding;
    Vector3 velocity;
    bool isProjectile = true;
    PolygonCollider2D polygonCollider2D;
    Rigidbody2D rb2d;
    bool headShotHappened;
    bool isPenetrationArrow;
    float countDown = 3f;
    float blastRadius = 5f;
    Coroutine explosionRoutine;
    Decoy decoy;
    int damageDone = 0;
    bool lightningStroke;
    bool isHittingWall; // Flag is for wall hit check for penetration arrow

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        rb2d = GetComponent<Rigidbody2D>();
        decoy = GetComponent<Decoy>();
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
            else if (activeItemDetails.activeItemType == ActiveItemType.Trap)
            {
                blastRadius = activeItemDetails.blastRadius;
            }
        }

        if (tag == "meteor")
        {
            spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        }
    }

    private void Start()
    {
        if (projectileDetails != null)
        {
            damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);
        }
    }

    private void Update()
    {
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

        // Don't move projectile if movement has been overriden - e.g. this projectile is part of an projectile pattern
        if (!overrideProjectileMovement)
        {
            // Disable after max range reached
            projectileRange -= velocity.magnitude * Time.deltaTime;

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
                        DisableProjectile();
                    }
                    else if (activeItemDetails != null && activeItemDetails.activeItemType != ActiveItemType.Bomb && activeItemDetails.activeItemType != ActiveItemType.Incendiary 
                        && activeItemDetails.activeItemType != ActiveItemType.Dummy)
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
        }
        else
        {
            if (isStopped)
            {
                transform.position = stoppedPosition;
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If already colliding with something return
            if (isColliding) return;

        if (activeItemDetails != null)
        {
            if (activeItemDetails.activeItemType == ActiveItemType.Bomb) return;
        }

        // Block process if shield equipped
        if (collision.tag == Settings.playerTag)
        {
            Player player = collision.GetComponent<Player>();

            int diceRoll = Random.Range(0, 100);
            bool deflectHappened = 100 - player.currentDeflectionValue * 100 < diceRoll ? true : false;

            if (deflectHappened)
            {
                player.health.isBlocking = true;
                player.healthEvent.CallDeflectionEvent();
                player.health.TakeDamage(0, transform.position, player.health.transform.position, false);
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

                    Vector2 pointerDirection = (cursorWorldPosition - new Vector2(player.transform.position.x, player.transform.position.y)).
                        normalized;

                    // Calculate the dot product between the shield's forward direction and the projectile direction
                    float dotProduct = Vector2.Dot(pointerDirection, enemyProjectileDirection);

                    float blockingThreshold = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.projectileDeflectRatio;

                    // Check if the dot product is greater than the threshold, deflection fails
                    if (dotProduct > blockingThreshold - 1f)
                    {
                        // Status checks
                        CheckPoisonStatus(player);
                        CheckAcidStatus(player);
                        CheckFrostStatus(player);
                        CheckStunStatus(player);
                        CheckCurseStatus(player);

                        // Deal Damage To Collision Object
                        DealDamage(collision);
                    }
                    else
                    {
                        // If the player is attacking, guard is down so block is disabled
                        if (player.meleeAttackRightHand.IsAttackingAtRightHand)
                        {
                            // Status checks
                            CheckPoisonStatus(player);
                            CheckAcidStatus(player);
                            CheckFrostStatus(player);
                            CheckStunStatus(player);
                            CheckCurseStatus(player);

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
                    CheckPoisonStatus(player);
                    CheckAcidStatus(player);
                    CheckFrostStatus(player);
                    CheckStunStatus(player);
                    CheckCurseStatus(player);

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
                    else if (activeItemDetails.activeItemType == ActiveItemType.Trap)
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
                        enemy.healthEvent.CallDeflectionEvent();
                        enemy.health.TakeDamage(0, transform.position, enemy.health.transform.position, false);
                    }
                    else
                    {
                        if (activeItemDetails == null)
                        {
                            // Status checks - PROJECTILE
                            CheckPoisonStatus(enemy);
                            CheckAcidStatus(enemy);
                            CheckFrostStatus(enemy);
                            CheckStunStatus(enemy);
                            CheckCurseStatus(enemy);
                        }
                        else
                        {
                            // Status checks - ACTIVE ITEM
                            CheckPoisonStatus(enemy, true);
                            CheckAcidStatus(enemy, true);
                            CheckFrostStatus(enemy, true);
                            CheckStunStatus(enemy, true);
                            CheckCurseStatus(enemy, true);
                        }

                        // Deal Damage To Collision Object
                        DealDamage(collision);
                    }
                }
                else
                {
                    if (activeItemDetails == null)
                    {
                        // Status checks - PROJECTILE
                        CheckPoisonStatus(enemy);
                        CheckAcidStatus(enemy);
                        CheckFrostStatus(enemy);
                        CheckStunStatus(enemy);
                        CheckCurseStatus(enemy);
                    }
                    else
                    {
                        // Status checks - ACTIVE ITEM
                        CheckPoisonStatus(enemy, true);
                        CheckAcidStatus(enemy, true);
                        CheckFrostStatus(enemy, true);
                        CheckStunStatus(enemy, true);
                        CheckCurseStatus(enemy, true);
                    }

                    // Deal Damage To Collision Object
                    DealDamage(collision);
                }
            }
            else
            {
                if (activeItemDetails == null)
                {
                    // Status checks - PROJECTILE
                    CheckPoisonStatus(enemy);
                    CheckAcidStatus(enemy);
                    CheckFrostStatus(enemy);
                    CheckStunStatus(enemy);
                    CheckCurseStatus(enemy);
                }
                else
                {
                    // Status checks - ACTIVE ITEM
                    CheckPoisonStatus(enemy, true);
                    CheckAcidStatus(enemy, true);
                    CheckFrostStatus(enemy, true);
                    CheckStunStatus(enemy, true);
                    CheckCurseStatus(enemy, true);
                }

                // Deal Damage To Collision Object
                DealDamage(collision);
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
        else if (collision.tag == "playerWeapon")
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
            DealDamage(collision);

            // Show ammo hit effect
            ProjectileHitEffect();

            DisableProjectile();
        }
    }

    IEnumerator PlayerBlockAnimRoutine(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        // Adjust animator layer weights
        player.transform.GetChild(1).GetComponent<Animator>().SetTrigger(Settings.block);
        SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponSwingSoundEffect);

        yield return new WaitForSeconds(0.6f);

        playerBlockCoroutine = null;
        player.animatePlayer.ResetAnimatonParameters();
        player.animator.SetBool(Settings.isIdle, true);
    }

    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        if (health != null)
        {
            // Set isColliding to prevent ammo dealing damage multiple times
            if (isPenetrationArrow)
            {
                StartCoroutine(ColliderTimeThreshold());
            }
            else
            {
                isColliding = true;
            }

            // Damage produced by player
            if (activeItemDetails == null)
            {
                damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);
            }
            else
            { 
                damageDone = Random.Range(activeItemDetails.projectileDamageMin, activeItemDetails.projectileDamageMax);
            }

            if (isPenetrationArrow)
            {
                float increasedDamage = damageDone * 1.25f;
                damageDone = (int)increasedDamage;
            }

            int inflictedDamage = 0;

            if (collision != null && collision.GetComponent<Enemy>() != null)
            {
                Enemy enemy = collision.GetComponent<Enemy>();

                if (projectileDetails != null)
                {
                    if (projectileDetails.isPlayerProjectile)
                    {
                        // LOWER DAMAGE IF PLAYER IS CURSED - PROJECTILE
                        damageDone = GameManager.Instance.GetPlayer().isCursed ? projectileDetails.projectileDamageMin :
                            Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);
                    }
                }
                else if (activeItemDetails != null)
                {
                    // LOWER DAMAGE IF PLAYER IS CURSED - ACTIVE ITEM
                    damageDone = GameManager.Instance.GetPlayer().isCursed ? activeItemDetails.projectileDamageMin :
                        Random.Range(activeItemDetails.projectileDamageMin, activeItemDetails.projectileDamageMax);
                }

                if (headShotHappened)
                {
                    // x3 damage if used headshot
                    damageDone *= 3;
                }

                // Segregate elemental and non-elemental damage
                int elementalDamage = (int)(projectileDetails.belongingWeaponDetails.elementalForgeRate * damageDone);
                int nonElementalDamage = damageDone - elementalDamage;

                int inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - enemy.currentPhysicalResistance));

                int inflictedElementalDamage = 0;

                // Calculate inflicted elemental damage
                switch (projectileDetails.belongingWeaponDetails.elementalBias)
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

                // Damage inflicted to enemy after deducting enemy armor
                inflictedDamage = inflictedElementalDamage + inflictedNonElementalDamage;
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

            health.TakeDamage(inflictedDamage, transform.position, health.transform.position, polygonCollider2D, headShotHappened);
        }
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
    public void InitializeProjectile(bool headShotHappened, ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle,
        float projectileSpeed, Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false, bool fallingFromSkies = false,
        bool isPenetrationArrow = false, int projectileCounter = 0, int projectilesPerShot = 0,CentaurPhase centaurPhase = CentaurPhase.None,
        TreantPhase treantPhase = TreantPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None)
    {
        #region Projectile

        this.projectileDetails = projectileDetails;

        // Set head shot bool
        this.headShotHappened = headShotHappened;

        // Set penetration arrow bool
        this.isPenetrationArrow = isPenetrationArrow;

        // Initialize isColliding
        isColliding = false;

        // Set fire direction
        SetFireDirection(projectileDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileCounter, projectilesPerShot, centaurPhase, treantPhase, galvanusPhase);

        // Set projectile sprite
        spriteRenderer.sprite = projectileDetails.projectileSprite;

        // Play sound if it is a unique projectile
        if (galvanusPhase == GalvanusPhase.Lightning)
        {
            SoundEffectManager.Instance.PlaySoundEffect(projectileDetails.projectileImpactSoundEffect);
            lightningStroke = true;
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
        GalvanusPhase galvanusPhase = GalvanusPhase.None)
    {
        float projectileSpreadModifier;

        if (GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentHeadPassiveItem()?.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
        {
            projectileSpreadModifier = 0.5f;
        }
        else
        {
            projectileSpreadModifier = 1f;
        }

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
        else if (treantPhase == TreantPhase.RazorLeaf)
        {
            // Define the total angle spread (e.g., 60 degrees spread)
            float totalSpreadAngle = 60f;

            // Calculate the total weight for the decreasing intervals
            float weightSum = 0f;
            for (int i = 0; i < totalProjectiles; i++)
            {
                weightSum += (float)Math.Pow(2, -i); // Exponential decrease
            }

            // Determine the incremental angle for each projectile
            float cumulativeAngle = 0f;
            for (int i = 0; i < totalProjectiles; i++)
            {
                float weight = (float)Math.Pow(2, -i) / weightSum; // Normalize weight
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
            float randomSpread = Random.Range(projectileDetails.projectileSpreadMin * projectileSpreadModifier, projectileDetails.projectileSpreadMax * projectileSpreadModifier);

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
        if (galvanusPhase == GalvanusPhase.Lightning)
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
                case ActiveItemType.Dummy:
                    return;
                case ActiveItemType.Generic:
                case ActiveItemType.Shiruken:
                case ActiveItemType.Trap:
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
    /// Check poison status - Player
    /// </summary>
    private void CheckPoisonStatus(Player player, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.isPoisonous)
            {
                // Check get poisoned
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.poisonChance)
                {
                    player.healthEvent.CallGetPoisonedEvent();
                    player.healthStatus = HealthStatus.Poisoned;
                }
            }
        }
        else
        {
            if (activeItemDetails.isPoisonous)
            {
                // Check get poisoned
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < activeItemDetails.poisonChance)
                {
                    player.healthEvent.CallGetPoisonedEvent();
                    player.healthStatus = HealthStatus.Poisoned;
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
                    enemy.healthStatus = HealthStatus.Poisoned;
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
                    enemy.healthStatus = HealthStatus.Poisoned;
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
                if (randomDice < projectileDetails.acidEfficiency)
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
                float randomDice = Random.Range(0f, 1f);
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
                if (randomDice < projectileDetails.stunChance)
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
                if (randomDice < activeItemDetails.stunChance)
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
        if (!isActiveItem)
        {
            if (projectileDetails.hasFrostDamage && player.moveStatus != MoveStatus.Frozen)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.frostChance)
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
                if (randomDice < activeItemDetails.frostChance)
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
    /// Check curse status - Player
    /// </summary>
    private void CheckCurseStatus(Player player, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            if (projectileDetails.hasCurseDamage && !player.isCursed)
            {
                float randomDice = Random.Range(0f, 1f);
                if (randomDice < projectileDetails.curseChance)
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
                if (randomDice < activeItemDetails.curseChance)
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

    IEnumerator ExplosionRoutine()
    {
        Animator animator = GetComponent<Animator>();
        animator.SetTrigger("burst");
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

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Explosion()
    {
        StaticEventHandler.CallCameraShakeEvent(4, 0.6f);

        foreach (Collider2D collider in Physics2D.OverlapCircleAll(transform.position, blastRadius, layerMask))
        {
            if (collider.GetType() == typeof(PolygonCollider2D))
            {
                // Don't hit yourself if player is also in the collider list
                if (collider.tag == Settings.playerTag) continue;

                if (collider.tag == Settings.enemyTag)
                {
                    Enemy enemy = collider.GetComponent<Enemy>();

                    int inflictedDamage = CalculateDamageAmount(enemy);
                    enemy.GetComponent<Health>().TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);

                    CheckAcidStatus(enemy, true);
                    CheckStunStatus(enemy, true);

                    if (!enemy.enemyDetails.hasKnockbackResistance && enemy.GetComponent<Health>().currentHealth > 0)
                    {
                        enemy.enemyAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                    }
                }
            }
            else
            {
                collider.GetComponent<Health>().TakeDamage(Random.Range(activeItemDetails.burstDamageMin, activeItemDetails.burstDamageMax),
                    transform.position, collider.transform.position, false);
            }
        }
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    private int CalculateDamageAmount(Enemy enemy)
    {
        // Damage produced by player
        int damageDone = Random.Range(activeItemDetails.burstDamageMin, activeItemDetails.burstDamageMax);

        // Damage inflicted to enemy after deducting enemy armor
        Health enemyHealth = enemy.GetComponent<Health>();

        int inflictedDamage = (int)(damageDone * (1 - enemy.currentPhysicalResistance));

        return inflictedDamage;
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
