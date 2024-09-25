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
        GetDamageFromSummonedEnemies(collision);
    }

    // Trigger contact damage when enter a collider
    private void OnTriggerStay2D(Collider2D collision)
    {
        // If already colliding with something return
        if (isColliding) return;

        ContactDamage(collision);
        GetDamageFromSummonedEnemies(collision);
    }

    private void ContactDamage(Collider2D collision)
    {
        // If the collision object isn't in the specified layer then return (use bitwise comparison)
        int collisionObjectLayerMask = (1 << collision.gameObject.layer);
        if ((layerMask.value & collisionObjectLayerMask) == 0) return;

        // Check to see if the colliding object should take contact damage
        ReceiveContactDamage receiveContactDamage = collision.GetComponent<ReceiveContactDamage>();

        if (receiveContactDamage != null)
        {
            isColliding = true;

            // Reset the contact collision after set time
            Invoke("ResetContactCollision", Settings.contactDamageCollisionResetDelay);

            if (collision.tag == Settings.playerTag)
            {
                if (enemy.isDead) return;

                Player player = collision.GetComponent<Player>();

                // Hit successful
                if (100 - player.currentDeflectionValue * 100 > Random.Range(0, 100))
                {
                    // Damage produced by enemy
                    int damageDone = Random.Range(contactDamageAmountMin, contactDamageAmountMin);

                    if (player.health.isDamageable)
                    {
                        if (enemy != null)
                        {
                            if (player.isBlockingActive)
                            {
                                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveTwoSoundEffect);
                                player.health.PostHitImmunity(true);
                                player.isBlockingActive = false;
                                player.healthEvent.CallArmorWoreOffEvent();
                            }
                            else
                            {
                                // Check if collider is a decoy
                                if (collision.GetComponent<Decoy>() != null)
                                {
                                    receiveContactDamage.TakeContactDamage(damageDone, receiveContactDamage.transform.position, transform.position);
                                    return;
                                }

                                if (player.onStealth) return;

                                CheckPoisonStatus(player);
                                CheckAcidStatus(player);
                                CheckStunStatus(player);
                                CheckCurseStatus(player);

                                // Damage inflicted to enemy after deducting enemy armor
                                int inflictedDamage = damageDone > player.health.GetArmorValue() ? damageDone - player.health.GetArmorValue() : 1;

                                receiveContactDamage.TakeContactDamage(inflictedDamage, receiveContactDamage.transform.position, transform.position);
                            }

                            // Apply knockback
                            player.movementByVelocity.TriggerKnockback((player.transform.position - transform.position).normalized);
                        }
                    }
                }
                else
                {
                    player.health.isBlocking = true;
                    player.healthEvent.CallDeflectionEvent();
                    player.health.TakeDamage(0, transform.position, player.health.transform.position, false);
                }
            }
            else if (collision.tag == Settings.summonedEnemyTag)
            {
                // Damage produced by enemy - %70 less effect to summoned enemy than player
                int damageDone = Random.Range((int)(contactDamageAmountMin * 0.3f), (int)(contactDamageAmountMin * 0.3f));

                // Apply damage
                receiveContactDamage.TakeContactDamage(damageDone, receiveContactDamage.transform.position, transform.position);
            }
            else if (collision.tag == Settings.decoyTag)
            {
                receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
                enemy.enemyAI.TriggerKnockback((transform.position - collision.transform.position));
            }
            else
            {
                receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
            }
        }
    }

    private void GetDamageFromSummonedEnemies(Collider2D collision)
    {
        // If the collision object isn't in the specified layer then return (use bitwise comparison)
        int collisionObjectLayerMask = (1 << collision.gameObject.layer);
        if ((layerMask.value & collisionObjectLayerMask) == 0) return;

        if (collision.tag == Settings.enemyTag)
        {
            isColliding = true;

            Enemy enemy = collision.GetComponent<Enemy>();

            // Reset the contact collision after set time
            Invoke("ResetContactCollision", Settings.contactDamageCollisionResetDelay);

            // Damage produced by enemy
            int damageDone = Random.Range(contactDamageAmountMin, contactDamageAmountMin);

            // Apply knockback and damage the enemy
            enemy.enemyAI.TriggerKnockback(transform.position - collision.transform.position);
            enemy.health.TakeDamage(damageDone, transform.position, collision.transform.position, false);
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
                if (player.armorStatus == ArmorStatus.SilverArmor)
                {
                    player.healthEvent.CallArmorWoreOffEvent();
                }

                player.armorStatus = ArmorStatus.Acid;
                player.health.SetArmorValue((int)(player.currentPhysicalResistanceValue * (1 - enemy.enemyDetails.acidEfficiency)));
                player.healthEvent.CallGetAcidEvent();
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
    /// Check curse status - Player
    /// </summary>
    private void CheckCurseStatus(Player player)
    {
        if (enemy.enemyDetails.hasCurseDamage)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.curseChance)
            {
                player.isCursed = true;
                player.healthEvent.CallGetCurseEvent();
            }
        }
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
