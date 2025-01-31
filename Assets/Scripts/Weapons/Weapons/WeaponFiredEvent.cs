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

    public event Action<WeaponFiredEvent, ActiveItemFiredEventArgs> OnActiveItemFired;

    public void CallActiveItemFiredEvent(ActiveItem activeItem)
    {
        OnActiveItemFired?.Invoke(this, new ActiveItemFiredEventArgs { activeItem = activeItem});
    }
}

public class WeaponFiredEventArgs : EventArgs
{
    public Weapon weapon;
    public bool mainHand;
}

public class ActiveItemFiredEventArgs : EventArgs
{
    public ActiveItem activeItem;
}
