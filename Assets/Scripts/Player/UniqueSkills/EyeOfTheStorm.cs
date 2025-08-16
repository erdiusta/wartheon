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
        player = GameManager.Instance.GetPlayer();

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
        if (mainHandWeapon == null || mainHandWeapon.weaponDetails.weaponCurrentProjectile == null) return;

        float multiplier = isEpicenter ? 1.2f : 0.6f;

        int rng = Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue);
        int tornadoDamage = (int)(rng * multiplier);

        affectedEnemy.health.TakeDamage(tornadoDamage, player.transform.position, affectedEnemy.transform.position, enemyCollider, MeleeHand.None);

        affectedEnemies[affectedEnemy] = Time.time;
    }
}
