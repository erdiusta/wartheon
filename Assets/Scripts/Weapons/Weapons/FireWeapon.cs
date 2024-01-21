using System;
using Random = UnityEngine.Random;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    float firePrechargeTimer = 0f;
    float fireRateCooldownTimer = 0f;
    ActiveWeapon activeWeapon;
    FireWeaponEvent fireWeaponEvent;
    ReloadWeaponEvent reloadWeaponEvent;
    WeaponFiredEvent weaponFiredEvent;

    private void Awake()
    {
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
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
        // Handle weapon precharge timer
        WeaponPrecharge(fireWeaponEventArgs);

        // Weapon fire
        if (fireWeaponEventArgs.fire)
        {
            // Test if weapon is ready to fire
            if (IsWeaponReadyToFire())
            {
                FireProjectile(fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector);
                ResetCooldownTimer();
                ResetPrechargeTimer();
            }
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
            // Decrease precharge timer if fire button held previous frame
            firePrechargeTimer -= Time.deltaTime;
        }
        else
        {
            // Else reset the precharge timer
            ResetPrechargeTimer();
        }
    }

    /// <summary>
    /// Returns true if the weapon is ready to fire, else returns false.
    /// </summary>
    private bool IsWeaponReadyToFire()
    {
        // If there is no projectile and weapon doesn't have infinite projectile then return false
        if (!activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasInfiniteProjectile && activeWeapon.GetCurrentRightHandWeapon().weaponRemainingProjectile <= 0)
            return false;

        // If no projectile in the clip and the weapon doesn't have infinite clip capacity then return false
        if (!activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasInfiniteClipCapacity && activeWeapon.GetCurrentRightHandWeapon().weaponClipRemainingProjectile <= 0)
        {
            // Trigger a reload weapon event
            reloadWeaponEvent.CallReloadWeaponEvent(activeWeapon.GetCurrentRightHandWeapon(), 0);

            return false;
        }

        // If the weapon is reloading then return false
        if (activeWeapon.GetCurrentRightHandWeapon().isWeaponReloading)
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
    private void FireProjectile(float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
    {
        ProjectileDetailsSO currentProjectile = activeWeapon.GetCurrentProjectile();

        if (currentProjectile != null)
        {
            // Fire projectile routine
            StartCoroutine(FireProjectileRoutine(currentProjectile, aimAngle, weaponAimAngle, weaponAimDirectionVector));
        }
    }

    /// <summary>
    /// Coroutine to spawn multiple ammo per shot if specified in the ammo details
    /// </summary>
    IEnumerator FireProjectileRoutine(ProjectileDetailsSO currentProjectile, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector)
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
            IFireable projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, activeWeapon.GetRightHandShootPosition(), Quaternion.identity);

            // Initialize projectile
            projectile.InitializeProjectile(currentProjectile, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector);

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        // Reduce projectile clip count if not infinite clip capacity
        if (!activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasInfiniteClipCapacity)
        {
            activeWeapon.GetCurrentRightHandWeapon().weaponClipRemainingProjectile--;
            activeWeapon.GetCurrentRightHandWeapon().weaponRemainingProjectile--;
        }

        // Call weapon fired event
        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentRightHandWeapon());

        // Display weapon shoot effect
        DoWeaponShootEffect(aimAngle);

        // Weapon fired sound effect
        WeaponSoundEffect();
    }

    /// <summary>
    /// Reset cooldown timer
    /// </summary>
    private void ResetCooldownTimer()
    {
        // Reset cooldown timer
        fireRateCooldownTimer = activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponFireRate;
    }

    /// <summary>
    /// Reset precharge timer
    /// </summary>
    private void ResetPrechargeTimer()
    {
        // Reset precharge timer
        firePrechargeTimer = activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponPrechargeTime;
    }

    /// <summary>
    /// Display the weapon shoot effect
    /// </summary>
    private void DoWeaponShootEffect(float aimAngle)
    {
        // Process if there is a shoot effect & prefab
        if (activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponShootEffect != null && activeWeapon.GetCurrentRightHandWeapon().
            weaponDetails.weaponShootEffect.weaponShootEffectPrefab != null)
        {
            // Get weapon shoot effect gameobject from the pool with particle system component
            WeaponShootEffect weaponShootEffect = (WeaponShootEffect)PoolManager.Instance.ReuseComponent(activeWeapon.GetCurrentRightHandWeapon().
                weaponDetails.weaponShootEffect.weaponShootEffectPrefab, activeWeapon.GetRightHandShootEffectPosition(), Quaternion.identity);

            // Set shoot effect
            weaponShootEffect.SetShootEffect(activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponShootEffect, aimAngle);

            // Set gameobject active (the particle system is set to automatically disable the
            // gameobject once finished)
            weaponShootEffect.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Play weapon shooting sound effect
    /// </summary>
    private void WeaponSoundEffect()
    {
        if (activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponFiringSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponFiringSoundEffect);
        }
    }
}
