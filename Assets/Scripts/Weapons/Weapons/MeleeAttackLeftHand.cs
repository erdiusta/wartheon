using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackLeftHand : MonoBehaviour
{
    public bool IsAttackingAtLeftHand { get; set; }

    [HideInInspector] public Coroutine playerAttackLeftHandRoutine;

    MeleeAttackEvent meleeAttackEvent;
    Animator leftHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperLeft leftHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    Transform circleOriginTransform;
    Health enemyHealth;
    Player player;
    bool leftHandAttackBlocked;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        leftHandMeleeAnimator = transform.GetChild(1).GetComponent<Animator>();
        leftHandAnimationEventHelper = leftHandMeleeAnimator.GetComponent<AnimationEventHelperLeft>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnLeftHandMeleeAttack += MeleeAttackEvent_OnLeftHandMeleeAttack;
        leftHandAnimationEventHelper.OnAnimationLeftHandEventTriggered.AddListener(ResetIsAttackingLeftHand);
        leftHandAnimationEventHelper.OnAttackLeftHandPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnLeftHandMeleeAttack -= MeleeAttackEvent_OnLeftHandMeleeAttack;
        leftHandAnimationEventHelper.OnAnimationLeftHandEventTriggered.RemoveListener(ResetIsAttackingLeftHand);
        leftHandAnimationEventHelper.OnAttackLeftHandPerformed.RemoveListener(DetectColliders);
    }

    void Start()
    {
        // Assuming there is a SpriteRenderer component on the weapon GameObject
        weaponSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        if (weaponSpriteRenderer == null)
        {
            Debug.LogError("SpriteRenderer component not found on the weapon GameObject!");
        }

        circleOriginTransform = circleOrigin.transform;

    }

    private void MeleeAttackEvent_OnLeftHandMeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        AttackAtLeftHand(meleeAttackEventArgs.weapon, meleeAttackEventArgs.meleeAttackType);
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectColliders()
    {
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

                    CheckSuddenDeathStatus(enemy);

                    if (enemyHealth.suddenDeathHappened)
                    {
                        if (enemy != null)
                        {
                            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
                            enemyHealth.TakeDamage(enemy.health.currentHealth, transform.position, enemy.transform.position, false);
                        }
                    }
                    else
                    {
                        if (enemy != null)
                        {
                            int inflictedDamage = CalculateDamageAmount(enemy);
                            enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                        }
                    }
                    SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponImpactSoundEffect);

                    CheckAcidStatus(enemy);
                    CheckStunStatus(enemy);

                    if (player.playerDetails.playerCharacterName == Settings.erebus && player.playerDetails.onStealth)
                    {
                        player.playerControl.Unstealth();
                    }

                    if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                    {
                        enemy.enemyMovementAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                    }

                    if (playerAttackLeftHandRoutine == null)
                    {
                        player.meleeAttackRightHand.playerAttackMotionRoutine = null;
                    }
                }
            }
        }
    }

    /// <summary>
    /// Calculate damage amount
    /// </summary>
    private int CalculateDamageAmount(Enemy enemy)
    {
        // Damage produced by player
        int damageDone = Random.Range(player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.meleeDamageMin,
            player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.meleeDamageMax);

        // Critical hit check
        bool criticalHitHappened;
        if (player.playerDetails.onStealth)
        {
            criticalHitHappened = true;
        }
        else
        {
            float randomCriticalDice = Random.Range(0f, 1f);
            criticalHitHappened = randomCriticalDice < player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.criticalHitChance ?
                true : false;
        }

        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        }

        damageDone = criticalHitHappened == true ? (int)(damageDone * player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.
            criticalHitDamageMultiplier) : damageDone;

        // Damage inflicted to enemy after deducting enemy armor
        int inflictedDamage = damageDone > enemyHealth.GetArmorValue() ? damageDone - enemyHealth.GetArmorValue() : 1;
        return inflictedDamage;
    }

    /// <summary>
    /// Check sudden death status
    /// </summary>
    private void CheckSuddenDeathStatus(Enemy enemy)
    {
        if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.canKillSuddenly && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.suddenKillChance)
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
        if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.hasAcid && enemy.armorStatus != ArmorStatus.Acid &
            enemy.health.currentHealth > 0)
        {
            // Check get acid
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.acidEfficiency)
            {
                enemy.armorStatus = ArmorStatus.Acid;
                enemyHealth.SetArmorValue((int)(enemy.enemyDetails.enemyArmorValue *
                    (1 - player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.acidEfficiency)));

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

        if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.hasStunDamage && enemyMovementAI.moveStatus != MoveStatus.Stun
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

    IEnumerator PlayerAttackAnimRoutine()
    {
        player.movementByVelocity.moveSpeed = 0;

        // Adjust animator layer weights
        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

        switch (player.playerControl.GetAimDirection())
        {
            case AimDirection.Up:
                break;
            case AimDirection.UpLeft:
            case AimDirection.UpRight:
            case AimDirection.Right:
            case AimDirection.Left:
            case AimDirection.Down:
                player.animator.SetTrigger(Settings.attackMotion);
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(0.25f);

        player.meleeAttackRightHand.playerAttackMotionRoutine = null;
        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.GetMoveSpeed();
    }

    public void ResetIsAttackingLeftHand()
    {
        IsAttackingAtLeftHand = false;
        player.health.isDamageable = true;
    }

    void AttackAtLeftHand(Weapon weapon, MeleeAttackType meleeAttackType)
    {
        if (leftHandAttackBlocked) return;

        player.health.isDamageable = false;
        leftHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtLeftHand);

        switch (meleeAttackType)
        {
            case MeleeAttackType.None:
                break;
            case MeleeAttackType.Slash:
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
        SoundEffect(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponSwingSoundEffect);
    }

    IEnumerator DelayAttackLeftHand(Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.weaponDetails.weaponFireRate);

        leftHandAttackBlocked = false;
    }

    /// <summary>
    /// Play weapon shooting sound effect
    /// </summary>
    private void SoundEffect(SoundEffectSO soundEffect)
    {
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponSwingSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(soundEffect);
        }
    }
}


