using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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

    PlayerDetailsSO[] playerDetailsList;
    GameObject playerSelectionPrefab;
    CurrentPlayerSO currentPlayer;
    List<GameObject> playerCharacterGameObjectList = new List<GameObject>();
    Coroutine coroutine;
    int selectedPlayerIndex = 0;
    float offset = 4f;


    private void Awake()
    {
        playerSelectionPrefab = GameResources.Instance.playerSelectionPrefab;
        playerDetailsList = GameResources.Instance.playerDetailsArray;
        currentPlayer = GameResources.Instance.currentPlayer;
    }

    private void Start()
    {
        // Instantiate player characters
        for (int i = 0; i < playerDetailsList.Length; i++)
        {
            GameObject playerSelectionObject = Instantiate(playerSelectionPrefab, characterSelector);
            playerCharacterGameObjectList.Add(playerSelectionObject);
            playerSelectionObject.transform.localPosition = new Vector3((offset * i), 0f, 0f);
            PopulatePlayerDetails(playerSelectionObject.GetComponent<PlayerSelectionUI>(), playerDetailsList[i]);
        }

        // Initialize the current player
        currentPlayer.playerDetails = playerDetailsList[selectedPlayerIndex];
        characterNameText.text = playerDetailsList[selectedPlayerIndex].playerCharacterName.ToString();
    }

    /// <summary>
    /// Populate player character details for display
    /// </summary>
    private void PopulatePlayerDetails(PlayerSelectionUI playerSelection, PlayerDetailsSO playerDetails)
    {
        playerSelection.playerMainHandSpriteRenderer.sprite = playerDetails.playerHandSprite;
        playerSelection.playerOffHandSpriteRenderer.sprite = playerDetails.playerHandSprite;
        playerSelection.playerMainHandWeaponAnimator.runtimeAnimatorController = playerDetails.mainHandAnimatorController;

        if (playerDetails.offHandAnimatorController != null)
        {
            playerSelection.playerOffHandWeaponAnimator.runtimeAnimatorController = playerDetails.offHandAnimatorController;
        }
        else
        {
            playerSelection.playerOffHandWeaponAnimator.enabled = false;
        }

        playerSelection.playerWeaponMainHandSpriteRenderer.sprite = playerDetails.startingWeaponList[0].weaponFrontSprite;
        if (playerDetails.startingWeaponList.Count > 1)
        {
            if (playerDetails.startingWeaponList[1].weaponClass == WeaponClass.Shield)
            {
                playerSelection.playerOffHandSpriteRenderer.sortingOrder = 0;
                playerSelection.playerWeaponOffHandSpriteRenderer.sprite = playerDetails.startingWeaponList[1].weaponFrontSprite;
            }
            else if (playerDetails.startingWeaponList[1].weaponClass == WeaponClass.Dagger)
            {
                playerSelection.playerWeaponOffHandSpriteRenderer.sprite = playerDetails.startingWeaponList[1].weaponFrontSprite;
            }
        }

        if (playerDetails.playerCharacterIndex == Character.Astraeus)
        {
            playerSelection.animator.runtimeAnimatorController = playerDetails.oneHandRuntimeAnimatorController;
        }
        else if (playerDetails.playerCharacterIndex == Character.Orion)
        {
            playerSelection.thirdHandGameObject.SetActive(true);
            playerSelection.thirdHandGameObject.GetComponent<SpriteRenderer>().sprite = playerDetails.playerHandSprite;
            playerSelection.thirdHandGameObject.transform.localPosition = new Vector3(-0.4f, 0f, 0f);

            playerSelection.animator.runtimeAnimatorController = playerDetails.bowRuntimeAnimatorController;

            playerSelection.playerOffHandWeaponAnimator.enabled = false;
            playerSelection.offHandWeaponAnchorTransform.gameObject.SetActive(false);
        }
        else if (playerDetails.playerCharacterIndex == Character.Erebus)
        {
            playerSelection.animator.runtimeAnimatorController = playerDetails.oneHandRuntimeAnimatorController;
        }
        else if (playerDetails.playerCharacterIndex == Character.Lyrisa)
        {
            playerSelection.thirdHandGameObject.SetActive(false);
            playerSelection.animator.runtimeAnimatorController = playerDetails.staffRuntimeAnimatorController;

            playerSelection.thirdHandGameObject.SetActive(false);
            playerSelection.offHandWeaponAnchorTransform.gameObject.SetActive(true);
            playerSelection.playerOffHandWeaponAnimator.enabled = true;
        }
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
        if (coroutine != null)
            StopCoroutine(coroutine);

        coroutine = StartCoroutine(MoveToSelectedCharacterRoutine(index));
    }

    IEnumerator MoveToSelectedCharacterRoutine(int index)
    {
        float currentLocalXPosition = characterSelector.localPosition.x;
        float targetLocalXPosition = index * offset * characterSelector.localScale.x * -1f;

        while (Mathf.Abs(currentLocalXPosition - targetLocalXPosition) > 0.01f)
        {
            currentLocalXPosition = Mathf.Lerp(currentLocalXPosition, targetLocalXPosition, Time.deltaTime * 10f);
            characterSelector.localPosition = new Vector3(currentLocalXPosition, characterSelector.localPosition.y, 0f);

            yield return null;
        }

        characterSelector.localPosition = new Vector3(targetLocalXPosition, characterSelector.localPosition.y, 0f);
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
