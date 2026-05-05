using Mirror;
using UnityEngine;

public class PlayerWeaponState : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnMainHandChanged))]
    public WeaponTitle mainHandWeaponTitle;

    [SyncVar(hook = nameof(OnOffHandChanged))]
    public WeaponTitle offhandWeaponTitle;

    [SyncVar] public Rarity mainHandWeaponRarity;
    [SyncVar] public Rarity offHandWeaponRarity;

    [SyncVar(hook = nameof(OnWeaponSetChanged))] 
    public int currentWeaponSetIndex = 1;

    Player player;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    [Command]
    public void CmdChangeWeaponSet(int newIndex, bool onStart)
    {
        currentWeaponSetIndex = newIndex;
    }

    [Command]
    public void CmdSetOffHandVisual(bool active, WeaponStats weaponStats, Rarity rarity, int weaponIndex, bool onStart)
    {
        RpcSetOffHandVisual(active, weaponStats, rarity, weaponIndex, onStart);
    }

    [ClientRpc]
    void RpcSetOffHandVisual(bool active, WeaponStats weaponStats, Rarity rarity, int weaponIndex, bool onStart)
    {
        if (isLocalPlayer) return;

        // This runs on All Clients
        if (!active)
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent(isStatUpdateAllowed: true);
        }
        else
        {
            player.setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(weaponStats, rarity, weaponIndex, onStart, isStatUpdateAllowed: true);
        }
    }

    private void OnWeaponSetChanged(int oldIndex, int newIndex)
    {
        currentWeaponSetIndex = newIndex;
        player.currentWeaponSlotSetIndex = newIndex;

        player.playerControl.SetWeaponSetByIndex(onStart: false, currentWeaponSetIndex, false, false, false, isMultiplayer: true);
    }

    private void OnMainHandChanged(WeaponTitle oldW, WeaponTitle newW)
    {
        if (!player.initialWeaponStateApplied) return;

        // None means drop weapon
        if (newW == WeaponTitle.None)
        {
            // De-active dropped main hand weapon
            if (player.weaponSlotSetArray[currentWeaponSetIndex - 1][1]?.weaponStats.wieldType == WieldType.OneHanded)
            {
                // Prevent off-hand ui weapon icon lost in case weapon swapping on drop while equipping a shield
                player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent(true);
            }
            else
            {
                player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent(isStatUpdateAllowed: true);
            }
        }
    }

    private void OnOffHandChanged(WeaponTitle oldW, WeaponTitle newW)
    {
        if (!player.initialWeaponStateApplied) return;

        // None means drop weapon
        if (newW == WeaponTitle.None)
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent(isStatUpdateAllowed: true);
        }
    }
}
