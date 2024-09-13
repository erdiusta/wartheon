using System;
using System.Collections.Generic;
using UnityEngine;

[DisallowMultipleComponent]
public class HealthUI : MonoBehaviour
{
    List<GameObject> healthHeartsList = new List<GameObject>();

    private void OnEnable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        GameManager.Instance.GetPlayer().healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }


    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        SetHealthBar(healthEventArgs);
    }

    private void SetHealthBar(HealthEventArgs healthEventArgs)
    {
        ClearHealthBar();

        // Instantiate heart image prefabs
<<<<<<< Updated upstream
        int healthHearts = Mathf.CeilToInt(healthEventArgs.healthPercent * 100f / 20f);

        for (int i = 0; i < healthHearts; i++)
=======
        int healthHeartCount = healthEventArgs.healthPercent * ((20 + player.playerDetails.constitution * 10) % 20f) > 10 ? 
            Mathf.CeilToInt(healthEventArgs.healthPercent * ((20 + player.playerDetails.constitution * 10) / 20f)) : 
            Mathf.FloorToInt(healthEventArgs.healthPercent * ((20 + player.playerDetails.constitution * 10) / 20f));

        int halfHeartCount;

        // Instantiate half heart image prefabs
        if (healthEventArgs.healthAmount >= 0f)
        {
            halfHeartCount = healthEventArgs.healthPercent * ((20 + player.playerDetails.constitution * 10) % 20f) <= 10 ? 1 : 0;
        }
        else
        {
            halfHeartCount = 0;
        }

        for (int i = 0; i < healthHeartCount; i++)
>>>>>>> Stashed changes
        {
            // Instantiate heart prefabs
            GameObject heart = Instantiate(GameResources.Instance.heartPrefab, transform);

            // Position
            heart.GetComponent<RectTransform>().anchoredPosition = new Vector2(Settings.uiHeartSpacing * i, 0f);

            healthHeartsList.Add(heart);
        }
<<<<<<< Updated upstream
=======

        if (halfHeartCount > 0)
        {
            if (Mathf.FloorToInt(healthEventArgs.healthPercent * ((20 + player.playerDetails.constitution * 10) % 20f)) != 0)
            {
                // Instantiate half heart prefab if exists
                GameObject halfHeart = Instantiate(GameResources.Instance.halfHeartPrefab, transform);

                // Position
                halfHeart.GetComponent<RectTransform>().anchoredPosition = new Vector2(Settings.uiHeartSpacing * healthHeartCount, 0f);

                healthHeartsList.Add(halfHeart);
            }
            else if (healthEventArgs.healthAmount <= 0f)
            {
                foreach (GameObject halfHeartPrefab in healthHalfHeartsList)
                {
                    Destroy(halfHeartPrefab);
                }

                healthHalfHeartsList.Clear();
            }
        }
>>>>>>> Stashed changes
    }

    private void ClearHealthBar()
    {
        foreach (GameObject heartIcon in healthHeartsList)
        {
            Destroy(heartIcon);
        }

        healthHeartsList.Clear();
    }
}
