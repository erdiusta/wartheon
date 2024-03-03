using System;
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
        enemy.healthEvent.OnHeadShot += HealthEvent_OnHeadShot;
    }

    private void OnDisable()
    {
        enemy.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
        enemy.healthEvent.OnCriticalHit -= HealthEvent_OnCriticalHit;
        enemy.healthEvent.OnHeadShot -= HealthEvent_OnHeadShot;
    }

    private void Start()
    {
        damageDisplayText.text = "";
        criticalHitText.text = "";
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if (!enemy.health.suddenDeathHappened)
        {
            StartCoroutine(DisplayDamage(healthEventArgs.damageAmount));
        }
    }

    private void HealthEvent_OnCriticalHit(HealthEvent healthEvent)
    {
        StartCoroutine(DisplayCriticalDamage());
    }

    private void HealthEvent_OnHeadShot(HealthEvent healthEvent)
    {
        StartCoroutine(DisplayHeadShot());
    }

    /// <summary>
    /// Display critical hit text
    /// </summary>
    IEnumerator DisplayCriticalDamage()
    {
        criticalHitText.color = new Color(1, 0.93f, 0.59f);
        criticalHitText.text = "CRITICAL HIT";

        yield return new WaitForSeconds(0.5f);

        criticalHitText.text = "";

    }

    /// <summary>
    /// Display head shot text
    /// </summary>
    IEnumerator DisplayHeadShot()
    {
        criticalHitText.color = Color.red;
        criticalHitText.text = "HEAD SHOT";

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
