using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    Player player;

    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject HealthImage")]
    #endregion Tooltip
    [SerializeField] Image healthImage;
    #region Tooltip
    [Tooltip("Populate with healthText")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI healthText;

    [Space(10)]
    [SerializeField] Sprite standardSprite;
    [SerializeField] Sprite flashSprite;

    Coroutine playerHealthBarCoroutine;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        TrySubscribeToHealthEvents();

        player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void TrySubscribeToHealthEvents()
    {
        if (player == null) player = GameManager.Instance.GetPlayer();

        if (player != null)
        {
            player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged; // prevent duplicates
            player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;

            //// Optional: Update UI once on init
            //UpdateHealth(new HealthEventArgs());
        }
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
        float duration = 0.6f; // Adjust duration as needed
        float elapsed = 0f;
        float startValue = healthImage.transform.localScale.x;

        // Update availability bar
        float barFill =  Mathf.Clamp((float)player.health.GetCurrentHealth() / (float)player.health.GetMaximumHealth(), 0, 1);

        // Apply sprite change
        Image barImage = transform.GetChild(0).GetChild(0).GetComponent<Image>();
        barImage.sprite = flashSprite;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, barFill, elapsed / duration);
            healthImage.transform.localScale = new Vector3(newValue, 1f, 1f);

            yield return null;
        }

        // Reset sprite
        barImage.sprite = standardSprite;
        healthImage.transform.localScale = new Vector3(barFill, 1f, 1f);
        playerHealthBarCoroutine = null;
    }
}
