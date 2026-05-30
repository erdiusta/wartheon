using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using Random = UnityEngine.Random;

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
[RequireComponent(typeof(WeaponMastery))]
#endregion
[DisallowMultipleComponent]
public class Player : MonoBehaviour
{ 
    public event Action<Player, PlayerDetailsSO> OnPlayerReady;
    public event Action<Player> OnLocalAuthReady;

    public bool initialWeaponStateApplied = false;

    public bool isInitialized { get; private set; }

    public PlayerDetailsSO playerDetails { get; set; }
    public PlayerNetworkAuthority NetAuth { get; private set; }
    public bool IsLocal
    {
        get
        {
            // True-single player
            if (!NetworkServer.active && !NetworkClient.active) return true;

            // Multiplayer
            return NetAuth != null && NetAuth.isLocalPlayer;
        }
    }

    public bool IsReady { get; private set; }
    public event Action<Player> OnPlayerReadySafe;

    public Transform forcefieldTransform;
    public Animator levelUpAnimator;
    public LevelUpDetailsSO levelUpDetails;

    [HideInInspector] public CameraManager cameraManager;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public Health health;
    [HideInInspector] public ManaEvent manaEvent;
    [HideInInspector] public Mana mana;
    [HideInInspector] public ConsumableEvent consumableEvent;
    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public HealthStatus healthStatus = HealthStatus.Normal;
    [HideInInspector] public ArmorStatus armorStatus = ArmorStatus.Normal;
    [HideInInspector] public DestroyedEvent destroyedEvent;
    [HideInInspector] public PlayerControl playerControl;
    [HideInInspector] public PlayerSkillController playerSkillController;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    [HideInInspector] public FireWeapon fireWeapon;
    [HideInInspector] public MeleeAttackEvent meleeAttackEvent;
    [HideInInspector] public MeleeAttackMainHand meleeAttackMainHand;
    [HideInInspector] public RangedAttackEvent rangedAttackEvent;
    [HideInInspector] public SetActiveWeaponEvent setActiveWeaponEvent;
    [HideInInspector] public SetPassiveItemEvent setPassiveItemEvent; 
    [HideInInspector] public AimWeapon aimWeapon;
    [HideInInspector] public AimWeaponNetwork aimWeaponNetwork;
    [HideInInspector] public ActiveWeapon activeWeapon;
    [HideInInspector] public PlayerWeaponState weaponState;
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
    [HideInInspector] public StatusEffectAnimators statusEffectAnimators;
    [HideInInspector] public PlayerAnimationSync animSync;
    [HideInInspector] public PlayerNetworkAuthority networkAuthority;
    [HideInInspector] public PlayerNetworkState state;
    [HideInInspector] public PlayerInventory playerInventory;
    [HideInInspector] public PlayerInventoryNetwork playerInventoryNetwork;
    [HideInInspector] public AimDirection LastAim { get; set; }
    [HideInInspector] public AttackDirection LastAttackdir { get; set; }

    [HideInInspector] public int keyCount = 0;
    [HideInInspector] public int previousSetIndex = 1;
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
    [HideInInspector] public int currentDamageVsLowHealthModifierValue;

    // CRITICAL HIT AND ATTACK RATING-- VALUES
    [HideInInspector] public float currentMainHandCriticalHitChance;
    [HideInInspector] public float currentOffHandCriticalHitChance;
    [HideInInspector] public float currentMainHandCriticalHitDamage;
    [HideInInspector] public float currentOffHandCriticalHitDamage;
    [HideInInspector] public float currentAttackRatingValue;

    // CURRENT BLOCK AND EVASIVENESS VALUES
    [HideInInspector] public float? currentBlockValue;
    [HideInInspector] public float currentDodgeValue;

    // CURRENT TOTAL EXPERIENCE - LEVEL STATS
    [HideInInspector] public int currentLevel = 1;
    [HideInInspector] public int currentGainedTotalExperiencePoints = 0;
    [HideInInspector] public int currentSkillPoints = 0;
    [HideInInspector] public int currentStatPoints = 0;

    // --- EXTRA MODIFIERS ----
    // ARMOR
    [HideInInspector] public float additionalArmorModifier = 0f;
    [HideInInspector] public float additionalShieldArmorModifier = 0f;
    [HideInInspector] public float currentShieldArmor = 0f;
    [HideInInspector] public bool isShieldCalculated = false;

    // ATTACK COOLDOWN
    [HideInInspector] public float additionalAttackCoolDownModifier = 0f;

    // ATTACK RANGE
    [HideInInspector] public float additionalAttackRangeModifier = 0f;

    // ARMOR PENETRATION
    [HideInInspector] public float currentArmorPenetrationValue = 0f;
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
    [HideInInspector] public float currentCriticalResistanceValue = 0f;
    [HideInInspector] public float additionalCriticalResistanceModifier = 0f;

    // DAMAGE
    [HideInInspector] public float additionalPhysicalDamageModifer = 0f;
    [HideInInspector] public float additionalMagicDamageModifier = 0f;

    // DAMAGE REDUCTION
    [HideInInspector] public float currentDamageReductionValue;
    [HideInInspector] public float additionalDamageReductionModifier = 0f;

    // DODGE
    [HideInInspector] public float additionalDodgeRateModifier = 0f;

    // LIFE STEAL
    [HideInInspector] public int currentLifeStealValue = 0;
    [HideInInspector] public int additionalLifeStealModifier = 0;

    // SKILL COOLDOWN
    [HideInInspector] public float currentSkillCooldownReducer = 0f;
    [HideInInspector] public float additionalSkillCoolDownModifier = 0f;

    // SKILL DURATION
    [HideInInspector] public float currentSkillDurationModifier = 0f;
    [HideInInspector] public float additionalSkillDurationModifier = 0f;

    // SPEED
    [HideInInspector] public float additionalSpeedModifier = 0f;

    // STATUS RESISTANCE
    [HideInInspector] public float additionalStatusResistanceModifier = 0f;

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

    [HideInInspector] public float currentStatusResistance = 0f;
    [HideInInspector] public float additionalNPCCostModifier = 0;
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
    [HideInInspector] public bool isSeismicSlamActive;
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

    // STATUS EFFECT
    [HideInInspector] public bool isCursed;
    [HideInInspector] public bool isFeared;
    [HideInInspector] public bool isRevealed;
    [HideInInspector] public bool isStatic;
    [HideInInspector] public bool isWarmed;
    [HideInInspector] public bool isChilled;
    [HideInInspector] public bool isBlind;
    [HideInInspector] public bool isSlowed;

    // STATUS INFLICT TYPES
    [HideInInspector] public float additionalPoisonChance = 0f;
    [HideInInspector] public float additionalBleedChance = 0f;
    [HideInInspector] public float additionalRootChance = 0f;
    [HideInInspector] public float additionalStunChance = 0f;
    [HideInInspector] public float additionalCurseChance = 0f;
    [HideInInspector] public float additionalFearChance = 0f;
    [HideInInspector] public float additionalRevealChance = 0f;
    [HideInInspector] public float additionalParalyzeChance = 0f;
    [HideInInspector] public float additionalBurnChance = 0f;
    [HideInInspector] public float additionalFreezeChance = 0f;
    [HideInInspector] public float additionalBlindChance = 0f;
    [HideInInspector] public float additionalSlowChance = 0f;

    [HideInInspector] public uint netId;

    float testAdditionArmor = 0.1f;

    // Tracks only the sum coming from equipped weapons so we can remove it cleanly on change
    private float _weaponAttackCooldownContribution = 0f;
    private float _passiveAttackCooldownContribution = 0f;

    public LayerMask layerMask;
    [HideInInspector] public ContactFilter2D filter = new ContactFilter2D();

    bool subscribed;

