using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
<<<<<<< Updated upstream
=======
using System.Linq;
>>>>>>> Stashed changes
using System;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DealContactDamage))]
[RequireComponent(typeof(ReceiveContactDamage))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
[RequireComponent(typeof(PlayerControl))]
[RequireComponent(typeof(MovementByVelocityEvent))]
[RequireComponent(typeof(MovementByVelocity))]
[RequireComponent(typeof(IdleEvent))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AimWeaponEvent))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(FireWeapon))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(ReloadWeapon))]
[RequireComponent(typeof(AnimatePlayer))]
[RequireComponent(typeof(SortingGroup))]
[RequireComponent(typeof(SpriteRenderer))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(PolygonCollider2D))]
[RequireComponent(typeof(Rigidbody2D))]
#endregion
[DisallowMultipleComponent]
public class Player : MonoBehaviour
{
    [HideInInspector] public PlayerDetailsSO playerDetails;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public Health health;
    [HideInInspector] public DestroyedEvent destroyedEvent;
    [HideInInspector] public PlayerControl playerControl;
    [HideInInspector] public MovementByVelocityEvent movementByVelocityEvent;
    [HideInInspector] public IdleEvent idleEvent;
    [HideInInspector] public AimWeaponEvent aimWeaponEvent;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    [HideInInspector] public SetActiveWeaponEvent setActiveWeaponEvent;
    [HideInInspector] public ActiveWeapon activeWeapon;
    [HideInInspector] public WeaponFiredEvent weaponFiredEvent;
    [HideInInspector] public ReloadWeaponEvent reloadWeaponEvent;
    [HideInInspector] public WeaponReloadedEvent weaponReloadedEvent;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;

<<<<<<< Updated upstream
    public List<Weapon> weaponList = new List<Weapon>();
=======
    // PLAYER PRIMARY STATS
    [HideInInspector] public int currentStrengthValue;
    [HideInInspector] public int currentConstitutionValue;
    [HideInInspector] public int currentDexterityValue;
    [HideInInspector] public int currentIntelligenceValue;
    [HideInInspector] public int currentAgilityValue;
    [HideInInspector] public float currentPhysicalResistanceValue;
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
    [HideInInspector] public float? currentWeaponHandlingValue;

    // CURRENT DEFLECTION VALUES
    [HideInInspector] public float? currentDeflectionValue;

    [HideInInspector] public ParticleSystem specialMoveParticlesSystem;
    [HideInInspector] public Weapon[][] weaponSlotSetArray = new Weapon[3][] { new Weapon[2] {null, null}, new Weapon[2] {null, null}, new Weapon[2] {null, null}};
    [HideInInspector] public int currentWeaponSlotSetIndex = 1;
    [HideInInspector] public List<PassiveItem> passiveItemList = new List<PassiveItem>();
    [HideInInspector] public List<GameObject> summonedEnemies = new List<GameObject>();

    [HideInInspector] public bool mainHandSlotFilled = false;
    [HideInInspector] public bool offHandSlotFilled = false;
    [HideInInspector] public short specialSkillNumber = 0;

    [HideInInspector] public bool onStealth;
    [HideInInspector] public bool isBlockingActive;
    [HideInInspector] public bool isGemSkinActive;
    [HideInInspector] public static bool hasClone;
    [HideInInspector] public GameObject playerCloneObject;

