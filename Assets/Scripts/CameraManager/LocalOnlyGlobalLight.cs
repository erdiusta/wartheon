using Mirror;
using UnityEngine.Rendering.Universal;

public class LocalOnlyGlobalLight : NetworkBehaviour
{
    Light2D light2D;

    private void Awake()
    {
        light2D = GetComponent<Light2D>();

        // Default OFF (Important)
        light2D.enabled = false;
    }

    public override void OnStartLocalPlayer()
    {
        base.OnStartLocalPlayer();

        // Enable it only for local player
        light2D.enabled = true;
    }

    public override void OnStopClient()
    {
        base.OnStopClient();

        // Safety: ensure cleanup
        light2D.enabled = false;
    }
}
