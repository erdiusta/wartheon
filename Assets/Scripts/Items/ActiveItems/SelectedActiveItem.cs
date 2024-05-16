using System;
using UnityEngine;

public class SelectedActiveItem : MonoBehaviour
{
    SetActiveWeaponEvent setActiveWeaponEvent;
    ActiveItem currentActiveItem;

    private void Awake()
    {
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    private void OnEnable()
    {
        setActiveWeaponEvent.OnSelectedActiveItem += SetActiveWeaponEvent_OnSelectedActiveItem;
    }

    private void OnDisable()
    {
        setActiveWeaponEvent.OnSelectedActiveItem -= SetActiveWeaponEvent_OnSelectedActiveItem;
    }

    private void SetActiveWeaponEvent_OnSelectedActiveItem(SetActiveWeaponEvent setActiveWeaponEvent, SetSelectedActiveItemArgs setSelectedActiveItemArgs)
    {
        SetActiveItem(setSelectedActiveItemArgs.activeItem);
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
        currentActiveItem.activeItemRemainingProjectile += projectileQuantity;
    }
}
