using Mirror;
using UnityEngine;

public abstract class PlayerOwnedBehaviour : NetworkBehaviour
{
    protected Player player;
    protected bool initialized;

    public override void OnStartClient()
    {
        base.OnStartClient();

        TryBind();
    }

    private void TryBind()
    {
        if (initialized) return;

        player = GetComponent<Player>();

        if (player == null) Debug.Log("Player is null during getting player component!");

        if (player == null) return;

        initialized = true;
        HandlePlayerReady(player);
    }

    protected abstract void HandlePlayerReady(Player player);
}
