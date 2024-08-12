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
        player = GameManager.Instance.GetPlayer();
        cooldownBarParent = cooldownBar.parent;
    }

    private void OnEnable()
    {
        player.setActiveWeaponEvent.OnSetActiveOffHandWeapon += SetActiveWeaponEvent_OnSetActiveOffHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveOffHandWeapon += SetActiveWeaponEvent_OnSetInactiveLeftHandWeapon;
        player.setActiveWeaponEvent.OnTwoHandWeaponEquipped += SetActiveWeaponEvent_OnTwoHandWeaponEquipped;
        player.setActiveWeaponEvent.OnOneHandWeaponEquipped += SetActiveWeaponEvent_OnOneHandWeaponEquipped;
        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;
    }

    private void OnDisable()
    {
        player.setActiveWeaponEvent.OnSetActiveOffHandWeapon -= SetActiveWeaponEvent_OnSetActiveOffHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveOffHandWeapon -= SetActiveWeaponEvent_OnSetInactiveLeftHandWeapon;
        player.setActiveWeaponEvent.OnTwoHandWeaponEquipped -= SetActiveWeaponEvent_OnTwoHandWeaponEquipped;
        player.setActiveWeaponEvent.OnOneHandWeaponEquipped -= SetActiveWeaponEvent_OnOneHandWeaponEquipped;
        player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;
    }

    private void Start()
    {
        // Update active weapon status on the UI
        SetActiveWeapon(player.activeWeapon.GetCurrentOffHandWeapon());
    }

    private void Update()
    {
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

    /// <summary>
    /// Clear active weapon event on the UI
    /// </summary>
    private void SetActiveWeaponEvent_OnSetInactiveLeftHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        MakeWeaponInactive();
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
    private void SetActiveWeaponEvent_OnOneHandWeaponEquipped(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        RemoveLockImage();
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
            if (cooldownRoutine != null)
            {
                StopCoroutine(cooldownRoutine);
            }

            if (weapon.weaponDetails.weaponClass != WeaponClass.Shield)
            {
                ResetWeaponCooldownBar(weapon);
            }
            else
            {
                cooldownBarParent.gameObject.SetActive(false);
            }

            UpdateActiveWeaponImage(weapon.weaponDetails);
            UpdateActiveWeaponName(weapon);
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

    /// <summary>
    /// Populate active weapon name
    private void UpdateActiveWeaponName(Weapon weapon)
    {
        weaponNameText.text = weapon.weaponDetails.weaponName;
    }

    private void MakeWeaponInactive()
    {
        weaponImage.sprite = noWeaponSprite;
        weaponNameText.text = "";
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
        while (currentWeapon.onCooldown)
        {
            // Set bar color as green
            barImage.color = Color.red;

            // Update cooldown bar
            float barFill = cooldownTimer / currentWeapon.weaponDetails.weaponCooldownDuration;

            // Update bar fill
            if (barFill > 0f)
            {
                cooldownBar.transform.localScale = new Vector3(barFill, 1f, 1f);
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
        cooldownTimer = currentWeapon.weaponDetails.weaponCooldownDuration;

        // Set bar color as green
        barImage.color = Color.green;

        // Set bar scale to 1
        cooldownBar.transform.localScale = new Vector3(1f, 1f, 1f);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponImage), weaponImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponNameText), weaponNameText);
    }
#endif
    #endregion
}