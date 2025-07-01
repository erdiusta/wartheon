using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnAttack;

    public void CallAttackEvent(AimDirection aimDirection, Weapon weapon, MeleeAttackType meleeAttackType, MeleeHand meleeHand, bool isBloodDrain = false,
        bool shieldBash = false, bool isCullTheMeek = false)
    {
        OnAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon, meleeAttackType = meleeAttackType, meleeHand = meleeHand,
            isBloodDrain = isBloodDrain, shieldBash = shieldBash, isCullTheMeek = isCullTheMeek });
    }
}

public class MeleeAttackEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public Weapon weapon;
    public MeleeAttackType meleeAttackType;
    public MeleeHand meleeHand;
    public bool isBloodDrain;
    public bool shieldBash;
    public bool isCullTheMeek;
}