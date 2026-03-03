using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject HealthBarImage")]
    #endregion Tooltip
    [SerializeField] Image healthBar;
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject ReservedHPImage")]
    #endregion Tooltip
    [SerializeField] Image shieldBar;
    #region Tooltip
    [Tooltip("Populate with healthText")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI healthText;
    #region Tooltip
    [Tooltip("Populate with shieldText")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI shieldText;

    [Space(10)]
    [SerializeField] Sprite standardSprite;
    [SerializeField] Sprite flashSprite;

    Player player;
    Coroutine playerHealthBarCoroutine;

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
        while (player == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        Subscribe();
        UpdateHealthText();
        UpdateHealthBar();
    }

    private void Subscribe()
    {
        if (player == null || player.healthEvent == null) return;

        player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void Unsubscribe()
    {
        if (player == null || player.healthEvent == null) return;

        player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        UpdateHealth(healthEventArgs);
    }

    private void UpdateHealth(HealthEventArgs healthEventArgs)
    {
        UpdateHealthText();
        UpdateHealthBar();
    }

    private void UpdateHealthText()
    {
        int health = Mathf.Clamp(player.health.GetCurrentHealth(), 0, player.health.GetMaximumHealth());

        healthText.text = $"{health}/{player.health.GetMaximumHealth()}";
    }

    private void UpdateHealthBar()
    {
        if (gameObject.activeInHierarchy)
        {
            if (playerHealthBarCoroutine != null) StopCoroutine(playerHealthBarCoroutine);

            StartCoroutine(UpdateHealthBarRoutine());
        }
    }

    /// <summary>
    /// Animate health bar coroutine
    /// </summary>
    private IEnumerator UpdateHealthBarRoutine()
    {
        float duration = 0.6f;
        float elapsed = 0f;

        float maxHealth = player.health.GetMaximumHealth();
        float currentHealth = Mathf.Clamp(player.health.GetCurrentHealth(), 0, maxHealth);  // Health 96
        float currentShield = Mathf.Clamp(player.health.GetCurrentShield(), 0, maxHealth);  // Shield 20

        if (currentShield > 0)
        {
            Debug.Log(currentShield + " is different than zero.");
        }

        float total = currentShield > 0 ? Mathf.Max(currentHealth + currentShield, 1) : maxHealth; // prevent div/0  // Total 116

        float targetHealthFill = currentHealth / total; // 0.82
        float targetShieldFill = currentShield / total; // 0.18

        float startHealthFill = healthBar.transform.localScale.x;
        float startShieldFill = shieldBar.transform.localScale.x; 

        RectTransform barParent = (RectTransform)healthBar.transform.parent; 
        float totalWidth = barParent.rect.width; // 55

        // Optional sprite flash
        healthBar.sprite = flashSprite;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Interpolate fill values
            float currentHealthFill = Mathf.Lerp(startHealthFill, targetHealthFill, t);
            float currentShieldFill = Mathf.Lerp(startShieldFill, targetShieldFill, t);

            // Update scales
            healthBar.transform.localScale = new Vector3(currentHealthFill, 1f, 1f);
            shieldBar.transform.localScale = new Vector3(-currentShieldFill, 1f, 1f); // Negative for leftward fill

            // Compute combined visual width
            total = Mathf.Max(currentHealth + currentShield, 1f);
            float interpolatedHealth = currentHealthFill * total;
            float interpolatedShield = currentShieldFill * total;
            float totalVisual = Mathf.Max(interpolatedHealth + interpolatedShield, 1f);

            float visualRatio = Mathf.Min(totalVisual / maxHealth, 1f);
            float combinedVisualWidth = totalWidth * visualRatio;

            float healthPortion = Mathf.Clamp01(interpolatedHealth / totalVisual);
            float healthWidth = combinedVisualWidth * healthPortion;

            // ShieldBar always starts at end of HealthBar
            ((RectTransform)shieldBar.transform).anchoredPosition = new Vector2(healthWidth - 1f, 0f);

            // Optional: Update health text position
            float fullBarCenter = totalWidth / 2f;
            float healthBarCenter = healthWidth / 2f;
            float healthBarAnchorDeviation = Mathf.Clamp(fullBarCenter - healthBarCenter - 4f, -18f, 17f);
            //((RectTransform)healthText.transform).anchoredPosition = new Vector2(-healthBarAnchorDeviation, 0f);

            // Optional: Shield text visibility
            shieldText.text = (currentShieldFill < 0.05f) ? "" : player.health.GetCurrentShield().ToString();

            yield return null;
        }

        // Final values
        healthBar.transform.localScale = new Vector3(targetHealthFill, 1f, 1f);
        shieldBar.transform.localScale = new Vector3(-targetShieldFill, 1f, 1f);

        ShiftShieldBar(maxHealth, currentHealth, currentShield, totalWidth);

        healthBar.sprite = standardSprite;
        playerHealthBarCoroutine = null;
    }

    private void ShiftShieldBar(float maxHealth, float currentHealth, float currentShield, float totalWidth)
    {
        float totalVisual = currentHealth + currentShield;

        // Clamp the total so that over-healing/shielding doesn't overflow bar
        float visualRatio = Mathf.Min(totalVisual / maxHealth, 1f);
        float combinedVisualWidth = totalWidth + visualRatio;

        // Health portion
        float healthPortion = Mathf.Clamp01(currentHealth / totalVisual); // 0–1 within combined fill
        float healthWidth = combinedVisualWidth * healthPortion;
        float shieldWidth = combinedVisualWidth - healthWidth;


        // Place shield bar after health
        ((RectTransform)shieldBar.transform).anchoredPosition = new Vector2(healthWidth - 1f, 0f);

        // Calculate center shift
        float fullBarCenter = totalWidth / 2f; // original center is 0
        float healthBarCenter = healthWidth / 2f;

        float healthBarAnchorDeviation = fullBarCenter - healthBarCenter;

        // Apply deviation (leftward shift)
        ((RectTransform)healthText.transform).anchoredPosition = new Vector2(-healthBarAnchorDeviation, 0f);
    }
}
