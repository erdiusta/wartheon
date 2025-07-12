using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class AbsoluteZero : MonoBehaviour
{
    public UnityEvent OnAbsoluteZeroActivated;
    public UnityEvent OnAbsoluteZeroDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isAbsoluteZeroEnabled;
    Player player;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();
    float playerLastTime;

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

    void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    public void ActivateAbsoluteZeroEvent() => OnAbsoluteZeroActivated?.Invoke();
    public void DeactivateAbsoluteZeroEvent() => OnAbsoluteZeroActivated?.Invoke();
    public void EnableAbsoluteZero() => isAbsoluteZeroEnabled = true;
    public void DisableAbsoluteZero() => isAbsoluteZeroEnabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
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
            player.meleeAttackMainHand.CheckSlowStatus(affectedEnemy, true);
            player.meleeAttackMainHand.CheckChillStatus(affectedEnemy, true);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
