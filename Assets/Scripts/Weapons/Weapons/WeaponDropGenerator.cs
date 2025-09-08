using System.Collections.Generic;
using Random = UnityEngine.Random;
using System;
using UnityEngine;

public static class WeaponDropGenerator
{
    public static Weapon CreateRolledInstance(WeaponDetailsSO weaponDetails)
    {
        Rarity weaponRarity = Rarity.Basic;
        int choice = 0;

        if (InputManager.TutorialEnabled) choice = 0;
        else
        {
            choice = Random.Range(0, 100);
        }

        if (choice >= 0 && choice <= 55) { weaponRarity = Rarity.Basic; }
        else if (choice > 55 && choice <= 85) { weaponRarity = Rarity.Enchanted; }
        else if (choice > 85 && choice <= 100) { weaponRarity = Rarity.Mythic; }

        Weapon weapon = new Weapon(weaponRarity)
        {
            // Copy passive flags from SO so tooltips & effects know what this instance can do
            weaponDetails = weaponDetails,
            baseUniqueRolled = weaponDetails.baseUniqueModifier,
            baseTypeRolled = weaponDetails.baseTypeModifier,
            attackCooldownModifier = weaponDetails.weaponCooldownDuration,
            attackRatingIncrease = weaponDetails.weaponAttackRating,

            physicalAttackDamageIncrease = Mathf.RoundToInt((weaponDetails.isShield ? 0 : weaponDetails.isMeleeWeapon ? weaponDetails.physicalDamageMin
            : weaponDetails.weaponCurrentProjectile.projectilePhyDamageMin) * (1 - weaponDetails.elementalForgeRate)),

            magicAttackDamageIncrease = Mathf.RoundToInt((weaponDetails.isShield ? 0 : weaponDetails.isMeleeWeapon ? weaponDetails.physicalDamageMin
            : weaponDetails.weaponCurrentProjectile.projectilePhyDamageMin) * weaponDetails.elementalForgeRate),

            criticalHitChanceIncrease = weaponDetails.criticalHitChance,
            criticalHitDamageIncrease = weaponDetails.criticalHitDamageMultiplier
        };

        // Additional pool rolls depend on rarity
        List<BoostType> pool = weaponDetails.additionalModifierPoolForType;

        if(pool != null && pool.Count > 0)
        {
            // Roll 1 for Enchanted+, roll 2 distinct for Mythic
            SetWeaponModifier(ref weapon, weapon.baseUniqueRolled, weaponDetails);
            SetWeaponModifier(ref weapon, weapon.baseTypeRolled, weaponDetails);

            if (weaponRarity == Rarity.Mythic)
            {
                weapon.mythicBoostType = RollOne(pool, exclude: new HashSet<BoostType>
                {
                    weaponDetails.baseUniqueModifier,
                    weaponDetails.baseTypeModifier,
                    weapon.enchantedBoostType
                });

                SetWeaponModifier(ref weapon, weapon.enchantedBoostType, weaponDetails);
                SetWeaponModifier(ref weapon, weapon.mythicBoostType, weaponDetails);
            }
            else if (weaponRarity == Rarity.Enchanted)
            {
                weapon.enchantedBoostType = RollOne(pool, exclude: new HashSet<BoostType>()
                {
                    weaponDetails.baseUniqueModifier,
                    weaponDetails.baseTypeModifier
                });

                SetWeaponModifier(ref weapon, weapon.enchantedBoostType, weaponDetails);
            }
        }

        return weapon;
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

    public static void SetWeaponModifier(ref Weapon weapon, BoostType boostType, WeaponDetailsSO weaponDetails)
    {
        bool isMeleeWeapon = weaponDetails.isMeleeWeapon;

        // Determine base damage safely (shields -> 0)
        int baseMin = 0, baseMax = 0;

        // Prefer an explicit flag if you have one; otherwise derive from class
        bool isShield = (weaponDetails.weaponClass == WeaponClass.Shield);

        float rng = Random.Range(1f, 1.5f);
        float rng2 = 0f;

        if (!isShield)
        {
            if (weaponDetails.isMeleeWeapon)
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
                rng2 = Random.Range(0.05f, 0.12f);
                weapon.attackCooldownModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                rng2 = Random.Range(1, 10);
                weapon.physicalAttackDamageIncrease += Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                weapon.attackRatingIncrease += (float)Math.Round(rng - 1, 2);
                break;
            case BoostType.MagicDamage:
                rng2 = Random.Range(1, 10);
                weapon.magicAttackDamageIncrease += Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.criticalHitChanceIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritDamage:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.criticalHitDamageIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = Random.Range(1, 11);
                weapon.lifeStealAmount += Mathf.RoundToInt(rng2);
                break;
            case BoostType.BlockChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.blockChanceIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.dodgeChanceIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = Random.Range(-1f, 1f);
                weapon.increasedMaxHealth += Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = Random.Range(-1f, 1f);
                weapon.increasedMaxMana += Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.statusResistanceModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = Random.Range(1, 8);
                weapon.damageVsLowHealthEnemies += Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.criticalResistanceModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.armorIncrease += (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.magicResistance += (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = Random.Range(0.5f, 2f);
                weapon.speedIncreaseModifier += (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = Random.Range(0.05f, 0.15f);
                weapon.damageReductionRate += (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorPenetration:
                rng2 = Random.Range(0.05f, 0.15f);
                weapon.armorPenetration += (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackRange:
                rng2 = Random.Range(1f, 5f);
                weapon.attackRange += (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillCooldown:
                rng2 = Random.Range(0.05f, 0.13f);
                weapon.skillCooldown += (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillDuration:
                rng2 = Random.Range(0.05f, 0.4f);
                weapon.skillDuration += (float)Math.Round(rng2, 2);
                break;
            case BoostType.StatusInflict:
                rng2 = Random.Range(0.05f, 0.13f);
                StatusEffectType statusEffectType = (StatusEffectType)Random.Range(1, Enum.GetValues(typeof(StatusEffectType)).Length);

                switch (statusEffectType)
                {
                    case StatusEffectType.Poison:
                        weapon.additionalPoisonChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Bleed:
                        weapon.additionalBleedChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Root:
                        weapon.additionalRootChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Stun:
                        weapon.additionalStunChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Curse:
                        weapon.additionalCurseChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Fear:
                        weapon.additionalFearChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Reveal:
                        weapon.additionalRevealChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Paralyze:
                        weapon.additionalParalyzeChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Burn:
                        weapon.additionalBurnChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Freeze:
                        weapon.additionalFreezeChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Blind:
                        weapon.additionalBlindChance += (float)Math.Round(rng2, 2);
                        break;
                    case StatusEffectType.Slow:
                        weapon.additionalSlowChance += (float)Math.Round(rng2, 2);
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
