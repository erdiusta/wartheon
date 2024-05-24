using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ActiveItemUI : MonoBehaviour
{
    #region Header OBJECT REFERENCES
    [Space(10)]
    [Header("OBJECT REFERENCES")]
    #endregion Header OBJECT REFERENCES
    #region Tooltip
    [Tooltip("Populate with image component on the child ActiveItemImage gameobject")]
    #endregion Tooltip
    [SerializeField] Image activeItemImage;
    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child ItemNameText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI itemNameText;
    #region Tooltip
    [Tooltip("Populate with the RectTransform of the child gameobject AvailabilityBar")]
    #endregion Tooltip
    [SerializeField] Transform availabilityBar;
    #region Tooltip
    [Tooltip("Populate with the Image component of the child gameobject BarImage")]
    #endregion Tooltip
    [SerializeField] Image barImage;

    Player player;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        player.setActiveWeaponEvent.OnSelectedActiveItem += SetActiveWeaponEvent_OnSelectedActiveItem;
        player.setActiveWeaponEvent.OnRemovedActiveItem += SetActiveWeaponEvent_OnRemovedActiveItem;
        player.weaponFiredEvent.OnActiveItemFired += WeaponFiredEvent_OnActiveItemFired;
    }

    private void OnDisable()
    {
        player.setActiveWeaponEvent.OnSelectedActiveItem -= SetActiveWeaponEvent_OnSelectedActiveItem;
        player.setActiveWeaponEvent.OnRemovedActiveItem -= SetActiveWeaponEvent_OnRemovedActiveItem;
        player.weaponFiredEvent.OnActiveItemFired -= WeaponFiredEvent_OnActiveItemFired;
    }

    private void Start()
    {
        // Update active weapon status on the UI
        SetSelectedActiveItem(player.selectedActiveItem.GetCurrentActiveItem());
    }

    private void SetActiveWeaponEvent_OnSelectedActiveItem(SetActiveWeaponEvent setActiveWeaponEvent, SetSelectedActiveItemArgs setSelectedActiveItemArgs)
    {
        SetSelectedActiveItem(setSelectedActiveItemArgs.activeItem);
    }

    private void SetActiveWeaponEvent_OnRemovedActiveItem(SetActiveWeaponEvent setActiveWeaponEvent)
    {
        RemoveSelectedActiveItem();
    }

    private void WeaponFiredEvent_OnActiveItemFired(WeaponFiredEvent weaponFiredEvent, ActiveItemFiredEventArgs activeItemFiredEventArgs)
    {
        UpdateAvailabilityBar(activeItemFiredEventArgs.activeItem);
    }

    private void SetSelectedActiveItem(ActiveItem activeItem)
    {
        availabilityBar.gameObject.SetActive(true);
        UpdateActiveItemImage(activeItem.activeItemDetails);
        UpdateActiveItemName(activeItem.activeItemDetails);
        UpdateAvailabilityBar(activeItem);
    }

    private void RemoveSelectedActiveItem()
    {
        activeItemImage.enabled = false;
        itemNameText.text = "";
        availabilityBar.gameObject.SetActive(false);
    }

    private void UpdateActiveItemImage(ActiveItemDetailsSO activeItemDetails)
    {
        activeItemImage.enabled = true;
        activeItemImage.sprite = activeItemDetails.activeItemSprite;
    }

    private void UpdateActiveItemName(ActiveItemDetailsSO activeItemDetails)
    {
        itemNameText.text = activeItemDetails.activeItemName;
    }

    private void UpdateAvailabilityBar(ActiveItem activeItem)
    {
        StartCoroutine(UpdateAvailabilityBarRoutine(activeItem));
    }

    /// <summary>
    /// Animate reload weapon bar coroutine
    /// </summary>
    private IEnumerator UpdateAvailabilityBarRoutine(ActiveItem currentActiveItem)
    {
        // Set the reload bar to red
        barImage.color = Color.yellow;

        if (currentActiveItem.activeItemDetails.hasNoProjectileNumberLimit)
            availabilityBar.transform.localScale = new Vector3(1f, 1f, 1f);

        if (!currentActiveItem.activeItemDetails.hasNoProjectileNumberLimit)
        {
            // Update availability bar
            float barFill = (float)currentActiveItem.activeItemRemainingCharge / (float)currentActiveItem.activeItemDetails.activeItemMaxCharge;

            // Update bar fill
            availabilityBar.transform.localScale = new Vector3(1f, barFill, 1f);

            yield return null;
        }

    }
}