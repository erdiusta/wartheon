using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public IReceivable receivable;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform bookStatsPageContainer;
    [HideInInspector] public bool swapCanceled;
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

    private IReceivable GetReceivableInfo()
    {
        if (belongingSlot != null)
        {
            switch (belongingSlot.slotType)
            {
                case SlotType.Passive:
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

            if (weaponSetButton != null)
            {
                setIndex = weaponSetButton.SetIndex;

                // Ensure the set switch is only triggered if it's different from the current set
                if (player.currentWeaponSlotSetIndex != setIndex)
                {
                    transactionOnTheSameSet = false;

                    // Make drop button inactive while dragging a weapon to another set
                    dropButton.SetActive(false);

                    BackGroundAndEquippedSlotTransactions();

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

        if (contactSuccessful)
        {
            if (dragMainSlotOff)
            {
                Destroy(InventoryManager.Instance.mainHandEquippedSlot.GetChild(1)?.gameObject); // Destroy unnecessary duplicate weapon image on slots if has
            }

            if (justMoveNotSwap) return; // This is only valid for swaps

            if(belongingSlot.transform.GetChild(1).childCount > 1)
            {
                Destroy(belongingSlot.transform.GetChild(1).GetChild(belongingSlot.transform.GetChild(1).childCount - 1).gameObject);
            }
        }

        transactionOnTheSameSet = true;

        // Check if the drag was canceled due to an invalid swap
        if (swapCanceled)
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
                // For drop issue, don't assign weapon to null reference
                if (player.weaponSlotSetArray[InventoryManager.Instance.GetOriginalSlotIndex() - 1][0] == null && 
                    player.weaponSlotSetArray[InventoryManager.Instance.GetOriginalSlotIndex() - 1][1] == null)
                {
                    Destroy(belongingSlot.transform.GetChild(1).GetChild(belongingSlot.transform.GetChild(1).childCount - 1).gameObject);
                    receivable = null;
                    Destroy(this);
                }
                if (belongingSlot != null)
                {
                    if (belongingSlot.slotType == SlotType.WeaponMainHand)
                    {
                        if (player.activeWeapon.weaponToBeDropped.weaponDetails.wieldType == WieldType.TwoHanded)
                        {
                            // Clear lock icon at off-hand slot when dropping two-handed weapon
                            InventoryManager.Instance.ClearIntendedElementInOffHandEquippedSlot(0);
                        }
                    }
                    else if (belongingSlot.slotType == SlotType.Active)
                    {
                        // Clear children duplicate slots if has
                        InventoryManager.Instance.ClearIntendedElementInActiveItemEquippedSlot();
                    }

                    receivable = null;
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

    private void BackGroundAndEquippedSlotTransactions()
    {
        EnableEquippedParentAndDisableBackgroundForMainHand();
        EnableEquippedParentAndDisableBackgroundForOffHand();
    }

    public void EnableEquippedParentAndDisableBackgroundForMainHand()
    {
        Transform mainWeaponBackground = bookStatsPageContainer.GetChild(3).GetChild(1).GetChild(0);
        Transform mainWeaponEquipped = bookStatsPageContainer.GetChild(3).GetChild(1).GetChild(1);
        mainWeaponBackground.gameObject.SetActive(false);
        mainWeaponEquipped.gameObject.SetActive(true);
    }

    public void EnableEquippedParentAndDisableBackgroundForOffHand()
    {
        Transform offHandWeaponBackground = bookStatsPageContainer.GetChild(4).GetChild(0).GetChild(0);
        Transform offHandWeaponEquipped = bookStatsPageContainer.GetChild(4).GetChild(0).GetChild(1);
        offHandWeaponBackground.gameObject.SetActive(false);
        offHandWeaponEquipped.gameObject.SetActive(true);
    }

    public Weapon GetDraggedWeapon() => (Weapon)receivable;

    public ActiveItem GetDraggedActiveItem() => (ActiveItem)receivable;

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
