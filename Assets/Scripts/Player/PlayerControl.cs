using Mirror;
using Pathfinding.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using Unity.Android.Gradle.Manifest;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[RequireComponent(typeof(Player))]
[DisallowMultipleComponent]
public class PlayerControl : MonoBehaviour
{
    Player player;

    [HideInInspector] public bool isSoundPlayed = false;

    [HideInInspector] public float movementTimer = 0;
    [HideInInspector] public bool isPlayerRolling;
    [HideInInspector] public bool IsParrying { get => isParrying; set { isParrying = value; } }
    [HideInInspector] public Vector3 playerBodycenterPosition;

    // Input gamepad
    [SerializeField] float cursorSpeed = 1000f;
    Vector2 lastValidGamepadAimInput = Vector2.zero;

    Vector2 movementInput;

    bool isPlayerMovementDisabled = false;
    [HideInInspector] public bool wasMoving = false;

    Coroutine stunCoroutine;
    Coroutine rootCoroutine;
    Coroutine frostCoroutine;
    Coroutine paralyzeCoroutine;
    Coroutine curseCoroutine;

    Coroutine playerRollCoroutine;
    WaitForFixedUpdate waitForFixedUpdate;
    float playerRollCooldownTimer = 0f;
    bool isParrying;
    float playerParryDurationTimer = 0f;
    float playerParryCooldownTimer = 0f;
    float playerParryEffectiveDuration = 0.4f;
    float playerParryCooldownDuration = 1.6f;

    [HideInInspector] public AimDirection aimDirection;
    [HideInInspector] public AttackDirection attackDirection;

    [HideInInspector] public bool isDashing;

    // Attack member variables
    [HideInInspector] public AttackShape meleeAttackTypeMainHand = AttackShape.None;
    [HideInInspector] public AttackShape meleeAttackTypeOffHand = AttackShape.None;

    List<SpriteRenderer> allSpriteRenderers = new List<SpriteRenderer>();

    GameState gameState;

    private void Awake()
    {
        player = GetComponent<Player>();
    }

    private void OnEnable()
    {
        player.healthEvent.OnHealthChanged += HealthEvent_OnHealthChanged;

        waitForFixedUpdate = new WaitForFixedUpdate();
        allSpriteRenderers.Add(player.spriteRenderer);
        if (InputManager.IsGamepad()) lastValidGamepadAimInput = Vector2.right;
    }

    private void OnDisable()
    {
        player.healthEvent.OnHealthChanged -= HealthEvent_OnHealthChanged;
    }

    private void Update()
    {
        if (player == null || !player.IsLocal) return;
        if (!player.isInitialized) return;

        playerBodycenterPosition = player.transform.position + new Vector3(0f, 0.65f, 0f);

        if (!InputManager.GameplayInputEnabled) return;

        if (!IsLocalInputAllowed()) return;

        // Passive Updates
        PlayerSpecificPassivesCheck();
        InnerPathPassivesCheck();

        HandleCurse();
        HandleCrowdControl();

        // Timers tick regardless of CC
        PlayerRollCooldownTimer();
        PlayerParryCooldownTimer();

        // Meta Input
        if (IsLocalInputAllowed())
        {
            HandleGameState();
            HandleBookState();
            HandlePopUpState();
            HandleSceneTransitionReady();
        }

        // Physical Input Gate
        if (!CanPerformPhysicalActions()) return;

        if (player.moveStatus == MoveStatus.Idle)
        {
            player.polygonCollider2D.enabled = true; //Reset if knocked back wore off

            if (IsLocalInputAllowed())
            {
                // Process the player weapon input
                WeaponInput();
                // Process the player movement input
                MovementInput();
                // Process the player use item input
                UseItemInput();
                // Process the player use special move input
                SpecialMoveInput();
            }
        }
    }

    private bool IsLocalInputAllowed()
    {
        if (player == null) return false;

        return player.IsLocal;
    }

    private void HandleCrowdControl()
    {
        switch (GetDominantCC())
        {
            case MoveStatus.KnockedBack:
                HandleKnockBack();
                break;
            case MoveStatus.Stun:
                HandleStun();
                break;
            case MoveStatus.Paralyze:
                HandleParalyze();
                break;
            case MoveStatus.Frozen:
                HandleFrozen();
                break;
            case MoveStatus.Root:
                HandleRoot();
                break;
            default:
                break;
        }
    }

    private MoveStatus GetDominantCC()
    {
        if ((player.moveStatus & MoveStatus.KnockedBack) != 0) return MoveStatus.KnockedBack;
        if ((player.moveStatus & MoveStatus.Stun) != 0) return MoveStatus.Stun;
        if ((player.moveStatus & MoveStatus.Paralyze) != 0) return MoveStatus.Paralyze;
        if ((player.moveStatus & MoveStatus.Frozen) != 0) return MoveStatus.Frozen;
        if ((player.moveStatus & MoveStatus.Root) != 0) return MoveStatus.Root;

        return MoveStatus.Idle;
    }

    private bool CanPerformPhysicalActions()
    {
        if (isPlayerMovementDisabled) return false;
        if (isPlayerRolling) return false;

        if ((player.moveStatus & MoveStatus.Frozen) != 0) return false;
        if ((player.moveStatus & MoveStatus.Stun) != 0) return false;
        if ((player.moveStatus & MoveStatus.Paralyze) != 0) return false;
        if ((player.moveStatus & MoveStatus.Root) != 0) return false;
        if ((player.moveStatus & MoveStatus.KnockedBack) != 0) return false;

        return true;
    }

    private void HandleCurse()
    {
        if (player.isCursed)
        {
            if (curseCoroutine == null) curseCoroutine = StartCoroutine(CurseRoutine());
        }
    }

    private void HandleFrozen()
    {
        if ((player.moveStatus & MoveStatus.Frozen) == 0) return;

        isPlayerRolling = false;
        player.meleeAttackMainHand.IsAttacking = false;

        if (frostCoroutine == null) frostCoroutine = StartCoroutine(FrostRoutine());
    }

    private void HandleStun()
    {
        if ((player.moveStatus & MoveStatus.Stun) == 0) return;

        isPlayerRolling = false;
        player.meleeAttackMainHand.IsAttacking = false;
        player.animatePlayer.ResetAnimatonParameters();

        // Reset the state to idle
        player.animator.SetFloat(Settings.motionType, 0f);
        player.animator.SetBool(Settings.isIdle, true);

        if (stunCoroutine == null) stunCoroutine = StartCoroutine(StunRoutine());
    }

    private void HandleParalyze()
    {
        if ((player.moveStatus & MoveStatus.Paralyze) == 0) return;

        isPlayerRolling = false;
        player.meleeAttackMainHand.IsAttacking = false;
        player.animatePlayer.ResetAnimatonParameters();

        // Reset the state to idle
        player.animator.SetFloat(Settings.motionType, 0f);
        player.animator.SetBool(Settings.isIdle, true);

        if (paralyzeCoroutine == null) paralyzeCoroutine = StartCoroutine(ParalyzeRoutine());
    }

