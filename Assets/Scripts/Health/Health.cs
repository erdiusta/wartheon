using Mirror;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

[RequireComponent(typeof(HealthEvent))]
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
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
    [HideInInspector] public uint LastDamageDealerNetId;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public bool isImmuneAfterHit;
    [HideInInspector] public float immunityTime = 0f;

    HealthEvent healthEvent;
    Player player;
    Coroutine immunityCoroutine;

    WaitForSeconds waitForSecondsSpriteFlashInterval = new WaitForSeconds(spriteFlashInterval);

    Coroutine bleedingCoroutine;
    Coroutine poisonCoroutine;
    Coroutine burnCoroutine;
    int bleedingPeriodCount = 0;
    int poisonPeriodCount = 0;
    int burnPeriodCount = 0;

    bool isProjectileHit = false;
    Dummy dummy;
    bool playerReady;
    bool isPhoenixRisingEffectsReady = false;

    // Inner Path
    float secondBreathHealTimer = 0f;

    private void Awake()
    {
#if UNITY_EDITOR
        var authorities = GetComponents<IHealthAuthority>();
        Debug.Assert(authorities.Length == 1, $"Expected exactly one IHealthAuthority on {name}");
#endif
        healthEvent = GetComponent<HealthEvent>();
        flashManager = GetComponent<FlashManager>();
        enemy = GetComponent<Enemy>();
        dummy = GetComponent<Dummy>();
    }

    private void Start()
    {
        if (SceneManager.GetActiveScene().buildIndex != 2) return;

        if (tag == Settings.practiceDummy)
        {
            currentHealth = 999999999;
            isImmuneAfterHit = true;
            immunityTime = 0.4f;
            spriteRenderer = GetComponent<SpriteRenderer>();
            return;
        }

        if (enemy != null)
        {
            isImmuneAfterHit = true;
            immunityTime = 0.4f;
            spriteRenderer = enemy.spriteRendererArray[0];
        }

        if (dummy != null)
        {
            isImmuneAfterHit = true;
            immunityTime = 0.4f;
            spriteRenderer = dummy.spriteRenderer;
        }
    }

    public void OnPlayerInitialized(Player player) // Called by player on initialization
    {
        if (player == null) return;

        this.player = player;
        healthEvent = GetComponent<HealthEvent>();
        flashManager = GetComponent<FlashManager>();

        playerReady = true;

        if (player.playerDetails.isImmuneAfterHit)
        {
            isImmuneAfterHit = true;
            immunityTime = player.playerDetails.hitImmunityTime;
            spriteRenderer = player.spriteRenderer;
        }

        // Initial sync event (SAFE now)
        healthEvent.CallHealthChangedEvent(currentHealth, 0, MeleeHand.None);
    }

    public void OnEnemyInitialized(Enemy enemy)// Called by enemy on initialization - SP
    {
        enemy.currentArmor = enemy.enemyDetails.physicalResistance;
    }      

    private void Update()
    {
        if (SceneManager.GetActiveScene().buildIndex != 2) return;

        if (playerReady && player != null)
        {
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

        if (!NetworkServer.active && !NetworkClient.active) // SP
        {
            DeathCheck();
        }
        else
        {
            if (NetworkClient.active && (playerReady || enemy != null))
            {
                DeathCheck();
            }
        }
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
                // Player death
                DestroyUtility.Destroy(player.gameObject, playerDied: true, 0);
            }
            else if (enemy != null && enemy.initializationCompleted)
            {
                if (player != null && player.resourcefulActive) player.mana.AddMana(4); // Add mana on kill

                // Enemy death
                enemy.dropOnDestroy.DropProcess();

                DestroyUtility.Destroy(enemy.gameObject, playerDied: false, enemy.health.LastDamageDealerNetId);
            }
            else if (dummy != null)
            {
                // Decoy death
                DestroyUtility.Destroy(dummy.gameObject, playerDied: false, dummy.health.LastDamageDealerNetId);
            }
        }
    }

    public void ApplyDamageInternal(int damageAmount, DamageContext ctx)
    {
        if (dummy != null)
        {
            damageTaken = true;
        }

        // Check if hit by projectile
        bool isProjectile = ctx.source == DamageSourceType.Projectile && ctx.owner == DamageOwner.Player;

        if (isProjectile)
        {
            isProjectileHit = true;
        }

        if (isDamageable || ctx.bypassImmunity)
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

                // Book UI health update && SP
                if (!NetworkServer.active && !NetworkClient.active)
                {
                    if (player != null && player.IsLocal)
                    {
                        StaticEventHandler.CallBookHealthChangedEvent(currentHealth);
                    }
                }
            }
            else
            {
                if (ctx.dealerNetId != 0)
                {
                    LastDamageDealerNetId = ctx.dealerNetId;
                }

                currentHealth -= damageAmount;
            }

            // Book UI health update
            if (player != null)
            {
                if (player.playerDetails.playerCharacterIndex == Character.Caelion)
                {
                    player.lastDamageHappenedTime = Time.time;
                }
            }

            // PHOENIX RISING PASSIVE
            if (player != null)
            {
                if (player.playerDetails.playerCharacterIndex == Character.Kynara && !player.phoenixRisingUsed
                    && currentHealth <= 0)
                {
                    player.phoenixRisingUsed = true;
                    isPhoenixRisingEffectsReady = true;

                    currentHealth = 0;
                    AddHealth(player.health.GetMaximumHealth() / 4);
                }
            }
        }

        // Call this event only in SP sessions, in MP it will called in Server
        if (!NetworkServer.active && !NetworkClient.active)
        {
            // Trigger health event
            healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, ctx.hand);
        }
    }

    public void ApplyReplicatedHealth(int newHealth, int damageAmount, DamageContext ctx)
    {
        currentHealth = newHealth;

        SyncHealthVisuals(damageAmount, ctx);

        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, ctx.hand);

        if (player != null && player.IsLocal)
        {
            StaticEventHandler.CallBookHealthChangedEvent(currentHealth);
        }
    }

    public void SyncHealthVisuals(int damageAmount, DamageContext ctx)
    {
        if (isDamageable || ctx.bypassImmunity)
        {
            if (player != null && currentHealth > 0)
            {
                PostHitImmunity();
            }
            // Decoy logic
            else if (dummy != null)
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
                    if (!isBlocking && !isDodging) PostHitImmunity();
                    EnemyGetHitProcess();
                }
                else
                {
                    if (!isBlocking) PostHitImmunity();
                    EnemyGetHitProcess();
                }
            }

            if (player != null && isPhoenixRisingEffectsReady)
            {
                isPhoenixRisingEffectsReady = false;
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.passiveSkillSoundEffect);
                player.playerSkillController.activeSkillTypeThreeAnimator.SetTrigger("phoenixRising");
            }
        }
    }

    private void DecoyGetHitRoutine()
    {
        //// Practice Dummy
        //SoundEffectManager.Instance.PlaySoundEffect(dummy.dummyHitSound);
    }

    private void EnemyGetHitProcess()
    {
        if (!isBlocking)
        {
            if (enemy.health.GetCurrentHealth() > 0f)
            {
                enemy.animateEnemy.ResetAnimatonParameters();
                enemy.animator.SetBool(Settings.block, false);

                if (!NetworkServer.active && !NetworkClient.active)
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GetComponent<Enemy>().enemyDetails.getHitSoundEffect);
                }
            }
            else
            {
                enemy.animateEnemy.ResetAnimatonParameters();
                enemy.animateEnemy.SetDeathAnimationParameters();
            }

            int randomNum = Random.Range(1, 8);

            if (!fxAnimatorPlayed)
            {
                fxAnimatorPlayed = true;
            }
        }
        else
        {
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animator.SetBool(Settings.block, true);

            if (!NetworkServer.active && !NetworkClient.active)
            {
                SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.deflectSoundEffect);
            }
        }

        fxAnimatorPlayed = false; // Reset hit fx animation
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
            if (isImmuneAfterHit || GetComponent<Dummy>() != null)
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

        DamageContext ctx = new DamageContext { source = DamageSourceType.DoT };

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        ApplyDamageInternal(damageAmount, ctx);

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
            damageAmount = (int)(7 - 7 * enemy.enemyDetails.magicResistance);
        }
        else
        {
            damageAmount = 7;
        }

        DamageContext ctx = new DamageContext { source = DamageSourceType.DoT };

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        ApplyDamageInternal(damageAmount, ctx);

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
            damageAmount = (int)(7 - 7 * enemy.enemyDetails.magicResistance);
        }
        else
        {
            damageAmount = 7;
        }

        DamageContext ctx = new DamageContext { source = DamageSourceType.DoT };

        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        ApplyDamageInternal(damageAmount, ctx);

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
            player.UpdateArmorValues();
        }
    }
}
