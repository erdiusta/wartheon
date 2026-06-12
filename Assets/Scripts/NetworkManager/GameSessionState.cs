public static class GameSessionState
{
    // Lobby Gameplay intent
    public static bool GameplayAuthorized { get; private set; }

    public static bool IsGameRunning => GameplayAuthorized;

    public static void ResetSession()
    {
        WartheonNetworkManager.Instance.connectionToCharacterIndex.Clear();
        WartheonNetworkManager.Instance.characterLocks.Clear();
    }
}
