using UnityEngine;

[CreateAssetMenu(fileName = "ProjectileDetails_", menuName = "Scriptable Objects/Weapons/Projectile Details")]
public class ProjectileDetailsSO : ScriptableObject
{
    #region Header BASIC PROJECTILE DETAILS
    [Space(10)]
    [Header("BASIC PROJECTILE DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("Name for the projectile")]
    #endregion
    public string projectileName;
    public bool isPlayerProjectile;

    #region Header PROJECTILE SPRITE, PREFAB & MATERIALS
    [Space(10)]
    [Header("PROJECTILE SPRITE, PREFAB & MATERIALS")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the prefab to be used for the projectile.  If multiple prefabs are specified then a random prefab from the array will be selecetd.  " +
        "The prefab can be an projectile pattern - as long as it conforms to the IFireable interface.")]
    #endregion
    public GameObject[] projectilePrefabArray;
    #region Tooltip
    [Tooltip("The material to be used for the projectile")]
    #endregion
    public Material projectileMaterial;
    #region Tooltip
    [Tooltip("If the projectile should 'charge' briefly before moving then set the time in seconds that the projectile is held charging after firing before release")]
    #endregion
    public float projectileChargeTime = 0.1f;
    #region Tooltip
    [Tooltip("If the projectile has a charge time then specify what material should be used to render the projectile while charging")]
    #endregion
    public Material projectileChargeMaterial;
    #region Tooltip
    [Tooltip("If have, belonging weapon details")]
    #endregion
    public WeaponDetailsSO belongingWeaponDetails;

    #region Header PROJECTILE HIT EFFECT
    [Space(10)]
    [Header("PROJECTILE HIT EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("The scriptable object that defines the parameters for the hit effect prefab")]
    #endregion
    public ProjectileHitEffectSO projectileHitEffect;

    #region Header PROJECTILE BASE PARAMETERS
    [Space(10)]
    [Header("PROJECTILE BASE PARAMETERS")]
    #endregion
    #region Tooltip
    [Tooltip("The min damage each projectile deals")]
    #endregion
    public int projectileDamageMin = 0;
    #region Tooltip
    [Tooltip("The max damage each projectile deals")]
    #endregion
    public int projectileDamageMax = 1;
    #region Tooltip
    [Tooltip("The speed of the projectile")]
    #endregion
    public float projectileSpeed = 20f;
    #region Tooltip
    [Tooltip("The range of the projectile (or projectile pattern) in unity units")]
    #endregion
    public float projectileRange = 20f;
    #region Tooltip
    [Tooltip("Check if has a lifetime")]
    #endregion
    public bool hasLifeTime = false;
    #region Tooltip
    [Tooltip("The blast radius of the item")]
    #endregion
    public float lifeDuration = 1;
    #region Tooltip
    [Tooltip("The rotation speed in degrees per second of the projectile pattern")]
    #endregion
    public float projectileRotationSpeed = 1f;

    #region Header PROJECTILE - BOMB & TRAP
    [Space(10)]
    [Header("PROJECTILE BOMB&TRAP PARAMETERS")]
    #endregion
    #region Tooltip
    [Tooltip("The guided missle check")]
    #endregion
    public bool isGuided = false;
    #region Tooltip
    [Tooltip("The bouncing projectile check")]
    #endregion
    public bool isBouncing = false;
    #region Tooltip
    [Tooltip("The bomb check")]
    #endregion
    public bool isBomb = false;
    #region Tooltip
    [Tooltip("The trap check")]
    #endregion
    public bool isTrap = false;
    #region Tooltip
    [Tooltip("The blast radius of the item")]
    #endregion
    public float blastRadius = 1f;
    #region Tooltip
    [Tooltip("The countdown until the item burst")]
    #endregion
    public float countDown = 3f;
    #region Tooltip
    [Tooltip("The min burst damage each projectile deals")]
    #endregion
    public int burstDamageMin = 15;
    #region Tooltip
    [Tooltip("The max burst damage each projectile deals")]
    #endregion
    public int burstDamageMax = 25;

    #region Header PASSIVE
    [Space(10)]
    [Header("PROJECTILE PASSIVE EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("Check if projectile can warm")]
    #endregion Tooltip
    public bool hasWarmDamage;
    #region Tooltip
    [Tooltip("The chance of projetile's warm effect")]
    #endregion Tooltip
    [Range(0f, 1f)] public float warmChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has burn damage")]
    #endregion Tooltip
    public bool hasBurnDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's burn")]
    #endregion Tooltip
    [Range(0f, 1f)] public float burnChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has stun damage")]
    #endregion Tooltip
    public bool hasStunDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's stun")]
    #endregion Tooltip
    [Range(0f, 1f)] public float stunChance = 0.2f;
    #region Tooltip
    [Tooltip("Check if projectile has root damage")]
    #endregion Tooltip
    public bool hasRootDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's root")]
    #endregion Tooltip
    [Range(0f, 1f)] public float rootChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has bleeding")]
    #endregion Tooltip
    public bool hasBleedingDamage;
    #region Tooltip
    [Tooltip("The efficiency of projectile's bleeding")]
    #endregion Tooltip
    [Range(0f, 1f)] public float bleedingChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has slow")]
    #endregion Tooltip
    public bool hasSlowDamage;
    #region Tooltip
    [Tooltip("The efficiency of projectile's slow")]
    #endregion Tooltip
    [Range(0f, 1f)] public float slowChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has poison damage")]
    #endregion Tooltip
    public bool hasPoisonDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's poison damage")]
    #endregion
    [Range(0f, 1f)] public float poisonChance = 0.2f;
    #region Tooltip
    [Tooltip("Check if projectile can static")]
    #endregion Tooltip
    public bool hasStaticDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's static effect")]
    #endregion Tooltip
    [Range(0f, 1f)] public float staticChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile can paralyze")]
    #endregion Tooltip
    public bool hasParalyzeDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's paralyze effect")]
    #endregion Tooltip
    [Range(0f, 1f)] public float paralyzeChance = 0f;
    #region Tooltip
    [Tooltip("The chance of projectile's acid effect")]
    #endregion Tooltip
    public bool hasAcidDamage;
    #region Tooltip
    [Tooltip("The efficiency of projectile's acid")]
    #endregion Tooltip
    [Range(0f, 1f)] public float acidEfficiency = 0.4f;
    #region Tooltip
    [Tooltip("Check if projectile has chill damage")]
    #endregion Tooltip
    public bool hasChillDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's chill")]
    #endregion Tooltip
    [Range(0f, 1f)] public float chillChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has frost damage")]
    #endregion Tooltip
    public bool hasFrostDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's frost")]
    #endregion Tooltip
    [Range(0f, 1f)] public float frostChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has blind damage")]
    #endregion Tooltip
    public bool hasBlindDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's blind")]
    #endregion Tooltip
    [Range(0f, 1f)] public float blindChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has reveal damage")]
    #endregion Tooltip
    public bool hasRevealDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's reveal")]
    #endregion Tooltip
    [Range(0f, 1f)] public float revealChance = 0f;
    #region Tooltip
    [Tooltip("The chance of projectile's curse")]
    #endregion Tooltip
    public bool hasCurseDamage;
    #region Tooltip
    [Tooltip("Check enemy's curse chance")]
    #endregion
    [Range(0f, 1f)] public float curseChance = 0f;
    #region Tooltip
    [Tooltip("Check if projectile has fear damage")]
    #endregion Tooltip
    public bool hasFearDamage;
    #region Tooltip
    [Tooltip("The chance of projectile's fear")]
    #endregion Tooltip
    [Range(0f, 1f)] public float fearChance = 0f;
    public bool canDrainHealth;
    #region Tooltip
    [Tooltip("The chance of health drained")]
    #endregion Tooltip
    [Range(0f, 1f)] public float healthDrainChance = 0f;


    #region Header PROJECTILE SPREAD DETAILS
    [Space(10)]
    [Header("PROJECTILE SPREAD DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("This is the minimum spread angle of the projectile. A higher spread means less accuracy. A random spread is calculated between the min and max values")]
    #endregion
    public float projectileSpreadMin = 0f;
    #region Tooltip
    [Tooltip(" This is the maximum spread angle of the projectile.  A higher spread means less accuracy. A random spread is calculated between the min and max values")]
    #endregion
    public float projectileSpreadMax = 0f;

    #region Header PROJECTILE SPREAD DETAILS
    [Space(10)]
    [Header("PROJECTILE CRITICAL DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("Critical hit chance of the projectile")]
    #endregion
    public float criticalHitChance = 0f;
    #region Tooltip
    [Tooltip("Critical hit damage multiplier")]
    #endregion
    public float criticalHitDamageMultiplier = 1.5f;

    #region Header PROJECTILE SPAWN DETAILS
    [Space(10)]
    [Header("PROJECTILE SPAWN DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("This is the minimum number of projectile that are spawned per shot. A random number of projectile are spawned between the minimum and maximum values. ")]
    #endregion
    public int projectileSpawnAmountMin = 1;
    #region Tooltip
    [Tooltip("This is the maximum number of projectile that are spawned per shot. A random number of projectile are spawned between the minimum and maximum values. ")]
    #endregion
    public int projectileSpawnAmountMax = 1;
    #region Tooltip
    [Tooltip("Minimum spawn interval time. The time interval in seconds between spawned projectile is a random value between the minimum and maximum values specified.")]
    #endregion
    public float projectileSpawnIntervalMin = 0f;
    #region Tooltip
    [Tooltip("Maximum spawn interval time. The time interval in seconds between spawned projectile is a random value between the minimum and maximum values specified.")]
    #endregion
    public float projectileSpawnIntervalMax = 0f;

    #region Header PROJECTILE SOUND
    [Space(10)]
    [Header("PROJECTILE SOUND EFFECT DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("The impact sound effect SO for the projectile")]
    #endregion Tooltip
    public SoundEffectSO projectileImpactSoundEffect;

    #region Header PROJECTILE TRAIL DETAILS
    [Space(10)]
    [Header("PROJECTILE TRAIL DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("Selected if a projectile trail is required, otherwise deselect. If selected then the rest of the projectile trail values should be populated")]
    #endregion
    public bool isProjectileTrail = false;
    #region Tooltip
    [Tooltip("Projectile trail lifetime in seconds.")]
    #endregion
    public float projectileTrailTime = 3f;
    #region Tooltip
    [Tooltip("Projectile trail material")]
    #endregion
    public Material projectileTrailMaterial;
    #region Tooltip
    [Tooltip("The starting width for the projectile trail")]
    #endregion
    [Range(0f, 1f)] public float projectileTrailStartWidth;
    #region Tooltip
    [Tooltip("The ending width for the ammo trail")]
    #endregion
    [Range(0f, 1f)] public float projectileTrailEndWidth;

    #region Header MISC
    [Space(10)]
    [Header("PROJECTILE MISC")]
    #endregion
    #region Tooltip
    [Tooltip("The misc info for projectile")]
    #endregion Tooltip
    public bool isCataclysmProjectile = false;
    #region Tooltip
    [Tooltip("The laser check for projectile")]
    #endregion Tooltip
    public bool isLaser = false;

    #region Validation
#if UNITY_EDITOR
    // Validate the scriptable object details entered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(projectileName), projectileName);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(projectilePrefabArray), projectilePrefabArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(projectileMaterial), projectileMaterial);
        if (projectileChargeTime > 0)
            HelperUtilities.ValidateCheckNullValue(this, nameof(projectileChargeMaterial), projectileChargeMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileDamageMin), projectileDamageMin, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileDamageMax), projectileDamageMax, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileRange), projectileRange, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(projectileSpreadMin), projectileSpreadMin, nameof(projectileSpreadMax), projectileSpreadMax, true);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(projectileSpawnAmountMin), projectileSpawnAmountMin, nameof(projectileSpawnAmountMax), projectileSpawnAmountMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(projectileSpawnIntervalMin), projectileSpawnIntervalMin, nameof(projectileSpawnIntervalMax), projectileSpawnIntervalMax, true);
        if (isProjectileTrail)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileTrailTime), projectileTrailTime, false);
            HelperUtilities.ValidateCheckNullValue(this, nameof(projectileTrailMaterial), projectileTrailMaterial);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileTrailStartWidth), projectileTrailStartWidth, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileTrailEndWidth), projectileTrailEndWidth, false);
        }
    }
#endif
    #endregion
}
