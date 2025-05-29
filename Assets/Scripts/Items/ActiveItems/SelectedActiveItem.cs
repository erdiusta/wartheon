using UnityEngine;

public class SelectedActiveItem : MonoBehaviour
{
    SetActiveItemEvent setActiveItemEvent;
    ActiveItem currentActiveItem;

    private void Awake()
    {
        setActiveItemEvent = GetComponent<SetActiveItemEvent>();
    }

    private void OnEnable()
    {

        if (setActiveItemEvent == null)
        {
            Debug.LogError("setActiveItemEvent is null on " + gameObject.name);
            return;
        }

        setActiveItemEvent.OnSelectedActiveItem += SetActiveItemEvent_OnSelectedActiveItem;
        setActiveItemEvent.OnRemovedActiveItem += SetActiveItemEvent_OnRemovedActiveItem;
    }

    private void OnDisable()
    {
        setActiveItemEvent.OnSelectedActiveItem -= SetActiveItemEvent_OnSelectedActiveItem;
        setActiveItemEvent.OnRemovedActiveItem -= SetActiveItemEvent_OnRemovedActiveItem;
    }

    private void SetActiveItemEvent_OnSelectedActiveItem(SetActiveItemEvent setActiveItemEvent, SetSelectedActiveItemArgs setSelectedActiveItemArgs)
    {
        Debug.Log($"[Event Triggered] by {gameObject.name}", gameObject);

        SetActiveItem(setSelectedActiveItemArgs.activeItem);

        StaticEventHandler.CallItemAddedToActiveItemSlot(setSelectedActiveItemArgs.activeItem);
        StaticEventHandler.CallActiveUnlockedEvent(setSelectedActiveItemArgs.activeItem.activeItemDetails.activeItemType);
    }

    private void SetActiveItemEvent_OnRemovedActiveItem(SetActiveItemEvent setActiveItemEvent)
    {
        currentActiveItem = null;
        StaticEventHandler.CallItemRemovedFromActiveItemSlot();
    }

    private void SetActiveItem(ActiveItem activeItem)
    {
        currentActiveItem = activeItem;
    }

    public ActiveItem GetCurrentActiveItem()
    {
        return currentActiveItem;
    }

    public void IncreaseRemainingProjectile(int projectileQuantity)
    {
        currentActiveItem.activeItemRemainingCharge += projectileQuantity;
    }
}
