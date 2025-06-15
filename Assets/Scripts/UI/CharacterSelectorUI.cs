using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class CharacterSelectorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Tooltip
    [Tooltip("Populate this with the parent canvas")]
    #endregion
    [SerializeField] Canvas parentCanvas;
    [SerializeField] GameObject firstSelectedButton;

    [Space(10)]
    [Header("CHARACTER SPOTLIGHTS")]
    [Space(10)]
    [SerializeField] Light2D lyrisaSpotlight;
    [SerializeField] Light2D astraeusSpotlight;
    [SerializeField] Light2D orionSpotlight;
    [SerializeField] Light2D erebusSpotlight;
    [Space(10)]
    [SerializeField] GameObject astraeusDetailsPopUp;
    [SerializeField] GameObject erebusDetailsPopUp;
    [SerializeField] GameObject lyrisaDetailsPopUp;
    [SerializeField] GameObject orionDetailsPopUp;

    [Space(10)]
    [Header("TUTORIAL TOGGLE")]
    [Space(10)]
    [SerializeField] Toggle tutorialToggle;
    [SerializeField] Image tutorialToggleCheckmarkImage;
    [SerializeField] SoundEffectSO buttonClickSound;

    PlayerDetailsSO[] playerDetailsList;
    CurrentPlayerSO currentPlayer;
    int selectedPlayerIndex = 1;

    private void Awake()
    {
        playerDetailsList = GameResources.Instance.playerDetailsArray;
        currentPlayer = GameResources.Instance.currentPlayer;
    }
    void OnEnable()
    {
        // Delay selection until the next frame to ensure UI is ready
        StartCoroutine(SetFirstSelected());

        // Load Tutorial toggle
        if (PlayerPrefs.HasKey("Tutorial"))
        {
            bool tutorialIsOn = PlayerPrefs.GetInt("Tutorial") == 1;
            tutorialToggle.SetIsOnWithoutNotify(tutorialIsOn);
            InputManager.TutorialEnabled = tutorialIsOn;
        }

        StaticEventHandler.OnCharacterButtonSelected += StaticEventHandler_OnCharacterButtonSelected;
        StaticEventHandler.OnCharacterButtonDeselected += StaticEventHandler_OnCharacterButtonDeselected;

        // UI Element listeners
        tutorialToggle.onValueChanged.AddListener(OnTutorialToggleChanged);
    }

    private void OnDisable()
    {
        StaticEventHandler.OnCharacterButtonSelected -= StaticEventHandler_OnCharacterButtonSelected;
        StaticEventHandler.OnCharacterButtonDeselected -= StaticEventHandler_OnCharacterButtonDeselected;

        // UI Element listeners
        tutorialToggle.onValueChanged.RemoveListener(OnTutorialToggleChanged);
    }

    private void StaticEventHandler_OnCharacterButtonDeselected()
    {
        DisableDetailsPopup(ref astraeusDetailsPopUp);
        DisableDetailsPopup(ref erebusDetailsPopUp);
        DisableDetailsPopup(ref orionDetailsPopUp);
        DisableDetailsPopup(ref lyrisaDetailsPopUp);
    }

    private void StaticEventHandler_OnCharacterButtonSelected(CharacterButtonArgs characterButtonArgs)
    {
        if (characterButtonArgs.charName == Settings.astraeus) HoverAstraeus();
        else if (characterButtonArgs.charName == Settings.erebus) HoverErebus();
        else if (characterButtonArgs.charName == Settings.lyrisa) HoverLyrisa();
        else if (characterButtonArgs.charName == Settings.orion) HoverOrion();
    }

    private IEnumerator SetFirstSelected()
    {
        yield return null; // wait 1 frame

        if (firstSelectedButton != null)
        {
            EventSystem.current.SetSelectedGameObject(firstSelectedButton);
        }
    }

    private void Start()
    {
        // Initialize the current player
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        astraeusSpotlight.gameObject.SetActive(true);

        OnTutorialToggleChanged(InputManager.TutorialEnabled);
    }

    private void Update()
    {
        if (InputManager.Instance.escapeButton.action.WasPressedThisFrame())
        {
            BackButton();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        OpenCharacterTooltip(eventData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CloseCharacterTooltip(eventData);
    }

    private void OpenCharacterTooltip(PointerEventData eventData)
    {
        if (eventData.pointerEnter.CompareTag(Settings.astraeusTag)) HoverAstraeus();
        else if (eventData.pointerEnter.CompareTag(Settings.lyrisaTag)) HoverLyrisa();
        else if (eventData.pointerEnter.CompareTag(Settings.erebusTag)) HoverErebus();
        else if (eventData.pointerEnter.CompareTag(Settings.orionTag)) HoverOrion();
    }

    private void CloseCharacterTooltip(PointerEventData eventData)
    {
        DisableDetailsPopup(ref astraeusDetailsPopUp);
        DisableDetailsPopup(ref erebusDetailsPopUp);
        DisableDetailsPopup(ref lyrisaDetailsPopUp);
        DisableDetailsPopup(ref orionDetailsPopUp);
    }

    public void HoverAstraeus()
    {
        StartCoroutine(SelectionRoutine(0, astraeusSpotlight, astraeusDetailsPopUp));
    }

    public void HoverErebus()
    {
        StartCoroutine(SelectionRoutine(1, erebusSpotlight, erebusDetailsPopUp));
    }

    public void HoverOrion()
    {
        StartCoroutine(SelectionRoutine(2, orionSpotlight, orionDetailsPopUp));
    }


    public void HoverLyrisa()
    {
        StartCoroutine(SelectionRoutine(3, lyrisaSpotlight, lyrisaDetailsPopUp));
    }

    private void DisableDetailsPopup(ref GameObject popupObject)
    {
        popupObject.SetActive(false);
    }

    public void BackButton()
    {
        // Set Play Button as selected
        MainMenuUI.Instance.StartCoroutine(MainMenuUI.Instance.HandleReturnFromCharacterScene());

        // Unload the current additive scene
        StaticEventHandler.CallAdditiveSceneRemoveEvent();

        SceneManager.UnloadSceneAsync(gameObject.scene);
    }

    public void OnTutorialToggleChanged(bool isOn)
    {
        if (InputManager.TutorialEnabled != isOn)
        {
            InputManager.TutorialEnabled = isOn;
            SoundEffectManager.Instance.PlaySoundEffect(buttonClickSound);
        }
        else
        {
            InputManager.TutorialEnabled = isOn;
        }

        PlayerPrefs.SetInt("Tutorial", tutorialToggle.isOn ? 1 : 0);
        PlayerPrefs.Save();
    }

    public void StartGame()
    {
        // Save the selected character immediately before any scene transition
        GameResources.Instance.currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        PlayerPrefs.SetInt("SelectedCharacterIndex", selectedPlayerIndex);
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

    IEnumerator SelectionRoutine(int index, Light2D selectedCharSpotlight, GameObject selectedCharPopUp)
    {
        lyrisaSpotlight.gameObject.SetActive(false);
        astraeusSpotlight.gameObject.SetActive(false);
        orionSpotlight.gameObject.SetActive(false);
        erebusSpotlight.gameObject.SetActive(false);

        yield return null;

        selectedPlayerIndex = index;
        currentPlayer.playerDetails = playerDetailsList[index];
        selectedCharSpotlight.gameObject.SetActive(true);
        selectedCharPopUp.SetActive(true);

        yield return null;
    }
}
