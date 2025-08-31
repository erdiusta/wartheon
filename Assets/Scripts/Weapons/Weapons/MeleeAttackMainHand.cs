using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackMainHand : MonoBehaviour
{
    public bool IsAttacking { get; set; }

    [SerializeField] Transform mainHandCircleOriginTransform;
    [SerializeField] Transform mainHandBoxOriginTransform;
    [SerializeField] Transform mainHandTriangleOriginTransform;

    [SerializeField] Transform offHandCircleOriginTransform;
    [SerializeField] Transform offHandBoxOriginTransform;
    [SerializeField] Transform offHandTriangleOriginTransform;

    MeleeAttackEvent meleeAttackEvent;
    AttackShape attackShape;
    AnimationEventHelperMainHand rightHandAnimationEventHelper;
    Animator mainHandWeaponAnimator;

    Player player;
    bool mainHandAttackBlocked;
    bool offHandAttackBlocked;

    // SpecialAttacks
    bool isBloodDrain;
    bool isCullTheMeek;
    bool isSheerCold;
    bool isDontBlink;
    bool isWhisperSlice;

    // Collision fields
    Collider2D[] _colliders = new Collider2D[10]; // Initial size
    ContactFilter2D _contactFilter = new ContactFilter2D().NoFilter(); // Default filter

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandAnimationEventHelper = GetComponent<AnimationEventHelperMainHand>();
        mainHandWeaponAnimator = transform.GetChild(0).GetComponent<Animator>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnAttack += MeleeAttackEvent_MeleeAttack;
        rightHandAnimationEventHelper.OnAnimationMainHandEventTriggered.AddListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackMainHandPerformed.AddListener(DetectColliders);

        rightHandAnimationEventHelper.OnShieldBashEventPerformed.AddListener(DetectCollidersForShieldBash);
        rightHandAnimationEventHelper.OnShieldBashEventCompleted.AddListener(ResetIsShieldBashing);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnAttack -= MeleeAttackEvent_MeleeAttack;
        rightHandAnimationEventHelper.OnAnimationMainHandEventTriggered.RemoveListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackMainHandPerformed.RemoveListener(DetectColliders);

        rightHandAnimationEventHelper.OnShieldBashEventPerformed.RemoveListener(DetectCollidersForShieldBash);
        rightHandAnimationEventHelper.OnShieldBashEventCompleted.RemoveListener(ResetIsShieldBashing);
    }

    private void MeleeAttackEvent_MeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        Attack(meleeAttackEventArgs.weapon, meleeAttackEventArgs.attackShape, meleeAttackEventArgs.meleeHand, meleeAttackEventArgs.isBloodDrain, 
            meleeAttackEventArgs.shieldBash, meleeAttackEventArgs.isCullTheMeek, meleeAttackEventArgs.isSheerCold, meleeAttackEventArgs.isDontBlink,
            meleeAttackEventArgs.isBladeDash);
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectCollidersForShieldBash()
    {
        if (!IsAttacking) return;

        // Optional: also include off-hand detection here if desired
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        if (offHandWeapon.weaponDetails.weaponClass == WeaponClass.Shield)
        {
            DetectHandHit(offHandWeapon, AttackShape.Cone, MeleeHand.OffHand, isBloodDrain, true);
        }
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectColliders()
    {
        if (!IsAttacking) return;

        Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

        if (mainHandWeapon.weaponDetails == null)
        {
            Debug.LogError("Main weapon's weapon details reference is null!");
        }

        DetectHandHit(mainHandWeapon, attackShape, MeleeHand.MainHand, isBloodDrain, false, isSheerCold, isDontBlink, isWhisperSlice);

        // Optional: also include off-hand detection here if desired
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        if (offHandWeapon?.weaponDetails.isMeleeWeapon == true)
        {
            if (isDontBlink)
            {
                DetectHandHit(offHandWeapon, AttackShape.Thrust, MeleeHand.OffHand, isBloodDrain, false, false, isDontBlink: true);
            }
            else
            {
                AttackShape offHandMeleeAttackType = offHandWeapon.weaponDetails.hasSwing ? AttackShape.Swing : AttackShape.Thrust;
                DetectHandHit(offHandWeapon, offHandMeleeAttackType, MeleeHand.OffHand, isBloodDrain);
            }
        }
    }

    private void DetectHandHit(Weapon weapon, AttackShape attackType, MeleeHand hand, bool isBloodDrain, bool shieldBash = false, bool isSheerCold = false,
        bool isDontBlink = false, bool isWhisperSlice = false)
    {
        if (hand == MeleeHand.OffHand && player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.isMeleeWeapon &&
            player.isAxeThrowActive)
        {
            Vector3 weaponDirection;
            float weaponAngleDegrees, playerAngleDegrees;
            AimDirection playerAimDirection;
            AttackDirection playerAttackDirection;

            // Aim weapon input
            player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

            // Trigger fire weapon event
            //SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false,
                0, 0, 0, 0, 0, 0, 0, 0, false, false, false, null, null, false, null, false, null, true, player.playerDetails.throwingAxeDetails);

            DropItem.droppedThrowingAxe = player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails;

            player.playerControl.DeactivateOffhandWeapon();

            // Update stat values
            player.UpdateDamageValues();
            player.UpdateArmorValues();
            player.UpdateAttackRatingAndCriticalValues();
            player.UpdateBlockAndEvasivenessValues();

            StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponOffHand);

            player.offHandSlotFilled = false;
        }

        Transform originTransform = null;

        if (hand == MeleeHand.MainHand)
        {
            originTransform = attackType switch
            {
                AttackShape.Swing => mainHandCircleOriginTransform,
                AttackShape.Thrust => mainHandBoxOriginTransform,
                AttackShape.Cone => mainHandTriangleOriginTransform,
                _ => null
            };
        }
        else if (hand == MeleeHand.OffHand)
        {
            originTransform = attackType switch
            {
                AttackShape.Swing => offHandCircleOriginTransform,
                AttackShape.Thrust => offHandBoxOriginTransform,
                AttackShape.Cone => offHandTriangleOriginTransform,
                _ => null
            };
        }

        Collider2D attackCollider = originTransform.GetComponent<Collider2D>();

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
            if (collider.CompareTag(Settings.playerTag) || collider.CompareTag(Settings.decoyTag) || collider.CompareTag(Settings.chestItemTag)) continue;

            // Ignore capsule colliders used for blocking
            if (collider is CapsuleCollider2D && (collider.GetComponent<Enemy>() != null || collider.GetComponent<Player>() != null)) continue;

            if(collider is CircleCollider2D && (collider.GetComponent<Enemy>() != null || collider.GetComponent<Player>() != null)) continue;

            // Handle destructibles / environment
            if (collider.TryGetComponent(out Environment environment) &&
                collider.TryGetComponent(out Health envHealth))
            {
                SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponImpactSoundEffect);
                envHealth.TakeDamage(100, transform.position, collider.transform.position, null, hand); continue;
            }

            if (!collider.TryGetComponent(out Health enemyHealth)) continue;

            if (collider.CompareTag(Settings.practiceDummy))
            {
                SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponImpactSoundEffect);
                DummyCheck(collider, hand);
                continue;
            }

            if (!collider.TryGetComponent(out Enemy enemy)) continue;

            // Calculate hit chance
            bool attackHits = player.currentAttackRatingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100);

            if (attackHits)
            {
                int inflictedDamage = 0;

                Vector2 knockbackDir = Vector2.zero;
                float knockbackForce = 0f;
                float dealDamageMass = 0f;

                float specialDamageModifier = 1f; // Default value

                if (shieldBash)
                {
                    CheckStunStatus(enemy, true);

                    Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

                    if (mainHandWeapon != null)
                    {
                        specialDamageModifier = 0.5f;

                        inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(enemy, mainHandWeapon, MeleeHand.MainHand, specialDamageModifier);

                        if (enemy.health != null)
                        {
                            enemy.health.TakeDamage(inflictedDamage, transform.position, enemy.health.transform.position, null, MeleeHand.None);
                        }
                    }

                    if (player.isTriadExecutionActive) player.triadExecutionCounter++;

                    // Knockback
                    knockbackDir = (enemy.transform.position - transform.position).normalized;
                    knockbackForce = 12f; // Shield bash hit force
                    dealDamageMass = 1f;

                    enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                    continue;
                }

                if (!enemy.enemyDetails.isEnemyBoss)
                {
                    CheckSuddenDeathStatus(enemy, enemyHealth);
                }

                if (enemyHealth.suddenDeathHappened)
                {
                    enemyHealth.TakeDamage(enemyHealth.GetCurrentHealth() + 10, transform.position, enemy.transform.position, null, hand);
                    if (player.isTriadExecutionActive) player.triadExecutionCounter++;
                    continue;
                }

                // Special damage calculation
                if (isSheerCold)
                {
                    specialDamageModifier = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 1f,
                        2 => 1.15f,
                        3 => 1.35f,
                        _ => 1f
                    };
                }

                if (isDontBlink)
                {
                    specialDamageModifier = player.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
                    {
                        1 => 1.2f,
                        2 => 1.3f,
                        3 => 1.5f,
                        _ => 1f
                    };
                }

                // DAMAGE CALCULATION
                inflictedDamage = CalculateDamageAmount(enemy, weapon, hand, specialDamageModifier);

                bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits
                enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, null, hand, bypass);
                if (player.isTriadExecutionActive) player.triadExecutionCounter++;

                if (!isSheerCold) SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponImpactSoundEffect);

                if (!enemy.enemyDetails.isEnemyBoss && enemy.health.GetCurrentHealth() > 0)
                {
                    CheckBleedingStatus(enemy);
                    CheckStunStatus(enemy);
                    CheckSlowStatus(enemy);
                    CheckAcidStatus(enemy);
                    CheckChillStatus(enemy);
                    CheckFrostStatus(enemy, isSheerCold);
                    CheckShatterStatus(enemy, ref inflictedDamage);
                    CheckStaticStatus(enemy, false, player.isConductiveTouchActive);
                    CheckParalyzeStatus(enemy);
                    CheckRootStatus(enemy);
                    CheckWarmStatus(enemy);
                    CheckBurnStatus(enemy);
                    CheckPoisonStatus(enemy);
                    CheckBlindStatus(enemy);
                    CheckFearStatus(enemy);
                }

                if (player.playerDetails.playerCharacterIndex == Character.Morven && player.isStealthActive)
                {
                    player.playerControl.Unstealth();
                }

                // Knockback
                knockbackDir = (enemy.transform.position - transform.position).normalized;
                knockbackForce = 8f; // Normal hit force
                dealDamageMass = 1f;

                enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
            }
            else
            {
                // Enemy dodged
                enemyHealth.isDodging = true;
                enemy.healthEvent.CallDodgeEvent();
                enemyHealth.PostHitImmunity(true);
                enemyHealth.TakeDamage(0, transform.position, enemyHealth.transform.position, null, hand);

                // Knockback
                Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                float knockbackForce = 3f; // Normal hit force
                float dealDamageMass = 1f;

                enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
            }
        }
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    public int CalculateDamageAmount(Enemy enemy, Weapon weapon, MeleeHand hand, float specialMoveDamageModifier = 1f)
    {
        int damageDone = player.isCursed
            ? (hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue)
            : Random.Range(
                hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue,
                hand == MeleeHand.MainHand ? player.currentMainHandMaxDamageValue : player.currentOffHandMaxDamageValue
            );

        damageDone = (int)(damageDone * specialMoveDamageModifier);

        float playerCurrentHealth = player.health.GetCurrentHealth();
        float playerMaximumHealth = player.health.GetMaximumHealth();

        float enemyCurrrentHealth = enemy.health.GetCurrentHealth();
        float enemyMaximumHealth = enemy.health.GetMaximumHealth();

        if(player.playerDetails.playerCharacterIndex == Character.Karnag)
        {
            int missingHealthDamage = (int)((playerCurrentHealth - playerMaximumHealth) * 0.2f);
            damageDone += missingHealthDamage;
        }

        float focusedAgrressionModifier = 0.05f;
        float punishersWillModifier = 0.05f;

        float totalDamageModifiers = 0f;

        // Focused Aggression Check
        if (player.isFocusedAggressionActive && playerCurrentHealth / playerMaximumHealth > 0.8f) totalDamageModifiers += focusedAgrressionModifier;

        // Punisher's Will Check
        if (player.isPunishersWillActive && enemyCurrrentHealth / enemyMaximumHealth < 0.5f) totalDamageModifiers += punishersWillModifier;

        damageDone = (int)(damageDone * (1 + totalDamageModifiers)); // Add additional damage modifiers

        bool criticalHitHappened = CriticalHitHappened(enemy);

        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        }

        float critMultiplier = weapon.weaponDetails.criticalHitDamageMultiplier + player.additionalCriticalMeleeDamageModifier;

        if ((enemy != null && enemy.isBlind) || player.isStealthActive) critMultiplier += player.additionalCriticalDamageOnCloakedPrecision;

        damageDone = criticalHitHappened ? (int)(damageDone * critMultiplier) : damageDone;

        int baseElemental = (int)(weapon.weaponDetails.elementalForgeRate * damageDone);
        int elementalDamage = (int)(baseElemental * (1 + player.additionalMagicDamageModifier));
        int nonElementalDamage = damageDone - baseElemental;

        // ARMOR DEDUCTIONS
        float effectiveArmor = enemy.currentArmor;

        float armorPenetrationModifier = 0f;

        if (isBloodDrain)
        {
            armorPenetrationModifier = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.2f,
                2 => 0.25f,
                3 => 0.3f,
                _ => 0f
            };

            effectiveArmor *= (1 - armorPenetrationModifier - player.additionalArmorPenetrationModifier);
        }

        if (isDontBlink) effectiveArmor *= 0.7f - player.additionalArmorPenetrationModifier; // 30% Armor Penetration

        if (player.isShatterCryActive) effectiveArmor *= 0.8f - player.additionalArmorPenetrationModifier;

        int inflictedNonElemental = (int)(nonElementalDamage * (1 - effectiveArmor));

        // Apply bonus dark damage if BloodDrain is active and weapon is Dark elemental
        if (isBloodDrain && weapon.weaponDetails.elementalBias == ElementalBias.Dark) elementalDamage = (int)(elementalDamage * 1.5f); // +50% dark damage

        // Apply bonus dark damage if CullTheMeek is active and weapon is Dark elemental
        if (isCullTheMeek && weapon.weaponDetails.elementalBias == ElementalBias.Dark)
        {
            int enemyMissingHealth = enemy.health.GetMaximumHealth() - enemy.health.GetCurrentHealth();

            float elementalDamageModifier = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.2f,
                2 => 0.25f,
                3 => 0.3f,
                _ => 0f
            };

            elementalDamage = (int)(enemyMissingHealth * elementalDamageModifier + elementalDamage);
        }

        int inflictedElemental = 0;

        switch (weapon.weaponDetails.elementalBias)
        {
            case ElementalBias.Fire:
                inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.fireResistance)); break;
            case ElementalBias.Water:
                inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.waterResistance)); break;
            case ElementalBias.Earth:
                inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.earthResistance)); break;
            case ElementalBias.Air:
                inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.airResistance)); break;
            case ElementalBias.Dark:
                inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.darkResistance)); break;
            case ElementalBias.Light:
                inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.lightResistance)); break;
        }

        int totalInflictedDamage = inflictedNonElemental + inflictedElemental;

        if (isBloodDrain)
        {
            // DRAIN HEALTH
            float drainedHealthPercentage = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.15f,
                2 => 0.18f,
                3 => 0.23f,
                _ => 0f
            };

            float drainedHealth = totalInflictedDamage * (drainedHealthPercentage + player.additionalLifeStealModifier);
            player.health.AddHealth((int)drainedHealth);
        }
        else if (player.isFeastOfWarActive)
        {
            float feastOfWarDrainPercentage = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.1f,
                2 => 0.12f,
                3 => 0.15f,
                _ => 0
            };

            // DRAIN HEALTH
            float drainedHealth = totalInflictedDamage * (feastOfWarDrainPercentage + player.additionalLifeStealModifier);
            player.health.AddHealth((int)drainedHealth);
        }
        else
        {
            // CHECK IF PLAYER HAS LIFE DRAIN
            float drainedHealth = totalInflictedDamage * player.additionalLifeStealModifier;
            if (drainedHealth > 0f + Mathf.Epsilon) player.health.AddHealth((int)drainedHealth);
        }

        return totalInflictedDamage;
    }

    /// <summary>
    /// Critical hit check
    /// </summary>
    /// <returns></returns>
    private bool CriticalHitHappened(Enemy enemy)
    {
        bool criticalHitHappened = false;

        float specialSkillCriticalChanceModifier = 0f;

        if (isDontBlink)
        {
            specialSkillCriticalChanceModifier = player.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.1f,
                2 => 0.15f,
                3 => 0.2f,
                _ => 0f
            };
        }

        if (player.isStealthActive)
        {
            if (!player.isBlind)
            {
                criticalHitHappened = true;
            }
        }
        else
        {
            float randomCriticalDice = Random.Range(0f, 1f);

            if (player.isBlind)
            {
                criticalHitHappened = false;
            }
            else if (enemy != null && enemy.isBlind && player.playerDetails.playerCharacterIndex == Character.Morven)
            {
                criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance + 0.25f; // Morven - Cloaked Precision Passive
            }
            else
            {
                criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance + specialSkillCriticalChanceModifier;
            }
        }

        return criticalHitHappened;
    }

    /// <summary>
    /// Check sudden death status
    /// </summary>
    private void CheckSuddenDeathStatus(Enemy enemy, Health enemyHealth)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.canKillSuddenly && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.suddenKillChance)
            {
                enemyHealth.suddenDeathHappened = true;
                enemy.destroyedEvent.CallDestroyedEvent(false);
                enemy.healthEvent.CallGetDeathEvent();
                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
            }
        }
    }

    /// <summary>
    /// Check bleeding status
    /// </summary>
    private void CheckBleedingStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasBleedingDamage)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.bleedingChance + player.additionalStatusEffectInflictModifier)
            {
                enemy.healthEvent.CallGetBleedingEvent();
                enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
            }
        }
    }

    /// <summary>
    /// Check warm status
    /// </summary>
    public void CheckWarmStatus(Enemy enemy, bool isFlameLotus = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isBurned = (enemy.healthStatus & HealthStatus.Burned) != 0;

        if ((player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasWarmDamage && enemy.health.currentHealth > 0) || isFlameLotus)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.warmChance + player.additionalStatusEffectInflictModifier || isFlameLotus)
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
    private void CheckBurnStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasBurnDamage)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.burnChance + player.additionalStatusEffectInflictModifier)
            {
                enemy.healthEvent.CallGetBurnEvent();
                enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
            }
        }
    }

    /// <summary>
    /// Check poison status
    /// </summary>
    public void CheckPoisonStatus(Enemy enemy, bool isActiveItem = false, bool isVenomousIvy = false)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasPoisonDamage || isVenomousIvy)
        {
            if (isVenomousIvy)
            {
                enemy.poisonDuration = player.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => 2
                };
            }

            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.poisonChance + player.additionalStatusEffectInflictModifier || isVenomousIvy)
            {
                enemy.healthEvent.CallGetPoisonedEvent();
                enemy.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    private void CheckAcidStatus(Enemy enemy)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasAcidDamage && enemy.armorStatus != ArmorStatus.Acid && 
            enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.acidEfficiency + player.additionalStatusEffectInflictModifier)
            {
                enemy.armorStatus = ArmorStatus.Acid;
                enemy.currentArmor = (float)Math.Round(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.acidEfficiency * enemy.currentArmor, 2);
                enemy.healthEvent.CallGetAcidEvent();
            }
        }
    }

    /// <summary>
    /// Check chill status
    /// </summary>
    public void CheckChillStatus(Enemy enemy, bool isBlizzard = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isFrozen = (enemyAI.moveStatus & MoveStatus.Frozen) != 0;

        if ((player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasChillDamage && enemy.health.currentHealth > 0) || isBlizzard)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.chillChance + player.additionalStatusEffectInflictModifier || isBlizzard)
            {
                if (enemy.isChilled && !isFrozen && !isBlizzard)
                {
                    enemy.enemyAI.moveStatus |= MoveStatus.Frozen; // Second chill 
                    enemy.healthEvent.CallChillCuredEvent();
                }
                else if (!enemy.isChilled && !isFrozen)
                {
                    if (isBlizzard) player.chillDuration = 5;

                    enemy.isChilled = true;
                    enemy.healthEvent.CallGetChillEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check frost status
    /// </summary>
    private void CheckFrostStatus(Enemy enemy, bool isSheerCold = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isFrozen = (enemyAI.moveStatus & MoveStatus.Frozen) != 0;

        if ((player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasFrostDamage && !isFrozen) || isSheerCold)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.frostChance + player.additionalStatusEffectInflictModifier || isSheerCold)
            {
                enemy.enemyAI.moveStatus |= MoveStatus.Frozen;
            }
        }
    }

    /// <summary>
    /// Check shatter status
    /// </summary>
    private void CheckShatterStatus(Enemy enemy, ref int inflictedDamage)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        Health enemyHealth = enemy.GetComponent<Health>();

        bool isFrozen = (enemyAI.moveStatus & MoveStatus.Frozen) != 0;

        if (isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < 0.25f)
            {
                inflictedDamage *= 2;
                enemy.isShattered = true;
                enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, null, MeleeHand.MainHand, true);
                enemy.healthEvent.CallGetShatteredEvent();
                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
            }
        }
    }



    /// <summary>
    /// Check static status
    /// </summary>
    public void CheckStaticStatus(Enemy enemy, bool isNymarasWindveil = false, bool isConductiveTouch = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isParalyzed = (enemyAI.moveStatus & MoveStatus.Paralyze) != 0;

        if ((player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasStaticDamage && enemy.health.currentHealth > 0) || isNymarasWindveil)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.staticChance + player.additionalStatusEffectInflictModifier 
                || isNymarasWindveil || isConductiveTouch)
            {
                if (enemy.isStatic && !isParalyzed && !isNymarasWindveil)
                {
                    enemy.enemyAI.moveStatus |= MoveStatus.Paralyze; // Second static
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
    private void CheckParalyzeStatus(Enemy enemy, bool isSheerCold = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isParalyzed = (enemyAI.moveStatus & MoveStatus.Paralyze) != 0;

        if ((player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasParalyzeDamage && !isParalyzed) || isSheerCold)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.paralyzeChance + player.additionalStatusEffectInflictModifier || isSheerCold)
            {
                enemy.enemyAI.moveStatus |= MoveStatus.Paralyze;
            }
        }
    }

    /// <summary>
    /// Check slow status
    /// </summary>
    public void CheckSlowStatus(Enemy enemy, bool isAbsoluteZero = false)
    {
        if (!enemy.isSlowed && (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasSlowDamage || isAbsoluteZero))
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.slowChance + player.additionalStatusEffectInflictModifier || isAbsoluteZero)
            {
                enemy.isSlowed = true;
                enemy.healthEvent.CallGetSlowEvent();
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    public void CheckStunStatus(Enemy enemy, bool shieldBash = false, bool isGrapple = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isStunned = (enemyAI.moveStatus & MoveStatus.Stun) != 0;

        if (shieldBash)
        {
            enemy.enemyAI.moveStatus |= MoveStatus.Stun;

            if (player != null)
            {
                enemy.stunDuration = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => enemy.stunDuration
                };
            }

            enemy.healthEvent.CallGetStunEvent();
            return;
        }

        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasStunDamage && !isStunned || isGrapple)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.stunChance + player.additionalStatusEffectInflictModifier || isGrapple)
            {
                enemy.enemyAI.moveStatus |= MoveStatus.Stun;
                enemy.healthEvent.CallGetStunEvent();
            }
        }
    }

    /// <summary>
    /// Check root status
    /// </summary>
    public void CheckRootStatus(Enemy enemy, bool isVenomousIvy = false)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isRooted = (enemyAI.moveStatus & MoveStatus.Root) != 0;

        if ((player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasRootDamage && !isRooted) || isVenomousIvy)
        {
            if (isVenomousIvy)
            {
                enemy.rootDuration = player.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => 2
                };
            }

            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.rootChance + player.additionalStatusEffectInflictModifier || isVenomousIvy)
            {
                enemy.enemyAI.moveStatus |= MoveStatus.Root;
                enemy.healthEvent.CallGetRootEvent();
            }
        }
    }

    /// <summary>
    /// Check blind status
    /// </summary>
    public void CheckBlindStatus(Enemy enemy, bool umbralMist = false)
    {
        if (umbralMist)
        {
            enemy.healthEvent.CallGetBlindEvent();
            return;
        }

        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasBlindDamage && !enemy.isBlind)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.blindChance + player.additionalBlindMakerModifier
                + player.additionalStatusEffectInflictModifier)
            {
                enemy.isBlind = true;
                enemy.healthEvent.CallGetBlindEvent();
            }
        }
    }

    /// <summary>
    /// Check fear status - Enemy
    /// </summary>
    public void CheckFearStatus(Enemy enemy, bool isShatterCry = false)
    {
        if ((player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasFearDamage || isShatterCry) && !enemy.isFeared)
        {
            if (isShatterCry)
            {
                enemy.fearDuration = player.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 2,
                    2 => 3,
                    3 => 4,
                    _ => 2
                };
            }

            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.fearChance + player.additionalStatusEffectInflictModifier || isShatterCry)
            {
                enemy.isFeared = true;
                enemy.healthEvent.CallGetFearEvent();
            }
        }
    }

    private void ShatterProcess(Enemy enemy)
    {
        enemy.healthEvent.CallGetShatteredEvent();
    }

    public void ResetIsAttackingRightHand()
    {
        IsAttacking = false;
        player.playerControl.meleeAttackTypeMainHand = AttackShape.None;
        ResetAnimations(isBloodDrain, isCullTheMeek, isSheerCold, isWhisperSlice);

        player.animatePlayer.SetIdleAnimationParameters();
    }

    private void ResetAnimations(bool isBloodDrain, bool isCullTheMeek, bool isSheerCold, bool isWhisperSlice)
    {
        if (isBloodDrain) player.animator.SetBool("blood", false);
        if (isCullTheMeek) player.animator.SetBool("cullTheMeek", false);
        if (isSheerCold)
        {
            player.animator.SetBool("sheerCold", false);
            mainHandWeaponAnimator.enabled = true;
        }
        if (isDontBlink) player.animator.SetBool("dontBlink", false);
        if (isWhisperSlice) player.animator.SetBool("whisperSlice", false);
    }

    public void ResetIsShieldBashing()
    {
        IsAttacking = false;
        player.animator.SetBool("shieldBash", false);
        transform.GetChild(2).GetComponent<Animator>().enabled = true;

        player.isShieldBashing = false;
        player.animatePlayer.SetIdleAnimationParameters();
    }

    private void Attack(Weapon weapon, AttackShape attackShape, MeleeHand hand,bool isBloodDrain, bool shieldBash, bool isCullTheMeek, bool isSheerCold, 
        bool isDontBlink, bool isWhisperSlice)
    {
        if (hand == MeleeHand.MainHand) if (mainHandAttackBlocked) return;
        if (hand == MeleeHand.OffHand) if (offHandAttackBlocked) return;

        if (isSheerCold) mainHandWeaponAnimator.enabled = false;

        // Ranged fire animation (Bow or staff)
        if (attackShape == AttackShape.None) // Ranged Attack
        {
            mainHandWeaponAnimator.SetTrigger(Settings.rangedWeaponAttack);
            return;
        }

        this.attackShape = attackShape; // Set attack shape

        this.isBloodDrain = isBloodDrain;
        this.isCullTheMeek = isCullTheMeek;
        this.isSheerCold = isSheerCold;
        this.isDontBlink = isDontBlink;
        this.isWhisperSlice = isWhisperSlice;

        if (!shieldBash && !isBloodDrain && !isCullTheMeek && !isSheerCold && !isDontBlink)
        {
            weapon.onCooldown = true;
            player.animatePlayer.SetAttackAnimationParameters();
        }

        Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        // This means that's neither a dual wield nor a shield
        if (offHandWeapon == null)
        {
            // If weapon is a spear, thrust motions should be enabled
            if (mainHandWeapon != null && mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Spear) 
            {
                if (mainHandWeapon.weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    player.animator.Play("EmptyThrustLong", 0, 0);
                }
                else
                {
                    player.animator.Play("EmptyThrustShort", 0, 0);
                }
            }
            else
            {
                if (mainHandWeapon != null)
                {
                    if (mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Dagger || mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw)
                    {
                        player.animator.Play("EmptyShort", 0, 0);  // Play short smear animation
                    }
                    else if (mainHandWeapon.weaponDetails.wieldType == WieldType.TwoHanded)
                    {
                        if(isSheerCold) 
                        {
                            player.animator.SetBool("sheerCold", true);
                            goto specialAttackMoves;
                        }
                        else player.animator.Play("EmptyLong", 0, 0);  // Play long smear animation
                    }
                    else
                    {
                        player.animator.Play("EmptyMedium", 0, 0); // Play medium smear animation
                    }
                }
            }
        }
        else if (offHandWeapon.weaponDetails.weaponClass == WeaponClass.Shield)
        {
            if (shieldBash)
            {
                player.animator.SetBool("shieldBash", true);

                goto specialAttackMoves;
            }

            if (mainHandWeapon != null && mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Spear)
            {
                player.animator.Play("EmptyThrustS", 0, 0);
            }
            else
            {
                if (mainHandWeapon != null)
                {
                    if (mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Dagger || mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw)
                    {
                        player.animator.Play("EmptyShortS", 0, 0);  // Play short smear animation
                    }
                    else
                    {
                        player.animator.Play("EmptyMediumS", 0, 0); // Play medium smear animation
                    }
                }
            }
        }
        else
        {
            if (mainHandWeapon != null)
            {
                if (mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Dagger)
                {
                    if (isBloodDrain)
                    {
                        player.animator.SetBool("blood", true);
                        goto specialAttackMoves;
                    }
                    else if (isCullTheMeek)
                    {
                        player.animator.SetBool("cullTheMeek", true);
                        goto specialAttackMoves;
                    }
                    else if (isDontBlink)
                    {
                        player.animator.SetBool("dontBlink", true);
                        goto specialAttackMoves;
                    }
                    else if (isWhisperSlice)
                    {
                        player.animator.SetBool("whisperSlice", true);
                        goto specialAttackMoves;
                    }

                    player.animator.Play("EmptyShortDW", 0, 0);  // Play short smear animation for dual wield
                }
                else if (mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw)
                {
                    if (isDontBlink)
                    {
                        player.animator.SetBool("dontBlink", true);
                        goto specialAttackMoves;
                    }
                    else if (isWhisperSlice)
                    {
                        player.animator.SetBool("whisperSlice", true);
                        goto specialAttackMoves;
                    }

                    player.animator.Play("EmptyThrustDW", 0, 0);  // Play thrust smear animation for dual wield
                }
                else
                {
                    player.animator.Play("EmptyMediumDW", 0, 0); // Play medium smear animation for dual wield
                }
            }
        }

    specialAttackMoves:

        //Force animator to update ASAP so new state will be active.
        IsAttacking = true;

        if (hand == MeleeHand.MainHand) mainHandAttackBlocked = true;
        else if (hand == MeleeHand.OffHand) offHandAttackBlocked = true;


        StartCoroutine(DelayAttack(weapon, hand, shieldBash, isBloodDrain, isCullTheMeek, isSheerCold, isDontBlink, isWhisperSlice));

        if (shieldBash)
        {
            //SoundEffectManager.Instance.PlaySoundEffect(soundEffect);  PLAY A SHIELD BASH SOUND EFFECT
        }
        else
        {
            player.animator.Update(0);
            // Melee attack sound effect
            if (mainHandWeapon.weaponDetails.isMeleeWeapon)
            {
                SoundEffect(mainHandWeapon.weaponDetails.weaponSwingSoundEffect);
            }

            // Weapon fired event for starting cooldown ui
            player.weaponFiredEvent.CallWeaponFiredEvent(mainHandWeapon, true);
        }
    }

    /// <summary>
    /// Dummy hit interactions
    /// </summary>
    private void DummyCheck(Collider2D collider, MeleeHand hand)
    {
        Health health = collider.GetComponent<Health>();

        // Damage produced by player
        int damageDone = player.isCursed
            ? (hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue)
            : Random.Range(hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue,
                hand == MeleeHand.MainHand ? player.currentMainHandMaxDamageValue : player.currentOffHandMaxDamageValue);

        Weapon weapon = new Weapon(Rarity.Basic);

        if (hand == MeleeHand.MainHand)
        {
            weapon = player.activeWeapon.GetCurrentMainHandWeapon();
        }
        else
        {
            weapon = player.activeWeapon.GetCurrentOffHandWeapon();
        }

        bool criticalHitHappened = CriticalHitHappened(null);

        // Calculate damage after critical hit check
        if (player.isStealthActive)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (weapon.weaponDetails.criticalHitDamageMultiplier +player.additionalCriticalMeleeDamageModifier + 
                player.additionalCriticalDamageOnCloakedPrecision)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * weapon.weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }


        bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits
        health.TakeDamage(damageDone, transform.position, health.transform.position, null, hand, bypass);
    }

    IEnumerator DelayAttack(Weapon weapon, MeleeHand hand,bool shieldBash = false, bool bloodDrain = false, bool isCullTheMeek = false, 
        bool isSheerCold = false, bool isDontBlink = false, bool isWhisperSlice = false)
    {
        bool isSpecialMove = shieldBash || bloodDrain || isCullTheMeek || isSheerCold || isDontBlink || isWhisperSlice;

        if (isSpecialMove) yield return new WaitForSeconds(1f);

        else yield return new WaitForSeconds(weapon.weaponDetails.weaponCooldownDuration * (1 - player.additionalAttackCoolDownModifier));

        weapon.onCooldown = false;

        if (hand == MeleeHand.MainHand) mainHandAttackBlocked = false;
        else if (hand == MeleeHand.OffHand) offHandAttackBlocked = false;
    }

    /// <summary>
    /// Play weapon shooting sound effect
    /// </summary>
    private void SoundEffect(SoundEffectSO soundEffect)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(soundEffect);
        }
    }
}
