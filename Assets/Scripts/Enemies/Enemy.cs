using Pathfinding;
using System.Collections;
using UnityEngine;
using UnityEngine.Rendering;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DealContactDamage))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
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
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(StatusManager))]
[RequireComponent(typeof(DamageDisplay))]
#endregion

[DisallowMultipleComponent]
public class Enemy : MonoBehaviour
{
    [HideInInspector] public EnemyDetailsSO enemyDetails;
    [HideInInspector] public AIDestinationSetter aiDestinationSetter;
    [HideInInspector] public PatrolRigidbody2D patrol;
    [HideInInspector] public AIRigidbody2D aiRigidbody2D;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    [HideInInspector] public FireWeapon fireWeapon;
    [HideInInspector] public EnemyAIEvent enemyAIEvent;
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
    [HideInInspector] public EnemyAI enemyAI;
    [HideInInspector] public DealContactDamage dealContactDamage;
    [HideInInspector] public Knockback knockback;
    [HideInInspector] public bool isFiring;
    [HideInInspector] public Health health;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public DropOnDestroy dropOnDestroy;
    [HideInInspector] public HealthStatus healthStatus = HealthStatus.Normal;
    [HideInInspector] public ArmorStatus armorStatus = ArmorStatus.Normal;
    [HideInInspector] public StatusManager statusManager;
    [HideInInspector] public DamageDisplay damageDisplay;
    [HideInInspector] public bool rightHandWeaponDamageHappened;
    [HideInInspector] public bool leftHandWeaponDamageHappened;
    [HideInInspector] public float currentArmor;
    [HideInInspector] public StatusEffectAnimators statusEffectAnimators;

    [HideInInspector] public bool isMaterializing;
    [HideInInspector] public float currentMoveSpeed;
    [HideInInspector] public float additionalSpeedModifier = 0f;
    [HideInInspector] public bool minionsSpawned = false;

    // STATUS EFFECTS
    [HideInInspector] public bool isCursed;
    [HideInInspector] public bool isFeared;
    [HideInInspector] public bool isRevealed;
    [HideInInspector] public bool isStatic;
    [HideInInspector] public bool isWarmed;
    [HideInInspector] public bool isChilled;
    [HideInInspector] public bool isBlind;
    [HideInInspector] public bool isSlowed;
    [HideInInspector] public bool isShattered;

    // STATUS EFFECT DURATION
    [HideInInspector] public float poisonDuration = 2;
    [HideInInspector] public float bleedDuration = 2;
    [HideInInspector] public float rootDuration = 2;
    [HideInInspector] public float stunDuration = 2;
    [HideInInspector] public float curseDuration = 4;
    [HideInInspector] public float fearDuration = 2;
    [HideInInspector] public float revealDuration = 2;
    [HideInInspector] public float staticDuration = 2;
    [HideInInspector] public float paralyzeDuration = 2;
    [HideInInspector] public float warmDuration = 2;
    [HideInInspector] public float burnDuration = 2;
    [HideInInspector] public float chillDuration = 2;
    [HideInInspector] public float freezeDuration = 2;
    [HideInInspector] public float blindDuration = 2;
    [HideInInspector] public float slowDuration = 2;

    // DISORIENTED
    [HideInInspector] public bool isDisoriented;
    [HideInInspector] public float disorientDuration = 0f;
    float disorientTimer;

    // STATUS EFFECT ANIMATORS
    public Animator rootAnimator;
    public Animator healAnimator;

    float blindTimer;
    SetActiveWeaponEvent setActiveWeaponEvent;
    MaterializeEffect materializeEffect;
    PolygonCollider2D polygonCollider2D;

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
        enemyAIEvent = GetComponent<EnemyAIEvent>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        enemyAI = GetComponent<EnemyAI>();
        dealContactDamage = GetComponent<DealContactDamage>();
        materializeEffect = GetComponent<MaterializeEffect>();
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
        aiDestinationSetter = GetComponent<AIDestinationSetter>();
        patrol = GetComponent<PatrolRigidbody2D>();
        aiRigidbody2D = GetComponent<AIRigidbody2D>();
        statusEffectAnimators = GetComponentInChildren<StatusEffectAnimators>();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthLost;
        healthEvent.GetSlow += HealthEvent_GetSlow;

