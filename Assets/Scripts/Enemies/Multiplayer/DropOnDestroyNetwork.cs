using Mirror;
using UnityEngine;

public class DropOnDestroyNetwork : NetworkBehaviour
{
    DropOnDestroy dropOnDestroy;

    private void Awake()
    {
        dropOnDestroy = GetComponent<DropOnDestroy>();
    }
}
