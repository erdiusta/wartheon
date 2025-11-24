using System.Collections;
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
    public int contactDamageAmountMin;
    #region Tooltip
    [Tooltip("The max contact damage to deal (is overridden by the receiver)")]
    #endregion
    public int contactDamageAmountMax;
    #region Tooltip
    [Tooltip("Specify what layers objects should be on to receive contact damage")]
    #endregion
    [SerializeField] private LayerMask layerMask;

    Enemy enemy;
    Player player;
    [HideInInspector] public bool isColliding = false;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
    }

    private void OnEnable()
    {
        player = GetComponentInParent<Player>();
    }

    // Trigger contact damage when enter a collider
    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If already colliding with something return
        if (isColliding) return;

        ContactDamage(collision.collider);
        GetDamageFromSummonedEnemies(collision.collider);
    }

    // Trigger contact damage when enter a collider
    private void OnCollisionStay2D(Collision2D collision)
    {
        // If already colliding with something return
        if (isColliding) return;

        ContactDamage(collision.collider);
        GetDamageFromSummonedEnemies(collision.collider);
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
            //// Reset the contact collision after set time
            //ResetContactCollision();

            if (collision.tag == Settings.playerTag)
            {
                if (enemy.health.hasDied) return;

                Player player = collision.GetComponent<Player>();

                if (player.playerControl.IsParrying)
                {
                    if (InputManager.TutorialEnabled && TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.Parry)
                    {
                        TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                    }

                    if (tag == Settings.enemyTag && player.isCounterRiposteActive)
                    {
                        int counterRiposteDamage = player.isCursed ? player.currentMainHandMinDamageValue / 2 :
                            Random.Range(player.currentMainHandMinDamageValue, player.currentMainHandMaxDamageValue) / 2;

                        enemy.health.TakeDamage(counterRiposteDamage, player.transform.position, transform.position);
                    }

                    player.healthEvent.CallParryEvent();
                    player.health.PostHitImmunity(true);
                    return;
                }

                if (player.playerControl.isDashing) return; // If player is in dash mode, player will receive no damage

                float blindPenalty = enemy.isBlind ? 0.5f : 0f;

                int diceRoll = Random.Range(1, 101);
                bool isAttackDodged = 100 - (player.currentDodgeValue + blindPenalty) * 100 < diceRoll ? true : false;

                // Evasiveness - dodge check
                if (!isAttackDodged && !player.playerControl.isPlayerRolling && !player.isWhirlrendActive)
                {
                    contactDamageAmountMin = enemy.enemyDetails.dealtMeleeDamageMin;
                    contactDamageAmountMax = enemy.enemyDetails.dealtMeleeDamageMax;

                    // Damage produced by enemy
                    int damageDone = enemy.isCursed ? contactDamageAmountMin : Random.Range(contactDamageAmountMin, contactDamageAmountMax);

                    if (player.health.isDamageable)
                    {
                        if (enemy != null)
                        {
                            if (player.isValorActive)
                            {
                                //SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.activeSkillTwoSoundEffect);
                                player.health.PostHitImmunity(true);
                            }
                            else
                            {
                                // Check if collider is a decoy
                                if (collision.GetComponent<Decoy>() != null)
                                {
                                    receiveContactDamage.TakeContactDamage(damageDone, receiveContactDamage.transform.position, transform.position);
                                    return;
                                }

                                if (player.isStealthActive) return;

                                int inflictedDamage = CalculateDamageAmount(player, damageDone);

                                if (player.isKynarasEmbraceActive)
                                {
                                    switch (player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel())
                                    {
                                        case 1:
                                            inflictedDamage = (int)(inflictedDamage * 0.6f);
                                            break;
                                        case 2:
                                            inflictedDamage = (int)(inflictedDamage * 0.5f);
                                            break;
                                        case 3:
                                            inflictedDamage = (int)(inflictedDamage * 0.4f);
                                            break;
                                        default:
                                            break;
                                    }

                                    // Enemy gets burned
                                    enemy.healthEvent.CallGetBurnEvent();
                                    enemy.healthStatus |= HealthStatus.Burned; // Add Burned status
                                }

                                receiveContactDamage.TakeContactDamage(inflictedDamage, receiveContactDamage.transform.position, transform.position);

                                if (player.health.GetCurrentHealth() > 0)
                                {
                                    CheckBleedingStatus(player);
                                    CheckStunStatus(player);
                                    CheckSlowStatus(player);
                                    CheckWarmStatus(player);
                                    CheckBurnStatus(player);
                                    CheckPoisonStatus(player);
                                    CheckRootStatus(player);
                                    CheckChillStatus(player);
                                    CheckFrostStatus(player);
                                    CheckStaticStatus(player);
                                    CheckParalyzeStatus(player);
                                    CheckBlindStatus(player);
                                    CheckCurseStatus(player);
                                    CheckFearStatus(player);
                                }
                            }

                            // Apply knockback
                            enemy.enemyAI.ApplyKnockbackToPlayer(player);
                        }
                    }
                }
                else
                {
                    if (isAttackDodged && player.isShiftingStanceActive)
                    {
                        if (!player.shiftingStanceOnProcess)
                        {
                            player.shiftingStanceOnProcess = true;
                            StartCoroutine(ShiftingStanceRoutine(player));
                        }
                    }

                    player.health.isDodging = true;
                    player.healthEvent.CallDodgeEvent();
                    player.health.PostHitImmunity(true);
                }
            }
            else if (collision.tag == Settings.summonedEnemyTag)
            {
                // Damage produced by enemy - %70 less effect to summoned enemy than player
                int damageDone = Random.Range((int)(contactDamageAmountMin * 0.3f), (int)(contactDamageAmountMax * 0.3f));

                // Apply damage
                receiveContactDamage.TakeContactDamage(damageDone, receiveContactDamage.transform.position, transform.position);
            }
            else if (collision.tag == Settings.decoyTag)
            {
                receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
                //enemy.enemyAI.TriggerKnockback((transform.position - collision.transform.position));
            }
            else if (collision.tag == Settings.practiceDummy)
            {
                return;
            }
            else if (collision.tag == Settings.environment)
            {
                if (player != null && player.isWhirlrendActive)
                {
                    // Damage produced by enemy - %70 less effect to summoned enemy than player
                    int damageDone = Random.Range(contactDamageAmountMin, contactDamageAmountMax);

                    Enemy enemy = collision.GetComponent<Enemy>();

                    // Apply damage
                    receiveContactDamage.TakeContactDamage(damageDone, receiveContactDamage.transform.position, transform.position);
                    return;
                }

                receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
            }
            else
            {
                if (tag != Settings.playerWeapon)
                {
                    receiveContactDamage.TakeContactDamage(contactDamageAmountMax, receiveContactDamage.transform.position, transform.position);
                }
            }
        }
    }

    private int CalculateDamageAmount(Player player, int damageDone)
    {
        // Segregate elemental and non-elemental damage
        int magicDamage = (int)(enemy.enemyDetails.elementalForgeRate * damageDone);
        int nonElementalDamage = damageDone - magicDamage;

        int inflictedNonElementalDamage = 0;

        if (player.thirtyPercentDamageAbsorbIsActive)
        {
            inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - player.currentArmorValue));
            int absorbedDamage = (int)(inflictedNonElementalDamage * 0.3f);
            inflictedNonElementalDamage -= absorbedDamage;
        }
        else
        {
            inflictedNonElementalDamage = (int)(nonElementalDamage * (1 - player.currentArmorValue));
        }

        int inflictedMagicDamage = (int)(magicDamage * (1 - player.currentMagicResistanceValue));

        int inflictedDamage = Mathf.RoundToInt((inflictedMagicDamage + inflictedNonElementalDamage) * (1 - player.currentDamageReductionValue));

        return inflictedDamage;
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
            Invoke(nameof(ResetContactCollision), Settings.contactDamageCollisionResetDelay);

            // Damage produced by enemy
            int damageDone = Random.Range(contactDamageAmountMin, contactDamageAmountMin);

            // Apply knockback and damage the enemy
            //enemy.enemyAI.TriggerKnockback(transform.position - collision.transform.position);
            enemy.health.TakeDamage(damageDone, transform.position, collision.transform.position, null, MeleeHand.MainHand);
        }
    }

    IEnumerator ShiftingStanceRoutine(Player player)
    {
        player.CurrentAgilityValue += 3;

        yield return new WaitForSeconds(3f);

        player.shiftingStanceOnProcess = false;
        player.CurrentAgilityValue -= 3;
    }

    /// <summary>
    /// Check bleeding status - Player
    /// </summary>
    private void CheckBleedingStatus(Player player)
    {
        if (player.isImmunetoBleeding) return;

        if (enemy.enemyDetails.hasBleedingDamage)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.bleedingChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.bleedAnimator.SetTrigger(Settings.activateVFX);
                player.healthEvent.CallGetBleedingEvent();
                player.healthStatus |= HealthStatus.Bleeding; // Add Burned status
            }
        }
    }

    /// <summary>
    /// Check warm status
    /// </summary>
    public void CheckWarmStatus(Player player)
    {
        if (player.isImmunetoBurn) return;

        bool isWarmed = (player.healthStatus & HealthStatus.Burned) != 0;

        if (enemy.enemyDetails.canWarm && player.health.GetCurrentHealth() > 0)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < enemy.enemyDetails.warmChance)
            {
                if (player.isWarmed && !isWarmed)
                {
                    player.healthStatus |= HealthStatus.Burned; // Second chill 
                    player.healthEvent.CallGetBurnEvent();

                    player.healthEvent.CallWarmCuredEvent();
                }
                else if (!player.isWarmed && !isWarmed)
                {
                    player.isWarmed = true;
                    player.healthEvent.CallGetWarmedEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check burn status
    /// </summary>
    private void CheckBurnStatus(Player player)
    {
        if (player.isImmunetoBurn) return;

        if (enemy.enemyDetails.canBurn)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.burnChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.burnAnimator.SetTrigger(Settings.activateVFX);
                player.healthEvent.CallGetBurnEvent();
                player.healthStatus |= HealthStatus.Burned; // Add Burned status
            }
        }
    }

    /// <summary>
    /// Check poison status
    /// </summary>
    private void CheckPoisonStatus(Player player)
    {
        if (player.isImmunetoPoison) return;

        if (enemy.enemyDetails.isPoisonous)
        {
            // Check get poisoned
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.poisonChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.poisonAnimator.SetTrigger(Settings.activateVFX);
                player.healthEvent.CallGetPoisonedEvent();
                player.healthStatus |= HealthStatus.Poisoned; // Add Poisoned status
            }
        }
    }

    /// <summary>
    /// Check stun status
    /// </summary>
    private void CheckStunStatus(Player player)
    {
        bool isStunned = (player.moveStatus & MoveStatus.Stun) != 0;

        if (enemy.enemyDetails.hasStunDamage && !isStunned)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.stunChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.stunAnimator.SetTrigger(Settings.activateVFX);
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Stun;
                player.healthEvent.CallGetStunEvent();
            }
        }
    }

    /// <summary>
    /// Check slow status
    /// </summary>
    private void CheckSlowStatus(Player player)
    {
        if (enemy.enemyDetails.hasSlowDamage && !player.isSlowed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.slowChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.slowAnimator.SetTrigger(Settings.activateVFX);
                player.playerControl.isPlayerRolling = false;

                player.isSlowed = true;
                player.healthEvent.CallGetSlowEvent();
            }
        }
    }

    /// <summary>
    /// Check root status
    /// </summary>
    private void CheckRootStatus(Player player)
    {
        bool isRooted = (player.moveStatus & MoveStatus.Root) != 0;

        if (enemy.enemyDetails.hasRootDamage && !isRooted)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.rootChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.rootAnimator.SetTrigger(Settings.activateVFX);
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Root;
                player.healthEvent.CallGetRootEvent();
            }
        }
    }

    /// <summary>
    /// Check chill status
    /// </summary>
    public void CheckChillStatus(Player player)
    {
        if (player.isImmunetoFrost) return;

        bool isFrozen = (player.moveStatus & MoveStatus.Frozen) != 0;

        if (enemy.enemyDetails.hasChillDamage && player.health.GetCurrentHealth() > 0)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < enemy.enemyDetails.chillChance)
            {
                if (player.isChilled && !isFrozen)
                {
                    player.moveStatus |= MoveStatus.Frozen; // Second chill 
                    player.healthEvent.CallGetFrostEvent();

                    player.healthEvent.CallChillCuredEvent();
                }
                else if (!player.isChilled && !isFrozen)
                {
                    player.isChilled = true;
                    player.healthEvent.CallGetChillEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check frost status
    /// </summary>
    private void CheckFrostStatus(Player player)
    {
        if (player.isImmunetoFrost) return;

        bool isFrozen = (player.moveStatus & MoveStatus.Frozen) != 0;

        if (enemy.enemyDetails.hasFrostDamage && !isFrozen)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.frostChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.frostAnimator.SetTrigger(Settings.activateVFX);
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Frozen;
                player.healthEvent.CallGetFrostEvent();
            }
        }
    }

    /// <summary>
    /// Check static status
    /// </summary>
    public void CheckStaticStatus(Player player)
    {
        if (player.isImmunetoParalyze) return;

        bool isParalyzed = (player.moveStatus & MoveStatus.Paralyze) != 0;

        if (enemy.enemyDetails.hasStaticDamage && player.health.GetCurrentHealth() > 0)
        {
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < enemy.enemyDetails.staticChance)
            {
                if (player.isStatic && !isParalyzed)
                {
                    player.moveStatus |= MoveStatus.Paralyze; // Second chill 
                    player.healthEvent.CallGetParalyzedEvent();

                    player.healthEvent.CallStaticCuredEvent();
                }
                else if (!player.isStatic && !isParalyzed)
                {
                    player.isStatic = true;
                    player.healthEvent.CallStaticCuredEvent();
                }
            }
        }
    }

    /// <summary>
    /// Check paralyze status
    /// </summary>
    private void CheckParalyzeStatus(Player player)
    {
        if (player.isImmunetoParalyze) return;

        bool isParalyzed = (player.moveStatus & MoveStatus.Paralyze) != 0;

        if (enemy.enemyDetails.hasParalyzeDamage && !isParalyzed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.paralyzeChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.paralyzeAnimator.SetTrigger(Settings.activateVFX);
                player.playerControl.isPlayerRolling = false;

                player.moveStatus |= MoveStatus.Paralyze;
                player.healthEvent.CallGetParalyzedEvent();
            }
        }
    }

    /// <summary>
    /// Check curse status - Player
    /// </summary>
    private void CheckCurseStatus(Player player)
    {
        if (enemy.enemyDetails.hasCurseDamage && !player.isCursed)
        {
            float randomDice = Random.Range(0f, 1f);
            if (randomDice < enemy.enemyDetails.curseChance - player.currentStatusResistance)
            {
                player.statusEffectAnimators.curseAnimator.SetTrigger(Settings.activateVFX);
                player.isCursed = true;
                player.healthEvent.CallGetCurseEvent();
            }
        }
    }

    /// <summary>
    /// Check blind status - Player
    /// </summary>
    private void CheckBlindStatus(Player player)
    {
        if (player.isImmunetoBlind) return;

        if (enemy.enemyDetails.hasBlindDamage && !player.isBlind)
        {
            player.statusEffectAnimators.blindAnimator.SetTrigger(Settings.activateVFX);
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < enemy.enemyDetails.blindChance - player.currentStatusResistance)
            {
                player.healthEvent.CallGetBlindEvent();
            }
        }
    }

    /// <summary>
    /// Check fear status - Player
    /// </summary>
    private void CheckFearStatus(Player player)
    {
        if (player.isImmunetoFear) return;

        if (enemy.enemyDetails.hasFearDamage && !player.isFeared)
        {
            player.statusEffectAnimators.fearAnimator.SetTrigger(Settings.activateVFX);
            float randomDice = Random.Range(0f, 1f);

            if (randomDice < enemy.enemyDetails.fearChance - player.currentStatusResistance)
            {
                player.healthEvent.CallGetFearEvent();
            }
        }
    }

    /// <summary>
    /// Reset the isColliding bool
    /// </summary>
    IEnumerator ResetContactCollision()
    {
        yield return null;

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
