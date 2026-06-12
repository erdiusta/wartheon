using UnityEngine;
using Mirror;

[DisallowMultipleComponent]
public class NetworkDestroyedRelay : NetworkBehaviour
{
    DestroyedEvent destroyedEvent;

    private void Awake()
    {
        destroyedEvent = GetComponent<DestroyedEvent>();
    }

    /// <summary>
    /// Called by server-only gameplay logic
    /// </summary>
    [Server]
    public void ServerDestroy(bool playerDied, uint killerNetId)
    {
        // Server authoritative event
        destroyedEvent.CallDestroyedEvent(playerDied, killerNetId);

        // Client visual event
        RpcDestroy(playerDied, killerNetId);
    }

    [ClientRpc]
    private void RpcDestroy(bool playerDied, uint killerNetId)
    {
        if (NetworkServer.active) return;

        destroyedEvent.CallDestroyedEvent(playerDied, killerNetId);
    }
}
