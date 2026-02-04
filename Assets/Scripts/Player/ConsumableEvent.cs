using System;
using UnityEngine;

public class ConsumableEvent : MonoBehaviour
{
    public event Action<ConsumableEvent, ConsumableEventArgs> OnKeyCountChanged;

    public void CallKeyCountChangedEvent(int keyAmount)
    {
        OnKeyCountChanged?.Invoke(this, new ConsumableEventArgs { keyAmount = keyAmount });
    }

    public event Action<ConsumableEvent, ConsumableEventArgs> OnCoinCountChanged;

    public void CallCoinCountChangedEvent(int coinAmount)
    {
        OnCoinCountChanged?.Invoke(this, new ConsumableEventArgs { coinAmount = coinAmount });
    }

    public event Action<ConsumableEvent, ConsumableEventArgs> OnShardCountChanged;

    public void CallSharCountChangedEvent(int shardAmount)
    {
        OnShardCountChanged?.Invoke(this, new ConsumableEventArgs { shardAmount = shardAmount });
    }
}

public class ConsumableEventArgs
{
    public int keyAmount;
    public int coinAmount;
    public int shardAmount;
}
