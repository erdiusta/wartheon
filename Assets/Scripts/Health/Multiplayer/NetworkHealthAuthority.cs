using Mirror;
using UnityEngine;

public class NetworkHealthAuthority : NetworkBehaviour, IHealthAuthority
{
    [HideInInspector] public Health health;

    [SyncVar] int currentHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => health.GetMaximumHealth();

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public override void OnStartServer()
    {
        base.OnStartServer();

        currentHealth = health.GetMaximumHealth();
    }

    public void ApplyDamage(int amount, DamageContext ctx)
    {
        if (!isServer) return;

        int oldHealth = currentHealth;

        health.ApplyDamageInternal(amount, ctx);
        currentHealth = health.GetCurrentHealth();

        RpcDamageVisuals(currentHealth, amount, ctx);
        RpcPlayHitFlash(ctx);
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
