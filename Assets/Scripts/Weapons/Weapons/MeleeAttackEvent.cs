using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnRightHandMeleeAttack;
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnLeftHandMeleeAttack;

    public void CallMainHandWeaponAnimEvent(AimDirection aimDirection, Weapon weapon, MeleeAttackType meleeAttackType)
    {
        OnRightHandMeleeAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon, meleeAttackType = meleeAttackType });
    }

    public void CallOffHandWeaponAnimEvent(AimDirection aimDirection, Weapon weapon, MeleeAttackType meleeAttackType)
    {
        OnLeftHandMeleeAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon, meleeAttackType = meleeAttackType});
    }
}

public class MeleeAttackEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public Weapon weapon;
    public MeleeAttackType meleeAttackType;
}