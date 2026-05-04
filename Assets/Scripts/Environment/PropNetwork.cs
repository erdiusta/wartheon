using Mirror;
using UnityEngine;
using System.Collections;

public class PropNetwork : NetworkBehaviour
{
    [SyncVar] public string roomId;

    DestroyableItem destroyable;

    private void Awake()
    {
        destroyable = GetComponent<DestroyableItem>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        TryBindToRoom();
    }

    [Server]
    public void NotifyDestroyed()
    {
        RpcPlayDestroyAnimation();
    }

    [ClientRpc]
    private void RpcPlayDestroyAnimation()
    {
        destroyable.PlayDestroyAnimationClient();
    }

    /// <summary>
    /// Bind the prop into related room then parent it for replacing.
    /// </summary>
    private void TryBindToRoom()
    {
        var room = DungeonRuntime.GetInstantiatedRoom(roomId);

        if (room == null)
        {
            StartCoroutine(BindNextFrame());
            return;
        }

        if (room.environmentGameObject != null)
        {
            transform.SetParent(room.environmentGameObject.transform, true);
        }
    }

    IEnumerator BindNextFrame()
    {
        yield return null;
        TryBindToRoom();
    }
}
