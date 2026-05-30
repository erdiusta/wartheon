using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EyeOfTheStormNetwork : NetworkBehaviour
{
    public UnityEvent OnEyeOfTheStormActivated;
    public UnityEvent OnEyeOfTheStormDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isEyeOfTheStormActivated;

    Animator anim;
    Player owner;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();

    private void OnEnable()
    {
        OnEyeOfTheStormActivated.AddListener(EnableEyeOfTheStorm);
        OnEyeOfTheStormDeactivated.AddListener(DisableEyeOfTheStorm);
    }

    private void OnDisable()
    {
        OnEyeOfTheStormActivated.RemoveListener(EnableEyeOfTheStorm);
        OnEyeOfTheStormDeactivated.RemoveListener(DisableEyeOfTheStorm);
    }

    public void Initialize(Player ownerPlayer, float duration, int slotIndex, uint ownerNetId)
    {
        owner = ownerPlayer;
        anim = GetComponent<Animator>();

        RpcPlayAnimation(false);

        isEyeOfTheStormActivated = true;

        if (isServer) StartCoroutine(Lifetime(duration, slotIndex, ownerNetId));
    }

    [ClientRpc]
    void RpcPlayAnimation(bool undo)
    {
        if (anim == null) anim = GetComponent<Animator>();

        anim.SetBool("eyeOfTheStorm", !undo);
    }

    public void ActivateEyeOfTheStormEvent() => OnEyeOfTheStormActivated?.Invoke();
    public void DeactivateEyeOfTheStormEvent() => OnEyeOfTheStormDeactivated?.Invoke();

    // Local Handlers
    public void EnableEyeOfTheStorm()
    {
        isEyeOfTheStormActivated = true;
    }

    public void DisableEyeOfTheStorm()
    {
        isEyeOfTheStormActivated = false;
    }

    IEnumerator Lifetime(float duration, int slotIndex, uint ownerNetId)
    {
        yield return new WaitForSeconds(duration);

        RpcPlayAnimation(undo: true);

        owner.NetAuth.TargetAbsoluteZeroFinished(owner.NetAuth.connectionToClient, slotIndex, ownerNetId);

        yield return new WaitForSeconds(0.5f); // animation fade

        NetworkServer.Destroy(gameObject);
    }

    public void ApplyStormEffect(Collider2D enemyCollider, bool isEpicenter)
    {
        Enemy affectedEnemy = enemyCollider.GetComponent<Enemy>();
        if (affectedEnemy == null) return;
        if (!isServer) return;

        if (affectedEnemies.TryGetValue(affectedEnemy, out float lastTime))
        {
            if (Time.time - lastTime < effectCooldown) return;
        }

        Weapon mainHandWeapon = owner.activeWeapon.GetCurrentMainHandWeapon();

        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(mainHandWeapon.weaponStats.weaponTitle);

        if (mainHandWeapon == null || weaponDetails.weaponCurrentProjectile == null) return;

        float damageModifier = 1f;

        if (isEpicenter)
        {
            damageModifier = owner.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 1.2f,
                2 => 1.35f,
                3 => 1.5f,
                _ => 1f
            };
        }
        else
        {
            damageModifier = owner.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.6f,
                2 => 0.7f,
                3 => 0.8f,
                _ => 1f
            };
        }

        int inflictedDamage = owner.meleeAttackMainHand.CalculateDamageAmount(affectedEnemy, mainHandWeapon, MeleeHand.MainHand, damageModifier);
        DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile };
        ReceiveProjectileDamage receiveProjectileDamage = enemyCollider.GetComponent<ReceiveProjectileDamage>();
        receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);

        IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(affectedEnemy.gameObject);

        // Check if player levels-after killing the enemy
        int levelBeforeKillingEnemy = owner.currentLevel;

        if (healthAuthority.CurrentHealth <= 0)
        {
            owner.NetAuth.Server_ExpGain(owner.NetAuth.netIdentity, levelBeforeKillingEnemy, affectedEnemy.enemyNetwork.netIdentity);
        }

        affectedEnemies[affectedEnemy] = Time.time;
    }
}
