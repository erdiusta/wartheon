using UnityEngine;
using Random = UnityEngine.Random;

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
    bool isColliding = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
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

        if (tag == "Chest Item") return;

        // Check to see if the colliding object should take contact damage
        ReceiveContactDamage receiveContactDamage = collision.GetComponent<ReceiveContactDamage>();

        if (receiveContactDamage != null)
        {
            isColliding = true;

            // Reset the contact collision after set time
            Invoke("ResetContactCollision", Settings.contactDamageCollisionResetDelay);

            if (collision.tag == Settings.playerTag)
            {
                Player player = collision.GetComponent<Player>();

                // Damage produced by enemy
                int damageDone = Random.Range(contactDamageAmountMin, contactDamageAmountMin);

                if (enemy != null)
                {
                    // Check if collider is a decoy
                    if (collision.GetComponent<Decoy>() != null)
                    {
                        receiveContactDamage.TakeContactDamage(damageDone, receiveContactDamage.transform.position, transform.position);
                        return;
                    }

                    if (player.playerDetails.onStealth) return;

                    CheckPoisonStatus(player);
                    CheckAcidStatus(player);
                    CheckBleedingStatus(player);
                    CheckStunStatus(player);
                    CheckSlowStatus(player);

                    // Apply knockback
                    player.movementByVelocity.TriggerKnockback((player.transform.position - transform.position).normalized);
                }

                // Damage inflicted to enemy after deducting enemy armor
                int inflictedDamage = damageDone > player.health.GetArmorValue() ?
                    damageDone - player.health.GetArmorValue() : 1;

                receiveContactDamage.TakeContactDamage(inflictedDamage, receiveContactDamage.transform.position, transform.position);
            }
            else if (collision.tag == Settings.decoyTag)
            {
                receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
                enemy.enemyMovementAI.TriggerKnockback((transform.position - collision.transform.position));
            }
            else
            {
                receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
            }
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
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.poisonChance)
            {
                player.healthEvent.CallGetPoisonedEvent();
                player.healthStatus = HealthStatus.Poisoned;
            }
        }
    }

    /// <summary>
    /// Check bleeding status
    /// </summary>
    private void CheckBleedingStatus(Player player)
    {
        if (enemy.enemyDetails.hasBleedingDamage)
        {
            // Check get bleeding
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.bleedingChance)
            {
                player.healthEvent.CallGetBleedingEvent();
                player.healthStatus = HealthStatus.Bleeding;
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
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.acidEfficiency)
            {
                if (player.armorStatus == ArmorStatus.SilverArmor || player.armorStatus == ArmorStatus.GoldenArmor)
                {
                    player.healthEvent.CallArmorWoreOffEvent();
                }

                player.armorStatus = ArmorStatus.Acid;
                player.health.SetArmorValue((int)(player.playerDetails.playerArmorValue * (1 - enemy.enemyDetails.acidEfficiency)));
                player.healthEvent.CallGetAcidEvent();
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    private void CheckStunStatus(Player player)
    {
        if (enemy.enemyDetails.hasStunDamage && player.moveStatus != MoveStatus.Stun && player.moveStatus != MoveStatus.Slow)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.stunChance)
            {
                player.moveStatus = MoveStatus.Stun;
                player.healthEvent.CallGetStunEvent();
                player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
                player.animator.SetBool(Settings.isStunned, true);
            }
        }
    }

    /// <summary>
    /// Check slow status
    /// </summary>
    private void CheckSlowStatus(Player player)
    {
        if (enemy.enemyDetails.hasSlowDamage && player.moveStatus != MoveStatus.Stun && player.moveStatus != MoveStatus.Slow)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.slowChance)
            {
                SlowPlayerSpeed(player);
            }
        }
    }

    private void SlowPlayerSpeed(Player player)
    {
        float slowedMinMoveSpeed = player.movementByVelocity.movementDetails.minMoveSpeed * 0.6f;
        float slowedMaxMoveSpeed = player.movementByVelocity.movementDetails.maxMoveSpeed * 0.6f;
        player.movementByVelocity.moveSpeed = Random.Range(slowedMinMoveSpeed, slowedMaxMoveSpeed);
        player.moveStatus = MoveStatus.Slow;
        player.healthEvent.CallGetSlowEvent();
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
