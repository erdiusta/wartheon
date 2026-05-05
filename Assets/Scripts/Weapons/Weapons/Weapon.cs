using System.Collections.Generic;
public class Weapon : ItemGeneric
{
    public WeaponStats weaponStats;

    ItemType itemType;
    Rarity rarity;
    ItemSlotStatus itemSlotStatus;
    int inventoryIndex;

    public Weapon(Rarity rarity)
    {
        this.rarity = rarity;
    }

    public override ItemType ItemType { get => itemType; set => itemType = value; }
    public override Rarity Rarity { get => rarity; set => rarity = value; }
    public override ItemSlotStatus ItemSlotStatus { get => itemSlotStatus; set => itemSlotStatus = value; }
    public override int InventoryIndex { get => inventoryIndex; set => inventoryIndex = value; }

    // Convenience helpers
    public IEnumerable<BoostType> GetAllBoosts()
    {
        if (weaponStats.baseUniqueRolled != BoostType.None) yield return weaponStats.baseUniqueRolled;
        if (weaponStats.baseTypeRolled != BoostType.None) yield return weaponStats.baseTypeRolled;
        if (weaponStats.enchantedBoostType != BoostType.None) yield return weaponStats.enchantedBoostType;
        if (weaponStats.mythicBoostType != BoostType.None) yield return weaponStats.mythicBoostType;
    }
}

public struct WeaponStats
{
    public WeaponClass weaponClass;
    public WeaponTitle weaponTitle;
    public WieldType wieldType;
    public bool isMeleeWeapon;
    public bool hasSwing;
    public bool hasThrust;
    public float elementalForgeRate;
    public int inventoryIndex;

    public float activeWeaponHandling;
    public int weaponBelongingToWhichMainHandSet;
    public int weaponBelongingToWhichOffHandSet;
    public bool firingCompletedIfWeaponIsPrecharged;
    public bool firingStoppedPrematurelyIfWeaponIsPrecharged;
    public bool onPrecharge;
    public bool onCooldown;
    public int activePrice;
    public bool isThrowingAxeWeapon;

    // Rolled modifiers
    public BoostType baseUniqueRolled;
    public BoostType baseTypeRolled;
    public BoostType enchantedBoostType;
    public BoostType mythicBoostType;

    // Weapon stats
    public int physicalDamageMin;
    public int physicalDamageMax;
    public int magicDamageMin;
    public int magicDamageMax;
    public float blockChance;
    public float weaponCooldownDuration;
    public float weaponAttackRating;
    public float criticalHitChance;
    public float criticalHitDamage;
    public float weaponPrechargeTime;

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
