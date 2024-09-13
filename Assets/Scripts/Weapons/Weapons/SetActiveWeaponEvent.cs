using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SetActiveWeaponEvent : MonoBehaviour
{
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveWeapon;

    public void CallSetActiveWeaponEvent(Weapon weapon, RuntimeAnimatorController weaponAnimatorController)
    {
<<<<<<< Updated upstream
        OnSetActiveWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon, weaponAnimatorController = weaponAnimatorController});
=======
        OnSetActiveMainHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon, weaponSetIndex = weaponSetIndex }); 
    }

    public event Action<SetActiveWeaponEvent> OnSetInactiveMainHandWeapon;

    public void CallSetInactiveWeaponAtMainHandEvent()
    {
        OnSetInactiveMainHandWeapon?.Invoke(this);
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

    public event Action<SetActiveWeaponEvent> OnOneHandWeaponEquipped;

    public void CallOneHandWeaponEquipEvent()
    {
        OnOneHandWeaponEquipped?.Invoke(this);
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

    public event Action<SetActiveWeaponEvent, SetSelectedPassiveItemArgs> OnSelectedPassiveItem;

    public void CallSelectedPassiveItem(PassiveItem passiveItem)
    {
        OnSelectedPassiveItem?.Invoke(this, new SetSelectedPassiveItemArgs { passiveItem = passiveItem });
    }

    public event Action<SetActiveWeaponEvent> OnRemovedPassiveItem;

    public void CallRemovedPassiveItem()
    {
        OnRemovedPassiveItem?.Invoke(this);
>>>>>>> Stashed changes
    }
}

public class SetActiveWeaponEventArgs : EventArgs
{
    public Weapon weapon;
    public RuntimeAnimatorController weaponAnimatorController;
}
