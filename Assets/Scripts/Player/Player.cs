using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using System.Collections;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(ManaEvent))]
[RequireComponent(typeof(Mana))]
[RequireComponent(typeof(DealContactDamage))]
[RequireComponent(typeof(ReceiveContactDamage))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
[RequireComponent(typeof(PlayerControl))]
[RequireComponent(typeof(MovementByForce))]
[RequireComponent(typeof(MovementToPositionEvent))]
[RequireComponent(typeof(MovementToPosition))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(FireWeapon))]
[RequireComponent(typeof(MeleeAttackEvent))]
[RequireComponent(typeof(MeleeAttackMainHand))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(SetPassiveItemEvent))]
[RequireComponent(typeof(SelectedPassiveItem))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(AnimatePlayer))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Knockback))]
[RequireComponent(typeof(CoinsAndShards))]
[RequireComponent(typeof(StatusManager))]
[RequireComponent(typeof(SpecialMoveEvent))]
[RequireComponent(typeof(BranchMastery))]
[RequireComponent(typeof(WeaponMastery))]
#endregion
[DisallowMultipleComponent]
public class Player : MonoBehaviour
{
    public Transform forcefieldTransform;
    public Animator levelUpAnimator;
    public LevelUpDetailsSO levelUpDetails;

    [HideInInspector] public bool isInitialized = false;
    [HideInInspector] public PlayerDetailsSO playerDetails;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public Health health;
    [HideInInspector] public ManaEvent manaEvent;
    [HideInInspector] public Mana mana;
    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public HealthStatus healthStatus = HealthStatus.Normal;
    [HideInInspector] public ArmorStatus armorStatus = ArmorStatus.Normal;
    [HideInInspector] public DestroyedEvent destroyedEvent;
    [HideInInspector] public PlayerControl playerControl;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    [HideInInspector] public FireWeapon fireWeapon;
    [HideInInspector] public MeleeAttackEvent meleeAttackEvent;
    [HideInInspector] public MeleeAttackMainHand meleeAttackMainHand;
    [HideInInspector] public RangedAttackEvent rangedAttackEvent;
    [HideInInspector] public SetActiveWeaponEvent setActiveWeaponEvent;
    [HideInInspector] public SetPassiveItemEvent setPassiveItemEvent; 
    [HideInInspector] public AimWeapon aimWeapon;
    [HideInInspector] public ActiveWeapon activeWeapon;
    [HideInInspector] public SelectedPassiveItem selectedPassiveItem;
    [HideInInspector] public WeaponFiredEvent weaponFiredEvent;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public SortingGroup sortingGroup;
    [HideInInspector] public PolygonCollider2D polygonCollider2D;
    [HideInInspector] public Transform mainHandWeaponAnchorTransform;
    [HideInInspector] public Transform offHandWeaponAnchorTransform;
    [HideInInspector] public Rigidbody2D rb2D;
    [HideInInspector] public Animator animator;
    [HideInInspector] public AnimatePlayer animatePlayer;
    [HideInInspector] public Knockback knockback;
    [HideInInspector] public CoinsAndShards coinsAndShards;
    [HideInInspector] public Idle idle;
    [HideInInspector] public MovementByForce movementByForce;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public StatusManager statusManager;
    [HideInInspector] public DamageTracker damageTracker;
    [HideInInspector] public SpringJoint2D springJoint2D;
    [HideInInspector] public Projectile playersGrapple;
    [HideInInspector] public Projectile playerSkillProjectile;

    [HideInInspector] public int keyCount = 0;
    [HideInInspector] public int previousSetIndex = 1;
    [HideInInspector] public BranchMastery branchMastery;
    [HideInInspector] public WeaponMastery weaponMastery;
    [HideInInspector] public DropItem activeDropItem;
    [HideInInspector] public ActiveUniqueSkillDetailsSO[] playersAllActiveUniqueSkills;
    [HideInInspector] public Dictionary<int, ActiveUniqueSkillDetailsSO> currentlyUsedActiveUniqueSkills = new Dictionary<int, ActiveUniqueSkillDetailsSO>();

    [HideInInspector] public Dictionary<PassiveItemSlotName, PassiveItem> equippedPassiveItems = new();

    // PLAYER PRIMARY STATS
    public int CurrentStrengthValue { get => currentStrengthValue; set { currentStrengthValue = value; RecalculateSecondaryStats(); } }
    public int CurrentConstitutionValue { get => currentConstitutionValue; set { currentConstitutionValue = value; RecalculateSecondaryStats(); } }
    public int CurrentDexterityValue { get => currentDexterityValue; set { currentDexterityValue = value; RecalculateSecondaryStats(); } }
    public int CurrentIntelligenceValue { get => currentIntelligenceValue; set { currentIntelligenceValue = value; RecalculateSecondaryStats(); } }
    public int CurrentAgilityValue { get => currentAgilityValue; set { currentAgilityValue = value; RecalculateSecondaryStats(); } }
    public int CurrentWillpowerValue { get => currentWillpowerValue; set { currentWillpowerValue = value; RecalculateSecondaryStats(); } }
    public int CurrentFerocityValue { get => currentFerocityValue; set { currentFerocityValue = value; RecalculateSecondaryStats(); } }
    public int CurrentResolveValue { get => currentResolveValue; set { currentResolveValue = value; RecalculateSecondaryStats(); } }

    int currentStrengthValue;
    int currentConstitutionValue;
    int currentDexterityValue;
    int currentIntelligenceValue;
    int currentAgilityValue;
    int currentWillpowerValue;
    int currentFerocityValue;
    int currentResolveValue;

    // RESISTANCES
    [HideInInspector] public float currentArmorValue;
    [HideInInspector] public float currentMagicResistanceValue;

    // DAMAGE STATS
    [HideInInspector] public bool isPhysicalDamage;
    [HideInInspector] public int currentMainHandMinDamageValue;
    [HideInInspector] public int currentMainHandMaxDamageValue;
    [HideInInspector] public int currentOffHandMinDamageValue;
    [HideInInspector] public int currentOffHandMaxDamageValue;

    // CRITICAL HIT AND WEAPON HANDLING VALUES
    [HideInInspector] public float currentMainHandCriticalHitChance;
    [HideInInspector] public float currentOffHandCriticalHitChance;
    [HideInInspector] public float currentMainHandCriticalHitDamage;
    [HideInInspector] public float currentOffHandCriticalHitDamage;
    [HideInInspector] public float currentAttackRatingValue;

    // AUXILLARY MODIFIERS
    [HideInInspector] public float buffDurationModifier = 0f;

    // CURRENT BLOCK AND EVASIVENESS VALUES
    [HideInInspector] public float? currentBlockValue;
    [HideInInspector] public float currentEvasivenessValue;

    // CURRENT TOTAL EXPERIENCE - LEVEL STATS
    [HideInInspector] public int currentLevel = 1;
    [HideInInspector] public int currentGainedTotalExperiencePoints = 0;
    [HideInInspector] public int currentSkillPoints = 0;
    [HideInInspector] public int currentStatPoints = 0;

    // --- EXTRA MODIFIERS ----
    // ARMOR
    [HideInInspector] public float additionalArmorModifier = 0f;
    [HideInInspector] public float acidArmorDebuffModifier = 0f;
    [HideInInspector] public float additionalShieldArmorModifier = 0f;
    [HideInInspector] public float currentShieldArmor = 0f;
    [HideInInspector] public bool isShieldCalculated = false;

    // ATTACK COOLDOWN
    [HideInInspector] public float additionalAttackCoolDownModifier = 0f;

