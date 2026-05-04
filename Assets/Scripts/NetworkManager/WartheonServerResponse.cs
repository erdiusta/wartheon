using Mirror.Discovery;
using System;

[Serializable]
public class WartheonServerResponse
{
    public ServerResponse baseResponse;

    public string lobbyName;
    public int playerCount;
    public int maxPlayers;
}
