using System;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    #region Tooltip
    [Tooltip("MovementDetailsSO scriptable object containing movement details such as speed")]
    #endregion Tooltip
    [SerializeField] private MovementDetailsSO movementDetails;

    Player player;
    bool leftMouseDownPreviousFrame = false;
    int currentWeaponIndex = 1;
    float moveSpeed;
    bool isPlayerMovementDisabled = false;

    private void Awake()
    {
        player = GetComponent<Player>();

        moveSpeed = movementDetails.GetMoveSpeed();
    }

    private void Start()
    {
        // Set starting weapon
        SetStartingWeapon();

        // Set player animation speed
        SetPlayerAnimationSpeed();
    }

    /// <summary>
    /// Set the player starting weapon
    /// </summary>
    private void SetStartingWeapon()
    {
        int index = 1;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon.weaponDetails == player.playerDetails.startingWeapon)
            {
                SetWeaponByIndex(index);
                break;
            }

            index++;
        }
    }

    /// <summary>
    /// Set player animator speed to match movement speed
    /// </summary>
    private void SetPlayerAnimationSpeed()
    {
        // Set animator speed to match movement speed
        player.animator.speed = moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Update()
    {
        // If player movement disabled then return
        if (isPlayerMovementDisabled)
            return;

        // Process the player movement input
        MovementInput();

        // Process the player weapon input
        WeaponInput();
    }

    /// <summary>
    /// Player movement input
    /// </summary>
    private void MovementInput()
    {
        // Get movement input
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");
        bool rightMouseButtonDown = Input.GetMouseButtonDown(1);

        // Create a direction vector based on the input
        Vector2 direction = new Vector2(horizontalMovement, verticalMovement);

        // Adjust distance for diagonal movement (pythagoras approximation)
        if (horizontalMovement != 0f && verticalMovement != 0f)
        {
            direction *= 0.7f;
        }

        // If there is movement
        if (direction != Vector2.zero)
        {
            // Trigger movement event
            player.movementByVelocityEvent.CallMovementByVelocityEvent(direction, moveSpeed);
        }
        // Else trigger idle event
        else
        {
            player.idleEvent.CallIdleEvent();
        }
    }

    /// <summary>
    /// Weapon Input
    /// </summary>
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        // Fire weapon input
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Switch weapon input
        SwitchWeaponInput();

        // Reload weapon input
        ReloadWeaponInput();
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection)
    {
        // Get mouse world position
        Vector3 mouseWorldPosition = HelperUtilities.GetMouseWorldPosition();

        // Calculate direction vector of mouse cursor from weapon shoot position
        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetShootPosition());

        // Calculate direction vector of mouse cursor from player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        // Get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get player to cursor angle
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // Set player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        // Trigger weapon aim event
        player.aimWeaponEvent.CallAimWeaponEvent(playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        // Fire when left mouse button is clicked
        if (Input.GetMouseButton(0))
        {
            // Trigger fire weapon event
            player.fireWeaponEvent.CallFireWeaponEvent(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees, 
                weaponAngleDegrees, weaponDirection);
            leftMouseDownPreviousFrame = true;
        }
        else
        {
            leftMouseDownPreviousFrame = false;
        }
    }

    private void SwitchWeaponInput()
    {
        // Switch weapon if mouse scroll wheel selecetd
        if (Input.mouseScrollDelta.y < 0f)
        {
            PreviousWeapon();
        }

        if (Input.mouseScrollDelta.y > 0f)
        {
            NextWeapon();
        }

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetWeaponByIndex(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetWeaponByIndex(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetWeaponByIndex(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetWeaponByIndex(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetWeaponByIndex(5);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetWeaponByIndex(6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetWeaponByIndex(7);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SetWeaponByIndex(8);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SetWeaponByIndex(9);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetWeaponByIndex(10);
        }

        if (Input.GetKeyDown(KeyCode.Minus))
        {
            SetCurrentWeaponToFirstInTheList();
        }
    }

    private void NextWeapon()
    {
        currentWeaponIndex++;

        if (currentWeaponIndex > player.weaponList.Count)
        {
            currentWeaponIndex = 1;
        }

        SetWeaponByIndex(currentWeaponIndex);

    }

    private void PreviousWeapon()
    {
        currentWeaponIndex--;

        if (currentWeaponIndex < 1)
        {
            currentWeaponIndex = player.weaponList.Count;
        }

        SetWeaponByIndex(currentWeaponIndex);
    }

    private void SetWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex - 1 < player.weaponList.Count)
        {
            currentWeaponIndex = weaponIndex;

            if (player.weaponList[weaponIndex - 1].weaponDetails.isMeleeWeapon)
            {
                player.setActiveWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[weaponIndex - 1], 
                    player.weaponList[weaponIndex - 1].weaponDetails.weaponAnimatorController);
            }
            else
            {
                player.setActiveWeaponEvent.CallSetActiveWeaponEvent(player.weaponList[weaponIndex - 1], null);
            }
        }
    }

    private void ReloadWeaponInput()
    {
        Weapon currentWeapon = player.activeWeapon.GetCurrentWeapon();

        // If current weapon is reloading return
        if (currentWeapon.isWeaponReloading) 
            return;

        // If remaining projectile is less than clip capacity then return and not infinite projectile then return
        if (currentWeapon.weaponRemainingProjectile < currentWeapon.weaponDetails.weaponClipProjectileCapacity && 
            !currentWeapon.weaponDetails.hasInfiniteProjectile) 
            return;

        // if projectile in clip equals clip capacity then return
        if (currentWeapon.weaponClipRemainingProjectile == currentWeapon.weaponDetails.weaponClipProjectileCapacity) 
            return;

        if (Input.GetKeyDown(KeyCode.R))
        {
            // Call the reload weapon event
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentWeapon(), 0);
        }
<<<<<<< Updated upstream
=======

        // Highlight the current button
        Button highlightedButton = buttonContainer.GetChild(player.currentWeaponSlotSetIndex - 1).GetComponent<Button>();
        ColorBlock highlightedCb = highlightedButton.colors;
        highlightedButton.image.color = highlightedCb.highlightedColor;
    }

    /// <summary>
    /// Stun routine
    /// </summary>
    IEnumerator StunRoutine()
    {
        player.movementByVelocity.moveSpeed = 0f;

        yield return new WaitForSeconds(3f);

        player.moveStatus = MoveStatus.Idle;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallStunCuredEvent();
        player.animator.SetBool(Settings.isStunned, false);
        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.GetMoveSpeed();
    }

    /// <summary>
    /// Use special move of the selected character
    /// </summary>
    private void SpecialMoveInput()
    {
        if (InputManager.Instance.specialMoveOne.action.WasPressedThisFrame() && !player.specialMoveOneOnCooldown)
        {
            switch (player.playerDetails.playerCharacterIndex)
            {
                case Character.Astraeus:
                    SeismicSlam();
                    player.specialMoveOneOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(1);
                    break;

                case Character.Orion:
                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow)
                    {
                        HeadShot();
                        player.specialMoveOneOnCooldown = true;
                        player.specialMoveEvent.CallSpecialMoveUsedEvent(1);
                    }
                    break;

                case Character.Erebus:
                    if (!player.onStealth)
                    {
                        Stealth();
                        player.specialMoveEvent.CallSpecialMoveUsedEvent(1, true);
                    }
                    break;

                case Character.Lyrisa:
                    Teleport();
                    player.specialMoveOneOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(1);
                    break;

                default:
                    break;
            }
        }

        if (InputManager.Instance.specialMoveTwo.action.WasPressedThisFrame() && !player.specialMoveTwoOnCooldown)
        {
            switch (player.playerDetails.playerCharacterIndex)
            {
                case Character.Astraeus:
                    Block();
                    player.specialMoveTwoOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(2);
                    break;
                case Character.Erebus:
                    BloodDrain();
                    player.specialMoveTwoOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(2);
                    break;
                case Character.Orion:
                    LightFeet();
                    player.specialMoveTwoOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(2);
                    break;
                case Character.Lyrisa:
                    ForceField();
                    player.specialMoveTwoOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(2);
                    break;
                default:
                    break;
            }
        }
        if (InputManager.Instance.specialMoveThree.action.WasPressedThisFrame() && !player.specialMoveThreeOnCooldown)
        {
            switch (player.playerDetails.playerCharacterIndex)
            {
                case Character.Astraeus:
                    GemSkin();
                    player.specialMoveThreeOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(3);
                    break;
                case Character.Erebus:
                    DoubleTeam();
                    player.specialMoveThreeOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(3);
                    break;
                case Character.Orion:
                    Penetrate();
                    player.specialMoveThreeOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(3);
                    break;
                case Character.Lyrisa:
                    Cataclysm();
                    player.specialMoveThreeOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent(3);
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Execute Teleport special move
    /// </summary>
    private void Teleport()
    {
        if (player.specialMoveOneOnCooldown == false)
        {
            // Start playing teleport particle system
            if (teleportParticleRoutine != null)
            {
                StopCoroutine(teleportParticleRoutine);
            }
            teleportParticleRoutine = StartCoroutine(ParticleSystemRoutine());

            // Wait for mouse click to teleport the character
            InputManager.Instance.pointerPosition.action.performed += OnTeleportInput;

            // Play special move sound effect
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveOneSoundEffect);
        }
    }

    /// <summary>
    /// Execute Cataclysm special move
    /// </summary>
    private void Cataclysm()
    {
        // Get mouse world position and teleport the character
        Vector3 pointerWorldPosition = HelperUtilities.GetMouseWorldPosition();

        // Locate the position where meteor starts to fall
        Vector3 meteorStartsToFallPosition = pointerWorldPosition + new Vector3(0f, 10f, 0f);

        // Calculate direction vector of mouse cursor from fall position
        Vector3 direction = (pointerWorldPosition - meteorStartsToFallPosition);

        // Calculate angle based on the vector
        float angle = HelperUtilities.GetAngleFromVector(direction);

        StartCoroutine(FireCataclysmMeteorRoutine(player.playerDetails.cataclysmMeteor, angle, angle, direction, meteorStartsToFallPosition));
    }

    private void OnTeleportInput(InputAction.CallbackContext context)
    {
        if (particlePlayed)
        {
            // Get current room and its bounds
            Room room = GameManager.Instance.GetCurrentRoom();

            // Get mouse world position and teleport the character
            Vector3 pointerWorldPosition = HelperUtilities.GetMouseWorldPosition();
            Vector3Int pointerCellPosition = room.instantiatedRoom.grid.WorldToCell(pointerWorldPosition);

            // Check if the clicked tile is not marked as an obstacle
            if (!IsObstacleTile(room, pointerCellPosition))
            {
                // Teleport the character to the clicked tile
                transform.position = pointerWorldPosition;

                // Stop playing teleport particle system
                player.specialMoveParticlesSystem.Stop();
                particlePlayed = false;

                // Unsubscribe from the event to prevent multiple teleports
                InputManager.Instance.pointerPosition.action.performed -= OnTeleportInput;
            }
        }
    }

    /// <summary>
    /// Coroutine to spawn multiple ammo per shot if specified in the ammo details - PROJECTILE
    /// </summary>
    IEnumerator FireCataclysmMeteorRoutine(ProjectileDetailsSO currentProjectile, float aimAngle, float weaponAimAngle,
        Vector3 direction, Vector3 meteorStartsToFallPosition)
    {
        int projectileCounter = 0;

        // Get random projectile per shot
        int projectilePerShot = Random.Range(currentProjectile.projectileSpawnAmountMin, currentProjectile.projectileSpawnAmountMax + 1);

        // Get random interval between projectile
        float projectileSpawnInterval;

        projectileSpawnInterval = Random.Range(currentProjectile.projectileSpawnIntervalMin, currentProjectile.projectileSpawnIntervalMax);

        // Loop for number of projectile per shot
        while (projectileCounter < projectilePerShot)
        {
            projectileCounter++;

            // Get projectile prefab from array
            GameObject projectilePrefab = currentProjectile.projectilePrefabArray[Random.Range(0, currentProjectile.projectilePrefabArray.Length)];

            // Get random speed value
            float projectileSpeed = Random.Range(currentProjectile.projectileSpeedMin, currentProjectile.projectileSpeedMax);

            // Get Gameobject with IFireable component
            IFireable projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, meteorStartsToFallPosition, Quaternion.identity);

            // Initialize projectile
            projectile.InitializeProjectile(false, currentProjectile, aimAngle, weaponAimAngle, projectileSpeed, direction, false, true);

            Projectile meteor = (Projectile)projectile;
            meteor.GetComponentInChildren<SpriteRenderer>().transform.eulerAngles = Vector3.zero;

            // Wait for projectile per shot timegap
            yield return new WaitForSeconds(projectileSpawnInterval);
        }

        //// Set weapon's onCooldown status to true for triggering Weapon status UI
        //if (!activeWeapon.GetCurrentMainHandWeapon().onPrecharge)
        //{
        //    activeWeapon.GetCurrentMainHandWeapon().onCooldown = true;
        //}
    }

    IEnumerator ParticleSystemRoutine()
    {
        player.specialMoveParticlesSystem.Play();

        yield return new WaitForSeconds(0.3f);

        particlePlayed = true;
    }

    private bool IsObstacleTile(Room room, Vector3Int cellPosition)
    {
        // Convert cell position to adjusted position relative to room bounds
        Vector2Int adjustedCellPosition = new Vector2Int(cellPosition.x - room.templateLowerBounds.x, 
            cellPosition.y - room.templateLowerBounds.y);

        // Check if the adjusted cell position is within the valid range
        if (adjustedCellPosition.x < 0 || adjustedCellPosition.y < 0 || adjustedCellPosition.x >= room.instantiatedRoom.
            aStarMovementPenalty.GetLength(0) || adjustedCellPosition.y >= room.instantiatedRoom.aStarMovementPenalty.GetLength(1))
        {
            // Cell position is outside the valid range (out of bounds)
            return true; // Treat it as an obstacle
        }

        // Check if the cell is marked as an obstacle
        return room.instantiatedRoom.aStarMovementPenalty[adjustedCellPosition.x, adjustedCellPosition.y] == 0;
    }

    /// <summary>
    /// Execute Stealth special move
    /// </summary>
    private void Stealth()
    {
        // Set player's stealth status to true
        player.onStealth = true;

        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveOneSoundEffect);

        // Set the alpha value to 0.3 (30% opacity)
        currentColor.a = 0.3f;

        // Apply the modified color back to the sprite renderer
        player.spriteRenderer.color = currentColor;

        // Start the coroutine to maintain the alpha value during stealth
        StartCoroutine(MaintainStealthAlpha());
    }

    IEnumerator MaintainStealthAlpha()
    {
        while (player.onStealth)
        {
            // Get the current color of the sprite renderer
            Color currentColor = player.spriteRenderer.color;

            // Ensure the alpha value remains at 0.3
            currentColor.a = 0.3f;

            // Apply the modified color back to the sprite renderer
            player.spriteRenderer.color = currentColor;

            yield return null; // Wait for the next frame
        }
    }

    /// <summary>
    /// Unstealth from special move
    /// </summary>
    public void Unstealth()
    {
        //if (stealthStarted) return;

        if (unstealthRoutine != null) return;

        // Trigger cooldown and ui components
        player.specialMoveOneOnCooldown = true;
        player.specialMoveEvent.CallSpecialMoveUsedEvent(1);

        unstealthRoutine = StartCoroutine(UnstealthRoutine());
    }

    IEnumerator UnstealthRoutine()
    {
        // Set immunity
        player.health.isDamageable = false;

        // Set player's stealth status to false
        player.onStealth = false;

        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;

        // Set the alpha value back to 0.7 (70% opacity)
        currentColor.a = 0.7f;
        player.spriteRenderer.color = currentColor; 

        yield return new WaitForSeconds(unstealthImmunityTime);

        // Set the alpha value back to 1 (100% opacity)
        currentColor.a = 1f;
        player.spriteRenderer.color = currentColor;

        player.health.isDamageable = true;
        unstealthRoutine = null;
    }

    /// <summary>
    /// Execute Seismic Slam special move
    /// </summary>
    private void SeismicSlam()
    {
        // Get all colliders within the radius of the seismic slam
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, seismicSlamCircleRadius);

        if (player.specialMoveParticlesSystem != null)
        {
            player.specialMoveParticlesSystem.Play();
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveOneSoundEffect);
        }

        if (player.playerDetails.applyScreenShake)
        {
            StaticEventHandler.CallCameraShakeEvent(player.playerDetails.shakeIntensity, player.playerDetails.shakeDuration);
        }

        foreach (Collider2D col in colliders)
        {
            // Check if the collider belongs to an enemy or any other object you want to affect
            if (col.CompareTag(Settings.enemyTag))
            {
                // Apply damage to the enemy
                Enemy enemy = col.GetComponent<Enemy>();

                if (!enemy.enemyDetails.hasKnockbackResistance && enemy.health.currentHealth > 0)
                {
                    enemy.GetComponent<EnemyMovementAI>().TriggerKnockback((enemy.transform.position - transform.position).normalized);
                }

                if (enemy.health != null)
                {
                    enemy.health.TakeDamage(seismicSlamDamage, transform.position, enemy.health.transform.position, false);
                }
            }
        }
    }

    /// <summary>
    /// Execute Block special move
    /// </summary>
    private void Block()
    {
        if (player.specialMoveTwoDurationTimer < player.playerDetails.specialMoveTwoDuration)
        {
            player.isBlockingActive = true;
            player.healthEvent.CallGetBlockSpecialMoveEvent(); // This is for displaying shield icon
        }
    }

    /// <summary>
    /// Execute Gem Skin special move
    /// </summary>
    private void GemSkin()
    {
        if (player.specialMoveThreeDurationTimer < player.playerDetails.specialMoveThreeDuration)
        {
            player.isGemSkinActive = true;
            player.healthEvent.CallGetGemSkinSpecialMoveEvent(); // This is for displaying gem skin icon
            player.health.currentArmorValue += 5;
        }
    }

    /// <summary>
    /// Execute Blood Drain speical move
    /// </summary>
    private void BloodDrain()
    {
        player.meleeAttackRightHand.IsAttackingAtRightHand = true;
        meleeAttackTypeMainHand = MeleeAttackType.Thrust;
        player.meleeAttackEvent.CallMainHandWeaponAnimEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), meleeAttackTypeMainHand, true);
    }

    /// <summary>
    /// Execute Double Team speical move
    /// </summary>
    private void DoubleTeam()
    {
        if (!Player.hasClone)
        {
            player.playerCloneObject = Instantiate(player.playerDetails.playerClonePrefab, transform.position + new Vector3(0f, 2f, 0f), Quaternion.identity);
            player.playerCloneObject.GetComponent<Player>().Initialize(player.playerDetails);
            player.playerCloneObject.GetComponent<Health>().currentHealth = 1;
            player.playerCloneObject.GetComponent<SpriteRenderer>().color = new Color(1f, 1f, 1f, 0.4f);
            Player.hasClone = true;
        }
    }

    /// <summary>
    /// Execute Force Field special move
    /// </summary>
    private void ForceField()
    {
        GameObject forceFieldObject = player.forcefieldTransform.gameObject;
        forceFieldObject.SetActive(true);
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveTwoSoundEffect);
    }

    /// <summary>
    /// Execute Head Shot special move
    /// </summary>
    private void HeadShot()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        //Reset precharge for loading again
        fireCompletedDuringPressed = false;
        isSoundPlayed = false;

        player.meleeAttackRightHand.IsAttackingAtRightHand = true;
        player.meleeAttackEvent.CallMainHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), MeleeAttackType.None);

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, true);
    }

    /// <summary>
    /// Execute Light Feet special move
    /// </summary>
    private void LightFeet()
    {
        player.movementByVelocity.moveSpeed += 1.5f;
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveTwoSoundEffect);
    }

    /// <summary>
    /// Execute Penetrate special move
    /// </summary>
    private void Penetrate()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        //Reset precharge for loading again
        fireCompletedDuringPressed = false;
        isSoundPlayed = false;

        player.meleeAttackRightHand.IsAttackingAtRightHand = true;
        player.meleeAttackEvent.CallMainHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), MeleeAttackType.None);

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, true);
    }

    /// <summary>
    /// Use the nearest item within 2 unity units from the player
    /// </summary>
    private void UseItemInput()
    {
        float useItemRadius = 2f;

        // Get any 'Usable' item near the player
        Collider2D[] collider2DArray = Physics2D.OverlapCircleAll(player.GetPlayerPosition(), useItemRadius);

        // Loop through detected items to see if any are 'usable'
        foreach (Collider2D collider2D in collider2DArray)
        {
            IUsable iusable = collider2D.GetComponent<IUsable>();

            if (iusable != null)
            {
                // Chest collectible
                Chest chest = collider2D.GetComponent<Chest>();

                // Open chest with key process
                if (InputManager.Instance.interaction.action.IsPressed())
                {
                    if (chest.chestState == ChestState.closed && !chest.dropCompleted)
                    {
                        iusable.StartChestProcess();
                    }
                }

                // Try open with bobby pin process
                if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemName == "Bobby Pin" && chest.bobbyPinTried == false)
                {
                    if (InputManager.Instance.activeItem.action.IsPressed())
                    {
                        chest.bobbyPinTried = true;
                        int diceRoll = Random.Range(0, 100);

                        if (diceRoll > 50)
                        {
                            if (chest.chestState == ChestState.closed && !chest.dropCompleted)
                            {
                                chest.bobbyPinTrySuccessful = true;
                                SoundEffectManager.Instance.PlaySoundEffect(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemSwingSoundEffect);
                                iusable.StartChestProcess();
                            }
                        }
                    }

                    if (InputManager.Instance.activeItem.action.WasPerformedThisFrame() && !chest.bobbyPinTrySuccessful)
                    {
                        chest.PlayLock();
                    }
                }
            }

            if (InputManager.Instance.interaction.action.WasPerformedThisFrame())
            {
                // Only interactable objects have capsule colliders. So if it's nut null, it means collider is an interactable (like NPC)
                if (collider2D.GetComponent<CapsuleCollider2D>() != null)
                {
                    Interaction interaction = collider2D.GetComponent<Interaction>();
                    interaction.TriggerDialogue();
                }
            }
        }
    }

    /// <summary>
    /// Drop current active weapon
    /// </summary>
    private void DropActiveItemInput()
    {
        if (InputManager.Instance.dropActiveItem.action.WasPressedThisFrame())
        {
            DropProcess(ChestItem.toBeDroppedChestItem, DropType.ActiveItem);
        }
    }

    public void DropProcess(ChestItem toBeDroppedChestItem, DropType dropType, IReceivable receivable = null, PassiveItemSlotName passiveItemSlotName = PassiveItemSlotName.None)
    {
        if (dropType == DropType.ActiveItem)
        {
            if (player.selectedActiveItem.GetCurrentActiveItem() != null)
            {
                if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Compass)
                {
                    StaticEventHandler.CallCompassDisabled();
                }

                GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
                ChestItem chestItem = chestItemObject.GetComponent<ChestItem>();
                toBeDroppedChestItem = chestItem;

                toBeDroppedChestItem.hasActiveDrop = true;
                toBeDroppedChestItem.droppedByPlayer = true;
                toBeDroppedChestItem.isColliding = true;

                toBeDroppedChestItem.Initialize(player.selectedActiveItem.GetCurrentActiveItem(), player.selectedActiveItem.GetCurrentActiveItem().
                    activeItemDetails.activeItemSprite, player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemName, transform.position);

                // Break free from the player object
                toBeDroppedChestItem.spriteRenderer.enabled = true;
                toBeDroppedChestItem.animator.enabled = true;
                toBeDroppedChestItem.textTMP.enabled = true;
                toBeDroppedChestItem.animator.runtimeAnimatorController = player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemAnimatorController;

                // Store remaining charge count during drop process
                toBeDroppedChestItem.remainingItemCharge = player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge;

                player.setActiveWeaponEvent.CallRemovedActiveItem();

                // Update stat values
                player.UpdateDamageValues();
                player.UpdateWeaponHandlingAndCriticalValues();
                player.UpdateEvasivenessValue();

                RemoveActiveItemFromBook();

                toBeDroppedChestItem.transform.SetParent(null);
                toBeDroppedChestItem.isPickedUp = false;
            }
        }
        else if (dropType == DropType.PassiveItem)
        {
            PassiveItem passiveItem = (PassiveItem)receivable;

            GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
            ChestItem chestItem = chestItemObject.GetComponent<ChestItem>();
            toBeDroppedChestItem = chestItem;

            toBeDroppedChestItem.hasSecondaryPassiveDrop = true;
            toBeDroppedChestItem.droppedByPlayer = true;
            toBeDroppedChestItem.isColliding = true;

            toBeDroppedChestItem.Initialize(passiveItem, passiveItem.passiveItemDetails.passiveItemSprite, passiveItem.passiveItemDetails.passiveItemName, transform.position);

            // Disable some components during equipped
            toBeDroppedChestItem.spriteRenderer.enabled = true;
            toBeDroppedChestItem.animator.enabled = true;
            toBeDroppedChestItem.textTMP.enabled = true;
            toBeDroppedChestItem.animator.runtimeAnimatorController = passiveItem.passiveItemDetails.passiveItemAnimatorController;

            player.setActiveWeaponEvent.CallRemovedPassiveItem();
            
            // Update stat values
            player.UpdateDamageValues();
            player.UpdateWeaponHandlingAndCriticalValues();
            player.UpdateEvasivenessValue();

            RemovePassiveItemFromBook(passiveItem.passiveItemDetails.passiveItemSprite, passiveItemSlotName);

            toBeDroppedChestItem.transform.SetParent(null);
            toBeDroppedChestItem.isPickedUp = false;
        }
        else if(dropType == DropType.Weapon)
        {
            Weapon weapon = (Weapon)receivable;

            if (weapon.onMaindHand)
            {
                if (IsMainHandDropNotPossible())
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.LessThanOneMainHandWeapon);
                    return;
                }
                else
                {
                    switch (weapon.weaponBelongingToWhichMainHandSet)
                    {
                        case 1:
                            if (player.weaponSlotSetArray[0][1] == null) // Drop main hand if only off-hand slot is empty
                            {
                                player.weaponSlotSetArray[0][0] = null;
                            }
                            else
                            {
                                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandFull);
                                return;
                            }
                            break;
                        case 2:
                            if (player.weaponSlotSetArray[1][1] == null)
                            {
                                player.weaponSlotSetArray[1][0] = null;
                            }
                            else
                            {
                                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandFull);
                                return;
                            }
                            break;
                        case 3:
                            if (player.weaponSlotSetArray[2][1] == null)
                            {
                                player.weaponSlotSetArray[2][0] = null;
                            }
                            else
                            {
                                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandFull);
                                return;
                            }
                            break;
                        default:
                            break;
                    }

                    GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
                    ChestItem chestItem = chestItemObject.GetComponent<ChestItem>();
                    toBeDroppedChestItem = chestItem;

                    toBeDroppedChestItem.hasWeaponDrop = true;
                    toBeDroppedChestItem.droppedByPlayer = true;
                    toBeDroppedChestItem.isColliding = true;

                    toBeDroppedChestItem.Initialize(weapon, weapon.weaponDetails.weaponFrontSprite, weapon.weaponDetails.weaponName, transform.position);

                    // Break free from the player object
                    toBeDroppedChestItem.spriteRenderer.enabled = true;
                    toBeDroppedChestItem.animator.enabled = true;
                    toBeDroppedChestItem.textTMP.enabled = true;
                    toBeDroppedChestItem.animator.runtimeAnimatorController = weapon.weaponDetails.weaponHoverAnimatorController;

                    // De-active dropped main hand weapon
                    player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent();

                    // Update stat values
                    player.UpdateDamageValues();
                    player.UpdateWeaponHandlingAndCriticalValues();
                    player.UpdateEvasivenessValue();

                    RemoveMainHandWeaponFromBook();

                    toBeDroppedChestItem.transform.SetParent(null);
                    toBeDroppedChestItem.isPickedUp = false;

                    player.mainHandSlotFilled = false;
                }
            }
            else
            {
                // Set dropped set's off hand null
                player.weaponSlotSetArray[weapon.weaponBelongingToWhichOffHandSet - 1][1] = null;

                GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
                ChestItem chestItem = chestItemObject.GetComponent<ChestItem>();
                toBeDroppedChestItem = chestItem;

                toBeDroppedChestItem.hasWeaponDrop = true;
                toBeDroppedChestItem.droppedByPlayer = true;
                toBeDroppedChestItem.isColliding = true;

                toBeDroppedChestItem.Initialize(weapon, weapon.weaponDetails.weaponFrontSprite, weapon.weaponDetails.weaponName, transform.position);

                // Break free from the player object
                toBeDroppedChestItem.spriteRenderer.enabled = true;
                toBeDroppedChestItem.animator.enabled = true;
                toBeDroppedChestItem.textTMP.enabled = true;
                toBeDroppedChestItem.animator.runtimeAnimatorController = weapon.weaponDetails.weaponHoverAnimatorController;

                player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();

                // Update stat values
                player.UpdateDamageValues();
                player.UpdateWeaponHandlingAndCriticalValues();
                player.UpdateEvasivenessValue();

                RemoveOffHandWeaponsFromBook();

                toBeDroppedChestItem.transform.SetParent(null);
                toBeDroppedChestItem.isPickedUp = false;

                player.offHandSlotFilled = false;
            }
        }

        // Make sure drop completed
        toBeDroppedChestItem.boxCollider2D.enabled = true;
        toBeDroppedChestItem.isColliding = false;

        //if (dropCoroutine == null)
        //{
        //    dropCoroutine = StartCoroutine(MoveItemDown(toBeDroppedChestItem));
        //}
    }

    public bool IsMainHandDropNotPossible()
    {
        int gauge = 0;

        for (int i = 0; i < 3; i++)
        {
            if (player.weaponSlotSetArray[i][0] != null)
            {
                gauge++;
            }
            else
            {
                continue;
            }
        }

        return gauge <= 1;
    }

    /// <summary>
    /// Slow motion item
    /// </summary>
    private IEnumerator MoveItemDown(ChestItem chestItem)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = chestItem.transform.position + new Vector3(0f, 0.5f, 0f);
        float xPos = 0f;
        float yPos = 0f;

        // Drop x position adjustment
        if (initialPosition.x - HelperUtilities.GetMouseWorldPosition().x > -1.5f && initialPosition.x - HelperUtilities.GetMouseWorldPosition().x < 1.5f)
        {
            xPos = HelperUtilities.GetMouseWorldPosition().x;
        }
        else if (initialPosition.x - HelperUtilities.GetMouseWorldPosition().x > 1.5f)
        {
            xPos = initialPosition.x - 1.5f;
        }
        else if (initialPosition.x - HelperUtilities.GetMouseWorldPosition().x < -1.5f)
        {
            xPos = initialPosition.x + 1.5f;
        }

        // Drop y position adjustment
        if (initialPosition.y - HelperUtilities.GetMouseWorldPosition().y > -1.5f && initialPosition.y - HelperUtilities.GetMouseWorldPosition().y < 1.5f)
        {
            yPos = HelperUtilities.GetMouseWorldPosition().y;
        }
        else if (initialPosition.y - HelperUtilities.GetMouseWorldPosition().y > 1.5f)
        {
            yPos = initialPosition.y - 1.5f;
        }
        else if (initialPosition.y - HelperUtilities.GetMouseWorldPosition().y < -1.5f)
        {
            yPos = initialPosition.y + 1.5f;
        }

        Vector3 targetPosition = new Vector3(xPos, yPos, 0f);

        while (elapsedTime < 0.9f)
        {
            elapsedTime += Time.deltaTime; // Increment time based on frame rate
            chestItem.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime);
            yield return null; // Wait for the next frame
        }

        // Ensure the item reaches the target position
        chestItem.transform.position = targetPosition;

        // Make sure drop completed
        dropCoroutine = null;
        chestItem.boxCollider2D.enabled = true;
        chestItem.isColliding = false;
>>>>>>> Stashed changes
    }

    /// <summary>
    /// Enable the player movement
    /// </summary>
    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
    }

    /// <summary>
    /// Disable the player movement
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.idleEvent.CallIdleEvent();
    }

    /// <summary>
    /// Set the current weapon to be first in the player weapon list
    /// </summary>
    private void SetCurrentWeaponToFirstInTheList()
    {
        // Create new temporary list
        List<Weapon> tempWeaponList = new List<Weapon>();

        // Add the current weapon to first in the temp list
        Weapon currentWeapon = player.weaponList[currentWeaponIndex - 1];
        currentWeapon.weaponListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        // Loop through existing weapon list and add - skipping current weapon
        int index = 2;

        foreach (Weapon weapon in player.weaponList)
        {
            if (weapon == currentWeapon) continue;

            tempWeaponList.Add(weapon);
            weapon.weaponListPosition = index;
            index++;
        }

        // Assign new list
        player.weaponList = tempWeaponList;

        currentWeaponIndex = 1;

        // Set current weapon
        SetWeaponByIndex(currentWeaponIndex);
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