    // ATTACK RANGE
    [HideInInspector] public float additionalAttackRangeModifier = 0f;

    // ARMOR PENETRATION
    [HideInInspector] public float additionalArmorPenetrationModifier = 0f;

    // ARMOR RATING
    [HideInInspector] public float additionalAttackRatingModifier = 0f;

    // BLOCK
    [HideInInspector] public float additionalBlockModifier = 0f;

    // CC CHANCE
    [HideInInspector] public float additionalStatusEffectInflictModifier = 0f;

    // CRITICAL CHANCE
    [HideInInspector] public float additionalCriticalHitChanceModifier = 0f;

    // CRITICAL DAMAGE
    [HideInInspector] public float additionalCriticalDamageOnCloakedPrecision = 0.5f;
    [HideInInspector] public float additionalCriticalDamageModifier = 0f;
    [HideInInspector] public float additionalCriticalMeleeDamageModifier = 0f;
    [HideInInspector] public float additionalCriticalRangedDamageModifier = 0f;

    // CRITICAL RESISTANCE
    [HideInInspector] public float additionalCriticalResistanceModifier = 0f;

    // DAMAGE
    [HideInInspector] public float additionalPhysicalDamageModifer = 0f;
    [HideInInspector] public float additionalMagicDamageModifier = 0f;

    // DAMAGE REDUCTION
    [HideInInspector] public float additionalDamageReductionModifier = 0f;

    // DODGE
    [HideInInspector] public float additionalEvasivenessModifier = 0f;

    // LIFE STEAL
    [HideInInspector] public float additionalLifeStealModifier = 0f;

    // SKILL COOLDOWN
    [HideInInspector] public float additionalSkillCoolDownModifier = 0f;

    // SPEED
    [HideInInspector] public float additionalSpeedModifier = 0f;

    // INNER PATH BOOLS
    // Attack
    [HideInInspector] public bool isFocusedAggressionActive = false;
    [HideInInspector] public bool isPunishersWillActive = false;
    [HideInInspector] public bool isViciousMomentumActive = false;
    [HideInInspector] public bool viciousMomentumTriggered = false;
    [HideInInspector] public bool viciousMomentumAttackBonusObtained = false;
    [HideInInspector] public float viciousMomentumCooldownTimer;
    [HideInInspector] public float viciousMomentumDuration = 3f;
    [HideInInspector] public bool isCounterRiposteActive = false;
    [HideInInspector] public bool isTriadExecutionActive = false;
    [HideInInspector] public int triadExecutionCounter = 0;
    [HideInInspector] public bool triadExecutionTriggered = false;

    // Defense
    [HideInInspector] public bool isDieHardActive = false;
    [HideInInspector] public bool isBattleScarsActive = false;
    [HideInInspector] public bool battleScarsArmorBoostActivated;
    [HideInInspector] public bool isSecondBreathActive = false;
    [HideInInspector] public bool secondBreathReset;
    [HideInInspector] public bool isFortifiedResolveActive = false;
    [HideInInspector] public bool fortifiedResolveTriggered = false;

    // Utility
    [HideInInspector] public bool isShiftingStanceActive = false;
    [HideInInspector] public bool shiftingStanceOnProcess = false;
    [HideInInspector] public bool resourcefulActive = false;
    [HideInInspector] public bool isSurgeTapGainActive = false;
    [HideInInspector] public bool isCombatFocusActive = false;
    [HideInInspector] public bool combatFocusTriggered = false;
    [HideInInspector] public bool combatFocusCooldownBonusObtained = false;
    [HideInInspector] public float combatFocusCooldownTimer;
    [HideInInspector] public float combatFocusDuration = 3f;
    [HideInInspector] public bool isBattleReadyActive = false;

    // MISC
    [HideInInspector] public float expGainModifier = 1f;
    [HideInInspector] public float additionalBowAccuracyModifier = 0f;
    [HideInInspector] public float additionalManaReductionModifier = 0f;
    [HideInInspector] public bool threeSecInvincilibityAfterTeleportEnabled = false;
    [HideInInspector] public float additionalCastDurationModifier = 1f;
    [HideInInspector] public float additionalDropChanceModifier = 0f;

    [HideInInspector] public float additionalStatusResistanceModifier = 0f;
    [HideInInspector] public float additinalNPCCostModifier = 0;
    [HideInInspector] public bool thirtyPercentDamageAbsorbIsActive = false;
    [HideInInspector] public float blindModifier = 0f;
    [HideInInspector] public float additionalBlindMakerModifier = 0f;
    [HideInInspector] public bool hasRingOfFortune;

    // STATUS IMMUNITIES
    [HideInInspector] public bool isImmunetoBleeding;
    [HideInInspector] public bool isImmunetoStun;
    [HideInInspector] public bool isImmunetoSlow;
    [HideInInspector] public bool isImmunetoBurn;
    [HideInInspector] public bool isImmunetoPoison;
    [HideInInspector] public bool isImmunetoFrost;
    [HideInInspector] public bool isImmunetoParalyze;
    [HideInInspector] public bool isImmunetoBlind;
    [HideInInspector] public bool isImmunetoCurse;
    [HideInInspector] public bool isImmunetoFear;

    [HideInInspector] public ParticleSystem specialMoveParticlesSystem;
    [HideInInspector] public Weapon[][] weaponSlotSetArray = new Weapon[3][] { new Weapon[2] {null, null}, new Weapon[2] {null, null}, new Weapon[2] {null, null}};
    [HideInInspector] public int currentWeaponSlotSetIndex = 1;
    [HideInInspector] public List<GameObject> summonedEnemies = new List<GameObject>();

    [HideInInspector] public bool mainHandSlotFilled = false;
    [HideInInspector] public bool offHandSlotFilled = false;
    [HideInInspector] public short specialSkillNumber = 0;

    // SPECIAL SKILLS
    [HideInInspector] public SpecialMoveEvent specialMoveEvent;
    [HideInInspector] public bool[] specialMovesCooldownCheckArray = new bool[3]; // Check if related slot is on cooldown or not
    [HideInInspector] public float[] specialMoveCooldownTimerArray = new float[3]; // Related slot's cooldown timer
    [HideInInspector] public float[] specialMoveDurationTimerArray = new float[3]; // Related slot's duration timer
    [HideInInspector] public int[] specialMoveRecastCountArray = new int[3]; // Recast repeat count
    [HideInInspector] public float[] specialMoveRecastCooldownTimerArray = new float[3]; // Related slot's recast timer
    [HideInInspector] public float[] specialMoveRecastDurationTimerArray = new float[3]; // Related slot's recast timer

    // CAELION SKILLS
    [HideInInspector] public float lastDamageHappenedTime;
    [HideInInspector] public bool passiveTriggered;
    [HideInInspector] public bool isGraceOfTheUnscarredPassiveOn;
    [HideInInspector] public int seismicSlamDamage = 10;
    [HideInInspector] public float seismicSlamCircleRadius = 5f;
    [HideInInspector] public bool isValorActive;
    [HideInInspector] public bool isShieldBashing;
    [HideInInspector] public bool isBreakTheLineActive;
    [HideInInspector] public bool isGuardedOathActive;
    [HideInInspector] public float originalShieldArmorModifier = 0f;
    [HideInInspector] public float originalBlockModifier = 0f;

    // MORVEN SKILLS
    [HideInInspector] public bool isUmbralMistActive;
    [HideInInspector] public bool isStealthActive;
    [HideInInspector] public bool isShadowStepActive;

