using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    [HideInInspector] public bool isSoundPlayed = false;
    [HideInInspector] public Coroutine unstealthRoutine;
    [HideInInspector] public float movementTimer = 0;
    [HideInInspector] public bool isPlayerRolling;
    [HideInInspector] public bool IsParrying { get => isParrying; set { isParrying = value; } }
    [HideInInspector] public Vector3 playerBodycenterPosition;

    // Input gamepad
    [SerializeField] float cursorSpeed = 1000f;
    Vector2 lastValidGamepadAimInput = Vector2.zero;

    public Animator activeSkillTypeOneAnimator;
    public Animator activeSkillTypeTwoAnimator;
    public Animator activeSkillTypeThreeAnimator;
    public Animator activeSkillTypeFourAnimator;

    Vector2 movementInput;
    Player player;
    bool isPlayerMovementDisabled = false;
    Coroutine teleportParticleRoutine;
    Coroutine dropCoroutine;
    Coroutine stunCoroutine;
    Coroutine rootCoroutine;
    Coroutine frostCoroutine;
    Coroutine healthPotionDrinkCoroutine;
    Coroutine playerRollCoroutine;
    WaitForFixedUpdate waitForFixedUpdate;
    AnimationEventHelperMainHand animationEventHelperMainHand;
    float playerRollCooldownTimer = 0f;
    bool isParrying;
    float playerParryDurationTimer = 0f;
    float playerParryCooldownTimer = 0f;
    float playerParryEffectiveDuration = 0.4f;
    float playerParryCooldownDuration = 1.6f;
    bool particlePlayed;
    float unstealthImmunityTime = 2f;
    AimDirection aimDirection;
    AttackDirection attackDirection;

    // Attack member variables
    [HideInInspector] public AttackShape meleeAttackTypeMainHand = AttackShape.None;
    [HideInInspector] public AttackShape meleeAttackTypeOffHand = AttackShape.None;

    List<SpriteRenderer> allSpriteRenderers = new List<SpriteRenderer>();

    private void Awake()
    {
        player = GetComponent<Player>();

        animationEventHelperMainHand = GetComponent<AnimationEventHelperMainHand>();
    }

    private void OnEnable()
    {
        player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;

        animationEventHelperMainHand.OnSeismicSlamTriggered.AddListener(PerformSeismicSlam);
        animationEventHelperMainHand.OnSeismicSlamSoundTriggered.AddListener(PlaySeismicSlamSound);
    }

    private void OnDisable()
    {
        player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;

        animationEventHelperMainHand.OnSeismicSlamTriggered.RemoveListener(PerformSeismicSlam);
        animationEventHelperMainHand.OnSeismicSlamSoundTriggered.RemoveListener(PlaySeismicSlamSound);
    }

    private void Start()
    {
        waitForFixedUpdate = new WaitForFixedUpdate();

        allSpriteRenderers.Add(player.spriteRenderer);

        if (InputManager.IsGamepad())
        {
            lastValidGamepadAimInput = Vector2.right;
        }

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
        playerBodycenterPosition = player.transform.position + new Vector3(0f, 0.65f, 0f);

        // CAELION - Grace of the Unscarred
        if (player.playerDetails.playerCharacterIndex == Character.Caelion)
        {
            player.isGraceOfTheUnscarredPassiveOn = Time.time - player.lastDamageHappenedTime > 8f ? true : false;
        }

        if (player.isGraceOfTheUnscarredPassiveOn)
        {
            if (!player.passiveTriggered)
            {
                player.passiveTriggered = true;

                player.healthEvent.CallGraceOfTheUnscarredSpecialMoveEvent(); // This is for displaying gem skin icon
                player.currentArmorValue += 0.2f;
                player.currentFireResistanceValue += 0.2f;
                player.currentWaterResistanceValue += 0.2f;
                player.currentAirResistanceValue += 0.2f;
                player.currentEarthResistanceValue += 0.2f;
                player.currentLightResistanceValue += 0.2f;
                player.currentDarkResistanceValue += 0.2f;
                StaticEventHandler.CallStatsChangedOnTheBookEvent();
            }
        }
        else
        {
            if (player.passiveTriggered)
            {
                player.passiveTriggered = false;

                player.healthEvent.CallGraceOfTheUnscarredSpecialMoveEndedEvent(); // This is for displaying gem skin icon
                player.currentArmorValue -= 0.2f;
                player.currentFireResistanceValue -= 0.2f;
                player.currentWaterResistanceValue -= 0.2f;
                player.currentAirResistanceValue -= 0.2f;
                player.currentEarthResistanceValue -= 0.2f;
                player.currentLightResistanceValue -= 0.2f;
                player.currentDarkResistanceValue -= 0.2f;
                StaticEventHandler.CallStatsChangedOnTheBookEvent();
            }
        }

        if (player.isGuardedOathActive && player.activeWeapon.GetCurrentOffHandWeapon() != null &&
            player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield)
        {
            foreach (var skill in player.currentlyUsedActiveUniqueSkills)
            {
                if (skill.Value.activeSkill == ActiveSkill.GuardedOath)
                {
                    player.mana.ResetReservedMana(skill.Value.activeUniqueSkillManaReserveCost);
                }
            }
        }

        // If player movement disabled then return
        if (isPlayerMovementDisabled) return;

        if (isPlayerRolling) return;

        // Frozen
        if ((player.moveStatus & MoveStatus.Frozen) != 0)
        {
            isPlayerRolling = false;
            player.meleeAttackMainHand.IsAttacking = false;

            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                {
                    // Trigger fire weapon event for precharge weapons
                    player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
                        AimDirection.Right, 0f, 0f, Vector3.zero, false);
                }

                if (frostCoroutine == null)
                {
                    frostCoroutine = StartCoroutine(FrostRoutine());
                }
            }
        }
        // Stun
        else if ((player.moveStatus & MoveStatus.Stun) != 0)
        {
            isPlayerRolling = false;
            player.meleeAttackMainHand.IsAttacking = false;
            player.animatePlayer.ResetAnimatonParameters();

            // Reset the state to idle
            player.animator.SetFloat(Settings.motionType, 0f);
            player.animator.SetBool(Settings.isIdle, true);

            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                {
                    // Trigger fire weapon event for precharge weapons
                    player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
                        AimDirection.Right, 0f, 0f, Vector3.zero, false);
                }
            }

            if (stunCoroutine == null)
            {
                stunCoroutine = StartCoroutine(StunRoutine());
            }
        }
        // Root
        else if ((player.moveStatus & MoveStatus.Root) != 0)
        {
            isPlayerRolling = false;
            player.meleeAttackMainHand.IsAttacking = false;
            player.animatePlayer.ResetAnimatonParameters();

            // Reset the state to idle
            player.animator.SetFloat(Settings.motionType, 0f);
            player.animator.SetBool(Settings.isIdle, true);

            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                {
                    // Trigger fire weapon event for precharge weapons
                    player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
                        AimDirection.Right, 0f, 0f, Vector3.zero, false);
                }
            }

            if (rootCoroutine == null)
            {
                rootCoroutine = StartCoroutine(RootRoutine());
            }
        }
        // Stagger
        else if((player.moveStatus & MoveStatus.Stagger) != 0)
        {
            isPlayerRolling = false;
            player.meleeAttackMainHand.IsAttacking = false;
            player.polygonCollider2D.enabled = false;
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                {
                    // Trigger fire weapon event for precharge weapons
                    player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.
                        weaponCurrentProjectile.isLaser, AimDirection.Right, 0f, 0f, Vector3.zero, false);
                }
            }

            StartCoroutine(Stagger());
        }
        else if (player.moveStatus == MoveStatus.Idle)
        {
            // Process the player weapon input
            WeaponAndActiveItemInput();
            // Process the player movement input
            MovementInput();
            // Player roll cooldown timer
            PlayerRollCooldownTimer();
            // Player parry cooldown timer
            PlayerParryCooldownTimer();
            // Process the player use item input
            UseItemInput();
            // Process the player use special move input
            SpecialMoveInput();
        }  
    }

    /// <summary>
    /// Player movement input
    /// </summary>
    private void MovementInput()
    {
        // Ensure movement doesn't override attack animations
        if (player.meleeAttackMainHand.IsAttacking || isParrying || player.isHuntersReachActive)
        {
            player.movementByVelocity.MovementInput = new Vector2(0f, 0f);
            return;
        }
            
        // Get movement input
        movementInput = InputManager.Instance.movement.action.ReadValue<Vector2>().normalized;

        float horizontalMovement = movementInput.x;
        float verticalMovement = movementInput.y;
        bool jumpButtonDown = InputManager.Instance.jumpButton.action.WasPerformedThisFrame();

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

        // If there is movement either move or roll
        if (direction != Vector2.zero)
        {
            if (!jumpButtonDown)
            {
                // Trigger movement event
                player.movementByVelocity.MoveRigidbody(direction.normalized, player.movementByVelocity.moveSpeed);

                // Trigger move animations
                player.animatePlayer.SetMovementAnimationParameters();
            }
            // Else player roll if not cooling down
            else if(playerRollCooldownTimer <= 0f)
            {
                if (InputManager.dodgeRollDisabled) return;

                PlayerRoll((Vector3)direction);
            }
        }
        // Else trigger idle event
        else
        {
            player.idle.StopVelocity();

            if (!player.meleeAttackMainHand.IsAttacking)
            {
                player.animatePlayer.SetIdleAnimationParameters();
            }
        }
    }

    private void PlayerRollCooldownTimer()
    {
        if (playerRollCooldownTimer >= 0f)
        {
            playerRollCooldownTimer -= Time.deltaTime;
        }
    }

    private void PlayerParryCooldownTimer()
    {
        if (playerParryDurationTimer >= 0f)
        {
            playerParryDurationTimer -= Time.deltaTime;
        }
        else if (isParrying)
        {
            isParrying = false;
            player.animatePlayer.SetIdleAnimationParameters();
        }

        if (playerParryCooldownTimer >= 0f)
        {
            playerParryCooldownTimer -= Time.deltaTime;
        }
    }

    private void PlayerRoll(Vector3 direction)
    {
        if (player.meleeAttackMainHand.IsAttacking) return;

        playerRollCoroutine = StartCoroutine(PlayerRollRoutine(direction));
    }

    /// <summary>
    /// Player roll coroutine
    /// </summary>
    IEnumerator PlayerRollRoutine(Vector3 direction)
    {
        // minDistance used to decide when to exit coroutine loop
        float minDistance = 0.2f;

        // Set roll animation parameters        
        player.animatePlayer.InitializeRollAnimationParameters();

        isPlayerRolling = true;

        Vector3 targetPosition = player.transform.position + (Vector3)direction * player.movementByVelocity.movementDetails.rollDistance;

        while (Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition, player.rb2D.position, player.movementByVelocity.movementDetails.rollSpeed,
                direction, isPlayerRolling);

            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;

        // Set cooldown timer
        playerRollCooldownTimer = player.movementByVelocity.movementDetails.rollCooldownTime;

        player.animatePlayer.SetIdleAnimationParameters();
        player.transform.position = targetPosition;
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
        AttackDirection attackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out attackDirection);

        // Fire weapon input
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Process the player active item input
        FireActiveItemInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Process the player parry input
        ParryWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Switch weapon input
        SwitchWeaponInput();
    }

    private void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection, 
        out AttackDirection playerAttackDirection)
    {
        playerAngleDegrees = 0f;
        weaponAngleDegrees = 0f;
        weaponDirection = Vector2.zero;

        Vector3 aimDirectionVec = Vector3.right; // Default direction
        Vector3 playerPos = transform.position;
        Vector3 weaponShootPos = player.activeWeapon.GetMainHandShootPositionUp();

        Vector2 rightStickInput = InputManager.Instance.gamepadAim.action.ReadValue<Vector2>();

        if (InputManager.IsGamepad())
        {
            if (rightStickInput.sqrMagnitude > 0.01f)
            {
                lastValidGamepadAimInput = rightStickInput.normalized;
            }

            if (lastValidGamepadAimInput.sqrMagnitude > 0.01f)
            {
                aimDirectionVec = lastValidGamepadAimInput;
                weaponDirection = aimDirectionVec;
                weaponAngleDegrees = HelperUtilities.GetAngleFromVector(aimDirectionVec);
                playerAngleDegrees = weaponAngleDegrees;
            }
        }
        else
        {
            lastValidGamepadAimInput = Vector2.zero;

            Vector3 mouseWorldPos = HelperUtilities.GetMouseWorldPosition();

            weaponDirection = mouseWorldPos - weaponShootPos;
            Vector3 playerDirection = mouseWorldPos - playerPos;

            weaponAngleDegrees = HelperUtilities.GetAngleFromVector(weaponDirection);
            playerAngleDegrees = HelperUtilities.GetAngleFromVector(playerDirection);
        }

        // Direction parsing
        playerAimDirection = HelperUtilities.GetAimDirection(playerAngleDegrees);
        aimDirection = playerAimDirection;

        playerAttackDirection = HelperUtilities.GetAttackDirection(playerAngleDegrees);
        attackDirection = playerAttackDirection;

        // Apply results
        player.aimWeapon.Aim(playerAimDirection, playerAttackDirection, playerAngleDegrees);
        player.animatePlayer.InitializeAimAnimationParameters();
        player.animatePlayer.SetAimWeaponAnimationParameters(playerAimDirection, playerAttackDirection);
    }

    private void FireWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        // If glossary book is open, disable attack
        if (GameManager.Instance.glossaryBookOpen) return;

        // If pop-up window is open, disable attack
        if (GameManager.Instance.popUpWindowOpen) return;

        if (player.activeWeapon.GetCurrentMainHandWeapon() == null) return;

        if (InputManager.firingDisabled) return; // For tutorial issues

        // Fire when left mouse button is clicked - melee
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
        {
            if (InputManager.Instance.attack.action.WasPressedThisFrame() && !IsClickingSpecificUILayer() && !isParrying)
            {
                // MAIN-HAND
                if (player.activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.isMeleeWeapon == true && !player.meleeAttackMainHand.IsAttacking)
                {
                    AttackShape mainHandAttackType = DetermineAttackType(player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails);

                    player.meleeAttackEvent.CallAttackEvent(aimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), mainHandAttackType, MeleeHand.MainHand);
                }

                // OFF-HAND
                if (player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.isMeleeWeapon == true && !player.meleeAttackMainHand.IsAttacking)
                {
                    AttackShape offHandAttackType = DetermineAttackType(player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails);

                    player.meleeAttackEvent.CallAttackEvent(aimDirection, player.activeWeapon.GetCurrentOffHandWeapon(), offHandAttackType, MeleeHand.OffHand);
                }
            }

            return;
        }
    
        // Fire for precharge weapons (fire once after precharge)
        if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0f)
        {
            if (InputManager.Instance.attack.action.IsPressed() && !player.activeWeapon.GetCurrentMainHandWeapon().firingCompletedIfWeaponIsPrecharged 
                && !IsClickingSpecificUILayer()) // Only trigger once per hold
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                {
                    if (!player.activeWeapon.GetCurrentMainHandWeapon().onCooldown || !player.activeWeapon.GetCurrentMainHandWeapon().onPrecharge)
                    {
                        player.meleeAttackMainHand.IsAttacking = true;
                    }

                    player.meleeAttackEvent.CallAttackEvent(playerAimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.None, MeleeHand.None);
                }

                // Start precharge process (firePreviousFrame is false because firing hasn't happened yet)
                player.fireWeaponEvent.CallFireWeaponEvent(true, true, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser, 
                    playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false);
            }
        }
        // Fire for non-precharge weapons (fire once per press)
        else if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime == 0f && InputManager.Instance.attack.action.WasPressedThisFrame()
            && !IsClickingSpecificUILayer())
        {
            isSoundPlayed = false;

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow || 
                player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow)
            {
                if (!player.activeWeapon.GetCurrentMainHandWeapon().onCooldown)
                {
                    player.meleeAttackMainHand.IsAttacking = true;

                    // Trigger fire weapon event
                    player.meleeAttackEvent.CallAttackEvent(playerAimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.None, MeleeHand.None);
                }
            }

            // Fire event (only once per press)
            player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
                playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false,
                player.isArrowOfTheSevenActive);
        }

        // Reset when fire button is released
        if (InputManager.Instance.attack.action.WasReleasedThisFrame())
        {
            isSoundPlayed = false;

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0f)
            {
                // Reset firing flag when releasing button for precharge weapons
                player.activeWeapon.GetCurrentMainHandWeapon().firingCompletedIfWeaponIsPrecharged = false;
            }

            // Stop firing
            player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
                playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false);
        }
    }

    private AttackShape DetermineAttackType(WeaponDetailsSO weaponDetails)
    {
        if (weaponDetails.hasSwing) return AttackShape.Swing;
        if (weaponDetails.hasThrust) return AttackShape.Thrust;

        return AttackShape.Swing; // default fallback
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
                if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Decoy)
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
                            healthPotionDrinkCoroutine = StartCoroutine(AddHealthCoroutine((int)(50f / player.health.GetMaximumHealth() * 100)));

                            player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge--;

                            // Call weapon fired event
                            player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());
                        }
                    }
                }
                else if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Summoner)
                {
                    if (GameManager.Instance.GetCurrentRoom().isClearedOfEnemies)
                    {
                        GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.SummonerFailed);
                        return;
                    }

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

                        enemy.EnemyInitialization(enemy.enemyAI.enemyDetails, 15, GameManager.Instance.GetCurrentDungeonLevel());
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
                    player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, true);
                }
            }
        }
    }

    /// <summary>
    /// Parry weapon input
    /// </summary>
    private void ParryWeaponInput(Vector3 weaponDirection, float weaponAngleDegrees, float playerAngleDegrees, AimDirection playerAimDirection)
    {
        if (InputManager.TutorialEnabled && InputManager.parryDisabled) return;

        if (player.activeWeapon.GetCurrentMainHandWeapon() != null && !player.meleeAttackMainHand.IsAttacking)
        {
            switch (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass)
            {
                case WeaponClass.Sword:
                case WeaponClass.Axe:
                case WeaponClass.Hammer:
                case WeaponClass.Spear:
                case WeaponClass.Dagger:
                case WeaponClass.Claw:
                    if (InputManager.Instance.parryButton.action.WasPressedThisFrame() && !isParrying && playerParryCooldownTimer < 0)
                    {
                        isParrying = true;
                        player.animatePlayer.ResetAnimatonParameters();
                        player.animatePlayer.InitializeParryAnimationParameters();

                        // Set cooldown timer
                        playerParryCooldownTimer = playerParryCooldownDuration;
                        playerParryDurationTimer = playerParryEffectiveDuration;
                    }
                    break;
                default:
                    break;
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

            yield return new WaitForSeconds(0.5f);
        }

        healthPotionDrinkCoroutine = null;

        yield return null;
    }

    private void HealthEvent_OnHealthChanged(HealthEvent healthEvent, HealthEventArgs healthEventArgs)
    {
        // Trigger reset prechager mechanism in case a hit taken during the precharge
        player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, false, AimDirection.Right, 0f, 0f, Vector3.zero, false);
    }

    public bool IsClickingSpecificUILayer()
    {
        PointerEventData pointerData = new PointerEventData(GameManager.Instance.eventSystem)
        {
            position = Mouse.current.position.ReadValue()
        };

        List<RaycastResult> results = new List<RaycastResult>();
        GameManager.Instance.uiRaycaster.Raycast(pointerData, results);

        foreach (RaycastResult result in results)
        {
            if ((GameManager.Instance.specialUILayerMask.value & (1 << result.gameObject.layer)) != 0)
            {
                return true; // Clicked on a UI element within the target layer
            }
        }

        return false;
    }

    private void SwitchWeaponInput(bool onStart = false)
    {
        float scrollValue = (InputManager.Instance.switchWeaponByWheel.action.ReadValue<Vector2>().normalized).y;

        if (InputManager.switchDisabled) return;

        bool switchForward = false;
        bool switchBack = false;

        // If wheel is not active then check switch buttons
        if (Mathf.Abs(scrollValue) < 0.05f)
        {
            switchForward = InputManager.Instance.switchWeaponForward.action.WasPressedThisFrame();
            switchBack = InputManager.Instance.switchWeaponBack.action.WasPressedThisFrame();
        }

        // Switch weapon if mouse scroll wheel selecetd
        if (scrollValue < 0f || switchBack)
        {
            PreviousWeaponSet(true, onStart);
        }

        if (scrollValue > 0f || switchForward)
        {

            NextWeaponSet(true, true, onStart);
        }
    }

    public void NextWeaponSet(bool onlySwitch, bool mouseWheel, bool onStart, int setNumber = 0)
    {
        if (setNumber > 0)
        {
            // Cache previous weapon slot index
            InventoryManager.Instance.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

            // Set previous index
            player.previousSetIndex = player.currentWeaponSlotSetIndex;

            // Increment the current weapon slot set index
            player.currentWeaponSlotSetIndex = setNumber;

            SetWeaponSetByIndex(onlySwitch, onStart);

            InventoryManager.Instance.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
        }
        else if (mouseWheel)
        {
            // Cache previous weapon slot index
            InventoryManager.Instance.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

            // Set previous index
            player.previousSetIndex = player.currentWeaponSlotSetIndex;

            // Increment the current weapon slot set index
            player.currentWeaponSlotSetIndex++;

            if (player.currentWeaponSlotSetIndex > 3)
            {
                player.currentWeaponSlotSetIndex = 1;
            }

            SetWeaponSetByIndex(onlySwitch, onStart);

            InventoryManager.Instance.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;

            HighlightWeaponSetButton(); //Light and color settings
        }
        else
        {
            if (player.currentWeaponSlotSetIndex == setNumber) return;

            // Cache previous weapon slot index
            InventoryManager.Instance.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

            player.currentWeaponSlotSetIndex = setNumber;
            SetWeaponSetByIndex(onlySwitch, onStart);

            InventoryManager.Instance.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;

            HighlightWeaponSetButton(); //Light and color settings
        }
    }

    public void PreviousWeaponSet(bool onlySwitch, bool onStart)
    {
        // Cache previous weapon slot index
        InventoryManager.Instance.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

        player.previousSetIndex = player.currentWeaponSlotSetIndex;

        // Decrease the current weapon slot set index
        player.currentWeaponSlotSetIndex--;

        if (player.currentWeaponSlotSetIndex < 1)
        {
            player.currentWeaponSlotSetIndex = 3;
        }

        SetWeaponSetByIndex(onlySwitch, onStart);
        InventoryManager.Instance.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;

        HighlightWeaponSetButton();
    }

    public void SetWeaponSetByIndex(bool onlySwitch, bool onStart, bool dragFromInventory = false, bool dragToInventory = false, bool inventorySwitch = false)
    {
        // WEAPON SLOTS SWITCH
        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null) // If next slot contains a main-hand weapon
        {
            player.setActiveWeaponEvent.CallSetActiveWeaponAtMainHandEvent(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0],
                player.currentWeaponSlotSetIndex, onStart, onlySwitch);

            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.OneHanded)
            {
                // If both hands are equipped with one-handed weapon, then swap is true
                if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
                {
                    player.setActiveWeaponEvent.CallOneHandWeaponEquipEvent(true);
                }
                else
                {
                    player.setActiveWeaponEvent.CallOneHandWeaponEquipEvent();
                }
            }
            else if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
            {
                player.setActiveWeaponEvent.CallTwoHandWeaponEquipEvent();
            }
        }
        else
        {
            // There is no weapon in this set
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent();
        }

        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] != null) // If next slot contains a off-hand weapon
        {
            player.setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1], 
                player.currentWeaponSlotSetIndex, onStart, onlySwitch);
        }
        else
        {
            if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null) // If next slot contains a main-hand weapon
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

        // Update stats after weapon switch
        player.RecalculateSecondaryStats();

        // Book UI SWITCH
        if(!dragFromInventory && !dragToInventory && !inventorySwitch) StaticEventHandler.CallWeaponSwitchedEventForBook();
    }

    /// <summary>
    /// Highlight weapon set button to be seen clearly
    /// </summary>
    private void HighlightWeaponSetButton()
    {       
        // Get the button container
        Transform buttonContainer = GameManager.Instance.bookView.transform.GetChild(1).GetChild(4).GetChild(0);

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
    /// Frost routine
    /// </summary>
    IEnumerator FrostRoutine()
    {
        player.movementByVelocity.moveSpeed = 0f;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        player.animator.SetBool(Settings.isFrozen, true);

        yield return new WaitForSeconds(3f);

        player.moveStatus &= ~MoveStatus.Frozen; // Remove frozen
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallFrostCuredEvent();
        player.animator.SetBool(Settings.isFrozen, false);
        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.GetBaseMoveSpeed() + player.CurrentAgilityValue * 0.25f;
        frostCoroutine = null;
    }

    /// <summary>
    /// Root routine
    /// </summary>
    IEnumerator RootRoutine()
    {
        player.movementByVelocity.moveSpeed = 0f;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForSeconds(2f);

        player.moveStatus &= ~MoveStatus.Root; // Remove root
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallRootCuredEvent();
        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.GetBaseMoveSpeed() + player.CurrentAgilityValue * 0.25f;
        rootCoroutine = null;
    }

    /// <summary>
    /// Stun routine
    /// </summary>
    IEnumerator StunRoutine()
    {
        player.movementByVelocity.moveSpeed = 0f;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        player.animator.SetBool(Settings.isStunned, true);

        yield return new WaitForSeconds(2f);

        player.moveStatus &= ~MoveStatus.Stun; // Remove stun
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallStunCuredEvent();
        player.animator.SetBool(Settings.isStunned, false);
        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.GetBaseMoveSpeed() + player.CurrentAgilityValue * 0.25f;
        stunCoroutine = null;
    }

    /// <summary>
    /// Use special move of the selected character
    /// </summary>
    private void SpecialMoveInput()
    {
        if (InputManager.TutorialEnabled)
        {
            if (InputManager.specialSkillOneDisabled || InputManager.specialSkillTwoDisabled || InputManager.specialSkillThreeDisabled) return;
        }

        int inputSlotNumber = InputManager.Instance.specialMoveOne.action.WasPressedThisFrame() ? 1 :
            InputManager.Instance.specialMoveTwo.action.WasPressedThisFrame() ? 2 :
            InputManager.Instance.specialMoveThree.action.WasPressedThisFrame() ? 3 : -1;

        if (inputSlotNumber > 0 && !player.specialMovesCooldownCheckArray[inputSlotNumber - 1])
        {
            if (!player.currentlyUsedActiveUniqueSkills.ContainsKey(inputSlotNumber)) return;

            if (player.mana.GetCurrentMana() >= player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost) // Check if there is enough mana
            {
                switch (player.playerDetails.playerCharacterIndex)
                {
                    case Character.Caelion:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.SeismicSlam:
                                player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                SeismicSlamProcess();
                                player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.SeismicSlam, inputSlotNumber);
                                break;
                            case ActiveSkill.Valor:
                                if (!player.isValorActive)
                                {
                                    player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                    Valor(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Valor, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.ShieldBash:
                                // OFF-HAND
                                if (player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Shield)
                                {
                                    player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                    // Shield bash attack
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    player.meleeAttackEvent.CallAttackEvent(aimDirection, player.activeWeapon.GetCurrentOffHandWeapon(),
                                        AttackShape.Cone, MeleeHand.OffHand, false, true);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ShieldBash, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.BreakTheLine:
                                if (!player.isBreakTheLineActive && player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Shield)
                                {
                                    player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                    BreakTheLine(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BreakTheLine, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.GuardedOath:
                                if (player.isGuardedOathActive)
                                {
                                    RemoveGuardedOathEffects(inputSlotNumber);
                                }
                                else if (!player.isGuardedOathActive && player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Shield)
                                {
                                    if (player.mana.GetCurrentMana() >= player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaReserveCost)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaReserveCost, true); // Mana reserved
                                        GuardedOath(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.GuardedOath, inputSlotNumber);
                                    }
                                }

                                break;
                            default:
                                break;
                        }
                        break;
                    case Character.Morven:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.UmbralMist:
                                if (!player.isUmbralMistActive)
                                {
                                    player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                    UmbralMist(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.UmbralMist, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.Stealth:
                                if (!player.isStealthActive)
                                {
                                    player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                    Stealth(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Stealth, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.BloodDrain:
                                // DAGGER CHECK
                                if (player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentMainHandWeapon() != null && !isParrying)
                                {
                                    if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger &&
                                        player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger && !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Blood drain attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        BloodDrain();
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BloodDrain, inputSlotNumber);
                                    }
                                }

                                break;
                            case ActiveSkill.ShadowStep:
                                if (!player.isShadowStepActive)
                                {
                                    player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                    ShadowStep(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ShadowStep, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.CullTheMeek:
                                // DAGGER CHECK
                                if (player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentMainHandWeapon() != null && !isParrying)
                                {
                                    if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger &&
                                        player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Dagger && !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Cull the meek attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        CullTheMeek();
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.CullTheMeek, inputSlotNumber);
                                    }
                                }

                                break;
                            default:
                                break;
                        }

                        break;
                    case Character.Nyveran:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.Penetrate:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow ||
                                    player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isPenetrateActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Penetrate attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        Penetrate(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Penetrate, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.TripleThreat:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow ||
                                    player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isTripleThreatActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Triple threat attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        TripleThreat(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.TripleThreat, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.BindingArrow:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow ||
                                    player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isBindingArrowActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Triple threat attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        BindingArrow(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BindingArrow, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.ArrowsOfTheSevenPlagues:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Bow ||
                                    player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isArrowOfTheSevenActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Arrow of The Seven Plagues attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        ArrowOfTheSevenPlagues(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ArrowsOfTheSevenPlagues, inputSlotNumber);
                                    }
                                }

                                break;
                            case ActiveSkill.HuntersReach:
                                if (!player.isHuntersReachActive && !isPlayerRolling)
                                {
                                    player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                    // Use Hunter's Reach - Grapple Hook
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    HuntersReach(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.HuntersReach, inputSlotNumber);
                                }

                                break;
                            default:
                                break;
                        }

                        break;
                    case Character.Karnag:
                        break;
                    case Character.Nyxa:
                        break;
                    case Character.Mycara:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.Blizzard:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff) 
                                {
                                    if (!player.isBlizzardActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Blizzard attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        Blizzard(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Blizzard, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.MycarasSeal:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isMycarasSealActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Activate Mycara's Seal
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        MycarasSeal(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.MycarasSeal, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.SheerCold:
                                // STAFF CHECK
                                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                                {
                                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff &&
                                        !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Sheer cold attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        SheerCold(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.SheerCold, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.Icebreaker:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff &&
                                    !player.meleeAttackMainHand.IsAttacking)
                                {
                                    if (!player.isIceBreakerActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Ice breaker attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        IceBreaker(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Icebreaker, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.AbsoluteZero:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isAbsoluteZeroActive)
                                    {
                                        player.mana.ConsumeMana(player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeUniqueSkillManaCost);

                                        // Absolute Zero attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        AbsoluteZero(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.AbsoluteZero, inputSlotNumber);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case Character.Kynara:
                        break;
                    case Character.Nymara:
                        break;
                    default:
                        break;
                }


            }
            else
            {
                Debug.Log("NOT ENOUGH MANA MY LORD!");
            }
        }
    }

    /// <summary>
    /// Execute Seismic Slam special move
    /// </summary>
    private void SeismicSlamProcess()
    {
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.firstActiveSkillDetails.activeUniqueSkillSoundEffectTwo);
        player.animator.SetTrigger("seismicSlam");
    }

    private void PlaySeismicSlamSound()
    {
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.firstActiveSkillDetails.activeUniqueSkillSoundEffectOne);
    }

    private void PerformSeismicSlam()
    {
        GameObject slamEffectObject = Instantiate(activeSkillTypeThreeAnimator.gameObject, transform.position, Quaternion.identity);

        // Get all colliders within the radius of the seismic slam
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, player.seismicSlamCircleRadius);

        slamEffectObject.GetComponent<Animator>().SetTrigger("slam");

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
                    enemy.GetComponent<EnemyAI>().TriggerKnockback((enemy.transform.position - transform.position).normalized);
                }

                if (enemy.health != null)
                {
                    enemy.health.TakeDamage(player.seismicSlamDamage, transform.position, enemy.health.transform.position, false, null, MeleeHand.None);
                }
            }
        }

        Destroy(slamEffectObject, 4f); // Destroy slam object after animation completed
    }

    /// <summary>
    /// Execute Valor special move
    /// </summary>
    private void Valor(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier))
        {
            if (!player.isValorActive)
            {
                activeSkillTypeOneAnimator.SetBool("valor", true);
                player.healthEvent.CallValorSpecialMoveEvent(); // This is for displaying valor icon
                player.isValorActive = true;
                StartCoroutine(ValorRoutine(slotIndex));
            }
        }
    }

    IEnumerator ValorRoutine(int slotIndex)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        player.isValorActive = false;
        player.healthEvent.CallValorWoreOffEvent();
        activeSkillTypeOneAnimator.SetBool("valor", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute BreakTheLine special move
    /// </summary>
    private void BreakTheLine(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier))
        {
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            int originalMinDamage = player.currentMainHandMinDamageValue;
            int originalMaxDamage = player.currentMainHandMaxDamageValue;

            // ENABLE SKILL EFFECTS
            player.additionalSpeedModifier += 2; // Increase speed
            player.currentMainHandMinDamageValue = (int)(player.currentMainHandMinDamageValue * 1.4f);
            player.currentMainHandMaxDamageValue = (int)(player.currentMainHandMaxDamageValue * 1.4f);
            Weapon droppedShield = player.activeWeapon.GetCurrentOffHandWeapon();
            player.UpdateDamageValues();
            player.UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();

            player.healthEvent.CallBreakTheLineSpecialMoveEvent(); // This is for displaying valor icon

            if (!player.isBreakTheLineActive)
            {
                player.isBreakTheLineActive = true;
                StartCoroutine(BreakTheLineRoutine(slotIndex, droppedShield, originalMinDamage, originalMaxDamage));
            }
        }
    }

    IEnumerator BreakTheLineRoutine(int slotIndex, Weapon droppedShield, int originalMinDamage, int originalMaxDamage)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        // DISABLE SKILL EFFECTS
        player.additionalSpeedModifier -= 2; // Reset speed
        player.currentMainHandMinDamageValue = originalMinDamage;
        player.currentMainHandMinDamageValue = originalMaxDamage;
        player.setActiveWeaponEvent.CallSetActiveWeaponAtOffHandEvent(droppedShield, player.currentWeaponSlotSetIndex, false, false);
        player.UpdateDamageValues();
        player.UpdateSpeedValue();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        player.isBreakTheLineActive = false;
        player.healthEvent.CallBreakTheLineWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }


    /// <summary>
    /// Execute GuardedOath special move
    /// </summary>
    private void GuardedOath(int slotIndex)
    {
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        activeSkillTypeTwoAnimator.SetBool("oath", true);

        // ENABLE SKILL EFFECTS
        StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

        player.healthEvent.CallGuardedOathSpecialMoveEvent(); // This is for displaying guarded oath icon

        // EFFECTS
        player.additionalMeleeDamageModifer -= 0.1f;
        player.additionalShieldArmorModifier++;
        player.additionalBlockModifier += 0.2f;
        player.additionalSpeedModifier -= 0.5f;

        player.UpdateDamageValues();
        player.UpdateBlockValue();
        player.UpdateArmorValues();
        player.UpdateSpeedValue();

        if (!player.isGuardedOathActive)
        {
            player.isGuardedOathActive = true;
            Debug.Log("isGuardedOath is " + player.isGuardedOathActive);
        }
    }

    /// <summary>
    /// Remove GuardedOath effects
    /// </summary>
    public void RemoveGuardedOathEffects(int slotIndex)
    {
        player.isGuardedOathActive = false;

        activeSkillTypeTwoAnimator.SetBool("oath", false);

        // EFFECTS WORE OFF
        player.additionalMeleeDamageModifer += 0.1f;
        player.additionalShieldArmorModifier--;
        player.additionalBlockModifier -= 0.2f;
        player.additionalSpeedModifier += 0.5f;

        player.UpdateDamageValues();
        player.UpdateBlockValue();
        player.UpdateArmorValues();
        player.UpdateSpeedValue();

        StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

        player.mana.ResetReservedMana(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillManaReserveCost);
        player.healthEvent.CallGuardedOathSpecialMoveEndEvent(); // This is for displaying guarded oath icon
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute Umbral Mist special move
    /// </summary>
    private void UmbralMist(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier))
        {
            player.healthEvent.CallUmbralMistSpecialMoveEvent(); // This is for displaying umbral mist icon

            if (!player.isUmbralMistActive)
            {
                player.isUmbralMistActive = true;
                StartCoroutine(UmbralMistRoutine(slotIndex));
            }
        }
    }

    IEnumerator UmbralMistRoutine(int slotIndex)
    {
        GameObject umbralMistObject = Instantiate(activeSkillTypeTwoAnimator.gameObject, transform.position, Quaternion.identity);

        umbralMistObject.GetComponent<Animator>().SetBool("umbralMist", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        player.isUmbralMistActive = false;

        player.healthEvent.CallUmbralMistWoreOffEvent();
        umbralMistObject.GetComponent<Animator>().SetBool("umbralMist", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(umbralMistObject, 2f); // Destroy mist object after animation completed
    }

    /// <summary>
    /// Execute Stealth special move
    /// </summary>
    private void Stealth(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration* (1 + player.buffDurationModifier))
        {
            // EFFECTS
            player.health.isDamageable = false;

            player.healthEvent.CallStealthSpecialMoveEvent(); // This is for displaying stealth icon

            // Set player's stealth status to true
            if (!player.isStealthActive)
            {
                player.isStealthActive = true;
            }
        }

        // Get the current color of the sprite renderer
        Color currentColor = player.spriteRenderer.color;
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        // Set the alpha value to 0.3 (30% opacity)
        currentColor.a = 0.3f;

        // Apply the modified color back to the sprite renderer
        player.spriteRenderer.color = currentColor;

        // Start the coroutine to maintain the alpha value during stealth
        StartCoroutine(StealthRoutine(slotIndex));
    }

    IEnumerator StealthRoutine(int slotIndex)
    {
        float stealthDuration = player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration * (1 + player.buffDurationModifier);

        yield return new WaitForSeconds(stealthDuration);

        Unstealth();

    }

    /// <summary>
    /// Unstealth from special move
    /// </summary>
    public void Unstealth()
    {
        if (unstealthRoutine != null) return;

        int slotIndex = -1;

        foreach (KeyValuePair<int, ActiveUniqueSkillDetailsSO> skill in player.currentlyUsedActiveUniqueSkills)
        {
            if(skill.Value.activeSkill == ActiveSkill.Stealth)
            {
                slotIndex = skill.Key;
            }
        }

        // Trigger cooldown and ui components
        player.healthEvent.CallStealthWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        unstealthRoutine = StartCoroutine(UnstealthRoutine());
    }

    IEnumerator UnstealthRoutine()
    {
        // Set immunity
        player.health.isDamageable = false;

        // Set player's stealth status to false
        player.isStealthActive = false;

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
    /// Execute Blood Drain speical move
    /// </summary>
    private void BloodDrain()
    {
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.Swing, MeleeHand.MainHand, true);
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentOffHandWeapon(), AttackShape.Swing, MeleeHand.OffHand, true);
    }

    /// <summary>
    /// Execute Shadowstep special move
    /// </summary>
    private void ShadowStep(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier))
        {
            if (!player.isShadowStepActive)
            {
                player.isShadowStepActive = true;

                activeSkillTypeThreeAnimator.SetBool("shadowStep", true);
                player.healthEvent.CallShadowStepSpecialMoveEvent(); // This is for displaying shadow step icon
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

                //EFFECTS
                player.additionalSpeedModifier++;
                player.UpdateSpeedValue();

                StaticEventHandler.CallStatsChangedOnTheBookEvent();

                StartCoroutine(ShadowStepRoutine(slotIndex));
            }
        }
    }

    IEnumerator ShadowStepRoutine(int slotIndex)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        player.isShadowStepActive = false;
        activeSkillTypeThreeAnimator.SetBool("shadowStep", false);

        //EFFECTS ENDED
        player.additionalSpeedModifier--;
        player.UpdateSpeedValue();

        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
        player.healthEvent.CallShadowStepWoreOffEvent();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    /// <summary>
    /// Execute Cull the Meek special move
    /// </summary>
    private void CullTheMeek()
    {
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.Thrust, MeleeHand.MainHand, false, false, true);
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentOffHandWeapon(), AttackShape.Thrust, MeleeHand.OffHand, false, false, true);
    }

    /// <summary>
    /// Execute Penetrate special move
    /// </summary>
    private void Penetrate(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        Debug.Log("Penetrate is " + player.isPenetrateActive);

        player.isPenetrateActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;
        activeSkillTypeTwoAnimator.SetTrigger("penetrate");

        // Trigger fire weapon event
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, true);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isPenetrateActive = false));
    }

    /// <summary>
    /// Execute Triple Threat special move
    /// </summary>
    private void TripleThreat(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isTripleThreatActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;

        activeSkillTypeThreeAnimator.SetTrigger("tripleThreat");

        // Trigger fire weapon event
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, true);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isTripleThreatActive = false));
    }

    /// <summary>
    /// Execute Binding Arrow special move
    /// </summary>
    private void BindingArrow(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isBindingArrowActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, true);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isBindingArrowActive = false));
    }

    /// <summary>
    /// Execute Arrow of the Seven Plagues special move
    /// </summary>
    private void ArrowOfTheSevenPlagues(int slotIndex)
    {
        player.isArrowOfTheSevenActive = true;
        player.healthEvent.CallSevenArrowsSpecialMoveEvent();
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        StartCoroutine(ArrowOfTheSevenPlaguesRoutine(slotIndex));
    }

    IEnumerator ArrowOfTheSevenPlaguesRoutine(int slotIndex)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        player.isArrowOfTheSevenActive = false;
        player.healthEvent.CallSevenArrowsWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true;
    }

    /// <summary>
    /// Execute Hunter's Reach special move    
    /// </summary>
    private void HuntersReach(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        Debug.Log("Hunter's reach is " + player.isHuntersReachActive);

        player.isHuntersReachActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;

        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.playerDetails.grappleDetails.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false,
            player.playerDetails.grappleDetails);
    }

    /// <summary>
    /// Execute Blizzard special move
    /// </summary>
    private void Blizzard(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier))
        {
            if (!player.isBlizzardActive)
            {
                player.isBlizzardActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(BlizzardRoutine(slotIndex));
            }
        }
    }

    IEnumerator BlizzardRoutine(int slotIndex)
    {
        activeSkillTypeOneAnimator.gameObject.SetActive(true);

        GameObject blizzardObject = Instantiate(activeSkillTypeOneAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(), Quaternion.identity);

        blizzardObject.GetComponent<Animator>().SetBool("blizzard", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isBlizzardActive = false;

        blizzardObject.GetComponent<Animator>().SetBool("blizzard", false);
        activeSkillTypeOneAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(blizzardObject, 2f); // Destroy blizzard object after animation completed
    }

    /// <summary>
    /// Execute Force Field special move
    /// </summary>
    private void MycarasSeal(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);

            activeSkillTypeTwoAnimator.SetBool("mycarasSeal", true);
            player.healthEvent.CallMycarasSealSpecialMoveEvent(); // This is for displaying valor icon
            player.isMycarasSealActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            StartCoroutine(MycarasSealRoutine(slotIndex));

            GameObject forceFieldObject = player.forcefieldTransform.gameObject;
            forceFieldObject.SetActive(true);
        }
    }

    IEnumerator MycarasSealRoutine(int slotIndex)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isMycarasSealActive = false;
        player.healthEvent.CallMycarasSealWoreOffEvent();
        activeSkillTypeTwoAnimator.SetBool("mycarasSeal", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute Sheer Cold special move
    /// </summary>
    private void SheerCold(int slotIndex)
    {
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.Cone, MeleeHand.MainHand,
            false, false, false, true);
    }

    /// <summary>
    /// Execute Ice Breaker special move
    /// </summary>
    private void IceBreaker(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isIceBreakerActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, true, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false, null,
            player.playerDetails.iceBreakerDetails);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isIceBreakerActive = false));
    }

    /// <summary>
    /// Execute Absolute Zero special move
    /// </summary>
    private void AbsoluteZero(int slotIndex)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier))
        {
            if (!player.isAbsoluteZeroActive)
            {
                player.isAbsoluteZeroActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(AbsoluteZeroRoutine(slotIndex));
            }
        }
    }

    IEnumerator AbsoluteZeroRoutine(int slotIndex)
    {
        activeSkillTypeFourAnimator.gameObject.SetActive(true);

        GameObject absoluteZeroObject = Instantiate(activeSkillTypeFourAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(), Quaternion.identity);

        absoluteZeroObject.GetComponent<Animator>().SetBool("absoluteZero", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillEffectiveDuration
            * (1 + player.buffDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isAbsoluteZeroActive = false;

        absoluteZeroObject.GetComponent<Animator>().SetBool("absoluteZero", false);
        activeSkillTypeFourAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(absoluteZeroObject, 2f); // Destroy blizzard object after animation completed
    }

    IEnumerator NullifyBooleanAfterTwoFrame(System.Action setFalseAction)
    {
        yield return null;
        yield return null;

        setFalseAction?.Invoke();
    }

    /// <summary>
    /// Execute Teleport special move
    /// </summary>
    private void Teleport()
    {
        if (player.specialMovesCooldownCheckArray[0] == false)
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
            //SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.activeSkillOneSoundEffect);

            if (player.threeSecInvincilibityAfterTeleportEnabled)
            {
                StartCoroutine(EnableInvincibility());
            }
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

    /// <summary>
    /// Enable Invincibility
    /// </summary>
    IEnumerator EnableInvincibility()
    {
        player.health.isDamageable = false;

        // 4 is duration of invincibility
        int iterations = Mathf.RoundToInt(4 / Health.spriteFlashInterval / 2);

        // Flash effect
        while (iterations > 0)
        {
            player.health.flashManager.WhiteFlashCharacter(player.spriteRenderer);
            yield return new WaitForSeconds(Health.spriteFlashInterval);

            player.health.flashManager.UnflashCharacter(player.spriteRenderer);
            yield return new WaitForSeconds(Health.spriteFlashInterval);

            iterations--;

            yield return null;
        }

        player.health.isDamageable = true;
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
            float projectileSpeed = currentProjectile.projectileSpeed;

            // Get Gameobject with IFireable component
            IFireable projectile = (IFireable)PoolManager.Instance.ReuseComponent(projectilePrefab, meteorStartsToFallPosition, Quaternion.identity);

            // Initialize projectile
            projectile.InitializeProjectile(null, false, currentProjectile, aimAngle, weaponAimAngle, projectileSpeed, direction, false, true);

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
    /// Execute Head Shot special move
    /// </summary>
    private void HeadShot()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        //Reset precharge for loading again
        isSoundPlayed = false;

        player.meleeAttackMainHand.IsAttacking = true;
        player.meleeAttackEvent.CallAttackEvent(playerAimDirection, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.None, MeleeHand.None);

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, true);
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
                if (player.selectedActiveItem?.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.BobbyPin && chest.bobbyPinTried == false)
                {
                    if (InputManager.Instance.activeItem.action.IsPressed())
                    {
                        chest.bobbyPinTried = true;
                        int diceRoll = Random.Range(1, 101) + (int)(player.additionalLockpickingModifier * 100);

                        if (diceRoll > 50)
                        {
                            if (chest.chestState == ChestState.closed && !chest.dropCompleted)
                            {
                                chest.bobbyPinTrySuccessful = true;
                                SoundEffectManager.Instance.PlaySoundEffect(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemSwingSoundEffect);
                                iusable.StartChestProcess();
                            }
                        }
                        else
                        {
                            // Lockpick failed
                            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.BobbyPinFailed);
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
                if (collider2D.GetComponent<Environment>() != null) return;

                // Only interactable objects have capsule colliders. So if it's nut null, it means collider is an interactable (like NPC)
                if (collider2D.GetComponent<CapsuleCollider2D>() != null && collider2D.tag != Settings.playerTag && collider2D.tag != Settings.enemyTag)
                {
                    Interaction interaction = collider2D.GetComponent<Interaction>();
                    NPC npc = interaction.GetComponent<NPC>();

                    if (npc != null && npc.npcType == NpcType.Gambler) return;

                    interaction.TriggerDialogue();
                }
            }
        }
    }

    public void DropProcess(DropType dropType, ItemGeneric itemGeneric = null, WeaponDetailsSO toBeSwappedWeaponDetails = null, bool dropOffHand = false, 
        ItemSlotStatus itemSlotStatus = ItemSlotStatus.None, int inventoryIndex = -1, bool dropButton = false)
    {
        if (dropType == DropType.ActiveItem)
        {
            if (player.selectedActiveItem.GetCurrentActiveItem() != null)
            {
                if (player.selectedActiveItem?.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Compass)
                {
                    StaticEventHandler.CallCompassDisabled();
                }

                GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
                DropItem chestItem = chestItemObject.GetComponent<DropItem>();
                DropItem.toBeDroppedDropItem = chestItem;

                DropItem.toBeDroppedDropItem.hasActiveDrop = true;
                DropItem.toBeDroppedDropItem.droppedByPlayer = true;
                DropItem.toBeDroppedDropItem.isColliding = true;

                DropItem.toBeDroppedDropItem.Initialize(player.selectedActiveItem.GetCurrentActiveItem(), player.selectedActiveItem.GetCurrentActiveItem().
                    activeItemDetails.activeItemSprite, transform.position);

                // Break free from the player object
                DropItem.toBeDroppedDropItem.spriteRenderer.enabled = true;
                DropItem.toBeDroppedDropItem.animator.enabled = true;
                DropItem.toBeDroppedDropItem.animator.runtimeAnimatorController = player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemAnimatorController;

                // Store remaining charge count during drop process
                DropItem.toBeDroppedDropItem.remainingItemCharge = player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge;

                player.setActiveItemEvent.CallRemovedActiveItem();

                // Update stat values
                player.UpdateDamageValues();
                player.UpdateWeaponHandlingAndCriticalValues();
                player.UpdateBlockAndEvasivenessValues();

                DropItem.toBeDroppedDropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);
                DropItem.toBeDroppedDropItem.isPickedUp = false;

                // Make sure drop completed
                DropItem.toBeDroppedDropItem.boxCollider2D.enabled = true;
                DropItem.toBeDroppedDropItem.isColliding = false;
            }
        }
        else if (dropType == DropType.PassiveItem)
        {
            PassiveItem passiveItem = (PassiveItem)itemGeneric;

            if (itemSlotStatus == ItemSlotStatus.Inventory) // Just drop from inventory to the floor
            {
                SpawnDroppedPassiveItem(passiveItem);
                InventoryManager.Instance.EmptyItemFromInventory(inventoryIndex);
            }
            else if (dropButton)
            {
                // Manual drop by clicking drop button
                SpawnDroppedPassiveItem(passiveItem);
                player.setPassiveItemEvent.CallRemovePassiveItem(passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName, isSwap: false, dropButton: true);
            }
            else if (InventoryManager.Instance.IsInventoryFull()) // Drop equipped item to the floor, pick-up new one if pick-up happened
            {
                // Forced drop due to full inventory during item pickup
                SpawnDroppedPassiveItem(passiveItem);
                player.setPassiveItemEvent.CallRemovePassiveItem(passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName);
            }
            else
            {
                // Regular swap - previous item will be inserted into inventory
                player.setPassiveItemEvent.CallRemovePassiveItem(passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName);
                int index = InventoryManager.Instance.FindIndexOfItem(passiveItem);

                passiveItem.itemSlotStatus = ItemSlotStatus.Inventory;
                StaticEventHandler.CallPassiveItemAddedToInventorySlot(passiveItem, index); // Item added to inventory
            }
        }
        else if(dropType == DropType.Weapon)
        {
            Weapon weapon = (Weapon)itemGeneric;

            Weapon toBeSwappedWeapon = new Weapon();
            toBeSwappedWeapon.weaponDetails = toBeSwappedWeaponDetails;

            if (itemSlotStatus == ItemSlotStatus.Inventory) // Just drop from inventory to the floor
            {
                SpawnDroppedWeapon(weapon);

                // Empty inventory slot
                InventoryManager.Instance.EmptyItemFromInventory(inventoryIndex);

                // Book update
                StaticEventHandler.CallInventoryWeaponDroppedEventForBook(inventoryIndex);
            }
            else if (weapon.itemSlotStatus == ItemSlotStatus.MainHand)
            {
                if (dropButton) // Drop from main hand slot to the floor
                {
                    if (!SlotPlacementRules.IsPlacementAllowed(weapon, SlotType.Drop, player.activeWeapon.GetCurrentMainHandWeapon(), player.currentWeaponSlotSetIndex))
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                        return;
                    }
                    else
                    {
                        SpawnDroppedWeapon(weapon);
                        DeactivateMainHandWeapon();
                    }
                }
                else if (InventoryManager.Instance.IsInventoryFull()) // Drop equipped item to the floor, pick-up new one if pick-up happened
                {
                    if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon, player.activeWeapon.GetCurrentMainHandWeapon(), player.activeWeapon.GetCurrentOffHandWeapon(), false))
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                        return;
                    }
                    else
                    {
                        // Forced drop due to full inventory during item pickup
                        SpawnDroppedWeapon(weapon);
                        DeactivateMainHandWeapon();
                    }
                }
                else  // Drop equipped item to the inventory, pick-up new one if pick-up happened
                {
                    if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon, player.activeWeapon.GetCurrentMainHandWeapon(), player.activeWeapon.GetCurrentOffHandWeapon(), false))
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                        return;
                    }
                    else
                    {
                        DeactivateMainHandWeapon();
                        int index = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(weapon);
                        weapon.itemSlotStatus = ItemSlotStatus.Inventory;
                        StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, index);
                    }
                }

                // Update stat values
                player.UpdateDamageValues();
                player.UpdateWeaponHandlingAndCriticalValues();
                player.UpdateBlockAndEvasivenessValues();

                StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponMainHand);

                player.mainHandSlotFilled = false;

            }
            else
            {
                if (dropButton)
                {
                    if (!SlotPlacementRules.IsPlacementAllowed(weapon, SlotType.Drop, player.activeWeapon.GetCurrentMainHandWeapon(), player.currentWeaponSlotSetIndex))
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                        return;
                    }
                    else
                    {
                        SpawnDroppedOffhandWeapon(weapon);
                        DeactivateOffhandWeapon();
                    }
                }
                else if (InventoryManager.Instance.IsInventoryFull()) // Drop equipped item to the floor, pick-up new one if pick-up happened
                {
                    if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon, player.activeWeapon.GetCurrentMainHandWeapon(), player.activeWeapon.GetCurrentOffHandWeapon(), true))
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                        return;
                    }
                    else
                    {
                        // Forced drop due to full inventory during item pickup
                        SpawnDroppedOffhandWeapon(weapon);
                        DeactivateOffhandWeapon();
                    }
                }
                else  // Drop equipped item to the inventory, pick-up new one if pick-up happened
                {
                    if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon, player.activeWeapon.GetCurrentMainHandWeapon(), player.activeWeapon.GetCurrentOffHandWeapon(), true))
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                        return;
                    }
                    else
                    {
                        DeactivateOffhandWeapon();
                        int index = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(weapon);
                        weapon.itemSlotStatus = ItemSlotStatus.Inventory;
                        StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, index);
                    }
                }

                if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    player.isShieldCalculated = false;
                }

                // Update stat values
                player.UpdateDamageValues();
                player.UpdateArmorValues();
                player.UpdateWeaponHandlingAndCriticalValues();
                player.UpdateBlockAndEvasivenessValues();

                StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponOffHand);

                player.offHandSlotFilled = false;
            }
        }
    }

    private void DeactivateMainHandWeapon()
    {
        // De-active dropped main hand weapon
        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1]?.weaponDetails.wieldType == WieldType.OneHanded)
        {
            // Prevent off-hand ui weapon icon lost in case weapon swapping on drop while equipping a shield
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent(true);
        }
        else
        {
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent();
        }

        player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = null;
    }

    private void DeactivateOffhandWeapon()
    {
        // Set dropped set's off hand null
        player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = null;

        player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();
    }

    private void SpawnDroppedWeapon(Weapon weapon)
    {
        GameObject dropItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        DropItem dropItem = dropItemObject.GetComponent<DropItem>();
        DropItem.toBeDroppedDropItem = dropItem;

        DropItem.toBeDroppedDropItem.hasWeaponDrop = true;
        DropItem.toBeDroppedDropItem.isColliding = true;
        DropItem.toBeDroppedDropItem.hasMainHandWeapon = true;

        DropItem.toBeDroppedDropItem.Initialize(weapon, weapon.weaponDetails.weaponFrontSprite, transform.position);

        // Break free from the player object
        DropItem.toBeDroppedDropItem.spriteRenderer.enabled = true;
        DropItem.toBeDroppedDropItem.animator.enabled = true;
        DropItem.toBeDroppedDropItem.animator.runtimeAnimatorController = weapon.weaponDetails.weaponHoverAnimatorController;

        DropItem.toBeDroppedDropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);
        DropItem.toBeDroppedDropItem.isPickedUp = false;

        // Make sure drop completed
        DropItem.toBeDroppedDropItem.boxCollider2D.enabled = true;
        DropItem.toBeDroppedDropItem.isColliding = false;
    }

    private void SpawnDroppedOffhandWeapon(Weapon weapon)
    {
        GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        DropItem chestItem = chestItemObject.GetComponent<DropItem>();
        DropItem.toBeDroppedDropItem = chestItem;

        DropItem.toBeDroppedDropItem.hasWeaponDrop = true;
        DropItem.toBeDroppedDropItem.isColliding = true;
        DropItem.toBeDroppedDropItem.hasOffHandWeapon = true;

        DropItem.toBeDroppedDropItem.Initialize(weapon, weapon.weaponDetails.weaponFrontSprite, transform.position);

        // Break free from the player object
        DropItem.toBeDroppedDropItem.spriteRenderer.enabled = true;
        DropItem.toBeDroppedDropItem.animator.enabled = true;
        DropItem.toBeDroppedDropItem.animator.runtimeAnimatorController = weapon.weaponDetails.weaponHoverAnimatorController;

        DropItem.toBeDroppedDropItem.transform.SetParent(null);
        DropItem.toBeDroppedDropItem.isPickedUp = false;


        // Make sure drop completed
        DropItem.toBeDroppedDropItem.boxCollider2D.enabled = true;
        DropItem.toBeDroppedDropItem.isColliding = false;
    }


    private void SpawnDroppedPassiveItem(PassiveItem passiveItem)
    {
        GameObject dropItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        DropItem dropItem = dropItemObject.GetComponent<DropItem>();
        DropItem.toBeDroppedDropItem = dropItem;

        dropItem.hasSecondaryPassiveDrop = true;
        dropItem.isColliding = true;

        dropItem.Initialize(passiveItem, passiveItem.passiveItemDetails.passiveItemSprite, transform.position);
        dropItem.spriteRenderer.enabled = true;
        dropItem.animator.enabled = true;
        dropItem.animator.runtimeAnimatorController = passiveItem.passiveItemDetails.passiveItemAnimatorController;
        dropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);

        dropItem.isPickedUp = false;
        dropItem.boxCollider2D.enabled = true;
        dropItem.isColliding = false;
    }

    public bool IsMainHandDropPossible(bool isWeaponSwapping)
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

        bool dropPossible = gauge > 1 ;

        if (isWeaponSwapping)
        {
            dropPossible = true;
        }

        return dropPossible;
    }

    /// <summary>
    /// Slow motion item
    /// </summary>
    private IEnumerator MoveItemDown(DropItem dropItem)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = dropItem.transform.position + new Vector3(0f, 0.5f, 0f);
        float xPos = 0f;
        float yPos = 0f;

        // Drop x position adjustment
        if (initialPosition.x - HelperUtilities.GetMouseWorldPosition().x > -0.5f && initialPosition.x - HelperUtilities.GetMouseWorldPosition().x < 0.5f)
        {
            xPos = HelperUtilities.GetMouseWorldPosition().x;
        }
        else if (initialPosition.x - HelperUtilities.GetMouseWorldPosition().x > 0.5f)
        {
            xPos = initialPosition.x - 0.5f;
        }
        else if (initialPosition.x - HelperUtilities.GetMouseWorldPosition().x < -0.5f)
        {
            xPos = initialPosition.x + 0.5f;
        }

        // Drop y position adjustment
        if (initialPosition.y - HelperUtilities.GetMouseWorldPosition().y > -0.5f && initialPosition.y - HelperUtilities.GetMouseWorldPosition().y < 0.5f)
        {
            yPos = HelperUtilities.GetMouseWorldPosition().y;
        }
        else if (initialPosition.y - HelperUtilities.GetMouseWorldPosition().y > 0.5f)
        {
            yPos = initialPosition.y - 0.5f;
        }
        else if (initialPosition.y - HelperUtilities.GetMouseWorldPosition().y < -0.5f)
        {
            yPos = initialPosition.y + 0.5f;
        }

        Vector3 targetPosition = new Vector3(xPos, yPos, 0f);

        float dropDuration = Random.Range(0.05f, 0.5f);

        while (elapsedTime < dropDuration)
        {
            elapsedTime += Time.deltaTime; // Increment time based on frame rate
            dropItem.transform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime);
            yield return null; // Wait for the next frame
        }

        // Ensure the item reaches the target position
        dropItem.transform.position = targetPosition;

        // Make sure drop completed
        dropCoroutine = null;
        dropItem.boxCollider2D.enabled = true;
        dropItem.isColliding = false;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        // If collided with something stop player roll coroutine
        StopPlayerRollRoutine();
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        // If collided with something stop player roll coroutine
        StopPlayerRollRoutine();
    }

    private void StopPlayerRollRoutine()
    {
        if (playerRollCoroutine != null)
        {
            StopCoroutine(playerRollCoroutine);
            isPlayerRolling = false;
        }
    }

    /// <summary>
    /// Enable the player movement
    /// </summary>
    public void EnablePlayer()
    {
        isPlayerMovementDisabled = false;
        player.movementByVelocity.moveSpeed = player.movementByVelocity.movementDetails.GetBaseMoveSpeed() + player.CurrentAgilityValue * 0.25f;
    }

    /// <summary>
    /// Disable the player movement
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.movementByVelocity.moveSpeed = 0f;
        player.idle.StopVelocity();
        player.animatePlayer.ResetAnimatonParameters();
        player.animator.SetBool(Settings.isIdle, true);
    }

    public AimDirection GetAimDirection()
    {
        return aimDirection;
    }

    // This method visualizes the radius of the seismic slam for debugging purposes.
    private void OnDrawGizmosSelected()
    {
        if (player == null) return;

        //Gizmos.color = Color.red;
        //Gizmos.DrawWireSphere(transform.position, player.seismicSlamCircleRadius);
    }

    public void PopulatePassiveItemsToBook(PassiveItem passiveItem, PassiveItemSlotName itemSlotName)
    {
        StaticEventHandler.CallItemAddedToPassiveItemSlot(passiveItem, itemSlotName);
    }

    public void RemovePassiveItemFromBook(PassiveItemSlotName itemSlotName)
    {
        StaticEventHandler.CallItemRemovedFromPassiveItemSlot(itemSlotName);
    }
}
