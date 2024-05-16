using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SetActiveWeaponEvent : MonoBehaviour
{
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveRightHandWeapon;
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveLeftHandWeapon;
    public event Action<SetActiveWeaponEvent> OnSetInactiveLeftHandWeapon;
    public event Action<SetActiveWeaponEvent> OnTwoHandWeaponEquipped;
    public event Action<SetActiveWeaponEvent> OnOneHandWeaponEquipped;
    public event Action<SetActiveWeaponEvent, SetSelectedActiveItemArgs> OnSelectedActiveItem;

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

    public void CallSetActiveWeaponAtLeftHandEvent(Weapon weapon)
    {
        OnSetActiveLeftHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon });
    }

    public void CallSetInactiveWeaponAtLeftHandEvent()
    {
        OnSetInactiveLeftHandWeapon?.Invoke(this);
    }

    public void CallSelectedActiveItem(ActiveItem activeItem)
    {
        OnSelectedActiveItem?.Invoke(this, new SetSelectedActiveItemArgs { activeItem = activeItem});
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