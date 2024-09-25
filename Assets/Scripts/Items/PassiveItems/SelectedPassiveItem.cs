using UnityEngine;

public class SelectedPassiveItem : MonoBehaviour
{
    public bool isHeadEquipped { get; private set; }
    public bool isChestEquipped { get; private set; }
    public bool isNeckEquipped { get; private set; }
    public bool isArmEquipped { get; private set; }
    public bool isFingerEquipped { get; private set; }
    public bool isWaistEquipped { get; private set; }
    public bool isBackEquipped { get; private set; }
    public bool isLegEquipped { get; private set; }

    Player player;

    PassiveItem headPassiveItem;
    PassiveItem chestPassiveItem;
    PassiveItem neckPassiveItem;
    PassiveItem armPassiveItem;
    PassiveItem fingerPassiveItem;
    PassiveItem waistPassiveItem;
    PassiveItem backPassiveItem;
    PassiveItem legPassiveItem;

    SetPassiveItemEvent setPassiveItemEvent;

    private void Awake()
    {
        setPassiveItemEvent = GetComponent<SetPassiveItemEvent>();
    }

    private void OnEnable()
    {
        setPassiveItemEvent.OnEquippedPassiveItem += SetPassiveItemEvent_OnEquippedPassiveItem;
        setPassiveItemEvent.OnRemovedPassiveItem += SetPassiveItemEvent_OnRemovedPassiveItem;
    }

    private void OnDisable()
    {
        setPassiveItemEvent.OnEquippedPassiveItem -= SetPassiveItemEvent_OnEquippedPassiveItem;
        setPassiveItemEvent.OnRemovedPassiveItem -= SetPassiveItemEvent_OnRemovedPassiveItem;
    }

    private void SetPassiveItemEvent_OnEquippedPassiveItem(SetPassiveItemEvent setPassiveItemEvent, SetPassiveItemEventArgs setPassiveItemEventArgs)
    {
        player = GameManager.Instance.GetPlayer();

        switch (setPassiveItemEventArgs.passiveItemSlotName)
        {
            case PassiveItemSlotName.Head:
                if (!isHeadEquipped)
                {
                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    headPassiveItem = setPassiveItemEventArgs.passiveItem;
                    isHeadEquipped = true;
                }
                break;
            case PassiveItemSlotName.Chest:
                if (!isChestEquipped)
                {
                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    chestPassiveItem = setPassiveItemEventArgs.passiveItem;
                    isChestEquipped = true;
                }
                break;
            case PassiveItemSlotName.Neck:
                if (!isNeckEquipped)
                {
                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    neckPassiveItem = setPassiveItemEventArgs.passiveItem;
                    isNeckEquipped = true;
                }
                break;
            case PassiveItemSlotName.Finger:
                if (!isFingerEquipped)
                {
                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    fingerPassiveItem = setPassiveItemEventArgs.passiveItem;
                    isFingerEquipped = true;
                }
                break;
            case PassiveItemSlotName.Back:
                if (!isBackEquipped)
                {
                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    backPassiveItem = setPassiveItemEventArgs.passiveItem;
                    isBackEquipped = true;
                }
                break;
            case PassiveItemSlotName.Waist:
                if (!isWaistEquipped)
                {
                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    waistPassiveItem = setPassiveItemEventArgs.passiveItem;
                    isWaistEquipped = true;
                }
                break;
            case PassiveItemSlotName.Arm:
                if (!isArmEquipped)
                {
                    armPassiveItem = setPassiveItemEventArgs.passiveItem;

                    if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                    {
                        player.currentPhysicalResistanceValue = player.currentPhysicalResistanceValue + 0.05f ;
                        player.currentAirResistanceValue = player.currentAirResistanceValue + 0.1f;
                    }

                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);

                    isArmEquipped = true;
                }
                break;
            case PassiveItemSlotName.Leg:
                if (!isLegEquipped)
                {
                    player.playerControl.PopulatePassiveItemsToBook(setPassiveItemEventArgs.passiveItem.passiveItemDetails.passiveItemSprite,
                        setPassiveItemEventArgs.passiveItemSlotName);
                    legPassiveItem = setPassiveItemEventArgs.passiveItem;
                    isLegEquipped = true;
                }
                break;
            default:
                break;
        }
    }

    private void SetPassiveItemEvent_OnRemovedPassiveItem(SetPassiveItemEvent setPassiveItemEvent, SetPassiveItemEventArgs setPassiveItemEventArgs)
    {
        player = GameManager.Instance.GetPlayer();

        switch (setPassiveItemEventArgs.passiveItemSlotName)
        {
            case PassiveItemSlotName.Head:
                if (isHeadEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    headPassiveItem = null;
                    isHeadEquipped = false;
                }
                break;
            case PassiveItemSlotName.Chest:
                if (isChestEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    chestPassiveItem = null;
                    isChestEquipped = false;
                }
                break;
            case PassiveItemSlotName.Neck:
                if (isNeckEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    neckPassiveItem = null;
                    isNeckEquipped = false;
                }
                break;
            case PassiveItemSlotName.Finger:
                if (isFingerEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    fingerPassiveItem = null;
                    isFingerEquipped = false;
                }
                break;
            case PassiveItemSlotName.Back:
                if (isBackEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    backPassiveItem = null;
                    isBackEquipped = false;
                }
                break;
            case PassiveItemSlotName.Waist:
                if (isWaistEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    waistPassiveItem = null;
                    isWaistEquipped = false;
                }
                break;
            case PassiveItemSlotName.Arm:
                if (isArmEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);

                    if (armPassiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                    {
                        player.currentPhysicalResistanceValue = player.currentPhysicalResistanceValue - 0.05f;
                        player.currentAirResistanceValue = player.currentAirResistanceValue - 0.1f;
                    }

                    armPassiveItem = null;
                    isArmEquipped = false;
                }
                break;
            case PassiveItemSlotName.Leg:
                if (isLegEquipped)
                {
                    player.playerControl.RemovePassiveItemFromBook(setPassiveItemEventArgs.passiveItemSlotName);
                    legPassiveItem = null;
                    isLegEquipped = false;
                }
                break;
            default:
                break;
        }
    }

    public PassiveItem GetCurrentHeadPassiveItem()
    {
        return headPassiveItem;
    }

    public PassiveItem GetCurrentChestPassiveItem()
    {
        return chestPassiveItem;
    }

    public PassiveItem GetCurrentNeckPassiveItem()
    {
        return neckPassiveItem;
    }

    public PassiveItem GetCurrentArmPassiveItem()
    {
        return armPassiveItem;
    }

    public PassiveItem GetCurrentFingerPassiveItem()
    {
        return fingerPassiveItem;
    }

    public PassiveItem GetCurrentWaistPassiveItem()
    {
        return waistPassiveItem;
    }

    public PassiveItem GetCurrentBackPassiveItem()
    {
        return backPassiveItem;
    }

    public PassiveItem GetCurrentLegPassiveItem()
    {
        return legPassiveItem;
    }
}
