using Mirror;
using UnityEngine;

public class AimWeaponNetwork : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnAimDirectionChanged))]
    AimDirection syncedAimDirection;

    [SyncVar] bool isBow;
    [SyncVar] bool isCrossbow;

    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    [Command]
    public void CmdSetAimDirectionForBow(AimDirection dir)
    {
        syncedAimDirection = dir;
        isBow = true;
    }

    [Command]
    public void CmdSetAimDirectionForCrossbow(AimDirection dir)
    {
        syncedAimDirection = dir;
        isCrossbow = true;
    }

    private void OnAimDirectionChanged(AimDirection oldDir, AimDirection newDir)
    {
        if (isBow) ApplyBowPosture(newDir);
        else if (isCrossbow) ApplyCrossbowPosture(newDir);

        // RESET
        isBow = false;
        isCrossbow = false;
    }

    private void ApplyBowPosture(AimDirection dir)
    {
        if (isLocalPlayer) return;

        player.aimWeapon.BowAim(dir);
    }

    private void ApplyCrossbowPosture(AimDirection dir)
    {
        if (isLocalPlayer) return;

        player.aimWeapon.CrossbowAim(dir);
    }
}
