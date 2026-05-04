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
        RpcDestroy(playerDied, killerNetId);
    }

    [ClientRpc]
    private void RpcDestroy(bool playerDied, uint killerNetId)
    {
        destroyedEvent.CallDestroyedEvent(playerDied, killerNetId);
    }
}
