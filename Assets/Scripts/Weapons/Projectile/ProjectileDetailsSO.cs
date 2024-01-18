using System.Collections;
using System.Collections.Generic;
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
    [Tooltip("Sprite to be used for the projectile")]
    #endregion
    public Sprite projectileSprite;
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
    [Tooltip("The damage each projectile deals")]
    #endregion
    public int projectileDamage = 1;
    #region Tooltip
    [Tooltip("The minimum speed of the projectile - the speed will be a random value between the min and max")]
    #endregion
    public float projectileSpeedMin = 20f;
    #region Tooltip
    [Tooltip("The maximum speed of the projectile - the speed will be a random value between the min and max")]
    #endregion
    public float projectileSpeedMax = 20f;
    #region Tooltip
    [Tooltip("The range of the projectile (or projectile pattern) in unity units")]
    #endregion
    public float projectileRange = 20f;
    #region Tooltip
    [Tooltip("The rotation speed in degrees per second of the projectile pattern")]
    #endregion
    public float projectileRotationSpeed = 1f;

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

    #region Validation
#if UNITY_EDITOR
    // Validate the scriptable object details entered
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(projectileName), projectileName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(projectileSprite), projectileSprite);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(projectilePrefabArray), projectilePrefabArray);
        HelperUtilities.ValidateCheckNullValue(this, nameof(projectileMaterial), projectileMaterial);
        if (projectileChargeTime > 0)
            HelperUtilities.ValidateCheckNullValue(this, nameof(projectileChargeMaterial), projectileChargeMaterial);
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileDamage), projectileDamage, false);
        HelperUtilities.ValidateCheckPositiveRange(this, nameof(projectileSpeedMin), projectileSpeedMin, nameof(projectileSpeedMax), projectileSpeedMax, false);
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
