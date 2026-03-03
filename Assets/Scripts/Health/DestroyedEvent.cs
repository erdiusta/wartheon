using System;
using UnityEngine;

[DisallowMultipleComponent]
public class DestroyedEvent : MonoBehaviour
{
    public event Action<DestroyedEvent, DestroyedEventArgs> OnDestroyed;

    public void CallDestroyedEvent(bool playerDied, uint killerNetId)
    {
        OnDestroyed?.Invoke(this, new DestroyedEventArgs { playerDied = playerDied, killetNetId = killerNetId });
    }
}

public class DestroyedEventArgs : EventArgs
{
    public bool playerDied;
    public uint killetNetId;
}