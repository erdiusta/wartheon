using Mirror;
using Mirror.Discovery;
using System.Net;
using UnityEngine;

[RequireComponent(typeof(NetworkManager))]
public class WartheonNetworkDiscovery : NetworkDiscoveryBase<ServerRequest, ServerResponse>
{
    protected override ServerResponse ProcessRequest(ServerRequest request, IPEndPoint endPoint)
    {
        int currentPlayers = NetworkServer.connections.Count;
        int maxPlayers = NetworkManager.singleton.maxConnections;

        string hostName = "Unknown";
        foreach (var conn in NetworkServer.connections.Values)
        {
            if (conn.identity == null) continue;

            var lobby = conn.identity.GetComponent<PlayerLobbyState>();
            if (lobby != null && !string.IsNullOrEmpty(lobby.playerName))
            {
                hostName = lobby.playerName;
                break;
            }
        }

        return new ServerResponse { uri = transport.ServerUri(), hostName = hostName, playerCount = currentPlayers, maxPlayers = maxPlayers, joinable = currentPlayers < maxPlayers };
    }

    protected override void ProcessResponse(ServerResponse response, IPEndPoint endpoint)
    {
        // We must manually notify listeners because base is abstract
        OnServerFound.Invoke(response);

        Debug.Log($"DISCOVERY RESPONSE FROM {endpoint}");
    }
}