using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;
using UnityEngine.Tilemaps;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.Controls;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    public static bool isDemo = true;
    public static bool tutorialEnabled = false;

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
    [SerializeField] GameObject pauseMenu;
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
    #region Tooltip
    [Tooltip("Populate with the MessageText textmeshpro component in the FadeScreenUI")]
    #endregion
    [SerializeField] TextMeshProUGUI messageTextTMP;
    #region Tooltip
    [Tooltip("Populate with the FadeImage canvasgroup component in the FadeScreenUI")]
    #endregion Tooltip
    [SerializeField] CanvasGroup canvasGroup;
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
    [Header("TOOLTIP PANEL REFERENCES")]
    // Tooltip panel
    public GameObject tooltipPanel;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI requirementText;
    public TextMeshProUGUI weaponClassText;
    public TextMeshProUGUI hitSpeedText;
    public TextMeshProUGUI weaponWieldText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI baseHandlingText;
    public TextMeshProUGUI crHitChanceText;
    public TextMeshProUGUI crHitDamageText;
    public TextMeshProUGUI elementalBiasText;
    public TextMeshProUGUI elementText;
    public TextMeshProUGUI elementalForgeRateText;
    public TextMeshProUGUI masteryText1;
    public TextMeshProUGUI masteryText2;
    public TextMeshProUGUI masteryText3;

    [Space(10)]
    // Tooltip panel equipped
    public GameObject tooltipPanelEquipped;
    public TextMeshProUGUI headerTextEquipped;
    public TextMeshProUGUI levelTextEquipped;
    public TextMeshProUGUI equippedText;
    public TextMeshProUGUI weaponClassTextEquipped;
    public TextMeshProUGUI hitSpeedTextEquipped;
    public TextMeshProUGUI weaponWieldTextEquipped;
    public TextMeshProUGUI damageTextEquipped;
    public TextMeshProUGUI baseHandlingTextEquipped;
    public TextMeshProUGUI crHitChanceTextEquipped;
    public TextMeshProUGUI crHitDamageTextEquipped;
    public TextMeshProUGUI elementalBiasTextEquipped;
    public TextMeshProUGUI elementTextEquipped;
    public TextMeshProUGUI elementalForgeRateTextEquipped;
    public TextMeshProUGUI masteryText1Equipped;
    public TextMeshProUGUI masteryText2Equipped;
    public TextMeshProUGUI masteryText3Equipped;

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
    Coroutine healthBarCoroutine;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    [HideInInspector] public Decoy decoy;
    [HideInInspector] public int exploredRoomCount = 0;
    [HideInInspector] public Queue<InstantiatedRoom> lastThreeRooms = new Queue<InstantiatedRoom>();


    bool bossHealthInitializationOnProcess;
    float invisibleTimer = 0f;
    const int ROOM_CONST = 6;
    Room currentRoom;
    Room previousRoom;
    PlayerDetailsSO playerDetails;
    Player player;
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

    // Tooltip
    TooltipSource currentTooltipSource;

    // Weapon Level 
    [Header("WEAPON LEVEL COLORS")]
    [Space(10)]
    [HideInInspector] public Color basicLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color basicLevelColor2 = new Color(0.2196078f, 0.172549f, 0.172549f);
    [HideInInspector] public Color enchantedLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color enchantedLevelColor2 = new Color(0f, 0.4588235f, 1f);
    [HideInInspector] public Color mythicLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color mythicLevelColor2 = new Color(0.6745098f, 0, 1);
    [HideInInspector] public Color legendaryLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color legendaryLevelColor2 = new Color(1f, 0.09411765f, 0f);

    // Weapon Level 
    [Header("ELEMENTAL COLORS")]
    [Space(10)]
    [HideInInspector] public Color noneElementalColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color noneElementalColor2 = new Color(1, 1, 1);
    [HideInInspector] public Color fireColor1 = new Color(0.9686275f, 1, 0.2980392f);
    [HideInInspector] public Color fireColor2 = new Color(1f, 0.1921569f, 0.2431373f);
    [HideInInspector] public Color waterColor1 = new Color(0.8431373f, 0.9647059f, 1);
    [HideInInspector] public Color waterColor2 = new Color(0, 0.5882353f, 1);
    [HideInInspector] public Color airColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color airColor2 = new Color(0.4235294f, 0.4235294f, 0.4235294f);
    [HideInInspector] public Color earthColor1 = new Color(0.5647059f, 1f, 0.2509804f);
    [HideInInspector] public Color earthColor2 = new Color(0.02352941f, 0.4352941f, 0.03529412f);
    [HideInInspector] public Color lightColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color lightColor2 = new Color(0.9716981f, 0.8067644f, 0f);
    [HideInInspector] public Color darkColor1 = new Color(0.627451f, 0, 1);
    [HideInInspector] public Color darkColor2 = new Color(0.6784314f, 0.01568628f, 0.5607843f);

    [HideInInspector] public Color passiveItemColor = new Color(0f, 0.7f, 1f);

    // Health Bar Materials
    [Header("HEALTH BAR MATERIALS")]
    [Space(10)]
    [SerializeField] Sprite standardSprite;
    [SerializeField] Sprite flashSprite;

    // Letterbox Materials
    [Header("Letterbox Cinematics")]
    [Space(10)]
    public Image topBar;
    public Image bottomBar;
    [SerializeField] float fadeDuration;

    // Check sprite overlap status
    bool spriteOverlapped = false;

    protected override void Awake()
    {
        base.Awake();

        //currentDungeonLevelListIndex = MainMenuUI.currentDungeonLevelListIndex;

        // Set player details - saved in current player scriptable object from the main menu
        playerDetails = GameResources.Instance.currentPlayer.playerDetails;

        // Instantiate player
        InstantiatePlayer();
    }

    /// <summary>
    /// Create player in scene at position
    /// </summary>
    private void InstantiatePlayer()
    {
        // Instantiate player
        GameObject playerGameObject = Instantiate(playerDetails.playerPrefab);

        // Initialize Player
        player = playerGameObject.GetComponent<Player>();

        player.Initialize(playerDetails);
    }

    private void OnEnable()
    {
        StaticEventHandler.OnLevelUp += StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnDecoySpawned += StaticEventHandler_OnDecoySpawned;
        StaticEventHandler.OnHourglassSpawned += StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired += StaticEventHandler_OnHourglasExpired;

        StaticEventHandler.OnNPCInteractionStarted += StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded += StaticEventHandler_OnNPCInteractionEnded;

        player.healthEvent.GetBlind += PlayerGetBlind;
        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;

        if (InputManager.Instance != null)
        {
            InputManager.Instance.overviewMapFullView.action.started += ControlDisplayDungeonOverviewMap;
            InputManager.Instance.overviewMapFullView.action.canceled += ControlClearDungeonOverviewMap;
        }
    }

    private void OnDisable()
    {
        StaticEventHandler.OnLevelUp -= StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnDecoySpawned -= StaticEventHandler_OnDecoySpawned;
        StaticEventHandler.OnHourglassSpawned -= StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired -= StaticEventHandler_OnHourglasExpired;

        StaticEventHandler.OnNPCInteractionStarted -= StaticEventHandler_OnNPCInteractionStarted;
        StaticEventHandler.OnNPCInteractionEnded -= StaticEventHandler_OnNPCInteractionEnded;

        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
        player.healthEvent.GetBlind -= PlayerGetBlind;

        if (InputManager.Instance  != null)
        {
            InputManager.Instance.overviewMapFullView.action.started -= ControlDisplayDungeonOverviewMap;
            InputManager.Instance.overviewMapFullView.action.canceled -= ControlClearDungeonOverviewMap;
        }
    }

    private void PlayerGetBlind(HealthEvent healthEvent)
    {
        blindTimer = 8f;
        player.isBlind = true;
        player.blindModifier = 0.5f;
        player.UpdateCurrentHandlingValues();
        StaticEventHandler.CallPrimaryStatsChangedEvent();
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);

        tooltipPanel.SetActive(false);

        if (decoy != null)
        {
            Destroy(decoy.gameObject);
        }

        if (Player.hasClone)
        {
            Destroy(player.playerCloneObject);

            if (player.tripleTeamEnabled)
            {
                Destroy(player.playerSecondCloneObject);
            }

            Player.hasClone = false;
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

                if (!visitedRooms.Contains(currentRoom))
                {
                    if (player.selectedActiveItem.GetCurrentActiveItem() == null) return;

                    if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge ==
                        player.selectedActiveItem.GetCurrentActiveItem().activeItemMaxCharge) return;

                    int refreshedCharge = (int)(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemChargeRegenerationPerSixRooms *
                        ++exploredRoomCount / ROOM_CONST);

                    if (refreshedCharge >= 1)
                    {
                        player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge += refreshedCharge;

                        if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge >
                            player.selectedActiveItem.GetCurrentActiveItem().activeItemMaxCharge)
                        {
                            player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge =
                                player.selectedActiveItem.GetCurrentActiveItem().activeItemMaxCharge;
                        }

                        if (player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemType == ActiveItemType.Dummy)
                        {
                            player.selectedActiveItem.GetCurrentActiveItem().decoyUsed = false;
                        }

                        player.weaponFiredEvent.CallActiveItemFiredEvent(player.selectedActiveItem.GetCurrentActiveItem());

                        exploredRoomCount = 0;
                    }
                }

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

    private static void TrailerModeItemsSpilling(RoomChangedEventArgs roomChangedEventArgs)
    {
        Transform chestItemContainerTransform = roomChangedEventArgs.room.instantiatedRoom.GetComponentInChildren<ChestItemContainer>().transform;
        ChestItemContainer chestItemContainer = chestItemContainerTransform.GetComponent<ChestItemContainer>();

        if (roomChangedEventArgs.room.prefab.CompareTag("trailerOnly"))
        {

            Transform playerCharsContainerTransform = chestItemContainer.transform.GetChild(0);
            Transform npcCharsContainerTransform = chestItemContainer.transform.GetChild(1);

            for (int i = 0; i < GameResources.Instance.npcPrefabs.Length; i++)
            {
                if (i < 3)
                {
                    GameObject npcObject = Instantiate(GameResources.Instance.npcPrefabs[i], npcCharsContainerTransform);

                    // Calculate 'world' grid parent doorway position
                    Vector2 center = (Vector2)(roomChangedEventArgs.room.lowerBounds + roomChangedEventArgs.room.upperBounds) / 2f;

                    npcObject.transform.position += new Vector3(i * 2, 0f, 0f);
                }
                else if (i < 7)
                {
                    GameObject playerObject = Instantiate(GameResources.Instance.npcPrefabs[i], playerCharsContainerTransform);

                    // Calculate 'world' grid parent doorway position
                    Vector2 center = (Vector2)(roomChangedEventArgs.room.lowerBounds + new Vector2Int(2, 2) +
                        roomChangedEventArgs.room.upperBounds + new Vector2Int(2, 2)) / 2f;

                    playerObject.transform.position += new Vector3(i * 2, 0f, 0f);
                }
            }
        }
        else if (roomChangedEventArgs.room.prefab.CompareTag("trailerBoss"))
        {
            GameObject moldranObject = Instantiate(GameResources.Instance.npcPrefabs[7], chestItemContainerTransform);
        }
        else
        {
            // Weapon populate loop
            for (int i = 0; i < chestItemContainerTransform.GetChild(0).childCount; i++)
            {
                DropItem chestItem = chestItemContainerTransform.GetChild(0).GetChild(i).GetComponent<DropItem>();

                chestItem.hasWeaponDrop = true;
                Weapon weapon = new Weapon();
                weapon.weaponDetails = chestItemContainer.chestWeaponItems[i];

                chestItem.Initialize(weapon, weapon.weaponDetails.weaponFrontSprite, chestItem.transform.position);
            }

            // Active item populate loop
            for (int i = 0; i < chestItemContainerTransform.GetChild(1).childCount; i++)
            {
                DropItem chestItem = chestItemContainerTransform.GetChild(1).GetChild(i).GetComponent<DropItem>();

                chestItem.hasActiveDrop = true;
                ActiveItem activeItem = new ActiveItem();
                activeItem.activeItemDetails = chestItemContainer.chestActiveItems[i];

                chestItem.Initialize(activeItem, activeItem.activeItemDetails.activeItemSprite, chestItem.transform.position);
            }

            // Passive item populate loop
            for (int i = 0; i < chestItemContainerTransform.GetChild(2).childCount; i++)
            {
                DropItem chestItem = chestItemContainerTransform.GetChild(2).GetChild(i).GetComponent<DropItem>();

                if (chestItemContainer.chestPassiveItems[i].passiveItemCategory == PassiveItemCategory.Primary)
                {
                    chestItem.hasPrimaryPassiveDrop = true;
                }
                else if (chestItemContainer.chestPassiveItems[i].passiveItemCategory == PassiveItemCategory.Secondary)
                {
                    chestItem.hasSecondaryPassiveDrop = true;
                }

                PassiveItem passiveItem = new PassiveItem();
                passiveItem.passiveItemDetails = chestItemContainer.chestPassiveItems[i];

                chestItem.Initialize(passiveItem, passiveItem.passiveItemDetails.passiveItemSprite, chestItem.transform.position);
            }
        }
    }

    private void StaticEventHandler_OnRoomEnemiesDefeated(RoomEnemiesDefeatedArgs roomEnemiesDefeatedArgs)
    {
        if (roomEnemiesDefeatedArgs.summonedEnemies.Count < 1) return;

        foreach (GameObject summonedEnemy in roomEnemiesDefeatedArgs.summonedEnemies)
        {
            Destroy(summonedEnemy);
        }

        RoomEnemiesDefeated();
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
        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    /// <summary>
    /// Handle decoy set
    /// </summary>
    private void SetDecoy(Decoy decoy)
    {
        this.decoy = decoy;
    }

    public Decoy GetDecoy()
    {
        return decoy;
    }

    private void Start()
    {
        actions = InputManager.Instance.actions;

        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;

        healthBar = healthBarContainer.transform.GetChild(0).GetChild(0).gameObject;

        bookCover.SetActive(false);
        bookView.SetActive(false);
        warningPopUp.SetActive(false);
        introductionPopUp.SetActive(false);

        // Set screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        // PAUSE MENU
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

    private void Update()
    {
        HandleBook();
        HandlePopUp();
        HandleLevelUpPanel();

        // GAME STATE
        HandleGameState();

        if (player.currentBuildPoints == 0)
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

        // Sorting player
        PlayerSortTileCheckForFrontTileMap();
        PlayerSortTileCheckForSideTileMap();

        // Adjust blind status
        blindTimer -= Time.deltaTime;

        if (blindTimer <= 0 && player.isBlind)
        {
            player.isBlind = false;
            player.blindModifier = 0f;
            player.healthEvent.CallBlindCuredEvent();
            player.UpdateCurrentHandlingValues();
            StaticEventHandler.CallPrimaryStatsChangedEvent();
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

    private void HandleBook()
    {
        if (pauseMenu.activeSelf) return;

        if (!bookView.activeSelf) 
        {
            gameplayUI.FadeGameplayUI(gameplayUI.canvasGroup, 1f, 0.6f);
        }

        if (bookZoomInFinished) bookZoomInFinished = false;

        if (bookZoomOutFinished)
        {
            bookZoomOutFinished = false;
            bookView.SetActive(false);
            bookCover.SetActive(false);
            glossaryBookOpen = false;
        }

        if (turnPageCompleted)
        {
            bookView.GetComponent<Animator>().SetBool(Settings.turnPage, false);
            turnPageCompleted = false;
        }

        if (InputManager.Instance.bookView.action.WasPressedThisFrame())
        {
            if (bookView.activeSelf)
            {
                gameplayUI.FadeGameplayUI(gameplayUI.canvasGroup, 1f, 0.6f); // Opaque

                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);

                bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);

                player.meleeAttackMainHand.IsAttacking = false; // To be safe-side
                player.playerControl.IsParrying = false;
                player.playerControl.isPlayerRolling = false;

                Time.timeScale = 1f;
            }
            else
            {
                bookView.SetActive(true);
                bookCover.SetActive(true);
                glossaryBookOpen = true;

                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);

                // First trigger the animation (it uses UnscaledTime, so it's safe to call here)
                bookView.GetComponent<Animator>().SetTrigger(Settings.zoomIn);

                Time.timeScale = 0f;

                // Finally, hide gameplay UI
                gameplayUI.FadeGameplayUI(gameplayUI.canvasGroup, 0f, 0.6f); // Transparent
            }
        }
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

    private void HandlePopUp()
    {
        if (popUpWindowOpen)
        {
            if (InputManager.Instance.OKButton.action.WasPressedThisFrame())
            {
                CloseWarningPopUpMenu();
            }
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
        switch (gameState)
        {
            case GameState.gameStarted:
                // Play first level or tutorial
                currentDungeonLevelListIndex = InputManager.cachedLevelIndex;

                PlayDungeonLevel(currentDungeonLevelListIndex);
                gameState = GameState.playingLevel;

                // Trigger room enemies defeated since we start in the entrance where there are no enemies (just in case you have a level with just a boss room)
                RoomEnemiesDefeated();
                break;

            // While playing the level handle the tab key for the dungeon overview map
            case GameState.playingLevel:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    // If book is open, firstly close the book instead of opening pause menu
                    if (bookView.activeSelf)
                    {
                        Time.timeScale = 1f;
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                        bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
                    }
                    else
                    {
                        PauseGameMenu();
                    }
                }

                if (InputManager.Instance.overviewMapFullView.action.WasPressedThisFrame())
                {
                    DisplayDungeonOverviewMap();
                }
                break;

            case GameState.engagingEnemies:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    // If book is open, firstly close the book instead of opening pause menu
                    if (bookView.activeSelf)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                        bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
                    }
                    else
                    {
                        PauseGameMenu();
                    }
                }
                break;

            case GameState.engagingBoss:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    // If book is open, firstly close the book instead of opening pause menu
                    if (bookView.activeSelf)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                        bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
                    }
                    else
                    {
                        PauseGameMenu();
                    }
                }
                break;

            // If in the dungeon overview map handle the release of the tab key to clear the map
            case GameState.dungeonOverviewMap:
                // Key released
                if (InputManager.Instance.overviewMapFullView.action.WasReleasedThisFrame())
                {
                    // Clear dungeonOverviewMap
                    DungeonMap.Instance.ClearDungeonOverViewMap();
                }
                break;

            // Handle the level being completed
            case GameState.levelCompleted:
                // Display level completed text
                StartCoroutine(LevelCompleted());
                break;

            // handle the game being won (only trigger this once - test the previous game state to do this)
            case GameState.gameWon:
                if (previousGameState != GameState.gameWon)
                    StartCoroutine(GameWon());
                break;

            // handle the game being lost (only trigger this once - test the previous game state to do this)
            case GameState.gameLost:
                if (previousGameState != GameState.gameLost)
                {
                    StopAllCoroutines(); // Prevent messages if you clear the level just as you get killed
                    StartCoroutine(GameLost());
                }
                break;

            // restart the game
            case GameState.restartGame:
                RestartGame();
                break;

            case GameState.gamePaused:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    // If book is open, firstly close the book instead of opening pause menu
                    if (bookView.activeSelf)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                        bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
                    }
                    else
                    {
                        PauseGameMenu();
                    }
                }
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

    /// <summary> - Front tile map
    /// Check player's sorting status
    /// </summary>
    private void PlayerSortTileCheckForFrontTileMap()
    {
        Tilemap frontTilemap = currentRoom.instantiatedRoom.frontTilemap;

        WeapoonSortingCheck(frontTilemap, true);
    }

    /// <summary>
    /// Check player's sorting status - Side tile map
    /// </summary>
    private void PlayerSortTileCheckForSideTileMap()
    {
        Tilemap sideTilemap = currentRoom.instantiatedRoom.sideTilemap;

        if (!spriteOverlapped)
        {
            WeapoonSortingCheck(sideTilemap);
        }

        spriteOverlapped = false;
    }

    /// <summary>
    /// Sort if weapon holding transform overlap front or side tilemap when player is not above the tile
    /// </summary>
    private void WeapoonSortingCheck(Tilemap tilemap, bool isFrontTilemap = false)
    {
        if(player != null)
        {
            // MAIN HAND
            // Player's weapon tile position - Main hand
            Vector3 localMainWeaponCenterPos = player.mainHandWeaponAnchorTransform.localPosition;
            Vector3 localMainWeaponUpperPos = player.mainHandWeaponAnchorTransform.localPosition;

            // Center point
            localMainWeaponCenterPos.x += 0.5f; // Increase X relative to the parent(player)
            localMainWeaponCenterPos.y += 1f; // Increase Y relative to the parent (player)

            // Upper point
            localMainWeaponUpperPos.x += 0.5f; // Increase X relative to the parent(player)
            localMainWeaponUpperPos.y += 2f; // Increase Y relative to the parent (player)


            // OFF-HAND
            // Player's weapon tile position - Off-hand
            Vector3 localOffWeaponCenterPos = player.offHandWeaponAnchorTransform.localPosition;
            Vector3 localOffWeaponUpperPos = player.offHandWeaponAnchorTransform.localPosition;

            // Center point
            localOffWeaponCenterPos.x += 0.5f; // Increase X relative to the parent(player)
            localOffWeaponCenterPos.y += 1.5f; // Increase Y relative to the parent (player)

            // Upper point
            localOffWeaponUpperPos.x += 0.5f; // Increase X relative to the parent(player)
            localOffWeaponUpperPos.y += 3f; // Increase Y relative to the parent (player)


            // Retrieve anchor positions
            Vector3Int mainHandWeaponAnchorCenterPos = GetWeaponIntPosition(tilemap, localMainWeaponCenterPos);
            Vector3Int mainHandWeaponAnchorUpperPos = GetWeaponIntPosition(tilemap, localMainWeaponUpperPos);

            Vector3Int offHandWeaponAnchorCenterPos = GetWeaponIntPosition(tilemap, localOffWeaponCenterPos);
            Vector3Int offHandWeaponAnchorUpperPos = GetWeaponIntPosition(tilemap, localOffWeaponUpperPos);

            // Check the tile directly above the player - CENTER
            TileBase tileForMainHand = tilemap.GetTile(mainHandWeaponAnchorCenterPos);
            tileForMainHand = tileForMainHand != null ? tileForMainHand : tilemap.GetTile(mainHandWeaponAnchorUpperPos);

            // Check the tile directly above the player - UPPER
            TileBase tileForOffHand = tilemap.GetTile(offHandWeaponAnchorCenterPos);
            tileForOffHand = tileForOffHand != null ? tileForOffHand : tilemap.GetTile(offHandWeaponAnchorUpperPos);

            if (tileForMainHand != null || (player.activeWeapon.GetCurrentOffHandWeapon() != null && tileForOffHand != null))
            {
                player.sortingGroup.sortingLayerID = SortingLayer.NameToID("Front");// Change to higher layer

                if (isFrontTilemap)
                {
                    spriteOverlapped = true;
                }
            }
            else
            {
                player.sortingGroup.sortingLayerID = SortingLayer.NameToID("Instances"); // Reset to default
            }
        }
    }

    private Vector3Int GetWeaponIntPosition(Tilemap tilemap, Vector3 localMainWeaponPos)
    {
        Vector3 adjustedMainHandWeaponWorldPos = player.transform.TransformPoint(localMainWeaponPos); // Convert adjusted local position to world position
        Vector3Int mainHandWeaponAnchorPos = tilemap.WorldToCell(adjustedMainHandWeaponWorldPos); // Convert adjusted world position to tile coordinates
        return mainHandWeaponAnchorPos;
    }

    /// <summary>
    /// Room enemies defeated - test if all dungeon rooms have been cleared of enemies - if so load next dungeon game level
    /// </summary>
    private void RoomEnemiesDefeated()
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
    public void PauseGameMenu()
    {
        if (gameState != GameState.gamePaused)
        {
            pauseMenu.SetActive(true);
            GetPlayer().playerControl.DisablePlayer();

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
            GetPlayer().playerControl.EnablePlayer();

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


    private void ControlDisplayDungeonOverviewMap(InputAction.CallbackContext context)
    {
        // While playing the level handle the tab key for the dungeon overview map.
        if (gameState == GameState.playingLevel)
        {
            DisplayDungeonOverviewMap();
        }
    }

    private void ControlClearDungeonOverviewMap(InputAction.CallbackContext context)
    {
        // If in the dungeon overview map handle the release of the tab key to clear the map
        if (gameState == GameState.dungeonOverviewMap)
        {
            // Clear dungeonOverviewMap
            DungeonMap.Instance.ClearDungeonOverViewMap();
        }
    }

    /// <summary>
    /// Dungeon Map Screen Display
    /// </summary>
    private void DisplayDungeonOverviewMap()
    {
        // return if fading
        if (isFading) return;

        // Display dungeonOverviewMap
        DungeonMap.Instance.DisplayDungeonOverViewMap();
    }

    private void PlayDungeonLevel(int dungeonLevelListIndex)
    {
        // Build dungeon for level
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex], tutorialEnabled);

        if (!dungeonBuiltSucessfully)
        {
            Debug.LogError("Couldn't build dungeon from specified rooms and node graphs");
        }

        // Call static event that room has changed
        StaticEventHandler.CallRoomChangedEvent(currentRoom);

        // Set player roughly mid-room
        player.gameObject.transform.position = new Vector3((currentRoom.lowerBounds.x + currentRoom.upperBounds.x) / 2f,
            (currentRoom.lowerBounds.y + currentRoom.upperBounds.y) / 2f, 0f);

        // Get nearest spawn point in room nearest to player
        player.gameObject.transform.position = HelperUtilities.GetSpawnPositionNearestToPlayer(player.gameObject.transform.position);

        // Display Dungeon Level Text
        StartCoroutine(DisplayDungeonLevelText());
    }

    /// <summary>
    /// Display the dungeon level text
    /// </summary>
    IEnumerator DisplayDungeonLevelText()
    {
        // Set screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

        player.playerControl.IsParrying = false;
        player.playerControl.isPlayerRolling = false;
        player.meleeAttackMainHand.IsAttacking = false; // Reset values before disable
        player.playerControl.DisablePlayer();

        string messageText = "LEVEL " + (currentDungeonLevelListIndex).ToString() + "\n\n" + dungeonLevelList[currentDungeonLevelListIndex].
            levelName.ToUpper();

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.nextLevelSoundEffect);

        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.yellow, 1f, true));

        GetPlayer().playerControl.EnablePlayer();

        // Fade In
        yield return StartCoroutine(Fade(1f, 0f, 1.5f, Color.black));

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

                while (displayTimer > 0f && !AnyInputPressed())
                {
                    displayTimer -= Time.deltaTime;
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
                while (!AnyInputPressed()) yield return null;
                
            }
        }
        else
        {
            // Stay in this loop unless next level key is pressed
            while (!AnyInputPressed())
            {
                yield return null;
            }
        }

        // Clear text
        messageTextTMP.SetText("");
    }


    /// <summary>
    /// Input check for any button with any device
    /// </summary>
    private bool AnyInputPressed()
    {
        // Exception buttons such as movement, scroll or pick up
        if (InputManager.Instance.movement.action.WasPressedThisFrame() || InputManager.Instance.pointerPosition.action.WasPressedThisFrame() ||
            InputManager.Instance.gamepadAim.action.WasPressedThisFrame() || InputManager.Instance.interaction.action.WasPressedThisFrame()) return false;

        if (Keyboard.current.anyKey.wasPressedThisFrame || Mouse.current.leftButton.wasPressedThisFrame || Mouse.current.rightButton.wasPressedThisFrame) return true;

        // Any button on gamepad
        if (Gamepad.current != null)
        {
            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame) return true;
            }
        }

        return false;
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
            // Display level completed
            yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + player.playerDetails.playerCharacterName + "! YOU'VE SURVIVED\n\nTHIS DUNGEON " +
                "LEVEL! PRESS ANY KEY FOR NEXT LEVEL!", Color.yellow, 5f));

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
        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgroundColor;

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
        GetPlayer().playerControl.DisablePlayer();

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        // Display game won - DEMO
        if (isDemo)
        {
            yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + player.playerDetails.playerCharacterName + "! YOU HAVE COMPLETED DEMO!",
                Color.green, 7f));
        }
        else
        {
            // Display game won
            yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + player.playerDetails.playerCharacterName + "! YOU HAVE SECURED THE WARTHEON",
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
        GetPlayer().playerControl.DisablePlayer();

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

        // Display game lost
        yield return StartCoroutine(DisplayMessageRoutine("BAD LUCK " + player.playerDetails.playerCharacterName + 
            "! YOU HAVE\nSUCCUMBED TO THE DUNGEON", Color.red, 2f, true));

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
    /// Get the player
    /// </summary>
    public Player GetPlayer()
    {
        return player;
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
                if (healthBarCoroutine != null)
                {
                    StopCoroutine(healthBarCoroutine);
                }

                float targetScaleValue = healthValue / enemy.health.GetMaximumHealth();

                healthBarCoroutine = StartCoroutine(SmoothHealthBarChange(targetScaleValue));
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
        player.playerControl.NextWeaponSet(true, false, onStart, setIndex);
    }

    public void WeaponSetOne(bool onStart = false)
    {
        player.playerControl.NextWeaponSet(true, false, onStart, 1);
    }

    public void WeaponSetTwo(bool onStart = false)
    {
        player.playerControl.NextWeaponSet(true, false, onStart, 2);
    }

    public void WeaponSetThree(bool onStart = false)
    {
        player.playerControl.NextWeaponSet(true, false, onStart, 3);
    }

    public void OpenWarningPopUpMenu(PopUpReason popUpReason)
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
            case PopUpReason.DontMeetRequiredPrimaryStats:
                warningText.text = "You don't have required stat points to wield this weapon.";
                break;
            case PopUpReason.BobbyPinFailed:
                warningText.text = "Lockpick with Bobby Pin failed.";
                break;
            case PopUpReason.SummonerFailed:
                warningText.text = "Try to summon your creature in the room full of enemies.";
                break;
            default:
                break;
        }
    }

    public void UpdateTooltipPanelInfo(ItemGeneric itemGeneric, bool hasWeaponDrop, bool hasActiveDrop, bool hasSecondaryPassiveDrop, TooltipSource source)
    {
        if (currentTooltipSource == source) return;

        currentTooltipSource = source;

        tooltipPanel.SetActive(true);
        if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (itemGeneric is Weapon)
            {
                Weapon weapon = (Weapon)itemGeneric;

                if (weapon.weaponDetails.weaponClass != WeaponClass.Shield)
                {
                    tooltipPanelEquipped.SetActive(true);
                }
            }
        }

        ClearTooltipPanel();
        ClearTooltipEquippedPanel();

        if (hasSecondaryPassiveDrop)
        {
            if (itemGeneric is PassiveItem)
            {
                headerText.colorGradient = new VertexGradient(passiveItemColor, passiveItemColor, passiveItemColor, passiveItemColor);
                levelText.colorGradient = new VertexGradient(passiveItemColor, passiveItemColor, passiveItemColor, passiveItemColor);
                PassiveItem passiveItem = (PassiveItem)itemGeneric;
                PassiveItemDetailsSO passiveItemDetails = passiveItem.passiveItemDetails;

                headerText.text = passiveItemDetails.passiveItemName;
                levelText.text = $"(Passive Item)";

                // HEAD
                if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
                {
                    weaponClassText.text = "+100% Accuracy for Bows";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                {
                    weaponClassText.text = "+10% Light Resistance";
                    hitSpeedText.text = "+10% Chance to Blind";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                {
                    weaponClassText.text = "+1 Dexterity";
                    hitSpeedText.text = "+10% Physical Resistance";
                    weaponWieldText.text = "+10% Evasiveness";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                {
                    weaponClassText.text = "+1 Intelligence";
                    hitSpeedText.text = "+5% All Elemental Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                {
                    weaponClassText.text = "+1 Dexterity";
                    hitSpeedText.text = "+15% Evasiveness";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                {
                    weaponClassText.text = "+1 Constitution";
                    hitSpeedText.text = "+20% Physical Resistance";
                }
                //NECK
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                {
                    weaponClassText.text = "+20% Fire Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                {
                    weaponClassText.text = "+20% Earth Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                {
                    weaponClassText.text = "+20% Air Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                {
                    weaponClassText.text = "+20% Water Resistance";
                }
                // CHEST
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                {
                    weaponClassText.text = "+1 Strength";
                    hitSpeedText.text = "+30% Physical Resistance";
                    weaponWieldText.text = "Absorbs +30% Physical Damage";
                    damageText.text = "When Healt is below 50%";
                    baseHandlingText.text = "";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BlazingHeartplate)
                {
                    weaponClassText.text = "+1 Strength";
                    hitSpeedText.text = "+20% Physical Resistance";
                    weaponWieldText.text = "+10% Fire Resistance";
                    damageText.text = "-5% Attack Cooldown";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.FrostboundChainmail)
                {
                    weaponClassText.text = "+15% Physical Resistance";
                    hitSpeedText.text = "+10% Water Resistance";
                    weaponWieldText.text = "Immune to Frost";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomweaveVest)
                {
                    weaponClassText.text = "+10% Physical Resistance";
                    hitSpeedText.text = "+10% Earth Resistance";
                    weaponWieldText.text = "Immune to Poison";
                }
                // WAIST
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                {
                    weaponClassText.text = "+1 Intelligence";
                    hitSpeedText.text = "-20% Cast Duration";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                {
                    weaponClassText.text = "+1 Constitution";
                    hitSpeedText.text = "+5% Physical Resistance";
                    weaponWieldText.text = "+15% Fire Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                {
                    weaponClassText.text = "+10% Air Resistance";
                    hitSpeedText.text = "+10% Light Resistance";
                    weaponWieldText.text = "Immune to Blind";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                {
                    weaponClassText.text = "+1 Strength";
                    hitSpeedText.text = "+1 Agility";
                    weaponWieldText.text = "-5% Melee Attack Cooldown";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                {
                    weaponClassText.text = "+1 Dexterity";
                    hitSpeedText.text = "+10% Evasiveness";
                    weaponWieldText.text = "+5% Cr.Hit Chance";
                }
                // FINGER
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfFortune)
                {
                    weaponClassText.text = "+15% Drop Chance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                {
                    weaponClassText.text = "-20% Attack Cooldown";
                    hitSpeedText.text = "-10% Physical Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfMight)
                {
                    weaponClassText.text = "+1 Strength";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfVitality)
                {
                    weaponClassText.text = "+1 Constitution";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfSagacity)
                {
                    weaponClassText.text = "+1 Intelligence";
                }
                // ARM
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                {
                    weaponClassText.text = "+5% Physical Resistance";
                    hitSpeedText.text = "+10% Air Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmbercladBracers)
                {
                    weaponClassText.text = "+10% Physical Resistance";
                    hitSpeedText.text = "+8% Fire Resistance";
                    weaponWieldText.text = "-5% Attack Cooldown";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomTouchedGloves)
                {
                    weaponClassText.text = "+10% Earth Resistance";
                    hitSpeedText.text = "Immunity to Poison";
                }
                // BACK
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    weaponClassText.text = "+5% Cr. Hit Chance";
                    hitSpeedText.text = "+10% Cr. Hit Chance When";
                    weaponWieldText.text = "Dual-Wield Dagger or Claw Equipped";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                {
                    weaponClassText.text = "+1 Agility";
                    hitSpeedText.text = "+10% Evasiveness";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                {
                    weaponClassText.text = "+5% Elemental Damage";
                    hitSpeedText.text = "+15% Elemental Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.CloakOfWindwalker)
                {
                    weaponClassText.text = "+2 Agility";
                    hitSpeedText.text = "+30% Air Resistance";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GoldenCloak)
                {
                    weaponClassText.text = "+1 All Primary Stats";
                }
                // LEG
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                {
                    weaponClassText.text = "+2 Agility";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BootsOfInfernalMarch)
                {
                    weaponClassText.text = "+1 Agility";
                    hitSpeedText.text = "+15% Fire Resistance";
                }
            }
        }
        if (hasActiveDrop)
        {
            if (itemGeneric is ActiveItem)
            {
                headerText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                levelText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                ActiveItem activeItem = (ActiveItem)itemGeneric;
                ActiveItemDetailsSO activeItemDetails = activeItem.activeItemDetails;

                headerText.text = activeItemDetails.activeItemName;
                levelText.text = $"(Active Item)";

                if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Dummy)
                {
                    weaponClassText.text = "Distracts Enemies Until Being";
                    hitSpeedText.text = "Destroyed";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Potion)
                {
                    weaponClassText.text = "Slowly Regenerates Health";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Bomb)
                {
                    weaponClassText.text = "Explodes and Gives AoE Damage";
                    //hitSpeedText.text = "AoE Damage";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Compass)
                {
                    weaponClassText.text = "Locates Boss Room's";
                    hitSpeedText.text = "Direction";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Boomerang)
                {
                    weaponClassText.text = "Strikes And Return, Useful";
                    hitSpeedText.text = "For Stunning Enemies";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Hourglass)
                {
                    weaponClassText.text = "Slows the Time Flow By";
                    hitSpeedText.text = "Half to Act More Precisely";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Shiruken)
                {
                    weaponClassText.text = "Several Quick Throwable Star Projectiles";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Pentagram)
                {
                    weaponClassText.text = "Trap for Enemies To Step On";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Summoner)
                {
                    weaponClassText.text = "Summoning Ally Mobs as Companion";
                    hitSpeedText.text = "For a Short Time";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.BobbyPin)
                {
                    weaponClassText.text = "Chance to Crack The";
                    hitSpeedText.text = "Chest Without a Key";
                    weaponWieldText.text = "Only One Attempt Permitted";
                }
            }
        }

        if (hasWeaponDrop)
        {
            if (itemGeneric is Weapon)
            {
                Weapon weapon = (Weapon)itemGeneric;
                WeaponDetailsSO weaponDetails = weapon.weaponDetails;

                // Populate text field based on the related weapon info
                switch (weaponDetails.weaponLevel)
                {
                    case WeaponLevel.Basic:
                        headerText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1,basicLevelColor2, basicLevelColor2);
                        levelText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                        break;
                    case WeaponLevel.Enchanted:
                        headerText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1,enchantedLevelColor2, enchantedLevelColor2);
                        levelText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                        break;
                    case WeaponLevel.Mythic:
                        headerText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1,mythicLevelColor2, mythicLevelColor2);
                        levelText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                        break;
                    case WeaponLevel.Legendary:
                        headerText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1,legendaryLevelColor2, legendaryLevelColor2);
                        levelText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1,legendaryLevelColor2, legendaryLevelColor2);
                        break;
                    default:
                        break;
                }

                Weapon equippedWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

                // Equipped
                if (equippedWeapon != null && weapon.weaponDetails.weaponClass != WeaponClass.Shield)
                {
                    switch (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponLevel)
                    {
                        case WeaponLevel.Basic:
                            headerTextEquipped.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                            break;
                        case WeaponLevel.Enchanted:
                            headerTextEquipped.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                            break;
                        case WeaponLevel.Mythic:
                            headerTextEquipped.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                            break;
                        case WeaponLevel.Legendary:
                            headerTextEquipped.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                            break;
                        default:
                            break;
                    }

                    equippedText.text = "Equipped";
                    headerTextEquipped.text = equippedWeapon.weaponDetails.weaponName;
                    levelTextEquipped.text = $"({equippedWeapon.weaponDetails.weaponLevel.ToString()})";
                    weaponClassTextEquipped.text = $"Class: {equippedWeapon.weaponDetails.weaponClass.ToString()}";
                    hitSpeedTextEquipped.text = $"Speed: {equippedWeapon.weaponDetails.weaponHitSpeed.ToString()}";
                    weaponWieldTextEquipped.text = $"Wield Type: {equippedWeapon.weaponDetails.wieldType.ToString()}";

                    if (equippedWeapon.weaponDetails.isMeleeWeapon)
                    {
                        damageTextEquipped.text = $"Damage: {equippedWeapon.weaponDetails.meleeDamageMin}-{equippedWeapon.weaponDetails.meleeDamageMax}";
                    }
                    else
                    {
                        damageTextEquipped.text = $"Damage: {equippedWeapon.weaponDetails.weaponCurrentProjectile.projectileDamageMin}-" +
                            $"{equippedWeapon.weaponDetails.weaponCurrentProjectile.projectileDamageMax}";
                    }

                    int dropWeaponDamageMax = weaponDetails.isMeleeWeapon ? weaponDetails.meleeDamageMax : weaponDetails.weaponCurrentProjectile.projectileDamageMax;
                    int equippedWeaponDamageMax = equippedWeapon.weaponDetails.isMeleeWeapon ? equippedWeapon.weaponDetails.meleeDamageMax : 
                        equippedWeapon.weaponDetails.weaponCurrentProjectile.projectileDamageMax;

                    if (equippedWeaponDamageMax > dropWeaponDamageMax)
                    {
                        damageTextEquipped.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                        damageText.colorGradient = new VertexGradient(Color.red, Color.red, Color.red, Color.red);
                    }
                    else if (equippedWeaponDamageMax == dropWeaponDamageMax)
                    {
                        damageTextEquipped.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
                        damageText.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
                    }
                    else
                    {
                        damageTextEquipped.colorGradient = new VertexGradient(Color.red, Color.red, Color.red, Color.red);
                        damageText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                    }

                    baseHandlingTextEquipped.text = $"Base Handling: {equippedWeapon.weaponDetails.weaponBaseHandling * 100}%";
                    crHitChanceTextEquipped.text = $"Base Cr. Hit Chance: {equippedWeapon.weaponDetails.criticalHitChance * 100}%";

                    if (equippedWeapon.weaponDetails.isMeleeWeapon)
                    {
                        crHitDamageTextEquipped.text = $"Base Cr. Hit Damage: {(equippedWeapon.weaponDetails.criticalHitDamageMultiplier + player.additionalCriticalMeleeDamageModifier) * 100}%";
                    }
                    else
                    {
                        crHitDamageTextEquipped.text = $"Base Cr. Hit Damage: {equippedWeapon.weaponDetails.criticalHitDamageMultiplier * 100}%";
                    }

                    elementalBiasTextEquipped.text = "Elemental Bias:";

                    // Populate text field based on the related elemental info
                    switch (equippedWeapon.weaponDetails.elementalBias)
                    {
                        case ElementalBias.None:
                            elementTextEquipped.colorGradient = new VertexGradient(noneElementalColor1, noneElementalColor1, noneElementalColor2, noneElementalColor2);
                            break;
                        case ElementalBias.Fire:
                            elementTextEquipped.colorGradient = new VertexGradient(fireColor1, fireColor1, fireColor2, fireColor2);
                            break;
                        case ElementalBias.Water:
                            elementTextEquipped.colorGradient = new VertexGradient(waterColor1, waterColor1, waterColor2, waterColor2);
                            break;
                        case ElementalBias.Earth:
                            elementTextEquipped.colorGradient = new VertexGradient(earthColor1, earthColor1, earthColor2, earthColor2);
                            break;
                        case ElementalBias.Air:
                            elementTextEquipped.colorGradient = new VertexGradient(airColor1, airColor1, airColor2, airColor2);
                            break;
                        case ElementalBias.Dark:
                            elementTextEquipped.colorGradient = new VertexGradient(darkColor1, darkColor1, darkColor2, darkColor2);
                            break;
                        case ElementalBias.Light:
                            elementTextEquipped.colorGradient = new VertexGradient(lightColor1, lightColor1, lightColor2, lightColor2);
                            break;
                        default:
                            break;
                    }

                    elementTextEquipped.text = equippedWeapon.weaponDetails.elementalBias.ToString();
                    elementalForgeRateTextEquipped.text = $"El. Forge Rate: {equippedWeapon.weaponDetails.elementalForgeRate * 100}%";

                    switch (equippedWeapon.weaponDetails.weaponLevel)
                    {
                        case WeaponLevel.Basic:
                            masteryText1Equipped.gameObject.SetActive(false);
                            masteryText2Equipped.gameObject.SetActive(false);
                            masteryText3Equipped.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Enchanted:
                            masteryText1Equipped.gameObject.SetActive(true);
                            masteryText1Equipped.text = "Enchanted Mastery: Locked";
                            masteryText2Equipped.gameObject.SetActive(false);
                            masteryText3Equipped.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Mythic:
                            masteryText1Equipped.gameObject.SetActive(true);
                            masteryText1Equipped.text = "Enchanted Mastery: Locked";
                            masteryText2Equipped.gameObject.SetActive(true);
                            masteryText2Equipped.text = "Mythic Mastery: Locked";
                            masteryText3Equipped.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Legendary:
                            masteryText1Equipped.gameObject.SetActive(true);
                            masteryText1Equipped.text = "Enchanted Mastery: Locked";
                            masteryText2Equipped.gameObject.SetActive(true);
                            masteryText2Equipped.text = "Mythic Mastery: Locked";
                            masteryText3Equipped.gameObject.SetActive(true);
                            masteryText3Equipped.text = "Legendary Mastery: Locked";
                            break;
                        default:
                            break;

                    }
                }

                headerText.text = weaponDetails.weaponName;
                levelText.text = $"({weaponDetails.weaponLevel.ToString()})";

                requirementText.text = UpdateRequirementText(weaponDetails);
                weaponClassText.text = $"Class: {weaponDetails.weaponClass.ToString()}";

                if (weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    weaponWieldText.text = $"Wield Type: {weaponDetails.wieldType.ToString()}";
                    damageText.text = $"Deflect Rate: {weaponDetails.projectileDeflectRatio * 100}%";
                }
                else
                {
                    hitSpeedText.text = $"Speed: {weaponDetails.weaponHitSpeed.ToString()}";
                    weaponWieldText.text = $"Wield Type: {weaponDetails.wieldType.ToString()}";

                    if (weaponDetails.isMeleeWeapon)
                    {
                        damageText.text = $"Damage: {weaponDetails.meleeDamageMin}-{weaponDetails.meleeDamageMax}";
                    }
                    else
                    {
                        damageText.text = $"Damage: {weaponDetails.weaponCurrentProjectile.projectileDamageMin}-{weaponDetails.weaponCurrentProjectile.projectileDamageMax}";
                    }
                }

                baseHandlingText.text = $"Base Handling: {weaponDetails.weaponBaseHandling * 100}%";
                crHitChanceText.text = $"Base Cr. Hit Chance: {weaponDetails.criticalHitChance * 100 }%";

                if (weaponDetails.isMeleeWeapon)
                {
                    crHitDamageText.text = $"Base Cr. Hit Damage: {(weaponDetails.criticalHitDamageMultiplier + player.additionalCriticalMeleeDamageModifier) * 100}%";
                }
                else
                {
                    crHitDamageText.text = $"Base Cr. Hit Damage: {weaponDetails.criticalHitDamageMultiplier * 100}%";
                }

                elementalBiasText.text = "Elemental Bias:";


                // Populate text field based on the related elemental info
                switch (weaponDetails.elementalBias)
                {
                    case ElementalBias.None:
                        elementText.colorGradient = new VertexGradient(noneElementalColor1, noneElementalColor1,noneElementalColor2, noneElementalColor2);
                        break;
                    case ElementalBias.Fire:
                        elementText.colorGradient = new VertexGradient(fireColor1, fireColor1, fireColor2, fireColor2);
                        break;
                    case ElementalBias.Water:
                        elementText.colorGradient = new VertexGradient(waterColor1, waterColor1, waterColor2, waterColor2);
                        break;
                    case ElementalBias.Earth:
                        elementText.colorGradient = new VertexGradient(earthColor1, earthColor1, earthColor2, earthColor2);
                        break;
                    case ElementalBias.Air:
                        elementText.colorGradient = new VertexGradient(airColor1, airColor1, airColor2, airColor2);
                        break;
                    case ElementalBias.Dark:
                        elementText.colorGradient = new VertexGradient(darkColor1, darkColor1, darkColor2, darkColor2);
                        break;
                    case ElementalBias.Light:
                        elementText.colorGradient = new VertexGradient(lightColor1, lightColor1, lightColor2, lightColor2);
                        break;
                    default:
                        break;
                }

                elementText.text = weaponDetails.elementalBias.ToString();
                elementalForgeRateText.text = $"El. Forge Rate: {weaponDetails.elementalForgeRate * 100}%";

                switch (weaponDetails.weaponLevel)
                {
                    case WeaponLevel.Basic:
                        masteryText1.gameObject.SetActive(false);
                        masteryText2.gameObject.SetActive(false);
                        masteryText3.gameObject.SetActive(false);
                        break;
                    case WeaponLevel.Enchanted:
                        masteryText1.gameObject.SetActive(true);
                        masteryText1.text = "Enchanted Mastery: Locked";
                        masteryText2.gameObject.SetActive(false);
                        masteryText3.gameObject.SetActive(false);
                        break;
                    case WeaponLevel.Mythic:
                        masteryText1.gameObject.SetActive(true);
                        masteryText1.text = "Enchanted Mastery: Locked";
                        masteryText2.gameObject.SetActive(true);
                        masteryText2.text = "Mythic Mastery: Locked";
                        masteryText3.gameObject.SetActive(false);
                        break;
                    case WeaponLevel.Legendary:
                        masteryText1.gameObject.SetActive(true);
                        masteryText1.text = "Enchanted Mastery: Locked";
                        masteryText2.gameObject.SetActive(true);
                        masteryText2.text = "Mythic Mastery: Locked";
                        masteryText3.gameObject.SetActive(true);
                        masteryText3.text = "Legendary Mastery: Locked";
                        break;
                    default:
                        break;

                }
            }
        }
    }

    /// <summary>
    /// Updates the requirement text based on the weapon's required stats.
    /// </summary>
    /// <param name="weaponDetails">The weapon details ScriptableObject.</param>
    public string UpdateRequirementText(WeaponDetailsSO weaponDetails)
    {
        PrimaryStats requiredStats = weaponDetails.requiredPrimaryStats;

        string requirementString = "Requirement: ";

        if (requiredStats.strength > 0) 
        {
            requirementString += $"STR: {requiredStats.strength} ";
        };

        if (requiredStats.dexterity > 0) requirementString += $"DEX: {requiredStats.dexterity} ";

        if (requiredStats.constitution > 0) requirementString += $"CON: {requiredStats.constitution} ";

        if (requiredStats.intelligence > 0) requirementString += $"INT: {requiredStats.intelligence} ";

        if (requiredStats.agility > 0) requirementString += $"AGI: {requiredStats.agility} ";

        if ((requiredStats.strength > 0 && player.CurrentStrengthValue< requiredStats.strength) ||
            (requiredStats.dexterity > 0 && player.CurrentDexterityValue < requiredStats.dexterity) ||
            (requiredStats.constitution > 0 && player.CurrentConstitutionValue < requiredStats.constitution) ||
            (requiredStats.intelligence > 0 && player.CurrentIntelligenceValue < requiredStats.intelligence) ||
            (requiredStats.agility > 0 && player.CurrentAgilityValue < requiredStats.agility))
        {
            requirementText.color = Color.red;
        }
        else
        {
            requirementText.color = Color.green;
        }

        return requirementString;
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
        Debug.Log("Enqueued room count is " + lastThreeRooms.Count);
    }

    public void ClearAllRoomItemsOnLevelEnd()
    {
        foreach (InstantiatedRoom room in lastThreeRooms)
        {
            room.DestroyAllDroppedItems();
        }

        lastThreeRooms.Clear();
    }

    private void ClearTooltipPanel()
    {
        currentTooltipSource = TooltipSource.None;

        foreach (Transform child in tooltipPanel.transform)
        {
            child.GetComponent<TextMeshProUGUI>().text = string.Empty;
        }
    }

    private void ClearTooltipEquippedPanel()
    {
        foreach (Transform child in tooltipPanelEquipped.transform)
        {
            child.GetComponent<TextMeshProUGUI>().text = string.Empty;
        }
    }

    public void CloseTooltipPanel()
    {
        tooltipPanel.SetActive(false);
    }

    public void CloseTooltipEquippedPanel()
    {
        tooltipPanelEquipped.SetActive(false);
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
    #endregion Validation
}
