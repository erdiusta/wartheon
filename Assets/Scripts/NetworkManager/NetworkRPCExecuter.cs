using Mirror;

public class NetworkRPCExecuter : NetworkBehaviour
{
    public static NetworkRPCExecuter Instance;

    public override void OnStartClient()
    {
        Instance = this;
    }

    [ClientRpc]
    public void RpcPrepareForSceneChange()
    {
        LoadingManager.SafeInstance?.ShowLoadingScreen();

        MultiplayerLobbyUI.Instance?.ShutdownLobby();

        //MainMenuUI.Instance?.ExitMultiplayerLobby();
    }
}
