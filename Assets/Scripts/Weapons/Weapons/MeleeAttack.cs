using System;
using System.Collections;
using UnityEngine;

[RequireComponent(typeof(MeleeAttackEvent))]
[DisallowMultipleComponent]
public class MeleeAttack : MonoBehaviour
{
    public bool IsAttackingAtRightHand { get; private set; }
    public bool IsAttackingAtLeftHand { get; private set; }

    MeleeAttackEvent meleeAttackEvent;
    Animator rightHandMeleeAnimator;
    Animator leftHandMeleeAnimator;
    SpriteRenderer weaponSpriteRenderer;
    AnimationEventHelper rightHandAnimationEventHelper;
    AnimationEventHelper leftHandAnimationEventHelper;
    CircleOrigin circleOrigin;
    Transform circleOriginTransform;
    float radius = 0.2f;
    Health enemyHealth;
    Player player;
    Knockback knockback;
    bool rightHandAttackBlocked;
    bool leftHandAttackBlocked;

    private void Awake()
    {
        player = GetComponent<Player>();
        meleeAttackEvent = GetComponent<MeleeAttackEvent>();
        rightHandMeleeAnimator = transform.GetChild(0).GetComponent<Animator>();
        leftHandMeleeAnimator = transform.GetChild(1).GetComponent<Animator>();
        rightHandAnimationEventHelper = rightHandMeleeAnimator.GetComponent<AnimationEventHelper>();
        leftHandAnimationEventHelper = leftHandMeleeAnimator.GetComponent<AnimationEventHelper>();
        circleOrigin = GetComponentInChildren<CircleOrigin>();
        knockback = GetComponent<Knockback>();
    }

    private void OnEnable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack += MeleeAttackEvent_OnRightHandMeleeAttack;
        meleeAttackEvent.OnLeftHandMeleeAttack += MeleeAttackEvent_OnLeftHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationRightHandEventTriggered.AddListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackPerformed.AddListener(DetectColliders);
        leftHandAnimationEventHelper.OnAnimationLeftHandEventTriggered.AddListener(ResetIsAttackingLeftHand);
        leftHandAnimationEventHelper.OnAttackPerformed.AddListener(DetectColliders);
    }

    private void OnDisable()
    {
        meleeAttackEvent.OnRightHandMeleeAttack -= MeleeAttackEvent_OnRightHandMeleeAttack;
        meleeAttackEvent.OnLeftHandMeleeAttack -= MeleeAttackEvent_OnLeftHandMeleeAttack;
        rightHandAnimationEventHelper.OnAnimationRightHandEventTriggered.AddListener(ResetIsAttackingRightHand);
        rightHandAnimationEventHelper.OnAttackPerformed.AddListener(DetectColliders);
        leftHandAnimationEventHelper.OnAnimationLeftHandEventTriggered.AddListener(ResetIsAttackingLeftHand);
        leftHandAnimationEventHelper.OnAttackPerformed.AddListener(DetectColliders);
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
                    enemyHealth.TakeDamage(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.meleeDamageMax);
                    SoundEffect(enemyHealth.GetComponent<Enemy>().enemyDetails.getHitSoundEffect);

                    collider.GetComponent<EnemyMovementAI>().Knockback((collider.transform.position - transform.position).normalized,
                        knockback.knockbackForce, knockback.knockbackTimeWeight);
                }
            }
        }
    }

    public void ResetIsAttackingRightHand()
    {
        IsAttackingAtRightHand = false;
    }

    public void ResetIsAttackingLeftHand()
    {
        IsAttackingAtLeftHand = false;
    }

    void AttackAtRightHand(Weapon weapon)
    {
        if (rightHandAttackBlocked)
            return;

        // Trigger the attack animation
        rightHandMeleeAnimator.SetTrigger(Settings.meleeAttackAtRightHand);

        IsAttackingAtRightHand = true;
        rightHandAttackBlocked = true;
        StartCoroutine(DelayAttackRightHand(weapon));

        // Melee attack sound effect
        SoundEffect(player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponFiringSoundEffect);
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

    IEnumerator DelayAttackRightHand(Weapon weapon)
    {
        yield return new WaitForSeconds(weapon.weaponDetails.meleeAttackCooldown);

        rightHandAttackBlocked = false;
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


