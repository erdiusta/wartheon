using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SpecialMoveUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child moveReadyText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI specialMoveReadyText;
    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child specialMoveNameText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI specialMoveNameText;
    #region Tooltip
    [Tooltip("Populate with the container bar")]
    #endregion Tooltip
    [SerializeField] GameObject containerBar;
    #region Tooltip
    [Tooltip("Populate with the RectTransform of the child gameobject moveReadyBar")]
    #endregion Tooltip
    [SerializeField] Transform moveReadyBar;
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject BarImage")]
    #endregion Tooltip
    [SerializeField] Image barImage;

    Player player;
    Coroutine reloadSpecialMoveCoroutine;
    Coroutine blinkingReloadTextCoroutine;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void Start()
    {
        switch (player.playerDetails.playerCharacterName)
        {
            case Settings.astraeus:
                // Set the reload bar to red
                barImage.color = Color.magenta;
                break;
            case Settings.erebus:
                // Set the reload bar to red
                barImage.color = Color.cyan;
                break;
            case Settings.lyrisa:
                // Set the reload bar to red
                barImage.color = Color.yellow;
                break;
            default:
                break;
        }

        // Set the special move text
        specialMoveNameText.text = player.playerDetails.specialMoveName;
        // Update bar fill
        containerBar.gameObject.SetActive(false);
    }

    private void OnEnable()
    {
        player.specialMoveEvent.OnSpecialMoveUsed += SpecialMoveEvent_OnSpecialMoveUsed;
    }

    private void OnDisable()
    {
        player.specialMoveEvent.OnSpecialMoveUsed -= SpecialMoveEvent_OnSpecialMoveUsed;
    }

    private void Update()
    {
        if (player.specialMoveOnCooldown)
        {
            player.specialMoveTimer += Time.deltaTime;

            containerBar.gameObject.SetActive(true);
            StopBlinkingMoveReadyTextCoroutine();

            if (player.specialMoveTimer > player.playerDetails.specialMoveDuration)
            {
                player.specialMoveTimer = player.playerDetails.specialMoveDuration;
                player.specialMoveOnCooldown = false;
            }
        }
        else
        {
            player.specialMoveTimer = 0f;

            if (player.playerDetails.playerCharacterName == Settings.erebus)
            {
                player.playerControl.Unstealth();
                player.playerDetails.onStealth = false;
            }

            containerBar.gameObject.SetActive(false);
            if (blinkingReloadTextCoroutine == null)
            {
                blinkingReloadTextCoroutine = StartCoroutine(StartBlinkingMoveReadyTextRoutine());
            }
        }
    }

    private void SpecialMoveEvent_OnSpecialMoveUsed()
    {
        StopSpecialMoveCoroutine();
        UpdateSpecialMoveText();

        reloadSpecialMoveCoroutine = StartCoroutine(UpdateSpecialMoveReloadBarRoutine());
    }

    /// <summary>
    /// Stop coroutine updating special move progress bar
    /// </summary>
    private void StopSpecialMoveCoroutine()
    {
        // Stop any active weapon reload bar on the UI
        if (reloadSpecialMoveCoroutine != null)
        {
            StopCoroutine(reloadSpecialMoveCoroutine);
        }
    }

    /// <summary>
    /// Update the blinking special move text
    /// </summary>
    private void UpdateSpecialMoveText()
    {
        // set the reload bar to red
        barImage.color = Color.red;

        StopBlinkingMoveReadyTextCoroutine();
    }

    /// <summary>
    /// Animate special move bar coroutine
    /// </summary>
    private IEnumerator UpdateSpecialMoveReloadBarRoutine()
    {
        switch (player.playerDetails.playerCharacterName)
        {
            case Settings.astraeus:
                // Set the reload bar to red
                barImage.color = Color.magenta;
                break;
            case Settings.erebus:
                // Set the reload bar to red
                barImage.color = Color.cyan;
                break;
            case Settings.lyrisa:
                // Set the reload bar to red
                barImage.color = Color.yellow;
                break;
            default:
                break;
        }

        // Animate the weapon reload bar
        while (player.specialMoveTimer < player.playerDetails.specialMoveDuration)
        {
            // update reloadbar
            float barFill = player.specialMoveTimer / player.playerDetails.specialMoveDuration;

            // update bar fill
            moveReadyBar.transform.localScale = new Vector3(barFill, 1f, 1f);

            blinkingReloadTextCoroutine = null;
            yield return null;
        }
    }

    /// <summary>
    /// Start the coroutine to blink the special move text
    /// </summary>
    private IEnumerator StartBlinkingMoveReadyTextRoutine()
    {
        while (!player.specialMoveOnCooldown)
        {
            specialMoveReadyText.text = "SPECIAL MOVE READY";
            yield return new WaitForSeconds(0.3f);
            specialMoveReadyText.text = "";
            yield return new WaitForSeconds(0.3f);
        }
    }

    /// <summary>
    /// Stop the blinking reload special move text coroutine
    /// </summary>
    private void StopBlinkingMoveReadyTextCoroutine()
    {
        if (blinkingReloadTextCoroutine != null)
        {
            specialMoveReadyText.text = "";
            StopCoroutine(blinkingReloadTextCoroutine);
            blinkingReloadTextCoroutine = null;
        }
    }
}
