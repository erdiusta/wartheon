using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using TMPro;

[DisallowMultipleComponent]
public class CharacterSelectorUI : MonoBehaviour
{
    #region Tooltip
    [Tooltip("Populate this with the child CharacterSelector gameobject")]
    #endregion
    [SerializeField] Transform characterSelector;
    #region Tooltip
    [Tooltip("Populate this with the TMPro of selected character")]
    #endregion
    [SerializeField] TextMeshProUGUI characterNameText;

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
        characterNameText.text = playerDetailsList[selectedPlayerIndex].playerCharacterName.ToString();
        astraeusSpotlight.gameObject.SetActive(true);
    }

    /// <summary>
    /// Select next character - this method is called from onClick event set in the inspector
    /// </summary>
    public void NextCharacter()
    {
        if (selectedPlayerIndex >= playerDetailsList.Length - 1) return;

        selectedPlayerIndex++;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        characterNameText.text = playerDetailsList[selectedPlayerIndex].playerCharacterName.ToString();
        MoveToSelectedCharacter(selectedPlayerIndex);
    }

    /// <summary>
    /// Select previous character - this method is called from onClick event set in the inspector
    /// </summary>
    public void PreviousCharacter()
    {
        if (selectedPlayerIndex == 0) return;

        selectedPlayerIndex--;
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        characterNameText.text = playerDetailsList[selectedPlayerIndex].playerCharacterName.ToString();
        MoveToSelectedCharacter(selectedPlayerIndex);
    }

    private void MoveToSelectedCharacter(int index)
    {
        DisableAllSpotlights();

        switch (index)
        {
            case 0:
                lyrisaSpotlight.gameObject.SetActive(true);
                break;
            case 1:
                astraeusSpotlight.gameObject.SetActive(true);
                break;
            case 2:
                orionSpotlight.gameObject.SetActive(true);
                break;
            case 3:
                erebusSpotlight.gameObject.SetActive(true);
                break;
            default:
                break;
        }
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
        HelperUtilities.ValidateCheckNullValue(this, nameof(characterSelector), characterSelector);
    }
#endif
    #endregion
}
