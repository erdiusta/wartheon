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

    // Special move cooldown reset event
    public event Action<SpecialMoveEvent, SpecialMoveEventArgs> OnSpecialMoveCooldownReset;

    public void CallSpecialMoveCooldownResetEvent(ActiveSkill activeSkill, int specialMoveNumber)
    {
        OnSpecialMoveCooldownReset?.Invoke(this, new SpecialMoveEventArgs { activeSkill = activeSkill, specialMoveNumber = specialMoveNumber });
    }
}

public class SpecialMoveEventArgs : EventArgs
{
    public ActiveSkill activeSkill;
    public int specialMoveNumber;
}
