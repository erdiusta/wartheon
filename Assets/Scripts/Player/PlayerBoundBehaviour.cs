using UnityEngine;

public abstract class PlayerBoundBehaviour : MonoBehaviour
{
    protected Player player;
    protected bool initialized;

    protected virtual PlayerBindMode BindMode => PlayerBindMode.LocalOnly;

    protected virtual void OnEnable()
    {
        // If this script is attached to a Player prefab, try immediately
        Player selfPlayer = GetComponent<Player>();
        if (selfPlayer == null) selfPlayer = GetComponentInParent<Player>();

        if (selfPlayer != null)
        {
            selfPlayer.OnPlayerReady += Player_OnPlayerReady;

            if (selfPlayer.playerDetails != null)
            {
                TryInitialize(selfPlayer, selfPlayer.playerDetails);
            }

            return;
        }

        // Late join fallback (ui only cases)
        if (GameManager.Instance != null)
        {
            Player p = GameManager.Instance.GetLocalPlayer();

            p.OnPlayerReady += Player_OnPlayerReady;

            if (p.playerDetails != null)
            {
                TryInitialize(p, p.playerDetails);
            }
        }
    }

    protected virtual void OnDisable()
    {
        if(player != null) player.OnPlayerReady -= Player_OnPlayerReady;
    }

    private void Player_OnPlayerReady(Player player, PlayerDetailsSO details)
    {
        TryInitialize(player, details);
    }

    private void TryInitialize(Player p, PlayerDetailsSO details)
    {
        if (initialized || p == null || details == null) return;

        bool isLocal = p.IsLocal;

        if (BindMode == PlayerBindMode.LocalOnly && !isLocal) return;
        if (BindMode == PlayerBindMode.RemoteOnly && isLocal) return;

        initialized = true;

        // Stop listening immediately
        p.OnPlayerReady -= Player_OnPlayerReady;

        player = p;
        HandlePlayerReady(player, details);
    }

    protected abstract void HandlePlayerReady(Player player, PlayerDetailsSO details);
}
