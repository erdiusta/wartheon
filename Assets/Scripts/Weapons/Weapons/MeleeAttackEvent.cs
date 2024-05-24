using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnRightHandMeleeAttack;
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnLeftHandMeleeAttack;

    public void CallRightHandWeaponAnimEvent(AimDirection aimDirection, Weapon weapon)
    {
        OnRightHandMeleeAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon });
    }

    public void CallLeftHandWeaponAnimEvent(AimDirection aimDirection, Weapon weapon)
    {
        OnLeftHandMeleeAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon });
    }
}

public class MeleeAttackEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public Weapon weapon;
}