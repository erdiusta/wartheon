using System;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponFiredEvent : MonoBehaviour
{
    public event Action<WeaponFiredEvent, WeaponFiredEventArgs> OnWeaponFired;

    public void CallWeaponFiredEvent(Weapon weapon, bool mainHand)
    {
        OnWeaponFired?.Invoke(this, new WeaponFiredEventArgs { weapon = weapon, mainHand = mainHand });
    }
}

public class WeaponFiredEventArgs : EventArgs
{
    public Weapon weapon;
    public bool mainHand;
}