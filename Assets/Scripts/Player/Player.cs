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
[RequireComponent(typeof(MeleeAttackRightHand))]
[RequireComponent(typeof(MeleeAttackLeftHand))]
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
[RequireComponent(typeof(Knockback))]
[RequireComponent(typeof(Coins))]
[RequireComponent(typeof(StatusManager))]
[RequireComponent(typeof(SpecialMoveEvent))]
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
    [HideInInspector] public MeleeAttackRightHand meleeAttackRightHand;
    [HideInInspector] public MeleeAttackLeftHand meleeAttackLeftHand;
    [HideInInspector] public SetActiveWeaponEvent setActiveWeaponEvent;
    [HideInInspector] public AimWeapon aimWeapon;
    [HideInInspector] public ActiveWeapon activeWeapon;
    [HideInInspector] public WeaponFiredEvent weaponFiredEvent;
    [HideInInspector] public ReloadWeaponEvent reloadWeaponEvent;
    [HideInInspector] public WeaponReloadedEvent weaponReloadedEvent;
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

    public ParticleSystem dustParticlesSystem;
    public ParticleSystem seismicSlamParticlesSystem;
    public List<Weapon> weaponRightHandList = new List<Weapon>();
    public List<Weapon> weaponLeftHandList = new List<Weapon>(); 

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        health = GetComponent<Health>();
        destroyedEvent = GetComponent<DestroyedEvent>();
        playerControl = GetComponent<PlayerControl>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        meleeAttackRightHand = GetComponent<MeleeAttackRightHand>();
        meleeAttackLeftHand = GetComponent<MeleeAttackLeftHand>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        aimWeapon = GetComponent<AimWeapon>();
        activeWeapon = GetComponent<ActiveWeapon>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
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
        // Clear list
        weaponRightHandList.Clear();
        weaponLeftHandList.Clear();

        // Populate weapon list from starting weapons for right hand and shield for left hand if have any
        foreach (WeaponDetailsSO weaponDetails in playerDetails.startingWeaponList)
        {
            // Add weapon to right hand list of player
            AddRightHandWeaponToPlayer(weaponDetails);
            AddShieldToLeftHandIfHave(weaponDetails);
        }

        AddLeftHandWeaponForSameOneHandedTypesWithRightHand();
    }

    /// <summary>
    /// Update weapons list if a new one acquired
    /// </summary>
    public void UpdateWieldedWeapons(WeaponDetailsSO weaponDetails)
    {
        List<WeaponDetailsSO> allEquippedWeaponsList = new List<WeaponDetailsSO> { weaponDetails };

        foreach (Weapon rightHandWeapon in weaponRightHandList)
        {
            allEquippedWeaponsList.Add(rightHandWeapon.weaponDetails);
        }

        foreach (Weapon leftHandWeapon in weaponLeftHandList)
        {
            allEquippedWeaponsList.Add(leftHandWeapon.weaponDetails);
        }

        weaponRightHandList = new List<Weapon>();
        weaponLeftHandList = new List<Weapon>();

        // Populate weapon list from starting weapons for right hand and shield for left hand if have any
        foreach (WeaponDetailsSO weapon in allEquippedWeaponsList)
        {
            // Add weapon to right hand list of player
            AddRightHandWeaponToPlayer(weapon);
            AddShieldToLeftHandIfHave(weapon);
        }

        AddLeftHandWeaponForSameOneHandedTypesWithRightHand();
    }

    /// <summary>
    /// Add a weapon to the right hand of player weapon list
    /// </summary>
    public Weapon AddRightHandWeaponToPlayer(WeaponDetailsSO weaponDetails)
    {
        Weapon weapon = new Weapon
        {
            weaponDetails = weaponDetails,
            weaponReloadTimer = 0f,
            weaponClipRemainingProjectile = weaponDetails.weaponClipProjectileCapacity,
            weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
            isWeaponReloading = false
        };

        // If the weapon is not a shield then it can equipped to the right hand
        if (weapon.weaponDetails.weaponClass != WeaponClass.Shield)
        {
            // Add the weapon to the list
            weaponRightHandList.Add(weapon);

            // Set weapon position in list
            weapon.weaponRightHandListPosition = weaponRightHandList.Count;

            // Set the added weapon as active
            setActiveWeaponEvent.CallSetActiveWeaponAtRightHandEvent(weapon);
        }

        return weapon;
    }

    public void AddShieldToLeftHandIfHave(WeaponDetailsSO weaponDetails)
    {
        if (weaponDetails.weaponClass == WeaponClass.Shield)
        {
            Weapon weapon = new Weapon
            {
                weaponDetails = weaponDetails,
                weaponReloadTimer = 0f,
                weaponClipRemainingProjectile = weaponDetails.weaponClipProjectileCapacity,
                weaponRemainingProjectile = weaponDetails.weaponProjectileCapacity,
                isWeaponReloading = false
            };

            // Add the weapon to the left hand list if it is a shield type
            weaponLeftHandList.Add(weapon);

            // Set weapon position in list
            weapon.weaponLeftHandListPosition = weaponLeftHandList.Count;
        }
    }

    public void AddLeftHandWeaponForSameOneHandedTypesWithRightHand()
    {
        List<Weapon> uniqueRightHandWeapons = new List<Weapon>();
        List<Weapon> uniqueLeftHandWeapons = weaponLeftHandList;

        for (int i = 0; i < weaponRightHandList.Count; i++)
        {
            for (int j = 0; j < weaponRightHandList.Count; j++)
            {
                if (i == j) continue;

                // Sort right hand weapons based on weapon names
                weaponRightHandList.Sort((i, j) => string.Compare(i.weaponDetails.weaponName, j.weaponDetails.weaponName, StringComparison.Ordinal));

                // Remove duplicates based on weapon names
                uniqueRightHandWeapons = weaponRightHandList.Distinct(new WeaponNameComparer()).ToList();
            }
        }

        // Keep track of encountered weapon names
        HashSet<string> encounteredWeaponNames = new HashSet<string>();

        foreach (Weapon weapon in weaponRightHandList)
        {
            // Check if the weapon name is a duplicate
            if (!encounteredWeaponNames.Add(weapon.weaponDetails.weaponName))
            {
                if (weapon.weaponDetails.weaponClass != WeaponClass.Spear)
                {
                    // If it's a duplicate, add it to the left hand list
                    weaponLeftHandList.Add(weapon);
                }
            }
        }

        weaponRightHandList = uniqueRightHandWeapons;

        // Correct duplicated left hand weapons
        for (int i = 0; i < weaponLeftHandList.Count; i++)
        {
            for (int j = 0; j < weaponLeftHandList.Count; j++)
            {
                if (i == j) continue;

                // Sort hand weapons based on weapon names
                weaponLeftHandList.Sort((i, j) => string.Compare(i.weaponDetails.weaponName, j.weaponDetails.weaponName, StringComparison.Ordinal));

                // Remove duplicates based on weapon names
                uniqueLeftHandWeapons = weaponLeftHandList.Distinct(new WeaponNameComparer()).ToList();
            }
        }

        weaponLeftHandList = uniqueLeftHandWeapons;

        // Correct if there is a two-handed weapon at left hand
        foreach (Weapon weapon in weaponLeftHandList)
        {
            if (weapon.weaponDetails.wieldType == WieldType.TwoHanded && weapon.weaponDetails.weaponClass != WeaponClass.Shield)
            {
                weaponLeftHandList.Remove(weapon);
            }
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
    /// Returns true if the weapon is held by the player right hand - otherwise returns false
    /// </summary>
    public bool IsWeaponHeldByPlayerRightHand(WeaponDetailsSO weaponDetails)
    {
        foreach (Weapon weapon in weaponRightHandList)
        {
            if (weapon.weaponDetails == weaponDetails) return true;
        }

        return false;
    }

    /// <summary>
    /// Returns true if the weapon is held by the player left hand- otherwise returns false
    /// </summary>
    public bool IsWeaponHeldByPlayerLeftHand(WeaponDetailsSO weaponDetails)
    {
        foreach (Weapon weapon in weaponLeftHandList)
        {
            if (weapon.weaponDetails == weaponDetails) return true;
        }

        return false;
    }
}
