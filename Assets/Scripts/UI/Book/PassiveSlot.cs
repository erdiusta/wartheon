using UnityEngine;
using UnityEngine.EventSystems;

public class PassiveSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{    
    public PassiveItemDetailsSO passiveItemDetails;

    [HideInInspector] public bool passiveUnlocked = false;

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (passiveUnlocked)
        {
            StaticEventHandler.CallPassiveHoveredEvent(passiveItemDetails.passiveItemType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StaticEventHandler.CallPassiveUnhoveredEvent();
    }
}
