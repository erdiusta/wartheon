using System;
using UnityEngine;

public class SpecialMoveEvent : MonoBehaviour
{
    // Special move used event
    public event Action OnSpecialMoveUsed;

    public void CallSpecialMoveUsedEvent()
    {
        OnSpecialMoveUsed?.Invoke();
    }
}
