using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;

[DisallowMultipleComponent]
public class DealContactDamage : MonoBehaviour
{
    #region Header DEAL DAMAGE
    [Space(10)]
    [Header("DEAL DAMAGE")]
    #endregion
    #region Tooltip
    [Tooltip("The min contact damage to deal (is overridden by the receiver)")]
    #endregion
    [SerializeField] int contactDamageAmountMin;
    #region Tooltip
    [Tooltip("The max contact damage to deal (is overridden by the receiver)")]
    #endregion
    [SerializeField] int contactDamageAmountMax;
    #region Tooltip
    [Tooltip("Specify what layers objects should be on to receive contact damage")]
    #endregion
    [SerializeField] private LayerMask layerMask;

    Enemy enemy;
    Player player;
    ChestItem chestItem;    
    bool isColliding = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        player = GetComponent<Player>();
    }

    // Trigger contact damage when enter a collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If already colliding with something return
        if (isColliding) return;

        ContactDamage(collision);
    }

    // Trigger contact damage when enter a collider
    private void OnTriggerStay2D(Collider2D collision)
    {
        // If already colliding with something return
        if (isColliding) return;

        ContactDamage(collision);
    }

    private void ContactDamage(Collider2D collision)
    {
        // If the collision object isn't in the specified layer then return (use bitwise comparison)
        int collisionObjectLayerMask = (1 << collision.gameObject.layer);

        if ((layerMask.value & collisionObjectLayerMask) == 0) return;

        if (tag == "Enemy")
        {
            EnemyAttack();
        }

        // Check to see if the colliding object should take contact damage
        ReceiveContactDamage receiveContactDamage = collision.GetComponent<ReceiveContactDamage>();

        if (receiveContactDamage != null)
        {
            isColliding = true;

            // Reset the contact collision after set time
            Invoke("ResetContactCollision", Settings.contactDamageCollisionResetDelay);

            if (collision.tag == "Player")
            {
                Player player = collision.GetComponent<Player>();

                if (enemy != null)
                {
                    CheckPoisonStatus(player);
                    CheckAcidStatus(player);
                    CheckStunStatus(player);

                    // Apply knockback
                    Knockback knockback = player.knockback;
                    player.movementByVelocity.TriggerKnockback((player.transform.position - transform.position).normalized);
                }

                // Damage produced by enemy
                int damageDone = Random.Range(contactDamageAmountMin,contactDamageAmountMin);

                // Damage inflicted to enemy after deducting enemy armor
                int inflictedDamage = damageDone > player.health.GetArmorValue() ?
                    damageDone - player.health.GetArmorValue() : 1;

                receiveContactDamage.TakeContactDamage(inflictedDamage, receiveContactDamage.transform.position, transform.position);
            }
            else
            {
                receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
            }
        }
    }

    /// <summary>
    /// Enemy character attack motion
    /// </summary>
    private void EnemyAttack()
    {
        if (enemy.health.currentHealth > 0f)
        {
            enemy.animator.SetBool(Settings.isIdle, false);
            enemy.animator.SetBool(Settings.isMoving, false);

            // Adjust animator layer weights
            enemy.animator.SetLayerWeight(enemy.animateEnemy.baseLayerIndex, 0f);
            enemy.animator.SetLayerWeight(enemy.animateEnemy.attackLayerIndex, 1f);
            enemy.animator.SetLayerWeight(enemy.animateEnemy.getHitLayerIndex, 0f);
            enemy.animator.SetLayerWeight(enemy.animateEnemy.deathLayerIndex, 0f);

            enemy.animator.SetBool(Settings.attackMotion, true);
            StartCoroutine(EnemyAttackRoutine());
        }
        else
        {
            enemy.animator.SetLayerWeight(enemy.animateEnemy.baseLayerIndex, 0f);
            enemy.animator.SetLayerWeight(enemy.animateEnemy.attackLayerIndex, 0f);
            enemy.animator.SetLayerWeight(enemy.animateEnemy.getHitLayerIndex, 0f);
            enemy.animator.SetLayerWeight(enemy.animateEnemy.deathLayerIndex, 1f);
        }
    }

    /// <summary>
    /// Check poison status
    /// </summary>
    private void CheckPoisonStatus(Player player)
    {
        if (enemy.enemyDetails.isPoisonous)
        {
            // Check get poisoned
            float randomPoisonNum = Random.Range(0f, 1f);
            if (randomPoisonNum > 0.6f)
            {
                player.healthEvent.CallGetPosionedEvent();
                player.healthStatus = HealthStatus.Poisoned;
            }
        }
    }

    /// <summary>
    /// Check acid status
    /// </summary>
    private void CheckAcidStatus(Player player)
    {
        if (enemy.enemyDetails.hasAcid && player.armorStatus != ArmorStatus.Acid)
        {
            // Check get acid
            float randomAcidNum = Random.Range(0f, 1f);
            if (randomAcidNum > 0.6f)
            {
                player.GetComponent<Health>().SetArmorValue((int)(player.playerDetails.playerArmorValue *
                    (1 - enemy.enemyDetails.acidEfficiency)));
                player.healthEvent.CallGetAcidEvent();
                player.armorStatus = ArmorStatus.Acid;
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    private void CheckStunStatus(Player player)
    {
        if (enemy.enemyDetails.hasStunDamage && player.moveStatus != MoveStatus.Stun)
        {
            float randomStunNum = Random.Range(0f, 1f);
            if (randomStunNum > 0.6f)
            {
                player.moveStatus = MoveStatus.Stun;
                player.healthEvent.CallGetStunEvent();
                player.animator.SetBool(Settings.isStunned, true);
            }
        }
    }

    IEnumerator EnemyAttackRoutine()
    {
        enemy.animateEnemy.isAttacking = true;

        yield return new WaitForSeconds(0.2f);

        enemy.animateEnemy.isAttacking = false;
    }

    /// <summary>
    /// Reset the isColliding bool
    /// </summary>
    private void ResetContactCollision()
    {
        isColliding = false;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(contactDamageAmountMax), contactDamageAmountMax, true);
    }
#endif
    #endregion
}
