using UnityEngine;

public interface IHealthAuthority
{
    void ApplyDamage(int amount, DamageContext ctx, GameObject target = null);
}
