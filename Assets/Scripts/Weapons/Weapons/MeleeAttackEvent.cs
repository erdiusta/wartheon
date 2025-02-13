using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnRightHandMeleeAttack;
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnLeftHandMeleeAttack;

    public void CallMainHandWeaponAnimEvent(AimDirection aimDirection, Weapon weapon, MeleeAttackType meleeAttackType, bool isBloodDrain = false)
    {
        OnRightHandMeleeAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon, meleeAttackType = meleeAttackType, isBloodDrain = isBloodDrain });
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
    public bool isBloodDrain;
}