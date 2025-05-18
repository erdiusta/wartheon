using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

[DisallowMultipleComponent]
public class CharacterSelectorUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    #region Tooltip
    [Tooltip("Populate this with the parent canvas")]
    #endregion
    [SerializeField] Canvas parentCanvas;

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

    private void Start()
    {
        // Initialize the current player
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        astraeusSpotlight.gameObject.SetActive(true);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter.CompareTag(Settings.astraeusTag))
        {
            HoverAstraeus();
        }
        else if (eventData.pointerEnter.CompareTag(Settings.lyrisaTag))
        {
            HoverLyrisa();
        }
        else if (eventData.pointerEnter.CompareTag(Settings.erebusTag))
        {
            HoverErebus();
        }
        else if (eventData.pointerEnter.CompareTag(Settings.orionTag))
        {
            HoverOrion();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (eventData.pointerEnter.CompareTag(Settings.astraeusTag))
        {
            DisableDetailsPopup(ref astraeusDetailsPopUp);
        }
        else if (eventData.pointerEnter.CompareTag(Settings.erebusTag))
        {
            DisableDetailsPopup(ref erebusDetailsPopUp);
        }
        else if (eventData.pointerEnter.CompareTag(Settings.lyrisaTag))
        {
            DisableDetailsPopup(ref lyrisaDetailsPopUp);
        }
        else if (eventData.pointerEnter.CompareTag(Settings.orionTag))
        {
            DisableDetailsPopup(ref orionDetailsPopUp);
        }
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

    #region Validation
#if UNITY_EDITOR
    private void OnValidate()
    {
        HelperUtilities.ValidateCheckNullValue(this, nameof(parentCanvas), parentCanvas);
    }
#endif
    #endregion
}
