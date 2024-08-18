using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public bool isMainHand;

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
            Transform currentChild = null;

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
                // If slot is not occupied, just relocate selected item
                MoveItemToSlot(draggableItem);
            }
        }
    }

    private void CheckIndexNumDebug(DraggableItem draggableItem)
    {
        if (draggableItem.weapon.onMaindHand)
        {
            Debug.Log("Draggable item's set number is " + draggableItem.weapon.weaponBelongingToWhichMainHandSet);
        }
        else
        {
            Debug.Log("Draggable item's set number is " + draggableItem.weapon.weaponBelongingToWhichOffHandSet);
        }

        Debug.Log("Current set index number is " + player.currentWeaponSlotSetIndex);
    }

    private void SwapItems(DraggableItem draggableItem, Transform currentChild)
    {
        DraggableItem currentSlotsDraggableItem = currentChild.GetComponent<DraggableItem>();

        // Move slot's weapon to draggable item's previous slot
        // Draggable item is on main hand
        if (draggableItem.weapon.onMaindHand)
        {
            // Slot and draggable items are both main hands
            if (currentSlotsDraggableItem.weapon.onMaindHand)
            {
                // Draggable item doesn't have an off-hand weapon
                if (player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                {
                    // Slot's current set doesn't have an off-hand weapon
                    if (player.weaponSlotSetArray[currentSlotsDraggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                    {
                        // Both set's off-hand slots are empty so swap is successful
                        SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                    }
                    else // Slot's current set has an off-hand weapon
                    {
                        // Draggable item is two-handed weapon
                        if (player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
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
                    if (player.weaponSlotSetArray[currentSlotsDraggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                    {
                        // Slot's current weapon is a two-handed weapon
                        if (player.weaponSlotSetArray[currentSlotsDraggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
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
                if (player.weaponSlotSetArray[currentSlotsDraggableItem.weapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    // Slot item is a shield, so swap is canceled
                    draggableItem.swapCanceled = true;
                    return;
                }
                // Draggable item is a two-handed weapon
                else if (player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    // Draggable item is two-handed weapon, so swap is canceled
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
            if (currentSlotsDraggableItem.weapon.onMaindHand)
            {
                // Draggable item is a shield
                if (draggableItem.weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    // Draggable item is a shield, so swap is canceled
                    draggableItem.swapCanceled = true;
                    return;
                }
                // Slot item is a two-handed weapon
                else if (player.weaponSlotSetArray[currentSlotsDraggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    // Slot item is two-handed weapon, so swap is canceled
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

        Debug.Log("Dragging " + draggableItem.weapon.weaponDetails.weaponName + " onto " + currentChild.GetComponent<DraggableItem>().weapon.weaponDetails.weaponName);

        if (isMainHand)
        {
            Debug.Log(draggableItem.weapon.weaponDetails.weaponName + " is main Hand");
        }
        else
        {
            Debug.Log(draggableItem.weapon.weaponDetails.weaponName + " is off-Hand");
        }

        if (currentChild.GetComponent<DraggableItem>().belongingSlot.isMainHand)
        {
            Debug.Log(currentChild.GetComponent<DraggableItem>().weapon.weaponDetails.weaponName + " is main Hand");
        }
        else
        {
            Debug.Log(currentChild.GetComponent<DraggableItem>().weapon.weaponDetails.weaponName + " is off-Hand");
        }
    }

    private void MoveItemToSlot(DraggableItem draggableItem)
    {
        // Empty previous slot
        if (draggableItem.weapon.onMaindHand) // Detect weapon's hand and remove it from list
        {
            player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0] = null;
        }
        else
        {
            player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichOffHandSet - 1][1] = null;
        }

        BackgroundAndEquippedSlotTransactions(draggableItem);
    }

    private void SwapProcess(DraggableItem draggableItem, DraggableItem currentSlotsDraggableItem, ItemSwapPos itemSwapPos)
    {
        switch (itemSwapPos)
        {
            case ItemSwapPos.None:
                break;
            case ItemSwapPos.DragMainSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0] = currentSlotsDraggableItem.weapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                currentSlotsDraggableItem.weapon.weaponBelongingToWhichMainHandSet = draggableItem.weapon.weaponBelongingToWhichMainHandSet;
                draggableItem.weapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                Destroy(equippedTransform.GetChild(0).gameObject);
                draggableItem.weapon.onMaindHand = true;
                currentSlotsDraggableItem.weapon.onMaindHand = true;
                player.ActivateWeapon(draggableItem.weapon, true, player.currentWeaponSlotSetIndex);
                break;
            case ItemSwapPos.DragMainSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0] = currentSlotsDraggableItem.weapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                currentSlotsDraggableItem.weapon.weaponBelongingToWhichOffHandSet = 0;
                currentSlotsDraggableItem.weapon.weaponBelongingToWhichMainHandSet = draggableItem.weapon.weaponBelongingToWhichMainHandSet;
                currentSlotsDraggableItem.weapon.onMaindHand = true;
                draggableItem.weapon.weaponBelongingToWhichMainHandSet = 0;
                draggableItem.weapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItem.weapon.onMaindHand = false;

                player.ActivateWeapon(draggableItem.weapon, true, player.currentWeaponSlotSetIndex);
                if (draggableItem.transactionOnTheSameSet)
                {
                    player.ActivateWeapon(currentSlotsDraggableItem.weapon, false, player.currentWeaponSlotSetIndex);
                    StaticEventHandler.CallWeaponAddedToMainHandBook(draggableItem.weapon, false);
                    Destroy(GameManager.Instance.GetOffHandEquippedSlot().GetChild(0).gameObject);
                }
                else
                {
                    Destroy(equippedTransform.GetChild(0).gameObject);
                }

                StaticEventHandler.CallWeaponAddedToOffHandBook(draggableItem.weapon);
                break;
            case ItemSwapPos.DragOffSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichOffHandSet - 1][1] = currentSlotsDraggableItem.weapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                currentSlotsDraggableItem.weapon.weaponBelongingToWhichMainHandSet = 0;
                currentSlotsDraggableItem.weapon.weaponBelongingToWhichOffHandSet = draggableItem.weapon.weaponBelongingToWhichOffHandSet;
                currentSlotsDraggableItem.weapon.onMaindHand = false;
                draggableItem.weapon.weaponBelongingToWhichOffHandSet = 0;
                draggableItem.weapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItem.weapon.onMaindHand = true;

                player.ActivateWeapon(draggableItem.weapon, false, player.currentWeaponSlotSetIndex);
                if (draggableItem.transactionOnTheSameSet)
                {
                    player.ActivateWeapon(currentSlotsDraggableItem.weapon, true, player.currentWeaponSlotSetIndex);
                    StaticEventHandler.CallWeaponSwappedAtOffHand(currentSlotsDraggableItem.weapon);
                    StaticEventHandler.CallWeaponSwappedAtMainHand(draggableItem.weapon);
                    Destroy(GameManager.Instance.GetOffHandEquippedSlot().GetChild(0).gameObject);
                    Destroy(equippedTransform.GetChild(1).gameObject);
                }
                else
                {
                    Destroy(equippedTransform.GetChild(0).gameObject);
                    StaticEventHandler.CallWeaponAddedToMainHandBook(draggableItem.weapon, false);
                }

                break;
            case ItemSwapPos.DragOffSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichOffHandSet - 1][1] = currentSlotsDraggableItem.weapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                currentSlotsDraggableItem.weapon.weaponBelongingToWhichOffHandSet = draggableItem.weapon.weaponBelongingToWhichOffHandSet;
                draggableItem.weapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                Destroy(equippedTransform.GetChild(0).gameObject);
                draggableItem.weapon.onMaindHand = false;
                currentSlotsDraggableItem.weapon.onMaindHand = false;
                player.ActivateWeapon(draggableItem.weapon, true, player.currentWeaponSlotSetIndex);
                break;
            default:
                break;
        }
    }

    private void BackgroundAndEquippedSlotTransactions(DraggableItem draggableItem)
    {
        if (isMainHand)
        {
            // If trying to place shild to main-hand, cancel the transaction
            if (draggableItem.weapon.weaponDetails.weaponClass == WeaponClass.Shield)
            {
                draggableItem.swapCanceled = true;
                return;
            }

            int draggedItemIndex;

            if (draggableItem.weapon.onMaindHand)
            {
                draggedItemIndex = draggableItem.weapon.weaponBelongingToWhichMainHandSet;
            }
            else
            {
                draggedItemIndex = draggableItem.weapon.weaponBelongingToWhichOffHandSet;
            }

            switch (draggedItemIndex)
            {
                case 1:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    player.ActivateWeapon(draggableItem.weapon, false, 1);
                    break;
                case 2:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    player.ActivateWeapon(draggableItem.weapon, false, 2);
                    break;
                case 3:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    player.ActivateWeapon(draggableItem.weapon, false, 3);
                    break;
                default:
                    break;
            }
        }
        else
        {
            int draggedItemIndex;

            if (draggableItem.weapon.onMaindHand)
            {
                draggedItemIndex = draggableItem.weapon.weaponBelongingToWhichMainHandSet;
            }
            else
            {
                draggedItemIndex = draggableItem.weapon.weaponBelongingToWhichOffHandSet;
            }

            switch (draggedItemIndex)
            {
                case 1:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    player.ActivateWeapon(draggableItem.weapon, true, 1);
                    break;
                case 2:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    player.ActivateWeapon(draggableItem.weapon, true, 2);
                    break;
                case 3:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    player.ActivateWeapon(draggableItem.weapon, true, 3);
                    break;
                default:
                    break;
            }
        }

        // Activate equipped transform and disable background
        backgroundTransform.gameObject.SetActive(false);
        equippedTransform.gameObject.SetActive(true);

        // Move the dragged item to the empty slot
        draggableItem.transform.SetParent(equippedTransform);
        draggableItem.transform.localPosition = Vector3.zero;
    }
}
