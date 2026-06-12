using Mirror;
using UnityEngine;

public class NetworkHealthAuthority : NetworkBehaviour, IHealthAuthority
{
    [HideInInspector] public Health health;

    [SyncVar] public int currentHealth;
    [SyncVar] public int maxHealth;
    [SyncVar (hook = nameof(OnDamagableChanged))] public bool isDamageable = true;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;
    public bool IsDamageable
    {
        get => isDamageable;
        set
        {
            if (!isServer) return;

            isDamageable = value;
        }
    }

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        currentHealth = health.GetMaximumHealth();
        maxHealth = health.GetMaximumHealth();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        currentHealth = health.GetMaximumHealth();
        maxHealth = health.GetMaximumHealth();

        health.healthAuthority = this;
        isDamageable = true;
    }

    private void OnDamagableChanged(bool oldVal, bool newVal)
    {
        if (health != null)
        {
            isDamageable = newVal;
        }
    }

    public void ApplyDamage(int amount, DamageContext ctx)
    {
        if (!isServer) return;

        health.ApplyDamageInternal(amount, ctx);
        currentHealth = health.GetCurrentHealth();

        bool isDead = health.IsDead();

        if (!isDead)
        {
            RpcDamageVisuals(currentHealth, amount, ctx);
            RpcPlayHitFlash(ctx);
        }
    }

    [ClientRpc]
    private void RpcDamageVisuals(int newHealth, int damageAmount, DamageContext ctx)
    {
        health.ApplyReplicatedHealth(newHealth, damageAmount, ctx);
    }

    [ClientRpc]
    private void RpcPlayHitFlash(DamageContext ctx)
    {
        health.SyncHealthVisuals(0, ctx);
    }
}
