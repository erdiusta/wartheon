using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "EnemyDetails_", menuName = "Scriptable Objects/Enemy/Enemy Details")]
public class EnemyDetailsSO : ScriptableObject
{
    #region Header BASE ENEMY DETAILS
    [Space(10)]
    [Header("BASE ENEMY DETAILS")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("The name of the enemy")]
    #endregion
    public string enemyName;
    [TextArea(6,15)]
    #region Tooltip
    [Tooltip("The name details of the enemy")]
    #endregion
    public string enemyDetails;
    #region Tooltip
    [Tooltip("The name of the enemy as enum")]
    #endregion
    public EnemyCategory enemyCategory;
    #region Tooltip
    [Tooltip("The prefab for the enemy")]
    #endregion
    public GameObject enemyPrefab;
    #region Tooltip
    [Tooltip("The type of the enemy")]
    #endregion
    public EnemyType enemyType;
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

    #region ATTACK MOVE DETAILS
    [Space(10)]
    [Header("ATTACK MOVE DETAILS")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("Distance to the player for attack triggering")]
    #endregion
    public float attackMoveTriggerDistance = 3f;
    #region Tooltip
    [Tooltip("Efficient distance of attack move performed")]
    #endregion
    public float attackMoveEfficentDistance = 8f;
    #region Tooltip
    [Tooltip("Countdown duration before dash attack")]
    #endregion
    public float countdownDurationBeforeDashAttack = 0.4f;
    #region Tooltip
    [Tooltip("Attack move dash duration")]
    #endregion
    public float attackMoveDashDuration = 0.3f;
    #region Tooltip
    [Tooltip("Cooldown duration after special attack performed")]
    #endregion
    public float attackMoveBaseCooldown = 0.6f;

    [Space(10)]
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
    [Tooltip("Check if enemy is a summoned minion")]
    #endregion
    public bool isSummonedMinion = false;
    #region Tooltip
    [Tooltip("Check if enemy is a decoy")]
    #endregion
    public bool isDecoy = false;
    #region Tooltip
    [Tooltip("Weapon elemental compound rate")]
    #endregion Tooltip
    public float elementalForgeRate;
    #region Tooltip
    [Tooltip("Enemy's minion details")]
    #endregion
    public EnemyDetailsSO enemyMinionDetails;

    #region Header PASSIVE
    [Space(10)]
    [Header("PASSIVE")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("Enemy starting armor amount")]
    #endregion
    public float physicalResistance = 0;
    #region Tooltip
    [Tooltip("Enemy magic resistance value")]
    #endregion
    public float magicResistance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has a shield")]
    #endregion
    public bool hasShield = false;
    #region Tooltip
    [Tooltip("Check enemy's deflect chance")]
    #endregion
    [Range(0f, 1f)] public float deflectChance = 0f;

    #region Header ATTACK
    [Space(10)]
    [Header("IMMUNITY DETAILS")]
    [Space(10)]
    #endregion
    public bool isImmuneToBleeding;
    public bool isImmuneToStun;
    public bool isImmuneToSlow;
    public bool isImmuneToBurn;
    public bool isImmuneToPoison;
    public bool isImmuneToRoot;
    public bool isImmuneToFrost;
    public bool isImmuneToParalyze;
    public bool isImmuneToBlind;
    public bool isImmuneToCurse;
    public bool isImmuneToFear;

