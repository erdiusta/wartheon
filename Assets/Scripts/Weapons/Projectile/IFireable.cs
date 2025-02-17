using UnityEngine;

public interface IFireable
{
    void InitializeProjectile(Enemy belongingEnemy, bool headShotHappened ,ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, float projectileSpeed,
        Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false, bool fallingFromSkies = false, bool isPenetrationArrow = false, 
        int projectileCounter = 0, int totalProjectiles = 0, CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None,
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None);

    void InitializeProjectile(bool headShotHappened, ActiveItemDetailsSO activeItemDetails, float aimAngle, float weaponAimAngle, float projectileSpeed,
    Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false);

    GameObject GetGameObject();
}
