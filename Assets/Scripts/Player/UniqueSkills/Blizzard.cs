using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Blizzard : MonoBehaviour
{
    public UnityEvent OnBlizzardActivated;
    public UnityEvent OnBlizzardDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isBlizzardEnabled;
    Player player;

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

    void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    public void ActivateBlizzardEvent() => OnBlizzardActivated?.Invoke();
    public void DeactivateBlizzardEvent() => OnBlizzardDeactivated?.Invoke();
    public void EnableBlizzard() => isBlizzardEnabled = true;
    public void DisableBlizzard() => isBlizzardEnabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isBlizzardEnabled) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        Player affectedPlayer = collision.GetComponent<Player>();

        if (affectedPlayer != null)
        {
            if (affectedPlayer.playerDetails.playerCharacterIndex != Character.Mycara) return;

            if (Time.time - playerLastTime < effectCooldown) return;

            // Apply effect
            if (affectedPlayer.health.GetCurrentHealth() <= affectedPlayer.health.GetMaximumHealth() / 2)
            {
                affectedPlayer.health.AddHealth(4);
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

            // Apply effects
            player.meleeAttackMainHand.CheckChillStatus(affectedEnemy, true);

            Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

            float specialMoveDamageModifier = 1f;

            if (player != null)
            {
                specialMoveDamageModifier = player.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 0.7f,
                    2 => 0.8f,
                    3 => 1f,
                    _ => 1f
                };
            }

            int inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(affectedEnemy, mainHandWeapon, MeleeHand.MainHand, specialMoveDamageModifier);

            affectedEnemy.health.TakeDamage(inflictedDamage, Vector2.zero, Vector2.zero, affectedEnemy.GetComponent<PolygonCollider2D>(), MeleeHand.None);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
