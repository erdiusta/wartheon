using UnityEngine;

[CreateAssetMenu(fileName = "WeaponDetails_", menuName = "Scriptable Objects/Weapons/Weapon Details")]
public class WeaponDetailsSO : ScriptableObject
{
    #region Header WEAPON BASE DETAILS
    [Space(10)]
    [Header("WEAPON BASE DETAILS")]
    #endregion Header WEAPON BASE DETAILS
    #region Tooltip
    [Tooltip("Weapon name")]
    #endregion Tooltip
    public string weaponName;
    #region Tooltip
    [Tooltip("Weapon name")]
    #endregion Tooltip
    public WeaponTitle weaponTitle;
    #region Tooltip
    [Tooltip("The front sprite for the weapon - the sprite should have the 'generate physics shape' option selected ")]
    #endregion Tooltip
    public Sprite weaponFrontSprite;
    #region Tooltip
    [Tooltip("The rear sprite for the weapon - the sprite should have the 'generate physics shape' option selected ")]
    #endregion Tooltip
    public Sprite weaponRearSprite;
    #region Tooltip
    [Tooltip("The animator controller for the weapon - weapon should be a melee weapon")]
    #endregion Tooltip
    public RuntimeAnimatorController weaponAnimatorController;
    #region Tooltip
    [Tooltip("The animator controller fot hovering during the drop")]
    #endregion Tooltip
    public RuntimeAnimatorController weaponHoverAnimatorController;
    #region Tooltip
    [Tooltip("Weapon class for the weapon")]
    #endregion Tooltip
    public WeaponClass weaponClass;
    #region Tooltip
    [Tooltip("Weapon wield type")]
    #endregion Tooltip
    public WieldType wieldType;
    #region Tooltip
    [Tooltip("Price of the weapon")]
    #endregion Tooltip
    public int price;

