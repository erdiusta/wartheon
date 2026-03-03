using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class ProjectileNetwork : NetworkBehaviour
{
    Projectile projectile;

    private void Awake()
    {
        projectile = GetComponent<Projectile>();
    }

    [ClientRpc]
    public void RpcInitializeProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, float projectileSpeed, ProjectileKind projectileKind,
        AttackContext ctx, bool overrideProjectileMovement, bool fallingFromSkies, int projectileCounter, int projectilePerShot, uint netId, int projectileIndex, uint enemyNetId)
    {
        projectile.InitializeProjectile(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, null, ctx, overrideProjectileMovement: false,
            fallingFromSkies: false, projectileCounter - 1, projectilePerShot, netId, projectileIndex, enemyNetId, null);

    }
}
