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
        while (player == null || player?.activeWeapon == null || player?.activeWeapon?.GetCurrentMainHandWeapon() == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        Subscribe();
        SetActiveWeapon(player.activeWeapon.GetCurrentMainHandWeapon());
    }

    private void Subscribe()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeaponForHud += SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveMainHandWeaponForHud += SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;

        player.weaponFiredEvent.OnWeaponFired += WeaponFiredEvent_OnWeaponFired;
    }

    private void Unsubscribe()
    {
        player.setActiveWeaponEvent.OnSetActiveMainHandWeaponForHud -= SetActiveWeaponEvent_OnSetActiveMainHandWeapon;
        player.setActiveWeaponEvent.OnSetInactiveMainHandWeaponForHud -= SetActiveWeaponEvent_OnSetInactiveMainHandWeapon;

        player.weaponFiredEvent.OnWeaponFired -= WeaponFiredEvent_OnWeaponFired;
    }

    private void Update()
    {
        if (player == null || player?.activeWeapon == null || player?.activeWeapon?.GetCurrentMainHandWeapon() == null || !player.IsLocal) return;

        if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (player.onCooldown)
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
        if (setActiveWeaponEventArgs.weaponStats.weaponTitle != WeaponTitle.None)
        {
            Weapon weapon = WeaponDropGenerator.GetWeaponWithStats(setActiveWeaponEventArgs.weaponStats, setActiveWeaponEventArgs.rarity, ItemSlotStatus.MainHand, -1);
            SetActiveWeapon(weapon);
        }
    }

    private void SetActiveWeaponEvent_OnSetInactiveMainHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent)
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
    
        UpdateCooldownBar(player, weapon);
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
        UpdateActiveWeaponImage(weapon);
    }

    /// <summary>
    /// Populate active weapon image
    /// </summary>
    private void UpdateActiveWeaponImage(Weapon weapon)
    {
        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);
        weaponImage.sprite = weaponDetails.weaponFrontSprite;
    }

    /// <summary>
    /// Reset the weapon cooldown bar on the UI
    /// </summary>
    private void ResetWeaponCooldownBar(Weapon currentWeapon, bool stoppedPrematurely = false)
    {
        if (currentWeapon != null)
        {
            cooldownTimer = (currentWeapon.weaponStats.weaponCooldownDuration * (1 - player.additionalAttackCoolDownModifier));

            // Set bar scale to 1
            barImage.transform.localScale = new Vector3(1f, 1f, 1f);

            if (!stoppedPrematurely)
            {
                barImage.color = new Color(1f, 1f, 1f, 0f);
            }
            else
            {
                player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.firingStoppedPrematurelyIfWeaponIsPrecharged = false;
            }
        }
    }


    /// <summary>
    /// Update cooldown bar
    /// </summary>
    void UpdateCooldownBar(Player player, Weapon currentWeapon)
    {
        cooldownRoutine = StartCoroutine(CooldownRoutine(player, currentWeapon));
    }

    /// <summary>
    /// Run cooldown routine
    /// </summary>
    IEnumerator CooldownRoutine(Player player, Weapon currentWeapon)
    {
        if (currentWeapon.ItemSlotStatus == ItemSlotStatus.MainHand)
        {
            cooldownBarParent.gameObject.SetActive(true);
        }

        while (player.onCooldown)
        {
            float barFill = 0f;

            // Update cooldown bar
            barFill = cooldownTimer / (currentWeapon.weaponStats.weaponCooldownDuration * (1 - player.additionalAttackCoolDownModifier));

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
}