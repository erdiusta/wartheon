using System;

public static class StaticDialogueHandler
{
    // Room changed event
    public static event Action OnInsufficientFunds;

    public static void CallInsufficientFundsEvent()
    {
        OnInsufficientFunds?.Invoke();
    }

    // Get hint event
    public static event Action OnTradeCompleted;

    public static void CallTradeCompletedEvent()
    {
        OnTradeCompleted?.Invoke();
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

    // Tutorial start text
    public static event Action<TutorialDialogueEventArgs> OnTutorialTextStarted;

    public static void CallTutorialTextStartedEvent(TutorialPhase tutorialPhase)
    {
        OnTutorialTextStarted?.Invoke(new TutorialDialogueEventArgs { tutorialPhase = tutorialPhase });
    }


    // Tutorial end text
    public static event Action<TutorialDialogueEventArgs> OnTutorialTextEnded;

    public static void CallTutorialTextEndedEvent(TutorialPhase tutorialPhase)
    {
        OnTutorialTextEnded?.Invoke(new TutorialDialogueEventArgs { tutorialPhase = tutorialPhase });
    }
}

public class MoldranDialogueEventArgs: EventArgs
{
    public int dialogueNumber;
    public MoldranSpeechOrder moldranSpeechOrder;
}

public class TutorialDialogueEventArgs: EventArgs
{
    public TutorialPhase tutorialPhase;
}