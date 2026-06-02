using UnityEngine;
using Steamworks;
using Mirror;

public class SteamTest : MonoBehaviour
{
    private void Start()
    {
        Debug.Log($"Active Transport:  {Transport.active}");

        Debug.Log($"Steam Initialized: {SteamManager.Initialized}");

        if (SteamManager.Initialized)
        {
            Debug.Log($"Steam Name: {SteamFriends.GetPersonaName()}");
            Debug.Log($"Steam ID: {SteamUser.GetSteamID()}");
        }
    }
}
