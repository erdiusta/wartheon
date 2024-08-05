using System.Collections.Generic;

public class Weapon
{
    public WeaponDetailsSO weaponDetails;
    public float activeWeaponHandling;
    public int weaponRightHandListPosition;
    public int weaponLeftHandListPosition;
    public int weaponRemainingProjectile;
    public bool onPrecharge;
    public bool onCooldown;
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
