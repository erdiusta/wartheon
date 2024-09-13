using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public SlotType slotType;

    Player player;
    Transform backgroundTransform;
    Transform equippedTransform;

    private void OnEnable()
    {
        player = GameManager.Instance.GetPlayer();
        backgroundTransform = transform.GetChild(0);
        equippedTransform = transform.GetChild(1);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedItem = eventData.pointerDrag;
        DraggableItem draggableItem = draggedItem.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            // If dragged item is not a weapon, cancel the swap or move
            if (draggableItem.receivable is not Weapon) return;

            Transform currentChild = null;

            draggableItem.contactSuccessful = true;

            if (ReferenceEquals(draggableItem.belongingSlot, this) && draggableItem.transactionOnTheSameSet) return;

            if (equippedTransform != null)
            {
                // Check if the slot is occupied
                if (equippedTransform.childCount > 0)
                {
                    currentChild = equippedTransform.GetChild(0);
                }
            }

            if (currentChild != null)
            {
                // If slot is occupied, swap items
                SwapItems(draggableItem, currentChild);
            }
            else
            {
                draggableItem.justMoveNotSwap = true;

                // If slot is not occupied, just relocate selected item
                MoveItemToSlot(draggableItem);
            }
        }
    }

    private void SwapItems(DraggableItem draggableItem, Transform currentChild)
    {
        DraggableItem currentSlotsDraggableItem = currentChild.GetComponent<DraggableItem>();

        if (currentSlotsDraggableItem.isLockIcon) return;

        Weapon draggableItemWeapon = (Weapon)draggableItem.receivable;
        Weapon currentSlotsDraggableItemWeapon = (Weapon)currentSlotsDraggableItem.receivable;

        // Move slot's weapon to draggable item's previous slot
        // Draggable item is on main hand
        if (draggableItemWeapon.onMaindHand)
        {
            if (currentSlotsDraggableItemWeapon == null)
            {
                // Draggable item is two-handed weapon, so swap is canceled

                draggableItem.swapCanceled = true;
                return;
            }
            // Slot and draggable items are both main hands
            else if (currentSlotsDraggableItemWeapon.onMaindHand)
            {
                // Draggable item doesn't have an off-hand weapon
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                {
                    // Slot's current set doesn't have an off-hand weapon
                    if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                    {
                        // Both set's off-hand slots are empty so swap is successful
                        SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                    }
                    else // Slot's current set has an off-hand weapon
                    {
                        // Draggable item is two-handed weapon
                        if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                        {
                            // Draggable item is two-handed weapon, so swap is canceled
                            draggableItem.swapCanceled = true;
                            return;
                        }
                        else
                        {
                            // Draggable item is not a two-handed weapon, so swap is successful
                            SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                        }
                    }
                }
                // Draggable item has an off-hand weapon
                else
                {
                    // Slot's current set doesn't have an off-hand weapon
                    if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                    {
                        // Slot's current weapon is a two-handed weapon
                        if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                        {
                            // Draggable item has an off-hand and slot item is a two-handed weapon, so swap is canceled
                            draggableItem.swapCanceled = true;
                            return;
                        }
                        else
                        {
                            // Draggable item has an off-hand and but slot item is a one-handed weapon, so swap is succesful
                            SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                        }
                    }
                    // Slot's current set has an off-hand weapon
                    else
                    {
                        // Both draggable and slot item sets have an off-hand weapon. This means both items are one-handed, so swap is succesfful
                        SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                    }
                }
            }
            // Draggable item is at main-hand and slot item is at off-hand
            else
            {
                // Slot item is a shield
                if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    // Slot item is a shield, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    draggableItem.swapCanceled = true;
                    return;
                }
                // Draggable item is a two-handed weapon
                else if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    // Draggable item is two-handed weapon, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandCantBeAddedToTwoHanded);
                    draggableItem.swapCanceled = true;
                    return;
                }
                else
                {
                    // Neither slot item is a shield nor draggable item is a two-handed weapon, so swap is succesfful
                    SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotOff);
                }
            }
        }
        // Draggable item is on off-hand
        else
        {
            // Draggable item is on off-hand and slot item is on main hand
            if (currentSlotsDraggableItemWeapon.onMaindHand)
            {
                // Draggable item is a shield
                if (draggableItemWeapon.weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    // Draggable item is a shield, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    draggableItem.swapCanceled = true;
                    return;
                }
                // Slot item is a two-handed weapon
                else if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    // Slot item is two-handed weapon, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandCantBeAddedToTwoHanded);
                    draggableItem.swapCanceled = true;
                    return;
                }
                else
                {
                    // Neither draggable item is a shield nor slot item is a two-handed weapon, so swap is succesfful
                    SwapProcess(draggableItem,  currentSlotsDraggableItem, ItemSwapPos.DragOffSlotMain);
                }
            }
            // Both draggable and slot items are off-hand
            else
            {
                // Both draggable and slot items are off-hand; it means they are either one-handed or a shield, so swap is successful
                SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragOffSlotOff);
            }
        }
    }

    private void MoveItemToSlot(DraggableItem draggableItem)
    {
        BackgroundAndEquippedSlotTransactions(draggableItem);
    }

    private void SwapProcess(DraggableItem draggableItem, DraggableItem currentSlotsDraggableItem, ItemSwapPos itemSwapPos)
    {
        Weapon draggableItemWeapon = (Weapon)draggableItem.receivable;
        Weapon currentSlotsDraggableItemWeapon = (Weapon)currentSlotsDraggableItem.receivable;

        switch (itemSwapPos)
        {
            case ItemSwapPos.None:
                break;
            case ItemSwapPos.DragMainSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponBelongingToWhichMainHandSet;
                currentSlotsDraggableItemWeapon.onMaindHand = true;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMaindHand = true;
                player.playerControl.SetWeaponSetByIndex(true);
                break;
            case ItemSwapPos.DragMainSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponBelongingToWhichMainHandSet;
                currentSlotsDraggableItemWeapon.onMaindHand = true;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMaindHand = false;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(true);
                }
                else
                {
                    Destroy(equippedTransform.GetChild(0).gameObject);
                    StaticEventHandler.CallWeaponAddedToOffHandBook(draggableItemWeapon);
                }
                break;
            case ItemSwapPos.DragOffSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponBelongingToWhichOffHandSet;
                currentSlotsDraggableItemWeapon.onMaindHand = false;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMaindHand = true;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(true);
                }
                else
                {
                    Destroy(equippedTransform.GetChild(0).gameObject);
                    StaticEventHandler.CallWeaponAddedToMainHandBook(draggableItemWeapon, false);
                }

                break;
            case ItemSwapPos.DragOffSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponBelongingToWhichOffHandSet;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMaindHand = false;
                currentSlotsDraggableItemWeapon.onMaindHand = false;
                player.playerControl.SetWeaponSetByIndex(true);
                break;
            default:
                break;
        }
    }

    private void BackgroundAndEquippedSlotTransactions(DraggableItem draggableItem)
    {
        Weapon draggableItemWeapon = (Weapon)draggableItem.receivable;

        if (draggableItemWeapon.onMaindHand)
        {
            if (slotType == SlotType.WeaponMainHand)
            {
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] != null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EmptyOffHandFirst);
                    draggableItem.swapCanceled = true;
                    return;
                }

                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;

                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                player.ActivateWeapon(draggableItemWeapon, false, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true);
            }
            else
            {
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] != null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EmptyOffHandFirst);
                    draggableItem.swapCanceled = true;
                    return;
                }
                else
                {
                    if (draggableItemWeapon.weaponBelongingToWhichMainHandSet == player.currentWeaponSlotSetIndex)
                    {
                        GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                        draggableItem.swapCanceled = true;
                        return;
                    }
                }

                if (draggableItem.transactionOnTheSameSet)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                    draggableItem.swapCanceled = true;
                    return;
                }

                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                draggableItemWeapon.onMaindHand = false;

                draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                player.ActivateWeapon(draggableItemWeapon, false, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true);
            }
        }
        else
        {
            if (slotType == SlotType.WeaponMainHand)
            {
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    draggableItem.swapCanceled = true;
                    return;
                }
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = null;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                draggableItemWeapon.onMaindHand = true;

                draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                player.ActivateWeapon(draggableItemWeapon, true, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true);
            }
            else
            {
                if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] == null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EquipMainHandFirst);
                    draggableItem.swapCanceled = true;
                    return;
                }

                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = null;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;

                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                player.ActivateWeapon(draggableItemWeapon, true, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true);
            }
        }
    }
}
