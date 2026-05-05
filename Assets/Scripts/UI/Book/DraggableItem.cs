using Mirror;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class DraggableItem : PlayerBoundBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public static bool IsDragging = false;

    [HideInInspector] public ItemGeneric itemGeneric;
    [HideInInspector] public Transform originalParent;
    [HideInInspector] public RectTransform rectTransform;
    [HideInInspector] public Transform bookStatsPageContainer;
    [HideInInspector] public Slot belongingSlot;
    [HideInInspector] public int originalIndexNum;
    [HideInInspector] public bool transactionOnTheSameSet;
    [HideInInspector] public bool contactSuccessful;
    [HideInInspector] public bool justMoveNotSwap;
    [HideInInspector] public bool isLockIcon;
    [HideInInspector] public Image image;

    CanvasGroup canvasGroup;
    Canvas canvas;
    Vector2 originalPosition;
    GameObject dropButton;
    bool swapCancelled;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        image = GetComponent<Image>();
        canvas = GetComponentInParent<Canvas>();
    }

    protected override void OnEnable()
    {
        base.OnEnable();

        StaticEventHandler.OnSwapFailed += StaticEventHandler_OnSwapFailed;
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        StaticEventHandler.OnSwapFailed -= StaticEventHandler_OnSwapFailed;
    }

    private void StaticEventHandler_OnSwapFailed(SwapFailedArgs obj)
    {
        swapCancelled = true;
    }

    protected override void HandlePlayerReady(Player player, PlayerDetailsSO details)
    {
        if (!player.IsLocal) return;

        bookStatsPageContainer = GetTopLevelParent(transform, 4);
        belongingSlot = GetTopLevelParent(transform, 2).GetComponent<Slot>();
        this.player = player;
        originalIndexNum = player.currentWeaponSlotSetIndex;
        transactionOnTheSameSet = true;
        dropButton = BookUI.Instance.dropButton.gameObject;

        GetItemGenericInfo();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (isLockIcon) return;

        originalPosition = rectTransform.anchoredPosition;
        originalParent = transform.parent;
        canvasGroup.alpha = 0.6f;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(bookStatsPageContainer); // Go 4 level up for preventing drag interruption while set change
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isLockIcon) return;

        IsDragging = true;

        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;  // HERE

        // Check if the item is dropped on one of the weapon set buttons
        if (eventData.pointerEnter != null && eventData.pointerEnter.CompareTag(Settings.weaponSetButton))
        {
            if (belongingSlot.slotType == SlotType.Passive) return;

            WeaponSetButton weaponSetButton = eventData.pointerEnter.GetComponent<WeaponSetButton>();
            int setIndex;

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

        // DO NOT leave the visual under the cursor — always reset visually.
        // We will either hide/destroy the object (multiplayer move) or reset/reparent it.
        // ResetPosition() will reparent back to originalParent and place at originalPosition.
        ResetPosition();

        dropButton.SetActive(true);

        // Update weapon set in book ui - only run this locally (owner) to avoid race/duplicate updates on remotes
        if (player != null && player.IsLocal)
        {
            StaticEventHandler.CallWeaponSwitchedEventForBook();
        }

        bool isMultiplayer = NetworkServer.active || NetworkClient.active;

        if (contactSuccessful)
        {
            if (justMoveNotSwap)
            {
                // Move (non-swap) succeeded. In multiplayer we must avoid duplicate visuals:
                // - Mark the source inventory slot as pending so network code can safely clear it later.
                // - Hide this local draggable immediately so it doesn't remain under cursor.
                if (isMultiplayer)
                {
                    int fromIndex = belongingSlot != null ? belongingSlot.inventoryIndexNumber : -1;
                    if (fromIndex >= 0 && player?.playerInventory != null)

                    // hide the visual and stop dragging — the network callbacks will create the final visual
                    gameObject.SetActive(false);
                    IsDragging = false;

                    contactSuccessful = false;
                    justMoveNotSwap = false;
                    return;
                }

                // Single-player: preserve original early return behaviour
                IsDragging = false;
                contactSuccessful = false;
                justMoveNotSwap = false;
                return;
            }
        }

        transactionOnTheSameSet = true;

        // Check if the drag was canceled due to an invalid swap
        if (swapCancelled)
        {
            swapCancelled = false;

            if (belongingSlot != null)
            {
                if (belongingSlot.slotType == SlotType.WeaponMainHand && belongingSlot.inventoryIndexNumber == -1)
                {
                    Weapon weapon = itemGeneric as Weapon;
                    player.weaponSlotSetArray[weapon.weaponStats.weaponBelongingToWhichMainHandSet - 1][0] = weapon;
                }
                else if (belongingSlot.slotType == SlotType.WeaponOffHand && belongingSlot.inventoryIndexNumber == -1)
                {
                    Weapon weapon = itemGeneric as Weapon;
                    player.weaponSlotSetArray[weapon.weaponStats.weaponBelongingToWhichOffHandSet - 1][1] = weapon;
                }
            }

            if (!transactionOnTheSameSet)
            {
                if (belongingSlot.transform.GetChild(1) != null)
                {
                    GameManager.Instance.OpenPopUpLog(PopUpReason.ShieldCantBePutOnMainHand);
                    dropButton.SetActive(true);
                }
            }

            IsDragging = false;

            // Revert to original place (already ResetPosition above, but keep semantics)
            contactSuccessful = false;
            justMoveNotSwap = false;
            return;
        }

        // If pointerEnter is null or not a valid drop target, ensure we revert (ResetPosition already executed)
        if (eventData.pointerEnter == null)
        {
            RevertWeaponBackToBelongingSlots();
            dropButton.SetActive(true);
            contactSuccessful = false;
            justMoveNotSwap = false;
            IsDragging = false;
            return;
        }

        // Handle known pointerEnter targets
        if (eventData.pointerEnter.CompareTag(Settings.weaponSetButton))
        {
            // Return to original parent if not dropped on a valid slot (ResetPosition already executed)
            RevertWeaponBackToBelongingSlots();
        }
        else if (eventData.pointerEnter.CompareTag(Settings.dropButton))
        {
            int originalIndex = player.playerInventory.GetOriginalSlotIndex();

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
        else if (eventData.pointerEnter.transform != null &&
            (eventData.pointerEnter.transform.parent != null && (eventData.pointerEnter.transform.parent.CompareTag(Settings.mainHandSlot) || eventData.pointerEnter.transform.parent.CompareTag(Settings.offHandSlot)) ||
             eventData.pointerEnter.transform.CompareTag(Settings.mainHandSlot) || eventData.pointerEnter.transform.CompareTag(Settings.offHandSlot)))
        {
            // Dropped on a valid slot — swap/move handling performed by Slot.OnDrop.
            // For swaps we keep the object (SwapProcess will manage belongingSlot references).
            // For simple moves in multiplayer we already hid the visual above.
        }
        else
        {
            // Not a valid slot -> revert (ResetPosition already executed)
            RevertWeaponBackToBelongingSlots();
        }

        dropButton.SetActive(true);

        contactSuccessful = false;
        justMoveNotSwap = false;

        IsDragging = false;
    }

    private void RevertWeaponBackToBelongingSlots()
    {
        if (belongingSlot != null)
        {
            if (belongingSlot.slotType == SlotType.WeaponMainHand && belongingSlot.inventoryIndexNumber == -1)
            {
                Weapon weapon = itemGeneric as Weapon;
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = weapon;
            }
            else if (belongingSlot.slotType == SlotType.WeaponOffHand && belongingSlot.inventoryIndexNumber == -1)
            {
                Weapon weapon = itemGeneric as Weapon;
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
            }
        }
    }

    public void SetDraggableItem(ItemGeneric itemGeneric, Slot belongingSlot, Sprite sprite, ItemSlotStatus itemSlotStatus)
    {
        this.itemGeneric = itemGeneric;
        this.belongingSlot = belongingSlot;

        itemGeneric.ItemSlotStatus = itemSlotStatus;

        if (image == null) image = GetComponent<Image>();
        if (canvasGroup == null) canvasGroup = GetComponent<CanvasGroup>();
        if (rectTransform == null) rectTransform = GetComponent<RectTransform>();
        if (canvas == null) canvas = GetComponentInParent<Canvas>();

        image.sprite = sprite;

        // Reset transform state
        transform.localScale = Vector3.one;
        rectTransform.anchoredPosition = Vector2.zero;

        // Set original parent (for drag return)
        originalParent = belongingSlot.equippedTransform;
    }

    public Weapon GetDraggedWeapon()
    {
        Weapon weapon = itemGeneric as Weapon;

        return weapon;
    }

    public PassiveItem GetDraggedPassiveItem() => (PassiveItem)itemGeneric;

    public int GetSetNumber()
    {
        if (belongingSlot != null)
        {
            if (belongingSlot.slotType == SlotType.WeaponMainHand && belongingSlot.inventoryIndexNumber == -1)
            {
                Weapon weapon = itemGeneric as Weapon;
                return weapon.weaponStats.weaponBelongingToWhichMainHandSet;
            }
            else if (belongingSlot.slotType == SlotType.WeaponOffHand && belongingSlot.inventoryIndexNumber == -1)
            {
                Weapon weapon = itemGeneric as Weapon;
                return weapon.weaponStats.weaponBelongingToWhichOffHandSet;
            }
        }

        return -1;
    }

    public void UpdateSetNumber(int newSetIndex)
    {
        if (belongingSlot != null)
        {
            if (belongingSlot.slotType == SlotType.WeaponMainHand && belongingSlot.inventoryIndexNumber == -1)
            {
                Weapon weapon = itemGeneric as Weapon;
                weapon.weaponStats.weaponBelongingToWhichMainHandSet = newSetIndex;
            }
            else if (belongingSlot.slotType == SlotType.WeaponOffHand && belongingSlot.inventoryIndexNumber == -1)
            {
                Weapon weapon = itemGeneric as Weapon;
                weapon.weaponStats.weaponBelongingToWhichOffHandSet = newSetIndex;
            }
        }
    }

    private ItemGeneric GetItemGenericInfo()
    {
        bool isMultiplayer = NetworkServer.active || NetworkClient.active;

        if (belongingSlot != null)
        {
            if (belongingSlot.inventoryIndexNumber == -1) // Non-inventory item
            {
                switch (belongingSlot.slotType)
                {
                    case SlotType.Passive:
                        itemGeneric = player.equippedPassiveItems[belongingSlot.passiveItemSlotName];
                        break;
                    case SlotType.WeaponMainHand:
                        itemGeneric = GameManager.Instance.GetLocalPlayer().activeWeapon.GetCurrentMainHandWeapon();
                        break;
                    case SlotType.WeaponOffHand:
                        itemGeneric = GameManager.Instance.GetLocalPlayer().activeWeapon.GetCurrentOffHandWeapon();
                        break;
                    default:
                        break;
                }
            }
            else if (belongingSlot.inventoryIndexNumber >= 0) // Inventory Item
            {
                if (!isMultiplayer) itemGeneric = player.playerInventory.GetInventoryItem(belongingSlot.inventoryIndexNumber);
                else itemGeneric = player.playerInventory.GetInventoryItem(belongingSlot.inventoryIndexNumber);
            }
        }

        return itemGeneric;
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