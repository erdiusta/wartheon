using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UIDropdownScroller : MonoBehaviour, ISelectHandler
{
    ScrollRect scrollRect;
    float scrollPosition = 1f;

    private void Start()
    {
        scrollRect = GetComponentInParent<ScrollRect>(true);

        int childCount = scrollRect.content.transform.childCount - 1; // Substract the invisible one (template item)
        int childIndex = transform.GetSiblingIndex();

        childIndex = childIndex < ((float)childCount / 2) ? childIndex - 1 : childIndex;

        scrollPosition = 1 - ((float)childIndex / childCount); 
    }

    public void OnSelect(BaseEventData eventData)
    {
        if(scrollRect) scrollRect.verticalScrollbar.value = scrollPosition;
    }
}