    #region Header ATTACK
    [Space(10)]
    [Header("ATTACK DETAILS")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("Enemy dealt damage by melee min")]
    #endregion
    public int dealtMeleeDamageMin = 12;
    #region Tooltip
    [Tooltip("Enemy dealt damage by melee max")]
    #endregion
    public int dealtMeleeDamageMax = 15;
    #region Tooltip
    [Tooltip("Check if enemy can warm")]
    #endregion
    public bool canWarm = false;
    #region Tooltip
    [Tooltip("Check enemy's warm chance")]
    #endregion
    [Range(0f, 1f)] public float warmChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy can burn")]
    #endregion
    public bool canBurn = false;
    #region Tooltip
    [Tooltip("Check enemy's burn chance")]
    #endregion
    [Range(0f, 1f)] public float burnChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has bleeding")]
    #endregion Tooltip
    public bool hasBleedingDamage;
    #region Tooltip
    [Tooltip("The efficiency of enemy's bleeding")]
    #endregion Tooltip
    [Range(0f, 1f)] public float bleedingChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has slow")]
    #endregion Tooltip
    public bool hasSlowDamage;
    #region Tooltip
    [Tooltip("The efficiency of enemy's slow")]
    #endregion Tooltip
    [Range(0f, 1f)] public float slowChance = 0f;
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
    [Tooltip("Check if enemy has root damage")]
    #endregion
    public bool hasRootDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's root chance")]
    #endregion
    [Range(0f, 1f)] public float rootChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has chill damage")]
    #endregion
    public bool hasChillDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's chill chance")]
    #endregion
    [Range(0f, 1f)] public float chillChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has frost damage")]
    #endregion
    public bool hasFrostDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's stun chance")]
    #endregion
    [Range(0f, 1f)] public float frostChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has static damage")]
    #endregion
    public bool hasStaticDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's static chance")]
    #endregion
    [Range(0f, 1f)] public float staticChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has paralyze damage")]
    #endregion
    public bool hasParalyzeDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's paralyze chance")]
    #endregion
    [Range(0f, 1f)] public float paralyzeChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has curse damage")]
    #endregion
    public bool hasCurseDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's curse chance")]
    #endregion
    [Range(0f, 1f)] public float curseChance = 0f;
    #region Tooltip
    [Tooltip("Check if enemy has fear damage")]
    #endregion Tooltip
    public bool hasFearDamage;
    #region Tooltip
    [Tooltip("The chance of enemy's fear")]
    #endregion Tooltip
    [Range(0f, 1f)] public float fearChance = 0f;
    public bool canDrainHealth;
    #region Tooltip
    [Tooltip("The chance of player's health drained")]
    #endregion Tooltip
    [Range(0f, 1f)] public float healthDrainChance = 0f;
    public bool hasBlindDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's blind chance")]
    #endregion
    [Range(0f, 1f)] public float blindChance = 0f;

    #region Header ENEMY MATERIALIZE SETTINGS
    [Space(10)]
    [Header("ENEMY MATERIALIZE SETTINGS")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("The time in seconds that it takes the enemy to materialize")]
    #endregion
    public float enemyMaterializeTime;
    #region Tooltip
    [Tooltip("The colour to use when the enemy materializes.  This is an HDR color so intensity can be set to cause glowing / bloom")]
    #endregion
    [ColorUsage(true, true)]
    public Color enemyMaterializeColor;

    #region Header ENEMY SOUND SETTINGS
    [Space(10)]
    [Header("ENEMY SOUND SETTINGS")]
    [Space(10)]
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
    [Tooltip("The sound effect for this enemy's charge")]
    #endregion
    public SoundEffectSO chargeSoundEffect;
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
    [Tooltip("The sound effect for this enemy to be rooted")]
    #endregion
    public SoundEffectSO rootSoundEffect;
    #region Tooltip
    [Tooltip("The sound effect for this enemy to be poisoned")]
    #endregion
    public SoundEffectSO poisonSoundEffect;


    #region Header ENEMY WEAPON SETTINGS
    [Space(10)]
    [Header("ENEMY WEAPON SETTINGS")]
    [Space(10)]
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
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("The health of the enemy for each level")]
    #endregion
    public EnemyHealthDetails[] enemyHealthDetailsArray;
    #region Tooltip
    [Tooltip("The damage of the enemy for each level")]
    #endregion
    public EnemyDamageDetails[] enemyDamageDetailsArray;
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
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("The enemy primary passive drops list")]
    #endregion
    public List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> primaryPassiveItemsByLevelList;
    #region Tooltip
    [Tooltip("The enemy weapon drops list")]
    #endregion
    public List<SpawnableObjectsByLevel<WeaponDetailsSO>> weaponsByLevelList;
    #region Tooltip
    [Tooltip("The enemy secondary passive drops list")]
    #endregion
    public List<SpawnableObjectsByLevel<PassiveItemDetailsSO>> secondaryPassiveItemsByLevelList;

    #region Header DROP SPAWN CHANCE
    [Space(10)]
    [Header("DROP SPAWN CHANCE")]
    [Space(10)]
    #endregion
    #region Tooltip
    [Tooltip("The minimum probability for spawning a drop")]
    #endregion Tooltip
    [Range(0, 100)] public int primaryPassiveDropChanceMax;
    #region Tooltip
    [Tooltip("The minimum probability for spawning a drop")]
    #endregion Tooltip
    [Range(0, 100)] public int dropSpawnChanceMin;
    #region Tooltip
    [Tooltip("The maximum probability for spawning a drop")]
    #endregion Tooltip
    [Range(0, 100)] public int dropSpawnChanceMax;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(enemyName), enemyName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(enemyPrefab), enemyPrefab);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(experiencePoint), experiencePoint, true);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(enemyMaterializeTime), enemyMaterializeTime, true);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(enemyHealthDetailsArray), enemyHealthDetailsArray);
    }
#endif
    #endregion
}
