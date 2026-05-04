using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

[DisallowMultipleComponent]
public class ManaUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject ManaImage")]
    #endregion Tooltip
    [SerializeField] Image manaBar;
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject ReservedManaImage")]
    #endregion Tooltip
    [SerializeField] Image reservedManaBar;
    #region Tooltip
    [Tooltip("Populate with healthText")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI manaText;

    [Space(10)]
    [SerializeField] Sprite standardSprite;
    [SerializeField] Sprite flashSprite;

    Player player;
    Coroutine playerManaBarCoroutine;

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        Subscribe();
        UpdateManaText();
    }

    private void Subscribe()
    {
        player.manaEvent.OnManaChanged += ManaEvent_OnManaChanged;
        player.manaEvent.OnReservedManaReset += ManaEvent_OnReservedManaReset;
    }

    private void Unsubscribe()
    {
        player.manaEvent.OnManaChanged -= ManaEvent_OnManaChanged;
        player.manaEvent.OnReservedManaReset -= ManaEvent_OnReservedManaReset;
    }

    private void ManaEvent_OnManaChanged(ManaEvent manaEvent, ManaEventArgs manaEventArgs)
    {
        UpdateMana(manaEventArgs);
    }

    private void ManaEvent_OnReservedManaReset(ManaEvent manaEvent, ManaEventArgs manaEventArgs)
    {
        ResetReservedMana();
    }

    private void UpdateMana(ManaEventArgs manaEventArgs)
    {
        UpdateManaText();
        UpdateManaBar(manaEventArgs.manaReserved);
    }

    private void UpdateManaText()
    {
        int mana = Mathf.Clamp(player.mana.GetCurrentMana(), 0, player.mana.GetMaximumMana());
        manaText.text = $"{mana}/{player.mana.GetMaximumMana()}";
    }

    private void UpdateManaBar(bool manaReserved)
    {
        if (gameObject.activeInHierarchy)
        {
            if (playerManaBarCoroutine != null) StopCoroutine(playerManaBarCoroutine);

            StartCoroutine(UpdateManaBarRoutine(manaReserved));
        }
    }

    private void ResetReservedMana()
    {
        StartCoroutine(UpdateManaBarRoutine(true, true));

        //if (player.mana.GetReservedMana() <= 0) reservedManaBar.gameObject.SetActive(false);

        UpdateManaText();
    }

    /// <summary>
    /// Animate mana bar coroutine
    /// </summary>
    private IEnumerator UpdateManaBarRoutine(bool manaReserved, bool manaReserveReset = false)
    {
        float duration = 0.6f;
        float elapsed = 0f;

        float maxMana = player.mana.GetMaximumMana();
        float currentMana = Mathf.Clamp(player.mana.GetCurrentMana(), 0, maxMana);
        float currentReservedMana = Mathf.Clamp(player.mana.GetReservedMana(), 0, maxMana);

        float targetManaFill = currentMana / maxMana;
        float targetReservedManaFill = currentReservedMana / maxMana;

        float startManaFill = manaBar.transform.localScale.x;
        float startReservedManaFill = reservedManaBar.transform.localScale.x;

        RectTransform barParent = (RectTransform)manaBar.transform.parent;
        float totalWidth = barParent.rect.width;

        // Optional flash
        manaBar.sprite = flashSprite;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;

            // Interpolate fill amounts based on maxMana
            float currentManaFill = Mathf.Lerp(startManaFill, targetManaFill, t);
            float currentReservedManaFill = Mathf.Lerp(startReservedManaFill, targetReservedManaFill, t);

            // Apply bar widths
            manaBar.transform.localScale = new Vector3(currentManaFill, 1f, 1f);
            reservedManaBar.transform.localScale = new Vector3(currentReservedManaFill, 1f, 1f);

            // Calculate pixel width of mana
            float manaWidth = totalWidth * currentManaFill;

            // Shift reserved bar right after mana bar
            ((RectTransform)reservedManaBar.transform).anchoredPosition = new Vector2(manaWidth, 0f);

            yield return null;
        }

        // Apply final values using the confirmed working method
        ShiftReservedManaBar(maxMana, currentMana, currentReservedMana, totalWidth);

        manaBar.sprite = standardSprite;
        playerManaBarCoroutine = null;
    }

    private void ShiftReservedManaBar(float maxMana, float currentMana, float currentReservedMana, float totalWidth)
    {
        float manaRatio = currentMana / maxMana;
        float reservedRatio = currentReservedMana / maxMana;

        manaBar.transform.localScale = new Vector3(manaRatio, 1f, 1f);
        reservedManaBar.transform.localScale = new Vector3(reservedRatio, 1f, 1f);

        float manaWidth = totalWidth * manaRatio;
        ((RectTransform)reservedManaBar.transform).anchoredPosition = new Vector2(manaWidth, 0f);
    }
}
