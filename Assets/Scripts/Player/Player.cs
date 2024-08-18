using UnityEngine;
using UnityEngine.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;

#region REQUIRE COMPONENTS
[RequireComponent(typeof(HealthEvent))]
[RequireComponent(typeof(Health))]
[RequireComponent(typeof(DealContactDamage))]
[RequireComponent(typeof(ReceiveContactDamage))]
[RequireComponent(typeof(DestroyedEvent))]
[RequireComponent(typeof(Destroyed))]
[RequireComponent(typeof(PlayerControl))]
[RequireComponent(typeof(MovementByVelocity))]
[RequireComponent(typeof(Idle))]
[RequireComponent(typeof(AimWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(FireWeapon))]
[RequireComponent(typeof(MeleeAttackEvent))]
[RequireComponent(typeof(MeleeAttackMainHand))]
[RequireComponent(typeof(MeleeAttackOffHand))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(SelectedActiveItem))]
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
    [HideInInspector] public PlayerDetailsSO playerDetails;
    [HideInInspector] public HealthEvent healthEvent;
    [HideInInspector] public Health health;
    [HideInInspector] public MoveStatus moveStatus = MoveStatus.Idle;
    [HideInInspector] public HealthStatus healthStatus = HealthStatus.Normal;
    [HideInInspector] public ArmorStatus armorStatus = ArmorStatus.Normal;
    [HideInInspector] public DestroyedEvent destroyedEvent;
    [HideInInspector] public PlayerControl playerControl;
    [HideInInspector] public FireWeaponEvent fireWeaponEvent;
    [HideInInspector] public MeleeAttackEvent meleeAttackEvent;
    [HideInInspector] public MeleeAttackMainHand meleeAttackRightHand;
    [HideInInspector] public MeleeAttackOffHand meleeAttackLeftHand;
    [HideInInspector] public SetActiveWeaponEvent setActiveWeaponEvent;
    [HideInInspector] public AimWeapon aimWeapon;
    [HideInInspector] public ActiveWeapon activeWeapon;
    [HideInInspector] public SelectedActiveItem selectedActiveItem;
    [HideInInspector] public WeaponFiredEvent weaponFiredEvent;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public PolygonCollider2D polygonCollider2D;
    [HideInInspector] public Rigidbody2D rb2D;
    [HideInInspector] public Animator animator;
    [HideInInspector] public AnimatePlayer animatePlayer;
    [HideInInspector] public Knockback knockback;
    [HideInInspector] public Coins coins;
    [HideInInspector] public Idle idle;
    [HideInInspector] public MovementByVelocity movementByVelocity;
    [HideInInspector] public bool isDead;
    [HideInInspector] public StatusManager statusManager;
    [HideInInspector] public SpecialMoveEvent specialMoveEvent;
    [HideInInspector] public bool specialMoveOnCooldown = false;
    [HideInInspector] public float specialMoveTimer;
    [HideInInspector] public int keyCount = 0;
    [HideInInspector] public BranchMastery branchMastery;
    [HideInInspector] public WeaponMastery weaponMastery;
    [HideInInspector] public ChestItem activeItemChestItem;
    [HideInInspector] public bool hasRingOfFortune;

    [HideInInspector] public float playerWeaponHandlingModifier = 0f;
    [HideInInspector] public float playerEvasivenessModifier = 0f;

    [HideInInspector] public ParticleSystem dustParticlesSystem;
    [HideInInspector] public ParticleSystem specialMoveParticlesSystem;
    [HideInInspector] public Weapon[][] weaponSlotSetArray = new Weapon[3][] { new Weapon[2] {null, null}, new Weapon[2] {null, null}, new Weapon[2] {null, null}};
    [HideInInspector] public int currentWeaponSlotSetIndex = 1;
    [HideInInspector] public List<PassiveItem> passiveItemList = new List<PassiveItem>();
    [HideInInspector] public HashSet<Sprite> weaponBookMainHandHashSet = new HashSet<Sprite>();
    [HideInInspector] public HashSet<Sprite> weaponBookOffHandHashSet = new HashSet<Sprite>();
    [HideInInspector] public List<GameObject> summonedEnemies = new List<GameObject>();

    [HideInInspector] public bool mainHandSlotFilled = false;
    [HideInInspector] public bool offHandSlotFilled = false;

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        playerControl = GetComponent<PlayerControl>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        meleeAttackRightHand = GetComponent<MeleeAttackMainHand>();
        meleeAttackLeftHand = GetComponent<MeleeAttackOffHand>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        aimWeapon = GetComponent<AimWeapon>();
        activeWeapon = GetComponent<ActiveWeapon>();
        selectedActiveItem = GetComponent<SelectedActiveItem>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        polygonCollider2D = GetComponent<PolygonCollider2D>();
        rb2D = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        animatePlayer = GetComponent<AnimatePlayer>();
        knockback = GetComponent<Knockback>();
        coins = GetComponent<Coins>();
        idle = GetComponent<Idle>();
        movementByVelocity = GetComponent<MovementByVelocity>();
        specialMoveEvent = GetComponent<SpecialMoveEvent>();
        branchMastery = GetComponent<BranchMastery>();
        weaponMastery = GetComponent<WeaponMastery>();
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

        // Set player starting health
        SetPlayerHealth();
    }

    private void OnEnable()
    {
        healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }


    private void OnDisable()
    {
        healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void Start()
    {
        specialMoveTimer = 0;
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
        // Populate weapon list from starting weapons for right hand and shield for left hand if have any
        foreach (WeaponDetailsSO weaponDetails in playerDetails.startingWeaponList)
        {
            // Add weapon to right hand list of player
            AddNextWeaponToPlayer(weaponDetails, false, true, false);
        }
    }

    public void AddSecondWeaponToFirstSetOffHandedIfOneHand(WeaponDetailsSO weaponDetails)
    {
        Weapon weapon = new Weapon
        {
            weaponDetails = weaponDetails,
            weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
            onMaindHand = false
        };

        if (weaponSlotSetArray[1][1] == null)
        {
            weaponSlotSetArray[1][1] = weapon;
            weapon.weaponBelongingToWhichOffHandSet = 1;
            ActivateWeapon(weapon, true, 1);
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
        chestItem.Initialize(null, activeItemDetails, null, activeItemDetails.activeItemSprite, activeItemDetails.activeItemName, transform.position);

        // Disable some components during equipped
        chestItem.textTMP.enabled = false;
        chestItem.spriteRenderer.enabled = false;
        chestItem.animator.enabled = false;

        // Declare this chest item as to-be-dropped chest item
        GameManager.Instance.SetToBeDroppedChestItem(chestItem);
        GameManager.Instance.GetToBeDroppedChestItem().toBeDroppedActiveItem = activeItem;

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
        playerControl.PopulatePassiveItemsToBook(passiveItemDetails.passiveItemSprite, passiveItemDetails.itemSlotName);

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
                        if (!onStart) // On start book ui events like Populate doesn't work due to script execution order so onStart weapon addition are excluded
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

    //public void AddLeftHandWeaponForSameOneHandedTypesWithRightHand(bool onStart)
    //{
    //    List<Weapon> uniqueRightHandWeapons = new List<Weapon>();
    //    List<Weapon> uniqueLeftHandWeapons = weaponOffHandList;

    //    for (int i = 0; i < weaponMainHandList.Count; i++)
    //    {
    //        for (int j = 0; j < weaponMainHandList.Count; j++)
    //        {
    //            if (i == j) continue;

    //            // Sort right hand weapons based on weapon names
    //            weaponMainHandList.Sort((i, j) => string.Compare(i.weaponDetails.weaponName, j.weaponDetails.weaponName, 
    //                StringComparison.Ordinal));
    //        }
    //    }

    //    // Remove duplicates based on weapon names
    //    uniqueRightHandWeapons = weaponMainHandList.Distinct(new WeaponNameComparer()).ToList();

    //    // Keep track of encountered weapon names
    //    HashSet<string> encounteredWeaponNames = new HashSet<string>();

    //    foreach (Weapon weapon in weaponMainHandList)
    //    {
    //        // Check if the weapon name is a duplicate
    //        if (!encounteredWeaponNames.Add(weapon.weaponDetails.weaponName))
    //        {
    //            if (weapon.weaponDetails.weaponClass != WeaponClass.Spear)
    //            {
    //                // If it's a duplicate, add it to the left hand list
    //                weaponOffHandList.Add(weapon);

    //                if (!weaponBookOffHandHashSet.Contains(weapon.weaponDetails.weaponFrontSprite) && !onStart &&
    //                    weapon.weaponDetails.wieldType != WieldType.TwoHanded && weapon.weaponDetails.weaponClass != WeaponClass.Spear
    //                    && weapon.weaponDetails.weaponClass != WeaponClass.Shield)
    //                {
    //                    weaponBookOffHandHashSet.Add(weapon.weaponDetails.weaponFrontSprite);
    //                    PopulateOffHandWeaponsToBook(weapon);
    //                }
    //            }
    //        }
    //    }

    //    weaponMainHandList = uniqueRightHandWeapons;

    //    // Correct duplicated left hand weapons
    //    for (int i = 0; i < weaponOffHandList.Count; i++)
    //    {
    //        for (int j = 0; j < weaponOffHandList.Count; j++)
    //        {
    //            if (i == j) continue;

    //            // Sort hand weapons based on weapon names
    //            weaponOffHandList.Sort((i, j) => string.Compare(i.weaponDetails.weaponName, j.weaponDetails.weaponName, StringComparison.Ordinal));

    //            // Remove duplicates based on weapon names
    //            uniqueLeftHandWeapons = weaponOffHandList.Distinct(new WeaponNameComparer()).ToList();
    //        }
    //    }

    //    weaponOffHandList = uniqueLeftHandWeapons;

    //    // Correct if there is a two-handed weapon at left hand
    //    foreach (Weapon weapon in weaponOffHandList)
    //    {
    //        if (weapon.weaponDetails.wieldType == WieldType.TwoHanded && weapon.weaponDetails.weaponClass != WeaponClass.Shield)
    //        {
    //            weaponOffHandList.Remove(weapon);
    //        }
    //    }
    //}

    public void ActivateWeapon(Weapon weapon, bool isOffHand, int setIndex)
    {
        if (!isOffHand)
        {
            // Set the added weapon as active - main hand
            setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(weapon, setIndex);

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
            setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(weapon);
        }
    }

    /// <summary>
    /// Set player health from playerDetails SO
    /// </summary>
    private void SetPlayerHealth()
    {
        health.SetStartingHealth(playerDetails.playerHealthAmount);
    }

    /// <summary>
    /// Returns the player position
    /// </summary>
    public Vector3 GetPlayerPosition()
    {
        return transform.position;
    }

    /// <summary>
    /// Create a chest item for to-be-dropped weapon
    /// </summary>
    public ChestItem CreateChestItemForWeapon(Weapon weapon)
    {
        if (weapon.onMaindHand)
        {
            if (IsMainHandDropNotPossible())
            {
                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.LessThanOneMainHandWeapon);
                return null;
            }
            else
            {
                switch (weapon.weaponBelongingToWhichMainHandSet)
                {
                    case 1:
                        if (weaponSlotSetArray[0][1] == null) // Drop main hand if only off-hand slot is empty
                        {
                            weaponSlotSetArray[0][0] = null;
                        }
                        else
                        {
                            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandFull);
                            return null;
                        }
                        break;
                    case 2:
                        if (weaponSlotSetArray[1][1] == null)
                        {
                            weaponSlotSetArray[1][0] = null;
                        }
                        else
                        {
                            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandFull);
                            return null;
                        }
                        break;
                    case 3:
                        if (weaponSlotSetArray[2][1] == null)
                        {
                            weaponSlotSetArray[2][0] = null;
                        }
                        else
                        {
                            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandFull);
                            return null;
                        }
                        break;
                    default:
                        break;
                }
            }
        }

        GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        ChestItem chestItem = chestItemObject.GetComponent<ChestItem>();

        chestItem.remainingItemCharge = playerDetails.selectedActiveItem.activeItemMaxCharge;
        chestItem.boxCollider2D.enabled = false;

        // Set hasActiveDrop flag to true
        chestItem.hasWeaponDrop = true;

        // Initialize chest item
        chestItem.Initialize(weapon.weaponDetails, null, null, weapon.weaponDetails.weaponFrontSprite, weapon.weaponDetails.weaponName, transform.position);

        // Disable some components during equipped
        chestItem.animator.runtimeAnimatorController = weapon.weaponDetails.weaponHoverAnimatorController;
        chestItem.textTMP.enabled = false;
        chestItem.spriteRenderer.enabled = false;
        chestItem.animator.enabled = false;

        return chestItem;
    }

    private bool IsMainHandDropNotPossible()
    {
        int gauge = 0;

        for (int i = 0; i < 3; i++)
        {
            if (weaponSlotSetArray[i][0] != null)
            {
                gauge++;
            }
            else
            {
                continue;
            }
        }

        return gauge <= 1;
    }
}
