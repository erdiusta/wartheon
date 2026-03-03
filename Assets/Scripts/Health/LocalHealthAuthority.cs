using UnityEngine;

public class LocalHealthAuthority: MonoBehaviour, IHealthAuthority
{
    [HideInInspector] public Health health;

    public int CurrentHealth => health.GetCurrentHealth();
    public int MaxHealth => health.GetMaximumHealth();

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void ApplyDamage(int amount, DamageContext ctx)
    {
        health.ApplyDamageInternal(amount, ctx);
        health.ApplyReplicatedHealth(CurrentHealth, amount, ctx);
    }
}
