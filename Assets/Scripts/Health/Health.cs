using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

[RequireComponent(typeof(HealthEvent))]
[DisallowMultipleComponent]
public class Health : MonoBehaviour
{
    #region Header References
    [Space(10)]
    [Header("References")]
    #endregion
    #region Tooltip
    [Tooltip("Populate with the HealthBar component on the HealthBar gameobject")]
    #endregion
    [SerializeField] HealthBar healthBar;

    [HideInInspector] public int currentHealth;
    [HideInInspector] public bool isDamageable = true;
    [HideInInspector] public Enemy enemy;
    [HideInInspector] public int currentArmorValue;
    [HideInInspector] public Coroutine getHitCoroutine;
    [HideInInspector] public bool isBlocking;

    int startingHealth;
    HealthEvent healthEvent;
    Player player;
    Coroutine immunityCoroutine;
    bool isImmuneAfterHit;
    float immunityTime = 0f;
    SpriteRenderer spriteRenderer;
    const float spriteFlashInterval = 0.1f;
    WaitForSeconds waitForSecondsSpriteFlashInterval = new WaitForSeconds(spriteFlashInterval);
    FlashManager flashManager;
    Coroutine poisonCoroutine;
    Coroutine bleedingCoroutine;
    int poisonPeriodCount = 0;

    private void Awake()
    {
        healthEvent = GetComponent<HealthEvent>();
        flashManager = GetComponent<FlashManager>();
    }

    private void Start()
    {
        // Trigger a health event for UI update
        CallHealthEvent(0);

        // Attempt to load enemy / player components
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
       
        // Get player / enemy hit immunity details
        if (player != null)
        {
            currentArmorValue = player.playerDetails.playerArmorValue;

            if (player.playerDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = player.playerDetails.hitImmunityTime;
                spriteRenderer = player.spriteRenderer;
            }
        }
        else if (enemy != null)
        {
            currentArmorValue = enemy.enemyDetails.enemyArmorValue;

            if (enemy.enemyDetails.isImmuneAfterHit)
            {
                isImmuneAfterHit = true;
                immunityTime = enemy.enemyDetails.hitImmunityTime;
                spriteRenderer = enemy.spriteRendererArray[0];
            }
        }

        // Enable the health bar if required
        if (enemy != null && enemy.enemyDetails.isHealthBarDisplayed == true && healthBar != null)
        {
            healthBar.EnableHealthBar();
        }
        else if (healthBar != null)
        {
            healthBar.DisableHealthBar();
        }
    }

    private void Update()
    {
        if (player != null)
        {
            if (player.healthStatus == HealthStatus.Poisoned)
            {
                if (poisonCoroutine == null)
                {
                    poisonCoroutine = StartCoroutine(GraduallyHealthReduceDuetoPoison());
                }
            }

            if (player.healthStatus == HealthStatus.Bleeding)
            {
                if (bleedingCoroutine == null)
                {
                    bleedingCoroutine = StartCoroutine(GraduallyHealthReduceDuetoBleeding());
                }
            }
        }
        else if (enemy != null)
        {
            if (enemy.healthStatus == HealthStatus.Poisoned)
            {
                if (poisonCoroutine == null)
                {
                    poisonCoroutine = StartCoroutine(GraduallyHealthReduceDuetoPoison());
                }
            }

            if (enemy.healthStatus == HealthStatus.Bleeding)
            {
                if (bleedingCoroutine == null)
                {
                    bleedingCoroutine = StartCoroutine(GraduallyHealthReduceDuetoBleeding());
                }
            }
        }
    }

