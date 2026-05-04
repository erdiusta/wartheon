using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ShatterCry : MonoBehaviour
{
    public UnityEvent OnShatterCryActivated;
    public UnityEvent OnShatterCryDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isShatterCryEnabled;
    Player player;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();
    float playerLastTime;
    private void OnEnable()
    {
        OnShatterCryActivated.AddListener(EnableShatterCry);
        OnShatterCryDeactivated.AddListener(DisableShatterCry);
    }

    private void OnDisable()
    {
        OnShatterCryActivated.RemoveListener(EnableShatterCry);
        OnShatterCryDeactivated.RemoveListener(DisableShatterCry);
    }

    void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    public void ActivateBlizzardEvent() => OnShatterCryActivated?.Invoke();
    public void DeactivateBlizzardEvent() => OnShatterCryDeactivated?.Invoke();
    public void EnableShatterCry() => isShatterCryEnabled = true;
    public void DisableShatterCry() => isShatterCryEnabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isShatterCryEnabled) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        Player affectedPlayer = collision.GetComponent<Player>();

        if (affectedPlayer != null)
        {
            if (affectedPlayer.playerDetails.playerCharacterIndex != Character.Karnag) return;

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

            IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(affectedEnemy.gameObject);

            // Apply effects
            player.meleeAttackMainHand.CheckFearStatus(affectedEnemy, enemyCombatData, isShatterCry: true);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
