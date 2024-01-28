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
    [Tooltip("The sprite for the weapon - the sprite should have the 'generate physics shape' option selected ")]
    #endregion Tooltip
    public Sprite weaponSprite;
    #region Tooltip
    [Tooltip("The animator controller for the weapon - weapon should be a melee weapon")]
    #endregion Tooltip
    public RuntimeAnimatorController weaponAnimatorController;
    #region Tooltip
    [Tooltip("Weapon class for the weapon")]
    #endregion Tooltip
    public WeaponClass weaponClass;
    #region Tooltip
    [Tooltip("Weapon wield type")]
    #endregion Tooltip
    public WieldType wieldType;

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
    [Tooltip("The firing sound effect SO for the weapon")]
    #endregion Tooltip
    public SoundEffectSO weaponFiringSoundEffect;
    #region Tooltip
    [Tooltip("The reloading sound effect SO for the weapon")]
    #endregion Tooltip
    public SoundEffectSO weaponReloadingSoundEffect;

    #region Header WEAPON RANGED/MELEE/SHIELD CHECK
    [Space(10)]
    [Header("WEAPON RANGED/MELEE/SHIELD CHECK")]
    #endregion
    #region Tooltip
    [Tooltip("Select if the weapon is a melee weapon")]
    #endregion Tooltip
    public bool isMeleeWeapon = false;
    #region Tooltip
<<<<<<< Updated upstream
=======
    [Tooltip("Select if the weapon is a shield")]
    #endregion Tooltip
    public bool isShield = false;
    #region Tooltip
    [Tooltip("Weapon Fire Rate - 0.2 means 5 shots a second")]
    #endregion Tooltip
    public float weaponFireRate = 0.2f;

    #region Header SHIELD OPERATING VALUES
    [Space(10)]
    [Header("SHIELD OPERATING VALUES")]
    #endregion
    #region Tooltip
    [Tooltip("Probability of deflecting projectiles")]
    #endregion Tooltip
    [Range(0f, 1f)] public float projectileDeflectChance = 0.4f;

    #region Header MELEE WEAPON OPERATING VALUES
    [Space(10)]
    [Header("MELEE WEAPON OPERATING VALUES")]
    #endregion
    #region Tooltip
>>>>>>> Stashed changes
    [Tooltip("Select radius amount if weapon is a melee weapon")]
    #endregion Tooltip
    public float circleRadius = 0.8f;
    #region Tooltip
    [Tooltip("Min melee damage of the weapon")]
    #endregion
    public int meleeDamageMin = 4;
    #region Tooltip
    [Tooltip("Max melee damage of the weapon")]
    #endregion
    public int meleeDamageMax = 7;
<<<<<<< Updated upstream
    #region Tooltip
    [Tooltip("Melee weapon attack cooldown duration")]
    #endregion
    public float meleeAttackCooldown = 0.3f;
=======

    #region Header RANGED WEAPON OPERATING VALUES
    [Space(10)]
    [Header("RANGED WEAPON OPERATING VALUES")]
    #endregion
>>>>>>> Stashed changes
    #region Tooltip
    [Tooltip("Select if the weapon has infinite projectile")]
    #endregion Tooltip
    public bool hasInfiniteProjectile = false;
    #region Tooltip
    [Tooltip("Select if the weapon has infinite clip capacity")]
    #endregion Tooltip
    public bool hasInfiniteClipCapacity = false;
    #region Tooltip
    [Tooltip("The weapon capacity - shots before a reload")]
    #endregion Tooltip
    public int weaponClipProjectileCapacity = 6;
    #region Tooltip
    [Tooltip("Weapon ammo capacity - the maximum number of rounds at that can be held for this weapon")]
    #endregion Tooltip
    public int weaponProjectileCapacity = 100;

    #region Tooltip
    [Tooltip("Weapon Precharge Time - time in seconds to hold fire button down before firing")]
    #endregion Tooltip
    public float weaponPrechargeTime = 0f;
    #region Tooltip
    [Tooltip("This is the weapon reload time in seconds")]
    #endregion Tooltip
    public float weaponReloadTime = 0f;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (isMeleeWeapon)
        {
            HelperUtilities.ValidateCheckNullValue(this, nameof(weaponAnimatorController), weaponAnimatorController);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(circleRadius), circleRadius, true);
<<<<<<< Updated upstream
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(meleeAttackCooldown), meleeAttackCooldown, true);
=======
        }
        else if (isShield)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(projectileDeflectChance), projectileDeflectChance, true);
>>>>>>> Stashed changes
        }
        else
        {
            HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
            HelperUtilities.ValidateCheckNullValue(this, nameof(weaponCurrentProjectile), weaponCurrentProjectile);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponFireRate), weaponFireRate, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);

            if (!hasInfiniteProjectile)
            {
                HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponProjectileCapacity), weaponProjectileCapacity, false);
            }

            if (!hasInfiniteClipCapacity)
            {
                HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponClipProjectileCapacity), weaponClipProjectileCapacity, false);
            }
        }
    }
#endif
    #endregion Validation
}

