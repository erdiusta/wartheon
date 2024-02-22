using System.Collections;
using TMPro;
using UnityEngine;

public class DamageDisplay : MonoBehaviour
{
    [SerializeField] TextMeshPro damageDisplayText;
    [SerializeField] TextMeshPro criticalHitText;

    Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        enemy.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
        enemy.healthEvent.OnCriticalHit += HealthEvent_OnCriticalHit;
    }

    private void OnDisable()
    {
        enemy.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
        enemy.healthEvent.OnCriticalHit -= HealthEvent_OnCriticalHit;
    }

    private void Start()
    {
        damageDisplayText.text = "";
        criticalHitText.text = "";
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        StartCoroutine(DisplayDamage(healthEventArgs.damageAmount));
    }

    private void HealthEvent_OnCriticalHit(HealthEvent healthEvent)
    {
        StartCoroutine(DisplayCriticalDamage());
    }

    /// <summary>
    /// Display critical hit text
    /// </summary>
    IEnumerator DisplayCriticalDamage()
    {
        criticalHitText.text = "CRITICAL HIT";

        yield return new WaitForSeconds(0.5f);

        criticalHitText.text = "";
    }

    /// <summary>
    /// Display amount of damage
    /// </summary>
    IEnumerator DisplayDamage(int damageAmount)
    {
        damageDisplayText.text = damageAmount.ToString();

        yield return new WaitForSeconds(0.7f);

        damageDisplayText.text = "";
    }
}
