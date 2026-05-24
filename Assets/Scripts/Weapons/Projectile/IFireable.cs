using UnityEngine;

public interface IFireable
{
    void InitializeProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, float projectileSpeed, ProjectileKind projectileKind, ProjectileDetailsSO projectileDetails, AttackContext attackContext,
        bool overrideProjectileMovement, bool fallingFromSkies, int projectileCounter, int projectilePerShot, int projectileIndex, uint ownerNetId, uint enemyNetId, Enemy ownerEnemyForSP = null);

    GameObject GetGameObject();
}
