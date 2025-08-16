using System;
using UnityEngine;

public class SpecialMoveEvent : MonoBehaviour
{
    // Special move used event
    public event Action<SpecialMoveEvent, SpecialMoveEventArgs> OnSpecialMoveUsed;

    public void CallSpecialMoveUsedEvent(ActiveSkill activeSkill, int specialMoveNumber)
    {
        OnSpecialMoveUsed?.Invoke(this, new SpecialMoveEventArgs { activeSkill = activeSkill, specialMoveNumber = specialMoveNumber});
    }
}

public class SpecialMoveEventArgs : EventArgs
{
    public ActiveSkill activeSkill;
    public int specialMoveNumber;
}
