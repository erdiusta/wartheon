using TMPro;
using UnityEngine;

public class DamageDisplay : MonoBehaviour
{
    [SerializeField] TMP_Text damageDisplayTextPrefab; // Prefab for damage text
    [SerializeField] Transform damageTextSpawnPoint; // The point where the damage text will appear
    [SerializeField] Transform textSpawnPoint; // The point where the critical hit text will appear

    [SerializeField] Color criticalHitColor = new Color(1, 0.93f, 0.59f);
    [SerializeField] Color headShotColor = Color.red;
    [SerializeField] float displayDuration = 0.7f;
    [SerializeField] float criticalHitDuration = 0.5f;
    [SerializeField] float headShotDuration = 0.5f;
    [SerializeField] Vector3 popUpOffset = new Vector3(0, 0.5f, 0);
    [SerializeField] float popUpDuration = 0.5f;

    Enemy enemy;
    Decoy decoy;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        decoy = GetComponent<Decoy>();
    }

    private void OnEnable()
    {
        if (decoy == null)
        {
            enemy.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
            enemy.healthEvent.OnCriticalHit += HealthEvent_OnCriticalHit;
            enemy.healthEvent.OnHeadShot += HealthEvent_OnHeadShot;
        }
        else if (enemy == null)
        {
            decoy.healthEvent.OnHealthChanged += HealthEvent_OnHealthChangedForDecoy;
        }
    }

    private void OnDisable()
    {
        if (decoy == null)
        {
            enemy.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
            enemy.healthEvent.OnCriticalHit -= HealthEvent_OnCriticalHit;
            enemy.healthEvent.OnHeadShot -= HealthEvent_OnHeadShot;
        }
        else if (enemy == null)
        {
            decoy.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChangedForDecoy;
        }
    }

    private void HealthEvent_OnHealthChangedForDecoy(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        DisplayDamage(healthEventArgs.damageAmount, healthEventArgs.hand);
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        if (!enemy.health.suddenDeathHappened && healthEventArgs.damageAmount > 0)
        {
            DisplayDamage(healthEventArgs.damageAmount, healthEventArgs.hand);
        }
    }

    private void HealthEvent_OnCriticalHit(HealthEvent healthEvent)
    {
        if (!enemy.health.suddenDeathHappened)
        {
            DisplayCriticalDamage();
        }
    }

    private void HealthEvent_OnHeadShot(HealthEvent healthEvent)
    {
        if (!enemy.health.suddenDeathHappened)
        {
            DisplayHeadShot();
        }
    }

    /// <summary>
    /// Display amount of damage
    /// </summary>
    private void DisplayDamage(int damageAmount, MeleeHand hand = MeleeHand.None, bool onStart = false)
    {
        var damageText = Instantiate(damageDisplayTextPrefab, damageTextSpawnPoint.position, Quaternion.identity, damageTextSpawnPoint);

        damageText.transform.localPosition = Vector3.zero;

        if (hand == MeleeHand.OffHand) damageText.transform.localPosition += new Vector3(-0.3f, 0.5f, 0f);
        else if (hand == MeleeHand.MainHand) damageText.transform.localPosition += new Vector3(0.3f, 0f, 0f);

        damageText.text = damageAmount.ToString();

        float delay = hand == MeleeHand.OffHand ? 0.05f : 0f;
        AnimateText(damageText, displayDuration, delay);
    }

    /// <summary>
    /// Display critical hit text
    /// </summary>
    private void DisplayCriticalDamage()
    {
        var criticalText = Instantiate(damageDisplayTextPrefab, textSpawnPoint.position, Quaternion.identity, textSpawnPoint);

        criticalText.color = criticalHitColor;
        criticalText.transform.localPosition += new Vector3(0f, 0f, 0f);

        criticalText.text = "CRITICAL HIT";
        AnimateText(criticalText, criticalHitDuration);
    }

    /// <summary>
    /// Display head shot text
    /// </summary>
    private void DisplayHeadShot()
    {
        var headShotText = Instantiate(damageDisplayTextPrefab, textSpawnPoint.position, Quaternion.identity, textSpawnPoint);

        headShotText.color = headShotColor;
        headShotText.transform.localPosition += new Vector3(0f, 0f, 0f);

        headShotText.text = "HEAD SHOT";
        AnimateText(headShotText, headShotDuration);
    }

    private void AnimateText(TMP_Text text, float duration, float delay = 0f)
    {
        Vector3 startPosition = text.transform.position;
        Vector3 endPosition = startPosition + popUpOffset; // e.g., new Vector3(0, 0.5f, 0)

        LeanTween.move(text.gameObject, endPosition, popUpDuration).setEase(LeanTweenType.linear).setDelay(delay);
        LeanTween.scale(text.gameObject, Vector3.zero, popUpDuration).setEaseInOutBounce().setDelay(duration + delay);

        Destroy(text.gameObject, duration + popUpDuration + delay);
    }
}
