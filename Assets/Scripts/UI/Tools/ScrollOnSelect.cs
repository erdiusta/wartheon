using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ScrollOnSelect : MonoBehaviour, ISelectHandler
{
    [Header("Scroll Target")]
    [Tooltip("The ScrollRect that this UI element lives inside")]
    [SerializeField] ScrollRect scrollRect;

    [Tooltip("Optional offset to apply when scrolling (0 = center)")]
    [Range(-1f, 1f)]
    [SerializeField] float verticalOffset = 0f;

    public void OnSelect(BaseEventData eventData)
    {
        if (scrollRect == null) return;

        RectTransform content = scrollRect.content;
        RectTransform selected = GetComponent<RectTransform>();

        // Convert the selected element's position into local position within the content
        Vector2 localPosition = (Vector2)content.InverseTransformPoint(selected.position);

        // Calculate normalized vertical scroll position (0 = bottom, 1 = top)
        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        float elementCenterY = localPosition.y + (selected.rect.height / 2f);
        float scrollPos = Mathf.Clamp01((elementCenterY + verticalOffset * viewportHeight) / (contentHeight - viewportHeight));

        // Update scroll position
        scrollRect.verticalNormalizedPosition = scrollPos;
    }
}
