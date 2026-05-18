using UnityEngine;

public interface IHealthAuthority
{
    bool IsDamageable { get; set; }
    int CurrentHealth { get; }
    int MaxHealth { get; }

    void ApplyDamage(int amount, DamageContext ctx);
}
