using Random = UnityEngine.Random;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(SelectedActiveItem))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    public Transform prechargeBarContainer;
    public RectTransform prechargeBar;

    Player player;
    Enemy enemy;
    float firePrechargeTimer = 0f;
    float fireRateCooldownTimer = 0f;
    ActiveWeapon activeWeapon;
    SelectedActiveItem selectedActiveItem;
    FireWeaponEvent fireWeaponEvent;
    WeaponFiredEvent weaponFiredEvent;
    int normalShotCounter = 0;

    private void Awake()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        selectedActiveItem = GetComponent<SelectedActiveItem>();
        weaponFiredEvent = GetComponent<WeaponFiredEvent>();
    }

    private void OnEnable()
    {
        fireWeaponEvent.OnFireWeapon += FireWeaponEvent_OnFireWeapon;
    }

    private void OnDisable()
    {
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
    }

    private void Update()
    {
        // Decrease cooldown timer.
        fireRateCooldownTimer -= Time.deltaTime;
    }

    /// <summary>
    /// Handle fire weapon event.
    /// </summary>
    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponFire(fireWeaponEventArgs);
    }

    /// <summary>
    /// Fire weapon
    /// </summary>
    private void WeaponFire(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // Standard weapon routine
        if (!fireWeaponEventArgs.isActiveItem)
        {
            if (tag == Settings.enemyTag)
            {
                // Flag firing
                enemy.isFiring = true;
            }

            // Handle weapon precharge timer
            WeaponPrecharge(fireWeaponEventArgs);

            // Weapon fire
            if (fireWeaponEventArgs.fire)
            {
                // Test if weapon is ready to fire
                if (IsWeaponReadyToFire())
                {
                    if (enemy != null)
                    {
                        if (fireWeaponEventArgs.centaurPhase != CentaurPhase.SpreadArrowShot)
                        {
                            // Trigger fire weapon event
                            enemy.animateEnemy.SetAttackAnimationParameters();
                        }
                    }

                    FireProjectile(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector,
                        fireWeaponEventArgs.headShotHappened, false, fireWeaponEventArgs.isPenetrationArrow, fireWeaponEventArgs.centaurPhase);
                    ResetCooldownTimer(fireWeaponEventArgs.centaurPhase);
                    ResetPrechargeTimer(fireWeaponEventArgs.firePreviousFrame);
                }
            }
        }
        // Active item routine
        else
        {
            FireProjectile(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector,
                fireWeaponEventArgs.headShotHappened, true);
        }
    }

    /// <summary>
    /// Handle weapon precharge
    /// </summary>
    private void WeaponPrecharge(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // Weapon precharge 
        if (fireWeaponEventArgs.firePreviousFrame)
        {
            // If cooldown continues return
            if (fireRateCooldownTimer > 0f) return;

            if (!prechargeBarContainer.gameObject.activeSelf)
            {
                // Activate precharge bar container
                firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime;
                prechargeBarContainer.gameObject.SetActive(true);
            }

            // Set precharging flag to true
            activeWeapon.GetCurrentMainHandWeapon().onPrecharge = true;

            // Decrease precharge timer if fire button held previous frame
            firePrechargeTimer -= Time.deltaTime;

            // Update precharge bar
            float barFill = firePrechargeTimer / activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime;

            // Update bar fill
            prechargeBar.transform.localScale = barFill > 0 ? new Vector3(barFill, 1f, 1f) : new Vector3(0f, 1f, 1f);
        }
        else
        {
            // Else reset the precharge timer
            ResetPrechargeTimer(fireWeaponEventArgs.firePreviousFrame);
        }
    }

    /// <summary>
    /// Returns true if the weapon is ready to fire, else returns false.
    /// </summary>
    private bool IsWeaponReadyToFire()
    {
        // If there is no projectile and weapon doesn't have infinite projectile then return false
        if (!activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasInfiniteProjectile && activeWeapon.GetCurrentMainHandWeapon().weaponRemainingProjectile <= 0)
            return false;

        // If the weapon isn't precharged or is cooling down then return false.
        if (firePrechargeTimer > 0f || fireRateCooldownTimer > 0f) return false;

        // Weapon is ready to fire - return true
        return true;
    }

    /// <summary>
    /// Set up ammo using an ammo gameObject and component from the object pool.
    /// </summary>
    private void FireProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool headShotHappened, bool isActiveItem = false, 
        bool isPenetrationArrow = false, CentaurPhase centaurPhase = CentaurPhase.None)
    {
        if (!isActiveItem)
        {
            ProjectileDetailsSO currentProjectile = activeWeapon.GetCurrentProjectile();

            if (currentProjectile != null)
            {
                // Fire projectile routine
                StartCoroutine(FireProjectileRoutine(currentProjectile, aimAngle, weaponAimAngle, weaponAimDirectionVector, headShotHappened, 
                    false, isPenetrationArrow, centaurPhase));
            }
        }
        else
        {
            ActiveItemDetailsSO currentActiveItem = selectedActiveItem.GetCurrentActiveItem().activeItemDetails;

            if (currentActiveItem != null)
            {
                if (selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge > 0)
                {
                    if (currentActiveItem.activeItemSwingSoundEffect != null)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(currentActiveItem.activeItemSwingSoundEffect);
                    }

                    // Fire projectile routine for active item
                    StartCoroutine(FireProjectileRoutine(currentActiveItem, aimAngle, weaponAimAngle, weaponAimDirectionVector, headShotHappened, true));
                }
            }
        }
    }

    /// <summary>
    /// Coroutine to spawn multiple ammo per shot if specified in the ammo details - PROJECTILE
    /// </summary>
    IEnumerator  FireProjectileRoutine(ProjectileDetailsSO currentProjectile, float aimAngle, float weaponAimAngle, 
        Vector3 weaponAimDirectionVector, bool headShotHappened = false, bool isActiveItem = false, 
        bool isPenetrationArrow = false, CentaurPhase centaurPhase = CentaurPhase.None)
    {      
        int projectileCounter = 0;

        int projectilePerShot = 1;

        // CENTAUR

        if (centaurPhase == CentaurPhase.SpreadArrowShot)
        {
            projectilePerShot = 7;
        }
        else
        {
            // Get random projectile per shot
            projectilePerShot = Random.Range(currentProjectile.projectileSpawnAmountMin, currentProjectile.projectileSpawnAmountMax + 1);
        }

        // Get random interval between projectile
        float projectileSpawnInterval;

        if (projectilePerShot > 1)
        {
            if (centaurPhase == CentaurPhase.SpreadArrowShot)
            {
                projectileSpawnInterval = 0;
            }
            else
            {
                projectileSpawnInterval = Random.Range(currentProjectile.projectileSpawnIntervalMin, currentProjectile.projectileSpawnIntervalMax);
            }

            // Reduce projectile clip count if not infinite clip capacity
            if (!activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasInfiniteProjectile)
            {
                activeWeapon.GetCurrentMainHandWeapon().weaponRemainingProjectile -= (int)projectileSpawnInterval;
            }
        }
        else
        {
            projectileSpawnInterval = 0f;

            // Reduce projectile clip count if not infinite clip capacity
            if (!activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasInfiniteProjectile)
            {
                activeWeapon.GetCurrentMainHandWeapon().weaponRemainingProjectile --;
            }
        }

        // Loop for number of projectile per shot
        while (projectileCounter < projectilePerShot)
        {
            projectileCounter++;

            GameObject projectilePrefab;

            // Get projectile prefab from array
            if (centaurPhase == CentaurPhase.SpreadArrowShot)
            {
                projectilePrefab = currentProjectile.projectilePrefabArray[1];
            }
            else
            {
                projectilePrefab = currentProjectile.projectilePrefabArray[0];
            }

            float projectileSpeed = Random.Range(currentProjectile.projectileSpeedMin, currentProjectile.projectileSpeedMax);

            // Get random speed value
            if (centaurPhase == CentaurPhase.SpreadArrowShot)
            {
                projectileSpeed = Random.Range(currentProjectile.projectileSpeedMin / 2, currentProjectile.projectileSpeedMax / 2);
            }

            // Get Gameobject with IFireable component
            IFireable projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, activeWeapon.GetRightHandShootPosition(), 
                Quaternion.identity);

            // Initialize projectile
            projectile.InitializeProjectile(headShotHappened, currentProjectile, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector, false, false, 
                isPenetrationArrow, projectileCounter - 1, projectilePerShot, centaurPhase);

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        // Set weapon's onCooldown status to true for triggering Weapon status UI
         if (!activeWeapon.GetCurrentMainHandWeapon().onPrecharge)
         {
            activeWeapon.GetCurrentMainHandWeapon().onCooldown = true;
         }

        //activeWeapon.GetCurrentMainHandWeapon().onCooldown = true;

        // Call weapon fired event
        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentMainHandWeapon(), true);

        // Display weapon shoot effect
        WeaponShootEffect(aimAngle);

        // Weapon fired sound effect
        WeaponSoundEffect(isActiveItem);

        if (enemy != null)
        {
            enemy.isFiring = false;
        }
    }

    /// <summary>
    /// Coroutine to spawn for specified active item details - ACTIVE ITEM
    /// </summary>
    IEnumerator FireProjectileRoutine(ActiveItemDetailsSO currentActiveItem, float aimAngle, float weaponAimAngle,
        Vector3 weaponAimDirectionVector, bool headShotHappened, bool isActiveItem = false)
    {
        int projectileCounter = 0;

        // Get random projectile per shot
        int projectilePerShot = Random.Range(currentActiveItem.projectileSpawnAmountMin, currentActiveItem.projectileSpawnAmountMax + 1);

        // Get random interval between projectile
        float projectileSpawnInterval;

        if (projectilePerShot > 1)
        {
            projectileSpawnInterval = Random.Range(currentActiveItem.projectileSpawnIntervalMin, currentActiveItem.projectileSpawnIntervalMax);
        }
        else
        {
            projectileSpawnInterval = 0f;
        }

        // Loop for number of projectile per shot
        while (projectileCounter < projectilePerShot)
        {
            projectileCounter++;

            // Get active item prefab from array
            GameObject activeItemPrefab = currentActiveItem.activeItemPrefabArray[Random.Range(0, currentActiveItem.activeItemPrefabArray.Length)];

            // Get random speed value
            float projectileSpeed = Random.Range(currentActiveItem.projectileSpeedMin, currentActiveItem.projectileSpeedMax);

            // Get Gameobject with IFireable component
            IFireable projectile = (IFireable)PoolManager.Instance.ReuseComponent(activeItemPrefab, activeWeapon.GetRightHandShootPosition(), Quaternion.identity);

            // Initialize projectile
            projectile.InitializeProjectile(headShotHappened, currentActiveItem, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector);

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        // Reduce projectile clip count if not infinite clip capacity
        if (!selectedActiveItem.GetCurrentActiveItem().activeItemDetails.hasNoProjectileNumberLimit)
        {
            selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;
        }

        // Call weapon fired event
        weaponFiredEvent.CallActiveItemFiredEvent(selectedActiveItem.GetCurrentActiveItem());

        // Display weapon shoot effect
        WeaponShootEffect(aimAngle);

        // Weapon fired sound effect
        WeaponSoundEffect(isActiveItem);

        if (enemy != null)
        {
            if (enemy.enemyDetails.enemyBehaviour == EnemyBehaviour.Centaur)
            {
                normalShotCounter = 0;
            }

            enemy.isFiring = false;
        }
    }

    /// <summary>
    /// Reset cooldown timer
    /// </summary>
    private void ResetCooldownTimer(CentaurPhase centaurPhase = CentaurPhase.None)
    {
        float coolDownTimerModifier = 1f;

        if (centaurPhase == CentaurPhase.SpreadArrowShot)
        {
            coolDownTimerModifier = 3f;
        }

        // Reset cooldown timer
        fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration * coolDownTimerModifier;
        activeWeapon.GetCurrentMainHandWeapon().onCooldown = false;
    }

    /// <summary>
    /// Reset precharge timer
    /// </summary>
    private void ResetPrechargeTimer(bool firePreviousFrame)
    {
        // Reset precharge timer
        if (activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime;
        }

        // Reset bar fill and disable the bar container
        prechargeBar.transform.localScale = new Vector3(1f, 1f, 1f);
        prechargeBarContainer.gameObject.SetActive(false);

        // Set weapon's precharge flag to false
        if (activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            activeWeapon.GetCurrentMainHandWeapon().onPrecharge = false;
        }

        if (tag == Settings.playerTag && activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.weaponPrechargeTime > 0f)
        {
            // Check for first frame for not jumping to fire completed
            if (firePreviousFrame == true)
            {
                GetComponent<PlayerControl>().fireCompletedDuringPressed = true;
            }
        }
    }

    /// <summary>
    /// Display the weapon shoot effect
    /// </summary>
    private void WeaponShootEffect(float aimAngle)
    {
        // Process if there is a shoot effect & prefab
        if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponShootEffect != null && activeWeapon.GetCurrentMainHandWeapon().
            weaponDetails.weaponShootEffect.weaponShootEffectPrefab != null)
        {
            // Get weapon shoot effect gameobject from the pool with particle system component
            WeaponShootEffect weaponShootEffect = (WeaponShootEffect)PoolManager.Instance.ReuseComponent(activeWeapon.GetCurrentMainHandWeapon().
                weaponDetails.weaponShootEffect.weaponShootEffectPrefab, activeWeapon.GetRightHandShootEffectPosition(), Quaternion.identity);

            // Set shoot effect
            weaponShootEffect.SetShootEffect(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponShootEffect, aimAngle);

            // Set gameobject active (the particle system is set to automatically disable the gameobject once finished)
            weaponShootEffect.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Play weapon shooting sound effect
    /// </summary>
    private void WeaponSoundEffect(bool isActiveItem)
    {
        if (isActiveItem) return;

        if (enemy != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect != null)
            {
                SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
            }
        }
        else
        {
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect != null &&
                GetComponent<PlayerControl>().isSoundPlayed == false)
            {
                GetComponent<PlayerControl>().isSoundPlayed = true;
                SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
            }
        }
    }
}
