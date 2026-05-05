using System;
using UnityEngine;

[DisallowMultipleComponent]
public class SetActiveWeaponEvent : MonoBehaviour
{
    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveMainHandWeapon;
    public void CallSetActiveWeaponAtMainHandEvent(WeaponStats weaponStats, Rarity rarity, int weaponSetIndex, bool onStart, bool isStatUpdateAllowed, int onStartWeaponIndex = 0)
    {
        OnSetActiveMainHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weaponStats = weaponStats, rarity = rarity, weaponSetIndex = weaponSetIndex, onStart = onStart, 
            isStatUpdateAllowed = isStatUpdateAllowed, onStartWeaponIndex = onStartWeaponIndex}); 
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveMainHandWeaponForHud;
    public void CallSetActiveWeaponAtMainHandEventForHud(WeaponStats weaponStats, Rarity rarity, int weaponSetIndex, bool onStart, int onStartWeaponIndex = 0)
    {
        OnSetActiveMainHandWeaponForHud?.Invoke(this, new SetActiveWeaponEventArgs { weaponStats = weaponStats, rarity = rarity, weaponSetIndex = weaponSetIndex, onStart = onStart, onStartWeaponIndex = onStartWeaponIndex });
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetInactiveMainHandWeapon;
    public void CallSetInactiveWeaponAtMainHandEvent(bool isStatUpdateAllowed, bool isWeaponSwapping = false)
    {
        OnSetInactiveMainHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { isWeaponSwapping = isWeaponSwapping, isStatUpdateAllowed = isStatUpdateAllowed});
    }

    public event Action<SetActiveWeaponEvent> OnSetInactiveMainHandWeaponForHud;
    public void CallSetInactiveWeaponAtMainHandEventForHud()
    {
        OnSetInactiveMainHandWeaponForHud?.Invoke(this);
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveOffHandWeapon;
    public void CallSetActiveWeaponAtOffHandEvent(WeaponStats weaponStats, Rarity rarity, int weaponSetIndex, bool onStart, bool isStatUpdateAllowed)
    {
        OnSetActiveOffHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { weaponStats = weaponStats, rarity = rarity, weaponSetIndex = weaponSetIndex, onStart = onStart, isStatUpdateAllowed = isStatUpdateAllowed});
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetActiveOffHandWeaponForHud;
    public void CallSetActiveWeaponAtOffHandEventForHud(WeaponStats weaponStats, Rarity rarity, int weaponSetIndex, bool onStart)
    {
        OnSetActiveOffHandWeaponForHud?.Invoke(this, new SetActiveWeaponEventArgs { weaponStats = weaponStats, rarity = rarity, weaponSetIndex = weaponSetIndex, onStart = onStart });
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnSetInactiveOffHandWeapon;
    public void CallSetInactiveWeaponAtOffHandEvent(bool isStatUpdateAllowed)
    {
        OnSetInactiveOffHandWeapon?.Invoke(this, new SetActiveWeaponEventArgs { isStatUpdateAllowed = isStatUpdateAllowed });
    }

    public event Action<SetActiveWeaponEvent> OnSetInactiveOffHandWeaponForHud;
    public void CallSetInactiveWeaponAtOffHandEventForHud()
    {
        OnSetInactiveOffHandWeaponForHud?.Invoke(this);
    }

    public event Action<SetActiveWeaponEvent> OnTwoHandWeaponEquippedLockIconHud;
    public void CallTwoHandWeaponEquipEventForLockIconHud()
    {
        OnTwoHandWeaponEquippedLockIconHud?.Invoke(this);
    }

    public event Action<SetActiveWeaponEvent, SetActiveWeaponEventArgs> OnOneHandWeaponEquippedLockIconHud;
    public void CallOneHandWeaponEquipEventForLockIconHud(bool isWeaponSwapping = false)
    {
        OnOneHandWeaponEquippedLockIconHud?.Invoke(this, new SetActiveWeaponEventArgs { isWeaponSwapping = isWeaponSwapping});
    }
}

public class SetActiveWeaponEventArgs : EventArgs
{
    public WeaponStats weaponStats;
    public Rarity rarity;
    public int weaponSetIndex;
    public bool isWeaponSwapping;
    public bool onStart;
    public Sprite shieldSprite;
    public int onStartWeaponIndex;
    public bool isStatUpdateAllowed;
}