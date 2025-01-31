using System;
using UnityEngine;

[DisallowMultipleComponent]
public class DestroyedEvent : MonoBehaviour
{
    public event Action<DestroyedEvent, DestroyedEventArgs> OnDestroyed;

    public void CallDestroyedEvent(bool playerDied, bool isClone = false)
    {
        OnDestroyed?.Invoke(this, new DestroyedEventArgs { playerDied = playerDied, isClone = isClone });
    }
}

public class DestroyedEventArgs : EventArgs
{
    public bool playerDied;
    public bool isClone;
}