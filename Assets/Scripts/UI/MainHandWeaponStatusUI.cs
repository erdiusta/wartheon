using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MainHandWeaponStatusUI : MonoBehaviour
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
    [Tooltip("Populate with the TextMeshPro-Text component on the child ProjectileRemainingText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI projectileRemainingText;
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

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon += SetActiveWeaponEvent_OnSetActiveRightHandWeapon;
        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;
    }

    private void OnDisable()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveWeaponEvent_OnSetActiveRightHandWeapon;
        player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;
    }

    private void Start()
    {
        // Update active weapon status on the UI
        SetActiveWeapon(player.activeWeapon.GetCurrentMainHandWeapon());
    }

    private void Update()
    {
        if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0 && player.activeWeapon.GetCurrentMainHandWeapon().onPrecharge)
            {
                ResetWeaponCooldownBar(player.activeWeapon.GetCurrentMainHandWeapon());
                return;
            }
            if (player.activeWeapon.GetCurrentMainHandWeapon().onCooldown)
            {
                cooldownTimer -= Time.deltaTime;
            }
        }
    }

    /// <summary>
    /// Handle set active weapon event on the UI
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveRightHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetActiveWeapon(setActiveWeaponEventArgs.weapon);
    }

    /// <summary>
    /// Handle Weapon fired event on the UI
    /// </summary>
    private void WeaponFiredEvent_OnWeaponFired(WeaponFiredEvent weaponFiredEvent, WeaponFiredEventArgs weaponFiredEventArgs)
    {
        WeaponFired(weaponFiredEventArgs.weapon);
    }

    /// <summary>
    /// Weapon fired update UI
    /// </summary>
    private void WeaponFired(Weapon weapon)
    {
        UpdateProjectileText(weapon);
        UpdateCooldownBar(weapon);
    }

    /// <summary>
    /// Set the active weapon on the UI
    /// </summary>
    private void SetActiveWeapon(Weapon weapon)
    {
        if (cooldownRoutine != null)
        {
            StopCoroutine(cooldownRoutine);
        }

        ResetWeaponCooldownBar(weapon);
        UpdateActiveWeaponImage(weapon.weaponDetails);
        UpdateActiveWeaponName(weapon);
        UpdateProjectileText(weapon);
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

    /// <summary>
    /// Update the ammo remaining text on the UI
    /// </summary>
    private void UpdateProjectileText(Weapon weapon)
    {
        if (!weapon.weaponDetails.isMeleeWeapon)
        {
            if (weapon.weaponDetails.hasInfiniteProjectile)
            {
                projectileRemainingText.text = "";
            }
            else
            {
                projectileRemainingText.text = weapon.weaponRemainingProjectile.ToString() + " / " + weapon.weaponDetails.weaponProjectileCapacity.ToString();
            }
        }
        else
        {
            projectileRemainingText.text = "";
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
        HelperUtilities.ValidateCheckNullValue(this, nameof(projectileRemainingText), projectileRemainingText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponNameText), weaponNameText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(cooldownBar), cooldownBar);
        HelperUtilities.ValidateCheckNullValue(this, nameof(barImage), barImage);
    }
#endif
    #endregion
}