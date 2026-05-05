using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class FlameLotus : MonoBehaviour
{
    public UnityEvent OnFlameLotusActivated;
    public UnityEvent OnFlameLotusDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isFlameLotusEnabled;
    Player player;

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

    void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    public void ActivateFlameLotusEvent() => OnFlameLotusActivated?.Invoke();
    public void DeactivateFlameLotusEvent() => OnFlameLotusDeactivated?.Invoke();
    public void EnableFlameLotus() => isFlameLotusEnabled = true;
    public void DisableFlameLotus() => isFlameLotusEnabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
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

            Weapon mainHandWeapon = player?.activeWeapon?.GetCurrentMainHandWeapon();

            if (player != null)
            {
                specialMoveDamageModifier = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 0.8f,
                    2 => 0.95f,
                    3 => 1.15f,
                    _ => 1f
                };
            }

            int inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(affectedEnemy, mainHandWeapon, MeleeHand.MainHand, specialMoveDamageModifier);

            IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(affectedEnemy.gameObject);

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle != lastWeaponTitle)
            {
                currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                lastWeaponTitle = player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle;
            }

            // Apply effects
            player.meleeAttackMainHand.CheckWarmStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, isFlameLotus: true);

            DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile };
            ReceiveProjectileDamage receiveProjectileDamage = affectedEnemy.GetComponent<ReceiveProjectileDamage>();
            receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
