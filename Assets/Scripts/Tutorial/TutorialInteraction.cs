using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class TutorialInteraction : SingletonMonobehaviour<TutorialInteraction>
{
    Player player;

    public List<Dialogue> dialogues;

    Animator parchmentAnimator;
    Coroutine questTextProcessRoutine;

    [HideInInspector] public TutorialPhase currentTutorialPhase;
    [HideInInspector] public TutorialProcess currentTutorialProcess = TutorialProcess.Starting;

    [SerializeField] TextMeshProUGUI headerText;
    [SerializeField] TextMeshProUGUI dialogueText;

    [Space(10)]
    [Header("QUEST")]
    [SerializeField] Transform questBarContainer;
    [Space(10)]

    [SerializeField] Sprite questCompletedSprite;
    [SerializeField] Sprite questNotCompletedSprite;

    // Fade
    [Space(10)]
    #region Tooltip
    [Tooltip("Populate with the MessageText textmeshpro component in the FadeScreenUI")]
    #endregion
    [SerializeField] TextMeshProUGUI messageTextTMP;
    #region Tooltip
    [Tooltip("Populate with the FadeImage canvasgroup component in the FadeScreenUI")]
    #endregion Tooltip
    [SerializeField] CanvasGroup canvasGroup;
    bool isFading;

    public Queue<string> sentences;

    bool dialogueStarted = false;
    bool clicked = false;
    bool isTyping = false;
    bool isCheckPlayed = false;
    bool isNextPhaseStartedSoundPlayed = false;
    bool passingToNextQuestOnProcess = false;
    bool onStart = true;

    // Movement
    private bool movedUp;
    private bool movedDown;
    private bool movedLeft;
    private bool movedRight;

    PlayerDetailsSO[] playerDetailsList;
    CurrentPlayerSO currentPlayer;
    int selectedPlayerIndex = 1;

    protected override void Awake()
    {
        base.Awake();

        playerDetailsList = GameResources.Instance.playerDetailsArray;
        currentPlayer = GameResources.Instance.currentPlayer;
    }

    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
        currentTutorialPhase = TutorialPhase.TutorialIntro;
        parchmentAnimator = GetComponentInChildren<Animator>();

        sentences = new Queue<string>();

        questBarContainer.gameObject.SetActive(false);
        questTextProcessRoutine = StartCoroutine(DialogueTextProcess());
    }

    private void Update()
    {
        switch (currentTutorialProcess)
        {
            case TutorialProcess.Starting:
                if (dialogueStarted && (InputManager.Instance.OKButton.action.WasPressedThisFrame() || InputManager.Instance.escapeButton.action.WasPressedThisFrame() ||
                    InputManager.Instance.attack.action.WasPressedThisFrame()))
                {
                    if (isTyping)
                    {
                        clicked = true; // Will finish typing current sentence
                    }
                    else
                    {
                        DisplayNextSentence(); // Will move to next sentence
                    }
                }
                break;
            case TutorialProcess.QuestDisplayed:
                int indexValue = (int)currentTutorialPhase;

                if (indexValue <= (int)TutorialPhase.FinishTutorial)
                {
                    TutorialPhasePassProcess();
                }

                break;
            case TutorialProcess.QuestPassed:
                if (questTextProcessRoutine == null)
                {
                    isNextPhaseStartedSoundPlayed = false;

                    if (!passingToNextQuestOnProcess)
                    {
                        StartCoroutine(NextPhaseProcess());
                    }
                }
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Trigger to start a dialogue
    /// </summary>
    public void StartDialogue(Dialogue dialogue)
    {
        if (!dialogueStarted)
        {
            sentences.Clear();
            dialogueStarted = true;

            foreach (string sentence in dialogue.sentences)
            {
                sentences.Enqueue(sentence);
            }
        }

        DisplayNextSentence();
    }

    /// <summary>
    /// Start next sentence
    /// </summary>
    public void DisplayNextSentence()
    {
        if (sentences.Count == 0)
        {
            EndDialogue();
            return;
        }

        string sentence = sentences.Dequeue();
        StopAllCoroutines();
        StartCoroutine(TypeSentence(sentence));
        clicked = false;
    }

    IEnumerator TypeSentence(string sentence)
    {
        dialogueText.text = "";
        isTyping = true;

        foreach (char letter in sentence.ToCharArray())
        {
            if (clicked)
            {
                dialogueText.text = sentence;
                break;
            }

            dialogueText.text += letter;
            yield return new WaitForSeconds(0.05f);
        }

        isTyping = false;
        clicked = false;
    }

    /// <summary>
    /// Finalize the dialogue
    /// </summary>
    void EndDialogue()
    {
        dialogueStarted = false;
        dialogueText.text = "";
        clicked = false;

        parchmentAnimator.SetTrigger("closing");

        currentTutorialPhase++;
        StartCoroutine(DialogueTextProcess());
        currentTutorialProcess = TutorialProcess.QuestDisplayed;
    }

    IEnumerator DialogueTextProcess()
    {
        TextMeshProUGUI questText = questBarContainer.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        Image questImage = questBarContainer.GetChild(0).GetChild(1).GetComponent<Image>();

        questImage.enabled = false;

        if (!onStart)
        {
            parchmentAnimator.SetTrigger("closing");
        }

        // Wait until animator reaches "Idle" state after closing
        yield return new WaitUntil(() => parchmentAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

        parchmentAnimator.SetTrigger("opening");
        headerText.text = string.Empty;
        questText.text = string.Empty;

        // Wait until animator reaches "Idle" state after closing
        yield return new WaitUntil(() => parchmentAnimator.GetCurrentAnimatorStateInfo(0).IsName("Idle"));

        questImage.enabled = true;
        headerText.text = Regex.Replace(currentTutorialPhase.ToString(), "(\\B[A-Z])", " $1");

        string keyboardBinding = string.Empty;
        string gamepadBinding = string.Empty;

        int seed = Random.Range(int.MinValue, int.MaxValue);
        WartheonRNG rng = new WartheonRNG(seed);

        switch (currentTutorialPhase)
        {
            case TutorialPhase.TutorialIntro:
                StartDialogue(dialogues[0]);
                break;
            case TutorialPhase.Move:
                onStart = false;
                questBarContainer.gameObject.SetActive(true);
                isCheckPlayed = false;

                SoundAndImageTrigger(questImage);

                questText.text = "Use up/right/down /left direction keys.\n\nIt is W/A/S/D buttons for keyboard.\n\nLeft Stick for gamepad.";
                break;
            case TutorialPhase.PickUpWeapon:
                isCheckPlayed = false;
                InputManager.interactionDisabled = false;

                keyboardBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Use interaction key for picking-up items & interacting with NPC.\n\nIt is " + keyboardBinding + " for keyboard.\n\n" +
                    gamepadBinding + " button for gamepad.";

                // Initialize drop
                Weapon weapon = WeaponDropGenerator.CreateRolledInstance(player.playerDetails.startingWeapon, rng);

                WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

                DropItem dropItem = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<DropItem>(true);
                dropItem.gameObject.SetActive(true);

                dropItem.hasWeaponDrop = true;
                dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, dropItem.transform.position, null, true);
                break;
            case TutorialPhase.AimAndFire:
                isCheckPlayed = false;
                InputManager.firingDisabled = false;

                keyboardBinding = InputManager.Instance.attack.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.attack.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Aim your weapon with Mouse Cursor for mouse and Right Stick for gamepad.\n\nAim and use attack key for firing your weapon at Dummy. " +
                    "Based on your character, it can act as ranged or melee weapon.\n\nIt is " + keyboardBinding + " for mouse.\n\n" + gamepadBinding + " button for gamepad.";
                break;
            case TutorialPhase.OpenGlossaryBook:
                isCheckPlayed = false;
                InputManager.glossaryDisabled = false;

                keyboardBinding = InputManager.Instance.bookView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.bookView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Now check your glossary you have. It contains many info regarding your journey. First opening page show your Equipped Items and Stats\n\nPress " 
                    + keyboardBinding + " for keyboard.\n\n" + gamepadBinding + " button for gamepad.";
                break;
            case TutorialPhase.MinimapCheck:
                isCheckPlayed = false;

                SoundAndImageTrigger(questImage);

                questText.text = "You can see minimap at the top-right of the screen.";

                break;
            case TutorialPhase.OverviewMapCheck:
                isCheckPlayed = false;
                InputManager.overviewMapDisabled = false;

                keyboardBinding = InputManager.Instance.overviewMapFullView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.overviewMapFullView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Use overview map button to see the map for larger view than minimap.\n\nPress " + keyboardBinding + 
                    " for keyboard.\n\n" + gamepadBinding + " button for gamepad.";

                break;
            case TutorialPhase.SecondRoom:
                isCheckPlayed = false;

                GameManager.Instance.GetCurrentRoom().instantiatedRoom.UnlockDoors(1f);

                SoundAndImageTrigger(questImage);

                questText.text = "First door's unlocked. You can go to the second room passing through the corrior.";

                break;
            case TutorialPhase.Combat:
                isCheckPlayed = false;

                keyboardBinding = InputManager.Instance.attack.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.attack.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "It's time to fight.\n\nKill the spawned enemy mob.";

                break;
            case TutorialPhase.PickUpPrimaryPassiveHealth:
                isCheckPlayed = false;

                keyboardBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Get close to the pick-up primary passive item. You don't need to use interaction button for these type of items. That one is health item" +
                    " and restores your hp by 20 immediately.";

                dropItem = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<DropItem>(true);
                dropItem.gameObject.SetActive(true);

                // Initialize drop
                PassiveItem passiveItem = new PassiveItem(GameResources.Instance.healthPassiveItem.rarity);

                dropItem.hasPrimaryPassiveDrop = true;
                dropItem.Initialize(passiveItem, GameResources.Instance.healthPassiveItem.passiveItemSprite, dropItem.transform.position, null, true);
                break;
            case TutorialPhase.PickUpPrimaryPassiveCoin:
                isCheckPlayed = false;

                keyboardBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Pick-up second primary passive item. That one is silver coin. You can use it buying things or playing gamble at NPCs.";

                dropItem = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<DropItem>(true);
                dropItem.gameObject.SetActive(true);

                // Initialize drop
                PassiveItem secondPassiveItem = new PassiveItem(GameResources.Instance.coinPassiveItem.rarity);

                dropItem.hasPrimaryPassiveDrop = true;
                dropItem.Initialize(secondPassiveItem, GameResources.Instance.coinPassiveItem.passiveItemSprite, dropItem.transform.position, null, true);
                break;
            case TutorialPhase.Parry:
                isCheckPlayed = false;
                InputManager.parryDisabled = false;
                player.health.isDamageable = true;

                keyboardBinding = InputManager.Instance.parryButton.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.parryButton.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "At this fight, timing is the key so beware. Use parry key to deflect enemy attack while mob is contacting" +
                    " you. Parry only works at melee weapons. Timing is very important. Maybe needs a little practice to master it.\n\nUse" + keyboardBinding + " for keyboard.\n\nUse " +
                    gamepadBinding + " for gampepad.";

                StartCoroutine(EnemySpawner.Instance.SpawnEnemiesRoutine(rng));

                MusicManager.Instance.PlayMusic(GameResources.Instance.combatMusic);

                break;
            case TutorialPhase.DodgeRoll:
                isCheckPlayed = false;
                InputManager.dodgeRollDisabled = false;

                keyboardBinding = InputManager.Instance.jumpButton.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.jumpButton.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Now another mob spawns. This one can fire projectiles. Parry doesn't work on projectiles but you can dodge from it by rolling." +
                    "\n\nUse " + keyboardBinding + " for keyboard.\n\nUse " + gamepadBinding + " for gampepad.";

                StartCoroutine(EnemySpawner.Instance.SpawnEnemiesRoutine(rng));

                break;
            case TutorialPhase.SpecialSkill:
                isCheckPlayed = false;
                InputManager.specialSkillOneDisabled = false;
                InputManager.specialSkillTwoDisabled = false;
                InputManager.specialSkillThreeDisabled = false;

                keyboardBinding = InputManager.Instance.specialMoveOne.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.specialMoveOne.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                string keyboardBindingTwo = InputManager.Instance.specialMoveTwo.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                string gamepadBindingTwo = InputManager.Instance.specialMoveTwo.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                string keyboardBindingThree = InputManager.Instance.specialMoveThree.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                string gamepadBindingThree = InputManager.Instance.specialMoveThree.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Now it is time to use your special skills. Each char has his/her unique 3 skills. Use them wisely. When used it will have cooldown" +
                    "so you won't be able to use it again for a while." +"\n\nUse " + keyboardBinding + ", " + keyboardBindingTwo + ", " + keyboardBindingThree +" for keyboard.\n\nUse " 
                    + gamepadBinding + ", " + gamepadBindingTwo + ", " + gamepadBindingThree + " for gampepad.";

                StartCoroutine(EnemySpawner.Instance.SpawnEnemiesRoutine(rng));
                break;
            case TutorialPhase.KillEmAll:
                isCheckPlayed = false;

                SoundAndImageTrigger(questImage);

                questText.text = "Kill all mobs in the room.";
                break;
            case TutorialPhase.PickUpSecondaryPassive:
                isCheckPlayed = false;

                keyboardBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.interaction.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Use interaction key for picking-up an secondary passive item. These passives are equipped at one of your body part. You can check glossary" +
                    "stats page for more details.\n\nIt is " + keyboardBinding + " for keyboard.\n\n" + gamepadBinding + " button for gamepad.";

                dropItem = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<DropItem>(true);
                dropItem.gameObject.SetActive(true);

                // Initialize drop
                passiveItem = PassiveDropGenerator.CreateRolledInstance(GameResources.Instance.secondaryPassiveItem, rng);

                dropItem.hasSecondaryPassiveDrop = true;
                dropItem.Initialize(passiveItem, GameResources.Instance.secondaryPassiveItem.passiveItemSprite, dropItem.transform.position, null, true);

                break;
            case TutorialPhase.SkillsPage:
                isCheckPlayed = false;

                keyboardBinding = InputManager.Instance.bookView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.bookView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "Now you have leveled-up. At every level-up, you will get one skill points. Open glossary books skill page to utilize it by either improving " +
                    "skill (one of 5 skills) or opening a inner-path \n\nIt is " + keyboardBinding + " for keyboard.\n\n" + gamepadBinding + " button for gamepad.";
                break;

            case TutorialPhase.WeaponSetSwitch:
                isCheckPlayed = false;
                InputManager.switchDisabled = false;

                keyboardBinding = InputManager.Instance.switchWeaponBack.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                keyboardBindingTwo = InputManager.Instance.switchWeaponForward.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));

                gamepadBinding = InputManager.Instance.switchWeaponBack.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));
                gamepadBindingTwo = InputManager.Instance.switchWeaponForward.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "You have three weapon sets as you can check glossary. At start, you use one weapon set actively as you have only one weapon. But during the game, " +
                    "you can place new drop weapons to the other set slots as a reserve.\n\nTo switch between weapon set, you can use " + keyboardBinding + " and " + keyboardBindingTwo + 
                    " for keyboard (You can use also mouse wheel).\n\n" + gamepadBinding + " and " + gamepadBindingTwo + " buttons for gamepad.";

                break;
            case TutorialPhase.OtherCollectionsPage:
                isCheckPlayed = false;

                keyboardBinding = InputManager.Instance.bookView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Keyboard&Mouse"));
                gamepadBinding = InputManager.Instance.bookView.action.GetBindingDisplayString(InputBinding.MaskByGroup("Gamepad"));

                SoundAndImageTrigger(questImage);

                questText.text = "In glossary, there are also different pages to check. There are collection pages based on weapons, active items, beastiary etc. " +
                    "For example you can check bosses page. For now, all of them seems obscured. Every time you face it, you will reveal that one.";

                break;
            case TutorialPhase.FinishTutorial:
                isCheckPlayed = false;

                SoundAndImageTrigger(questImage);

                questText.text = "Congratulations. Now you learned the ropes. For more, it is a good time to start your Wartheon journey.";

                break;
            default:
                break;
        }

        passingToNextQuestOnProcess = false;
        questTextProcessRoutine = null;
    }

    private void SoundAndImageTrigger(Image questImage)
    {
        if (!isNextPhaseStartedSoundPlayed)
        {
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.openBookSoundEffect);
            isNextPhaseStartedSoundPlayed = true;
        }

        questImage.sprite = questNotCompletedSprite;
    }

    private void TutorialPhasePassProcess()
    {
        switch (currentTutorialPhase)
        {
            case TutorialPhase.TutorialIntro:
                break;
            case TutorialPhase.Move:
                Vector2 input = InputManager.Instance.movement.action.ReadValue<Vector2>();

                if (input.y > 0.1f) movedUp = true;
                if (input.y < -0.1f) movedDown = true;
                if (input.x < -0.1f) movedLeft = true;
                if (input.x > 0.1f) movedRight = true;

                if (movedUp && movedDown && movedLeft && movedRight)
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.PickUpWeapon:
                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.AimAndFire:
                // This phase is being handled at Decoy.cs
                break;
            case TutorialPhase.OpenGlossaryBook:
                if (InputManager.Instance.bookView.action.WasPerformedThisFrame())
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.MinimapCheck:
                Invoke(nameof(PassTutorialProcess), 2f);
                break;
            case TutorialPhase.OverviewMapCheck:
                if (InputManager.Instance.overviewMapFullView.action.WasPerformedThisFrame())
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.SecondRoom:
                if (GameManager.Instance.GetCurrentRoom().roomNodeType.roomNodeTypeName == "Small Room")
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.Combat:
                // For the rest of the characters, Destroyed script handles it.
                break;
            case TutorialPhase.PickUpPrimaryPassiveHealth:
                // This is handled by Drop Item script.
                break;
            case TutorialPhase.PickUpPrimaryPassiveCoin:
                // This is handled by Drop Item script.
                break;
            case TutorialPhase.Parry:
                if (player.playerDetails.playerCharacterIndex == Character.Nyveran || player.playerDetails.playerCharacterIndex == Character.Mycara)
                {
                    Invoke(nameof(PassTutorialProcess), 2f); // Directly pass for these characters
                }
                // It is handled by DealContactDamage.cs
                break;
            case TutorialPhase.DodgeRoll:
                if (InputManager.Instance.jumpButton.action.WasPerformedThisFrame())
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.SpecialSkill:
                if (InputManager.Instance.specialMoveOne.action.WasPerformedThisFrame() || InputManager.Instance.specialMoveTwo.action.WasPerformedThisFrame()
                    || InputManager.Instance.specialMoveThree.action.WasPerformedThisFrame())
                {
                    
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.KillEmAll:
                if (EnemySpawner.Instance.transform.childCount == 0)
                {
                    PassTutorialProcess();
                    MusicManager.Instance.PlayMusic(GameResources.Instance.ambientMusic);
                }
                break;
            case TutorialPhase.PickUpSecondaryPassive:
                if (player.equippedPassiveItems.TryGetValue(PassiveItemSlotName.Finger, out PassiveItem value) && value != null)
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.SkillsPage:
                if (player.currentSkillPoints == 0)
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.WeaponSetSwitch:
                if (InputManager.Instance.switchWeaponForward.action.WasPerformedThisFrame() || InputManager.Instance.switchWeaponBack.action.WasPerformedThisFrame() 
                    || InputManager.Instance.switchWeaponByWheel.action.WasPerformedThisFrame())
                {
                    PassTutorialProcess();
                }
                break;
            case TutorialPhase.OtherCollectionsPage:
                // It is handled in BookUI.cs
                break;
            case TutorialPhase.FinishTutorial:
                InputManager.TutorialEnabled = false;

                Invoke(nameof(PassTutorialProcess), 1f);

                if (!isFading)
                {
                    StartCoroutine(TutorialCompleted());
                }

                break;
            default:
                break;
        }
    }

    private void PassTutorialProcess()
    {
        currentTutorialProcess = TutorialProcess.QuestPassed;
    }

    IEnumerator NextPhaseProcess()
    {
        passingToNextQuestOnProcess = true;

        if (!isCheckPlayed)
        {
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.tutorialPhasePassSoundEffect);
            isCheckPlayed = true;
        }

        Image questImage = questBarContainer.GetChild(0).GetChild(1).GetComponent<Image>();
        TextMeshProUGUI questText = questBarContainer.GetChild(0).GetChild(0).GetComponent<TextMeshProUGUI>();
        questImage.sprite = questCompletedSprite;

        yield return new WaitForSeconds(2f);

        questImage.sprite = questNotCompletedSprite;
        questText.text = string.Empty;

        currentTutorialPhase++;
        currentTutorialProcess = TutorialProcess.QuestDisplayed;

        StartCoroutine(DialogueTextProcess());
    }

    /// <summary>
    /// Tutorial Completed
    /// </summary>
    private IEnumerator TutorialCompleted()
    {
        // Disable player
        player.DisablePlayer();

        // Fade Out
        yield return StartCoroutine(Fade(0f, 1f, 2f, Color.black));

        // Tutorial finished
        string upperName = player.playerDetails.playerCharacterName.ToUpper(CultureInfo.InvariantCulture);

        yield return StartCoroutine(DisplayMessageRoutine("WELL DONE " + upperName + "! YOU COMPLETED TUTORIAL.", Color.green, 5f, true));

        yield return StartCoroutine(DisplayMessageRoutine("NOW IT'S TIME TO BEGIN YOUR JOURNEY.", Color.green, 1.5f, true));

        FinishTutorialStartGame(); // It's time to return to the main menu
    }

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

                while (displayTimer > 0f)
                {
                    displayTimer -= Time.deltaTime;
                    yield return null;
                }
            }
        }

        // Clear text
        messageTextTMP.SetText("");
    }

    private void FinishTutorialStartGame()
    {
        // Get current character safely
        Character currentCharacter = Character.Caelion; // default

        if (player != null && player.playerDetails != null)
        {
            currentCharacter = player.playerDetails.playerCharacterIndex;
        }

        PlayerPrefs.SetInt("SelectedCharacterIndex", (int)currentCharacter);
        PlayerPrefs.Save();

        // Safe loading call
        if (LoadingManager.SafeInstance != null)
        {
            LoadingManager.SafeInstance.StartCoroutine(LoadingManager.SafeInstance.LoadGameScene(3));
        }
        else
        {
            Debug.LogError("LoadingManager instance missing! Loading directly...");
            SceneManager.LoadScene(3);
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
}
