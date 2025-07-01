using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActiveSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public ActiveItemDetailsSO activeItemDetails;

    [HideInInspector] public bool activeUnlocked = false;

    private void Start()
    {
        switch (GameManager.Instance.GetPlayer().playerDetails.playerCharacterIndex)
        {
            case Character.Caelion:
                if (activeItemDetails.activeItemType == ActiveItemType.Decoy)
                {
                    activeUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Morven:
                if (activeItemDetails.activeItemType == ActiveItemType.BobbyPin)
                {
                    activeUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Nyveran:
                if (activeItemDetails.activeItemType == ActiveItemType.Boomerang)
                {
                    activeUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Lyrisa:
                if (activeItemDetails.activeItemType == ActiveItemType.Pentagram)
                {
                    activeUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            default:
                break;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (activeUnlocked)
        {
            StaticEventHandler.CallActiveHoveredEvent(activeItemDetails.activeItemType);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StaticEventHandler.CallActiveUnhoveredEvent();
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (activeUnlocked)
        {
            StaticEventHandler.CallActiveHoveredEvent(activeItemDetails.activeItemType);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StaticEventHandler.CallActiveUnhoveredEvent();
    }
}