    [HideInInspector] public bool isCursed;
>>>>>>> Stashed changes

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        playerControl = GetComponent<PlayerControl>();
        movementByVelocityEvent = GetComponent<MovementByVelocityEvent>();
        idleEvent = GetComponent<IdleEvent>();
        aimWeaponEvent = GetComponent<AimWeaponEvent>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        activeWeapon = GetComponent<ActiveWeapon>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    /// <summary>
    /// Initialize the player
    /// </summary>
    public void Initialize(PlayerDetailsSO playerDetails)
    {
        this.playerDetails = playerDetails;

        //Create player starting weapons
        CreatePlayerStartingWeapons();

        // Set player starting health
        SetPlayerHealth();

        // Set player starting primary stats
        SetPlayerPrimaryStats();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
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

    /// <summary>
    /// Set the player starting weapon
    /// </summary>
    private void CreatePlayerStartingWeapons()
    {
        // Clear list
        weaponList.Clear();

        // Populate weapon list from starting weapons
        foreach (WeaponDetailsSO weaponDetails in playerDetails.startingWeaponList)
        {
<<<<<<< Updated upstream
            // Add weapon to player
            AddWeaponToPlayer(weaponDetails);
=======
            // Add weapon to right hand list of player
            AddNextWeaponToPlayer(weaponDetails, false, true, false);
        }
    }

    /// <summary>
    /// Set the player starting active item
    /// </summary>
    private void CreatePlayerStartingActiveItem()
    {
        GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        activeItemChestItem = chestItemObject.GetComponent<ChestItem>();

        activeItemChestItem.remainingItemCharge = playerDetails.selectedActiveItem.activeItemMaxCharge;
        AddActiveItemToPlayer(playerDetails.selectedActiveItem, activeItemChestItem, activeItemChestItem.remainingItemCharge);
    }

    /// <summary>
    /// Set the player starting passive item
    /// </summary>
    private void CreatePlayerStartingPassiveItem()
    {
        passiveItemList.Clear();

        AddPassiveItemToPlayer(playerDetails.passiveItemsList[0]);
    }

    /// <summary>
    /// Update weapons list if a new one acquired
    /// </summary>
    public void UpdateWieldedWeapons(WeaponDetailsSO weaponDetails, bool updateHappenedAfterNewItemCollected, bool onStart)
    {
        AddNextWeaponToPlayer(weaponDetails, updateHappenedAfterNewItemCollected, onStart, false);
    }

    /// <summary>
    /// Set the player primary stats
    /// </summary>
    private void SetPlayerPrimaryStats()
    {
        currentStrengthValue = playerDetails.strength;
        currentConstitutionValue = playerDetails.constitution;
        currentDexterityValue = playerDetails.dexterity;
        currentIntelligenceValue = playerDetails.intelligence;
        currentAgilityValue = playerDetails.agility;

        currentPhysicalResistanceValue = playerDetails.physicalResistance;
        currentFireResistanceValue = playerDetails.fireResistance;
        currentWaterResistanceValue = playerDetails.waterResistance;
        currentAirResistanceValue = playerDetails.airResistance;
        currentEarthResistanceValue = playerDetails.earthResistance;
        currentLightResistanceValue = playerDetails.lightResistance;
        currentDarkResistanceValue = playerDetails.darkResistance;

        UpdateDamageValues();
        UpdateWeaponHandlingAndCriticalValues();
        UpdateEvasivenessValue();
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
                        currentMainHandMinDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMin + playerDetails.strength * 2;
                        currentMainHandMaxDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMax + playerDetails.strength * 2;
                    }
                    // Melee and physical for dagger
                    else
                    {
                        currentMainHandMinDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMin + playerDetails.dexterity;
                        currentMainHandMaxDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMax + playerDetails.dexterity;
                    }
                }
                else
                {
                    currentMainHandMinDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + playerDetails.intelligence * 2;
                    currentMainHandMaxDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + playerDetails.intelligence * 2;
                }
            }
            else
            {
                // Non-melee and physical damage (such as bow)
                if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasPhysicalDamage)
                {
                    currentMainHandMinDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + playerDetails.dexterity;
                    currentMainHandMaxDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + playerDetails.dexterity;
                }
                // Non-melee and non-physical damage (such as staff)
                else
                {
                    currentMainHandMinDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + playerDetails.intelligence * 2;
                    currentMainHandMaxDamageValue = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + playerDetails.intelligence * 2;
                }
            }
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
                        currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMin + playerDetails.strength * 2) * 0.6f, 2);
                        currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMax + playerDetails.strength * 2) * 0.6f, 2);
                    }
                    // Melee and physical for dagger
                    else
                    {
                        currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMin + playerDetails.dexterity) * 0.6f, 2);
                        currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.meleeDamageMax + playerDetails.dexterity) * 0.6f, 2);
                    }
                }
                else
                {
                    currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + playerDetails.intelligence * 2) * 0.6f, 2);
                    currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + playerDetails.intelligence * 2) * 0.6f, 2);
                }
            }
            else
            {
                // Non-melee and physical damage (such as bow)
                if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasPhysicalDamage)
                {
                    currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin + playerDetails.dexterity) * 0.6f, 2);
                    currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax + playerDetails.dexterity) * 0.6f, 2);
                }
                // Non-melee and non-physical damage (such as staff)
                else
                {
                    if (activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield)
                    {
                        currentOffHandMinDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMin 
                            + playerDetails.intelligence * 2) * 0.6f, 2);
                        currentOffHandMaxDamageValue = (int)Math.Round((activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax
                            + playerDetails.intelligence * 2) * 0.6f, 2);
                    }
                }
            }
        }      
    }

    public void UpdateWeaponHandlingAndCriticalValues()
    {
        // Update weapon handling value
        float? weaponHandlingModifier = activeWeapon.GetCurrentOffHandWeapon() != null ? (activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.weaponBaseHandling +
            activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponBaseHandling) / 2 * 0.7f ?? 0 : activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.weaponBaseHandling;
        float roundedModifier = weaponHandlingModifier != null ? (float)Math.Round((double)weaponHandlingModifier, 2) : 0;
        currentWeaponHandlingValue = (float)playerDetails.dexterity * 10 / 100 + weaponHandlingModifier;

        // Update equipped weapon critical hit chances
        currentMainHandCriticalHitChance = (float)Math.Round((double)playerDetails.dexterity * 2 / 100) + (activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.criticalHitChance ?? 0);
        currentOffHandCriticalHitChance = (float)Math.Round((double)playerDetails.dexterity * 2 / 100) + (activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.criticalHitChance ?? 0);
    }

    public void UpdateEvasivenessValue()
    {
        currentDeflectionValue = (float)Math.Round((double)(playerDetails.agility * 3 / 100), 2);

        float? blockValueIfHas = activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Shield ? 
            activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.projectileDeflectRatio : 0f;
        float roundedModifier = (float)Math.Round((double)blockValueIfHas, 2);
        currentDeflectionValue += roundedModifier;
    }

    /// <summary>
    /// Add an active item to the player
    /// </summary>
    public ActiveItem AddActiveItemToPlayer(ActiveItemDetailsSO activeItemDetails, ChestItem chestItem, int remainingItemCharge)
    {
        ActiveItem activeItem = new ActiveItem
        {
            activeItemDetails = activeItemDetails,
            activeItemRemainingCharge = remainingItemCharge,
            isReturning = false
        };

        chestItem.boxCollider2D.enabled = false;

        playerControl.PopulateActiveItemsToBook(activeItemDetails.activeItemSprite);

        // Set the added active item as active
        setActiveWeaponEvent.CallSelectedActiveItem(activeItem);

        // Set hasActiveDrop flag to true
        chestItem.hasActiveDrop = true; 

        // Initialize chest item
        chestItem.Initialize(activeItem, activeItemDetails.activeItemSprite, activeItemDetails.activeItemName, transform.position);

        // Disable some components during equipped
        chestItem.textTMP.enabled = false;
        chestItem.spriteRenderer.enabled = false;
        chestItem.animator.enabled = false;

        // Declare this chest item as to-be-dropped chest item
        ChestItem.toBeDroppedChestItem = chestItem;
        ChestItem.toBeDroppedChestItem.toBeDroppedActiveItem = activeItem;

        return activeItem;
    }

    public PassiveItem AddPassiveItemToPlayer(PassiveItemDetailsSO passiveItemDetails)
    {
        PassiveItem passiveItem = new PassiveItem
        {
            passiveItemDetails = passiveItemDetails
        };

        foreach (PassiveItem item in passiveItemList)
        {
            // If the passive item is already equipped then return null
            if (passiveItemList.Any(item => item.passiveItemDetails.passiveItemName == passiveItem.passiveItemDetails.passiveItemName))
            {
                return null;
            }
        }

        passiveItemList.Add(passiveItem);
        playerControl.PopulatePassiveItemsToBook(passiveItemDetails.passiveItemSprite, passiveItemDetails.passiveItemSlotName);

        return passiveItem;
    }

    /// <summary>
    /// Add a weapon to the right hand of player weapon list
    /// </summary>
    public void AddNextWeaponToPlayer(WeaponDetailsSO weaponDetails, bool pickingUp, bool onStart, bool onlySwitch)
    {
        if (!offHandSlotFilled)
        {
            // First check if it is a shield, if yes equip and return to avoid further checks
            if (weaponDetails.weaponClass == WeaponClass.Shield)
            {
                Weapon weapon = new Weapon
                {
                    weaponDetails = weaponDetails,
                    weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                    onMaindHand = false
                };

                if (weaponSlotSetArray[0][1] == null)
                {
                    if (weaponSlotSetArray[0][0].weaponDetails.wieldType != WieldType.TwoHanded)
                    {
                        weaponSlotSetArray[0][1] = weapon;
                        weapon.weaponBelongingToWhichOffHandSet = 1;
                        if (currentWeaponSlotSetIndex == 1)
                        {
                            ActivateWeapon(weapon, !weapon.onMaindHand, 1);
                        }
                        if (!onStart) // On start book ui events like Populate doesn't work due to script execution order so onStart weapon additions are excluded
                        {
                            playerControl.PopulateOffHandWeaponsToBook(weapon);
                        }
                        return;
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
                                ActivateWeapon(weapon, !weapon.onMaindHand, 2);
                            }
                            weapon.weaponBelongingToWhichOffHandSet = 2;
                            playerControl.PopulateOffHandWeaponsToBook(weapon);
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
                                ActivateWeapon(weapon, !weapon.onMaindHand, 3);
                            }
                            weapon.weaponBelongingToWhichOffHandSet = 3;
                            playerControl.PopulateOffHandWeaponsToBook(weapon);
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

        if (!mainHandSlotFilled)
        {
            if (weaponDetails.weaponClass != WeaponClass.Shield)
            {
                Weapon weapon = new Weapon
                {
                    weaponDetails = weaponDetails,
                    weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                    onMaindHand = true
                };

                // Exceptional for Erebus onStart offHand dagger wield then return
                if (playerDetails.playerCharacterIndex == Character.Erebus)
                {
                    if (onStart)
                    {
                        if (weaponSlotSetArray[0][1] == null)
                        {
                            weapon.onMaindHand = false;
                            weaponSlotSetArray[0][1] = weapon;
                            weapon.weaponBelongingToWhichOffHandSet = 1;
                            ActivateWeapon(weapon, true, 1);
                            return;
                        }
                    }
                }

                if (weaponSlotSetArray[0][0] == null)
                {
                    weaponSlotSetArray[0][0] = weapon;
                    weapon.weaponBelongingToWhichMainHandSet = 1;
                    if (currentWeaponSlotSetIndex == 1)
                    {
                        ActivateWeapon(weapon, !weapon.onMaindHand, 1);
                    }
                    if (!onStart)
                    {
                        playerControl.PopulateMainHandWeaponsToBook(weapon, onlySwitch);
                    }
                }
                else if (weaponSlotSetArray[1][0] == null)
                {
                    weaponSlotSetArray[1][0] = weapon;
                    weapon.weaponBelongingToWhichMainHandSet = 2;
                    if (currentWeaponSlotSetIndex == 2)
                    {
                        ActivateWeapon(weapon, !weapon.onMaindHand, 2);
                    }
                    if (!onStart)
                    {
                        playerControl.PopulateMainHandWeaponsToBook(weapon, onlySwitch);
                    }
                }
                else if (weaponSlotSetArray[2][0] == null)
                {
                    weaponSlotSetArray[2][0] = weapon;
                    weapon.weaponBelongingToWhichMainHandSet = 3;
                    if (currentWeaponSlotSetIndex == 3)
                    {
                        ActivateWeapon(weapon, !weapon.onMaindHand, 3);
                    }
                    if (!onStart)
                    {
                        playerControl.PopulateMainHandWeaponsToBook(weapon, onlySwitch);
                    }
                    mainHandSlotFilled = true; // All 3 main hand slots filled at start
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
                    onMaindHand = false
                };

                if (weaponSlotSetArray[0][1] == null)
                {
                    weaponSlotSetArray[0][1] = weapon;
                    weapon.weaponBelongingToWhichOffHandSet = 1;
                    if (currentWeaponSlotSetIndex == 1)
                    {
                        ActivateWeapon(weapon, !weapon.onMaindHand, 1);
                    }
                    if (!onStart) // On start book ui events like Populate doesn't work due to script execution order so onStart weapon addition are excluded
                    {
                        playerControl.PopulateOffHandWeaponsToBook(weapon);
                    }
                }
                else if (weaponSlotSetArray[1][1] == null)
                {
                    weaponSlotSetArray[1][1] = weapon;
                    weapon.weaponBelongingToWhichOffHandSet = 2;
                    if (currentWeaponSlotSetIndex == 2)
                    {
                        ActivateWeapon(weapon, !weapon.onMaindHand, 2);
                    }
                    playerControl.PopulateOffHandWeaponsToBook(weapon);
                }
                else if (weaponSlotSetArray[2][1] == null)
                {
                    weaponSlotSetArray[2][1] = weapon;
                    weapon.weaponBelongingToWhichOffHandSet = 3;
                    if (currentWeaponSlotSetIndex == 3)
                    {
                        ActivateWeapon(weapon, !weapon.onMaindHand, 3);
                    }
                    playerControl.PopulateOffHandWeaponsToBook(weapon);
                    offHandSlotFilled = true;
                }
                else
                {
                    offHandSlotFilled = true;
                }
            }
        }
    }
    
    public void ActivateWeapon(Weapon weapon, bool isOffHand, int setIndex)
    {
        if (!isOffHand)
        {
            // Set the added weapon as active - main hand
            setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(weapon, setIndex);

            // This section is for enabling/disabling lock icon based on weapon's one-hand or two-hand wield
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.OneHanded)
            {
                setActiveWeaponEvent.CallOneHandWeaponEquipEvent();
            }
            else if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
            {
                setActiveWeaponEvent.CallTwoHandWeaponEquipEvent();
            }
        }
        else
        {
            // Set the added weapon as active - main hand
            setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(weapon, setIndex);
>>>>>>> Stashed changes
        }
    }

    /// <summary>
    /// Set player health from playerDetails SO
    /// </summary>
    private void SetPlayerHealth()
    {
        health.SetStartingHealth(20 + playerDetails.constitution * 10);
    }

    /// <summary>
    /// Returns the player position
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// Add a weapon to the player weapon dictionary
    /// </summary>
    public Weapon AddWeaponToPlayer(WeaponDetailsSO weaponDetails)
    {
        Weapon weapon = new Weapon { weaponDetails = weaponDetails, weaponReloadTimer = 0f, 
            weaponClipRemainingProjectile = weaponDetails.weaponClipProjectileCapacity, 
            weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity, isWeaponReloading = false };

        // Add the weapon to the list
        weaponList.Add(weapon);

        // Set weapon position in list
        weapon.weaponListPosition = weaponList.Count;

        // Set the added weapon as active
        if (weaponDetails.isMeleeWeapon)
        {
            setActiveWeaponEvent.CallSetActiveWeaponEvent(weapon, weaponDetails.weaponAnimatorController);
        }
        else
        {
            setActiveWeaponEvent.CallSetActiveWeaponEvent(weapon, null);
        }

        return weapon;
    }

    /// <summary>
    /// Returns true if the weapon is held by the player - otherwise returns false
    /// </summary>
    public bool IsWeaponHeldByPlayer(WeaponDetailsSO weaponDetails)
    {
        foreach (Weapon weapon in weaponList)
        {
            if (weapon.weaponDetails == weaponDetails) return true;
        }

        return false;
    }
}
