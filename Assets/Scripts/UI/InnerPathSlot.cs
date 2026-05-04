using UnityEngine;
using UnityEngine.EventSystems;

public class InnerPathSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    [HideInInspector] public bool isSelected;

    public int indexNumber;
    public InnerPathDetailsSO innerPathDetails;

    Transform selectedBuildImageTransform;  

    private void Awake()
    {
        selectedBuildImageTransform = transform.GetChild(0);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        StaticEventHandler.CallInnerPathInfoHoveredEvent(innerPathDetails);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StaticEventHandler.CallInnerPathInfoUnhoveredEvent(innerPathDetails);
    }

    public void ActivateBuild()
    {
        if (GameManager.Instance.GetLocalPlayer().currentSkillPoints > 0 && !isSelected)
        {
            isSelected = true;
            selectedBuildImageTransform.gameObject.SetActive(true);
            GameManager.Instance.GetLocalPlayer().currentSkillPoints--;
            StaticEventHandler.CallSkillPointsUsed(innerPathDetails); // This is for activating build
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        StaticEventHandler.CallInnerPathInfoHoveredEvent(innerPathDetails);
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StaticEventHandler.CallInnerPathInfoUnhoveredEvent(innerPathDetails);
    }
}
