using System.Collections;
using TMPro;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    [Header("Debuff")]
    public GameObject bleedingImage;
    public GameObject stunImage;
    public GameObject slowImage;
    public GameObject poisonImage;
    public GameObject rootImage;
    public GameObject acidImage;
    public GameObject warmImage;
    public GameObject burnImage;
    public GameObject chillImage;
    public GameObject frostImage;
    public GameObject staticImage;
    public GameObject paralyzeImage;
    public GameObject blindImage;
    public GameObject revealedImage;
    public GameObject curseImage;
    public GameObject fearImage;
    public GameObject deathImage;

    [Space(10)]
    public TextMeshPro statusLogText;

    Coroutine logRoutine;
    Player player;
    Enemy enemy;

    void Awake()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        if (player != null)
        {
            player.healthEvent.OnDodged += HealthEvent_OnDodged;
            player.healthEvent.OnBlocked += HealthEvent_OnBlocked;
            player.healthEvent.OnParried += HealthEvent_OnParried;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetBleeding += EnableBleedingImage;
            enemy.healthEvent.GetStun += EnableStunImage;
            enemy.healthEvent.GetSlow += EnableSlowImage;
            enemy.healthEvent.GetWarm += EnableWarmImage;
            enemy.healthEvent.GetBurned += EnableBurnImage;
            enemy.healthEvent.GetPoisoned += EnablePoisonImage;
            enemy.healthEvent.GetAcid += EnableAcidImage;
            enemy.healthEvent.GetChill += EnableChillImage;
            enemy.healthEvent.GetFrost += EnableFrostImage;
            enemy.healthEvent.GetShattered += EnableShatterLog;
            enemy.healthEvent.GetStatic += EnableStaticImage;
            enemy.healthEvent.GetParalyzed += EnableParalyzeImage;
            enemy.healthEvent.GetRoot += EnableRootImage;
            enemy.healthEvent.GetBlind += EnableBlindImage;
            enemy.healthEvent.GetCursed += EnableCurseImage;
            enemy.healthEvent.GetFeared += EnableFearImage;
            enemy.healthEvent.GetDeath += EnableDeathImage;

            enemy.healthEvent.OnDodged += HealthEvent_OnDodged;
            enemy.healthEvent.OnBlocked += HealthEvent_OnBlocked;
            enemy.healthEvent.OnParried += HealthEvent_OnParried;

            enemy.healthEvent.BleedingCured += DisableBleedingImage;
            enemy.healthEvent.StunCured += DisableStunImage;
            enemy.healthEvent.SlowCured += DisableSlowImage;
            enemy.healthEvent.WarmCured += DisableWarmImage;
            enemy.healthEvent.BurnCured += DisableBurnImage;
            enemy.healthEvent.PoisonCured += DisablePoisonImage;
            enemy.healthEvent.AcidCured += DisableAcidImage;
            enemy.healthEvent.ChillCured += DisableChillImage;
            enemy.healthEvent.FrostCured += DisableFrostImage;
            enemy.healthEvent.ShatterCured += DisableShatterLog;
            enemy.healthEvent.StaticCured += DisableStaticImage;
            enemy.healthEvent.ParalyzeCured += DisableParalyzeImage;
            enemy.healthEvent.RootCured += DisableRootImage;
            enemy.healthEvent.BlindCured += DisableBlindImage;
            enemy.healthEvent.CurseCured += DisableCurseImage;
            enemy.healthEvent.FearCured += DisableFearImage;
        }
    }

    private void OnDisable()
    {
        if(player != null)
        {
            player.healthEvent.OnDodged -= HealthEvent_OnDodged;
            player.healthEvent.OnBlocked -= HealthEvent_OnBlocked;
            player.healthEvent.OnParried -= HealthEvent_OnParried;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetBleeding -= EnableBleedingImage;
            enemy.healthEvent.GetStun -= EnableStunImage;
            enemy.healthEvent.GetSlow -= EnableSlowImage;
            enemy.healthEvent.GetWarm -= EnableWarmImage;
            enemy.healthEvent.GetBurned -= EnableBurnImage;
            enemy.healthEvent.GetPoisoned -= EnablePoisonImage;
            enemy.healthEvent.GetAcid -= EnableAcidImage;
            enemy.healthEvent.GetChill -= EnableChillImage;
            enemy.healthEvent.GetFrost -= EnableFrostImage;
            enemy.healthEvent.GetShattered -= EnableShatterLog;
            enemy.healthEvent.GetStatic -= EnableStaticImage;
            enemy.healthEvent.GetParalyzed -= EnableParalyzeImage;
            enemy.healthEvent.GetRoot -= EnableRootImage;
            enemy.healthEvent.GetBlind -= EnableBlindImage;
            enemy.healthEvent.GetCursed -= EnableCurseImage;
            enemy.healthEvent.GetFeared -= EnableFearImage;
            enemy.healthEvent.GetDeath -= EnableDeathImage;

            enemy.healthEvent.OnDodged -= HealthEvent_OnDodged;
            enemy.healthEvent.OnBlocked -= HealthEvent_OnBlocked;
            enemy.healthEvent.OnParried -= HealthEvent_OnParried;

            enemy.healthEvent.BleedingCured -= DisableBleedingImage;
            enemy.healthEvent.StunCured -= DisableStunImage;
            enemy.healthEvent.SlowCured -= DisableSlowImage;
            enemy.healthEvent.WarmCured -= DisableWarmImage;
            enemy.healthEvent.BurnCured -= DisableBurnImage;
            enemy.healthEvent.PoisonCured -= DisablePoisonImage;
            enemy.healthEvent.AcidCured -= DisableAcidImage;
            enemy.healthEvent.ChillCured -= DisableChillImage;
            enemy.healthEvent.FrostCured -= DisableFrostImage;
            enemy.healthEvent.ShatterCured -= DisableShatterLog;
            enemy.healthEvent.StaticCured -= DisableStaticImage;
            enemy.healthEvent.ParalyzeCured -= DisableParalyzeImage;
            enemy.healthEvent.RootCured -= DisableRootImage;
            enemy.healthEvent.BlindCured -= DisableBlindImage;
            enemy.healthEvent.CurseCured -= DisableCurseImage;
            enemy.healthEvent.FearCured -= DisableFearImage;
        }
    }

    private void HealthEvent_OnParried(HealthEvent healthEvent)
    {
        ClearLog();

        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("PARRIED", Color.white));
    }

    private void HealthEvent_OnBlocked(HealthEvent healthEvent)
    {
        ClearLog();

        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("BLOCKED", Color.white));
    }

    private void HealthEvent_OnDodged(HealthEvent healthEvent)
    {
        ClearLog();

        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("DODGED", Color.white));
    }

    private void EnableBleedingImage(HealthEvent healthEvent)
    {
        bleedingImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("BLEEDING", Color.red));
    }

    private void EnableWarmImage(HealthEvent healthEvent)
    {
        warmImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("WARMED", new Color(0.75f, 0.647f, 0f)));
    }

    private void EnableBurnImage(HealthEvent healthEvent)
    {
        burnImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("BURNED", new Color(1f, 0.647f, 0f)));
    }

    private void EnablePoisonImage(HealthEvent healthEvent)
    {
        poisonImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("POISONED", Color.green));
    }

    private void EnableAcidImage(HealthEvent healthEvent)
    {
        acidImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("ACIDIFIED - ARMOR DOWN", Color.red));
    }

    private void EnableChillImage(HealthEvent healthEvent)
    {
        chillImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("CHILLED", new Color(0.15f, 0.66f, 0.68f)));
    }

    private void EnableFrostImage(HealthEvent healthEvent)
    {
        frostImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("FROZEN", new Color(0.15f, 0.66f, 0.88f)));
    }

    private void EnableStunImage(HealthEvent healthEvent)
    {
        stunImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("STUNNED", Color.yellow));
    }

    private void EnableSlowImage(HealthEvent healthEvent)
    {
        slowImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("SLOWED", new Color(0.36f, 0.32f, 0.16f)));
    }

    private void EnableRootImage(HealthEvent healthEvent)
    {
        rootImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("ROOTED", new Color(0.36f, 0.25f, 0.20f)));
    }

    private void EnableCurseImage(HealthEvent healthEvent)
    {
        curseImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("CURSED", new Color(0.35f, 0.0f, 0.45f)));
    }

    private void EnableFearImage(HealthEvent healthEvent)
    {
        fearImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("FEARED", Color.magenta));
    }

    private void EnableDeathImage(HealthEvent healthEvent)
    {
        deathImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("INSTANT DEATH", Color.white));
    }

    private void EnableShatterLog(HealthEvent healthEvent)
    {
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = StartCoroutine(WriteLog("SHATTERED", new Color(0.16f, 0.54f, 0.8f)));
        }

        logRoutine = StartCoroutine(WriteLog("SHATTERED", new Color(0.16f, 0.54f, 0.8f)));
    }

    private void EnableStaticImage(HealthEvent healthEvent)
    {
        staticImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("STATIC", new Color(1f, 1f, 0.7f)));
    }

    private void EnableParalyzeImage(HealthEvent healthEvent)
    {
        paralyzeImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("PARALYZED", new Color(1f, 0.85f, 0.3f)));
    }


    private void EnableBlindImage(HealthEvent healthEvent)
    {
        blindImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }

        logRoutine = StartCoroutine(WriteLog("BLIND", Color.yellow));
    }

    private void DisableBleedingImage(HealthEvent healthEvent)
    {
        bleedingImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableWarmImage(HealthEvent healthEvent)
    {
        warmImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableBurnImage(HealthEvent healthEvent)
    {
        burnImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisablePoisonImage(HealthEvent healthEvent)
    {
        poisonImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableAcidImage(HealthEvent healthEvent)
    {
        acidImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }
    private void DisableChillImage(HealthEvent healthEvent)
    {
        chillImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }


    private void DisableFrostImage(HealthEvent healthEvent)
    {
        frostImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableShatterLog(HealthEvent healthEvent)
    {
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableStaticImage(HealthEvent healthEvent)
    {
        staticImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableParalyzeImage(HealthEvent healthEvent)
    {
        paralyzeImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableStunImage(HealthEvent healthEvent)
    {
        stunImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableSlowImage(HealthEvent healthEvent)
    {
        slowImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableRootImage(HealthEvent healthEvent)
    {
        rootImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableCurseImage(HealthEvent healthEvent)
    {
        curseImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableFearImage(HealthEvent healthEvent)
    {
        fearImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void DisableBlindImage(HealthEvent healthEvent)
    {
        blindImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = null;
        }
    }

    private void ClearLog()
    {
        if (statusLogText != null)
        {
            statusLogText.text = string.Empty;
        }
    }

    IEnumerator WriteLog(string text, Color color)
    {
        // Update the text
        statusLogText.text = text;
        statusLogText.color = color;

        // Set initial scale to 0.2
        statusLogText.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

        // The target scale (1, 1, 1)
        Vector3 targetScale = new Vector3(1f, 1f, 1f);

        // The amount to increment the scale per step
        float scaleIncrement = 0.0625f; // Adjust for how "stepped" the scaling should feel

        // Gradually scale up from 0.2 to 1 in fixed increments
        while (statusLogText.transform.localScale.x < 1f)
        {
            // Increment the scale in fixed steps for a pixelated feel
            statusLogText.transform.localScale += new Vector3(scaleIncrement, scaleIncrement, scaleIncrement);

            // Clamp the scale to not exceed 1
            if (statusLogText.transform.localScale.x > 1f)
            {
                statusLogText.transform.localScale = targetScale;
            }

            // Wait for the next frame
            yield return new WaitForSeconds(0.05f); // Add a delay for the stepping effect
        }

        // Ensure the scale is exactly (1, 1, 1)
        statusLogText.transform.localScale = targetScale;

        // Wait for 0.5 seconds before finishing
        yield return new WaitForSeconds(2f);

        // Reset the text
        statusLogText.text = string.Empty;

        logRoutine = null;
    }
}
