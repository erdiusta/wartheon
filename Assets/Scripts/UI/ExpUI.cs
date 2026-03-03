using System.Collections;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

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

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        StaticEventHandler.OnLevelUp -= StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnExpGained -= StaticEventHandler_OnExpGained;
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        StaticEventHandler.OnLevelUp += StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnExpGained += StaticEventHandler_OnExpGained;
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
        // Update text
        if (GameManager.isDemo && player.currentLevel >= 4)
        {
            Color hudColor = new Color(1f, 0.8078f, 0.1568f);
            levelText.color = hudColor;
            levelText.text = "MAX LEVEL REACHED FOR DEMO";
        }
        else
        {
            levelText.text = "Level: " + player.currentLevel;
        }
    }
}
