using Mirror;
using UnityEngine;

public class NetworkHealthAuthority : NetworkBehaviour, IHealthAuthority
{
    [HideInInspector] public Health health;

    private void Awake()
    {
        health = GetComponent<Health>();
    }

    public void ApplyDamage(int amount, DamageContext ctx, GameObject target = null)
    {
        if (!authority)
        {
            CmdRequestApplyDamage(amount, ctx);
            return;
        }

        ApplyDamageServer(amount, ctx);
    }

    [Command]
    private void CmdRequestApplyDamage(int amount, DamageContext ctx)
    {
        health.ApplyDamageInternal(amount, ctx);
    }

    [Server]
    private void ApplyDamageServer(int amount, DamageContext ctx)
    {
        health.ApplyDamageInternal(amount, ctx);

        RpcOnHealthChanged(amount, ctx);
    }

    [ClientRpc]
    private void RpcOnHealthChanged(int amount, DamageContext ctx)
    {
        health.SyncHealthVisuals(amount, ctx);
    }
}
