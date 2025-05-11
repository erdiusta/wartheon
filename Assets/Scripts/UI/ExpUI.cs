using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class ExpUI : MonoBehaviour
{
    Player player;

    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject ExpBar")]
    #endregion Tooltip
    [SerializeField] Image expBar;
    #region Tooltip
    [Tooltip("Populate with the level text")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI levelText;
    #region Tooltip
    [Tooltip("Populate with the demo text")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI demoText;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnLevelUp += StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnExpGained += StaticEventHandler_OnExpGained;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnLevelUp -= StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnExpGained -= StaticEventHandler_OnExpGained;
    }

    private void StaticEventHandler_OnLevelUp()
    {
        UpdateExpBar();
        UpdateLevelText();
    }

    private void StaticEventHandler_OnExpGained()
    {
        UpdateExpBar();
    }

    private void UpdateExpBar()
    {
        StartCoroutine(UpdateExpBarRoutine());
    }

    /// <summary>
    /// Animate exp bar routine
    /// </summary>
    private IEnumerator UpdateExpBarRoutine()
    {
        float barFill;

        // Update availability bar
        if (GameManager.isDemo && player.currentLevel >= 4)
        {
            barFill = 1f; // EXCEED DEMO LIMIT
            demoText.gameObject.SetActive(true);
        }
        else
        {
            barFill = (float)(player.currentGainedTotalExperiencePoints - player.levelUpDetails.playerLevelDataList[player.currentLevel - 1].levelUpExpPointForNextLevel) /
                (float)player.levelUpDetails.playerLevelDataList[player.currentLevel].levelUpExpPointForNextLevel;
        }

        // Update bar fill
        expBar.transform.localScale = new Vector3(barFill, 1f, 1f);

        yield return null;

    }

    private void UpdateLevelText()
    {
        levelText.text = "Lvl: " + player.currentLevel;
    }
}
