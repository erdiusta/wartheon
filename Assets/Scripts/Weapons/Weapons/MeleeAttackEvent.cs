using UnityEngine;
using System;

public class MeleeAttackEvent : MonoBehaviour
{
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnRightHandMeleeAttack;
    public event Action<MeleeAttackEvent, MeleeAttackEventArgs> OnLeftHandMeleeAttack;

<<<<<<< Updated upstream
    public void CallRightHandMeleeAttackEvent(AimDirection aimDirection, Weapon weapon)
=======
    public void CallRightHandWeaponAnimEvent(AimDirection aimDirection, Weapon weapon)
>>>>>>> Stashed changes
    {
        OnRightHandMeleeAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon });
    }

<<<<<<< Updated upstream
    public void CallLeftHandMeleeAttackEvent(AimDirection aimDirection, Weapon weapon)
=======
    public void CallLeftHandWeaponAnimEvent(AimDirection aimDirection, Weapon weapon)
>>>>>>> Stashed changes
    {
        OnLeftHandMeleeAttack?.Invoke(this, new MeleeAttackEventArgs { aimDirection = aimDirection, weapon = weapon });
    }
}

public class MeleeAttackEventArgs : EventArgs
{
    public AimDirection aimDirection;
    public Weapon weapon;
}