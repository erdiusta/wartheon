using Mirror;
using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackMainHand : MonoBehaviour
{
    public bool IsAttacking { get; set; }

    public Transform mainHandCircleOriginTransform;
    public Transform mainHandBoxOriginTransform;
    public Transform mainHandTriangleOriginTransform;

    public Transform offHandCircleOriginTransform;
    public Transform offHandBoxOriginTransform;
    public Transform offHandTriangleOriginTransform;

    Player player;
    MeleeAttackEvent meleeAttackEvent;
    AttackShape attackShape;
    AnimationEventHelperMainHand rightHandAnimationEventHelper;
    Animator mainHandWeaponAnimator;

    bool mainHandAttackBlocked;
    bool offHandAttackBlocked;

    // SpecialAttacks
    [HideInInspector] public bool isBloodDrain;
    [HideInInspector] public bool isCullTheMeek;
    [HideInInspector] public bool isSheerCold;
    [HideInInspector] public bool isDontBlink;
    [HideInInspector] public bool isWhisperSlice;
    [HideInInspector] public bool isThrowingAxe;

    // Collision fields
    Collider2D[] _colliders = new Collider2D[10]; // Initial size
    ContactFilter2D _contactFilter = new ContactFilter2D().NoFilter(); // Default filter

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandAnimationEventHelper = GetComponent<AnimationEventHelperMainHand>();
    }

    private void OnEnable()
    {
        mainHandWeaponAnimator = transform.GetChild(0).GetComponent<Animator>();

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
            meleeAttackEventArgs.isBladeDash, meleeAttackEventArgs.isThrowingAxe);
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectCollidersForShieldBash()
    {
        if (!IsAttacking) return;

        // Optional: also include off-hand detection here if desired

        if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
        {
            DetectHandHit(AttackShape.Cone, MeleeHand.OffHand, isBloodDrain, true);
        }
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectColliders()
    {
        if (!IsAttacking) return;

        DetectHandHit(attackShape, MeleeHand.MainHand, isBloodDrain, false, isSheerCold, isDontBlink, isWhisperSlice, isThrowingAxe);

        // Optional: also include off-hand detection here if desired
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        if (offHandWeapon?.weaponDetails.isMeleeWeapon == true)
        {
            if (isDontBlink)
            {
                DetectHandHit(AttackShape.Thrust, MeleeHand.OffHand, isBloodDrain, false, isSheerCold, isDontBlink: true, isWhisperSlice: false, isThrowingAxe: false);
            }
            else
            {
                AttackShape offHandMeleeAttackType = offHandWeapon.weaponDetails.hasSwing ? AttackShape.Swing : AttackShape.Thrust;
                DetectHandHit(offHandMeleeAttackType, MeleeHand.OffHand, isBloodDrain, false, false, false, false, isThrowingAxe);
            }
        }
    }

    private void DetectHandHit(AttackShape attackType, MeleeHand hand, bool isBloodDrain, bool shieldBash = false, bool isSheerCold = false,
        bool isDontBlink = false, bool isWhisperSlice = false, bool isThrowingAxe = false)
    {
        Transform originTransform = null;

        if (isThrowingAxe) DropItem.droppedThrowingAxe = player.activeWeapon.GetCurrentOffHandWeapon();

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

        // Multiplayer - Server Authorative
        if (NetworkServer.active || NetworkClient.active)
        {
            DetectHandHit_MP(attackType, hand, isBloodDrain, shieldBash, isSheerCold, isDontBlink, isWhisperSlice, isThrowingAxe);
            return;
        }

        // Single Player
        DetectHandHit_SP(originTransform, attackType, hand, isBloodDrain, shieldBash, isSheerCold, isDontBlink, isWhisperSlice, isThrowingAxe);
    }

    private void DetectHandHit_MP(AttackShape attackType, MeleeHand hand, bool isBloodDrain, bool shieldBash = false, bool isSheerCold = false,
        bool isDontBlink = false, bool isWhisperSlice = false, bool isThrowingAxe = false)
    {
        if (player.NetAuth == null) return;

        player.NetAuth.CmdRequestMeleeHit(attackType, hand, isBloodDrain, shieldBash, isSheerCold, isDontBlink, isWhisperSlice, isThrowingAxe);
    }

    private void DetectHandHit_SP(Transform originTransform, AttackShape attackType, MeleeHand hand, bool isBloodDrain, bool shieldBash = false, bool isSheerCold = false,
        bool isDontBlink = false, bool isWhisperSlice = false, bool isThrowingAxe = false)
    {
        Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        Weapon hittingHandWeapon = hand switch
        {
            MeleeHand.MainHand => mainHandWeapon,
            MeleeHand.OffHand => offHandWeapon,
            _ => null
        };

        if (mainHandWeapon.weaponDetails == null)
        {
            Debug.LogError("Main weapon's weapon details reference is null!");
        }

        if (hand == MeleeHand.OffHand && offHandWeapon != null && offHandWeapon.weaponDetails.isMeleeWeapon && player.isAxeThrowActive && isThrowingAxe)
        {
            Vector3 weaponDirection;
            float weaponAngleDegrees, playerAngleDegrees;
            AimDirection playerAimDirection;
            AttackDirection playerAttackDirection;

            // Aim weapon input
            player.playerControl.AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

            // Trigger fire weapon event
            //SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, ProjectileKind.ThrowingAxe, default, 0, belongingEnemy: null);

            player.playerControl.DeactivateOffhandWeapon();

            // Update stat values
            player.UpdateDamageValues();
            player.UpdateArmorValues();
            player.UpdateAttackRatingAndCriticalValues();
            player.UpdateBlockAndDodgeValues();

            StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponOffHand);

            player.offHandSlotFilled = false;
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

            ReceiveMeleeDamage receiveMeleeDamage = collider.GetComponent<ReceiveMeleeDamage>();

            if (receiveMeleeDamage != null)
            {
                // Handle destructibles / environment
                if (collider.TryGetComponent(out Environment environment) &&
                    collider.TryGetComponent(out Health envHealth))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(hittingHandWeapon.weaponDetails.weaponImpactSoundEffect);

                    DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = collider.transform.position, hand = hand };
                    receiveMeleeDamage.TakeMeleeDamage(100, ctx);

                    continue;
                }

                if (!collider.TryGetComponent(out Health enemyHealth)) continue;

                if (collider.CompareTag(Settings.practiceDummy))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(hittingHandWeapon.weaponDetails.weaponImpactSoundEffect);
                    DummyCheck(hand, receiveMeleeDamage);
                    continue;
                }

                if (!collider.TryGetComponent(out Enemy enemy)) continue;

                IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(enemy.gameObject);

                // Calculate hit chance
                bool attackHits = player.currentAttackRatingValue * 100 - enemyCombatData.DeflectionValue * 100 > Random.Range(0, 100);

                if (attackHits)
                {
                    int inflictedDamage = 0;

                    Vector2 knockbackDir = Vector2.zero;
                    float knockbackForce = 0f;
                    float dealDamageMass = 0f;

                    float specialDamageModifier = 1f; // Default value

                    if (shieldBash)
                    {
                        CheckStunStatus(enemy, enemyCombatData, shieldBash: true, false);

                        if (mainHandWeapon != null)
                        {
                            specialDamageModifier = 0.5f;

                            inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(enemy, mainHandWeapon, MeleeHand.MainHand, specialDamageModifier);

                            if (enemy.health != null)
                            {
                                DamageContext ctxShieldBash = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = enemy.health.transform.position };
                                receiveMeleeDamage.TakeMeleeDamage(inflictedDamage, ctxShieldBash);
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

                    if (!enemyCombatData.Isboss)
                    {
                        CheckSuddenDeathStatus(enemy, enemyHealth, enemyCombatData);
                    }

                    if (enemyHealth.suddenDeathHappened)
                    {
                        DamageContext ctxSuddenDeath = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = enemy.transform.position, hand = hand };
                        receiveMeleeDamage.TakeMeleeDamage(enemyHealth.GetCurrentHealth() + 10, ctxSuddenDeath);

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
                    inflictedDamage = CalculateDamageAmount(enemy, hittingHandWeapon, hand, specialDamageModifier);

                    bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits

                    DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = enemy.transform.position, hand = hand, bypassImmunity = bypass };
                    receiveMeleeDamage.TakeMeleeDamage(inflictedDamage, ctx);

                    if (player.isTriadExecutionActive) player.triadExecutionCounter++;

                    if (!isSheerCold) SoundEffectManager.Instance.PlaySoundEffect(hittingHandWeapon.weaponDetails.weaponImpactSoundEffect);

                    if (!enemyCombatData.Isboss && enemy.health.GetCurrentHealth() > 0)
                    {
                        CheckBleedingStatus(enemy, enemyCombatData);
                        CheckStunStatus(enemy, enemyCombatData, false, false);
                        CheckSlowStatus(enemy, enemyCombatData, false);
                        CheckChillStatus(enemy, enemyCombatData, false);
                        CheckFrostStatus(enemy, enemyCombatData, isSheerCold);
                        CheckShatterStatus(enemy, enemyCombatData, ref inflictedDamage);
                        CheckStaticStatus(enemy, enemyCombatData, false, player.isConductiveTouchActive);
                        CheckParalyzeStatus(enemy, enemyCombatData);
                        CheckRootStatus(enemy, enemyCombatData, false);
                        CheckWarmStatus(enemy, enemyCombatData, false);
                        CheckBurnStatus(enemy, enemyCombatData);
                        CheckPoisonStatus(enemy, enemyCombatData, false);
                        CheckBlindStatus(enemy, enemyCombatData, false);
                        CheckFearStatus(enemy, enemyCombatData, false);
                    }

                    if (player.playerDetails.playerCharacterIndex == Character.Morven && player.isStealthActive)
                    {
                        player.playerSkillController.Unstealth();
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

                    DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = enemyHealth.transform.position, hand = hand };
                    receiveMeleeDamage.TakeMeleeDamage(0, ctx);

                    // Knockback
                    Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                    float knockbackForce = 3f; // Normal hit force
                    float dealDamageMass = 1f;

                    enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                }
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

        float enemyCurrentHealth = enemy.health.GetCurrentHealth();
        float enemyMaximumHealth = enemy.health.GetMaximumHealth();

        if(player.playerDetails.playerCharacterIndex == Character.Karnag)
        {
            int missingHealthDamage = (int)((playerMaximumHealth - playerCurrentHealth) * 0.2f);
            damageDone += missingHealthDamage;
        }

        float focusedAgrressionModifier = 0.05f;
        float punishersWillModifier = 0.05f;

        float totalDamageModifiers = 0f;

        // Focused Aggression Check
        if (player.isFocusedAggressionActive && playerCurrentHealth / playerMaximumHealth > 0.8f) totalDamageModifiers += focusedAgrressionModifier;

        // Punisher's Will Check
        if (player.isPunishersWillActive && enemyCurrentHealth / enemyMaximumHealth < 0.5f) totalDamageModifiers += punishersWillModifier;

        damageDone = (int)(damageDone * (1 + totalDamageModifiers)); // Add additional damage modifiers

        bool criticalHitHappened = CriticalHitHappened(enemy);

        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();

            if(!NetworkServer.active && !NetworkClient.active) SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        }

        float critMultiplier = 0f;

        if (hand == MeleeHand.MainHand) critMultiplier = player.currentMainHandCriticalHitDamage;
        else if(hand == MeleeHand.OffHand) critMultiplier = player.currentOffHandCriticalHitDamage;

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

            effectiveArmor *= (1 - armorPenetrationModifier - player.currentArmorPenetrationValue);
        }

        if (isDontBlink) effectiveArmor *= 0.7f - player.currentArmorPenetrationValue; // 30% Armor Penetration

        if (player.isShatterCryActive) effectiveArmor *= 0.8f - player.currentArmorPenetrationValue;

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

        inflictedElemental = (int)(elementalDamage * (1 - enemy.enemyDetails.magicResistance));

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

            float drainedHealth = totalInflictedDamage * (drainedHealthPercentage + player.currentLifeStealValue);
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
            float drainedHealth = totalInflictedDamage * (feastOfWarDrainPercentage + player.currentLifeStealValue);
            player.health.AddHealth((int)drainedHealth);
        }
        else
        {
            // CHECK IF PLAYER HAS LIFE DRAIN
            float drainedHealth = totalInflictedDamage * player.currentLifeStealValue;
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
    private void CheckSuddenDeathStatus(Enemy enemy, Health enemyHealth, IEnemyCombatData enemyCombatData)
    {
        if (enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.suddenKillChance)
            {
                enemyHealth.suddenDeathHappened = true;
                DestroyUtility.Destroy(enemy.gameObject, playerDied: false, enemyHealth.LastDamageDealerNetId);
                enemy.healthEvent.CallGetDeathEvent();

                if (!NetworkServer.active && !NetworkClient.active)
                {
                    SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
                }
            }
        }
    }

    /// <summary>
    /// Check bleeding status
    /// </summary>
    private void CheckBleedingStatus(Enemy enemy, IEnemyCombatData enemyCombatData)
    {
        if (enemyCombatData.IsImmuneToBleeding) return;

        // Check get bleeding
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.bleedingChance + player.additionalStatusEffectInflictModifier +
            player.additionalBleedChance)
        {
            enemy.healthEvent.CallGetBleedingEvent();
            enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
            enemy.statusEffectAnimators.bleedAnimator.SetTrigger(Settings.activateVFX);
        }
    }

    /// <summary>
    /// Check warm status
    /// </summary>
    public void CheckWarmStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isFlameLotus)
    {
        bool isBurned = (enemy.healthStatus & HealthStatus.Burned) != 0;

        if (enemy.health.currentHealth > 0 || isFlameLotus)
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
    private void CheckBurnStatus(Enemy enemy, IEnemyCombatData enemyCombatData)
    {
        if (enemyCombatData.IsImmuneToBurn) return;

        // Check get bleeding
        float randomDice = Random.Range(0f, 1f);
        if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.burnChance + player.additionalStatusEffectInflictModifier +
            player.additionalBurnChance)
        {
            enemy.healthEvent.CallGetBurnEvent();
            enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
            enemy.statusEffectAnimators.burnAnimator.SetTrigger(Settings.activateVFX);
        }
    }

    /// <summary>
    /// Check poison status
    /// </summary>
    public void CheckPoisonStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isVenomousIvy)
    {
        if (enemyCombatData.IsImmuneToPoison) return;

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
        if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.poisonChance + player.additionalStatusEffectInflictModifier +
            player.additionalPoisonChance || isVenomousIvy)
        {
            enemy.healthEvent.CallGetPoisonedEvent();
            enemy.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
            enemy.statusEffectAnimators.poisonAnimator.SetTrigger(Settings.activateVFX);
        }
    }

    /// <summary>
    /// Check chill status
    /// </summary>
    public void CheckChillStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isBlizzard)
    {
        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        if (enemy.health.currentHealth > 0 || isBlizzard)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.chillChance + player.additionalStatusEffectInflictModifier || isBlizzard)
            {
                if (enemy.isChilled && !isFrozen && !isBlizzard)
                {
                    enemy.moveStatus |= MoveStatus.Frozen; // Second chill 
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
    private void CheckFrostStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isSheerCold)
    {
        if (enemyCombatData.IsImmuneToFrost) return;

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isFrozen = (enemy.moveStatus & MoveStatus.Frozen) != 0;

        if (!isFrozen || isSheerCold)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.frostChance + player.additionalStatusEffectInflictModifier +
            player.additionalFreezeChance || isSheerCold)
            {
                enemy.moveStatus |= MoveStatus.Frozen;
                enemy.statusEffectAnimators.frostAnimator.SetTrigger(Settings.activateVFX);
            }
        }
    }

    /// <summary>
    /// Check shatter status
    /// </summary>
    private void CheckShatterStatus(Enemy enemy, IEnemyCombatData enemyCombatData, ref int inflictedDamage)
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
                DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = enemy.transform.position, hand = MeleeHand.MainHand, bypassImmunity = true };
                ReceiveMeleeDamage receiveMeleeDamage = enemyHealth.GetComponent<ReceiveMeleeDamage>();
                receiveMeleeDamage.TakeMeleeDamage(inflictedDamage, ctx);

                enemy.healthEvent.CallGetShatteredEvent();

                if (!NetworkServer.active && !NetworkClient.active) SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
            }
        }
    }

    /// <summary>
    /// Check static status
    /// </summary>
    public void CheckStaticStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isNymarasWindveil, bool isConductiveTouch)
    {
        bool isParalyzed = (enemy.moveStatus & MoveStatus.Paralyze) != 0;

        if (enemy.health.currentHealth > 0 || isNymarasWindveil)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.staticChance + player.additionalStatusEffectInflictModifier 
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
    private void CheckParalyzeStatus(Enemy enemy, IEnemyCombatData enemyCombatData)
    {
        if (enemyCombatData.IsImmuneToParalyze) return;

        bool isParalyzed = (enemy.moveStatus & MoveStatus.Paralyze) != 0;

        if (!isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.paralyzeChance + player.additionalStatusEffectInflictModifier +
                player.additionalParalyzeChance)
            {
                enemy.moveStatus |= MoveStatus.Paralyze;
                enemy.statusEffectAnimators.paralyzeAnimator.SetTrigger(Settings.activateVFX);
            }
        }
    }

    /// <summary>
    /// Check slow status
    /// </summary>
    public void CheckSlowStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isAbsoluteZero)
    {
        if (enemyCombatData.IsImmuneToSlow) return;

        if (!enemy.isSlowed && isAbsoluteZero)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.slowChance + player.additionalStatusEffectInflictModifier +
                player.additionalSlowChance || isAbsoluteZero)
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
    public void CheckStunStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool shieldBash, bool isGrapple)
    {
        if (enemyCombatData.IsImmuneToStun) return;

        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isStunned = (enemy.moveStatus & MoveStatus.Stun) != 0;

        if (shieldBash)
        {
            enemy.moveStatus |= MoveStatus.Stun;

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
            enemy.statusEffectAnimators.stunAnimator.SetTrigger(Settings.activateVFX);
            return;
        }

        if (!isStunned || isGrapple)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.stunChance + player.additionalStatusEffectInflictModifier +
                player.additionalStunChance || isGrapple)
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
    public void CheckRootStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isVenomousIvy)
    {
        if (enemyCombatData.IsImmuneToRoot) return;

        bool isRooted = (enemy.moveStatus & MoveStatus.Root) != 0;

        if (!isRooted || isVenomousIvy)
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
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.rootChance + player.additionalStatusEffectInflictModifier +
                player.additionalRootChance || isVenomousIvy)
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
    public void CheckBlindStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool umbralMist)
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
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.blindChance + player.additionalStatusEffectInflictModifier +
            player.additionalBlindChance + player.additionalStatusEffectInflictModifier)
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
    public void CheckFearStatus(Enemy enemy, IEnemyCombatData enemyCombatData, bool isShatterCry)
    {
        if (enemyCombatData.IsImmuneToFear) return;

        if (isShatterCry && !enemy.isFeared)
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
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.fearChance + player.additionalStatusEffectInflictModifier +
            player.additionalFearChance || isShatterCry)
            {
                enemy.statusEffectAnimators.fearAnimator.SetTrigger(Settings.activateVFX);
                enemy.isFeared = true;
                enemy.healthEvent.CallGetFearEvent();
            }
        }
    }

    public void ResetIsAttackingRightHand()
    {
        IsAttacking = false;

        player.playerControl.meleeAttackTypeMainHand = AttackShape.None;
        ResetAnimations(isBloodDrain, isCullTheMeek, isSheerCold, isWhisperSlice);

        player.animatePlayer.SetIdleAnimationParameters();

        // Sync
        player.animSync?.UpdateLocalAnimationState(player.playerControl.wasMoving, player.playerControl.aimDirection, player.playerControl.attackDirection);
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
        bool isDontBlink, bool isWhisperSlice, bool isThrowingAxe)
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
        this.isThrowingAxe = isThrowingAxe;

        if (!shieldBash && !isBloodDrain && !isCullTheMeek && !isSheerCold && !isDontBlink)
        {
            weapon.onCooldown = true;

            player.animatePlayer.SetAttackAnimationParameters();
            player.animSync?.CmdPlayAttack(player.LastAim, player.LastAttackdir);
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

        // Sync
        player.animSync?.UpdateLocalAnimationState(player.playerControl.wasMoving, player.playerControl.aimDirection, player.playerControl.attackDirection);

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
    private void DummyCheck(MeleeHand hand, ReceiveMeleeDamage receiveMeleeDamage)
    {
        // Damage produced by player
        int damageDone = player.isCursed ? (hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue)
            : Random.Range(hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue,
                hand == MeleeHand.MainHand ? player.currentMainHandMaxDamageValue : player.currentOffHandMaxDamageValue);

        Weapon weapon = new Weapon(Rarity.Basic);

        if (hand == MeleeHand.MainHand) weapon = player.activeWeapon.GetCurrentMainHandWeapon();
        else weapon = player.activeWeapon.GetCurrentOffHandWeapon();

        bool criticalHitHappened = CriticalHitHappened(null);

        // Calculate damage after critical hit check
        if (player.isStealthActive)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (weapon.weaponDetails.criticalHitDamageMultiplier  +player.additionalCriticalMeleeDamageModifier + 
                player.additionalCriticalDamageOnCloakedPrecision)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * weapon.weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits

        DamageContext ctx = new DamageContext { source = DamageSourceType.Melee, dealerPosition = transform.position, receiverPosition = receiveMeleeDamage.transform.position, hand = hand, bypassImmunity = bypass };
        receiveMeleeDamage.TakeMeleeDamage(damageDone, ctx);
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
