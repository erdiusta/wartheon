using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DealContactDamage))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
[RequireComponent(typeof(EnemyMovementAI))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(FireWeapon))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AnimateEnemy))]
[RequireComponent(typeof(MaterializeEffect))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(StatusManager))]
[RequireComponent(typeof(DamageDisplay))]
#endregion

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    [HideInInspector] public FireWeapon fireWeapon;
    [HideInInspector] public WeaponFiredEvent weaponFiredEvent;
    [HideInInspector] public DestroyedEvent destroyedEvent;
    [HideInInspector] public SpriteRenderer[] spriteRendererArray;
    [HideInInspector] public AimWeapon aimWeapon;
    [HideInInspector] public ActiveWeapon activeWeapon;
    [HideInInspector] public AnimateEnemy animateEnemy;
    [HideInInspector] public Idle idle;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D rb2D;
    [HideInInspector] public MovementToPosition movementToPosition;
    [HideInInspector] public EnemyMovementAI enemyMovementAI;
    [HideInInspector] public DealContactDamage dealContactDamage;
    [HideInInspector] public EnemyWeaponAI enemyWeaponAI;
    [HideInInspector] public Knockback knockback;
    [HideInInspector] public bool isDead;
    [HideInInspector] public bool isFiring;
    [HideInInspector] public Health health;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public DropOnDestroy dropOnDestroy;
    [HideInInspector] public HealthStatus healthStatus = HealthStatus.Normal;
    [HideInInspector] public ArmorStatus armorStatus = ArmorStatus.Normal;
    [HideInInspector] public bool isCursed;
    [HideInInspector] public StatusManager statusManager;
    [HideInInspector] public DamageDisplay damageDisplay;
    [HideInInspector] public bool rightHandWeaponDamageHappened;
    [HideInInspector] public bool leftHandWeaponDamageHappened;

    public ParticleSystem hitFxParticles;
    public ParticleSystem headShotFxParticles;

    SetActiveWeaponEvent setActiveWeaponEvent;
    MaterializeEffect materializeEffect;
    CircleCollider2D circleCollider2D;
    PolygonCollider2D polygonCollider2D;

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        enemyMovementAI = GetComponent<EnemyMovementAI>();
        dealContactDamage = GetComponent<DealContactDamage>();
        enemyWeaponAI = GetComponent<EnemyWeaponAI>();
        materializeEffect = GetComponent<MaterializeEffect>();
        circleCollider2D = GetComponent<CircleCollider2D>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        spriteRendererArray = GetComponentsInChildren<SpriteRenderer>();
        aimWeapon = GetComponent<AimWeapon>();
        animateEnemy = GetComponent<AnimateEnemy>();
        idle = GetComponent<Idle>();
        animator = GetComponent<Animator>();
        rb2D = GetComponent<Rigidbody2D>();
        movementToPosition = GetComponent<MovementToPosition>();
        knockback = GetComponent<Knockback>();
        dropOnDestroy = GetComponent<DropOnDestroy>();
        statusManager = GetComponent<StatusManager>();
        damageDisplay = GetComponent<DamageDisplay>();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthLost;
    }

    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthLost;
    }

    /// <summary>
    /// Handle health lost event
    /// </summary>
    private void HealthEvent_OnHealthLost(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if (healthEventArgs.healthAmount <= 0)
        {
            EnemyDestroyed();
        }
    }

    /// <summary>
    /// Enemy destroyed
    /// </summary>
    private void EnemyDestroyed()
    {
        DestroyedEvent destroyedEvent = GetComponent<DestroyedEvent>();
        destroyedEvent.CallDestroyedEvent(false);
    }

    /// <summary>
    /// Initialize the enemy
    /// </summary>
    public void EnemyInitialization(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel)
    {
        this.enemyDetails = enemyDetails;
        SetEnemyMovementUpdateFrame(enemySpawnNumber);
        SetEnemyStartingHealth(dungeonLevel);
        SetEnemyStartingWeapon();
        SetEnemyAnimationSpeed();
        StartCoroutine(MaterializeEnemy());
    }

    /// <summary>
    /// Set enemy movement update frame 
    /// </summary>
    private void SetEnemyMovementUpdateFrame(int enemySpawnNumber)
    {
        // Set frame number that enemy should process it's updates
        enemyMovementAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathfindingOver);
    }

    /// <summary>
    /// Set the starting health for the enemy
    /// </summary>
    private void SetEnemyStartingHealth(DungeonLevelSO dungeonLevel)
    {
        // Get the enemy health for the dungeon level
        foreach (EnemyHealthDetails enemyHealthDetails in enemyDetails.enemyHealthDetailsArray)
        {
            if (enemyHealthDetails.dungeonLevel == dungeonLevel)
            {
                health.SetStartingHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }

        health.SetStartingHealth(Settings.defaultEnemyHealth);
    }

    /// <summary>
    /// Set enemy starting weapon as per the weapon details SO
    /// </summary>
    private void SetEnemyStartingWeapon()
    {
        // Process if enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            Weapon weapon = new Weapon { weaponDetails = enemyDetails.enemyWeapon, weaponRemainingProjectile = enemyDetails.enemyWeapon.weaponProjectileCapacity };

            //Set weapon for enemy
            setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(weapon, 1);
        }
    }

    /// <summary>
    /// Set enemy animator speed to match movement speed
    /// </summary>
    private void SetEnemyAnimationSpeed()
    {
        // Set animator speed to match movement speed
        animator.speed = enemyMovementAI.moveSpeed / Settings.baseSpeedForEnemyAnimations;
    }

    IEnumerator MaterializeEnemy()
    {
        // Disable collider, Movement AI and Weapon AI
        EnemyEnable(false);

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(enemyDetails.enemyMaterializeShader, enemyDetails.enemyMaterializeColor, 
            enemyDetails.enemyMaterializeTime, spriteRendererArray, enemyDetails.enemyStandardMaterial));

        // Enable collider, Movement AI and Weapon AI
        EnemyEnable(true);
    }

    private void EnemyEnable(bool isEnabled)
    {
        // Enable/Disable colliders
        circleCollider2D.enabled = isEnabled;
        polygonCollider2D.enabled = isEnabled;

        // Enable/Disable movement AI
        enemyMovementAI.enabled = isEnabled;

        // Enable / Disable Fire Weapon
        fireWeapon.enabled = isEnabled;
    }
}
