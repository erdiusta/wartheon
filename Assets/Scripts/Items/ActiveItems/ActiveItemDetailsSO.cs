using UnityEngine;

[CreateAssetMenu(fileName = "ActiveItem_", menuName = "Scriptable Objects/Active Items")]
public class ActiveItemDetailsSO : ScriptableObject
{
    #region Header ACTIVE BASE DETAILS
    [Space(10)]
    [Header("ACTIVE BASE DETAILS")]
    #endregion 
    #region Tooltip
    [Tooltip("Active item name")]
    #endregion Tooltip
    public string activeItemName;
    public ActiveItemType activeItemType = ActiveItemType.Generic;


    #region Header PROJECTILE SPRITE, PREFAB & MATERIALS
    [Space(10)]
    [Header("PROJECTILE SPRITE, PREFAB & MATERIALS")]
    #endregion
    #region Tooltip
    [Tooltip("Sprite to be used for the projectile")]
    #endregion
    public Sprite activeItemSprite;
    #region Tooltip
    [Tooltip("Populate with the prefab to be used for the projectile.  If multiple prefabs are specified then a random prefab from the array will be selecetd.  " +
        "The prefab can be an projectile pattern - as long as it conforms to the IFireable interface.")]
    #endregion
    public GameObject[] activeItemPrefabArray;
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

    #region Header ACTIVE ITEM CONFIGURATION
    [Space(10)]
    [Header("ACTIVE ITEM CONFIGURATION")]
    #endregion Header WEAPON CONFIGURATION
    #region Tooltip
    [Tooltip("Active item shoot effect SO - contains particle effect parameters to be used in conjunction with the activeItemShootEffectPrefab ")]
    #endregion Tooltip
    public WeaponShootEffectSO activeItemShootEffect;
    #region Tooltip
    [Tooltip("The swing/fire sound effect SO for the active item")]
    #endregion Tooltip
    public SoundEffectSO activeItemSwingSoundEffect;
    #region Tooltip
    [Tooltip("The impact sound effect SO for the active item")]
    #endregion Tooltip
    public SoundEffectSO activeItemImpactSoundEffect;

    #region Header RANGED WEAPON OPERATING VALUES
    [Space(10)]
    [Header("RANGED WEAPON OPERATING VALUES")]
    #endregion
    #region Tooltip
    [Tooltip("Select if the weapon has no projectile number limit")]
    #endregion Tooltip
    public bool hasNoProjectileNumberLimit;
    #region Tooltip
    [Tooltip("Active item projectile capacity - the maximum number of projectiles that can be held for this weapon")]
    #endregion Tooltip
    public int activeItemProjectileCapacity = 5;

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
    [Tooltip("The countdown until the item burst")]
    #endregion
    public float countDown = 3f;
    #region Tooltip
    [Tooltip("The min burst damage each projectile deals")]
    #endregion
    public int burstDamageMin = 10;
    #region Tooltip
    [Tooltip("The max burst damage each projectile deals")]
    #endregion
    public int burstDamageMax = 20;
    #region Tooltip
    [Tooltip("The blast radius of the item")]
    #endregion
    public float blastRadius = 5f;
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

    #region Header PASSIVE
    [Space(10)]
    [Header("ACTIVE ITEM PASSIVE EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("Check if item has acid")]
    #endregion Tooltip
    public bool hasAcid;
    #region Tooltip
    [Tooltip("The efficiency of item's acid")]
    #endregion Tooltip
    [Range(0f, 1f)] public float acidEfficiency = 0.4f;
    #region Tooltip
    [Tooltip("Check if item has poison damage")]
    #endregion Tooltip
    public bool isPoisonous;
    #region Tooltip
    [Tooltip("The chance of item's poison damage")]
    #endregion
    [Range(0f, 1f)] public float poisonChance = 0.2f;
    #region Tooltip
    [Tooltip("Check if item has bleeding damage")]
    #endregion Tooltip
    public bool hasBleedingDamage;
    #region Tooltip
    [Tooltip("The chance of item's bleeding damage")]
    #endregion Tooltip
    [Range(0f, 1f)] public float bleedingChance = 0.2f;
    #region Tooltip
    [Tooltip("Check if item has stun damage")]
    #endregion Tooltip
    public bool hasStunDamage;
    #region Tooltip
    [Tooltip("The chance of item's stun")]
    #endregion Tooltip
    [Range(0f, 1f)] public float stunChance = 0.2f;
    #region Tooltip
    [Tooltip("Check if item has slow damage")]
    #endregion Tooltip
    public bool hasSlowDamage;
    #region Tooltip
    [Tooltip("The chance of item's slow damage")]
    #endregion Tooltip
    [Range(0f, 1f)] public float slowChance = 0.2f;

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
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckEmptyString(this, nameof(activeItemName), activeItemName);
        HelperUtilities.ValidateCheckNullValue(this, nameof(activeItemSprite), activeItemSprite);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(activeItemPrefabArray), activeItemPrefabArray);
    }
#endif
    #endregion Validation
}
