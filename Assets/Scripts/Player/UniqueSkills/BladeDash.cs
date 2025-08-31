using UnityEngine;

public class BladeDash : MonoBehaviour
{
    Player player;

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == Settings.enemyTag)
        {
            if (player.isBladeAndDashActive)
            {
                Enemy currentEnemy = collision.GetComponent<Enemy>();

                Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

                if (currentEnemy != null)
                {
                    float damageModifier = 0;

                    if (!player.bladeAndDashOnRecast)
                    {
                        damageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 0.5f,
                            2 => 0.65f,
                            3 => 0.85f,
                            _ => 0f
                        };
                    }
                    else
                    {
                        damageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 0.7f,
                            2 => 0.8f,
                            3 => 1f,
                            _ => 0f
                        };
                    }

                    int inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(currentEnemy, mainHandWeapon, MeleeHand.MainHand, damageModifier);

                    currentEnemy.health.TakeDamage(inflictedDamage, player.rb2D.position, currentEnemy.rb2D.position);
                }
            }
        }
    }
}
