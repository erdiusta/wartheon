using UnityEngine;
using UnityEngine.EventSystems;

public class DraggableItem : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public Weapon weapon;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public Transform bookStatsPageContainer;
    [HideInInspector] public bool swapCanceled;
    [HideInInspector] public Slot belongingSlot;

    CanvasGroup canvasGroup;
    RectTransform rectTransform;
    Canvas canvas;
    Vector2 originalPosition;
    Player player;

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
                    // Make previous weapon slot null
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = null;

                    Transform mainWeaponBackground = null;
                    Transform mainWeaponEquipped = null;
                    Transform offHandWeaponBackground = null;
                    Transform offHandWeaponEquipped = null;
                    BackGroundAndEquippedSlotTransactions(setIndex, ref mainWeaponBackground, ref mainWeaponEquipped, ref offHandWeaponBackground, ref offHandWeaponEquipped);

                    // Switch to the desired weapon set
                    GameManager.Instance.GoToWeaponSetWithIndex(setIndex);
                }
            }
        }
    }

    private void BackGroundAndEquippedSlotTransactions(int setIndex, ref Transform mainWeaponBackground, ref Transform mainWeaponEquipped, 
        ref Transform offHandWeaponBackground, ref Transform offHandWeaponEquipped)
    {
        if (IsWeaponOnMainHand())
        {
            switch (setIndex)
            {
                case 1:
                    if (player.weaponSlotSetArray[0][0] == null)
                    {
                        EnableEquippedParentAndDisableBackgroundForMainHand(out mainWeaponBackground, out mainWeaponEquipped);
                    }
                    break;
                case 2:
                    if (player.weaponSlotSetArray[1][0] == null)
                    {
                        EnableEquippedParentAndDisableBackgroundForMainHand(out mainWeaponBackground, out mainWeaponEquipped);
                    }
                    break;
                case 3:
                    if (player.weaponSlotSetArray[2][0] == null)
                    {
                        EnableEquippedParentAndDisableBackgroundForMainHand(out mainWeaponBackground, out mainWeaponEquipped);
                    }
                    break;
                default:
                    break;
            }
        }
        else
        {
            switch (setIndex)
            {
                case 1:
                    if (player.weaponSlotSetArray[0][1] == null)
                    {
                        EnableEquippedParentAndDisableBackgroundForOffHand(out offHandWeaponBackground, out offHandWeaponEquipped);
                    }
                    break;
                case 2:
                    if (player.weaponSlotSetArray[1][1] == null)
                    {
                        EnableEquippedParentAndDisableBackgroundForOffHand(out offHandWeaponBackground, out offHandWeaponEquipped);
                    }
                    break;
                case 3:
                    if (player.weaponSlotSetArray[2][1] == null)
                    {
                        EnableEquippedParentAndDisableBackgroundForOffHand(out offHandWeaponBackground, out offHandWeaponEquipped);
                    }
                    break;
                default:
                    break;
            }
        }
    }

    private void EnableEquippedParentAndDisableBackgroundForMainHand(out Transform mainWeaponBackground, out Transform mainWeaponEquipped)
    {
        mainWeaponBackground = bookStatsPageContainer.GetChild(3).GetChild(1).GetChild(0);
        mainWeaponEquipped = bookStatsPageContainer.GetChild(3).GetChild(1).GetChild(1);
        mainWeaponBackground.gameObject.SetActive(false);
        mainWeaponEquipped.gameObject.SetActive(true);
    }

    private void EnableEquippedParentAndDisableBackgroundForOffHand(out Transform offHandWeaponBackground, out Transform offHandWeaponEquipped)
    {
        offHandWeaponBackground = bookStatsPageContainer.GetChild(4).GetChild(1).GetChild(0);
        offHandWeaponEquipped = bookStatsPageContainer.GetChild(4).GetChild(1).GetChild(1);
        offHandWeaponBackground.gameObject.SetActive(false);
        offHandWeaponEquipped.gameObject.SetActive(true);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        canvasGroup.alpha = 1.0f;  // Reset the transparency
        canvasGroup.blocksRaycasts = true;  // Re-enable blocking raycasts

        ResetPosition();

        // Check if the drag was canceled due to an invalid swap
        if (swapCanceled)
        {
            // Reset the position of the dragged item to its original slot
            player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
            ResetPosition();
            return;
        }

        if (eventData.pointerEnter != null)
        {
            if (eventData.pointerEnter.CompareTag(Settings.weaponSetButton))
            {
                // Return to original parent if not dropped on a valid slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
                ResetPosition();
            }
            else if (!eventData.pointerEnter.CompareTag(Settings.mainHandSlot) || !eventData.pointerEnter.CompareTag(Settings.offHandSlot))
            {
                // Return to original parent if not dropped on a valid slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
            }
            else
            {
                // Return to original parent if not dropped on a valid slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
                ResetPosition();
            }
        }
        else
        {
            // Return to original parent if not dropped on a valid slot
            player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
            ResetPosition();
        }
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
