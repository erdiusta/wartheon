using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using System.Collections;

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

        StaticEventHandler.OnCharacterButtonSelected += StaticEventHandler_OnCharacterButtonSelected;
        StaticEventHandler.OnCharacterButtonDeselected += StaticEventHandler_OnCharacterButtonDeselected;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnCharacterButtonSelected -= StaticEventHandler_OnCharacterButtonSelected;
        StaticEventHandler.OnCharacterButtonDeselected -= StaticEventHandler_OnCharacterButtonDeselected;
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

    public void HoverLyrisa()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 0;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        lyrisaSpotlight.gameObject.SetActive(true);
        lyrisaDetailsPopUp.SetActive(true);
    }

    public void HoverAstraeus()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 1;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        astraeusSpotlight.gameObject.SetActive(true);
        astraeusDetailsPopUp.SetActive(true);
    }

    public void HoverOrion()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 2;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        orionSpotlight.gameObject.SetActive(true);
        orionDetailsPopUp.SetActive(true);
    }

    public void HoverErebus()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 3;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        erebusSpotlight.gameObject.SetActive(true);
        erebusDetailsPopUp.SetActive(true);
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

    public void StartGame()
    {
        SceneManager.LoadScene("MainGameScene");
    }

    private void DisableAllSpotlights()
    {
        lyrisaSpotlight.gameObject.SetActive(false);
        astraeusSpotlight.gameObject.SetActive(false);
        orionSpotlight.gameObject.SetActive(false);
        erebusSpotlight.gameObject.SetActive(false);
    }
}
