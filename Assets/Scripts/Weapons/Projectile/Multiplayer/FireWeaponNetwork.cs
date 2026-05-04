using Mirror;
using UnityEngine;

[RequireComponent(typeof(FireWeapon))]
public class FireWeaponNetwork : NetworkBehaviour
{
    FireWeapon fireWeapon;

    private void Awake()
    {
        fireWeapon = GetComponent<FireWeapon>();
    }

    // Called by FireWeapon
    public void RequestFireWeapon(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, AttackContext attackContext)
    {
        CmdFireWeapon(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, attackContext);
    }

    [Command]
    private void CmdFireWeapon(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, AttackContext attackContext)
    {
        fireWeapon.ServerFireWeapon(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, attackContext);
    }
}
