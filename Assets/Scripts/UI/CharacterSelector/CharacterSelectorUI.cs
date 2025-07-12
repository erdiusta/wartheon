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
    [SerializeField] Light2D mycaraSpotlight;
    [SerializeField] Light2D caelionSpotlight;
    [SerializeField] Light2D nyveranSpotlight;
    [SerializeField] Light2D morvenSpotlight;

    [Space(10)]
    [Header("CHARACTER DETAILS TOOLTIP")]
    [Space(10)]
    [SerializeField] GameObject caelionDetailsPopUp;
    [SerializeField] GameObject morvenDetailsPopUp;
    [SerializeField] GameObject mycaraDetailsPopUp;
    [SerializeField] GameObject kynaraDetailsPopUp;
    [SerializeField] GameObject nymaraDetailsPopUp;
    [SerializeField] GameObject nyveranDetailsPopUp;
    [SerializeField] GameObject karnagDetailsPopUp;
    [SerializeField] GameObject nyxaDetailsPopUp;

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
        DisableDetailsPopup(ref caelionDetailsPopUp);
        DisableDetailsPopup(ref morvenDetailsPopUp);
        DisableDetailsPopup(ref nyveranDetailsPopUp);
        DisableDetailsPopup(ref mycaraDetailsPopUp);
        DisableDetailsPopup(ref karnagDetailsPopUp);
    }

    private void StaticEventHandler_OnCharacterButtonSelected(CharacterButtonArgs characterButtonArgs)
    {
        if (characterButtonArgs.charName == Settings.caelionTag) HoverCaelion();
        else if (characterButtonArgs.charName == Settings.morvenTag) HoverMorven();
        else if (characterButtonArgs.charName == Settings.mycaraTag) HoverMycara();
        else if (characterButtonArgs.charName == Settings.nyveranTag) HoverNyveran();
        else if (characterButtonArgs.charName == Settings.karnagTag) HoverKarnag();
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
        caelionSpotlight.gameObject.SetActive(true);

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
        SelectedCharacterButton charButton = eventData.pointerEnter.GetComponentInParent<SelectedCharacterButton>(true);

        if (charButton != null)
        {
            switch (charButton.selectedCharacter)
            {
                case Character.Caelion:
                    HoverCaelion();
                    break;
                case Character.Morven:
                    HoverMorven();
                    break;
                case Character.Nyveran:
                    HoverNyveran();
                    break;
                case Character.Mycara:
                    HoverMycara();
                    break;
                case Character.Karnag:
                    HoverKarnag();
                    break;
                case Character.Kynara:
                    break;
                case Character.Nymara:
                    break;
                case Character.Nyxa:
                    break;
                default:
                    break;
            }
        }
    }

    private void CloseCharacterTooltip(PointerEventData eventData)
    {
        DisableDetailsPopup(ref caelionDetailsPopUp);
        DisableDetailsPopup(ref morvenDetailsPopUp);
        DisableDetailsPopup(ref mycaraDetailsPopUp);
        DisableDetailsPopup(ref nyveranDetailsPopUp);
        DisableDetailsPopup(ref karnagDetailsPopUp);
    }

    public void HoverCaelion()
    {
        StartCoroutine(SelectionRoutine(0, caelionSpotlight, caelionDetailsPopUp));
    }

    public void HoverMorven()
    {
        StartCoroutine(SelectionRoutine(1, morvenSpotlight, morvenDetailsPopUp));
    }

    public void HoverNyveran()
    {
        StartCoroutine(SelectionRoutine(2, nyveranSpotlight, nyveranDetailsPopUp));
    }

    public void HoverMycara()
    {
        StartCoroutine(SelectionRoutine(3, mycaraSpotlight, mycaraDetailsPopUp));
    }

    public void HoverKarnag()
    {
        StartCoroutine(SelectionRoutine(4, nyveranSpotlight, karnagDetailsPopUp));
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

            StaticEventHandler.ClearAll(); // Clear all events before start (to prevent holding old data in case of restarts)
            SceneManager.LoadScene(3);
        }
    }

    IEnumerator SelectionRoutine(int index, Light2D selectedCharSpotlight, GameObject selectedCharPopUp)
    {
        mycaraSpotlight.gameObject.SetActive(false);
        caelionSpotlight.gameObject.SetActive(false);
        nyveranSpotlight.gameObject.SetActive(false);
        morvenSpotlight.gameObject.SetActive(false);

        //yield return null;

        selectedPlayerIndex = index;
        currentPlayer.playerDetails = playerDetailsList[index];
        selectedCharSpotlight.gameObject.SetActive(true);
        selectedCharPopUp.SetActive(true);

        yield return null;
    }
}
