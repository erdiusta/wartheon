using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class ProjectileNetwork : NetworkBehaviour
{
    [SyncVar] public uint ownerNetId;

    Projectile projectile;

    Player owner;

    private void Awake()
    {
        projectile = GetComponent<Projectile>();
    }

    public override void OnStartServer()
    {
        ResolveOwnerServer();
    }   

    void ResolveOwnerServer()
    {
        if (NetworkServer.spawned.TryGetValue(ownerNetId, out var identity))
        {
            owner = identity.GetComponent<Player>();
        }
    }

    [ClientRpc]
    public void RpcInitializeProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, float projectileSpeed, ProjectileKind projectileKind,
        AttackContext ctx, bool overrideProjectileMovement, bool fallingFromSkies, int projectileCounter, int projectilePerShot, int projectileIndex, uint ownerNetId,  uint targetNetId)
    {
        projectile.InitializeProjectile(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, null, ctx, overrideProjectileMovement: false,
            fallingFromSkies: false, projectileCounter - 1, projectilePerShot, projectileIndex, ownerNetId,  targetNetId, null);
    }

    [ClientRpc]
    public void RpcDisableProjectile()
    {
        StopAllCoroutines();

        projectile.isHittingWall = false;
        gameObject.SetActive(false);
    }
}
