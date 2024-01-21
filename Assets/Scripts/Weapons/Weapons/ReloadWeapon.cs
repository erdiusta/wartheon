using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(ReloadWeaponEvent))]
[RequireComponent(typeof(WeaponReloadedEvent))]
[RequireComponent(typeof(SetActiveWeaponEvent))]
[DisallowMultipleComponent]
public class ReloadWeapon : MonoBehaviour
{
    ReloadWeaponEvent reloadWeaponEvent;
    WeaponReloadedEvent weaponReloadedEvent;
    SetActiveWeaponEvent setActiveWeaponEvent;
    Coroutine reloadWeaponCoroutine;

    private void Awake()
    {
        reloadWeaponEvent = GetComponent<ReloadWeaponEvent>();
        weaponReloadedEvent = GetComponent<WeaponReloadedEvent>();
        setActiveWeaponEvent = GetComponent<SetActiveWeaponEvent>();
    }

    private void OnEnable()
    {
        reloadWeaponEvent.OnReloadWeapon += ReloadWeaponEvent_OnReloadWeapon;
        setActiveWeaponEvent.OnSetActiveRightHandWeapon += SetActiveWeaponEvent_OnSetActiveRightHandWeapon;
    }

    private void OnDisable()
    {
        reloadWeaponEvent.OnReloadWeapon -= ReloadWeaponEvent_OnReloadWeapon;
        setActiveWeaponEvent.OnSetActiveRightHandWeapon -= SetActiveWeaponEvent_OnSetActiveRightHandWeapon;
    }

    /// <summary>
    /// Handle reload weapon event
    /// </summary>
    private void ReloadWeaponEvent_OnReloadWeapon(ReloadWeaponEvent reloadWeaponEvent, ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        StartReloadingWeapon(reloadWeaponEventArgs);
    }

    /// <summary>
    /// Start reloading the weapon
    /// </summary>
    private void StartReloadingWeapon(ReloadWeaponEventArgs reloadWeaponEventArgs)
    {
        if (reloadWeaponCoroutine != null)
        {
            StopCoroutine(reloadWeaponCoroutine);
        }

        reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(reloadWeaponEventArgs.weapon, reloadWeaponEventArgs.topUpProjectilePercent));
    }

    /// <summary>
    /// Reload weapon coroutine
    /// </summary>
    IEnumerator ReloadWeaponRoutine(Weapon weapon, int topUpProjectilePercent)
    {
        // Play reload sound if there is one
        if (weapon.weaponDetails.weaponReloadingSoundEffect != null)
        {
            SoundEffectManager.Instance.PlaySoundEffect(weapon.weaponDetails.weaponReloadingSoundEffect);
        }

        // Set weapon as reloading
        weapon.isWeaponReloading = true;

        // Update reload progress timer
        while (weapon.weaponReloadTimer < weapon.weaponDetails.weaponReloadTime)
        {
            weapon.weaponReloadTimer += Time.deltaTime;
            yield return null;
        }

        // If total projectile is to be increased then update
        if (topUpProjectilePercent != 0)
        {
            int projectileIncrease = Mathf.RoundToInt((weapon.weaponDetails.weaponProjectileCapacity * topUpProjectilePercent) / 100f);

            int totalProjectile = weapon.weaponRemainingProjectile + projectileIncrease;

            weapon.weaponRemainingProjectile = totalProjectile > weapon.weaponDetails.weaponProjectileCapacity ?
                weapon.weaponDetails.weaponProjectileCapacity : totalProjectile;

        }

        // If weapon has infinite projectile or remaining projectile is greater than the amount required to
        // refill the clip then just refill the clip
        if (weapon.weaponDetails.hasInfiniteProjectile || weapon.weaponRemainingProjectile >= weapon.weaponDetails.weaponClipProjectileCapacity)
        {
            weapon.weaponClipRemainingProjectile = weapon.weaponDetails.weaponClipProjectileCapacity;
        }
        //Else set the clip to the remaining ammo
        else
        {
            weapon.weaponClipRemainingProjectile = weapon.weaponRemainingProjectile;
        }

        // Reset weapon reload timer
        weapon.weaponReloadTimer = 0f;

        // Set weapon as not reloading
        weapon.isWeaponReloading = false;

        // Call weapon reloaded event
        weaponReloadedEvent.CallWeaponReloadedEvent(weapon);
    }

    /// <summary>
    /// Set active weapon event handler
    /// </summary>
    private void SetActiveWeaponEvent_OnSetActiveRightHandWeapon(SetActiveWeaponEvent setActiveWeaponEvent, SetActiveWeaponEventArgs setActiveWeaponEventArgs)
    {
        if (setActiveWeaponEventArgs.weapon.isWeaponReloading)
        {
            if (reloadWeaponCoroutine != null)
            {
                StopCoroutine(reloadWeaponCoroutine);
            }

            reloadWeaponCoroutine = StartCoroutine(ReloadWeaponRoutine(setActiveWeaponEventArgs.weapon, 0));
        }
    }
}
