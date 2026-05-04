using Mirror;
using UnityEngine;

public class ProjectilePatternNetwork : NetworkBehaviour
{
    ProjectilePattern projectilePattern;

    private void Awake()
    {
        projectilePattern = GetComponent<ProjectilePattern>();
    }

    [ClientRpc]
    public void RpcInitializeProjectilePattern(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, float projectileSpeed, ProjectileKind projectileKind,
        AttackContext ctx, bool overrideProjectileMovement, bool fallingFromSkies, int projectileCounter, int projectilePerShot, uint netId, int projectileIndex, uint enemyNetId)
    {
        projectilePattern.InitializeProjectile(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, null, ctx, overrideProjectileMovement: false,
            fallingFromSkies: false, projectileCounter - 1, projectilePerShot, netId, projectileIndex, enemyNetId, null);
    }
}
