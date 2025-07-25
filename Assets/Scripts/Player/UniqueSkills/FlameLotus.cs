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
        player = GameManager.Instance.GetPlayer();
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

            // Apply effects
            player.meleeAttackMainHand.CheckWarmStatus(affectedEnemy, true);

            affectedEnemy.health.TakeDamage(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.projectileDamageMax,
                Vector2.zero, Vector2.zero, affectedEnemy.GetComponent<PolygonCollider2D>(), MeleeHand.None);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
