using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlameLotusNetwork : NetworkBehaviour
{
    public UnityEvent OnFlameLotusActivated;
    public UnityEvent OnFlameLotusDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isFlameLotusEnabled;

    Animator anim;
    Player owner;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();
    float playerLastTime;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    private void OnEnable()
    {
        OnFlameLotusActivated.AddListener(EnableFlameLotus);
        OnFlameLotusDeactivated.AddListener(DisableFlameLotus);
    }

    private void OnDisable()
    {
        OnFlameLotusActivated.RemoveListener(EnableFlameLotus);
        OnFlameLotusDeactivated.RemoveListener(DisableFlameLotus);
    }

    public void Initialize(Player ownerPlayer, float duration, int slotIndex, uint ownerNetId)
    {
        owner = ownerPlayer;
        anim = GetComponent<Animator>();

        RpcPlayAnimation(false);

        isFlameLotusEnabled = true;

        if (isServer) StartCoroutine(Lifetime(duration, slotIndex, ownerNetId));
    }

    [ClientRpc]
    void RpcPlayAnimation(bool undo)
    {
        if (anim == null) anim = GetComponent<Animator>();

        anim.SetBool("flameLotus", !undo);
    }

    public void ActivateFlameLotusEvent() => OnFlameLotusActivated?.Invoke();
    public void DeactivateFlameLotusEvent() => OnFlameLotusDeactivated?.Invoke();

    // Local Handlers
    public void EnableFlameLotus()
    {
        isFlameLotusEnabled = true;
    }

    public void DisableFlameLotus()
    {
        isFlameLotusEnabled = false;
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
        if (!isFlameLotusEnabled) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        Player affectedPlayer = collision.GetComponent<Player>();

        if (affectedPlayer != null)
        {
            if (affectedPlayer.playerDetails.playerCharacterIndex != Character.Kynara) return;

            if (Time.time - playerLastTime < effectCooldown) return;

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

            float specialMoveDamageModifier = 0f;

            Weapon mainHandWeapon = owner?.activeWeapon?.GetCurrentMainHandWeapon();

            if (owner != null)
            {
                specialMoveDamageModifier = owner.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 0.8f,
                    2 => 0.95f,
                    3 => 1.15f,
                    _ => 1f
                };
            }

            int inflictedDamage = owner.meleeAttackMainHand.CalculateDamageAmount(affectedEnemy, mainHandWeapon, MeleeHand.MainHand, specialMoveDamageModifier);

            IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(affectedEnemy.gameObject);

            if (owner.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle != lastWeaponTitle)
            {
                currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(owner.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                lastWeaponTitle = owner.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle;
            }

            // Apply effects
            owner.meleeAttackMainHand.CheckWarmStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, isFlameLotus: true);

            DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile };
            ReceiveProjectileDamage receiveProjectileDamage = affectedEnemy.GetComponent<ReceiveProjectileDamage>();
            receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
