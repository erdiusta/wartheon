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

    // Moldran cutsene talk
    public static event Action<MoldranDialogueEventArgs> OnMoldranTalk;

    public static void CallMoldranTalkEvent(int dialogueNumber, MoldranSpeechOrder moldranSpeechOrder)
    {
        OnMoldranTalk?.Invoke(new MoldranDialogueEventArgs { dialogueNumber = dialogueNumber, moldranSpeechOrder = moldranSpeechOrder });
    }

    // Moldran cutsene talk finished
    public static event Action OnMoldranFinished;

    public static void CallMoldranFinishedEvent()
    {
        OnMoldranFinished?.Invoke();
    }
}

public class MoldranDialogueEventArgs: EventArgs
{
    public int dialogueNumber;
    public MoldranSpeechOrder moldranSpeechOrder;
}
