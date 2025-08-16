using UnityEngine;
using System;

[DisallowMultipleComponent]
public class FireWeaponEvent : MonoBehaviour
{
    public event Action<FireWeaponEvent, FireWeaponEventArgs> OnFireWeapon;

    public void CallFireWeaponEvent(bool fire, bool firePreviousFrame, Enemy belongingEnemy, bool isLaser, AimDirection aimDirection, float aimAngle, float weaponAimAngle,
        Vector3 weaponAimDirectionVector, bool isIceBreaker = false, bool isActiveItem = false, bool isPenetrationArrow = false, MoravellePhase moravellePhase = MoravellePhase.None,
        SylvarokPhase treantPhase = SylvarokPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None,
        FrostWrymPhase frostWrymPhase = FrostWrymPhase.None, VenomancerPhase venomancerPhase = VenomancerPhase.None, FireWrymPhase fireWrymPhase = FireWrymPhase.None,
        MoldranPhase moldranPhase = MoldranPhase.None, bool isTripleThreat = false, bool isBindingArrow = false, bool isArrowOfTheSeven = false, 
        ProjectileDetailsSO grappleDetails = null, ProjectileDetailsSO iceBreakerDetails = null, bool isFireBlast = false, ProjectileDetailsSO fireBlastDetails = null,
        bool isBlazingCyclone = false, ProjectileDetailsSO blazingCycloneDetails = null, bool isThrowingAxe = false, ProjectileDetailsSO throwingAxeDetails = null,
        bool isShiruken = false, ProjectileDetailsSO shirukenDetails = null, bool isChainLightning = false, ProjectileDetailsSO chainLightningDetails = null,
        ChainLightningPhase chainLightningPhase = ChainLightningPhase.None)
    {
        OnFireWeapon?.Invoke(this, new FireWeaponEventArgs
        {
            fire = fire,
            firePreviousFrame = firePreviousFrame,
            belongingEnemy = belongingEnemy,
            isLaser = isLaser,
            aimDirection = aimDirection,
            aimAngle = aimAngle,
            weaponAimAngle = weaponAimAngle,
            weaponAimDirectionVector = weaponAimDirectionVector,
            isActiveItem = isActiveItem,
            isPenetrationArrow = isPenetrationArrow,
            moravellePhase = moravellePhase,
            treantPhase = treantPhase,
            galvanusPhase = galvanusPhase,
            sepharothPhase = sepharothPhase,
            frostWrymPhase = frostWrymPhase,
            venomancerPhase = venomancerPhase,
            fireWrymPhase = fireWrymPhase,
            moldranPhase = moldranPhase,
            isTripleThreat = isTripleThreat,
            isBindingArrow = isBindingArrow,
            isArrowOfTheSeven = isArrowOfTheSeven,
            grappleDetails = grappleDetails,
            isIceBreaker = isIceBreaker,
            iceBreakerDetails = iceBreakerDetails,
            isFireBlast = isFireBlast,
            fireBlastDetails = fireBlastDetails,
            isBlazingCyclone = isBlazingCyclone,
            blazingCycloneDetails = blazingCycloneDetails,
            isThrowingAxe = isThrowingAxe,
            throwingAxeDetails = throwingAxeDetails,
            isShiruken = isShiruken,
            shirukenDetails = shirukenDetails,
            isChainLightning = isChainLightning,
            chainLightningDetails = chainLightningDetails,
            chainLightningPhase = chainLightningPhase
        });
    }

    public event Action<FireWeaponEvent, FireFocusedShotEventArgs> OnFocousedAim;

    public void CallFocusedAimEvent(Vector3 lockedTargetVector, float lockedAngle)
    {
        OnFocousedAim?.Invoke(this, new FireFocusedShotEventArgs { lockedTargetVector = lockedTargetVector, lockedAngle = lockedAngle });
    }
}

public class FireWeaponEventArgs : EventArgs
{
    public bool fire;
    public bool firePreviousFrame;
    public Enemy belongingEnemy;
    public bool isLaser;
    public AimDirection aimDirection;
    public float aimAngle;
    public float weaponAimAngle;
    public Vector3 weaponAimDirectionVector;
    public bool isIceBreaker;
    public bool isActiveItem;
    public bool isPenetrationArrow;
    public MoravellePhase moravellePhase;
    public SylvarokPhase treantPhase;
    public GalvanusPhase galvanusPhase;
    public SepharothPhase sepharothPhase;
    public FrostWrymPhase frostWrymPhase;
    public VenomancerPhase venomancerPhase;
    public FireWrymPhase fireWrymPhase;
    public MoldranPhase moldranPhase;
    public bool isTripleThreat;
    public bool isBindingArrow;
    public bool isArrowOfTheSeven;
    public ProjectileDetailsSO grappleDetails;
    public ProjectileDetailsSO iceBreakerDetails;
    public bool isFireBlast;
    public ProjectileDetailsSO fireBlastDetails;
    public bool isBlazingCyclone;
    public ProjectileDetailsSO blazingCycloneDetails;
    public bool isThrowingAxe;
    public ProjectileDetailsSO throwingAxeDetails;
    public bool isShiruken;
    public ProjectileDetailsSO shirukenDetails;
    public bool isChainLightning;
    public ProjectileDetailsSO chainLightningDetails;
    public ChainLightningPhase chainLightningPhase;
}

public class FireFocusedShotEventArgs : EventArgs
{
    public Vector3 lockedTargetVector;
    public float lockedAngle;
}
