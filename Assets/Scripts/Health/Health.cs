using System.Collections;
using UnityEngine;
using UnityEngine.XR;
using Random = UnityEngine.Random;

[RequireComponent(typeof(HealthEvent))]
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    public Animator hitFXAnimator;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int maximumHealth;
    [HideInInspector] public int currentShield;
    [HideInInspector] public bool isDamageable = true;
    [HideInInspector] public Enemy enemy;
    [HideInInspector] public bool isBlocking;
    [HideInInspector] public bool isDodging;
    [HideInInspector] public bool suddenDeathHappened;
    [HideInInspector] public FlashManager flashManager;
    [HideInInspector] public const float spriteFlashInterval = 0.1f;
    [HideInInspector] public bool fxAnimatorPlayed;
    [HideInInspector] public bool damageTaken;
    [HideInInspector] public bool hasDied = false;

    HealthEvent healthEvent;
    Player player;
    Coroutine immunityCoroutine;
    bool isImmuneAfterHit;
    float immunityTime = 0f;
    SpriteRenderer spriteRenderer;
    WaitForSeconds waitForSecondsSpriteFlashInterval = new WaitForSeconds(spriteFlashInterval);

    Coroutine bleedingCoroutine;
    Coroutine poisonCoroutine;
    Coroutine burnCoroutine;
    int bleedingPeriodCount = 0;
    int poisonPeriodCount = 0;
    int burnPeriodCount = 0;

    bool isProjectileHit = false;
    Decoy decoy;

    // Inner Path
    float secondBreathHealTimer = 0f;

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        flashManager = GetComponent<FlashManager>();
    }

    private void Start()
    {
        // Trigger a health event for UI update
        if (player != null && !player.isInitialized) return;

        // Trigger health event
        if (player == null && tag != Settings.practiceDummy)
        {
            healthEvent.CallHealthChangedEvent(currentHealth, 0, MeleeHand.None);
        }

        // Attempt to load enemy / player / decoy components
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
        decoy = GetComponent<Decoy>();

        if (tag == Settings.practiceDummy)
        {
            currentHealth = 999999999;
        }

        // Get player / enemy hit immunity details
        if (player != null)
        {
            if (player.playerDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = player.playerDetails.hitImmunityTime;
                spriteRenderer = player.spriteRenderer;
            }
        }
        else if (enemy != null)
        {
            enemy.currentArmor = enemy.enemyDetails.physicalResistance;

            if (enemy.enemyDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = enemy.enemyDetails.hitImmunityTime;
                spriteRenderer = enemy.spriteRendererArray[0];
            }
        }
        else if (decoy != null)
        {
            isImmuneAfterHit = true;
            immunityTime = 0.4f;
            spriteRenderer = decoy.spriteRenderer;
        }
    }

    private void Update()
    {
        if (player != null)
        {
            if (!player.isInitialized) return;

            if (InputManager.TutorialEnabled)
            {
                int indexValue = (int)TutorialInteraction.Instance.currentTutorialPhase;

                if (indexValue >= (int)TutorialPhase.Parry)
                {
                    isDamageable = true;
                }
                else
                {
                    isDamageable = false;
                }
            }

            // Battle scars mechanism
            if (player.isBattleScarsActive && player.damageTracker.GetRecentDamage() > 100 && !hasDied)
            {
                if (!player.battleScarsArmorBoostActivated)
                {
                    StartCoroutine(BattleScarsRoutine());
                }
            }

            float currentHealth = GetCurrentHealth();
            float maximumHealth = GetMaximumHealth();

            // Second breath mechanism
            if (player.isSecondBreathActive && currentHealth / maximumHealth < 0.2f && !hasDied)
            {
                player.secondBreathReset = false;

                secondBreathHealTimer += Time.deltaTime;

                if (secondBreathHealTimer >= 1f)
                {
                    AddHealth(2);
                    player.healthEvent.CallSecondBreathEvent();
                    secondBreathHealTimer = 0f;
                }
            }
            else if(!player.secondBreathReset)
            {
                player.secondBreathReset = true;
                player.healthEvent.CallSecondBreathWoreOffEvent();
            }

            //// Passive item effect - ChestplateOfTheLastLight Specific
            //if (player.equippedPassiveItems.TryGetValue(PassiveItemSlotName.Chest, out PassiveItem passiveItem) && passiveItem != null)
            //{
            //    if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight && currentHealth < maximumHealth * 0.5f)
            //    {
            //        player.thirtyPercentDamageAbsorbIsActive = true;
            //    }
            //    else
            //    {
            //        player.thirtyPercentDamageAbsorbIsActive = false;
            //    }
            //}
            //else
            //{
            //    player.thirtyPercentDamageAbsorbIsActive = false;
            //}

            // Bleeding effect
            if ((player.healthStatus & HealthStatus.Bleeding) != 0)
            {
                if (bleedingCoroutine == null)
                {
                    bleedingCoroutine = StartCoroutine(GraduallyHealthReduceDuetoBleeding());
                }
            }
            else if (bleedingCoroutine != null)
            {
                StopCoroutine(bleedingCoroutine);
                bleedingCoroutine = null;
            }

            // Poison effect
            if ((player.healthStatus & HealthStatus.Poisoned) != 0)
            {
                if (poisonCoroutine == null)
                {
                    poisonCoroutine = StartCoroutine(GraduallyHealthReduceDuetoPoison());
                }
            }
            else if (poisonCoroutine != null)
            {
                StopCoroutine(poisonCoroutine);
                poisonCoroutine = null;
            }

            // Burn effect
            if ((player.healthStatus & HealthStatus.Burned) != 0)
            {
                if (burnCoroutine == null)
                {
                    burnCoroutine = StartCoroutine(GraduallyHealthReduceDuetoBurn());
                }
            }
            else if (burnCoroutine != null)
            {
                StopCoroutine(burnCoroutine);
                burnCoroutine = null;
            }
        }
        else if (enemy != null)
        {
            // Bleeding effect
            if ((enemy.healthStatus & HealthStatus.Bleeding) != 0)
            {
                if (bleedingCoroutine == null)
                {
                    bleedingCoroutine = StartCoroutine(GraduallyHealthReduceDuetoBleeding());
                }
            }
            else if (bleedingCoroutine != null)
            {
                StopCoroutine(bleedingCoroutine);
                bleedingCoroutine = null;
            }

            // Poison effect
            if ((enemy.healthStatus & HealthStatus.Poisoned) != 0)
            {
                if (poisonCoroutine == null)
                {
                    poisonCoroutine = StartCoroutine(GraduallyHealthReduceDuetoPoison());
                }
            }
            else if (poisonCoroutine != null)
            {
                StopCoroutine(poisonCoroutine);
                poisonCoroutine = null;
            }

            // Burn effect
            if ((enemy.healthStatus & HealthStatus.Burned) != 0)
            {
                if (burnCoroutine == null)
                {
                    burnCoroutine = StartCoroutine(GraduallyHealthReduceDuetoBurn());
                }
            }
            else if (burnCoroutine != null)
            {
                StopCoroutine(burnCoroutine);
                burnCoroutine = null;
            }
        }

        DeathCheck();
    }

    IEnumerator BattleScarsRoutine()
    {
        player.battleScarsArmorBoostActivated = true;
        player.additionalArmorModifier += 0.4f;
        player.UpdateArmorValues();
        player.healthEvent.CallBattleScarsEvent();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        yield return new WaitForSeconds(3f);

        player.battleScarsArmorBoostActivated = false;
        player.additionalArmorModifier -= 0.4f;
        player.UpdateArmorValues();
        player.healthEvent.CallBattleScarsWoreOffEvent();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    private void DeathCheck()
    {
        // Death check
        if (currentHealth <= 0 && !hasDied)
        {
            hasDied = true;
            fxAnimatorPlayed = true;

            if (player != null)
            {
                player.destroyedEvent.CallDestroyedEvent(true); // Player death
            }
            else if (enemy != null)
            {
                if (player != null && player.resourcefulActive) player.mana.AddMana(4); // Add mana on kill

                enemy.dropOnDestroy.DropProcess();
                enemy.destroyedEvent.CallDestroyedEvent(false); // Enemy death
            }
            else if (decoy != null)
            {
                decoy.destroyedEvent.CallDestroyedEvent(false); // Decoy death
            }
        }
    }

    public void TakeDamage(int damageAmount, Vector2 dealerPosition, Vector2 receiverPosition, Collider2D collider = null,
        MeleeHand hand = MeleeHand.None, bool bypassImmunity = false)
    {
        if (decoy != null)
        {
            damageTaken = true;
        }

        // Check if hit by projectile
        bool isProjectile = collider != null && collider.CompareTag(Settings.playerProjectile);
        if (isProjectile)
        {
            isProjectileHit = true;
        }

        if (isDamageable || bypassImmunity)
        {
            if (player != null)
            {
                // Apply to shield first
                if (currentShield > 0)
                {
                    int shieldDamage = Mathf.Min(damageAmount, currentShield);
                    currentShield -= shieldDamage;
                    damageAmount -= shieldDamage;
                }
                else
                {
                    player.isGuardedOathActive = false;
                }

                float playerCurrentHealth = player.health.GetCurrentHealth();
                float playerMaximumHealth = player.health.GetMaximumHealth();

                if (player.isDieHardActive && playerCurrentHealth / playerMaximumHealth < 0.25f) damageAmount = (int)(damageAmount * 0.75f); // %25 damage reduction

                // Remaining damage goes to health
                if (damageAmount > 0)
                {
                    currentHealth = Mathf.Max(currentHealth - damageAmount, 0);
                }
            }
            else
            {
                currentHealth -= damageAmount;
            }


            // Book UI health update
            if (player != null)
            {
                StaticEventHandler.CallBookHealthChangedEvent(currentHealth);

                if (player.playerDetails.playerCharacterIndex == Character.Caelion)
                {
                    player.lastDamageHappenedTime = Time.time;
                }
            }

            if (player != null && currentHealth > 0)
            {
                PostHitImmunity();
            }
            // Decoy logic
            else if (decoy != null)
            {
                DecoyGetHitRoutine();
                PostHitImmunity();
            }
            // Enemy logic
            else if (enemy != null)
            {
                // Update enemy health bar
                if (GameManager.Instance.healthBarContainer.activeSelf)
                {
                    GameManager.Instance.SetHealthBarValue(currentHealth, enemy);
                }

                if (tag == Settings.summonedEnemyTag)
                {
                    if (!isBlocking && !isDodging)
                    {
                        PostHitImmunity();
                    }

                    EnemyGetHitProcess();

                }
                else
                {
                    if (!isBlocking)
                    {
                        PostHitImmunity();
                    }

                    EnemyGetHitProcess();

                }
            }

            // PHOENIX RISING PASSIVE
            if (player != null)
            {
                if (player.playerDetails.playerCharacterIndex == Character.Kynara && !player.phoenixRisingUsed
                    && currentHealth <= 0)
                {
                    player.phoenixRisingUsed = true;
                    currentHealth = 0;
                    AddHealth(player.health.GetMaximumHealth() / 4);
                    SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.passiveSkillSoundEffect);
                    player.playerControl.activeSkillTypeThreeAnimator.SetTrigger("phoenixRising");
                }
            }

            // Trigger health event
            healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, hand);
        }
    }

    private void DecoyGetHitRoutine()
    {
        // Practice Dummy
        SoundEffectManager.Instance.PlaySoundEffect(decoy.dummyHitSound);
    }

    private void EnemyGetHitProcess()
    {
        if (!isBlocking)
        {
            if (enemy.health.GetCurrentHealth() > 0f)
            {
                enemy.animateEnemy.ResetAnimatonParameters();
                enemy.animator.SetBool(Settings.block, false);
                SoundEffectManager.Instance.PlaySoundEffect(GetComponent<Enemy>().enemyDetails.getHitSoundEffect);
            }
            else
            {
                enemy.animateEnemy.ResetAnimatonParameters();
                enemy.animateEnemy.SetDeathAnimationParameters();
            }

            int randomNum = Random.Range(1, 8);

            if (!fxAnimatorPlayed)
            {
                hitFXAnimator.SetInteger(Settings.impactNumber, randomNum);
                fxAnimatorPlayed = true;
            }
        }
        else
        {
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.block, true);
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.deflectSoundEffect);
        }

        fxAnimatorPlayed = false; // Reset hit fx animation
        hitFXAnimator.SetInteger(Settings.impactNumber, 0);
        isBlocking = false;
    }

    /// <summary>
    /// Indicate a hit and give some post hit immunity
    /// </summary>
    public void PostHitImmunity(bool dodgedOrBlocked = false)
    {
        // Check if gameobject is active - if not return
        if (gameObject.activeSelf == false) return;

        if (dodgedOrBlocked)
        {
            if (immunityCoroutine != null)
            {
                StopCoroutine(immunityCoroutine);
            }

            // Flash red&white and give period of immunity
            immunityCoroutine = StartCoroutine(PostHitImmunityRoutine(immunityTime, spriteRenderer, true));
        }
        else
        {
            // If there is post hit immunity then
            if (isImmuneAfterHit || GetComponent<Decoy>() != null)
            {
                if (immunityCoroutine != null)
                {
                    StopCoroutine(immunityCoroutine);
                }

                // Flash red&white and give period of immunity
                immunityCoroutine = StartCoroutine(PostHitImmunityRoutine(immunityTime, spriteRenderer));
            }
        }
    }

    /// <summary>
    /// Coroutine to indicate a hit and give some post hit immunity
    /// </summary>
    IEnumerator PostHitImmunityRoutine(float immunityTime, SpriteRenderer spriteRenderer, bool dodgedOrBlocked = false)
    {
        int iterations = Mathf.RoundToInt(immunityTime / spriteFlashInterval / 4);

        isDamageable = isProjectileHit;

        if (dodgedOrBlocked)
        {
            // Flash effect
            while (iterations > 0)
            {
                flashManager.WhiteFlashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;

                flashManager.UnflashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;

                flashManager.WhiteFlashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;

                flashManager.UnflashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;

                iterations--;

                yield return null;
            }
        }
        else
        {
            iterations = Mathf.RoundToInt(immunityTime / spriteFlashInterval / 4);

            // Flash effect
            while (iterations > 0)
            {
                flashManager.RedFlashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;

                flashManager.UnflashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;

                if (player != null)
                {
                    bool flashed = false;

                    if ((player.healthStatus & HealthStatus.Poisoned) != 0)
                    {
                        flashManager.PoisonFlashCharacter(spriteRenderer);
                        flashed = true;
                    }
                    if ((player.healthStatus & HealthStatus.Burned) != 0)
                    {
                        flashManager.BurnFlashCharacter(spriteRenderer);
                        flashed = true;
                    }

                    if (flashed)
                    {
                        yield return waitForSecondsSpriteFlashInterval;
                        flashManager.UnflashCharacter(spriteRenderer);
                        yield return waitForSecondsSpriteFlashInterval;
                    }
                }
                else
                {
                    flashManager.WhiteFlashCharacter(spriteRenderer);
                    yield return waitForSecondsSpriteFlashInterval;

                    flashManager.UnflashCharacter(spriteRenderer);
                    yield return waitForSecondsSpriteFlashInterval;
                }

                iterations--;

                yield return null;
            }
        }

        // If not hit by a projectile, re-enable damageability
        isDamageable = true;

        isProjectileHit = false; // Reset the projectile hit flag
        immunityCoroutine = null;
    }

    /// <summary>
    /// Gradually reduce health - Bleeding
    /// </summary>
    IEnumerator GraduallyHealthReduceDuetoBleeding()
    {
        bleedingPeriodCount++;

        int damageAmount = 0;

        if (player != null)
        {
            damageAmount = (int)(7 - 7 * player.currentArmorValue);
        }
        else if (enemy != null)
        {
            damageAmount = (int)(7 - 7 * enemy.enemyDetails.physicalResistance);
        }
        else
        {
            damageAmount = 7;
        }

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        TakeDamage(damageAmount, Vector2.zero, transform.position, null, MeleeHand.None);

        if (bleedingPeriodCount > 3)
        {
            if (player != null)
            {
                player.healthStatus &= ~HealthStatus.Bleeding; // Remove only bleeding status
                player.healthEvent.CallBleedingCuredEvent();
            }
            if (enemy != null)
            {
                enemy.healthStatus &= ~HealthStatus.Bleeding;
                enemy.healthEvent.CallBleedingCuredEvent();
            }

            bleedingPeriodCount = 0;
        }

        yield return new WaitForSeconds(2f);

        bleedingCoroutine = null; // Reset the coroutine reference when it's finished
    }

    /// <summary>
    /// Gradually reduce health - Burn
    /// </summary>
    IEnumerator GraduallyHealthReduceDuetoBurn()
    {
        burnPeriodCount++;

        int damageAmount = 0;

        if (player != null)
        {
            damageAmount = (int)(7 - 7 * player.currentMagicResistanceValue);
        }
        else if (enemy != null)
        {
            damageAmount = (int)(7 - 7 * enemy.enemyDetails.fireResistance);
        }
        else
        {
            damageAmount = 7;
        }

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        TakeDamage(damageAmount, Vector2.zero, transform.position, null, MeleeHand.None);

        if (burnPeriodCount > 3)
        {
            if (player != null)
            {
                player.healthStatus &= ~HealthStatus.Burned; // Remove only burn status
                player.healthEvent.CallBurnCuredEvent();
            }
            if (enemy != null)
            {
                enemy.healthStatus &= ~HealthStatus.Burned;
                enemy.healthEvent.CallBurnCuredEvent();
            }

            burnPeriodCount = 0;
        }

        yield return new WaitForSeconds(2f);

        burnCoroutine = null; // Reset the coroutine reference when it's finished
    }

    /// <summary>
    /// Gradually reduce health - Poison
    /// </summary>
    IEnumerator GraduallyHealthReduceDuetoPoison()
    {
        poisonPeriodCount++;

        int damageAmount = 0;

        if (player != null)
        {
            damageAmount = (int)(7 - 7 * player.currentMagicResistanceValue);
        }
        else if (enemy != null)
        {
            damageAmount = (int)(7 - 7 * enemy.enemyDetails.earthResistance);
        }
        else
        {
            damageAmount = 7;
        }

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        TakeDamage(damageAmount, Vector2.zero, transform.position, null, MeleeHand.None);

        if (poisonPeriodCount > 3)
        {
            if (player != null)
            {
                player.healthStatus &= ~HealthStatus.Poisoned; // Remove only poison status
                player.healthEvent.CallPoisonCuredEvent();
            }
            if (enemy != null)
            {
                enemy.healthStatus &= ~HealthStatus.Poisoned;
                enemy.healthEvent.CallPoisonCuredEvent();
            }

            poisonPeriodCount = 0;
        }

        yield return new WaitForSeconds(2f);

        poisonCoroutine = null; // Reset the coroutine reference when it's finished
    }

    /// <summary>
    /// Set starting health - Enemy
    /// </summary>
    public void SetMaximumHealth(int maximumHealth)
    {
        this.maximumHealth = maximumHealth;

        // If current health maximized together with increasing max health or not
        currentHealth = maximumHealth;

        // Trigger health event
        if(player != null) healthEvent.CallHealthChangedEvent(currentHealth, 0, MeleeHand.None);
    }


    /// <summary>
    /// Set starting health - Player
    /// </summary>
    public void SetMaximumHealth(int maximumHealth, bool shouldHealthFilled = true, bool onStart = false)
    {
        this.maximumHealth = maximumHealth;

        // If current health maximized together with increasing max health or not
        currentHealth = onStart ? maximumHealth : shouldHealthFilled ? maximumHealth : currentHealth;
    }

    /// <summary>
    /// Get the starting health
    /// </summary>
    public int GetMaximumHealth()
    {
        return maximumHealth;
    }

    /// <summary>
    /// Get current health
    /// </summary>
    public int GetCurrentHealth()
    {
        return currentHealth;
    }

    /// <summary>
    /// Get current shield
    /// </summary>
    public int GetCurrentShield()
    {
        return currentShield;
    }

    /// <summary>
    /// Increase health by specified percent
    /// </summary>
    public void AddHealth(int healthIncrease)
    {
        currentHealth = Mathf.Clamp(currentHealth + healthIncrease, 0, maximumHealth);
            
        if (enemy != null)
        {
            // Set health bar as the percentage of health remaining
            if (GameManager.Instance.healthBarContainer.activeSelf)
            {
                GameManager.Instance.SetHealthBarValue(currentHealth, enemy);
            }
        }

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, 0, MeleeHand.None);
        StaticEventHandler.CallBookHealthChangedEvent(currentHealth); // BOOK UI
    }


    /// <summary>
    /// Increase shield by specified percent
    /// </summary>
    public void AddShield(int shieldIncrease)
    {
        currentShield += shieldIncrease;

        if (enemy != null)
        {
            // Set health bar as the percentage of health remaining
            if (GameManager.Instance.healthBarContainer.activeSelf)
            {
                GameManager.Instance.SetHealthBarValue(currentHealth, enemy);
            }
        }

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, 0, MeleeHand.None);
        StaticEventHandler.CallBookHealthChangedEvent(currentHealth);
    }

    /// <summary>
    /// Remove shield
    /// </summary>
    public void RemoveShield()
    {
        currentShield = 0;

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, 0, MeleeHand.None);
        StaticEventHandler.CallBookHealthChangedEvent(currentHealth);
    }

    /// <summary>
    /// Reset armor value
    /// </summary>
    public void ResetArmorValue()
    {
        if (player != null)
        {
            player.acidArmorDebuffModifier = 0f;
            player.UpdateArmorValues();
        }
    }
}