    // NYVERAN SKILLS
    [HideInInspector] public bool isPenetrateActive;
    [HideInInspector] public bool isTripleThreatActive;
    [HideInInspector] public bool isBindingArrowActive;
    [HideInInspector] public bool isArrowOfTheSevenActive;
    [HideInInspector] public bool isHuntersReachActive;

    // MYCARA SKILLS
    [HideInInspector] public bool isBlizzardActive;
    [HideInInspector] public bool isMycarasSealActive;
    [HideInInspector] public bool isIceBreakerActive;
    [HideInInspector] public bool isAbsoluteZeroActive;

    // KYNARA SKILLS
    [HideInInspector] public bool isFireBlastActive;
    [HideInInspector] public bool isMoltenRiftActive;
    [HideInInspector] public bool isFlameLotusActive;
    [HideInInspector] public bool isKynarasEmbraceActive;
    [HideInInspector] public int kynarasEmbraceDamage = 10;
    [HideInInspector] public float kynarasEmbraceCircleRadius = 1.5f;
    [HideInInspector] public bool isKynarasEmbraceShieldExploding;
    [HideInInspector] public bool isBlazingCycloneActive;
    [HideInInspector] public bool phoenixRisingUsed;

    // KARNAG SKILLS
    [HideInInspector] public bool isRageActive;
    [HideInInspector] public bool isShatterCryActive;
    [HideInInspector] public bool isAxeThrowActive;
    [HideInInspector] public bool isWhirlrendActive;
    [HideInInspector] public int whirlrendDamage = 10;
    [HideInInspector] public bool isFeastOfWarActive;

    // NYXA SKILLS
    [HideInInspector] public bool isNyxasReflexPassiveOn;
    [HideInInspector] public float lastDashTeleportHappenedTime;
    [HideInInspector] public bool isDontBlinkActive;
    [HideInInspector] public bool cancelledDueToInvalidTile;
    [HideInInspector] public bool isVenomousIvyActive;
    [HideInInspector] public bool isFadeAndFeedActive;
    [HideInInspector] public bool isBladeAndDashActive;
    [HideInInspector] public bool bladeAndDashOnRecast;
    [HideInInspector] public bool isShirukenActive;

    // NYMARA SKILLS
    [HideInInspector] public bool isMistOfDisruptionActive;
    [HideInInspector] public bool isInMistOfDisruption;
    [HideInInspector] public bool isNymarasWindveilActive;
    [HideInInspector] public float NymarasWindveilCircleRadius = 1.5f;
    [HideInInspector] public bool isChainLightningActive;
    [HideInInspector] public bool isEyeOfTheStormActive;
    [HideInInspector] public bool isIonicRejuvenationActive;
    [HideInInspector] public bool isConductiveTouchActive;
    [HideInInspector] public float nymaraSkillUsageTimer = 0f;


    [HideInInspector] public bool shadowCloakEquipped;

    // STATUS EFFECT DURATION
    [HideInInspector] public float poisonDuration = 2;
    [HideInInspector] public float bleedDuration = 2;
    [HideInInspector] public float rootDuration = 2;
    [HideInInspector] public float stunDuration = 2;
    [HideInInspector] public float curseDuration = 2;
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

    // STATUS EFFECT
    [HideInInspector] public bool isCursed;
    [HideInInspector] public bool isFeared;
    [HideInInspector] public bool isRevealed;
    [HideInInspector] public bool isStatic;
    [HideInInspector] public bool isWarmed;
    [HideInInspector] public bool isChilled;
    [HideInInspector] public bool isBlind;
    [HideInInspector] public bool isSlowed;

    float testAdditionArmor = 0.1f;

    // Track which instances currently contribute to the player
    Weapon _appliedMain;
    Weapon _appliedOff;

