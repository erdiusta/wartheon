using System;
using UnityEngine;

[DisallowMultipleComponent]
public class ManaEvent : MonoBehaviour
{
    public event Action<ManaEvent, ManaEventArgs> OnManaChanged;

    public void CallManaChangedEvent(int manaAmount, bool manaReserved = false)
    {
        OnManaChanged?.Invoke(this, new ManaEventArgs { manaAmount = manaAmount, manaReserved = manaReserved});
    }

    public event Action<ManaEvent, ManaEventArgs> OnReservedManaReset;

    public void CallReservedManaResetEvent(int manaAmount)
    {
        OnReservedManaReset?.Invoke(this, new ManaEventArgs { manaAmount = manaAmount });
    }
}

public class ManaEventArgs : EventArgs
{
    public int manaAmount;
    public bool manaReserved;
}