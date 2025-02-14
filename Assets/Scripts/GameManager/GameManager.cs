using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header GAMEOBJECT REFERENCES
    [Space(10)]
    [Header("GAMEOBJECT REFERENCES")]
    #endregion Header GAMEOBJECT REFERENCES

    #region Tooltip
    [Tooltip("Populate with pause menu gameobject in the hierarchy")]
    #endregion
    [SerializeField] GameObject pauseMenu;
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

    // Gameplay UI
    public GameObject gamePlayUI;

    // Pop-ups
    public GameObject warningPopUp;

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
    [SerializeField]GameObject introductionPopUp;
    [SerializeField] TextMeshProUGUI weaponText;
    [SerializeField] TextMeshProUGUI introductionText;
    [SerializeField] Image introductionItemImage;

    [HideInInspector] public bool glossaryBookOpen;
    [HideInInspector] public bool turnPageCompleted;
    [HideInInspector] public bool zoomOutFinished;
    [HideInInspector] public bool zoomInFinished;
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
    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    [HideInInspector] public Decoy decoy;
    [HideInInspector] public int exploredRoomCount = 0;

    Coroutine introductionTextRoutine;
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

    protected override void Awake()
    {
        base.Awake();

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
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        //StaticEventHandler.OnDropPickedUp += StaticEventHandler_OnDropPickedUp;
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnDecoySpawned += StaticEventHandler_OnDecoySpawned;
        StaticEventHandler.OnHourglassSpawned += StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired += StaticEventHandler_OnHourglasExpired;

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
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        //StaticEventHandler.OnDropPickedUp -= StaticEventHandler_OnDropPickedUp;
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnDecoySpawned -= StaticEventHandler_OnDecoySpawned;
        StaticEventHandler.OnHourglassSpawned -= StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired -= StaticEventHandler_OnHourglasExpired;

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

    //private void StaticEventHandler_OnDropPickedUp(IntroductionPopUpUIArgs introductionPopUpUIArgs)
    //{
    //    switch (introductionPopUpUIArgs.dropType)
    //    {
    //        case DropType.PassiveItem:
    //            PassiveItem passiveItem = (PassiveItem)introductionPopUpUIArgs.receivable;

    //            if (passiveItem == null) return;

    //            // PRIMARY PASSIVES
    //            if (passiveItem.passiveItemDetails.passiveItemName == "Silver Coin")
    //            {
    //                IntroductionPopUpProcess("SILVER\nCOIN" ,"Coin for buying things.", passiveItem.passiveItemDetails.passiveItemSprite);
    //            }
    //            if (passiveItem.passiveItemDetails.passiveItemName == "Golden Coin")
    //            {
    //                IntroductionPopUpProcess("GOLDEN\nCOIN", "Worth 5 silver coins.", passiveItem.passiveItemDetails.passiveItemSprite);
    //            }
    //            else if (passiveItem.passiveItemDetails.passiveItemName == "Quiver")
    //            {
    //                IntroductionPopUpProcess("QUIVER", "Refills projectiles for bow class weapons.", passiveItem.passiveItemDetails.passiveItemSprite);
    //            }
    //            else if (passiveItem.passiveItemDetails.passiveItemName == "Medicine")
    //            {
    //                IntroductionPopUpProcess("MEDICINE", "Cures basic negative status effects.", passiveItem.passiveItemDetails.passiveItemSprite);
    //            }
    //            else if (passiveItem.passiveItemDetails.passiveItemName == "Holy Water")
    //            {
    //                IntroductionPopUpProcess("HOLY\nWATER", "Cures curse.", passiveItem.passiveItemDetails.passiveItemSprite);
    //            }
    //            else if (passiveItem.passiveItemDetails.passiveItemName == "Health")
    //            {
    //                IntroductionPopUpProcess("HEALTH", "Recovers one heart - 20 hp.", passiveItem.passiveItemDetails.passiveItemSprite);
    //            }
    //            else if (passiveItem.passiveItemDetails.passiveItemName == "Key")
    //            {
    //                IntroductionPopUpProcess("KEY", "You will need it for opening chests.", passiveItem.passiveItemDetails.passiveItemSprite);
    //            }
    //            break;
    //        case DropType.ActiveItem:
    //            break;
    //        case DropType.Weapon:
    //            break;
    //        default:
    //            break;
    //    }
    //}

    //private void IntroductionPopUpProcess(string weaponTextContent, string introductionTextContent, Sprite itemSprite)
    //{
    //    if (introductionTextRoutine == null)
    //    {
    //        introductionTextRoutine = StartCoroutine(IntroductionTextRoutine(weaponTextContent, introductionTextContent, itemSprite));
    //    }
    //    else
    //    {
    //        StopCoroutine(introductionTextRoutine);
    //        introductionTextRoutine = StartCoroutine(IntroductionTextRoutine(weaponTextContent, introductionTextContent, itemSprite));
    //    }
    //}

    //IEnumerator IntroductionTextRoutine(string weaponTextContent, string introductionTextContent, Sprite itemSprite)
    //{
    //    introductionPopUp.SetActive(true);
    //    weaponText.text = weaponTextContent;
    //    introductionText.text = introductionTextContent;
    //    introductionItemImage.sprite = itemSprite;

    //    yield return new WaitForSeconds(4f);

    //    introductionPopUp.SetActive(false);
    //    introductionTextRoutine = null;
    //}

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
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;

        healthBar = healthBarContainer.transform.GetChild(1).GetChild(1).GetChild(0).gameObject;

        bookCover.SetActive(false);
        bookView.SetActive(false);
        warningPopUp.SetActive(false);
        introductionPopUp.SetActive(false);

        // Set screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));

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
        HandleGameState();

        if (EnemySpawner.Instance.isBossInstantiated)
        {
            bossEnemy = EnemySpawner.Instance.GetBoss();
            healthBarContainer.SetActive(true);
            healthBarContainer.GetComponentInChildren<TextMeshProUGUI>().text = bossEnemy.enemyDetails.enemyName;
        }
        else
        {
            healthBarContainer.GetComponentInChildren<TextMeshProUGUI>().text = string.Empty;
            healthBar.transform.localScale = new Vector3(-1f, 1f, 1f);
            healthBarContainer.SetActive(false);
        }

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

    private void HandleBook()
    {
        if (pauseMenu.activeSelf) return;

        if (!bookView.activeSelf) { gamePlayUI.SetActive(true); }

        if (zoomInFinished)
        {
            zoomInFinished = false;
            bookView.GetComponent<Animator>().enabled = false;
            bookView.GetComponent<Animator>().enabled = true;
        }

        if (zoomOutFinished)
        {
            zoomOutFinished = false;
            bookView.GetComponent<Animator>().enabled = false;
            bookView.GetComponent<Animator>().enabled = true;
            bookView.SetActive(false);
            bookCover.SetActive(false);
            glossaryBookOpen = false;
        }

        if (turnPageCompleted)
        {
            bookView.GetComponent<Animator>().SetBool(Settings.turnPage, false);
            turnPageCompleted = false;
            bookView.GetComponent<Animator>().enabled = false;
            bookView.GetComponent<Animator>().enabled = true;
        }

        if (InputManager.Instance.bookView.action.WasPressedThisFrame())
        {
            if (bookView.activeSelf)
            {
                gamePlayUI.SetActive(true);
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
            }
            else
            {
                gamePlayUI.SetActive(false);
                bookView.SetActive(true);
                bookCover.SetActive(true);
                glossaryBookOpen = true;
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                bookView.GetComponent<Animator>().SetTrigger(Settings.zoomIn);
            }
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
        zoomOutFinished = false;
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
                // Play first level
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
    /// Set the current room the player in in
    /// </summary>
    public void SetCurrentRoom(Room room)
    {
        previousRoom = currentRoom;
        currentRoom = room;
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
        }
        else if (gameState == GameState.gamePaused)
        {
            BackFromAudioMenu(); // If inside the audio menu then esc is clicked return to the default pause menu when esc clicked again

            pauseMenu.SetActive(false);
            GetPlayer().playerControl.EnablePlayer();

            // Set game state
            gameState = previousGameState;
            previousGameState = GameState.gamePaused;
        }
    }

    /// <summary>
    /// Called from Audio button
    /// </summary>
    public void OpenAudioMenu()
    {
        // Clear buttons on pause menu
        Transform pauseContainer = pauseMenu.transform.GetChild(0).GetChild(0).GetChild(0);

        for (int i = 0; i < pauseContainer.childCount; i++)
        {
            if (i == 0 || i == 1) continue;

            pauseContainer.GetChild(i).gameObject.SetActive(false);
        }

        // Open audio menu

        pauseContainer.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Audio";
        Transform audioContainer = pauseContainer.GetChild(3);
        audioContainer.GetChild(0).gameObject.SetActive(false); // Disable audio text
        audioContainer.GetChild(1).gameObject.SetActive(true); // Enable music volume contents
        audioContainer.GetChild(2).gameObject.SetActive(true); // Enable sound volume contents
        audioContainer.GetComponent<Image>().enabled = false;
        audioContainer.GetComponent<Button>().enabled = false;
        audioContainer.gameObject.SetActive(true);
    }

    /// <summary>
    /// Called from Back button in Audio menu
    /// </summary>
    public void BackFromAudioMenu()
    {
        Transform pauseContainer = pauseMenu.transform.GetChild(0).GetChild(0).GetChild(0);

        // Close audio menu
        Transform audioContainer = pauseContainer.GetChild(3);
        audioContainer.GetChild(0).gameObject.SetActive(true); // Enable audio text
        audioContainer.GetChild(1).gameObject.SetActive(false); // Disable music volume contents
        audioContainer.GetChild(2).gameObject.SetActive(false); // Disable sound volume contents
        audioContainer.GetComponent<Image>().enabled = true;
        audioContainer.GetComponent<Button>().enabled = true;
        audioContainer.gameObject.SetActive(false);

        for (int i = 0; i < pauseContainer.childCount; i++)
        {
            if (i == 0 || i == 1) continue;

            pauseContainer.GetChild(i).gameObject.SetActive(true);
        }

        pauseContainer.GetChild(1).GetComponent<TextMeshProUGUI>().text = "Options";
    }

    /// <summary>
    /// Called from Play Game button
    /// </summary>
    public void QuitGame()
    {
        SceneManager.LoadScene("MainMenuScene");
    }

    /// <summary>
    /// Called from Exit button
    /// </summary>
    public void ExitGame()
    {
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
        bool dungeonBuiltSucessfully = DungeonBuilder.Instance.GenerateDungeon(dungeonLevelList[dungeonLevelListIndex]);

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

        GetPlayer().playerControl.DisablePlayer();

        string messageText = "LEVEL " + (currentDungeonLevelListIndex + 1).ToString() + "\n\n" + dungeonLevelList[currentDungeonLevelListIndex].
            levelName.ToUpper();

        yield return StartCoroutine(DisplayMessageRoutine(messageText, Color.white, 2f));

        GetPlayer().playerControl.EnablePlayer();

        // Fade In
        yield return StartCoroutine(Fade(1f, 0f, 2f, Color.black));

    }

    /// <summary>
    /// Display the message text for displaySeconds  - if displaySeconds =0 then the message is displayed until the return key is pressed
    /// </summary>
    private IEnumerator DisplayMessageRoutine(string text, Color textColor, float displaySeconds)
    {
        // Set text
        messageTextTMP.SetText(text);
        messageTextTMP.color = textColor;

        // Display the message for the given time
        if (displaySeconds > 0f)
        {
            float timer = displaySeconds;

            while (timer > 0f && !InputManager.Instance.nextLevel.action.WasPerformedThisFrame())
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        // else display the message until the return button is pressed
        {
            while (!InputManager.Instance.nextLevel.action.WasPerformedThisFrame())
            {
                yield return null;
            }
        }

        yield return null;

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

        // Display level completed
        yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + player.playerDetails.playerCharacterName + "! \n\nYOU'VE SURVIVED THIS DUNGEON " +
            "LEVEL", Color.white, 5f));

        // Fade out canvas
        yield return StartCoroutine(Fade(1f, 0f, 2f, new Color(0f, 0f, 0f, 0.4f)));

        // Increase index to next level
        currentDungeonLevelListIndex++;

        PlayDungeonLevel(currentDungeonLevelListIndex);
    }

    /// <summary>
    /// Fade Canvas Group
    /// </summary>
    public IEnumerator Fade(float startFadeAlpha, float targetFadeAlpha, float fadeSeconds, Color backgroundColor)
    {
        isFading = true;
        Image image = canvasGroup.GetComponent<Image>();
        image.color = backgroundColor;

        float time = 0;

        while (time <= fadeSeconds)
        {
            time += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(startFadeAlpha, targetFadeAlpha, time / fadeSeconds);
            yield return null;
        }

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

        // Display game won
        yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + player.playerDetails.playerCharacterName + "! YOU HAVE DEFEATED THE DUNGEON", 
            Color.white, 3f));

        yield return StartCoroutine(DisplayMessageRoutine("PRESS ENTER TO RESTART THE GAME", Color.white, 0f));

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
        Enemy[] enemyArray = FindObjectsOfType<Enemy>();
        foreach (Enemy enemy in enemyArray)
        {
            enemy.gameObject.SetActive(false);
        }

        // Display game lost
        yield return StartCoroutine(DisplayMessageRoutine("BAD LUCK " + player.playerDetails.playerCharacterName + 
            "! YOU HAVE SUCCUMBED TO THE DUNGEON", Color.white, 2f));

        yield return StartCoroutine(DisplayMessageRoutine("PRESS ENTER TO RESTART THE GAME", Color.white, 0f));

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
    /// Set health bar value with health percent between 0 and 1
    /// </summary>
    public void SetHealthBarValue(float healthPercent, Enemy enemy)
    {
        if (enemy != null)
        {
            if (enemy.enemyDetails.isEnemyBoss)
            {
                healthBar.transform.localScale = new Vector3(healthPercent * -1f, 1f, 1f);

                if (enemy.health.GetCurrentHealth() <= 0f)
                {
                    healthBar.transform.localScale = new Vector3(0f, 1f, 1f);
                }
            }
        }
    }

    public void GoToWeaponSetWithIndex(int setIndex)
    {
        player.playerControl.NextWeaponSet(true, false, setIndex);
    }

    public void WeaponSetOne()
    {
        player.playerControl.NextWeaponSet(true, false, 1);
    }

    public void WeaponSetTwo()
    {
        player.playerControl.NextWeaponSet(true, false, 2);
    }

    public void WeaponSetThree()
    {
        player.playerControl.NextWeaponSet(true, false, 3);
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
            default:
                break;
        }
    }

    public void UpdateTooltipPanelInfo(IReceivable receivable, bool hasWeaponDrop, bool hasActiveDrop, bool hasSecondaryPassiveDrop)
    {
        tooltipPanel.SetActive(true);
        ClearTooltipPanel();

        if (hasSecondaryPassiveDrop)
        {
            if (receivable is PassiveItem)
            {
                headerText.colorGradient = new VertexGradient(passiveItemColor, passiveItemColor, passiveItemColor, passiveItemColor);
                levelText.colorGradient = new VertexGradient(passiveItemColor, passiveItemColor, passiveItemColor, passiveItemColor);
                PassiveItem passiveItem = (PassiveItem)receivable;
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
                    weaponClassText.text = "+1 Stength";
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
            if (receivable is ActiveItem)
            {
                headerText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                levelText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                ActiveItem activeItem = (ActiveItem)receivable;
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
                    hitSpeedText.text = "AoE DamageFor Stunning Enemies";
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
            if (receivable is Weapon)
            {
                Weapon weapon = (Weapon)receivable;
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

        if ((requiredStats.strength > 0 && player.currentStrengthValue< requiredStats.strength) ||
            (requiredStats.dexterity > 0 && player.currentDexterityValue < requiredStats.dexterity) ||
            (requiredStats.constitution > 0 && player.currentConstitutionValue < requiredStats.constitution) ||
            (requiredStats.intelligence > 0 && player.currentIntelligenceValue < requiredStats.intelligence) ||
            (requiredStats.agility > 0 && player.currentAgilityValue < requiredStats.agility))
        {
            requirementText.color = Color.red;
        }
        else
        {
            requirementText.color = Color.green;
        }

        return requirementString;
    }

    private void ClearTooltipPanel()
    {
        foreach (Transform child in tooltipPanel.transform)
        {
            child.GetComponent<TextMeshProUGUI>().text = string.Empty;
        }
    }

    public void CloseTooltipPanel()
    {
        tooltipPanel.SetActive(false);
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
