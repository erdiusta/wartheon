using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackMainHand : MonoBehaviour
{
    public bool IsAttacking { get; set; }

    MeleeAttackEvent meleeAttackEvent;
    AttackShape attackShape;
    AnimationEventHelperMainHand rightHandAnimationEventHelper;
    Animator mainHandWeaponAnimator;
    CircleOrigin circleOrigin;
    BoxOrigin boxOrigin;
    TriangleOrigin triangleOrigin;
    Transform circleOriginTransform;
    Transform boxOriginTransform;
    Transform triangleOriginTransform;
    Player player;
    bool mainHandAttackBlocked;
    bool offHandAttackBlocked;

    // SpecialAttacks
    bool isBloodDrain;
    bool isCullTheMeek;
    bool isSheerCold;

    // Collision fields
    Collider2D[] _colliders = new Collider2D[10]; // Initial size
    ContactFilter2D _contactFilter = new ContactFilter2D().NoFilter(); // Default filter

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandAnimationEventHelper = GetComponent<AnimationEventHelperMainHand>();
        mainHandWeaponAnimator = transform.GetChild(0).GetComponent<Animator>();

        circleOrigin = GetComponentInChildren<CircleOrigin>();
        boxOrigin = GetComponentInChildren<BoxOrigin>();
        triangleOrigin = GetComponentInChildren<TriangleOrigin>();
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

    void Start()
    {
        circleOriginTransform = circleOrigin.transform;
        boxOriginTransform = boxOrigin.transform;
        triangleOriginTransform = triangleOrigin.transform;
    }

    private void MeleeAttackEvent_MeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        Attack(meleeAttackEventArgs.weapon, meleeAttackEventArgs.attackShape, meleeAttackEventArgs.meleeHand, meleeAttackEventArgs.isBloodDrain, 
            meleeAttackEventArgs.shieldBash, meleeAttackEventArgs.isCullTheMeek, meleeAttackEventArgs.isSheerCold);
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
            DetectHandHit(offHandWeapon, AttackShape.Swing, MeleeHand.OffHand, isBloodDrain, true);
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

        DetectHandHit(mainHandWeapon, attackShape, MeleeHand.MainHand, isBloodDrain, false, isSheerCold);

        // Optional: also include off-hand detection here if desired
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        if (offHandWeapon?.weaponDetails.isMeleeWeapon == true)
        {
            AttackShape offHandMeleeAttackType = offHandWeapon.weaponDetails.hasSwing ? AttackShape.Swing : AttackShape.Thrust;
            DetectHandHit(offHandWeapon, offHandMeleeAttackType, MeleeHand.OffHand, isBloodDrain);
        }
    }

    private void DetectHandHit(Weapon weapon, AttackShape attackType, MeleeHand hand, bool isBloodDrain, bool shieldBash = false, bool isSheerCold = false)
    {

        Transform originTransform = attackType switch
        {
            AttackShape.Swing => circleOriginTransform,
            AttackShape.Thrust => boxOriginTransform,
            AttackShape.Cone => triangleOriginTransform,
            _ => null
        };

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
                envHealth.TakeDamage(100, transform.position, collider.transform.position, false, null, hand); continue;
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
                if (shieldBash)
                {
                    CheckStunStatus(enemy, true);
                    enemyHealth.TakeDamage(20, transform.position, enemy.transform.position, false, null, hand, true);
                    continue;
                }

                if (!enemy.enemyDetails.isEnemyBoss)
                {
                    CheckSuddenDeathStatus(enemy, enemyHealth);
                }

                if (enemyHealth.suddenDeathHappened)
                {
                    enemyHealth.TakeDamage(enemyHealth.GetCurrentHealth() + 10, transform.position, enemy.transform.position, false, null, hand);
                    continue;
                }

                int inflictedDamage = CalculateDamageAmount(enemy, weapon, hand);

                bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits
                enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false, null, hand, bypass);

                if(!isSheerCold) SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponImpactSoundEffect);


                if (!enemy.enemyDetails.isEnemyBoss && enemy.health.GetCurrentHealth() > 0)
                {
                    CheckBleedingStatus(enemy);
                    CheckStunStatus(enemy);
                    CheckSlowStatus(enemy);
                    CheckAcidStatus(enemy);
                    CheckChillStatus(enemy);
                    CheckFrostStatus(enemy, isSheerCold);
                    CheckShatterStatus(enemy, ref inflictedDamage);
                    CheckRootStatus(enemy);
                    CheckBurnStatus(enemy);
                    CheckPoisonStatus(enemy);
                    CheckBlindStatus(enemy);
                    CheckFearStatus(enemy);
                }

                if (player.playerDetails.playerCharacterIndex == Character.Morven && player.isStealthActive)
                {
                    player.playerControl.Unstealth();
                }
            }
            else
            {
                // Enemy dodged
                enemyHealth.isDodging = true;
                enemy.healthEvent.CallDodgeEvent();
                enemyHealth.PostHitImmunity(true);
                enemyHealth.TakeDamage(0, transform.position, enemyHealth.transform.position, false, null, hand);
            }
        }
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    private int CalculateDamageAmount(Enemy enemy, Weapon weapon, MeleeHand hand)
    {
        int damageDone = player.isCursed
            ? (hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue)
            : Random.Range(
                hand == MeleeHand.MainHand ? player.currentMainHandMinDamageValue : player.currentOffHandMinDamageValue,
                hand == MeleeHand.MainHand ? player.currentMainHandMaxDamageValue : player.currentOffHandMaxDamageValue
            );

        float focusedAgrressionModifier = 0.05f;
        float punishersWillModifier = 0.05f;

        float totalDamageModifiers = 0f;

        // Focused Aggression Check
        if (player.isFocusedAggressionActive && player.health.GetCurrentHealth() / player.health.GetMaximumHealth() > 0.8f) totalDamageModifiers += focusedAgrressionModifier;

        // Punisher's Will Check
        if (player.isPunishersWillActive && enemy.health.GetCurrentHealth() / enemy.health.GetMaximumHealth() < 0.5f) totalDamageModifiers += punishersWillModifier;

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
        int elementalDamage = (int)(baseElemental * (1 + player.additionalElementalDamageModifier));
        int nonElementalDamage = damageDone - baseElemental;

        // ARMOR DEDUCTIONS
        float effectiveArmor = enemy.currentArmor;

        if (isBloodDrain) effectiveArmor *= 0.8f; // 20% Armor Penetration

        int inflictedNonElemental = (int)(nonElementalDamage * (1 - effectiveArmor));

        // Apply bonus dark damage if BloodDrain is active and weapon is Dark elemental
        if (isBloodDrain && weapon.weaponDetails.elementalBias == ElementalBias.Dark) elementalDamage = (int)(elementalDamage * 1.5f); // +50% dark damage

        // Apply bonus dark damage if CullTheMeek is active and weapon is Dark elemental
        if (isCullTheMeek && weapon.weaponDetails.elementalBias == ElementalBias.Dark)
        {
            int enemyMissingHealth = enemy.health.GetMaximumHealth() - enemy.health.GetCurrentHealth();
            elementalDamage = (int)(enemyMissingHealth * 0.3f + elementalDamage);
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
            float drainedHealth = totalInflictedDamage * 0.15f;
            player.health.AddHealth((int)drainedHealth);
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

            if (enemy != null && enemy.isBlind && player.playerDetails.playerCharacterIndex == Character.Morven)
            {
                randomCriticalDice = Mathf.Clamp01(randomCriticalDice ); 
            }

            if (player.isBlind)
            {
                criticalHitHappened = false;
            }
            else
            {
                criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance + 0.25f; // Morven - Cloaked Precision Passive
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
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.bleedingChance)
            {
                enemy.healthEvent.CallGetBleedingEvent();
                enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
            }
        }
    }

    /// <summary>
    /// Check burn status
    /// </summary>
    private void CheckBurnStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.canBurn)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.burnChance)
            {
                enemy.healthEvent.CallGetBurnEvent();
                enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
            }
        }
    }

    /// <summary>
    /// Check poison status
    /// </summary>
    private void CheckPoisonStatus(Enemy enemy, bool isActiveItem = false)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isPoisonous)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.poisonChance)
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
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasAcid && enemy.armorStatus != ArmorStatus.Acid && 
            enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.acidEfficiency)
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

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.chillChance || isBlizzard)
            {
                if (enemy.isChilled && !isFrozen)
                {
                    enemy.enemyAI.moveStatus |= MoveStatus.Frozen; // Second chill 
                    enemy.healthEvent.CallChillCuredEvent();
                }
                else if (!enemy.isChilled && !isFrozen)
                {
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

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.frostChance || isSheerCold)
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
                enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false, null, MeleeHand.MainHand, true);
                enemy.healthEvent.CallGetShatteredEvent();
                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
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

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.slowChance || isAbsoluteZero)
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
            enemy.healthEvent.CallGetStunEvent();
            return;
        }

        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasStunDamage && !isStunned || isGrapple)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.stunChance || isGrapple)
            {
                enemy.enemyAI.moveStatus |= MoveStatus.Stun;
                enemy.healthEvent.CallGetStunEvent();
            }
        }
    }

    /// <summary>
    /// Check root status
    /// </summary>
    private void CheckRootStatus(Enemy enemy)
    {
        EnemyAI enemyAI = enemy.GetComponent<EnemyAI>();
        bool isRooted = (enemyAI.moveStatus & MoveStatus.Root) != 0;

        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasRootDamage && !isRooted)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.rootChance)
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
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.blindChance + player.additionalBlindMakerModifier)
            {
                enemy.isBlind = true;
                enemy.healthEvent.CallGetBlindEvent();
            }
        }
    }

    /// <summary>
    /// Check fear status - Enemy
    /// </summary>
    private void CheckFearStatus(Enemy enemy)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasFearDamage && !enemy.isFeared)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.fearChance)
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
        ResetAnimations(isBloodDrain, isCullTheMeek, isSheerCold);

        player.animatePlayer.SetIdleAnimationParameters();
    }

    private void ResetAnimations(bool isBloodDrain, bool isCullTheMeek, bool isSheerCold)
    {
        if (isBloodDrain) player.animator.SetBool("blood", false);
        if (isCullTheMeek) player.animator.SetBool("cullTheMeek", false);
        if (isSheerCold)
        {
            player.animator.SetBool("sheerCold", false);
            mainHandWeaponAnimator.enabled = true;
        }
    }

    public void ResetIsShieldBashing()
    {
        IsAttacking = false;
        player.animator.SetBool("shieldBash", false);
        transform.GetChild(2).GetComponent<Animator>().enabled = true;

        player.isShieldBashing = false;
        player.animatePlayer.SetIdleAnimationParameters();
    }

    private void Attack(Weapon weapon, AttackShape attackShape, MeleeHand hand,bool isBloodDrain, bool shieldBash, bool isCullTheMeek, bool isSheerCold)
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

        if (!shieldBash && !isBloodDrain && !isCullTheMeek && !isSheerCold)
        {
            weapon.onCooldown = true;
            player.animatePlayer.SetAttackAnimationParameters();
        }

        // This means that's neither a dual wield nor a shield
        if (player.activeWeapon.GetCurrentOffHandWeapon() == null)
        {
            // If weapon is a spear, thrust motions should be enabled
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null && player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Spear) 
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
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
                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger || player.activeWeapon.GetCurrentMainHandWeapon().
                        weaponDetails.weaponClass == WeaponClass.Claw)
                    {
                        player.animator.Play("EmptyShort", 0, 0);  // Play short smear animation
                    }
                    else if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
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
        else if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
        {
            if (shieldBash)
            {
                player.animator.SetBool("shieldBash", true);

                goto specialAttackMoves;
            }

            if (player.activeWeapon.GetCurrentMainHandWeapon() != null && player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Spear)
            {
                player.animator.Play("EmptyThrustS", 0, 0);
            }
            else
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger || player.activeWeapon.GetCurrentMainHandWeapon().
                        weaponDetails.weaponClass == WeaponClass.Claw)
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
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger)
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

                    player.animator.Play("EmptyShortDW", 0, 0);  // Play short smear animation for dual wield
                }
                else if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Claw)
                {
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


        StartCoroutine(DelayAttack(weapon, hand, shieldBash, isBloodDrain, isCullTheMeek, isSheerCold));

        if (shieldBash)
        {
            //SoundEffectManager.Instance.PlaySoundEffect(soundEffect);  PLAY A SHIELD BASH SOUND EFFECT
        }
        else
        {
            player.animator.Update(0);
            // Melee attack sound effect
            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
            {
                SoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
            }

            // Weapon fired event for starting cooldown ui
            player.weaponFiredEvent.CallWeaponFiredEvent(player.activeWeapon.GetCurrentMainHandWeapon(), true);
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

        Weapon weapon = new Weapon();

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
        health.TakeDamage(damageDone, transform.position, health.transform.position, false, null, hand, bypass);
    }

    IEnumerator DelayAttack(Weapon weapon, MeleeHand hand,bool shieldBash = false, bool bloodDrain = false, bool isCullTheMeek = false, bool isSheerCold = false)
    {
        bool isSpecialMove = shieldBash || bloodDrain || isCullTheMeek || isSheerCold;

        if (isSpecialMove) yield return new WaitForSeconds(1f);

        else yield return new WaitForSeconds(weapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalMeleeAttackCoolDownModifier));

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
