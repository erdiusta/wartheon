using Mirror;
using UnityEngine;

public class NetworkHealthAuthority : NetworkBehaviour, IHealthAuthority
{
    [HideInInspector] public Health health;

    [SyncVar (hook = nameof(OnHealthChanged))] public int currentHealth;
    [SyncVar (hook = nameof(OnMaxHealthChanged))] public int maxHealth;

    public int CurrentHealth => currentHealth;
    public int MaxHealth => maxHealth;

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
    }

    private void OnHealthChanged(int oldValue, int newValue)
    {
        if (health == null) return;

        //int damageAmount = Mathf.Clamp(oldValue - newValue, 0, maxHealth);
        health.ApplyReplicatedHealth(newValue, oldValue - newValue, default);

        if (GetComponent<Environment>() != null) return;

        Debug.Log("Health after change is " + health.currentHealth);
    }

    private void OnMaxHealthChanged(int oldValue, int newValue)
    {
        if (health == null) return;

        health.SetMaximumHealth(newValue);
    }

    public void ApplyDamage(int amount, DamageContext ctx)
    {
        if (!isServer) return;

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
