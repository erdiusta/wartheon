using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class BlizzardNetwork : NetworkBehaviour
{
    public UnityEvent OnBlizzardActivated;
    public UnityEvent OnBlizzardDeactivated;

    Animator anim;
    Player owner;

    public float effectCooldown = 1f; // seconds of immunity

    [SyncVar] public bool isBlizzardEnabled;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();
    float playerLastTime;

    private void OnEnable()
    {
        OnBlizzardActivated.AddListener(EnableBlizzard);
        OnBlizzardDeactivated.AddListener(DisableBlizzard);
    }

    private void OnDisable()
    {
        OnBlizzardActivated.RemoveListener(EnableBlizzard);
        OnBlizzardDeactivated.RemoveListener(DisableBlizzard);
    }

    public void Initialize(Player ownerPlayer, float duration, int slotIndex, uint ownerNetId)
    {
        owner = ownerPlayer;
        anim = GetComponent<Animator>();

        RpcPlayAnimation(false);

        isBlizzardEnabled = true;

        if (isServer) StartCoroutine(Lifetime(duration, slotIndex, ownerNetId));
    }

    [ClientRpc]
    void RpcPlayAnimation(bool undo)
    {
        if (anim == null) anim = GetComponent<Animator>();

        anim.SetBool("blizzard", !undo);
    }

    public void ActivateBlizzardEvent() => OnBlizzardActivated?.Invoke();
    public void DeactivateBlizzardEvent() => OnBlizzardDeactivated?.Invoke();

    // Local Handlers
    public void EnableBlizzard()
    {
        isBlizzardEnabled = true;
    }

    public void DisableBlizzard()
    {
        isBlizzardEnabled = false;
    }

    IEnumerator Lifetime(float duration, int slotIndex, uint ownerNetId)
    {
        yield return new WaitForSeconds(duration);

        RpcPlayAnimation(undo: true);

        owner.NetAuth.TargetBlizzardFinished(owner.NetAuth.connectionToClient, slotIndex, ownerNetId);

        yield return new WaitForSeconds(0.5f); // animation fade

        NetworkServer.Destroy(gameObject);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isServer) return;
        if (!isBlizzardEnabled) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        Player affectedPlayer = collision.GetComponent<Player>();

        IHealthAuthority healthAuthority;

        if (affectedPlayer != null)
        {
            if (affectedPlayer.playerDetails.playerCharacterIndex != Character.Mycara) return;

            if (Time.time - playerLastTime < effectCooldown) return;

            // Apply effect
            if (affectedPlayer.health.GetCurrentHealth() <= affectedPlayer.health.GetMaximumHealth() / 2)
            {
                healthAuthority = HealthAuthorityResolver.GetAuthority(affectedPlayer.gameObject);
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
            owner.meleeAttackMainHand.CheckChillStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, isBlizzard: true);

            Weapon mainHandWeapon = owner.activeWeapon.GetCurrentMainHandWeapon();

            float specialMoveDamageModifier = 1f;

            if (owner != null)
            {
                specialMoveDamageModifier = owner.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 0.7f,
                    2 => 0.8f,
                    3 => 1f,
                    _ => 1f
                };
            }

            int inflictedDamage = owner.meleeAttackMainHand.CalculateDamageAmount(affectedEnemy, mainHandWeapon, MeleeHand.MainHand, specialMoveDamageModifier);

            DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile };
            ReceiveProjectileDamage receiveProjectileDamage = affectedEnemy.GetComponent<ReceiveProjectileDamage>();
            receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);

            // Check if player levels-after killing the enemy
            int levelBeforeKillingEnemy = owner.currentLevel;

            healthAuthority = HealthAuthorityResolver.GetAuthority(affectedEnemy.gameObject);

            if (healthAuthority.CurrentHealth <= 0)
            {
                if (!NetworkServer.active && !NetworkClient.active) goto jumpExp;

                owner?.NetAuth.Server_ExpGain(owner.NetAuth.netIdentity, levelBeforeKillingEnemy, affectedEnemy.enemyNetwork?.netIdentity);
            }

        jumpExp:

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
