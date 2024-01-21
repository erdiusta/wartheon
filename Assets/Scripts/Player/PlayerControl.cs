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
    bool rightMouseDownPreviousFrame = false;
    int currentRightHandWeaponIndex = 1;
    int currentLeftHandWeaponIndex = 0;
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
        weaponDirection = (mouseWorldPosition - player.activeWeapon.GetRightHandShootPosition());

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
            }
        }
        else
        {
            leftMouseDownPreviousFrame = false;
        }

        // Fire when right mouse button is clicked
        if (Input.GetMouseButtonDown(1))
        {
            if (player.activeWeapon.GetCurrentLeftHandWeapon() == null)
                return;

            if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield ||
                player.activeWeapon.GetCurrentLeftHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentLeftHandWeapon().weaponDetails.isMeleeWeapon)
                {
                    player.meleeAttackEvent.CallLeftHandMeleeAttackEvent(playerAimDirection,
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
        // Switch weapon if mouse scroll wheel selecetd
        if (Input.mouseScrollDelta.y < 0f)
        {
            LeftHandWeaponCheck();
        }

        if (Input.mouseScrollDelta.y > 0f)
        {
            NextRightHandWeapon();
        }

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

        if (Input.GetKeyDown(KeyCode.R))
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

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(movementDetails), movementDetails);
    }
#endif
    #endregion
}
