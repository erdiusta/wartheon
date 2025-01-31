using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SetActiveWeaponEvent : MonoBehaviour
{
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveMainHandWeapon;

    public void CallSetActiveWeaponAtMainHandEvent(Weapon weapon, int weaponSetIndex)
    {
        OnSetActiveMainHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon, weaponSetIndex = weaponSetIndex }); 
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetInactiveMainHandWeapon;

    public void CallSetInactiveWeaponAtMainHandEvent(bool isWeaponSwapping = false)
    {
        OnSetInactiveMainHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { isWeaponSwapping = isWeaponSwapping});
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveOffHandWeapon;

    public void CallSetActiveWeaponAtOffHandEvent(Weapon weapon, int weaponSetIndex)
    {
        OnSetActiveOffHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon, weaponSetIndex = weaponSetIndex });
    }

    public event Action<SetActiveWeaponEvent> OnSetInactiveOffHandWeapon;

    public void CallSetInactiveWeaponAtOffHandEvent()
    {
        OnSetInactiveOffHandWeapon?.Invoke(this);
    }

    public event Action<SetActiveWeaponEvent> OnTwoHandWeaponEquipped;

    public void CallTwoHandWeaponEquipEvent()
    {
        OnTwoHandWeaponEquipped?.Invoke(this);
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnOneHandWeaponEquipped;

    public void CallOneHandWeaponEquipEvent(bool isWeaponSwapping = false)
    {
        OnOneHandWeaponEquipped?.Invoke(this, new SetActiveWeaponEventArgs { isWeaponSwapping = isWeaponSwapping});
    }

    public event Action<SetActiveWeaponEvent, SetSelectedActiveItemArgs> OnSelectedActiveItem;

    public void CallSelectedActiveItem(ActiveItem activeItem)
    {
        OnSelectedActiveItem?.Invoke(this, new SetSelectedActiveItemArgs { activeItem = activeItem });
    }

    public event Action<SetActiveWeaponEvent> OnRemovedActiveItem;

    public void CallRemovedActiveItem()
    {
        OnRemovedActiveItem?.Invoke(this);
    }
}

public class SetActiveWeaponEventArgs : EventArgs
{
    public Weapon weapon;
    public int weaponSetIndex;
    public bool isWeaponSwapping;
}

public class SetSelectedActiveItemArgs : EventArgs
{
    public ActiveItem activeItem;
}