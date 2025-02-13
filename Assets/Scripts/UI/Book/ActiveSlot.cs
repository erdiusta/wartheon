using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ActiveSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public ActiveItemDetailsSO activeItemDetails;

    [HideInInspector] public bool activeUnlocked = false;

    private void Start()
    {
        switch (GameManager.Instance.GetPlayer().playerDetails.playerCharacterIndex)
        {
            case Character.Astraeus:
                if (activeItemDetails.activeItemType == ActiveItemType.Dummy)
                {
                    activeUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Erebus:
                if (activeItemDetails.activeItemType == ActiveItemType.BobbyPin)
                {
                    activeUnlocked = true;
                    transform.GetComponent<Image>().color = Color.white;
                }
                break;
            case Character.Orion:
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
}
