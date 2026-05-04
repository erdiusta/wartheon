using UnityEngine;

public interface IHealthAuthority
{
    int CurrentHealth { get; }
    int MaxHealth { get; }

    void ApplyDamage(int amount, DamageContext ctx);
}
