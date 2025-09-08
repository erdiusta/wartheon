using System.Collections.Generic;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;

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
    Coroutine curseCoroutine;
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

    [HideInInspector] public bool isDashing;

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
        animationEventHelperMainHand.OnShatterCryTriggered.AddListener(PerformShatterCry);
    }

    private void OnDisable()
    {
        player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;

        animationEventHelperMainHand.OnSeismicSlamTriggered.RemoveListener(PerformSeismicSlam);
        animationEventHelperMainHand.OnShatterCryTriggered.RemoveListener(PerformShatterCry);
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
        player.animator.speed = player.movementByForce.moveSpeed / Settings.baseSpeedForPlayerAnimations;
    }

    private void Update()
    {
        playerBodycenterPosition = player.transform.position + new Vector3(0f, 0.65f, 0f);

        // NYMARA - Conductive Touch
        if (player.playerDetails.playerCharacterIndex == Character.Nymara)
        {
            if (player.isConductiveTouchActive)
            {
                player.nymaraSkillUsageTimer += Time.deltaTime;

                if (player.nymaraSkillUsageTimer > 3)
                {
                    player.isConductiveTouchActive = false;
                    player.healthEvent.CallConductiveTouchWoreOffEvent();
                    player.nymaraSkillUsageTimer = 0f;
                }
            }
        }

        // NYXA - Nyxa's Reflex
        if (player.playerDetails.playerCharacterIndex == Character.Nyxa)
        {
            player.isNyxasReflexPassiveOn = Time.time - player.lastDashTeleportHappenedTime < 5f ? true : false;

            if (player.isNyxasReflexPassiveOn)
            {
                if (!player.passiveTriggered)
                {
                    player.passiveTriggered = true;

                    player.healthEvent.CallNyxasReflexSpecialMoveEvent();
                    player.additionalDodgeRateModifier += 0.2f;
                    player.UpdateDodgeValue();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }
            else
            {
                if (player.passiveTriggered)
                {
                    player.passiveTriggered = false;

                    player.healthEvent.CallNyxasReflexSpecialMoveEndEvent();
                    player.additionalDodgeRateModifier -= 0.2f;
                    player.UpdateDodgeValue();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }
        }

        // CAELION - Grace of the Unscarred
        if (player.playerDetails.playerCharacterIndex == Character.Caelion)
        {
            player.isGraceOfTheUnscarredPassiveOn = Time.time - player.lastDamageHappenedTime > 8f ? true : false;

            if (player.isGraceOfTheUnscarredPassiveOn)
            {
                if (!player.passiveTriggered)
                {
                    player.passiveTriggered = true;

                    player.healthEvent.CallGraceOfTheUnscarredSpecialMoveEvent();
                    player.currentArmorValue += 0.2f;
                    player.currentMagicResistanceValue += 0.2f;
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }
            else
            {
                if (player.passiveTriggered)
                {
                    player.passiveTriggered = false;

                    player.healthEvent.CallGraceOfTheUnscarredSpecialMoveEndedEvent();
                    player.currentArmorValue -= 0.2f;
                    player.currentMagicResistanceValue -= 0.2f;
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }

            // GUARDED OATH
            if (player.isGuardedOathActive && player.activeWeapon.GetCurrentOffHandWeapon() != null &&
                player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass != WeaponClass.Shield)
            {
                foreach (var skill in player.currentlyUsedActiveUniqueSkills)
                {
                    if (skill.Value.activeSkill == ActiveSkill.GuardedOath)
                    {
                        int manaReserveCost = skill.Value.levels[skill.Value.GetCurrentActiveLevel()].manaReserveCost;
                        player.mana.ResetReservedMana(manaReserveCost);
                    }
                }
            }
        }

        // INNER PATH
        // Vicious Momentum
        ViciousMomentumCheck();

        // Combat Focus
        CombatFocusCheck();

        // Triad Execution
        TriadExecutionCheck();

        // Fortified Resolve
        if (player.isFortifiedResolveActive)
        {
            if (!player.fortifiedResolveTriggered)
            {
                if (HasNegativeStatusEffect())
                {
                    player.currentArmorValue += 0.15f;
                    player.healthEvent.CalllFortifiedResolveEvent();
                    player.UpdateArmorValues();
                    StaticEventHandler.CallStatPointChangedEvent();

                    player.fortifiedResolveTriggered = true;
                }
            }
            else
            {
                if (!HasNegativeStatusEffect())
                {
                    player.currentArmorValue -= 0.15f;
                    player.healthEvent.CallFortifiedResolveWoreOffEvent();
                    player.UpdateArmorValues();
                    StaticEventHandler.CallStatPointChangedEvent();

                    player.fortifiedResolveTriggered = false;
                }
            }
        }

        // If player movement disabled then return
        if (isPlayerMovementDisabled) return;

        if (isPlayerRolling) return;

        if (player.isCursed)
        {
            if (curseCoroutine == null)
            {
                curseCoroutine = StartCoroutine(CurseRoutine());
            }
        }

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
        // Knocked Back
        else if ((player.moveStatus & MoveStatus.KnockedBack) != 0)
        {
            isPlayerRolling = false;
            player.meleeAttackMainHand.IsAttacking = false;
            player.polygonCollider2D.enabled = false;

            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.rb2D.linearVelocity.magnitude < 0.08f)
                {
                    player.rb2D.linearVelocity = Vector2.zero;
                }

                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                {
                    // Trigger fire weapon event for precharge weapons
                    player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.
                        weaponCurrentProjectile.isLaser, AimDirection.Right, 0f, 0f, Vector3.zero, false);
                }
            }
        }
        else if (player.moveStatus == MoveStatus.Idle)
        {
            player.polygonCollider2D.enabled = true; //Reset if knocked back wore off

            // Process the player weapon input
            WeaponInput();
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
        // Cancel movement if attacking, parrying, or using special skill
        if (player.meleeAttackMainHand.IsAttacking || isParrying || isDashing || player.isHuntersReachActive)
        {
            player.movementByForce.MovementInput = Vector2.zero;
            return;
        }

        // Read and normalize input
        movementInput = InputManager.Instance.movement.action.ReadValue<Vector2>().normalized;
        bool jumpButtonDown = InputManager.Instance.jumpButton.action.WasPerformedThisFrame();

        // Update movement timer (for trail dust)
        movementTimer = movementInput.magnitude > 0.1f ? movementTimer + Time.deltaTime : 0f;

        // Store for physics force application
        player.movementByForce.MovementInput = movementInput;

        if (movementInput != Vector2.zero)
        {
            if (!jumpButtonDown)
            {
                player.animatePlayer.SetMovementAnimationParameters();
            }
            else if (playerRollCooldownTimer <= 0f && !InputManager.dodgeRollDisabled)
            {
                PlayerRoll(movementInput);
            }
        }
        else
        {
            // You may want to disable this if StopVelocity resets Rigidbody velocity
            player.rb2D.linearVelocity = Vector2.zero;

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

        Vector3 targetPosition = player.transform.position + direction * player.movementByForce.movementDetails.rollDistance;

        while (Vector3.Distance(player.transform.position, targetPosition) > minDistance)
        {
            player.movementToPositionEvent.CallMovementToPositionEvent(targetPosition, player.rb2D.position, player.movementByForce.movementDetails.rollSpeed,
                direction, isPlayerRolling);

            yield return waitForFixedUpdate;
        }

        isPlayerRolling = false;

        // Set cooldown timer
        playerRollCooldownTimer = player.movementByForce.movementDetails.rollCooldownTime;

        player.animatePlayer.SetIdleAnimationParameters();
        player.transform.position = targetPosition;
    }

    /// <summary>
    /// Weapon Input
    /// </summary>
    private void WeaponInput()
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection attackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out attackDirection);

        // Fire weapon input
        FireWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Process the player parry input
        ParryWeaponInput(weaponDirection, weaponAngleDegrees, playerAngleDegrees, playerAimDirection);

        // Switch weapon input
        SwitchWeaponInput();
    }

    public void AimWeaponInput(out Vector3 weaponDirection, out float weaponAngleDegrees, out float playerAngleDegrees, out AimDirection playerAimDirection, 
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
        Weapon mainHand = player.activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHand = player.activeWeapon.GetCurrentOffHandWeapon();

        // If glossary book is open, disable attack
        if (GameManager.Instance.glossaryBookOpen) return;

        // If pop-up window is open, disable attack
        if (GameManager.Instance.popUpWindowOpen) return;

        if (mainHand == null) return;

        if (InputManager.firingDisabled) return; // For tutorial issues

        // Fire when left mouse button is clicked - melee
        if (mainHand.weaponDetails.isMeleeWeapon && !GameManager.Instance.isOverviewCameraEnabled)
        {
            if (InputManager.Instance.attack.action.WasPressedThisFrame() && !IsClickingSpecificUILayer() && !isParrying)
            {
                // MAIN-HAND
                if (mainHand?.weaponDetails.isMeleeWeapon == true && !player.meleeAttackMainHand.IsAttacking)
                {
                    AttackShape mainHandAttackType = DetermineAttackType(mainHand.weaponDetails);

                    player.meleeAttackEvent.CallAttackEvent(aimDirection, mainHand, mainHandAttackType, MeleeHand.MainHand);
                }

                // OFF-HAND
                if (offHand?.weaponDetails.isMeleeWeapon == true && !player.meleeAttackMainHand.IsAttacking)
                {
                    AttackShape offHandAttackType = DetermineAttackType(offHand?.weaponDetails);

                    player.meleeAttackEvent.CallAttackEvent(aimDirection, offHand, offHandAttackType, MeleeHand.OffHand);
                }
            }

            return;
        }
    
        // Fire for non-precharge weapons (fire once per press)
        if (mainHand.weaponDetails.weaponPrechargeTime == 0f && InputManager.Instance.attack.action.WasPressedThisFrame()
            && !IsClickingSpecificUILayer() && !GameManager.Instance.isOverviewCameraEnabled)
        {
            isSoundPlayed = false;

            if (mainHand.weaponDetails.weaponClass == WeaponClass.Bow || mainHand.weaponDetails.weaponClass == WeaponClass.Crossbow ||
                mainHand.weaponDetails.weaponClass == WeaponClass.Staff)
            {
                if (!mainHand.onCooldown)
                {
                    //player.meleeAttackMainHand.IsAttacking = true;

                    // Trigger fire weapon event
                    player.meleeAttackEvent.CallAttackEvent(playerAimDirection, mainHand, AttackShape.None, MeleeHand.None);
                }
            }

            // Fire event (only once per press)
            player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, mainHand.weaponDetails.weaponCurrentProjectile.isLaser,
                playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false,
                player.isArrowOfTheSevenActive);
        }
    }

    private AttackShape DetermineAttackType(WeaponDetailsSO weaponDetails)
    {
        if (weaponDetails.hasSwing) return AttackShape.Swing;
        if (weaponDetails.hasThrust) return AttackShape.Thrust;

        return AttackShape.Swing; // default fallback
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
        if (player.meleeAttackMainHand.IsAttacking) return;

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
        Transform buttonContainer = GameManager.Instance.bookView.transform.GetChild(1).GetChild(6).GetChild(0);

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
    /// Curse routine
    /// </summary>
    IEnumerator CurseRoutine()
    {
        yield return new WaitForSeconds(player.curseDuration);

        player.isCursed = false;
        player.healthEvent.CallCurseCuredEvent();
        curseCoroutine = null;
    }

    /// <summary>
    /// Frost routine
    /// </summary>
    IEnumerator FrostRoutine()
    {
        player.movementByForce.moveSpeed = 0f;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        player.animator.SetBool(Settings.isFrozen, true);

        yield return new WaitForSeconds(player.freezeDuration);

        player.moveStatus &= ~MoveStatus.Frozen; // Remove frozen
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallFrostCuredEvent();
        player.animator.SetBool(Settings.isFrozen, false);
        player.UpdateSpeedValue();

        frostCoroutine = null;
    }

    /// <summary>
    /// Root routine
    /// </summary>
    IEnumerator RootRoutine()
    {
        player.movementByForce.moveSpeed = 0f;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;

        yield return new WaitForSeconds(player.rootDuration);

        player.moveStatus &= ~MoveStatus.Root; // Remove root
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallRootCuredEvent();
        player.UpdateSpeedValue();
        rootCoroutine = null;
    }

    /// <summary>
    /// Stun routine
    /// </summary>
    IEnumerator StunRoutine()
    {
        player.movementByForce.moveSpeed = 0f;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        player.animator.SetBool(Settings.isStunned, true);

        yield return new WaitForSeconds(player.stunDuration);

        player.moveStatus &= ~MoveStatus.Stun; // Remove stun
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallStunCuredEvent();
        player.animator.SetBool(Settings.isStunned, false);
        player.UpdateSpeedValue();
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

            ActiveUniqueSkillDetailsSO usedActiveUniqueSkillContainer = player.currentlyUsedActiveUniqueSkills[inputSlotNumber];
            int activeSkillLevel = usedActiveUniqueSkillContainer.GetCurrentActiveLevel();
            var activeSkillData = usedActiveUniqueSkillContainer.levels[activeSkillLevel - 1];

            int activeUniqueSkillManaCost = usedActiveUniqueSkillContainer.levels[activeSkillLevel - 1].manaCost;
            int activeUniqueSkillManaReserveCost = usedActiveUniqueSkillContainer.levels[activeSkillLevel - 1].manaReserveCost;

            if (player.mana.GetCurrentMana() >= activeUniqueSkillManaCost * (1 - player.additionalManaReductionModifier))
                // Check if there is enough mana
            {
                int consumedMana = (int)(activeUniqueSkillManaCost * (1 - player.additionalManaReductionModifier));
                int reservedMana = (int)(activeUniqueSkillManaReserveCost * (1 - player.additionalManaReductionModifier));

                switch (player.playerDetails.playerCharacterIndex)
                {
                    case Character.Caelion:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.SeismicSlam:
                                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    SeismicSlamProcess();
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.SeismicSlam, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.Valor:
                                if (!player.isValorActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    Valor(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Valor, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.ShieldBash:
                                // OFF-HAND
                                if (player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Shield)
                                {
                                    player.mana.ConsumeMana(consumedMana);

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
                                    player.mana.ConsumeMana(consumedMana);

                                    BreakTheLine(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BreakTheLine, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.GuardedOath:
                                if (player.isGuardedOathActive)
                                {
                                    RemoveGuardedOathEffects(inputSlotNumber, ref activeSkillData);
                                }
                                else if (!player.isGuardedOathActive && player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails.weaponClass == WeaponClass.Shield)
                                {
                                    if (player.mana.GetCurrentMana() >= reservedMana)
                                    {
                                        player.mana.ConsumeMana(reservedMana, true); // Mana reserved
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
                                    player.mana.ConsumeMana(consumedMana);

                                    UmbralMist(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.UmbralMist, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.Stealth:
                                if (!player.isStealthActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    Stealth(inputSlotNumber, ref activeSkillData);
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
                                        player.mana.ConsumeMana(consumedMana);

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
                                    player.mana.ConsumeMana(consumedMana);

                                    ShadowStep(inputSlotNumber, ref activeSkillData);
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
                                        player.mana.ConsumeMana(consumedMana);

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
                                        player.mana.ConsumeMana(consumedMana);

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
                                        player.mana.ConsumeMana(consumedMana);

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
                                        player.mana.ConsumeMana(consumedMana);

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
                                        player.mana.ConsumeMana(consumedMana);

                                        // Arrow of The Seven Plagues attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        ArrowOfTheSevenPlagues(inputSlotNumber, ref activeSkillData);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ArrowsOfTheSevenPlagues, inputSlotNumber);
                                    }
                                }

                                break;
                            case ActiveSkill.HuntersReach:
                                if (!player.isHuntersReachActive && !isPlayerRolling)
                                {
                                    player.mana.ConsumeMana(consumedMana);

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
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.Rage:
                                if (!player.isRageActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);
                                    Rage(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Rage, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.Shattercry:
                                if (!player.isShatterCryActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);
                                    ShatterCry(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Shattercry, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.AxeThrow:
                                // AXE CHECK
                                if (player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentMainHandWeapon() != null 
                                    && !player.isAxeThrowActive)
                                {
                                    if (player.activeWeapon.GetCurrentOffHandWeapon().weaponDetails.weaponClass == WeaponClass.Axe &&
                                        player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Axe && 
                                        !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Axe throw attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        AxeThrow(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.AxeThrow, inputSlotNumber);
                                    }
                                }

                                break;
                            case ActiveSkill.Whirlrend:
                                player.mana.ConsumeMana(consumedMana);

                                player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                Whirlrend(inputSlotNumber, ref activeSkillData);
                                player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Whirlrend, inputSlotNumber);
                                break;
                            case ActiveSkill.FeastOfWar:
                                if (!player.isFeastOfWarActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    FeastOfWar(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.FeastOfWar, inputSlotNumber);
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case Character.Nyxa:
                        Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
                        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

                        bool mainHanddaggerOrClaw = mainHandWeapon != null && (mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Dagger ||
                            mainHandWeapon.weaponDetails.weaponClass == WeaponClass.Claw);

                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.DontBlink:
                                // DAGGER OR CLAW CHECK
                                if (mainHanddaggerOrClaw && !player.isDontBlinkActive)
                                {
                                    // Don't blink attack
                                    DontBlink(inputSlotNumber, ref activeSkillData);

                                    if (!player.cancelledDueToInvalidTile)
                                    {
                                        player.lastDashTeleportHappenedTime = Time.time;

                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.DontBlink, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.VenomousIvy:
                                if (!player.isVenomousIvyActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    // Venomous Ivy attack
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    VenomousIvy(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.VenomousIvy, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.FadeAndFeed:
                                if (!player.isVenomousIvyActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    // Fade and Feed
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    FadeAndFeed(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.FadeAndFeed, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.BladeDash:
                                // DAGGER OR CLAW CHECK
                                if (mainHanddaggerOrClaw && !isParrying)
                                {
                                    if (!player.isBladeAndDashActive)
                                    {
                                        int index = inputSlotNumber - 1;
                                        int repeatCount = activeSkillData.recastRepeatCount;

                                        // Initial use check via sentinel value
                                        bool isInitialCast = player.specialMoveRecastCountArray[index] == -1;

                                        if (isInitialCast)
                                        {
                                            player.mana.ConsumeMana(consumedMana);
                                            player.specialMoveRecastCountArray[index] = repeatCount;
                                        }

                                        // Blade dash Attack
                                        if (player.specialMoveRecastCountArray[index] > 0)
                                        {
                                            player.specialMoveRecastCountArray[index]--;
                                            BladeDash(inputSlotNumber, ref activeSkillData, isInitialCast); // Dash and damage
                                            player.lastDashTeleportHappenedTime = Time.time;
                                            player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BladeDash, inputSlotNumber);
                                        }
                                    }
                                }
                                break;
                            case ActiveSkill.Shiruken:
                                if (!player.isShirukenActive)
                                {
                                    int index = inputSlotNumber - 1;
                                    int repeatCount = activeSkillData.recastRepeatCount;

                                    // Initial use check via sentinel value
                                    bool isInitialCast = player.specialMoveRecastCountArray[index] == -1;

                                    if (isInitialCast)
                                    {
                                        player.mana.ConsumeMana(consumedMana);
                                        player.specialMoveRecastCountArray[index] = repeatCount;
                                    }

                                    // Shiruken attack
                                    if (player.specialMoveRecastCountArray[index] > 0)
                                    {
                                        player.specialMoveRecastCountArray[index]--;
                                        Shiruken(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Shiruken, inputSlotNumber);
                                    }
                                }
                                break;

                            default:
                                break;
                        }
                        break;
                    case Character.Mycara:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.Blizzard:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff) 
                                {
                                    if (!player.isBlizzardActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Blizzard attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        Blizzard(inputSlotNumber, ref activeSkillData); ;
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Blizzard, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.MycarasSeal:
                                if (!player.isMycarasSealActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    // Activate Mycara's Seal
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    MycarasSeal(inputSlotNumber, ref activeSkillData); ;
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.MycarasSeal, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.SheerCold:
                                // STAFF CHECK
                                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                                {
                                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff &&
                                        !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

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
                                        player.mana.ConsumeMana(consumedMana);

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
                                        player.mana.ConsumeMana(consumedMana);

                                        // Absolute Zero attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        AbsoluteZero(inputSlotNumber, ref activeSkillData); ;
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.AbsoluteZero, inputSlotNumber);
                                    }
                                }
                                break;
                            default:
                                break;
                        }
                        break;
                    case Character.Kynara:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.FireBlast:

                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isFireBlastActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Fire blast attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        FireBlast(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.FireBlast, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.MoltenRift:
                                if (!player.isMoltenRiftActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    // Molten rift skill
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    MoltenRift(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.MoltenRift, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.FlameLotus:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isFlameLotusActive)
                                    {
                                        player.mana.ConsumeMana(activeUniqueSkillManaCost);

                                        // Flame lotus attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        FlameLotus(inputSlotNumber, ref activeSkillData); ;
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.FlameLotus, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.KynarasEmbrace:
                                if (!player.isKynarasEmbraceActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    // Activate Kynara's Embrace
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    KynarasEmbrace(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.KynarasEmbrace, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.BlazingCyclone:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isBlazingCycloneActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Blazing Cyclone attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        BlazingCyclone(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BlazingCyclone, inputSlotNumber);
                                    }
                                }
                                break;
                        }
                        break;
                    case Character.Nymara:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.MistOfDisruption:
                                if (!player.isMistOfDisruptionActive)
                                {
                                    player.isConductiveTouchActive = true; // Conductive Touch Flag
                                    player.nymaraSkillUsageTimer = 0f;
                                    player.healthEvent.CallConductiveTouchSpecialMoveEvent();

                                    // Mist of Disruption skill
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    MistOfDisruption(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.MistOfDisruption, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.NymarasWindveil:
                                if (!player.isNymarasWindveilActive)
                                {
                                    player.isConductiveTouchActive = true; // Conductive Touch Flag
                                    player.nymaraSkillUsageTimer = 0f;
                                    player.healthEvent.CallConductiveTouchSpecialMoveEvent();

                                    player.mana.ConsumeMana(consumedMana);

                                    // Nymara's Windveil skill
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    NymarasWindveil(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.NymarasWindveil, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.ChainLightning:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isChainLightningActive)
                                    {
                                        player.isConductiveTouchActive = true; // Conductive Touch Flag
                                        player.nymaraSkillUsageTimer = 0f;
                                        player.healthEvent.CallConductiveTouchSpecialMoveEvent();

                                        player.mana.ConsumeMana(consumedMana);

                                        // Chain Lightning attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        ChainLightning(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ChainLightning, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.EyeOfTheStorm:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isEyeOfTheStormActive)
                                    {
                                        player.isConductiveTouchActive = true; // Conductive Touch Flag
                                        player.nymaraSkillUsageTimer = 0f;
                                        player.healthEvent.CallConductiveTouchSpecialMoveEvent();

                                        player.mana.ConsumeMana(consumedMana);

                                        // Eye of the storm attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        EyeOfTheStorm(inputSlotNumber, ref activeSkillData);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.EyeOfTheStorm, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.IonicRejuvenation:
                                if (!player.isIonicRejuvenationActive)
                                {
                                    player.isConductiveTouchActive = true; // Conductive Touch Flag
                                    player.nymaraSkillUsageTimer = 0f;
                                    player.healthEvent.CallConductiveTouchSpecialMoveEvent();

                                    player.mana.ConsumeMana(consumedMana);

                                    // Ionic Rejuvenation skill
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    IonicRejuvenation(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.IonicRejuvenation, inputSlotNumber);
                                }
                                break;
                        }
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

    private void PerformSeismicSlam()
    {
        GameObject slamEffectObject = Instantiate(activeSkillTypeThreeAnimator.gameObject, transform.position, Quaternion.identity);

        slamEffectObject.GetComponent<Animator>().SetTrigger("slam");

        Destroy(slamEffectObject, 4f); // Destroy slam object after animation completed
    }

    /// <summary>
    /// Execute Valor special move
    /// </summary>
    private void Valor(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isValorActive)
            {
                activeSkillTypeOneAnimator.SetBool("valor", true);
                player.healthEvent.CallValorSpecialMoveEvent(); // This is for displaying valor icon
                player.isValorActive = true;
                StartCoroutine(ValorRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator ValorRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isValorActive = false;
        player.healthEvent.CallValorWoreOffEvent();
        activeSkillTypeOneAnimator.SetBool("valor", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute BreakTheLine special move
    /// </summary>
    private void BreakTheLine(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            int originalMinDamage = player.currentMainHandMinDamageValue;
            int originalMaxDamage = player.currentMainHandMaxDamageValue;

            // ENABLE SKILL EFFECTS
            switch (player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel())
            {
                case 1:
                    player.currentMainHandMinDamageValue = (int)(player.currentMainHandMinDamageValue * 1.4f);
                    player.currentMainHandMaxDamageValue = (int)(player.currentMainHandMaxDamageValue * 1.4f);
                    break;
                case 2:
                    player.currentMainHandMinDamageValue = (int)(player.currentMainHandMinDamageValue * 1.5f);
                    player.currentMainHandMaxDamageValue = (int)(player.currentMainHandMaxDamageValue * 1.5f);
                    break;
                case 3:
                    player.currentMainHandMinDamageValue = (int)(player.currentMainHandMinDamageValue * 1.6f);
                    player.currentMainHandMaxDamageValue = (int)(player.currentMainHandMaxDamageValue * 1.6f);
                    break;
                default:
                    break;
            }

            player.additionalSpeedModifier += 2; // Increase speed

            Weapon droppedShield = player.activeWeapon.GetCurrentOffHandWeapon();
            player.UpdateDamageValues();
            player.UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent();

            player.healthEvent.CallBreakTheLineSpecialMoveEvent(); // This is for displaying valor icon

            if (!player.isBreakTheLineActive)
            {
                player.isBreakTheLineActive = true;
                StartCoroutine(BreakTheLineRoutine(slotIndex, droppedShield, originalMinDamage, originalMaxDamage, activeSkillData));
            }
        }
    }

    IEnumerator BreakTheLineRoutine(int slotIndex, Weapon droppedShield, int originalMinDamage, int originalMaxDamage, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

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
        player.isGuardedOathActive = true;
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        activeSkillTypeTwoAnimator.SetBool("oath", true);

        player.originalShieldArmorModifier = player.additionalShieldArmorModifier;
        player.originalBlockModifier = player.additionalBlockModifier;

        // EFFECTS
        switch (player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel())
        {
            case 1:
                player.additionalShieldArmorModifier += 1f;
                player.additionalBlockModifier += 0.2f;
                break;
            case 2:
                player.additionalShieldArmorModifier += 1.2f;
                player.additionalBlockModifier += 0.25f;
                break;
            case 3:
                player.additionalShieldArmorModifier += 1.5f;
                player.additionalBlockModifier += 0.3f;
                break;
            default:
                break;
        }

        player.additionalPhysicalDamageModifer -= 0.1f;
        player.additionalSpeedModifier -= 0.5f;

        player.UpdateDamageValues();
        player.UpdateBlockValue();
        player.UpdateArmorValues();
        player.UpdateSpeedValue();

        // ENABLE SKILL EFFECTS
        StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI
        player.healthEvent.CallGuardedOathSpecialMoveEvent(); // This is for displaying guarded oath icon
    }

    /// <summary>
    /// Remove GuardedOath effects
    /// </summary>
    public void RemoveGuardedOathEffects(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        player.isGuardedOathActive = false;

        activeSkillTypeTwoAnimator.SetBool("oath", false);

        // EFFECTS WORE OFF
        player.additionalShieldArmorModifier = player.originalShieldArmorModifier;
        player.additionalBlockModifier = player.originalBlockModifier;

        player.additionalPhysicalDamageModifer += 0.1f;
        player.additionalSpeedModifier += 0.5f;

        player.UpdateDamageValues();
        player.UpdateBlockValue();
        player.UpdateArmorValues();
        player.UpdateSpeedValue();

        StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

        player.mana.ResetReservedMana(activeSkillData.manaReserveCost);
        player.healthEvent.CallGuardedOathSpecialMoveEndEvent(); // This is for displaying guarded oath icon
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute Umbral Mist special move
    /// </summary>
    private void UmbralMist(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.healthEvent.CallUmbralMistSpecialMoveEvent(); // This is for displaying umbral mist icon

            if (!player.isUmbralMistActive)
            {
                player.isUmbralMistActive = true;
                StartCoroutine(UmbralMistRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator UmbralMistRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        GameObject umbralMistObject = Instantiate(activeSkillTypeTwoAnimator.gameObject, transform.position, Quaternion.identity);

        umbralMistObject.GetComponent<Animator>().SetBool("umbralMist", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isUmbralMistActive = false;

        player.healthEvent.CallUmbralMistWoreOffEvent();
        umbralMistObject.GetComponent<Animator>().SetBool("umbralMist", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(umbralMistObject, 2f); // Destroy mist object after animation completed
    }

    /// <summary>
    /// Execute Stealth special move
    /// </summary>
    private void Stealth(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
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
        StartCoroutine(StealthRoutine(slotIndex, activeSkillData));
    }

    IEnumerator StealthRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        float stealthDuration = activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier);

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
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentMainHandWeapon(), AttackShape.Swing, MeleeHand.MainHand, isBloodDrain: true);
        player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, player.activeWeapon.GetCurrentOffHandWeapon(), AttackShape.Swing, MeleeHand.OffHand, isBloodDrain: true);
    }

    /// <summary>
    /// Execute Shadowstep special move
    /// </summary>
    private void ShadowStep(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isShadowStepActive)
            {
                player.isShadowStepActive = true;
                activeSkillTypeThreeAnimator.SetBool("shadowStep", true);
                player.healthEvent.CallShadowStepSpecialMoveEvent(); // This is for displaying shadow step icon
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

                float modifier = 0f;

                //EFFECTS
                switch (player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel())
                {
                    case 1: modifier = 2; break;
                    case 2: modifier = 3; break;
                    case 3: modifier = 4; break;
                    default: break;
                }

                player.additionalSpeedModifier += modifier;
                player.UpdateSpeedValue();
                StaticEventHandler.CallStatsChangedOnTheBookEvent();
                StartCoroutine(ShadowStepRoutine(slotIndex, activeSkillData, modifier));
            }
        }
    }

    IEnumerator ShadowStepRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData, float modifier)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isShadowStepActive = false;
        activeSkillTypeThreeAnimator.SetBool("shadowStep", false);

        //EFFECTS ENDED
        player.additionalSpeedModifier -= modifier;
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
    private void ArrowOfTheSevenPlagues(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        player.isArrowOfTheSevenActive = true;
        player.healthEvent.CallSevenArrowsSpecialMoveEvent();
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        StartCoroutine(ArrowOfTheSevenPlaguesRoutine(slotIndex, activeSkillData));
    }

    IEnumerator ArrowOfTheSevenPlaguesRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

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
    private void Blizzard(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isBlizzardActive)
            {
                player.isBlizzardActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(BlizzardRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator BlizzardRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeOneAnimator.gameObject.SetActive(true);

        GameObject blizzardObject = Instantiate(activeSkillTypeOneAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(), Quaternion.identity);

        blizzardObject.GetComponent<Animator>().SetBool("blizzard", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isBlizzardActive = false;

        blizzardObject.GetComponent<Animator>().SetBool("blizzard", false);
        activeSkillTypeOneAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(blizzardObject, 2f); // Destroy blizzard object after animation completed
    }

    /// <summary>
    /// Execute Mycara's Seal special move
    /// </summary>
    private void MycarasSeal(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);

            activeSkillTypeTwoAnimator.SetBool("mycarasSeal", true);
            player.healthEvent.CallMycarasSealSpecialMoveEvent(); // This is for displaying valor icon
            player.isMycarasSealActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            StartCoroutine(MycarasSealRoutine(slotIndex, activeSkillData));

            GameObject forceFieldObject = player.forcefieldTransform.gameObject;
            forceFieldObject.SetActive(true);
        }
    }

    IEnumerator MycarasSealRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

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
    private void AbsoluteZero(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isAbsoluteZeroActive)
            {
                player.isAbsoluteZeroActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(AbsoluteZeroRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator AbsoluteZeroRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeFourAnimator.gameObject.SetActive(true);

        GameObject absoluteZeroObject = Instantiate(activeSkillTypeFourAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(), Quaternion.identity);

        absoluteZeroObject.GetComponent<Animator>().SetBool("absoluteZero", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isAbsoluteZeroActive = false;

        absoluteZeroObject.GetComponent<Animator>().SetBool("absoluteZero", false);
        activeSkillTypeFourAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(absoluteZeroObject, 2f); // Destroy blizzard object after animation completed
    }

    /// <summary>
    /// Execute Fire Blast special move
    /// </summary>
    private void FireBlast(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isFireBlastActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false, null,
            null, true, player.playerDetails.fireBlastDetails);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isFireBlastActive = false));
    }

    /// <summary>
    /// Execute Molten Rift special move
    /// </summary>
    private void MoltenRift(int slotIndex)
    {
        player.isMoltenRiftActive = true;

        // Start playing teleport particle system
        if (teleportParticleRoutine != null)
        {
            StopCoroutine(teleportParticleRoutine);
        }
        teleportParticleRoutine = StartCoroutine(ParticleSystemRoutine());

        // Wait for mouse click to teleport the character
        InputManager.Instance.pointerPosition.action.performed += OnMoltenRiftInput;

        // Play special move sound effect
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        StartCoroutine(EnableInvincibility());
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

        player.isMoltenRiftActive = false;
        player.health.isDamageable = true;
    }

    private void OnMoltenRiftInput(InputAction.CallbackContext context)
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
                player.rb2D.position = pointerWorldPosition;

                // Stop playing teleport particle system
                player.specialMoveParticlesSystem.Stop();
                particlePlayed = false;

                // Unsubscribe from the event to prevent multiple teleports
                InputManager.Instance.pointerPosition.action.performed -= OnMoltenRiftInput;
            }
        }
    }

    /// <summary>
    /// Execute Flame Lotus special move
    /// </summary>
    private void FlameLotus(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isFlameLotusActive)
            {
                player.isFlameLotusActive = true;
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                StartCoroutine(FlameLotusRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator FlameLotusRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeOneAnimator.gameObject.SetActive(true);

        GameObject flameLotusObject = Instantiate(activeSkillTypeOneAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(), Quaternion.identity);

        flameLotusObject.GetComponent<Animator>().SetBool("flameLotus", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isFlameLotusActive = false;

        flameLotusObject.GetComponent<Animator>().SetBool("flameLotus", false);
        activeSkillTypeOneAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(flameLotusObject, 2f); // Destroy flame lotus object after animation completed
    }

    /// <summary>
    /// Execute Kynara's Embrace special move
    /// </summary>
    private void KynarasEmbrace(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);

            activeSkillTypeTwoAnimator.SetBool("kynarasEmbrace", true);
            player.healthEvent.CallKynarasEmbraceSpecialMoveEvent(); // This is for displaying Kynara's Embrace icon
            player.isKynarasEmbraceActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            StartCoroutine(KynarasEmbraceRoutine(slotIndex, activeSkillData));

            GameObject forceFieldObject = player.forcefieldTransform.gameObject;
            forceFieldObject.SetActive(true);
        }
    }

    IEnumerator KynarasEmbraceRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        if (!player.isKynarasEmbraceShieldExploding)
        {
            player.isKynarasEmbraceShieldExploding = false;
            player.healthEvent.CallKynarasEmbraceWoreOffEvent();

            activeSkillTypeTwoAnimator.SetTrigger("explosion");
            Explosion(slotIndex);
        }

        yield return new WaitForSeconds(0.5f); // Explosion duration

        player.isKynarasEmbraceActive = false;
        activeSkillTypeTwoAnimator.SetBool("kynarasEmbrace", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Explosion(int slotIndex)
    {
        player.filter.SetLayerMask(player.layerMask);
        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectTwo);

        Collider2D[] results = new Collider2D[20];
        int hitCount = Physics2D.OverlapCircle(transform.position, player.kynarasEmbraceCircleRadius, player.filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D collider = results[i];
            if (collider == null) continue;

            if (collider is PolygonCollider2D)
            {
                // Don't hit yourself if player is also in the collider list
                if (collider.tag == Settings.playerTag) continue;

                if (collider.tag == Settings.enemyTag)
                {
                    Enemy enemy = collider.GetComponent<Enemy>();

                    Weapon weapon = player.activeWeapon.GetCurrentMainHandWeapon();
                    float damageModifier = 0f;

                    if (player != null)
                    {
                        damageModifier = player.playerDetails.fourthActiveSkillDetails.GetCurrentActiveLevel() switch
                        {
                            1 => 1f,
                            2 => 1.1f,
                            3 => 1.2f,
                            _ => 1f
                        };
                    }

                    int inflictedDamage = player.meleeAttackMainHand.CalculateDamageAmount(enemy, weapon, MeleeHand.MainHand, damageModifier);

                    enemy.health.TakeDamage(inflictedDamage, transform.position, enemy.transform.position, collider, MeleeHand.None);

                    if (!enemy.enemyDetails.hasKnockbackResistance && enemy.GetComponent<Health>().currentHealth > 0)
                    {
                        // Knockback
                        Vector2 knockbackDir = (enemy.transform.position - transform.position).normalized;
                        float knockbackForce = 5f;
                        float dealDamageMass = 1f;

                        enemy.movementToPosition.ApplyKnockbackToEnemy(knockbackDir, knockbackForce, dealDamageMass);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Execute Blazing Cyclone special move
    /// </summary>
    private void BlazingCyclone(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isBlazingCycloneActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false, null,
            null, false, null, true, player.playerDetails.blazingCycloneDetails);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isBlazingCycloneActive = false));
    }

    /// <summary>
    /// Execute Rage special move
    /// </summary>
    private void Rage(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.animator.SetTrigger("rage");
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            player.healthEvent.CallRageSpecialMoveEvent(); // This is for displaying rage icon
            player.isRageActive = true;
            StartCoroutine(RageRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator RageRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        float meleeDamageIncrease = 0f;
        float armorDecrease = 0f;

        switch (player.playerDetails.firstActiveSkillDetails.GetCurrentActiveLevel())
        {
            case 1:
                meleeDamageIncrease = 0.5f;
                armorDecrease = 0.25f;
                break;
            case 2:
                meleeDamageIncrease = 0.6f;
                armorDecrease = 0.2f;
                break;
            case 3:
                meleeDamageIncrease = 0.75f;
                armorDecrease = 0.15f;
                break;
            default:
                break;
        }

        player.additionalPhysicalDamageModifer += meleeDamageIncrease;
        player.currentArmorValue -= armorDecrease;
        player.UpdateDamageValues();
        player.UpdateArmorValues();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isRageActive = false;

        player.additionalPhysicalDamageModifer -= meleeDamageIncrease;
        player.currentArmorValue += armorDecrease;
        player.UpdateDamageValues();
        player.UpdateArmorValues();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        player.healthEvent.CallRageWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Shatter Cry special move
    /// </summary>
    private void ShatterCry(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.animator.SetTrigger("shatterCry");
            activeSkillTypeOneAnimator.gameObject.SetActive(true);

            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            player.healthEvent.CallShatterCrySpecialMoveEvent(); // This is for displaying shatter cry icon
            player.isShatterCryActive = true;
            StartCoroutine(ShatterCryRoutine(slotIndex, activeSkillData));
        }
    }

    // Triggered By Animation Event
    private void PerformShatterCry()
    {
        CircleCollider2D circleCollider2D = activeSkillTypeOneAnimator.gameObject.GetComponent<CircleCollider2D>();

        // Get all colliders within the radius of the seismic slam
        Collider2D[] colliders = Physics2D.OverlapCircleAll(transform.position, circleCollider2D.radius);

        foreach (Collider2D col in colliders)
        {
            // Check if the collider belongs to an enemy or any other object you want to affect
            if (col.CompareTag(Settings.enemyTag))
            {
                // Apply damage to the enemy
                Enemy affectedEnemy = col.GetComponent<Enemy>();

                if (affectedEnemy.health.currentHealth > 0)
                {
                    // Apply effects
                    player.meleeAttackMainHand.CheckFearStatus(affectedEnemy, true);
                }
            }
        }
    }

    IEnumerator ShatterCryRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isShatterCryActive = false;
        activeSkillTypeOneAnimator.gameObject.SetActive(false);
        player.healthEvent.CallShatterCryWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Axe throw special move
    /// </summary>
    private void AxeThrow(int slotIndex)
    {
        //Reset precharge for loading again
        isSoundPlayed = false;
        player.isAxeThrowActive = true;

        player.playerSkillProjectile = player.playerDetails.throwingAxeDetails.projectilePrefabArray[0].GetComponentInChildren<Projectile>();

        // Modify throwing axe's damage
        player.playerSkillProjectile.maxDamage = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
        {
            1 => player.currentMainHandMaxDamageValue,
            2 => player.currentMainHandMaxDamageValue * 1.2f,
            3 => player.currentMainHandMaxDamageValue * 1.5f,
            _ => player.currentMainHandMaxDamageValue
        };

        // OFF-HAND
        AttackShape offHandAttackType = DetermineAttackType(player.activeWeapon.GetCurrentOffHandWeapon()?.weaponDetails);
        player.meleeAttackEvent.CallAttackEvent(aimDirection, player.activeWeapon.GetCurrentOffHandWeapon(), offHandAttackType, MeleeHand.OffHand);
    }

    /// <summary>
    /// Execute Whirlrend special move
    /// </summary>
    private void Whirlrend(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isWhirlrendActive)
            {
                player.animator.SetBool("whirlrend", true);
                activeSkillTypeTwoAnimator.gameObject.SetActive(true);
                SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
                player.healthEvent.CallWhirlrendSpecialMoveEvent(); // This is for displaying rage icon
                player.isWhirlrendActive = true;
                StartCoroutine(WhirlrendRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator WhirlrendRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isWhirlrendActive = false;

        player.animator.SetBool("whirlrend", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.healthEvent.CallWhirlrendWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Feast of War special move
    /// </summary>
    private void FeastOfWar(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isFeastOfWarActive)
            {
                player.healthEvent.CallFeastOfWarSpecialMoveEvent(); // This is for displaying valor icon
                player.isFeastOfWarActive = true;
                StartCoroutine(FeastOfWarRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator FeastOfWarRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isFeastOfWarActive = false;
        player.healthEvent.CallFeastOfWarWoreOffEvent();
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Execute Don't Blink special move
    /// </summary>
    private void DontBlink(int inputSlotNumber, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        StartCoroutine(DontBlinkRoutine(inputSlotNumber, activeSkillData));
    }

    IEnumerator DontBlinkRoutine(int inputSlotNumber, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        DontBlink dontBlink = activeSkillTypeOneAnimator.GetComponent<DontBlink>();

        // Get target position from mouse
        Vector3 mouseWorld = HelperUtilities.GetMouseWorldPosition();
        Vector2 targetPos = new Vector2(mouseWorld.x, mouseWorld.y);

        // Range Check
        if (!dontBlink.IsWithinRange(targetPos)) yield break;

        // Save original position
        Vector2 originalPos = player.rb2D.position;

        Room currentRoom = GameManager.Instance.GetCurrentRoom();

        Vector3Int pointerCellPosition = currentRoom.instantiatedRoom.grid.WorldToCell(mouseWorld);

        // Check if the clicked tile is not marked as an obstacle
        if (!IsObstacleTile(currentRoom, pointerCellPosition))
        {
            player.health.isDamageable = false;

            player.cancelledDueToInvalidTile = false;
            int consumedMana = (int)(activeSkillData.manaCost * (1 - player.additionalManaReductionModifier));

            player.mana.ConsumeMana(consumedMana);

            // Teleport the character to the clicked tile
            player.rb2D.position = mouseWorld;

            // Disable movement + velocity
            player.rb2D.linearVelocity = Vector2.zero;

            // Trigger both hand attacks with DontBlink flag
            Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
            Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

            player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, mainHandWeapon, AttackShape.Thrust, MeleeHand.MainHand, false, false, false, false, isDontBlink: true);
            player.meleeAttackEvent.CallAttackEvent(AimDirection.Up, offHandWeapon, AttackShape.Thrust, MeleeHand.OffHand, false, false, false, false, isDontBlink: true);

            // Wait for attack animation duration (must match animation event timing)
            yield return new WaitForSeconds(0.4f); // Adjust based on the animation that calls DetectColliders()

            player.isDontBlinkActive = false;

            // Teleport back to original position
            player.rb2D.linearVelocity = Vector2.zero;
            player.rb2D.position = originalPos;

            yield return new WaitForSeconds(0.6f);

            player.health.isDamageable = true;
        }
        else
        {
            player.cancelledDueToInvalidTile = true;
        }
    }

    /// <summary>
    /// Execute Venomous Ivy special move
    /// </summary>
    private void VenomousIvy(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);
            activeSkillTypeTwoAnimator.SetBool("venomousIvy", true);

            //SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            player.isVenomousIvyActive = true;
            StartCoroutine(VenomousIvyRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator VenomousIvyRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isVenomousIvyActive = false;
        activeSkillTypeTwoAnimator.SetBool("venomousIvy", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Fade and Feed special move
    /// </summary>
    private void FadeAndFeed(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeThreeAnimator.gameObject.SetActive(true);
            activeSkillTypeThreeAnimator.SetBool("fadeAndFeed", true);

            // ENABLE SKILL EFFECTS
            player.healthEvent.CallFadeAndFeedSpecialMoveEvent(); // This is for displaying guarded oath icon

            // EFFECTS
            int gainedHealth = player.playerDetails.thirdActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 8,
                2 => 10,
                3 => 12,
                _ => 8
            };

            player.health.AddHealth(gainedHealth);

            player.UpdateDamageValues();
            player.UpdateBlockValue();
            player.UpdateArmorValues();
            player.UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            player.isFadeAndFeedActive = true;
            StartCoroutine(FadeAndFeedRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator FadeAndFeedRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        // ENABLE SKILL EFFECTS
        player.healthEvent.CallFadeAndFeedSpecialMoveEndEvent(); // This is for displaying fade and feed icon

        player.isFadeAndFeedActive = false;
        activeSkillTypeThreeAnimator.SetBool("fadeAndFeed", false);
        activeSkillTypeThreeAnimator.gameObject.SetActive(false);

        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    /// <summary>
    /// Execute Blade Dash special move
    /// </summary>
    private void BladeDash(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData, bool isInitialCast)
    {
        float dashForce = 6f;     // Tune this based on weight/mass
        float dashDuration = 0.2f; // Very short, explosive
        isDashing = true;

        // Get target position from mouse
        Vector3 mouseWorld = HelperUtilities.GetMouseWorldPosition();
        Vector2 targetPos = new Vector2(mouseWorld.x, mouseWorld.y);

        Vector2 direction = (targetPos - player.rb2D.position).normalized;

        player.animator.SetBool("bladeDash", true);
        activeSkillTypeFourAnimator.gameObject.SetActive(true);

        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        StartCoroutine(BladeDashRoutine(direction, dashForce, dashDuration, activeSkillData, isInitialCast));
    }

    IEnumerator BladeDashRoutine(Vector2 direction, float dashForce, float dashDuration, ActiveUniqueSkillDetailsSO.LevelData activeSkillData, bool isInitialCast)
    {
        LayerMask originalMask = player.polygonCollider2D.forceReceiveLayers;
        player.polygonCollider2D.forceReceiveLayers = LayerMask.GetMask(); // Nothing

        player.isBladeAndDashActive = true;
        player.bladeAndDashOnRecast = !isInitialCast;

        float elapsed = 0f;
        float dashSpeed = dashForce / dashDuration;

        while (elapsed < dashDuration)
        {
            Vector2 moveStep = direction * dashSpeed * Time.fixedDeltaTime;
            player.rb2D.MovePosition(player.rb2D.position + moveStep);

            elapsed += Time.fixedDeltaTime;
            yield return new WaitForFixedUpdate();
        }

        activeSkillTypeFourAnimator.gameObject.SetActive(false);

        // Reset collision
        player.polygonCollider2D.forceReceiveLayers = originalMask;

        isDashing = false;
        player.animator.SetBool("bladeDash", false);
        player.isBladeAndDashActive = false;
    }

    /// <summary>
    /// Execute Shiruken special move
    /// </summary>
    private void Shiruken(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        //Reset precharge for loading again
        isSoundPlayed = false;
        player.isShirukenActive = true;

        player.playerSkillProjectile = player.playerDetails.shirukenDetails.projectilePrefabArray[0].GetComponentInChildren<Projectile>();

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection,
            false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false, null, null, false, null, false, null, false, null, true, player.playerDetails.shirukenDetails);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isShirukenActive = false));
    }

    /// <summary>
    /// Execute Mist of Disruption special move
    /// </summary>
    private void MistOfDisruption(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            if (!player.isMistOfDisruptionActive)
            {
                player.isMistOfDisruptionActive = true;
                StartCoroutine(MistOfDisruptionRoutine(slotIndex, activeSkillData));
            }
        }
    }

    IEnumerator MistOfDisruptionRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        GameObject mistOfDisruptionObject = Instantiate(activeSkillTypeOneAnimator.gameObject, transform.position, Quaternion.identity);

        mistOfDisruptionObject.GetComponent<Animator>().SetBool("mistOfDisruption", true);

        SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        player.isMistOfDisruptionActive = false;

        mistOfDisruptionObject.GetComponent<Animator>().SetBool("mistOfDisruption", false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended

        Destroy(mistOfDisruptionObject, 2f); // Destroy mist object after animation completed
    }

    /// <summary>
    /// Execute Nymara's Windveil special move
    /// </summary>
    private void NymarasWindveil(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeTwoAnimator.gameObject.SetActive(true);

            activeSkillTypeTwoAnimator.SetBool("nymarasWindveil", true);
            player.healthEvent.CallNymarasWindveilSpecialMoveEvent(); // This is for displaying Nymara'w Windveil icon
            player.isNymarasWindveilActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            // Effects
            float speedIncrease = player.playerDetails.secondActiveSkillDetails.GetCurrentActiveLevel() switch
            {
                1 => 1,
                2 => 2,
                3 => 3,
                _ => 0,
            };

            player.additionalSpeedModifier += speedIncrease;
            player.UpdateSpeedValue();
            StaticEventHandler.CallStatsChangedOnTheBookEvent();

            StartCoroutine(NymarasWindveilRoutine(slotIndex, activeSkillData, speedIncrease));

            GameObject forceFieldObject = player.forcefieldTransform.gameObject;
            forceFieldObject.SetActive(true);
        }
    }

    IEnumerator NymarasWindveilRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData, float speedIncrease)
    {
        float duration = activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier); // Duration
        float elapsed = 0f;

        while (elapsed < duration)
        {
            Windveil(slotIndex);
            yield return new WaitForSeconds(0.2f);
            elapsed += 0.2f;
        }

        // Effects wore off
        player.additionalSpeedModifier -= speedIncrease;
        player.UpdateSpeedValue();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        player.isNymarasWindveilActive = false;
        player.healthEvent.CallNymarasWindveilWoreOffEvent();
        activeSkillTypeTwoAnimator.SetBool("nymarasWindveil", false);
        activeSkillTypeTwoAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duratin of aura skill ended
    }

    /// <summary>
    /// Based on circle radius of the bomb, detect all enemy colliders for damage
    /// </summary>
    public void Windveil(int slotIndex)
    {
        CircleCollider2D circleCollider = player.forcefieldTransform.GetComponent<CircleCollider2D>();

        if (circleCollider == null)
        {
            Debug.LogWarning("CircleCollider2D not found on forcefieldTransform.");
            return;
        }

        // Use world position of the forcefield object
        Vector2 origin = player.forcefieldTransform.position;

        // Convert local radius to world-space radius (in case of scaling)
        float worldRadius = circleCollider.radius * Mathf.Max(player.forcefieldTransform.lossyScale.x, player.forcefieldTransform.lossyScale.y);

        // Perform overlap check
        player.filter.SetLayerMask(player.layerMask);
        Collider2D[] results = new Collider2D[20];

        int hitCount = Physics2D.OverlapCircle(origin, worldRadius, player.filter, results);

        for (int i = 0; i < hitCount; i++)
        {
            Collider2D collider = results[i];
            if (collider == null) continue;

            // Match by tag instead of collider type
            if (collider.CompareTag(Settings.enemyTag))
            {
                Enemy enemy = collider.GetComponent<Enemy>();

                if (enemy != null && !enemy.isStatic)
                {
                    player.meleeAttackMainHand.CheckStaticStatus(enemy, true, player.isConductiveTouchActive);
                }
            }
        }
    }

    /// <summary>
    /// Execute Chain Lightning special move
    /// </summary>
    private void ChainLightning(int slotIndex)
    {
        Vector3 weaponDirection;
        float weaponAngleDegrees, playerAngleDegrees;
        AimDirection playerAimDirection;
        AttackDirection playerAttackDirection;

        // Aim weapon input
        AimWeaponInput(out weaponDirection, out weaponAngleDegrees, out playerAngleDegrees, out playerAimDirection, out playerAttackDirection);

        player.isChainLightningActive = true;
        //Reset precharge for loading again
        isSoundPlayed = false;

        // Trigger fire weapon event
        player.fireWeaponEvent.CallFireWeaponEvent(true, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponCurrentProjectile.isLaser,
            playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, false, false, false, 0, 0, 0, 0, 0, 0, 0, 0, false, false, false, null,
            null, false, null, false, null, false, null, false, null, true, player.playerDetails.chainLightningDetails, ChainLightningPhase.First);

        StartCoroutine(NullifyBooleanAfterTwoFrame(() => player.isChainLightningActive = false));
    }

    /// <summary>
    /// Execute Eye of the storm special move
    /// </summary>
    private void EyeOfTheStorm(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            player.isEyeOfTheStormActive = true;
            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
            StartCoroutine(EyeOfTheStormRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator EyeOfTheStormRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        activeSkillTypeFourAnimator.gameObject.SetActive(true);

        GameObject eyeOfTheStormObject = Instantiate(activeSkillTypeFourAnimator.gameObject, HelperUtilities.GetMouseWorldPosition(), Quaternion.identity);

        eyeOfTheStormObject.GetComponent<Animator>().SetBool("eyeOfTheStorm", true);

        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        SoundEffectManager.Instance.StopSoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);
        player.isEyeOfTheStormActive = false;

        eyeOfTheStormObject.GetComponent<Animator>().SetBool("eyeOfTheStorm", false);
        activeSkillTypeFourAnimator.gameObject.SetActive(false);
        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended

        Destroy(eyeOfTheStormObject, 2f); // Destroy eye of the storm object after animation completed
    }

    /// <summary>
    /// Execute Ionic Rejuvenation special move
    /// </summary>
    private void IonicRejuvenation(int slotIndex, ref ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        if (player.specialMoveDurationTimerArray[slotIndex - 1] < activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier))
        {
            activeSkillTypeThreeAnimator.gameObject.SetActive(true);
            activeSkillTypeThreeAnimator.SetBool("ionicRejuvenation", true);

            // ENABLE SKILL EFFECTS
            player.healthEvent.CallIonicRejuvenationSpecialMoveEvent(); // This is for displaying ionic rejuvenation icon

            // EFFECTS
            ClearNegativeStatusEffects();

            int restoredHealth = 0;

            if (player.isInMistOfDisruption)
            {
                restoredHealth = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 12,
                    2 => 14,
                    3 => 16,
                    _ => 0
                };
            }
            else
            {
                restoredHealth = player.playerDetails.fifthActiveSkillDetails.GetCurrentActiveLevel() switch
                {
                    1 => 6,
                    2 => 7,
                    3 => 8,
                    _ => 0
                };
            }

            player.health.AddHealth(restoredHealth);

            player.UpdateDamageValues();
            player.UpdateBlockValue();
            player.UpdateArmorValues();
            player.UpdateSpeedValue();

            StaticEventHandler.CallStatsChangedOnTheBookEvent(); // Book UI

            SoundEffectManager.Instance.PlaySoundEffect(player.currentlyUsedActiveUniqueSkills[slotIndex].activeUniqueSkillSoundEffectOne);

            player.isIonicRejuvenationActive = true;
            StartCoroutine(IonicRejuvenationRoutine(slotIndex, activeSkillData));
        }
    }

    IEnumerator IonicRejuvenationRoutine(int slotIndex, ActiveUniqueSkillDetailsSO.LevelData activeSkillData)
    {
        // Wait until effective duration of skill ended
        yield return new WaitForSeconds(activeSkillData.effectiveDuration * (1 + player.currentSkillDurationModifier));

        // ENABLE SKILL EFFECTS
        player.healthEvent.CallIonicRejuvenationWoreOffEvent(); // This is for displaying ionic rejuvenation icon

        player.isIonicRejuvenationActive = false;
        activeSkillTypeThreeAnimator.SetBool("ionicRejuvenation", false);
        activeSkillTypeThreeAnimator.gameObject.SetActive(false);

        player.specialMovesCooldownCheckArray[slotIndex - 1] = true; // Start cooldown process after effective duration of aura skill ended
    }

    IEnumerator NullifyBooleanAfterTwoFrame(System.Action setFalseAction)
    {
        yield return null;
        yield return null;

        setFalseAction?.Invoke();
    }

    private void ViciousMomentumCheck()
    {
        if (player.isViciousMomentumActive)
        {
            if (player.viciousMomentumTriggered)
            {
                player.viciousMomentumCooldownTimer += Time.deltaTime;

                if (!player.viciousMomentumAttackBonusObtained)
                {
                    // Damage Boost
                    player.additionalPhysicalDamageModifer += 0.05f;
                    player.UpdateDamageValues();
                    player.healthEvent.CallViciousMomentumEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.viciousMomentumAttackBonusObtained = true;
                }

                if (player.viciousMomentumCooldownTimer > player.viciousMomentumDuration)
                {
                    // Damage Reset
                    player.additionalPhysicalDamageModifer -= 0.05f;
                    player.UpdateDamageValues();
                    player.healthEvent.CallViciousMomentumWoreOffEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.viciousMomentumCooldownTimer = 0f;
                    player.viciousMomentumTriggered = false;
                    player.viciousMomentumAttackBonusObtained = false;
                }
            }
        }
    }

    private void CombatFocusCheck()
    {
        if (player.isCombatFocusActive)
        {
            if (player.combatFocusTriggered)
            {
                player.combatFocusCooldownTimer += Time.deltaTime;

                Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

                float originalIncreaseAmount = 0f;

                if (!player.combatFocusCooldownBonusObtained)
                {
                    if (mainHandWeapon != null)
                    {
                        switch (mainHandWeapon.weaponDetails.weaponClass)
                        {
                            case WeaponClass.Sword:
                            case WeaponClass.Axe:
                            case WeaponClass.Hammer:
                            case WeaponClass.Spear:
                            case WeaponClass.Dagger:
                            case WeaponClass.Claw:
                                originalIncreaseAmount = 0.2f;
                                player.additionalAttackCoolDownModifier += originalIncreaseAmount;
                                break;
                            case WeaponClass.Staff:
                            case WeaponClass.Bow:
                            case WeaponClass.Crossbow:
                                originalIncreaseAmount = 0.1f;
                                player.additionalAttackCoolDownModifier += originalIncreaseAmount;
                                break;
                            case WeaponClass.Shield:
                                break;
                            default:
                                break;
                        }
                    }

                    player.healthEvent.CallCombatFocusEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.viciousMomentumAttackBonusObtained = true;
                }

                if (player.combatFocusCooldownTimer > player.combatFocusDuration)
                {
                    if (mainHandWeapon != null)
                    {
                        switch (mainHandWeapon.weaponDetails.weaponClass)
                        {
                            case WeaponClass.Sword:
                            case WeaponClass.Axe:
                            case WeaponClass.Hammer:
                            case WeaponClass.Spear:
                            case WeaponClass.Dagger:
                            case WeaponClass.Claw:
                                player.additionalAttackCoolDownModifier -= originalIncreaseAmount; // Cooldown reset
                                break;
                            case WeaponClass.Staff:
                            case WeaponClass.Bow:
                            case WeaponClass.Crossbow:
                                player.additionalAttackCoolDownModifier -= originalIncreaseAmount;
                                break;
                            case WeaponClass.Shield:
                                break;
                            default:
                                break;
                        }
                    }

                    player.healthEvent.CallCombatFocusWoreOffEvent();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    player.combatFocusCooldownTimer = 0f;
                    player.combatFocusTriggered = false;
                    player.combatFocusCooldownBonusObtained = false;
                }
            }
        }
    }

    private void TriadExecutionCheck()
    {
        if (player.isTriadExecutionActive)
        {
            Weapon mainHandWeapon = player?.activeWeapon?.GetCurrentMainHandWeapon();
            float originalIncreaseAmount = 0f;

            if (player.triadExecutionCounter >= 3 && !player.triadExecutionTriggered)
            {
                player.triadExecutionTriggered = true;
                player.triadExecutionCounter = 0;

                if (mainHandWeapon != null)
                {
                    switch (mainHandWeapon.weaponDetails.weaponClass)
                    {
                        case WeaponClass.Sword:
                        case WeaponClass.Axe:
                        case WeaponClass.Hammer:
                        case WeaponClass.Spear:
                        case WeaponClass.Dagger:
                        case WeaponClass.Claw:
                            originalIncreaseAmount = 0.2f;
                            player.additionalPhysicalDamageModifer -= originalIncreaseAmount; // Cooldown reset
                            break;
                        case WeaponClass.Staff:
                        case WeaponClass.Bow:
                        case WeaponClass.Crossbow:
                            originalIncreaseAmount = 0.1f;
                            player.additionalPhysicalDamageModifer -= originalIncreaseAmount;
                            break;
                        case WeaponClass.Shield:
                            break;
                        default:
                            break;
                    }
                }

                player.UpdateDamageValues();
                player.healthEvent.CallTriadExecutionEvent();
                StaticEventHandler.CallStatsChangedOnTheBookEvent();
            }
            else
            {
                if (player.triadExecutionTriggered)
                {
                    player.additionalPhysicalDamageModifer -= originalIncreaseAmount;
                    player.UpdateDamageValues();
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                    StartCoroutine(DisableImageForTriadExecutionRoutine());

                    player.triadExecutionTriggered = false;
                }
            }
        }
    }

    private bool HasNegativeStatusEffect()
    {
        if ((player.moveStatus & MoveStatus.Root) != 0 || (player.moveStatus & MoveStatus.Stun) != 0 || (player.moveStatus & MoveStatus.Paralyze) != 0 ||
            (player.moveStatus & MoveStatus.Frozen) != 0)
        {
            return true;
        }

        if ((player.healthStatus & HealthStatus.Poisoned) != 0 || (player.healthStatus & HealthStatus.Burned) != 0 || (player.healthStatus & HealthStatus.Bleeding) != 0)
        {
            return true;
        }

        if (player.isCursed || player.isFeared || player.isRevealed || player.isWarmed || player.isChilled || player.isBlind || player.isSlowed) return true;

        return false;
    }

    private void ClearNegativeStatusEffects()
    {
        player.moveStatus = MoveStatus.Idle;
        player.healthStatus = HealthStatus.Normal;

        player.isCursed = false;
        player.isFeared = false;
        player.isRevealed = false;
        player.isCursed = false;
        player.isBlind = false;
        player.isSlowed = false;

        player.healthEvent.CallCuredCompletelyEvent();
    }

    IEnumerator DisableImageForTriadExecutionRoutine()
    {
        yield return new WaitForSeconds(1f);

        player.healthEvent.CallTriadExecutionWoreOffEvent();
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
        DropProcess(dropType, itemGeneric, (Weapon)null, dropOffHand, itemSlotStatus, inventoryIndex, dropButton);
    }

    public void DropProcess(DropType dropType, ItemGeneric itemGeneric = null, Weapon toBeSwappedWeapon = null, bool dropOffHand = false, 
        ItemSlotStatus itemSlotStatus = ItemSlotStatus.None, int inventoryIndex = -1, bool dropButton = false)
    {
        if (dropType == DropType.PassiveItem)
        {
            PassiveItem passiveItem = (PassiveItem)itemGeneric;

            if (itemSlotStatus == ItemSlotStatus.Inventory)
            {
                // Just drop from inventory to the floor
                SpawnDroppedPassiveItem(passiveItem);
                InventoryManager.Instance.EmptyItemFromInventory(inventoryIndex);
            }
            else if (dropButton)
            {
                // Manual drop by clicking drop button
                SpawnDroppedPassiveItem(passiveItem);
                player.setPassiveItemEvent.CallRemovePassiveItem(
                    passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName, isSwap: false, dropButton: true);
            }
            else if (InventoryManager.Instance.IsInventoryFull())
            {
                // Forced drop due to full inventory during item pickup
                SpawnDroppedPassiveItem(passiveItem);
                player.setPassiveItemEvent.CallRemovePassiveItem(
                    passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName);
            }
            else
            {
                // Regular swap -> previous goes to inventory
                player.setPassiveItemEvent.CallRemovePassiveItem(
                    passiveItem, passiveItem.passiveItemDetails.passiveItemSlotName);

                int index = InventoryManager.Instance.FindIndexOfItem(passiveItem);
                passiveItem.itemSlotStatus = ItemSlotStatus.Inventory;
                StaticEventHandler.CallPassiveItemAddedToInventorySlot(passiveItem, index);
            }
            return;
        }

        // === WEAPON ===
        Weapon weapon = (Weapon)itemGeneric;

        bool IsSwapScenario() => !dropButton && toBeSwappedWeapon != null;

        // Inventory-drop case (no active slot involvement)
        if (itemSlotStatus == ItemSlotStatus.Inventory)
        {
            SpawnDroppedWeapon(weapon);

            // Empty inventory slot + Book update
            InventoryManager.Instance.EmptyItemFromInventory(inventoryIndex);
            StaticEventHandler.CallInventoryWeaponDroppedEventForBook(inventoryIndex);
            return;
        }

        // MAIN-HAND
        if (weapon.itemSlotStatus == ItemSlotStatus.MainHand)
        {
            if (dropButton)
            {
                if (!SlotPlacementRules.IsPlacementAllowed(weapon, SlotType.Drop,
                        player.activeWeapon.GetCurrentMainHandWeapon(), player.currentWeaponSlotSetIndex))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                SpawnDroppedWeapon(weapon);
                DeactivateMainHandWeapon();
            }
            else if (InventoryManager.Instance.IsInventoryFull())
            {
                // Forced floor drop during pickup
                if (IsSwapScenario() &&
                    !SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                        player.activeWeapon.GetCurrentMainHandWeapon(),
                        player.activeWeapon.GetCurrentOffHandWeapon(), false))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                SpawnDroppedWeapon(weapon);
                DeactivateMainHandWeapon();
            }
            else
            {
                // Move current to inventory during pickup
                if (IsSwapScenario() &&
                    !SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                        player.activeWeapon.GetCurrentMainHandWeapon(),
                        player.activeWeapon.GetCurrentOffHandWeapon(), false))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                DeactivateMainHandWeapon();
                int index = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(weapon);
                weapon.itemSlotStatus = ItemSlotStatus.Inventory;
                StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, index);
            }

            // Stats + Book + flags
            player.UpdateDamageValues();
            player.UpdateAttackRatingAndCriticalValues();
            player.UpdateBlockAndDodgeValues();

            StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponMainHand);
            player.mainHandSlotFilled = false;

            return;
        }

        // OFF-HAND
        {
            if (dropButton)
            {
                if (!SlotPlacementRules.IsPlacementAllowed(weapon, SlotType.Drop,
                        player.activeWeapon.GetCurrentMainHandWeapon(), player.currentWeaponSlotSetIndex))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                SpawnDroppedOffhandWeapon(weapon);
                DeactivateOffhandWeapon();
            }
            else if (InventoryManager.Instance.IsInventoryFull())
            {
                if (IsSwapScenario() &&
                    !SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                        player.activeWeapon.GetCurrentMainHandWeapon(),
                        player.activeWeapon.GetCurrentOffHandWeapon(), true))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                // Forced floor drop during pickup
                SpawnDroppedOffhandWeapon(weapon);
                DeactivateOffhandWeapon();
            }
            else
            {
                // Move current to inventory during pickup
                if (IsSwapScenario() &&
                    !SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                        player.activeWeapon.GetCurrentMainHandWeapon(),
                        player.activeWeapon.GetCurrentOffHandWeapon(), true))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                DeactivateOffhandWeapon();
                int index = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(weapon);
                weapon.itemSlotStatus = ItemSlotStatus.Inventory;
                StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, index);
            }

            if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
            {
                player.isShieldCalculated = false;
            }

            // Stats + Book + flags
            player.UpdateDamageValues();
            player.UpdateArmorValues();
            player.UpdateAttackRatingAndCriticalValues();
            player.UpdateBlockAndDodgeValues();

            StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponOffHand);
            player.offHandSlotFilled = false;
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

    public void DeactivateOffhandWeapon()
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
        player.movementByForce.moveSpeed = player.movementByForce.movementDetails.GetBaseMaxMoveSpeed() + player.CurrentAgilityValue * 0.25f;
    }

    /// <summary>
    /// Disable the player movement
    /// </summary>
    public void DisablePlayer()
    {
        isPlayerMovementDisabled = true;
        player.movementByForce.moveSpeed = 0f;
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
