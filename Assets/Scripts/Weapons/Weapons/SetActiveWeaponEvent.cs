using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SetActiveWeaponEvent : MonoBehaviour
{
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveWeapon;

    public void CallSetActiveWeaponEvent(Weapon weapon, RuntimeAnimatorController weaponAnimatorController)
    {
        OnSetActiveWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weapon = weapon, weaponAnimatorController = weaponAnimatorController});
    }
}

public class SetActiveWeaponEventArgs : EventArgs
{
    public Weapon weapon;
    public RuntimeAnimatorController weaponAnimatorController;
}
