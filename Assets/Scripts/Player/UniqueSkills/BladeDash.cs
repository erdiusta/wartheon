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

                if (currentEnemy != null)
                {
                    int damage = (int)(player.currentMainHandMaxDamageValue * 1.5f);
                    currentEnemy.health.TakeDamage(damage, player.rb2D.position, currentEnemy.rb2D.position);
                }
            }
        }
    }
}
