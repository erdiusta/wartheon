using UnityEngine;
using System.Collections.Generic;

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
    [Tooltip("Weapon level")]
    #endregion Tooltip
    public Rarity rarity;
    #region Tooltip
    [Tooltip("Weapon class for the weapon")]
    #endregion Tooltip
    public WeaponClass weaponClass;
    #region Tooltip
    [Tooltip("Weapon hit speed")]
    #endregion Tooltip
    public WeaponHitSpeed weaponHitSpeed;
    #region Tooltip
    [Tooltip("Weapon wield type")]
    #endregion Tooltip
    public WieldType wieldType;
    #region Tooltip
    [Tooltip("Weapon elemental bias")]
    #endregion Tooltip
    public ElementalBias elementalBias;
    #region Tooltip
    [Tooltip("Weapon elemental compound rate")]
    #endregion Tooltip
    public float elementalForgeRate;
    #region Tooltip
    [Tooltip("Price of the weapon")]
    #endregion Tooltip
    public int price;
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
    [Tooltip("The ui sprite for the weapon")]
    #endregion Tooltip
    public Sprite weaponUISprite;

    #region Header DROP MODIFIERS
    [Space(10)]
    [Header("DROP MODIFIERS")]
    #endregion
    #region Tooltip
    [Tooltip("Weapon's base unique modifier")]
    #endregion Tooltip
    public BoostType baseUniqueModifier;
    #region Tooltip
    [Tooltip("Weapon's base type modifier")]
    #endregion Tooltip
    public BoostType baseTypeModifier;
    #region Tooltip
    [Tooltip("Weapon's additonal modifier type pool")]
    #endregion Tooltip
    public List<BoostType> additionalModifierPoolForType;

    #region Header PASSIVE
    [Space(10)]
    [Header("WEAPON PASSIVE EFFECT")]
    #endregion
    #region Tooltip
    [Tooltip("The chance of weapon's sudden death")]
    #endregion Tooltip
    [Range(0f, 1f)] public float suddenKillChance = 0f;
    #region Tooltip
    [Tooltip("The efficiency of weapon's bleeding")]
    #endregion Tooltip
    [Range(0f, 1f)] public float bleedingChance = 0f;
    #region Tooltip
    [Tooltip("The efficiency of weapon's slow")]
    #endregion Tooltip
    [Range(0f, 1f)] public float slowChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's warm effect")]
    #endregion Tooltip
    [Range(0f, 1f)] public float warmChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's burn effect")]
    #endregion Tooltip
    [Range(0f, 1f)] public float burnChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's poison")]
    #endregion Tooltip
    [Range(0f, 1f)] public float poisonChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's static effect")]
    #endregion Tooltip
    [Range(0f, 1f)] public float staticChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's paralyze effect")]
    #endregion Tooltip
    [Range(0f, 1f)] public float paralyzeChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's stun")]
    #endregion Tooltip
    [Range(0f, 1f)] public float stunChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's root")]
    #endregion Tooltip
    [Range(0f, 1f)] public float rootChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's chill")]
    #endregion Tooltip
    [Range(0f, 1f)] public float chillChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's frost")]
    #endregion Tooltip
    [Range(0f, 1f)] public float frostChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's blind")]
    #endregion Tooltip
    [Range(0f, 1f)] public float blindChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's reveal")]
    #endregion Tooltip
    [Range(0f, 1f)] public float revealChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's curse")]
    #endregion Tooltip
    [Range(0f, 1f)] public float curseChance = 0f;
    #region Tooltip
    [Tooltip("The chance of weapon's fear")]
    #endregion Tooltip
    [Range(0f, 1f)] public float fearChance = 0f;
    #region Tooltip
    [Tooltip("The chance of enemy's health drained")]
    #endregion Tooltip
    [Range(0f, 1f)] public float healthDrainChance = 0f;

    #region Header WEAPON CONFIGURATION
    [Space(10)]
    [Header("WEAPON CONFIGURATION")]
    #endregion Header WEAPON CONFIGURATION
    #region Tooltip
    [Tooltip("Weapon Right Shoot Position - the offset position for the end of the weapon from the sprite pivot pont")]
    #endregion Tooltip
    public Vector3 weaponRightShootPosition;
    #region Tooltip
    [Tooltip("Weapon Left Shoot Position - the offset position for the end of the weapon from the sprite pivot pont")]
    #endregion Tooltip
    public Vector3 weaponLeftShootPosition;
    #region Tooltip
    [Tooltip("Weapon Up Shoot Position - the offset position for the end of the weapon from the sprite pivot pont")]
    #endregion Tooltip
    public Vector3 weaponUpShootPosition;
    #region Tooltip
    [Tooltip("Weapon Down Shoot Position - the offset position for the end of the weapon from the sprite pivot pont")]
    #endregion Tooltips
    public Vector3 weaponDownShootPosition;
    #region Tooltip
    [Tooltip("Weapon current projectile")]
    #endregion Tooltip
    public ProjectileDetailsSO weaponCurrentProjectile;
    #region Tooltip
    [Tooltip("Weapon secondary projectile if has")]
    #endregion Tooltip
    public ProjectileDetailsSO weaponSecondaryProjectile;
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
    [Tooltip("Armor rate provided by the shield")]
    #endregion Tooltip
    [Range(0f, 1f)] public float shieldArmorRate = 0.1f;
    #region Tooltip
    [Tooltip("Probability of deflecting projectiles")]
    #endregion Tooltip
    [Range(0f, 1f)] public float blockChance = 0.4f;


    #region Header MELEE WEAPON OPERATING VALUES
    [Space(10)]
    [Header("MELEE WEAPON OPERATING VALUES")]
    #endregion
    #region Tooltip
    [Tooltip("Select radius amount if weapon is a melee weapon")]
    #endregion Tooltip
    public float circleRadius = 0.8f;
    #region Tooltip
    [Tooltip("Select box length amount if weapon is a melee thrust weapon")]
    #endregion Tooltipt
    public float boxLength = 1.2f;
    #region Tooltip
    [Tooltip("Select box height amount if weapon is a melee thrust weapon")]
    #endregion Tooltip
    public float boxHeight = 2.6f;
    #region Tooltip
    [Tooltip("Select cone length amount if weapon attack cone shaped attack")]
    #endregion Tooltip
    public float coneLength = 3f;
    #region Tooltip
    [Tooltip("Select cone angle amount weapon covers")]
    #endregion Tooltip
    public float coneAngle = 35f;
    #region Tooltip
    [Tooltip("Check if the weapon is physical damaged weapon or not")]
    #endregion Tooltip
    public bool hasPhysicalDamage = false;
    #region Tooltip
    [Tooltip("Min melee damage of the weapon")]
    #endregion
    public int physicalDamageMin = 4;
    #region Tooltip
    [Tooltip("Max melee damage of the weapon")]
    #endregion
    public int physicalDamageMax = 7;
    #region Tooltip
    [Tooltip("Min magic damage of the weapon")]
    #endregion
    public int magicDamageMin = 0;
    #region Tooltip
    [Tooltip("Max magic damage of the weapon")]
    #endregion
    public int magicDamageMax = 0;
    #region Tooltip
    [Tooltip("Elemental bias of the melee damage")]
    #endregion
    public ElementalBias meleeDamageElementalBias;
    #region Tooltip
    [Tooltip("Second min melee damage of the weapon")]
    #endregion
    public int secondMeleeDamageMin = 4;
    #region Tooltip
    [Tooltip("Second max melee damage of the weapon")]
    #endregion
    public int secondMeleeDamageMax = 7;
    #region Tooltip
    [Tooltip("Elemental bias of the second melee damage")]
    #endregion
    public ElementalBias secondMeleeDamageElementalBias;
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
    public float weaponAttackRating = 0.8f;
    #region Tooltip
    [Tooltip("Probability of evading damages")]
    #endregion Tooltip
    [Range(0f, 1f)] public float dodgeChance = 0.4f;

    #region Tooltip
    [Tooltip("Check if melee weapon has slash fx")]
    #endregion
    public bool hasSwing = false;
    #region Tooltip
    [Tooltip("Check if melee weapon has thrust fx")]
    #endregion
    public bool hasThrust = false;

    #region Tooltip
    [Tooltip("Weapon Precharge Time - time in seconds to hold fire button down before firing")]
    #endregion Tooltip
    public float weaponPrechargeTime = 0f;

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (isShield)
        {
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(blockChance), blockChance, true);
        }
        else
        {
            HelperUtilities.ValidateCheckEmptyString(this, nameof(weaponName), weaponName);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponCooldownDuration), weaponCooldownDuration, false);
            HelperUtilities.ValidateCheckPositiveValue(this, nameof(weaponPrechargeTime), weaponPrechargeTime, true);
        }
    }
#endif
    #endregion Validation
}
