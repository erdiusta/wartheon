using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnAttack;

    public void CallAttackEvent(AimDirection aimDirection, Weapon weapon, MeleeAttackType meleeAttackType, MeleeHand meleeHand,bool isBloodDrain = false)
    {
        OnAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon, meleeAttackType = meleeAttackType, meleeHand = meleeHand,
            isBloodDrain = isBloodDrain });
    }
}

public class MeleeAttackEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public Weapon weapon;
    public MeleeAttackType meleeAttackType;
    public bool isBloodDrain;
    public MeleeHand meleeHand;
}