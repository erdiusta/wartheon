using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttackLeftHand : MonoBehaviour
{
    public bool IsAttackingAtLeftHand { get; private set; }

    MeleeAttackEvent meleeAttackEvent;
    Animator leftHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelperLeft leftHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    Transform circleOriginTransform;
    float radius = 0.2f;
    Health enemyHealth;
    Player player;
    Knockback knockback;
    bool leftHandAttackBlocked;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        leftHandMeleeAnimator = transform.GetChild(1).GetComponent<Animator>();
        leftHandAnimationEventHelper = leftHandMeleeAnimator.GetComponent<AnimationEventHelperLeft>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        knockback = GetComponent<Knockback>();
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
        AttackAtLeftHand(meleeAttackEventArgs.weapon);
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
                if (collider.tag == "Player")
                    continue;

                if (enemyHealth = collider.GetComponent<Health>())
                {
                    enemyHealth.TakeDamage(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.meleeDamageMax,
                        transform.position, collider.transform.position);
                    SoundEffect(enemyHealth.GetComponent<Enemy>().enemyDetails.getHitSoundEffect);

                    collider.GetComponent<EnemyMovementAI>().Knockback((collider.transform.position - transform.position).normalized,
                        knockback.knockbackForce, knockback.knockbackTimeWeight);
                }
            }
        }
    }

    public void ResetIsAttackingLeftHand()
    {
        IsAttackingAtLeftHand = false;
    }

    void AttackAtLeftHand(Weapon weapon)
    {
        if (leftHandAttackBlocked)
            return;

        // Trigger the attack animation
        leftHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtLeftHand);

        IsAttackingAtLeftHand = true;
        leftHandAttackBlocked = true;
        StartCoroutine(DelayAttackLeftHand(weapon));

        // Melee attack sound effect
        SoundEffect(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponFiringSoundEffect);
    }

    IEnumerator DelayAttackLeftHand(Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.weaponDetails.meleeAttackCooldown);

        leftHandAttackBlocked = false;
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


