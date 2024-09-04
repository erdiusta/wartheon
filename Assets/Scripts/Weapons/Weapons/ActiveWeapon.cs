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
    [SerializeField] SpriteRenderer weaponMainHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the PolygonCollider2D on the child Weapon gameobject")]
    #endregion
    [SerializeField] PolygonCollider2D weaponMainHandPolygonCollider2D;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandShootPositionTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponEffectPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandEffectPositionTransform;

    [Header("LEFT HAND")]
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer on the child Weapon Left Hand gameobject")]
    #endregion
    [SerializeField] SpriteRenderer weaponOffHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the PolygonCollider2D on the child Weapon Left Hand gameobject")]
    #endregion
    [SerializeField] PolygonCollider2D weaponOffHandPolygonCollider2D;

    Player player;
    Transform offHandAnchorPosition;
    GameObject thirdHandGameObject;
    Vector3 startRightHandPosition;
    SetActiveWeaponEvent setActiveWeaponEvent;
    Animator playerAnimator;
    Animator weaponMainHandAnimator;
    Animator weaponOffHandAnimator;
    Weapon currentMainHandWeapon;
    Weapon currentOffHandWeapon;
    Transform offHandWeaponTransform;

    private void Awake()
    {
        player = GetComponent<Player>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        playerAnimator = GetComponent<Animator>();
        weaponMainHandAnimator = transform.GetChild(0).GetComponent<Animator>();

        if (player != null)
        {
            thirdHandGameObject = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).gameObject;
            weaponOffHandAnimator = transform.GetChild(1).GetComponent<Animator>();
            offHandAnchorPosition = weaponOffHandAnimator.transform;
            offHandWeaponTransform = offHandAnchorPosition.GetChild(0).GetChild(0);
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
        weaponMainHandAnimator.SetBool(Settings.isLeft, false);
    }

    private void SetActiveWeaponEvent_OnSetInactiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectMainHandWeapon();
    }

    private void SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetOffHandWeapon(setActiveWeaponEventArgs.weapon);
        weaponOffHandAnimator.SetBool(Settings.isLeft, true);
    }

    private void SetActiveWeaponEvent_OnSetInactiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectOffHandWeapon();
    }

    private void SetMainHandWeapon(Weapon weapon)
    {
        isSwitching = true;
        currentMainHandWeapon = weapon;

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
                thirdHandGameObject.SetActive(true);
                weaponOffHandAnimator.enabled = false;
                offHandAnchorPosition.gameObject.SetActive(false);
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

        // Set current weapon sprite
        weaponMainHandSpriteRenderer.sprite = currentMainHandWeapon.weaponDetails.weaponFrontSprite;

        // If the weapon has a polygon collider and a sprite then set it to the weapon sprite physics shape
        if (weaponMainHandPolygonCollider2D != null && weaponMainHandSpriteRenderer.sprite != null)
        {
            // Get sprite physics shape - this returns the sprite physics shape points as a list of Vector2s
            List<Vector2> spritePhysicsShapePointsList = new List<Vector2>();
            weaponMainHandSpriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsShapePointsList);

            // Set polygon collider on weapon to pick up physics shape for sprite - set collider points to sprite physics shape points
            weaponMainHandPolygonCollider2D.points = spritePhysicsShapePointsList.ToArray();
        }

        // Set weapon shoot position
        weaponMainHandShootPositionTransform.localPosition = currentMainHandWeapon.weaponDetails.weaponShootPosition;

        isSwitching = false;
    }

    private void SetOffHandWeapon(Weapon weapon)
    {
        currentOffHandWeapon = weapon;

        // Set animator controller to the weapon animator
        weaponOffHandAnimator.runtimeAnimatorController = currentOffHandWeapon.weaponDetails.weaponAnimatorController;

        // Set current weapon sprite
        weaponOffHandSpriteRenderer.sprite = currentOffHandWeapon.weaponDetails.weaponFrontSprite;

        // If the weapon has a polygon collider and a sprite then set it to the weapon sprite physics shape
        if (weaponOffHandPolygonCollider2D != null && weaponOffHandSpriteRenderer.sprite != null)
        {
            // Get sprite physics shape - this returns the sprite physics shape points as a list of Vector2s
            List<Vector2> spritePhysicsShapePointsList = new List<Vector2>();
            weaponOffHandSpriteRenderer.sprite.GetPhysicsShape(0, spritePhysicsShapePointsList);

            // Set polygon collider on weapon to pick up physics shape for sprite - set collider points to sprite physics shape points
            weaponOffHandPolygonCollider2D.points = spritePhysicsShapePointsList.ToArray();
        }

        if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
        {
            offHandWeaponTransform.localEulerAngles = Vector3.zero;
        }
    }

    private void DeselectMainHandWeapon()
    {
        currentMainHandWeapon = null;

        // Set current weapon sprite
        weaponMainHandSpriteRenderer.sprite = null;

        // Set very small bounds for the Polygon Collider 2D
        Vector2[] smallBounds = new Vector2[]
        {
            new Vector2(0.1f, 0.1f),
            new Vector2(0.1f, -0.1f),
            new Vector2(-0.1f, -0.1f),
            new Vector2(-0.1f, 0.1f)
        };
        weaponMainHandPolygonCollider2D.SetPath(0, smallBounds);

        weaponMainHandAnimator.runtimeAnimatorController = null;
    }

    private void DeselectOffHandWeapon()
    {
        currentOffHandWeapon = null;

        // Set current weapon sprite
        weaponOffHandSpriteRenderer.sprite = null;

        // Set very small bounds for the Polygon Collider 2D
        Vector2[] smallBounds = new Vector2[]
        {
            new Vector2(0.1f, 0.1f),
            new Vector2(0.1f, -0.1f),
            new Vector2(-0.1f, -0.1f),
            new Vector2(-0.1f, 0.1f)
        };
        weaponOffHandPolygonCollider2D.SetPath(0, smallBounds);

        weaponOffHandAnimator.runtimeAnimatorController = null;
    }

    public ProjectileDetailsSO GetCurrentProjectile() => currentMainHandWeapon.weaponDetails.weaponCurrentProjectile;

    public Weapon GetCurrentMainHandWeapon() => currentMainHandWeapon;

    public Vector3 GetRightHandShootPosition() => weaponMainHandShootPositionTransform.position;

    public Vector3 GetRightHandShootEffectPosition() => weaponMainHandEffectPositionTransform.position;

    public void RemoveCurrentRightHandWeapon()
    {
        currentMainHandWeapon = null;
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
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandSpriteRenderer), weaponMainHandSpriteRenderer);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandPolygonCollider2D), weaponMainHandPolygonCollider2D);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandShootPositionTransform), weaponMainHandShootPositionTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandEffectPositionTransform), weaponMainHandEffectPositionTransform);
    }
#endif
    #endregion
}
