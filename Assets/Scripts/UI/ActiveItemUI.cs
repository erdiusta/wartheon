using System;
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
    [Tooltip("Populate with the TextMeshPro-Text component on the child ItemRemainingText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI itemRemainingText;
    #region Tooltip
    [Tooltip("Populate with the TextMeshPro-Text component on the child ItemNameText gameobject")]
    #endregion Tooltip
    [SerializeField] TextMeshProUGUI itemNameText;

    Player player;

    private void Awake()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        player.setActiveWeaponEvent.OnSelectedActiveItem += SetActiveWeaponEvent_OnSelectedActiveItem;
        player.weaponFiredEvent.OnActiveItemFired += WeaponFiredEvent_OnActiveItemFired;
    }


    private void OnDisable()
    {
        player.setActiveWeaponEvent.OnSelectedActiveItem -= SetActiveWeaponEvent_OnSelectedActiveItem;
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

    private void WeaponFiredEvent_OnActiveItemFired(WeaponFiredEvent weaponFiredEvent, ActiveItemFiredEventArgs activeItemFiredEventArgs)
    {
        UpdateActiveItemRemainingProjectile(activeItemFiredEventArgs.activeItem);
    }


    private void SetSelectedActiveItem(ActiveItem activeItem)
    {
        UpdateActiveItemImage(activeItem.activeItemDetails);
        UpdateActiveItemName(activeItem.activeItemDetails);
        UpdateActiveItemRemainingProjectile(activeItem);
    }


    private void UpdateActiveItemImage(ActiveItemDetailsSO activeItemDetails)
    {
        activeItemImage.sprite = activeItemDetails.activeItemSprite;
    }

    private void UpdateActiveItemName(ActiveItemDetailsSO activeItemDetails)
    {
        itemNameText.text = activeItemDetails.activeItemName;
    }

    private void UpdateActiveItemRemainingProjectile(ActiveItem activeItem)
    {
        if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Boomerang)
        {
            itemRemainingText.text = "1";
            return;
        }

        if (activeItem.activeItemDetails.hasNoProjectileNumberLimit)
        {
            itemRemainingText.text = "";
        }
        else
        {
            itemRemainingText.text = activeItem.activeItemRemainingProjectile.ToString();
        }
    }
}