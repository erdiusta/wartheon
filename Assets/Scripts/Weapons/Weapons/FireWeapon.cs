using Random = UnityEngine.Random;
using UnityEngine;
using System.Collections;

[RequireComponent(typeof(ActiveWeapon))]
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
    FireWeaponEvent fireWeaponEvent;
    WeaponFiredEvent weaponFiredEvent;
    bool isFiringCoroutineRunning = false;

    private void Awake()
    {
        player = GetComponent<Player>();
        enemy = GetComponent<Enemy>();
        activeWeapon = GetComponent<ActiveWeapon>();
        fireWeaponEvent = GetComponent<FireWeaponEvent>();
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
        if (fireWeaponEventArgs.isLaser)
        {
            // Handle laser beam as a continuousattack
            if (tag == Settings.enemyTag)
            {
                // Ensure laser fires only once
                if (!enemy.isFiring)
                {
                    enemy.isFiring = true;

                    // Fire Laser Beam Projectile (only once)
                    FireProjectile(fireWeaponEventArgs.belongingEnemy, fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector, 
                        fireWeaponEventArgs.isLaser, fireWeaponEventArgs.isIceBreaker, false, fireWeaponEventArgs.isPenetrationArrow, fireWeaponEventArgs.moravellePhase, 
                        fireWeaponEventArgs.treantPhase, fireWeaponEventArgs.galvanusPhase, fireWeaponEventArgs.sepharothPhase, fireWeaponEventArgs.frostWrymPhase,
                        fireWeaponEventArgs.venomancerPhase, fireWeaponEventArgs.fireWrymPhase, fireWeaponEventArgs.moldranPhase);

                    // Keep laser active for its full duration
                    StartCoroutine(LaserDurationCoroutine(false));                 
                }

                return; // Prevent further processing for standard projectiles
            }
            else if (tag == Settings.playerTag)
            {
                if (fireWeaponEventArgs.grappleDetails != null && player.isHuntersReachActive)
                {
                    // Fire Laser Beam Projectile (only once)
                    FireProjectile(fireWeaponEventArgs.belongingEnemy, fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector,
                        fireWeaponEventArgs.isLaser, fireWeaponEventArgs.isIceBreaker, false, fireWeaponEventArgs.isPenetrationArrow, fireWeaponEventArgs.moravellePhase,
                        fireWeaponEventArgs.treantPhase, fireWeaponEventArgs.galvanusPhase, fireWeaponEventArgs.sepharothPhase, fireWeaponEventArgs.frostWrymPhase,
                        fireWeaponEventArgs.venomancerPhase, fireWeaponEventArgs.fireWrymPhase, fireWeaponEventArgs.moldranPhase, false, false, false,
                        fireWeaponEventArgs.grappleDetails, fireWeaponEventArgs.iceBreakerDetails);

                    // Keep laser active for its full duration
                    StartCoroutine(LaserDurationCoroutine(true));
                }

                return; // Prevent further processing for standard projectiles
            }
        }

        // Standard weapon routine
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

            bool isShotSpecialSkill = fireWeaponEventArgs.isIceBreaker || fireWeaponEventArgs.isFireBlast || fireWeaponEventArgs.isPenetrationArrow ||
                fireWeaponEventArgs.isBindingArrow || fireWeaponEventArgs.isTripleThreat|| fireWeaponEventArgs.grappleDetails != null ||
                fireWeaponEventArgs.isBlazingCyclone || fireWeaponEventArgs.isThrowingAxe || fireWeaponEventArgs.isShiruken || fireWeaponEventArgs.isChainLightning;

            // Test if weapon is ready to fire
            if (IsWeaponReadyToFire() || isShotSpecialSkill)
            {
                FireProjectile(fireWeaponEventArgs.belongingEnemy, fireWeaponEventArgs.aimAngle, fireWeaponEventArgs.weaponAimAngle, fireWeaponEventArgs.weaponAimDirectionVector,
                    fireWeaponEventArgs.isLaser, fireWeaponEventArgs.isIceBreaker, false, fireWeaponEventArgs.isPenetrationArrow, fireWeaponEventArgs.moravellePhase,
                    fireWeaponEventArgs.treantPhase, fireWeaponEventArgs.galvanusPhase, fireWeaponEventArgs.sepharothPhase, fireWeaponEventArgs.frostWrymPhase,
                    fireWeaponEventArgs.venomancerPhase, fireWeaponEventArgs.fireWrymPhase, fireWeaponEventArgs.moldranPhase, fireWeaponEventArgs.isTripleThreat,
                    fireWeaponEventArgs.isBindingArrow, fireWeaponEventArgs.isArrowOfTheSeven, fireWeaponEventArgs.grappleDetails, fireWeaponEventArgs.iceBreakerDetails,
                    fireWeaponEventArgs.isFireBlast, fireWeaponEventArgs.fireBlastDetails, fireWeaponEventArgs.isBlazingCyclone, fireWeaponEventArgs.blazingCycloneDetails,
                    fireWeaponEventArgs.isThrowingAxe, fireWeaponEventArgs.throwingAxeDetails, fireWeaponEventArgs.isShiruken, fireWeaponEventArgs.shirukenDetails,
                    fireWeaponEventArgs.isChainLightning, fireWeaponEventArgs.chainLightningDetails, fireWeaponEventArgs.chainLightningPhase);
                ResetCooldownTimer(fireWeaponEventArgs.moravellePhase);
                ResetPrechargeTimer(fireWeaponEventArgs.firePreviousFrame);
            }
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
    IEnumerator LaserDurationCoroutine(bool playerGrapple)
    {
        float laserDuration = 0f;

        if (playerGrapple) laserDuration = 999999999999999f;
        else laserDuration = enemy.enemyDetails.enemyWeapon.weaponCooldownDuration; // Adjust this if necessary

        yield return new WaitForSeconds(laserDuration);

        if (!playerGrapple)
        {
            ResetCooldownTimer();
            activeWeapon.GetCurrentMainHandWeapon().onCooldown = true;
            enemy.isFiring = false; // Allow firing again only after the laser ends
        }
    }

    /// <summary>
    /// Set up ammo using an ammo gameObject and component from the object pool.
    /// </summary>
    private void FireProjectile(Enemy belongingEnemy, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser, bool isIceBreaker, 
        bool isActiveItem = false, bool isPenetrationArrow = false, MoravellePhase moravellePhase = MoravellePhase.None, SylvarokPhase treantPhase = SylvarokPhase.None, 
        GalvanusPhase galvanusPhase = GalvanusPhase.None,SepharothPhase sepharothPhase = SepharothPhase.None, CryotharPhase frostWrymPhase = CryotharPhase.None, 
        VenomancerPhase venomancerPhase = VenomancerPhase.None, PyrotharPhase fireWrymPhase = PyrotharPhase.None, MoldranPhase moldranPhase = MoldranPhase.None,
        bool isTripleThreat = false, bool isBindingArrow = false, bool isArrowOfTheSeven = false, ProjectileDetailsSO grappleDetails = null,
        ProjectileDetailsSO iceBreakerDetails = null, bool isFireBlast = false, ProjectileDetailsSO fireBlastDetails = null, bool isBlazingCyclone = false,
        ProjectileDetailsSO blazingCycloneDetails = null, bool isThrowingAxe = false, ProjectileDetailsSO throwingAxeDetails = null, bool isShiruken = false,
        ProjectileDetailsSO shirukenDetails = null, bool isChainLightning = false, ProjectileDetailsSO chainLightningDetails = null,
        ChainLightningPhase chainLightningPhase = ChainLightningPhase.None)
    {
        ProjectileDetailsSO currentProjectile;

        if (galvanusPhase == GalvanusPhase.LightningBolt || sepharothPhase == SepharothPhase.InvisibleAndMine || frostWrymPhase == CryotharPhase.Icicle ||
            venomancerPhase == VenomancerPhase.StoneRain || fireWrymPhase == PyrotharPhase.FirePillar || moldranPhase == MoldranPhase.Spike)
        {
            currentProjectile = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSecondaryProjectile;
        }
        else if (grappleDetails != null)
        {
            currentProjectile = grappleDetails;
        }
        else if (iceBreakerDetails != null)
        {
            currentProjectile = iceBreakerDetails;
        }
        else if (fireBlastDetails != null)
        {
            currentProjectile = fireBlastDetails;
        }
        else if (blazingCycloneDetails != null)
        {
            currentProjectile = blazingCycloneDetails;
        }
        else if (throwingAxeDetails != null)
        {
            currentProjectile = throwingAxeDetails;
        }
        else if (shirukenDetails != null)
        {
            currentProjectile = shirukenDetails;
        }
        else if (chainLightningDetails != null)
        {
            currentProjectile = chainLightningDetails;
        }
        else
        {
            currentProjectile = activeWeapon.GetCurrentProjectile();
        }

        if (currentProjectile != null && !isFiringCoroutineRunning)
        {
            // Fire projectile routine
            StartCoroutine(FireProjectileRoutine(belongingEnemy, currentProjectile, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, isIceBreaker,
                false, isPenetrationArrow, moravellePhase, treantPhase, galvanusPhase, sepharothPhase, frostWrymPhase, venomancerPhase, fireWrymPhase, moldranPhase,
                isTripleThreat, isBindingArrow, isArrowOfTheSeven, grappleDetails, iceBreakerDetails, isFireBlast, fireBlastDetails, isBlazingCyclone, blazingCycloneDetails,
                isThrowingAxe, throwingAxeDetails, isShiruken, shirukenDetails, isChainLightning, chainLightningDetails, chainLightningPhase));
        }
    }

    /// <summary>
    /// Coroutine to spawn multiple ammo per shot if specified in the projectile details - PROJECTILE
    /// </summary>
    IEnumerator  FireProjectileRoutine(Enemy belongingEnemy, ProjectileDetailsSO currentProjectile, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, 
        bool isLaser = false, bool isIceBreaker = false, bool isActiveItem = false, bool isPenetrationArrow = false, MoravellePhase moravellePhase = MoravellePhase.None, 
        SylvarokPhase treantPhase = SylvarokPhase.None, GalvanusPhase galvanusPhase = GalvanusPhase.None, SepharothPhase sepharothPhase = SepharothPhase.None,
        CryotharPhase frostWrymPhase = CryotharPhase.None, VenomancerPhase venomancerPhase = VenomancerPhase.None, PyrotharPhase fireWrymPhase = PyrotharPhase.None,
        MoldranPhase moldranPhase = MoldranPhase.None, bool isTripleThreat = false, bool isBindingArrow = false, bool isArrowOfTheSeven = false,
        ProjectileDetailsSO grappleDetails = null, ProjectileDetailsSO iceBreakerDetails = null, bool isFireBlast = false, ProjectileDetailsSO fireBlastDetails = null,
        bool isBlazingCyclone = false, ProjectileDetailsSO blazingCycloneDetails = null, bool isThrowingAxe = false, ProjectileDetailsSO throwingAxeDetails = null,
        bool isShiruken = false, ProjectileDetailsSO shirukenDetails = null, bool isChainLightning = false, ProjectileDetailsSO chainLightningDetails = null,
        ChainLightningPhase chainLightningPhase = ChainLightningPhase.None)
    {      
        int projectileCounter = 0;

        int projectilePerShot = 1;

        // CENTAUR - SPREAD ARROW SHOT OR FROST WRYM - PROJECTILE
        if (moravellePhase == MoravellePhase.SpreadArrowShot || frostWrymPhase == CryotharPhase.IceProjectile 
            || fireWrymPhase == PyrotharPhase.FireProjectile)
        {
            projectilePerShot = 10;
        }
        else if (moldranPhase == MoldranPhase.Projectile)
        {
            projectilePerShot = 15;
        }
        // TREANT - RAZOR LEAF
        else if (treantPhase == SylvarokPhase.RazorLeaf || venomancerPhase == VenomancerPhase.SludgeThrow)
        {
            projectilePerShot = 30;
        }
        // GALVANUS - LIGHTNING
        else if (galvanusPhase == GalvanusPhase.Lightning)
        {
            projectilePerShot = 2;
        }
        // VENOMANCER - STONE RAIN
        else if (frostWrymPhase == CryotharPhase.Icicle || venomancerPhase == VenomancerPhase.StoneRain || fireWrymPhase == PyrotharPhase.FirePillar ||
            moldranPhase == MoldranPhase.Spike)
        {
            projectilePerShot = 3;
        }
        // SEPHAROTH - LASER
        else if (sepharothPhase == SepharothPhase.LaserBeam)
        {
            projectilePerShot = 2;
        }
        // SEPHAROTH - MINE
        else if (sepharothPhase == SepharothPhase.InvisibleAndMine || venomancerPhase == VenomancerPhase.ToxicPool)
        {
            projectilePerShot = 3;
        }
        // TRIPLE THREAT
        else if (isTripleThreat)
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
            if (moravellePhase == MoravellePhase.SpreadArrowShot || treantPhase == SylvarokPhase.RazorLeaf || frostWrymPhase == CryotharPhase.IceProjectile ||
                venomancerPhase == VenomancerPhase.SludgeThrow || fireWrymPhase == PyrotharPhase.FireProjectile || moldranPhase == MoldranPhase.Projectile
                || isTripleThreat)
            {
                projectileSpawnInterval = 0;
            }
            else if (galvanusPhase == GalvanusPhase.Lightning || frostWrymPhase == CryotharPhase.Icicle || venomancerPhase == VenomancerPhase.StoneRain ||
                fireWrymPhase == PyrotharPhase.FirePillar || moldranPhase == MoldranPhase.Spike)
            {
                projectileSpawnInterval = 1f;
            }
            else if (venomancerPhase == VenomancerPhase.ToxicPool)
            {
                projectileSpawnInterval = 0.8f;
            }
            else
            {
                projectileSpawnInterval = Random.Range(currentProjectile.projectileSpawnIntervalMin, currentProjectile.projectileSpawnIntervalMax);
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

        AimDirection aimDirection = HelperUtilities.GetAimDirection(weaponAimAngle);

        Vector3 weaponShootPosition = Vector3.zero;

        if (enemy != null)
        {
            switch (aimDirection)
            {
                case AimDirection.Up:
                    weaponShootPosition = activeWeapon.GetMainHandShootPositionUp();
                    break;
                case AimDirection.UpRight:
                case AimDirection.Right:
                case AimDirection.DownRight:
                    weaponShootPosition = activeWeapon.GetMainHandShootPositionRight();
                    break;
                case AimDirection.Down:
                    weaponShootPosition = activeWeapon.GetMainHandShootPositionDown();
                    break;
                case AimDirection.Left:
                case AimDirection.DownLeft:
                case AimDirection.UpLeft:
                    weaponShootPosition = activeWeapon.GetMainHandShootPositionLeft();
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (isIceBreaker)
            {
                weaponShootPosition = player.transform.position;
            }
            else
            {
                weaponShootPosition = activeWeapon.GetMainHandShootPositionUp();
            }
        }


        Room currentRoom = GameManager.Instance.GetCurrentRoom();
        Grid grid = currentRoom.instantiatedRoom.grid;

        // Default position
        Vector3 projectileSpawnPoint = activeWeapon.GetMainHandShootPositionUp();
     
        // Loop for number of projectile per shot
        while (projectileCounter < projectilePerShot)
        {
            projectileCounter++;

            int selectedIndexNum = -1;

            GameObject projectilePrefab;
            // Get projectile prefab from array
            if (moravellePhase == MoravellePhase.SpreadArrowShot || venomancerPhase == VenomancerPhase.ToxicPool)
            {
                projectilePrefab = currentProjectile.projectilePrefabArray[1];
            }
            else
            {
                projectilePrefab = currentProjectile.projectilePrefabArray[0];
            }

            switch (projectileCounter)
            {
                case 1:
                    if ( venomancerPhase == VenomancerPhase.ToxicPool)
                    {
                        // Selected second mine' position
                        selectedIndexNum = Random.Range(0, currentRoom.spawnPositionArray.Length);
                        Vector3Int selectedFirstSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[selectedIndexNum].x,
                            currentRoom.spawnPositionArray[selectedIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedFirstSpawnPoint);
                    }
                    break;

                case 2:
                    if (sepharothPhase == SepharothPhase.LaserBeam)
                    {
                        aimAngle += 120;
                        weaponAimAngle += 120;
                    }
                    else if (sepharothPhase == SepharothPhase.InvisibleAndMine || venomancerPhase == VenomancerPhase.ToxicPool)
                    {
                        // Selected second mine' position
                        selectedIndexNum = Random.Range(0, currentRoom.spawnPositionArray.Length);
                        Vector3Int selectedFirstSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[selectedIndexNum].x, 
                            currentRoom.spawnPositionArray[selectedIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedFirstSpawnPoint);
                    }
                    break;
                case 3:
                    if (sepharothPhase == SepharothPhase.InvisibleAndMine || venomancerPhase == VenomancerPhase.ToxicPool)
                    {
                        // Selected thir mine' position
                        selectAgain:
                        int selectedSecondIndexNum = Random.Range(0, currentRoom.spawnPositionArray.Length);

                        if (selectedSecondIndexNum == selectedIndexNum) goto selectAgain;

                        Vector3Int selectedSecondSpawnPoint = new Vector3Int(currentRoom.spawnPositionArray[selectedSecondIndexNum].x, 
                            currentRoom.spawnPositionArray[selectedSecondIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedSecondSpawnPoint);
                    }
                    break;
                default:
                    break;
            }

            float projectileSpeed = currentProjectile.projectileSpeed;

            // Get random speed value
            if (moravellePhase == MoravellePhase.SpreadArrowShot)
            {
                projectileSpeed = currentProjectile.projectileSpeed / 1.5f;
            }
            else if (galvanusPhase == GalvanusPhase.Lightning || frostWrymPhase == CryotharPhase.Icicle || venomancerPhase == VenomancerPhase.StoneRain ||
                fireWrymPhase == PyrotharPhase.FirePillar || moldranPhase == MoldranPhase.Spike || venomancerPhase == VenomancerPhase.ToxicPool)
            {
                projectileSpeed = 0;
            }

            // Get Gameobject with IFireable component
            IFireable projectile;

            if (galvanusPhase == GalvanusPhase.Lightning || frostWrymPhase == CryotharPhase.Icicle || venomancerPhase == VenomancerPhase.StoneRain || 
                fireWrymPhase == PyrotharPhase.FirePillar || moldranPhase == MoldranPhase.Spike)
            {
                // Get room bounds from template bounds
                Vector2Int lowerBounds = currentRoom.templateLowerBounds;
                Vector2Int upperBounds = currentRoom.templateUpperBounds;

                Vector3Int playerCellPosition = new Vector3Int();

                // Get the player's current cell position
                if (GameManager.Instance.GetPlayer() != null)
                {
                    playerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(GameManager.Instance.GetPlayer().transform.position);
                }

                // Generate a random lightning strike position within bounds
                Vector3Int randomCellPosition;
                Vector3 worldPosition;

                int attemptCount = 0;

                do
                {
                    attemptCount++;

                    // Generate random position within 4 tiles around the player
                    int randomX = Mathf.Clamp(Random.Range(playerCellPosition.x - 4, playerCellPosition.x + 5), lowerBounds.x, upperBounds.x);
                    int randomY = Mathf.Clamp(Random.Range(playerCellPosition.y - 4, playerCellPosition.y + 5), lowerBounds.y, upperBounds.y);
                    randomCellPosition = new Vector3Int(randomX, randomY, 0);

                    // Convert to room-local zero-based coordinates
                    Vector3Int zeroBasedCellPosition = new Vector3Int(randomCellPosition.x - lowerBounds.x, randomCellPosition.y - lowerBounds.y, 0);

                    // Tile is valid, convert cell position to world position
                    worldPosition = currentRoom.instantiatedRoom.grid.CellToWorld(randomCellPosition);

                    if (GameManager.Instance.GetPlayer() != null)
                    {
                        projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, GameManager.Instance.GetPlayer().transform.position, Quaternion.identity);
                    }
                    else
                    {
                        projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, transform.position, Quaternion.identity);
                    }

                    break;

                    //// Validate position using penalty system
                    //if (currentRoom.instantiatedRoom.GetRoomTilePenaltyValue(zeroBasedCellPosition) == 1)
                    //{
                    //    // Tile is valid, convert cell position to world position
                    //    worldPosition = currentRoom.instantiatedRoom.grid.CellToWorld(randomCellPosition);

                    //    projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, GameManager.Instance.GetPlayer().transform.position, Quaternion.identity);
                    //    break;
                    //}
                    //else if (attemptCount > 5)
                    //{
                    //    break; // Position validation is unsuccessful. Cast failed.
                    //}
                }
                while (true);
            }
            else
            {
                if (sepharothPhase == SepharothPhase.InvisibleAndMine || venomancerPhase == VenomancerPhase.ToxicPool)
                {
                    projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, projectileSpawnPoint, Quaternion.identity);
                }
                else if (grappleDetails != null || iceBreakerDetails != null)
                {
                    projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, player.GetPlayerPosition(), Quaternion.identity, transform);
                }
                else if (throwingAxeDetails != null || shirukenDetails != null)
                {
                    projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, player.GetPlayerPosition() + new Vector3(0f, 0.6f, 0f), 
                        Quaternion.identity, transform);
                }
                else
                {
                    projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, activeWeapon.GetMainHandShootPositionUp(), Quaternion.identity);
                }
            }

            if (isLaser)
            {
                Projectile spawnedProjectile = (Projectile)projectile;

                spawnedProjectile.lockedTargetVector = weaponAimDirectionVector;
                spawnedProjectile.lockedAngle = aimAngle;
            }

            // Initialize projectile
            projectile.InitializeProjectile(belongingEnemy, isIceBreaker, currentProjectile, aimAngle, weaponAimAngle, projectileSpeed, weaponAimDirectionVector, false, 
                false, isPenetrationArrow, projectileCounter - 1, projectilePerShot, moravellePhase, treantPhase, galvanusPhase, sepharothPhase, frostWrymPhase, 
                venomancerPhase, fireWrymPhase, moldranPhase, isTripleThreat, isBindingArrow, isArrowOfTheSeven, grappleDetails, iceBreakerDetails, 
                isFireBlast, fireBlastDetails, isBlazingCyclone, blazingCycloneDetails, isThrowingAxe, throwingAxeDetails, isShiruken, shirukenDetails,
                isChainLightning, chainLightningDetails, chainLightningPhase);

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        bool isShotSpecialSkill = isIceBreaker || isFireBlast || isPenetrationArrow || isBindingArrow || isArrowOfTheSeven || grappleDetails != null || isBlazingCyclone || 
            isThrowingAxe || isShiruken || isChainLightning;

        // Set weapon's onCooldown status to true for triggering Weapon status UI
        activeWeapon.GetCurrentMainHandWeapon().onCooldown = !isLaser && !isShotSpecialSkill ? true : false;

        // Call weapon fired event
        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentMainHandWeapon(), true);

        // Display weapon shoot effect
        WeaponShootEffect(aimAngle);

        bool isGrapple = grappleDetails != null; // Don't play arrow sound if projectile is grapple

        // Weapon fired sound effect
        WeaponSoundEffect(isActiveItem, isGrapple, isPenetrationArrow, isIceBreaker, isFireBlast, isBlazingCyclone, isChainLightning);

        if (enemy != null)
        {
            enemy.isFiring = false;
        }

        isFiringCoroutineRunning = false;
    }

    /// <summary>
    /// Reset cooldown timer
    /// </summary>
    private void ResetCooldownTimer(MoravellePhase moravellePhase = MoravellePhase.None)
    {
        float coolDownTimerModifier = 1f;

        if (moravellePhase == MoravellePhase.SpreadArrowShot)
        {
            coolDownTimerModifier = 3f;
        }

        // Reset cooldown timer
        if (player != null)
        {
            player.activeWeapon.GetCurrentMainHandWeapon().onCooldown = false;

            if (!player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
            {
                fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCooldownDuration *
                    (1 - player.additionalAttackCoolDownModifier);
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
    private void WeaponSoundEffect(bool isActiveItem, bool isGrapple, bool isPenetrationArrow, bool isIceBreaker, bool isFireBlast, bool isBlazingCyclone, bool isChainLightning)
    {
        if (isActiveItem) return;

        if (isGrapple || isPenetrationArrow) return;

        if (enemy != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect != null)
            {
                SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
            }
        }
        else
        {
            if (isIceBreaker) 
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.iceBreakerDetails.projectileFireSoundEffect);
                return;
            }
            else if (isFireBlast)
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.fireBlastDetails.projectileFireSoundEffect);
                return;
            }
            else if (isBlazingCyclone)
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.blazingCycloneDetails.projectileFireSoundEffect);
                return;
            }
            else if (isChainLightning)
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.chainLightningDetails.projectileFireSoundEffect);
                return;
            }

            if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect != null &&
                GetComponent<PlayerControl>().isSoundPlayed == false)
            {
                GetComponent<PlayerControl>().isSoundPlayed = true;
                SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
            }
        }
    }
}
