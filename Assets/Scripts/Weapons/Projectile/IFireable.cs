using UnityEngine;

public interface IFireable
{
    void InitializeProjectile(Enemy belongingEnemy, bool isIceBreaker ,ProjectileDetailsSO projectileDetails, float aimAngle, float weaponAimAngle, float projectileSpeed,
        Vector3 weaponAimDirectionVector, bool overrideProjectileMovement = false, bool fallingFromSkies = false, bool isPenetrationArrow = false, 
        int projectileCounter = 0, int totalProjectiles = 0, MoravellePhase moravellePhase = MoravellePhase.None, SylvarokPhase treantPhase = SylvarokPhase.None,
        GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None, CryotharPhase frostWrymPhase = CryotharPhase.None,
        VenomancerPhase venomancerPhase = VenomancerPhase.None, PyrotharPhase fireWrymPhase = PyrotharPhase.None, MoldranPhase moldranPhase = MoldranPhase.None,
        bool isTripleThreat = false, bool isBindingArrow = false, bool isArrowOfTheSeven = false, ProjectileDetailsSO grappleDetails = null,
        ProjectileDetailsSO iceBreakerDetails = null, bool isFireBlast = false, ProjectileDetailsSO fireBlastDetails = null, bool isBlazingCyclone = false,
        ProjectileDetailsSO blazingCycloneDetails = null, bool isThrowingAxe = false, ProjectileDetailsSO throwingAxeDetails = null, bool isShiruken = false,
        ProjectileDetailsSO shirukenDetails = null, bool isChainLightning = false, ProjectileDetailsSO chainLightningDetails = null,
        ChainLightningPhase chainLightningPhase = ChainLightningPhase.None);

    GameObject GetGameObject();
}
