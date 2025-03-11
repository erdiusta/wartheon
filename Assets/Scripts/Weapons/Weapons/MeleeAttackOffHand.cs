using System;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackOffHand : MonoBehaviour
{
    public bool IsAttackingAtLeftHand { get; set; }

    [HideInInspector] public Coroutine playerAttackLeftHandRoutine;

    MeleeAttackEvent meleeAttackEvent;
    //Animator leftHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperOffHand leftHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    BoxOrigin boxOrigin;
    Transform circleOriginTransform;
    Transform boxOriginTransform;
    Health enemyHealth;
    Player player;
    bool leftHandAttackBlocked;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        //leftHandMeleeAnimator = transform.GetChild(1).GetComponent<Animator>();
        //leftHandAnimationEventHelper = leftHandMeleeAnimator.GetComponent<AnimationEventHelperOffHand>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        boxOrigin = GetComponentInChildren<BoxOrigin>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnLeftHandMeleeAttack += MeleeAttackEvent_OnLeftHandMeleeAttack;
        //leftHandAnimationEventHelper.OnAnimationOffHandEventTriggered.AddListener(ResetIsAttackingLeftHand);
        //leftHandAnimationEventHelper.OnAttackOffHandPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnLeftHandMeleeAttack -= MeleeAttackEvent_OnLeftHandMeleeAttack;
        //leftHandAnimationEventHelper.OnAnimationOffHandEventTriggered.RemoveListener(ResetIsAttackingLeftHand);
        //leftHandAnimationEventHelper.OnAttackOffHandPerformed.RemoveListener(DetectColliders);
    }

    void Start()
    {
        circleOriginTransform = circleOrigin.transform;
        boxOriginTransform = boxOrigin.transform;
    }

    private void MeleeAttackEvent_OnLeftHandMeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        AttackWithOffHand(meleeAttackEventArgs.weapon, meleeAttackEventArgs.meleeAttackType);
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectColliders()
    {
        if (!IsAttackingAtLeftHand) return;

        switch (player.playerControl.meleeAttackTypeOffHand)
        {
            case MeleeAttackType.None:
                break;
            case MeleeAttackType.Swing:
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
                                enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponImpactSoundEffect);

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
                // Use box shape attack in case of thrust attack
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
                                enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponImpactSoundEffect);

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
        int damageDone = player.isCursed ? player.currentOffHandMinDamageValue : Random.Range(player.currentOffHandMinDamageValue, player.currentOffHandMaxDamageValue);

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
            // Add critical damage modifier if player is on stealth
            damageDone = criticalHitHappened ? (int)(damageDone * (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier + player.additionalCriticalDamageOnStealth)) : damageDone;
        }
        else
        {
            damageDone = criticalHitHappened ? (int)(damageDone * player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.criticalHitDamageMultiplier +
                player.additionalCriticalMeleeDamageModifier) : damageDone;
        }

        // Segregate elemental and non-elemental damage
        int elementalDamage = (int)(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.elementalForgeRate * damageDone);

        int additionalElementalDamage = (int)(elementalDamage * player.additionalElementalDamageModifier);
        elementalDamage += additionalElementalDamage;

        int nonElementalDamage = damageDone - elementalDamage + additionalElementalDamage;

        int inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - enemy.currentPhysicalResistance));

        int inflictedElementalDamage = 0;
        // Calculate inflicted elemental damage
        switch (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.elementalBias)
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
                    criticalHitChanceModifier = 0.1f;
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
        if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.canKillSuddenly && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice > player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.suddenKillChance)
            {
                enemyHealth.suddenDeathHappened = true;
                enemyHealth.TakeDamage(5000, transform.position, enemy.transform.position, false);
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
    /// Check acid status
    /// </summary>
    private void CheckAcidStatus(Enemy enemy)
    {
        if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasAcid && enemy.armorStatus != ArmorStatus.Acid &
            enemy.health.currentHealth > 0)
        {
            // Check get acid
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.acidEfficiency)
            {
                enemy.armorStatus = ArmorStatus.Acid;
                enemy.currentPhysicalResistance = (float)Math.Round(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.acidEfficiency * enemy.currentPhysicalResistance, 2);
                enemy.healthEvent.CallGetAcidEvent();
            }
        }
    }
    /// <summary>
    /// Check poison status
    /// </summary>
    private void CheckPoisonStatus(Enemy enemy)
    {
        if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.isPoisonous)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.poisonChance)
            {
                enemy.healthEvent.CallGetPoisonedEvent();
                enemy.healthStatus = HealthStatus.Poisoned;
            }
        }
    }


    /// <summary>
    /// Check frost status
    /// </summary>
    private void CheckFrostStatus(Enemy enemy)
    {
        EnemyAI enemyMovementAI = enemy.GetComponent<EnemyAI>();

        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasFrostDamage && enemyMovementAI.moveStatus != MoveStatus.Frozen
            && enemy.health.currentHealth > 0)
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

        if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasStunDamage && enemyMovementAI.moveStatus != MoveStatus.Stun
            && enemy.health.currentHealth > 0)
        {
            float randomStunNum = Random.Range(0f, 1f);
            if (randomStunNum > 0.6f)
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
        if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasBlindDamage)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.blindChance + player.additionalBlindMakerModifier)
            {
                enemy.healthEvent.CallGetBlindEvent();
            }
        }
    }

    public void ResetIsAttackingLeftHand()
    {
        IsAttackingAtLeftHand = false;
        player.playerControl.meleeAttackTypeOffHand = MeleeAttackType.None;
    }

    void AttackWithOffHand(Weapon weapon, MeleeAttackType meleeAttackType)
    {
        if (leftHandAttackBlocked) return;

        //leftHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtLeftHand);

        weapon.onCooldown = true;

        //switch (meleeAttackType)
        //{
        //    case MeleeAttackType.None:
        //        break;
        //    case MeleeAttackType.Swing:
        //        leftHandMeleeAnimator.SetInteger("attackMoveType", 0);
        //        break;
        //    case MeleeAttackType.Sweep:
        //        leftHandMeleeAnimator.SetInteger("attackMoveType", 1);
        //        break;
        //    case MeleeAttackType.Thrust:
        //        leftHandMeleeAnimator.SetInteger("attackMoveType", 2);
        //        break;
        //    default:
        //        break;
        //}

        //// Trigger the attack animation
        //leftHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtLeftHand);

        IsAttackingAtLeftHand = true;
        leftHandAttackBlocked = true;
        StartCoroutine(DelayAttackLeftHand(weapon));

        // Melee attack sound effect
        SoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);

        // Weapon fired event for starting cooldown ui
        player.weaponFiredEvent.CallWeaponFiredEvent(player.activeWeapon.GetCurrentOffHandWeapon(), false);
    }

    IEnumerator DelayAttackLeftHand(Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalMeleeAttackCoolDownModifier));

        weapon.onCooldown = false;
        leftHandAttackBlocked = false;
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


