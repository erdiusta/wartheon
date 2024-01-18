using UnityEngine;

public interface IFireable
{
    void InitializeProjectile(ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, float projectileSpeed,
        Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false);

    GameObject GetGameObject();
}
