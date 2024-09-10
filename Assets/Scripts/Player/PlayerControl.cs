using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    [SerializeField] float seismicSlamCircleRadius = 5f;
    int seismicSlamDamage = 10;

    [HideInInspector] public bool fireCompletedDuringPressed = false;
    [HideInInspector] public bool isSoundPlayed = false;
    [HideInInspector] public Coroutine unstealthRoutine;
    [HideInInspector] public float movementTimer = 0;

    Vector2 movementInput;
    Player player;
    bool leftMouseDownPreviousFrame = false;
    bool rightMouseDownPreviousFrame = false;
    bool isPlayerMovementDisabled = false;
    Coroutine teleportParticleRoutine;
    Coroutine dropCoroutine;
    Coroutine healthPotionDrinkCoroutine;
    bool particlePlayed;
    float unstealthImmunityTime = 2f;
    AimDirection aimDirection;
    int previousIndex = 1;

    // Attack member variables
    [HideInInspector] public MeleeAttackType meleeAttackTypeMainHand = MeleeAttackType.None;
    [HideInInspector] public MeleeAttackType meleeAttackTypeOffHand = MeleeAttackType.None;

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
        // Set player animation speed
        SetPlayerAnimationSpeed();
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
                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                    {
                        // Trigger fire weapon event for precharge weapons
                        player.fireWeaponEvent.CallFireWeaponEvent(false, false, AimDirection.Right, 0f, 0f, Vector3.zero, false);
                    }
                }
                StartCoroutine(Stagger());
                break;
            case MoveStatus.Stun:
                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                {

                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                    {
                        // Trigger fire weapon event for precharge weapons
                        player.fireWeaponEvent.CallFireWeaponEvent(false, false, AimDirection.Right, 0f, 0f, Vector3.zero, false);
                    }
                }
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
        movementInput = InputManager.Instance.movement.action.ReadValue<Vector2>().normalized;

        float horizontalMovement = movementInput.x;
        float verticalMovement = movementInput.y;

        player.movementByVelocity.MovementInput = movementInput;

        if (Mathf.Abs(movementInput.x) > 0.1f || Mathf.Abs(movementInput.y) > 0.1f)
        {
            movementTimer += Time.deltaTime;
        }
        else
        {
            movementTimer = 0;
        }

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
        // If glossary book is open, disable attack
        if (GameManager.Instance.glossaryBookOpen) return;

        // If pop-up window is open, disable attack
        if (GameManager.Instance.popUpWindowOpen) return;

        if (player.activeWeapon.GetCurrentMainHandWeapon() == null) return;

        // Fire when left mouse button is clicked - melee
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
        {
            // Check for quick tap input
            if (InputManager.Instance.attack.action.WasPerformedThisFrame())
            {
                player.meleeAttackRightHand.IsAttackingAtRightHand = true;

                diceAgainForMainHand:

                int randomNum = Random.Range(1, 101);
                int selectedWeaponMoveIndex;

                if (randomNum <= 40)
                {
                    selectedWeaponMoveIndex = 1;
                }
                else if (randomNum <= 70)
                {
                    selectedWeaponMoveIndex = 2;
                }
                else
                {
                    selectedWeaponMoveIndex = 3;
                }

                switch (selectedWeaponMoveIndex)
                {
                    case 1:
                        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasSwing)
                        {
                            meleeAttackTypeMainHand = MeleeAttackType.Swing;
                        }
                        else
                        {
                            goto diceAgainForMainHand;
                        }
                        break;
                    case 2:
                        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasSweep)
                        {
                            meleeAttackTypeMainHand = MeleeAttackType.Sweep;
                        }
                        else
                        {
                            goto diceAgainForMainHand;
                        }
                        break;
                    case 3:
                        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasThrust)
                        {
                            meleeAttackTypeMainHand = MeleeAttackType.Thrust;
                        }
                        else
                        {
                            goto diceAgainForMainHand;
                        }
                        break;
                    default:
                        break;
                }

                player.meleeAttackEvent.CallMainHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), meleeAttackTypeMainHand);
            }

            // Return after moves finished if off-hand weapon is free or a shield
            if (player.activeWeapon.GetCurrentOffHandWeapon() == null || player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Shield)
            {
                return;
            }
        }

        if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
        {
            // Fire when right mouse button is clicked - melee off-hand
            if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.isMeleeWeapon)
            {
                // Check for quick tap input
                if (InputManager.Instance.attackOffHand.action.WasPerformedThisFrame())
                {
                    player.meleeAttackLeftHand.IsAttackingAtLeftHand = true;

                    diceAgainForOffHand:

                    int randomNum = Random.Range(1, 101);
                    int selectedWeaponMoveIndex;

                    if (randomNum <= 40)
                    {
                        selectedWeaponMoveIndex = 1;
                    }
                    else if (randomNum <= 70)
                    {
                        selectedWeaponMoveIndex = 2;
                    }
                    else
                    {
                        selectedWeaponMoveIndex = 3;
                    }

                    switch (selectedWeaponMoveIndex)
                    {
                        case 1:
                            if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasSwing)
                            {
                                meleeAttackTypeOffHand = MeleeAttackType.Swing;
                            }
                            else
                            {
                                goto diceAgainForOffHand;
                            }
                            break;
                        case 2:
                            if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasSweep)
                            {
                                meleeAttackTypeOffHand = MeleeAttackType.Sweep;
                            }
                            else
                            {
                                goto diceAgainForOffHand;
                            }
                            break;
                        case 3:
                            if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.hasThrust)
                            {
                                meleeAttackTypeOffHand = MeleeAttackType.Thrust;
                            }
                            else
                            {
                                goto diceAgainForOffHand;
                            }
                            break;
                        default:
                            break;
                    }

                    player.meleeAttackEvent.CallOffHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentOffHandWeapon(), meleeAttackTypeOffHand);

                }

                // Don't pass to the ranged weapon elements so finish method here while returning
                return;
            }
        }

        // Fire when left mouse button is clicked
        if (InputManager.Instance.attack.action.WasPerformedThisFrame())
        {
            //Reset precharge for loading again
            fireCompletedDuringPressed = false;
            isSoundPlayed = false;

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0f) return;

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow)
            {
                player.meleeAttackRightHand.IsAttackingAtRightHand = true;
                player.meleeAttackEvent.CallMainHandWeaponAnimEvent(playerAimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), MeleeAttackType.None);
            }

            // Trigger fire weapon event
            player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false);
        }

        // Fire for precharge weapons
        if (InputManager.Instance.attack.action.IsPressed())
        {
            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0f && !fireCompletedDuringPressed)
            {
                leftMouseDownPreviousFrame = true;

                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                {
                    player.meleeAttackEvent.CallMainHandWeaponAnimEvent(playerAimDirection,player.activeWeapon.GetCurrentMainHandWeapon(), MeleeAttackType.None);
                }

                // Trigger fire weapon event for precharge weapons
                player.fireWeaponEvent.CallFireWeaponEvent(true, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false);
            }

            if (fireCompletedDuringPressed) return;
        }
        else
        {
            // Reset hasFired when the mouse button is released
            leftMouseDownPreviousFrame = false;

            // Trigger fire weapon event for precharge weapons
            player.fireWeaponEvent.CallFireWeaponEvent(false, leftMouseDownPreviousFrame, playerAimDirection, playerAngleDegrees,weaponAngleDegrees, weaponDirection, false);
        }

        // Fire when right mouse button is clicked
        if (InputManager.Instance.attackOffHand.action.WasPerformedThisFrame())
        {
            if (player.activeWeapon.GetCurrentOffHandWeapon() == null)
                return;

            if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield ||
                player.activeWeapon.GetCurrentOffHandWeapon() != null)
            {
                if (!player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.isMeleeWeapon)
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
                    if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge > 0)
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
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Hourglass)
                {
                    if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge > 0)
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
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Compass)
                {
                    StaticEventHandler.CallCompassEnabled();
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Potion &&
                    !player.selectedActiveItem.GetCurrentActiveItem().potionDrank)
                {
                    if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge > 0)
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

                    if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge > 0)
                    {
                        int selectedIndex = Random.Range(0, player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemPrefabArray.Length);
                        GameObject summonedEnemyPrefab = player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemPrefabArray[selectedIndex];

                        GameObject summonedEnemyObject = Instantiate(summonedEnemyPrefab, transform.position, Quaternion.identity);
                        Enemy enemy = summonedEnemyObject.GetComponent<Enemy>();
                        SoundEffectManager.Instance.PlaySoundEffect(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemUseSoundEffect);

                        enemy.EnemyInitialization(enemy.enemyMovementAI.enemyDetails, 15, GameManager.Instance.GetCurrentDungeonLevel());
                        player.summonedEnemies.Add(summonedEnemyObject);

                        player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;

                        if (GameManager.Instance.GetCurrentRoom().isClearedOfEnemies)
                        {
                            Destroy(summonedEnemyObject);
                        }

                        // Call weapon fired event
                        player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());
                    }
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Potion)
                {
                    if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge > 0)
                    {
                        player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;

                        // Call weapon fired event
                        player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());
                    }
                }
                // Trigger fire weapon event if item is treated as a projectile
                else
                {
                    player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, true);
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

    private void SwitchWeaponInput()
    {
        float scrollValue = (InputManager.Instance.switchWeapon.action.ReadValue<Vector2>().normalized).y;

        // Switch weapon if mouse scroll wheel selecetd
        if (scrollValue < 0f)
        {
            PreviousWeaponSet(true);
        }

        if (scrollValue > 0f)
        {

            NextWeaponSet(true, true);
        }
    }

    public void NextWeaponSet(bool onlySwitch, bool mouseWheel, int setNumber = 0)
    {
        if (mouseWheel)
        {
            // Cache previous weapon slot index
            InventoryManager.Instance.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

            // Set previous index
            previousIndex = player.currentWeaponSlotSetIndex;

            // Increment the current weapon slot set index
            player.currentWeaponSlotSetIndex++;

            if (player.currentWeaponSlotSetIndex > 3)
            {
                player.currentWeaponSlotSetIndex = 1;
            }

            SetWeaponSetByIndex(onlySwitch);
        }
        else
        {
            if (player.currentWeaponSlotSetIndex == setNumber) return;

            // Cache previous weapon slot index
            InventoryManager.Instance.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

            player.currentWeaponSlotSetIndex = setNumber;
            SetWeaponSetByIndex(onlySwitch);
        }

        HighlightWeaponSetButton();
    }

    public void PreviousWeaponSet(bool onlySwitch)
    {
        // Cache previous weapon slot index
        InventoryManager.Instance.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

        previousIndex = player.currentWeaponSlotSetIndex;

        // Decrease the current weapon slot set index
        player.currentWeaponSlotSetIndex--;

        if (player.currentWeaponSlotSetIndex < 1)
        {
            player.currentWeaponSlotSetIndex = 3;
        }

        SetWeaponSetByIndex(onlySwitch);

        HighlightWeaponSetButton();
    }

    public void SetWeaponSetByIndex(bool onlySwitch)
    {
        // ACTIVE WEAPON VARIABLES SWITCH
        if (player.weaponSlotSetArray[previousIndex - 1][1] != null)
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();
        }

        // WEAPON SLOTS SWITCH
        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
        {
            player.setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0],
                player.currentWeaponSlotSetIndex);

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.OneHanded)
            {
                player.setActiveWeaponEvent.CallOneHandWeaponEquipEvent();
            }
            else if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
            {
                player.setActiveWeaponEvent.CallTwoHandWeaponEquipEvent();
            }
        }
        else
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent();
        }

        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] != null)
        {
            player.setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1], player.currentWeaponSlotSetIndex);
        }
        else
        {
            if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
            {
                if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0].weaponDetails.wieldType != WieldType.TwoHanded)
                {
                    player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();
                }
            }
            else
            {
                player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();
            }
        }

        // BOOK UI SWITCH
        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
        {
            PopulateMainHandWeaponsToBook(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0], onlySwitch);
        }
        else
        {
            RemoveMainHandWeaponFromBook();
        }

        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] != null)
        {
            PopulateOffHandWeaponsToBook(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1]);
        }
        else
        {
            if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
            {
                if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    RemoveOffHandWeaponsFromBook();
                }
            }
            else
            {
                RemoveOffHandWeaponsFromBook();
            }
        }
    }

    /// <summary>
    /// Highlight weapon set button to be seen clearly
    /// </summary>
    private void HighlightWeaponSetButton()
    {
        // Get the button container
        Transform buttonContainer = GameManager.Instance.bookView.transform.GetChild(1).GetChild(3).GetChild(0);

        // Clamp the index within the valid range (assuming 3 weapon slots)
        player.currentWeaponSlotSetIndex = Mathf.Clamp(player.currentWeaponSlotSetIndex, 1, 3);

        // Loop through all buttons to reset them to the normal state
        for (int i = 0; i < buttonContainer.childCount; i++)
        {
            Button button = buttonContainer.GetChild(i).GetComponent<Button>();
            ColorBlock cb = button.colors;
            button.image.color = cb.normalColor;  // Reset to normal color
        }

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

                toBeDroppedChestItem.boxCollider2D.enabled = false;

                // Break free from the player object
                toBeDroppedChestItem.spriteRenderer.enabled = true;
                toBeDroppedChestItem.animator.enabled = true;
                toBeDroppedChestItem.textTMP.enabled = true;
                toBeDroppedChestItem.animator.runtimeAnimatorController = player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemAnimatorController;

                // Store remaining charge count during drop process
                toBeDroppedChestItem.remainingItemCharge = player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge;

                player.setActiveWeaponEvent.CallRemovedActiveItem();
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

            toBeDroppedChestItem.boxCollider2D.enabled = false;

            // Disable some components during equipped
            toBeDroppedChestItem.spriteRenderer.enabled = true;
            toBeDroppedChestItem.animator.enabled = true;
            toBeDroppedChestItem.textTMP.enabled = true;
            toBeDroppedChestItem.animator.runtimeAnimatorController = passiveItem.passiveItemDetails.passiveItemAnimatorController;

            player.setActiveWeaponEvent.CallRemovedPassiveItem();
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

                    toBeDroppedChestItem.boxCollider2D.enabled = false;

                    // Break free from the player object
                    toBeDroppedChestItem.spriteRenderer.enabled = true;
                    toBeDroppedChestItem.animator.enabled = true;
                    toBeDroppedChestItem.textTMP.enabled = true;
                    toBeDroppedChestItem.animator.runtimeAnimatorController = weapon.weaponDetails.weaponHoverAnimatorController;

                    // De-active dropped main hand weapon
                    player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent();
                    RemoveMainHandWeaponFromBook();

                    toBeDroppedChestItem.transform.SetParent(null);
                    toBeDroppedChestItem.isPickedUp = false;

                    player.mainHandSlotFilled = false;
                }
            }
            else
            {
                switch (weapon.weaponBelongingToWhichOffHandSet)
                {
                    case 1:
                        player.weaponSlotSetArray[0][1] = null;
                        break;
                    case 2:
                        player.weaponSlotSetArray[1][1] = null;
                        break;
                    case 3:
                        player.weaponSlotSetArray[2][1] = null;
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

                toBeDroppedChestItem.boxCollider2D.enabled = false;

                // Break free from the player object
                toBeDroppedChestItem.spriteRenderer.enabled = true;
                toBeDroppedChestItem.animator.enabled = true;
                toBeDroppedChestItem.textTMP.enabled = true;
                toBeDroppedChestItem.animator.runtimeAnimatorController = weapon.weaponDetails.weaponHoverAnimatorController;

                player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();
                RemoveOffHandWeaponsFromBook();

                toBeDroppedChestItem.transform.SetParent(null);
                toBeDroppedChestItem.isPickedUp = false;

                player.offHandSlotFilled = false;
            }
        }

        if (dropCoroutine == null)
        {
            dropCoroutine = StartCoroutine(MoveItemDown(toBeDroppedChestItem));
        }
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
    }

    /// <summary>
    /// Enable the player movement
    /// </summary>
    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.moveSpeed;
    }

    /// <summary>
    /// Disable the player movement
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.movementByVelocity.moveSpeed = 0f;
        player.idle.StopVelocity();
        player.animatePlayer.SetIdleAnimationParameters();
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

    public void PopulateMainHandWeaponsToBook(Weapon weapon, bool onlySwitch)
    {
        StaticEventHandler.CallWeaponAddedToMainHandBook(weapon, onlySwitch);
    }

    public void RemoveMainHandWeaponFromBook()
    {
        StaticEventHandler.CallWeaponRemovedFromMainHandBook();
    }

    public void PopulateOffHandWeaponsToBook(Weapon weapon)
    {
        StaticEventHandler.CallWeaponAddedToOffHandBook(weapon);
    }

    public void RemoveOffHandWeaponsFromBook()
    {
        StaticEventHandler.CallWeaponRemovedFromOffHandBook();
    }

    public void PopulateActiveItemsToBook(Sprite sprite)
    {
        StaticEventHandler.CallItemAddedToActiveItemSlot(sprite);
    }

    public void RemoveActiveItemFromBook()
    {
        StaticEventHandler.CallItemRemovedFromActiveItemSlot();
    }

    public void PopulatePassiveItemsToBook(Sprite sprite, PassiveItemSlotName itemSlotName)
    {
        StaticEventHandler.CallItemAddedToPassiveItemSlot(sprite, itemSlotName);
    }

    public void RemovePassiveItemFromBook(Sprite sprite, PassiveItemSlotName itemSlotName)
    {
        StaticEventHandler.CallItemRemovedFromPassiveItemSlot(sprite, itemSlotName);
    }
}