    /// <summary>
    /// Public method called when damage is taken
    /// </summary>
    public void TakeDamage(int damageAmount, Vector2 dealerPosition, Vector2 receiverPosition)
    {
        if (isDamageable)
        {
            currentHealth -= damageAmount;
            CallHealthEvent(damageAmount);

            if (player != null)
            {
                if (getHitCoroutine == null)
                {
                    if (currentHealth > 0)
                    {
                        getHitCoroutine = StartCoroutine(PlayerGetHitRoutine());
                        PostHitImmunity();
                    }
                }
            }
            if (enemy != null)
            {
                if (getHitCoroutine == null)
                {
                    if (!isBlocking)
                    {
                        PostHitImmunity();
                    }

                    getHitCoroutine = StartCoroutine(EnemyGetHitRoutine());
                }

                if (currentHealth <= 0)
                {
                    enemy.dropOnDestroy.DropProcess();
                }
            }

            // Set health bar as the percentage of health remaining
            if (healthBar != null)
            {
                healthBar.SetHealthBarValue((float)currentHealth / (float)startingHealth);
            }
        }
    }

    IEnumerator PlayerGetHitRoutine()
    {
        if (player.health.currentHealth > 0f)
        {
            player.animatePlayer.SetGetHitAnimationParameters();
            player.animator.SetBool(Settings.getHit, true);
            SoundEffectManager.Instance.PlaySoundEffect(GetComponent<Player>().playerDetails.getHitSoundEffect);
        }
        else
        {
            player.animatePlayer.SetDeathAnimationParameters();
        }

        yield return new WaitForSeconds(0.4f);

        player.animator.SetBool(Settings.getHit, false);
        getHitCoroutine = null;
    }

    IEnumerator EnemyGetHitRoutine()
    {
        if (!isBlocking)
        {
            if (enemy.health.currentHealth > 0f)
            {
                enemy.animateEnemy.ResetAnimatonParameters();
                enemy.animateEnemy.SetGetHitAnimationParameters();
                enemy.animator.SetBool(Settings.getHit, true);
                enemy.animator.SetBool(Settings.block, false);
                SoundEffectManager.Instance.PlaySoundEffect(GetComponent<Enemy>().enemyDetails.getHitSoundEffect);
            }
            else
            {
                enemy.animateEnemy.ResetAnimatonParameters();
                enemy.animateEnemy.SetDeathAnimationParameters();
            }

            enemy.particlesSystem.Play();
        }
        else
        {
            enemy.animateEnemy.ResetAnimatonParameters();
            enemy.animateEnemy.SetGetHitAnimationParameters();
            enemy.animator.SetBool(Settings.getHit, false);
            enemy.animator.SetBool(Settings.block, true);
            SoundEffectManager.Instance.PlaySoundEffect(enemy.enemyDetails.deflectSoundEffect);
        }

        yield return new WaitForSeconds(0.6f);

        enemy.particlesSystem.Stop();
        enemy.animator.SetBool(Settings.getHit, false);
        enemy.animator.SetBool(Settings.block, false);
        isBlocking = false;
        getHitCoroutine = null;
    }

    /// <summary>
    /// Indicate a hit and give some post hit immunity
    /// </summary>
    private void PostHitImmunity()
    {
        // Check if gameobject is active - if not return
        if (gameObject.activeSelf == false)
            return;

        // If there is post hit immunity then
        if (isImmuneAfterHit)
        {
            if (immunityCoroutine != null)
            {
                StopCoroutine(immunityCoroutine);
            }

            // Flash red&white and give period of immunity
            immunityCoroutine = StartCoroutine(PostHitImmunityRoutine(immunityTime, spriteRenderer));
        }
    }