    #region Header PASSIVE
    [Space(10)]
    [Header("WEAPON PASSIVE EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("Check if weapon has sudden death chance")]
    #endregion Tooltip
    public bool canKillSuddenly;
    #region Tooltip
    [Tooltip("The chance of weapon's sudden death")]
    #endregion Tooltip
    [Range(0f, 1f)] public float suddenKillChance = 0.1f;
    #region Tooltip
    [Tooltip("Check if weapon has acid")]
    #endregion Tooltip
    public bool hasAcid;
    #region Tooltip
    [Tooltip("The efficiency of weapon's acid")]
    #endregion Tooltip
    [Range(0f, 1f)] public float acidEfficiency = 0.4f;
    #region Tooltip
    [Tooltip("Check if weapon has stun damage")]
    #endregion Tooltip
    public bool hasStunDamage;
    #region Tooltip
    [Tooltip("The chance of weapon's stun")]
    #endregion Tooltip
    [Range(0f, 1f)] public float stunChance = 0.2f;

    #region Header WEAPON CONFIGURATION
    [Space(10)]
    [Header("WEAPON CONFIGURATION")]
    #endregion Header WEAPON CONFIGURATION
    #region Tooltip
    [Tooltip("Weapon Shoot Position - the offset position for the end of the weapon from the sprite pivot pont")]
    #endregion Tooltip
    public Vector3 weaponShootPosition;
    #region Tooltip
    [Tooltip("Weapon current projectile")]
    #endregion Tooltip
    public ProjectileDetailsSO weaponCurrentProjectile;
    #region Tooltip
    [Tooltip("Weapon shoot effect SO - contains particle effect parameters to be used in conjunction with the weaponShootEffectPrefab ")]
    #endregion Tooltip
    public WeaponShootEffectSO weaponShootEffect;
    #region Tooltip
    [Tooltip("The swing/fire sound effect SO for the weapon")]
    #endregion Tooltip
    public SoundEffectSO weaponSwingSoundEffect;
    #region Tooltip
    [Tooltip("The impact sound effect SO for the weapon")]
    #endregion Tooltip
    public SoundEffectSO weaponImpactSoundEffect;

    #region Header WEAPON RANGED/MELEE/SHIELD CHECK
    [Space(10)]
    [Header("WEAPON RANGED/MELEE/SHIELD CHECK")]
    #endregion
    #region Tooltip
    [Tooltip("Select if the weapon is a melee weapon")]
    #endregion Tooltip
    public bool isMeleeWeapon = false;
    #region Tooltip
    [Tooltip("Select if the weapon is a shield")]
    #endregion Tooltip
    public bool isShield = false;

    #region Header SHIELD OPERATING VALUES
    [Space(10)]
    [Header("SHIELD OPERATING VALUES")]
    #endregion
    #region Tooltip
    [Tooltip("Probability of deflecting projectiles")]
    #endregion Tooltip
    [Range(0f, 1f)] public float projectileDeflectRatio = 0.4f;

    #region Header MELEE WEAPON OPERATING VALUES
    [Space(10)]
    [Header("MELEE WEAPON OPERATING VALUES")]
    #endregion
    #region Tooltip
    [Tooltip("Select radius amount if weapon is a melee weapon")]
    #endregion Tooltip
    public float circleRadius = 0.8f;
    #region Tooltip
    [Tooltip("Check if the weapon is physical damaged weapon or not")]
    #endregion Tooltip
    public bool hasPhysicalDamage = false;
    #region Tooltip
    [Tooltip("Min melee damage of the weapon")]
    #endregion
    public int meleeDamageMin = 4;
    #region Tooltip
    [Tooltip("Max melee damage of the weapon")]
    #endregion
    public int meleeDamageMax = 7;
    #region Tooltip
    [Tooltip("Critical hit chance of the weapon")]
    #endregion
    public float criticalHitChance = 0.1f;
    #region Tooltip
    [Tooltip("Critical hit damage multiplier")]
    #endregion
    public float criticalHitDamageMultiplier = 2f;
    #region Tooltip
    [Tooltip("Weapon Fire Rate - 0.2 means 5 shots a second")]
    #endregion Tooltip
    public float weaponCooldownDuration = 0.2f;
    #region Tooltip
    [Tooltip("Weapon base handling rate to hit enemy successfully")]
    #endregion Tooltip
    public float weaponBaseHandling = 0.8f;
    #region Tooltip
    [Tooltip("Check if melee weapon has slash fx")]
    #endregion
    public bool hasSwing = false;
    #region Tooltip
    [Tooltip("Check if melee weapon has sweep fx")]
    #endregion
    public bool hasSweep = false;
    #region Tooltip
    [Tooltip("Check if melee weapon has thrust fx")]
    #endregion
    public bool hasThrust = false;

    #region Header RANGED WEAPON OPERATING VALUES
    [Space(10)]
    [Header("RANGED WEAPON OPERATING VALUES")]
    #endregion
    #region Tooltip
    [Tooltip("Select if the weapon has infinite projectile")]
    #endregion Tooltip
    public bool hasInfiniteProjectile = false;
    #region Tooltip
    [Tooltip("Weapon ammo capacity - the maximum number of rounds at that can be held for this weapon")]
    #endregion Tooltip
    public int weaponProjectileCapacity = 100;

    #region Tooltip
    [Tooltip("Weapon Precharge Time - time in seconds to hold fire button down before firing")]
    #endregion Tooltip
    public float weaponPrechargeTime = 0f;

    public Weapon GetWeapon()
    {
        Weapon weapon = new Weapon
        {
            weaponDetails = this,
            weaponRemainingProjectile = weaponProjectileCapacity
        };

        return weapon;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (isMeleeWeapon)
        {
            HelperUtilities.ValidateCheckNullValue(this, nameof(weaponAnimatorController), weaponAnimatorController);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(circleRadius), circleRadius, true);
        }
        else if (isShield)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileDeflectRatio), projectileDeflectRatio, true);
        }
        else
        {
            HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
            HelperUtilities.ValidateCheckNullValue(this, nameof(weaponCurrentProjectile), weaponCurrentProjectile);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponCooldownDuration), weaponCooldownDuration, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);

            if (!hasInfiniteProjectile)
            {
                HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponProjectileCapacity), weaponProjectileCapacity, false);
            }
        }
    }
#endif
    #endregion Validation
}

