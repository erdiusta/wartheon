using System;
using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackMainHand : MonoBehaviour
{
    public bool IsAttacking { get; set; }

    MeleeAttackEvent meleeAttackEvent;
    AnimationEventHelperMainHand rightHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    BoxOrigin boxOrigin;
    Transform circleOriginTransform;
    Transform boxOriginTransform;
    Health enemyHealth;
    Player player;
    bool rightHandAttackBlocked;
    bool isBloodDrain;

    // Collision fields
    Collider2D[] _colliders = new Collider2D[10]; // Initial size
    ContactFilter2D _contactFilter = new ContactFilter2D().NoFilter(); // Default filter

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandAnimationEventHelper = GetComponent<AnimationEventHelperMainHand>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        boxOrigin = GetComponentInChildren<BoxOrigin>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnAttack += MeleeAttackEvent_MainHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationMainHandEventTriggered.AddListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackMainHandPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnAttack -= MeleeAttackEvent_MainHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationMainHandEventTriggered.RemoveListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackMainHandPerformed.RemoveListener(DetectColliders);
    }

    void Start()
    {
        circleOriginTransform = circleOrigin.transform;
        boxOriginTransform = boxOrigin.transform;
    }

    private void MeleeAttackEvent_MainHandMeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        AttackAtMainHand(meleeAttackEventArgs.weapon, meleeAttackEventArgs.meleeAttackType, meleeAttackEventArgs.isBloodDrain);
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

        MeleeAttackType mainHandMeleeAttackType = mainHandWeapon.weaponDetails.hasSwing ? MeleeAttackType.Swing : MeleeAttackType.Thrust;

        DetectHandHit(mainHandWeapon, mainHandMeleeAttackType, MeleeHand.MainHand, isBloodDrain);

        // Optional: also include off-hand detection here if desired
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        if (offHandWeapon?.weaponDetails.isMeleeWeapon == true)
        {
            MeleeAttackType offHandMeleeAttackType = offHandWeapon.weaponDetails.hasSwing ? MeleeAttackType.Swing : MeleeAttackType.Thrust;
            DetectHandHit(offHandWeapon, offHandMeleeAttackType, MeleeHand.OffHand, isBloodDrain);
        }
    }

    private void DetectHandHit(Weapon weapon, MeleeAttackType attackType, MeleeHand hand, bool isBloodDrain)
    {
        Transform originTransform = attackType == MeleeAttackType.Swing ? circleOriginTransform : boxOriginTransform;

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
                envHealth.TakeDamage(100, transform.position, collider.transform.position, false, null, hand); continue;
            }

            if (!collider.TryGetComponent(out Health enemyHealth)) continue;

            if (collider.CompareTag(Settings.practiceDummy))
            {
                DummyCheck(collider, hand); 
                continue;
            }

            if (!collider.TryGetComponent(out Enemy enemy)) continue;

            // Calculate hit chance
            bool attackHits = player.currentWeaponHandlingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100);

            if (attackHits)
            {
                if (!enemy.enemyDetails.isEnemyBoss)
                {
                    CheckSuddenDeathStatus(enemy, enemyHealth);
                    CheckShatterStatus(enemy, enemyHealth);
                }

                if (enemyHealth.suddenDeathHappened)
                {
                    enemyHealth.TakeDamage(enemyHealth.GetCurrentHealth() + 10, transform.position, enemy.transform.position, false, null, hand);
                    continue;
                }

                int inflictedDamage = isBloodDrain
                    ? Mathf.Max((int)(enemyHealth.currentHealth * (0.2f + player.bloodDrainSkillAdditionalDamagePercentageModifier)),
                                CalculateDamageAmount(enemy, weapon, hand)): CalculateDamageAmount(enemy, weapon, hand);

                bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits
                enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false, null, hand, bypass);

                SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponImpactSoundEffect);

                if (!enemy.enemyDetails.isEnemyBoss)
                {
                    CheckAcidStatus(enemy);
                    CheckFrostStatus(enemy);
                    CheckStunStatus(enemy);
                    CheckBurnStatus(enemy);
                    CheckPoisonStatus(enemy);
                    CheckBlindStatus(enemy);
                }

                if (player.playerDetails.playerCharacterIndex == Character.Erebus && player.onStealth)
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

        bool criticalHitHappened = CriticalHitHappened();

        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        }

        float critMultiplier = weapon.weaponDetails.criticalHitDamageMultiplier + player.additionalCriticalMeleeDamageModifier;

        if (player.onStealth) critMultiplier += player.additionalCriticalDamageOnStealth;

        damageDone = criticalHitHappened ? (int)(damageDone * critMultiplier) : damageDone;

        int baseElemental = (int)(weapon.weaponDetails.elementalForgeRate * damageDone);
        int elementalDamage = (int)(baseElemental * (1 + player.additionalElementalDamageModifier));
        int nonElementalDamage = damageDone - baseElemental;

        int inflictedNonElemental = (int)(nonElementalDamage * (1 - enemy.currentPhysicalResistance));
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

        return inflictedNonElemental + inflictedElemental;
    }

    /// <summary>
    /// Critical hit check
    /// </summary>
    /// <returns></returns>
    private bool CriticalHitHappened()
    {
        bool criticalHitHappened = false;

        if (player.onStealth)
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
            else
            {
                criticalHitHappened = randomCriticalDice < player.currentMainHandCriticalHitChance;
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
            this.enemyHealth = enemyHealth;

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
    /// Check shatter status
    /// </summary>
    private void CheckShatterStatus(Enemy enemy, Health enemyHealth)
    {
        this.enemyHealth = enemyHealth;

        EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

        if (enemyMovementAI.moveStatus == MoveStatus.Frozen && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < 0.25f)
            {
                enemyHealth.suddenDeathHappened = true;
                enemyHealth.TakeDamage(5000, transform.position, enemy.transform.position, false, null, MeleeHand.MainHand);
                enemy.destroyedEvent.CallDestroyedEvent(false);
                enemy.healthEvent.CallGetShatteredEvent();
                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
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
                enemy.currentPhysicalResistance = (float)Math.Round(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.acidEfficiency * enemy.currentPhysicalResistance, 2);
                enemy.healthEvent.CallGetAcidEvent();
            }
        }
    }

    /// <summary>
    /// Check frost status
    /// </summary>
    private void CheckFrostStatus(Enemy enemy)
    {
        EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasFrostDamage && enemy.health.currentHealth > 0 && enemyMovementAI.moveStatus != MoveStatus.Frozen)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.frostChance)
            {
                enemy.enemyAI.moveStatus = MoveStatus.Frozen;
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    private void CheckStunStatus(Enemy enemy)
    {
        EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasStunDamage && enemyMovementAI.moveStatus != MoveStatus.Stun 
            && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.stunChance)
            {
                enemy.enemyAI.moveStatus = MoveStatus.Stun;
                enemy.healthEvent.CallGetStunEvent();
            }
        }
    }

    /// <summary>
    /// Check blind status
    /// </summary>
    private void CheckBlindStatus(Enemy enemy)
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasBlindDamage)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.blindChance + player.additionalBlindMakerModifier)
            {
                enemy.healthEvent.CallGetBlindEvent();
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
        player.playerControl.meleeAttackTypeMainHand = MeleeAttackType.None;

        player.animatePlayer.SetIdleAnimationParameters();
    }

    private void AttackAtMainHand(Weapon weapon, MeleeAttackType meleeAttackType, bool isBloodDrain)
    {
        if (rightHandAttackBlocked) return;

        // Ranged fire animation (Bow or staff)
        if (meleeAttackType == MeleeAttackType.None) // Ranged Attack
        {
            Animator weaponAnimator = transform.GetChild(0).GetComponent<Animator>();
            weaponAnimator.SetTrigger(Settings.rangedWeaponAttack);
            return;
        }

        this.isBloodDrain = isBloodDrain;

        weapon.onCooldown = true;

        player.animatePlayer.SetAttackAnimationParameters();

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
                        player.animator.Play("EmptyLong", 0, 0);  // Play long smear animation
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

        //Force animator to update ASAP so new state will be active.
        player.animator.Update(0);

        IsAttacking = true;
        rightHandAttackBlocked = true;
        StartCoroutine(DelayAttackRightHand(weapon));

        // Melee attack sound effect
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
        {
            SoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
        }

        // Weapon fired event for starting cooldown ui
        player.weaponFiredEvent.CallWeaponFiredEvent(player.activeWeapon.GetCurrentMainHandWeapon(), true);
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

        bool criticalHitHappened = CriticalHitHappened();

        // Calculate damage after critical hit check
        if (player.onStealth)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (weapon.weaponDetails.criticalHitDamageMultiplier +player.additionalCriticalMeleeDamageModifier + 
                player.additionalCriticalDamageOnStealth)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * weapon.weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }


        bool bypass = hand == MeleeHand.OffHand; // only bypass for off-hand hits
        health.TakeDamage(damageDone, transform.position, health.transform.position, false, null, hand, bypass);
    }

    IEnumerator DelayAttackRightHand(Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalMeleeAttackCoolDownModifier));

        weapon.onCooldown = false;
        rightHandAttackBlocked = false;
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
