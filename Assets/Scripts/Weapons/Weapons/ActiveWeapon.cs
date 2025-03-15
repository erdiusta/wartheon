using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ActiveWeapon : MonoBehaviour
{
    [HideInInspector] public bool isSwitching;
    [HideInInspector] public Weapon weaponToBeDropped; // Cache it for clearing lock icon two-handed dropping issues 

    [Header("MAIN HAND")]
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer on the child Weapon gameobject")]
    #endregion
    [SerializeField] SpriteRenderer weaponMainHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandShootPositionTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponEffectPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandEffectPositionTransform;

    [Header("OFF-HAND")]
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer on the child Weapon Left Hand gameobject")]
    #endregion
    [SerializeField] SpriteRenderer weaponOffHandSpriteRenderer;

    Player player;
    Enemy enemy;
    Transform mainHandAnchorPosition;
    Transform offHandAnchorPosition;
    GameObject thirdHandGameObject;
    Vector3 startRightHandPosition;
    SetActiveWeaponEvent setActiveWeaponEvent;
    Animator playerAnimator;
    Animator weaponMainHandAnimator;
    Animator weaponOffHandAnimator;
    Weapon currentMainHandWeapon;
    Weapon currentOffHandWeapon;

    private void Awake()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        playerAnimator = GetComponent<Animator>();
        weaponMainHandAnimator = transform.GetChild(0).GetComponent<Animator>();

        if (player != null)
        {
            mainHandAnchorPosition = transform.GetChild(0);
            offHandAnchorPosition = transform.GetChild(1);

            thirdHandGameObject = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(3).gameObject;
            weaponOffHandAnimator = transform.GetChild(1).GetComponent<Animator>();
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

    private void Start()
    {
        if (player != null)
        {
            playerAnimator.runtimeAnimatorController = player.playerDetails.bodyRuntimeAnimatorController;
        }
    }

    private void SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetMainHandWeapon(setActiveWeaponEventArgs.weapon);

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateWeaponHandlingAndCriticalValues();
        player?.UpdateBlockAndEvasivenessValues();
    }

    private void SetActiveWeaponEvent_OnSetInactiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        DeselectMainHandWeapon();

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateWeaponHandlingAndCriticalValues();
        player?.UpdateBlockAndEvasivenessValues();
    }

    private void SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetOffHandWeapon(setActiveWeaponEventArgs.weapon);

        // Update new weapon values
        player.UpdateDamageValues();
        player.UpdateWeaponHandlingAndCriticalValues();
        player.UpdateBlockAndEvasivenessValues();
    }

    private void SetActiveWeaponEvent_OnSetInactiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectOffHandWeapon();

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateWeaponHandlingAndCriticalValues();
        player?.UpdateBlockAndEvasivenessValues();
    }

    private void SetMainHandWeapon(Weapon weapon)
    {
        isSwitching = true;
        currentMainHandWeapon = weapon;
        weaponToBeDropped = weapon; // for removing lock icon during two-handed weapon drop issue

        if (player != null)
        {
            // Reset transform values
            player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
            player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
            player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);

            if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Bow)
            {
                OffHandWeaponRemoveCheck(); // Remove possible off-hand during weapon switch
                ResetAnimationParameters();

                offHandAnchorPosition.gameObject.SetActive(false);

                weaponOffHandAnimator.enabled = false;
                weaponMainHandAnimator.enabled = true;

                playerAnimator.SetFloat(Settings.mainPosture, 2); // Bow posture
                playerAnimator.SetBool(Settings.isMeleeWeapon, false);

                weaponMainHandAnimator.runtimeAnimatorController = currentMainHandWeapon.weaponDetails.weaponAnimatorController;
            }
            else if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Staff)
            {
                OffHandWeaponRemoveCheck(); // Remove possible off-hand during weapon switch
                ResetAnimationParameters();

                weaponOffHandAnimator.enabled = false;
                weaponMainHandAnimator.enabled = true;

                playerAnimator.SetFloat(Settings.mainPosture, 1); // Two-handed posture
                playerAnimator.SetBool(Settings.isMeleeWeapon, false);

                weaponMainHandAnimator.runtimeAnimatorController = currentMainHandWeapon.weaponDetails.weaponAnimatorController;
            }
            else if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Crossbow)
            {
                OffHandWeaponRemoveCheck(); // Remove possible off-hand during weapon switch
                ResetAnimationParameters();

                offHandAnchorPosition.gameObject.SetActive(true);

                weaponOffHandAnimator.enabled = false;
                weaponMainHandAnimator.enabled = true;

                playerAnimator.SetFloat(Settings.mainPosture, 1); // Two-handed posture
                playerAnimator.SetBool(Settings.isMeleeWeapon, false);

                weaponMainHandAnimator.runtimeAnimatorController = currentMainHandWeapon.weaponDetails.weaponAnimatorController;
            }
            else if (currentOffHandWeapon == null) // This means that's not a dual wield nor shield
            {
                OffHandWeaponRemoveCheck(); // Remove possible off-hand during weapon switch
                ResetAnimationParameters();

                playerAnimator.SetBool(Settings.isDualWield, false);
                playerAnimator.SetBool(Settings.isShielded, false);

                offHandAnchorPosition.gameObject.SetActive(true);
                playerAnimator.SetBool(Settings.isMeleeWeapon, true);

                weaponOffHandAnimator.enabled = false;
                weaponMainHandAnimator.enabled = false;

                if (currentMainHandWeapon.weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    playerAnimator.SetFloat(Settings.mainPosture, 1); // Two handed posture

                    if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Spear)
                    {
                        playerAnimator.SetInteger(Settings.smearSize, 0); // Reset smear values as this motion won't use swings
                        playerAnimator.SetInteger(Settings.thrustSize, 2); // Long size thrust for two handed spear
                    }
                    else
                    {
                        playerAnimator.SetInteger(Settings.smearSize, 3); // If weapon is two-handed swing weapon
                        playerAnimator.SetInteger(Settings.thrustSize, 0); // Reset thrust values
                    }

                    //return; // Exit method to prevent further one handed checks
                }

                ThrustSwingAnimationCheck();
            }
            else
            {
                offHandAnchorPosition.gameObject.SetActive(true);

                OffHandWeaponRemoveCheck();  // Remove possible off-hand during weapon switch

                playerAnimator.SetBool(Settings.isMeleeWeapon, true);

                ThrustSwingAnimationCheck();
            }
        }

        // Set current weapon sprite
        weaponMainHandSpriteRenderer.sprite = currentMainHandWeapon.weaponDetails.weaponFrontSprite;

        weaponMainHandShootPositionTransform.localPosition = currentMainHandWeapon.weaponDetails.weaponRightShootPosition;

        isSwitching = false;
    }

    private void OffHandWeaponRemoveCheck()
    {
        if (currentOffHandWeapon != null)
        {
            DeselectOffHandWeapon();

            // Update new weapon values
            player?.UpdateDamageValues();
            player?.UpdateWeaponHandlingAndCriticalValues();
            player?.UpdateBlockAndEvasivenessValues();
        }
    }

    private void ThrustSwingAnimationCheck()
    {
        if (currentMainHandWeapon.weaponDetails.wieldType == WieldType.TwoHanded)
        {
            playerAnimator.SetFloat(Settings.mainPosture, 1); // Two handed posture
        }
        else
        {
            playerAnimator.SetFloat(Settings.mainPosture, 0); // One handed posture
        }

        if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Spear) // If weapon is a spear, thrust motions should be enabled
        {
            playerAnimator.SetInteger(Settings.smearSize, 0); // Reset smear values as this motion won't use swings
            playerAnimator.SetInteger(Settings.thrustSize, 1);
        }
        else
        {
            playerAnimator.SetInteger(Settings.thrustSize, 0);

            if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Dagger || currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw)
            {
                playerAnimator.SetInteger(Settings.smearSize, 1); // Set smear size to 1 for dagger or claws
            }
            else
            {
                playerAnimator.SetInteger(Settings.smearSize, 2); // Set smear size to 2 for other one-handed melee weapons
            }
        }
    }

    private void SetOffHandWeapon(Weapon weapon)
    {
        currentOffHandWeapon = weapon;

        if (currentOffHandWeapon.weaponDetails.weaponClass == WeaponClass.Shield)
        {
            weaponOffHandAnimator.enabled = true;

            playerAnimator.SetBool(Settings.isShielded, true);
            playerAnimator.SetBool(Settings.isDualWield, false);

            // Set animator controller to the weapon animator
            weaponOffHandAnimator.runtimeAnimatorController = currentOffHandWeapon.weaponDetails.weaponAnimatorController;
        }
        else
        {
            weaponOffHandAnimator.enabled = false;

            playerAnimator.SetBool(Settings.isShielded, false);
            playerAnimator.SetBool(Settings.isDualWield, true);
        }

        // Set current weapon sprite
        weaponOffHandSpriteRenderer.sprite = currentOffHandWeapon.weaponDetails.weaponFrontSprite;
    }

    private void DeselectMainHandWeapon()
    {
        offHandAnchorPosition.gameObject.SetActive(true);
        playerAnimator.SetFloat(Settings.mainPosture, 0f); // Reset posture for non-armed situation
        playerAnimator.SetBool(Settings.isMeleeWeapon, true);

        DeselectOffHandWeapon(); // As main hand is empty, empty off-hand as well to be safe-side

        player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
        player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;

        currentMainHandWeapon = null;

        // Set current weapon sprite and animator
        weaponMainHandAnimator.runtimeAnimatorController = null; // REMOVE ANIMATOR BEFORE SPRITE !
        weaponMainHandSpriteRenderer.sprite = null;
    }

    private void DeselectOffHandWeapon()
    {
        currentOffHandWeapon = null;

        playerAnimator.SetBool(Settings.isShielded, false);
        playerAnimator.SetBool(Settings.isDualWield, false);

        // Set current weapon sprite and animator
        weaponOffHandAnimator.runtimeAnimatorController = null; // REMOVE ANIMATOR BEFORE SPRITE !
        weaponOffHandSpriteRenderer.sprite = null;
    }

    private void ResetAnimationParameters()
    {
        playerAnimator.SetInteger(Settings.smearSize, 0);
        playerAnimator.SetInteger(Settings.thrustSize, 0);
        playerAnimator.SetBool(Settings.isShielded, false);
        playerAnimator.SetBool(Settings.isDualWield, false);
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
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandShootPositionTransform), weaponMainHandShootPositionTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandEffectPositionTransform), weaponMainHandEffectPositionTransform);
    }
#endif
    #endregion
}
