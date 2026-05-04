using System.Collections.Generic;

public class PassiveItem : ItemGeneric
{
    public PassiveItemDetailsSO passiveItemDetails;
    public PassiveItemStats passiveStats;

    public PassiveItem(Rarity rarity) : base(rarity)
    {
        this.rarity = rarity;
    }

    // Convenience helpers
    public IEnumerable<BoostType> GetAllBoosts()
    {
        if (passiveStats.baseUniqueRolled != BoostType.None) yield return passiveStats.baseUniqueRolled;
        if (passiveStats.baseTypeRolled != BoostType.None) yield return passiveStats.baseTypeRolled;
        if (passiveStats.enchantedBoostType != BoostType.None) yield return passiveStats.enchantedBoostType;
        if (passiveStats.mythicBoostType != BoostType.None) yield return passiveStats.mythicBoostType;
    }
}

public struct PassiveItemStats
{
    public PassiveItemType passiveItemType;
    public PassiveItemSlotName passiveItemSlotName;
    public int activePrice;

    // Rolled modifiers
    public BoostType baseUniqueRolled;
    public BoostType baseTypeRolled;
    public BoostType enchantedBoostType;
    public BoostType mythicBoostType;

    // Passive item stats
    public float attackCooldown;
    public int physicalAttackDamageIncrease;
    public float attackRating;
    public int magicAttackDamageIncrease;
    public float criticalHitChance;
    public float criticalHitDamage;
    public int lifeStealAmount;
    public float statusInflictChance;
    public float dodgeChance;
    public float blockChance;
    public int increasedMaxHealth;
    public int increasedMaxMana;
    public float statusResistanceModifier;
    public int damageVsLowHealthEnemies;
    public float criticalResistanceModifier;
    public float armorIncrease;
    public float magicResistanceModifier;
    public float speedIncreaseModifier;
    public float damageReductionRate;
    public float armorPenetration;
    public float attackRange;
    public float skillCooldown;
    public float skillDuration;

    // CC
    public float additionalPoisonChance;
    public float additionalBleedChance;
    public float additionalRootChance;
    public float additionalStunChance;
    public float additionalCurseChance;
    public float additionalFearChance;
    public float additionalRevealChance;
    public float additionalParalyzeChance;
    public float additionalBurnChance;
    public float additionalFreezeChance;
    public float additionalBlindChance;
    public float additionalSlowChance;
}
