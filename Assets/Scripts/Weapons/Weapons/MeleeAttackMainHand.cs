using System;
using System.Collections;
using System.Linq;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackMainHand : MonoBehaviour
{
    public bool IsAttackingAtRightHand { get; set; }

    [HideInInspector] public Coroutine playerAttackMotionRoutine;

    MeleeAttackEvent meleeAttackEvent;
    Animator rightHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperMainHand rightHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    BoxOrigin boxOrigin;
    Transform circleOriginTransform;
    Transform boxOriginTransform;
    Health enemyHealth;
    Player player;
    bool rightHandAttackBlocked;
    bool isSpecialMeleeAttack;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandMeleeAnimator = transform.GetChild(0).GetComponent<Animator>();
        rightHandAnimationEventHelper = rightHandMeleeAnimator.GetComponent<AnimationEventHelperMainHand>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        boxOrigin = GetComponentInChildren<BoxOrigin>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack += MeleeAttackEvent_MainHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationMainHandEventTriggered.AddListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackOffHandPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack -= MeleeAttackEvent_MainHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationMainHandEventTriggered.RemoveListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackOffHandPerformed.RemoveListener(DetectColliders);
    }

    void Start()
    {
        circleOriginTransform = circleOrigin.transform;
        boxOriginTransform = boxOrigin.transform;
    }

    private void MeleeAttackEvent_MainHandMeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        AttackAtMainHand(meleeAttackEventArgs.weapon, meleeAttackEventArgs.meleeAttackType, meleeAttackEventArgs.specialMeleeMove);
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectColliders()
    {
        if (!IsAttackingAtRightHand) return;

        switch (player.playerControl.meleeAttackTypeMainHand)
        {
            case MeleeAttackType.None:
                break;
            case MeleeAttackType.Swing:
            case MeleeAttackType.Sweep:
                foreach (Collider2D collider in Physics2D.OverlapCircleAll(circleOriginTransform.position, circleOrigin.circleRadius))
                {
                    if (collider.GetType() == typeof(PolygonCollider2D))
                    {
                        // Don't hit yourself if player is also in the collider list
                        if (collider.tag == Settings.playerTag) continue;

                        if (collider.tag == Settings.decoyTag) continue;

                        if (collider.tag == Settings.chestItemTag) continue;

                        if (enemyHealth = collider.GetComponent<Health>())
                        {
                            Enemy enemy = collider.GetComponent<Enemy>();

                            // Check if hit is successful or dodged by enemy
                            if (player.currentWeaponHandlingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100))
                            {
                                if (!enemy.enemyDetails.isEnemyBoss)
                                {
                                    CheckSuddenDeathStatus(enemy);
                                    CheckShatterStatus(enemy);
                                }

                                if (enemyHealth.suddenDeathHappened) return;

                                int inflictedDamage = CalculateDamageAmount(enemy);

                                if (isSpecialMeleeAttack)
                                {
                                    int inflictedProportionalDamage = (int)(enemy.enemyDetails.enemyHealthDetailsArray[GameManager.Instance.GetCurrentDungeonLevel().levelNumber - 1]
                                        .enemyHealthAmount * 0.15f);
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
                                enemy.health.isBlocking = true;
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
                    if (collider.GetType() == typeof(PolygonCollider2D))
                    {
                        // Don't hit yourself if player is also in the collider list
                        if (collider.tag == Settings.playerTag) continue;
                        if (collider.tag == Settings.decoyTag) continue;
                        if (collider.tag == Settings.chestItemTag) continue;

                        if (enemyHealth = collider.GetComponent<Health>())
                        {
                            Enemy enemy = collider.GetComponent<Enemy>();

                            // Check if hit is successful or dodged by enemy
                            if (player.currentWeaponHandlingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100))
                            {
                                if (!enemy.enemyDetails.isEnemyBoss)
                                {
                                    CheckSuddenDeathStatus(enemy);
                                    CheckShatterStatus(enemy);
                                }

                                if (enemyHealth.suddenDeathHappened) return;
                                
                                if (isSpecialMeleeAttack)
                                {
                                    // BLOOD DRAIN SKILL FOR EREBUS
                                    int inflictedProportionalDamage = (int)(enemyHealth.currentHealth * (0.15f + player.bloodDrainSkillAdditionalDamagePercentageModifier));
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
                                enemy.health.isBlocking = true;
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
        int nonElementalDamage = damageDone - elementalDamage;

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

        Debug.Log("Inflicted elemental damage " + inflictedElementalDamage);
        Debug.Log("Inflicted non-elemental damage " + inflictedNonElementalDamage);

        return inflictedElementalDamage + inflictedNonElementalDamage;
    }

    /// <summary>
    /// Critical hit check
    /// </summary>
    /// <returns></returns>
    private bool CriticalHitHappened()
    {
        bool criticalHitHappened;
        if (player.onStealth)
        {
            criticalHitHappened = true;
        }
        else
        {
            float randomCriticalDice = Random.Range(0f, 1f);

            float criticalHitChanceModifier = 0f;

            if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Dagger &&
                    player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Dagger &&
                    player.selectedPassiveItem.GetCurrentBackPassiveItem()?.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    criticalHitChanceModifier = 0.15f;
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

            criticalHitHappened = randomCriticalDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitChance + criticalHitChanceModifier ?
                true : false;
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
            if (randomDice > player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.suddenKillChance)
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

    private void ShatterProcess(Enemy enemy)
    {
        enemy.healthEvent.CallGetShatteredEvent();
    }

    public void ResetIsAttackingRightHand()
    {
        IsAttackingAtRightHand = false;
        player.playerControl.meleeAttackTypeMainHand = MeleeAttackType.None;
        player.health.isDamageable = true;
    }

    private void AttackAtMainHand(Weapon weapon, MeleeAttackType meleeAttackType, bool specialMeleeMove)
    {
        if (rightHandAttackBlocked) return;

        isSpecialMeleeAttack = specialMeleeMove;

        player.health.isDamageable = false;
        rightHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtRightHand);

        weapon.onCooldown = true;

        switch (meleeAttackType)
        {
            case MeleeAttackType.None:
                break;
            case MeleeAttackType.Swing:
                rightHandMeleeAnimator.SetInteger("attackMoveType", 0);
                break;
            case MeleeAttackType.Sweep:
                rightHandMeleeAnimator.SetInteger("attackMoveType", 1);
                break;
            case MeleeAttackType.Thrust:
                rightHandMeleeAnimator.SetInteger("attackMoveType", 2);
                break;
            default:
                break;
        }

        IsAttackingAtRightHand = true;
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
