using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public IReceivable receivable;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform bookStatsPageContainer;
    [HideInInspector] public bool swapCancelled;
    [HideInInspector] public Slot belongingSlot;
    [HideInInspector] public int originalIndexNum ;
    [HideInInspector] public bool transactionOnTheSameSet;
    [HideInInspector] public bool contactSuccessful;
    [HideInInspector] public bool justMoveNotSwap;
    [HideInInspector] public bool isLockIcon;
    [HideInInspector] public bool dragMainSlotOff;

    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Canvas canvas;
    Vector2 originalPosition;
    Player player;
    GameObject dropButton;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
    }

    private void OnEnable()
    {
        player = GameManager.Instance.GetPlayer();
        bookStatsPageContainer = GetTopLevelParent(transform, 4);
        belongingSlot = GetTopLevelParent(transform, 2).GetComponent<Slot>();
        dropButton = InventoryManager.Instance.GetDropButtonObject();
        originalIndexNum = player.currentWeaponSlotSetIndex;
        transactionOnTheSameSet = true;

        GetReceivableInfo();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;  
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(bookStatsPageContainer); // Go 4 level up for preventing drag interruption while set change
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;

        // Check if the item is dropped on one of the weapon set buttons
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag(Settings.weaponSetButton))
        {
            if (belongingSlot.slotType == SlotType.Active || belongingSlot.slotType == SlotType.Passive) return;

            WeaponSetButton weaponSetButton = eventData.pointerEnter.GetComponent<WeaponSetButton>();
            int setIndex ;

            // Dragging happening between different sets
            if (weaponSetButton != null)
            {
                setIndex = weaponSetButton.SetIndex;

                // Ensure the set switch is only triggered if it's different from the current set
                if (player.currentWeaponSlotSetIndex != setIndex)
                {
                    transactionOnTheSameSet = false;

                    // Make drop button inactive while dragging a weapon to another set
                    dropButton.SetActive(false);

                    // Switch to the desired weapon set
                    GameManager.Instance.GoToWeaponSetWithIndex(setIndex);
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;  // Reset the transparency
        canvasGroup.blocksRaycasts = true;  // Re-enable blocking raycasts

        ResetPosition();

        dropButton.SetActive(true);

        // Update weapon set in book ui
        StaticEventHandler.CallWeaponSwitchedEventForBook();

        if (contactSuccessful)
        {

            if (justMoveNotSwap) return; // This is only valid for swaps
        }

        transactionOnTheSameSet = true;

        // Check if the drag was canceled due to an invalid swap
        if (swapCancelled)
        {
            if (belongingSlot != null)
            {
                if (belongingSlot.slotType == SlotType.WeaponMainHand)
                {
                    Weapon weapon = (Weapon)receivable;
                    player.weaponSlotSetArray[weapon.weaponBelongingToWhichMainHandSet - 1][0] = weapon;
                }
                else if (belongingSlot.slotType == SlotType.WeaponOffHand)
                {
                    Weapon weapon = (Weapon)receivable;
                    player.weaponSlotSetArray[weapon.weaponBelongingToWhichOffHandSet - 1][1] = weapon;
                }
            }

            if (!transactionOnTheSameSet)
            {
                if (belongingSlot.transform.GetChild(1) != null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    dropButton.SetActive(true);
                }
            }

            return;
        }

        if (eventData.pointerEnter != null)
        {
            if (eventData.pointerEnter.CompareTag(Settings.weaponSetButton))
            {
                // Return to original parent if not dropped on a valid slot
                RevertWeaponBackToBelongingSlots();
                ResetPosition();
            }
            else if (eventData.pointerEnter.CompareTag(Settings.dropButton))
            {
                int originalIndex = InventoryManager.Instance.GetOriginalSlotIndex();

                // Drop off hand weapon
                if (originalParent.GetChild(0).GetComponent<DraggableItem>().belongingSlot.slotType == SlotType.WeaponOffHand)
                {
                    Destroy(originalParent.GetChild(0).gameObject);
                }
                // Drop main hand weapon if off-hand is empty
                else if (originalParent.GetChild(0).GetComponent<DraggableItem>().belongingSlot.slotType == SlotType.WeaponMainHand &&
                    player.weaponSlotSetArray[originalIndex - 1][1] == null)
                {
                    Destroy(originalParent.GetChild(0).gameObject);
                }
            }
            else if (eventData.pointerEnter.CompareTag(Settings.bookCover))
            {
                RevertWeaponBackToBelongingSlots();
            }
            else if (eventData.pointerEnter.transform.parent.CompareTag(Settings.mainHandSlot) || eventData.pointerEnter.transform.parent.CompareTag(Settings.offHandSlot) ||
                eventData.pointerEnter.transform.CompareTag(Settings.mainHandSlot) || eventData.pointerEnter.transform.CompareTag(Settings.offHandSlot))
            {
                // If hits one of the slots, don't do anything
            }
            else
            {
                // Return to original parent if not dropped on a valid slot
                RevertWeaponBackToBelongingSlots();
                ResetPosition();
            }
        }
        else
        {
            // Return to original parent if not dropped on a valid slot
            RevertWeaponBackToBelongingSlots();
            ResetPosition();
        }

        dropButton.SetActive(true);

        contactSuccessful = false;
        justMoveNotSwap = false;
    }

    private void RevertWeaponBackToBelongingSlots()
    {
        if (belongingSlot != null)
        {
            if (belongingSlot.slotType == SlotType.WeaponMainHand)
            {
                Weapon weapon = (Weapon)receivable;
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = weapon;
            }
            else if (belongingSlot.slotType == SlotType.WeaponOffHand)
            {
                Weapon weapon = (Weapon)receivable;
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
            }
        }
    }

    public Weapon GetDraggedWeapon() => (Weapon)receivable;

    public ActiveItem GetDraggedActiveItem() => (ActiveItem)receivable;

    public PassiveItem GetDraggedPassiveItem() => (PassiveItem)receivable;

    public int GetSetNumber()
    {
        if (belongingSlot != null)
        {
            if (belongingSlot.slotType == SlotType.WeaponMainHand)
            {
                Weapon weapon = (Weapon)receivable;
                return weapon.weaponBelongingToWhichMainHandSet;
            }
            else if (belongingSlot.slotType == SlotType.WeaponOffHand)
            {
                Weapon weapon = (Weapon)receivable;
                return weapon.weaponBelongingToWhichOffHandSet;
            }
        }

        return -1;
    }

    public void UpdateSetNumber(int newSetIndex)
    {
        if (belongingSlot != null)
        {
            if (belongingSlot.slotType == SlotType.WeaponMainHand)
            {
                Weapon weapon = (Weapon)receivable;
                weapon.weaponBelongingToWhichMainHandSet = newSetIndex;
            }
            else if (belongingSlot.slotType == SlotType.WeaponOffHand)
            {
                Weapon weapon = (Weapon)receivable;
                weapon.weaponBelongingToWhichOffHandSet = newSetIndex;
            }
        }
    }

    private IReceivable GetReceivableInfo()
    {
        if (belongingSlot != null)
        {
            switch (belongingSlot.slotType)
            {
                case SlotType.Passive:
                    switch (belongingSlot.passiveItemSlotName)
                    {
                        case PassiveItemSlotName.Head:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentHeadPassiveItem();
                            break;
                        case PassiveItemSlotName.Chest:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentChestPassiveItem();
                            break;
                        case PassiveItemSlotName.Neck:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentNeckPassiveItem();
                            break;
                        case PassiveItemSlotName.Finger:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentFingerPassiveItem();
                            break;
                        case PassiveItemSlotName.Back:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentBackPassiveItem();
                            break;
                        case PassiveItemSlotName.Waist:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentWaistPassiveItem();
                            break;
                        case PassiveItemSlotName.Arm:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentArmPassiveItem();
                            break;
                        case PassiveItemSlotName.Leg:
                            receivable = GameManager.Instance.GetPlayer().selectedPassiveItem.GetCurrentLegPassiveItem();
                            break;
                        default:
                            break;
                    }
                    break;
                case SlotType.Active:
                    receivable = GameManager.Instance.GetPlayer().selectedActiveItem.GetCurrentActiveItem();
                    break;
                case SlotType.WeaponMainHand:
                    receivable = GameManager.Instance.GetPlayer().activeWeapon.GetCurrentMainHandWeapon();
                    break;
                case SlotType.WeaponOffHand:
                    receivable = GameManager.Instance.GetPlayer().activeWeapon.GetCurrentOffHandWeapon();
                    break;
                default:
                    break;
            }
        }

        return receivable;
    }

    public Transform GetTopLevelParent(Transform child, int level)
    {
        Transform currentParent = child;

        for (int i = 0; i < level; i++)
        {
            if (currentParent.parent != null)
            {
                currentParent = currentParent.parent;
            }
            else
            {
                // If the parent is null, we've reached the top of the hierarchy
                break;
            }
        }

        return currentParent;
    }

    public void ResetPosition()
    {
        transform.SetParent(originalParent);
        rectTransform.anchoredPosition = originalPosition;
    }
}
