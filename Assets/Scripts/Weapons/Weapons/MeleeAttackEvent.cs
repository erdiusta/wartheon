using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnAttack;

    public void CallAttackEvent(AimDirection aimDirection, Weapon weapon, AttackShape attackShape, MeleeHand meleeHand, bool isBloodDrain = false,
        bool shieldBash = false, bool isCullTheMeek = false, bool isSheerCold = false, bool isDontBlink = false, bool isBladeDash = false, bool isThrowingAxe = false)
    {
        OnAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon, attackShape = attackShape, meleeHand = meleeHand,
            isBloodDrain = isBloodDrain, shieldBash = shieldBash, isCullTheMeek = isCullTheMeek, isSheerCold = isSheerCold, isDontBlink = isDontBlink,
            isBladeDash = isBladeDash, isThrowingAxe = isThrowingAxe});
    }
}

public class MeleeAttackEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public Weapon weapon;
    public AttackShape attackShape;
    public MeleeHand meleeHand;
    public bool isBloodDrain;
    public bool shieldBash;
    public bool isCullTheMeek;
    public bool isSheerCold;
    public bool isDontBlink;
    public bool isBladeDash;
    public bool isThrowingAxe;
}