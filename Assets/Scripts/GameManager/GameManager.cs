using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class GameManager : SingletonMonobehaviour<GameManager>
{
    #region Header GAMEOBJECT REFERENCES
    [Space(10)]
    [Header("GAMEOBJECT REFERENCES")]
    #endregion Header GAMEOBJECT REFERENCES
    #region Tooltip
    [Tooltip("Populate with the MessageText textmeshpro component in the FadeScreenUI")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI messageTextTMP;
    #region Tooltip
    [Tooltip("Populate with the FadeImage canvasgroup component in the FadeScreenUI")]
    #endregion Tooltip
    [SerializeField] CanvasGroup canvasGroup;

    #region Header DUNGEON LEVELS
    [Space(10)]
    [Header("DUNGEON LEVELS")]
    #endregion Header DUNGEON LEVELS
    #region Tooltip
    [Tooltip("Populate with the dungeon level scriptable objects")]
    #endregion Tooltip
    [SerializeField] List<DungeonLevelSO> dungeonLevelList;
    #region Tooltip
    [Tooltip("Populate with the starting dungeon level for testing , first level = 0")]
    #endregion Tooltip
    [SerializeField] int currentDungeonLevelListIndex = 0;

    Room currentRoom;
    Room previousRoom;
    PlayerDetailsSO playerDetails;
    Player player;
    bool isFading = false;

    [HideInInspector] public GameState gameState;
    [HideInInspector] public GameState previousGameState;

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
        player.destroyedEvent.OnDestroyed += Player_OnDestroyed;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        player.destroyedEvent.OnDestroyed -= Player_OnDestroyed;
    }

    /// <summary>
    /// Handle room changed event
    /// </summary>
    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        SetCurrentRoom(roomChangedEventArgs.room);
<<<<<<< Updated upstream
=======

        if (decoy != null)
        {
            Destroy(decoy.gameObject);
        }

        if (Player.hasClone)
        {
            Destroy(player.playerCloneObject);
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

    private void StaticEventHandler_OnDropPickedUp(IntroductionPopUpUIArgs introductionPopUpUIArgs)
    {
        switch (introductionPopUpUIArgs.dropType)
        {
            case DropType.PassiveItem:
                PassiveItem passiveItem = (PassiveItem)introductionPopUpUIArgs.receivable;

                if (passiveItem == null) return;

                // PRIMARY PASSIVES
                if (passiveItem.passiveItemDetails.passiveItemName == "Silver Coin")
                {
                    IntroductionPopUpProcess("SILVER\nCOIN" ,"Coin for buying things.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                if (passiveItem.passiveItemDetails.passiveItemName == "Golden Coin")
                {
                    IntroductionPopUpProcess("GOLDEN\nCOIN", "Worth 5 silver coins.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemName == "Quiver")
                {
                    IntroductionPopUpProcess("QUIVER", "Refills projectiles for bow class weapons.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemName == "Medicine")
                {
                    IntroductionPopUpProcess("MEDICINE", "Cures basic negative status effects.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemName == "Holy Water")
                {
                    IntroductionPopUpProcess("HOLY\nWATER", "Cures curse.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemName == "Health")
                {
                    IntroductionPopUpProcess("HEALTH", "Recovers one heart - 20 hp.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemName == "Key")
                {
                    IntroductionPopUpProcess("KEY", "You will need it for opening chests.", passiveItem.passiveItemDetails.passiveItemSprite);
                }

                // SECONDARY PASSIVES
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                {
                    IntroductionPopUpProcess("BELT OF\nSORCERY", "Shiny look.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfFortune)
                {
                    IntroductionPopUpProcess("RING OF\nFORTUNE", "More drop chance.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    IntroductionPopUpProcess("SHADOW\nCLOAK", "More critical chance.", passiveItem.passiveItemDetails.passiveItemSprite);
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
                {
                    IntroductionPopUpProcess("WARDEN OF\nFOREST", "More projectile accuracy.", passiveItem.passiveItemDetails.passiveItemSprite);
                }

                break;
            case DropType.ActiveItem:
                ActiveItem activeItem = (ActiveItem)introductionPopUpUIArgs.receivable;

                if (activeItem == null) return;

                if (activeItem.activeItemDetails.activeItemName == "Bobby Pin")
                {
                    IntroductionPopUpProcess("BOBBY PIN", "Can open chest without key.. sometimes.", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Bomb")
                {
                    IntroductionPopUpProcess("BOMB", "Obvious. It's a bomb. No more no less.", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Boomerang")
                {
                    IntroductionPopUpProcess("BOOMERANG", "Strike and return.", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Chronos Hourglass")
                {
                    IntroductionPopUpProcess("CHRONOS\nHOURGLASS", "Slow down timeflow.", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Dummy")
                {
                    IntroductionPopUpProcess("DUMMY", "Throw and distract mobs.", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Elysian Elixir")
                {
                    IntroductionPopUpProcess("ELYSIAN\nELIXIR", "Slowly regenates health.", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Oracle's Compass")
                {
                    IntroductionPopUpProcess("ORACLE'S\nCOMPASS", "Shows the directin of where the boss is.", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Pentagram")
                {
                    IntroductionPopUpProcess("PENTAGRAM", "Throw then wait for mobs to step on", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Shiruken")
                {
                    IntroductionPopUpProcess("SHIRUKEN", "Limited number of ninja star projectiles", activeItem.activeItemDetails.activeItemSprite);
                }
                else if (activeItem.activeItemDetails.activeItemName == "Bronze Summoner")
                {
                    IntroductionPopUpProcess("BRONZE\nSUMMONER", "Summons a lower class of ally mob", activeItem.activeItemDetails.activeItemSprite);
                }

                break;
            case DropType.Weapon:
                Weapon weapon = (Weapon)introductionPopUpUIArgs.receivable;

                if (weapon.weaponDetails == null) return;

                if (weapon.weaponDetails.weaponTitle == WeaponTitle.Carnage)
                {
                    IntroductionPopUpProcess("CARNAGE", "A two-handed axe.", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Hatchet)
                {
                    IntroductionPopUpProcess("HATCHET", "Basic one-handed axe.", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Dirk)
                {
                    IntroductionPopUpProcess("DIRK", "Basic dagger.", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Gambit)
                {
                    IntroductionPopUpProcess("GAMBIT", "A critical effective dagger.", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.ClobberingTime)
                {
                    IntroductionPopUpProcess("CLOBBERING\nTIME", "A kind of one-hand hammer. Can stun mobs", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Crusher)
                {
                    IntroductionPopUpProcess("CRUSHER", "A stunning two-handed hammer", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.PhalanxSpear)
                {
                    IntroductionPopUpProcess("PHALANX\nSPEAR", "A basic spear", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Gladius)
                {
                    IntroductionPopUpProcess("GLADIUS", "Basic one-handed sword", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Scimitar)
                {
                    IntroductionPopUpProcess("SCIMITAR", "Can't pierce but effective", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.SizzlingSword)
                {
                    IntroductionPopUpProcess("SIZZLING\nSWORD", "Can acidify mobs", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.AncientKatana)
                {
                    IntroductionPopUpProcess("ANCIENT\nKATANA", "Two-handed sword which can have instant death on mobs directly", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.HolySword)
                {
                    IntroductionPopUpProcess("HOLY\nSWORD", "Sword of the light.. Especially well against undeads", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Bow)
                {
                    IntroductionPopUpProcess("BOW", "A basic bow", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Crossbow)
                {
                    IntroductionPopUpProcess("CROSSBOW", "Takes time to load but more deadly than bow", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Staff)
                {
                    IntroductionPopUpProcess("STAFF", "Ordinary staff but no precharge time to fire", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.HeavensGale)
                {
                    IntroductionPopUpProcess("HEAVEN'S\nGALE", "Fires patterned light projectiles.. Especially effective against undeads", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.SolarFlare)
                {
                    IntroductionPopUpProcess("SOLAR\nFLARE", "Fires dispersed fire projectiles.. Especially effective against vermins", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.Shield)
                {
                    IntroductionPopUpProcess("SHIELD", "Basic shield", weapon.weaponDetails.weaponFrontSprite);
                }
                else if (weapon.weaponDetails.weaponTitle == WeaponTitle.ApolloShield)
                {
                    IntroductionPopUpProcess("APOLLO\nSHIELD", "High-deflect rate from projectiles", weapon.weaponDetails.weaponFrontSprite);
                }
                break;
            default:
                break;
        }
    }

    private void IntroductionPopUpProcess(string weaponTextContent, string introductionTextContent, Sprite itemSprite)
    {
        if (introductionTextRoutine == null)
        {
            introductionTextRoutine = StartCoroutine(IntroductionTextRoutine(weaponTextContent, introductionTextContent, itemSprite));
        }
        else
        {
            StopCoroutine(introductionTextRoutine);
            introductionTextRoutine = StartCoroutine(IntroductionTextRoutine(weaponTextContent, introductionTextContent, itemSprite));
        }
    }

    IEnumerator IntroductionTextRoutine(string weaponTextContent, string introductionTextContent, Sprite itemSprite)
    {
        introductionPopUp.SetActive(true);
        weaponText.text = weaponTextContent;
        introductionText.text = introductionTextContent;
        introductionItemImage.sprite = itemSprite;

        yield return new WaitForSeconds(4f);

        introductionPopUp.SetActive(false);
        introductionTextRoutine = null;
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
>>>>>>> Stashed changes
    }

    /// <summary>
    /// Handle player destroyed event
    /// </summary>
    private void Player_OnDestroyed(DestroyedEvent destroyedEvent, DestroyedEventArgs destroyedEventArgs)
    {
        previousGameState = gameState;
        gameState = GameState.gameLost;
    }

    private void Start()
    {
        previousGameState = GameState.gameStarted;
        gameState = GameState.gameStarted;

        // Set screen to black
        StartCoroutine(Fade(0f, 1f, 0f, Color.black));
    }

    private void Update()
    {
        HandleGameState();
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
                break;

            // While playing the level handle the tab key for the dungeon overview map.
            case GameState.playingLevel:

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
                }
                break;

            // if in the dungeon overview map handle the release of the tab key to clear the map
            case GameState.dungeonOverviewMap:

                // Key released
                if (Input.GetKeyUp(KeyCode.Tab))
                {
                    // Clear dungeonOverviewMap
                    DungeonMap.Instance.ClearDungeonOverViewMap();
                }
                break;

            // While playing the level and before the boss is engaged, handle the tab key for the dungeon overview map.
            case GameState.bossStage:

                if (Input.GetKeyDown(KeyCode.Tab))
                {
                    DisplayDungeonOverviewMap();
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
    /// Dungeon Map Screen Display
    /// </summary>
    private void DisplayDungeonOverviewMap()
    {
        // return if fading
        if (isFading)
            return;

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

            while (timer > 0f && !Input.GetKeyDown(KeyCode.Return))
            {
                timer -= Time.deltaTime;
                yield return null;
            }
        }
        else
        // else display the message until the return button is pressed
        {
            while (!Input.GetKeyDown(KeyCode.Return))
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

        Debug.Log("Level Completed - Press Return To Progress To The Next Level");

        // When player presses the return key proceed to the next level
        while (!Input.GetKeyDown(KeyCode.Return))
        {
            yield return null;
        }

        yield return null; // to avoid enter being detected twice

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
        yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + GameResources.Instance.currentPlayer.playerName + "! YOU HAVE DEFEATED THE DUNGEON", 
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
        yield return StartCoroutine(DisplayMessageRoutine("BAD LUCK " + GameResources.Instance.currentPlayer.playerName + "! YOU HAVE SUCCUMBED TO THE DUNGEON", 
            Color.white, 2f));

        yield return StartCoroutine(DisplayMessageRoutine("PRESS RETURN TO RESTART THE GAME", Color.white, 0f));

        // Set game state to restart game
        gameState = GameState.restartGame;
    }

    /// <summary>
    /// Restart the game
    /// </summary>
    private void RestartGame()
    {
        SceneManager.LoadScene("MainGameScene");
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

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(messageTextTMP), messageTextTMP);
        HelperUtilities.ValidateCheckNullValue(this, nameof(canvasGroup), canvasGroup);
        HelperUtilities.ValidateCheckEnumerableValues(this, nameof(dungeonLevelList), dungeonLevelList);
    }
#endif
    #endregion Validation
}
