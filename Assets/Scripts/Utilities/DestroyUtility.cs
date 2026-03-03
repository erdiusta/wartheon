using Mirror;
using UnityEngine;

public static class DestroyUtility
{
    public static void Destroy(GameObject target, bool playerDied, uint killerNetId)
    {
        // Multiplayer
        if (NetworkServer.active)
        {
            if (target.TryGetComponent(out NetworkDestroyedRelay relay))
            {
                relay.ServerDestroy(playerDied, killerNetId);
                return;
            }
        }

        // Singleplayer fallback
        if (target.TryGetComponent(out DestroyedEvent destroyed))
        {
            destroyed.CallDestroyedEvent(playerDied, killerNetId);
        }
    }
}
