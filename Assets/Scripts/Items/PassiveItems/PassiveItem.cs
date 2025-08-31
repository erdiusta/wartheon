
using System.Collections.Generic;

public class PassiveItem : ItemGeneric
{
    public PassiveItemDetailsSO passiveItemDetails;
    public int activePrice;

    // Rolled modifiers
    public BoostType baseUniqueRolled;
    public BoostType baseTypeRolled;
    public BoostType enchantedBoostType = BoostType.None;
    public BoostType mythicBoostType = BoostType.None;

    // Weapon stats
    public float attackCooldown;
    public int physicalAttackDamageIncrease;
    public float attackRating;
    public int magicAttackDamageIncrease;
    public float criticalHitChance;
    public float criticalHitDamage;
    public float lifeStealAmount;
    public float statusInflictChance;
    public float dodgeChance;
    public float blockChance;
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

    public PassiveItem(Rarity rarity) : base(rarity)
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
