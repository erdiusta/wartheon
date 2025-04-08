using System;

public static class StaticDialogueHandler
{
    // Room changed event
    public static event Action OnInsufficientFunds;

    public static void CallInsufficientFundsEvent()
    {
        OnInsufficientFunds?.Invoke();
    }

    // Gamble lost event
    public static event Action OnGambleLost;

    public static void CallGambleLostEvent()
    {
        OnGambleLost?.Invoke();
    }

    // Gamble won event
    public static event Action OnGambleWon;

    public static void CallGambleWonEvent()
    {
        OnGambleWon?.Invoke();
    }
}
