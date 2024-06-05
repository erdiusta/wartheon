using Random = UnityEngine.Random;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(SelectedActiveItem))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    public Transform prechargeBarContainer;
    public RectTransform prechargeBar;

    Enemy enemy;
    float firePrechargeTimer = 0f;
    float fireRateCooldownTimer = 0f;
    ActiveWeapon activeWeapon;
    SelectedActiveItem selectedActiveItem;
    FireWeaponEvent fireWeaponEvent;
    ReloadWeaponEvent reloadWeaponEvent;
    WeaponFiredEvent weaponFiredEvent;

    private void Awake()
    {
        enemy = GetComponent<Enemy>();
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
        selectedActiveItem = GetComponent<SelectedActiveItem>();
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
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
                    FireProjectile(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector,
                        fireWeaponEventArgs.headShotHappened);
                    ResetCooldownTimer();
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
            if (!prechargeBarContainer.gameObject.activeSelf)
            {
                // Activate precharge bar container
                firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime;
                prechargeBarContainer.gameObject.SetActive(true);
            }

            // Decrease precharge timer if fire button held previous frame
            firePrechargeTimer -= Time.deltaTime;

            // Update precharge bar
            float barFill = firePrechargeTimer / GameManager.Instance.GetPlayer().activeWeapon.GetCurrentMainHandWeapon().weaponDetails.
                weaponPrechargeTime;

            // Update bar fill
            prechargeBar.transform.localScale = new Vector3(barFill, 1f, 1f);
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
        if (!activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasInfiniteProjectile && activeWeapon.GetCurrentMainHandWeapon().
            weaponRemainingProjectile <= 0)
            return false;

        // If no projectile in the clip and the weapon doesn't have infinite clip capacity then return false
        if (!activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasInfiniteClipCapacity && activeWeapon.GetCurrentMainHandWeapon().
            weaponClipRemainingProjectile <= 0)
        {
            // Trigger a reload weapon event
            reloadWeaponEvent.CallReloadWeaponEvent(activeWeapon.GetCurrentMainHandWeapon(), 0);

            return false;
        }

        // If the weapon is reloading then return false
        if (activeWeapon.GetCurrentMainHandWeapon().isWeaponReloading)
            return false;

        // If the weapon isn't precharged or is cooling down then return false.
        if (firePrechargeTimer > 0f || fireRateCooldownTimer > 0f)
            return false;

        // Weapon is ready to fire - return true
        return true;
    }

    /// <summary>
    /// Set up ammo using an ammo gameObject and component from the object pool.
    /// </summary>
    private void FireProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool headShotHappened, bool isActiveItem = false)
    {
        if (!isActiveItem)
        {
            ProjectileDetailsSO currentProjectile = activeWeapon.GetCurrentProjectile();

            if (currentProjectile != null)
            {
                // Fire projectile routine
                StartCoroutine(FireProjectileRoutine(currentProjectile, aimAngle, weaponAimAngle, weaponAimDirectionVector, headShotHappened));
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
    IEnumerator FireProjectileRoutine(ProjectileDetailsSO currentProjectile, float aimAngle, float weaponAimAngle, 
        Vector3 weaponAimDirectionVector, bool headShotHappened, bool isActiveItem = false)
    {      
        int projectileCounter = 0;

        // Get random projectile per shot
        int projectilePerShot = Random.Range(currentProjectile.projectileSpawnAmountMin, currentProjectile.projectileSpawnAmountMax + 1);

        // Get random interval between projectile
        float projectileSpawnInterval;

        if (projectilePerShot > 1)
        {
            projectileSpawnInterval = Random.Range(currentProjectile.projectileSpawnIntervalMin, currentProjectile.projectileSpawnIntervalMax);
        }
        else
        {
            projectileSpawnInterval = 0f;
        }

        // Loop for number of projectile per shot
        while (projectileCounter < projectilePerShot)
        {
            projectileCounter++;

            // Get projectile prefab from array
            GameObject projectilePrefab = currentProjectile.projectilePrefabArray[Random.Range(0, currentProjectile.projectilePrefabArray.Length)];

            // Get random speed value
            float projectileSpeed = Random.Range(currentProjectile.projectileSpeedMin, currentProjectile.projectileSpeedMax);

            // Get Gameobject with IFireable component
            IFireable projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, activeWeapon.GetRightHandShootPosition(), 
                Quaternion.identity);

            // Initialize projectile
            projectile.InitializeProjectile(headShotHappened, currentProjectile, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector);

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        // Reduce projectile clip count if not infinite clip capacity
        if (!activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasInfiniteClipCapacity)
        {
            activeWeapon.GetCurrentMainHandWeapon().weaponClipRemainingProjectile--;
            activeWeapon.GetCurrentMainHandWeapon().weaponRemainingProjectile--;
        }

        // Call weapon fired event
        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentMainHandWeapon());

        // Display weapon shoot effect
        DoWeaponShootEffect(aimAngle);

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
        DoWeaponShootEffect(aimAngle);

        // Weapon fired sound effect
        WeaponSoundEffect(isActiveItem);

        if (enemy != null)
        {
            enemy.isFiring = false;
        }
    }

    /// <summary>
    /// Reset cooldown timer
    /// </summary>
    private void ResetCooldownTimer()
    {
        // Reset cooldown timer
        fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponFireRate;
    }

    /// <summary>
    /// Reset precharge timer
    /// </summary>
    private void ResetPrechargeTimer(bool firePreviousFrame)
    {
        // Reset precharge timer
        firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime;

        // Reset bar fill and disable the bar container
        prechargeBar.transform.localScale = new Vector3(1f, 1f, 1f);
        prechargeBarContainer.gameObject.SetActive(false);

        if (tag == Settings.playerTag && GetComponent<Player>().activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0f)
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
    private void DoWeaponShootEffect(float aimAngle)
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

            // Set gameobject active (the particle system is set to automatically disable the
            // gameobject once finished)
            weaponShootEffect.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Play weapon shooting sound effect
    /// </summary>
    private void WeaponSoundEffect(bool isActiveItem)
    {
        if (isActiveItem) return;

        if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect != null &&
            GetComponent<PlayerControl>().isSoundPlayed == false)
        {           
            GetComponent<PlayerControl>().isSoundPlayed = true;
            SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
        }
    }
}
