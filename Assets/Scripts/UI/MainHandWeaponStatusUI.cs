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
    [Tooltip("Populate with blank 1x1 as no weapon sprite on the child WeaponImage gameobject")]
    #endregion Tooltip
    [SerializeField] Sprite noWeaponSprite;
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
    Transform cooldownBarParent;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
        cooldownBarParent = cooldownBar.parent;
    }

    private void OnEnable()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon += SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveMainHandWeapon += SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;
    }

    private void OnDisable()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeapon -= SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveMainHandWeapon -= SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;
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
            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().firingStoppedPrematurelyIfWeaponIsPrecharged)
                {
                    ResetWeaponCooldownBar(player.activeWeapon.GetCurrentMainHandWeapon(), player.activeWeapon.GetCurrentMainHandWeapon().firingStoppedPrematurelyIfWeaponIsPrecharged);
                    return;
                }

                if (player.activeWeapon.GetCurrentMainHandWeapon().onPrecharge)
                {
                    {
                        ResetWeaponCooldownBar(player.activeWeapon.GetCurrentMainHandWeapon());
                        return;
                    }
                }
            }

            if (player.activeWeapon.GetCurrentMainHandWeapon().onCooldown)
            {
                cooldownTimer -= Time.deltaTime;
            }
            else
            {
                ResetWeaponCooldownBar(player.activeWeapon.GetCurrentMainHandWeapon());
            }
        }
    }

    /// <summary>
    /// Handle set active weapon event on the UI
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        SetActiveWeapon(setActiveWeaponEventArgs.weapon);
    }

    private void SetActiveWeaponEvent_OnSetInactiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        MakeWeaponInactive();
        cooldownBarParent.gameObject.SetActive(false);
    }

    /// <summary>
    /// Handle Weapon fired event on the UI
    /// </summary>
    private void WeaponFiredEvent_OnWeaponFired(WeaponFiredEvent weaponFiredEvent, WeaponFiredEventArgs weaponFiredEventArgs)
    {
        WeaponFired(weaponFiredEventArgs.weapon);
    }

    IEnumerator FadeCanvasGroup(CanvasGroup cg, float targetAlpha, float duration)
    {
        float startAlpha = cg.alpha;
        float time = 0f;

        while (time < duration)
        {
            cg.alpha = Mathf.Lerp(startAlpha, targetAlpha, time / duration);
            time += Time.unscaledDeltaTime;
            yield return null;
        }

        cg.alpha = targetAlpha;
    }

    /// <summary>
    /// Weapon fired update UI
    /// </summary>
    private void WeaponFired(Weapon weapon)
    {
        if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
        {
            if (ReferenceEquals(player.activeWeapon.GetCurrentOffHandWeapon(), weapon)) return;
        }
    
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
    /// Reset the weapon cooldown bar on the UI
    /// </summary>
    private void ResetWeaponCooldownBar(Weapon currentWeapon, bool stoppedPrematurely = false)
    {
        if (currentWeapon.weaponDetails.isMeleeWeapon)
        {
            cooldownTimer = (currentWeapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalMeleeAttackCoolDownModifier));
        }
        else if (currentWeapon.weaponDetails.weaponClass == WeaponClass.Bow || currentWeapon.weaponDetails.weaponClass == WeaponClass.Crossbow)
        {
            cooldownTimer = (currentWeapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalBowAttackCoolDownModifier));
        }
        else
        {
            cooldownTimer = currentWeapon.weaponDetails.weaponCooldownDuration;
        }


        // Set bar scale to 1
        barImage.transform.localScale = new Vector3(1f, 1f, 1f);

        if (!stoppedPrematurely)
        {
            barImage.color = new Color(1f, 1f, 1f, 0f);
        }
        else
        {
            player.activeWeapon.GetCurrentMainHandWeapon().firingStoppedPrematurelyIfWeaponIsPrecharged = false;
        }
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
        if (currentWeapon.itemSlotStatus == ItemSlotStatus.MainHand)
        {
            cooldownBarParent.gameObject.SetActive(true);
        }

        while (currentWeapon.onCooldown)
        {
            float barFill = 0f;

            // Update cooldown bar
            if (currentWeapon.weaponDetails.isMeleeWeapon)
            {
                barFill = cooldownTimer / (currentWeapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalMeleeAttackCoolDownModifier));
            }
            else if (currentWeapon.weaponDetails.weaponClass == WeaponClass.Bow || currentWeapon.weaponDetails.weaponClass == WeaponClass.Crossbow)
            {
                barFill = cooldownTimer / (currentWeapon.weaponDetails.weaponCooldownDuration * (1 + player.additionalBowAttackCoolDownModifier));
            }
            else
            {
                barFill = cooldownTimer / currentWeapon.weaponDetails.weaponCooldownDuration;
            }

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

    private void MakeWeaponInactive()
    {
        weaponImage.sprite = noWeaponSprite;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponImage), weaponImage);
        HelperUtilities.ValidateCheckNullValue(this, nameof(projectileRemainingText), projectileRemainingText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(weaponNameText), weaponNameText);
        HelperUtilities.ValidateCheckNullValue(this, nameof(barImage), barImage);
    }
#endif
    #endregion
}