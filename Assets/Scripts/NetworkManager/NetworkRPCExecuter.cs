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
        MultiplayerLobbyUI.Instance?.ShutdownLobby();
        //MainMenuUI.Instance?.ExitMultiplayerLobby();
    }
}
