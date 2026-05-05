using System.Collections.Generic;
using System;
using UnityEngine;

public static class WeaponDropGenerator
{
    public static Weapon GetWeaponWithStats(WeaponStats weaponStats, Rarity rarity, ItemSlotStatus slotStatus, int inventoryIndex)
    {
        Weapon weapon = new Weapon(rarity);
        weapon.weaponStats = weaponStats;
        weapon.ItemSlotStatus = slotStatus;
        weapon.ItemType = ItemType.Weapon;
        weapon.InventoryIndex = inventoryIndex;

        return weapon;
    }

    public static Weapon CreateRolledInstance(WeaponDetailsSO weaponDetails, WartheonRNG rng, bool rarityAlreadySet = false, Rarity rarity = Rarity.Basic)
    {
        Rarity weaponRarity = GetRarity(rng);

        Weapon weapon = new Weapon(weaponRarity);

        weapon.weaponStats = new WeaponStats
        {
            weaponClass = weaponDetails.weaponClass,
            weaponTitle = weaponDetails.weaponTitle,
            wieldType = weaponDetails.wieldType,
            isMeleeWeapon = weaponDetails.isMeleeWeapon,
            hasSwing = weaponDetails.hasSwing,
            hasThrust = weaponDetails.hasThrust,
            elementalForgeRate = weaponDetails.elementalForgeRate,

            baseUniqueRolled = weaponDetails.baseUniqueModifier,
            baseTypeRolled = weaponDetails.baseTypeModifier,

            physicalDamageMin = weaponDetails.physicalDamageMin,
            physicalDamageMax = weaponDetails.physicalDamageMax,
            magicDamageMin = weaponDetails.magicDamageMin,
            magicDamageMax = weaponDetails.magicDamageMax,

            physicalAttackDamageIncrease = Mathf.RoundToInt((weaponDetails.isShield ? 0 : weaponDetails.isMeleeWeapon ? weaponDetails.physicalDamageMin
            : weaponDetails.weaponCurrentProjectile.projectilePhyDamageMin) * (1 - weaponDetails.elementalForgeRate)),

            magicAttackDamageIncrease = Mathf.RoundToInt((weaponDetails.isShield ? 0 : weaponDetails.isMeleeWeapon ? weaponDetails.physicalDamageMin
            : weaponDetails.weaponCurrentProjectile.projectilePhyDamageMin) * weaponDetails.elementalForgeRate),

            weaponCooldownDuration = weaponDetails.weaponCooldownDuration,
            blockChance = weaponDetails.blockChance,
            weaponAttackRating = weaponDetails.weaponAttackRating,
            criticalHitChance = weaponDetails.criticalHitChance,
            criticalHitDamage = weaponDetails.criticalHitDamageMultiplier,

            criticalHitChanceIncrease = weaponDetails.criticalHitChance,
            criticalHitDamageIncrease = weaponDetails.criticalHitDamageMultiplier,

            weaponPrechargeTime = weaponDetails.weaponPrechargeTime
        };

        // Additional pool rolls depend on rarity
        List<BoostType> pool = weaponDetails.additionalModifierPoolForType;

        if (pool != null && pool.Count > 0)
        {
            // Roll 1 for Enchanted+, roll 2 distinct for Mythic
            SetWeaponModifier(ref weapon, weapon.weaponStats.baseUniqueRolled, weaponDetails, rng);
            SetWeaponModifier(ref weapon, weapon.weaponStats.baseTypeRolled, weaponDetails, rng);

            if (weaponRarity == Rarity.Mythic)
            {
                weapon.weaponStats.mythicBoostType = RollOne(pool, exclude: new HashSet<BoostType>
                {
                    weaponDetails.baseUniqueModifier,
                    weaponDetails.baseTypeModifier,
                    weapon.weaponStats.enchantedBoostType
                }, rng);

                SetWeaponModifier(ref weapon, weapon.weaponStats.enchantedBoostType, weaponDetails, rng);
                SetWeaponModifier(ref weapon, weapon.weaponStats.mythicBoostType, weaponDetails, rng);
            }
            else if (weaponRarity == Rarity.Enchanted)
            {
                weapon.weaponStats.enchantedBoostType = RollOne(pool, exclude: new HashSet<BoostType>()
                {
                    weaponDetails.baseUniqueModifier,
                    weaponDetails.baseTypeModifier
                }, rng);

                SetWeaponModifier(ref weapon, weapon.weaponStats.enchantedBoostType, weaponDetails, rng);
            }
        }

        return weapon;
    }

