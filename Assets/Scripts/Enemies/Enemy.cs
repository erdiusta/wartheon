using Mirror;
using Pathfinding;
using System;
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
public class Enemy : MonoBehaviour, IEnemyCombatData, IEnemyMovementData
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
    [HideInInspector] public EnemyAnimationSync enemyAnimSync;
    [HideInInspector] public Idle idle;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Rigidbody2D rb2D;
    [HideInInspector] public MovementToPosition movementToPosition;
    [HideInInspector] public EnemyAI enemyAI;
    [HideInInspector] public EnemyAINetwork enemyAINetwork;
    [HideInInspector] public EnemyNetwork enemyNetwork;
    [HideInInspector] public EnemyMovementNetwork enemyMovementNetwork;
    [HideInInspector] public DealContactDamage dealContactDamage;
    [HideInInspector] public Knockback knockback;
    [HideInInspector] public bool isFiring;
    [HideInInspector] public Health health;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public DropOnDestroy dropOnDestroy;
    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
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
    [HideInInspector] public float speedReducer = 0f;
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

    [SerializeField] bool isBoss;

    float blindTimer;
    SetActiveWeaponEvent setActiveWeaponEvent;
    MaterializeEffect materializeEffect;
    PolygonCollider2D polygonCollider2D;

    // MULTIPLAYER
    [HideInInspector] public bool initializationCompleted = false;
    [HideInInspector] public AimDirection LastAim { get; set; }

    [HideInInspector] public RoomNetData belongingRoomData;
    [HideInInspector] public Room belongingRoom;

    public event Action<Enemy> OnEnemyReady;
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
        enemyMovementNetwork = GetComponent<EnemyMovementNetwork>();
        aimWeapon = GetComponent<AimWeapon>();
        animateEnemy = GetComponent<AnimateEnemy>();
        enemyAnimSync = GetComponent<EnemyAnimationSync>();
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

        enemyAINetwork = GetComponent<EnemyAINetwork>();
        enemyNetwork = GetComponent<EnemyNetwork>();
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
        DestroyUtility.Destroy(gameObject, playerDied: false, health.LastDamageDealerNetId);
    }

    /// <summary>
    /// Initialize the enemy
    /// </summary>
    public void EnemyInitialization(EnemyDetailsSO enemyDetails, int enemySpawnNumber, DungeonLevelSO dungeonLevel, bool isMultiplayer)
    {
        this.enemyDetails = enemyDetails;

        currentArmor = enemyDetails.physicalResistance;

        //SetEnemyMovementUpdateFrame(enemySpawnNumber);
        SetEnemyStartingHealth(dungeonLevel);
        SetEnemyStartingDamage(dungeonLevel);
        SetEnemyStartingWeapon();

        // These setup will be handled by server for MP case
        if (!isMultiplayer)
        {
            SetEnemyAnimationSpeed();
            StartCoroutine(MaterializeEnemy());
        }

        health.OnEnemyInitialized(this);
        dropOnDestroy.InitializeDropList();

        EnemyRegistry.ActiveEnemies.Add(this);

        if (!isMultiplayer)
        {
            initializationCompleted = true;
        }
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
        yield return null; // Let Unity + Mirror settle

        isMaterializing = true;

        // Disable collider, Movement AI and Weapon AI
        EnemyEnable(false);

        yield return StartCoroutine(materializeEffect.MaterializeRoutine(enemyDetails.enemyMaterializeColor, enemyDetails.enemyMaterializeTime, spriteRendererArray));

        // Enable collider, Movement AI and Weapon AI
        EnemyEnable(true);

        // Materializing completed
        isMaterializing = false;
    }

    public void EnemyEnable(bool isEnabled)
    {
        // Enable/Disable colliders
        polygonCollider2D.enabled = isEnabled;
        fireWeapon.enabled = isEnabled;

        enemyAI.enabled = isEnabled;
        if (enemyAI != null && !isBoss) aiRigidbody2D.enabled = isEnabled;
    }

    public Vector3 GetEnemyPosition(bool isBoss = false)
    {
        Vector3 rb2dPosition = new Vector3(rb2D.position.x, rb2D.position.y, 0f);

        return isBoss ? rb2dPosition + new Vector3(0f, 1.4f, 0f) : rb2dPosition + new Vector3(0f, 0.7f, 0f);   
    }

    #region INTERFACES

    public bool Isboss => isBoss;
    public float ElementalForgeRate => enemyDetails.elementalForgeRate;
    public int ExperiencePoints => enemyDetails.experiencePoint;
    public EnemyBehaviour EnemyBehaviour => enemyDetails.enemyBehaviour;
    public EnemyCategory EnemyCategory => enemyDetails.enemyCategory;

    // COMBAT
    public float PhysicalResistance => enemyDetails.physicalResistance;
    public float MagicResistance => enemyDetails.magicResistance;
    public bool HasShield => enemyDetails.hasShield;
    public float DeflectChance => enemyDetails.deflectChance;
    public float DeflectionValue => enemyDetails.deflectionValue;

    public bool IsImmuneToBleeding => enemyDetails.isImmuneToBleeding;
    public bool IsImmuneToStun => enemyDetails.isImmuneToStun;
    public bool IsImmuneToSlow => enemyDetails.isImmuneToSlow;
    public bool IsImmuneToBurn => enemyDetails.isImmuneToBurn;
    public bool IsImmuneToPoison => enemyDetails.isImmuneToPoison;
    public bool IsImmuneToRoot => enemyDetails.isImmuneToRoot;
    public bool IsImmuneToFrost => enemyDetails.isImmuneToFrost;
    public bool IsImmuneToParalyze => enemyDetails.isImmuneToParalyze;
    public bool IsImmuneToBlind => enemyDetails.isImmuneToBlind;
    public bool IsImmuneToCurse => enemyDetails.isImmuneToCurse;
    public bool IsImmuneToFear => enemyDetails.isImmuneToFear;
    public int DealtMeleeDamageMin => enemyDetails.dealtMeleeDamageMin;
    public int DealtMeleeDamageMax => enemyDetails.dealtMeleeDamageMax;
    public bool CanWarm => enemyDetails.canWarm;
    public float WarmChance => enemyDetails.warmChance;
    public bool CanBurn => enemyDetails.canBurn;
    public float BurnChance => enemyDetails.burnChance;
    public bool HasBleedingDamage => enemyDetails.hasBleedingDamage;
    public float BleedingChance => enemyDetails.bleedingChance;
    public bool HasSlowDamage => enemyDetails.hasSlowDamage;
    public float SlowChance => enemyDetails.slowChance;
    public bool IsPoisonous => enemyDetails.isPoisonous;
    public float PoisonChance => enemyDetails.poisonChance;
    public bool HasAcid => enemyDetails.hasAcid;
    public float AcidEfficiency => enemyDetails.acidEfficiency;
    public bool HasStunDamage => enemyDetails.hasStunDamage;
    public float StunChance => enemyDetails.stunChance;
    public bool HasRootDamage => enemyDetails.hasRootDamage;
    public float RootChance => enemyDetails.rootChance;
    public bool HasChillDamage => enemyDetails.hasChillDamage;
    public float ChillChance => enemyDetails.chillChance;
    public bool HasFrostDamage => enemyDetails.hasFrostDamage;
    public float FrostChance => enemyDetails.frostChance;
    public bool HasStaticDamage => enemyDetails.hasStaticDamage;
    public float StaticChance => enemyDetails.staticChance;
    public bool HasParalyzeDamage => enemyDetails.hasParalyzeDamage;
    public float ParalyzeChance => enemyDetails.paralyzeChance;
    public bool HasCurseDamage => enemyDetails.hasCurseDamage;
    public float CurseChance => enemyDetails.curseChance;
    public bool HasFearDamage => enemyDetails.hasFearDamage;
    public float FearChance => enemyDetails.fearChance;
    public bool CanDrainHealth => enemyDetails.canDrainHealth;
    public float HealthDrainChance => enemyDetails.healthDrainChance;
    public bool HasBlindDamage => enemyDetails.hasBlindDamage;
    public float BlindChance => enemyDetails.blindChance;

    // Movement
    public float MaxBaseMoveSpeed => enemyDetails.movementDetails.GetBaseMaxMoveSpeed();
    #endregion
}
