using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Canvas canvas;
    GridLayoutGroup gridLayoutGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        gridLayoutGroup = GetComponentInParent<GridLayoutGroup>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.enabled = false;
        }

        canvasGroup.alpha = 0.6f;  // Make the item semi-transparent during drag
        canvasGroup.blocksRaycasts = false;  // Disable blocking raycasts so we can drop it on the drop area
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;  // Reset the transparency
        canvasGroup.blocksRaycasts = true;  // Re-enable blocking raycasts

        if (gridLayoutGroup != null)
        {
            gridLayoutGroup.enabled = true;
        }
    }
}
