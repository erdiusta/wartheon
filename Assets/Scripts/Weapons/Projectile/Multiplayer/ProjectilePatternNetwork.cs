using Mirror;
using System.Collections;
using UnityEngine;

public class ProjectilePatternNetwork : NetworkBehaviour
{
    ProjectilePattern projectilePattern;

    private void Awake()
    {
        projectilePattern = GetComponent<ProjectilePattern>();
    }

    [Command(requiresAuthority = false)]
    public void CmdDestroyProjectilePattern()
    {
        if (GetComponentInChildren<Projectile>() == null)
        {
            Debug.LogError("To be destroyed projectile child is null");
            return;
        }

        foreach (Projectile projectile in GetComponentsInChildren<Projectile>())
        {
            projectile.Server_DestroyProjectile();
        }

        StartCoroutine(DestroyRoutine());
    }

    IEnumerator DestroyRoutine()
    {
        yield return new WaitForSeconds(1f);

        NetworkServer.Destroy(gameObject);
    }

    [ClientRpc]
    public void RpcInitializeProjectilePattern(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, float projectileSpeed, ProjectileKind projectileKind,
        AttackContext ctx, bool overrideProjectileMovement, bool fallingFromSkies, int projectileCounter, int projectilePerShot, int projectileIndex, uint ownerNetId, uint targetNetId)
    {
        projectilePattern.InitializeProjectile(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, null, ctx, overrideProjectileMovement: false,
            fallingFromSkies: false, projectileCounter - 1, projectilePerShot, projectileIndex, ownerNetId, targetNetId, null);
    }
}
