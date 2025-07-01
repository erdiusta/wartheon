using System;
using UnityEngine;

public class SpecialMoveEvent : MonoBehaviour
{
    // Special move used event
    public event Action<SpecialMoveEvent, SpecialMoveEventArgs> OnSpecialMoveUsed;

    public void CallSpecialMoveUsedEvent(ActiveSkill activeSkill, int specialMoveNumber, bool onlyChangeAlpha = false)
    {
        OnSpecialMoveUsed?.Invoke(this, new SpecialMoveEventArgs { activeSkill = activeSkill, specialMoveNumber = specialMoveNumber, onlyChangeAlpha = onlyChangeAlpha});
    }
}

public class SpecialMoveEventArgs : EventArgs
{
    public ActiveSkill activeSkill;
    public int specialMoveNumber;
    public bool onlyChangeAlpha;
}
