using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackRightHand : MonoBehaviour
{
    public bool IsAttackingAtRightHand { get; set; }

    [HideInInspector] public Coroutine playerAttackMotionRoutine;

    MeleeAttackEvent meleeAttackEvent;
    Animator rightHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperRight rightHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    Transform circleOriginTransform;
    Health enemyHealth;
    Player player;
    bool rightHandAttackBlocked;
    int smearFxChangeCounter;
    float smearFxTimer = 3;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandMeleeAnimator = transform.GetChild(0).GetComponent<Animator>();
        rightHandAnimationEventHelper = rightHandMeleeAnimator.GetComponent<AnimationEventHelperRight>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack += MeleeAttackEvent_OnRightHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationRightHandEventTriggered.AddListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackRightHandPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack -= MeleeAttackEvent_OnRightHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationRightHandEventTriggered.RemoveListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackRightHandPerformed.RemoveListener(DetectColliders);
    }

    private void Update()
    {
        if (smearFxTimer < 0f)
        {
            smearFxChangeCounter = 0;
            smearFxTimer = 2;
        }

        smearFxTimer -= Time.deltaTime;
    }

    void Start()
    {
        //// Assuming there is a SpriteRenderer component on the weapon GameObject
        //weaponSpriteRenderer = GetComponentInChildren<SpriteRenderer>();
        //if (weaponSpriteRenderer == null)
        //{
        //    Debug.LogError("SpriteRenderer component not found on the weapon GameObject!");
        //}

        circleOriginTransform = circleOrigin.transform;
    }

    private void MeleeAttackEvent_OnRightHandMeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        AttackAtRightHand(meleeAttackEventArgs.weapon, meleeAttackEventArgs.meleeAttackType);
    }

    /// <summary>
    /// Based on circle radius of melee weapon, detect all enemy colliders for damage
    /// </summary>
    public void DetectColliders()
    {
        if (!IsAttackingAtRightHand) return;

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
                        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.suddenDeathSoundEffect);
                        enemyHealth.TakeDamage(enemy.health.currentHealth, transform.position, enemy.transform.position, false);
                    }
                    else
                    {
                        int inflictedDamage = CalculateDamageAmount(enemy);
                        enemyHealth.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, false);
                    }

                    SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponImpactSoundEffect);

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
        int damageDone = Random.Range(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMin,
            player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.meleeDamageMax);

        // Critical hit check
        bool criticalHitHappened;
        if (player.playerDetails.onStealth)
        {
            criticalHitHappened = true;   
        }
        else
        {
            float randomCriticalDice = Random.Range(0f, 1f);
            criticalHitHappened = randomCriticalDice < player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.criticalHitChance ?
                true : false;
        }

        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();
            //SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        } 

        damageDone = criticalHitHappened == true ? (int)(damageDone * player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.
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
        EnemyMovementAI enemyMovementAI = enemy.GetComponent<EnemyMovementAI>();

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
        enemy.enemyMovementAI.moveStatus = MoveStatus.Stun;
        enemy.healthEvent.CallGetStunEvent();
        enemy.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        enemy.animator.SetBool(Settings.isStunned, true);
        SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.stunSoundEffect);

        yield return new WaitForFixedUpdate();
    }

    public IEnumerator PlayerAttackAnimRoutine()
    {
        // Adjust animator layer weights
        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

        switch (player.playerControl.GetAimDirection())
        {
            case AimDirection.Up:
            case AimDirection.UpLeft:
            case AimDirection.UpRight:
                break;

            case AimDirection.Right:
            case AimDirection.Left:
            case AimDirection.Down:
                player.animator.SetTrigger(Settings.attackMotion);
                break;
            default:
                break;
        }

        yield return new WaitForSeconds(0.25f);

        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.GetMoveSpeed();
        playerAttackMotionRoutine = null;
    }

    public void ResetIsAttackingRightHand()
    {
        IsAttackingAtRightHand = false;
        player.health.isDamageable = true;
    }

    void AttackAtRightHand(Weapon weapon, MeleeAttackType meleeAttackType)
    {
        if (rightHandAttackBlocked) return;

        player.health.isDamageable = false;
        rightHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtRightHand);

        switch (meleeAttackType)
        {
            case MeleeAttackType.None:
                break;
            case MeleeAttackType.Slash:
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

        if (playerAttackMotionRoutine == null)
        {
            playerAttackMotionRoutine = StartCoroutine(PlayerAttackAnimRoutine());
        }

        IsAttackingAtRightHand = true;
        rightHandAttackBlocked = true;
        StartCoroutine(DelayAttackRightHand(weapon));

        // Melee attack sound effect
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
        {
            SoundEffect(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
        }
    }

    IEnumerator DelayAttackRightHand(Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.weaponDetails.weaponFireRate);

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
