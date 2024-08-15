using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler
{
    public bool isMainHand;

    Player player;
    Transform backgroundTransform;
    Transform equippedTransform;

    private void Start()
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

    private void SwapItems(DraggableItem draggableItem, Transform currentChild)
    {
        BackgroundAndEquippedSlotTransactions(draggableItem, true, currentChild);
    }

    private void MoveItemToSlot(DraggableItem draggableItem)
    {
        // Empty previous slot
        if (draggableItem.IsWeaponOnMainHand()) // Detect weapon's hand and remove it from list
        {
            player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichMainHandSet - 1][0] = null;
        }
        else
        {
            player.weaponSlotSetArray[draggableItem.weapon.weaponBelongingToWhichOffHandSet - 1][1] = null;
        }

        BackgroundAndEquippedSlotTransactions(draggableItem, false);
    }

    private void BackgroundAndEquippedSlotTransactions(DraggableItem draggableItem, bool isSwapping, Transform currentChild = null)
    {
        if (isMainHand)
        {
            // If trying to place shild to main-hand, cancel the transaction
            if (draggableItem.weapon.weaponDetails.weaponClass == WeaponClass.Shield)
            {
                draggableItem.swapCanceled = true;
                return;
            }   

            int draggedItemIndex = draggableItem.weapon.weaponBelongingToWhichMainHandSet > 0 ?
                draggableItem.weapon.weaponBelongingToWhichMainHandSet : draggableItem.weapon.weaponBelongingToWhichOffHandSet;

            switch (draggedItemIndex)
            {
                case 1:
                    draggableItem.weapon.onMaindHand = true;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichMainHandSet = 1;
                    player.playerControl.PopulateMainHandWeaponsToBook(draggableItem.weapon, true);
                    player.ActivateWeapon(false, draggableItem.weapon, false, 1);
                    break;
                case 2:
                    draggableItem.weapon.onMaindHand = true;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichMainHandSet = 2;
                    player.playerControl.PopulateMainHandWeaponsToBook(draggableItem.weapon, true);
                    player.ActivateWeapon(false, draggableItem.weapon, false, 2);
                    break;
                case 3:
                    draggableItem.weapon.onMaindHand = true;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichMainHandSet = 3;
                    player.playerControl.PopulateMainHandWeaponsToBook(draggableItem.weapon, true);
                    player.ActivateWeapon(false, draggableItem.weapon, false, 3);
                    break;
                default:
                    break;
            }
        }
        else
        {
            if (isSwapping)
            {
                // If trying to place shild to main-hand, cancel the transaction
                if (currentChild.GetComponent<DraggableItem>().weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    draggableItem.swapCanceled = true;
                    return;
                }

                // If trying to place two-weapon to main-hand, cancel the transaction
                if (currentChild.GetComponent<DraggableItem>().weapon.weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    draggableItem.swapCanceled = true;
                    return;
                }
            }

            int draggedItemIndex = draggableItem.weapon.weaponBelongingToWhichOffHandSet > 0 ?
                draggableItem.weapon.weaponBelongingToWhichOffHandSet : draggableItem.weapon.weaponBelongingToWhichOffHandSet;

            switch (draggedItemIndex)
            {
                case 1:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichOffHandSet = 1;
                    player.playerControl.PopulateOffHandWeaponsToBook(draggableItem.weapon);
                    player.ActivateWeapon(false, draggableItem.weapon, true, 1);
                    break;
                case 2:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichOffHandSet = 2;
                    player.playerControl.PopulateOffHandWeaponsToBook(draggableItem.weapon);
                    player.ActivateWeapon(false, draggableItem.weapon, true, 2);
                    break;
                case 3:
                    draggableItem.weapon.onMaindHand = false;
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItem.weapon;
                    draggableItem.weapon.weaponBelongingToWhichOffHandSet = 3;
                    player.playerControl.PopulateOffHandWeaponsToBook(draggableItem.weapon);
                    player.ActivateWeapon(false, draggableItem.weapon, true, 3);
                    break;
                default:
                    break;
            }
        }

        if (isSwapping)
        {
            // Set the original item to the dragged item's previous parent
            currentChild.SetParent(draggableItem.originalParent);
            currentChild.localPosition = Vector3.zero;

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

        // Activate equipped transform and disable background
        backgroundTransform.gameObject.SetActive(false);
        equippedTransform.gameObject.SetActive(true);

        // Move the dragged item to the empty slot
        draggableItem.transform.SetParent(equippedTransform);
        draggableItem.transform.localPosition = Vector3.zero;
    }
}
