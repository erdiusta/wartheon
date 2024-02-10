using UnityEngine;
using Random = UnityEngine.Random;
using System.Collections;
using UnityEngine.Tilemaps;

[DisallowMultipleComponent]
public class DealContactDamage : MonoBehaviour
{
    #region Header DEAL DAMAGE
    [Space(10)]
    [Header("DEAL DAMAGE")]
    #endregion
    #region Tooltip
    [Tooltip("The contact damage to deal (is overridden by the receiver)")]
    #endregion
    [SerializeField] int contactDamageAmount;
    #region Tooltip
    [Tooltip("Specify what layers objects should be on to receive contact damage")]
    #endregion
    [SerializeField] private LayerMask layerMask;

    Enemy enemy;
    bool isColliding = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    // Trigger contact damage when enter a collider
    private void OnTriggerEnter2D(Collider2D collision)
    {
        // If already colliding with something return
        if (isColliding)
            return;

        if (tag == "Chest Item") return;

        if (tag == "Enemy")
        {
            EnemyAttack();
        }

        ContactDamage(collision);
    }

    // Trigger contact damage when staying withing a collider
    private void OnTriggerStay2D(Collider2D collision)
    {
        // If already colliding with something return
        if (isColliding) return;

        // Exclude wall collision
        if (collision.GetComponent<TilemapCollider2D>() == null)
        {
            if (tag == "Chest Item") return;

            if (tag == "Enemy")
            {
                EnemyAttack();
            }

            ContactDamage(collision);
        }
    }

    private void ContactDamage(Collider2D collision)
    {
        // If the collision object isn't in the specified layer then return (use bitwise comparison)
        int collisionObjectLayerMask = (1 << collision.gameObject.layer);

        if ((layerMask.value & collisionObjectLayerMask) == 0)
            return;

        // Check to see if the colliding object should take contact damage
        ReceiveContactDamage receiveContactDamage = collision.GetComponent<ReceiveContactDamage>();

        if (receiveContactDamage != null)
        {
            isColliding = true;

            // Reset the contact collision after set time
            Invoke("ResetContactCollision", Settings.contactDamageCollisionResetDelay);

            receiveContactDamage.TakeContactDamage(contactDamageAmount, receiveContactDamage.transform.position, transform.position);

            if (collision.tag == "Player")
            {
                // Apply knockback
                Knockback knockback = collision.GetComponent<Player>().GetComponent<Knockback>();
                collision.GetComponent<Player>().movementByVelocity.Knockback((collision.transform.position - transform.position).normalized,
                    knockback.knockbackForce, knockback.knockbackTimeWeight);

                if (enemy != null && enemy.enemyDetails.isPoisonous)
                {
                    // Check get poisoned
                    float randomPoisonNum = Random.Range(0f, 1f);
                    if (randomPoisonNum > 0.7f)
                    {
                        collision.GetComponent<Player>().healthEvent.CallGetPosionedEvent();
                        collision.GetComponent<Player>().healthStatus = HealthStatus.Poisoned;
                    }
                }
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
        HelperUtilities.ValidateCheckPositiveValue(this, nameof(contactDamageAmount), contactDamageAmount, true);
    }
#endif
    #endregion
}
