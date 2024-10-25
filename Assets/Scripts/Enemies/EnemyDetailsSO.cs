using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/Enemy Details")]
public class EnemyDetailsSO : ScriptableObject
{
    #region Header BASE ENEMY DETAILS
    [Space(10)]
    [Header("BASE ENEMY DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("The name of the enemy")]
    #endregion
    public string enemyName;
    #region Tooltip
    [Tooltip("The prefab for the enemy")]
    #endregion
    public GameObject enemyPrefab;
    #region Tooltip
    [Tooltip("Movement details of enemy")]
    #endregion
    public MovementDetailsSO movementDetails;
    #region Tooltip
    [Tooltip("Experience points of enemy")]
    #endregion
    public int experiencePoint = 30;
    #region Tooltip
    [Tooltip("Distance to the player before enemy starts chasing")]
    #endregion
    public float chaseDistance = 50f;
    #region Tooltip
    [Tooltip("Check if enemy has an attack move in a certain distance")]
    #endregion
    public bool hasAttackMove;
    #region Tooltip
    [Tooltip("Distance to the player for attack triggering")]
    #endregion
    public float attackMoveTriggerDistance = 3f;
    #region Tooltip
    [Tooltip("Efficient distance of attack move performed")]
    #endregion
    public float attackMoveEfficentDistance = 8f;
    #region Tooltip
    [Tooltip("Cooldown duration after special attack performed")]
    #endregion
    public float attackMoveBaseCooldown = 8f;
    #region Tooltip
    [Tooltip("Base evasivenes of the enemy to dodge player")]
    #endregion
    public float deflectionValue = .8f;
    #region Tooltip
    [Tooltip("Enemy behaviour type")]
    #endregion
    public EnemyBehaviour enemyBehaviour;
    #region Tooltip
    [Tooltip("Check if enemy is a boss")]
    #endregion
    public bool isEnemyBoss = false;
    #region Tooltip
    [Tooltip("Check if weapon animator exists")]
    #endregion
    public bool hasAnimator = false;

    #region Header PASSIVE
    [Space(10)]
    [Header("PASSIVE")]
    #endregion
    #region Tooltip
    [Tooltip("Enemy starting armor amount")]
    #endregion
    public int enemyArmorValue = 0;
    #region Tooltip
    [Tooltip("Check if enemy has a shield")]
    #endregion
    public bool hasShield = false;
    #region Tooltip
    [Tooltip("Check enemy's deflect chance")]
    #endregion
    [Range(0f, 1f)] public float deflectChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy is a poisonous")]
    #endregion
    public bool isPoisonous = false;
    #region Tooltip
    [Tooltip("Check enemy's poison chance")]
    #endregion
    [Range(0f, 1f)] public float poisonChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has acid")]
    #endregion
    public bool hasAcid = false;
    #region Tooltip
    [Tooltip("Check acid efficiency which absorbes enemy defense")]
    #endregion
    [Range(0f, 1f)] public float acidEfficiency = 0.4f;
    #region Tooltip
    [Tooltip("Check if enemy has stun damage")]
    #endregion
    public bool hasStunDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's stun chance")]
    #endregion
    [Range(0f, 1f)] public float stunChance = 0.4f;
    #region Tooltip
    [Tooltip("Check if enemy has frost damage")]
    #endregion
    public bool hasFrostDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's stun chance")]
    #endregion
    [Range(0f, 1f)] public float frostChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has curse damage")]
    #endregion
    public bool hasCurseDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's curse chance")]
    #endregion
    [Range(0f, 1f)] public float curseChance = 0f;

    #region Header ENEMY MATERIAL
    [Space(10)]
    [Header("ENEMY MATERIAL")]
    #endregion
    #region Tooltip
    [Tooltip("This is the standard lit shader material for the enemy (used after the enemy materializes")]
    #endregion
    public Material enemyStandardMaterial;

    #region Header ENEMY MATERIALIZE SETTINGS
    [Space(10)]
    [Header("ENEMY MATERIALIZE SETTINGS")]
    #endregion
    #region Tooltip
    [Tooltip("The time in seconds that it takes the enemy to materialize")]
    #endregion
    public float enemyMaterializeTime;
    #region Tooltip
    [Tooltip("The shader to be used when the enemy materializes")]
    #endregion
    public Shader enemyMaterializeShader;
    #region Tooltip
    [Tooltip("The colour to use when the enemy materializes.  This is an HDR color so intensity can be set to cause glowing / bloom")]
    #endregion
    [ColorUsage(true, true)]
    public Color enemyMaterializeColor;

    #region Header ENEMY SOUND SETTINGS
    [Space(10)]
    [Header("ENEMY SOUND SETTINGS")]
    #endregion
    #region Tooltip
    [Tooltip("The sound effect for this enemy to be sudden dead")]
    #endregion
    public SoundEffectSO suddenDeathSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to get git")]
    #endregion
    public SoundEffectSO getHitSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to get critical hit")]
    #endregion
    public SoundEffectSO criticalHitSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to be killed")]
    #endregion
    public SoundEffectSO deathSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy's roar")]
    #endregion
    public SoundEffectSO roarSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to attack")]
    #endregion
    public SoundEffectSO attackSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to deflect")]
    #endregion
    public SoundEffectSO deflectSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to be stunned")]
    #endregion
    public SoundEffectSO stunSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to be poisoned")]
    #endregion
    public SoundEffectSO poisonSoundEffect;


    #region Header ENEMY WEAPON SETTINGS
    [Space(10)]
    [Header("ENEMY WEAPON SETTINGS")]
    #endregion
    #region Tooltip
    [Tooltip("The weapon for the enemy - none if the enemy doesn't have a weapon")]
    #endregion
    public WeaponDetailsSO enemyWeapon;
    #region Tooltip
    [Tooltip("The minimum time delay interval in seconds between bursts of enemy shooting.  This value should be greater than 0. " +
        "A random value will be selected between the minimum value and the maximum value")]
    #endregion
    public float firingIntervalMin = 0.1f;
    #region Tooltip
    [Tooltip("The maximum time delay interval in seconds between bursts of enemy shooting.  A random value will be selected between " +
        "the minimum value and the maximum value")]
    #endregion
    public float firingIntervalMax = 1f;
    #region Tooltip
    [Tooltip("The minimum firing duration that the enemy shoots for during a firing burst.  This value should be greater than zero.  " +
        "A random value will be selected between the minimum value and the maximum value.")]
    #endregion
    public float firingDurationMin = 1f;
    #region Tooltip
    [Tooltip("The maximum firing duration that the enemy shoots for during a firing burst.  A random value will be selected between " +
        "the minimum value and the maximum value.")]
    #endregion
    public float firingDurationMax = 2f;
    #region Tooltip
    [Tooltip("Select this if line of sight is required of the player before the enemy fires.  If line of sight isn't selected the enemy " +
        "will fire regardless of obstacles whenever the player is 'in range'")]
    #endregion
    public bool firingLineOfSightRequired;

    #region Header ENEMY HEALTH
    [Space(10)]
    [Header("ENEMY HEALTH")]
    #endregion
    #region Tooltip
    [Tooltip("The health of the enemy for each level")]
    #endregion
    public EnemyHealthDetails[] enemyHealthDetailsArray;
    #region Tooltip
    [Tooltip("Select if has immunity period immediately after being hit.  If so specify the immunity time in seconds in the other field")]
    #endregion
    public bool isImmuneAfterHit = false;
    #region Tooltip
    [Tooltip("Immunity time in seconds after being hit")]
    #endregion
    public float hitImmunityTime;
    #region Tooltip
    [Tooltip("Select to display a health bar for the enemy")]
    #endregion
    public bool isHealthBarDisplayed = false;
    #region Tooltip
    [Tooltip("Select if the enemy is resistant to knockback")]
    #endregion
    public bool hasKnockbackResistance = false;

    #region Header ENEMY DROP SETTINGS
    [Space(10)]
    [Header("ENEMY DROP SETTINGS")]
    #endregion
    #region Tooltip
    [Tooltip("The enemy weapon drops list")]
    #endregion
    public List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponsByLevelList;
    #region Tooltip
    [Tooltip("The enemy passive drops list")]
    #endregion
    public List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> passiveItemsByLevelList;
    #region Tooltip
    [Tooltip("The enemy active drops list")]
    #endregion
    public List<SpawnableObjectsByLevel<ActiveItemDetailsSO>> activeItemsByLevelList;

    #region Header DROP SPAWN CHANCE
    [Space(10)]
    [Header("DROP SPAWN CHANCE")]
    #endregion
    #region Tooltip
    [Tooltip("The minimum probability for spawning a drop")]
    #endregion Tooltip
    [Range(0, 100)] public int dropSpawnChanceMin;
    #region Tooltip
    [Tooltip("The maximum probability for spawning a drop")]
    #endregion Tooltip
    [Range(0, 100)] public int dropSpawnChanceMax;

    #region Header DROP SPAWN DETAILS
    [Space(10)]
    [Header("DROP SPAWN DETAILS")]
    #endregion
    #region Tooltip
    [Tooltip("The minimum number of items to spawn (note that a maximum of 1 of each type of ammo, health, and weapon will be spawned")]
    #endregion
    [Range(0, 3)] public int numberOfItemsToSpawnMin;
    #region Tooltip
    [Tooltip("The maximum number of items to spawn (note that a maximum of 1 of each type of ammo, health, and weapon will be spawned")]
    #endregion
    [Range(0, 3)] public int numberOfItemsToSpawnMax;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(experiencePoint), experiencePoint, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(enemyArmorValue), enemyArmorValue, true);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyStandardMaterial), enemyStandardMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(enemyMaterializeTime), enemyMaterializeTime, true);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyMaterializeShader), enemyMaterializeShader);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingIntervalMin), firingIntervalMin, nameof(firingIntervalMax), firingIntervalMax, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(firingDurationMin), firingDurationMin, nameof(firingDurationMax), firingDurationMax, false);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyHealthDetailsArray), enemyHealthDetailsArray);

        if (isImmuneAfterHit)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(hitImmunityTime), hitImmunityTime, false);
        }
    }
#endif
    #endregion
}
