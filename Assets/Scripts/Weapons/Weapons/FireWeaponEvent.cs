using UnityEngine;
using System;

[DisallowMultipleComponent]
public class FireWeaponEvent : MonoBehaviour
{
    public event Action<FireWeaponEvent, FireWeaponEventArgs> OnFireWeapon;

    public void CallFireWeaponEvent(bool fire, bool firePreviousFrame, AimDirection aimDirection ,float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser, 
        ProjectileKind projectileKind, AttackContext attackContext, uint enemyNetId, Enemy belongingEnemy = null)
    {
        OnFireWeapon?.Invoke(this, new FireWeaponEventArgs
        {
            fire = fire,
            firePreviousFrame = firePreviousFrame,
            aimDirection = aimDirection,
            aimAngle = aimAngle,
            weaponAimAngle = weaponAimAngle,
            weaponAimDirectionVector = weaponAimDirectionVector,
            isLaser = isLaser,
            projectileKind = projectileKind,
            attackContext = attackContext,
            enemyNetId = enemyNetId,
            belongingEnemy = belongingEnemy,
        });
    }

    public event Action<FireWeaponEvent, FireFocusedShotEventArgs> OnFocousedAim;

    public void CallFocusedAimEvent(Vector3 lockedTargetVector, float lockedAngle)
    {
        OnFocousedAim?.Invoke(this, new FireFocusedShotEventArgs { lockedTargetVector = lockedTargetVector, lockedAngle = lockedAngle });
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
    public bool isLaser;
    public ProjectileKind projectileKind;
    public AttackContext attackContext;
    public uint enemyNetId;
    public Enemy belongingEnemy;
}

public class FireFocusedShotEventArgs : EventArgs
{
    public Vector3 lockedTargetVector;
    public float lockedAngle;
}
