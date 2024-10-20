using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class StatusManager : MonoBehaviour
{
    public GameObject poisonImage;
    public GameObject acidImage;
    public GameObject stunImage;
    public GameObject frostImage;
    public GameObject curseImage;
    public GameObject blockSpecialMoveImage;
    public GameObject gemSkinSpecialMoveImage;
    public GameObject deathImage;
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
            player.healthEvent.GetPoisoned += EnablePoisonImage;
            player.healthEvent.GetAcid += EnableAcidImage;
            player.healthEvent.GetFrost += EnableFrostImage;
            player.healthEvent.GetStun += EnableStunImage;
            player.healthEvent.GetCursed += EnableCurseImage;
            player.healthEvent.GetBlockSpecialMove += EnableBlockSkillImage;
            player.healthEvent.GetGemSkinSpecialMove += EnableGemSkinSkillImage;
            player.healthEvent.GetDeath += EnableDeathImage;
            player.healthEvent.OnDeflected += HealthEvent_OnDeflected;

            player.healthEvent.PoisonCured += DisablePoisonImage;
            player.healthEvent.AcidCured += DisableAcidImage;
            player.healthEvent.FrostCured += DisableFrostImage;
            player.healthEvent.StunCured += DisableStunImage;
            player.healthEvent.CurseCured += DisableCurseImage;
            player.healthEvent.BlockSpecialMoveDurationEnded += DisableBlockSkillImage;
            player.healthEvent.OnGemSkinSpecialMoveEnded += DisableGemSkinSkillImage;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetPoisoned += EnablePoisonImage;
            enemy.healthEvent.GetAcid += EnableAcidImage;
            enemy.healthEvent.GetFrost += EnableFrostImage;
            enemy.healthEvent.GetStun += EnableStunImage;
            enemy.healthEvent.GetCursed += EnableCurseImage;
            enemy.healthEvent.GetBlockSpecialMove += EnableBlockSkillImage;
            enemy.healthEvent.GetGemSkinSpecialMove += EnableGemSkinSkillImage;
            enemy.healthEvent.GetDeath += EnableDeathImage;
            enemy.healthEvent.GetShattered += EnableShatterLog;

            enemy.healthEvent.PoisonCured += DisablePoisonImage;
            enemy.healthEvent.AcidCured += DisableAcidImage;
            enemy.healthEvent.FrostCured += DisableFrostImage;
            enemy.healthEvent.StunCured += DisableStunImage;
            enemy.healthEvent.CurseCured += DisableCurseImage;
            enemy.healthEvent.BlockSpecialMoveDurationEnded += DisableBlockSkillImage;
            enemy.healthEvent.OnGemSkinSpecialMoveEnded += DisableGemSkinSkillImage;
        }
    }

    private void OnDisable()
    {
        if (player != null)
        {
            player.healthEvent.GetPoisoned -= EnablePoisonImage;
            player.healthEvent.GetAcid -= EnableAcidImage;
            player.healthEvent.GetFrost -= EnableFrostImage;
            player.healthEvent.GetStun -= EnableStunImage;
            player.healthEvent.GetCursed -= EnableCurseImage;
            player.healthEvent.GetBlockSpecialMove -= EnableBlockSkillImage;
            player.healthEvent.GetGemSkinSpecialMove -= EnableGemSkinSkillImage;
            player.healthEvent.GetDeath -= EnableDeathImage;
            player.healthEvent.OnDeflected -= HealthEvent_OnDeflected;

            player.healthEvent.PoisonCured -= DisablePoisonImage;
            player.healthEvent.AcidCured -= DisableAcidImage;
            player.healthEvent.FrostCured -= DisableFrostImage;
            player.healthEvent.StunCured -= DisableStunImage;
            player.healthEvent.CurseCured -= DisableCurseImage;
            player.healthEvent.BlockSpecialMoveDurationEnded -= DisableBlockSkillImage;
            player.healthEvent.OnGemSkinSpecialMoveEnded -= DisableGemSkinSkillImage;
        }

        if (enemy != null)
        {
            enemy.healthEvent.GetPoisoned -= EnablePoisonImage;
            enemy.healthEvent.GetAcid -= EnableAcidImage;
            enemy.healthEvent.GetFrost -= EnableFrostImage;
            enemy.healthEvent.GetStun -= EnableStunImage;
            enemy.healthEvent.GetCursed -= EnableCurseImage;
            enemy.healthEvent.GetBlockSpecialMove -= EnableBlockSkillImage;
            enemy.healthEvent.GetGemSkinSpecialMove -= EnableGemSkinSkillImage;
            enemy.healthEvent.GetDeath -= EnableDeathImage;
            enemy.healthEvent.GetShattered -= EnableShatterLog;

            enemy.healthEvent.PoisonCured -= DisablePoisonImage;
            enemy.healthEvent.AcidCured -= DisableAcidImage;
            enemy.healthEvent.FrostCured -= DisableFrostImage;
            enemy.healthEvent.StunCured -= DisableStunImage;
            enemy.healthEvent.CurseCured -= DisableCurseImage;
            enemy.healthEvent.BlockSpecialMoveDurationEnded -= DisableBlockSkillImage;
            enemy.healthEvent.OnGemSkinSpecialMoveEnded -= DisableGemSkinSkillImage;
        }
    }

    private void HealthEvent_OnDeflected(HealthEvent healthEvent)
    {
        ClearLog();

        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("DEFLECTED", Color.gray));
        }
    }

    private void EnableBlockSkillImage(HealthEvent healthEvent)
    {
        blockSpecialMoveImage.SetActive(true);
        ClearLog();

        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("BLOCKED", Color.gray));
        }
    }

    private void EnableGemSkinSkillImage(HealthEvent healthEvent)
    {
        gemSkinSpecialMoveImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("GEM SKIN", Color.gray));
        }
    }

    private void EnablePoisonImage(HealthEvent healthEvent)
    {
        poisonImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("POISONED", Color.green));
        }
    }

    private void EnableAcidImage(HealthEvent healthEvent)
    {
        acidImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("ACIDIFIED - ARMOR DOWN", Color.red));
        }
    }

    private void EnableFrostImage(HealthEvent healthEvent)
    {
        frostImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("FROZEN", new Color(0.15f, 0.66f, 0.88f)));
        }
    }

    private void EnableStunImage(HealthEvent healthEvent)
    {
        stunImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("STUNNED", Color.yellow));
        }
    }

    private void EnableCurseImage(HealthEvent healthEvent)
    {
        curseImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("CURSED", Color.magenta));
        }
    }

    private void EnableDeathImage(HealthEvent healthEvent)
    {
        deathImage.SetActive(true);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("INSTANT DEATH", Color.white));
        }
    }

    private void EnableShatterLog(HealthEvent healthEvent)
    {
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
            logRoutine = StartCoroutine(WriteLog("SHATTERED", new Color(0.16f, 0.54f, 0.8f)));
        }
        else
        {
            logRoutine = StartCoroutine(WriteLog("SHATTERED", new Color(0.16f, 0.54f, 0.8f)));
        }
    }

    private void DisableBlockSkillImage(HealthEvent healthEvent)
    {
        blockSpecialMoveImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
        }
    }

    private void DisableGemSkinSkillImage(HealthEvent healthEvent)
    {
        gemSkinSpecialMoveImage.SetActive(false);
        ClearLog();
        if (logRoutine != null)
        {
            StopCoroutine(logRoutine);
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
        yield return new WaitForSeconds(3f);

        // Reset the text
        statusLogText.text = string.Empty;

        logRoutine = null;
    }
}
