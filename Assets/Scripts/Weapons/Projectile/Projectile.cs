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

    [HideInInspector] public Coroutine playerBlockCoroutine;
    [HideInInspector] public Coroutine enemyBlockCoroutine;

    float projectileRange = 0f;
    float projectileSpeed;
    Vector3 fireDirectionVector;
    float fireDirectionAngle;
    SpriteRenderer spriteRenderer;
    ProjectileDetailsSO projectileDetails;
    float projectileChargeTimer;
    bool isProjectileMaterialSet;
    bool overrideProjectileMovement;
    bool isColliding;

    private void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
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
            SetProjectileMaterial(projectileDetails.projectileMaterial);
            isProjectileMaterialSet = true;
        }

        // Don't move projectile if movement has been overriden - e.g. this projectile is part of an ammo pattern
        if (!overrideProjectileMovement)
        {
            // Calculate distance vector to move projectile
            Vector3 distanceVector = fireDirectionVector * projectileSpeed * Time.deltaTime;

            transform.position += distanceVector;

            // Disable after max range reached
            projectileRange -= distanceVector.magnitude;

            if (projectileRange < 0f)
            {
                DisableProjectile();
            }
        }
    }
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If already colliding with something return
        if (isColliding) 
            return;

        // Block process if shield equipped
        if (collision.tag == Settings.playerTag)
        {
            Player player = collision.GetComponent<Player>();

            if (player.activeWeapon.GetCurrentLeftHandWeapon() != null && player.activeWeapon.GetCurrentLeftHandWeapon().
                weaponDetails.weaponClass == WeaponClass.Shield)
            {
                // Get enemy projectile direction
                Vector2 enemyProjectileDirection = (player.transform.position - transform.position).normalized;

                // Get weapon pointer direction
                Vector2 cursorPosition = GameManager.Instance.pointerPosition.action.ReadValue<Vector2>();
                Vector2 cursorWorldPosition = Camera.main.ScreenToWorldPoint(cursorPosition);

                Vector2 pointerDirection = (cursorWorldPosition - new Vector2(player.transform.position.x, player.transform.position.y)).
                    normalized;

                // Calculate the dot product between the shield's forward direction and the projectile direction
                float dotProduct = Vector2.Dot(pointerDirection, enemyProjectileDirection);

                float blockingThreshold = player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.projectileDeflectRatio;

                // Check if the dot product is greater than the threshold, deflection fails
                if (dotProduct > blockingThreshold - 1f)
                {
                    // Deal Damage To Collision Object
                    DealDamage(collision);
                }
                else
                {
                    // If the player is attacking, guard is down so block is disabled
                    if (player.meleeAttackRightHand.IsAttackingAtRightHand)
                    {
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
                // Deal Damage To Collision Object
                DealDamage(collision);
            }
        }
        else
        {
            if (collision.GetComponent<Enemy>() != null)
            {
                Enemy enemy = collision.GetComponent<Enemy>();

                if (enemy.enemyDetails.hasShield)
                {
                    if (enemyBlockCoroutine == null)
                    {
                        float diceRoll = Random.Range(0f, 1f);
                        bool deflectHapped = diceRoll < enemy.enemyDetails.deflectChance ? true : false;

                        if (deflectHapped)
                            enemyBlockCoroutine = StartCoroutine(EnemyBlockAnimRoutine(collision));
                        else
                            // Deal Damage To Collision Object
                            DealDamage(collision);
                    }
                }
                else
                {
                    // Deal Damage To Collision Object
                    DealDamage(collision);
                }
            }
            else
            {
                // Deal Damage To Collision Object
                DealDamage(collision);
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
        SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponFiringSoundEffect);

        yield return new WaitForSeconds(0.6f);

        playerBlockCoroutine = null;
        player.animatePlayer.SetIdleAnimationParameters();
    }

    IEnumerator EnemyBlockAnimRoutine(Collider2D collision)
    {
        Enemy enemy = collision.GetComponent<Enemy>();

        // Adjust animator layer weights
        enemy.animateEnemy.SetGetHitAnimationParameters();
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.deflectSoundEffect);
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.block, true);
        yield return new WaitForSeconds(0.2f);

        enemyBlockCoroutine = null;
        enemy.animator.SetBool(Settings.block, false);
        enemy.animateEnemy.SetIdleAnimationParameters();
    }

    private void DealDamage(Collider2D collision)
    {
        Health health = collision.GetComponent<Health>();

        if (health != null)
        {
            // Set isColliding to prevent ammo dealing damage multiple times
            isColliding = true;

            // Damage produced by player
            int damageDone = Random.Range(projectileDetails.projectileDamageMin, projectileDetails.projectileDamageMax);

            int inflictedDamage = 0;

            if (collision != null && collision.GetComponent<Enemy>() != null)
            {
                // Damage inflicted to enemy after deducting enemy armor
                inflictedDamage = damageDone > health.GetArmorValue() ?
                    damageDone - health.GetArmorValue() : 1;
            }
            else if (collision != null && collision.GetComponent<Player>() != null)
            {
                // Damage inflicted to enemy after deducting enemy armor
                inflictedDamage = damageDone > health.GetArmorValue() ?
                    damageDone - health.GetArmorValue() : 1;
            }

            health.TakeDamage(inflictedDamage, transform.position, health.transform.position);
        }
    }

    /// <summary>
    /// Initialize the projectile being fired - using the projectileDetails, the aimangle, weaponAngle, and weaponAimDirectionVector. If this 
    /// projectile is part of a pattern the projectile movement can be overriden by setting overrideAmmoMovement to true
    /// </summary>
    public void InitializeProjectile(ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, float projectileSpeed, 
        Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false)
    {
        #region Projectile

        this.projectileDetails = projectileDetails;

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
    /// Set projectile fire direction and angle based on the input angle and direction adjusted by the
    /// random spread
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
    /// Disable the projectile - thus returning it to the object pool
    /// </summary>
    private void DisableProjectile()
    {
        gameObject.SetActive(false);
    }

    /// <summary>
    /// Display the ammo hit effect
    /// </summary>
    private void DoProjectileHitEffect()
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