    public LayerMask layerMask;
    [HideInInspector] public ContactFilter2D filter = new ContactFilter2D();

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        manaEvent = GetComponent<ManaEvent>();
        mana = GetComponent<Mana>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        playerControl = GetComponent<PlayerControl>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rangedAttackEvent = GetComponentInChildren<RangedAttackEvent>();
        meleeAttackMainHand = GetComponent<MeleeAttackMainHand>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        setPassiveItemEvent = GetComponent<SetPassiveItemEvent>();
        aimWeapon = GetComponent<AimWeapon>();
        animator = GetComponent<Animator>();
        activeWeapon = GetComponent<ActiveWeapon>();
        selectedPassiveItem = GetComponent<SelectedPassiveItem>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        sortingGroup = GetComponent<SortingGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        animatePlayer = GetComponent<AnimatePlayer>();
        knockback = GetComponent<Knockback>();
        coinsAndShards = GetComponent<CoinsAndShards>();
        idle = GetComponent<Idle>();
        movementByForce = GetComponent<MovementByForce>();
        movementToPositionEvent = GetComponent<MovementToPositionEvent>();
        specialMoveEvent = GetComponent<SpecialMoveEvent>();
        branchMastery = GetComponent<BranchMastery>();
        weaponMastery = GetComponent<WeaponMastery>();
        damageTracker = GetComponent<DamageTracker>();
        mainHandWeaponAnchorTransform = transform.GetChild(0);
        offHandWeaponAnchorTransform = transform.GetChild(1);
        springJoint2D = GetComponent<SpringJoint2D>();
    }
    
    /// <summary>
    /// Initialize the player
    /// </summary>
    public void Initialize(PlayerDetailsSO playerDetails)
    {
        this.playerDetails = playerDetails;

        // Set player primary stats
        SetPlayerPrimaryStats(playerDetails);

        //Create player starting weapons
        CreatePlayerStartingWeapons();

        //Create player active item
        CreatePlayerStartingPassiveItem();

        // Set player starting secondary stats
        SetPlayerSecondaryStats();

        // Set player all active unique skills
        SetPlayerActiveUniqueSkills();

        healthEvent.CallHealthChangedEvent(health.currentHealth, 0, MeleeHand.None);
        manaEvent.CallManaChangedEvent(mana.currentMana);

        isInitialized = true;

        specialMoveRecastCountArray = new int[] { -1, -1, -1 };
    }

    private void OnEnable()
    {
        manaEvent.OnManaChanged += ManaEvent_OnManaChanged;
        healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
        healthEvent.GetSlow += HealthEvent_GetSlow;

        rangedAttackEvent.OnAnimationRangedAttackAnimationTriggered.AddListener(ResetAttackForRangedAttack);
    }

    private void OnDisable()
    {
        manaEvent.OnManaChanged -= ManaEvent_OnManaChanged;
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
        healthEvent.GetSlow -= HealthEvent_GetSlow;

        rangedAttackEvent.OnAnimationRangedAttackAnimationTriggered.RemoveListener(ResetAttackForRangedAttack);
    }

    private void Start()
    {
        // Add it to dynamicGameObjectsInScene
        SceneObjectsManager.Instance.dynamicGameObjectsInScene.Add(gameObject);
    }

    /// <summary>
    /// Handle health changed event
    /// </summary>
    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        // If player has died
        if (healthEventArgs.healthAmount <= 0f)
        {
            destroyedEvent.CallDestroyedEvent(true);
        }   
    }

    private void ManaEvent_OnManaChanged(ManaEvent manaEvent, ManaEventArgs manaEventArgs)
    {

    }

    private void HealthEvent_GetSlow(HealthEvent healthEvent)
    {
        StartCoroutine(SlowProcess());
    }

    IEnumerator SlowProcess()
    {
        additionalSpeedModifier -= 2f;
        UpdateSpeedValue();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        yield return new WaitForSeconds(4f);

        isSlowed = false;
        additionalSpeedModifier += 2f;
        UpdateSpeedValue();
        healthEvent.CallSlowCuredEvent();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    public void ResetAttackForRangedAttack()
    {
        meleeAttackMainHand.IsAttacking = false;
    }

    private void SetPlayerPrimaryStats(PlayerDetailsSO playerDetails)
    {
        currentStrengthValue = playerDetails.primaryStats.strength;
        currentConstitutionValue = playerDetails.primaryStats.constitution;
        currentDexterityValue = playerDetails.primaryStats.dexterity;
        currentIntelligenceValue = playerDetails.primaryStats.intelligence;
        currentAgilityValue = playerDetails.primaryStats.agility;
        currentWillpowerValue = playerDetails.primaryStats.willpower;
        currentResolveValue = playerDetails.primaryStats.resolve;
        currentFerocityValue = playerDetails.primaryStats.ferocity;
    }

    /// <summary>
    /// Set the player starting weapon
    /// </summary>
    private void CreatePlayerStartingWeapons()
    {
        if (InputManager.TutorialEnabled) return;

        for (int i = 0; i < playerDetails.startingWeaponList.Count; i++)
        {
            bool dualWieldOnStart = false;
            bool equipOffHand = false;

            // Special case: Caelion should equip shield (2nd item) to off-hand
            if (playerDetails.playerCharacterIndex == Character.Caelion && i == 1)
            {
                equipOffHand = true;
            }

            // Other dual wield characters equip off-hand if i == 1
            if (i == 1 && (playerDetails.playerCharacterIndex == Character.Morven || playerDetails.playerCharacterIndex == Character.Karnag || 
                playerDetails.playerCharacterIndex == Character.Nyxa))
            {
                dualWieldOnStart = true;
            }

            AddNextWeaponToPlayer(playerDetails.startingWeaponList[i], pickingUp: false, onStart: true, onlySwitch: false, dualWieldOnStart: dualWieldOnStart,
                equipOffHand: equipOffHand);
        }
    }

    /// <summary>
    /// Set the player starting passive item
    /// </summary>
    private void CreatePlayerStartingPassiveItem()
    {
        foreach (PassiveItemSlotName slot in Enum.GetValues(typeof(PassiveItemSlotName)))
        {
            if (slot == PassiveItemSlotName.None) continue;

            equippedPassiveItems[slot] = null;
        }
    }

    public void UpdateWieldedWeapons(Weapon weaponInstance, bool pickUp, bool onStart)
    {
        // Snapshot what's currently applied (before the equip)
        Weapon prevMain = activeWeapon.GetCurrentMainHandWeapon();
        Weapon prevOff = activeWeapon.GetCurrentOffHandWeapon();

        // Let your existing SO-based equip logic run
        UpdateWieldedWeapons(weaponInstance.weaponDetails, pickUp, onStart, weaponInstance);

        // Read the new equipped references
        Weapon newMain = activeWeapon.GetCurrentMainHandWeapon();
        Weapon newOff = activeWeapon.GetCurrentOffHandWeapon();

        //// Copy the rolls to whichever slot received this weapon (if your ActiveWeapon created a new object)
        //void CopyRolls(Weapon src, Weapon dst)
        //{
        //    if (src == null || dst == null) return;

        //    dst.baseUniqueRolled = src.baseUniqueRolled;
        //    dst.baseTypeRolled = src.baseTypeRolled;
        //    dst.enchantedBoostType = src.enchantedBoostType;
        //    dst.mythicBoostType = src.mythicBoostType;
        //}

        //// Detect slot changes and update modifiers symmetrically
        //if(!ReferenceEquals(prevMain, newMain))
        //{
        //    if (_appliedMain != null) { RemoveWeaponBoosts(_appliedMain); _appliedMain = null; }

        //    if(newMain != null)
        //    {
        //        // if this equip was meant for main hand, ensure its rolls are on the newMain object
        //        if (!ReferenceEquals(weaponInstance, newMain) && weaponInstance.weaponDetails == newMain.weaponDetails)
        //            CopyRolls(weaponInstance, newMain);

        //        ApplyWeaponBoosts(newMain);
        //        _appliedMain = newMain;
        //    }
        //}

        //if(!ReferenceEquals(prevOff, newOff))
        //{
        //    if(_appliedOff != null) { RemoveWeaponBoosts(_appliedOff); _appliedOff = null; }

        //    if(newOff != null)
        //    {
        //        if(!ReferenceEquals(weaponInstance, newOff) && weaponInstance.weaponDetails == newOff.weaponDetails)
        //            CopyRolls(weaponInstance, newOff);

        //        ApplyWeaponBoosts(newOff);
        //        _appliedOff = newOff;
        //    }
        //}
    }

    /// <summary>
    /// Update weapons list if a new one acquired
    /// </summary>
    public void UpdateWieldedWeapons(WeaponDetailsSO weaponDetails, bool pickingUp, bool onStart, Weapon weaponInstance)
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        if (mainHandWeapon == null)
        {
            AddNextWeaponToPlayer(weaponDetails, pickingUp, onStart, false, false, false, weaponInstance);

            // Set player starting health
            UpdatePlayerHealth(0, false, false);
            UpdatePlayerMana(0, false, false);
            UpdateArmorValues();
            UpdateDamageValues();
            UpdateAttackRatingAndCriticalValues();
            UpdateBlockAndEvasivenessValues();
            UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent();
        }
        // If inventory is full replace weapon
        else if (InventoryManager.Instance.IsInventoryFull())
        {
            AddNextWeaponToPlayer(weaponDetails, pickingUp, onStart, false, false, false, weaponInstance);

            // Set player starting health
            UpdatePlayerHealth(0, false, false);
            UpdatePlayerMana(0, false, false);
            UpdateArmorValues();
            UpdateDamageValues();
            UpdateAttackRatingAndCriticalValues();
            UpdateBlockAndEvasivenessValues();
            UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent();
        }
        // If weapon is one-handed off hand weapon
        else if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] == null && weaponDetails.wieldType == WieldType.OneHanded && weaponDetails.weaponClass != WeaponClass.Spear
            && weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] != null && weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0].weaponDetails.wieldType == WieldType.OneHanded &&
            weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0].weaponDetails.weaponClass != WeaponClass.Spear)
        {
            AddNextWeaponToPlayer(weaponDetails, pickingUp, onStart, false, false, false, weaponInstance);

            // Set player starting health
            UpdatePlayerHealth(0, false, false);
            UpdatePlayerMana(0, false, false);
            UpdateArmorValues();
            UpdateDamageValues();
            UpdateAttackRatingAndCriticalValues();
            UpdateBlockAndEvasivenessValues();
            UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent();
        }
        else
        {
            // Add it to inventory slot
            Weapon weapon = new Weapon(weaponDetails.rarity)
            {
                weaponDetails = weaponDetails,
                weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                itemSlotStatus = ItemSlotStatus.Inventory
            };

            int retrievedInventoryIndex = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(weapon);

            StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, retrievedInventoryIndex);
            StaticEventHandler.CallWeaponUnlockedEvent(weaponDetails.weaponTitle);
        }
    }

    /// <summary>
    /// Set the player primary stats
    /// </summary>
    private void SetPlayerSecondaryStats()
    {
        // Set player starting health
        UpdatePlayerHealth(0, true, true, true);
        UpdatePlayerMana(0, true, true, true);
        UpdateArmorValues(onStart: true);
        UpdateDamageValues();
        UpdateAttackRatingAndCriticalValues();
        UpdateBlockAndEvasivenessValues();
        UpdateSpeedValue();
    }

    /// <summary>
    /// Set the player's all active unique skills
    /// </summary>
    private void SetPlayerActiveUniqueSkills()
    {
        playersAllActiveUniqueSkills = new ActiveUniqueSkillDetailsSO[]{playerDetails.firstActiveSkillDetails, playerDetails.secondActiveSkillDetails,
            playerDetails.thirdActiveSkillDetails,playerDetails.fourthActiveSkillDetails, playerDetails.fifthActiveSkillDetails};

        StartCoroutine(PopulateSkillIconToGameHUD()); // Wait a bit for invked event after subscription
    }

    IEnumerator PopulateSkillIconToGameHUD()
    {
        yield return null;

        // Call event in order to update SkillUI in Gameplay HUD
        for (int i = 1; i <= 3; i++)
        {
            // Populate dragged skill 
            currentlyUsedActiveUniqueSkills.Add(i, playersAllActiveUniqueSkills[i - 1]);
            StaticEventHandler.CallActiveUniqueSkillPlacedEvent(i, currentlyUsedActiveUniqueSkills[i], false);
        }
    }

    public void UpdateArmorValues(bool onStart = false)
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        float weaponPassiveIncrease = 0f;

        if (mainHandWeapon != null) weaponPassiveIncrease = mainHandWeapon.armorIncrease;
        else weaponPassiveIncrease = 0;

        if (offHandWeapon != null) weaponPassiveIncrease += offHandWeapon.armorIncrease;
        else weaponPassiveIncrease += 0;


        if (mainHandWeapon != null && offHandWeapon != null && offHandWeapon.weaponDetails.weaponClass == WeaponClass.Shield)
        {
            if (!isShieldCalculated)
            {
                isShieldCalculated = true;
                currentShieldArmor = offHandWeapon.armorIncrease * (1 + additionalShieldArmorModifier);
                currentArmorValue += currentShieldArmor;
            }
        }
        else
        {
            if (!isShieldCalculated)
            {
                isShieldCalculated = true;
                currentArmorValue -= currentShieldArmor;
            }
        }

        currentArmorValue = (float)Math.Round(currentArmorValue + additionalArmorModifier + acidArmorDebuffModifier, 2);

        if (playerDetails.playerCharacterIndex == Character.Nyxa && onStart) currentArmorValue = (float)Math.Round(currentArmorValue + testAdditionArmor, 2);
    }

    public void UpdateDamageValues()
    {
        // MAIN-HAND
        (currentMainHandMinDamageValue, currentMainHandMaxDamageValue) = ComputeEquippedDamage(activeWeapon.GetCurrentMainHandWeapon(), isOffHand: false);

        // OFF-HAND
        (currentOffHandMinDamageValue, currentOffHandMaxDamageValue) = ComputeEquippedDamage(activeWeapon.GetCurrentOffHandWeapon(), isOffHand: true);
    }

    /// <summary>
    /// Computes final (min,max) damage for an equipped weapon using the weapon's
    /// dynamic rolled stats (physical/magic split), player attributes, and global modifiers.
    /// Off-hand damage is scaled by 0.6f. Shields always return (0,0).
    /// </summary>
    private (int min, int max) ComputeEquippedDamage(Weapon weapon, bool isOffHand)
    {
        if (weapon == null || weapon.weaponDetails == null) return (0, 0);

        // Shields don't deal damage
        if (weapon.weaponDetails.weaponClass == WeaponClass.Shield) return (0, 0);

        WeaponDetailsSO weaponDetails = weapon.weaponDetails;

        // Rolled base damage from the instance (NOT from ScriptableObject)
        int physMin = weaponDetails.physicalDamageMin + Mathf.Max(0, weapon.physicalAttackDamageIncrease);
        int physMax = weaponDetails.physicalDamageMax + Mathf.Max(0, weapon.physicalAttackDamageIncrease);
        int magMin = weaponDetails.magicDamageMin + Mathf.Max(0, weapon.magicAttackDamageIncrease);
        int magMax = weaponDetails.magicDamageMax + Mathf.Max(0, weapon.magicAttackDamageIncrease);

        // ----- SCALING -----
        // Physical scaling
        int physAdd = 0;
        if (physMin > 0 || physMax > 0)
        {
            if (weaponDetails.isMeleeWeapon)
            {
                // Dagger/Claw scale with DEX, other melee scale with STR
                if (weaponDetails.weaponClass == WeaponClass.Dagger) physAdd = Mathf.RoundToInt(currentDexterityValue * 0.7f);
                else physAdd = Mathf.RoundToInt(currentStrengthValue * 1.5f);
            }
            else
            {
                // Ranged physical (Bow/Crossbow) scale with DEX
                physAdd = Mathf.RoundToInt(currentDexterityValue * 0.7f);
            }
        }

        // Magic scaling
        int magAdd = 0;
        if (magMin > 0 || magMax > 0)
        {
            // Keep legacy feel: melee-magic scales a bit harder than ranged-magic
            float intCoef = weaponDetails.isMeleeWeapon ? 2f : 1.5f;
            magAdd = Mathf.RoundToInt(currentIntelligenceValue * intCoef);
        }

        // Combine, then apply global modifier(s)
        int totalMin = physMin + magMin + physAdd + magAdd;
        int totalMax = physMax + magMax + physAdd + magAdd;

        // Historical behavior applied the same modifier broadly; preserve that
        totalMin = Mathf.RoundToInt(totalMin * (1f + additionalPhysicalDamageModifer));
        totalMax = Mathf.RoundToInt(totalMax * (1f + additionalPhysicalDamageModifer));

        // Off-hand penalty
        if (isOffHand)
        {
            totalMin = Mathf.RoundToInt(totalMin * (1f + additionalPhysicalDamageModifer) * 0.6f);
            totalMax = Mathf.RoundToInt(totalMax * (1f + additionalPhysicalDamageModifer) * 0.6f);
        }

        return (totalMin, totalMax);
    }

    public void UpdateAttackRatingAndCriticalValues()
    {
        // Update attack rating value
        UpdateCurrentAttackRatingValues();

        // Update equipped weapon critical hit chances
        UpdateCurrentCriticalHitChance();

        // Update equipped weapon critical hit damage
        UpdateCurrentCriticalHitDamage();
    }

    public void UpdateBlockAndEvasivenessValues()
    {
        // Update block value
        UpdateBlockValue();

        // Update evasiveness value
        UpdateEvasivenessValue();
    }

    public void UpdateSpeedValue()
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        float weaponPassiveIncrease = 0f;

        if (mainHandWeapon != null) weaponPassiveIncrease = mainHandWeapon.speedIncreaseModifier;
        else weaponPassiveIncrease = 0f;

        if (offHandWeapon != null) weaponPassiveIncrease += offHandWeapon.speedIncreaseModifier;
        else weaponPassiveIncrease += 0f;

        movementByForce.moveSpeed = movementByForce.movementDetails.baseMaxMoveSpeed + currentAgilityValue * 0.25f + 
            additionalSpeedModifier + weaponPassiveIncrease;
    }

    public PassiveItem AddPassiveItemToPlayer(ref PassiveItem passiveItem, DropItem dropItem = null)
    {
        setPassiveItemEvent.CallEquipPassiveItem(passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName);

        if (equippedPassiveItems[passiveItem.passiveItemDetails.passiveItemSlotName].itemSlotStatus == ItemSlotStatus.Inventory)
        {
            // Place it inventory
            int inventoryItemIndex = InventoryManager.Instance.FindIndexOfItem(passiveItem);
            StaticEventHandler.CallPassiveItemAddedToInventorySlot(passiveItem, inventoryItemIndex);
        }
        else
        {
            // Equip to inventory
            StaticEventHandler.CallItemAddedToPassiveItemSlot(passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName);
        }

        return passiveItem;
    }

    // Public helpers for other systems (Drop/Swap/Set change)
    public void ApplyWeaponBoosts(Weapon w) => ModifyWeaponBoosts(w, +1);
    public void RemoveWeaponBoosts(Weapon w) => ModifyWeaponBoosts(w, -1);

    // Core modifier applier (sign = +1 apply, -1 remove)
    private void ModifyWeaponBoosts(Weapon w, int sign)
    {
        if (w == null) return;

        // Map each BoostType to your player fields — same numbers you use elsewhere
        void Add(ref float field, float amount) => field += sign * amount;

        foreach (var boost in w.GetAllBoosts())
        {
            switch (boost)
            {
                case BoostType.AttackCooldown: Add(ref additionalAttackCoolDownModifier, -0.05f); break;
                case BoostType.AttackDamage: Add(ref additionalPhysicalDamageModifer, +0.10f); break;
                case BoostType.MagicDamage: Add(ref additionalMagicDamageModifier, +0.10f); break;
                case BoostType.AttackRating: Add(ref additionalAttackRatingModifier, +0.10f); break;
                case BoostType.CritChance: Add(ref additionalCriticalHitChanceModifier, +0.05f); break;
                case BoostType.CritDamage: Add(ref additionalCriticalDamageModifier, +0.30f); break;
                case BoostType.LifeSteal: Add(ref additionalLifeStealModifier, +0.05f); break;
                case BoostType.BlockChance: Add(ref additionalBlockModifier, +0.08f); break;
                case BoostType.DodgeChance: Add(ref additionalEvasivenessModifier, +0.08f); break;
                case BoostType.ArmorIncrease: Add(ref additionalArmorModifier, +0.10f); break;
                case BoostType.MagicResistance: Add(ref currentMagicResistanceValue, +0.10f); break;
                case BoostType.MoveSpeed: Add(ref additionalSpeedModifier, +0.05f); break;
                case BoostType.DamageReduction: Add(ref additionalDamageReductionModifier, +0.05f); break; 
                case BoostType.ArmorPenetration: Add(ref additionalArmorPenetrationModifier, +0.10f); break;
                case BoostType.AttackRange: Add(ref additionalAttackRangeModifier, + 0.10f); break;

                // If you later wire these:
                case BoostType.HealthIncrease:     /* increase max HP via your recalculation */ break;
                case BoostType.ManaIncrease:       /* increase max mana via your recalculation */ break;
                case BoostType.StatusResistance: Add(ref additionalStatusResistanceModifier, +0.10f); break;
                case BoostType.AttackVsLowHealthEnemies:
                case BoostType.CritResistance:
                default: break;
            }
        }

        // If you have a derived-stats recompute, trigger it here.
        // RecalculateStats();
    }

    /// <summary>
    /// Add a weapon to the player weapon list
    /// </summary>
    public void AddNextWeaponToPlayer(WeaponDetailsSO weaponDetails, bool pickingUp, bool onStart, bool onlySwitch, bool dualWieldOnStart = false, 
        bool equipOffHand = false, Weapon weaponInstance = null) // <- pass the real instance for pickups/switches
    {
        // Guardrails: only create new instances on start.
        Weapon weapon = null;

        if (onStart)
        {
            // Starting gear: create a fresh instance using SO.rarity
            weapon = CreateStartingWeaponInstance(weaponDetails, equipOffHand ? ItemSlotStatus.OffHand : ItemSlotStatus.MainHand);
        }
        else
        {
            // Non-start flows MUST provide the real rolled instance to prevent duplication.
            if (weaponInstance == null)
            {
                Debug.LogError("AddNextWeaponToPlayer: Non-start flow requires a Weapon instance (weaponInstance != null) to avoid duplicates.");
                return;
            }

            weapon = weaponInstance;

            // Ensure slot is correct for equipOffHand requests
            if (equipOffHand) weapon.itemSlotStatus = ItemSlotStatus.OffHand;
            else if (weapon.itemSlotStatus == ItemSlotStatus.None) weapon.itemSlotStatus = ItemSlotStatus.MainHand; // sane default if not set yet
        }

        // From here on, ALWAYS work with the single 'weapon' reference
        WeaponDetailsSO details = weapon.weaponDetails;

        // Early validations
        if (equipOffHand && details.wieldType == WieldType.TwoHanded) return;

        // ---------- OFF-HAND EQUIP PATH ----------
        if (equipOffHand)
        {
            if (details.weaponClass == WeaponClass.Shield)
            {
                // Equip shield to off-hand (start or non-start)
                weapon.itemSlotStatus = ItemSlotStatus.OffHand;

                if (onStart)
                {
                    // stats were set when created; nothing else to do
                }

                weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] = weapon;
                ActivateWeapon(weapon, weapon.itemSlotStatus, currentWeaponSlotSetIndex, onStart, onlySwitch);
                StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                weapon.weaponBelongingToWhichOffHandSet = currentWeaponSlotSetIndex;

                if (weaponSlotSetArray[0][1] != null && weaponSlotSetArray[1][1] != null && weaponSlotSetArray[2][1] != null)
                    offHandSlotFilled = true;

                return;
            }
            else
            {
                // Non-shield off-hand (must be one-handed)
                weapon.itemSlotStatus = ItemSlotStatus.OffHand;

                weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] = weapon;
                ActivateWeapon(weapon, weapon.itemSlotStatus, currentWeaponSlotSetIndex, onStart, onlySwitch);
                StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                weapon.weaponBelongingToWhichOffHandSet = currentWeaponSlotSetIndex;

                if (weaponSlotSetArray[0][1] != null && weaponSlotSetArray[1][1] != null && weaponSlotSetArray[2][1] != null)
                    offHandSlotFilled = true;

                return;
            }
        }

        // ---------- AUTO FILL OFF-HAND ON START (shield or dual wield) ----------
        if (!offHandSlotFilled && (details.weaponClass == WeaponClass.Shield || dualWieldOnStart))
        {
            // Only allow off-hand if current main-hand in that set isn't two-handed
            for (int setIdx = pickingUp ? currentWeaponSlotSetIndex - 1 : 0; setIdx < 3; setIdx++)
            {
                bool tryThisSet = pickingUp ? setIdx == currentWeaponSlotSetIndex - 1 : weaponSlotSetArray[setIdx][1] == null;
                if (!tryThisSet) continue;

                var main = weaponSlotSetArray[setIdx][0];
                if (main != null && main.weaponDetails.wieldType == WieldType.TwoHanded) continue;

                // Place to off-hand
                weapon.itemSlotStatus = ItemSlotStatus.OffHand;
                weaponSlotSetArray[setIdx][1] = weapon;
                weapon.weaponBelongingToWhichOffHandSet = setIdx + 1;

                if (currentWeaponSlotSetIndex - 1 == setIdx)
                    ActivateWeapon(weapon, weapon.itemSlotStatus, setIdx + 1, onStart, onlySwitch);

                if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);

                // Mark filled if all sets have off-hand
                if (weaponSlotSetArray[0][1] != null && weaponSlotSetArray[1][1] != null && weaponSlotSetArray[2][1] != null)
                    offHandSlotFilled = true;

                return;
            }
        }

        // ---------- MAIN-HAND EQUIP PATH ----------
        if (!mainHandSlotFilled && details.weaponClass != WeaponClass.Shield)
        {
            // Fill the current set first when picking up; else fill first available at start
            if (pickingUp)
            {
                if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] == null)
                {
                    weapon.itemSlotStatus = ItemSlotStatus.MainHand;
                    weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] = weapon;
                    weapon.weaponBelongingToWhichMainHandSet = currentWeaponSlotSetIndex;

                    ActivateWeapon(weapon, weapon.itemSlotStatus, currentWeaponSlotSetIndex, onStart, onlySwitch);
                    StaticEventHandler.CallWeaponUnlockedEvent(details.weaponTitle);
                    if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);

                    if (weaponSlotSetArray[0][0] != null && weaponSlotSetArray[1][0] != null && weaponSlotSetArray[2][0] != null)
                        mainHandSlotFilled = true;

                    return;
                }
            }
            else
            {
                // On start, fill first empty main-hand slot from set 1..3
                for (int setIdx = 0; setIdx < 3; setIdx++)
                {
                    if (weaponSlotSetArray[setIdx][0] != null) continue;

                    weapon.itemSlotStatus = ItemSlotStatus.MainHand;
                    weaponSlotSetArray[setIdx][0] = weapon;
                    weapon.weaponBelongingToWhichMainHandSet = setIdx + 1;

                    if (currentWeaponSlotSetIndex - 1 == setIdx)
                        ActivateWeapon(weapon, weapon.itemSlotStatus, setIdx + 1, onStart, onlySwitch);

                    if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);

                    if (weaponSlotSetArray[0][0] != null && weaponSlotSetArray[1][0] != null && weaponSlotSetArray[2][0] != null)
                        mainHandSlotFilled = true;

                    return;
                }
            }
        }
        else
        {
            // All main-hand filled; try off-hand (non-two-handed & non-shield already handled above)
            if (!offHandSlotFilled && details.wieldType != WieldType.TwoHanded && details.weaponClass != WeaponClass.Shield)
            {
                for (int setIdx = 0; setIdx < 3; setIdx++)
                {
                    if (weaponSlotSetArray[setIdx][1] != null) continue;

                    weapon.itemSlotStatus = ItemSlotStatus.OffHand;
                    weaponSlotSetArray[setIdx][1] = weapon;
                    weapon.weaponBelongingToWhichOffHandSet = setIdx + 1;

                    if (currentWeaponSlotSetIndex - 1 == setIdx)
                        ActivateWeapon(weapon, weapon.itemSlotStatus, setIdx + 1, onStart, onlySwitch);

                    if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);

                    if (weaponSlotSetArray[0][1] != null && weaponSlotSetArray[1][1] != null && weaponSlotSetArray[2][1] != null)
                        offHandSlotFilled = true;

                    return;
                }

                offHandSlotFilled = true;
            }
        }
    }

    private Weapon CreateStartingWeaponInstance(WeaponDetailsSO details, ItemSlotStatus slot)
    {
        // Shields: keep the instance minimal; other runtime stats are irrelevant
        if (details.weaponClass == WeaponClass.Shield)
        {
            return new Weapon(details.rarity)
            {
                weaponDetails = details,
                itemSlotStatus = slot
            };
        }

        // Create with so rarity only for start gear
        Weapon weapon = new Weapon(details.rarity)
        {
            weaponDetails = details, 
            weaponRemainingProjectile = details.weaponProjectileCapacity, 
            itemSlotStatus = slot,

            attackCooldown = details.weaponCooldownDuration,
            attackRatingIncrease = details.weaponAttackRating,
            criticalHitChanceIncrease = details.criticalHitChance,
            criticalHitDamageIncrease = details.criticalHitDamageMultiplier
        };

        // Split base damage into physical/magic by forge rate, rounded safely
        int physMin = details.isMeleeWeapon ? details.physicalDamageMin : details.weaponCurrentProjectile.projectilePhyDamageMin;
        int physMax = details.isMeleeWeapon ? details.physicalDamageMax : details.weaponCurrentProjectile.projectilePhyDamageMax;
        int magicMin = details.isMeleeWeapon ? details.magicDamageMin : details.weaponCurrentProjectile.projectileMagicDamageMin;
        int magicMax = details.isMeleeWeapon ? details.magicDamageMax : details.weaponCurrentProjectile.projectileMagicDamageMax;

        return weapon;
    }

    public void ActivateWeapon(Weapon weapon, ItemSlotStatus itemSlotStatus, int setIndex, bool onStart, bool onSwitch)
    {
        if (itemSlotStatus == ItemSlotStatus.MainHand)
        {
            // Set the added weapon as active - main hand
            setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(weapon, setIndex, onStart, onSwitch);
            // MAIN HAND WEAPON ACTIVATED

            // This section is for enabling/disabling lock icon based on weapon's one-hand or two-hand wield
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.OneHanded)
            {
                if (activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.wieldType == WieldType.OneHanded)
                {
                    setActiveWeaponEvent.CallOneHandWeaponEquipEvent(true);
                }
                else
                {
                    setActiveWeaponEvent.CallOneHandWeaponEquipEvent();
                }
            }
            else if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
            {
                setActiveWeaponEvent.CallTwoHandWeaponEquipEvent();
            }
        }
        else
        {
            // Set the added weapon as active - main hand
            setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(weapon, setIndex, onStart, onSwitch);
        }
    }

    public void UpdateCurrentCriticalHitChance()
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        if (mainHandWeapon != null)
        {
            if (mainHandWeapon.weaponDetails.isMeleeWeapon)
            {
                currentMainHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100, 2) +
                    mainHandWeapon.weaponDetails.criticalHitChance + additionalCriticalHitChanceModifier + mainHandWeapon.criticalHitChanceIncrease;
            }
            else
            {
                currentMainHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100, 2) +
                    mainHandWeapon.weaponDetails.weaponCurrentProjectile.criticalHitChance + additionalCriticalHitChanceModifier + mainHandWeapon.criticalHitChanceIncrease;
            }
        }
        else
        {
            currentMainHandCriticalHitChance = 0;
        }

        if (offHandWeapon != null)
        {
            if (offHandWeapon.weaponDetails.isMeleeWeapon)
            {
                currentOffHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100 , 2) +
                    offHandWeapon.weaponDetails.criticalHitChance + additionalCriticalHitChanceModifier + offHandWeapon.criticalHitChanceIncrease;
            }
            else if (offHandWeapon.weaponDetails.isShield)
            {
                currentOffHandCriticalHitChance = 0;
            }
            else
            {
                currentOffHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100, 2) +
                    offHandWeapon.weaponDetails.weaponCurrentProjectile.criticalHitChance + additionalCriticalHitChanceModifier + offHandWeapon.criticalHitChanceIncrease;
            }
        }
        else
        {
            currentOffHandCriticalHitChance = 0;
        }

        currentMainHandCriticalHitChance = Mathf.Clamp(currentMainHandCriticalHitChance, 0f, 0.5f);
        currentOffHandCriticalHitChance = Mathf.Clamp(currentOffHandCriticalHitChance, 0f, 0.5f);

        // SHADOW CLOAK
        if (shadowCloakEquipped)
        {
            if (mainHandWeapon != null && offHandWeapon != null)
            {
                if (equippedPassiveItems.TryGetValue(PassiveItemSlotName.Back, out PassiveItem item) && item != null)
                {
                    if (item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                    {
                        if ((mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Dagger && offHandWeapon.weaponDetails.weaponClass
                            == WeaponClass.Dagger) || (mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw &&
                            offHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw)) // Grant extra cr. chance if dual-wield
                        {
                            currentMainHandCriticalHitChance += 0.05f;
                            currentOffHandCriticalHitChance += 0.05f;
                        }
                        else // If cloak equipped but then dual set is broken, then cr.chance is reduced a bit
                        {
                            currentMainHandCriticalHitChance -= 0.05f;
                            currentOffHandCriticalHitChance -= 0.05f;
                        }
                    }
                }

            }
        }
    }

    private void UpdateCurrentCriticalHitDamage()
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        if (mainHandWeapon != null)
        {
            if (mainHandWeapon.weaponDetails.isMeleeWeapon)
            {
                currentMainHandCriticalHitDamage = mainHandWeapon.weaponDetails.criticalHitDamageMultiplier +
                     currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalMeleeDamageModifier + mainHandWeapon.criticalHitDamageIncrease;
            }
            else
            {
                currentMainHandCriticalHitDamage = mainHandWeapon.weaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier +
                    currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalRangedDamageModifier + mainHandWeapon.criticalHitDamageIncrease;
            }
        }
        else
        {
            currentMainHandCriticalHitDamage = 0;
        }

        if (offHandWeapon != null)
        {
            if (offHandWeapon.weaponDetails.isMeleeWeapon)
            {
                currentOffHandCriticalHitDamage = offHandWeapon.weaponDetails.criticalHitDamageMultiplier +
                    currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalMeleeDamageModifier + offHandWeapon.criticalHitDamageIncrease;
            }
            else if (offHandWeapon.weaponDetails.isShield)
            {
                currentOffHandCriticalHitDamage = 0;
            }
            else
            {
                currentOffHandCriticalHitDamage = offHandWeapon.weaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier +
                    currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalRangedDamageModifier + offHandWeapon.criticalHitDamageIncrease;
            }
        }
        else
        {
            currentOffHandCriticalHitDamage = 0;
        }

        currentMainHandCriticalHitDamage = (float)Math.Round(currentMainHandCriticalHitDamage, 2);
        currentOffHandCriticalHitDamage = (float)Math.Round(currentOffHandCriticalHitDamage, 2);
    }

    public void UpdateCurrentAttackRatingValues()
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        float weaponHandlingModifier = 0f;

        if (offHandWeapon != null)
        {
            if (mainHandWeapon != null)
            {
                weaponHandlingModifier = ((mainHandWeapon.weaponDetails.weaponAttackRating + offHandWeapon.weaponDetails.weaponAttackRating + 
                    mainHandWeapon.attackRatingIncrease + offHandWeapon.attackRatingIncrease) / 2
                   + additionalAttackRatingModifier) * 0.7f;
            }
            else
            {
                weaponHandlingModifier = 0;
            }
        }
        else
        {
            if (mainHandWeapon != null)
            {
                weaponHandlingModifier = mainHandWeapon.weaponDetails.weaponAttackRating + additionalAttackRatingModifier + mainHandWeapon.attackRatingIncrease;
            }
            else
            {
                weaponHandlingModifier = 0;
            }
        }

        currentAttackRatingValue =  0.2f + ((float)currentDexterityValue / 3 + currentFerocityValue / 3 + currentStrengthValue / 4) / 100 + weaponHandlingModifier - blindModifier;
        currentAttackRatingValue = (float)Math.Round(currentAttackRatingValue, 2);
    }

    public void UpdateBlockValue()
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        float blockModifier = 0;

        if (mainHandWeapon != null) blockModifier = mainHandWeapon.blockChanceIncrease;
        else blockModifier = 0;

        if (offHandWeapon != null) blockModifier += offHandWeapon.blockChanceIncrease;
        else blockModifier += 0;

        float? blockValueIfHas = mainHandWeapon?.weaponDetails.weaponClass == WeaponClass.Shield ? offHandWeapon?.weaponDetails.blockChance + 
            additionalBlockModifier : 0f;

        currentBlockValue = Mathf.Min(0.5f, (float)Math.Round((double)(blockValueIfHas + blockModifier), 2)); //Clamp max block value
    }

    public void UpdateEvasivenessValue()
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        float dodgeModifier = 0;

        if (mainHandWeapon != null) dodgeModifier = mainHandWeapon.dodgeChanceIncrease;
        else dodgeModifier = 0;

        if (offHandWeapon != null) dodgeModifier += offHandWeapon.dodgeChanceIncrease;
        else dodgeModifier += 0;

        currentEvasivenessValue = Mathf.Min(0.5f, (float)Math.Round((double)(currentAgilityValue * 0.5f / 100 + 
            additionalEvasivenessModifier + dodgeModifier), 2));
    }

    /// <summary>
    /// Set player health
    /// </summary>
    public void UpdatePlayerHealth(int healthIncrease, bool shouldHealthFilled, bool isMaxHealthChanged, bool onStart = false)
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        int weaponPassiveIncrease = 0;

        if (mainHandWeapon != null) weaponPassiveIncrease = mainHandWeapon.increasedMaxHealth;
        else weaponPassiveIncrease = 0;

        if (offHandWeapon != null) weaponPassiveIncrease += offHandWeapon.increasedMaxHealth;
        else weaponPassiveIncrease += 0;

        int maxHealthValue = 120 + currentConstitutionValue * 25 + weaponPassiveIncrease;

        if (isMaxHealthChanged)
        {
            health.SetMaximumHealth(maxHealthValue, shouldHealthFilled, onStart);
        }

        health.AddHealth(healthIncrease);
    }

    /// <summary>
    /// Set player mana
    /// </summary>
    public void UpdatePlayerMana(int manaIncrease, bool shouldManaFilled, bool isMaxManaChanged, bool onStart = false)
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        int weaponPassiveIncrease = 0;

        if (mainHandWeapon != null) weaponPassiveIncrease = mainHandWeapon.increasedMaxMana;
        else weaponPassiveIncrease = 0;

        if (offHandWeapon != null) weaponPassiveIncrease += offHandWeapon.increasedMaxMana;
        else weaponPassiveIncrease += 0;

        int maxManaValue = 20 + currentWillpowerValue * 35 + weaponPassiveIncrease;

        if (isMaxManaChanged)
        {
            mana.SetMaximumMana(maxManaValue, shouldManaFilled, onStart);
        }

        mana.AddMana(manaIncrease);
    }

    public void RecalculateSecondaryStats()
    {
        UpdatePlayerHealth(0, false, true);
        UpdatePlayerMana(0, false, true);
        UpdateDamageValues();
        UpdateAttackRatingAndCriticalValues();
        UpdateBlockAndEvasivenessValues();
        UpdateSpeedValue();
    }

    public bool HasPlayerStrongNegativeStatusEffectExcludingHealth()
    {
        bool hasnegativeStatusEffect = isFeared || isChilled || isBlind || isSlowed || moveStatus != MoveStatus.Idle;

        return hasnegativeStatusEffect;
    }


    /// <summary>
    /// Returns the player position
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        Vector3 rb2dPosition = new Vector3(rb2D.position.x, rb2D.position.y, 0f);

        return rb2dPosition + new Vector3(0f, 0.7f, 0f);
    }



    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHuntersReachActive && collision.CompareTag(Settings.collisionTilemap))
        { 
            Debug.Log("Player hit a wall during grapple. Cancelling...");
            playersGrapple.ReleaseGrapple(); // Reference to hook or use event
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = this == null ? Vector3.zero : transform.position + new Vector3(0f, 0.8f, 0f);
        Gizmos.DrawWireSphere(position, NymarasWindveilCircleRadius);
    }
}
