using UnityEngine;
using UnityEngine.EventSystems;

public class MobSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public EnemyDetailsSO mobDetails;
    public bool isBoss = false;

    [HideInInspector] public bool mobUnlocked = false;


    public void OnPointerEnter(PointerEventData eventData)
    {
        if (mobUnlocked)
        {
            StaticEventHandler.CallMobHoveredEvent(mobDetails.enemyCategory, isBoss);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        StaticEventHandler.CallMobUnhoveredEvent(isBoss);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (mobUnlocked)
        {
            StaticEventHandler.CallMobHoveredEvent(mobDetails.enemyCategory, isBoss);
        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        StaticEventHandler.CallMobUnhoveredEvent(isBoss);
    }
}
