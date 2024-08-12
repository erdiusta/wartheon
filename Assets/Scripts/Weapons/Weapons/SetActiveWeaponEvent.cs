using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SetActiveWeaponEvent : MonoBehaviour
{
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveRightHandWeapon;
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveOffHandWeapon;
    public event Action<SetActiveWeaponEvent> OnSetInactiveOffHandWeapon;
    public event Action<SetActiveWeaponEvent> OnTwoHandWeaponEquipped;
    public event Action<SetActiveWeaponEvent> OnOneHandWeaponEquipped;
    public event Action<SetActiveWeaponEvent, SetSelectedActiveItemArgs> OnSelectedActiveItem;
    public event Action<SetActiveWeaponEvent> OnRemovedActiveItem;

    public void CallTwoHandWeaponEquipEvent()
    {
        OnTwoHandWeaponEquipped?.Invoke(this);
    }

    public void CallOneHandWeaponEquipEvent()
    {
        OnOneHandWeaponEquipped?.Invoke(this);
    }

    public void CallSetActiveWeaponAtRightHandEvent(Weapon weapon)
    {
        OnSetActiveRightHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon });
    }

    public void CallSetActiveWeaponAtOffHandEvent(Weapon weapon)
    {
        OnSetActiveOffHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon });
    }

    public void CallSetInactiveWeaponAtOffHandEvent()
    {
        OnSetInactiveOffHandWeapon?.Invoke(this);
    }

    public void CallSelectedActiveItem(ActiveItem activeItem)
    {
        OnSelectedActiveItem?.Invoke(this, new SetSelectedActiveItemArgs { activeItem = activeItem});
    }

    public void CallRemovedActiveItem()
    {
        OnRemovedActiveItem?.Invoke(this);
    }
}

public class SetActiveWeaponEventArgs : EventArgs
{
    public Weapon weapon;
}

public class SetSelectedActiveItemArgs : EventArgs
{
    public ActiveItem activeItem;
}