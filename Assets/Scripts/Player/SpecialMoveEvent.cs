using System;
using UnityEngine;

public class SpecialMoveEvent : MonoBehaviour
{
    // Special move used event
    public event Action<SpecialMoveEvent, SpecialMoveEventArgs> OnSpecialMoveUsed;

    public void CallSpecialMoveUsedEvent(int specialMoveNumber)
    {
        OnSpecialMoveUsed?.Invoke(this, new SpecialMoveEventArgs { specialMoveNumber = specialMoveNumber});
    }
}

public class SpecialMoveEventArgs : EventArgs
{
    public int specialMoveNumber;
}
