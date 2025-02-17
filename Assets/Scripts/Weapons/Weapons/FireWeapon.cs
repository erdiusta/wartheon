using Random = UnityEngine.Random;
using UnityEngine;
using System.Collections;
using UnityEngine.UIElements;

[RequireComponent(typeof(ActiveWeapon))]
[RequireComponent(typeof(SelectedActiveItem))]
[RequireComponent(typeof(FireWeaponEvent))]
[RequireComponent(typeof(WeaponFiredEvent))]
[DisallowMultipleComponent]
public class FireWeapon : MonoBehaviour
{
    public Transform prechargeBarContainer;
    public RectTransform prechargeBar;
    public float fireRateCooldownTimer = 0f;

    Player player;
    Enemy enemy;
    float firePrechargeTimer = 0f;
    ActiveWeapon activeWeapon;
    SelectedActiveItem selectedActiveItem;
    FireWeaponEvent fireWeaponEvent;
    WeaponFiredEvent weaponFiredEvent;
    int normalShotCounter = 0;
    int laserFrameGauge;

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
        //fireWeaponEvent.OnFocousedAim += FireWeaponEvent_OnFocousedAim;
    }

    private void OnDisable()
    {
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;
        //fireWeaponEvent.OnFocousedAim -= FireWeaponEvent_OnFocousedAim;
    }

    private void Update()
    {
        // Decrease cooldown timer.
        fireRateCooldownTimer -= Time.deltaTime;

        if (player != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (fireRateCooldownTimer < 0 && activeWeapon.GetCurrentMainHandWeapon().onCooldown && !activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
                {
                    activeWeapon.GetCurrentMainHandWeapon().onCooldown = false;
                }
            }
        }
        else if (enemy != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (fireRateCooldownTimer < 0 && activeWeapon.GetCurrentMainHandWeapon().onCooldown)
                {
                    activeWeapon.GetCurrentMainHandWeapon().onCooldown = false;
                }
            }
        }
    }

    /// <summary>
    /// Handle fire weapon event.
    /// </summary>
    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs fireWeaponEventArgs)
    {
        WeaponFire(fireWeaponEventArgs);
    }

    ///// <summary>
    ///// Handle focus aim event
    ///// </summary>
    //private void FireWeaponEvent_OnFocousedAim(FireWeaponEvent fireWeaponEvent, FireFocusedShotEventArgs focusedShotEventArgs)
    //{
    //    lockedTargetVector = focusedShotEventArgs.lockedTargetVector;
    //    lockedAngle = focusedShotEventArgs.lockedAngle;
    //    lockedAimDirection = focusedShotEventArgs.lockedAimDirection;
    //}

    /// <summary>
    /// Fire weapon
    /// </summary>
    private void WeaponFire(FireWeaponEventArgs fireWeaponEventArgs)
    {
        // Laser beam check
        if (tag == Settings.enemyTag)
        {
            // Handle laser beam as a continuousattack
            if (fireWeaponEventArgs.isLaser)
            {
                // Ensure laser fires only once
                if (!enemy.isFiring)
                {
                    enemy.isFiring = true;

                    // Fire Laser Beam Projectile (only once)
                    FireProjectile(fireWeaponEventArgs.belongingEnemy, fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector, 
                        fireWeaponEventArgs.isLaser, fireWeaponEventArgs.headShotHappened, false, fireWeaponEventArgs.isPenetrationArrow, fireWeaponEventArgs.centaurPhase, 
                        fireWeaponEventArgs.treantPhase, fireWeaponEventArgs.galvanusPhase, fireWeaponEventArgs.sepharothPhase);

                    // Keep laser active for its full duration
                    StartCoroutine(LaserDurationCoroutine());                 
                }

                return; // Prevent further processing for standard projectiles
            }
        }

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
                if (fireWeaponEventArgs.sepharothPhase == SepharothPhase.InvisibleAndMine)
                {
                    firePrechargeTimer = -3f; // Prevent charger issue for mine
                }

                // Test if weapon is ready to fire
                if (IsWeaponReadyToFire())
                {
                    FireProjectile(fireWeaponEventArgs.belongingEnemy, fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector, 
                        fireWeaponEventArgs.isLaser, fireWeaponEventArgs.headShotHappened, false, fireWeaponEventArgs.isPenetrationArrow, fireWeaponEventArgs.centaurPhase, 
                        fireWeaponEventArgs.treantPhase,fireWeaponEventArgs.galvanusPhase, fireWeaponEventArgs.sepharothPhase);
                    ResetCooldownTimer(fireWeaponEventArgs.centaurPhase);
                    ResetPrechargeTimer(fireWeaponEventArgs.firePreviousFrame);
                }
            }
        }
        // Active item routine
        else
        {
            FireProjectile(fireWeaponEventArgs.belongingEnemy, fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector,
                fireWeaponEventArgs.isLaser, fireWeaponEventArgs.headShotHappened, true);
        }
    }

    /// <summary>
    /// Handle weapon precharge
    /// </summary>
    private void WeaponPrecharge(FireWeaponEventArgs fireWeaponEventArgs)
    {
        if (fireRateCooldownTimer <= 0f)
        {
            // Weapon precharge 
            if (fireWeaponEventArgs.firePreviousFrame)
            {
                if (!prechargeBarContainer.gameObject.activeSelf)
                {
                    // Activate precharge bar container
                    if (player != null)
                    {
                        firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime * player.additionalCastDurationModifier;
                    }
                    else
                    {
                        firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime;
                    }

                    prechargeBarContainer.gameObject.SetActive(true);
                }

                // Set precharging flag to true
                activeWeapon.GetCurrentMainHandWeapon().onPrecharge = true;

                // Decrease precharge timer if fire button held previous frame
                firePrechargeTimer -= Time.deltaTime;

                // Update precharge bar
                float barFill = firePrechargeTimer / (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime * player.additionalCastDurationModifier);

                // Update bar fill
                prechargeBar.transform.localScale = barFill > 0 ? new Vector3(barFill, 1f, 1f) : new Vector3(0f, 1f, 1f);
            }
            // If precharge stops prematurely
            else if (!fireWeaponEventArgs.firePreviousFrame && firePrechargeTimer > 0f)
            {
                ResetPrechargeTimer(fireWeaponEventArgs.firePreviousFrame);

                if (activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    activeWeapon.GetCurrentMainHandWeapon().firingStoppedPrematurelyIfWeaponIsPrecharged = true;
                    activeWeapon.GetCurrentMainHandWeapon().onCooldown = false;
                }
            }
            //else
            //{
            //    // Else reset the precharge timer
            //    ResetPrechargeTimer(fireWeaponEventArgs.firePreviousFrame);             
            //}
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
    /// Ensures the laser remains active for its full duration before allowing another shot.
    /// </summary>
    IEnumerator LaserDurationCoroutine()
    {
        float laserDuration = enemy.enemyDetails.enemyWeapon.weaponCooldownDuration; // Adjust this if necessary
        yield return new WaitForSeconds(laserDuration);

        ResetCooldownTimer();
        activeWeapon.GetCurrentMainHandWeapon().onCooldown = true;
        enemy.isFiring = false; // Allow firing again only after the laser ends
    }

    /// <summary>
    /// Set up ammo using an ammo gameObject and component from the object pool.
    /// </summary>
    private void FireProjectile(Enemy belongingEnemy, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser, bool headShotHappened, bool isActiveItem = false, 
        bool isPenetrationArrow = false, CentaurPhase centaurPhase = CentaurPhase.None, TreantPhase treantPhase = TreantPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None,
        SepharothPhase sepharothPhase = SepharothPhase.None)
    {
        if (!isActiveItem)
        {
            ProjectileDetailsSO currentProjectile;

            if (sepharothPhase == SepharothPhase.InvisibleAndMine)
            {
                currentProjectile = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSecondaryProjectile;
            }
            else
            {
                currentProjectile = activeWeapon.GetCurrentProjectile();
            }

            if (currentProjectile != null)
            {
                // Fire projectile routine
                StartCoroutine(FireProjectileRoutine(belongingEnemy, currentProjectile, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, headShotHappened, 
                    false, isPenetrationArrow, centaurPhase, treantPhase, galvanusPhase, sepharothPhase));
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
    IEnumerator  FireProjectileRoutine(Enemy belongingEnemy, ProjectileDetailsSO currentProjectile, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, 
        bool isLaser = false, bool headShotHappened = false, bool isActiveItem = false, bool isPenetrationArrow = false, CentaurPhase centaurPhase = CentaurPhase.None, 
        TreantPhase treantPhase = TreantPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None)
    {      
        int projectileCounter = 0;

        int projectilePerShot = 1;

        // CENTAUR - SPREAD ARROW SHOT
        if (centaurPhase == CentaurPhase.SpreadArrowShot)
        {
            projectilePerShot = 7;
        }
        // TREANT - RAZOR LEAF
        else if (treantPhase == TreantPhase.RazorLeaf)
        {
            projectilePerShot = 12;
        }
        // GALVANUS - LIGHTNING
        else if (galvanusPhase == GalvanusPhase.Lightning)
        {
            projectilePerShot = 3;
        }
        // SEPHAROTH - LASER
        else if (sepharothPhase == SepharothPhase.LaserBeam)
        {
            projectilePerShot = 2;
        }
        // SEPHAROTH - MINE
        else if (sepharothPhase == SepharothPhase.InvisibleAndMine)
        {
            projectilePerShot = 3;
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
            if (centaurPhase == CentaurPhase.SpreadArrowShot || treantPhase == TreantPhase.RazorLeaf)
            {
                projectileSpawnInterval = 0;
            }
            else if (galvanusPhase == GalvanusPhase.Lightning)
            {
                projectileSpawnInterval = 1f;
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

        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Default position
        Vector3 projectileSpawnPoint = activeWeapon.GetRightHandShootPosition();

        // Loop for number of projectile per shot
        while (projectileCounter < projectilePerShot)
        {
            projectileCounter++;

            int selectedIndexNum = -1;

            switch (projectileCounter)
            {
                case 2:
                    if (sepharothPhase == SepharothPhase.LaserBeam)
                    {
                        aimAngle += 120;
                        weaponAimAngle += 120;
                    }
                    else if (sepharothPhase == SepharothPhase.InvisibleAndMine)
                    {
                        // Selected second mine' position
                        selectedIndexNum = Random.Range(0, currentRoom.spawnPositionArray.Length);
                        Vector3Int selectedFirstSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[selectedIndexNum].x, currentRoom.spawnPositionArray[selectedIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedFirstSpawnPoint);
                    }
                    break;
                case 3:
                    if (sepharothPhase == SepharothPhase.InvisibleAndMine)
                    {
                        // Selected thir mine' position
                        selectAgain:
                        int selectedSecondIndexNum = Random.Range(0, currentRoom.spawnPositionArray.Length);

                        if (selectedSecondIndexNum == selectedIndexNum) goto selectAgain;

                        Vector3Int selectedSecondSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[selectedSecondIndexNum].x, currentRoom.spawnPositionArray[selectedSecondIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedSecondSpawnPoint);
                    }
                    break;
                default:
                    break;
            }

            GameObject projectilePrefab;

            // Get projectile prefab from array
            if (centaurPhase == CentaurPhase.SpreadArrowShot)
            {
                projectilePrefab = currentProjectile.projectilePrefabArray[1];
            }
            else if (treantPhase == TreantPhase.RazorLeaf)
            {
                projectilePrefab = currentProjectile.projectilePrefabArray[0];
            }
            else if (galvanusPhase == GalvanusPhase.Lightning)
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
            else if (galvanusPhase == GalvanusPhase.Lightning)
            {
                projectileSpeed = 0;
            }

            // Get Gameobject with IFireable component
            IFireable projectile;

            if (galvanusPhase == GalvanusPhase.Lightning)
            {
                // Get room bounds from template bounds
                Vector2Int lowerBounds = currentRoom.templateLowerBounds;
                Vector2Int upperBounds = currentRoom.templateUpperBounds;

                // Get the player's current cell position
                Vector3Int playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(GameManager.Instance.GetPlayer().transform.position);

                // Generate a random lightning strike position within bounds
                Vector3Int randomCellPosition;
                Vector3 worldPosition;

                do
                {
                    // Generate random position within 4 tiles around the player
                    int randomX = Mathf.Clamp(Random.Range(playerCellPosition.x - 4, playerCellPosition.x + 5), lowerBounds.x, upperBounds.x);
                    int randomY = Mathf.Clamp(Random.Range(playerCellPosition.y - 4, playerCellPosition.y + 5), lowerBounds.y, upperBounds.y);
                    randomCellPosition = new Vector3Int(randomX, randomY, 0);

                    // Convert to room-local zero-based coordinates
                    Vector3Int zeroBasedCellPosition = new Vector3Int(randomCellPosition.x - lowerBounds.x, randomCellPosition.y - lowerBounds.y, 0);

                    // Validate position using penalty system
                    if (currentRoom.instantiatedRoom.GetRoomTilePenaltyValue(zeroBasedCellPosition) == 1)
                    {
                        // Tile is valid, convert cell position to world position
                        worldPosition = currentRoom.instantiatedRoom.grid.CellToWorld(randomCellPosition);

                        projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, GameManager.Instance.GetPlayer().transform.position, Quaternion.identity);
                        break;
                    }
                }
                while (true);

            }
            else
            {
                if (sepharothPhase != SepharothPhase.InvisibleAndMine)
                {
                    projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, activeWeapon.GetRightHandShootPosition(), Quaternion.identity);
                }
                else
                {
                    projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, projectileSpawnPoint, Quaternion.identity);
                }
            }

            if (isLaser)
            {
                Projectile spawnedProjectile = (Projectile)projectile;

                spawnedProjectile.lockedTargetVector = weaponAimDirectionVector;
                spawnedProjectile.lockedAngle = aimAngle;
            }

            // Initialize projectile
            projectile.InitializeProjectile(belongingEnemy, headShotHappened, currentProjectile, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector, false, false, 
                isPenetrationArrow, projectileCounter - 1, projectilePerShot, centaurPhase, treantPhase, galvanusPhase, sepharothPhase);

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        // Set weapon's onCooldown status to true for triggering Weapon status UI
        activeWeapon.GetCurrentMainHandWeapon().onCooldown = !isLaser ? true : false;

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
        if (player != null)
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow ||
                player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow)
            {
                fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration *
                    (1 + player.additionalBowAttackCoolDownModifier);
            }
            else
            {
                fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration * coolDownTimerModifier;
            }
        }
        else
        {
            fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration * coolDownTimerModifier;
        }
    }

    /// <summary>
    /// Reset precharge timer
    /// </summary>
    private void ResetPrechargeTimer(bool firePreviousFrame)
    {
        // Reset precharge timer
        if (player != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                // Reset precharge timer
                firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime * player.additionalCastDurationModifier;

                // Set weapon's precharge flag to false
                activeWeapon.GetCurrentMainHandWeapon().onPrecharge = false;

                if (firePreviousFrame)
                {
                    activeWeapon.GetCurrentMainHandWeapon().firingCompletedIfWeaponIsPrecharged = true;
                }
            }
        }

        // Reset bar fill and disable the bar container
        prechargeBar.transform.localScale = new Vector3(1f, 1f, 1f);
        prechargeBarContainer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Display the weapon shoot effect
    /// </summary>
    private void WeaponShootEffect(float aimAngle)
    {
        // Process if there is a shoot effect & prefab
        if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponShootEffect != null && activeWeapon?.GetCurrentMainHandWeapon().
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
