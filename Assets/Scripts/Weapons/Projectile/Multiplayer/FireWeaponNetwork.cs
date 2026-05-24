using Mirror;
using UnityEngine;

[RequireComponent(typeof(FireWeapon))]
public class FireWeaponNetwork : NetworkBehaviour
{
    FireWeaponEvent fireWeaponEvent;
    FireWeapon fireWeapon;

    private void Awake()
    {
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        fireWeapon = GetComponent<FireWeapon>();
    }

    // Called by FireWeapon
    public void RequestFireWeapon(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, AttackContext attackContext, Vector3 shootPos, uint ownerNetId, uint targetNetId)
    {
        CmdFireWeapon(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, attackContext, shootPos, ownerNetId, targetNetId);
    }

    [Command]
    private void CmdFireWeapon(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, AttackContext attackContext, Vector3 shootPos, uint ownerNetId, uint targetNetId)
    {
        fireWeapon.ServerFireWeapon(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, attackContext, shootPos, ownerNetId, targetNetId);
    }
}
