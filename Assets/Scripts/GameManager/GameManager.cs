using Mirror;
using System.Collections;
using System;
using System.Collections.Generic;
using System.Globalization;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    public static bool isDemo = false;

    public bool HasLocalPlayer => localPlayer != null;
    Player localPlayer;

    bool gameplayInitialized;
    bool subscribed;

    #region Header GAMEOBJECT REFERENCES
    [Space(10)]
    [Header("GAMEOBJECT REFERENCES")]
    #endregion Header GAMEOBJECT REFERENCES

    InputActionAsset actions;

    [Space(10)]
    [Header("PAUSE MENU REFERENCES")]
    #region Tooltip
    [Tooltip("Populate with pause menu gameobject in the hierarchy")]
    #endregion
    public GameObject pauseMenu;
    [SerializeField] GameObject pauseContainer;
    [SerializeField] GameObject settingsContainer;
    [SerializeField] GameObject controlsContainer;
    [SerializeField] GameObject keyboardRebindingsContainer;
    [SerializeField] GameObject gamepadRebindingsContainer;

    [Space(10)]
    [SerializeField] Button resumeButton;
    [SerializeField] Button controlsButton;
    [SerializeField] Button settingsButton;
    [SerializeField] Button quitGameButton;
    [SerializeField] Button exitButton;

    [SerializeField] SoundEffectSO buttonClickSound;

    [Header("Video")]
    [SerializeField] TMP_Dropdown resolutionDropdown;
    [SerializeField] TMP_Dropdown screenModeDropDown;
    [SerializeField] TMP_Dropdown refreshRateDropdown;
    [SerializeField] Toggle postProcessingToggle;
    [SerializeField] Image postProcessingCheckmarkImage;
    [SerializeField] Toggle vsyncToggle;
    [SerializeField] Image vysncCheckmarkImage;

    [Header("Audio")]
    [SerializeField] Slider musicVolumeSlider;
    [SerializeField] Slider soundVolumeSlider;

    [Header("Game")]
    [SerializeField] Toggle dynamicCameraToggle;
    [SerializeField] Image dynamicCameraCheckmarkImage;

    [Space(10)]
    [Header("Fade Canvas")]
    #region Tooltip
    [Tooltip("Populate with the FadeImage canvasgroup component in the FadeScreenUI")]
    #endregion Tooltip
    [SerializeField] CanvasGroup canvasGroup;
    #region Tooltip
    [Tooltip("Populate with the MessageText textmeshpro component in the FadeScreenUI")]
    #endregion
    [SerializeField] TextMeshProUGUI messageTextTMP;
    #region Tooltip
    [Tooltip("Populate with the fade image component in the FadeScreenUI")]
    #endregion
    [SerializeField] Image fadeImage;

    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the Post processing volume")]
    #endregion
    [SerializeField] Volume volume;

    // Light member
    #region Tooltip
    [Tooltip("Populate with Light2D component")]
    #endregion
    public Light2D light2D;

    // Book members
    public GameObject bookView;
    public GameObject bookCover;
    [HideInInspector] public bool isBookClosing;

    // Level up panel
    public GameObject levelUpPanel;
    float levelUpPanelTimer;
    float levelUpPanelPopupDuration = 3f;

    // Gameplay UI
    public GameplayUI gameplayUI;
    public GameObject buttonBuildButton;

    // Pop-ups
    public GameObject warningPopUp;

    [Space(10)]
    [Header("SPECIAL UI LAYER REFERENCES")]
    // Special UI layer
    public GraphicRaycaster uiRaycaster;
    public EventSystem eventSystem;
    public LayerMask specialUILayerMask;

    [Space(10)]
    [SerializeField]GameObject introductionPopUp;
    [SerializeField] TextMeshProUGUI weaponText;
    [SerializeField] TextMeshProUGUI introductionText;
    [SerializeField] Image introductionItemImage;

    [HideInInspector] public bool glossaryBookOpen;
    [HideInInspector] public bool turnPageCompleted;
    [HideInInspector] public bool bookZoomOutFinished;
    [HideInInspector] public bool bookZoomInFinished;
    [HideInInspector] public bool levelUpZoomOutFinished;
    [HideInInspector] public bool levelUpZoomInFinished;
    [HideInInspector] public bool statsPageChanged;

    [HideInInspector] public bool popUpWindowOpen;

    // Overview camera
    [HideInInspector] public bool isOverviewCameraClicked;
    [HideInInspector] public bool isOverviewCameraEnabled;


    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS
    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion Tooltip
    public List<DungeonLevelSO> dungeonLevelList;
    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for testing , first level = 0")]
    #endregion Tooltip
    public int currentDungeonLevelListIndex = 0;
    #region Tooltip
    [Tooltip("Populate with the health bar")]
    #endregion Tooltip
    public GameObject healthBarContainer;

    [HideInInspector] public GameObject healthBar;
    Coroutine enemyHealthBarCoroutine;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    [HideInInspector] public Dummy decoy;
    [HideInInspector] public Queue<InstantiatedRoom> lastThreeRooms = new Queue<InstantiatedRoom>();

    bool bossHealthInitializationOnProcess;
    float invisibleTimer = 0f;
    const int ROOM_CONST = 6;
    Room currentRoom;
    Room previousRoom;
    PlayerDetailsSO playerDetails;
    InstantiatedRoom bossRoom;
    bool isFading = false;
    Vignette vignette;
    HashSet<Room> visitedRooms = new HashSet<Room>();

    Enemy bossEnemy;
    float blindTimer = 0f;

    // Pause Menu
    Resolution[] resolutions;
    Dictionary<string, List<int>> resolutionToHzMap;
    List<string> resolutionOptions;

    // Health Bar Materials
    [Header("HEALTH BAR MATERIALS")]
    [Space(10)]
    [SerializeField] Sprite standardSprite;
    [SerializeField] Sprite flashSprite;

    bool isManaRegenRunning = false;
    Coroutine manaRegenCoroutine;

    // Letterbox Materials
    [Header("Letterbox Cinematics")]
    [Space(10)]
    public Image topBar;
    public Image bottomBar;
    [SerializeField] float fadeDuration;

    bool deathChecked;

    protected override void Awake()
    {
        base.Awake();

        // Reset state only
        gameplayInitialized = false;
        localPlayer = null;

        // Single player spawn and initialization start
        if (!NetworkServer.active && !NetworkClient.active)
        {
            StartCoroutine(FinalizeLoadingSequence()); // Hide loading scene

            playerDetails = GameResources.Instance.currentPlayer.playerDetails;
            SpawnSinglePlayer(playerDetails.singlePlayerPrefab);
        }
    }

    #region SinglePlayer Initialization
    /// <summary>
    /// Create player in scene at position
    /// </summary>
    private void SpawnSinglePlayer(GameObject instantiatedPlayerPrefab)
    {
        // Instantiate player
        GameObject playerGameObject = Instantiate(instantiatedPlayerPrefab);

        // Initialize Player
        localPlayer = playerGameObject.GetComponent<Player>();
        localPlayer.Initialize(playerDetails);

        PrepareGameManagerSP();
    }
    private void PrepareGameManagerSP()
    {
        if (gameplayInitialized) return; // Prevent duplicate call

        gameplayInitialized = true;

        SubscribeMethods();
        SetInitialGameStateForSinglePlayer();
    }
