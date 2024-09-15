using UnityEngine;

public interface IFireable
{
    void InitializeProjectile(bool headShotHappened ,ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, float projectileSpeed,
        Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false, bool fallingFromSkies = false, bool isPenetrationArrow = false);

    void InitializeProjectile(bool headShotHappened, ActiveItemDetailsSO activeItemDetails, float aimAngle, float weaponAimAngle, float projectileSpeed,
    Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false);

    GameObject GetGameObject();
}
