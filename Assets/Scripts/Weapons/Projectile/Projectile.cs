using System.Collections;
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

    float projectileRange = 0f;
    float projectileSpeed;
    Vector3 fireDirectionVector;
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
    float countDown = 3f;
    float blastRadius = 5f;
    Coroutine explosionRoutine;
    Decoy decoy;

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
            if (activeItemDetails.activeItemType == ActiveItemType.Bomb)
            {
                countDown = activeItemDetails.countDown;
                blastRadius = activeItemDetails.blastRadius;
            }
            else if (activeItemDetails.activeItemType == ActiveItemType.Trap)
            {
                blastRadius = activeItemDetails.blastRadius;
            }
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
            if (activeItemDetails.activeItemType == ActiveItemType.Bomb)
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

        // Don't move projectile if movement has been overriden - e.g. this projectile is part of an ammo pattern
        if (!overrideProjectileMovement)
        {
            // Disable after max range reached
            projectileRange -= velocity.magnitude * Time.deltaTime;

            // Don't move projectile if movement has been overriden - e.g. this projectile is part of an ammo pattern
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
                    else if (activeItemDetails != null && activeItemDetails.activeItemType != ActiveItemType.Bomb && activeItemDetails.activeItemType != ActiveItemType.Dummy)
                    {
                        DisableProjectile();
                    }
                    else
                    {
                        velocity = Vector3.zero;
                    }
                }
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

            if (player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentOffHandWeapon().
                weaponDetails.weaponClass == WeaponClass.Shield)
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
                    CheckStunStatus(player);

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
                        CheckStunStatus(player);

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
                CheckStunStatus(player);

                // Deal Damage To Collision Object
                DealDamage(collision);
            }
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
                    float diceRoll = Random.Range(0f, 1f);
                    bool deflectHappened = diceRoll < enemy.enemyDetails.deflectChance ? true : false;

                    if (deflectHappened)
                    {
                        enemy.health.isBlocking = true;
                        enemy.health.TakeDamage(0, transform.position, enemy.health.transform.position, false);
                    }
                    else
                    {
                        if (activeItemDetails == null)
                        {
                            // Status checks - PROJECTILE
                            CheckPoisonStatus(enemy);
                            CheckAcidStatus(enemy);
                            CheckStunStatus(enemy);
                        }
                        else
                        {
                            // Status checks - ACTIVE ITEM
                            CheckPoisonStatus(enemy, true);
                            CheckAcidStatus(enemy, true);
                            CheckStunStatus(enemy, true);
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
                        CheckStunStatus(enemy);
                    }
                    else
                    {
                        // Status checks - ACTIVE ITEM
                        CheckPoisonStatus(enemy, true);
                        CheckAcidStatus(enemy, true);
                        CheckStunStatus(enemy, true);
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
                    CheckStunStatus(enemy);
                }
                else
                {
                    // Status checks - ACTIVE ITEM
                    CheckPoisonStatus(enemy, true);
                    CheckAcidStatus(enemy, true);
                    CheckStunStatus(enemy, true);
                }

                // Deal Damage To Collision Object
                DealDamage(collision);
            }
        }
        else if (collision.tag == Settings.decoyTag)
        {
            // Deal Damage To Collision Object
            DealDamage(collision);
        }
        else if (collision.tag == "playerWeapon")
        {
            return;
        }
        else // HIT WALL CHECK
        {
            // Deal Damage To Collision Object
            DealDamage(collision);

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
        }
   
        // Show ammo hit effect
        DoProjectileHitEffect();

        DisableProjectile();
    }

    IEnumerator PlayerBlockAnimRoutine(Collider2D collision)
    {
        Player player = collision.GetComponent<Player>();

        // Adjust animator layer weights
        player.animatePlayer.SetGetHitAnimationParameters();
        player.transform.GetChild(1).GetComponent<Animator>().SetTrigger(Settings.block);
        SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponSwingSoundEffect);

        yield return new WaitForSeconds(0.6f);

        playerBlockCoroutine = null;
        player.animatePlayer.SetIdleAnimationParameters();
    }

    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        if (health != null)
        {
            // Set isColliding to prevent ammo dealing damage multiple times
            isColliding = true;
            int damageDone = 0;

            // Damage produced by player
            if (activeItemDetails == null)
            {
                damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);
            }
            else
            {
                damageDone = Random.Range(activeItemDetails.projectileDamageMin, activeItemDetails.projectileDamageMax);
            }


            int inflictedDamage = 0;

            if (collision != null && collision.GetComponent<Enemy>() != null)
            {
                // Damage inflicted to enemy after deducting enemy armor
                inflictedDamage = damageDone > health.GetArmorValue() ? damageDone - health.GetArmorValue() : 1;

                if (headShotHappened)
                {
                    // x3 damage if used headshot
                    inflictedDamage *= 3;
                }
            }
            else if (collision != null && collision.GetComponent<Player>() != null)
            {
                // Damage inflicted to enemy after deducting enemy armor
                inflictedDamage = damageDone > health.GetArmorValue() ? damageDone - health.GetArmorValue() : 1;
            }
            else if (collision != null && collision.GetComponent<Environment>() != null)
            {
                // Damage inflicted equals damage done for environment objects
                inflictedDamage = damageDone;
            }

            health.TakeDamage(inflictedDamage, transform.position, health.transform.position, polygonCollider2D, headShotHappened);
        }
    }

    /// <summary>
    /// Initialize the projectile being fired - using the projectileDetails, the aimangle, weaponAngle, and weaponAimDirectionVector. If this 
    /// projectile is part of a pattern the projectile movement can be overriden by setting overrideAmmoMovement to true - PROJECTILE
    /// </summary>
    public void InitializeProjectile(bool headShotHappened, ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, 
        float projectileSpeed, Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false)
    {
        #region Projectile

        this.projectileDetails = projectileDetails;

        // Set head shot bool
        this.headShotHappened = headShotHappened;

        // Initialize isColliding
        isColliding = false;

        // Set fire direction
        SetFireDirection(projectileDetails, aimAngle, weaponAimAngle, weaponAimDirectionVector);

        // Set projectile sprite
        spriteRenderer.sprite = projectileDetails.projectileSprite;

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
        projectileRange = projectileDetails.projectileRange;

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
    private void SetFireDirection(ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        // Calculate random spread angle between min and max
        float randomSpread = Random.Range(projectileDetails.projectileSpreadMin, projectileDetails.projectileSpreadMax);

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

        gameObject.SetActive(false);
    }

    /// <summary>
    /// Display the ammo hit effect
    /// </summary>
    private void DoProjectileHitEffect()
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
                    if (player.armorStatus == ArmorStatus.SilverArmor || player.armorStatus == ArmorStatus.GoldenArmor)
                    {
                        player.healthEvent.CallArmorWoreOffEvent();
                    }

                    player.armorStatus = ArmorStatus.Acid;
                    player.health.SetArmorValue((int)(player.playerDetails.playerArmorValue * (1 - projectileDetails.acidEfficiency)));
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
                    if (player.armorStatus == ArmorStatus.SilverArmor || player.armorStatus == ArmorStatus.GoldenArmor)
                    {
                        player.healthEvent.CallArmorWoreOffEvent();
                    }

                    player.armorStatus = ArmorStatus.Acid;
                    player.health.SetArmorValue((int)(player.playerDetails.playerArmorValue * (1 - activeItemDetails.acidEfficiency)));
                    player.healthEvent.CallGetAcidEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check stun status - Enemy
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
                    enemy.health.SetArmorValue((int)(enemy.enemyDetails.enemyArmorValue * (1 - projectileDetails.acidEfficiency)));
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
                    enemy.health.SetArmorValue((int)(enemy.enemyDetails.enemyArmorValue * (1 - activeItemDetails.acidEfficiency)));
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
            EnemyMovementAI enemyMovementAI = enemy.GetComponent<EnemyMovementAI>();

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
            EnemyMovementAI enemyMovementAI = enemy.GetComponent<EnemyMovementAI>();

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


    IEnumerator StunRoutine(Enemy enemy)
    {
        enemy.enemyMovementAI.moveStatus = MoveStatus.Stun;
        enemy.healthEvent.CallGetStunEvent();
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
        yield return new WaitForSeconds(0.5f);

        DisableProjectile();
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
                        enemy.enemyMovementAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
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

        int inflictedDamage = damageDone > enemyHealth.GetArmorValue() ? damageDone - enemyHealth.GetArmorValue() : 1;
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
