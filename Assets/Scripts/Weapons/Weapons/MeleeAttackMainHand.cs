using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using static UnityEngine.EventSystems.EventTrigger;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackMainHand : MonoBehaviour
{
    public bool IsAttacking { get; set; }

    [HideInInspector] public Coroutine playerAttackMotionRoutine;

    MeleeAttackEvent meleeAttackEvent;
    //Animator rightHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperMainHand rightHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    BoxOrigin boxOrigin;
    Transform circleOriginTransform;
    Transform boxOriginTransform;
    Health enemyHealth;
    Player player;
    bool rightHandAttackBlocked;
    bool isBloodDrain;

    // DEBUG
    int animationCounter = 0;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        //rightHandMeleeAnimator = transform.GetChild(0).GetComponent<Animator>();
        rightHandAnimationEventHelper = GetComponent<AnimationEventHelperMainHand>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        boxOrigin = GetComponentInChildren<BoxOrigin>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack += MeleeAttackEvent_MainHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationMainHandEventTriggered.AddListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackMainHandPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack -= MeleeAttackEvent_MainHandMeleeAttack;
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

        switch (player.playerControl.meleeAttackTypeMainHand)
        {
            case MeleeAttackType.None:
                break;
            case MeleeAttackType.Swing:
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(circleOriginTransform.position, circleOrigin.circleRadius))
                {
                    if (collider.GetComponent<Environment>() != null)
                    {
                        collider.GetComponent<Health>().TakeDamage(100, transform.position, collider.transform.position, false);
                    }

                    if (collider.GetType() == typeof(PolygonCollider2D))
                    {
                        // Don't hit yourself if player is also in the collider list
                        if (collider.tag == Settings.playerTag) continue;

                        if (collider.tag == Settings.decoyTag) continue;

                        if (collider.tag == Settings.chestItemTag) continue;

                        if (enemyHealth = collider.GetComponent<Health>())
                        {
                            if(collider.tag == "PracticeDummy")
                            {
                                DummyCheck(collider);

                                return; // Exit here to prevent enemy checks
                            }

                            Enemy enemy = collider.GetComponent<Enemy>();

                            // Check if hit is successful or dodged by enemy
                            if (player.currentWeaponHandlingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100))
                            {
                                if (!enemy.enemyDetails.isEnemyBoss)
                                {
                                    CheckSuddenDeathStatus(enemy);
                                    CheckShatterStatus(enemy);
                                }

                                if (enemyHealth.suddenDeathHappened)
                                {
                                    enemyHealth.TakeDamage(enemyHealth.GetCurrentHealth() + 10, transform.position, enemy.transform.position, false);
                                    return;
                                }

                                int inflictedDamage = CalculateDamageAmount(enemy);

                                if (isBloodDrain)
                                {
                                    int inflictedProportionalDamage = (int)(enemy.enemyDetails.enemyHealthDetailsArray[GameManager.Instance.GetCurrentDungeonLevel().levelNumber - 1]
                                        .enemyHealthAmount * 0.2f);
                                    if (inflictedDamage > inflictedProportionalDamage)
                                    {
                                        enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                                    }
                                    else
                                    {
                                        enemyHealth.TakeDamage(inflictedProportionalDamage, transform.position, enemy.transform.position, false);
                                    }
                                }
                                else
                                {
                                    enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                                }

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                if (!enemy.enemyDetails.isEnemyBoss)
                                {
                                    CheckAcidStatus(enemy);
                                    CheckFrostStatus(enemy);
                                    CheckStunStatus(enemy);
                                    CheckPoisonStatus(enemy);
                                    CheckBlindStatus(enemy);
                                }

                                if (player.playerDetails.playerCharacterIndex == Character.Erebus && player.onStealth)
                                {
                                    player.playerControl.Unstealth();
                                }

                                //if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                                //{
                                //    enemy.enemyAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                                //}
                            }
                            else
                            {
                                enemy.health.isDodging = true;
                                enemy.healthEvent.CallDodgeEvent();
                                enemy.health.PostHitImmunity(true);
                                enemy.health.TakeDamage(0, transform.position, enemy.health.transform.position, false);
                            }
                        }
                    }
                }
                break;
            case MeleeAttackType.Thrust:
                // Fill here
                Vector2 boxSize = new Vector2(boxOrigin.boxLength, boxOrigin.boxHeight);
                foreach (Collider2D collider in Physics2D.OverlapBoxAll(boxOriginTransform.position, boxSize, 0))
                {
                    if (collider.GetComponent<Environment>() != null)
                    {
                        collider.GetComponent<Health>().TakeDamage(100, transform.position, collider.transform.position, false);
                    }

                    if (collider.GetType() == typeof(PolygonCollider2D))
                    {
                        // Don't hit yourself if player is also in the collider list
                        if (collider.tag == Settings.playerTag) continue;
                        if (collider.tag == Settings.decoyTag) continue;
                        if (collider.tag == Settings.chestItemTag) continue;

                        if (enemyHealth = collider.GetComponent<Health>())
                        {
                            if (collider.tag == "PracticeDummy")
                            {
                                DummyCheck(collider);

                                return; // Exit here to prevent enemy checks
                            }

                            Enemy enemy = collider.GetComponent<Enemy>();

                            // Check if hit is successful or dodged by enemy
                            if (player.currentWeaponHandlingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100))
                            {
                                if (!enemy.enemyDetails.isEnemyBoss)
                                {
                                    CheckSuddenDeathStatus(enemy);
                                    CheckShatterStatus(enemy);
                                }

                                if (enemyHealth.suddenDeathHappened)
                                {
                                    enemyHealth.TakeDamage(enemyHealth.GetCurrentHealth() + 10, transform.position, enemy.transform.position, false);
                                    return;
                                }

                                if (isBloodDrain)
                                {
                                    // BLOOD DRAIN SKILL FOR EREBUS
                                    int inflictedProportionalDamage = (int)(enemyHealth.currentHealth * (0.2f + player.bloodDrainSkillAdditionalDamagePercentageModifier));
                                    enemyHealth.TakeDamage(inflictedProportionalDamage, transform.position, enemy.transform.position, false);
                                }
                                else
                                {
                                    int inflictedDamage = CalculateDamageAmount(enemy);
                                    enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                                }

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                if (!enemy.enemyDetails.isEnemyBoss)
                                {
                                    CheckAcidStatus(enemy);
                                    CheckFrostStatus(enemy);
                                    CheckStunStatus(enemy);
                                    CheckPoisonStatus(enemy);
                                    CheckBlindStatus(enemy);
                                }

                                if (player.playerDetails.playerCharacterIndex == Character.Erebus && player.onStealth)
                                {
                                    player.playerControl.Unstealth();
                                }

                                //if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                                //{
                                //    enemy.enemyAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                                //}
                            }
                            else
                            {
                                enemy.health.isDodging = true;
                                enemy.healthEvent.CallDodgeEvent();
                                enemy.health.PostHitImmunity(true);
                                enemy.health.TakeDamage(0, transform.position, enemy.health.transform.position, false);
                            }
                        }
                    }
                }
                    break;
            default:
                break;
        }
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    private int CalculateDamageAmount(Enemy enemy)
    {
        // Damage produced by player
        int damageDone = player.isCursed ? player.currentMainHandMinDamageValue : Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);

        bool criticalHitHappened = CriticalHitHappened();

        // Critical hit check
        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        }

        // Calculate damage after critical hit check
        if (player.onStealth)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier + player.additionalCriticalDamageOnStealth)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        // Segregate elemental and non-elemental damage
        int elementalDamage = (int)(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.elementalForgeRate * damageDone);

        int additionalElementalDamage = (int)(elementalDamage * player.additionalElementalDamageModifier);
        elementalDamage += additionalElementalDamage;

        int nonElementalDamage = damageDone - elementalDamage + additionalElementalDamage;

        int inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - enemy.currentPhysicalResistance));

        int inflictedElementalDamage = 0;
        // Calculate inflicted elemental damage
        switch (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.elementalBias)
        {
            case ElementalBias.None:
                break;
            case ElementalBias.Fire:
                inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.fireResistance));
                break;
            case ElementalBias.Water:
                inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.waterResistance));
                break;
            case ElementalBias.Earth:
                inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.earthResistance));
                break;
            case ElementalBias.Air:
                inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.airResistance));
                break;
            case ElementalBias.Dark:
                inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.darkResistance));
                break;
            case ElementalBias.Light:
                inflictedElementalDamage = (int)(elementalDamage * (1 - enemy.enemyDetails.lightResistance));
                break;
            default:
                break;
        }

        return inflictedElementalDamage + inflictedNonElementalDamage;
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

            float criticalHitChanceModifier = 0f;

            if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
            {
                if ((player.activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Dagger &&
                    player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Dagger) ||
                    (player.activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Claw &&
                    player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Claw) &&
                    player.selectedPassiveItem.GetCurrentBackPassiveItem()?.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    criticalHitChanceModifier += 0.1f;
                }
                else
                {
                    criticalHitChanceModifier = 0f;
                }
            }
            else
            {
                criticalHitChanceModifier = 0f;
            }

            if (player.isBlind)
            {
                criticalHitHappened = false;
            }
            else
            {
                criticalHitHappened = randomCriticalDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitChance + criticalHitChanceModifier ?
                    true : false;
            }
        }

        return criticalHitHappened;
    }

    /// <summary>
    /// Check sudden death status
    /// </summary>
    private void CheckSuddenDeathStatus(Enemy enemy)
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
    /// Check shatter status
    /// </summary>
    private void CheckShatterStatus(Enemy enemy)
    {
        EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

        if (enemyMovementAI.moveStatus == MoveStatus.Frozen && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < 0.25f)
            {
                enemyHealth.suddenDeathHappened = true;
                enemyHealth.TakeDamage(5000, transform.position, enemy.transform.position, false);
                enemy.destroyedEvent.CallDestroyedEvent(false);
                enemy.healthEvent.CallGetShatteredEvent();
                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
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
                enemy.healthStatus = HealthStatus.Poisoned;
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
        if (meleeAttackType == MeleeAttackType.None)
        {
            Animator weaponAnimator = transform.GetChild(0).GetComponent<Animator>();

            player.animatePlayer.SetAttackAnimationParameters();
            weaponAnimator.SetTrigger(Settings.rangedWeaponAttack);

            //weaponAnimator.Play("AttackAtRightHand", 0, 0);
            //weaponAnimator.Update(0);

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
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger || player.activeWeapon.GetCurrentMainHandWeapon().
                    weaponDetails.weaponClass == WeaponClass.Claw)
                {
                    player.animator.Play("EmptyShortDW", 0, 0);  // Play short smear animation for dual wield
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
    private void DummyCheck(Collider2D collider)
    {
        Health health = collider.GetComponent<Health>();

        // Damage produced by player
        int damageDone = player.isCursed ? player.currentMainHandMinDamageValue : Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);

        bool criticalHitHappened = CriticalHitHappened();

        // Calculate damage after critical hit check
        if (player.onStealth)
        {
            damageDone = criticalHitHappened ? (int)(damageDone * (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier + player.additionalCriticalDamageOnStealth)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        health.PostHitImmunity();
        health.TakeDamage(damageDone, transform.position, health.transform.position, false);
        collider.GetComponent<HealthEvent>().CallHealthChangedEvent(damageDone / 1000000000, 1000000000, damageDone);
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