    private void Awake()
    {
        animSync = GetComponent<PlayerAnimationSync>();
        NetAuth = GetComponent<PlayerNetworkAuthority>();

        cameraManager = GetComponentInChildren<CameraManager>();
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        manaEvent = GetComponent<ManaEvent>();
        mana = GetComponent<Mana>();
        consumableEvent = GetComponent<ConsumableEvent>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        playerControl = GetComponent<PlayerControl>();
        playerSkillController = GetComponent<PlayerSkillController>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rangedAttackEvent = GetComponentInChildren<RangedAttackEvent>();
        meleeAttackMainHand = GetComponent<MeleeAttackMainHand>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        setPassiveItemEvent = GetComponent<SetPassiveItemEvent>();
        aimWeapon = GetComponent<AimWeapon>();
        aimWeaponNetwork = GetComponent<AimWeaponNetwork>();
        animator = GetComponent<Animator>();
        activeWeapon = GetComponent<ActiveWeapon>();
        weaponState = GetComponent<PlayerWeaponState>();
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
        weaponMastery = GetComponent<WeaponMastery>();
        damageTracker = GetComponent<DamageTracker>();
        mainHandWeaponAnchorTransform = transform.GetChild(0);
        offHandWeaponAnchorTransform = transform.GetChild(1);
        springJoint2D = GetComponent<SpringJoint2D>();
        statusEffectAnimators = GetComponentInChildren<StatusEffectAnimators>();
        specialMoveParticlesSystem = GetComponentInChildren<ParticleSystem>();
        networkAuthority = GetComponent<PlayerNetworkAuthority>();
        state = GetComponent<PlayerNetworkState>();
        playerInventory = GetComponent<PlayerInventory>();
        playerInventoryNetwork = GetComponent<PlayerInventoryNetwork>();

        LastAim = AimDirection.Down;
        LastAttackdir = AttackDirection.Down;

        if (state != null) state.CharacterAssigned += OnCharacterAssigned;
    }

    private void OnCharacterAssigned(int characterIndex)
    {
        PlayerDetailsSO details = GameResources.Instance.playerDetailsArray[characterIndex];
        Initialize(details);
    }

    /// <summary>
    /// Initialize the player
    /// </summary>
    public void Initialize(PlayerDetailsSO details)
    {
        if (isInitialized) return;

        isInitialized = true;
        playerDetails = details;

        animator.runtimeAnimatorController = playerDetails.bodyRuntimeAnimatorController;

        // Set player primary stats
        SetPlayerPrimaryStats(playerDetails);

        // After primary stats because initial move speed is affected by Agility
        OnPlayerReady?.Invoke(this, playerDetails); // HERE ADDED
        IsReady = true;
        netId = NetworkServer.active ? NetAuth.netId : 0;

        keyCount = 30;
        coinsAndShards.coinAmount = 200;

        // Set starting equipment
        CreatePlayerStartingWeapons();
        CreatePlayerStartingPassiveItem();

        initialWeaponStateApplied = true;

        // Set player starting secondary stats
        SetPlayerSecondaryStats();

        // Set player all active unique skills
        SetPlayerActiveUniqueSkills();

        specialMoveRecastCountArray = new int[] { -1, -1, -1 };

        // THIS IS THE KEY LINE
        Subscribe();

        moveStatus = MoveStatus.Idle;
    }

    private void Subscribe()
    {
        if (subscribed) return;

        subscribed = true;

        manaEvent.OnManaChanged += ManaEvent_OnManaChanged;
        healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
        healthEvent.GetSlow += HealthEvent_GetSlow;

        rangedAttackEvent.OnAnimationRangedAttackAnimationTriggered.AddListener(ResetAttackForRangedAttack);

        // Notify health AFTER everything is ready
        health.OnPlayerInitialized(this);

        // Fire initial sync AFTER subscription
        manaEvent.CallManaChangedEvent(mana.currentMana);

        // Add it to dynamicGameObjectsInScene
        SceneObjectsManager.dynamicGameObjectsInScene.Add(gameObject);
    }

    private void OnDisable()
    {
        if (!subscribed) return;

        manaEvent.OnManaChanged -= ManaEvent_OnManaChanged;
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
        healthEvent.GetSlow -= HealthEvent_GetSlow;

        rangedAttackEvent.OnAnimationRangedAttackAnimationTriggered.RemoveListener(ResetAttackForRangedAttack);

        subscribed = false;
    }

    public void MarkLocalAuthorityReady(Player player)
    {
        OnLocalAuthReady?.Invoke(this);
        GameManager.Instance.NotifyLocalPlayerReady(player);
    }

    /// <summary>
    /// Handle health changed event
    /// </summary>
    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        // If player has died
        if (healthEventArgs.healthAmount <= 0f)
        {
            DestroyUtility.Destroy(destroyedEvent.gameObject, playerDied: true, 0);
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

            Weapon startedWeapon = new Weapon(playerDetails.startingWeaponList[i].rarity);

            startedWeapon.weaponStats = new WeaponStats
            {
                weaponClass = playerDetails.startingWeaponList[i].weaponClass,
                weaponTitle = playerDetails.startingWeaponList[i].weaponTitle,
                wieldType = playerDetails.startingWeaponList[i].wieldType,
                isMeleeWeapon = playerDetails.startingWeaponList[i].isMeleeWeapon,
                hasSwing = playerDetails.startingWeaponList[i].hasSwing,
                elementalForgeRate = playerDetails.startingWeaponList[i].elementalForgeRate,

                baseUniqueRolled = playerDetails.startingWeaponList[i].baseUniqueModifier,
                baseTypeRolled = playerDetails.startingWeaponList[i].baseTypeModifier,

                physicalDamageMin = playerDetails.startingWeaponList[i].physicalDamageMin,
                physicalDamageMax = playerDetails.startingWeaponList[i].physicalDamageMax,
                magicDamageMin = playerDetails.startingWeaponList[i].magicDamageMin,
                magicDamageMax = playerDetails.startingWeaponList[i].magicDamageMax,

                weaponCooldownDuration = playerDetails.startingWeaponList[i].weaponCooldownDuration,
                blockChance = playerDetails.startingWeaponList[i].blockChance,
                weaponAttackRating = playerDetails.startingWeaponList[i].weaponAttackRating,
                criticalHitChance = playerDetails.startingWeaponList[i].criticalHitChance,
                criticalHitDamage = playerDetails.startingWeaponList[i].criticalHitDamageMultiplier,

                criticalHitChanceIncrease = playerDetails.startingWeaponList[i].criticalHitChance,
                criticalHitDamageIncrease = playerDetails.startingWeaponList[i].criticalHitDamageMultiplier
            };

            startedWeapon.ItemType = ItemType.Weapon;

            // Special case: Caelion should equip shield (2nd item) to off-hand
            if (i == 0)
            {
                if (NetworkClient.active)
                {
                    weaponState.mainHandWeaponRarity = startedWeapon.Rarity;
                    weaponState.mainHandWeaponTitle = startedWeapon.weaponStats.weaponTitle;
                }
            }
            else if (i == 1)
            {
                if (playerDetails.playerCharacterIndex == Character.Caelion)
                {
                    equipOffHand = true;

                    if (NetworkClient.active)
                    {
                        weaponState.offHandWeaponRarity = startedWeapon.Rarity;
                        weaponState.offhandWeaponTitle = startedWeapon.weaponStats.weaponTitle;
                    }
                }
                else if (playerDetails.playerCharacterIndex == Character.Morven || playerDetails.playerCharacterIndex == Character.Karnag ||
                    playerDetails.playerCharacterIndex == Character.Nyxa)
                {
                    dualWieldOnStart = true;

                    if (NetworkClient.active)
                    {
                        weaponState.offHandWeaponRarity = startedWeapon.Rarity;
                        weaponState.offhandWeaponTitle = startedWeapon.weaponStats.weaponTitle;
                    }
                }
            }

            AddNextWeaponToPlayer(ref startedWeapon, playerDetails.startingWeaponList[i], pickingUp: false, onStart: true, onlySwitch: false, dualWieldOnStart, equipOffHand, i);
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

    /// <summary>
    /// Set the player primary stats
    /// </summary>
    private void SetPlayerSecondaryStats()
    {
        // Set player starting health
        UpdatePlayerHealth(healthIncrease: 0, shouldHealthFilled: true, isMaxHealthChanged: true, onStart: true);
        UpdatePlayerMana(manaIncrease: 0, shouldManaFilled: true, isMaxManaChanged: true, onStart: true);
        UpdateArmorValues(onStart: true);
        UpdateDamageValues();
        UpdateAttackRatingAndCriticalValues(onStart: true);
        UpdateBlockAndDodgeValues();
        UpdateSpeedAndAttackCooldownValues();
        UpdateResistanceValues();
        UpdateSecondaryDamageValues();
        UpdateSkillAndStatusModifierValues();
    }

    /// <summary>
    /// Set the player's all active unique skills
    /// </summary>
    private void SetPlayerActiveUniqueSkills()
    {
        playersAllActiveUniqueSkills = new ActiveUniqueSkillDetailsSO[]{playerDetails.firstActiveSkillDetails, playerDetails.secondActiveSkillDetails,
            playerDetails.thirdActiveSkillDetails,playerDetails.fourthActiveSkillDetails, playerDetails.fifthActiveSkillDetails};
    }

    public void UpdateArmorValues(bool onStart = false)
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        // Base armor from weapons (non-shield offhand contributes normally
        float mainArmor = main?.weaponStats.armorIncrease ?? 0f;

        float offHandNonShieldArmor = 0f;
        float shieldArmor = 0f;

        if (off != null)
        {
            if (off.weaponStats.weaponClass == WeaponClass.Shield)
            {
                // Shield base armor, scaled by shield-specific modifier
                shieldArmor = off.weaponStats.armorIncrease * (1f + additionalShieldArmorModifier);
            }
            else
            {
                offHandNonShieldArmor = off.weaponStats.armorIncrease;
            }
        }

        // Armor from passives (sum each roll that is ArmorIncrease)
        float passiveArmor = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
                if (HasBoostType(item, BoostType.ArmorIncrease, i)) passiveArmor += item.passiveStats.armorIncrease;
        }