    /// <summary>
    /// Coroutine to indicate a hit and give some post hit immunity
    /// </summary>
    IEnumerator PostHitImmunityRoutine(float immunityTime, SpriteRenderer spriteRenderer)
    {
        int iterations = Mathf.RoundToInt(immunityTime / spriteFlashInterval / 4);
        isDamageable = false;

        // Flash effect
        while (iterations > 0)
        {
            flashManager.RedFlashCharacter(spriteRenderer);
            yield return waitForSecondsSpriteFlashInterval;

            flashManager.UnflashCharacter(spriteRenderer);
            yield return waitForSecondsSpriteFlashInterval;

            if (player != null && player.healthStatus == HealthStatus.Poisoned)
            {
                flashManager.PoisonFlashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;

                flashManager.UnflashCharacter(spriteRenderer);
                yield return waitForSecondsSpriteFlashInterval;
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

        isDamageable = true;
        immunityCoroutine = null;
    }

    /// <summary>
    /// Gradually reduce health - Poison
    /// </summary>
    IEnumerator GraduallyHealthReduceDuetoPoison()
    {
        poisonPeriodCount++;

        int damageAmount = 7;
        // Trigger health event
        healthEvent.CallHealthChangedEvent(((float)currentHealth / (float)startingHealth), currentHealth, damageAmount);
        TakeDamage(damageAmount, Vector2.zero, transform.position);

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
    /// Gradually reduce health - Bleeding
    /// </summary>
    IEnumerator GraduallyHealthReduceDuetoBleeding()
    {
        int damageAmount = 1;
        // Trigger health event
        healthEvent.CallHealthChangedEvent(((float)currentHealth / (float)startingHealth), currentHealth, damageAmount);
        TakeDamage(damageAmount, Vector2.zero, transform.position);

        float randomDice = Random.Range(0f, 1f);

        if (randomDice > 0.9f)
        {
            if (player != null)
            {
                player.healthStatus = HealthStatus.Normal;
                player.healthEvent.CallBleedingCuredEvent();
                damageAmount = 0;
            }
            if (enemy != null)
            {
                enemy.healthStatus = HealthStatus.Normal;
                enemy.healthEvent.CallBleedingCuredEvent();
                damageAmount = 0;
            }
        }

        yield return new WaitForSeconds(2.5f);

        damageAmount++;
        bleedingCoroutine = null; // Reset the coroutine reference when it's finished
    }

    private void CallHealthEvent(int damageAmount)
    {
        // Trigger health event
        healthEvent.CallHealthChangedEvent(((float)currentHealth / (float)startingHealth), currentHealth, damageAmount);
    }

    /// <summary>
    /// Set starting health 
    /// </summary>
    public void SetStartingHealth(int startingHealth)
    {
        this.startingHealth = startingHealth;
        currentHealth = startingHealth;
    }

    /// <summary>
    /// Get the starting health
    /// </summary>
    public int GetStartingHealth()
    {
        return startingHealth;
    }

    /// <summary>
    /// Increase health by specified percent
    /// </summary>
    public void AddHealth(int healthPercent)
    {
        int healthIncrease = Mathf.RoundToInt((startingHealth * healthPercent) / 100f);

        int totalHealth = currentHealth + healthIncrease;

        if (totalHealth > startingHealth)
        {
            currentHealth = startingHealth;
        }
        else
        {
            currentHealth = totalHealth;
        }

        CallHealthEvent(0);
    }


    /// <summary>
    /// Set current armor value - Silver or Golden Armors
    /// </summary>
    public void SetArmorValue()
    {
        if (player != null)
        {
            if (player.armorStatus == ArmorStatus.SilverArmor)
            {
                currentArmorValue = 5 + (int)(player.playerDetails.playerArmorValue * 1.5f);
            }
            else if (player.armorStatus == ArmorStatus.GoldenArmor)
            {
                currentArmorValue = 10 + player.playerDetails.playerArmorValue * 2;
            }
        }
    }

    /// <summary>
    /// Set current armor value - Acid
    /// </summary>
    public void SetArmorValue(int armorValue)
    {
        if (player != null)
        {
            if (player.armorStatus == ArmorStatus.Acid)
            {
                currentArmorValue = armorValue;
            }
            else if (player.armorStatus == ArmorStatus.SilverArmor)
            {
                currentArmorValue = 5 + (int) (player.playerDetails.playerArmorValue * 1.5f);
            }
            else if (player.armorStatus == ArmorStatus.GoldenArmor)
            {
                currentArmorValue = 10 + player.playerDetails.playerArmorValue * 2;
            }
        }

        if (enemy != null)
        {
            currentArmorValue = armorValue;
        }
    }

    /// <summary>
    /// Reset armor value
    /// </summary>
    public void ResetArmorValue()
    {
        if (player != null)
        {
            currentArmorValue = player.playerDetails.playerArmorValue;
        }
    }

    /// <summary>
    /// Get current armor value
    /// </summary>
    public int GetArmorValue()
    {
        return currentArmorValue;
    }
}
