using System.Collections.Generic;
using System;
using UnityEngine;

public static class PassiveDropGenerator
{
    public static PassiveItem GetPassiveWithStats(PassiveItemStats passiveStats, Rarity rarity, ItemSlotStatus slotStatus, int inventoryIndex)
    {
        PassiveItem passiveItem = new PassiveItem(rarity);
        passiveItem.passiveStats = passiveStats;
        passiveItem.ItemSlotStatus = slotStatus;
        passiveItem.ItemType = ItemType.PassiveItem;
        passiveItem.InventoryIndex = inventoryIndex;

        return passiveItem;
    }

    public static PassiveItem CreateRolledInstance(PassiveItemDetailsSO passiveItemDetails, WartheonRNG rng, bool rarityAlreadySet = false, Rarity rarity = Rarity.Basic, bool isPrimaryPassive = false)
    {
        Rarity itemRarity = GetRarity(rng);

        PassiveItem passiveItem = new PassiveItem(itemRarity);

        passiveItem.passiveStats = new PassiveItemStats();

        if (isPrimaryPassive)
        {
            passiveItem.passiveStats.passiveItemSlotName = PassiveItemSlotName.None;
            passiveItem.passiveStats.passiveItemType = passiveItemDetails.passiveItemType;
        }
        else
        {
            passiveItem.passiveStats.passiveItemSlotName = passiveItemDetails.passiveItemSlotName;
            passiveItem.passiveStats.passiveItemType = passiveItemDetails.passiveItemType;

            passiveItem.passiveStats.baseUniqueRolled = passiveItemDetails.baseUniqueModifier;
            passiveItem.passiveStats.baseTypeRolled = passiveItemDetails.baseTypeModifier;

            // Additional pool rolls depend on rarity
            List<BoostType> pool = passiveItemDetails.additionalModifierPoolForType;

            if (pool != null && pool.Count > 0)
            {
                SetPassiveItemModifier(ref passiveItem, passiveItem.passiveStats.baseUniqueRolled, passiveItemDetails, rng);
                SetPassiveItemModifier(ref passiveItem, passiveItem.passiveStats.baseTypeRolled, passiveItemDetails, rng);

                if (itemRarity == Rarity.Mythic)
                {
                    passiveItem.passiveStats.mythicBoostType = RollOne(pool, exclude: new HashSet<BoostType>
                {
                    passiveItemDetails.baseUniqueModifier,
                    passiveItemDetails.baseTypeModifier,
                    passiveItem.passiveStats.enchantedBoostType
                }, rng);

                    SetPassiveItemModifier(ref passiveItem, passiveItem.passiveStats.enchantedBoostType, passiveItemDetails, rng);
                    SetPassiveItemModifier(ref passiveItem, passiveItem.passiveStats.mythicBoostType, passiveItemDetails, rng);
                }
                else if (itemRarity == Rarity.Enchanted)
                {
                    passiveItem.passiveStats.enchantedBoostType = RollOne(pool, exclude: new HashSet<BoostType>()
                {
                    passiveItemDetails.baseUniqueModifier,
                    passiveItemDetails.baseTypeModifier
                }, rng);

                    SetPassiveItemModifier(ref passiveItem, passiveItem.passiveStats.enchantedBoostType, passiveItemDetails, rng);
                }
            }
        }

        return passiveItem;
    }

    public static Rarity GetRarity(WartheonRNG rng)
    {
        int choice = rng.Range(0, 100);
        Rarity itemRarity = Rarity.Basic;

        if (choice >= 0 && choice <= 55) { itemRarity = Rarity.Basic; }
        else if (choice > 55 && choice <= 85) { itemRarity = Rarity.Enchanted; }
        else if (choice > 85 && choice <= 100) { itemRarity = Rarity.Mythic; }

        return itemRarity;
    }

    private static BoostType RollOne(List<BoostType> pool, HashSet<BoostType> exclude, WartheonRNG rng)
    {
        // build candidate list excluding blocked ones
        List<BoostType> candidates = new List<BoostType>();

        foreach (BoostType b in pool)
        {
            if (!exclude.Contains(b)) candidates.Add(b);
        }

        if (candidates.Count == 0) return BoostType.None;

        int idx = rng.Range(0, candidates.Count);
        return candidates[idx];
    }

