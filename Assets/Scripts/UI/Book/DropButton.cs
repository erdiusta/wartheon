using UnityEngine;
using UnityEngine.EventSystems;

public class DropButton : MonoBehaviour, IDropHandler
{
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            // Handle the logic for dropping the item
            Debug.Log("Item dropped on the drop button");

            // You can destroy the dropped item or handle it in another way
            Destroy(eventData.pointerDrag);
        }
    }
}
