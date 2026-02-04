using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NeutralizeSpriteSelection : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    Button button;
    bool selectedSpriteHighlighted = false;
    bool initialPhase = true;

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Clear selection if this is currently selected
        if (EventSystem.current.currentSelectedGameObject == gameObject)
        {
            // Force the selectedSpriteHighlighted sprite to look like highlighted (sprite1)
            if (!selectedSpriteHighlighted)
            {
                SpriteState state = button.spriteState;
                state.selectedSprite = state.highlightedSprite; // or state.selectedSprite = null;
                button.spriteState = state;
                selectedSpriteHighlighted = true;
            }
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Force the selected sprite to look like normal(sprite0)
        if (selectedSpriteHighlighted)
        {
            SpriteState state = button.spriteState;
            state.selectedSprite = state.disabledSprite; // or state.selectedSprite = null;
            button.spriteState = state;
            selectedSpriteHighlighted = false;
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!initialPhase)
        {
            // Force the selectedSpriteHighlighted sprite to look like highlighted (sprite1)
            SpriteState state = button.spriteState;
            state.selectedSprite = state.highlightedSprite; // or state.selectedSprite = null;
            button.spriteState = state;
            selectedSpriteHighlighted = true;
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Force the selectedSpriteHighlighted sprite to look like highlighted (sprite1)
        SpriteState state = button.spriteState;
        state.selectedSprite = state.highlightedSprite; // or state.selectedSprite = null;
        button.spriteState = state;
        selectedSpriteHighlighted = true;
        initialPhase = false;
    }
}
