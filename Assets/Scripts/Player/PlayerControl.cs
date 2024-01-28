using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    [HideInInspector] public bool fireCompletedDuringPressed = false;
    [HideInInspector] public bool isSoundPlayed = false;

    Vector2 movementInput;
    Player player;
    bool leftMouseDownPreviousFrame = false;
<<<<<<< Updated upstream
    bool rightMouseDownPreviousFrame = false;
    int currentRightHandWeaponIndex = 1;
    int currentLeftHandWeaponIndex = 0;
    float moveSpeed;
=======
    bool rightMouseDownPreviousFrame = false;  
    int currentRightHandWeaponIndex = 1;
    int currentLeftHandWeaponIndex = 0;
>>>>>>> Stashed changes
    bool isPlayerMovementDisabled = false;
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

        switch (player.playerStatus)
        {
            case Status.Idle:
                // Process the player weapon input
                WeaponInput();
                // Process the player movement input
                MovementInput();
                break;
            case Status.Stagger:
                player.polygonCollider2D.enabled = false;
                StartCoroutine(Stagger());
                break;
            case Status.Poisoned:
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
<<<<<<< Updated upstream
        float horizontalMovement = Input.GetAxisRaw("Horizontal");
        float verticalMovement = Input.GetAxisRaw("Vertical");
=======
        movementInput = GameManager.Instance.movement.action.ReadValue<Vector2>().normalized;

        float horizontalMovement = movementInput.x;
        float verticalMovement = movementInput.y;

        player.movementByVelocity.MovementInput = movementInput;
>>>>>>> Stashed changes

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

        player.playerStatus = Status.Idle;
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
<<<<<<< Updated upstream
        if (Input.GetMouseButtonDown(0))
        {
            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon)
            {
                player.meleeAttackEvent.CallRightHandMeleeAttackEvent(playerAimDirection, 
                    player.activeWeapon.GetCurrentRightHandWeapon());
            }
            else
            {
                // Trigger fire weapon event
                player.fireWeaponEvent.CallFireWeaponEvent(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees,
                    weaponAngleDegrees, weaponDirection);
                leftMouseDownPreviousFrame = true;
=======
        if (GameManager.Instance.attack.action.WasPerformedThisFrame())
        {
            //Reset precharge for loading again
            fireCompletedDuringPressed = false;
            isSoundPlayed = false;

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.weaponPrechargeTime > 0f) return;

            if (player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon || player.activeWeapon.GetCurrentRightHandWeapon().
                weaponDetails.weaponClass == WeaponClass.Bow)
            {
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
>>>>>>> Stashed changes
            }
        }
        else
        {
            // Reset hasFired when the mouse button is released
            leftMouseDownPreviousFrame = false;
        }

        // Fire when right mouse button is clicked
<<<<<<< Updated upstream
        if (Input.GetMouseButtonDown(1))
=======
        if (GameManager.Instance.attackLeftHand.action.WasPerformedThisFrame())
>>>>>>> Stashed changes
        {
            if (player.activeWeapon.GetCurrentLeftHandWeapon() == null)
                return;

            if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield ||
                player.activeWeapon.GetCurrentLeftHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.isMeleeWeapon)
                {
<<<<<<< Updated upstream
                    player.meleeAttackEvent.CallLeftHandMeleeAttackEvent(playerAimDirection,
=======
                    player.meleeAttackEvent.CallLeftHandWeaponAnimEvent(playerAimDirection,
>>>>>>> Stashed changes
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

<<<<<<< Updated upstream
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            SetRightHandWeaponByIndex(1);
        }

        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            SetRightHandWeaponByIndex(2);
        }

        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            SetRightHandWeaponByIndex(3);
        }

        if (Input.GetKeyDown(KeyCode.Alpha4))
        {
            SetRightHandWeaponByIndex(4);
        }

        if (Input.GetKeyDown(KeyCode.Alpha5))
        {
            SetRightHandWeaponByIndex(5);
        }

        if (Input.GetKeyDown(KeyCode.Alpha6))
        {
            SetRightHandWeaponByIndex(6);
        }

        if (Input.GetKeyDown(KeyCode.Alpha7))
        {
            SetRightHandWeaponByIndex(7);
        }

        if (Input.GetKeyDown(KeyCode.Alpha8))
        {
            SetRightHandWeaponByIndex(8);
        }

        if (Input.GetKeyDown(KeyCode.Alpha9))
        {
            SetRightHandWeaponByIndex(9);
        }

        if (Input.GetKeyDown(KeyCode.Alpha0))
        {
            SetRightHandWeaponByIndex(10);
        }

        if (Input.GetKeyDown(KeyCode.Minus))
=======
        if (GameManager.Instance.resetWeaponIndex.action.triggered)
>>>>>>> Stashed changes
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
}
