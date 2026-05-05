using UnityEngine;
using UnityEngine.Events;

public class UmbralMist : MonoBehaviour
{
    public UnityEvent OnSmokeActivated;
    public UnityEvent OnSmokeDeactivated;

    bool isSmokeEnabled;
    Player player;

    WeaponTitle lastWeaponTitle;
    WeaponDetailsSO currentWeaponDetails;

    private void OnEnable()
    {
        OnSmokeActivated.AddListener(EnableSmoke);
        OnSmokeDeactivated.AddListener(DisableSmoke);
    }

    private void OnDisable()
    {
        OnSmokeActivated.RemoveListener(EnableSmoke);
        OnSmokeDeactivated.RemoveListener(DisableSmoke);
    }

    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    public void ActivateSmokeEvent()
    {
        OnSmokeActivated?.Invoke();
    }

    public void DeactivateSmokeEvent()
    {
        OnSmokeDeactivated?.Invoke();
    }

    public void EnableSmoke() => isSmokeEnabled = true;

    public void DisableSmoke() => isSmokeEnabled = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.enemyTag) && isSmokeEnabled)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle != lastWeaponTitle)
            {
                currentWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
                lastWeaponTitle = player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle;
            }

            Enemy affectedEnemy = collision.GetComponent<Enemy>();

            if (affectedEnemy != null)
            {
                IEnemyCombatData enemyCombatData = EnemyDataResolver.Resolve<IEnemyCombatData>(affectedEnemy.gameObject);
                player.meleeAttackMainHand.CheckBlindStatus(currentWeaponDetails, affectedEnemy, enemyCombatData, umbralMist: true);
            }
        }
    }
}
