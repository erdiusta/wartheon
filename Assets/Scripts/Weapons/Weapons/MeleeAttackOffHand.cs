using System.Collections;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackOffHand : MonoBehaviour
{
    public bool IsAttackingAtLeftHand { get; set; }

    [HideInInspector] public Coroutine playerAttackLeftHandRoutine;

    MeleeAttackEvent meleeAttackEvent;
    Animator leftHandMeleeAnimator;
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
        leftHandMeleeAnimator = transform.GetChild(1).GetComponent<Animator>();
        leftHandAnimationEventHelper = leftHandMeleeAnimator.GetComponent<AnimationEventHelperOffHand>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        boxOrigin = GetComponentInChildren<BoxOrigin>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnLeftHandMeleeAttack += MeleeAttackEvent_OnLeftHandMeleeAttack;
        leftHandAnimationEventHelper.OnAnimationOffHandEventTriggered.AddListener(ResetIsAttackingLeftHand);
        leftHandAnimationEventHelper.OnAttackOffHandPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnLeftHandMeleeAttack -= MeleeAttackEvent_OnLeftHandMeleeAttack;
        leftHandAnimationEventHelper.OnAnimationOffHandEventTriggered.RemoveListener(ResetIsAttackingLeftHand);
        leftHandAnimationEventHelper.OnAttackOffHandPerformed.RemoveListener(DetectColliders);
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
                                    int inflictedDamage = CalculateDamageAmount(enemy);
                                    enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                                }

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                CheckAcidStatus(enemy);
                                CheckStunStatus(enemy);

                                if (player.playerDetails.playerCharacterIndex == Character.Erebus && player.onStealth)
                                {
                                    player.playerControl.Unstealth();
                                }

                                if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                                {
                                    enemy.enemyMovementAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                                }
                            }
                            else
                            {
                                enemy.health.isBlocking = true;
                                enemy.healthEvent.CallDeflectionEvent();
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
                                CheckSuddenDeathStatus(enemy);

                                if (enemyHealth.suddenDeathHappened)
                                {
                                    SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
                                    enemyHealth.TakeDamage(enemy.health.currentHealth, transform.position, enemy.transform.position, false);
                                }
                                else
                                {
                                    int inflictedDamage = CalculateDamageAmount(enemy);
                                    enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                                }

                                SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponImpactSoundEffect);

                                CheckAcidStatus(enemy);
                                CheckStunStatus(enemy);

                                if (player.playerDetails.playerCharacterIndex == Character.Erebus && player.onStealth)
                                {
                                    player.playerControl.Unstealth();
                                }

                                if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                                {
                                    enemy.enemyMovementAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                                }
                            }
                            else
                            {
                                enemy.health.isBlocking = true;
                                enemy.healthEvent.CallDeflectionEvent();
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

        // Critical hit check
        if (CriticalHitHappened())
        {
            enemy.healthEvent.CallCriticalHitEvent();
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        }

        damageDone = CriticalHitHappened() == true ? (int)(damageDone * player.currentMainHandCriticalHitChance) : damageDone;

        // Damage inflicted to enemy after deducting enemy armor
        int inflictedDamage = damageDone > enemyHealth.GetArmorValue() ? damageDone - enemyHealth.GetArmorValue() : 1;
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
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger &&
                    player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger &&
                    player.passiveItemList.Any(item => item.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak))
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

            criticalHitHappened = randomCriticalDice < player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.criticalHitChance + criticalHitModifier ?
                true : false;
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
            if (randomDice < player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.suddenKillChance)
            {
                enemyHealth.suddenDeathHappened = true;
                enemy.healthEvent.CallGetDeathEvent();
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
                enemyHealth.SetArmorValue((int)(enemy.enemyDetails.enemyArmorValue *
                    (1 - player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.acidEfficiency)));

                enemy.healthEvent.CallGetAcidEvent();
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    private void CheckStunStatus(Enemy enemy)
    {
        EnemyMovementAI enemyMovementAI = enemy.GetComponent<EnemyMovementAI>();

        if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasStunDamage && enemyMovementAI.moveStatus != MoveStatus.Stun
            && enemy.health.currentHealth > 0)
        {
            float randomStunNum = Random.Range(0f, 1f);
            if (randomStunNum > 0.6f)
            {
                StartCoroutine(StunRoutine(enemy));
            }
        }
    }

    IEnumerator StunRoutine(Enemy enemy)
    {
        enemy.enemyMovementAI.moveStatus = MoveStatus.Stun;
        enemy.healthEvent.CallGetStunEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForFixedUpdate();
    }

    public void ResetIsAttackingLeftHand()
    {
        IsAttackingAtLeftHand = false;
        player.playerControl.meleeAttackTypeOffHand = MeleeAttackType.None;
        player.health.isDamageable = true;
    }

    void AttackWithOffHand(Weapon weapon, MeleeAttackType meleeAttackType)
    {
        if (leftHandAttackBlocked) return;

        player.health.isDamageable = false;
        leftHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtLeftHand);

        weapon.onCooldown = true;

        switch (meleeAttackType)
        {
            case MeleeAttackType.None:
                break;
            case MeleeAttackType.Swing:
                leftHandMeleeAnimator.SetInteger("attackMoveType", 0);
                break;
            case MeleeAttackType.Sweep:
                leftHandMeleeAnimator.SetInteger("attackMoveType", 1);
                break;
            case MeleeAttackType.Thrust:
                leftHandMeleeAnimator.SetInteger("attackMoveType", 2);
                break;
            default:
                break;
        }

        // Trigger the attack animation
        leftHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtLeftHand);

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
        yield return new WaitForSeconds(weapon.weaponDetails.weaponCooldownDuration);

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


