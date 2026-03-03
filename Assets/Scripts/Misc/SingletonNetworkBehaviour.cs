using Mirror;
using UnityEngine;

public class SingletonNetworkBehaviour<T> : NetworkBehaviour where T: NetworkBehaviour
{
    public static T Instance { get; private set; }

    public override void OnStartServer()
    {
        base.OnStartServer();

        if (Instance != null && Instance != this)
        {
            NetworkServer.Destroy(gameObject);
            return;
        }

        Instance = this as T;
    }

    public override void OnStopServer()
    {
        if (Instance == this) Instance = null;
    }
}
