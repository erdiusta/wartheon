using System.Collections;
using System.Linq;
using UnityEngine;

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

                            // Check if hit is successful or deflected by enemy
                            if (player.currentWeaponHandlingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100))
                            {
                                CheckSuddenDeathStatus(enemy);

                                if (enemyHealth.suddenDeathHappened)
                                {
                                    SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
                                    enemyHealth.TakeDamage(enemy.health.currentHealth + 10, transform.position, enemy.transform.position, false);
                                }
                                else
                                {
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
                                }

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                CheckAcidStatus(enemy);
                                CheckStunStatus(enemy);

                                if (player.playerDetails.playerCharacterIndex == Character.Erebus && player.onStealth)
                                {
                                    player.playerControl.Unstealth();
                                }

                                if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                                {
                                    enemy.enemyAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                                }
                            }
                            else
                            {
                                enemy.health.isBlocking = true;
                                enemy.healthEvent.CallDeflectionEvent();
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

                            // Check if hit is successful or deflected by enemy
                            if (player.currentWeaponHandlingValue * 100 - enemy.enemyDetails.deflectionValue * 100 > Random.Range(0, 100))
                            {
                                CheckSuddenDeathStatus(enemy);

                                if (enemyHealth.suddenDeathHappened)
                                {
                                    SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
                                    enemyHealth.TakeDamage(enemy.health.currentHealth, transform.position, enemy.transform.position, false);
                                }
                                else
                                {
                                    if (isSpecialMeleeAttack)
                                    {
                                        int inflictedProportionalDamage = (int)(enemyHealth.currentHealth * 0.15f);
                                        enemyHealth.TakeDamage(inflictedProportionalDamage, transform.position, enemy.transform.position, false);
                                    }
                                    else
                                    {
                                        int inflictedDamage = CalculateDamageAmount(enemy);
                                        enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                                    }
                                }

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                CheckAcidStatus(enemy);
                                CheckStunStatus(enemy);

                                if (player.playerDetails.playerCharacterIndex == Character.Erebus && player.onStealth)
                                {
                                    player.playerControl.Unstealth();
                                }

                                if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                                {
                                    enemy.enemyAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                                }
                            }
                            else
                            {
                                enemy.health.isBlocking = true;
                                enemy.healthEvent.CallDeflectionEvent();
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

        damageDone = criticalHitHappened ? (int)(damageDone * player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitDamageMultiplier) : damageDone;
        int inflictedDamage = damageDone > enemyHealth.GetArmorValue() ? damageDone - enemyHealth.GetArmorValue() : 1;
        Debug.Log("Damage inflicted is " + inflictedDamage);

        return inflictedDamage;
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

            float criticalHitModifier = 0f;

            if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Dagger &&
                    player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Dagger &&
                    player.selectedPassiveItem.GetCurrentBackPassiveItem()?.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    criticalHitModifier = 0.15f;
                }
                else
                {
                    criticalHitModifier = 0f;
                }
            }
            else
            {
                criticalHitModifier = 0f;
            }

            criticalHitHappened = randomCriticalDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitChance + criticalHitModifier ?
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
            if (randomDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.suddenKillChance)
            {
                enemyHealth.suddenDeathHappened = true;
                enemy.healthEvent.CallGetDeathEvent();
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
                enemyHealth.SetArmorValue((int)(enemy.enemyDetails.enemyArmorValue *
                    (1 - player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.acidEfficiency)));

                enemy.GetComponent<HealthEvent>().CallGetAcidEvent();
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
                StartCoroutine(StunRoutine(enemy));
            }
        }
    }

    IEnumerator StunRoutine(Enemy enemy)
    {
        enemy.enemyAI.moveStatus = MoveStatus.Stun;
        enemy.healthEvent.CallGetStunEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForFixedUpdate();
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
        yield return new WaitForSeconds(weapon.weaponDetails.weaponCooldownDuration);

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
