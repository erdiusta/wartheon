using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(HealthEvent))]
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    public Animator hitFXAnimator;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public int maximumHealth;
    [HideInInspector] public bool isDamageable = true;
    [HideInInspector] public Enemy enemy;
    [HideInInspector] public Coroutine getHitCoroutine;
    [HideInInspector] public bool isBlocking;
    [HideInInspector] public bool isDodging;
    [HideInInspector] public bool suddenDeathHappened;
    [HideInInspector] public FlashManager flashManager;
    [HideInInspector] public const float spriteFlashInterval = 0.1f;
    [HideInInspector] public bool fxAnimatorPlayed;

    HealthEvent healthEvent;
    Player player;
    Coroutine immunityCoroutine;
    bool isImmuneAfterHit;
    float immunityTime = 0f;
    SpriteRenderer spriteRenderer;
    WaitForSeconds waitForSecondsSpriteFlashInterval = new WaitForSeconds(spriteFlashInterval);
    Coroutine burnCoroutine;
    Coroutine poisonCoroutine;
    Coroutine bleedingCoroutine;
    int poisonPeriodCount = 0;
    int burnPeriodCount = 0;
    bool isProjectileHit = false;
    Decoy decoy;


    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        flashManager = GetComponent<FlashManager>();
    }

    private void Start()
    {
        // Trigger a health event for UI update
        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, 0, MeleeHand.None);

        // Attempt to load enemy / player / decoy components
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
        decoy = GetComponent<Decoy>();

        if (tag == "PracticeDummy")
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
            enemy.currentPhysicalResistance = enemy.enemyDetails.physicalResistance;

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
        // If player dies, clones should be destroyed immediately
        if (player != null)
        {
            if (player.isDead)
            {
                Destroy(gameObject);
            }
        }


        if (player != null)
        {
            // Passive item effect
            PassiveItem chestItem = player.selectedPassiveItem?.GetCurrentChestPassiveItem();

            if (chestItem != null && player.selectedPassiveItem.GetCurrentChestPassiveItem().passiveItemDetails.passiveItemType ==
                PassiveItemType.ChestplateOfTheLastLight && currentHealth < maximumHealth * 0.5f)
            {
                player.thirtyPercentDamageAbsorbIsActive = true;
            }
            else
            {
                player.thirtyPercentDamageAbsorbIsActive = false;
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
        else if (decoy != null)
        {
            if (currentHealth <= 0f)
            {
                if (decoy.tag == "Dummy")
                {
                    SoundEffectManager.Instance.PlaySoundEffect(decoy.activeItemDetails.activeItemImpactSoundEffect);
                }

                Destroy(gameObject);
            }
        }
    }

    /// <summary>
    /// Public method called when damage is taken - Projectile
    /// </summary>
    public void TakeDamage(int damageAmount, Vector2 dealerPosition, Vector2 receiverPosition, Collider2D collider, bool headShotHappened)
    {
        bool isRolling = false;

        if (player != null)
        {
            isRolling = player.playerControl.isPlayerRolling;
        }

        // Check if the collider is a projectile
        bool isProjectile = collider.CompareTag("playerProjectile");

        if (isProjectile)
        {
            // If hit by a projectile, set the projectile hit flag
            isProjectileHit = true;
        }

        if (isDamageable && !isRolling)
        {
            currentHealth -= damageAmount;
            if (player != null && !player.isClone)
            {
                StaticEventHandler.CallBookHealthChangedEvent(currentHealth);
            }

            if (player != null)
            {
                if (getHitCoroutine == null)
                {
                    if (currentHealth > 0)
                    {
                        //getHitCoroutine = StartCoroutine(PlayerGetHitRoutine());
                        PostHitImmunity();
                    }
                    else
                    {
                        if (player.isClone)
                        {
                            Player.hasClone = false;
                            Destroy(player.gameObject);
                        }
                    }
                }
            }
            else if (decoy != null)
            {
                if (getHitCoroutine == null)
                {
                    getHitCoroutine = StartCoroutine(DecoyGetHitRoutine());
                    PostHitImmunity();
                }
            }
            else if (enemy != null)
            {
                // Set health bar as the percentage of health remaining
                if (GameManager.Instance.healthBarContainer.activeSelf)
                {
                    GameManager.Instance.SetHealthBarValue(currentHealth, enemy);
                }

                if (getHitCoroutine == null)
                {
                    if (!isBlocking || !isDodging)
                    {
                        PostHitImmunity();
                    }

                    getHitCoroutine = StartCoroutine(EnemyGetHitRoutine(headShotHappened));
                }

                if (currentHealth <= 0)
                {
                    fxAnimatorPlayed = true; // Reset hit fx animation

                    enemy.dropOnDestroy.DropProcess();
                }
            }

            // Trigger health event
            healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        }
    }

    /// <summary>
    /// Public method called when damage is taken - Melee & Contact
    /// </summary>
    public void TakeDamage(int damageAmount, Vector2 dealerPosition, Vector2 receiverPosition, bool headShotHappened, MeleeHand hand = MeleeHand.None,
        bool bypassImmunity = false)
    {
        bool isRolling = false;

        if (player != null)
        {
            isRolling = player.playerControl.isPlayerRolling;
        }

        if ((isDamageable || bypassImmunity) && !isRolling)
        {
            currentHealth -= damageAmount;

            if (player != null && !player.isClone)
            {
                StaticEventHandler.CallBookHealthChangedEvent(currentHealth);

                if (getHitCoroutine == null)
                {
                    if (currentHealth > 0)
                    {
                        PostHitImmunity();
                    }
                }
            }
            else if (decoy != null)
            {
                if (currentHealth > 0)
                {
                    PostHitImmunity();
                }
            }
            else if (enemy != null)
            {
                if (tag == Settings.summonedEnemyTag)
                {
                    if (getHitCoroutine == null)
                    {
                        if (!isBlocking && !isDodging)
                        {
                            PostHitImmunity();
                        }

                        getHitCoroutine = StartCoroutine(EnemyGetHitRoutine(headShotHappened));
                    }
                }
                else
                {
                    // Set health bar as the percentage of health remaining
                    if (GameManager.Instance.healthBarContainer.activeSelf)
                    {
                        GameManager.Instance.SetHealthBarValue(currentHealth, enemy);
                    }

                    if (getHitCoroutine != null)
                    {
                        StopCoroutine(getHitCoroutine);
                    }

                    if (!isBlocking)
                    {
                        PostHitImmunity();
                    }

                    getHitCoroutine = StartCoroutine(EnemyGetHitRoutine(headShotHappened));


                    if (currentHealth <= 0)
                    {
                        fxAnimatorPlayed = true; // Reset hit fx animation
                        enemy.dropOnDestroy.DropProcess();
                    }
                }
            }

            // Trigger health event
            healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, hand);
        }
    }

    IEnumerator DecoyGetHitRoutine()
    {
        if (decoy.tag == "Dummy")
        {
            SoundEffectManager.Instance.PlaySoundEffect(decoy.activeItemDetails.activeItemSwingSoundEffect);
        }

        yield return new WaitForSeconds(0.1f);

        getHitCoroutine = null;
    }

    IEnumerator EnemyGetHitRoutine(bool headShotHappened)
    {
        if (!isBlocking)
        {
            //enemy.enemyAI.enemyPhase = EnemyPhase.GetHit;

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

            if (headShotHappened)
            {
                enemy.healthEvent.CallHeadShotEvent();
                SoundEffectManager.Instance.PlaySoundEffect(GameManager.Instance.GetPlayer().playerDetails.specialMoveOneSoundEffect);
            }

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

        yield return new WaitForSeconds(0.4f);

        fxAnimatorPlayed = false; // Reset hit fx animation
        hitFXAnimator.SetInteger(Settings.impactNumber, 0);
        enemy.animator.SetBool(Settings.block, false);
        isBlocking = false;
        getHitCoroutine = null;
        //enemy.enemyAI.enemyPhase = EnemyPhase.Patrol;
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
    /// Gradually reduce health - Burn
    /// </summary>
    IEnumerator GraduallyHealthReduceDuetoBurn()
    {
        burnPeriodCount++;

        int damageAmount = 7;
        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        TakeDamage(damageAmount, Vector2.zero, transform.position, false, MeleeHand.None);

        float rndNumber = Random.Range(0f, 1f);

        if (rndNumber > 0.5f && burnPeriodCount > 2)
        {
            if (player != null)
            {
                player.healthStatus = HealthStatus.Normal;
                player.healthEvent.CallBurnCuredEvent();
            }
            if (enemy != null)
            {
                enemy.healthStatus = HealthStatus.Normal;
                enemy.healthEvent.CallBurnCuredEvent();
            }

            burnPeriodCount = 0;
        }

        yield return new WaitForSeconds(2.5f);

        burnCoroutine = null; // Reset the coroutine reference when it's finished
    }

    /// <summary>
    /// Gradually reduce health - Poison
    /// </summary>
    IEnumerator GraduallyHealthReduceDuetoPoison()
    {
        poisonPeriodCount++;

        int damageAmount = 7;
        // Trigger health event
        healthEvent.CallHealthChangedEvent(currentHealth, damageAmount, MeleeHand.None);
        TakeDamage(damageAmount, Vector2.zero, transform.position, false, MeleeHand.None);

        float rndNumber = Random.Range(0f, 1f);

        if (rndNumber > 0.5f && poisonPeriodCount > 2)
        {
            if (player != null)
            {
                player.healthStatus = HealthStatus.Normal;
                player.healthEvent.CallPoisonCuredEvent();
            }
            if (enemy != null)
            {
                enemy.healthStatus = HealthStatus.Normal;
                enemy.healthEvent.CallPoisonCuredEvent();
            }

            poisonPeriodCount = 0;
        }

        yield return new WaitForSeconds(2.5f);

        poisonCoroutine = null; // Reset the coroutine reference when it's finished
    }


    /// <summary>
    /// Set starting health 
    /// </summary>
    public void SetMaximumHealth(int maximumHealth, bool shouldHealthFilled = true)
    {
        this.maximumHealth = maximumHealth;

        // If current health maximized together with increasing max health or not
        currentHealth = shouldHealthFilled ? maximumHealth : currentHealth;
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
    /// Increase health by specified percent
    /// </summary>
    public void AddHealth(int healthIncrease)
    {
        int totalHealth = currentHealth + healthIncrease;

        if (totalHealth > maximumHealth)
        {
            currentHealth = maximumHealth;
        }
        else
        {
            currentHealth = totalHealth;
        }

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


    public void ResetStatusInCaseOfDeath()
    {
        immunityCoroutine = null;
        getHitCoroutine = null;
        flashManager.UnflashCharacter(spriteRenderer);
    }

    /// <summary>
    /// Reset armor value
    /// </summary>
    public void ResetArmorValue()
    {
        if (player != null)
        {
            player.currentPhysicalResistanceValue = player.playerDetails.physicalResistance;
        }
    }
}
