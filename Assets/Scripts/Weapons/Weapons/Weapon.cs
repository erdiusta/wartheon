using System.Collections.Generic;
using UnityEngine;

public class Weapon : ItemGeneric
{
    [Header("Definition")]
    public WeaponDetailsSO weaponDetails;

    [Space(10)]
    [Header("Runtime State")]
    public float activeWeaponHandling;
    public int weaponBelongingToWhichMainHandSet;
    public int weaponBelongingToWhichOffHandSet;
    public int weaponRemainingProjectile;
    public bool firingCompletedIfWeaponIsPrecharged;
    public bool firingStoppedPrematurelyIfWeaponIsPrecharged;
    public bool onPrecharge;
    public bool onCooldown;
    public int activePrice;
    public bool isThrowingAxeWeapon;

    // Rolled modifiers
    public BoostType baseUniqueRolled;
    public BoostType baseTypeRolled;
    public BoostType enchantedBoostType = BoostType.None;
    public BoostType mythicBoostType = BoostType.None;

    // Weapon stats
    public float attackCooldownModifier;
    public int physicalAttackDamageIncrease;
    public float attackRatingIncrease;
    public int magicAttackDamageIncrease;
    public float criticalHitChanceIncrease;
    public float criticalHitDamageIncrease;
    public int lifeStealAmount;
    public float statusInflictChance;
    public float dodgeChanceIncrease;
    public float blockChanceIncrease;
    public int increasedMaxHealth;
    public int increasedMaxMana;
    public float statusResistanceModifier;
    public int damageVsLowHealthEnemies;
    public float criticalResistanceModifier;
    public float armorIncrease;
    public float magicResistance;
    public float speedIncreaseModifier;
    public float damageReductionRate;
    public float armorPenetration;
    public float attackRange;
    public float skillCooldown;
    public float skillDuration;

    // CC
    public float additionalPoisonChance = 0f;
    public float additionalBleedChance = 0f;
    public float additionalRootChance = 0f;
    public float additionalStunChance = 0f;
    public float additionalCurseChance = 0f;
    public float additionalFearChance = 0f;
    public float additionalRevealChance = 0f;
    public float additionalParalyzeChance = 0f;
    public float additionalBurnChance = 0f;
    public float additionalFreezeChance = 0f;
    public float additionalBlindChance = 0f;
    public float additionalSlowChance = 0f;

    public Weapon(Rarity rarity) : base(rarity)
    {
        this.rarity = rarity;
    }

    // Convenience helpers
    public IEnumerable<BoostType> GetAllBoosts()
    {
        if (baseUniqueRolled != BoostType.None) yield return baseUniqueRolled;
        if (baseTypeRolled != BoostType.None) yield return baseTypeRolled;
        if (enchantedBoostType != BoostType.None) yield return enchantedBoostType;
        if (mythicBoostType != BoostType.None) yield return mythicBoostType;
    }
}
