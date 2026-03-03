using UnityEngine;

[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ActiveWeapon : PlayerBoundBehaviour
{
    [HideInInspector] public bool isSwitching;
    [HideInInspector] public Weapon weaponToBeDropped; // Cache it for clearing lock icon two-handed dropping issues 

    protected override PlayerBindMode BindMode => PlayerBindMode.Both;

    [Header("MAIN HAND")]
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the SpriteRenderer on the child Weapon gameobject")]
    #endregion
    [SerializeField] SpriteRenderer weaponMainHandSpriteRenderer;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandShootPositionUpTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandShootPositionRightTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandShootPositionDownTransform;
    #region Tooltip
    [Tooltip("Populate with the Transform on the WeaponShootPosition gameobject")]
    #endregion
    [SerializeField] Transform weaponMainHandShootPositionLeftTransform;
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

    Enemy enemy;
    Transform mainHandAnchorPosition;
    Transform offHandAnchorPosition;
    SetActiveWeaponEvent setActiveWeaponEvent;
    Animator playerAnimator;
    Animator weaponMainHandAnimator;
    Animator weaponOffHandAnimator;
    Weapon currentMainHandWeapon;
    Weapon currentOffHandWeapon;

    int playerReadyCount = 0;
    int setMainWeaponCount = 0;

    private void Awake()
    {
        // Only valid for enemy case
        enemy = GetComponent<Enemy>();
        if(enemy != null) setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if(player == null && enemy != null) setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon;
        if (player != null && enemy == null) Unsubscribe();
    }

    private void Subscribe()
    {
        setActiveWeaponEvent.OnSetActiveMainHandWeapon += SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon;
        setActiveWeaponEvent.OnSetInactiveMainHandWeapon += SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        setActiveWeaponEvent.OnSetActiveOffHandWeapon += SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon;
        setActiveWeaponEvent.OnSetInactiveOffHandWeapon += SetActiveWeaponEvent_OnSetInactiveOffHandWeapon;
    }

    private void Unsubscribe()
    {
        setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon;
        setActiveWeaponEvent.OnSetInactiveMainHandWeapon -= SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        setActiveWeaponEvent.OnSetActiveOffHandWeapon -= SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon;
        setActiveWeaponEvent.OnSetInactiveOffHandWeapon -= SetActiveWeaponEvent_OnSetInactiveOffHandWeapon;
    }

    protected override void HandlePlayerReady(Player player, PlayerDetailsSO details)
    {
        // Don't go further for enemy prefabs
        if (enemy != null)
        {
            setActiveWeaponEvent.OnSetActiveMainHandWeapon += SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon;
            return;
        }

        PopulateComponents();
        Subscribe();

        if (player.IsLocal)
        {
            //player.ReplayActiveWeaponsForListeners(index: 1);
        }
    }

    private void PopulateComponents()
    {
        playerAnimator = GetComponent<Animator>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
        weaponMainHandAnimator = transform.GetChild(0).GetComponent<Animator>();
        mainHandAnchorPosition = transform.GetChild(0);
        offHandAnchorPosition = transform.GetChild(2);
        weaponOffHandAnimator = offHandAnchorPosition.GetComponent<Animator>();
    }

    private void SetActiveMainWeaponEvent_OnSetActiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetMainHandWeapon(setActiveWeaponEventArgs.weapon, setActiveWeaponEventArgs.weaponSetIndex, setActiveWeaponEventArgs.onStart,
            setActiveWeaponEventArgs.onSwitch);

        if (enemy != null) return;

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateArmorValues();
        player?.UpdateAttackRatingAndCriticalValues();
        player?.UpdateBlockAndDodgeValues();
    }

    private void SetActiveWeaponEvent_OnSetInactiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        DeselectMainHandWeapon(setActiveWeaponEventArgs.isWeaponSwapping, setActiveWeaponEventArgs.weaponSetIndex, setActiveWeaponEventArgs.onSwitch);

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateArmorValues();
        player?.UpdateAttackRatingAndCriticalValues();
        player?.UpdateBlockAndDodgeValues();
    }

    private void SetActiveOffHandWeaponEvent_OnSetActiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, 
        SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetOffHandWeapon(setActiveWeaponEventArgs.weapon, setActiveWeaponEventArgs.onStart);

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateArmorValues();
        player?.UpdateAttackRatingAndCriticalValues();
        player?.UpdateBlockAndDodgeValues();
    }

    private void SetActiveWeaponEvent_OnSetInactiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DeselectOffHandWeapon();

        // Update new weapon values
        player?.UpdateDamageValues();
        player?.UpdateArmorValues();
        player?.UpdateAttackRatingAndCriticalValues();
        player?.UpdateBlockAndDodgeValues();
    }

    private void SetMainHandWeapon(Weapon weapon, int weaponSetIndex, bool onStart, bool onSwitch)
    {
        currentMainHandWeapon = weapon;

        if (enemy != null) return; // Equip weapon for ranged enemies, then exit

        isSwitching = true;
        weaponToBeDropped = weapon; // for removing lock icon during two-handed weapon drop issue

        Weapon equippedWeapon; 

        if (player != null)
        {
            equippedWeapon = onStart ? currentMainHandWeapon : player.weaponSlotSetArray[weaponSetIndex - 1][0];

            // Reset transform values
            player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localPosition = Vector3.zero;
            player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).eulerAngles = Vector3.zero;
            player.aimWeapon.mainHandWeaponAnchorPointTransform.GetChild(0).localScale = new Vector3(1f, 1f, 1f);

            if (equippedWeapon.weaponDetails.weaponClass == WeaponClass.Bow)
            {
                OffHandWeaponRemoveCheck(weaponSetIndex); // Remove possible off-hand during weapon switch
                ResetAnimationParameters();

                offHandAnchorPosition.gameObject.SetActive(false);

                weaponOffHandAnimator.enabled = false;
                weaponMainHandAnimator.enabled = true;

                playerAnimator.SetFloat(Settings.mainPosture, 2); // Bow posture
                playerAnimator.SetBool(Settings.isMeleeWeapon, false);

                weaponMainHandAnimator.runtimeAnimatorController = currentMainHandWeapon.weaponDetails.weaponAnimatorController;
            }
            else if (equippedWeapon.weaponDetails.weaponClass == WeaponClass.Staff)
            {
                OffHandWeaponRemoveCheck(weaponSetIndex); // Remove possible off-hand during weapon switch
                ResetAnimationParameters();

                weaponOffHandAnimator.enabled = false;
                weaponMainHandAnimator.enabled = true;

                playerAnimator.SetFloat(Settings.mainPosture, 1); // Two-handed posture
                playerAnimator.SetBool(Settings.isMeleeWeapon, false);

                weaponMainHandAnimator.runtimeAnimatorController = currentMainHandWeapon.weaponDetails.weaponAnimatorController;
            }
            else if (equippedWeapon.weaponDetails.weaponClass == WeaponClass.Crossbow)
            {
                OffHandWeaponRemoveCheck(weaponSetIndex); // Remove possible off-hand during weapon switch
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
                OffHandWeaponRemoveCheck(weaponSetIndex); // Remove possible off-hand during weapon switch
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

                OffHandWeaponRemoveCheck(weaponSetIndex);  // Remove possible off-hand during weapon switch

                playerAnimator.SetBool(Settings.isMeleeWeapon, true);

                ThrustSwingAnimationCheck();
            }
        }

        // Set current weapon sprite
        weaponMainHandSpriteRenderer.sprite = currentMainHandWeapon.weaponDetails.weaponFrontSprite;

        weaponMainHandShootPositionUpTransform.localPosition = currentMainHandWeapon.weaponDetails.weaponRightShootPosition;

        isSwitching = false;
    }

    private void OffHandWeaponRemoveCheck(int weaponSetIndex)
    {
        if (currentOffHandWeapon != null && currentMainHandWeapon.weaponDetails.wieldType != WieldType.OneHanded)
        {
            DeselectOffHandWeapon();

            // Update new weapon values
            player.UpdateDamageValues();
            player.UpdateArmorValues();
            player.UpdateAttackRatingAndCriticalValues();
            player.UpdateBlockAndDodgeValues();
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
            if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Dagger)
            {
                playerAnimator.SetInteger(Settings.thrustSize, 0);
                playerAnimator.SetInteger(Settings.smearSize, 1); // Set smear size to 1 for dagger
            }
            else if (currentMainHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw)
            {
                playerAnimator.SetInteger(Settings.smearSize, 0);
                playerAnimator.SetInteger(Settings.thrustSize, 1); // Set smear size to 1 for claws
            }
            else
            {
                playerAnimator.SetInteger(Settings.smearSize, 2); // Set smear size to 2 for other one-handed melee weapons
            }
        }
    }

    private void SetOffHandWeapon(Weapon weapon, bool onStart)
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

    private void DeselectMainHandWeapon(bool isWeaponSwapped, int weaponSetIndex, bool onSwitch)
    {
        offHandAnchorPosition.gameObject.SetActive(true);
        playerAnimator.SetFloat(Settings.mainPosture, 0f); // Reset posture for non-armed situation
        playerAnimator.SetBool(Settings.isMeleeWeapon, true);

        if (!isWeaponSwapped)
        {
            DeselectOffHandWeapon(); // As main hand is empty, empty off-hand as well to be safe-side
        }

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

    public void RefreshFromNetworkState()
    {
        var weaponState = GetComponent<PlayerWeaponState>();

        Weapon main = currentMainHandWeapon;
        Weapon off = currentOffHandWeapon;
    }

    public ProjectileDetailsSO GetCurrentProjectile() => currentMainHandWeapon.weaponDetails.weaponCurrentProjectile;

    public Weapon GetCurrentMainHandWeapon() => currentMainHandWeapon;

    public Vector3 GetMainHandShootPositionUp() => weaponMainHandShootPositionUpTransform.position;

    public Vector3 GetMainHandShootPositionRight() => weaponMainHandShootPositionRightTransform.position;

    public Vector3 GetMainHandShootPositionDown() => weaponMainHandShootPositionDownTransform.position;

    public Vector3 GetMainHandShootPositionLeft() => weaponMainHandShootPositionLeftTransform.position;

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
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandShootPositionUpTransform), weaponMainHandShootPositionUpTransform);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponMainHandEffectPositionTransform), weaponMainHandEffectPositionTransform);
    }
#endif
    #endregion
}
