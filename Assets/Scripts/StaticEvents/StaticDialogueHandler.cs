using System;

public static class StaticDialogueHandler
{
    // Room changed event
    public static event Action OnInsufficientFunds;

    public static void CallInsufficientFundsEvent()
    {
        OnInsufficientFunds?.Invoke();
    }
}