        // Keep this in sync for any UI/other logic that reads it
        currentShieldArmor = shieldArmor;

        // Global modifiers
        float total = mainArmor + offHandNonShieldArmor + passiveArmor + currentShieldArmor + additionalShieldArmorModifier;

        currentArmorValue = (float)Math.Round(total, 2);

        if (playerDetails.playerCharacterIndex == Character.Nyxa && onStart) currentArmorValue = (float)Math.Round(currentArmorValue + testAdditionArmor, 2);
    }

    public void UpdateDamageValues()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        // MAIN-HAND
        (currentMainHandMinDamageValue, currentMainHandMaxDamageValue) = ComputeEquippedDamage(main, isOffHand: false);

        // OFF-HAND
        (currentOffHandMinDamageValue, currentOffHandMaxDamageValue) = ComputeEquippedDamage(off, isOffHand: true);

        // Armor Penetration
        UpdateArmorPenetrationValue();

        // Attack Range
        UpdateAttackRange();
    }

    /// <summary>
    /// Computes final (min,max) damage for an equipped weapon using the weapon's
    /// dynamic rolled stats (physical/magic split), player attributes, and global modifiers.
    /// Off-hand damage is scaled by 0.6f. Shields always return (0,0).
    /// </summary>
    private (int min, int max) ComputeEquippedDamage(Weapon weapon, bool isOffHand, bool onStart = false)
    {
        if (weapon == null || weapon.weaponStats.weaponTitle == WeaponTitle.None) return (0, 0);

        // Shields don't deal damage
        if (weapon.weaponStats.weaponClass == WeaponClass.Shield) return (0, 0);

        // Rolled base damage from the instance (NOT from ScriptableObject)
        int physMin = weapon.weaponStats.physicalDamageMin + Mathf.Max(0, weapon.weaponStats.physicalAttackDamageIncrease);
        int physMax = weapon.weaponStats.physicalDamageMax + Mathf.Max(0, weapon.weaponStats.physicalAttackDamageIncrease);

        int magMin = weapon.weaponStats.magicDamageMin + Mathf.Max(0, weapon.weaponStats.magicAttackDamageIncrease);
        int magMax = weapon.weaponStats.magicDamageMax + Mathf.Max(0, weapon.weaponStats.magicAttackDamageIncrease);

        // --- add PASSIVE FLATS (per equipped passive item) ---
        // These are additive, not multiplicative.
        var (passivePhysFlat, passiveMagFlat) = GetPassiveFlatDamageAdds();
        physMin += passivePhysFlat; physMax += passivePhysFlat;
        magMin += passiveMagFlat; magMax += passiveMagFlat;

        // --- attribute scaling (unchanged) ---
        int physAdd = 0;
        if (physMin > 0 || physMax > 0)
        {
            if (weapon.weaponStats.isMeleeWeapon)
            {
                if (weapon.weaponStats.weaponClass == WeaponClass.Dagger || weapon.weaponStats.weaponClass == WeaponClass.Claw)
                    physAdd = Mathf.RoundToInt(currentDexterityValue * 0.7f);
                else
                    physAdd = Mathf.RoundToInt(currentStrengthValue * 1.5f);
            }
            else
            {
                physAdd = Mathf.RoundToInt(currentDexterityValue * 0.7f);
            }
        }

        int magAdd = 0;
        if (magMin > 0 || magMax > 0)
        {
            float intCoef = weapon.weaponStats.isMeleeWeapon ? 2f : 1.5f;
            magAdd = Mathf.RoundToInt(currentIntelligenceValue * intCoef);
        }

        int totalMin = physMin + physAdd + magMin + magAdd;
        int totalMax = physMax + physAdd + magMax + magAdd;

        // global modifier you already had (physical); keep or split if you have magic too
        totalMin = Mathf.RoundToInt(totalMin * (1f + additionalPhysicalDamageModifer));
        totalMax = Mathf.RoundToInt(totalMax * (1f + additionalPhysicalDamageModifer));

        if (isOffHand)
        {
            totalMin = Mathf.RoundToInt(totalMin * 0.6f);
            totalMax = Mathf.RoundToInt(totalMax * 0.6f);
        }

        totalMin = Mathf.Max(0, totalMin);
        totalMax = Mathf.Max(totalMin, totalMax);
        return (totalMin, totalMax);
    }

    private (int physFlat, int magFlat) GetPassiveFlatDamageAdds()
    {
        int phys = 0, mag = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            // Physical
            if (HasBoostType(item, BoostType.AttackDamage, 1) ||
                HasBoostType(item, BoostType.AttackDamage, 2) ||
                HasBoostType(item, BoostType.AttackDamage, 3) ||
                HasBoostType(item, BoostType.AttackDamage, 4))
            {
                phys += Mathf.Max(0, item.passiveStats.physicalAttackDamageIncrease);
            }

            // Magic
            if (HasBoostType(item, BoostType.MagicDamage, 1) ||
                HasBoostType(item, BoostType.MagicDamage, 2) ||
                HasBoostType(item, BoostType.MagicDamage, 3) ||
                HasBoostType(item, BoostType.MagicDamage, 4))
            {
                mag += Mathf.Max(0, item.passiveStats.magicAttackDamageIncrease);
            }
        }

        return (phys, mag);
    }

    private void UpdateArmorPenetrationValue()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float armorPenIncrease = 0f;
        if (main != null) armorPenIncrease += main.weaponStats.armorPenetration;
        if (off != null) armorPenIncrease += off.weaponStats.armorPenetration;

        float prePassive = additionalArmorPenetrationModifier + armorPenIncrease;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.ArmorPenetration, i)) withPassive += item.passiveStats.armorPenetration;
            }
        }

        currentArmorPenetrationValue = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.35f);
    }

    private void UpdateAttackRange()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float attackRangeIncrease = 0f;
        if (main != null) attackRangeIncrease += main.weaponStats.attackRange;
        if (off != null) attackRangeIncrease += off.weaponStats.attackRange;

        float prePassive = attackRangeIncrease;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.AttackRange, i)) withPassive += item.passiveStats.attackRange;
            }
        }

        additionalAttackRangeModifier = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.35f);
    }

    public void UpdateAttackRatingAndCriticalValues(bool onStart = false)
    {
        UpdateCurrentAttackRatingValues();
        UpdateCurrentCriticalHitChance(onStart);
        UpdateCurrentCriticalHitDamage(onStart);
    }

    public void UpdateBlockAndDodgeValues()
    {
        UpdateBlockValue();
        UpdateDodgeValue();
    }

    public void UpdateSpeedAndAttackCooldownValues()
    {
        UpdateSpeedValue();
        UpdateAttackCooldown();
    }

    public void UpdateResistanceValues()
    {
        UpdateArmorValues();
        UpdateMagicResistance();
        UpdateStatusResistance();
        UpdateDamageReduction();
        UpdateCriticalResistance();
    }

    private void UpdateMagicResistance()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float magicResistanceIncrease = 0f;

        if (main != null) magicResistanceIncrease = main.weaponStats.magicResistance;
        else magicResistanceIncrease = 0f;

        if (off != null) magicResistanceIncrease += off.weaponStats.magicResistance;
        else magicResistanceIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.MagicResistance, i)) withPassive += item.passiveStats.magicResistanceModifier;
            }
        }

        currentMagicResistanceValue = Mathf.Min((float)Math.Round(currentWillpowerValue * 0.01f + magicResistanceIncrease + withPassive, 2), 0.5f);
    }

    private void UpdateStatusResistance()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float statusResistanceIncrease = 0f;

        if (main != null) statusResistanceIncrease = main.weaponStats.statusResistanceModifier;
        else statusResistanceIncrease = 0f;

        if (off != null) statusResistanceIncrease += off.weaponStats.statusResistanceModifier;
        else statusResistanceIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.StatusResistance, i)) withPassive += item.passiveStats.statusResistanceModifier;
            }
        }

        currentStatusResistance = Mathf.Min((float)Math.Round(currentResolveValue * 0.01f + statusResistanceIncrease + withPassive + additionalStatusResistanceModifier, 2), 0.5f);
    }

    private void UpdateDamageReduction()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float damageReductionIncrease = 0f;

        if (main != null) damageReductionIncrease = main.weaponStats.damageReductionRate;
        else damageReductionIncrease = 0f;

        if (off != null) damageReductionIncrease += off.weaponStats.damageReductionRate;
        else damageReductionIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.DamageReduction, i)) withPassive += item.passiveStats.damageReductionRate;
            }
        }

        currentDamageReductionValue = Mathf.Min((float)Math.Round(currentStrengthValue * 0.005f + damageReductionIncrease + withPassive + 
            additionalDamageReductionModifier, 2), 0.35f);
    }

    public void UpdateCriticalResistance()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float criticalResistanceIncrease = 0f;

        if (main != null) criticalResistanceIncrease = main.weaponStats.criticalResistanceModifier;
        else criticalResistanceIncrease = 0f;

        if (off != null) criticalResistanceIncrease += off.weaponStats.criticalResistanceModifier;
        else criticalResistanceIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.CritResistance, i)) withPassive += item.passiveStats.criticalResistanceModifier;
            }
        }

        currentCriticalResistanceValue = Mathf.Min((float)Math.Round(currentResolveValue * 0.01f + criticalResistanceIncrease + withPassive + 
            additionalCriticalResistanceModifier, 2), 0.5f);
    }

    public void UpdateSpeedValue()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float weaponSpeedIncrease = 0f;

        if (main != null) weaponSpeedIncrease = main.weaponStats.speedIncreaseModifier;
        else weaponSpeedIncrease = 0f;

        if (off != null) weaponSpeedIncrease += off.weaponStats.speedIncreaseModifier;
        else weaponSpeedIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.MoveSpeed, i)) withPassive += item.passiveStats.speedIncreaseModifier;
            }
        }

        movementByForce.moveSpeed = movementByForce.movementDetails.baseMaxMoveSpeed + currentAgilityValue * 0.25f +
            additionalSpeedModifier + weaponSpeedIncrease + withPassive;
    }

    public void UpdateAttackCooldown()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        // Recompute weapon contribution (seconds; >= 0)
        float newWeaponContribution = 0f;

        if (main != null) newWeaponContribution += Mathf.Max(0f, main.weaponStats.attackCooldownModifier);

        // Ignore shields (or any off-hand that shouldn't affect cooldown)
        if (off != null && off.weaponStats.weaponClass != WeaponClass.Shield)
            newWeaponContribution += Mathf.Max(0f, off.weaponStats.attackCooldownModifier);

        // --- 2) Recompute PASSIVE contribution (seconds; >= 0, summed per matching slot) ---
        float newPassiveContribution = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            // Each matching slot adds item.attackCooldown (your roll is additive, not %)
            if (HasBoostType(item, BoostType.AttackCooldown, 1)) newPassiveContribution += Mathf.Max(0f, item.passiveStats.attackCooldown);
            if (HasBoostType(item, BoostType.AttackCooldown, 2)) newPassiveContribution += Mathf.Max(0f, item.passiveStats.attackCooldown);
            if (HasBoostType(item, BoostType.AttackCooldown, 3)) newPassiveContribution += Mathf.Max(0f, item.passiveStats.attackCooldown);
            if (HasBoostType(item, BoostType.AttackCooldown, 4)) newPassiveContribution += Mathf.Max(0f, item.passiveStats.attackCooldown);
        }

        // --- 3) Apply delta to the global modifier: remove old, add new ---
        additionalAttackCoolDownModifier -= _weaponAttackCooldownContribution;
        additionalAttackCoolDownModifier -= _passiveAttackCooldownContribution;

        additionalAttackCoolDownModifier += newWeaponContribution;
        additionalAttackCoolDownModifier += newPassiveContribution;

        // Cache for next time
        _weaponAttackCooldownContribution = newWeaponContribution;
        _passiveAttackCooldownContribution = newPassiveContribution;
    }

    public PassiveItem  AddPassiveItemToPlayer(ref PassiveItem passiveItem, PassiveItemSlotName passiveItemSlotName, DropItem dropItem = null)
    {
        setPassiveItemEvent.CallEquipPassiveItem(this, passiveItem, passiveItemSlotName);

        if (equippedPassiveItems[passiveItemSlotName] != null)
        {
            PassiveItem oldItem = equippedPassiveItems[passiveItemSlotName];

            if (!playerInventory.IsInventoryFull())
            {
                int index = playerInventory.PlaceItemToInventoryIndexSlot(oldItem);
                oldItem.ItemSlotStatus = ItemSlotStatus.Inventory;

                StaticEventHandler.CallPassiveItemAddedToInventorySlot(passiveItem, index);
            }
        }
        else
        {
            // Equip to inventory
            StaticEventHandler.CallItemAddedToPassiveItemSlot(passiveItem, passiveItemSlotName);
        }

        return passiveItem;
    }

    /// <summary>
    /// Add a weapon to the player weapon list
    /// </summary>
    public void AddNextWeaponToPlayer(ref Weapon weapon, WeaponDetailsSO weaponDetails, bool pickingUp, bool onStart, bool onlySwitch, bool dualWieldOnStart = false, 
        bool equipOffHand = false, int startingWeaponIndex = 0) // <- pass the real instance for pickups/switches
    {
        if (onStart)
        {
            // Starting gear: create a fresh instance using SO.rarity
            CreateStartingWeaponInstance(ref weapon, equipOffHand ? ItemSlotStatus.OffHand : ItemSlotStatus.MainHand, weaponDetails);
        }
        else
        {
            // Non-start flows MUST provide the real rolled instance to prevent duplication.
            if (weapon == null && weapon == DropItem.droppedThrowingAxe)
            {
                Debug.LogError("AddNextWeaponToPlayer: Non-start flow requires a Weapon instance (weaponInstance != null) to avoid duplicates.");
                return;
            }

            // Ensure slot is correct for equipOffHand requests
            if (equipOffHand) weapon.ItemSlotStatus = ItemSlotStatus.OffHand;
            else if (weapon.ItemSlotStatus == ItemSlotStatus.None) weapon.ItemSlotStatus = ItemSlotStatus.MainHand; // sane default if not set yet
        }

        // Early validations
        if (equipOffHand && weapon.weaponStats.wieldType == WieldType.TwoHanded) return;

        // ---------- OFF-HAND EQUIP PATH ----------
        if (equipOffHand)
        {
            if (weapon.weaponStats.weaponClass == WeaponClass.Shield)
            {
                // Equip shield to off-hand (start or non-start)
                weapon.ItemSlotStatus = ItemSlotStatus.OffHand;

                if (onStart)
                {
                    // stats were set when created; nothing else to do
                }

                weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] = weapon;
                ActivateWeapon(weapon, weapon.ItemSlotStatus, currentWeaponSlotSetIndex, onStart);
                StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                weapon.weaponStats.weaponBelongingToWhichOffHandSet = currentWeaponSlotSetIndex;
                return;
            }
            else
            {
                // Non-shield off-hand (must be one-handed)
                weapon.ItemSlotStatus = ItemSlotStatus.OffHand;

                weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] = weapon;
                ActivateWeapon(weapon, weapon.ItemSlotStatus, currentWeaponSlotSetIndex, onStart);
                StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                weapon.weaponStats.weaponBelongingToWhichOffHandSet = currentWeaponSlotSetIndex;
                return;
            }
        }

        // ---------- AUTO FILL OFF-HAND ON START (shield or dual wield) ----------
        if (!playerInventory.IsAllOffhandWeaponSetsFull() && (weaponDetails.weaponClass == WeaponClass.Shield || dualWieldOnStart))
        {
            // Only allow off-hand if current main-hand in that set isn't two-handed
            for (int setIdx = pickingUp ? currentWeaponSlotSetIndex - 1 : 0; setIdx < 3; setIdx++)
            {
                bool tryThisSet = pickingUp ? setIdx == currentWeaponSlotSetIndex - 1 : weaponSlotSetArray[setIdx][1] == null;
                if (!tryThisSet) continue;

                var main = weaponSlotSetArray[setIdx][0];
                if (main != null && main.weaponStats.wieldType == WieldType.TwoHanded) continue;

                // Place to off-hand
                weapon.ItemSlotStatus = ItemSlotStatus.OffHand;
                weaponSlotSetArray[setIdx][1] = weapon;
                weapon.weaponStats.weaponBelongingToWhichOffHandSet = setIdx + 1;

                if (currentWeaponSlotSetIndex - 1 == setIdx)
                    ActivateWeapon(weapon, weapon.ItemSlotStatus, setIdx + 1, onStart);

                if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                return;
            }
        }

        // ---------- MAIN-HAND EQUIP PATH ----------
        if (!playerInventory.IsAllMainWeaponSetsFull() && weaponDetails.weaponClass != WeaponClass.Shield)
        {
            // Fill the current set first when picking up; else fill first available at start
            if (pickingUp)
            {
                if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] == null)
                {
                    weapon.ItemSlotStatus = ItemSlotStatus.MainHand;
                    weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] = weapon;
                    weapon.weaponStats.weaponBelongingToWhichMainHandSet = currentWeaponSlotSetIndex;

                    ActivateWeapon(weapon, weapon.ItemSlotStatus, currentWeaponSlotSetIndex, onStart);
                    StaticEventHandler.CallWeaponUnlockedEvent(weaponDetails.weaponTitle);
                    if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);

                    return;
                }
            }
            else
            {
                // On start, fill first empty main-hand slot from set 1..3
                for (int setIdx = 0; setIdx < 3; setIdx++)
                {
                    if (weaponSlotSetArray[setIdx][0] != null) continue;

                    weapon.ItemSlotStatus = ItemSlotStatus.MainHand;
                    weaponSlotSetArray[setIdx][0] = weapon;
                    weapon.weaponStats.weaponBelongingToWhichMainHandSet = setIdx + 1;

                    if (currentWeaponSlotSetIndex - 1 == setIdx)
                        ActivateWeapon(weapon, weapon.ItemSlotStatus, setIdx + 1, onStart, startingWeaponIndex);

                    if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);

                    return;
                }
            }
        }
        else
        {
            // All main-hand filled; try off-hand (non-two-handed & non-shield already handled above)
            if (!playerInventory.IsAllOffhandWeaponSetsFull() && weaponDetails.wieldType != WieldType.TwoHanded && weaponDetails.weaponClass != WeaponClass.Shield)
            {
                for (int setIdx = 0; setIdx < 3; setIdx++)
                {
                    if (weaponSlotSetArray[setIdx][1] != null) continue;

                    weapon.ItemSlotStatus = ItemSlotStatus.OffHand;
                    weaponSlotSetArray[setIdx][1] = weapon;
                    weapon.weaponStats.weaponBelongingToWhichOffHandSet = setIdx + 1;

                    if (currentWeaponSlotSetIndex - 1 == setIdx) 
                        ActivateWeapon(weapon, weapon.ItemSlotStatus, setIdx + 1, onStart);

                    if (!onStart) StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);

                    return;
                }
            }
        }
    }

    private Weapon CreateStartingWeaponInstance(ref Weapon weapon, ItemSlotStatus slot, WeaponDetailsSO startingWeaponDetails)
    {
        // Generate seed
        int seed = Random.Range(int.MinValue, int.MaxValue);
        WartheonRNG rng = new WartheonRNG(seed);

        // Create with so rarity only for start gear
        if (weapon.weaponStats.weaponClass == WeaponClass.Shield)
        {
            // Shields: keep the instance minimal; other runtime stats are irrelevant
            weapon.weaponStats.baseUniqueRolled = startingWeaponDetails.baseUniqueModifier;
            weapon.weaponStats.baseTypeRolled = startingWeaponDetails.baseTypeModifier;

            WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.weaponStats.baseUniqueRolled, startingWeaponDetails, rng);
            WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.weaponStats.baseTypeRolled, startingWeaponDetails, rng);

            return weapon;
        }

        weapon.ItemSlotStatus = slot;
        weapon.weaponStats.criticalHitChanceIncrease = startingWeaponDetails.criticalHitChance;
        weapon.weaponStats.criticalHitDamageIncrease = startingWeaponDetails.criticalHitDamageMultiplier;
        weapon.weaponStats.baseUniqueRolled = startingWeaponDetails.baseUniqueModifier;
        weapon.weaponStats.baseTypeRolled = startingWeaponDetails.baseTypeModifier;

        WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.weaponStats.baseUniqueRolled, startingWeaponDetails, rng);
        WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.weaponStats.baseTypeRolled, startingWeaponDetails, rng);

        return weapon;
    }

    public void ActivateWeapon(Weapon weapon, ItemSlotStatus itemSlotStatus, int setIndex, bool onStart, int onStartWeaponIndex = 0)
    {
        bool isOwnerContext = (!NetworkServer.active && !NetworkClient.active) || IsLocal;
        bool isWeaponSwapping = false; // original ActivateWeapon callers didn't pass swapping flags

        // Delegate to unified notification method
        ApplyWeaponActivationEvents(weapon, itemSlotStatus, setIndex, onStart, isOwnerContext, allowHudEvents: true, allowLockIconUpdate: true, isStatUpdateAllowed: true, isWeaponSwapping, onStartWeaponIndex);
    }

    public void ApplyWeaponActivationEvents(Weapon weapon, ItemSlotStatus itemSlotStatus, int setIndex, bool onStart, bool isOwnerContext, bool allowHudEvents, bool allowLockIconUpdate, bool isStatUpdateAllowed, 
        bool isWeaponSwapping = false, int onStartWeaponIndex = 0)
    {
        if (weapon == null)
        {
            // Deactivate paths
            if (itemSlotStatus == ItemSlotStatus.MainHand)
            {
                // Gameplay deactivation
                setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent(isWeaponSwapping);

                // HUD/owner
                if (isOwnerContext && allowHudEvents) setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEventForHud();
            }
            else // OffHand
            {
                setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent(isStatUpdateAllowed);

                if (isOwnerContext && allowHudEvents) setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEventForHud();
            }

            return;
        }

        if (itemSlotStatus == ItemSlotStatus.MainHand)
        {
            // Gameplay activation
            setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(weapon.weaponStats, weapon.Rarity, setIndex, onStart, isStatUpdateAllowed, onStartWeaponIndex);

            // HUD/owner activation
            if (isOwnerContext && allowHudEvents) setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEventForHud(weapon.weaponStats, weapon.Rarity, setIndex, onStart, onStartWeaponIndex);

            // Now trigger one/two-hand helper events based on currently active weapon state
            if(allowLockIconUpdate) UpdateWeaponHudLockStateSP(isWeaponSwapping);
        }
        else // OffHand
        {
            setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(weapon.weaponStats, weapon.Rarity, setIndex, onStart, isStatUpdateAllowed);

            if (isOwnerContext && allowHudEvents) setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEventForHud(weapon.weaponStats, weapon.Rarity, setIndex, onStart);
        }
    }

    public void UpdateWeaponHudLockStateSP(bool isWeaponSwapping)
    {
        Weapon currentMain = activeWeapon.GetCurrentMainHandWeapon();
        Weapon currentOff = activeWeapon.GetCurrentOffHandWeapon();

        if (currentMain != null)
        {
            if (currentMain.weaponStats.wieldType == WieldType.OneHanded)
            {
                bool swappingOrHasOff = isWeaponSwapping || currentOff != null;
                setActiveWeaponEvent.CallOneHandWeaponEquipEventForLockIconHud(swappingOrHasOff);
            }
            else if (currentMain.weaponStats.wieldType == WieldType.TwoHanded)
            {
                setActiveWeaponEvent.CallTwoHandWeaponEquipEventForLockIconHud();
            }
        }
    }

    public void UpdateWeaponHudLockStateMP(Weapon main, Weapon off)
    {
        if (main == null) return;

        if (main.weaponStats.wieldType == WieldType.OneHanded)
        {
            bool hasOff = off != null;
            setActiveWeaponEvent.CallOneHandWeaponEquipEventForLockIconHud(hasOff);
        }
        else if (main.weaponStats.wieldType == WieldType.TwoHanded)
        {
            setActiveWeaponEvent.CallTwoHandWeaponEquipEventForLockIconHud();
        }
    }

    public void UpdateCurrentAttackRatingValues()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float attackRatingModifier = 0f;

        if (off != null)
        {
            if (main != null)
            {
                attackRatingModifier = ((main.weaponStats.weaponAttackRating + off.weaponStats.weaponAttackRating +
                      main.weaponStats.attackRatingIncrease + off.weaponStats.attackRatingIncrease) / 2f + additionalAttackRatingModifier) * 0.85f;
            }
            else attackRatingModifier = 0f;
        }
        else
        {
            if (main != null) attackRatingModifier = main.weaponStats.weaponAttackRating + additionalAttackRatingModifier + main.weaponStats.attackRatingIncrease;
            else attackRatingModifier = 0f;
        }

        float baseFromStats = 0.2f + ((currentDexterityValue / 3f + currentFerocityValue / 3f + currentStrengthValue / 4f) / 100f);
        float prePassive = baseFromStats + attackRatingModifier - blindModifier;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.AttackRating, i)) withPassive += item.passiveStats.attackRating;
            }
        }

        currentAttackRatingValue = (float)Math.Round(prePassive + withPassive, 2);
    }

    public void UpdateCurrentCriticalHitChance(bool onStart = false)
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float dexCrit = (float)Math.Round(currentDexterityValue * 0.75f / 100f, 2);

        WeaponDetailsSO mainWeaponDetails = (onStart && main != null) ? playerDetails.startingWeaponList[0] : main != null ? WartheonDatabase.Instance.GetWeaponDetails(main.weaponStats.weaponTitle) : null;
        WeaponDetailsSO offWeaponDetails = (onStart && off != null) ? playerDetails.startingWeaponList[1] : off != null ? WartheonDatabase.Instance.GetWeaponDetails(off.weaponStats.weaponTitle) : null;

        // MAIN HAND
        if (main != null)
        {
            float baseFromWeapon = main.weaponStats.isMeleeWeapon ? main.weaponStats.criticalHitChance : mainWeaponDetails.weaponCurrentProjectile.criticalHitChance;
            float prePassive = dexCrit + baseFromWeapon + additionalCriticalHitChanceModifier + main.weaponStats.criticalHitChanceIncrease;

            float withPassive = 0f;

            foreach (var kvp in equippedPassiveItems)
            {
                var item = kvp.Value;

                if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                for (int i = 1; i <= 4; i++)
                {
                    if (HasBoostType(item, BoostType.CritChance, i)) withPassive += item.passiveStats.criticalHitChance;
                }
            }

            currentMainHandCriticalHitChance = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);

        }
        else currentMainHandCriticalHitChance = 0f;

        // OFF HAND 
        if (off != null)
        {
            if(offWeaponDetails.isShield) currentOffHandCriticalHitChance = 0f; // shields don’t crit
            else
            {
                float baseFromWeapon = off.weaponStats.isMeleeWeapon ? off.weaponStats.criticalHitChance : offWeaponDetails.weaponCurrentProjectile.criticalHitChance;
                float prePassive = dexCrit + baseFromWeapon + additionalCriticalHitChanceModifier + off.weaponStats.criticalHitChanceIncrease;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritChance, i)) withPassive += item.passiveStats.criticalHitChance;
                    }
                }

                currentOffHandCriticalHitChance = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);
            }
        }
        else currentOffHandCriticalHitChance = 0f;

        // Final clamp (design cap 50%)
        currentMainHandCriticalHitChance = Mathf.Clamp((float)Math.Round(currentMainHandCriticalHitChance, 2), 0f, 0.5f);
        currentOffHandCriticalHitChance = Mathf.Clamp((float)Math.Round(currentOffHandCriticalHitChance, 2), 0f, 0.5f);
    }

    private void UpdateCurrentCriticalHitDamage(bool onStart = false)
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        WeaponDetailsSO mainWeaponDetails = (onStart && main != null) ? playerDetails.startingWeaponList[0] : main != null ? WartheonDatabase.Instance.GetWeaponDetails(main.weaponStats.weaponTitle) : null;
        WeaponDetailsSO offWeaponDetails = (onStart && off != null) ? playerDetails.startingWeaponList[1] : off != null ? WartheonDatabase.Instance.GetWeaponDetails(off.weaponStats.weaponTitle) : null;

        float feroBonus = (float)Math.Round(currentFerocityValue * 0.02f, 2);

        // --- OFF ---
        if (off != null)
        {
            if (offWeaponDetails.isShield)
            {
                currentOffHandCriticalHitDamage = 0f;
            }
            else
            {
                float baseFromWeapon = offWeaponDetails.isMeleeWeapon
                    ? off.weaponStats.criticalHitDamage
                    : offWeaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier;

                float typeBonus = offWeaponDetails.isMeleeWeapon ? additionalCriticalMeleeDamageModifier : additionalCriticalRangedDamageModifier;

                float prePassive = off.weaponStats.criticalHitDamageIncrease + feroBonus + additionalCriticalDamageModifier + typeBonus;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritDamage, i)) withPassive += item.passiveStats.criticalHitDamage;
                    }
                }

                currentOffHandCriticalHitDamage = Mathf.Max((float)Math.Round(prePassive + withPassive, 2), 0.5f);
            }

            // --- MAIN ---
            if (main != null)
            {
                float baseFromWeapon = mainWeaponDetails.isMeleeWeapon
                    ? main.weaponStats.criticalHitDamage
                    : mainWeaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier;

                float typeBonus = mainWeaponDetails.isMeleeWeapon ? additionalCriticalMeleeDamageModifier : additionalCriticalRangedDamageModifier;

                float prePassive = main.weaponStats.criticalHitDamageIncrease + feroBonus + additionalCriticalDamageModifier + typeBonus;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritDamage, i)) withPassive += item.passiveStats.criticalHitDamage;
                    }
                }

                currentMainHandCriticalHitDamage = Mathf.Max((float)Math.Round(prePassive + withPassive, 2), 0.5f);
            }
            else currentMainHandCriticalHitDamage = 0f;
        }
        else
        {
            // --- MAIN ---
            if (main != null)
            {
                float baseFromWeapon = mainWeaponDetails.isMeleeWeapon ? main.weaponStats.criticalHitDamage
                    : mainWeaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier;

                float typeBonus = mainWeaponDetails.isMeleeWeapon ? additionalCriticalMeleeDamageModifier : additionalCriticalRangedDamageModifier;

                float prePassive = main.weaponStats.criticalHitDamageIncrease + feroBonus + additionalCriticalDamageModifier + typeBonus;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritDamage, i)) withPassive += item.passiveStats.criticalHitDamage;
                    }
                }

                currentMainHandCriticalHitDamage = Mathf.Max((float)Math.Round(prePassive + withPassive, 2), 0.5f);
            }
            else currentMainHandCriticalHitDamage = 0f;

            currentOffHandCriticalHitDamage = 0f;
        }
    }

    public void UpdateSecondaryDamageValues()
    {
        UpdateDamageVsLowHealthValue();
        UpdateLifeStealValue();
    }

    public void UpdateDamageVsLowHealthValue()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        int dmgVsLowIncrease = 0;
        if (main != null) dmgVsLowIncrease += main.weaponStats.damageVsLowHealthEnemies;
        if (off != null) dmgVsLowIncrease += off.weaponStats.damageVsLowHealthEnemies;

        int baseFromStr = Mathf.RoundToInt(currentStrengthValue * 0.05f);
        int prePassive = additionalLifeStealModifier + dmgVsLowIncrease + baseFromStr;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.AttackVsLowHealthEnemies, i)) withPassive += item.passiveStats.damageVsLowHealthEnemies;
            }
        }

        currentDamageVsLowHealthModifierValue = prePassive + withPassive;       
    }

    public void UpdateLifeStealValue()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        int lifeStealIncrease = 0;
        if (main != null) lifeStealIncrease += main.weaponStats.lifeStealAmount;
        if (off != null) lifeStealIncrease += off.weaponStats.lifeStealAmount;

        int baseFromFer = Mathf.RoundToInt(currentFerocityValue * 0.5f / 100f);
        int prePassive =  additionalLifeStealModifier + lifeStealIncrease + baseFromFer;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.LifeSteal, i)) withPassive += item.passiveStats.lifeStealAmount;
            }
        }

        currentLifeStealValue = prePassive + withPassive;
    }

    public void UpdateSkillAndStatusModifierValues()
    {
        UpdateSkillCooldown();
        UpdateSkillDuration();
        UpdateStatusInflictValues();
    }

    private void UpdateSkillCooldown()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float skillCooldownIncrease = 0f;
        if (main != null) skillCooldownIncrease += main.weaponStats.skillCooldown;
        if (off != null) skillCooldownIncrease += off.weaponStats.skillCooldown;

        float prePassive = additionalSkillCoolDownModifier + skillCooldownIncrease;

        float withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.SkillCooldown, i)) withPassive += item.passiveStats.skillCooldown;
            }
        }

        currentSkillCooldownReducer = prePassive + withPassive;
    }

    private void UpdateSkillDuration()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float skillDurationIncrease = 0f;
        if (main != null) skillDurationIncrease += main.weaponStats.skillCooldown;
        if (off != null) skillDurationIncrease += off.weaponStats.skillCooldown;

        float prePassive = additionalSkillCoolDownModifier + skillDurationIncrease;

        float withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.SkillDuration, i)) withPassive += item.passiveStats.skillDuration;
            }
        }

        currentSkillDurationModifier = prePassive + withPassive;
    }

    private void UpdateStatusInflictValues()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float poisonInflictValue = 0f, bleedInflictValue = 0f, rootInflictValue = 0f, stunInflictValue = 0f, curseInflictValue = 0f, fearInflictValue = 0f;
        float revealInflictValue = 0f, paralyzeInflictValue = 0f, burnInflictValue = 0f, freezeInflictValue = 0f, blindInflictValue = 0f, slowInflictValue = 0f;

        for (int i = 1; i < Enum.GetValues(typeof(StatusEffectType)).Length; i++)
        {
            StatusEffectType statusEffectType = (StatusEffectType)i;

            switch (statusEffectType)
            {
                case StatusEffectType.Poison:
                    if (main != null) poisonInflictValue += main.weaponStats.additionalPoisonChance;
                    if (off != null) poisonInflictValue += off.weaponStats.additionalPoisonChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) poisonInflictValue += item.passiveStats.additionalPoisonChance;
                        }
                    }
                    break;
                case StatusEffectType.Bleed:
                    if (main != null) bleedInflictValue += main.weaponStats.additionalBleedChance;
                    if (off != null) bleedInflictValue += off.weaponStats.additionalBleedChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) bleedInflictValue += item.passiveStats.additionalBleedChance;
                        }
                    }
                    break;
                case StatusEffectType.Root:
                    if (main != null) rootInflictValue += main.weaponStats.additionalRootChance;
                    if (off != null) rootInflictValue += off.weaponStats.additionalRootChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) rootInflictValue += item.passiveStats.additionalRootChance;
                        }
                    }
                    break;
                case StatusEffectType.Stun:
                    if (main != null) stunInflictValue += main.weaponStats.additionalStunChance;
                    if (off != null) stunInflictValue += off.weaponStats.additionalStunChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) stunInflictValue += item.passiveStats.additionalStunChance;
                        }
                    }
                    break;
                case StatusEffectType.Curse:
                    if (main != null) curseInflictValue += main.weaponStats.additionalCurseChance;
                    if (off != null) curseInflictValue += off.weaponStats.additionalCurseChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) curseInflictValue += item.passiveStats.additionalCurseChance;
                        }
                    }
                    break;
                case StatusEffectType.Fear:
                    if (main != null) fearInflictValue += main.weaponStats.additionalFearChance;
                    if (off != null) fearInflictValue += off.weaponStats.additionalFearChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) fearInflictValue += item.passiveStats.additionalFearChance;
                        }
                    }
                    break;
                case StatusEffectType.Reveal:
                    if (main != null) revealInflictValue += main.weaponStats.additionalRevealChance;
                    if (off != null) revealInflictValue += off.weaponStats.additionalRevealChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) revealInflictValue += item.passiveStats.additionalRevealChance;
                        }
                    }
                    break;
                case StatusEffectType.Paralyze:
                    if (main != null) paralyzeInflictValue += main.weaponStats.additionalParalyzeChance;
                    if (off != null) paralyzeInflictValue += off.weaponStats.additionalParalyzeChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) paralyzeInflictValue += item.passiveStats.additionalParalyzeChance;
                        }
                    }
                    break;
                case StatusEffectType.Burn:
                    if (main != null) burnInflictValue += main.weaponStats.additionalBurnChance;
                    if (off != null) burnInflictValue += off.weaponStats.additionalBurnChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) burnInflictValue += item.passiveStats.additionalBurnChance;
                        }
                    }
                    break;
                case StatusEffectType.Freeze:
                    if (main != null) freezeInflictValue += main.weaponStats.additionalFreezeChance;
                    if (off != null) freezeInflictValue += off.weaponStats.additionalFreezeChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) freezeInflictValue += item.passiveStats.additionalFreezeChance;
                        }
                    }
                    break;
                case StatusEffectType.Blind:
                    if (main != null) blindInflictValue += main.weaponStats.additionalBlindChance;
                    if (off != null) blindInflictValue += off.weaponStats.additionalBlindChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) blindInflictValue += item.passiveStats.additionalBlindChance;
                        }
                    }
                    break;
                case StatusEffectType.Slow:
                    if (main != null) slowInflictValue += main.weaponStats.additionalSlowChance;
                    if (off != null) slowInflictValue += off.weaponStats.additionalSlowChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) slowInflictValue += item.passiveStats.additionalSlowChance;
                        }
                    }
                    break;
                default:
                    break;
            }
        }

        additionalPoisonChance = poisonInflictValue;
        additionalBleedChance = bleedInflictValue;
        additionalRootChance = rootInflictValue;
        additionalStunChance = stunInflictValue;
        additionalCurseChance = curseInflictValue;
        additionalFearChance = fearInflictValue;
        additionalRevealChance = revealInflictValue;
        additionalParalyzeChance = paralyzeInflictValue;
        additionalBurnChance = burnInflictValue;
        additionalFreezeChance = freezeInflictValue;
        additionalBlindChance = blindInflictValue;
        additionalSlowChance = slowInflictValue;
    }

    public void UpdateBlockValue()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float baseShieldBlock = 0f;
        if (off != null && off.weaponStats.weaponClass == WeaponClass.Shield) baseShieldBlock = Mathf.Max(0f, off.weaponStats.blockChance);

        float weaponBlockMods = 0f;
        if (main != null) weaponBlockMods += Mathf.Max(0f, main.weaponStats.blockChanceIncrease);
        if (off != null) weaponBlockMods += Mathf.Max(0f, off.weaponStats.blockChanceIncrease);

        float prePassive = baseShieldBlock + additionalBlockModifier + weaponBlockMods;
        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.BlockChance, i)) withPassive += item.passiveStats.blockChance;
            }
        }

        currentBlockValue = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);
    }

    public void UpdateDodgeValue()
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        float weaponDodge = 0f;
        if (main != null) weaponDodge += main.weaponStats.dodgeChanceIncrease;
        if (off != null) weaponDodge += off.weaponStats.dodgeChanceIncrease;

        float baseFromAgi = (float)Math.Round(currentAgilityValue * 0.15f / 100f, 2);
        float prePassive = baseFromAgi + additionalDodgeRateModifier + weaponDodge;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.DodgeChance, i)) withPassive += item.passiveStats.dodgeChance;
            }
        }

        currentDodgeValue = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);
    }

    /// <summary>
    /// Set player health
    /// </summary>
    public void UpdatePlayerHealth(int healthIncrease, bool shouldHealthFilled, bool isMaxHealthChanged, bool onStart = false)
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        int weaponAdd = 0;
        if (main != null) weaponAdd += main.weaponStats.increasedMaxHealth;
        if (off != null) weaponAdd += off.weaponStats.increasedMaxHealth;

        int baseMax = 120 + currentConstitutionValue * 15 + weaponAdd;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.HealthIncrease, i)) withPassive += item.passiveStats.increasedMaxHealth;
            }
        }

        int newMaxHealth = baseMax + withPassive;
        health.SetMaximumHealth(newMaxHealth, shouldHealthFilled);
    }

    /// <summary>
    /// Set player mana
    /// </summary>
    public void UpdatePlayerMana(int manaIncrease, bool shouldManaFilled, bool isMaxManaChanged, bool onStart = false)
    {
        Weapon main = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0];
        Weapon off = weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1];

        int weaponAdd = 0;
        if (main != null) weaponAdd += main.weaponStats.increasedMaxMana;
        if (off != null) weaponAdd += off.weaponStats.increasedMaxMana;

        int baseMax = 20 + currentWillpowerValue * 20 + weaponAdd;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveStats.passiveItemType == PassiveItemType.None) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.ManaIncrease, i)) withPassive += item.passiveStats.increasedMaxMana;
            }
        }

        int newMaxMana = baseMax + withPassive;
        mana.SetMaximumMana(newMaxMana, shouldManaFilled);
    }

    public void RecalculateSecondaryStats(bool shouldHealthFilled = false)
    {
        UpdatePlayerHealth(0, false, shouldHealthFilled);
        UpdatePlayerMana(0, false, shouldHealthFilled);
        UpdateArmorValues();
        UpdateDamageValues();
        UpdateAttackRatingAndCriticalValues();
        UpdateBlockAndDodgeValues();
        UpdateSpeedAndAttackCooldownValues();
        UpdateResistanceValues();
        UpdateSecondaryDamageValues();
        UpdateSkillAndStatusModifierValues();
    }

    public bool HasPlayerStrongNegativeStatusEffectExcludingHealth()
    {
        bool hasnegativeStatusEffect = isFeared || isChilled || isBlind ||  isSlowed || moveStatus != MoveStatus.Idle;

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

    /// <summary>
    /// Enable the player movement
    /// </summary>
    public void EnablePlayer()
    {
        UpdateSpeedValue();
    }

    /// <summary>
    /// Disable the player movement
    /// </summary>
    public void DisablePlayer()
    {
        movementByForce.moveSpeed = 0f;
        idle.StopVelocity();
        animatePlayer.ResetAnimatonParameters();
        animator.SetBool(Settings.isIdle, true);
    }

    private bool HasBoostType(PassiveItem passiveItem, BoostType boostType, int boostIndex)
    {
        switch (boostIndex)
        {
            case 1: // Unique boost
                if (passiveItem.passiveStats.baseUniqueRolled == boostType) return true;
                break;
            case 2: // Type boost
                if (passiveItem.passiveStats.baseTypeRolled == boostType) return true;
                break;
            case 3: // Enchanted boost
                if (passiveItem.passiveStats.enchantedBoostType == boostType) return true;
                break;
            case 4: // Mythic boost
                if (passiveItem.passiveStats.mythicBoostType == boostType) return true;
                break;
        }

        return false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (isHuntersReachActive && collision.CompareTag(Settings.collisionTilemap))
        { 
            Debug.Log("Player hit a wall during grapple. Cancelling...");
            playersGrapple.ReleaseGrapple(NetworkServer.active || NetworkClient.active); // Reference to hook or use event
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Vector3 position = this == null ? Vector3.zero : transform.position + new Vector3(0f, 0.8f, 0f);
        Gizmos.DrawWireSphere(position, NymarasWindveilCircleRadius);
    }
}


