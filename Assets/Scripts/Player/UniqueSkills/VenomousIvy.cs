using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class VenomousIvy : MonoBehaviour
{
    public UnityEvent OnVenomousIvyActivated;
    public UnityEvent OnVenomousIvyDeactivated;

    public float effectCooldown = 1f; // seconds of immunity

    bool isVenomousIvyEnabled;
    Player player;

    // Dictionary to track last affected time per enemy
    Dictionary<Enemy, float> affectedEnemies = new Dictionary<Enemy, float>();
    float playerLastTime;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    private void OnEnable()
    {
        OnVenomousIvyActivated.AddListener(EnableVenomousIvy);
        OnVenomousIvyDeactivated.AddListener(DisableVenomousIvy);
    }

    private void OnDisable()
    {
        OnVenomousIvyActivated.RemoveListener(EnableVenomousIvy);
        OnVenomousIvyDeactivated.RemoveListener(DisableVenomousIvy);
    }

    void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    public void ActivateVenomousIvyEvent() => OnVenomousIvyActivated?.Invoke();
    public void DeactivateVenomousIvyEvent() => OnVenomousIvyDeactivated?.Invoke();
    public void EnableVenomousIvy() => isVenomousIvyEnabled = true;
    public void DisableVenomousIvy() => isVenomousIvyEnabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (!isVenomousIvyEnabled) return;

        Enemy affectedEnemy = collision.GetComponent<Enemy>();
        Player affectedPlayer = collision.GetComponent<Player>();

        if (affectedPlayer != null)
        {
            if (affectedPlayer.playerDetails.playerCharacterIndex != Character.Nyxa) return;

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

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle != lastWeaponTitle)
            {
                currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                lastWeaponTitle = player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle;
            }

            // Apply effects
            player.meleeAttackMainHand.CheckPoisonStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, isVenomousIvy: true);
            player.meleeAttackMainHand.CheckRootStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, isVenomousIvy: true);

            // Update last affected time
            affectedEnemies[affectedEnemy] = Time.time;
        }
    }
}
