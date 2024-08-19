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
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI messageTextTMP;
    #region Tooltip
    [Tooltip("Populate with the FadeImage canvasgroup component in the FadeScreenUI")]
    #endregion Tooltip
    [SerializeField] CanvasGroup canvasGroup;
    #region Tooltip
    [Tooltip("Populate with the Post processing volume")]
    #endregion Tooltip
    [SerializeField] Volume volume;

    // Book members
    public GameObject bookView;
    public GameObject bookCover;
    public GameObject warningPopUp;
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

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;
    [HideInInspector] public Decoy decoy;
    [HideInInspector] public int exploredRoomCount = 0;
    [HideInInspector] public ChestItem toBeDroppedChestItem;

    const int ROOM_CONST = 6;
    Room currentRoom;
    Room previousRoom;
    PlayerDetailsSO playerDetails;
    Player player;
    InstantiatedRoom bossRoom;
    bool isFading = false;
    Vignette vignette;
    HashSet<Room> visitedRooms = new HashSet<Room>();

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
        StaticEventHandler.OnRoomEnemiesDefeated += StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnDecoySpawned += StaticEventHandler_OnDecoySpawned;
        StaticEventHandler.OnHourglassSpawned += StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired += StaticEventHandler_OnHourglasExpired;
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
        StaticEventHandler.OnRoomEnemiesDefeated -= StaticEventHandler_OnRoomEnemiesDefeated;
        StaticEventHandler.OnDecoySpawned -= StaticEventHandler_OnDecoySpawned;
        StaticEventHandler.OnHourglassSpawned -= StaticEventHandler_OnHourglassSpawned;
        StaticEventHandler.OnHourglasExpired -= StaticEventHandler_OnHourglasExpired;
        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;

        if (InputManager.Instance  != null)
        {
            InputManager.Instance.overviewMapFullView.action.started -= ControlDisplayDungeonOverviewMap;
            InputManager.Instance.overviewMapFullView.action.canceled -= ControlClearDungeonOverviewMap;
        }
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

            default:

                if (!visitedRooms.Contains(currentRoom))
                {
                    if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge ==
                        player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemMaxCharge) return;

                    int refreshedCharge = (int)(player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemChargeRegenerationPerSixRooms *
                        ++exploredRoomCount / ROOM_CONST);

                    if (refreshedCharge >= 1)
                    {
                        player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge += refreshedCharge;

                        if (player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge >
                            player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemMaxCharge)
                        {
                            player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge =
                                player.selectedActiveItem.GetCurrentActiveItem().activeItemDetails.activeItemMaxCharge;
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

        bookCover.SetActive(false);
        bookView.SetActive(false);
        warningPopUp.SetActive(false);

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
        HandleGameState();
    }

    private void HandleBook()
    {
        if (pauseMenu.activeSelf) return;

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
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                bookView.GetComponent<Animator>().SetTrigger(Settings.zoomOut);
            }
            else
            {
                bookView.SetActive(true);
                bookCover.SetActive(true);
                glossaryBookOpen = true;
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.closeBookSoundEffect);
                bookView.GetComponent<Animator>().SetTrigger(Settings.zoomIn);
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
                    PauseGameMenu();
                }

                if (InputManager.Instance.overviewMapFullView.action.WasPressedThisFrame())
                {
                    DisplayDungeonOverviewMap();
                }
                break;

            case GameState.engagingEnemies:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    PauseGameMenu();
                }
                break;

            case GameState.engagingBoss:
                if (InputManager.Instance.pause.action.WasPressedThisFrame())
                {
                    PauseGameMenu();
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
                    PauseGameMenu();
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
            pauseMenu.SetActive(false);
            GetPlayer().playerControl.EnablePlayer();

            // Set game state
            gameState = previousGameState;
            previousGameState = GameState.gamePaused;
        }
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

        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART THE GAME", Color.white, 0f));

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

        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART THE GAME", Color.white, 0f));

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

    public void SetToBeDroppedChestItem(ChestItem chestItem) 
    {
        toBeDroppedChestItem = chestItem;
    }

    public ChestItem GetToBeDroppedChestItem()
    {
        return toBeDroppedChestItem;
    }

    public void GoToWeaponSetWithIndex(int setIndex)
    {
        player.playerControl.NextWeaponSet(false, false, setIndex);
    }

    public void WeaponSetOne()
    {
        player.playerControl.NextWeaponSet(false, false, 1);
    }

    public void WeaponSetTwo()
    {
        player.playerControl.NextWeaponSet(false, false, 2);
    }

    public void WeaponSetThree()
    {
        player.playerControl.NextWeaponSet(false, false, 3);
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
                warningText.text = "Equipped main hand weapon can't be less than 1 in 3 sets";
                break;
            case PopUpReason.DontHaveWeaponOnSelectedSet:
                warningText.text = "Can't switch to next set because there is no weapon";
                break;
            case PopUpReason.OffHandFull:
                warningText.text = "You can't drop main weapon. Active weapon set's off-hand is full";
                break;
            case PopUpReason.ShieldCantBePutOnMainHand:
                warningText.text = "Shield can not be equipped on the main hand";
                break;
            case PopUpReason.OffHandCantBeAddedToTwoHanded:
                warningText.text = "Off-hand weapon can't be added while main hand has a two-handed weapon";
                break;
            default:
                break;
        }
    }

    public Transform GetMainHandEquippedSlot()
    {
        return bookView.transform.GetChild(1).GetChild(3).GetChild(1).GetChild(1);
    }
    
    public Transform GetOffHandEquippedSlot()
    {
        return bookView.transform.GetChild(1).GetChild(4).GetChild(1).GetChild(1);
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
