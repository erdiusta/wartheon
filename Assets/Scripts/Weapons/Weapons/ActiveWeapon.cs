using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ActiveWeapon : MonoBehaviour
{
    [Header("RIGHT HAND")]
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer on the child Weapon gameobject")]
    #endregion
    [SerializeField] SpriteRenderer weaponRightHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the PolygonCollider2D on the child Weapon gameobject")]
    #endregion
    [SerializeField] PolygonCollider2D weaponRightHandPolygonCollider2D;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponRightHandShootPositionTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponEffectPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponRightHandEffectPositionTransform;

    [Header("LEFT HAND")]
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer on the child Weapon Left Hand gameobject")]
    #endregion
    [SerializeField] SpriteRenderer weaponLeftHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the PolygonCollider2D on the child Weapon Left Hand gameobject")]
    #endregion
    [SerializeField] PolygonCollider2D weaponLeftHandPolygonCollider2D;

    SetActiveWeaponEvent setActiveWeaponEvent;
    Animator weaponRightHandAnimator;
    Animator weaponLeftHandAnimator;
    Weapon currentRightHandWeapon;
    Weapon currentLeftHandWeapon;

    private void Awake()
    {
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();

        weaponRightHandAnimator = transform.GetChild(0).GetComponent<Animator>();
        weaponLeftHandAnimator = transform.GetChild(1).GetComponent<Animator>();
    }

    private void OnEnable()
    {
        setActiveWeaponEvent.OnSetActiveRightHandWeapon += SetActiveRightWeaponEvent_OnSetActiveRightHandWeapon;
        setActiveWeaponEvent.OnSetActiveLeftHandWeapon += SetActiveLeftWeaponEvent_OnSetActiveLeftHandWeapon;
        setActiveWeaponEvent.OnSetInactiveLeftHandWeapon += SetInactiveLeftWeaponEvent_OnSetInactiveLeftWeapon;
    }

    private void OnDisable()
    {
        setActiveWeaponEvent.OnSetActiveRightHandWeapon -= SetActiveRightWeaponEvent_OnSetActiveRightHandWeapon;
        setActiveWeaponEvent.OnSetActiveLeftHandWeapon -= SetActiveLeftWeaponEvent_OnSetActiveLeftHandWeapon;
        setActiveWeaponEvent.OnSetInactiveLeftHandWeapon -= SetInactiveLeftWeaponEvent_OnSetInactiveLeftWeapon;
    }

    private void SetActiveRightWeaponEvent_OnSetActiveRightHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetRightHandWeapon(setActiveWeaponEventArgs.weapon);
<<<<<<< Updated upstream
=======
        weaponRightHandAnimator.SetBool(Settings.isLeft, false);
>>>>>>> Stashed changes
    }

    private void SetActiveLeftWeaponEvent_OnSetActiveLeftHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetLeftHandWeapon(setActiveWeaponEventArgs.weapon);
<<<<<<< Updated upstream
=======
        weaponLeftHandAnimator.SetBool(Settings.isLeft, true);
>>>>>>> Stashed changes
    }

    private void SetInactiveLeftWeaponEvent_OnSetInactiveLeftWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectLeftHandWeapon();
    }

    private void SetRightHandWeapon(Weapon weapon)
    {
        currentRightHandWeapon = weapon;

        // Set animator controller to the weapon animator
        weaponRightHandAnimator.runtimeAnimatorController = currentRightHandWeapon.weaponDetails.weaponAnimatorController;

        // Set current weapon sprite
        weaponRightHandSpriteRenderer.sprite = currentRightHandWeapon.weaponDetails.weaponSprite;

        // If the weapon has a polygon collider and a sprite then set it to the weapon sprite physics shape
        if (weaponRightHandPolygonCollider2D != null && weaponRightHandSpriteRenderer.sprite != null)
        {
            // Get sprite physics shape - this returns the sprite physics shape points as a list of Vector2s
            List<Vector2> spritePhysicsShapePointsList = new List<Vector2>();
            weaponRightHandSpriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsShapePointsList);

            // Set polygon collider on weapon to pick up physics shape for sprite - set collider points to sprite physics shape points
            weaponRightHandPolygonCollider2D.points = spritePhysicsShapePointsList.ToArray();
        }

        // Set weapon shoot position
        weaponRightHandShootPositionTransform.localPosition = currentRightHandWeapon.weaponDetails.weaponShootPosition;
    }

    private void SetLeftHandWeapon(Weapon weapon)
    {
        currentLeftHandWeapon = weapon;

        // Set animator controller to the weapon animator
        weaponLeftHandAnimator.runtimeAnimatorController = currentLeftHandWeapon.weaponDetails.weaponAnimatorController;

        // Set current weapon sprite
        weaponLeftHandSpriteRenderer.sprite = currentLeftHandWeapon.weaponDetails.weaponSprite;

        // If the weapon has a polygon collider and a sprite then set it to the weapon sprite physics shape
        if (weaponLeftHandPolygonCollider2D != null && weaponLeftHandSpriteRenderer.sprite != null)
        {
            // Get sprite physics shape - this returns the sprite physics shape points as a list of Vector2s
            List<Vector2> spritePhysicsShapePointsList = new List<Vector2>();
            weaponLeftHandSpriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsShapePointsList);

            // Set polygon collider on weapon to pick up physics shape for sprite - set collider points to sprite physics shape points
            weaponLeftHandPolygonCollider2D.points = spritePhysicsShapePointsList.ToArray();
        }
    }

    private void DeselectLeftHandWeapon()
    {
        currentLeftHandWeapon = null;

        // Set current weapon sprite
        weaponLeftHandSpriteRenderer.sprite = null;

        // Set very small bounds for the Polygon Collider 2D
        Vector2[] smallBounds = new Vector2[]
        {
            new Vector2(0.1f, 0.1f),
            new Vector2(0.1f, -0.1f),
            new Vector2(-0.1f, -0.1f),
            new Vector2(-0.1f, 0.1f)
        };
        weaponLeftHandPolygonCollider2D.SetPath(0, smallBounds);

        weaponLeftHandAnimator.runtimeAnimatorController = null;
    }

    public ProjectileDetailsSO GetCurrentProjectile()
    {
        return currentRightHandWeapon.weaponDetails.weaponCurrentProjectile;
    }

    public Weapon GetCurrentRightHandWeapon()
    {
        return currentRightHandWeapon;
    }

    public Vector3 GetRightHandShootPosition()
    {
        return weaponRightHandShootPositionTransform.position;
    }

    public Vector3 GetRightHandShootEffectPosition()
    {
        return weaponRightHandEffectPositionTransform.position;
    }

    public void RemoveCurrentRightHandWeapon()
    {
        currentRightHandWeapon = null;
    }

    public Weapon GetCurrentLeftHandWeapon()
    {
        return currentLeftHandWeapon;
    }

    public void RemoveCurrentLeftHandWeapon()
    {
        currentLeftHandWeapon = null;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRightHandSpriteRenderer), weaponRightHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRightHandPolygonCollider2D), weaponRightHandPolygonCollider2D);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRightHandShootPositionTransform), weaponRightHandShootPositionTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponRightHandEffectPositionTransform), weaponRightHandEffectPositionTransform);
    }
#endif
    #endregion
}
