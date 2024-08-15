using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ActiveWeapon : MonoBehaviour
{
    [HideInInspector] public bool isSwitching;

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

    Player player;
    Transform leftHandAnchorPosition;
    GameObject thirdHandGameObject;
    Vector3 startRightHandPosition;
    SetActiveWeaponEvent setActiveWeaponEvent;
    Animator playerAnimator;
    Animator weaponRightHandAnimator;
    Animator weaponLeftHandAnimator;
    Weapon currentRightHandWeapon;
    Weapon currentOffHandWeapon;

    private void Awake()
    {
        player = GetComponent<Player>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        playerAnimator = GetComponent<Animator>();
        weaponRightHandAnimator = transform.GetChild(0).GetComponent<Animator>();

        if (player != null)
        {
            thirdHandGameObject = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).gameObject;
            weaponLeftHandAnimator = transform.GetChild(1).GetComponent<Animator>();
            leftHandAnchorPosition = weaponLeftHandAnimator.transform;
        }
    }

    private void OnEnable()
    {
        setActiveWeaponEvent.OnSetActiveMainHandWeapon += SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon;
        setActiveWeaponEvent.OnSetInactiveMainHandWeapon += SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        setActiveWeaponEvent.OnSetActiveOffHandWeapon += SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon;
        setActiveWeaponEvent.OnSetInactiveOffHandWeapon += SetActiveWeaponEvent_OnSetInactiveOffHandWeapon;
    }

    private void OnDisable()
    {
        setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon;
        setActiveWeaponEvent.OnSetInactiveMainHandWeapon -= SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        setActiveWeaponEvent.OnSetActiveOffHandWeapon -= SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon;
        setActiveWeaponEvent.OnSetInactiveOffHandWeapon -= SetActiveWeaponEvent_OnSetInactiveOffHandWeapon;
    }

    private void SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetMainHandWeapon(setActiveWeaponEventArgs.weapon);
        weaponRightHandAnimator.SetBool(Settings.isLeft, false);
    }

    private void SetActiveWeaponEvent_OnSetInactiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectMainHandWeapon();
    }

    private void SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetOffHandWeapon(setActiveWeaponEventArgs.weapon);
        weaponLeftHandAnimator.SetBool(Settings.isLeft, true);
    }

    private void SetActiveWeaponEvent_OnSetInactiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectOffHandWeapon();
    }

    private void SetMainHandWeapon(Weapon weapon)
    {
        isSwitching = true;
        currentRightHandWeapon = weapon;

        if (player != null)
        {
            // If equipped weapon is two-handed, temporarily disable animator and change the position and enable again
            if (currentRightHandWeapon.weaponDetails.weaponClass == WeaponClass.Bow && currentRightHandWeapon.weaponDetails.weaponName != "Crossbow")
            {
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.bowRuntimeAnimatorController;
                thirdHandGameObject.SetActive(true);
                weaponLeftHandAnimator.enabled = false;
                leftHandAnchorPosition.gameObject.SetActive(false);
            }
            else if (currentRightHandWeapon.weaponDetails.weaponClass == WeaponClass.Staff)
            {
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.staffRuntimeAnimatorController;
                thirdHandGameObject.SetActive(true);
                weaponLeftHandAnimator.enabled = false;
                leftHandAnchorPosition.gameObject.SetActive(false);
            }
            else if (currentRightHandWeapon.weaponDetails.wieldType == WieldType.TwoHanded && currentRightHandWeapon.weaponDetails.weaponName != "Crossbow")
            {
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.twoHandRuntimeAnimatorController;
                thirdHandGameObject.SetActive(true);
                weaponLeftHandAnimator.enabled = false;
                leftHandAnchorPosition.gameObject.SetActive(false);
            }
            // If equipped one - hand, revert position and animator settings to default
            else
            {
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
                player.aimWeapon.rightHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
                playerAnimator.runtimeAnimatorController = player.playerDetails.oneHandRuntimeAnimatorController;
                thirdHandGameObject.SetActive(false);
                leftHandAnchorPosition.gameObject.SetActive(true);
                weaponLeftHandAnimator.enabled = true;
            }
        }

        // Set animator controller to the weapon animator
        weaponRightHandAnimator.runtimeAnimatorController = currentRightHandWeapon.weaponDetails.weaponAnimatorController;

        // Set current weapon sprite
        weaponRightHandSpriteRenderer.sprite = currentRightHandWeapon.weaponDetails.weaponFrontSprite;

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

        isSwitching = false;
    }

    private void SetOffHandWeapon(Weapon weapon)
    {
        currentOffHandWeapon = weapon;

        // Set animator controller to the weapon animator
        weaponLeftHandAnimator.runtimeAnimatorController = currentOffHandWeapon.weaponDetails.weaponAnimatorController;

        // Set current weapon sprite
        weaponLeftHandSpriteRenderer.sprite = currentOffHandWeapon.weaponDetails.weaponFrontSprite;

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

    private void DeselectMainHandWeapon()
    {
        currentRightHandWeapon = null;

        // Set current weapon sprite
        weaponRightHandSpriteRenderer.sprite = null;

        // Set very small bounds for the Polygon Collider 2D
        Vector2[] smallBounds = new Vector2[]
        {
            new Vector2(0.1f, 0.1f),
            new Vector2(0.1f, -0.1f),
            new Vector2(-0.1f, -0.1f),
            new Vector2(-0.1f, 0.1f)
        };
        weaponRightHandPolygonCollider2D.SetPath(0, smallBounds);

        weaponRightHandAnimator.runtimeAnimatorController = null;
    }

    private void DeselectOffHandWeapon()
    {
        currentOffHandWeapon = null;

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

    public Weapon GetCurrentMainHandWeapon()
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

    public Weapon GetCurrentOffHandWeapon()
    {
        return currentOffHandWeapon;
    }

    public void RemoveCurrentLeftHandWeapon()
    {
        currentOffHandWeapon = null;
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
