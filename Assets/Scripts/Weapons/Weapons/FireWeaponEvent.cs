using UnityEngine;
using System;

[DisallowMultipleComponent]
public class FireWeaponEvent : MonoBehaviour
{
    public event Action<FireWeaponEvent, FireWeaponEventArgs> OnFireWeapon;

    public void CallFireWeaponEvent(bool fire, bool firePreviousFrame,AimDirection aimDirection, float aimAngle, float weaponAimAngle, 
        Vector3 weaponAimDirectionVector, bool headShotHappened = false, bool isActiveItem = false, bool isPenetrationArrow = false)
    {
        OnFireWeapon?.Invoke(this, new FireWeaponEventArgs { fire = fire, firePreviousFrame = firePreviousFrame, aimDirection = aimDirection, aimAngle = aimAngle, 
            weaponAimAngle = weaponAimAngle, weaponAimDirectionVector = weaponAimDirectionVector, headShotHappened = headShotHappened, isActiveItem = isActiveItem,
            isPenetrationArrow = isPenetrationArrow});
    }
}

public class FireWeaponEventArgs : EventArgs
{
    public bool fire;
    public bool firePreviousFrame;
    public AimDirection aimDirection;
    public float aimAngle;
    public float weaponAimAngle;
    public Vector3 weaponAimDirectionVector;
    public bool headShotHappened;
    public bool isActiveItem;
    public bool isPenetrationArrow;
}
