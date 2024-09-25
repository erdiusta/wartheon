using System.Collections;
using UnityEngine;

[RequireComponent(typeof(Enemy))]
[DisallowMultipleComponent]
public class EnemyWeaponAI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate this with the WeaponShootPosition child gameobject transform")]
    #endregion Tooltip
    public Vector3 weaponShootPosition;

    #region Tooltip
    [Tooltip("Select the layers that the enemy bullets will hit")]
    #endregion Tooltip
    [SerializeField] LayerMask layerMask;

    [HideInInspector] public Coroutine enemyAttackCoroutine;

    Enemy enemy;
    EnemyDetailsSO enemyDetails;
    float firingIntervalTimer;
    float firingDurationTimer;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void Start()
    {
        enemyDetails = enemy.enemyDetails;

        firingIntervalTimer = WeaponShootInterval();
        firingDurationTimer = WeaponShootDuration();
    }

    private void Update()
    {
        if (GameManager.Instance.GetPlayer().onStealth) return;

        if (enemy.enemyAI.moveStatus == MoveStatus.Stun) return;

        if (enemy.enemyAI.moveStatus == MoveStatus.Stagger) return;

        if (enemy.enemyAI.attackMoveEnemyRoutine != null) return;

        if (enemy.health.getHitCoroutine != null) return;

        // Update timers
        firingIntervalTimer -= Time.deltaTime;

        // Interval Timer
        if (firingIntervalTimer < 0f)
        {
            if (firingDurationTimer >= 0)
            {
                firingDurationTimer -= Time.deltaTime;
                FireWeapon();
            }
            else
            {
                // Reset timers
                firingIntervalTimer = WeaponShootInterval();
                firingDurationTimer = WeaponShootDuration();
            }
        }
    }

    /// <summary>
    /// Calculate a random weapon shoot duration between the min and max values
    /// </summary>
    private float WeaponShootDuration()
    {
        // Calculate a random weapon shoot duration
        return Random.Range(enemyDetails.firingDurationMin, enemyDetails.firingDurationMax);
    }

    /// <summary>
    /// Calculate a random weapon shoot interval between the min and max values
    /// </summary>
    private float WeaponShootInterval()
    {
        // Calculate a random weapon shoot interval
        return Random.Range(enemyDetails.firingIntervalMin, enemyDetails.firingIntervalMax);
    }

    /// <summary>
    /// Fire the weapon
    /// </summary>
    private void FireWeapon()
    {
        if (enemy.isDead) return;

        Vector3 playerDirectionVector, weaponDirection;
        float weaponAngleDegrees, enemyAngleDegrees;
        AimDirection enemyAimDirection;

        Aim(out playerDirectionVector, out weaponDirection, out weaponAngleDegrees, out enemyAngleDegrees, out enemyAimDirection);

        // Only fire if enemy has a weapon
        if (enemyDetails.enemyWeapon != null)
        {
            // Get projectile range
            float enemyProjectileRange = enemyDetails.enemyWeapon.weaponCurrentProjectile.projectileRange;

            // Is the player in range
            if (playerDirectionVector.magnitude <= enemyProjectileRange)
            {
                // Does this enemy require line of sight to the player before firing?
                if (enemyDetails.firingLineOfSightRequired && !IsPlayerInLineOfSight(weaponDirection, enemyProjectileRange)) return;

                // Trigger fire weapon event
                if (enemyAttackCoroutine == null)
                {
                    enemy.animateEnemy.SetAttackAnimationParameters();
                    enemy.animator.SetBool(Settings.isAttacking, true);

                    enemyAttackCoroutine = StartCoroutine(EnemyAttackAnimRoutine());
                    enemy.fireWeaponEvent.CallFireWeaponEvent(true, false, enemyAimDirection, enemyAngleDegrees, weaponAngleDegrees, weaponDirection, false);
                }
            }
        }
    }

    public void Aim(out Vector3 playerDirectionVector, out Vector3 weaponDirection, out float weaponAngleDegrees, out float enemyAngleDegrees,
        out AimDirection enemyAimDirection)
    {
        // Player distance
        playerDirectionVector = GameManager.Instance.GetPlayer().GetPlayerPosition() - transform.position;

        // Calculate direction vector of player from weapon shoot position
        weaponDirection = GameManager.Instance.GetPlayer().GetPlayerPosition() - weaponShootPosition;

        // Get weapon to player angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get enemy to player angle
        enemyAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirectionVector);

        // Set enemy aim direction
        enemyAimDirection = HelperUtilities.GetAimDirection(enemyAngleDegrees);

        // Adjust weapon shoot position
        switch (enemyAimDirection)
        {
            case AimDirection.Up:
                weaponShootPosition = enemy.enemyDetails.enemyWeapon.weaponUpShootPosition;
                break;
            case AimDirection.Down:
                weaponShootPosition = enemy.enemyDetails.enemyWeapon.weaponDownShootPosition;
                break;
            case AimDirection.Left:
            case AimDirection.UpLeft:
                weaponShootPosition = enemy.enemyDetails.enemyWeapon.weaponLeftShootPosition;
                break;
            case AimDirection.Right:
            case AimDirection.UpRight:
                weaponShootPosition = enemy.enemyDetails.enemyWeapon.weaponRightShootPosition;
                break;
            default:
                break;
        }

        // Trigger weapon aim methods
        enemy.aimWeapon.Aim(enemyAimDirection, enemyAngleDegrees);
        enemy.animateEnemy.ResetAimAnimationParameters();
        enemy.animateEnemy.SetAimWeaponAnimationParameters(enemyAimDirection);
    }

    private bool IsPlayerInLineOfSight(Vector3 weaponDirection, float enemyProjectileRange)
    {
        RaycastHit2D raycastHit2D = Physics2D.Raycast(weaponShootPosition, (Vector2)weaponDirection, enemyProjectileRange, layerMask);

        if (raycastHit2D && raycastHit2D.transform.CompareTag(Settings.playerTag))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// Enemy character attack motion
    /// </summary>
    IEnumerator EnemyAttackAnimRoutine()
    {
        enemy.enemyAI.enemyPhase = EnemyPhase.Attack;

        if (enemy.health.currentHealth > 0f)
        {
            yield return new WaitForSeconds(1f);
        }

        enemyAttackCoroutine = null;
        enemy.enemyAI.enemyPhase = EnemyPhase.Patrol;
    }
}