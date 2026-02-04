using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

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
    public event Action<Player, PlayerDetailsSO> OnPlayerReady;
    public event Action<Player> OnLocalAuthReady;

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
        set
        {

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
    [HideInInspector] public StatusEffectAnimators statusEffectAnimators;
    [HideInInspector] public PlayerAnimationSync animSync;
    [HideInInspector] public PlayerNetworkAuthority networkAuthority;
    [HideInInspector] public PlayerNetworkState state;

    [HideInInspector] public AimDirection LastAim { get; set; }
    [HideInInspector] public AttackDirection LastAttackdir { get; set; }

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

    [HideInInspector] public bool shadowCloakEquipped;

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
        statusEffectAnimators = GetComponentInChildren<StatusEffectAnimators>();
        specialMoveParticlesSystem = GetComponentInChildren<ParticleSystem>();
        networkAuthority = GetComponent<PlayerNetworkAuthority>();
        state = GetComponent<PlayerNetworkState>();

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

        // Set starting equipment
        CreatePlayerStartingWeapons();
        CreatePlayerStartingPassiveItem();

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

            Weapon startedWeapon = new Weapon(playerDetails.startingWeaponList[i].rarity)
            {
                weaponDetails = playerDetails.startingWeaponList[i]
            };

            AddNextWeaponToPlayer(ref startedWeapon, pickingUp: false, onStart: true, onlySwitch: false, dualWieldOnStart: dualWieldOnStart,
                equipOffHand: equipOffHand);
        }
    }

    public void ReplayActiveWeaponsForListeners(int index)
    {
        Weapon main = weaponSlotSetArray[index - 1][0];
        Weapon off = weaponSlotSetArray[index - 1][1];

        if(main != null) ActivateWeapon(main, ItemSlotStatus.MainHand, index - 1, onStart: true, onSwitch: false);
        if(off != null) ActivateWeapon(off, ItemSlotStatus.OffHand, index - 1, onStart: true, onSwitch: false);
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
    /// Update weapons list if a new one acquired
    /// </summary>
    public void UpdateWieldedWeapons(ref Weapon weapon, bool pickingUp, bool onStart, WeaponDetailsSO weaponDetails)
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        if (mainHandWeapon == null)
        {
            AddNextWeaponToPlayer(ref weapon, pickingUp, onStart, false, false, equipOffHand: false);

            // Set player starting health
            UpdatePlayerHealth(0, false, true);
            UpdatePlayerMana(0, false, true);

            UpdateDamageValues();
            UpdateAttackRatingAndCriticalValues();
            UpdateBlockAndDodgeValues();
            UpdateSpeedAndAttackCooldownValues();
            UpdateResistanceValues();
            UpdateSecondaryDamageValues();

            StaticEventHandler.CallStatsChangedOnTheBookEvent();
        }
        // If inventory is full replace weapon
        else if (InventoryManager.Instance.IsInventoryFull())
        {
            AddNextWeaponToPlayer(ref weapon, pickingUp, onStart, false, false, equipOffHand: false);

            // Set player starting health
            UpdatePlayerHealth(0, false, true);
            UpdatePlayerMana(0, false, true);
            UpdateDamageValues();
            UpdateAttackRatingAndCriticalValues();
            UpdateBlockAndDodgeValues();
            UpdateSpeedAndAttackCooldownValues();
            UpdateResistanceValues();
            UpdateSecondaryDamageValues();

            StaticEventHandler.CallStatsChangedOnTheBookEvent();
        }
        // If weapon is one-handed off hand weapon
        else if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] == null && weapon.weaponDetails.wieldType == WieldType.OneHanded && 
            weapon.weaponDetails.weaponClass != WeaponClass.Spear && weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] != null && 
            weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0].weaponDetails.wieldType == WieldType.OneHanded && 
            weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0].weaponDetails.weaponClass != WeaponClass.Spear)
        {
            if (weapon == DropItem.droppedThrowingAxe)
            {
                foreach (KeyValuePair<int, ActiveUniqueSkillDetailsSO> keyValuePair in currentlyUsedActiveUniqueSkills)
                {
                    if (keyValuePair.Value.activeSkill == ActiveSkill.AxeThrow)
                    {
                        specialMovesCooldownCheckArray[keyValuePair.Key - 1] = false;
                        specialMoveRecastCountArray[keyValuePair.Key - 1] = 0;
                        specialMoveEvent.CallSpecialMoveCooldownResetEvent(ActiveSkill.AxeThrow, keyValuePair.Key);
                    }
                }
            }

            if (playerDetails.playerCharacterIndex == Character.Nyveran) AddNextWeaponToPlayer(ref weapon, pickingUp, onStart, false, false, equipOffHand: false); // Nyveran can't wield dual-wield dagger
            else AddNextWeaponToPlayer(ref weapon, pickingUp, onStart, false, false, equipOffHand: true);

            // Set player starting health
            UpdatePlayerHealth(0, false, true);
            UpdatePlayerMana(0, false, true);
            UpdateArmorValues();
            UpdateDamageValues();
            UpdateAttackRatingAndCriticalValues();
            UpdateBlockAndDodgeValues();
            UpdateSpeedAndAttackCooldownValues();
            UpdateResistanceValues();
            UpdateSecondaryDamageValues();

            StaticEventHandler.CallStatsChangedOnTheBookEvent();
        }
        else
        {
            // Add it to inventory slot
            weapon.weaponDetails = weaponDetails;
            weapon.itemSlotStatus = ItemSlotStatus.Inventory;

            int retrievedInventoryIndex = InventoryManager.Instance.PlaceItemToInventoryIndexSlot(weapon);

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
        UpdatePlayerHealth(healthIncrease: 0, shouldHealthFilled: true, isMaxHealthChanged: true, onStart: true);
        UpdatePlayerMana(manaIncrease: 0, shouldManaFilled: true, isMaxManaChanged: true, onStart: true);
        UpdateArmorValues(onStart: true);
        UpdateDamageValues();
        UpdateAttackRatingAndCriticalValues();
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
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        // Base armor from weapons (non-shield offhand contributes normally
        float mainArmor = main?.armorIncrease ?? 0f;

        float offHandNonShieldArmor = 0f;
        float shieldArmor = 0f;

        if (off != null)
        {
            if (off.weaponDetails.weaponClass == WeaponClass.Shield)
            {
                // Shield base armor, scaled by shield-specific modifier
                shieldArmor = off.armorIncrease * (1f + additionalShieldArmorModifier);
            }
            else
            {
                offHandNonShieldArmor = off.armorIncrease;
            }
        }

        // Armor from passives (sum each roll that is ArmorIncrease)
        float passiveArmor = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
                if (HasBoostType(item, BoostType.ArmorIncrease, i)) passiveArmor += item.armorIncrease;
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
        // MAIN-HAND
        (currentMainHandMinDamageValue, currentMainHandMaxDamageValue) = ComputeEquippedDamage(activeWeapon.GetCurrentMainHandWeapon(), isOffHand: false);

        // OFF-HAND
        (currentOffHandMinDamageValue, currentOffHandMaxDamageValue) = ComputeEquippedDamage(activeWeapon.GetCurrentOffHandWeapon(), isOffHand: true);

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

        // --- add PASSIVE FLATS (per equipped passive item) ---
        // These are additive, not multiplicative.
        var (passivePhysFlat, passiveMagFlat) = GetPassiveFlatDamageAdds();
        physMin += passivePhysFlat; physMax += passivePhysFlat;
        magMin += passiveMagFlat; magMax += passiveMagFlat;

        // --- attribute scaling (unchanged) ---
        int physAdd = 0;
        if (physMin > 0 || physMax > 0)
        {
            if (weaponDetails.isMeleeWeapon)
            {
                if (weaponDetails.weaponClass == WeaponClass.Dagger || weaponDetails.weaponClass == WeaponClass.Claw)
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
            float intCoef = weaponDetails.isMeleeWeapon ? 2f : 1.5f;
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
            if (item == null || item.passiveItemDetails == null) continue;

            // Physical
            if (HasBoostType(item, BoostType.AttackDamage, 1) ||
                HasBoostType(item, BoostType.AttackDamage, 2) ||
                HasBoostType(item, BoostType.AttackDamage, 3) ||
                HasBoostType(item, BoostType.AttackDamage, 4))
            {
                phys += Mathf.Max(0, item.physicalAttackDamageIncrease);
            }

            // Magic
            if (HasBoostType(item, BoostType.MagicDamage, 1) ||
                HasBoostType(item, BoostType.MagicDamage, 2) ||
                HasBoostType(item, BoostType.MagicDamage, 3) ||
                HasBoostType(item, BoostType.MagicDamage, 4))
            {
                mag += Mathf.Max(0, item.magicAttackDamageIncrease);
            }
        }

        return (phys, mag);
    }

    private void UpdateArmorPenetrationValue()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float armorPenIncrease = 0f;
        if (main != null) armorPenIncrease += main.armorPenetration;
        if (off != null) armorPenIncrease += off.armorPenetration;

        float prePassive = additionalArmorPenetrationModifier + armorPenIncrease;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.ArmorPenetration, i)) withPassive += item.armorPenetration;
            }
        }

        currentArmorPenetrationValue = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.35f);
    }

    private void UpdateAttackRange()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float attackRangeIncrease = 0f;
        if (main != null) attackRangeIncrease += main.attackRange;
        if (off != null) attackRangeIncrease += off.attackRange;

        float prePassive = attackRangeIncrease;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.AttackRange, i)) withPassive += item.attackRange;
            }
        }

        additionalAttackRangeModifier = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.35f);
    }

    public void UpdateAttackRatingAndCriticalValues()
    {
        UpdateCurrentAttackRatingValues();
        UpdateCurrentCriticalHitChance();
        UpdateCurrentCriticalHitDamage();
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
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float magicResistanceIncrease = 0f;

        if (main != null) magicResistanceIncrease = main.magicResistance;
        else magicResistanceIncrease = 0f;

        if (off != null) magicResistanceIncrease += off.magicResistance;
        else magicResistanceIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.MagicResistance, i)) withPassive += item.magicResistanceModifier;
            }
        }

        currentMagicResistanceValue = Mathf.Min((float)Math.Round(currentWillpowerValue * 0.01f + magicResistanceIncrease + withPassive, 2), 0.5f);
    }

    private void UpdateStatusResistance()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float statusResistanceIncrease = 0f;

        if (main != null) statusResistanceIncrease = main.statusResistanceModifier;
        else statusResistanceIncrease = 0f;

        if (off != null) statusResistanceIncrease += off.statusResistanceModifier;
        else statusResistanceIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.StatusResistance, i)) withPassive += item.statusResistanceModifier;
            }
        }

        currentStatusResistance = Mathf.Min((float)Math.Round(currentResolveValue * 0.01f + statusResistanceIncrease + withPassive + additionalStatusResistanceModifier, 2), 0.5f);
    }

    private void UpdateDamageReduction()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float damageReductionIncrease = 0f;

        if (main != null) damageReductionIncrease = main.damageReductionRate;
        else damageReductionIncrease = 0f;

        if (off != null) damageReductionIncrease += off.damageReductionRate;
        else damageReductionIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.DamageReduction, i)) withPassive += item.damageReductionRate;
            }
        }

        currentDamageReductionValue = Mathf.Min((float)Math.Round(currentStrengthValue * 0.005f + damageReductionIncrease + withPassive + 
            additionalDamageReductionModifier, 2), 0.35f);
    }

    public void UpdateCriticalResistance()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float criticalResistanceIncrease = 0f;

        if (main != null) criticalResistanceIncrease = main.criticalResistanceModifier;
        else criticalResistanceIncrease = 0f;

        if (off != null) criticalResistanceIncrease += off.criticalResistanceModifier;
        else criticalResistanceIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.CritResistance, i)) withPassive += item.criticalResistanceModifier;
            }
        }

        currentCriticalResistanceValue = Mathf.Min((float)Math.Round(currentResolveValue * 0.01f + criticalResistanceIncrease + withPassive + 
            additionalCriticalResistanceModifier, 2), 0.5f);
    }

    public void UpdateSpeedValue()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float weaponSpeedIncrease = 0f;

        if (main != null) weaponSpeedIncrease = main.speedIncreaseModifier;
        else weaponSpeedIncrease = 0f;

        if (off != null) weaponSpeedIncrease += off.speedIncreaseModifier;
        else weaponSpeedIncrease += 0f;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.MoveSpeed, i)) withPassive += item.speedIncreaseModifier;
            }
        }

        movementByForce.moveSpeed = movementByForce.movementDetails.baseMaxMoveSpeed + currentAgilityValue * 0.25f +
            additionalSpeedModifier + weaponSpeedIncrease + withPassive;
    }

    public void UpdateAttackCooldown()
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        // Recompute weapon contribution (seconds; >= 0)
        float newWeaponContribution = 0f;

        if (mainHandWeapon != null) newWeaponContribution += Mathf.Max(0f, mainHandWeapon.attackCooldownModifier);

        // Ignore shields (or any off-hand that shouldn't affect cooldown)
        if (offHandWeapon != null && offHandWeapon.weaponDetails.weaponClass != WeaponClass.Shield)
            newWeaponContribution += Mathf.Max(0f, offHandWeapon.attackCooldownModifier);

        // --- 2) Recompute PASSIVE contribution (seconds; >= 0, summed per matching slot) ---
        float newPassiveContribution = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            // Each matching slot adds item.attackCooldown (your roll is additive, not %)
            if (HasBoostType(item, BoostType.AttackCooldown, 1)) newPassiveContribution += Mathf.Max(0f, item.attackCooldown);
            if (HasBoostType(item, BoostType.AttackCooldown, 2)) newPassiveContribution += Mathf.Max(0f, item.attackCooldown);
            if (HasBoostType(item, BoostType.AttackCooldown, 3)) newPassiveContribution += Mathf.Max(0f, item.attackCooldown);
            if (HasBoostType(item, BoostType.AttackCooldown, 4)) newPassiveContribution += Mathf.Max(0f, item.attackCooldown);
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

    /// <summary>
    /// Add a weapon to the player weapon list
    /// </summary>
    public void AddNextWeaponToPlayer(ref Weapon weapon, bool pickingUp, bool onStart, bool onlySwitch, bool dualWieldOnStart = false, 
        bool equipOffHand = false) // <- pass the real instance for pickups/switches
    {

        if (onStart)
        {
            // Starting gear: create a fresh instance using SO.rarity
            CreateStartingWeaponInstance(ref weapon, equipOffHand ? ItemSlotStatus.OffHand : ItemSlotStatus.MainHand);
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

    private Weapon CreateStartingWeaponInstance(ref Weapon weapon, ItemSlotStatus slot)
    {
        // Create with so rarity only for start gear
        if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
        {
            // Shields: keep the instance minimal; other runtime stats are irrelevant
            weapon.baseUniqueRolled = weapon.weaponDetails.baseUniqueModifier;
            weapon.baseTypeRolled = weapon.weaponDetails.baseTypeModifier;

            WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.baseUniqueRolled, weapon.weaponDetails);
            WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.baseTypeRolled, weapon.weaponDetails);

            return weapon;
        }

        weapon.itemSlotStatus = slot;
        weapon.criticalHitChanceIncrease = weapon.weaponDetails.criticalHitChance;
        weapon.criticalHitDamageIncrease = weapon.weaponDetails.criticalHitDamageMultiplier;
        weapon.baseUniqueRolled = weapon.weaponDetails.baseUniqueModifier;
        weapon.baseTypeRolled = weapon.weaponDetails.baseTypeModifier;

        WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.baseUniqueRolled, weapon.weaponDetails);
        WeaponDropGenerator.SetWeaponModifier(ref weapon, weapon.baseTypeRolled, weapon.weaponDetails);

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
            if (activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.wieldType == WieldType.OneHanded)
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
            else if (activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.wieldType == WieldType.TwoHanded)
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

    public void UpdateCurrentAttackRatingValues()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float attackRatingModifier = 0f;

        if (off != null)
        {
            if (main != null)
            {
                attackRatingModifier = ((main.weaponDetails.weaponAttackRating + off.weaponDetails.weaponAttackRating +
                      main.attackRatingIncrease + off.attackRatingIncrease) / 2f + additionalAttackRatingModifier) * 0.85f;
            }
            else attackRatingModifier = 0f;
        }
        else
        {
            if (main != null) attackRatingModifier = main.weaponDetails.weaponAttackRating + additionalAttackRatingModifier + main.attackRatingIncrease;
            else attackRatingModifier = 0f;
        }

        float baseFromStats = 0.2f + ((currentDexterityValue / 3f + currentFerocityValue / 3f + currentStrengthValue / 4f) / 100f);
        float prePassive = baseFromStats + attackRatingModifier - blindModifier;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.AttackRating, i)) withPassive += item.attackRating;
            }
        }

        currentAttackRatingValue = (float)Math.Round(prePassive + withPassive, 2);
    }

    public void UpdateCurrentCriticalHitChance()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float dexCrit = (float)Math.Round(currentDexterityValue * 0.75f / 100f, 2);

        // MAIN HAND
        if(main != null)
        {
            float baseFromWeapon = main.weaponDetails.isMeleeWeapon ? main.weaponDetails.criticalHitChance : main.weaponDetails.weaponCurrentProjectile.criticalHitChance;
            float prePassive = dexCrit + baseFromWeapon + additionalCriticalHitChanceModifier + main.criticalHitChanceIncrease;

            float withPassive = 0f;

            foreach (var kvp in equippedPassiveItems)
            {
                var item = kvp.Value;
                if (item == null || item.passiveItemDetails == null) continue;

                for (int i = 1; i <= 4; i++)
                {
                    if (HasBoostType(item, BoostType.CritChance, i)) withPassive += item.criticalHitChance;
                }
            }

            currentMainHandCriticalHitChance = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);

        }
        else currentMainHandCriticalHitChance = 0f;

        // OFF HAND 
        if (off != null)
        {
            if(off.weaponDetails.isShield) currentOffHandCriticalHitChance = 0f; // shields don’t crit
            else
            {
                float baseFromWeapon = off.weaponDetails.isMeleeWeapon ? off.weaponDetails.criticalHitChance : off.weaponDetails.weaponCurrentProjectile.criticalHitChance;
                float prePassive = dexCrit + baseFromWeapon + additionalCriticalHitChanceModifier + off.criticalHitChanceIncrease;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveItemDetails == null) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritChance, i)) withPassive += item.criticalHitChance;
                    }
                }

                currentOffHandCriticalHitChance = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);
            }
        }
        else currentOffHandCriticalHitChance = 0f;

        // --- Shadow Cloak conditional tweak (after passives, before final clamp) ---
        if (shadowCloakEquipped)
        {
            if (main != null && off != null &&
                equippedPassiveItems.TryGetValue(PassiveItemSlotName.Back, out PassiveItem item) && item != null &&
                item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
            {
                bool dualDaggers = main.weaponDetails.weaponClass == WeaponClass.Dagger && off.weaponDetails.weaponClass == WeaponClass.Dagger;
                bool dualClaws = main.weaponDetails.weaponClass == WeaponClass.Claw && off.weaponDetails.weaponClass == WeaponClass.Claw;
                bool isDualBonus = dualDaggers || dualClaws;

                float tweak = isDualBonus ? +0.05f : -0.05f;
                currentMainHandCriticalHitChance += tweak;
                currentOffHandCriticalHitChance += tweak;
            }
        }

        // Final clamp (design cap 50%)
        currentMainHandCriticalHitChance = Mathf.Clamp((float)Math.Round(currentMainHandCriticalHitChance, 2), 0f, 0.5f);
        currentOffHandCriticalHitChance = Mathf.Clamp((float)Math.Round(currentOffHandCriticalHitChance, 2), 0f, 0.5f);
    }

    private void UpdateCurrentCriticalHitDamage()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float feroBonus = (float)Math.Round(currentFerocityValue * 0.02f, 2);

        // --- OFF ---
        if (off != null)
        {
            if (off.weaponDetails.isShield)
            {
                currentOffHandCriticalHitDamage = 0f;
            }
            else
            {
                float baseFromWeapon = off.weaponDetails.isMeleeWeapon
                    ? off.weaponDetails.criticalHitDamageMultiplier
                    : off.weaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier;

                float typeBonus = off.weaponDetails.isMeleeWeapon ? additionalCriticalMeleeDamageModifier : additionalCriticalRangedDamageModifier;

                float prePassive = off.criticalHitDamageIncrease + feroBonus + additionalCriticalDamageModifier + typeBonus;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveItemDetails == null) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritDamage, i)) withPassive += item.criticalHitDamage;
                    }
                }

                currentOffHandCriticalHitDamage = Mathf.Max((float)Math.Round(prePassive + withPassive, 2), 0.5f);
            }

            // --- MAIN ---
            if (main != null)
            {
                float baseFromWeapon = main.weaponDetails.isMeleeWeapon
                    ? main.weaponDetails.criticalHitDamageMultiplier
                    : main.weaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier;

                float typeBonus = main.weaponDetails.isMeleeWeapon ? additionalCriticalMeleeDamageModifier : additionalCriticalRangedDamageModifier;

                float prePassive = main.criticalHitDamageIncrease + feroBonus + additionalCriticalDamageModifier + typeBonus;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveItemDetails == null) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritDamage, i)) withPassive += item.criticalHitDamage;
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
                float baseFromWeapon = main.weaponDetails.isMeleeWeapon
                    ? main.weaponDetails.criticalHitDamageMultiplier
                    : main.weaponDetails.weaponCurrentProjectile.criticalHitDamageMultiplier;

                float typeBonus = main.weaponDetails.isMeleeWeapon ? additionalCriticalMeleeDamageModifier : additionalCriticalRangedDamageModifier;

                float prePassive = main.criticalHitDamageIncrease + feroBonus + additionalCriticalDamageModifier + typeBonus;

                float withPassive = 0f;

                foreach (var kvp in equippedPassiveItems)
                {
                    var item = kvp.Value;
                    if (item == null || item.passiveItemDetails == null) continue;

                    for (int i = 1; i <= 4; i++)
                    {
                        if (HasBoostType(item, BoostType.CritDamage, i)) withPassive += item.criticalHitDamage;
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
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        int dmgVsLowIncrease = 0;
        if (main != null) dmgVsLowIncrease += main.damageVsLowHealthEnemies;
        if (off != null) dmgVsLowIncrease += off.damageVsLowHealthEnemies;

        int baseFromStr = Mathf.RoundToInt(currentStrengthValue * 0.05f);
        int prePassive = additionalLifeStealModifier + dmgVsLowIncrease + baseFromStr;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.AttackVsLowHealthEnemies, i)) withPassive += item.damageVsLowHealthEnemies;
            }
        }

        currentDamageVsLowHealthModifierValue = prePassive + withPassive;       
    }

    public void UpdateLifeStealValue()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        int lifeStealIncrease = 0;
        if (main != null) lifeStealIncrease += main.lifeStealAmount;
        if (off != null) lifeStealIncrease += off.lifeStealAmount;

        int baseFromFer = Mathf.RoundToInt(currentFerocityValue * 0.5f / 100f);
        int prePassive =  additionalLifeStealModifier + lifeStealIncrease + baseFromFer;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.LifeSteal, i)) withPassive += item.lifeStealAmount;
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
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float skillCooldownIncrease = 0f;
        if (main != null) skillCooldownIncrease += main.skillCooldown;
        if (off != null) skillCooldownIncrease += off.skillCooldown;

        float prePassive = additionalSkillCoolDownModifier + skillCooldownIncrease;

        float withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.SkillCooldown, i)) withPassive += item.skillCooldown;
            }
        }

        currentSkillCooldownReducer = prePassive + withPassive;
    }

    private void UpdateSkillDuration()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float skillDurationIncrease = 0f;
        if (main != null) skillDurationIncrease += main.skillCooldown;
        if (off != null) skillDurationIncrease += off.skillCooldown;

        float prePassive = additionalSkillCoolDownModifier + skillDurationIncrease;

        float withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.SkillDuration, i)) withPassive += item.skillDuration;
            }
        }

        currentSkillDurationModifier = prePassive + withPassive;
    }

    private void UpdateStatusInflictValues()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float poisonInflictValue = 0f, bleedInflictValue = 0f, rootInflictValue = 0f, stunInflictValue = 0f, curseInflictValue = 0f, fearInflictValue = 0f;
        float revealInflictValue = 0f, paralyzeInflictValue = 0f, burnInflictValue = 0f, freezeInflictValue = 0f, blindInflictValue = 0f, slowInflictValue = 0f;

        for (int i = 1; i < Enum.GetValues(typeof(StatusEffectType)).Length; i++)
        {
            StatusEffectType statusEffectType = (StatusEffectType)i;

            switch (statusEffectType)
            {
                case StatusEffectType.Poison:
                    if (main != null) poisonInflictValue += main.additionalPoisonChance;
                    if (off != null) poisonInflictValue += off.additionalPoisonChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) poisonInflictValue += item.additionalPoisonChance;
                        }
                    }
                    break;
                case StatusEffectType.Bleed:
                    if (main != null) bleedInflictValue += main.additionalBleedChance;
                    if (off != null) bleedInflictValue += off.additionalBleedChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) bleedInflictValue += item.additionalBleedChance;
                        }
                    }
                    break;
                case StatusEffectType.Root:
                    if (main != null) rootInflictValue += main.additionalRootChance;
                    if (off != null) rootInflictValue += off.additionalRootChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) rootInflictValue += item.additionalRootChance;
                        }
                    }
                    break;
                case StatusEffectType.Stun:
                    if (main != null) stunInflictValue += main.additionalStunChance;
                    if (off != null) stunInflictValue += off.additionalStunChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) stunInflictValue += item.additionalStunChance;
                        }
                    }
                    break;
                case StatusEffectType.Curse:
                    if (main != null) curseInflictValue += main.additionalCurseChance;
                    if (off != null) curseInflictValue += off.additionalCurseChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) curseInflictValue += item.additionalCurseChance;
                        }
                    }
                    break;
                case StatusEffectType.Fear:
                    if (main != null) fearInflictValue += main.additionalFearChance;
                    if (off != null) fearInflictValue += off.additionalFearChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) fearInflictValue += item.additionalFearChance;
                        }
                    }
                    break;
                case StatusEffectType.Reveal:
                    if (main != null) revealInflictValue += main.additionalRevealChance;
                    if (off != null) revealInflictValue += off.additionalRevealChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) revealInflictValue += item.additionalRevealChance;
                        }
                    }
                    break;
                case StatusEffectType.Paralyze:
                    if (main != null) paralyzeInflictValue += main.additionalParalyzeChance;
                    if (off != null) paralyzeInflictValue += off.additionalParalyzeChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) paralyzeInflictValue += item.additionalParalyzeChance;
                        }
                    }
                    break;
                case StatusEffectType.Burn:
                    if (main != null) burnInflictValue += main.additionalBurnChance;
                    if (off != null) burnInflictValue += off.additionalBurnChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) burnInflictValue += item.additionalBurnChance;
                        }
                    }
                    break;
                case StatusEffectType.Freeze:
                    if (main != null) freezeInflictValue += main.additionalFreezeChance;
                    if (off != null) freezeInflictValue += off.additionalFreezeChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) freezeInflictValue += item.additionalFreezeChance;
                        }
                    }
                    break;
                case StatusEffectType.Blind:
                    if (main != null) blindInflictValue += main.additionalBlindChance;
                    if (off != null) blindInflictValue += off.additionalBlindChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) blindInflictValue += item.additionalBlindChance;
                        }
                    }
                    break;
                case StatusEffectType.Slow:
                    if (main != null) slowInflictValue += main.additionalSlowChance;
                    if (off != null) slowInflictValue += off.additionalSlowChance;

                    foreach (var kvp in equippedPassiveItems)
                    {
                        var item = kvp.Value;
                        if (item == null || item.passiveItemDetails == null) continue;

                        for (int j = 1; j <= 4; j++)
                        {
                            if (HasBoostType(item, BoostType.StatusInflict, i)) slowInflictValue += item.additionalSlowChance;
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
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float baseShieldBlock = 0f;
        if (off != null && off.weaponDetails.weaponClass == WeaponClass.Shield) baseShieldBlock = Mathf.Max(0f, off.weaponDetails.blockChance);

        float weaponBlockMods = 0f;
        if (main != null) weaponBlockMods += Mathf.Max(0f, main.blockChanceIncrease);
        if (off != null) weaponBlockMods += Mathf.Max(0f, off.blockChanceIncrease);

        float prePassive = baseShieldBlock + additionalBlockModifier + weaponBlockMods;
        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.BlockChance, i)) withPassive += item.blockChance;
            }
        }

        currentBlockValue = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);
    }

    public void UpdateDodgeValue()
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        float weaponDodge = 0f;
        if (main != null) weaponDodge += main.dodgeChanceIncrease;
        if (off != null) weaponDodge += off.dodgeChanceIncrease;

        float baseFromAgi = (float)Math.Round(currentAgilityValue * 0.15f / 100f, 2);
        float prePassive = baseFromAgi + additionalDodgeRateModifier + weaponDodge;

        float withPassive = 0f;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.DodgeChance, i)) withPassive += item.dodgeChance;
            }
        }

        currentDodgeValue = Mathf.Min((float)Math.Round(prePassive + withPassive, 2), 0.5f);
    }

    /// <summary>
    /// Set player health
    /// </summary>
    public void UpdatePlayerHealth(int healthIncrease, bool shouldHealthFilled, bool isMaxHealthChanged, bool onStart = false)
    {
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        int weaponAdd = 0;
        if (main != null) weaponAdd += main.increasedMaxHealth;
        if (off != null) weaponAdd += off.increasedMaxHealth;

        int baseMax = 120 + currentConstitutionValue * 15 + weaponAdd;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.HealthIncrease, i)) withPassive += item.increasedMaxHealth;
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
        Weapon main = activeWeapon.GetCurrentMainHandWeapon();
        Weapon off = activeWeapon.GetCurrentOffHandWeapon();

        int weaponAdd = 0;
        if (main != null) weaponAdd += main.increasedMaxMana;
        if (off != null) weaponAdd += off.increasedMaxMana;

        int baseMax = 20 + currentWillpowerValue * 20 + weaponAdd;

        int withPassive = 0;

        foreach (var kvp in equippedPassiveItems)
        {
            var item = kvp.Value;
            if (item == null || item.passiveItemDetails == null) continue;

            for (int i = 1; i <= 4; i++)
            {
                if (HasBoostType(item, BoostType.ManaIncrease, i)) withPassive += item.increasedMaxMana;
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
                if (passiveItem.baseUniqueRolled == boostType) return true;
                break;
            case 2: // Type boost
                if (passiveItem.baseTypeRolled == boostType) return true;
                break;
            case 3: // Enchanted boost
                if (passiveItem.enchantedBoostType == boostType) return true;
                break;
            case 4: // Mythic boost
                if (passiveItem.mythicBoostType == boostType) return true;
                break;
        }

        return false;
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