    public static Rarity GetRarity(WartheonRNG rng)
    {
        Rarity weaponRarity = Rarity.Basic;
        int choice = 0;

        if (InputManager.TutorialEnabled) choice = 0;
        else
        {
            choice = rng.Range(0, 100);
        }

        if (choice >= 0 && choice <= 55) { weaponRarity = Rarity.Basic; }
        else if (choice > 55 && choice <= 85) { weaponRarity = Rarity.Enchanted; }
        else if (choice > 85 && choice <= 100) { weaponRarity = Rarity.Mythic; }

        return weaponRarity;
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

    public static void SetWeaponModifier(ref Weapon weapon, BoostType boostType, WeaponDetailsSO weaponDetails, WartheonRNG rng)
    {
        bool isMeleeWeapon = weapon.weaponStats.isMeleeWeapon;

        // Determine base damage safely (shields -> 0)
        int baseMin = 0, baseMax = 0;

        // Prefer an explicit flag if you have one; otherwise derive from class
        bool isShield = (weaponDetails.weaponClass == WeaponClass.Shield);

        float rng1 = rng.Range(1f, 1.5f);
        float rng2 = 0f;

        if (!isShield)
        {
            if (weapon.weaponStats.isMeleeWeapon)
            {
                baseMin = weaponDetails.physicalDamageMin;
                baseMax = weaponDetails.physicalDamageMax;
            }
            else
            {
                var proj = weaponDetails.weaponCurrentProjectile;
                baseMin = proj != null ? proj.projectilePhyDamageMin : 0;
                baseMax = proj != null ? proj.projectilePhyDamageMax : 0;
            }
        }

        // Split by forge as before
        float forge = Mathf.Clamp01(weaponDetails.elementalForgeRate);
        int basePhysMin = Mathf.RoundToInt(baseMin * (1f - forge));
        int basePhysMax = Mathf.RoundToInt(baseMax * (1f - forge));
        int baseMagicMin = Mathf.RoundToInt(baseMin * forge);
        int baseMagicMax = Mathf.RoundToInt(baseMax * forge);

        switch (boostType)
        {
            case BoostType.AttackCooldown:
                rng2 = rng.Range(0.05f, 0.12f);
                weapon.weaponStats.attackCooldownModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                rng2 = rng.Range(1, 10);
                weapon.weaponStats.physicalAttackDamageIncrease += Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                weapon.weaponStats.attackRatingIncrease += (float)Math.Round(rng1 - 1, 2);
                break;
            case BoostType.MagicDamage:
                rng2 = rng.Range(1, 10);
                weapon.weaponStats.magicAttackDamageIncrease += Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = rng.Range(0.05f, 0.25f);
                weapon.weaponStats.criticalHitChanceIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritDamage:
                rng2 = rng.Range(0.05f, 0.25f);
                weapon.weaponStats.criticalHitDamageIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = rng.Range(1, 11);
                weapon.weaponStats.lifeStealAmount += Mathf.RoundToInt(rng2);
                break;
            case BoostType.BlockChance:
                rng2 = rng.Range(0.05f, 0.25f);
                weapon.weaponStats.blockChanceIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = rng.Range(0.01f, 0.10f);
                weapon.weaponStats.dodgeChanceIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = rng.Range(-1f, 1f);
                weapon.weaponStats.increasedMaxHealth += Mathf.RoundToInt(100 * rng1 * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = rng.Range(-1f, 1f);
                weapon.weaponStats.increasedMaxMana += Mathf.RoundToInt(100 * rng1 * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = rng.Range(0.1f, 0.25f);
                weapon.weaponStats.statusResistanceModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = rng.Range(1, 8);
                weapon.weaponStats.damageVsLowHealthEnemies += Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritResistance:
                rng2 = rng.Range(0.1f, 0.25f);
                weapon.weaponStats.criticalResistanceModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = rng.Range(0.05f, 0.25f);
                weapon.weaponStats.armorIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = rng.Range(0.05f, 0.25f);
                weapon.weaponStats.magicResistance += (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = rng.Range(0.5f, 2f);
                weapon.weaponStats.speedIncreaseModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = rng.Range(0.05f, 0.15f);
                weapon.weaponStats.damageReductionRate += (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorPenetration:
                rng2 = rng.Range(0.05f, 0.15f);
                weapon.weaponStats.armorPenetration += (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackRange:
                rng2 = rng.Range(1f, 5f);
                weapon.weaponStats.attackRange += (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillCooldown:
                rng2 = rng.Range(0.05f, 0.13f);
                weapon.weaponStats.skillCooldown += (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillDuration:
                rng2 = rng.Range(0.05f, 0.4f);
                weapon.weaponStats.skillDuration += (float)Math.Round(rng2, 2);
                break;
            case BoostType.StatusInflict:
                rng2 = rng.Range(0.05f, 0.13f);
                StatusEffectType statusEffectType = (StatusEffectType)rng.Range(1, Enum.GetValues(typeof(StatusEffectType)).Length);

                switch (statusEffectType)
                {
                    case StatusEffectType.Poison:
                        weapon.weaponStats.additionalPoisonChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Bleed:
                        weapon.weaponStats.additionalBleedChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Root:
                        weapon.weaponStats.additionalRootChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Stun:
                        weapon.weaponStats.additionalStunChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Curse:
                        weapon.weaponStats.additionalCurseChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Fear:
                        weapon.weaponStats.additionalFearChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Reveal:
                        weapon.weaponStats.additionalRevealChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Paralyze:
                        weapon.weaponStats.additionalParalyzeChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Burn:
                        weapon.weaponStats.additionalBurnChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Freeze:
                        weapon.weaponStats.additionalFreezeChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Blind:
                        weapon.weaponStats.additionalBlindChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Slow:
                        weapon.weaponStats.additionalSlowChance += (float)Math.Round(rng2, 2);
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
