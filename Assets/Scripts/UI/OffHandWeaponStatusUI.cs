using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class OffHandWeaponStatusUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("Populate with image component on the child WeaponImage gameobject")]
    #endregion Tooltip
    [SerializeField] Image weaponImage;
    #region Tooltip
    [Tooltip("Populate with blank 1x1 as no weapon sprite on the child WeaponImage gameobject")]
    #endregion Tooltip
    [SerializeField] Sprite noWeaponSprite;
    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child WeaponNameText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI weaponNameText;
    #region Tooltip
    [Tooltip("Populate with the RectTransform of the child gameobject CooldownBar")]
    #endregion Tooltip
    [SerializeField] Transform cooldownBar;
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject BarImage")]
    #endregion Tooltip
    [SerializeField] Image barImage;

    Player player;
    Coroutine cooldownRoutine;
    float cooldownTimer;
    Transform cooldownBarParent;

    private void Awake()
    {
        cooldownBarParent = cooldownBar.parent;
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || player?.activeWeapon == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetPlayer();
            yield return null;
        }

        Subscribe();
        SetActiveWeapon(player.activeWeapon.GetCurrentOffHandWeapon());
    }

    private void Subscribe()
    {
        player.setActiveWeaponEvent.OnSetActiveOffHandWeapon += SetActiveWeaponEvent_OnSetActiveOffHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveOffHandWeapon += SetActiveWeaponEvent_OnSetInactiveOffHandWeapon;
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon += SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveMainHandWeapon += SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        player.setActiveWeaponEvent.OnTwoHandWeaponEquipped += SetActiveWeaponEvent_OnTwoHandWeaponEquipped;
        player.setActiveWeaponEvent.OnOneHandWeaponEquipped += SetActiveWeaponEvent_OnOneHandWeaponEquipped;
        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;
    }

    private void Unsubscribe()
    {
        player.setActiveWeaponEvent.OnSetActiveOffHandWeapon -= SetActiveWeaponEvent_OnSetActiveOffHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveOffHandWeapon -= SetActiveWeaponEvent_OnSetInactiveOffHandWeapon;
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveMainHandWeapon -= SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        player.setActiveWeaponEvent.OnTwoHandWeaponEquipped -= SetActiveWeaponEvent_OnTwoHandWeaponEquipped;
        player.setActiveWeaponEvent.OnOneHandWeaponEquipped -= SetActiveWeaponEvent_OnOneHandWeaponEquipped;
        player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;
    }

    private void Update()
    {
        if (player == null || player?.activeWeapon == null || !player.IsLocal) return;

        if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
        {
            if (player.activeWeapon.GetCurrentOffHandWeapon().onCooldown)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Handle set active weapon event on the UI
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetActiveWeapon(setActiveWeaponEventArgs.weapon);
    }

    private void SetActiveWeaponEvent_OnSetInactiveOffHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        MakeWeaponInactive();
        cooldownBarParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Remove unnecessary elements in case of no off-hand or shield
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (setActiveWeaponEventArgs.weapon.weaponDetails.wieldType == WieldType.TwoHanded)
        {
            MakeWeaponInactive();
            cooldownBarParent.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Remove lock placeholder if right hand hold two-handed weapon
    /// </summary>
    private void SetActiveWeaponEvent_OnSetInactiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (!setActiveWeaponEventArgs.isWeaponSwapping)
        {
            RemoveLockImage();
        }
    }

    /// <summary>
    /// Place lock placeholder if right hand hold two-handed weapon
    /// </summary>
    private void SetActiveWeaponEvent_OnTwoHandWeaponEquipped(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        DisplayLockImage();
    }

    /// <summary>
    /// Remove lock place holder if right hand hold one-handed weapon
    /// </summary>
    private void SetActiveWeaponEvent_OnOneHandWeaponEquipped(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (!setActiveWeaponEventArgs.isWeaponSwapping)
        {
            RemoveLockImage();
        }
    }

    private void WeaponFiredEvent_OnWeaponFired(WeaponFiredEvent weaponFiredEvent, WeaponFiredEventArgs weaponFiredEventArgs)
    {
        if (weaponFiredEventArgs.weapon.weaponDetails.weaponClass != WeaponClass.Shield && !weaponFiredEventArgs.mainHand)
        {
            WeaponFired(weaponFiredEventArgs.weapon);
        }
    }

    /// <summary>
    /// Weapon fired update UI
    /// </summary>
    private void WeaponFired(Weapon weapon)
    {
        //UpdateProjectileText(weapon);
        UpdateCooldownBar(weapon);
    }

    /// <summary>
    /// Set the active weapon on the UI
    /// </summary>
    private void SetActiveWeapon(Weapon weapon)
    {
        if (weapon != null)
        {
            UpdateActiveWeaponImage(weapon.weaponDetails);

            if (weapon.weaponDetails.weaponClass != WeaponClass.Shield) // If weapon is dual-wield
            {
                cooldownBarParent.gameObject.SetActive(true);
                ResetWeaponCooldownBar(weapon);
            }
            else // If weapon is a shield
            {
                cooldownBarParent.gameObject.SetActive(false);
            }

            if (cooldownRoutine != null)
            {
                StopCoroutine(cooldownRoutine);
            }
        }
        else
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    MakeWeaponInactive();
                    DisplayLockImage();
                }
            }
        }
    }

    /// <summary>
    /// Populate with the lock image
    /// </summary>
    private void DisplayLockImage()
    {
        cooldownBarParent.gameObject.SetActive(false);
        weaponImage.sprite = GameResources.Instance.lockIcon;
    }

    /// <summary>
    /// Remove lock image
    /// </summary>
    private void RemoveLockImage()
    {
        if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
        {
            cooldownBarParent.gameObject.SetActive(true);
        }
        else
        {
            cooldownBarParent.gameObject.SetActive(false);
        }

        weaponImage.sprite = noWeaponSprite;
    }

    /// <summary>
    /// Populate active weapon image
    /// </summary>
    private void UpdateActiveWeaponImage(WeaponDetailsSO weaponDetails)
    {
        weaponImage.sprite = weaponDetails.weaponFrontSprite;
    }

    private void MakeWeaponInactive()
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType != WieldType.TwoHanded)
            {
                weaponImage.sprite = noWeaponSprite;
            }
        }

        if (player.activeWeapon.GetCurrentOffHandWeapon() == null)
        {
            weaponImage.sprite = noWeaponSprite;
        }
    }

    /// <summary>
    /// Update cooldown bar
    /// </summary>
    void UpdateCooldownBar(Weapon currentWeapon)
    {
        cooldownRoutine = StartCoroutine(CooldownRoutine(currentWeapon));
    }

    /// <summary>
    /// Run cooldown routine
    /// </summary>
    IEnumerator CooldownRoutine(Weapon currentWeapon)
    {
        if (currentWeapon.itemSlotStatus == ItemSlotStatus.OffHand)
        {
            cooldownBarParent.gameObject.SetActive(true);
        }

        while (currentWeapon.onCooldown)
        {
            // Update cooldown bar
            float barFill = currentWeapon.weaponDetails.isMeleeWeapon ? cooldownTimer / 
                (currentWeapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalAttackRatingModifier)):
                cooldownTimer / currentWeapon.weaponDetails.weaponCooldownDuration;

            // Update bar fill
            if (barFill > 0f)
            {
                barImage.color = new Color(1f, 1f, 1f, 0.4f);
                barImage.transform.localScale = new Vector3(barFill, 1f, 1f);
            }

            yield return null;
        }

        ResetWeaponCooldownBar(currentWeapon);
    }

    /// <summary>
    /// Reset the weapon cooldown bar on the UI
    /// </summary>
    private void ResetWeaponCooldownBar(Weapon currentWeapon)
    {
        cooldownTimer = currentWeapon.weaponDetails.weaponCooldownDuration * (1 - player.additionalAttackRatingModifier);

        // Set bar scale to 1
        barImage.transform.localScale = new Vector3(1f, 1f, 1f);
        barImage.color = new Color(1f, 1f, 1f, 0f);
    }
}