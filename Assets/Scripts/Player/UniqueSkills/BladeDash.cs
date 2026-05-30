using Mirror;
using UnityEngine;

public class BladeDash : MonoBehaviour
{
    Player player;

    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
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

                    DamageContext ctx = new DamageContext { dealerPosition = player.rb2D.position, receiverPosition = currentEnemy.rb2D.position };
                    ReceiveMeleeDamage receiveMeleeDamage = collision.GetComponent<ReceiveMeleeDamage>();
                    receiveMeleeDamage.TakeMeleeDamage(inflictedDamage, ctx);

                    if (NetworkServer.active)
                    {
                        IHealthAuthority healthAuthority = HealthAuthorityResolver.GetAuthority(currentEnemy.gameObject);

                        // Check if player levels-after killing the enemy
                        int levelBeforeKillingEnemy = player.currentLevel;

                        if (healthAuthority.CurrentHealth <= 0)
                        {
                            player.NetAuth.Server_ExpGain(player.NetAuth.netIdentity, levelBeforeKillingEnemy, currentEnemy.enemyNetwork.netIdentity);
                        }
                    }
                }
            }
        }
    }
}
