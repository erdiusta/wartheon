using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EyeOfTheStorm : MonoBehaviour
{
    public UnityEvent OnTornadoActivated;
    public UnityEvent OnTornadoDeactivated;

    public CapsuleCollider2D epicenterCollider;
    public CircleCollider2D outerCollider;

    public float effectCooldown = 2f; // seconds of immunity

    [HideInInspector] public bool isTornadoEnabled;
    Player player;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();

    private void OnEnable()
    {
        OnTornadoActivated.AddListener(EnableTornado);
        OnTornadoDeactivated.AddListener(DisableTornado);
    }

    private void OnDisable()
    {
        OnTornadoActivated.RemoveListener(EnableTornado);
        OnTornadoDeactivated.RemoveListener(DisableTornado);
    }

    void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();

        epicenterCollider = GetComponent<CapsuleCollider2D>();
        outerCollider = GetComponent<CircleCollider2D>();
    }

    public void ActivateTornadoEvent() => OnTornadoActivated?.Invoke();
    public void DeactivateTornadoEvent() => OnTornadoDeactivated?.Invoke();
    public void EnableTornado() => isTornadoEnabled = true;
    public void DisableTornado() => isTornadoEnabled = false;

    public void ApplyStormEffect(Collider2D enemyCollider, bool isEpicenter)
    {
        Enemy affectedEnemy = enemyCollider.GetComponent<Enemy>();
        if (affectedEnemy == null) return;

        if (affectedEnemies.TryGetValue(affectedEnemy, out float lastTime))
        {
            if (Time.time - lastTime < effectCooldown) return;
        }

        Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(mainHandWeapon.weaponStats.weaponTitle);

        if (mainHandWeapon == null || weaponDetails.weaponCurrentProjectile == null) return;

        float damageModifier = 1f;

        if (isEpicenter)
        {
            damageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 1.2f,
                2 => 1.35f,
                3 => 1.5f,
                _ => 1f
            };
        }
        else
        {
            damageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 0.6f,
                2 => 0.7f,
                3 => 0.8f,
                _ => 1f
            };
        }

        int inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(affectedEnemy, mainHandWeapon, MeleeHand.MainHand, damageModifier);
        DamageContext ctx = new DamageContext { owner = DamageOwner.Player, source = DamageSourceType.Projectile };
        ReceiveProjectileDamage receiveProjectileDamage = enemyCollider.GetComponent<ReceiveProjectileDamage>();
        receiveProjectileDamage.TakeProjectileDamage(inflictedDamage, ctx);
        affectedEnemies[affectedEnemy] = Time.time;
    }
}
