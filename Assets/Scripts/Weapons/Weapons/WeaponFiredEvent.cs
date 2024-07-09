using System;
using UnityEngine;

[DisallowMultipleComponent]
public class WeaponFiredEvent : MonoBehaviour
{
    public event Action<WeaponFiredEvent, WeaponFiredEventArgs> OnWeaponFired;

    public void CallWeaponFiredEvent(Weapon weapon)
    {
        OnWeaponFired?.Invoke(this, new WeaponFiredEventArgs { weapon = weapon });
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
}

public class ActiveItemFiredEventArgs : EventArgs
{
    public ActiveItem activeItem;
}
