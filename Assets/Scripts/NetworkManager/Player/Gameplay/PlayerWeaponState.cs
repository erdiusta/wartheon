using Mirror;
using UnityEngine;

public class PlayerWeaponState : NetworkBehaviour
{
    [SyncVar(hook = nameof(OnMainHandChanged))]
    public int mainHandWeaponId = -1;

    [SyncVar(hook = nameof(OnOffHandChanged))]
    public int offHandWeaponId = -1;

    [SyncVar]
    public int activeWeaponSetIndex;

    private void OnMainHandChanged(int oldId, int newId)
    {
        NotifyVisuals();
    }

    private void OnOffHandChanged(int oldId, int newId)
    {
        NotifyVisuals();
    }

    private void NotifyVisuals()
    {
        var activeWeapon = GetComponent<ActiveWeapon>();
        if (activeWeapon != null) activeWeapon.RefreshFromNetworkState();
    }

    [Server]
    public void EquipMainHandWeapon(Weapon weapon)
    {
        //mainHandWeaponId = weapon.weaponId;
    }
}
