using UnityEngine;

public class LocalHealthAuthority: MonoBehaviour, IHealthAuthority
{
    [HideInInspector] public Health health;

    public int CurrentHealth => health.GetCurrentHealth();
    public int MaxHealth => health.GetMaximumHealth();
    public bool IsDamageable { get => isDamageable; set => isDamageable = value; }

    private bool isDamageable = true;

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
