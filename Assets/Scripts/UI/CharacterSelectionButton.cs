using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelectionButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public void OnSelect(BaseEventData eventData)
    {
        StaticEventHandler.CallCharacterButtonSelectedEvent(tag);
    }
    public void OnDeselect(BaseEventData eventData)
    {

        StaticEventHandler.CallCharacterButtonDeselectedEvent();
    }
}
