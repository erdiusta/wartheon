using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using System.Collections;

[RequireComponent(typeof(RectTransform), typeof(CanvasGroup), typeof(Image))]
public class DraggableSkillIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IPointerEnterHandler, IPointerExitHandler
{
    public static bool IsDragging = false;

    bool isHighlighted;

    [Header("Binding")]
    public int skillIconIndexNumber = 0;
    public Image frameImage;
    public SoundEffectSO clickButtonSound;

    [HideInInspector] public ActiveUniqueSkillDetailsSO activeUniqueSkillDetails;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Image image;

    Vector2 originalPosition;
    CanvasGroup canvasGroup;
    Canvas canvas;
    Player player;

    int skillLevel = 1;
    bool uiReady;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>(true);
    }

    private void OnEnable()
    {
        uiReady = false;

        player = GameManager.Instance.GetPlayer();

        // Pull the SO by index (already prepared on Player)
        if(skillIconIndexNumber > 0)
        {
            activeUniqueSkillDetails = player.playersAllActiveUniqueSkills[skillIconIndexNumber - 1];

            // Set icon sprite
            if (activeUniqueSkillDetails != null) image.sprite = activeUniqueSkillDetails.activeUniqueSkillSprite;

            // Sync local level from SO and update frame
            int maxLevel = GetMaxLevel();
            skillLevel = Mathf.Clamp(activeUniqueSkillDetails?.GetCurrentActiveLevel() ?? 1, 1, maxLevel);
            UpdateFrameSprite();

            // Avoid accidental clicks during enable
            StartCoroutine(EnableClicksNextFrame());
        }
        else if (skillIconIndexNumber == 0)
        {
            activeUniqueSkillDetails = player.playerDetails.passiveSkillDetails;
            image.sprite = player.playerDetails.passiveSkillDetails.activeUniqueSkillSprite;
        }
    }

    private void OnDisable()
    {
        uiReady = false;
    }

    IEnumerator EnableClicksNextFrame()
    {
        yield return null;
        uiReady = true;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (player == null || activeUniqueSkillDetails == null) return;

        StaticEventHandler.CallUniqueSkillInfoHoveredEvent(activeUniqueSkillDetails);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StaticEventHandler.CallUniqueSkillInfoUnhoveredEvent(activeUniqueSkillDetails);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (skillIconIndexNumber == 0) return;

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (skillIconIndexNumber == 0) return;

        IsDragging = true;

        float scale = canvas ? canvas.scaleFactor : 1f;
        rectTransform.anchoredPosition += eventData.delta / Mathf.Max(scale, 0.0001f);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;  // Reset the transparency
        canvasGroup.blocksRaycasts = true;  // Re-enable blocking raycasts

        rectTransform.anchoredPosition = originalPosition; // Reset position

        IsDragging = false;
    }

    public void OnPickWithGamePad()
    {
        if (activeUniqueSkillDetails == null) return;

        SkillSelectionManager.Pick(this);
        PlayButtonClickSound();
    }

    public void Highlight(bool on) => isHighlighted = on;

    public void SkillBoost()
    {
        if (!uiReady) return;
        if (player == null || activeUniqueSkillDetails == null) return;

        if (player.currentSkillPoints <= 0) return;

        if (skillLevel >= GetMaxLevel()) return;

        // Apply upgrade
        skillLevel++;
        UpdateFrameSprite();

        // Sync SO (your setter is incremental)
        int currentSOLevel = activeUniqueSkillDetails.GetCurrentActiveLevel();
        int delta = skillLevel - currentSOLevel;
        if (delta != 0) activeUniqueSkillDetails.SetCurrentActiveLevel(delta);

        player.currentSkillPoints--;

        // Notify AFTER successful upgrade
        StaticEventHandler.CallSkillBoostUsed(skillLevel);
        StaticEventHandler.CallUniqueSkillInfoHoveredEvent(activeUniqueSkillDetails);
    }

    /// <summary>
    /// SO defaults to 3 levels; this will handle any future count too
    /// </summary>
    private int GetMaxLevel() => Mathf.Max(1, activeUniqueSkillDetails?.levels?.Count ?? 1);

    private void UpdateFrameSprite()
    {
        if (frameImage == null) return;

        // Map 1..3 -> specific frames; >=3 uses level three frame
        if (skillIconIndexNumber > 0)
        {
            frameImage.sprite = skillLevel switch
            {
                1 => GameResources.Instance.levelOneFrameSprite,
                2 => GameResources.Instance.levelTwoFrameSprite,
                _ => GameResources.Instance.levelThreeFrameSprite
            };
        }
    }

    public void PlayButtonClickSound()
    {
        SoundEffectManager.Instance.PlaySoundEffect(clickButtonSound);
    }
}
