using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackRightHand : MonoBehaviour
{
    public bool IsAttackingAtRightHand { get; set; }

    [HideInInspector] public Coroutine playerAttackRightHandRoutine;

    MeleeAttackEvent meleeAttackEvent;
    Animator rightHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperRight rightHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    Transform circleOriginTransform;
    float radius = 0.2f;
    Health enemyHealth;
    Player player;
    bool rightHandAttackBlocked;

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

    private void MeleeAttackEvent_OnRightHandMeleeAttack(MeleeAttackEvent meleeAttackEvent, MeleeAttackEventArgs meleeAttackEventArgs)
    {
        AttackAtRightHand(meleeAttackEventArgs.weapon);
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
                if (collider.tag == Settings.playerTag)
                    continue;

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

                    SoundEffectManager.Instance.PlaySoundEffect(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponImpactSoundEffect);

                    CheckAcidStatus(enemy);
                    CheckBleedingStatus(enemy);
                    CheckStunStatus(enemy);
                    CheckSlowStatus(enemy);

                    if (player.playerDetails.playerCharacterName == Settings.erebus && player.playerDetails.onStealth)
                    {
                        player.playerControl.Unstealth();
                    }

                    if (!enemy.enemyDetails.hasKnockbackResistance && enemyHealth.currentHealth > 0)
                    {
                        enemy.enemyMovementAI.TriggerKnockback((enemy.transform.position - transform.position).normalized);
                    }

                    if (playerAttackRightHandRoutine == null)
                    {
                        playerAttackRightHandRoutine = StartCoroutine(PlayerAttackAnimRoutine());
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
        int damageDone = Random.Range(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.meleeDamageMin,
            player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.meleeDamageMax);

        // Critical hit check
        float randomCriticalDice = Random.Range(0f, 1f);
        bool criticalHitHappened = randomCriticalDice < player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.criticalHitChance ?
            true : false;

        if (criticalHitHappened)
        {
            enemy.healthEvent.CallCriticalHitEvent();
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.criticalHitSoundEffect);
        } 

        damageDone = criticalHitHappened == true ? (int)(damageDone * player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.
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
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.canKillSuddenly && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.suddenKillChance)
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
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasAcid && enemy.armorStatus != ArmorStatus.Acid && 
            enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.acidEfficiency)
            {
                enemy.armorStatus = ArmorStatus.Acid;
                enemyHealth.SetArmorValue((int)(enemy.enemyDetails.enemyArmorValue *
                    (1 - player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.acidEfficiency)));

                enemy.GetComponent<HealthEvent>().CallGetAcidEvent();
            }
        }
    }

    /// <summary>
    /// Check bleed status
    /// </summary>
    private void CheckBleedingStatus(Enemy enemy)
    {
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasBleedingDamage)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.bleedingChance)
            {
                enemy.healthEvent.CallGetBleedingEvent();
                enemy.healthStatus = HealthStatus.Bleeding;
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    private void CheckStunStatus(Enemy enemy)
    {
        EnemyMovementAI enemyMovementAI = enemy.GetComponent<EnemyMovementAI>();

        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasStunDamage && enemyMovementAI.moveStatus != MoveStatus.Stun 
            && enemyMovementAI.moveStatus != MoveStatus.Slow && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.stunChance)
            {
                StartCoroutine(StunRoutine(enemy));
            }
        }
    }

    /// <summary>
    /// Check slow status
    /// </summary>
    private void CheckSlowStatus(Enemy enemy)
    {
        EnemyMovementAI enemyMovementAI = enemy.GetComponent<EnemyMovementAI>();

        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasSlowDamage && enemyMovementAI.moveStatus != MoveStatus.Stun
            && enemyMovementAI.moveStatus != MoveStatus.Slow && enemy.health.currentHealth > 0)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.slowChance)
            {
                SlowEnemySpeed(enemy);
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

    private void SlowEnemySpeed(Enemy enemy)
    {
        float slowedMinMoveSpeed = enemy.enemyDetails.movementDetails.minMoveSpeed * 0.6f;
        float slowedMaxMoveSpeed = enemy.enemyDetails.movementDetails.maxMoveSpeed * 0.6f;
        enemy.enemyMovementAI.moveSpeed = Random.Range(slowedMinMoveSpeed, slowedMaxMoveSpeed);
        enemy.enemyMovementAI.moveStatus = MoveStatus.Slow;
        enemy.healthEvent.CallGetSlowEvent();
    }

    public IEnumerator PlayerAttackAnimRoutine()
    {
        // Adjust animator layer weights
        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

        player.animator.SetBool(Settings.attackMotion, true);

        yield return new WaitForSeconds(0.3f);

        playerAttackRightHandRoutine = null;
    }

    public void ResetIsAttackingRightHand()
    {
        IsAttackingAtRightHand = false;
    }

    void AttackAtRightHand(Weapon weapon)
    {
        if (rightHandAttackBlocked) return;

        rightHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtRightHand);

        IsAttackingAtRightHand = true;
        rightHandAttackBlocked = true;
        StartCoroutine(DelayAttackRightHand(weapon));

        // Melee attack sound effect
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon)
        {
            SoundEffect(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponSwingSoundEffect);
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
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponSwingSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(soundEffect);
        }
    }
}
