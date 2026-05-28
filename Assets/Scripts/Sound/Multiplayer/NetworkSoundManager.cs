using Mirror;
using UnityEngine;

public class NetworkSoundManager : NetworkBehaviour
{
    public static NetworkSoundManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    [Command(requiresAuthority = false)]
    public void CmdPlaySound(SoundName soundName, Vector3 position)
    {
        RpcPlaySound(soundName, position);
    }

    [Server]
    public void ServerPlaySound(SoundName soundName, Vector3 position)
    {
        RpcPlaySound(soundName, position);
    }

    [ClientRpc]
    void RpcPlaySound(SoundName soundName, Vector3 position)
    {
        SoundEffectSO soundEffect = WartheonDatabase.Instance.GetSound(soundName);

        if (soundEffect == null) return;

        WorldSoundManager.Instance.PlayWorldSound(soundEffect, position, true);
    }
}
