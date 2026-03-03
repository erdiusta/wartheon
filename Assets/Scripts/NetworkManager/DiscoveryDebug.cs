using Mirror;
using Mirror.Discovery;
using UnityEngine;

public class DiscoveryDebug : MonoBehaviour
{
    public WartheonNetworkDiscovery discovery;

    private void Start()
    {
        discovery.OnServerFound.AddListener(OnServerFound);

        if (NetworkServer.active)
        {
            Debug.Log("HOST: Advertising server");
            discovery.AdvertiseServer();
        }
        else
        {
            Debug.Log("CLIENT: Starting discovery");
            discovery.StartDiscovery();
        }
    }

    private void OnServerFound(ServerResponse info)
    {
        string address = info.EndPoint != null ? info.EndPoint.Address.ToString() : "UNKNOWN-ENDPOINT";

        Debug.Log($"FOUND SERVER: {info.EndPoint.Address} | URI = {info.uri}");
    }
}
