using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbsoluteZeroNetwork : NetworkBehaviour
{
    public UnityEvent OnAbsoluteZeroActivated;
    public UnityEvent OnAbsoluteZeroDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isAbsoluteZeroEnabled;

    Animator anim;
    Player owner;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();
    float playerLastTime;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    private void OnEnable()
    {
        OnAbsoluteZeroActivated.AddListener(EnableAbsoluteZero);
        OnAbsoluteZeroDeactivated.AddListener(DisableAbsoluteZero);
    }

    private void OnDisable()
    {
        OnAbsoluteZeroActivated.RemoveListener(EnableAbsoluteZero);
        OnAbsoluteZeroDeactivated.RemoveListener(DisableAbsoluteZero);
    }

    public void Initialize(Player ownerPlayer, float duration, int slotIndex, uint ownerNetId)
    {
        owner = ownerPlayer;
        anim = GetComponent<Animator>();

        RpcPlayAnimation(false);

        isAbsoluteZeroEnabled = true;

        if (isServer) StartCoroutine(Lifetime(duration, slotIndex, ownerNetId));
    }

    [ClientRpc]
    void RpcPlayAnimation(bool undo)
    {
        if (anim == null) anim = GetComponent<Animator>();

        anim.SetBool("absoluteZero", !undo);
    }

    public void ActivateAbsoluteZeroEvent() => OnAbsoluteZeroActivated?.Invoke();
    public void DeactivateAbsoluteZeroEvent() => OnAbsoluteZeroDeactivated?.Invoke();

    // Local Handlers
    public void EnableAbsoluteZero()
    {
        isAbsoluteZeroEnabled = true;
    }

    public void DisableAbsoluteZero()
    {
        isAbsoluteZeroEnabled = false;
    }

    IEnumerator Lifetime(float duration, int slotIndex, uint ownerNetId)
    {
        yield return new WaitForSeconds(duration);

        RpcPlayAnimation(undo: true);

        owner.NetAuth.TargetAbsoluteZeroFinished(owner.NetAuth.connectionToClient, slotIndex, ownerNetId);

        yield return new WaitForSeconds(0.5f); // animation fade

        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isServer) return;
        if (!isAbsoluteZeroEnabled) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        Player affectedPlayer = collision.GetComponent<Player>();

        if (affectedPlayer != null)
        {
            if (affectedPlayer.playerDetails.playerCharacterIndex != Character.Mycara) return;

            if (Time.time - playerLastTime < effectCooldown) return;

            // Apply effect
            if (affectedPlayer.health.GetCurrentHealth() <= affectedPlayer.health.GetMaximumHealth() / 2)
            {
                IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(affectedPlayer.gameObject);
                healthAuthority.ApplyDamage(-4, default);
            }

            // Update last affected time
            playerLastTime = Time.time;
        }
        else if (affectedEnemy != null)
        {
            // Check cooldown
            if (affectedEnemies.TryGetValue(affectedEnemy, out float lastTime))
            {
                if (Time.time - lastTime < effectCooldown) return;
            }

            IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(affectedEnemy.gameObject);

            if (owner.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle != lastWeaponTitle)
            {
                currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(owner.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                lastWeaponTitle = owner.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle;
            }

            // Apply effects
            owner.meleeAttackMainHand.CheckSlowStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, isAbsoluteZero: true);
            owner.meleeAttackMainHand.CheckChillStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, isBlizzard: true);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
