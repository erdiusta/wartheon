using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableSkillIcon : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static bool IsDragging = false;

    public int skillIconIndexNumber = 0;
    
    [HideInInspector] public ActiveUniqueSkillDetailsSO activeUniqueSkillDetails;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Image image;

    Vector2 originalPosition;
    CanvasGroup canvasGroup;
    Canvas canvas;
    Player player;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        player = GameManager.Instance.GetPlayer();

        activeUniqueSkillDetails = player.playersAllActiveUniqueSkills[skillIconIndexNumber];
        image.sprite = activeUniqueSkillDetails.activeUniqueSkillSprite;
    }


    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        IsDragging = true;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;  // Reset the transparency
        canvasGroup.blocksRaycasts = true;  // Re-enable blocking raycasts

        rectTransform.anchoredPosition = originalPosition; // Reset position

        IsDragging = false;
    }
}