    public static void SetPassiveItemModifier(ref PassiveItem passiveItem, BoostType boostType, PassiveItemDetailsSO passiveItemDetails, WartheonRNG rng)
    {
        float rng1 = rng.Range(1f, 1.5f);
        float rng2 = 0f;
            
        switch (boostType)
        {
            case BoostType.AttackCooldown:
                rng2 = rng.Range(0.05f, 0.12f);
                passiveItem.passiveStats.attackCooldown += (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                rng2 = rng.Range(1, 10);
                passiveItem.passiveStats.physicalAttackDamageIncrease += Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                passiveItem.passiveStats.attackRating += (float)Math.Round(rng1 - 1, 2);
                break;
            case BoostType.MagicDamage:
                rng2 = rng.Range(1, 10);
                passiveItem.passiveStats.magicAttackDamageIncrease += Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = rng.Range(0.05f, 0.25f);
                passiveItem.passiveStats.criticalHitChance += (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritDamage:
                rng2 = rng.Range(0.05f, 0.25f);
                passiveItem.passiveStats.criticalHitDamage += (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = rng.Range(1, 8);
                passiveItem.passiveStats.lifeStealAmount += Mathf.RoundToInt(rng2);
                break;
            case BoostType.BlockChance:
                rng2 = rng.Range(0.05f, 0.25f);
                passiveItem.passiveStats.blockChance += (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = rng.Range(0.01f, 0.10f);
                passiveItem.passiveStats.dodgeChance += (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = rng.Range(-1f, 1f);
                passiveItem.passiveStats.increasedMaxHealth += Mathf.RoundToInt(100 * rng1 * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = rng.Range(-1f, 1f);
                passiveItem.passiveStats.increasedMaxMana += Mathf.RoundToInt(100 * rng1 * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = rng.Range(0.1f, 0.25f);
                passiveItem.passiveStats.statusResistanceModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = rng.Range(1, 8);
                passiveItem.passiveStats.damageVsLowHealthEnemies += Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritResistance:
                rng2 = rng.Range(0.1f, 0.25f);
                passiveItem.passiveStats.criticalResistanceModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = rng.Range(0.05f, 0.25f);
                passiveItem.passiveStats.armorIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = rng.Range(0.05f, 0.25f);
                passiveItem.passiveStats.magicResistanceModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = rng.Range(0.05f, 0.25f);
                passiveItem.passiveStats.speedIncreaseModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = rng.Range(0.05f, 0.15f);
                passiveItem.passiveStats.damageReductionRate += (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorPenetration:
                rng2 = rng.Range(0.05f, 0.15f);
                passiveItem.passiveStats.armorPenetration += (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillCooldown:
                rng2 = rng.Range(0.05f, 0.13f);
                passiveItem.passiveStats.skillCooldown += (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillDuration:
                rng2 = rng.Range(0.05f, 0.4f);
                passiveItem.passiveStats.skillDuration += (float)Math.Round(rng2, 2);
                break;
            case BoostType.StatusInflict:
                rng2 = rng.Range(0.05f, 0.13f);
                StatusEffectType statusEffectType = (StatusEffectType)rng.Range(1, Enum.GetValues(typeof(StatusEffectType)).Length);

                switch (statusEffectType)
                {
                    case StatusEffectType.Poison:
                        passiveItem.passiveStats.additionalPoisonChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Bleed:
                        passiveItem.passiveStats.additionalBleedChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Root:
                        passiveItem.passiveStats.additionalRootChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Stun:
                        passiveItem.passiveStats.additionalStunChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Curse:
                        passiveItem.passiveStats.additionalCurseChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Fear:
                        passiveItem.passiveStats.additionalFearChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Reveal:
                        passiveItem.passiveStats.additionalRevealChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Paralyze:
                        passiveItem.passiveStats.additionalParalyzeChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Burn:
                        passiveItem.passiveStats.additionalBurnChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Freeze:
                        passiveItem.passiveStats.additionalFreezeChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Blind:
                        passiveItem.passiveStats.additionalBlindChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Slow:
                        passiveItem.passiveStats.additionalSlowChance += (float)Math.Round(rng2, 2);
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
}
