using System.Collections.Generic;
using Random = UnityEngine.Random;
using System;
using UnityEngine;

public static class PassiveDropGenerator
{
    public static PassiveItem CreateRolledInstance(PassiveItemDetailsSO passiveItemDetails)
    {
        int choice = Random.Range(0, 100);
        Rarity itemRarity = Rarity.Basic;

        if (choice >= 0 && choice <= 55) { itemRarity = Rarity.Basic; }
        else if (choice > 55 && choice <= 85) { itemRarity = Rarity.Enchanted; }
        else if (choice > 85 && choice <= 100) { itemRarity = Rarity.Mythic; }

        PassiveItem passiveItem = new PassiveItem(itemRarity)
        {
            passiveItemDetails = passiveItemDetails
        };

        // Additional pool rolls depend on rarity
        List<BoostType> pool = passiveItemDetails.additionalModifierPoolForType;

        if(pool != null && pool.Count > 0)
        {
            // Roll 1 for Enchanted+, roll 2 distinct for Mythic

            if (itemRarity >= Rarity.Enchanted)
            {
                passiveItem.enchantedBoostType = RollOne(pool, exclude: new HashSet<BoostType>
                {
                    passiveItemDetails.baseUniqueModifier,
                });

                SetPassiveItemModifier(ref passiveItem, passiveItem.enchantedBoostType, passiveItemDetails);
            }

            if(itemRarity >= Rarity.Mythic)
            {
                passiveItem.mythicBoostType = RollOne(pool, exclude: new HashSet<BoostType>
                {
                    passiveItemDetails.baseUniqueModifier,
                    passiveItem.enchantedBoostType
                });

                SetPassiveItemModifier(ref passiveItem, passiveItem.mythicBoostType, passiveItemDetails);
            }
        }

        return passiveItem;
    }

    private static BoostType RollOne(List<BoostType> pool, HashSet<BoostType> exclude)
    {
        // build candidate list excluding blocked ones
        List<BoostType> candidates = new List<BoostType>();

        foreach (BoostType b in pool)
        {
            if (!exclude.Contains(b)) candidates.Add(b);
        }

        if (candidates.Count == 0) return BoostType.None;

        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }

    private static void SetPassiveItemModifier(ref PassiveItem passiveItem, BoostType boostType, PassiveItemDetailsSO passiveItemDetails)
    {
        float rng = Random.Range(1f, 1.5f);
        float rng2 = 0f;

        switch (boostType)
        {
            case BoostType.AttackCooldown:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.attackCooldown = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                rng2 = Random.Range(1, 10);
                passiveItem.physicalAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                passiveItem.attackRating = (float)Math.Round(rng - 1, 2);
                break;
            case BoostType.MagicDamage:
                rng2 = Random.Range(1, 10);
                passiveItem.magicAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.criticalHitChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritDamage:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.criticalHitDamage = (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.lifeStealAmount = Mathf.RoundToInt(passiveItem.physicalAttackDamageIncrease * rng2);
                break;
            case BoostType.BlockChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.blockChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.dodgeChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = Random.Range(-1f, 1f);
                passiveItem.increasedMaxHealth = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = Random.Range(-1f, 1f);
                passiveItem.increasedMaxMana = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.statusResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.attackRateVsLowHealthEnemies = (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.criticalResistance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.armorIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.magicResistance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.speedIncreaseModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = Random.Range(0.05f, 0.15f);
                passiveItem.damageReductionRate = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorPenetration:
                rng2 = Random.Range(0.05f, 0.15f);
                passiveItem.armorPenetration = (float)Math.Round(rng2, 2);
                break;
            default:
                break;
        }
    }
}
