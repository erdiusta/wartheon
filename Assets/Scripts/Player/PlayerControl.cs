using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] float seismicSlamCircleRadius = 5f;
    int seismicSlamDamage = 10;

    [HideInInspector] public bool fireCompletedDuringPressed = false;
    [HideInInspector] public bool isSoundPlayed = false;
    [HideInInspector] public Coroutine unstealthRoutine;

    Vector2 movementInput;
    Player player;
    bool leftMouseDownPreviousFrame = false;
    bool rightMouseDownPreviousFrame = false;
    int currentRightHandWeaponIndex = 1;
    int currentLeftHandWeaponIndex = 0;
    bool isPlayerMovementDisabled = false;
    Coroutine teleportParticleRoutine;
    Coroutine dropCoroutine;
    Coroutine healthPotionDrinkCoroutine;
    Coroutine attackMotionCoroutine;
    bool particlePlayed;
    float unstealthImmunityTime = 2f;
    bool startStealth = true;
    AimDirection aimDirection;
    float attackMotionTransitionTimer = 0f;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;
    }

    private void OnDisable()
    {
        player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
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

        foreach (Weapon weapon in player.weaponRightHandList)
        {
            if (weapon.weaponDetails == player.playerDetails.startingWeapon)
            {
                SetRightHandWeaponByIndex(index);
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
        player.animator.speed = player.movementByVelocity.moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Update()
    {
        // If player movement disabled then return
        if (isPlayerMovementDisabled) return;

        if (attackMotionCoroutine == null)
        {
            switch (player.moveStatus)
            {
                case MoveStatus.Idle:
                    // Process the player weapon input
                    WeaponAndActiveItemInput();
                    // Process the player movement input
                    MovementInput();
                    // Process the player use item input
                    UseItemInput();
                    // Process the player use special move input
                    SpecialMoveInput();
                    // Drop the player's active item if have
                    DropActiveItemInput();
                    break;
                case MoveStatus.Stagger:
                    player.polygonCollider2D.enabled = false;
                    if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                    {
                        // Trigger fire weapon event for precharge weapons
                        player.fireWeaponEvent.CallFireWeaponEvent(false, false, AimDirection.Right, 0f,
                            0f, Vector3.zero, false);
                    }
                    StartCoroutine(Stagger());
                    break;
                case MoveStatus.Stun:
                    if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                    {
                        // Trigger fire weapon event for precharge weapons
                        player.fireWeaponEvent.CallFireWeaponEvent(false, false, AimDirection.Right, 0f,
                            0f, Vector3.zero, false);
                    }
                    StartCoroutine(StunRoutine());
                    break;
                default:
                    break;
            }
        }
    }

    /// <summary>
    /// Player movement input
    /// </summary>
    private void MovementInput()
    {
        if (player.meleeAttackRightHand.playerAttackMotionRoutine == null)
        {
            // Get movement input
            movementInput = InputManager.Instance.movement.action.ReadValue<Vector2>().normalized;

            float horizontalMovement = movementInput.x;
            float verticalMovement = movementInput.y;

            player.movementByVelocity.MovementInput = movementInput;

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
                player.movementByVelocity.MoveRigidbody(direction, player.movementByVelocity.moveSpeed);

                // Trigger move animations
                player.animatePlayer.SetMovementAnimationParameters();
            }
            // Else trigger idle event
            else
            {
                player.idle.StopVelocity();
                player.animatePlayer.SetIdleAnimationParameters();
            }
        }
    }

    IEnumerator Stagger()
    {
        yield return new WaitForSeconds(player.knockback.knockbackTimeWeight);

        player.moveStatus = MoveStatus.Idle;
        player.polygonCollider2D.enabled = true;
    }

    /// <summary>
    /// Weapon Input
    /// </summary>
    private void WeaponAndActiveItemInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        // Fire weapon input
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Process the player active item input
        FireActiveItemInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

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
        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetRightHandShootPosition());

        // Calculate direction vector of mouse cursor from player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        // Get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get player to cursor angle
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // Set player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);
        aimDirection = playerAimDirection;

        // Trigger weapon aim methods
        player.aimWeapon.Aim(playerAimDirection, playerAngleDegrees);
        player.animatePlayer.InitializeAimAnimationParameters();
        player.animatePlayer.SetAimWeaponAnimationParameters(playerAimDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        // Fire when left mouse button is clicked
        if (InputManager.Instance.attack.action.WasPerformedThisFrame())
        {
            StartCoroutine(PlayerAttackAnimRoutine());

            //Reset precharge for loading again
            fireCompletedDuringPressed = false;
            isSoundPlayed = false;

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponPrechargeTime > 0f) return;

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon)
            {
                player.meleeAttackRightHand.IsAttackingAtRightHand = true;
                player.meleeAttackEvent.CallRightHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentRightHandWeapon());
            }

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponClipRemainingProjectile > 0)
            {
                if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow)
                {
                    player.meleeAttackRightHand.IsAttackingAtRightHand = true;
                    player.meleeAttackEvent.CallRightHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentRightHandWeapon());
                }

                // Trigger fire weapon event
                player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees,
                    weaponAngleDegrees, weaponDirection, false);
            }
        }

        // Fire for precharge weapons
        if (InputManager.Instance.attack.action.IsPressed())
        {
            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponPrechargeTime > 0f && !fireCompletedDuringPressed)
            {
                leftMouseDownPreviousFrame = true;

                if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                {
                    player.meleeAttackEvent.CallRightHandWeaponAnimEvent(playerAimDirection,player.activeWeapon.GetCurrentRightHandWeapon());
                }

                // Trigger fire weapon event for precharge weapons
                player.fireWeaponEvent.CallFireWeaponEvent(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees,
                    weaponAngleDegrees, weaponDirection, false);
            }

            if (fireCompletedDuringPressed) return;
        }
        else
        {
            // Reset hasFired when the mouse button is released
            leftMouseDownPreviousFrame = false;

            // Trigger fire weapon event for precharge weapons
            player.fireWeaponEvent.CallFireWeaponEvent(false, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees,
                weaponAngleDegrees, weaponDirection, false);
        }

        // Fire when right mouse button is clicked
        if (InputManager.Instance.attackLeftHand.action.WasPerformedThisFrame())
        {
            if (player.activeWeapon.GetCurrentLeftHandWeapon() == null)
                return;

            if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield ||
                player.activeWeapon.GetCurrentLeftHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.isMeleeWeapon)
                {
                    player.meleeAttackEvent.CallLeftHandWeaponAnimEvent(playerAimDirection,
                        player.activeWeapon.GetCurrentLeftHandWeapon());
                }
                else
                {
                    rightMouseDownPreviousFrame = true;
                }
            }
        }
        else
        {
            rightMouseDownPreviousFrame = false;
        }
    }

    /// <summary>
    /// Active Item Input
    /// </summary>
    private void FireActiveItemInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        if (player.selectedActiveItem.GetCurrentActiveItem() != null)
        {
            // Use active item when clicked if it is a static item like a dummy
            if (InputManager.Instance.activeItem.action.WasPressedThisFrame())
            {
                if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Dummy)
                {
                    if (player.selectedActiveItem.GetCurrentActiveItem().decoyUsed == false)
                    {
                        player.selectedActiveItem.GetCurrentActiveItem().decoyUsed = true;

                        GameObject decoyObject = Instantiate(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemPrefabArray[0],
                            transform.position, Quaternion.identity);

                        StaticEventHandler.CallDecoySpawned(decoyObject.GetComponent<Decoy>());
                        player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;

                        // Call weapon fired event
                        player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());
                    }
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Hourglass)
                {
                    if (player.selectedActiveItem.GetCurrentActiveItem().hourGlassUsed == false)
                    {
                        player.selectedActiveItem.GetCurrentActiveItem().hourGlassUsed = true;

                        GameObject hourGlassObject = Instantiate(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemPrefabArray[0],
                        transform.position, Quaternion.identity);

                        hourGlassObject.GetComponent<Animator>().SetTrigger("burst");
                        SoundEffectManager.Instance.PlaySoundEffect(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemSwingSoundEffect);
                        Time.timeScale = 0.5f;

                        StaticEventHandler.CallHourglassSpawned();
                        player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;

                        // Call weapon fired event
                        player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());
                    }
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Compass)
                {
                    StaticEventHandler.CallCompassEnabled();
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Potion &&
                    !player.selectedActiveItem.GetCurrentActiveItem().potionDrank)
                {
                    if (healthPotionDrinkCoroutine == null)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemUseSoundEffect);
                        player.selectedActiveItem.GetCurrentActiveItem().potionDrank = true;
                        healthPotionDrinkCoroutine = StartCoroutine(AddHealthCoroutine((int)(50f / player.health.GetStartingHealth() * 100)));

                        player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;

                        // Call weapon fired event
                        player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());
                    }
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Summoner)
                {
                    switch (GameManager.Instance.GetCurrentRoom().roomNodeType.roomNodeTypeName)
                    {
                        // If player is in these below room types, summon is not allowed.
                        case "Boss Foyer":
                        case "Chest Room":
                        case "Corridor":
                        case "Corridor EW":
                        case "Corridor NS":
                        case "Entrance":
                        case "Shop Room":
                            return;

                        default:
                            break;
                    }

                    int selectedIndex = Random.Range(0, player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemPrefabArray.Length);
                    GameObject summonedEnemyPrefab = player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemPrefabArray[selectedIndex];

                    GameObject summonedEnemyObject = Instantiate(summonedEnemyPrefab, transform.position, Quaternion.identity);
                    Enemy enemy = summonedEnemyObject.GetComponent<Enemy>();
                    SoundEffectManager.Instance.PlaySoundEffect(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemUseSoundEffect);

                    enemy.EnemyInitialization(enemy.enemyMovementAI.enemyDetails, 15, GameManager.Instance.GetCurrentDungeonLevel());
                    player.summonedEnemies.Add(summonedEnemyObject);

                    player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;

                    // Call weapon fired event
                    player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());
                }
                // Trigger fire weapon event if item is treated as a projectile
                else
                {
                    player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees,
                        weaponAngleDegrees, weaponDirection, false, true);
                }
            }
        }
    }

    IEnumerator AddHealthCoroutine(float healthAmount)
    {
        float healthForEachStep = 3f;
        float accumulatedHealth = 0f;

        while (healthAmount > accumulatedHealth)
        {
            accumulatedHealth += healthForEachStep;
            player.health.AddHealth((int)healthForEachStep);
            StaticEventHandler.CallBookHealthChangedEvent(player.health.currentHealth);

            yield return new WaitForSeconds(0.5f);
        }

        healthPotionDrinkCoroutine = null;

        yield return null;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        // Trigger reset prechager mechanism in case a hit taken during the precharge
        player.fireWeaponEvent.CallFireWeaponEvent(false, false, AimDirection.Right, 0f, 0f, Vector3.zero, false);
    }

    /// <summary>
    /// Player character attack motivation
    /// </summary>
    IEnumerator PlayerAttackAnimRoutine()
    {
        // Adjust animator layer weights
        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

        yield return new WaitForSeconds(0.5f);

        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);
    }

    private void SwitchWeaponInput()
    {
        float scrollValue = (InputManager.Instance.switchWeapon.action.ReadValue<Vector2>().normalized).y;

        // Switch weapon if mouse scroll wheel selecetd
        if (scrollValue < 0f)
        {
            LeftHandWeaponCheck();
        }

        if (scrollValue > 0f)
        {
            NextRightHandWeapon();
        }

        if (InputManager.Instance.resetWeaponIndex.action.triggered)
        {
            SetCurrentWeaponToFirstInTheList();
        }
    }

    private void NextRightHandWeapon()
    {
        if (player.activeWeapon.GetCurrentLeftHandWeapon() == null)
        {
            currentRightHandWeaponIndex++;

            if (currentRightHandWeaponIndex > player.weaponRightHandList.Count)
            {
                currentRightHandWeaponIndex = 1;
            }

            SetRightHandWeaponByIndex(currentRightHandWeaponIndex);
        }
    }

    private void LeftHandWeaponCheck()
    {
        if (player.activeWeapon.GetCurrentLeftHandWeapon() == null && player.activeWeapon.GetCurrentRightHandWeapon().
            weaponDetails.wieldType == WieldType.OneHanded)
        {
            currentLeftHandWeaponIndex++;

            if (currentLeftHandWeaponIndex > player.weaponLeftHandList.Count)
            {
                currentLeftHandWeaponIndex = 1;
            }

            SetLeftHandWeaponByIndex(currentLeftHandWeaponIndex);
        }
        else if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
        {
            return;
        }
        else
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtLeftHandEvent();
        }
    }

    private void SetRightHandWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex - 1 < player.weaponRightHandList.Count)
        {
            currentRightHandWeaponIndex = weaponIndex;

            player.setActiveWeaponEvent.CallSetActiveWeaponAtRightHandEvent(player.weaponRightHandList[weaponIndex - 1]);

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.wieldType == WieldType.OneHanded)
            {
                player.setActiveWeaponEvent.CallOneHandWeaponEquipEvent();
            }
            else if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
            {
                player.setActiveWeaponEvent.CallTwoHandWeaponEquipEvent();
            }
        }
    }

    private void SetLeftHandWeaponByIndex(int weaponIndex)
    {
        if (weaponIndex - 1 < player.weaponLeftHandList.Count)
        {
            currentLeftHandWeaponIndex = weaponIndex;

            player.setActiveWeaponEvent.CallSetActiveWeaponAtLeftHandEvent(player.weaponLeftHandList[weaponIndex - 1]);
        }
    }

    private void ReloadWeaponInput()
    {
        Weapon currentWeapon = player.activeWeapon.GetCurrentRightHandWeapon();

        // If current weapon is reloading return
        if (currentWeapon.isWeaponReloading) return;

        // If remaining projectile is less than clip capacity then return and not infinite projectile then return
        if (currentWeapon.weaponRemainingProjectile < currentWeapon.weaponDetails.weaponClipProjectileCapacity && 
            !currentWeapon.weaponDetails.hasInfiniteProjectile) 
            return;

        // if projectile in clip equals clip capacity then return
        if (currentWeapon.weaponClipRemainingProjectile == currentWeapon.weaponDetails.weaponClipProjectileCapacity) 
            return;

        if (InputManager.Instance.reload.action.triggered)
        {
            // Call the reload weapon event
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentRightHandWeapon(), 0);
        }
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
        if (InputManager.Instance.specialMove.action.WasPressedThisFrame() && !player.specialMoveOnCooldown)
        {
            switch (player.playerDetails.playerCharacterName)
            {
                case Settings.astraeus:
                    SeismicSlam();
                    player.specialMoveOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent();
                    break;

                case Settings.orion:
                    if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow &&
                        player.activeWeapon.GetCurrentRightHandWeapon().weaponClipRemainingProjectile > 0)
                    {
                        HeadShot();
                        player.specialMoveOnCooldown = true;
                        player.specialMoveEvent.CallSpecialMoveUsedEvent();
                    }
                    break;

                case Settings.erebus:
                    Stealth();
                    break;

                case Settings.lyrisa:
                    Teleport();
                    player.specialMoveOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent();
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
        if (player.specialMoveOnCooldown == false)
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
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveSoundEffect);
        }
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
        player.playerDetails.onStealth = true;
        startStealth = false;

        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveSoundEffect);

        // Set the alpha value to 0.3 (30% opacity)
        currentColor.a = 0.3f;

        // Apply the modified color back to the sprite renderer
        player.spriteRenderer.color = currentColor;

        // Start the coroutine to maintain the alpha value during stealth
        StartCoroutine(MaintainStealthAlpha());
    }

    IEnumerator MaintainStealthAlpha()
    {
        while (player.playerDetails.onStealth)
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
        if (startStealth) return;

        if (unstealthRoutine != null) return;

        // Trigger cooldown and ui components
        player.specialMoveOnCooldown = true;
        player.specialMoveEvent.CallSpecialMoveUsedEvent();

        unstealthRoutine = StartCoroutine(UnstealthRoutine());
    }

    IEnumerator UnstealthRoutine()
    {
        // Set immunity
        player.health.isDamageable = false;

        // Set player's stealth status to false
        player.playerDetails.onStealth = false;

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
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveSoundEffect);
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
    /// Execute Head Shot special move
    /// </summary>
    private void HeadShot()
    {
        StartCoroutine(PlayerAttackAnimRoutine());

        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection);

        //Reset precharge for loading again
        fireCompletedDuringPressed = false;
        isSoundPlayed = false;

        player.meleeAttackRightHand.IsAttackingAtRightHand = true;
        player.meleeAttackEvent.CallRightHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentRightHandWeapon());

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, true);
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
            SetRemainingCharges(player.chestItem);
            DropProcess(player.chestItem);
        }
    }

    public void SetRemainingCharges(ChestItem chestItem)
    {
        if (player.selectedActiveItem.GetCurrentActiveItem() != null)
        {
            chestItem.remainingItemCharge = GameManager.Instance.GetToBeDroppedChestItem().toBeDroppedActiveItem.activeItemRemainingCharge;
        }
    }

    public void DropProcess(ChestItem chestItem)
    {
        if (player.selectedActiveItem.GetCurrentActiveItem() != null)
        {
            if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Compass)
            {
                StaticEventHandler.CallCompassDisabled();
            }

            if (dropCoroutine == null)
            {
                dropCoroutine = StartCoroutine(MoveItemDown(chestItem));
            }

            chestItem.hasActiveDrop = true;
            chestItem.droppedByPlayer = true;

            // Break free from the player object
            chestItem.spriteRenderer.enabled = true;
            chestItem.animator.enabled = true;
            chestItem.textTMP.enabled = true;
            chestItem.Initialize(null, player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails, null,
                player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemSprite, player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemName,
                transform.position);

            player.setActiveWeaponEvent.CallRemovedActiveItem();
            player.RemoveActiveItemFromBook();

            chestItem.boxCollider2D.enabled = true;
            chestItem.isPickedUp = false;
            chestItem.gameObject.transform.SetParent(null);
        }
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
        player.idle.StopVelocity();
        player.animatePlayer.SetIdleAnimationParameters();
    }

    /// <summary>
    /// Set the current weapon to be first in the player weapon list
    /// </summary>
    private void SetCurrentWeaponToFirstInTheList()
    {
        // Create new temporary list
        List<Weapon> tempWeaponList = new List<Weapon>();

        // Add the current weapon to first in the temp list
        Weapon currentWeapon = player.weaponRightHandList[currentRightHandWeaponIndex - 1];
        currentWeapon.weaponRightHandListPosition = 1;
        tempWeaponList.Add(currentWeapon);

        // Loop through existing weapon list and add - skipping current weapon
        int index = 2;

        foreach (Weapon weapon in player.weaponRightHandList)
        {
            if (weapon == currentWeapon) continue;

            tempWeaponList.Add(weapon);
            weapon.weaponRightHandListPosition = index;
            index++;
        }

        // Assign new list
        player.weaponRightHandList = tempWeaponList;

        currentRightHandWeaponIndex = 1;

        // Set current weapon
        SetRightHandWeaponByIndex(currentRightHandWeaponIndex);
    }

    public AimDirection GetAimDirection()
    {
        return aimDirection;
    }

    // This method visualizes the radius of the seismic slam for debugging purposes.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, seismicSlamCircleRadius);
    }
}
