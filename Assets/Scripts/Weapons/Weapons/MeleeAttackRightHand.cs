using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackRightHand : MonoBehaviour
{
    public bool IsAttackingAtRightHand { get; set; }

    MeleeAttackEvent meleeAttackEvent;
    Animator rightHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperRight rightHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    Transform circleOriginTransform;
    float radius = 0.2f;
    Health enemyHealth;
    Player player;
    Knockback knockback;
    bool rightHandAttackBlocked;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandMeleeAnimator = transform.GetChild(0).GetComponent<Animator>();
        rightHandAnimationEventHelper = rightHandMeleeAnimator.GetComponent<AnimationEventHelperRight>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        knockback = GetComponent<Knockback>();
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
                if (collider.tag == "Player")
                    continue;

                if (enemyHealth = collider.GetComponent<Health>())
                {
                    PlayerAttackAnimation();
                    enemyHealth.TakeDamage(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.meleeDamageMax,
                        transform.position, collider.transform.position);

                    if (!enemyHealth.GetComponent<Enemy>().enemyDetails.hasKnockbackResistance)
                    {
                        Knockback enemyKnockback = collider.GetComponent<Enemy>().GetComponent<Knockback>();
                        collider.GetComponent<EnemyMovementAI>().Knockback((collider.transform.position - transform.position).normalized,
                            enemyKnockback.knockbackForce, enemyKnockback.knockbackTimeWeight);
                    }
                }
            }
        }
    }

    private void PlayerAttackAnimation()
    {
        // Adjust animator layer weights
        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

        player.animator.SetTrigger(Settings.attackMotion);
    }

    public void ResetIsAttackingRightHand()
    {
        IsAttackingAtRightHand = false;
    }

    void AttackAtRightHand(Weapon weapon)
    {
        if (rightHandAttackBlocked) return;

        // Trigger the attack animation
        rightHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtRightHand);

        IsAttackingAtRightHand = true;
        rightHandAttackBlocked = true;
        StartCoroutine(DelayAttackRightHand(weapon));

        // Melee attack sound effect
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon)
        {
            SoundEffect(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponFiringSoundEffect);
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
        if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponFiringSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(soundEffect);
        }
    }
}


