using UnityEngine;

public class Whirlend : MonoBehaviour
{
    [HideInInspector] public SoundEffectSO playedWhirlrendSound;

    Player player;
    CircleCollider2D circleCollider2D;

    private void Awake()
    {
        circleCollider2D = GetComponent<CircleCollider2D>();
    }

    private void OnEnable()
    {
        circleCollider2D.enabled = false;
    }


    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    private void Update()
    {
        if (player.isWhirlrendActive)
        {
            circleCollider2D.enabled = true;
            ReceiveContactDamage playerContactDamage = player.GetComponent<ReceiveContactDamage>();

            // Get all colliders within the radius of the seismic slam
            Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, circleCollider2D.radius);

            foreach (Collider2D col in colliders)
            {
                // Check if the collider belongs to an enemy or any other object you want to affect
                if (col.CompareTag(Settings.enemyTag))
                {
                    // Apply damage to the enemy
                    Enemy enemy = col.GetComponent<Enemy>();

                    ReceiveContactDamage enemyContactDamage = enemy.GetComponent<ReceiveContactDamage>();

                    if (enemy.health != null)
                    {
                        float enemyDamageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 0.7f,
                            2 => 0.8f,
                            3 => 1f,
                            _ => 1f
                        };

                        DamageContext ctx = new DamageContext { source = DamageSourceType.Melee };
                        enemyContactDamage.TakeContactDamage((int)(player.whirlrendDamage * enemyDamageModifier), ctx);

                        float playerDamageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 0.15f,
                            2 => 0.13f,
                            3 => 0.1f,
                            _ => 0f
                        };

                        playerContactDamage.TakeContactDamage((int)(player.whirlrendDamage * playerDamageModifier), ctx);
                    }

                    if (!enemy.enemyDetails.hasKnockbackResistance && enemy.health.currentHealth > 0)
                    {
                        CheckBleedingStatus(enemy);

                        // Knockback
                        Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        float knockbackForce = 7f; //Whirl rend hit force
                        float dealDamageMass = 1f;

                        enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                    }

                }
                else if (col.CompareTag(Settings.practiceDummy) || col.CompareTag(Settings.environment))
                {
                    ReceiveContactDamage propContactDamage = col.GetComponent<ReceiveContactDamage>();
                    DamageContext ctx = new DamageContext { source = DamageSourceType.Melee };

                    propContactDamage.TakeContactDamage(player.whirlrendDamage, ctx);
                    playerContactDamage.TakeContactDamage((int)(player.whirlrendDamage * 0.15f), ctx);
                }
            }
        }
    }

    /// <summary>
    /// Check bleeding status - Enemy
    /// </summary>
    private void CheckBleedingStatus(Enemy enemy)
    {
        // Check get bleeding
        float randomDice = Random.Range(0f, 1f);

        float chanceToBleed = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
        {
            1 => 0.1f,
            2 => 0.15f,
            3 => 0.2f,
            _ => 0
        };

        if (randomDice < chanceToBleed)
        {
            enemy.healthEvent.CallGetBleedingEvent();
            enemy.healthStatus |= HealthStatus.Bleeding; // Add Bleeding status
        }
    }
}
