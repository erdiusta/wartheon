using Mirror;
using System.Collections;
using UnityEngine;
using Random = UnityEngine.Random;

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
    InstantiatedRoom instantiatedRoom;
    float firePrechargeTimer = 0f;
    ActiveWeapon activeWeapon;
    FireWeaponEvent fireWeaponEvent;
    WeaponFiredEvent weaponFiredEvent;
    bool isFiringCoroutineRunning = false;

    Room currentRoom;
    RoomNetData currentRoomNetData;

    WeaponDetailsSO currentWeaponDetails;

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

        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        fireWeaponEvent.OnFireWeapon -= FireWeaponEvent_OnFireWeapon;

        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs args)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            currentRoom = args.room;
            instantiatedRoom = currentRoom.instantiatedRoom;
        }
        else if(NetworkServer.active)
        {
            currentRoomNetData = args.roomNetData;
            instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId);
        }
    }

    private void Update()
    {
        // Decrease cooldown timer.
        fireRateCooldownTimer -= Time.deltaTime;

        if (player != null)
        {
            if (activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (fireRateCooldownTimer < 0 && activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown && !activeWeapon.GetCurrentMainHandWeapon().weaponStats.isMeleeWeapon)
                {
                    activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown = false;
                }
            }
        }
        else if (enemy != null && enemy.initializationCompleted)
        {
            if (activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (fireRateCooldownTimer < 0 && activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown)
                {
                    activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown = false;
                }
            }
        }
    }

    /// <summary>
    /// Handle fire weapon event.
    /// </summary>
    private void FireWeaponEvent_OnFireWeapon(FireWeaponEvent fireWeaponEvent, FireWeaponEventArgs args)
    {
        ClientFireWeapon(args);
    }

    public void ClientFireWeapon(FireWeaponEventArgs args)
    {
        Vector3 shootPos = activeWeapon.GetMainHandShootPositionUp();

        // Single player
        if (!NetworkServer.active && !NetworkClient.active)
        {
            WeaponFire(args.fire, args.firePreviousFrame, args.aimAngle, args.weaponAimAngle, args.weaponAimDirectionVector, args.isLaser, args.projectileKind, args.attackContext,
                activeWeapon.GetMainHandShootPositionUp(), args.enemyNetId);
            return;
        }

        // Client - ask server
        if (player != null)
        {
            GetComponent<FireWeaponNetwork>().RequestFireWeapon(args.fire, args.firePreviousFrame, args.aimAngle, args.weaponAimAngle, args.weaponAimDirectionVector, args.isLaser, args.projectileKind, 
                args.attackContext, shootPos);
        }
        else if (enemy != null)
        {
            ServerFireWeapon(args.fire, args.firePreviousFrame, args.aimAngle, args.weaponAimAngle, args.weaponAimDirectionVector, args.isLaser, args.projectileKind, args.attackContext, shootPos);
        }
    }

    [Server]
    public void ServerFireWeapon(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, AttackContext attackContext, Vector3 shootPos)
    {
        WeaponFire(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, attackContext, shootPos);
    }

    /// <summary>
    /// Fire weapon
    /// </summary>
    private void WeaponFire(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, AttackContext attackContext, Vector3 shootPos, uint netId = 0)
    {
        // Laser beam check
        if (isLaser)
        {
            // Handle laser beam as a continuousattack
            if (tag == Settings.enemyTag)
            {
                if (!enemy.initializationCompleted) return;

                // Ensure laser fires only once
                if (!enemy.isFiring)
                {
                    enemy.isFiring = true;

                    // Fire Laser Beam Projectile (only once)
                    FireProjectile(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, attackContext, netId, shootPos);

                    // Keep laser active for its full duration
                    StartCoroutine(LaserDurationCoroutine(false));                 
                }

                return; // Prevent further processing for standard projectiles
            }
            else if (tag == Settings.playerTag)
            {
                if (projectileKind == ProjectileKind.Grapple && player.isHuntersReachActive)
                {
                    // Fire Laser Beam Projectile (only once)
                    FireProjectile(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, attackContext, netId, shootPos);

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
        WeaponPrecharge(firePreviousFrame);

        // Weapon fire
        if (fire)
        {
            if (attackContext.sepharothPhase == SepharothPhase.InvisibleAndMine)
            {
                firePrechargeTimer = -3f; // Prevent charger issue for mine
            }

            // Test if weapon is ready to fire
            if (IsWeaponReadyToFire() || IsShotASpecialSkill(projectileKind, attackContext))
            {
                FireProjectile(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser: false, projectileKind, attackContext, netId, shootPos);
                ResetCooldownTimer(attackContext.moravellePhase);
                ResetPrechargeTimer(firePreviousFrame);
            }
        }
    }

    /// <summary>
    /// Handle weapon precharge
    /// </summary>
    private void WeaponPrecharge(bool firePreviousFrame)
    {
        if (player == null) return;

        if (fireRateCooldownTimer <= 0f)
        {
            // Weapon precharge 
            if (firePreviousFrame)
            {
                if (!prechargeBarContainer.gameObject.activeSelf)
                {
                    // Activate precharge bar container
                    if (player != null)
                    {
                        firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponPrechargeTime * player.additionalCastDurationModifier;
                    }
                    else
                    {
                        firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponPrechargeTime;
                    }

                    prechargeBarContainer.gameObject.SetActive(true);
                }

                // Set precharging flag to true
                activeWeapon.GetCurrentMainHandWeapon().weaponStats.onPrecharge = true;

                // Decrease precharge timer if fire button held previous frame
                firePrechargeTimer -= Time.deltaTime;

                // Update precharge bar
                float barFill = firePrechargeTimer / (activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponPrechargeTime * player.additionalCastDurationModifier);

                // Update bar fill
                prechargeBar.transform.localScale = barFill > 0 ? new Vector3(barFill, 1f, 1f) : new Vector3(0f, 1f, 1f);
            }
            // If precharge stops prematurely
            else if (!firePreviousFrame && firePrechargeTimer > 0f)
            {
                ResetPrechargeTimer(firePreviousFrame);

                if (activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    activeWeapon.GetCurrentMainHandWeapon().weaponStats.firingStoppedPrematurelyIfWeaponIsPrecharged = true;
                    activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown = false;
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
            activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown = true;
            enemy.isFiring = false; // Allow firing again only after the laser ends
        }
    }

    /// <summary>
    /// Set up ammo using an ammo gameObject and component from the object pool.
    /// </summary>
    private void FireProjectile(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, AttackContext ctx, uint netId, Vector3 shootPos, Enemy belongingEnemy = null)
    {
        ProjectileDetailsSO currentProjectile;

        if (ctx.galvanusPhase == GalvanusPhase.LightningBolt || ctx.sepharothPhase == SepharothPhase.InvisibleAndMine || ctx.cryotharPhase == CryotharPhase.Icicle ||
            ctx.venomancerPhase == VenomancerPhase.StoneRain || ctx.pyrotharPhase == PyrotharPhase.FirePillar || ctx.moldranPhase == MoldranPhase.Spike)
        {
            WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);
            currentProjectile = weaponDetails.weaponSecondaryProjectile;
        }
        else
        {
            currentProjectile = ResolveProjectile(projectileKind);
        }

        if (currentProjectile != null && !isFiringCoroutineRunning)
        {
            // Fire projectile routine
            StartCoroutine(FireProjectileRoutine(fire, firePreviousFrame, aimAngle, weaponAimAngle, weaponAimDirectionVector, isLaser, projectileKind, currentProjectile, ctx, netId, shootPos, belongingEnemy));
        }
    }

    /// <summary>
    /// Coroutine to spawn multiple ammo per shot if specified in the projectile details - PROJECTILE
    /// </summary>
    IEnumerator FireProjectileRoutine(bool fire, bool firePreviousFrame, float aimAngle, float weaponAimAngle, Vector3 weaponAimDirectionVector, bool isLaser,
        ProjectileKind projectileKind, ProjectileDetailsSO currentProjectile, AttackContext ctx, uint netId, Vector3 shootPos, Enemy belongingEnemy = null)
    {
        // MP client must never spawn projectiles
        if (NetworkClient.active && !NetworkServer.active) yield break;

        int projectileCounter = 0;

        int projectilePerShot = 1;

        // MORAVELLE - SPREAD ARROW SHOT OR FROST WRYM - PROJECTILE
        if (ctx.moravellePhase == MoravellePhase.SpreadArrowShot || ctx.cryotharPhase == CryotharPhase.IceProjectile
            || ctx.pyrotharPhase == PyrotharPhase.FireProjectile)
        {
            projectilePerShot = 10;
        }
        else if (ctx.moldranPhase == MoldranPhase.Projectile)
        {
            projectilePerShot = 15;
        }
        // SYLVAROK - RAZOR LEAF
        else if (ctx.sylvarokPhase == SylvarokPhase.RazorLeaf || ctx.venomancerPhase == VenomancerPhase.SludgeThrow)
        {
            projectilePerShot = 30;
        }
        // GALVANUS - LIGHTNING
        else if (ctx.galvanusPhase == GalvanusPhase.Lightning)
        {
            projectilePerShot = 2;
        }
        // VENOMANCER - STONE RAIN
        else if (ctx.cryotharPhase == CryotharPhase.Icicle || ctx.venomancerPhase == VenomancerPhase.StoneRain || ctx.pyrotharPhase == PyrotharPhase.FirePillar ||
            ctx.moldranPhase == MoldranPhase.Spike)
        {
            projectilePerShot = 3;
        }
        // SEPHAROTH - LASER
        else if (ctx.sepharothPhase == SepharothPhase.LaserBeam)
        {
            projectilePerShot = 2;
        }
        // SEPHAROTH - MINE
        else if (ctx.sepharothPhase == SepharothPhase.InvisibleAndMine || ctx.venomancerPhase == VenomancerPhase.ToxicPool)
        {
            projectilePerShot = 3;
        }
        // TRIPLE THREAT
        else if (ctx.isTripleThreat)
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
            if (ctx.moravellePhase == MoravellePhase.SpreadArrowShot || ctx.sylvarokPhase == SylvarokPhase.RazorLeaf || ctx.cryotharPhase == CryotharPhase.IceProjectile ||
                ctx.venomancerPhase == VenomancerPhase.SludgeThrow || ctx.pyrotharPhase == PyrotharPhase.FireProjectile || ctx.moldranPhase == MoldranPhase.Projectile
                || ctx.isTripleThreat)
            {
                projectileSpawnInterval = 0;
            }
            else if (ctx.galvanusPhase == GalvanusPhase.Lightning || ctx.cryotharPhase == CryotharPhase.Icicle || ctx.venomancerPhase == VenomancerPhase.StoneRain ||
                ctx.pyrotharPhase == PyrotharPhase.FirePillar || ctx.moldranPhase == MoldranPhase.Spike)
            {
                projectileSpawnInterval = 1f;
            }
            else if (ctx.venomancerPhase == VenomancerPhase.ToxicPool)
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
            if (projectileKind == ProjectileKind.IceBreaker)
            {
                weaponShootPosition = activeWeapon.GetMainHandShootPositionUp() - new Vector3(0f, 0.7f, 0f);
            }
            else
            {
                weaponShootPosition = activeWeapon.GetMainHandShootPositionUp();
            }
        }

        Grid grid;
        Vector2Int[] spawnPositionArray;
        Vector2Int lowerBounds;
        Vector2Int upperBounds;
        Player targetPlayer;

        if (!NetworkServer.active && !NetworkClient.active)
        {
            grid = enemy != null ? enemy.belongingRoom.instantiatedRoom.grid : instantiatedRoom.grid;
            spawnPositionArray = enemy != null ? enemy.belongingRoom.spawnPositionArray : currentRoom.spawnPositionArray;
            lowerBounds = enemy != null ? enemy.belongingRoom.templateLowerBounds : currentRoom.templateLowerBounds;
            upperBounds = enemy != null ? enemy.belongingRoom.templateLowerBounds : currentRoom.templateUpperBounds;

            if (belongingEnemy != null) targetPlayer = GameManager.Instance.GetLocalPlayer();
            else targetPlayer = null;
        }
        else if (NetworkServer.active)
        {
            grid = enemy != null ? DungeonRuntime.GetInstantiatedRoom(enemy.belongingRoomData.roomId).grid : DungeonRuntime.GetInstantiatedRoom(currentRoomNetData.roomId).grid;
            spawnPositionArray = enemy != null ? enemy.belongingRoomData.spawnPositions : currentRoomNetData.spawnPositions;
            lowerBounds = enemy != null ? enemy.belongingRoomData.templateLowerBounds : currentRoomNetData.templateLowerBounds;
            upperBounds = enemy != null ? enemy.belongingRoomData.templateUpperBounds : currentRoomNetData.templateUpperBounds;

            if (belongingEnemy != null) targetPlayer = GameManager.Instance.GetLocalPlayer();
            else targetPlayer = null;
        }
        else
        {
            grid = null;
            spawnPositionArray = null;
            lowerBounds = Vector2Int.zero;
            upperBounds = Vector2Int.zero;
            targetPlayer = null;
        }

        // Default position
        Vector3 projectileSpawnPoint = activeWeapon.GetMainHandShootPositionUp();

        // Loop for number of projectile per shot
        while (projectileCounter < projectilePerShot)
        {
            projectileCounter++;

            int selectedIndexNum = -1;

            GameObject projectilePrefab;
            // Get projectile prefab from array
            if (!NetworkServer.active && !NetworkClient.active) // SP
            {
                if (ctx.moravellePhase == MoravellePhase.SpreadArrowShot || ctx.venomancerPhase == VenomancerPhase.ToxicPool) projectilePrefab = currentProjectile.projectilePrefabArray[2];
                else projectilePrefab = currentProjectile.projectilePrefabArray[0];
            }
            else if (NetworkServer.active) // MP - Server Only
            {
                if (ctx.moravellePhase == MoravellePhase.SpreadArrowShot || ctx.venomancerPhase == VenomancerPhase.ToxicPool) projectilePrefab = currentProjectile.projectilePrefabArray[3];
                else projectilePrefab = currentProjectile.projectilePrefabArray[1];
            }
            else projectilePrefab = null;

            switch (projectileCounter)
            {
                case 1:
                    if (ctx.venomancerPhase == VenomancerPhase.ToxicPool)
                    {
                        // Selected second mine' position
                        selectedIndexNum = Random.Range(0, spawnPositionArray.Length);
                        Vector3Int selectedFirstSpawnPoint = new Vector3Int(spawnPositionArray[selectedIndexNum].x, spawnPositionArray[selectedIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedFirstSpawnPoint);
                    }
                    break;

                case 2:
                    if (ctx.sepharothPhase == SepharothPhase.LaserBeam)
                    {
                        aimAngle += 120;
                        weaponAimAngle += 120;
                    }
                    else if (ctx.sepharothPhase == SepharothPhase.InvisibleAndMine || ctx.venomancerPhase == VenomancerPhase.ToxicPool)
                    {
                        // Selected second mine' position
                        selectedIndexNum = Random.Range(0, spawnPositionArray.Length);
                        Vector3Int selectedFirstSpawnPoint = new Vector3Int(spawnPositionArray[selectedIndexNum].x, spawnPositionArray[selectedIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedFirstSpawnPoint);
                    }
                    break;
                case 3:
                    if (ctx.sepharothPhase == SepharothPhase.InvisibleAndMine || ctx.venomancerPhase == VenomancerPhase.ToxicPool)
                    {
                    // Selected thir mine' position
                    selectAgain:
                        int selectedSecondIndexNum = Random.Range(0, spawnPositionArray.Length);

                        if (selectedSecondIndexNum == selectedIndexNum) goto selectAgain;

                        Vector3Int selectedSecondSpawnPoint = new Vector3Int(spawnPositionArray[selectedSecondIndexNum].x, spawnPositionArray[selectedSecondIndexNum].y, 0);

                        // Convert the cell position to world position
                        projectileSpawnPoint = grid.CellToWorld(selectedSecondSpawnPoint);
                    }
                    break;
                default:
                    break;
            }

            float projectileSpeed = currentProjectile.projectileSpeed;

            // Get random speed value
            if (ctx.moravellePhase == MoravellePhase.SpreadArrowShot)
            {
                projectileSpeed = currentProjectile.projectileSpeed / 1.5f;
            }
            else if (ctx.galvanusPhase == GalvanusPhase.Lightning || ctx.cryotharPhase == CryotharPhase.Icicle || ctx.venomancerPhase == VenomancerPhase.StoneRain ||
                ctx.pyrotharPhase == PyrotharPhase.FirePillar || ctx.moldranPhase == MoldranPhase.Spike || ctx.venomancerPhase == VenomancerPhase.ToxicPool)
            {
                projectileSpeed = 0;
            }

            // Get Gameobject with IFireable component
            IFireable projectile;

            if (ctx.galvanusPhase == GalvanusPhase.Lightning || ctx.cryotharPhase == CryotharPhase.Icicle || ctx.venomancerPhase == VenomancerPhase.StoneRain ||
                ctx.pyrotharPhase == PyrotharPhase.FirePillar || ctx.moldranPhase == MoldranPhase.Spike)
            {
                Vector3Int playerCellPosition = new Vector3Int();

                // Get the player's current cell position
                if (targetPlayer != null)
                {
                    playerCellPosition = instantiatedRoom.grid.WorldToCell(GameManager.Instance.GetLocalPlayer().transform.position);
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
                    worldPosition = instantiatedRoom.grid.CellToWorld(randomCellPosition);

                    if (targetPlayer != null)
                    {
                        projectile = (IFireable)PoolManager.Instance.Reuse(projectilePrefab, GameManager.Instance.GetLocalPlayer().transform.position, Quaternion.identity);
                    }
                    else
                    {
                        projectile = (IFireable)PoolManager.Instance.Reuse(projectilePrefab, transform.position, Quaternion.identity);
                    }

                    break;
                }
                while (true);
            }
            else
            {
                if (ctx.sepharothPhase == SepharothPhase.InvisibleAndMine || ctx.venomancerPhase == VenomancerPhase.ToxicPool)
                {
                    projectile = (IFireable)PoolManager.Instance.Reuse(projectilePrefab, projectileSpawnPoint, Quaternion.identity);
                }
                else if (projectileKind == ProjectileKind.Grapple || projectileKind == ProjectileKind.IceBreaker)
                {
                    projectile = (IFireable)PoolManager.Instance.Reuse(projectilePrefab, player.GetPlayerPosition(), Quaternion.identity);
                }
                else if (projectileKind == ProjectileKind.ThrowingAxe || projectileKind == ProjectileKind.Shiruken)
                {
                    projectile = (IFireable)PoolManager.Instance.Reuse(projectilePrefab, player.GetPlayerPosition() + new Vector3(0f, 0.6f, 0f),Quaternion.identity);
                }
                else
                {
                    projectile = (IFireable)PoolManager.Instance.Reuse(projectilePrefab, shootPos, Quaternion.identity);
                }
            }

            if (isLaser)
            {
                Projectile spawnedProjectile = (Projectile)projectile;

                spawnedProjectile.lockedTargetVector = weaponAimDirectionVector;
                spawnedProjectile.lockedAngle = aimAngle;
            }

            if (NetworkServer.active || NetworkClient.active)
            {
                if (player != null && player.NetAuth != null)
                {
                    netId = player.NetAuth.netId;
                }
                else if(enemy != null && enemy.enemyNetwork != null)
                {
                    netId = enemy.enemyNetwork.netId;
                }
            }

            // Initialize projectile - SP
            if (!NetworkServer.active && !NetworkClient.active)
            {
                projectile.InitializeProjectile(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, currentProjectile, ctx, overrideProjectileMovement: false,
                    fallingFromSkies: false, projectileCounter - 1, projectilePerShot, netId, -1, 0, belongingEnemy);
            }

            // Initialize projectile - MP
            if (NetworkServer.active)
            {
                int projectileIndex = WartheonDatabase.Instance.GetProjectileId(currentProjectile);

                if (projectile is Projectile)
                {
                    Projectile initializeProjectile = (Projectile)projectile;
                    initializeProjectile.GetComponent<ProjectileNetwork>().RpcInitializeProjectile(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, ctx,
                        false, false, projectileCounter - 1, projectilePerShot, netId, projectileIndex, netId);
                }
                else if (projectile is ProjectilePattern)
                {
                    ProjectilePattern initializeProjectilePattern = (ProjectilePattern)projectile;

                    initializeProjectilePattern.GetComponent<ProjectilePatternNetwork>().RpcInitializeProjectilePattern(aimAngle, weaponAimAngle, weaponAimDirectionVector, projectileSpeed, projectileKind, ctx,
                        false, false, projectileCounter - 1, projectilePerShot, netId, projectileIndex, netId);
                }
            }

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        // Set weapon's onCooldown status to true for triggering Weapon status UI
        activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown = !isLaser && !IsShotASpecialSkill(projectileKind, ctx);

        // Call weapon fired event
        weaponFiredEvent.CallWeaponFiredEvent(activeWeapon.GetCurrentMainHandWeapon(), true);

        //// Display weapon shoot effect
        //WeaponShootEffect(aimAngle);

        bool isGrapple = projectileKind == ProjectileKind.Grapple;

        // Weapon fired sound effect
        WeaponSoundEffect(isGrapple, projectileKind, ctx);

        if (enemy != null)
        {
            enemy.isFiring = false;
        }

        isFiringCoroutineRunning = false;
    }

    private static bool IsShotASpecialSkill(ProjectileKind projectileKind, AttackContext ctx)
    {
        bool isShotSpecialSkill = false;

        switch (projectileKind)
        {
            case ProjectileKind.Default:
                break;
            case ProjectileKind.Grapple:
            case ProjectileKind.IceBreaker:
            case ProjectileKind.FireBlast:
            case ProjectileKind.BlazingCyclone:
            case ProjectileKind.ThrowingAxe:
            case ProjectileKind.Shiruken:
            case ProjectileKind.ChainLightning:
                isShotSpecialSkill = true;
                break;
            default:
                break;
        }

        if (ctx.isPenetrationArrow || ctx.isBindingArrow || ctx.isTripleThreat) isShotSpecialSkill = true;
        return isShotSpecialSkill;
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
            player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.onCooldown = false;

            if (!player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.isMeleeWeapon)
            {
                fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponCooldownDuration *
                    (1 - player.additionalAttackCoolDownModifier);
            }
            else
            {
                fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponCooldownDuration * coolDownTimerModifier;
            }
        }
        else
        {
            fireRateCooldownTimer = activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponCooldownDuration * coolDownTimerModifier;
        }
    }

    private ProjectileDetailsSO ResolveProjectile(ProjectileKind kind)
    {
        Weapon weapon = activeWeapon.GetCurrentMainHandWeapon();
        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

        return kind switch
        {
            ProjectileKind.Grapple => player.playerDetails.grappleDetails,
            ProjectileKind.IceBreaker => player.playerDetails.iceBreakerDetails,
            ProjectileKind.FireBlast => player.playerDetails.fireBlastDetails,
            ProjectileKind.BlazingCyclone => player.playerDetails.blazingCycloneDetails,
            ProjectileKind.ThrowingAxe => player.playerDetails.throwingAxeDetails,
            ProjectileKind.Shiruken => player.playerDetails.shirukenDetails,
            ProjectileKind.ChainLightning => player.playerDetails.chainLightningDetails,
            _ => weaponDetails.weaponCurrentProjectile
        };
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
                firePrechargeTimer = activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponPrechargeTime * player.additionalCastDurationModifier;

                // Set weapon's precharge flag to false
                activeWeapon.GetCurrentMainHandWeapon().weaponStats.onPrecharge = false;

                if (firePreviousFrame)
                {
                    activeWeapon.GetCurrentMainHandWeapon().weaponStats.firingCompletedIfWeaponIsPrecharged = true;
                }
            }
        }

        // Reset bar fill and disable the bar container
        prechargeBar.transform.localScale = new Vector3(1f, 1f, 1f);
        prechargeBarContainer.gameObject.SetActive(false);
    }

    /// <summary>
    /// Play weapon shooting sound effect
    /// </summary>
    private void WeaponSoundEffect(bool isGrapple, ProjectileKind kind, AttackContext ctx)
    {
        if (isGrapple || ctx.isPenetrationArrow) return;

        if (enemy != null)
        {
            //if (activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect != null)
            //{
            //    SoundEffectManager.Instance.PlaySoundEffect(activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponSwingSoundEffect);
            //}
        }
        else
        {
            WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponTitle);

            if (kind == ProjectileKind.IceBreaker) 
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.iceBreakerDetails.projectileFireSoundEffect);
                return;
            }
            else if (kind == ProjectileKind.FireBlast)
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.fireBlastDetails.projectileFireSoundEffect);
                return;
            }
            else if (kind == ProjectileKind.BlazingCyclone)
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.blazingCycloneDetails.projectileFireSoundEffect);
                return;
            }
            else if (kind == ProjectileKind.ChainLightning)
            {
                SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.chainLightningDetails.projectileFireSoundEffect);
                return;
            }

            if (weaponDetails.weaponSwingSoundEffect != null &&
                GetComponent<PlayerControl>().isSoundPlayed == false)
            {
                GetComponent<PlayerControl>().isSoundPlayed = true;
                SoundEffectManager.Instance.PlaySoundEffect(weaponDetails.weaponSwingSoundEffect);
            }
        }
    }
}
