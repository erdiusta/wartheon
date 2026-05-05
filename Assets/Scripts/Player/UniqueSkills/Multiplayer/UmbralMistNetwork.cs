using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class UmbralMistNetwork : NetworkBehaviour
{
    public UnityEvent OnSmokeActivated;
    public UnityEvent OnSmokeDeactivated;

    Animator anim;
    Player owner;

    [SyncVar] public bool isSmokeEnabled;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    private void OnEnable()
    {
        OnSmokeActivated.AddListener(EnableSmoke);
        OnSmokeDeactivated.AddListener(DisableSmoke);
    }

    private void OnDisable()
    {
        OnSmokeActivated.RemoveListener(EnableSmoke);
        OnSmokeDeactivated.RemoveListener(DisableSmoke);
    }

    public void Initialize(Player ownerPlayer, float duration, int slotIndex, uint ownerNetId)
    {
        owner = ownerPlayer;
        anim = GetComponent<Animator>();

        RpcPlayAnimation(false);

        isSmokeEnabled = true;

        if (isServer) StartCoroutine(Lifetime(duration, slotIndex, ownerNetId));
    }

    [ClientRpc]
    void RpcPlayAnimation(bool undo)
    {
        if (anim == null) anim = GetComponent<Animator>();

        anim.SetBool("umbralMist", !undo);
    }

    public void ActivateSmokeEvent()
    {
        OnSmokeActivated?.Invoke();
    }

    public void DeactivateSmokeEvent()
    {
        OnSmokeDeactivated?.Invoke();
    }

    // Local Handlers
    public void EnableSmoke()
    {
        isSmokeEnabled = true;
    }

    public void DisableSmoke()
    {
        isSmokeEnabled = false;
    }

    IEnumerator Lifetime(float duration, int slotIndex, uint ownerNetId)
    {
        yield return new WaitForSeconds(duration);

        RpcPlayAnimation(undo: true);

        owner.NetAuth.TargetUmbralMistFinished(owner.NetAuth.connectionToClient, slotIndex, ownerNetId);

        yield return new WaitForSeconds(0.5f); // animation fade

        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isServer) return;
        if (!isSmokeEnabled) return;
        if (!collision.CompareTag(Settings.enemyTag)) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        if (affectedEnemy == null) return;
        if (owner == null) return;

        var weapon = owner.activeWeapon.GetCurrentMainHandWeapon();
        if (weapon == null) return;

        // Cache weapon details

        if (weapon.weaponStats.weaponTitle != lastWeaponTitle)
        {
            currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);
            lastWeaponTitle = weapon.weaponStats.weaponTitle;
        }

        IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(affectedEnemy.gameObject);
        owner.meleeAttackMainHand.CheckBlindStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, umbralMist: true);
    }
}
