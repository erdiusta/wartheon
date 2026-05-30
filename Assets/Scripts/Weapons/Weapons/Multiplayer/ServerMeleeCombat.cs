using Mirror;
using System;
using UnityEngine;
using Random = UnityEngine.Random;

public static class ServerMeleeCombat
{
    [Server]
    public static void ResolveMeleeHit(NetworkConnectionToClient attackerConn, uint netId, AttackShape attackType, MeleeHand hand, bool isBloodDrain, bool shieldBash, bool isSheerCold, 
        bool isDontBlink, bool isWhisperSlice, bool isThrowingAxe)
    {
        Player damageDealerPlayer = null;

        foreach (Player player in GameSessionManager.Instance.ServerPlayers)
        {
            if(player.NetAuth.netId == netId)
            {
                damageDealerPlayer = player;
                goto damageDealerFound;
            }
        }

    damageDealerFound:
        if (damageDealerPlayer != null)
        {
            Weapon mainHandWeapon = damageDealerPlayer.activeWeapon.GetCurrentMainHandWeapon();
            Weapon offHandWeapon = damageDealerPlayer.activeWeapon.GetCurrentOffHandWeapon();

            Weapon hittingHandWeapon = hand switch
            {
                MeleeHand.MainHand => mainHandWeapon,
                MeleeHand.OffHand => offHandWeapon,
                _ => null
            };

            WeaponDetailsSO mainHandWeaponDetails = mainHandWeapon != null ? WartheonDatabase.Instance.GetWeaponDetails(mainHandWeapon.weaponStats.weaponTitle) : null;
            WeaponDetailsSO offHandWeaponDetails = offHandWeapon != null ? WartheonDatabase.Instance.GetWeaponDetails(offHandWeapon.weaponStats.weaponTitle) : null;

            WeaponDetailsSO hittingHandWeaponDetails = hittingHandWeapon != null ? WartheonDatabase.Instance.GetWeaponDetails(hittingHandWeapon.weaponStats.weaponTitle) : null;

            Transform originTransform = ResolveAttackOrigin(damageDealerPlayer, attackType, hand);
            Collider2D attackCollider = originTransform.GetComponent<Collider2D>();

            Collider2D[] _colliders = new Collider2D[10];
            ContactFilter2D _contactFilter = new ContactFilter2D().NoFilter();

            int hitCount = 0;

            while (true)
            {
                hitCount = Physics2D.OverlapCollider(attackCollider, _contactFilter, _colliders);

                if (hitCount < _colliders.Length) break;

                _colliders = new Collider2D[_colliders.Length * 2];
            }

            Span<Collider2D> hitSpan = _colliders.AsSpan(0, hitCount);

            foreach (var collider in hitSpan)
            {
                if (collider.CompareTag(Settings.playerTag) || collider.CompareTag(Settings.decoyTag) || collider.CompareTag(Settings.chestItemTag) || collider.CompareTag(Settings.roomRoot)) continue;

                // Ignore capsule colliders used for blocking
                if (collider is CapsuleCollider2D && (collider.GetComponent<Enemy>() != null || collider.GetComponent<Player>() != null)) continue;

                if (collider is CircleCollider2D && (collider.GetComponent<Enemy>() != null || collider.GetComponent<Player>() != null)) continue;

                ReceiveMeleeDamage receiveMeleeDamage = collider.GetComponent<ReceiveMeleeDamage>();

                if (receiveMeleeDamage != null)
                {
                    // Handle destructibles / environment
                    if (collider.TryGetComponent(out Environment environment) &&
                        collider.TryGetComponent(out Health envHealth))
                    {
                        NetworkSoundManager.Instance.ServerPlaySound(SoundName.SwordImpact, damageDealerPlayer.transform.position);

                        DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = damageDealerPlayer.transform.position, 
                            receiverPosition = collider.transform.position, hand = hand };
                        receiveMeleeDamage.TakeMeleeDamage(100, ctx);

                        continue;
                    }

                    if (!collider.TryGetComponent(out Health enemyHealth)) continue;

                    if (collider.CompareTag(Settings.practiceDummy))
                    {
                        NetworkSoundManager.Instance.ServerPlaySound(SoundName.SwordImpact, damageDealerPlayer.transform.position);

                        DummyCheck(hand, receiveMeleeDamage, damageDealerPlayer, hittingHandWeaponDetails, isShieldBash: true);
                        continue;
                    }

                    if (!collider.TryGetComponent(out Enemy enemy)) continue;

                    IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(enemy.gameObject);
                    IHealthAuthority healthAuthority = enemy.GetComponent<IHealthAuthority>();

                    // Calculate hit chance
                    bool attackHits = damageDealerPlayer.currentAttackRatingValue * 100 - enemyCombatData.DeflectionValue * 100 > Random.Range(0, 100);

                    if (attackHits)
                    {
                        int inflictedDamage = 0;

                        Vector2 knockbackDir = Vector2.zero;
                        float knockbackForce = 0f;
                        float dealDamageMass = 0f;

                        float specialDamageModifier = 1f; // Default value

                        if (shieldBash)
                        {
                            CheckStunStatus(hittingHandWeaponDetails, enemy, enemyCombatData, shieldBash: true, false, damageDealerPlayer);

                            if (mainHandWeapon != null)
                            {
                                specialDamageModifier = 0.5f;

                                inflictedDamage = CalculateDamageAmount(enemy, mainHandWeapon, MeleeHand.MainHand, specialDamageModifier, damageDealerPlayer);

                                if (enemy.health != null)
                                {
                                    enemy.enemyNetwork.lastDamageDealerNetId = damageDealerPlayer.NetAuth.netId;

                                    DamageContext ctxShieldBash = new DamageContext { source = DamageSourceType.Melee, dealerPosition = damageDealerPlayer.transform.position, 
                                        receiverPosition = enemy.health.transform.position};
                                    receiveMeleeDamage.TakeMeleeDamage(inflictedDamage, ctxShieldBash);
                                }
                            }

                            if (damageDealerPlayer.isTriadExecutionActive) damageDealerPlayer.triadExecutionCounter++;

                            // Knockback
                            knockbackDir = (enemy.transform.position - damageDealerPlayer.transform.position).normalized;
                            knockbackForce = 12f; // Shield bash hit force
                            dealDamageMass = 1f;

                            enemy.enemyMovementNetwork.ServerApplyKnockback(knockbackDir, knockbackForce, dealDamageMass);
                            continue;
                        }

                        if (enemyHealth.suddenDeathHappened)
                        {
                            DamageContext ctxSuddenDeath = new DamageContext { source = DamageSourceType.Melee, dealerPosition = damageDealerPlayer.transform.position, receiverPosition = enemy.transform.position, hand = hand };
                            receiveMeleeDamage.TakeMeleeDamage(enemyHealth.GetCurrentHealth() + 10, ctxSuddenDeath);

                            if (damageDealerPlayer.isTriadExecutionActive) damageDealerPlayer.triadExecutionCounter++;
                            continue;
                        }

                        // Special damage calculation
                        if (isSheerCold)
                        {
                            specialDamageModifier = damageDealerPlayer.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                            {
                                1 => 1f,
                                2 => 1.15f,
                                3 => 1.35f,
                                _ => 1f
                            };
                        }

                        if (isDontBlink)
                        {
                            specialDamageModifier = damageDealerPlayer.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
                            {
                                1 => 1.2f,
                                2 => 1.3f,
                                3 => 1.5f,
                                _ => 1f
                            };
                        }

                        // DAMAGE CALCULATION
                        inflictedDamage = CalculateDamageAmount(enemy, hittingHandWeapon, hand, specialDamageModifier, damageDealerPlayer);

                        bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits

                        if(enemy != null) enemy.enemyNetwork.lastDamageDealerNetId = damageDealerPlayer.NetAuth.netId;

                        DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = damageDealerPlayer.transform.position, 
                            receiverPosition = enemy.transform.position, hand = hand, bypassImmunity = bypass};

                        receiveMeleeDamage.TakeMeleeDamage(inflictedDamage, ctx);

                        // EXP GAIN
                        if (enemy != null)
                        {
                            // Check if player levels-after killing the enemy
                            int levelBeforeKillingEnemy = damageDealerPlayer.currentLevel;
                            IHealthAuthority enemyHealthAuthority = HealthAuthorityResolver.GetAuthority(enemy.gameObject);

                            if (enemyHealthAuthority.CurrentHealth <= 0)
                            {
                                damageDealerPlayer.NetAuth.Server_ExpGain(damageDealerPlayer.NetAuth.netIdentity, levelBeforeKillingEnemy, enemy.enemyNetwork.netIdentity);
                            }
                        }

                        if (damageDealerPlayer.isTriadExecutionActive) damageDealerPlayer.triadExecutionCounter++;

                        if (!isSheerCold) NetworkSoundManager.Instance.ServerPlaySound(SoundName.SwordImpact, damageDealerPlayer.transform.position);

                        if (!enemyCombatData.Isboss && enemy.health.GetCurrentHealth() > 0)
                        {
                            CheckBleedingStatus(hittingHandWeaponDetails, enemy, enemyCombatData, damageDealerPlayer);
                            CheckStunStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, false, damageDealerPlayer);
                            CheckSlowStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer);
                            CheckChillStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer);
                            CheckFrostStatus(hittingHandWeaponDetails, enemy, enemyCombatData, isSheerCold, damageDealerPlayer);
                            CheckShatterStatus(hittingHandWeaponDetails, enemy, enemyCombatData, ref inflictedDamage, damageDealerPlayer);
                            CheckStaticStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer.isConductiveTouchActive, damageDealerPlayer);
                            CheckParalyzeStatus(hittingHandWeaponDetails, enemy, enemyCombatData, damageDealerPlayer);
                            CheckRootStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer);
                            CheckWarmStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer);
                            CheckBurnStatus(hittingHandWeaponDetails, enemy, enemyCombatData, damageDealerPlayer);
                            CheckPoisonStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer);
                            CheckBlindStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer);
                            CheckFearStatus(hittingHandWeaponDetails, enemy, enemyCombatData, false, damageDealerPlayer);
                        }

                        if (damageDealerPlayer.playerDetails.playerCharacterIndex == Character.Morven && damageDealerPlayer.isStealthActive)
                        {
                            damageDealerPlayer.playerSkillController.Unstealth();
                        }

                        // Knockback
                        knockbackDir = (enemy.transform.position - damageDealerPlayer.transform.position).normalized;
                        knockbackForce = 8f; // Normal hit force
                        dealDamageMass = 1f;

                        enemy.enemyMovementNetwork.ServerApplyKnockback(knockbackDir, knockbackForce, dealDamageMass);
                    }
                    else
                    {
                        // Enemy dodged
                        enemyHealth.isDodging = true;
                        enemy.healthEvent.CallDodgeEvent();
                        enemyHealth.PostHitImmunity(true);

                        enemy.enemyNetwork.lastDamageDealerNetId = damageDealerPlayer.NetAuth.netId;

                        DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = damageDealerPlayer.transform.position, 
                            receiverPosition = enemyHealth.transform.position, hand = hand};
                        receiveMeleeDamage.TakeMeleeDamage(0, ctx);

                        // Knockback
                        Vector2 knockbackDir = (enemy.transform.position - damageDealerPlayer.transform.position).normalized;
                        float knockbackForce = 3f; // Normal hit force
                        float dealDamageMass = 1f;

                        enemy.enemyMovementNetwork.ServerApplyKnockback(knockbackDir, knockbackForce, dealDamageMass);
                    }
                }
            }
        }
    }

    private static Transform ResolveAttackOrigin(Player player, AttackShape attackShape, MeleeHand hand)
    {
        var melee = player.meleeAttackMainHand;

        if (hand == MeleeHand.MainHand)
        {
            return attackShape switch
            {
                AttackShape.Swing => melee.mainHandCircleOriginTransform,
                AttackShape.Thrust => melee.mainHandBoxOriginTransform,
                AttackShape.Cone => melee.mainHandTriangleOriginTransform,
                _ => null
            };
        }
        else
        {
            return attackShape switch
            {
                AttackShape.Swing => melee.offHandCircleOriginTransform,
                AttackShape.Thrust => melee.offHandBoxOriginTransform,
                AttackShape.Cone => melee.offHandTriangleOriginTransform,
                _ => null
            };
        }
    }

    /// <summary>
    /// Dummy hit interactions
    /// </summary>
    private static void DummyCheck(MeleeHand hand, ReceiveMeleeDamage receiveMeleeDamage, Player dealerPlayer, WeaponDetailsSO weaponDetails, bool isShieldBash = false)
    {
        // Damage produced by player
        int damageDone = dealerPlayer.isCursed ? (hand == MeleeHand.MainHand ? dealerPlayer.currentMainHandMinDamageValue : dealerPlayer.currentOffHandMinDamageValue)
            : Random.Range(hand == MeleeHand.MainHand ? dealerPlayer.currentMainHandMinDamageValue : dealerPlayer.currentOffHandMinDamageValue,
                hand == MeleeHand.MainHand ? dealerPlayer.currentMainHandMaxDamageValue : dealerPlayer.currentOffHandMaxDamageValue);

        if (isShieldBash)
        {
            damageDone = (int)(Random.Range(dealerPlayer.currentMainHandMinDamageValue, dealerPlayer.currentMainHandMaxDamageValue) * 0.5f);
        }

        Weapon weapon = new Weapon(Rarity.Basic);

        if (hand == MeleeHand.MainHand) weapon = dealerPlayer.activeWeapon.GetCurrentMainHandWeapon();
        else weapon = dealerPlayer.activeWeapon.GetCurrentOffHandWeapon();

        bool criticalHitHappened = CriticalHitHappened(null, dealerPlayer);

        // Calculate damage after critical hit check
        if (dealerPlayer.isStealthActive)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (weaponDetails.criticalHitDamageMultiplier + dealerPlayer.additionalCriticalMeleeDamageModifier +
                dealerPlayer.additionalCriticalDamageOnCloakedPrecision)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * weaponDetails.criticalHitDamageMultiplier +
                dealerPlayer.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits

        DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = dealerPlayer.transform.position, receiverPosition = receiveMeleeDamage.transform.position, hand = hand, bypassImmunity = bypass };
        receiveMeleeDamage.TakeMeleeDamage(damageDone, ctx);
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    public static int CalculateDamageAmount(Enemy enemy, Weapon weapon, MeleeHand hand, float specialMoveDamageModifier, Player dealerPlayer)
    {
        int damageDone = dealerPlayer.isCursed
            ? (hand == MeleeHand.MainHand ? dealerPlayer.currentMainHandMinDamageValue : dealerPlayer.currentOffHandMinDamageValue)
            : Random.Range(
                hand == MeleeHand.MainHand ? dealerPlayer.currentMainHandMinDamageValue : dealerPlayer.currentOffHandMinDamageValue,
                hand == MeleeHand.MainHand ? dealerPlayer.currentMainHandMaxDamageValue : dealerPlayer.currentOffHandMaxDamageValue
            );

        damageDone = (int)(damageDone * specialMoveDamageModifier);

        float playerCurrentHealth = dealerPlayer.health.GetCurrentHealth();
        float playerMaximumHealth = dealerPlayer.health.GetMaximumHealth();

        float enemyCurrentHealth = enemy.health.GetCurrentHealth();
        float enemyMaximumHealth = enemy.health.GetMaximumHealth();

        if (dealerPlayer.playerDetails.playerCharacterIndex == Character.Karnag)
        {
            int missingHealthDamage = (int)((playerMaximumHealth - playerCurrentHealth) * 0.2f);
            damageDone += missingHealthDamage;
        }

        float focusedAgrressionModifier = 0.05f;
        float punishersWillModifier = 0.05f;

        float totalDamageModifiers = 0f;

        // Focused Aggression Check
        if (dealerPlayer.isFocusedAggressionActive && playerCurrentHealth / playerMaximumHealth > 0.8f) totalDamageModifiers += focusedAgrressionModifier;

        // Punisher's Will Check
        if (dealerPlayer.isPunishersWillActive && enemyCurrentHealth / enemyMaximumHealth < 0.5f) totalDamageModifiers += punishersWillModifier;

        damageDone = (int)(damageDone * (1 + totalDamageModifiers)); // Add additional damage modifiers

        bool criticalHitHappened = CriticalHitHappened(enemy, dealerPlayer);

        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();

            NetworkSoundManager.Instance.ServerPlaySound(SoundName.CriticalHit, dealerPlayer.transform.position);
        }

        float critMultiplier = 0f;

        if (hand == MeleeHand.MainHand) critMultiplier = dealerPlayer.currentMainHandCriticalHitDamage;
        else if (hand == MeleeHand.OffHand) critMultiplier = dealerPlayer.currentOffHandCriticalHitDamage;

        if ((enemy != null && enemy.isBlind) || dealerPlayer.isStealthActive) critMultiplier += dealerPlayer.additionalCriticalDamageOnCloakedPrecision;

        damageDone = criticalHitHappened ? (int)(damageDone * critMultiplier) : damageDone;

        int baseElemental = (int)(weapon.weaponStats.elementalForgeRate * damageDone);
        int elementalDamage = (int)(baseElemental * (1 + dealerPlayer.additionalMagicDamageModifier));
        int nonElementalDamage = damageDone - baseElemental;

        // ARMOR DEDUCTIONS
        float effectiveArmor = enemy.currentArmor;

        float armorPenetrationModifier = 0f;

        if (dealerPlayer.meleeAttackMainHand.isBloodDrain)
        {
            armorPenetrationModifier = dealerPlayer.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.2f,
                2 => 0.25f,
                3 => 0.3f,
                _ => 0f
            };

            effectiveArmor *= (1 - armorPenetrationModifier - dealerPlayer.currentArmorPenetrationValue);
        }

        if (dealerPlayer.meleeAttackMainHand.isDontBlink) effectiveArmor *= 0.7f - dealerPlayer.currentArmorPenetrationValue; // 30% Armor Penetration

        if (dealerPlayer.isShatterCryActive) effectiveArmor *= 0.8f - dealerPlayer.currentArmorPenetrationValue;

        int inflictedNonElemental = (int)(nonElementalDamage * (1 - effectiveArmor));

        // Apply bonus dark damage if BloodDrain is active and weapon is Dark elemental
        if (dealerPlayer.meleeAttackMainHand.isBloodDrain) elementalDamage = (int)(elementalDamage * 1.5f); // +50% dark damage

        // Apply bonus dark damage if CullTheMeek is active and weapon is Dark elemental
        if (dealerPlayer.meleeAttackMainHand.isCullTheMeek)
        {
            int enemyMissingHealth = enemy.health.GetMaximumHealth() - enemy.health.GetCurrentHealth();

            float elementalDamageModifier = dealerPlayer.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.2f,
                2 => 0.25f,
                3 => 0.3f,
                _ => 0f
            };

            elementalDamage = (int)(enemyMissingHealth * elementalDamageModifier + elementalDamage);
        }

        int inflictedElemental = 0;


        inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.magicResistance));

        int totalInflictedDamage = inflictedNonElemental + inflictedElemental;

        if (dealerPlayer.meleeAttackMainHand.isBloodDrain)
        {
            // DRAIN HEALTH
            float drainedHealthPercentage = dealerPlayer.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.15f,
                2 => 0.18f,
                3 => 0.23f,
                _ => 0f
            };

            float drainedHealth = totalInflictedDamage * (drainedHealthPercentage + dealerPlayer.currentLifeStealValue);
            dealerPlayer.health.AddHealth((int)drainedHealth);
        }
        else if (dealerPlayer.isFeastOfWarActive)
        {
            float feastOfWarDrainPercentage = dealerPlayer.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.1f,
                2 => 0.12f,
                3 => 0.15f,
                _ => 0
            };

            // DRAIN HEALTH
            float drainedHealth = totalInflictedDamage * (feastOfWarDrainPercentage + dealerPlayer.currentLifeStealValue);
            dealerPlayer.health.AddHealth((int)drainedHealth);
        }
        else
        {
            // CHECK IF PLAYER HAS LIFE DRAIN
            float drainedHealth = totalInflictedDamage * dealerPlayer.currentLifeStealValue;
            if (drainedHealth > 0f + Mathf.Epsilon) dealerPlayer.health.AddHealth((int)drainedHealth);
        }

        return totalInflictedDamage;
    }

    /// <summary>
    /// Critical hit check
    /// </summary>
    /// <returns></returns>
    private static bool CriticalHitHappened(Enemy enemy, Player dealerPlayer)
    {
        bool criticalHitHappened = false;

        float specialSkillCriticalChanceModifier = 0f;

        if (dealerPlayer.meleeAttackMainHand.isDontBlink)
        {
            specialSkillCriticalChanceModifier = dealerPlayer.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.1f,
                2 => 0.15f,
                3 => 0.2f,
                _ => 0f
            };
        }

        if (dealerPlayer.isStealthActive)
        {
            if (!dealerPlayer.isBlind)
            {
                criticalHitHappened = true;
            }
        }
        else
        {
            float randomCriticalDice = Random.Range(0f, 1f);

            if (dealerPlayer.isBlind)
            {
                criticalHitHappened = false;
            }
            else if (enemy != null && enemy.isBlind && dealerPlayer.playerDetails.playerCharacterIndex == Character.Morven)
            {
                criticalHitHappened = randomCriticalDice < dealerPlayer.currentMainHandCriticalHitChance + 0.25f; // Morven - Cloaked Precision Passive
            }
            else
            {
                criticalHitHappened = randomCriticalDice < dealerPlayer.currentMainHandCriticalHitChance + specialSkillCriticalChanceModifier;
            }
        }

        return criticalHitHappened;
    }

    /// <summary>
    /// Check bleeding status
    /// </summary>
    private static void CheckBleedingStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToBleeding) return;

        // Check get bleeding
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < weaponDetails.bleedingChance + dealerPlayer.additionalStatusEffectInflictModifier +
            dealerPlayer.additionalBleedChance)
        {
            enemy.healthEvent.CallGetBleedingEvent();
            enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
            enemy.statusEffectAnimators.bleedAnimator.SetTrigger(Settings.activateVFX);
        }
    }

    /// <summary>
    /// Check warm status
    /// </summary>
    public static void CheckWarmStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isFlameLotus, Player dealerPlayer)
    {
        bool isBurned = (enemy.healthStatus & HealthStatus.Burned) != 0;

        if (enemy.health.currentHealth > 0 || isFlameLotus)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < weaponDetails.warmChance + dealerPlayer.additionalStatusEffectInflictModifier || isFlameLotus)
            {
                if (enemy.isWarmed && !isBurned)
                {
                    enemy.healthStatus |= HealthStatus.Burned; // Second warm 
                    enemy.healthEvent.CallWarmCuredEvent();
                }
                else if (!enemy.isChilled && !isBurned)
                {
                    enemy.isWarmed = true;
                    enemy.healthEvent.CallGetWarmedEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check burn status
    /// </summary>
    private static void CheckBurnStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToBurn) return;

        // Check get bleeding
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < weaponDetails.burnChance + dealerPlayer.additionalStatusEffectInflictModifier +
            dealerPlayer.additionalBurnChance)
        {
            enemy.healthEvent.CallGetBurnEvent();
            enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
            enemy.statusEffectAnimators.burnAnimator.SetTrigger(Settings.activateVFX);
        }
    }

    /// <summary>
    /// Check poison status
    /// </summary>
    public static void CheckPoisonStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isVenomousIvy, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToPoison) return;

        if (isVenomousIvy)
        {
            enemy.poisonDuration = dealerPlayer.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 2,
                2 => 3,
                3 => 4,
                _ => 2
            };
        }

        // Check get bleeding
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < weaponDetails.poisonChance + dealerPlayer.additionalStatusEffectInflictModifier +
            dealerPlayer.additionalPoisonChance || isVenomousIvy)
        {
            enemy.healthEvent.CallGetPoisonedEvent();
            enemy.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
            enemy.statusEffectAnimators.poisonAnimator.SetTrigger(Settings.activateVFX);
        }
    }

    /// <summary>
    /// Check chill status
    /// </summary>
    public static void CheckChillStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isBlizzard, Player dealerPlayer)
    {
        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        if (enemy.health.currentHealth > 0 || isBlizzard)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < weaponDetails.chillChance + dealerPlayer.additionalStatusEffectInflictModifier || isBlizzard)
            {
                if (enemy.isChilled && !isFrozen && !isBlizzard)
                {
                    enemy.moveStatus |= MoveStatus.Frozen; // Second chill 
                    enemy.healthEvent.CallChillCuredEvent();
                }
                else if (!enemy.isChilled && !isFrozen)
                {
                    if (isBlizzard) dealerPlayer.chillDuration = 5;

                    enemy.isChilled = true;
                    enemy.healthEvent.CallGetChillEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check frost status
    /// </summary>
    private static void CheckFrostStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isSheerCold, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToFrost) return;

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        if (!isFrozen || isSheerCold)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < weaponDetails.frostChance + dealerPlayer.additionalStatusEffectInflictModifier +
            dealerPlayer.additionalFreezeChance || isSheerCold)
            {
                enemy.moveStatus |= MoveStatus.Frozen;
                enemy.statusEffectAnimators.frostAnimator.SetTrigger(Settings.activateVFX);
            }
        }
    }

    /// <summary>
    /// Check shatter status
    /// </summary>
    private static void CheckShatterStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, ref int inflictedDamage, Player dealerPlayer)
    {
        Health enemyHealth = enemy.GetComponent<Health>();

        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        if (isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < 0.25f)
            {
                inflictedDamage *= 2;
                enemy.isShattered = true;
                DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = dealerPlayer.transform.position, receiverPosition = enemy.transform.position, hand = MeleeHand.MainHand, bypassImmunity = true };
                ReceiveMeleeDamage receiveMeleeDamage = enemyHealth.GetComponent<ReceiveMeleeDamage>();
                receiveMeleeDamage.TakeMeleeDamage(inflictedDamage, ctx);

                enemy.healthEvent.CallGetShatteredEvent();

                NetworkSoundManager.Instance.ServerPlaySound(SoundName.SuddenDeath, enemy.transform.position);
            }
        }
    }

    /// <summary>
    /// Check static status
    /// </summary>
    public static void CheckStaticStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isNymarasWindveil, bool isConductiveTouch, Player dealerPlayer)
    {
        bool isParalyzed = (enemy.moveStatus & MoveStatus.Paralyze) != 0;

        if (enemy.health.currentHealth > 0 || isNymarasWindveil)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < weaponDetails.staticChance + dealerPlayer.additionalStatusEffectInflictModifier
                || isNymarasWindveil || isConductiveTouch)
            {
                if (enemy.isStatic && !isParalyzed && !isNymarasWindveil)
                {
                    enemy.moveStatus |= MoveStatus.Paralyze; // Second static
                    enemy.healthEvent.CallStaticCuredEvent();
                }
                else if (!enemy.isChilled && !isParalyzed)
                {
                    enemy.isStatic = true;
                    enemy.healthEvent.CallGetStaticEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check paralyze status
    /// </summary>
    private static void CheckParalyzeStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToParalyze) return;

        bool isParalyzed = (enemy.moveStatus & MoveStatus.Paralyze) != 0;

        if (!isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < weaponDetails.paralyzeChance + dealerPlayer.additionalStatusEffectInflictModifier +
                dealerPlayer.additionalParalyzeChance)
            {
                enemy.moveStatus |= MoveStatus.Paralyze;
                enemy.statusEffectAnimators.paralyzeAnimator.SetTrigger(Settings.activateVFX);
            }
        }
    }

    /// <summary>
    /// Check slow status
    /// </summary>
    public static void CheckSlowStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isAbsoluteZero, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToSlow) return;

        if (!enemy.isSlowed && isAbsoluteZero)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < weaponDetails.slowChance + dealerPlayer.additionalStatusEffectInflictModifier +
                dealerPlayer.additionalSlowChance || isAbsoluteZero)
            {
                enemy.isSlowed = true;
                enemy.healthEvent.CallGetSlowEvent();
                enemy.statusEffectAnimators.slowAnimator.SetTrigger(Settings.activateVFX);
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    public static void CheckStunStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool shieldBash, bool isGrapple, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToStun) return;

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isStunned = (enemy.moveStatus & MoveStatus.Stun) != 0;

        if (shieldBash)
        {
            enemy.moveStatus |= MoveStatus.Stun;

            if (dealerPlayer != null)
            {
                enemy.stunDuration = dealerPlayer.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => enemy.stunDuration
                };
            }

            enemy.healthEvent.CallGetStunEvent();
            enemy.statusEffectAnimators.stunAnimator.SetTrigger(Settings.activateVFX);
            return;
        }

        if (!isStunned || isGrapple)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < weaponDetails.stunChance + dealerPlayer.additionalStatusEffectInflictModifier +
                dealerPlayer.additionalStunChance || isGrapple)
            {
                enemy.statusEffectAnimators.stunAnimator.SetTrigger(Settings.activateVFX);
                enemy.moveStatus |= MoveStatus.Stun;
                enemy.healthEvent.CallGetStunEvent();
            }
        }
    }

    /// <summary>
    /// Check root status
    /// </summary>
    public static void CheckRootStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isVenomousIvy, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToRoot) return;

        bool isRooted = (enemy.moveStatus & MoveStatus.Root) != 0;

        if (!isRooted || isVenomousIvy)
        {
            if (isVenomousIvy)
            {
                enemy.rootDuration = dealerPlayer.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => 2
                };
            }

            float randomDice = Random.Range(0f, 1f);
            if (randomDice < weaponDetails.rootChance + dealerPlayer.additionalStatusEffectInflictModifier +
                dealerPlayer.additionalRootChance || isVenomousIvy)
            {
                enemy.statusEffectAnimators.rootAnimator.SetTrigger(Settings.activateVFX);
                enemy.moveStatus |= MoveStatus.Root;
                enemy.healthEvent.CallGetRootEvent();
            }
        }
    }

    /// <summary>
    /// Check blind status
    /// </summary>
    public static void CheckBlindStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool umbralMist, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToBlind) return;

        if (umbralMist)
        {
            enemy.healthEvent.CallGetBlindEvent();
            return;
        }

        if (!enemy.isBlind)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < weaponDetails.blindChance + dealerPlayer.additionalStatusEffectInflictModifier +
            dealerPlayer.additionalBlindChance + dealerPlayer.additionalStatusEffectInflictModifier)
            {
                enemy.statusEffectAnimators.blindAnimator.SetTrigger(Settings.activateVFX);
                enemy.isBlind = true;
                enemy.healthEvent.CallGetBlindEvent();
            }
        }
    }

    /// <summary>
    /// Check fear status - Enemy
    /// </summary>
    public static void CheckFearStatus(WeaponDetailsSO weaponDetails, Enemy enemy, IEnemyCombatData enemyCombatData, bool isShatterCry, Player dealerPlayer)
    {
        if (enemyCombatData.IsImmuneToFear) return;

        if (isShatterCry && !enemy.isFeared)
        {
            if (isShatterCry)
            {
                enemy.fearDuration = dealerPlayer.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => 2
                };
            }

            float randomDice = Random.Range(0f, 1f);
            if (randomDice < weaponDetails.fearChance + dealerPlayer.additionalStatusEffectInflictModifier +
            dealerPlayer.additionalFearChance || isShatterCry)
            {
                enemy.statusEffectAnimators.fearAnimator.SetTrigger(Settings.activateVFX);
                enemy.isFeared = true;
                enemy.healthEvent.CallGetFearEvent();
            }
        }
    }
}
