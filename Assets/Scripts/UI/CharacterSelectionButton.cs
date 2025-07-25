using UnityEngine;
using UnityEngine.EventSystems;

public class CharacterSelectionButton : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public Character selectedCharacter;

    public void OnSelect(BaseEventData eventData)
    {
        StaticEventHandler.CallCharacterButtonSelectedEvent(selectedCharacter);
    }
    public void OnDeselect(BaseEventData eventData)
    {

        StaticEventHandler.CallCharacterButtonDeselectedEvent();
    }
}
