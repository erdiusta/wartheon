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
    [Tooltip("Countdown duration before dash attack")]
    #endregion
    public float countdownDurationBeforeDashAttack = 0.4f;
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
    [Tooltip("Check if enemy is a dummy")]
    #endregion
    public bool isDummy = false;
    #region Tooltip
    [Tooltip("Check if weapon animator exists")]
    #endregion
    public bool hasAnimator = false;
    #region Tooltip
    [Tooltip("Weapon elemental bias")]
    #endregion Tooltip
    public ElementalBias elementalBias;
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
    #endregion
    #region Tooltip
    [Tooltip("Enemy starting armor amount")]
    #endregion
    public float physicalResistance = 0;
    #region Tooltip
    [Tooltip("Enemy fire resistance value")]
    #endregion
    public float fireResistance = 0f;
    #region Tooltip
    [Tooltip("Enemy water resistance value")]
    #endregion
    public float waterResistance = 0f;
    #region Tooltip
    [Tooltip("Enemy air resistance value")]
    #endregion
    public float airResistance = 0f;
    #region Tooltip
    [Tooltip("Enemy earth resistance value")]
    #endregion
    public float earthResistance = 0f;
    #region Tooltip
    [Tooltip("Enemy light resistance value")]
    #endregion
    public float lightResistance = 0f;
    #region Tooltip
    [Tooltip("Enemy dark resistance value")]
    #endregion
    public float darkResistance = 0f;
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
    [Header("ATTACK DETAILS")]
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
    public bool hasBlindDamage = false;
    #region Tooltip
    [Tooltip("Check enemy's blind chance")]
    #endregion
    [Range(0f, 1f)] public float blindChance = 0f;

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
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(experiencePoint), experiencePoint, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(chaseDistance), chaseDistance, false);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(physicalResistance), physicalResistance, true);
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
