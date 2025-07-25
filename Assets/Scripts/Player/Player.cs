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
[RequireComponent(typeof(SelectedActiveItem))]
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
[RequireComponent(typeof(Coins))]
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
    [HideInInspector] public SetActiveItemEvent setActiveItemEvent;
    [HideInInspector] public SetPassiveItemEvent setPassiveItemEvent; 
    [HideInInspector] public AimWeapon aimWeapon;
    [HideInInspector] public ActiveWeapon activeWeapon;
    [HideInInspector] public SelectedActiveItem selectedActiveItem;
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
    [HideInInspector] public Coins coins;
    [HideInInspector] public Idle idle;
    [HideInInspector] public MovementByForce movementByForce;
    [HideInInspector] public MovementToPositionEvent movementToPositionEvent;
    [HideInInspector] public StatusManager statusManager;
    [HideInInspector] public DamageTracker damageTracker;
    [HideInInspector] public SpringJoint2D springJoint2D;
    [HideInInspector] public Projectile playersGrapple;
    [HideInInspector] public Projectile playersThrowingAxe;

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
    [HideInInspector] public float currentFireResistanceValue;
    [HideInInspector] public float currentWaterResistanceValue;
    [HideInInspector] public float currentAirResistanceValue;
    [HideInInspector] public float currentEarthResistanceValue;
    [HideInInspector] public float currentLightResistanceValue;
    [HideInInspector] public float currentDarkResistanceValue;

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
    // DAMAGE
    [HideInInspector] public float additionalMeleeDamageModifer = 0f;
    [HideInInspector] public float additionalRangedDamageModifier = 0f;
    [HideInInspector] public float additionalElementalDamageModifier = 0f;

    // ARMOR PENETRATION
    [HideInInspector] public float additionalArmorPenetrationModifier;

    // ATTACK COOLDOWN
    [HideInInspector] public float additionalMeleeAttackCoolDownModifier = 0f;
    [HideInInspector] public float additionalRangedAttackCoolDownModifier = 0f;

    // SKILL COOLDOWN
    [HideInInspector] public float additionalSkillCoolDownModifier = 0f;

    // SPEED
    [HideInInspector] public float additionalSpeedModifier;

    // CRITICAL CHANCE
    [HideInInspector] public float additionalMeleeCriticalHitChanceModifier = 0f;

    // CRITICAL DAMAGE
    [HideInInspector] public float additionalCriticalDamageOnCloakedPrecision = 0.5f;
    [HideInInspector] public float additionalCriticalDamageModifier = 0f;
    [HideInInspector] public float additionalCriticalMeleeDamageModifier = 0f;
    [HideInInspector] public float additionalCriticalRangedDamageModifier = 0f;

    // ARMOR
    [HideInInspector] public float additionalArmorModifier = 0f;
    [HideInInspector] public float acidArmorDebuffModifier = 0f;
    [HideInInspector] public float additionalShieldArmorModifier = 0f;
    [HideInInspector] public float currentShieldArmor = 0f;
    [HideInInspector] public bool isShieldCalculated = false;

    // BLOCK
    [HideInInspector] public float additionalBlockModifier = 0f;

    // DODGE
    [HideInInspector] public float additionalEvasivenessModifier = 0f;

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
    [HideInInspector] public float additionalHeadShotDamageModifier = 0f;
    [HideInInspector] public bool threeSecInvincilibityAfterTeleportEnabled = false;
    [HideInInspector] public float bloodDrainSkillAdditionalDamagePercentageModifier = 0f;
    [HideInInspector] public float barrierSkillAdditionalDurationModifier = 0f;
    [HideInInspector] public float additionalShadowStepSkillDurationModifier = 0f;
    [HideInInspector] public float additionalPenetrationSkillDamageModifier = 0f;
    [HideInInspector] public float additionalCastDurationModifier = 1f;
    [HideInInspector] public int additionalActiveItemCharge = 0;
    [HideInInspector] public int additionalCoinIncreaserModifier = 0;
    [HideInInspector] public bool additionalCoinIncreaseActivated = false;
    [HideInInspector] public float additionalLockpickingModifier = 0f;
    [HideInInspector] public float additionalDropChanceModifier = 0f;

    [HideInInspector] public float additionalNegativeStatusEffectNegatorModifier = 0f;
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
    [HideInInspector] public float whirlrendBleedChance = 0.1f;
    [HideInInspector] public int whirlrendDamage = 10;
    [HideInInspector] public bool isFeastOfWarActive;
    [HideInInspector] public float feastOfWarDrainPercentage = 0.1f;

    [HideInInspector] public bool shadowCloakEquipped;

    // STATUS EFFECT
    [HideInInspector] public bool isCursed;
    [HideInInspector] public bool isFeared;
    [HideInInspector] public bool isRevealed;
    [HideInInspector] public bool isStatic;
    [HideInInspector] public bool isWarmed;
    [HideInInspector] public bool isChilled;
    [HideInInspector] public bool isBlind;
    [HideInInspector] public bool isSlowed;

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
        setActiveItemEvent = GetComponent<SetActiveItemEvent>();
        setPassiveItemEvent = GetComponent<SetPassiveItemEvent>();
        aimWeapon = GetComponent<AimWeapon>();
        animator = GetComponent<Animator>();
        activeWeapon = GetComponent<ActiveWeapon>();
        selectedActiveItem = GetComponent<SelectedActiveItem>();
        selectedPassiveItem = GetComponent<SelectedPassiveItem>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        sortingGroup = GetComponent<SortingGroup>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        animatePlayer = GetComponent<AnimatePlayer>();
        knockback = GetComponent<Knockback>();
        coins = GetComponent<Coins>();
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

        //Create player starting weapons
        CreatePlayerStartingWeapons();

        //Create player active item
        CreatePlayerStartingActiveItem();

        //Create player active item
        CreatePlayerStartingPassiveItem();

        // Set player starting primary stats
        SetPlayerPrimaryStats();

        // Set player all active unique skills
        SetPlayerActiveUniqueSkills();

        healthEvent.CallHealthChangedEvent(health.currentHealth, 0, MeleeHand.None);
        manaEvent.CallManaChangedEvent(mana.currentMana);

        isInitialized = true;
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

    /// <summary>
    /// Set the player starting weapon
    /// </summary>
    private void CreatePlayerStartingWeapons()
    {
        if (InputManager.TutorialEnabled) return;

        // Populate weapon list from starting weapons for right hand and shield for left hand if have any
        for (int i = 0; i < playerDetails.startingWeaponList.Count; i++)
        {
            // Add weapon to right hand list of player
            bool dualWieldOnStart = false;

            if (i == 1 &&  (playerDetails.playerCharacterIndex == Character.Morven || playerDetails.playerCharacterIndex == Character.Karnag))
            {
                dualWieldOnStart = true;
            }

            AddNextWeaponToPlayer(playerDetails.startingWeaponList[i], false, true, false, dualWieldOnStart);
        }
    }

    /// <summary>
    /// Set the player starting active item
    /// </summary>
    private void CreatePlayerStartingActiveItem()
    {
        if (InputManager.TutorialEnabled) return;

        GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        activeDropItem = chestItemObject.GetComponent<DropItem>();

        activeDropItem.remainingItemCharge = playerDetails.selectedActiveItem.activeItemMaxCharge;
        AddActiveItemToPlayer(playerDetails.selectedActiveItem, activeDropItem, activeDropItem.remainingItemCharge);
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
    public void UpdateWieldedWeapons(WeaponDetailsSO weaponDetails, bool pickingUp, bool onStart)
    {
        Weapon mainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        if (mainHandWeapon == null)
        {
            AddNextWeaponToPlayer(weaponDetails, pickingUp, onStart, false);

            // Set player starting health
            UpdatePlayerHealth(0, false, false);
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
            AddNextWeaponToPlayer(weaponDetails, pickingUp, onStart, false);

            // Set player starting health
            UpdatePlayerHealth(0, false, false);
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
            AddNextWeaponToPlayer(weaponDetails, pickingUp, onStart, false, false, true);

            // Set player starting health
            UpdatePlayerHealth(0, false, false);
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
            Weapon weapon = new Weapon
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
    private void SetPlayerPrimaryStats()
    {
        currentStrengthValue = playerDetails.primaryStats.strength;
        currentConstitutionValue = playerDetails.primaryStats.constitution;
        currentDexterityValue = playerDetails.primaryStats.dexterity;
        currentIntelligenceValue = playerDetails.primaryStats.intelligence;
        currentAgilityValue = playerDetails.primaryStats.agility;
        currentWillpowerValue = playerDetails.primaryStats.willpower;
        currentResolveValue = playerDetails.primaryStats.resolve;
        currentFerocityValue = playerDetails.primaryStats.ferocity;

        // Set player starting health
        UpdatePlayerHealth(0, true, true, true);
        UpdatePlayerMana(0, true, true, true);
        UpdateArmorValues();
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

    public void UpdateArmorValues()
    {
        if (activeWeapon.GetCurrentOffHandWeapon() != null && activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
        {
            if (!isShieldCalculated)
            {
                isShieldCalculated = true;
                currentShieldArmor = activeWeapon.GetCurrentOffHandWeapon().weaponDetails.shieldArmorRate * (1 + additionalShieldArmorModifier);
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
    }

    public void UpdateDamageValues()
    {
        // Set Damage For Main Hand
        if (activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
            {
                if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasPhysicalDamage)
                {
                    // Melee and physical damage excluding dagger (such as swords, axes)
                    if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass != WeaponClass.Dagger)
                    {
                        currentMainHandMinDamageValue = (int)((activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMin + currentStrengthValue * 1.5f) *
                            (1 + additionalMeleeDamageModifer));
                        currentMainHandMaxDamageValue = (int)((activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMax + currentStrengthValue * 1.5f) * 
                            (1 + additionalMeleeDamageModifer));
                    }
                    // Melee and physical for dagger
                    else
                    {
                        currentMainHandMinDamageValue = (int)((activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMin + currentDexterityValue * 0.7f) * 
                            (1 + additionalMeleeDamageModifer));
                        currentMainHandMaxDamageValue = (int)((activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMax + currentDexterityValue * 0.7f) * 
                            (1 + additionalMeleeDamageModifer));
                    }
                }
                else
                {
                    currentMainHandMinDamageValue = (int)((activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + 
                        currentIntelligenceValue * 2) * (1 + additionalMeleeDamageModifer));
                    currentMainHandMaxDamageValue = (int)((activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + 
                        currentIntelligenceValue * 2) * (1 + additionalMeleeDamageModifer));
                }
            }
            else
            {
                // Non-melee and physical damage (such as bow)
                if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasPhysicalDamage)
                {
                    currentMainHandMinDamageValue = (int)(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + currentDexterityValue * 0.7f);
                    currentMainHandMaxDamageValue = (int)(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + currentDexterityValue * 0.7f);

                    currentMainHandMinDamageValue = (int)(currentMainHandMinDamageValue * (1 + additionalRangedDamageModifier));
                    currentMainHandMaxDamageValue = (int)(currentMainHandMaxDamageValue * (1 + additionalRangedDamageModifier));
                }
                // Non-melee and non-physical damage (such as staff)
                else
                {
                    currentMainHandMinDamageValue = (int)(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + currentIntelligenceValue * 1.5f);
                    currentMainHandMaxDamageValue = (int)(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + currentIntelligenceValue * 1.5f);

                    currentMainHandMinDamageValue = (int)(currentMainHandMinDamageValue * (1 + additionalRangedDamageModifier));
                    currentMainHandMaxDamageValue = (int)(currentMainHandMaxDamageValue * (1 + additionalRangedDamageModifier));
                }
            }
        }
        else if (activeWeapon.GetCurrentMainHandWeapon() == null)
        {
            currentMainHandMinDamageValue = 0;
            currentMainHandMaxDamageValue = 0;
        }

        // Set Damage For Off-Hand
        if (activeWeapon.GetCurrentOffHandWeapon() != null)
        {
            if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.isMeleeWeapon)
            {
                if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasPhysicalDamage)
                {
                    // Melee and physical damage excluding dagger (such as swords, axes)
                    if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass != WeaponClass.Dagger)
                    {
                        currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMin + currentStrengthValue * 1.5f
                            * (1 + additionalMeleeDamageModifer)) * 0.6f, 2);
                        currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMax + currentStrengthValue * 1.5f
                            * (1 + additionalMeleeDamageModifer)) * 0.6f, 2);
                    }
                    // Melee and physical for dagger
                    else
                    {
                        currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMin + currentDexterityValue * 0.7f
                            * (1 + additionalMeleeDamageModifer)) * 0.6f, 2);
                        currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMax + currentDexterityValue * 0.7f
                            * (1 + additionalMeleeDamageModifer)) * 0.6f, 2);
                    }
                }
                else
                {
                    currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + currentIntelligenceValue * 2
                        * (1 + additionalMeleeDamageModifer)) * 0.6f, 2);
                    currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + currentIntelligenceValue * 2
                        * (1 + additionalMeleeDamageModifer)) * 0.6f, 2);
                }
            }
            else
            {
                // Non-melee and physical damage (such as bow)
                if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasPhysicalDamage)
                {
                    currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + 
                        currentDexterityValue * 0.7f) * 0.6f, 2);
                    currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + 
                        currentDexterityValue * 0.7f) * 0.6f, 2);

                    currentOffHandMinDamageValue = (int)(currentOffHandMinDamageValue * (1 + additionalRangedDamageModifier));
                    currentOffHandMaxDamageValue = (int)(currentOffHandMaxDamageValue * (1 + additionalRangedDamageModifier));
                }
                // Non-melee and non-physical damage (such as staff)
                else
                {
                    if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield)
                    {
                        currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin 
                            + currentIntelligenceValue * 2) * 0.6f, 2);
                        currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax
                            + currentIntelligenceValue * 2) * 0.6f, 2);

                        currentOffHandMinDamageValue = (int)(currentOffHandMinDamageValue * (1 + additionalRangedDamageModifier));
                        currentOffHandMaxDamageValue = (int)(currentOffHandMaxDamageValue * (1 + additionalRangedDamageModifier));
                    }
                }
            }
        }
        else
        {
            currentOffHandMinDamageValue = 0;
            currentOffHandMaxDamageValue = 0;
        }
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
        movementByForce.moveSpeed = movementByForce.movementDetails.baseMaxMoveSpeed + currentAgilityValue * 0.25f + additionalSpeedModifier;
    }

    /// <summary>
    /// Add an active item to the player
    /// </summary>
    public ActiveItem AddActiveItemToPlayer(ActiveItemDetailsSO activeItemDetails, DropItem dropItem, int remainingItemCharge)
    {
        ActiveItem activeItem = new ActiveItem();

        activeItem = new ActiveItem
        {
            activeItemDetails = activeItemDetails,
            activeItemMaxCharge = activeItemDetails.activeItemMaxCharge + additionalActiveItemCharge,
            activeItemRemainingCharge = remainingItemCharge,
        };

        dropItem.boxCollider2D.enabled = false;

        // Set the added active item as active
        setActiveItemEvent.CallSelectedActiveItem(activeItem);

        // Set hasActiveDrop flag to true
        dropItem.hasActiveDrop = true; 

        // Initialize chest item
        dropItem.Initialize(activeItem, activeItemDetails.activeItemSprite, transform.position);

        // Disable some components during equipped
        dropItem.spriteRenderer.enabled = false;
        dropItem.animator.enabled = false;

        // Declare this chest item as to-be-dropped chest item
        DropItem.toBeDroppedDropItem = dropItem;
        DropItem.toBeDroppedDropItem.toBeDroppedActiveItem = activeItem;

        return activeItem;
    }

    public PassiveItem AddPassiveItemToPlayer(PassiveItemDetailsSO passiveItemDetails, DropItem dropItem = null)
    {
        PassiveItem passiveItem = new PassiveItem
        {
            passiveItemDetails = passiveItemDetails
        };

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
    /// Add a weapon to the right hand of player weapon list
    /// </summary>
    public void AddNextWeaponToPlayer(WeaponDetailsSO weaponDetails, bool pickingUp, bool onStart, bool onlySwitch, bool dualWieldOnStart = false, 
        bool equipOffHand = false)
    {
        if (equipOffHand)
        {
            // Main hand slot filled at weapon if it is not a two-handed weapon
            if (weaponDetails.wieldType == WieldType.TwoHanded) return;

            // Shield check for off-hand is already done above, so avoid duplicate check
            if (weaponDetails.weaponClass == WeaponClass.Shield) return;

            Weapon weapon = new Weapon
            {
                weaponDetails = weaponDetails,
                weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                itemSlotStatus = ItemSlotStatus.OffHand
            };

            weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] = weapon;

            ActivateWeapon(weapon, weapon.itemSlotStatus, currentWeaponSlotSetIndex, onStart, onlySwitch);
            StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
            weapon.weaponBelongingToWhichOffHandSet = currentWeaponSlotSetIndex;

            if (weaponSlotSetArray[0][1] != null && weaponSlotSetArray[1][1] != null && weaponSlotSetArray[2][1] != null) // All three set are full
            {
                offHandSlotFilled = true;
            }

            return;
        }

        if (!offHandSlotFilled)
        {
            // First check if it is a shield, if yes equip and return to avoid further checks
            if (weaponDetails.weaponClass == WeaponClass.Shield || dualWieldOnStart)
            {
                Weapon weapon = new Weapon
                {
                    weaponDetails = weaponDetails,
                    weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                    itemSlotStatus = ItemSlotStatus.OffHand
                };

                if (pickingUp)
                {
                    if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] == null)
                    {
                        if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] != null)
                        {
                            if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0].weaponDetails.wieldType != WieldType.TwoHanded)
                            {
                                weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] = weapon;
                                weapon.weaponBelongingToWhichOffHandSet = currentWeaponSlotSetIndex;                               
                                ActivateWeapon(weapon, weapon.itemSlotStatus, currentWeaponSlotSetIndex, onStart, onlySwitch);
                                StaticEventHandler.CallWeaponUnlockedEvent(weapon.weaponDetails.weaponTitle);

                                if (!onStart) // On start book ui events like Populate doesn't work due to script execution order so onStart weapon additions are excluded
                                {
                                    StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                                }
                                return;
                            }
                        }
                    }

                    if(weaponSlotSetArray[0][1] != null && weaponSlotSetArray[1][1] != null && weaponSlotSetArray[2][1] != null)
                    {
                        offHandSlotFilled = true;
                        return;
                    }
                }
                else
                {
                    if (weaponSlotSetArray[0][1] == null)
                    {
                        if (weaponSlotSetArray[0][0] != null)
                        {
                            if (weaponSlotSetArray[0][0].weaponDetails.wieldType != WieldType.TwoHanded)
                            {
                                weaponSlotSetArray[0][1] = weapon;
                                weapon.weaponBelongingToWhichOffHandSet = 1;
                                if (currentWeaponSlotSetIndex == 1)
                                {
                                    ActivateWeapon(weapon, weapon.itemSlotStatus, 1, onStart, onlySwitch);
                                }
                                if (!onStart) // On start book ui events like Populate doesn't work due to script execution order so onStart weapon additions are excluded
                                {
                                    StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                                }
                                return;
                            }
                        }
                    }
                    else if (weaponSlotSetArray[1][1] == null)
                    {
                        if (weaponSlotSetArray[1][0] != null)
                        {
                            if (weaponSlotSetArray[1][0].weaponDetails.wieldType != WieldType.TwoHanded)
                            {
                                weaponSlotSetArray[1][1] = weapon;
                                if (currentWeaponSlotSetIndex == 2)
                                {
                                    ActivateWeapon(weapon, weapon.itemSlotStatus, 2, onStart, onlySwitch);
                                }
                                weapon.weaponBelongingToWhichOffHandSet = 2;
                                StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                                return;
                            }
                        }
                    }
                    else if (weaponSlotSetArray[2][1] == null)
                    {
                        if (weaponSlotSetArray[2][0] != null)
                        {
                            if (weaponSlotSetArray[2][0].weaponDetails.wieldType != WieldType.TwoHanded)
                            {
                                weaponSlotSetArray[2][1] = weapon;
                                if (currentWeaponSlotSetIndex == 3)
                                {
                                    ActivateWeapon(weapon, weapon.itemSlotStatus, 3, onStart, onlySwitch);
                                }
                                weapon.weaponBelongingToWhichOffHandSet = 3;
                                StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                                offHandSlotFilled = true;
                                return;
                            }
                        }
                    }
                    else
                    {
                        offHandSlotFilled = true;
                        return;
                    }
                }
            }
        }

        if (!mainHandSlotFilled)
        {
            Weapon weapon = new Weapon
            {
                weaponDetails = weaponDetails,
                weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                itemSlotStatus = ItemSlotStatus.MainHand
            };

            if (weaponDetails.weaponClass != WeaponClass.Shield)
            {
                if (pickingUp)
                {
                    if (weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] == null)
                    {
                        weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] = weapon;
                        weapon.weaponBelongingToWhichMainHandSet = currentWeaponSlotSetIndex;
                        ActivateWeapon(weapon, weapon.itemSlotStatus, currentWeaponSlotSetIndex, onStart, onlySwitch);
                        StaticEventHandler.CallWeaponUnlockedEvent(weapon.weaponDetails.weaponTitle);

                        if (!onStart)
                        {
                            //StaticEventHandler.CallWeaponAddedToMainHandBook(weapon, onlySwitch);

                            StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);
                        }
                    }

                    if (weaponSlotSetArray[0][0] != null && weaponSlotSetArray[1][0] != null && weaponSlotSetArray[2][0] != null)
                    {
                        mainHandSlotFilled = true;
                        return;
                    }
                }
                else
                {
                    if (weaponSlotSetArray[0][0] == null)
                    {
                        weaponSlotSetArray[0][0] = weapon;
                        weapon.weaponBelongingToWhichMainHandSet = 1;
                        if (currentWeaponSlotSetIndex == 1)
                        {
                            ActivateWeapon(weapon, weapon.itemSlotStatus, 1, onStart, onlySwitch);
                        }
                        if (!onStart)
                        {
                            //StaticEventHandler.CallWeaponAddedToMainHandBook(weapon, onlySwitch);

                            StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);
                        }
                    }
                    else if (weaponSlotSetArray[1][0] == null)
                    {
                        weaponSlotSetArray[1][0] = weapon;
                        weapon.weaponBelongingToWhichMainHandSet = 2;
                        if (currentWeaponSlotSetIndex == 2)
                        {
                            ActivateWeapon(weapon, weapon.itemSlotStatus, 2, onStart, onlySwitch);
                        }
                        if (!onStart)
                        {
                            //StaticEventHandler.CallWeaponAddedToMainHandBook(weapon, onlySwitch);

                            StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);
                        }
                    }
                    else if (weaponSlotSetArray[2][0] == null)
                    {
                        weaponSlotSetArray[2][0] = weapon;
                        weapon.weaponBelongingToWhichMainHandSet = 3;
                        if (currentWeaponSlotSetIndex == 3)
                        {
                            ActivateWeapon(weapon, weapon.itemSlotStatus, 3, onStart, onlySwitch);
                        }
                        if (!onStart)
                        {
                            //StaticEventHandler.CallWeaponAddedToMainHandBook(weapon, onlySwitch);

                            StaticEventHandler.CallWeaponPickedUpEventForBook(weapon);
                        }
                        mainHandSlotFilled = true; // All 3 main hand slots filled at start
                    }
                }
            }
        }
        else
        {
            if (!offHandSlotFilled)
            {
                // Main hand slot filled at weapon if it is not a two-handed weapon
                if (weaponDetails.wieldType == WieldType.TwoHanded) return;

                // Shield check for off-hand is already done above, so avoid duplicate check
                if (weaponDetails.weaponClass == WeaponClass.Shield) return;

                Weapon weapon = new Weapon
                {
                    weaponDetails = weaponDetails,
                    weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                    itemSlotStatus = ItemSlotStatus.OffHand
                };

                if (weaponSlotSetArray[0][1] == null)
                {
                    weaponSlotSetArray[0][1] = weapon;
                    weapon.weaponBelongingToWhichOffHandSet = 1;
                    if (currentWeaponSlotSetIndex == 1)
                    {
                        ActivateWeapon(weapon, weapon.itemSlotStatus, 1, onStart, onlySwitch);
                    }
                    if (!onStart) // On start book ui events like Populate doesn't work due to script execution order so onStart weapon addition are excluded
                    {
                        StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                    }
                }
                else if (weaponSlotSetArray[1][1] == null)
                {
                    weaponSlotSetArray[1][1] = weapon;
                    weapon.weaponBelongingToWhichOffHandSet = 2;
                    if (currentWeaponSlotSetIndex == 2)
                    {
                        ActivateWeapon(weapon, weapon.itemSlotStatus, 2, onStart, onlySwitch);
                    }

                    StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                }
                else if (weaponSlotSetArray[2][1] == null)
                {
                    weaponSlotSetArray[2][1] = weapon;
                    weapon.weaponBelongingToWhichOffHandSet = 3;
                    if (currentWeaponSlotSetIndex == 3)
                    {
                        ActivateWeapon(weapon, weapon.itemSlotStatus, 3, onStart, onlySwitch);
                    }

                    StaticEventHandler.CallWeaponPickedUpEventForBook(weapon, true);
                    offHandSlotFilled = true;
                }
                else
                {
                    offHandSlotFilled = true;
                }
            }
        }

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
        if (activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
            {
                currentMainHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100, 2) + 
                    activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitChance + additionalMeleeCriticalHitChanceModifier;
            }
            else
            {
                currentMainHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100, 2) +
                    activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.criticalHitChance + additionalCriticalRangedDamageModifier;
            }
        }
        else
        {
            currentMainHandCriticalHitChance = 0;
        }

        if (activeWeapon.GetCurrentOffHandWeapon() != null)
        {
            if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.isMeleeWeapon)
            {
                currentOffHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100 , 2) + 
                    activeWeapon.GetCurrentOffHandWeapon().weaponDetails.criticalHitChance + additionalMeleeCriticalHitChanceModifier;
            }
            else
            {
                currentOffHandCriticalHitChance = (float)Math.Round((double)currentDexterityValue * 0.75f / 100, 2) + 
                    activeWeapon.GetCurrentOffHandWeapon().weaponDetails.criticalHitChance;
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
            if (activeWeapon.GetCurrentMainHandWeapon() != null && activeWeapon.GetCurrentOffHandWeapon() != null)
            {
                if (equippedPassiveItems.TryGetValue(PassiveItemSlotName.Back, out PassiveItem item) && item != null)
                {
                    if (item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                    {
                        if ((activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger && activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass
                            == WeaponClass.Dagger) || (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Claw && activeWeapon.GetCurrentOffHandWeapon().
                            weaponDetails.weaponClass == WeaponClass.Claw)) // Grant extra cr. chance if dual-wield
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
        if (activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
            {
                currentMainHandCriticalHitDamage = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                     currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalMeleeDamageModifier;
            }
            else
            {
                currentMainHandCriticalHitDamage = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                    currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalRangedDamageModifier;
            }
        }
        else
        {
            currentMainHandCriticalHitDamage = 0;
        }

        if (activeWeapon.GetCurrentOffHandWeapon() != null)
        {
            if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.isMeleeWeapon)
            {
                currentOffHandCriticalHitDamage = activeWeapon.GetCurrentOffHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                    currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalMeleeDamageModifier;
            }
            else
            {
                currentOffHandCriticalHitDamage = activeWeapon.GetCurrentOffHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                    currentFerocityValue * 0.02f + additionalCriticalDamageModifier + additionalCriticalRangedDamageModifier;
            }
        }
        else
        {
            currentOffHandCriticalHitDamage = 0;
        }
    }

    public void UpdateCurrentAttackRatingValues()
    {
        Weapon currentMainHandWeapon = activeWeapon.GetCurrentMainHandWeapon();
        Weapon currentOffHandWeapon = activeWeapon.GetCurrentOffHandWeapon();

        float weaponHandlingModifier;

        if (currentOffHandWeapon != null)
        {
            if (currentMainHandWeapon != null)
            {
                weaponHandlingModifier = ((currentMainHandWeapon.weaponDetails.weaponBaseHandling + currentOffHandWeapon.weaponDetails.weaponBaseHandling) / 2) * 0.7f;
            }
            else
            {
                weaponHandlingModifier = 0;
            }
        }
        else
        {
            if (currentMainHandWeapon != null)
            {
                weaponHandlingModifier = currentMainHandWeapon.weaponDetails.weaponBaseHandling;
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
        float? blockValueIfHas = activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Shield ?
            activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.blockRate + additionalBlockModifier : 0f;
        currentBlockValue = (float)Math.Round((double)blockValueIfHas, 2);
    }

    public void UpdateEvasivenessValue()
    {
        currentEvasivenessValue = (float)Math.Round((double)(currentAgilityValue * 0.5f / 100 + additionalEvasivenessModifier), 2) ;
    }

    /// <summary>
    /// Set player health
    /// </summary>
    public void UpdatePlayerHealth(int healthIncrease, bool shouldHealthFilled, bool isMaxHealthChanged, bool onStart = false)
    {
        if (isMaxHealthChanged)
        {
            health.SetMaximumHealth(120 + currentConstitutionValue * 25, shouldHealthFilled, onStart);
        }

        health.AddHealth(healthIncrease);
    }

    /// <summary>
    /// Set player mana
    /// </summary>
    public void UpdatePlayerMana(int manaIncrease, bool shouldManaFilled, bool isMaxManaChanged, bool onStart = false)
    {
        if (isMaxManaChanged)
        {
            mana.SetMaximumMana(20 + currentWillpowerValue * 35, shouldManaFilled, onStart);
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
        Gizmos.DrawWireSphere(position, kynarasEmbraceCircleRadius);
    }
}
