using System;
using UnityEngine;

public class SetActiveItemEvent : MonoBehaviour
{
    public event Action<SetActiveItemEvent, SetSelectedActiveItemArgs> OnSelectedActiveItem;

    public void CallSelectedActiveItem(ActiveItem activeItem)
    {
        OnSelectedActiveItem?.Invoke(this, new SetSelectedActiveItemArgs { activeItem = activeItem });
    }

    public event Action<SetActiveItemEvent> OnRemovedActiveItem;

    public void CallRemovedActiveItem()
    {
        OnRemovedActiveItem?.Invoke(this);
    }
}

public class SetSelectedActiveItemArgs : EventArgs
{
    public ActiveItem activeItem;
}
