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

    // Rolled modifiers
    public BoostType baseUniqueRolled;
    public BoostType baseTypeRolled;
    public BoostType enchantedBoostType = BoostType.None;
    public BoostType mythicBoostType = BoostType.None;

    // Weapon stats
    public float attackCooldown;
    public int physicalAttackDamageIncrease;
    public float attackRatingIncrease;
    public int magicAttackDamageIncrease;
    public float criticalHitChanceIncrease;
    public float criticalHitDamageIncrease;
    public float lifeStealAmount;
    public float statusInflictChance;
    public float dodgeChanceIncrease;
    public float blockChanceIncrease;
    public int increasedMaxHealth;
    public int increasedMaxMana;
    public float statusResistanceModifier;
    public float attackRateVsLowHealthEnemies;
    public float criticalResistance;
    public float armorIncrease;
    public float magicResistance;
    public float speedIncreaseModifier;
    public float damageReductionRate;
    public float armorPenetration;
    public float attackRange;

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
