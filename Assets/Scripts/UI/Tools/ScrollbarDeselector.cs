using UnityEngine;
using UnityEngine.EventSystems;

public class ScrollbarDeselector : MonoBehaviour, IBeginDragHandler, IEndDragHandler
{
    public void OnBeginDrag(PointerEventData eventData)
    {
        // Optional: could log or debug
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // Deselect the dragged object to stop auto-focus
        EventSystem.current.SetSelectedGameObject(null);
    }
}
