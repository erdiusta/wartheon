using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ActiveWeapon : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer on the child Weapon gameobject")]
    #endregion
    [SerializeField] SpriteRenderer weaponSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the PolygonCollider2D on the child Weapon gameobject")]
    #endregion
    [SerializeField] PolygonCollider2D weaponPolygonCollider2D;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponShootPositionTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponEffectPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponEffectPositionTransform;
    #region Tooltip
    [Tooltip("Populate with the Animatior in the WeaponAnchorPosition gameobject")]
    #endregion
    [SerializeField] Animator weaponAnimator;

    SetActiveWeaponEvent setActiveWeaponEvent;
    Weapon currentWeapon;

    private void Awake()
    {
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    private void OnEnable()
    {
        setActiveWeaponEvent.OnSetActiveWeapon += SetActiveWeaponEvent_OnSetActiveWeapon;
    }

    private void OnDisable()
    {
<<<<<<< Updated upstream
        setActiveWeaponEvent.OnSetActiveWeapon -= SetActiveWeaponEvent_OnSetActiveWeapon;
=======
        setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon;
        setActiveWeaponEvent.OnSetInactiveMainHandWeapon -= SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        setActiveWeaponEvent.OnSetActiveOffHandWeapon -= SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon;
        setActiveWeaponEvent.OnSetInactiveOffHandWeapon -= SetActiveWeaponEvent_OnSetInactiveOffHandWeapon;
    }
    private void SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetMainHandWeapon(setActiveWeaponEventArgs.weapon);
        weaponMainHandAnimator.SetBool(Settings.isLeft, false);

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateWeaponHandlingAndCriticalValues();
        player?.UpdateEvasivenessValue();
>>>>>>> Stashed changes
    }

    private void SetActiveWeaponEvent_OnSetActiveWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetWeapon(setActiveWeaponEventArgs.weapon);
    }

    private void SetWeapon(Weapon weapon)
    {
<<<<<<< Updated upstream
        currentWeapon = weapon;
=======
        SetOffHandWeapon(setActiveWeaponEventArgs.weapon);
        weaponOffHandAnimator.SetBool(Settings.isLeft, true);

        // Update new weapon values
        player.UpdateDamageValues();
        player.UpdateWeaponHandlingAndCriticalValues();
        player.UpdateEvasivenessValue();
    }

    private void SetActiveWeaponEvent_OnSetInactiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectOffHandWeapon();
    }

    private void SetMainHandWeapon(Weapon weapon)
    {
        isSwitching = true;
        currentMainHandWeapon = weapon;
        weaponToBeDropped = weapon; // for removing lock icon during two-handed weapon drop issue

        if (player != null)
        {
            // If equipped weapon is two-handed, temporarily disable animator and change the position and enable again
            if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Bow && currentMainHandWeapon.weaponDetails.weaponName != "Crossbow")
            {
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.bowRuntimeAnimatorController;
                thirdHandGameObject.SetActive(true);
                weaponOffHandAnimator.enabled = false;
                offHandAnchorPosition.gameObject.SetActive(false);
            }
            else if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Staff)
            {
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.staffRuntimeAnimatorController;
                thirdHandGameObject.SetActive(false);
                weaponOffHandAnimator.enabled = true;
                offHandAnchorPosition.gameObject.SetActive(true);
            }
            else if (currentMainHandWeapon.weaponDetails.wieldType == WieldType.TwoHanded && currentMainHandWeapon.weaponDetails.weaponName != "Crossbow")
            {
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.twoHandRuntimeAnimatorController;
                thirdHandGameObject.SetActive(true);
                weaponOffHandAnimator.enabled = false;
                offHandAnchorPosition.gameObject.SetActive(false);
            }
            // If equipped one - hand, revert position and animator settings to default
            else
            {
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.oneHandRuntimeAnimatorController;
                thirdHandGameObject.SetActive(false);
                offHandAnchorPosition.gameObject.SetActive(true);
                weaponOffHandAnimator.enabled = true;
            }
        }

        // Set animator controller to the weapon animator
        weaponMainHandAnimator.runtimeAnimatorController = currentMainHandWeapon.weaponDetails.weaponAnimatorController;
>>>>>>> Stashed changes

        // Set current weapon sprite
        weaponSpriteRenderer.sprite = currentWeapon.weaponDetails.weaponSprite;

        // If the weapon has a polygon collider and a sprite then set it to the weapon sprite physics shape
        if (weaponPolygonCollider2D != null && weaponSpriteRenderer.sprite != null)
        {
            // Get sprite physics shape - this returns the sprite physics shape points as a list of Vector2s
            List<Vector2> spritePhysicsShapePointsList = new List<Vector2>();
            weaponSpriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsShapePointsList);

            // Set polygon collider on weapon to pick up physics shape for sprite - set collider points to sprite physics shape points
            weaponPolygonCollider2D.points = spritePhysicsShapePointsList.ToArray();
        }

        // If weapon is a melee weapon, set the animator controller
        if (weapon.weaponDetails.isMeleeWeapon)
        {
            weaponAnimator.runtimeAnimatorController = weapon.weaponDetails.weaponAnimatorController;
        }
        else
        {
            weaponAnimator.runtimeAnimatorController = null;
        }

        // Set weapon shoot position
        weaponShootPositionTransform.localPosition = currentWeapon.weaponDetails.weaponShootPosition;
    }

    public ProjectileDetailsSO GetCurrentProjectile()
    {
        return currentWeapon.weaponDetails.weaponCurrentProjectile;
    }

    public Weapon GetCurrentWeapon()
    {
        return currentWeapon;
    }

    public Vector3 GetShootPosition()
    {
        return weaponShootPositionTransform.position;
    }

    public Vector3 GetShootEffectPosition()
    {
        return weaponEffectPositionTransform.position;
    }

    public void RemoveCurrentWeapon()
    {
        currentWeapon = null;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponSpriteRenderer), weaponSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponPolygonCollider2D), weaponPolygonCollider2D);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponShootPositionTransform), weaponShootPositionTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponEffectPositionTransform), weaponEffectPositionTransform);
    }
#endif
    #endregion
}
