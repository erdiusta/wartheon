using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class DamageDisplay : MonoBehaviour
{
    [SerializeField] TextMeshPro textMeshPro;

    Enemy enemy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        enemy.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        enemy.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void Start()
    {
        textMeshPro.text = "";
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        StartCoroutine(DisplayDamage(healthEventArgs.damageAmount));
    }

    /// <summary>
    /// Display amount of damage
    /// </summary>
    IEnumerator DisplayDamage(int damageAmount)
    {
        textMeshPro.text = damageAmount.ToString();

        yield return new WaitForSeconds(0.7f);

        textMeshPro.text = "";
    }
}
