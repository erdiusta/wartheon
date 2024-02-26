using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] float seismicSlamCircleRadius = 5f;
    int seismicSlamDamage = 10;

    [HideInInspector] public bool fireCompletedDuringPressed = false;
    [HideInInspector] public bool isSoundPlayed = false;

    Vector2 movementInput;
    Player player;
    bool leftMouseDownPreviousFrame = false;
    bool rightMouseDownPreviousFrame = false;  
    int currentRightHandWeaponIndex = 1;
    int currentLeftHandWeaponIndex = 0;
    bool isPlayerMovementDisabled = false;
    bool isTeleporting;

    private void Awake()
    {
        player = GetComponent<Player>();
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
        if (isPlayerMovementDisabled)
            return;

        switch (player.moveStatus)
        {
            case MoveStatus.Idle:
            case MoveStatus.Slow:
                // Process the player weapon input
                WeaponInput();
                // Process the player movement input
                MovementInput();
                // Process the player use item input
                UseItemInput();
                // Process the player use special move input
                SpecialMoveInput();
                break;
            case MoveStatus.Stagger:
                player.polygonCollider2D.enabled = false;
                StartCoroutine(Stagger());
                break;
            case MoveStatus.Stun:
                StartCoroutine(StunRoutine());
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Player movement input
    /// </summary>
    private void MovementInput()
    {
        // Get movement input
        movementInput = GameManager.Instance.movement.action.ReadValue<Vector2>().normalized;

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

    IEnumerator Stagger()
    {
        yield return new WaitForSeconds(player.knockback.knockbackTimeWeight);

        player.moveStatus = MoveStatus.Idle;
        player.polygonCollider2D.enabled = true;
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
        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetRightHandShootPosition());

        // Calculate direction vector of mouse cursor from player transform position
        Vector3 playerDirection = (mouseWorldPosition - transform.position);

        // Get weapon to cursor angle
        weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);

        // Get player to cursor angle
        playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);

        // Set player aim direction
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);

        // Trigger weapon aim methods
        player.aimWeapon.Aim(playerAimDirection, playerAngleDegrees);
        player.animatePlayer.InitializeAimAnimationParameters();
        player.animatePlayer.SetAimWeaponAnimationParameters(playerAimDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        // Fire when left mouse button is clicked
        if (GameManager.Instance.attack.action.WasPerformedThisFrame() && !isTeleporting)
        {
            StartCoroutine(PlayerAttackAnimRoutine());

            //Reset precharge for loading again
            fireCompletedDuringPressed = false;
            isSoundPlayed = false;

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponPrechargeTime > 0f) return;

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon || player.activeWeapon.GetCurrentRightHandWeapon().
                weaponDetails.weaponClass == WeaponClass.Bow)
            {
                player.meleeAttackRightHand.IsAttackingAtRightHand = true;
                player.meleeAttackEvent.CallRightHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentRightHandWeapon());
            }

            // Trigger fire weapon event
            player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees,
                weaponAngleDegrees, weaponDirection);
        }

        // Fire for precharge weapons
        if (GameManager.Instance.attack.action.IsPressed())
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
                    weaponAngleDegrees, weaponDirection);
            }

            if (fireCompletedDuringPressed)
            {
                return;
            }
        }
        else
        {
            // Reset hasFired when the mouse button is released
            leftMouseDownPreviousFrame = false;
        }

        // Fire when right mouse button is clicked
        if (GameManager.Instance.attackLeftHand.action.WasPerformedThisFrame())
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
    /// Player character attack motivation
    /// </summary>
    IEnumerator PlayerAttackAnimRoutine()
    {
        // Adjust animator layer weights
        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);

        player.animator.SetBool(Settings.attackMotion, true);

        yield return new WaitForSeconds(0.6f);

        player.animator.SetBool(Settings.attackMotion, false);
        player.animator.SetLayerWeight(player.animatePlayer.baseLayerIndex, 1f);
        player.animator.SetLayerWeight(player.animatePlayer.attackLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.getHitLayerIndex, 0f);
        player.animator.SetLayerWeight(player.animatePlayer.deathLayerIndex, 0f);
    }

    private void SwitchWeaponInput()
    {
        float scrollValue = (GameManager.Instance.switchWeapon.action.ReadValue<Vector2>().normalized).y;

        // Switch weapon if mouse scroll wheel selecetd
        if (scrollValue < 0f)
        {
            LeftHandWeaponCheck();
        }

        if (scrollValue > 0f)
        {
            NextRightHandWeapon();
        }

        if (GameManager.Instance.resetWeaponIndex.action.triggered)
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
        if (player.activeWeapon.GetCurrentLeftHandWeapon() == null && 
            player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.wieldType == WieldType.OneHanded)
        {
            currentLeftHandWeaponIndex++;

            if (currentLeftHandWeaponIndex > player.weaponLeftHandList.Count)
            {
                currentLeftHandWeaponIndex = 1;
            }

            SetLeftHandWeaponByIndex(currentLeftHandWeaponIndex);
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
        if (currentWeapon.isWeaponReloading) 
            return;

        // If remaining projectile is less than clip capacity then return and not infinite projectile then return
        if (currentWeapon.weaponRemainingProjectile < currentWeapon.weaponDetails.weaponClipProjectileCapacity && 
            !currentWeapon.weaponDetails.hasInfiniteProjectile) 
            return;

        // if projectile in clip equals clip capacity then return
        if (currentWeapon.weaponClipRemainingProjectile == currentWeapon.weaponDetails.weaponClipProjectileCapacity) 
            return;

        if (GameManager.Instance.reload.action.triggered)
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
        if (GameManager.Instance.specialMove.action.WasPressedThisFrame() && !player.specialMoveOnCooldown)
        {
            switch (player.playerDetails.playerCharacterName)
            {
                case Settings.astraeus:
                    SeismicSlam();
                    player.specialMoveOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent();
                    break;

                case Settings.erebus:
                    Stealth();
                    player.specialMoveOnCooldown = true;
                    player.specialMoveEvent.CallSpecialMoveUsedEvent();
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
            // Set teleporting status true to make firing inactive
            isTeleporting = true;

            // Start playing teleport particle system
            player.specialMoveParticlesSystem.Play();

            // Wait for mouse click to teleport the character
            GameManager.Instance.attack.action.performed += OnTeleportInput;

            // Play special move sound effect
            SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveSoundEffect);
        }
    }

    private void OnTeleportInput(InputAction.CallbackContext context)
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

            // Unsubscribe from the event to prevent multiple teleports
            GameManager.Instance.attack.action.performed -= OnTeleportInput;
            isTeleporting = false;
        }
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
        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.specialMoveSoundEffect);

        // Set the alpha value to 0.3 (30% opacity)
        currentColor.a = 0.3f;

        // Apply the modified color back to the sprite renderer
        player.spriteRenderer.color = currentColor;

        // Set player's stealth status to true
        player.playerDetails.onStealth = true;
    }

    /// <summary>
    /// Unstealth from special move
    /// </summary>
    public void Unstealth()
    {
        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;

        // Set the alpha value back to 1 (100% opacity)
        currentColor.a = 1f;

        // Apply the modified color back to the sprite renderer
        player.spriteRenderer.color = currentColor;

        // Set player's stealth status to false
        player.playerDetails.onStealth = false;
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

                if (GetComponent<MeleeAttackRightHand>().playerAttackRightHandRoutine == null)
                {
                    GetComponent<MeleeAttackRightHand>().playerAttackRightHandRoutine = StartCoroutine(PlayerAttackAnimRoutine());
                }

                if (enemy.health != null)
                {
                    enemy.health.TakeDamage(seismicSlamDamage, transform.position, enemy.health.transform.position);
                }
            }
        }
    }

    /// <summary>
    /// Use the nearest item within 2 unity units from the player
    /// </summary>
    private void UseItemInput()
    {
        if (GameManager.Instance.interaction.action.WasPerformedThisFrame())
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

                    if (chest.chestState == ChestState.closed && !chest.dropCompleted)
                    {
                        iusable.UseItem();
                    }

                    if (chest.dropCompleted)
                    {
                        iusable.UseItem();
                    }
                }

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

    // This method visualizes the radius of the seismic slam for debugging purposes.
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, seismicSlamCircleRadius);
    }
}
