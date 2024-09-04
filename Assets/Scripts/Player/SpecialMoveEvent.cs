using System;
using UnityEngine;

public class SpecialMoveEvent : MonoBehaviour
{
    // Special move used event
    public event Action<SpecialMoveEvent, SpecialMoveEventArgs> OnSpecialMoveUsed;

    public void CallSpecialMoveUsedEvent(int specialMoveNumber, bool onlyChangeAlpha = false)
    {
        OnSpecialMoveUsed?.Invoke(this, new SpecialMoveEventArgs { specialMoveNumber = specialMoveNumber, onlyChangeAlpha = onlyChangeAlpha});
    }
}

public class SpecialMoveEventArgs : EventArgs
{
    public int specialMoveNumber;
    public bool onlyChangeAlpha;
}
