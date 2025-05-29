using System.Collections.Generic;

public class Weapon : ItemGeneric
{
    public WeaponDetailsSO weaponDetails;
    public float activeWeaponHandling;
    public int weaponBelongingToWhichMainHandSet;
    public int weaponBelongingToWhichOffHandSet;
    public int weaponRemainingProjectile;
    public bool firingCompletedIfWeaponIsPrecharged;
    public bool firingStoppedPrematurelyIfWeaponIsPrecharged;
    public bool onPrecharge;
    public bool onCooldown;
    public int activePrice;
}

public class WeaponNameComparer : IEqualityComparer<Weapon>
{
    public bool Equals(Weapon x, Weapon y)
    {
        return x.weaponDetails.weaponName == y.weaponDetails.weaponName;
    }

    public int GetHashCode(Weapon obj)
    {
        return obj.weaponDetails.weaponName.GetHashCode();
    }
}