#endregion

#region Multiplayer Initialization
    public void NotifyLocalPlayerReady(Player player)
    {
        localPlayer = player;

        // MP-Only Setup
        ClientEnterGameplay(player);
    }

    /// <summary>
    /// Player entrance into gameplay in MP
    /// </summary>
    private void ClientEnterGameplay(Player player)
    {
        if (gameplayInitialized) return;

        // Disable player before ui
        player.DisablePlayer();
        InputManager.Instance.DisableGameplayInput();

        localPlayer = player;
        gameplayInitialized = true;

        PreparePrefs();
        SubscribeMethods();
    }

    /// <summary>
    /// Set the local player for multiplayer
    /// </summary>
    private void SubscribeMethods()
    {
        HookPlayerEvents();
        Subscribe();

        healthBar = healthBarContainer.transform.GetChild(0).GetChild(0).gameObject;
        bookCover.SetActive(false);
        bookView.SetActive(false);
        warningPopUp.SetActive(false);
        introductionPopUp.SetActive(false);
    }
#endregion

    public Player GetPlayer()
    {
        return localPlayer;
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    private void Subscribe()
    {
        if (subscribed) return;
        subscribed = true;

        if (localPlayer == null) return;

        if (!NetworkServer.active && !NetworkClient.active)
        {
            StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnDecoySpawned += StaticEventHandler_OnDecoySpawned;
        }

        StaticEventHandler.OnOverviewCameraToggled += StaticEventHandler_OnOverviewCameraToggled;
        StaticEventHandler.OnLevelUp += StaticEventHandler_OnLevelUp;

        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;

        StaticEventHandler.OnHourglassSpawned += StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired += StaticEventHandler_OnHourglasExpired;

        StaticEventHandler.OnNPCInteractionStarted += StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded += StaticEventHandler_OnNPCInteractionEnded;
    }

    private void Unsubscribe()
    {
        if (!subscribed) return;
        subscribed = false;

        if (localPlayer == null) return;

        if (!NetworkServer.active && !NetworkClient.active)
        {
            StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
            StaticEventHandler.OnDecoySpawned -= StaticEventHandler_OnDecoySpawned;
        }

        StaticEventHandler.OnOverviewCameraToggled -= StaticEventHandler_OnOverviewCameraToggled;
        StaticEventHandler.OnLevelUp -= StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnHourglassSpawned -= StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired -= StaticEventHandler_OnHourglasExpired;

        StaticEventHandler.OnNPCInteractionStarted -= StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded -= StaticEventHandler_OnNPCInteractionEnded;

        localPlayer.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
        localPlayer.healthEvent.GetBlind -= PlayerGetBlind;
    }

    private void StaticEventHandler_OnOverviewCameraToggled(OverviewCameraFollowArgs overviewCameraFollowArgs)
    {
        isOverviewCameraEnabled = overviewCameraFollowArgs.isOn;
    }

    private void HookPlayerEvents()
    {
        if (localPlayer == null) return;

        localPlayer.destroyedEvent.OnDestroyed += Player_OnDestroyed;
        localPlayer.healthEvent.GetBlind += PlayerGetBlind;
    }

    private void SetInitialGameStateForSinglePlayer()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            previousGameState = GameState.lobby;
            gameState = GameState.playingLevel;
        }
    }

    private void PlayerGetBlind(HealthEvent healthEvent)
    {
        blindTimer = 8f;
        localPlayer.isBlind = true;
        localPlayer.blindModifier = 0.5f;
        localPlayer.UpdateCurrentAttackRatingValues();
        StaticEventHandler.CallStatsChangedOnTheBookEvent();
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);

        if (decoy != null)
        {
            Destroy(decoy.gameObject);
        }

        switch (roomChangedEventArgs.room.roomNodeType.roomNodeTypeName)
        {
            case "Corridor":
            case "Corridor NS":
            case "Corridor EW":
                return;

            case "Entrance":
                //TrailerModeItemsSpilling(roomChangedEventArgs);
                break;

            default:
                break;
        }

        visitedRooms.Add(currentRoom);
    }

    private void StaticEventHandler_OnLevelUp()
    {
        levelUpPanel.SetActive(true);
        levelUpPanel.GetComponentInChildren<Animator>().SetTrigger(Settings.zoomIn);
        levelUpPanelTimer = 0f;

        buttonBuildButton.SetActive(true);
    }

    public void ClickOpenCharacterBuild()
    {
        if (!bookView.activeSelf)
        {
            gameplayUI.FadeGameplayUI(gameplayUI.canvasGroup, 0f, 0.6f); // Make transparent

            bookView.SetActive(true);
            bookCover.SetActive(true);
            glossaryBookOpen = true;
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
            bookView.GetComponent<Animator>().SetTrigger(Settings.zoomIn);
        }

        StaticEventHandler.CallOpenBuildPageEvent();
    }

    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        if (roomEnemiesDefeatedArgs.summonedEnemies.Count < 1) return;

        foreach (GameObject summonedEnemy in roomEnemiesDefeatedArgs.summonedEnemies)
        {
            Destroy(summonedEnemy);
        }

        FindBossRoom();
    }

    private void StaticEventHandler_OnDecoySpawned(DecoySpawnedArgs decoySpawnedArgs)
    {
        SetDecoy(decoySpawnedArgs.decoy);
    }

    private void StaticEventHandler_OnHourglassSpawned()
    {
        vignette.color.value = new Color(0.67f, 0.66f, 0.18f);
        vignette.intensity.value = 0.7f;
    }

    private void StaticEventHandler_OnHourglasExpired()
    {
        vignette.color.value = new Color(1f, 1f, 1f);
        vignette.intensity.value = 0f;
    }

    private void StaticEventHandler_OnNPCInteractionStarted(NpcInteractionStartedArgs npcInteractionStartedArgs)
    {
        // Show Letterbox
        topBar.gameObject.SetActive(true);
        bottomBar.gameObject.SetActive(true);

        StopAllCoroutines();
        StartCoroutine(FadeBars(1f));
    }

    private void StaticEventHandler_OnNPCInteractionEnded()
    {
        // Hide Letterbox
        StopAllCoroutines();
        StartCoroutine(FadeBars(0f));

        topBar.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
        bottomBar.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
        topBar.gameObject.SetActive(false);
        bottomBar.gameObject.SetActive(false);
    }

    IEnumerator FadeBars(float targetAlpha)
    {
        float t = 0f;
        Color topColor = topBar.color;
        Color bottomColor = bottomBar.color;

        float initialTopAlpha = topColor.a;
        float initialBottomAlpha = bottomColor.a;

        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float alpha = Mathf.Lerp(initialTopAlpha, targetAlpha, t / fadeDuration);

            topBar.color = new Color(topColor.r, topColor.g, topColor.b, alpha);
            bottomBar.color = new Color(bottomColor.r, bottomColor.g, bottomColor.b, alpha);

            yield return null;
        }

        topBar.color = new Color(topColor.r, topColor.g, topColor.b, targetAlpha);
        bottomBar.color = new Color(bottomColor.r, bottomColor.g, bottomColor.b, targetAlpha);
    }

    /// <summary>
    /// Handle player destroyed event
    /// </summary>
    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        StartCoroutine(WaitAWhileForDeathProcessCompletedAtDestroyed());
    }

    IEnumerator WaitAWhileForDeathProcessCompletedAtDestroyed()
    {
        yield return new WaitForSeconds(0.6f);

        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    /// <summary>
    /// Handle decoy set
    /// </summary>
    private void SetDecoy(Dummy decoy)
    {
        this.decoy = decoy;
    }

    public Dummy GetDecoy()
    {
        return decoy;
    }

    private void Start()
    {
        // Set screen to black
        if (!NetworkServer.active && !NetworkClient.active)
        {
            // Enable camera
            localPlayer.cameraManager.ShowGameplay();

            previousGameState = GameState.gameStarted;
            gameState = GameState.gameStarted;
            StartCoroutine(Fade(0f, 1f, 0f, Color.black));
        }
    }

    public void PreparePrefs()
    {
        // Initialize and categorize resolutions
        resolutions = Screen.resolutions;
        resolutionToHzMap = new Dictionary<string, List<int>>();
        resolutionOptions = new List<string>();

        foreach (Resolution res in resolutions)
        {
            string key = $"{res.width} x {res.height}";
            int hz = (int)res.refreshRateRatio.value;

            if (!resolutionToHzMap.ContainsKey(key))
            {
                resolutionToHzMap[key] = new List<int>();
                resolutionOptions.Add(key);
            }

            if (!resolutionToHzMap[key].Contains(hz)) resolutionToHzMap[key].Add(hz);
        }

        // Sort Hz lists
        foreach (var kvp in resolutionToHzMap) kvp.Value.Sort();

        // Populate resolution dropdown
        resolutionDropdown.ClearOptions();
        resolutionDropdown.AddOptions(resolutionOptions);
        resolutionDropdown.onValueChanged.AddListener(OnResolutionDropdownChanged);

        // Sync toggle with current fullscreen state
        LoadSettingsFromPlayerPrefs();

        // Screen modes
        screenModeDropDown.ClearOptions();
        screenModeDropDown.AddOptions(new List<string> { "Exclusive Fullscreen", "Borderless Window", "Windowed" });

        // Add listeners
        refreshRateDropdown.onValueChanged.AddListener(OnRefreshRateChanged);
        screenModeDropDown.onValueChanged.AddListener(SetScreenMode);
        postProcessingToggle.onValueChanged.AddListener(OnPostProcessingToggleChanged);
        vsyncToggle.onValueChanged.AddListener(OnVsyncToggleChanged);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChanged);
        soundVolumeSlider.onValueChanged.AddListener(OnSoundVolumeSliderChanged);
        dynamicCameraToggle.onValueChanged.AddListener(OnDynamicCameraFollowToggleChanged);

        // Ensure the volume has a Vignette effect and store a reference to it
        if (volume != null && volume.profile.TryGet(out vignette))
        {
            vignette.intensity.value = 0f; // Set initial intensity if needed
        }
        else
        {
            Debug.LogError("Vignette effect not found on the Volume component.");
        }
    }

    IEnumerator FinalizeLoadingSequence()
    {
        yield return new WaitForSecondsRealtime(0.5f); // Let things settle

        if (LoadingManager.Instance != null)
        {
            LoadingManager.Instance.HideLoadingScreen();
        }
    }

    IEnumerator SmoothManaRegen()
    {
        float regenRate = 8f; // mana per second
        float regenInterval = 0.5f; // wait time between regen ticks

        while (localPlayer.mana.GetCurrentMana() < localPlayer.mana.GetMaximumMana() - localPlayer.mana.GetReservedMana())
        {
            float manaToAdd = regenRate * regenInterval;
            localPlayer.mana.AddMana((int)Mathf.Ceil(manaToAdd)); // round up to ensure visible progress
            yield return new WaitForSeconds(regenInterval);
        }

        isManaRegenRunning = false;
    }

    private void Update()
    {
        if (localPlayer == null) return;

        actions = InputManager.Instance.actions;

        bool roomCleared = currentRoom != null && EnemySpawner.Instance.transform.childCount <= 0;

        bool manaNotFull = localPlayer.mana.GetCurrentMana() < localPlayer.mana.GetMaximumMana();

        if (roomCleared && manaNotFull && !isManaRegenRunning)
        {
            manaRegenCoroutine = StartCoroutine(SmoothManaRegen());
            isManaRegenRunning = true;
        }

        // Stop regeneration if enemies return
        if (!roomCleared && isManaRegenRunning)
        {
            if(manaRegenCoroutine != null) StopCoroutine(manaRegenCoroutine);

            isManaRegenRunning = false;
        }

        if (localPlayer.health.hasDied && !deathChecked)
        {
            Debug.Log(localPlayer.playerDetails.playerCharacterIndex.ToString().ToUpper() + " is dead.");

            gameState = GameState.gameLost; // Update here in case of stuck
            deathChecked = true;
        }

        // GAME STATE
        HandleGameState();

        HandleLevelUpPanel();

        if (localPlayer.currentSkillPoints == 0)
        {
            buttonBuildButton.SetActive(false);
        }

        if (EnemySpawner.Instance.isBossInstantiated)
        {
            if (!bossHealthInitializationOnProcess)
            {
                StartCoroutine(EnemyHealthBarInitialization());
            }
        }
        else
        {
            healthBarContainer.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
            healthBar.transform.localScale = new Vector3(1f, 1f, 1f);
            healthBarContainer.SetActive(false);
        }

        //// Sorting player
        //PlayerSortTileCheckForFrontTileMap();
        //PlayerSortTileCheckForSideTileMap();

        // Adjust blind status
        blindTimer -= Time.deltaTime;

        if (blindTimer <= 0 && localPlayer.isBlind)
        {
            localPlayer.isBlind = false;
            localPlayer.blindModifier = 0f;
            localPlayer.healthEvent.CallBlindCuredEvent();
            localPlayer.UpdateCurrentAttackRatingValues();
            StaticEventHandler.CallStatsChangedOnTheBookEvent();
        }
    }

    IEnumerator EnemyHealthBarInitialization()
    {
        bossHealthInitializationOnProcess = true;

        float completeInvisibleDuration = 1f;

        bossEnemy = EnemySpawner.Instance.GetBoss();
        healthBarContainer.SetActive(true);
        healthBarContainer.GetComponentInChildren<TextMeshProUGUI>().text = bossEnemy.enemyDetails.enemyName;

        // Become invisible
        while (invisibleTimer < completeInvisibleDuration)
        {
            invisibleTimer += Time.deltaTime;

            float newAlpha = 0.2f; // Default to start value

            if (invisibleTimer > 0.8f) newAlpha = 1f;
            else if (invisibleTimer > 0.6f) newAlpha = 0.8f;
            else if (invisibleTimer > 0.3f) newAlpha = 0.6f;
            else if (invisibleTimer > 0.2f) newAlpha = 0.4f;

            // Apply alpha change
            Image barImage = healthBarContainer.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
            Color color = barImage.color;
            color.a = newAlpha;
            barImage.color = color;

            yield return null;
        }

        yield return null;

        bossHealthInitializationOnProcess = false;
    }

    private void HandleLevelUpPanel()
    {
        levelUpPanelTimer += Time.deltaTime;

        if (levelUpZoomInFinished)
        {
            levelUpPanelTimer = 0f;
        }
        else if (levelUpPanelTimer > levelUpPanelPopupDuration && levelUpPanel.activeInHierarchy)
        {
            levelUpZoomInFinished = false;
            levelUpPanel.GetComponentInChildren<Animator>().SetTrigger(Settings.zoomOut);
        }

        if (levelUpZoomOutFinished)
        {
            levelUpZoomOutFinished = false;
            levelUpPanel.SetActive(false);

        }
    }

    public void CloseBookInCasePauseClick()
    {
        bookZoomOutFinished = false;
        bookView.GetComponent<Animator>().enabled = false;
        bookView.GetComponent<Animator>().enabled = true;
        bookView.SetActive(false);
        bookCover.SetActive(false);
        glossaryBookOpen = false;
    }

    /// <summary>
    /// Handle game state
    /// </summary>
    private void HandleGameState()
    {
        // Handle game state
        if (!NetworkServer.active && !NetworkClient.active) HandleSinglePlayerState();
    }

    private void HandleSinglePlayerState()
    {
        if(NetworkClient.active) Debug.Log("IT'S FORBIDDEN ZONE IN MP!");

        switch (gameState)
        {
            case GameState.gameStarted:
                PreparePrefs();
                actions = InputManager.Instance.actions;
                currentDungeonLevelListIndex = InputManager.cachedLevelIndex > 1 ?
                    InputManager.cachedLevelIndex : (InputManager.TutorialEnabled && InputManager.cachedLevelIndex == 1 ? 0 : 1);
                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;
                FindBossRoom();
                break;
            case GameState.playingLevel:
                break;
            case GameState.engagingEnemies:
                break;
            case GameState.engagingBoss:
                break;
            case GameState.levelCompleted:
                // Display level completed text
                StartCoroutine(LevelCompleted());
                break;
            case GameState.gameWon:
                if (previousGameState != GameState.gameWon) StartCoroutine(GameWon());
                break;
            case GameState.gameLost:
                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines(); // Prevent messages if you clear the level just as you get killed
                    StartCoroutine(GameLost());
                }
                break;
            case GameState.gamePaused:
                break;
            case GameState.restartGame:
                RestartGame();
                break;
            case GameState.lobby:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Set the current room the player is in
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
    }

    /// <summary>
    /// Room enemies defeated - test if all dungeon rooms have been cleared of enemies - if so load next dungeon game level
    /// </summary>
    private void FindBossRoom()
    {
        // Loop through all dungeon rooms to see if cleared of enemies
        foreach (KeyValuePair<string, Room> keyValuePair in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            // Detect boss room
            if (keyValuePair.Value.roomNodeType.isBossRoom)
            {
                bossRoom = keyValuePair.Value.instantiatedRoom;
                break;
            }
        }
    }

    /// <summary>
    /// Pause game menu - also called from resume game button on pause menu
    /// </summary>
    public void TogglePauseUI()
    {
        if (gameState != GameState.gamePaused)
        {
            pauseMenu.SetActive(true);
            localPlayer.DisablePlayer();

            // Set game state
            previousGameState = gameState;
            gameState = GameState.gamePaused;

            // Delay setting selection to next frame to ensure layout is updated
            StartCoroutine(SetResumeButtonAsFirstSelected());
        }
        else if (gameState == GameState.gamePaused)
        {
            // Close all pause menu window panels
            ExitFromKeyboardMouseRebindingsMenu();
            ExitFromGamepadRebindingsMenu();
            BackFromSettingsMenu();
            BackFromControlsMenu();

            pauseMenu.SetActive(false);
            localPlayer.EnablePlayer();

            // Set game state
            gameState = previousGameState;
            previousGameState = GameState.gamePaused;
        }
    }

    IEnumerator SetResumeButtonAsFirstSelected()
    {
        yield return null; // wait one frame

        EventSystem.current.SetSelectedGameObject(null); // Clear selection to force new one
        EventSystem.current.SetSelectedGameObject(resumeButton.gameObject);
    }

    /// <summary>
    /// Called from Settings button
    /// </summary>
    public void OpenSettingsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Clear buttons on pause menu
        Transform buttonContainer = pauseContainer.transform.GetChild(1);

        for (int i = 0; i < buttonContainer.transform.childCount; i++)
        {
            buttonContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Write head-line text
        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "SETTINGS";

        // Open settings menu
        settingsContainer.SetActive(true);
    }

    /// <summary>
    /// Called from the Controls Button
    /// </summary>
    public void OpenControlsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Clear buttons on pause menu
        Transform buttonContainer = pauseContainer.transform.GetChild(1);

        for (int i = 0; i < buttonContainer.transform.childCount; i++)
        {
            buttonContainer.transform.GetChild(i).gameObject.SetActive(false);
        }

        // Open controls menu
        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "CONTROLS";

        // Get controls ui
        controlsContainer.gameObject.SetActive(true);
    }

    /// <summary>
    /// Called from the Keyboard&Mouse Button
    /// </summary>
    public void OpenKeyboardMouseRebindingsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Close controls container ui
        controlsContainer.gameObject.SetActive(false);

        // Open keyboard&mouse menu
        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "KEYBOARD&MOUSE";

        keyboardRebindingsContainer.SetActive(true);
    }

    /// <summary>
    /// Called from the Gamepad Button
    /// </summary>
    public void OpenGamepadRebindingsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Close controls container ui
        controlsContainer.gameObject.SetActive(false);

        // Open gamepad menu
        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "GAMEPAD";

        gamepadRebindingsContainer.SetActive(true);
    }

    /// <summary>
    /// Called from the Keyboard&Mouse Button
    /// </summary>
    public void ExitFromKeyboardMouseRebindingsMenu(bool escClicked = false)
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Close keyboard controls container ui
        keyboardRebindingsContainer.gameObject.SetActive(false);

        // Open keyboard&mouse menu
        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "CONTROLS";

        controlsContainer.SetActive(true);
    }

    /// <summary>
    /// Called from the Gamepad Button
    /// </summary>
    public void ExitFromGamepadRebindingsMenu(bool escClicked = false)
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Close game pad controls container ui
        gamepadRebindingsContainer.gameObject.SetActive(false);

        // Open gamepad menu
        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "CONTROLS";

        controlsContainer.SetActive(true);
    }

    /// <summary>
    /// Called from Back button in Settings menu
    /// </summary>
    public void BackFromSettingsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Close settings menu
        settingsContainer.SetActive(false); // Disable settings container

        // Get and activate button container
        Transform buttonContainer = pauseContainer.transform.GetChild(1);

        for (int i = 0; i < buttonContainer.transform.childCount; i++)
        {
            buttonContainer.transform.GetChild(i).gameObject.SetActive(true);
        }

        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "PAUSE MENU";
    }

    /// <summary>
    /// Called from Back button in Controls menu
    /// </summary>
    public void BackFromControlsMenu()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        // Close controls menu
        controlsContainer.gameObject.SetActive(false); // Disable settings container

        // Get and activate button container
        Transform buttonContainer = pauseContainer.transform.GetChild(1);

        for (int i = 0; i < buttonContainer.transform.childCount; i++)
        {
            buttonContainer.transform.GetChild(i).gameObject.SetActive(true);
        }

        pauseContainer.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = "PAUSE MENU";
    }

    private void LoadSettingsFromPlayerPrefs()
    {
        // VIDEO
        // Resolution
        if (PlayerPrefs.HasKey("ResolutionIndex") && PlayerPrefs.HasKey("RefreshRateIndex"))
        {
            int resIndex = PlayerPrefs.GetInt("ResolutionIndex");
            int hzIndex = PlayerPrefs.GetInt("RefreshRateIndex");

            resolutionDropdown.value = Mathf.Clamp(resIndex, 0, resolutionOptions.Count - 1);
            OnResolutionDropdownChanged(resolutionDropdown.value); // populates refreshRateDropdown

            string selectedRes = resolutionOptions[resolutionDropdown.value];
            int clampedHz = Mathf.Clamp(hzIndex, 0, resolutionToHzMap[selectedRes].Count - 1);
            refreshRateDropdown.value = clampedHz;

            ApplyResolution();
        }

        // Screen Mode
        if (PlayerPrefs.HasKey("ScreenModeIndex"))
        {
            int modeIndex = PlayerPrefs.GetInt("ScreenModeIndex");
            screenModeDropDown.value = modeIndex;
            SetScreenMode(modeIndex);
        }

        // Post-processing
        if (PlayerPrefs.HasKey("PostProcessing"))
        {
            bool pp = PlayerPrefs.GetInt("PostProcessing") == 1;
            postProcessingToggle.isOn = pp;
            PostProcessingEnabler.Instance.isOn = pp;
            UpdatePostProcessingCheckmarkVisibility(pp);
        }

        // VSync
        if (PlayerPrefs.HasKey("Vsync"))
        {
            bool vsync = PlayerPrefs.GetInt("Vsync") == 1;
            vsyncToggle.isOn = vsync;
            QualitySettings.vSyncCount = vsync ? 1 : 0;
            UpdateVysncCheckmarkVisibility(vsync);
        }

        resolutionDropdown.RefreshShownValue();
        refreshRateDropdown.RefreshShownValue();
        screenModeDropDown.RefreshShownValue();

        // AUDIO
        // Music Volume
        if (PlayerPrefs.HasKey("MusicVolume"))
        {
            float musicVol = PlayerPrefs.GetFloat("MusicVolume");
            musicVolumeSlider.value = musicVol;
            MusicManager.Instance.SetVolume((int)musicVol);
        }

        // Sound Volume
        if (PlayerPrefs.HasKey("SoundVolume"))
        {
            float soundVol = PlayerPrefs.GetFloat("SoundVolume");
            soundVolumeSlider.value = soundVol;
            SoundEffectManager.Instance.SetVolume((int)soundVol);
        }

        // GAME
        // Load Dynamic amera toggle
        if (PlayerPrefs.HasKey("DynamicCamera"))
        {
            bool dynamicCamera = PlayerPrefs.GetInt("DynamicCamera") == 1;
            dynamicCameraToggle.isOn = dynamicCamera;
            InterScenesSingleton.dynamicCameraFollowEnabled = dynamicCamera;
            StaticEventHandler.CallDynamicCameraToggled(dynamicCameraToggle.isOn);
            UpdateDynamicCameraFollowCheckmarkVisibility(dynamicCamera);
        }

        // CONTROLS
        if (PlayerPrefs.HasKey("Rebinds"))
        {
            var rebinds = PlayerPrefs.GetString("Rebinds");
            if (!string.IsNullOrEmpty(rebinds)) actions.LoadBindingOverridesFromJson(rebinds);
        }
    }

    private void OnResolutionDropdownChanged(int index)
    {
        string selectedRes = resolutionOptions[index];
        List<int> hzOptions = resolutionToHzMap[selectedRes];

        // Convert Hz list to readable labels
        List<string> hzLabels = hzOptions.ConvertAll(hz => hz + " Hz");

        refreshRateDropdown.ClearOptions();
        refreshRateDropdown.AddOptions(hzLabels);

        // Set dropdown.value *after* options are added and always within bounds
        refreshRateDropdown.value = Mathf.Clamp(refreshRateDropdown.value, 0, hzOptions.Count - 1);

        refreshRateDropdown.RefreshShownValue();

        // Immediately apply resolution with new Hz
        ApplyResolution();
    }

    private void OnRefreshRateChanged(int hzIndex)
    {
        ApplyResolution();
    }

    private void ApplyResolution()
    {
        string selectedRes = resolutionOptions[resolutionDropdown.value];
        string[] parts = selectedRes.Split('x');
        int width = int.Parse(parts[0].Trim());
        int height = int.Parse(parts[1].Trim());

        List<int> hzList = resolutionToHzMap[selectedRes];
        int safeHzIndex = Mathf.Clamp(refreshRateDropdown.value, 0, hzList.Count - 1);
        int selectedHz = hzList[safeHzIndex];

        // Create the RefreshRate struct directly
        var refreshRate = new RefreshRate
        {
            numerator = (uint)selectedHz,
            denominator = 1
        };
        Screen.SetResolution(width, height, Screen.fullScreenMode, refreshRate);
    }

    private void SetScreenMode(int modeIndex)
    {
        //SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        switch (modeIndex)
        {
            case 0:
                Screen.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                break;
            case 1:
                Screen.fullScreenMode = FullScreenMode.FullScreenWindow;
                break;
            case 2:
                Screen.fullScreenMode = FullScreenMode.Windowed;
                break;
            default:
                break;
        }

        //// Re-apply resolution to honor screen mode change
        //ApplyResolution();
    }

    private void OnPostProcessingToggleChanged(bool isOn)
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        PostProcessingEnabler.Instance.isOn = isOn;
        UpdatePostProcessingCheckmarkVisibility(isOn);
    }

    private void UpdatePostProcessingCheckmarkVisibility(bool show)
    {
        postProcessingCheckmarkImage.enabled = show;
    }

    private void OnDynamicCameraFollowToggleChanged(bool isOn)
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        InterScenesSingleton.dynamicCameraFollowEnabled = isOn ? true : false;
        StaticEventHandler.CallDynamicCameraToggled(dynamicCameraToggle.isOn);
        UpdateDynamicCameraFollowCheckmarkVisibility(isOn);
    }

    private void UpdateDynamicCameraFollowCheckmarkVisibility(bool show)
    {
        dynamicCameraCheckmarkImage.enabled = show;
    }

    private void OnVsyncToggleChanged(bool isOn)
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        QualitySettings.vSyncCount = isOn ? 1 : 0;
        UpdateVysncCheckmarkVisibility(isOn);
    }

    private void UpdateVysncCheckmarkVisibility(bool show)
    {
        vysncCheckmarkImage.enabled = show;
    }

    private void OnMusicVolumeSliderChanged(float newValue)
    {
        MusicManager.Instance.SetVolume((int)newValue);
    }

    private void OnSoundVolumeSliderChanged(float newValue)
    {
        SoundEffectManager.Instance.SetVolume((int)newValue);
    }

    /// <summary>
    /// Called from Play Game button
    /// </summary>
    public void QuitGame()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Called from Exit button
    /// </summary>
    public void ExitGame()
    {
        SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);

        Application.Quit();
    }

    /// <summary>
    /// Dungeon Map Screen Display
    /// </summary>
    public void DisplayDungeonOverviewMap()
    {
        // return if fading
        if (isFading) return;

        // Display dungeonOverviewMap
        localPlayer.cameraManager.dungeonMap.DisplayDungeonOverViewMap();
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // Build dungeon for level
        bool dungeonBuiltSuccessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex], InputManager.TutorialEnabled);

        if (!dungeonBuiltSuccessfully) Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");

        // Bake nav meshes when dungeon build is successful
        if (AstarPath.active != null) AstarPath.active.Scan();

        // Call static event that room has changed
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // Set player roughly mid-room
        localPlayer.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
            (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        // Get nearest spawn point in room nearest to player
        localPlayer.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(localPlayer.gameObject.transform.position);

        // Reset Phoenix Rising
        localPlayer.phoenixRisingUsed = false;

        // Display Dungeon Level Text
        StartCoroutine(DisplayDungeonLevelText());
    }

    /// <summary>
    /// Display the dungeon level text
    /// </summary>
    IEnumerator DisplayDungeonLevelText()
    {
        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f, 1f, Color.black));

        // Disable Gameplay After Fade
        localPlayer.playerControl.IsParrying = false;
        localPlayer.playerControl.isPlayerRolling = false;
        localPlayer.meleeAttackMainHand.IsAttacking = false;
        localPlayer.DisablePlayer();

        string messageText = "LEVEL " + (currentDungeonLevelListIndex).ToString() + "\n\n" + dungeonLevelList[currentDungeonLevelListIndex].levelName.ToUpper();

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.nextLevelSoundEffect);

        // Show Message (Safe)
        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.yellow, 1f, true));

        // Re-Enable Before Fade In
        localPlayer.EnablePlayer();
        InputManager.Instance.EnableGameplayInput();

        // Fade In
        yield return StartCoroutine(Fade(1f, 0f, 1f, Color.black));

        canvasGroup.alpha = 0f;
    }

    /// <summary>
    /// Display the message text for displaySeconds  - if displaySeconds =0 then the message is displayed until the return key is pressed
    /// </summary>
    private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds, bool timed = false)
    {
        // Set text
        messageTextTMP.SetText(text);
        messageTextTMP.color = textColor;

        float inputBuffer = 0.5f; // Delay before input is accepted
        float timer = 0f;

        // Wait for buffer to expire
        while (timer < inputBuffer)
        {
            timer += Time.unscaledDeltaTime;
            yield return null;
        }

        if (timed)
        {
            // Display the message for the given time
            if (displaySeconds > 0f)
            {
                float displayTimer = displaySeconds;

                yield return null;

                while (displayTimer > 0f && (!InputManager.Instance.AnyUIConfirmIntent()))
                {
                    displayTimer -= Time.unscaledDeltaTime;
                    yield return null;
                }

                if (gameState == GameState.levelCompleted)
                {
                    ClearAllRoomItemsOnLevelEnd();
                }
            }
            else
            // else display the message until the return button is pressed
            {
                while (!InputManager.Instance.AnyUIConfirmIntent())
                    yield return null;
                
            }
        }
        else
        {
            // Stay in this loop unless next level key is pressed
            while (!InputManager.Instance.AnyUIConfirmIntent())
            {
                yield return null;
            }
        }

        // Clear text
        messageTextTMP.SetText("");
    }

    /// <summary>
    /// Show level as being completed - load next level
    /// </summary>
    private IEnumerator LevelCompleted()
    {
        // Play next level
        gameState = GameState.playingLevel;

        // Wait 2 seconds
        yield return new WaitForSeconds(2f);

        // Fade in canvas to display text message
        yield return StartCoroutine(Fade(0f, 1f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        // Increase index to next level
        currentDungeonLevelListIndex++;

        // DEMO CASE
        if (currentDungeonLevelListIndex >= 3 && isDemo)
        {
            gameState = GameState.gameWon;
        }
        else if (currentDungeonLevelListIndex >= 9)
        {
            gameState = GameState.gameWon;
        }
        else
        {
            string upperName = localPlayer.playerDetails.playerCharacterName.ToUpper(CultureInfo.InvariantCulture);

            // Display level completed
            yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + upperName + "! YOU'VE SURVIVED\n\nTHIS DUNGEON " +
                "LEVEL! PRESS OK FOR NEXT LEVEL!", Color.yellow, 5f));

            // Fade out canvas
            yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

            PlayDungeonLevel(currentDungeonLevelListIndex);
        }
    }

    /// <summary>
    /// Fade Canvas Group
    /// </summary>
    public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color backgroundColor)
    {
        isFading = true;
        fadeImage.color = backgroundColor;

        float elapsed = 0f;

        while (elapsed < fadeSeconds)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(elapsed / fadeSeconds);
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, t);
            yield return null;
        }

        // Ensure exact final value
        canvasGroup.alpha = targetFadeAlpha;

        isFading = false;
    }

    /// <summary>
    /// Game Won
    /// </summary>
    private IEnumerator GameWon()
    {
        previousGameState = GameState.gameWon;

        // Disable player
        localPlayer.DisablePlayer();

        string upperName = localPlayer.playerDetails.playerCharacterName.ToUpper(CultureInfo.InvariantCulture);

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        // Display game won - DEMO
        if (isDemo)
        {
            yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + upperName + "!\nYOU HAVE COMPLETED DEMO!",
                Color.green, 7f));
        }
        else
        {
            // Display game won
            yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + upperName + "\nYOU HAVE SECURED THE WARTHEON",
                Color.green, 7f));
        }

        yield return StartCoroutine(DisplayMessageRoutine("PRESS ENTER TO RESTART THE GAME", Color.yellow, 0f));

        // Set game state to restart game
        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Game Lost
    /// </summary>
    private IEnumerator GameLost()
    {
        previousGameState = GameState.gameLost;

        // Disable player
        localPlayer.DisablePlayer();

        // Wait 1 seconds
        yield return new WaitForSeconds(1f);

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        // Disable enemies (FindObjectsOfType is resource hungry - but ok to use in this end of game situation)
        Enemy[] enemyArray = FindObjectsByType<Enemy>(FindObjectsSortMode.None);
        foreach (Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        string upperName = localPlayer.playerDetails.playerCharacterName.ToUpper(CultureInfo.InvariantCulture);

        // Display game lost
        yield return StartCoroutine(DisplayMessageRoutine("BAD LUCK " + upperName + 
            "! YOU HAVE\nDIED SOMEWHERE IN WARTHEON.", Color.red, 2f, true));

        yield return StartCoroutine(DisplayMessageRoutine("PRESS ENTER TO RESTART THE GAME", Color.yellow, 0f));

        // Set game state to restart game
        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Restart the game
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Get the player minimap icon
    /// </summary>
    public Sprite GetPlayerMinimapIcon()
    {
        return playerDetails.playerMiniMapIcon;
    }

    /// <summary>
    /// Get the current room the player is in
    /// </summary>
    public Room GetCurrentRoom()
    {
        return currentRoom;
    }

    /// <summary>
    /// Get the current dungeon level
    /// </summary>
    public DungeonLevelSO GetCurrentDungeonLevel()
    {
        return dungeonLevelList[currentDungeonLevelListIndex];
    }

    /// <summary>
    /// Spawn object
    /// </summary>
    public Transform SpawnObject(Vector3 position, GameObject toDrop)
    {
        Transform t = Instantiate(toDrop, transform).transform;
        t.position = position;

        return t;
    }

    /// <summary>
    /// Get Boss room
    /// </summary>
    public Room GetBossRoom()
    {
        foreach (KeyValuePair<string, Room> item in DungeonBuilder.Instance.dungeonBuilderRoomDictionary)
        {
            if (item.Value.roomNodeType.isBossRoom)
            {
                return item.Value;
            }
        }

        return null;
    }

    /// <summary>
    /// Set health bar value with health between 0 and 1
    /// </summary>
    public void SetHealthBarValue(float healthValue, Enemy enemy)
    {
        if (enemy != null)
        {
            if (enemy.enemyDetails.isEnemyBoss)
            {
                if (enemyHealthBarCoroutine != null)
                {
                    StopCoroutine(enemyHealthBarCoroutine);
                }

                float targetScaleValue = healthValue / enemy.health.GetMaximumHealth();
                enemyHealthBarCoroutine = StartCoroutine(SmoothHealthBarChange(targetScaleValue));
            }
        }
    }

    IEnumerator SmoothHealthBarChange(float targetValue)
    {
        float duration = 1f; // Adjust duration as needed
        float elapsed = 0f;
        float startValue = healthBar.transform.localScale.x;

        // Apply sprite change
        Image barImage = healthBarContainer.transform.GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();
        barImage.sprite = flashSprite;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float newValue = Mathf.Lerp(startValue, targetValue, elapsed / duration);
            healthBar.transform.localScale = new Vector3(newValue, 1f, 1f);

            yield return null;
        }

        barImage.sprite = standardSprite;
        healthBar.transform.localScale = new Vector3(targetValue, 1f, 1f);
    }

    public void GoToWeaponSetWithIndex(int setIndex, bool onStart = false)
    {
        localPlayer.playerControl.NextWeaponSet(true, false, onStart, setIndex);
    }

    public void WeaponSetOne(bool onStart = false)
    {
        localPlayer.playerControl.NextWeaponSet(true, false, onStart, 1);
    }

    public void WeaponSetTwo(bool onStart = false)
    {
        localPlayer.playerControl.NextWeaponSet(true, false, onStart, 2);
    }

    public void WeaponSetThree(bool onStart = false)
    {
        localPlayer.playerControl.NextWeaponSet(true, false, onStart, 3);
    }

    public void OpenPopUpLog(PopUpReason popUpReason, int shardGain = 0)
    {
        warningPopUp.SetActive(true);
        popUpWindowOpen = true; // It's used for disabling mouse fire button while window is open

        TextMeshProUGUI warningText = warningPopUp.transform.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();

        switch (popUpReason)
        {
            case PopUpReason.None:
                break;
            case PopUpReason.LessThanOneMainHandWeapon:
                warningText.text = "Equipped main hand weapon can't be less than 1 in 3 sets.";
                break;
            case PopUpReason.DontHaveWeaponOnSelectedSet:
                warningText.text = "Can't switch to next set because there is no weapon.";
                break;
            case PopUpReason.OffHandFull:
                warningText.text = "You can't drop main weapon. Active weapon set's off-hand is full.";
                break;
            case PopUpReason.ShieldCantBePutOnMainHand:
                warningText.text = "Shield can not be equipped on the main hand.";
                break;
            case PopUpReason.OffHandCantBeAddedToTwoHanded:
                warningText.text = "Off-hand weapon can't be added while main hand has a two-handed weapon.";
                break;
            case PopUpReason.TwoHandCantBeEquippedToOffHand:
                warningText.text = "Two-hand weapon can't be equipped to off-hand.";
                break;
            case PopUpReason.OffHandCatBeAddedToEmptyMainHand:
                warningText.text = "Off-hand weapon can't be addet to the set not having weapon on main hand.";
                break;
            case PopUpReason.EmptyOffHandFirst:
                warningText.text = "Empty your off-hand first.";
                break;
            case PopUpReason.EquipMainHandFirst:
                warningText.text = "Equip your main first.";
                break;
            case PopUpReason.CantMoveYourMainHandWithEmptyOffHand:
                warningText.text = "You can't move your main hand weapon if your off-hand weapon is empty at the same set.";
                break;
            case PopUpReason.YourHandsFull:
                warningText.text = "All sets in main hand is full. Drop one of your weapons first.";
                break;
            case PopUpReason.DontMeetRequiredCharacter:
                warningText.text = "You don't have required character to wield this weapon. Instead you dismantled this item and obtained "
                    + shardGain + " shards.";
                break;
            case PopUpReason.NotEnoughShardsForUpgrade:
                warningText.text = "You don't have enough shards.\nYou need to upgrade to:\nTo Enchanted: 100 Shards\nTo Mythic: 350 Shards";
                break;
            case PopUpReason.ReachedMaxUpgradeLevel:
                warningText.text = "You reached max upgrade level of this item.";
                break;
            case PopUpReason.UpgradeCompletedLog:
                warningText.text = "Item upgrade is completed.";
                break;
            case PopUpReason.DismantleCompletedLog:
                warningText.text = "Item dismantle is completed. \n\n" + shardGain + " shards obtained.";
                break;
            default:
                break;
        }
    }

    public void RegisterRoomVisit(InstantiatedRoom room)
    {
        if (lastThreeRooms.Contains(room)) return;

        if (room.IsCorridor()) return;

        if (lastThreeRooms.Count == 3)
        {
            InstantiatedRoom roomToClear = lastThreeRooms.Dequeue();
            roomToClear.DestroyAllDroppedItems();
        }

        lastThreeRooms.Enqueue(room);
    }

    public void ClearAllRoomItemsOnLevelEnd()
    {
        foreach (InstantiatedRoom room in lastThreeRooms)
        {
            room.DestroyAllDroppedItems();
        }

        lastThreeRooms.Clear();
    }

    public static void TutorialIndicatorArrowTransactions(Transform arrowTransform, Transform secondArrowTransform)
    {
        if (InputManager.TutorialEnabled)
        {
            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.SkillsPage)
            {
                arrowTransform.gameObject.SetActive(true);
            }
            else if (TutorialInteraction.Instance.currentTutorialPhase >= TutorialPhase.SkillsPage)
            {
                arrowTransform.gameObject.SetActive(false);
            }

            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.OtherCollectionsPage)
            {
                secondArrowTransform.gameObject.SetActive(true);
            }
            else if (TutorialInteraction.Instance.currentTutorialPhase >= TutorialPhase.OtherCollectionsPage)
            {
                secondArrowTransform.gameObject.SetActive(false);
            }
        }
        else
        {
            arrowTransform.gameObject.SetActive(false);
            secondArrowTransform.gameObject.SetActive(false);
        }
    }

    public void CloseWarningPopUpMenu()
    {
        warningPopUp.SetActive(false);
        popUpWindowOpen = false;
    }

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(pauseMenu), pauseMenu);
        HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }
#endif
    #endregion
}
