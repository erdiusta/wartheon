using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class BuildSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [HideInInspector] public bool isSelected;

    public bool isLocked = true;
    public int indexNumber;

    Transform buildDescriptionContainer;
    Transform selectedBuildImageTransform;  

    private void Awake()
    {
        buildDescriptionContainer = transform.GetChild(transform.childCount - 1);
        selectedBuildImageTransform = transform.GetChild(0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isLocked)
        {
            StaticEventHandler.CallBuildInfoHoveredEvent(indexNumber);
            //buildDescriptionContainer.gameObject.SetActive(true);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isLocked)
        {
            StaticEventHandler.CallBuildInfoUnhoveredEvent(indexNumber);
            //buildDescriptionContainer.gameObject.SetActive(false);
        }
    }

    public void ActivateBuild()
    {
        if (GameManager.Instance.GetPlayer().currentBuildPoints > 0 && !isLocked && !isSelected)
        {
            isSelected = true;
            selectedBuildImageTransform.gameObject.SetActive(true);
            GameManager.Instance.GetPlayer().currentBuildPoints--;
            StaticEventHandler.CallBuildPointsUsed(indexNumber); // This is for activating build
                                                                 // ed image
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (!isLocked)
        {
            StaticEventHandler.CallBuildInfoHoveredEvent(indexNumber);
            //buildDescriptionContainer.gameObject.SetActive(true);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (!isLocked)
        {
            StaticEventHandler.CallBuildInfoUnhoveredEvent(indexNumber);
            //buildDescriptionContainer.gameObject.SetActive(false);
        }
    }
}

[System.Serializable]
public class CharacterBuildData
{
    public Character character;
    public BuildDetailsSO[] characterBuildList;
}