        healthEvent.GetBlind += HealthEvent_GetBlind;
    }

    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthLost;
        healthEvent.GetSlow -= HealthEvent_GetSlow;

        healthEvent.GetBlind -= HealthEvent_GetBlind;
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

    private void HealthEvent_GetSlow(HealthEvent healthEvent)
    {
        StartCoroutine(SlowProcess());
    }

    IEnumerator SlowProcess()
    {
        currentMoveSpeed -= 2f;

        yield return new WaitForSeconds(4f);

        isSlowed = false;
        currentMoveSpeed += 2f;
        healthEvent.CallSlowCuredEvent();
    }

    private void HealthEvent_GetBlind(HealthEvent healthEvent)
    {
        isBlind = true;
        blindTimer = 8f;
    }

    private void Start()
    {
        currentArmor = enemyDetails.physicalResistance;
    }

    private void Update()
    {
        blindTimer -= Time.deltaTime;
        disorientTimer += Time.deltaTime;

        if (blindTimer <= 0 && isBlind)
        {
            isBlind = false;
            healthEvent.CallBlindCuredEvent();
        }

        if (disorientTimer >= disorientDuration && isDisoriented)
        {
            disorientTimer = 0;
            isDisoriented = false;
        }

        if (InputManager.TutorialEnabled)
        {
            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry || TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.DodgeRoll 
                || TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.SpecialSkill)
            {
                health.isDamageable = false;
            }
            else
            {
                health.isDamageable = true;
            }
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
        //SetEnemyMovementUpdateFrame(enemySpawnNumber);
        SetEnemyStartingHealth(dungeonLevel);
        SetEnemyStartingDamage(dungeonLevel);
        SetEnemyStartingWeapon();
        SetEnemyAnimationSpeed();
        StartCoroutine(MaterializeEnemy());
    }

    ///// <summary>
    ///// Set enemy movement update frame 
    ///// </summary>
    //private void SetEnemyMovementUpdateFrame(int enemySpawnNumber)
    //{
    //    // Set frame number that enemy should process it's updates
    //    if (enemyDetails.enemyCategory != EnemyCategory.None) // Skip this code for test purposes
    //    {
    //        enemyAI.SetUpdateFrameNumber(enemySpawnNumber % Settings.targetFrameRateToSpreadPathfindingOver);
    //    }
    //}

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
                health.SetMaximumHealth(enemyHealthDetails.enemyHealthAmount);
                return;
            }
        }

        health.SetMaximumHealth(Settings.defaultEnemyHealth);
    }

    /// <summary>
    /// Set the starting damage for the enemy
    /// </summary>
    private void SetEnemyStartingDamage(DungeonLevelSO dungeonLevel)
    {
        // Get the enemy health for the dungeon level
        foreach (EnemyDamageDetails enemyDamageDetails in enemyDetails.enemyDamageDetailsArray)
        {
            if (enemyDamageDetails.dungeonLevel == dungeonLevel)
            {
                dealContactDamage.contactDamageAmountMin = enemyDamageDetails.minDamageAmount;
                dealContactDamage.contactDamageAmountMax = enemyDamageDetails.maxDamageAmount;
                return;
            }
        }

        // Default damage values
        dealContactDamage.contactDamageAmountMin = 6;
        dealContactDamage.contactDamageAmountMax = 10;
    }

    /// <summary>
    /// Set enemy starting weapon as per the weapon details SO
    /// </summary>
    private void SetEnemyStartingWeapon(bool animatorRequired = false)
    {
        // Process if enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            Weapon weapon = new Weapon (enemyDetails.enemyWeapon.rarity)
            { weaponDetails = enemyDetails.enemyWeapon, weaponRemainingProjectile = enemyDetails.enemyWeapon.weaponProjectileCapacity };

            //Set weapon for enemy
            setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(weapon, 1, false, false);
        }
    }

    /// <summary>
    /// Set enemy animator speed to match movement speed
    /// </summary>
    private void SetEnemyAnimationSpeed()
    {
        // Set animator speed to match movement speed
        animator.speed = currentMoveSpeed / Settings.baseSpeedForEnemyAnimations;
    }

    IEnumerator MaterializeEnemy()
    {
        isMaterializing = true;

        // Disable collider, Movement AI and Weapon AI
        EnemyEnable(false);

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(enemyDetails.enemyMaterializeShader, enemyDetails.enemyMaterializeColor, 
            enemyDetails.enemyMaterializeTime, spriteRendererArray, enemyDetails.enemyStandardMaterial));

        // Enable collider, Movement AI and Weapon AI
        EnemyEnable(true);

        // Materializing completed
        isMaterializing = false;
    }

    private void EnemyEnable(bool isEnabled)
    {
        // Enable/Disable colliders
        polygonCollider2D.enabled = isEnabled;
        enemyAI.enabled = isEnabled;

        // Enable/Disable movement AI
        if (enemyAI != null && !enemyDetails.isEnemyBoss)
        {
            aiRigidbody2D.enabled = isEnabled;
        }

        // Enable / Disable Fire Weapon
        fireWeapon.enabled = isEnabled;
    }

    public Vector3 GetEnemyPosition(bool isBoss = false)
    {
        Vector3 rb2dPosition = new Vector3(rb2D.position.x, rb2D.position.y, 0f);

        return isBoss ? rb2dPosition + new Vector3(0f, 1.4f, 0f) : rb2dPosition + new Vector3(0f, 0.7f, 0f);   
    }
}
