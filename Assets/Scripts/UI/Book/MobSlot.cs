using UnityEngine;
using UnityEngine.EventSystems;

public class MobSlot : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
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
}
