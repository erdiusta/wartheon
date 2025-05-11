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
        int health = player.health.GetCurrentHealth() < 0 ? 0 : player.health.GetCurrentHealth();
        healthText.text = $"{health}/{player.health.GetMaximumHealth()}";
    }

    private void UpdateHealthBar()
    {
        if (gameObject.activeInHierarchy) StartCoroutine(UpdateHealthBarRoutine());
    }

    /// <summary>
    /// Animate health bar coroutine
    /// </summary>
    private IEnumerator UpdateHealthBarRoutine()
    {
        // Update availability bar
        float barFill =  Mathf.Clamp((float)player.health.GetCurrentHealth() / (float)player.health.GetMaximumHealth(), 0, 1);

        // Update bar fill
        healthImage.transform.localScale = new Vector3(barFill, 1f, 1f);

        yield return null;

    }
}