    private void HandleRoot()
    {
        if ((player.moveStatus & MoveStatus.Root) == 0) return;

        isPlayerRolling = false;
        player.meleeAttackMainHand.IsAttacking = false;
        player.animatePlayer.ResetAnimatonParameters();

        // Reset the state to idle
        player.animator.SetFloat(Settings.motionType, 0f);
        player.animator.SetBool(Settings.isIdle, true);

        if (rootCoroutine == null) rootCoroutine = StartCoroutine(RootRoutine());
    }

    private void HandleKnockBack()
    {
        if ((player.moveStatus & MoveStatus.KnockedBack) == 0) return;

        isPlayerRolling = false;
        player.meleeAttackMainHand.IsAttacking = false;
        player.polygonCollider2D.enabled = false;

        {
            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                if (player.rb2D.linearVelocity.magnitude < 0.08f) player.rb2D.linearVelocity = Vector2.zero;

                //if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponPrechargeTime > 0)
                //{
                //    // Trigger fire weapon event for precharge weapons
                //    player.fireWeaponEvent.CallFireWeaponEvent(false, false, null, player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.
                //        weaponCurrentProjectile.isLaser, AimDirection.Right, 0f, 0f, Vector3.zero, false);
                //}
            }
        }
    }

    private void HandleGameState()
    {
        gameState = !NetworkServer.active && !NetworkClient.active ? GameManager.Instance.gameState : GameSessionManager.Instance.gameState;

        switch (gameState)
        {
            // While playing the level handle the tab key for the dungeon overview map
            case GameState.playingLevel:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    // Book always closes locally first
                    if (GameManager.Instance.bookView.activeSelf)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                        GameManager.Instance.bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
                        return;
                    }

                    // Single Player
                    if(!NetworkServer.active && !NetworkClient.active)
                    {
                        GameManager.Instance.ServerTogglePause_SP();                  
                    }
                    else
                    {
                        player.NetAuth.CmdRequestTogglePause();
                    }
                }

                // Overview map key is pressed
                if (InputManager.Instance.overviewMapFullView.action.WasPressedThisFrame() && !InputManager.overviewMapDisabled && !GameManager.Instance.isOverviewCameraClicked)
                {
                    GameManager.Instance.isOverviewCameraClicked = true;
                    GameManager.Instance.DisplayDungeonOverviewMap();
                }

                // Overview map key is released
                if (InputManager.Instance.overviewMapFullView.action.WasReleasedThisFrame() && !InputManager.overviewMapDisabled && GameManager.Instance.isOverviewCameraClicked)
                {
                    // Clear dungeonOverviewMap
                    GameManager.Instance.isOverviewCameraClicked = false;
                    player.cameraManager.dungeonMap.ClearDungeonOverViewMap();

                    player.cameraManager.dungeonMap.ClearDungeonOverViewMap();
                }
                break;

            case GameState.engagingEnemies:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    // Book always closes locally first
                    if (GameManager.Instance.bookView.activeSelf)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                        GameManager.Instance.bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
                        return;
                    }

                    // Single Player
                    if (!NetworkServer.active && !NetworkClient.active)
                    {
                        GameManager.Instance.ServerTogglePause_SP();
                    }
                    else
                    {
                        player.NetAuth.CmdRequestTogglePause();
                    }
                }
                break;

            case GameState.gamePaused:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    // Single Player
                    if (!NetworkServer.active && !NetworkClient.active)
                    {
                        GameManager.Instance.ServerTogglePause_SP();
                    }
                    else
                    {
                        player.NetAuth.CmdRequestTogglePause();
                    }
                }
                break;
            default:
                break;
        }
    }

    private void HandleBookState()
    {
        #region Animation Phase
        if (GameManager.Instance.pauseMenu.activeSelf) return;

        if (InputManager.TutorialEnabled) GameManager.Instance.bookView.GetComponent<Animator>().updateMode = AnimatorUpdateMode.Normal;
        else GameManager.Instance.bookView.GetComponent<Animator>().updateMode = AnimatorUpdateMode.UnscaledTime;

        Transform arrowTransform = GameManager.Instance.bookView.transform.GetChild(5);
        Transform secondArrowTransform = GameManager.Instance.bookView.transform.GetChild(6);

        GameManager.TutorialIndicatorArrowTransactions(arrowTransform, secondArrowTransform);

        if (!GameManager.Instance.bookView.activeSelf)
        {
            GameManager.Instance.gameplayUI.FadeGameplayUI(GameManager.Instance.gameplayUI.canvasGroup, 1f, 0.2f);
        }

        if (GameManager.Instance.bookZoomInFinished)
        {
            GameManager.Instance.bookZoomInFinished = false;
            BookUI.IsBookOpen = true;
        }

        if (GameManager.Instance.bookZoomOutFinished)
        {
            GameManager.Instance.bookZoomOutFinished = false;
            GameManager.Instance.bookView.SetActive(false);
            GameManager.Instance.bookCover.SetActive(false);
            GameManager.Instance.glossaryBookOpen = false;

            // Reset the time scale after the animation is done
            if (!InputManager.TutorialEnabled) Time.timeScale = 1f;
            GameManager.Instance.isBookClosing = false;

            // Fade in the gameplay UI
            GameManager.Instance.gameplayUI.FadeGameplayUI(GameManager.Instance.gameplayUI.canvasGroup, 1f, 0.2f);

            // Play the close book sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
        }

        if (GameManager.Instance.turnPageCompleted)
        {
            GameManager.Instance.bookView.GetComponent<Animator>().SetBool(Settings.turnPage, false);
            GameManager.Instance.turnPageCompleted = false;
        }
        #endregion

        if (!InputManager.glossaryDisabled)
        {
            if (InputManager.Instance.bookView.action.WasPressedThisFrame())
            {
                if (GameManager.Instance.bookView.activeSelf)
                {
                    if (!GameManager.Instance.isBookClosing)
                    {
                        GameManager.Instance.isBookClosing = true;
                        BookUI.IsBookOpen = false;

                        BookCloseProcess();
                    }
                }
                else
                {
                    GameManager.Instance.bookView.SetActive(true);
                    GameManager.Instance.bookCover.SetActive(true);
                    GameManager.Instance.glossaryBookOpen = true;

                    StaticEventHandler.CallOpenStatPageEvent();

                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);

                    // First trigger the animation (it uses UnscaledTime, so it's safe to call here)
                    GameManager.Instance.bookView.GetComponent<Animator>().SetTrigger(Settings.zoomIn);

                    if (!InputManager.TutorialEnabled) Time.timeScale = 0f;

                    // Finally, hide gameplay UI
                    GameManager.Instance.gameplayUI.FadeGameplayUI(GameManager.Instance.gameplayUI.canvasGroup, 0f, 0.1f); // Transparent
                }
            }
            if (InputManager.Instance.skillsInnerPathPage.action.WasPressedThisFrame())
            {
                if (GameManager.Instance.bookView.activeSelf)
                {
                    if (!GameManager.Instance.isBookClosing)
                    {
                        GameManager.Instance.isBookClosing = true;
                        BookCloseProcess();
                    }
                }
                else
                {
                    GameManager.Instance.bookView.SetActive(true);
                    GameManager.Instance.bookCover.SetActive(true);
                    GameManager.Instance.glossaryBookOpen = true;

                    StaticEventHandler.CallOpenBuildPageEvent();

                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);

                    // First trigger the animation (it uses UnscaledTime, so it's safe to call here)
                    GameManager.Instance.bookView.GetComponent<Animator>().SetTrigger(Settings.zoomIn);

                    if (!InputManager.TutorialEnabled) Time.timeScale = 0f;

                    // Finally, hide gameplay UI
                    GameManager.Instance.gameplayUI.FadeGameplayUI(GameManager.Instance.gameplayUI.canvasGroup, 0f, 0.1f); // Transparent
                }
            }
        }
    }

    private void BookCloseProcess()
    {
        // Play the close animation
        GameManager.Instance.bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);

        // Reset player states
        player.meleeAttackMainHand.IsAttacking = false;
        IsParrying = false;
        isPlayerRolling = false;
    }

    private void HandlePopUpState()
    {
        if (GameManager.Instance.popUpWindowOpen)
        {
            if (InputManager.Instance.OKButton.action.WasPressedThisFrame())
            {
                GameManager.Instance.CloseWarningPopUpMenu();
            }
        }
    }

    private void HandleSceneTransitionReady()
    {
        //if (!GameManager.Instance.IsAwaitingSceneTransition)
        //    return;

        //if (InputManager.Instance.AnyUIConfirmIntent())
        //{
        //    CmdNotifyReadyForSceneTransition();
        //}
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

            // Sync
            player.animSync?.UpdateLocalAnimationState(wasMoving, aimDirection, attackDirection);

            return;
        }

        // Read and normalize input
        movementInput = InputManager.Instance.movement.action.ReadValue<Vector2>().normalized;
        bool rollButtonDown = InputManager.Instance.jumpButton.action.WasPerformedThisFrame();

        // Update movement timer (for trail dust)
        movementTimer = movementInput.sqrMagnitude > 0.01f ? movementTimer + Time.deltaTime : 0f;

        // Sync
        bool movingNow = movementTimer > 0f;

        wasMoving = movingNow;

        // Store for physics force application
        player.movementByForce.MovementInput = movementInput;

        if (movementInput != Vector2.zero)
        {
            if (!rollButtonDown)
            {
                if (movingNow != wasMoving)
                {
                    player.animSync?.UpdateLocalAnimationState(movingNow, aimDirection, attackDirection);
                }

                player.animatePlayer.SetMovementAnimationParameters();
            }
            else if (playerRollCooldownTimer <= 0f && !InputManager.dodgeRollDisabled)
            {
                PlayerRoll(movementInput);
            }
        }
        else
        {
            if (!player.meleeAttackMainHand.IsAttacking)
            {
                player.animatePlayer.SetIdleAnimationParameters();

                player.animSync?.UpdateLocalAnimationState(wasMoving, aimDirection, attackDirection);

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

            // Sync
            player.animSync?.CmdEndParry(player.LastAim, player.LastAttackdir);

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

        RollDirection rollDir = HelperUtilities.GetRollDirection(direction);

        // Sync
        player.animSync?.CmdPlayRoll(rollDir);

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

        // Sync roll end
        player.animSync?.CmdEndRoll();

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

            Vector3 mouseWorldPos = HelperUtilities.GetMouseWorldPosition(player);

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

        // Sync
        if(playerAimDirection != player.LastAim || playerAttackDirection != player.LastAttackdir)
        {
            player.LastAim = playerAimDirection;
            player.LastAttackdir = playerAttackDirection;

            player.animSync?.UpdateLocalAnimationState(wasMoving, aimDirection, attackDirection);
        }

        // Apply results
        player.aimWeapon.Aim(playerAimDirection, playerAttackDirection, playerAngleDegrees, EnemyCategory.None);
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

        WeaponDetailsSO mainHandWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(mainHand.weaponStats.weaponTitle);
        WeaponDetailsSO offHandWeaponDetails = offHand != null ? WartheonDatabase.Instance.GetWeaponDetails(offHand.weaponStats.weaponTitle) : null;

        if (mainHandWeaponDetails != null)
        {
            // Fire when left mouse button is clicked - melee
            if (mainHand.weaponStats.isMeleeWeapon && !GameManager.Instance.isOverviewCameraEnabled)
            {
                if (InputManager.Instance.attack.action.WasPressedThisFrame() && !IsClickingSpecificUILayer() && !isParrying)
                {
                    // MAIN-HAND
                    if (mainHand.weaponStats.isMeleeWeapon == true && !player.meleeAttackMainHand.IsAttacking)
                    {
                        AttackShape mainHandAttackType = DetermineAttackType(mainHand.weaponStats);
                        player.meleeAttackEvent.CallAttackEvent(aimDirection, mainHand, mainHandAttackType, MeleeHand.MainHand);
                    }

                    // OFF-HAND
                    if (offHandWeaponDetails != null && offHand.weaponStats.isMeleeWeapon == true && !player.meleeAttackMainHand.IsAttacking)
                    {
                        AttackShape offHandAttackType = DetermineAttackType(offHand.weaponStats);
                        player.meleeAttackEvent.CallAttackEvent(aimDirection, offHand, offHandAttackType, MeleeHand.OffHand);
                    }
                }

                return;
            }

            // Fire for non-precharge weapons (fire once per press)
            if (mainHandWeaponDetails.weaponPrechargeTime == 0f && InputManager.Instance.attack.action.WasPressedThisFrame()
                && !IsClickingSpecificUILayer() && !GameManager.Instance.isOverviewCameraEnabled)
            {
                isSoundPlayed = false;

                if (mainHand.weaponStats.weaponClass == WeaponClass.Bow || mainHand.weaponStats.weaponClass == WeaponClass.Crossbow ||
                    mainHand.weaponStats.weaponClass == WeaponClass.Staff)
                {
                    if (!mainHand.weaponStats.onCooldown)
                    {
                        //player.meleeAttackMainHand.IsAttacking = true;

                        // Trigger fire weapon event
                        player.meleeAttackEvent.CallAttackEvent(playerAimDirection, mainHand, AttackShape.None, MeleeHand.None);
                    }
                }

                // Fire event (only once per press)
                player.fireWeaponEvent.CallFireWeaponEvent(true, false, playerAimDirection, playerAngleDegrees, weaponAngleDegrees, weaponDirection, mainHandWeaponDetails.weaponCurrentProjectile.isLaser,
                    ProjectileKind.Default, default, player.netId, targetNetId: 0, null);
            }
        }
    }

    public AttackShape DetermineAttackType(WeaponStats weaponStats)
    {
        if (weaponStats.hasSwing) return AttackShape.Swing;
        if (weaponStats.hasThrust) return AttackShape.Thrust;

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
            switch (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass)
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

                        // Sync
                        player.animSync?.CmdPlayParry(player.LastAttackdir);

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
        player.fireWeaponEvent.CallFireWeaponEvent(false, false, AimDirection.Right, 0f, 0f, Vector3.zero, false, ProjectileKind.Default, default, player.netId, targetNetId: 0, null);
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
            PreviousWeaponSet(onStart);
        }

        if (scrollValue > 0f || switchForward)
        {

            NextWeaponSet(true, onStart);
        }
    }

    public void NextWeaponSet(bool mouseWheel, bool onStart, int setNumber = 0)
    {
        if (setNumber > 0)
        {
            if (NetworkClient.active)
            {
                if (player.IsLocal)
                {
                    // Cache previous weapon slot index
                    player.playerInventory.SetOriginalSlotIndex(player.weaponState.currentWeaponSetIndex);
                }

                // Set previous index
                player.previousSetIndex = player.weaponState.currentWeaponSetIndex;

                // Increment the current weapon slot set index
                player.currentWeaponSlotSetIndex = setNumber;

                player.weaponState.CmdChangeWeaponSet(setNumber, onStart);

                //player.playerInventoryNetwork.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
            }
            else
            {
                // Cache previous weapon slot index
                player.playerInventory.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

                // Set previous index
                player.previousSetIndex = player.currentWeaponSlotSetIndex;

                // Increment the current weapon slot set index
                player.currentWeaponSlotSetIndex = setNumber;

                SetWeaponSetByIndex(onStart, player.currentWeaponSlotSetIndex);

                //player.playerInventory.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
            }
        }
        else if (mouseWheel)
        {
            if (NetworkClient.active)
            {
                if (player.IsLocal)
                {
                    // Cache previous weapon slot index
                    player.playerInventory.SetOriginalSlotIndex(player.weaponState.currentWeaponSetIndex);
                }

                // Set previous index
                player.previousSetIndex = player.weaponState.currentWeaponSetIndex;

                int index = player.weaponState.currentWeaponSetIndex;

                // Increment the current weapon slot set index
                index++;

                if (index > 3) index = 1;

                player.currentWeaponSlotSetIndex = index;

                player.weaponState.CmdChangeWeaponSet(index, onStart);

                //player.playerInventoryNetwork.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
            }
            else
            {
                // Cache previous weapon slot index
                player.playerInventory.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

                // Set previous index
                player.previousSetIndex = player.currentWeaponSlotSetIndex;

                // Increment the current weapon slot set index
                player.currentWeaponSlotSetIndex++;

                if (player.currentWeaponSlotSetIndex > 3)
                {
                    player.currentWeaponSlotSetIndex = 1;
                }

                SetWeaponSetByIndex(onStart, player.currentWeaponSlotSetIndex);

                //player.playerInventory.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
            }


            HighlightWeaponSetButton(); //Light and color settings
        }
        else
        {
            if (player.currentWeaponSlotSetIndex == setNumber) return;

            if (NetworkClient.active)
            {
                if (player.IsLocal)
                {
                    // Cache previous weapon slot index
                    player.playerInventory.SetOriginalSlotIndex(player.weaponState.currentWeaponSetIndex);
                }

                player.currentWeaponSlotSetIndex = setNumber;

                player.weaponState.CmdChangeWeaponSet(setNumber, onStart);

                //player.playerInventoryNetwork.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
            }
            else
            {
                // Cache previous weapon slot index
                player.playerInventory.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

                player.currentWeaponSlotSetIndex = setNumber;
                SetWeaponSetByIndex(onStart, player.currentWeaponSlotSetIndex);

                //player.playerInventory.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
            }

            HighlightWeaponSetButton(); //Light and color settings
        }
    }

    public void PreviousWeaponSet(bool onStart)
    {
        if (NetworkClient.active)
        {
            if (player.IsLocal)
            {
                // Cache previous weapon slot index
                player.playerInventory.SetOriginalSlotIndex(player.weaponState.currentWeaponSetIndex);
            }

            // Set previous index
            player.previousSetIndex = player.weaponState.currentWeaponSetIndex;

            int index = player.weaponState.currentWeaponSetIndex;

            // Decrease the current weapon slot set index
            index--;

            if (index < 1) index = 3;

            player.currentWeaponSlotSetIndex = index;

            player.weaponState.CmdChangeWeaponSet(index, onStart);

            //player.playerInventoryNetwork.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
        }
        else
        {
            // Cache previous weapon slot index
            player.playerInventory.SetOriginalSlotIndex(player.currentWeaponSlotSetIndex);

            player.previousSetIndex = player.currentWeaponSlotSetIndex;

            // Decrease the current weapon slot set index
            player.currentWeaponSlotSetIndex--;

            if (player.currentWeaponSlotSetIndex < 1)
            {
                player.currentWeaponSlotSetIndex = 3;
            }

            SetWeaponSetByIndex(onStart, player.currentWeaponSlotSetIndex);

            //player.playerInventory.CurrentWeaponSlotSetIndex = player.currentWeaponSlotSetIndex;
        }

        HighlightWeaponSetButton();
    }

    public void SetWeaponSetByIndex(bool onStart, int playerWeaponIndex, bool dragFromInventory = false, bool dragToInventory = false, bool inventorySwitch = false, bool isMultiplayer = false)
    {
        // Defensive validation
        if (player == null) player = GetComponent<Player>();
        if (player == null) return;

        // Clamp index and ensure arrays exist
        playerWeaponIndex = Mathf.Clamp(playerWeaponIndex, 1, player.weaponSlotSetArray?.Length ?? 1);

        bool isWeaponSwapping = dragFromInventory || dragToInventory || inventorySwitch;

        // Treat single-player as "owner" for HUD/book updates
        bool isOwnerContext = (!NetworkServer.active && !NetworkClient.active) || player.IsLocal;

        // Get references for the set (may be null)
        Weapon mainHandWeaponInSet = null;
        Weapon offHandWeaponInSet = null;

        if (player.weaponSlotSetArray != null && player.weaponSlotSetArray.Length >= playerWeaponIndex && player.weaponSlotSetArray[playerWeaponIndex - 1] != null)
        {
            mainHandWeaponInSet = player.weaponSlotSetArray[playerWeaponIndex - 1][0];
            offHandWeaponInSet = player.weaponSlotSetArray[playerWeaponIndex - 1][1];
        }

        // Use Player helper to fire unified events (gameplay + optional HUD owner events)
        player.ApplyWeaponActivationEvents(mainHandWeaponInSet, ItemSlotStatus.MainHand, playerWeaponIndex, onStart, isOwnerContext, allowHudEvents: true, allowLockIconUpdate: true, isWeaponSwapping);
        player.ApplyWeaponActivationEvents(offHandWeaponInSet, ItemSlotStatus.OffHand, playerWeaponIndex, onStart, isOwnerContext, allowHudEvents: true, allowLockIconUpdate: true, isWeaponSwapping);

        // Update stats after weapon switch (owner recalculates and book UI updated locally)
        player.RecalculateSecondaryStats();

        // Book UI SWITCH: only affect the owner (local or single-player)
        if (!dragFromInventory && !dragToInventory && !inventorySwitch && isOwnerContext)
        {
            StaticEventHandler.CallWeaponSwitchedEventForBook();
        }
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
    /// Paralyze routine
    /// </summary>
    IEnumerator ParalyzeRoutine()
    {
        player.movementByForce.moveSpeed = 0f;
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeAll;
        player.animator.SetBool(Settings.isStunned, true);

        yield return new WaitForSeconds(player.paralyzeDuration);

        player.moveStatus &= ~MoveStatus.Paralyze; // Remove paralyze
        player.rb2D.constraints = RigidbodyConstraints2D.FreezeRotation;
        player.healthEvent.CallParalyzeCuredEvent();
        player.animator.SetBool(Settings.isStunned, false);
        player.UpdateSpeedValue();
        stunCoroutine = null;
    }

    private void InnerPathPassivesCheck()
    {
        // Vicious Momentum
        player.playerSkillController.ViciousMomentumCheck();
        // Combat Focus
        player.playerSkillController.CombatFocusCheck();
        // Triad Execution
        player.playerSkillController.TriadExecutionCheck();
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
    }

    private void PlayerSpecificPassivesCheck()
    {
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
                player.activeWeapon.GetCurrentOffHandWeapon().weaponStats.weaponClass != WeaponClass.Shield)
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

                Weapon mainHand = player.activeWeapon.GetCurrentMainHandWeapon();
                Weapon offHand = player.activeWeapon.GetCurrentOffHandWeapon();

                switch (player.playerDetails.playerCharacterIndex)
                {
                    case Character.Caelion:
                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.SeismicSlam:
                                if (mainHand != null)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    player.playerSkillController.SeismicSlamProcess();
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.SeismicSlam, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.Valor:
                                if (!player.isValorActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    player.playerSkillController.Valor(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Valor, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.ShieldBash:
                                // OFF-HAND
                                if (offHand?.weaponStats.weaponClass == WeaponClass.Shield)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    // Shield bash attack
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    player.meleeAttackEvent.CallAttackEvent(aimDirection, offHand, AttackShape.Cone, MeleeHand.OffHand, false, true);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ShieldBash, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.BreakTheLine:
                                if (!player.isBreakTheLineActive && offHand?.weaponStats.weaponClass == WeaponClass.Shield)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    player.playerSkillController.BreakTheLine(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BreakTheLine, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.GuardedOath:
                                if (player.isGuardedOathActive)
                                {
                                    player.playerSkillController.RemoveGuardedOathEffects(inputSlotNumber, ref activeSkillData);
                                }
                                else if (!player.isGuardedOathActive && offHand?.weaponStats.weaponClass == WeaponClass.Shield)
                                {
                                    if (player.mana.GetCurrentMana() >= reservedMana)
                                    {
                                        player.mana.ConsumeMana(reservedMana, true); // Mana reserved
                                        player.playerSkillController.GuardedOath(inputSlotNumber);
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

                                    player.playerSkillController.UmbralMist(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.UmbralMist, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.Stealth:
                                if (!player.isStealthActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    player.playerSkillController.Stealth(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Stealth, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.BloodDrain:
                                // DAGGER CHECK
                                if (offHand != null && mainHand != null && !isParrying)
                                {
                                    if (offHand.weaponStats.weaponClass == WeaponClass.Dagger &&
                                        mainHand.weaponStats.weaponClass == WeaponClass.Dagger && !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Blood drain attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.BloodDrain();
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BloodDrain, inputSlotNumber);
                                    }
                                }

                                break;
                            case ActiveSkill.ShadowStep:
                                if (!player.isShadowStepActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    player.playerSkillController.ShadowStep(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ShadowStep, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.CullTheMeek:
                                // DAGGER CHECK
                                if (offHand != null && mainHand != null && !isParrying)
                                {
                                    if (offHand.weaponStats.weaponClass == WeaponClass.Dagger &&
                                        mainHand.weaponStats.weaponClass == WeaponClass.Dagger && !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Cull the meek attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.CullTheMeek();
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
                                if (mainHand.weaponStats.weaponClass == WeaponClass.Bow ||
                                    mainHand.weaponStats.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isPenetrateActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Penetrate attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.Penetrate(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Penetrate, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.TripleThreat:
                                if (mainHand.weaponStats.weaponClass == WeaponClass.Bow || mainHand.weaponStats.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isTripleThreatActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Triple threat attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.TripleThreat(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.TripleThreat, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.BindingArrow:
                                if (mainHand.weaponStats.weaponClass == WeaponClass.Bow || mainHand.weaponStats.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isBindingArrowActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Triple threat attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.BindingArrow(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.BindingArrow, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.ArrowsOfTheSevenPlagues:
                                if (mainHand.weaponStats.weaponClass == WeaponClass.Bow || mainHand.weaponStats.weaponClass == WeaponClass.Crossbow)
                                {
                                    if (!player.isArrowOfTheSevenActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Arrow of The Seven Plagues attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.ArrowOfTheSevenPlagues(inputSlotNumber, ref activeSkillData);
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
                                    player.playerSkillController.HuntersReach(inputSlotNumber);
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
                                    player.playerSkillController.Rage(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Rage, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.Shattercry:
                                if (!player.isShatterCryActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);
                                    player.playerSkillController.ShatterCry(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Shattercry, inputSlotNumber);
                                }

                                break;
                            case ActiveSkill.Whirlrend:
                                player.mana.ConsumeMana(consumedMana);

                                player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                player.playerSkillController.Whirlrend(inputSlotNumber, ref activeSkillData);
                                player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Whirlrend, inputSlotNumber);
                                break;
                            case ActiveSkill.AxeThrow:
                                // AXE CHECK
                                if (player.activeWeapon.GetCurrentOffHandWeapon() != null && player.activeWeapon.GetCurrentMainHandWeapon() != null 
                                    && !player.isAxeThrowActive)
                                {
                                    if (player.activeWeapon.GetCurrentOffHandWeapon().weaponStats.weaponClass == WeaponClass.Axe &&
                                        player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Axe && 
                                        !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Axe throw attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.AxeThrow(inputSlotNumber, ref activeSkillData);

                                    }
                                }
                                break;
                            case ActiveSkill.FeastOfWar:
                                if (!player.isFeastOfWarActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    player.playerSkillController.FeastOfWar(inputSlotNumber, ref activeSkillData);
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

                        bool mainHanddaggerOrClaw = mainHandWeapon != null && (mainHandWeapon.weaponStats.weaponClass == WeaponClass.Dagger ||
                            mainHandWeapon.weaponStats.weaponClass == WeaponClass.Claw);

                        switch (player.currentlyUsedActiveUniqueSkills[inputSlotNumber].activeSkill)
                        {
                            case ActiveSkill.DontBlink:
                                // DAGGER OR CLAW CHECK
                                if (mainHanddaggerOrClaw && !player.isDontBlinkActive)
                                {
                                    // Don't blink attack
                                    player.playerSkillController.DontBlink(inputSlotNumber, ref activeSkillData);

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
                                    player.playerSkillController.VenomousIvy(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.VenomousIvy, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.FadeAndFeed:
                                if (!player.isVenomousIvyActive)
                                {
                                    player.mana.ConsumeMana(consumedMana);

                                    // Fade and Feed
                                    player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                    player.playerSkillController.FadeAndFeed(inputSlotNumber, ref activeSkillData);
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
                                            player.playerSkillController.BladeDash(inputSlotNumber, ref activeSkillData, isInitialCast); // Dash and damage
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
                                        player.playerSkillController.Shiruken(inputSlotNumber);
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
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff) 
                                {
                                    if (!player.isBlizzardActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Blizzard attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.Blizzard(inputSlotNumber, ref activeSkillData);
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
                                    player.playerSkillController.MycarasSeal(inputSlotNumber, ref activeSkillData); ;
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.MycarasSeal, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.SheerCold:
                                // STAFF CHECK
                                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                                {
                                    if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff &&
                                        !player.meleeAttackMainHand.IsAttacking)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Sheer cold attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.SheerCold(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.SheerCold, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.Icebreaker:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff &&
                                    !player.meleeAttackMainHand.IsAttacking)
                                {
                                    if (!player.isIceBreakerActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Ice breaker attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.IceBreaker(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.Icebreaker, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.AbsoluteZero:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isAbsoluteZeroActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Absolute Zero attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.AbsoluteZero(inputSlotNumber, ref activeSkillData); ;
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

                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isFireBlastActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Fire blast attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.FireBlast(inputSlotNumber);
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
                                    player.playerSkillController.MoltenRift(inputSlotNumber);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.MoltenRift, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.FlameLotus:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isFlameLotusActive)
                                    {
                                        player.mana.ConsumeMana(activeUniqueSkillManaCost);

                                        // Flame lotus attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.FlameLotus(inputSlotNumber, ref activeSkillData); ;
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
                                    player.playerSkillController.KynarasEmbrace(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.KynarasEmbrace, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.BlazingCyclone:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isBlazingCycloneActive)
                                    {
                                        player.mana.ConsumeMana(consumedMana);

                                        // Blazing Cyclone attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.BlazingCyclone(inputSlotNumber);
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
                                    player.playerSkillController.MistOfDisruption(inputSlotNumber, ref activeSkillData);
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
                                    player.playerSkillController.NymarasWindveil(inputSlotNumber, ref activeSkillData);
                                    player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.NymarasWindveil, inputSlotNumber);
                                }
                                break;
                            case ActiveSkill.ChainLightning:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isChainLightningActive)
                                    {
                                        player.isConductiveTouchActive = true; // Conductive Touch Flag
                                        player.nymaraSkillUsageTimer = 0f;
                                        player.healthEvent.CallConductiveTouchSpecialMoveEvent();

                                        player.mana.ConsumeMana(consumedMana);

                                        // Chain Lightning attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.ChainLightning(inputSlotNumber);
                                        player.specialMoveEvent.CallSpecialMoveUsedEvent(ActiveSkill.ChainLightning, inputSlotNumber);
                                    }
                                }
                                break;
                            case ActiveSkill.EyeOfTheStorm:
                                if (player.activeWeapon.GetCurrentMainHandWeapon().weaponStats.weaponClass == WeaponClass.Staff)
                                {
                                    if (!player.isEyeOfTheStormActive)
                                    {
                                        player.isConductiveTouchActive = true; // Conductive Touch Flag
                                        player.nymaraSkillUsageTimer = 0f;
                                        player.healthEvent.CallConductiveTouchSpecialMoveEvent();

                                        player.mana.ConsumeMana(consumedMana);

                                        // Eye of the storm attack
                                        player.specialMovesCooldownCheckArray[inputSlotNumber - 1] = true;
                                        player.playerSkillController.EyeOfTheStorm(inputSlotNumber, ref activeSkillData);
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
                                    player.playerSkillController.IonicRejuvenation(inputSlotNumber, ref activeSkillData);
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
                    if (!NetworkServer.active && !NetworkClient.active)
                    {
                        if (chest.chestState == ChestState.closed)
                        {
                            iusable.StartChestProcess();
                        }
                    }
                    else
                    {
                        if (chest.chestNetwork.chestState == ChestState.closed)
                        {
                            ChestNetwork chestNetwork = chest.GetComponent<ChestNetwork>();
                            player.NetAuth.CmdRequestOpenChest(chestNetwork.netIdentity);
                        }
                    }
                }
            }

            if (InputManager.Instance.interaction.action.WasPerformedThisFrame())
            {
                if (collider2D.GetComponent<Environment>() != null) return;

                // Only interactable objects have capsule colliders. So if it's nut null, it means collider is an interactable (like NPC)
                if (collider2D.GetComponent<CapsuleCollider2D>() != null && collider2D.tag != Settings.playerTag && collider2D.tag != Settings.enemyTag)
                {
                    DialogueManager dialogueManager = collider2D.GetComponent<DialogueManager>();
                    NPC npc = dialogueManager?.GetComponent<NPC>();

                    if (npc != null && npc.npcType == NpcType.Gambler) return;

                    dialogueManager.TriggerDialogue();
                }
            }
        }
    }

    public void DropProcess(ItemType dropType, WeaponStats weaponStats, PassiveItemStats passiveItemStats, Rarity rarity, bool isServer,
        ItemSlotStatus itemSlotStatus, int inventoryIndex = -1, int setIndex = -1, bool dropButton = false)
    {
        bool isMultiplayer = NetworkServer.active || NetworkClient.active;

        // Single Player
        if (!isMultiplayer)
        {
            DropProcess_SP(dropType, weaponStats, passiveItemStats, rarity, itemSlotStatus, inventoryIndex, setIndex, dropButton);
            return;
        }
        if (isServer)
        {
            DropProcess_Server(dropType, weaponStats, passiveItemStats, rarity, itemSlotStatus, inventoryIndex, setIndex, dropButton);
            return;
        }
           
        // CLIENT
        DropProcess_Client(dropType, weaponStats, passiveItemStats, rarity, itemSlotStatus, inventoryIndex, setIndex, dropButton);
    }

    private void DropProcess_Server(ItemType dropType, WeaponStats weaponStats, PassiveItemStats passiveItemStats, Rarity rarity,
        ItemSlotStatus itemSlotStatus = ItemSlotStatus.None, int inventoryIndex = -1, int setIndex = -1, bool dropButton = false)
    {
        bool isInventoryFull = player.playerInventory.IsInventoryFull();
        Vector3 pos = player.transform.position;

        if (dropType == ItemType.PassiveItem)
        {
            if (itemSlotStatus == ItemSlotStatus.Inventory)
            {
                player.playerInventoryNetwork.SpawnDropWeapon_Server(weaponStats, weaponStats.weaponTitle);
            }
            else if (true)
            {

            }

        }
        else
        {
            player.playerInventoryNetwork.SpawnDropPassiveItem_Server(passiveItemStats);
        }
    }

    private void DropProcess_Client(ItemType dropType, WeaponStats weaponStats, PassiveItemStats passiveItemStats, Rarity rarity,
        ItemSlotStatus fromStatus, int fromIndex, int setIndex, bool dropButton)
    {
        if (!dropButton) return;
        if (!player.IsLocal) return;

        if (dropType == ItemType.Weapon) player.playerInventoryNetwork.CmdDropWeapon(weaponStats, weaponStats.weaponTitle, fromStatus, fromIndex, setIndex);
        else if (dropType == ItemType.PassiveItem) player.playerInventoryNetwork.CmdDropPassive(passiveItemStats, passiveItemStats.passiveItemType, fromStatus, fromIndex, passiveItemStats.passiveItemSlotName);
    }

    private void DropProcess_SP(ItemType dropType, WeaponStats weaponStats, PassiveItemStats passiveItemStats, Rarity rarity, 
        ItemSlotStatus itemSlotStatus = ItemSlotStatus.None, int inventoryIndex = -1, int setIndex = -1, bool dropButton = false)
    {
        bool isInventoryFull = player.playerInventory.IsInventoryFull();

        if (dropType == ItemType.PassiveItem)
        {
            PassiveItem passiveItem = PassiveDropGenerator.GetPassiveWithStats(passiveItemStats, rarity, itemSlotStatus, -1);
            PassiveItemDetailsSO passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);

            if (itemSlotStatus == ItemSlotStatus.Inventory)
            {
                // Just drop from inventory to the floor
                SpawnDroppedPassiveItemSP(passiveItem);
                EmptyItemFromInventorySP(inventoryIndex);
            }
            else if (dropButton)
            {
                // Manual drop by clicking drop button
                SpawnDroppedPassiveItemSP(passiveItem);
                DeactivatePassiveItem(passiveItem.passiveStats, passiveItem.Rarity, passiveItem.ItemSlotStatus, isSwap: false, dropButton: true);
            }
            else if (isInventoryFull)
            {
                // Forced drop due to full inventory during item pickup
                SpawnDroppedPassiveItemSP(passiveItem);
                DeactivatePassiveItem(passiveItem.passiveStats, passiveItem.Rarity, passiveItem.ItemSlotStatus, isSwap: false, dropButton: false);
            }
            else
            {
                // Regular swap -> previous goes to inventory
                DeactivatePassiveItem(passiveItem.passiveStats, passiveItem.Rarity, passiveItem.ItemSlotStatus, isSwap: false, dropButton: false);
                PlaceItemIntoInventorySP(default, passiveItem.passiveStats, passiveItem.Rarity, passiveItem.ItemSlotStatus);
            }
            return;
        }

        // === WEAPON ===
        Weapon weapon = WeaponDropGenerator.GetWeaponWithStats(weaponStats, rarity, itemSlotStatus, -1);
        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

        // Inventory-drop case (no active slot involvement)
        if (itemSlotStatus == ItemSlotStatus.Inventory)
        {
            SpawnDroppedWeaponSP(weapon);
            EmptyItemFromInventorySP(inventoryIndex);
            return;
        }

        // MAIN-HAND
        if (weapon.ItemSlotStatus == ItemSlotStatus.MainHand)
        {
            if (dropButton)
            {
                if (!SlotPlacementRules.IsPlacementAllowed(weapon, SlotType.Drop,
                        player.activeWeapon.GetCurrentMainHandWeapon(), player.currentWeaponSlotSetIndex))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                SpawnDroppedWeaponSP(weapon);
                DeactivateMainHandWeapon(player.currentWeaponSlotSetIndex, ItemSlotStatus.MainHand);
                ValueAndUIUpdate(player.currentWeaponSlotSetIndex, ItemSlotStatus.MainHand);
            }
            else if (isInventoryFull)
            {
                //// Forced floor drop during pickup
                //if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                //        player.activeWeapon.GetCurrentMainHandWeapon(),
                //        player.activeWeapon.GetCurrentOffHandWeapon(), false))
                //{
                //    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                //    return;
                //}

                SpawnDroppedWeaponSP(weapon);
                DeactivateMainHandWeapon(player.currentWeaponSlotSetIndex, ItemSlotStatus.MainHand);
                ValueAndUIUpdate(player.currentWeaponSlotSetIndex, ItemSlotStatus.MainHand);
            }
            else
            {
                //// Move current to inventory during pickup
                //if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                //        player.activeWeapon.GetCurrentMainHandWeapon(),
                //        player.activeWeapon.GetCurrentOffHandWeapon(), false))
                //{
                //    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                //    return;
                //}

                DeactivateMainHandWeapon(player.currentWeaponSlotSetIndex, ItemSlotStatus.MainHand);
                PlaceItemIntoInventorySP(weaponStats, passiveItemStats, rarity, itemSlotStatus);
            }

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

                SpawnDroppedOffhandWeaponSP(weapon);
                DeactivateOffhandWeapon(player.currentWeaponSlotSetIndex, ItemSlotStatus.OffHand);
                ValueAndUIUpdate(player.currentWeaponSlotSetIndex, ItemSlotStatus.OffHand);
            }
            else if (isInventoryFull)
            {
                //if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                //        player.activeWeapon.GetCurrentMainHandWeapon(),
                //        player.activeWeapon.GetCurrentOffHandWeapon(), true))
                //{
                //    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                //    return;
                //}

                // Forced floor drop during pickup
                SpawnDroppedOffhandWeaponSP(weapon);
                DeactivateOffhandWeapon(player.currentWeaponSlotSetIndex, ItemSlotStatus.OffHand);
                ValueAndUIUpdate(player.currentWeaponSlotSetIndex, ItemSlotStatus.OffHand);
            }
            else
            {
                //// Move current to inventory during pickup
                //if (!SlotPlacementRules.IsSwapAllowed(toBeSwappedWeapon, weapon,
                //        player.activeWeapon.GetCurrentMainHandWeapon(),
                //        player.activeWeapon.GetCurrentOffHandWeapon(), true))
                //{
                //    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                //    return;
                //}

                DeactivateOffhandWeapon(player.currentWeaponSlotSetIndex, ItemSlotStatus.OffHand);
                PlaceItemIntoInventorySP(weapon.weaponStats, default, weapon.Rarity, weapon.ItemSlotStatus);
                ValueAndUIUpdate(player.currentWeaponSlotSetIndex, ItemSlotStatus.OffHand);
            }

            if (weapon.weaponStats.weaponClass == WeaponClass.Shield)
            {
                player.isShieldCalculated = false;
            }
        }
    }

    public void DeactivateMainHandWeapon(int currentWeaponSlotSetIndex, ItemSlotStatus slotStatus)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            player.weaponSlotSetArray[currentWeaponSlotSetIndex - 1][0] = null;

            player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEvent(true);
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEventForHud();
        }
        else if (NetworkClient.active)
        {
            player.NetAuth.CmdDeactivateWeaponAfterDrop(player.currentWeaponSlotSetIndex, slotStatus);
        }
    }

    public void DeactivateOffhandWeapon(int currentWeaponSlotSetIndex, ItemSlotStatus slotStatus)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            // Set dropped set's off hand null
            player.weaponSlotSetArray[currentWeaponSlotSetIndex - 1][1] = null;

            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEvent(isStatUpdateAllowed: true);
            player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEventForHud();
        }
        else if (NetworkClient.active)
        {
            player.NetAuth.CmdDeactivateWeaponAfterDrop(currentWeaponSlotSetIndex, slotStatus);
        }
    }

    public void DeactivateMainHandWeapon_Client(int index)
    {
        player.setActiveWeaponEvent.CallSetInactiveWeaponAtMainHandEventForHud();
    }

    public void DeactivateOffhandWapon_Client(int index)
    {
        player.setActiveWeaponEvent.CallSetInactiveWeaponAtOffHandEventForHud();
    }

    public void DeactivatePassiveItem(PassiveItemStats passiveStats, Rarity rarity, ItemSlotStatus slotStatus, bool isSwap, bool dropButton)
    {
        PassiveItem passiveItem = null;
        PassiveItemDetailsSO passiveItemDetails = null;

        if (passiveStats.passiveItemType != PassiveItemType.None)
        {
            passiveItem = PassiveDropGenerator.GetPassiveWithStats(passiveStats, rarity, slotStatus, -1);
            passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);
        }

        if (!NetworkServer.active && !NetworkClient.active)
        {
            player.setPassiveItemEvent.CallRemovePassiveItem(player, passiveItem, passiveStats.passiveItemSlotName, isSwap, dropButton);
        }
        else if(NetworkClient.active)
        {
            player.NetAuth.CmdDeactivatePassiveItemAfterDrop(passiveStats, rarity, slotStatus, isSwap, dropButton);
        }
    }

    public void EmptyItemFromInventorySP(int inventoryIndex)
    {
        player.playerInventory.EmptyItemFromInventory(inventoryIndex);
        StaticEventHandler.CallInventoryWeaponDroppedEventForBook(inventoryIndex);
    }

    public void PlaceItemIntoInventorySP(WeaponStats weaponStats, PassiveItemStats passiveStats, Rarity rarity, ItemSlotStatus slotStatus)
    {
        Weapon weapon = null;
        PassiveItem passiveItem = null;
        WeaponDetailsSO weaponDetails = null;
        PassiveItemDetailsSO passiveItemDetails = null;

        if (weaponStats.weaponTitle != WeaponTitle.None)
        {
            weapon = WeaponDropGenerator.GetWeaponWithStats(weaponStats, rarity, slotStatus, -1);
            weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);
        }
        else if (passiveStats.passiveItemType != PassiveItemType.None)
        {
            passiveItem = PassiveDropGenerator.GetPassiveWithStats(passiveStats, rarity, slotStatus, -1);
            passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);
        }

        if (weapon != null)
        {
            int index = player.playerInventory.PlaceItemToInventoryIndexSlot(weapon);
            weapon.ItemSlotStatus = ItemSlotStatus.Inventory;
            StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(weapon, index);
        }
        else if (passiveItem != null)
        {
            int index = player.playerInventory.PlaceItemToInventoryIndexSlot(passiveItem);
            passiveItem.ItemSlotStatus = ItemSlotStatus.Inventory;
            StaticEventHandler.CallPassiveItemAddedToInventorySlot(passiveItem, index);
        }
    }

    public void ValueAndUIUpdate(int currentWeaponSlotSetIndex, ItemSlotStatus slotStatus)
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            player.UpdateDamageValues();
            player.UpdateArmorValues();
            player.UpdateAttackRatingAndCriticalValues();
            player.UpdateBlockAndDodgeValues();

            if(slotStatus == ItemSlotStatus.MainHand)
            {
                StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponMainHand);
            }
            else if (slotStatus == ItemSlotStatus.OffHand)
            {
                StaticEventHandler.CallWeaponDroppedEventForBook(SlotType.WeaponOffHand);
            }
        }
        else if (NetworkClient.active)
        {
            player.UpdateDamageValues();
            player.UpdateArmorValues();
            player.UpdateAttackRatingAndCriticalValues();
            player.UpdateBlockAndDodgeValues();

            player.NetAuth.CmdValueAndBookUpdate(currentWeaponSlotSetIndex, slotStatus);
        }
    }

    private void SpawnDroppedWeaponSP(Weapon weapon)
    {
        {
            GameObject dropItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
            DropItem dropItem = dropItemObject.GetComponent<DropItem>();

            dropItem.hasWeaponDrop = true;
            dropItem.dropSourceType = DropSourceType.Player;

            dropItem.isColliding = true;

            WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

            dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position, null);

            // Break free from the player object
            dropItem.spriteRenderer.enabled = true;
            dropItem.animator.enabled = true;
            dropItem.animator.runtimeAnimatorController = weaponDetails.weaponHoverAnimatorController;

            dropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);
            dropItem.isPickedUp = false;
            dropItem.dropCompleted = true;

            // Make sure drop completed
            dropItem.boxCollider2D.enabled = true;
            dropItem.isColliding = false;
        }
    }

    private void SpawnDroppedOffhandWeaponSP(Weapon weapon)
    {
        GameObject chestItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        DropItem dropItem = chestItemObject.GetComponent<DropItem>();

        dropItem.hasWeaponDrop = true;
        dropItem.dropSourceType = DropSourceType.Player;

        dropItem.isColliding = true;

        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

        dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position, null);

        // Break free from the player object
        dropItem.spriteRenderer.enabled = true;
        dropItem.animator.enabled = true;
        dropItem.animator.runtimeAnimatorController = weaponDetails.weaponHoverAnimatorController;

        dropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);
        dropItem.isPickedUp = false;
        dropItem.dropCompleted = true;

        // Make sure drop completed
        dropItem.boxCollider2D.enabled = true;
        dropItem.isColliding = false;
    }

    private void SpawnDroppedPassiveItemSP(PassiveItem passiveItem)
    {
        GameObject dropItemObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        DropItem dropItem = dropItemObject.GetComponent<DropItem>();

        dropItem.dropSourceType = DropSourceType.Player;
        dropItem.hasSecondaryPassiveDrop = true;
        dropItem.isColliding = true;

        PassiveItemDetailsSO passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);

        dropItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, transform.position, null);
        dropItem.spriteRenderer.enabled = true;
        dropItem.animator.enabled = true;
        dropItem.animator.runtimeAnimatorController = passiveItemDetails.passiveItemAnimatorController;
        dropItem.transform.SetParent(GameManager.Instance.GetCurrentRoom().instantiatedRoom.transform);

        dropItem.isPickedUp = false;
        dropItem.boxCollider2D.enabled = true;
        dropItem.isColliding = false;
        dropItem.dropCompleted = true;
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
}