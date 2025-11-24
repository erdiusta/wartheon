using UnityEngine;
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
        DisableAllPopUps();
    }

    private void StaticEventHandler_OnCharacterButtonSelected(CharacterButtonArgs characterButtonArgs)
    {
        switch (characterButtonArgs.charIndex)
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
                HoverKynara();
                break;
            case Character.Nymara:
                HoverNymara();
                break;
            case Character.Nyxa:
                HoverNyxa();
                break;
            default:
                break;
        }
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

        OnTutorialToggleChanged(InputManager.TutorialEnabled);
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
        CharacterSelectionButton charButton = eventData.pointerEnter.GetComponentInParent<CharacterSelectionButton>(true);

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
                    HoverKynara();
                    break;
                case Character.Nymara:
                    HoverNymara();
                    break;
                case Character.Nyxa:
                    HoverNyxa();
                    break;
                default:
                    break;
            }
        }
    }

    private void CloseCharacterTooltip(PointerEventData eventData)
    {
        DisableAllPopUps();
    }

    public void HoverCaelion()
    {
        SelectionRoutine(0, caelionDetailsPopUp);
    }

    public void HoverMorven()
    {
        SelectionRoutine(1, morvenDetailsPopUp);
    }

    public void HoverNyveran()
    {
        SelectionRoutine(2, nyveranDetailsPopUp);
    }

    public void HoverMycara()
    {
        SelectionRoutine(3, mycaraDetailsPopUp);
    }

    public void HoverKynara()
    {
        SelectionRoutine(4, kynaraDetailsPopUp);
    }

    public void HoverKarnag()
    {
        SelectionRoutine(5, karnagDetailsPopUp);
    }

    public void HoverNyxa()
    {
        SelectionRoutine(6, nyxaDetailsPopUp);
    }

    public void HoverNymara()
    {
        SelectionRoutine(7, nymaraDetailsPopUp);
    }

    private void DisableDetailsPopup(ref GameObject popupObject)
    {
        popupObject.SetActive(false);
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
            LoadingManager.SafeInstance.StartCoroutine(LoadingManager.SafeInstance.LoadGameScene(2));
        }
        else
        {
            Debug.LogError("LoadingManager instance missing! Loading directly...");

            StaticEventHandler.ClearAll(); // Clear all events before start (to prevent holding old data in case of restarts)
            SceneManager.LoadScene(3);
        }
    }

    void SelectionRoutine(int index, GameObject selectedCharPopUp)
    {
        DisableAllPopUps();

        selectedPlayerIndex = index;
        currentPlayer.playerDetails = playerDetailsList[index];
        selectedCharPopUp.SetActive(true);

        //yield return new WaitForEndOfFrame();
    }

    private void DisableAllPopUps()
    {
        DisableDetailsPopup(ref caelionDetailsPopUp);
        DisableDetailsPopup(ref morvenDetailsPopUp);
        DisableDetailsPopup(ref nyveranDetailsPopUp);
        DisableDetailsPopup(ref mycaraDetailsPopUp);
        DisableDetailsPopup(ref kynaraDetailsPopUp);
        DisableDetailsPopup(ref karnagDetailsPopUp);
        DisableDetailsPopup(ref nyxaDetailsPopUp);
        DisableDetailsPopup(ref nymaraDetailsPopUp);
    }
}
