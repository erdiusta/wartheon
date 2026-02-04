using Mirror;
using UnityEngine;

public class RoomPropHealthAuthority : NetworkBehaviour, IHealthAuthority
{
    public void ApplyDamage(int amount, DamageContext ctx, GameObject target = null)
    {
        var prop = target.GetComponent<RoomProp>();
        if (prop == null) return;

        Debug.Log("TARGET IS HIT!");

        if (!isServer)
        {
            CmdApplyPropDamage(prop.propId, amount, ctx);
            return;
        }

        ApplyDamageServer(prop.propId, amount, ctx);
    }

    [Command]
    public void CmdApplyPropDamage(int propId, int damage, DamageContext ctx)
    {
        ApplyDamageServer(propId, damage, ctx);
    }

    [Server]
    private void ApplyDamageServer(int propId, int damage, DamageContext ctx)
    {
        var room = GetComponentInChildren<InstantiatedRoom>();
        var prop = room.propIndexer.Get(propId);

        prop.health.ApplyDamageInternal(damage, ctx);
        RpcPropHealthChanged(propId, damage, ctx);
    }

    [ClientRpc]
    private void RpcPropHealthChanged(int propId, int damage, DamageContext ctx)
    {
        var room = GetComponentInChildren<InstantiatedRoom>();
        RoomProp prop = room.propIndexer.Get(propId);

        prop.health.SyncHealthVisuals(damage, ctx);
    }
}
