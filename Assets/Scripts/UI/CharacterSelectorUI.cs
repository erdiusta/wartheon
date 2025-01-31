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
        DisableAllSpotlights();
    }

    public void HoverLyrisa()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 0;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        lyrisaSpotlight.gameObject.SetActive(true);
    }

    public void HoverAstraeus()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 1;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        astraeusSpotlight.gameObject.SetActive(true);
    }

    public void HoverOrion()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 2;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        orionSpotlight.gameObject.SetActive(true);
    }

    public void HoverErebus()
    {
        DisableAllSpotlights();
        selectedPlayerIndex = 3;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        erebusSpotlight.gameObject.SetActive(true);
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
