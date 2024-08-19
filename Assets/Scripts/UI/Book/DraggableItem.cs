using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Weapon weapon;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform bookStatsPageContainer;
    [HideInInspector] public bool swapCanceled;
    [HideInInspector] public Slot belongingSlot;
    [HideInInspector] public int originalIndexNum ;
    [HideInInspector] public bool transactionOnTheSameSet;
    [HideInInspector] public bool contactSuccessful;
    [HideInInspector] public bool justMoveNotSwap;

    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Canvas canvas;
    Vector2 originalPosition;
    Player player;
    Transform dropButton;
    Transform buttonSetContainer;
    bool originalSetSwitched;

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
        dropButton = bookStatsPageContainer.GetChild(7);
        buttonSetContainer = bookStatsPageContainer.GetChild(3).GetChild(0);
        originalIndexNum = player.currentWeaponSlotSetIndex;
        transactionOnTheSameSet = true;

        if (IsWeaponOnMainHand())
        {
            weapon = GameManager.Instance.GetPlayer().activeWeapon.GetCurrentMainHandWeapon();
        }
        else
        {
            weapon = GameManager.Instance.GetPlayer().activeWeapon.GetCurrentOffHandWeapon();
        }     
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
            WeaponSetButton weaponSetButton = eventData.pointerEnter.GetComponent<WeaponSetButton>();
            int setIndex;

            if (weaponSetButton != null)
            {
                setIndex = weaponSetButton.SetIndex;

                // Ensure the set switch is only triggered if it's different from the current set
                if (player.currentWeaponSlotSetIndex != setIndex)
                {
                    if (!originalSetSwitched)
                    {
                        transactionOnTheSameSet = false;

                        // Make drop button inactive while dragging a weapon to another set
                        dropButton.gameObject.SetActive(false);

                        BackGroundAndEquippedSlotTransactions();

                        // Switch to the desired weapon set
                        GameManager.Instance.GoToWeaponSetWithIndex(setIndex);

                        // Change original set switch flag
                        originalSetSwitched = true;
                    }
                }
            }
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;  // Reset the transparency
        canvasGroup.blocksRaycasts = true;  // Re-enable blocking raycasts

        ResetPosition();

        if (contactSuccessful)
        {
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
            // Return to original parent if not dropped on a valid slot
            if (IsWeaponOnMainHand())
            {
                player.weaponSlotSetArray[weapon.weaponBelongingToWhichMainHandSet - 1][0] = weapon;
            }
            else
            {
                player.weaponSlotSetArray[weapon.weaponBelongingToWhichOffHandSet - 1][1] = weapon;
            }

            if (!transactionOnTheSameSet)
            {
                if (belongingSlot.transform.GetChild(1) != null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    dropButton.gameObject.SetActive(true);
                }
            }

            return;
        }

        if (eventData.pointerEnter != null)
        {

            if (eventData.pointerEnter.CompareTag(Settings.weaponSetButton))
            {
                // Return to original parent if not dropped on a valid slot
                if (IsWeaponOnMainHand())
                {
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = weapon;
                }
                else
                {
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
                }
                ResetPosition();
            }
            else if (eventData.pointerEnter.CompareTag(Settings.dropButton))
            {
                // For drop issuse, don't assign weapon to null reference
            }
            else if (eventData.pointerEnter.CompareTag(Settings.bookCover))
            {
                if (IsWeaponOnMainHand())
                {
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = weapon;
                }
                else
                {
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
                }
            }
            else if (eventData.pointerEnter.transform.parent.CompareTag(Settings.mainHandSlot) || eventData.pointerEnter.transform.parent.CompareTag(Settings.offHandSlot) ||
                eventData.pointerEnter.transform.CompareTag(Settings.mainHandSlot) || eventData.pointerEnter.transform.CompareTag(Settings.offHandSlot))
            {
                // If hits one of the slots, don't do anything
            }
            else
            {
                // Return to original parent if not dropped on a valid slot
                if (IsWeaponOnMainHand())
                {
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = weapon;
                }
                else
                {
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
                }
                ResetPosition();
            }
        }
        else
        {
            // Return to original parent if not dropped on a valid slot
            if (IsWeaponOnMainHand())
            {
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = weapon;
            }
            else
            {
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
            }
            ResetPosition();
        }

        dropButton.gameObject.SetActive(true);

        // Change original set switch flag
        originalSetSwitched = true;

        contactSuccessful = false;
        justMoveNotSwap = false;
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
        Transform offHandWeaponBackground = bookStatsPageContainer.GetChild(4).GetChild(1).GetChild(0);
        Transform offHandWeaponEquipped = bookStatsPageContainer.GetChild(4).GetChild(1).GetChild(1);
        offHandWeaponBackground.gameObject.SetActive(false);
        offHandWeaponEquipped.gameObject.SetActive(true);
    }

    public bool IsWeaponOnMainHand()
    {
        if (belongingSlot != null)
        {
            if (belongingSlot.isMainHand)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        return false;
    }

    public Weapon GetDraggedWeapon() => weapon;

    public int GetSetNumber()
    {
        if (IsWeaponOnMainHand())
        {
            return weapon.weaponBelongingToWhichMainHandSet;
        }
        else
        {
            return weapon.weaponBelongingToWhichOffHandSet;
        }
    }

    public void UpdateSetNumber(int newSetIndex)
    {
        if (IsWeaponOnMainHand())
        {
            weapon.weaponBelongingToWhichMainHandSet = newSetIndex;
        }
        else
        {
            weapon.weaponBelongingToWhichOffHandSet = newSetIndex;
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
