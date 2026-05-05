using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;
using Mirror;

public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public static Slot selectedSlot;
    public static GameObject currentOpenTooltip;

    [HideInInspector] public DraggableItem selectedSlotDraggableItem;
    [HideInInspector] public Transform equippedTransform;
    [HideInInspector] public Transform backgroundTransform;

    public SlotType slotType;
    public PassiveItemSlotName passiveItemSlotName;
    public RectTransform tooltipRect;
    public int inventoryIndexNumber;

    Player player;

    // Tooltip Panel Item Texts
    TMP_Text headerText;
    TMP_Text levelText;
    TMP_Text contentText;
    TMP_Text bonusText;

    RectTransform tooltipParent;
    RectTransform slotRect;

    // Weapon Level 
    [Header("WEAPON LEVEL COLORS")]
    [Space(10)]
    Color basicLevelColor1 = new Color(1, 1, 1);
    Color basicLevelColor2 = new Color(0.2196078f, 0.172549f, 0.172549f);
    Color enchantedLevelColor1 = new Color(1, 1, 1);
    Color enchantedLevelColor2 = new Color(0f, 0.4588235f, 1f);
    Color mythicLevelColor1 = new Color(1, 1, 1);
    Color mythicLevelColor2 = new Color(0.6745098f, 0, 1);
    Color legendaryLevelColor1 = new Color(1, 1, 1);
    Color legendaryLevelColor2 = new Color(1f, 0.09411765f, 0f);

    bool isOnHoverProcess = false;
    bool isClicked = false;
    float clickSafetyDuration = 0.2f;

    bool isSelectedViaGamepad = false;

    private void Awake()
    {
        slotRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        player = GameManager.Instance.GetLocalPlayer();

        if (slotType != SlotType.Drop && slotType != SlotType.Upgrade && slotType != SlotType.Dismantle)
        {
            backgroundTransform = transform.GetChild(0);
            equippedTransform = transform.GetChild(1);
        }
    }

    private void OnDisable()
    {
        isOnHoverProcess = false;

        if (BookUI.Instance != null && BookUI.Instance.tooltipManager != null)
        {
            BookUI.Instance.tooltipManager.Hide();
        }
    }

    private void Update()
    {
        if (isClicked)
        {
            clickSafetyDuration -= Time.deltaTime;

            if (clickSafetyDuration < 0f)
            {
                clickSafetyDuration = 0.2f;
                isClicked = false;
            }
        }
    }

    /// <summary>
    /// Open tooltip panel when hovering over the related item or weapon
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isSelectedViaGamepad) return;
        if (BookUI.Instance.tooltipManager == null) return;

        //if (!BookUI.IsBookOpen) return;

        if (equippedTransform == null || equippedTransform.childCount == 0) return;

        DraggableItem draggableItem = equippedTransform.GetChild(0).GetComponent<DraggableItem>();
        if (draggableItem == null) return;

        isOnHoverProcess = true;
        UpdateTooltipPanelInfo();
    }

    /// <summary>
    /// Close tooltip panel when stop hovering over the related item or weapon
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (isSelectedViaGamepad) return;
        if (BookUI.Instance.tooltipManager == null) return;

        isOnHoverProcess = false;
        BookUI.Instance.tooltipManager.Hide();
    }

    public void OnSelect(BaseEventData eventData)
    {
        isOnHoverProcess = true;
        ShowTooltip();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        isOnHoverProcess = false;
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltipRect == null) return;

        if (slotType != SlotType.Drop) return;

        // Hide previous one
        if (currentOpenTooltip != null && currentOpenTooltip != tooltipRect)
            currentOpenTooltip.SetActive(false);

        tooltipRect.gameObject.SetActive(true);
        UpdateTooltipPanelInfo(); // This should be your own method
        currentOpenTooltip = tooltipRect.gameObject;
    }

    private void HideTooltip()
    {
        if (tooltipRect != null && tooltipRect.gameObject.activeSelf)
        {
            tooltipRect.gameObject.SetActive(false);

            if (currentOpenTooltip == tooltipRect) currentOpenTooltip = null;
        }
    }

    public void UpdateTooltipPanelInfo()
    {
        if (BookUI.Instance.tooltipManager == null) return;

        BookUI.Instance.tooltipManager.Show();

        if (equippedTransform == null) return;

        tooltipRect = BookUI.Instance.tooltipManager.TooltipRect;
        tooltipParent = BookUI.Instance.tooltipManager.TooltipParent;

        if (tooltipRect == null || tooltipParent == null) return;

        // Slot Empty Check
        if (equippedTransform.childCount == 0)
        {
            BookUI.Instance.tooltipManager.Hide();
            return;
        }

        Transform currentChild = equippedTransform.GetChild(0);

        DraggableItem draggableItem = currentChild.GetComponent<DraggableItem>();

        if (draggableItem == null || draggableItem.isLockIcon || draggableItem.itemGeneric == null)
        {
            BookUI.Instance.tooltipManager.Hide();
            return;
        }

        headerText = BookUI.Instance.tooltipManager.HeaderText;
        levelText = BookUI.Instance.tooltipManager.LevelText;
        contentText = BookUI.Instance.tooltipManager.ContentText;
        bonusText = BookUI.Instance.tooltipManager.BonusText;

        // Inventory Slot Validation
        if (inventoryIndexNumber >= 0)
        {
            if (player.playerInventory.inventoryArray[inventoryIndexNumber] == null)
            {
                BookUI.Instance.tooltipManager.Hide();
                return;
            }
        }

        // Passive Item
        if (draggableItem.itemGeneric is PassiveItem)
        {
            PassiveItem passiveItem = draggableItem.GetDraggedPassiveItem();

            if (passiveItem == null)
            {
                BookUI.Instance.tooltipManager.Hide();
                return;
            }

            ReshapeTooltip(false);
            ClearTooltipTexts();
            PositionTooltip();
            WriteTooltipTextForPassiveItem(passiveItem);

            BookUI.Instance.tooltipManager.Show();
            return;
        }

        // Weapon
        if (draggableItem.itemGeneric is Weapon weapon)
        {
            // Off-hand lock check
            if (slotType == SlotType.WeaponOffHand && player.activeWeapon.GetCurrentMainHandWeapon()?.weaponStats.wieldType == WieldType.TwoHanded)
            {
                BookUI.Instance.tooltipManager.Hide();
                return;
            }

            ReshapeTooltip(isWeapon: true);
            ClearTooltipTexts();
            PositionTooltip();
            WriteToolTipTextForWeapon(weapon);

            BookUI.Instance.tooltipManager.Show();
            return;
        }

        // Fallback
        BookUI.Instance.tooltipManager.Hide();
    }

    private void ReshapeTooltip(bool isWeapon)
    {
        if (isWeapon) tooltipRect.sizeDelta = new Vector2(tooltipRect.sizeDelta.x, 120);
        else tooltipRect.sizeDelta = new Vector2(tooltipRect.sizeDelta.x, 60);
    }

    private void ClearTooltipTexts()
    {
        headerText.text = string.Empty;
        levelText.text = string.Empty;
        contentText.text = string.Empty;
        bonusText.text = string.Empty;
    }

    private void PositionTooltip()
    {
        Vector2 anchoredPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(tooltipParent, RectTransformUtility.WorldToScreenPoint(null, slotRect.position), null, out anchoredPos);

        if (slotType == SlotType.WeaponMainHand) anchoredPos += GetOffsetForSlot(isWeaponMainHand: true);
        else if (slotType == SlotType.WeaponOffHand) anchoredPos += GetOffsetForSlot(isWeaponMainHand: false);
        else if (slotType == SlotType.Passive) anchoredPos += GetOffsetForSlot(passiveItemSlotName);
        else if (slotType == SlotType.Inventory) anchoredPos += GetOffsetForSlot(isInventorySlot: true, inventoryIndexNumber);

        tooltipRect.anchoredPosition = anchoredPos;
    }

    private Vector2 GetOffsetForSlot(bool isWeaponMainHand = true)
    {
        if (isWeaponMainHand) return new Vector2(85f, -40f);
        else return new Vector2(85f, -40f);
    }

    private Vector2 GetOffsetForSlot(PassiveItemSlotName passiveItemSlotName)
    {
        return passiveItemSlotName switch
        {
            PassiveItemSlotName.Head => new Vector2(85f, -30f), // top-centre
            PassiveItemSlotName.Neck => new Vector2(85f, -30f), // top-left
            PassiveItemSlotName.Back => new Vector2(85f, -30f), // top-right

            PassiveItemSlotName.Chest => new Vector2(85f, -30f), // centre

            PassiveItemSlotName.Arm => new Vector2(85f, -30f), // bottom-left
            PassiveItemSlotName.Leg => new Vector2(85f, -30f), // bottom-centre
            PassiveItemSlotName.Finger => new Vector2(85f, -30f), // bottom-right
            _ => new Vector2(85f, -30f),
        };
    }

    private Vector2 GetOffsetForSlot(bool isInventorySlot = false, int inventoryIndex = -1)
    {
        return inventoryIndex switch
        {
            0 => new Vector2(85f, -56f),
            1 => new Vector2(85f, -56f),
            2 => new Vector2(85f, -56f),
            3 => new Vector2(85f, -56f),
            4 => new Vector2(85f, -56f),
            5 => new Vector2(85f, -56f),
            6 => new Vector2(85f, -33f),
            7 => new Vector2(85f, -33f),
            8 => new Vector2(85f, -33f),
            9 => new Vector2(85f, -33f),
            10 => new Vector2(85f, -33f),
            11 => new Vector2(85f, -33f),
            _ => new Vector2(85f, -33f)
        };
    }

    private void WriteTooltipTextForPassiveItem(PassiveItem passiveItem)
    {
        BoostTypeColorUpdate(passiveItem);

        PassiveItemDetailsSO passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);

        headerText.text = passiveItemDetails.passiveItemName;
        levelText.text = $"({passiveItem.Rarity.ToString()})";

        switch (passiveItem.Rarity)
        {
            case Rarity.Basic:
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseUniqueRolled, BoostPhase.Unique);
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseTypeRolled, BoostPhase.Type);
                break;
            case Rarity.Enchanted:
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseUniqueRolled, BoostPhase.Unique);
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseTypeRolled, BoostPhase.Type);
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.enchantedBoostType, BoostPhase.Enchanted);
                break;
            case Rarity.Mythic:
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseUniqueRolled, BoostPhase.Unique);
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseTypeRolled, BoostPhase.Type);
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.enchantedBoostType, BoostPhase.Enchanted);
                BoostForPassiveItem(passiveItem, passiveItem.passiveStats.mythicBoostType, BoostPhase.Mythic);
                break;
            case Rarity.Legendary:
                break;
            default:
                break;
        }
    }

    private void WriteToolTipTextForWeapon(Weapon weapon)
    {
        // Populate text field based on the related weapon info
        BoostTypeColorUpdate(weapon);

        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

        headerText.text = weaponDetails.weaponName;
        levelText.text = $"({weapon.Rarity.ToString()})";
        contentText.text = $"Class: {weapon.weaponStats.weaponClass.ToString()}\n";

        if (weapon.weaponStats.weaponClass == WeaponClass.Shield)
        {
            contentText.text = $"\nWield Type: {weapon.weaponStats.wieldType.ToString()}\n";
            contentText.text += $"Block Rate: {(weapon.weaponStats.blockChance + weapon.weaponStats.blockChanceIncrease) * 100}%\n";
        }
        else
        {
            float fireRate = (float)Math.Round(1 / (weapon.weaponStats.weaponCooldownDuration - (weapon.weaponStats.attackCooldownModifier / 2)), 2);
            contentText.text += $"Attack Speed: {fireRate}\n";
            contentText.text += $"Wield Type: {weapon.weaponStats.wieldType.ToString()}\n";

            contentText.text += $"Phy. Damage: {weapon.weaponStats.physicalDamageMin + weapon.weaponStats.physicalAttackDamageIncrease}-" +
                $"{weapon.weaponStats.physicalDamageMax + weapon.weaponStats.physicalAttackDamageIncrease}\n";

            contentText.text += $"Magic Damage: {weapon.weaponStats.magicDamageMin + weapon.weaponStats.magicAttackDamageIncrease}-" +
                $"{weapon.weaponStats.magicDamageMax + weapon.weaponStats.magicAttackDamageIncrease}\n";

            float updatedAttackRating = (float)Math.Round(weapon.weaponStats.weaponAttackRating * weapon.weaponStats.attackRatingIncrease, 2);
            contentText.text += $"Attack Rating: {updatedAttackRating * 100}%\n";

            contentText.text += $"Cr. Hit Chance: {weapon.weaponStats.criticalHitChanceIncrease * 100}%\n";
            contentText.text += $"Cr. Hit Damage: {weapon.weaponStats.criticalHitDamageIncrease * 100}%\n";
        }

        switch (weapon.Rarity)
        {
            case Rarity.Basic:
                BoostForWeapon(weapon, weapon.weaponStats.baseUniqueRolled, BoostPhase.Unique);
                BoostForWeapon(weapon, weapon.weaponStats.baseTypeRolled, BoostPhase.Type);
                break;
            case Rarity.Enchanted:
                BoostForWeapon(weapon, weapon.weaponStats.baseUniqueRolled, BoostPhase.Unique);
                BoostForWeapon(weapon, weapon.weaponStats.baseTypeRolled, BoostPhase.Type);
                BoostForWeapon(weapon, weapon.weaponStats.enchantedBoostType, BoostPhase.Enchanted);
                break;
            case Rarity.Mythic:
                BoostForWeapon(weapon, weapon.weaponStats.baseUniqueRolled, BoostPhase.Unique);
                BoostForWeapon(weapon, weapon.weaponStats.baseTypeRolled, BoostPhase.Type);
                BoostForWeapon(weapon, weapon.weaponStats.enchantedBoostType, BoostPhase.Enchanted);
                BoostForWeapon(weapon, weapon.weaponStats.mythicBoostType, BoostPhase.Mythic);
                break;
            case Rarity.Legendary:
                break;
            default:
                break;
        }
    }

    // (Only the modified OnClick method section is shown; rest of file unchanged)
    public void OnClick()
    {
        if (!isClicked)
        {
            if (selectedSlot == null)
            {
                if (equippedTransform.childCount > 0)
                {
                    DraggableItem draggableItem = equippedTransform.GetChild(0).GetComponent<DraggableItem>();

                    if (draggableItem.isLockIcon) return;

                    selectedSlot = this;
                    selectedSlot.selectedSlotDraggableItem = draggableItem;
                }
            }
            else if (selectedSlot == this)
            {
                if (slotType != SlotType.Drop) tooltipRect.gameObject.SetActive(false); // Close tooltip

                // Clicked same slot again - deselect
                tooltipRect.gameObject.SetActive(false); // Ensure this runs unconditionally
                selectedSlot.selectedSlotDraggableItem = null;
                selectedSlot = null;
            }
            else
            {
                // Validate placement rules
                if (!SlotPlacementRules.IsPlacementAllowed(selectedSlot.selectedSlotDraggableItem.itemGeneric, slotType, player.activeWeapon.GetCurrentMainHandWeapon(),
                    player.currentWeaponSlotSetIndex))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }

                bool isSwap = (equippedTransform.childCount > 0);

                if (isSwap) // SWAP ACTION
                {
                    DraggableItem targetDraggableItem = equippedTransform.GetChild(0).GetComponent<DraggableItem>();

                    SwapItems(selectedSlot.selectedSlotDraggableItem, targetDraggableItem, player.activeWeapon.GetCurrentMainHandWeapon(),
                        player.activeWeapon.GetCurrentOffHandWeapon());
                }
                else // SLOT MOVE ACTION
                {
                    // Second click - perform item transfer or swap
                    MoveItemToSlot(selectedSlot.selectedSlotDraggableItem, clickTransport: true);

                    // Only perform immediate Book UI refresh in single-player.
                    bool isMultiplayer = NetworkServer.active || NetworkClient.active;
                    if (!isMultiplayer)
                    {
                        if (slotType == SlotType.WeaponMainHand || slotType == SlotType.WeaponOffHand || slotType == SlotType.Passive)
                        {
                            // BookUIWrapper (single-player only)
                            BookUIRefreshHelper.RefreshBookUIAfterItemPlacement(selectedSlot.selectedSlotDraggableItem.itemGeneric, selectedSlot.inventoryIndexNumber);
                        }
                    }
                }

                // Clear old visual
                ClearOldVisual();
            }
        }
    }

    private void ClearOldVisual()
    {
        // Guard: selectedSlot should be valid when this is called, but double-check
        if (selectedSlot == null) return;

        if (selectedSlot.equippedTransform.childCount > 0)
        {
            Transform oldChild = selectedSlot.equippedTransform.GetChild(0);
            Destroy(oldChild.gameObject);
        }

        // Re-enable the background sprite when the slot becomes empty
        // so the empty slot shows its background image (main hand/off hand/passive/inventory).
        // This applies to both single-player and multiplayer.
        Transform background = selectedSlot.transform.childCount > 0 ? selectedSlot.transform.GetChild(0) : null;

        if (background != null)
        {
            background.gameObject.SetActive(true);
        }

        if (selectedSlot.equippedTransform != null)
        {
            selectedSlot.equippedTransform.gameObject.SetActive(false);
        }

        if (tooltipRect != null) tooltipRect.gameObject.SetActive(false);

        selectedSlot.selectedSlotDraggableItem = null;
        selectedSlot = null;
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedItem = eventData.pointerDrag;
        DraggableItem draggableItem;

        if (slotType == SlotType.Drop)
        {
            // If this was a click-based drop (not drag), ignore
            if (selectedSlot != null && eventData.pointerDrag == null) return;

            bool dropFailed = false;

            if (eventData.pointerDrag != null)
            {
                draggableItem = eventData.pointerDrag?.GetComponentInParent<DraggableItem>() ?? eventData.pointerDrag?.GetComponent<DraggableItem>();
                dropFailed = DropProcess(dropFailed, draggableItem);
            }

            return;
        }
        else if (slotType == SlotType.Dismantle)
        {
            // If this was a click-based drop (not drag), ignore
            if (selectedSlot != null && eventData.pointerDrag == null) return;

            bool dismantleFailed = false;

            if (eventData.pointerDrag != null)
            {
                draggableItem = eventData.pointerDrag?.GetComponentInParent<DraggableItem>() ?? eventData.pointerDrag?.GetComponent<DraggableItem>();
                if (draggableItem.isLockIcon) return;

                int shardGain = 0;

                dismantleFailed = DismantleProcess(draggableItem, out shardGain);

                if (!dismantleFailed)
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    GameManager.Instance.OpenPopUpLog(PopUpReason.DismantleCompletedLog, shardGain);
                }

            }

            return;
        }
        else if (slotType == SlotType.Upgrade)
        {
            // Ignore click-based drop
            if (eventData.pointerDrag == null) return;

            draggableItem = eventData.pointerDrag.GetComponentInParent<DraggableItem>() ?? eventData.pointerDrag.GetComponent<DraggableItem>();

            if (draggableItem == null) return;

            bool upgradeFailed = UpgradeProcess(draggableItem);

            if (!upgradeFailed && draggableItem.itemGeneric.Rarity != Rarity.Legendary && draggableItem.itemGeneric.Rarity != Rarity.Mythic)
            {
                if (selectedSlot != null)
                {
                    selectedSlot.selectedSlotDraggableItem = null;
                    selectedSlot = null;
                }
            }

            return;
        }

        draggableItem = draggedItem.GetComponent<DraggableItem>();

        if (draggableItem != null && !draggableItem.isLockIcon)
        {
            if (inventoryIndexNumber < 0)
            {
                // Only now is selectedSlot guaranteed to be non-null
                if (!SlotPlacementRules.IsPlacementAllowed(draggableItem.itemGeneric, slotType, player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0], player.currentWeaponSlotSetIndex))
                {
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                    return;
                }
            }

            Transform currentChild = null;

            draggableItem.contactSuccessful = true;

            if (ReferenceEquals(draggableItem.belongingSlot, this) && draggableItem.transactionOnTheSameSet) return;

            if (equippedTransform != null)
            {
                // Check if the slot is occupied
                if (equippedTransform.childCount > 0) currentChild = equippedTransform.GetChild(0);
            }

            if (currentChild != null)
            {
                DraggableItem currentSlotsDraggableItem = currentChild.GetComponent<DraggableItem>();

                // If slot is occupied, swap items
                SwapItems(draggableItem, currentSlotsDraggableItem, player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0], player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1]);
            }
            else
            {
                draggableItem.justMoveNotSwap = true;

                // If slot is not occupied, just relocate selected item
                MoveItemToSlot(draggableItem);
            }
        }
    }

    private void SwapItems(DraggableItem draggableItem, DraggableItem targetItem, Weapon playerMainHand, Weapon playerOffHand)
    {
        if (targetItem.isLockIcon || draggableItem.isLockIcon) return;

        ItemSwapPos itemSwapPos = ItemSwapPos.None;

        // Safe cast - returns null if itemGeneric is not a Weapon
        Weapon targetItemWeaponIfItIs = null;
        Weapon peekedWeaponSetsOffHandWeapon = null;

        if (!IsInventorySwap(draggableItem, targetItem))
        {
            if ((draggableItem.belongingSlot.slotType == SlotType.WeaponMainHand && targetItem.belongingSlot.slotType == SlotType.WeaponMainHand) ||
                (draggableItem.belongingSlot.slotType == SlotType.WeaponOffHand && targetItem.belongingSlot.slotType == SlotType.WeaponOffHand)) goto jump;

            if (playerMainHand.weaponStats.weaponBelongingToWhichMainHandSet != playerOffHand?.weaponStats.weaponBelongingToWhichOffHandSet)
            {
                targetItemWeaponIfItIs = targetItem.itemGeneric as Weapon;

                // If it's a weapon, peek into the appropriate offhand slot
                if (targetItemWeaponIfItIs != null)
                {
                    int index = targetItemWeaponIfItIs.weaponStats.weaponBelongingToWhichOffHandSet - 1;
                    peekedWeaponSetsOffHandWeapon = player.weaponSlotSetArray[index][1];
                }
            }
        }

    jump:

        bool isAllowed;

        if (peekedWeaponSetsOffHandWeapon != null)
        {
            isAllowed = SlotPlacementRules.IsSwapAllowed(draggableItem, targetItem, draggableItem.itemGeneric, targetItem.itemGeneric, playerMainHand, playerOffHand,
                peekedWeaponSetsOffHandWeapon, out itemSwapPos);
        }
        else
        {
            isAllowed = SlotPlacementRules.IsSwapAllowed(draggableItem, targetItem, draggableItem.itemGeneric, targetItem.itemGeneric, playerMainHand, playerOffHand,
                null, out itemSwapPos);
        }

        if (!isAllowed)
        {
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
            return;
        }

        if (NetworkServer.active || NetworkClient.active) HandleSwapMP(player.playerDetails.playerCharacterIndex, draggableItem, targetItem, itemSwapPos);
        else HandleSwapSP(draggableItem, targetItem, itemSwapPos);
    }

    private void HandleSwapSP(DraggableItem draggableItem, DraggableItem targetItem, ItemSwapPos swapPos)
    {
        SwapProcess(draggableItem, targetItem, swapPos, isMultiplayer: false);
    }

    private void HandleSwapMP(Character character, DraggableItem draggableItem, DraggableItem targetItem, ItemSwapPos swapPos)
    {
        if (!player.IsLocal) return;

        Slot fromSlot = draggableItem.belongingSlot;
        Slot toSlot = targetItem.belongingSlot;

        int fromIndex = fromSlot.inventoryIndexNumber;
        int toIndex = toSlot.inventoryIndexNumber;

        int setIndex = player.currentWeaponSlotSetIndex;
        bool isInventoryFull = player.playerInventory.IsInventoryFull();

        player.playerInventoryNetwork.RequestSwapItem(character, draggableItem.itemGeneric, targetItem.itemGeneric, draggableItem.itemGeneric.ItemSlotStatus, targetItem.itemGeneric.ItemSlotStatus,
            fromIndex, toIndex, setIndex, swapPos, isInventoryFull, draggableItem.transactionOnTheSameSet);
    }

    private bool IsInventorySwap(DraggableItem draggedItem, DraggableItem targetItem) =>
        draggedItem.itemGeneric.ItemSlotStatus == ItemSlotStatus.Inventory || targetItem.itemGeneric.ItemSlotStatus == ItemSlotStatus.Inventory;

    private void MoveItemToSlot(DraggableItem draggableItem, bool clickTransport = false)
    {
        ItemGeneric item = draggableItem.itemGeneric;

        bool isMultiplayer = NetworkServer.active || NetworkClient.active;
        bool isInventoryFull = player.playerInventory.IsInventoryFull();

        int fromIndex = draggableItem.belongingSlot.inventoryIndexNumber;
        int targetIndex = inventoryIndexNumber;
        int setIndex = player.currentWeaponSlotSetIndex;

        if (isMultiplayer)
        {
            if (!player.IsLocal) return;

            Slot fromSlot = draggableItem.belongingSlot;
            Slot toSlot = this;

            ItemSwapPos swapPos = ResolveSwapPosition(fromSlot, toSlot, item);

            player.playerInventoryNetwork.RequestMoveItem(player.playerDetails.playerCharacterIndex, item, item.ItemSlotStatus, slotType, fromIndex, targetIndex, setIndex, swapPos, 
                isInventoryFull, draggableItem.transactionOnTheSameSet);

            return;
        }

        if (item.ItemSlotStatus == ItemSlotStatus.Inventory) // If weapon moved from inventory slot
        {
            // Remove draggable item from inventory
            player.playerInventory.EmptyItemFromInventory(fromIndex);

            if (item is Weapon weapon)
            {
                if (slotType == SlotType.WeaponMainHand)
                {
                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = weapon;
                    weapon.weaponStats.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    weapon.ItemSlotStatus = ItemSlotStatus.MainHand;

                    // Activation
                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex, dragFromInventory: true);

                    // Book update
                    StaticEventHandler.CallInventoryWeaponDroppedEventForBook(fromIndex);
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else if (slotType == SlotType.WeaponOffHand)
                {
                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = weapon;
                    weapon.weaponStats.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    weapon.ItemSlotStatus = ItemSlotStatus.OffHand;

                    // Activation
                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex, dragFromInventory: true);

                    // Book update
                    StaticEventHandler.CallInventoryWeaponDroppedEventForBook(fromIndex);
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else if (slotType == SlotType.Inventory)
                {
                    player.playerInventory.PlaceItemToInventoryIndexSlot(draggableItem.itemGeneric, inventoryIndexNumber);

                    // Book update
                    StaticEventHandler.CallGenericItemPlacedToEmptyInInventory(draggableItem.itemGeneric, fromIndex, inventoryIndexNumber);
                }
            }
            // Passive item in the inventory moves to passive item slot
            else if (item is PassiveItem)
            {
                if (slotType == SlotType.Inventory)
                {
                    player.playerInventory.PlaceItemToInventoryIndexSlot(draggableItem.itemGeneric, inventoryIndexNumber);

                    // Book update
                    StaticEventHandler.CallGenericItemPlacedToEmptyInInventory(draggableItem.itemGeneric, fromIndex, inventoryIndexNumber);
                }
                else
                {
                    PassiveItem draggableInventoryPassiveItem = (PassiveItem)item;

                    // Equip event and update stats
                    player.setPassiveItemEvent.CallEquipPassiveItem(player, draggableInventoryPassiveItem, draggableInventoryPassiveItem.passiveStats.passiveItemSlotName);
                    draggableInventoryPassiveItem.ItemSlotStatus = ItemSlotStatus.Passive;

                    // Book update for passive slot addition and inventory slot drop
                    StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(fromIndex);
                    StaticEventHandler.CallItemAddedToPassiveItemSlot(draggableInventoryPassiveItem, draggableInventoryPassiveItem.passiveStats.passiveItemSlotName);
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }

            return; // This is dragged from inventory so don't go further
        }

        if (draggableItem.itemGeneric is Weapon draggableItemWeapon)
        {
            if (draggableItemWeapon.ItemSlotStatus == ItemSlotStatus.MainHand)
            {
                if (inventoryIndexNumber >= 0 && !isInventoryFull) // IT MEANS, DRAGGED SLOT IS AN INVENTORY SLOT AND INVENTORY IS NOT FULL
                {
                    // Empty weapon on hand
                    player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet - 1][0] = null;

                    // Place weapon into inventory
                    player.playerInventory.PlaceItemToInventoryIndexSlot(draggableItemWeapon, inventoryIndexNumber);

                    draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.Inventory;
                    draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet = 0;

                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex, dragFromInventory: false, dragToInventory: true);

                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(draggableItemWeapon, inventoryIndexNumber);
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else if (slotType == SlotType.WeaponMainHand)
                {
                    player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet - 1][0] = null;

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;

                    draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else
                {
                    if (player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet - 1][1] != null)
                    {
                        StaticEventHandler.CallSwapFailedEvent(PopUpReason.EmptyOffHandFirst);
                        return;
                    }
                    else
                    {
                        if (draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet == player.currentWeaponSlotSetIndex)
                        {
                            StaticEventHandler.CallSwapFailedEvent(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                            return;
                        }
                    }

                    if (draggableItem.transactionOnTheSameSet)
                    {
                        StaticEventHandler.CallSwapFailedEvent(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                        return;
                    }

                    player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet - 1][0] = null;

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                    draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.OffHand;

                    draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet = 0;
                    draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }
            else
            {
                if (inventoryIndexNumber >= 0 && !isInventoryFull)
                {
                    // Empty weapon on off-hand
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = null;

                    // Place weapon into inventory
                    player.playerInventory.PlaceItemToInventoryIndexSlot(draggableItemWeapon, inventoryIndexNumber);

                    draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.Inventory;
                    draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet = 0;

                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex, false, true);

                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(draggableItemWeapon, inventoryIndexNumber);
                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else if (slotType == SlotType.WeaponMainHand)
                {
                    if (player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet - 1][1].weaponStats.weaponClass == WeaponClass.Shield)
                    {
                        StaticEventHandler.CallSwapFailedEvent(PopUpReason.ShieldCantBePutOnMainHand);
                        return;
                    }

                    player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet - 1][1] = null;

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                    draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.MainHand;

                    draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet = 0;
                    draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else
                {
                    if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] == null)
                    {
                        StaticEventHandler.CallSwapFailedEvent(PopUpReason.EquipMainHandFirst);
                        return;
                    }

                    player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet - 1][1] = null;

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;

                    draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    player.playerControl.SetWeaponSetByIndex(onStart: false, player.currentWeaponSlotSetIndex);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }
        }
        else if (draggableItem.itemGeneric is PassiveItem draggableInventoryPassiveItem)
        {
            if (!isInventoryFull)
            {
                // Passive item in the slot moves to inventory slot
                player.setPassiveItemEvent.CallRemovePassiveItem(player, draggableInventoryPassiveItem, draggableInventoryPassiveItem.passiveStats.passiveItemSlotName);

                // Place weapon into inventory
                player.playerInventory.PlaceItemToInventoryIndexSlot(draggableInventoryPassiveItem, inventoryIndexNumber);

                // Book update for passive slot inventory addition and passive slot drop
                StaticEventHandler.CallItemRemovedFromPassiveItemSlot(draggableInventoryPassiveItem.passiveStats.passiveItemSlotName);
                StaticEventHandler.CallPassiveItemAddedToInventorySlot(draggableInventoryPassiveItem, inventoryIndexNumber); // This is placed slot's index number

                StaticEventHandler.CallStatsChangedOnTheBookEvent();
            }
        }
    }

    private void SwapProcess(DraggableItem draggableItem, DraggableItem targetItem, ItemSwapPos itemSwapPos, bool isMultiplayer)
    {
        Weapon draggableItemWeapon = draggableItem.itemGeneric as Weapon;
        Weapon targetWeapon = targetItem.itemGeneric as Weapon;

        PassiveItem draggablePassiveItem = draggableItem.itemGeneric as PassiveItem;
        PassiveItem targetPassiveItem = targetItem.itemGeneric as PassiveItem;

        int index = 0;
        int setIndex = player.currentWeaponSlotSetIndex - 1;

        switch (itemSwapPos)
        {
            case ItemSwapPos.None:
                break;
            case ItemSwapPos.DragPassiveInventorySlotPassive:
                // Dragged item to equipped passive item slot - Stat Update
                player.setPassiveItemEvent.CallRemovePassiveItem(player, draggablePassiveItem, draggablePassiveItem.passiveStats.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(player, targetPassiveItem, targetPassiveItem.passiveStats.passiveItemSlotName, true);
                draggableItem.itemGeneric.ItemSlotStatus = ItemSlotStatus.None;

                // Target item to inventory slot - Stat Update
                player.setPassiveItemEvent.CallRemovePassiveItem(player, targetPassiveItem, targetPassiveItem.passiveStats.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(player, draggablePassiveItem, draggablePassiveItem.passiveStats.passiveItemSlotName, true);
                targetItem.itemGeneric.ItemSlotStatus = ItemSlotStatus.Inventory;

                // Inventory update
                if (!isMultiplayer)
                {
                    player.playerInventory.inventoryArray[draggableItem.belongingSlot.inventoryIndexNumber] = targetPassiveItem;
                }
                else
                {
                    player.playerInventory.inventoryArray[draggableItem.belongingSlot.inventoryIndexNumber] = targetPassiveItem;
                }

                // Book update
                StaticEventHandler.CallPassiveItemsSwappedEvent(targetPassiveItem, draggablePassiveItem, draggableItem.belongingSlot.inventoryIndexNumber);

                // Swap slots
                SwapBelongingSlots(draggableItem, targetItem);

                break;

            case ItemSwapPos.DragPassiveSlotPassiveInventory:
                // Dragged item to inventory slot - Stat Update
                player.setPassiveItemEvent.CallRemovePassiveItem(player, targetPassiveItem, targetPassiveItem.passiveStats.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(player, draggablePassiveItem, draggablePassiveItem.passiveStats.passiveItemSlotName, true);
                draggableItem.itemGeneric.ItemSlotStatus = ItemSlotStatus.Inventory;

                // Target item to equipped passive item slot - Stat Update
                player.setPassiveItemEvent.CallRemovePassiveItem(player, draggablePassiveItem, draggablePassiveItem.passiveStats.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(player, targetPassiveItem, targetPassiveItem.passiveStats.passiveItemSlotName, true);
                targetItem.itemGeneric.ItemSlotStatus = ItemSlotStatus.None;

                // Inventory update
                if (!isMultiplayer)
                {
                    player.playerInventory.inventoryArray[targetItem.belongingSlot.inventoryIndexNumber] = draggablePassiveItem;
                }
                else
                {
                    player.playerInventory.inventoryArray[targetItem.belongingSlot.inventoryIndexNumber] = draggablePassiveItem;
                }

                // Book update
                StaticEventHandler.CallPassiveItemsSwappedEvent(draggablePassiveItem, targetPassiveItem, targetItem.belongingSlot.inventoryIndexNumber);

                // Swap slots
                SwapBelongingSlots(draggableItem, targetItem);

                break;

            case ItemSwapPos.DragMainSlotInventory:
                index = targetItem.belongingSlot.inventoryIndexNumber;

                // Store old inventory weapon into main hand slot
                player.weaponSlotSetArray[setIndex][0] = targetWeapon;

                // Place dragged weapon into inventory array
                if (!isMultiplayer)
                {
                    player.playerInventory.inventoryArray[index] = draggableItemWeapon;
                }
                else
                {
                    player.playerInventory.inventoryArray[index] = draggableItemWeapon;
                }

                // Inventory weapon to main hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.Inventory);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.MainHand);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex, dragFromInventory: false, dragToInventory: true, inventorySwitch: false, isMultiplayer);

                // Book update
                StaticEventHandler.CallWeaponsSwappedWithInventoryEvent(draggableItemWeapon, targetWeapon, index, setIndex, true, draggableItem, targetItem);

                // Swap belonging slots for drag-drop references
                SwapBelongingSlots(draggableItem, targetItem);

                break;
            case ItemSwapPos.DragOffSlotInventory:
                index = targetItem.belongingSlot.inventoryIndexNumber;

                // Store old inventory weapon into main hand slot
                player.weaponSlotSetArray[setIndex][1] = targetWeapon;

                // Place dragged weapon into inventory array
                if (!isMultiplayer)
                {
                    player.playerInventory.inventoryArray[index] = draggableItemWeapon;
                }
                else
                {
                    player.playerInventory.inventoryArray[index] = draggableItemWeapon;
                }

                // Inventory weapon to off-hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.Inventory);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.OffHand);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex, dragFromInventory: false, dragToInventory: true, inventorySwitch: false, isMultiplayer);

                // Book update
                StaticEventHandler.CallWeaponsSwappedWithInventoryEvent(draggableItemWeapon, targetWeapon, index, setIndex, false, draggableItem, targetItem);

                // Swap belonging slots for drag-drop references
                SwapBelongingSlots(draggableItem, targetItem);

                break;
            case ItemSwapPos.DragInventorySlotMain:
                index = draggableItem.belongingSlot.inventoryIndexNumber;

                // Store old inventory weapon into main hand slot
                player.weaponSlotSetArray[setIndex][0] = draggableItemWeapon;

                // Place dragged weapon into inventory array
                if (!isMultiplayer)
                {
                    player.playerInventory.inventoryArray[index] = targetWeapon;
                }
                else
                {
                    player.playerInventory.inventoryArray[index] = targetWeapon;
                }

                // Inventory weapon to main hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.MainHand);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.Inventory);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex, dragFromInventory: true, dragToInventory: false, inventorySwitch: false, isMultiplayer);

                // Book update
                StaticEventHandler.CallWeaponsSwappedWithInventoryEvent(targetWeapon, draggableItemWeapon, index, setIndex, true, draggableItem, targetItem);

                // Swap belonging slots for drag-drop references
                SwapBelongingSlots(draggableItem, targetItem);

                break;
            case ItemSwapPos.DragInventorySlotOff:
                index = draggableItem.belongingSlot.inventoryIndexNumber;

                // Store old inventory weapon into main hand slot
                player.weaponSlotSetArray[setIndex][1] = draggableItemWeapon;

                // Place dragged weapon into inventory array
                if (!isMultiplayer)
                {
                    player.playerInventory.inventoryArray[index] = targetWeapon;
                }
                else
                {
                    player.playerInventory.inventoryArray[index] = targetWeapon;
                }

                // Inventory weapon to main hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.OffHand);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.Inventory);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex, dragFromInventory: true, dragToInventory: false, inventorySwitch: false, isMultiplayer);

                // Book update
                StaticEventHandler.CallWeaponsSwappedWithInventoryEvent(targetWeapon, draggableItemWeapon, index, setIndex, false, draggableItem, targetItem);

                // Swap belonging slots for drag-drop references
                SwapBelongingSlots(draggableItem, targetItem);

                if (draggableItemWeapon == null || targetWeapon == null)
                {
                    Debug.LogWarning("SwapProcess aborted: One or both weapons were null.");
                    return;
                }

                break;
            case ItemSwapPos.DragMainSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet - 1][0] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                targetWeapon.weaponStats.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet;
                targetWeapon.ItemSlotStatus = ItemSlotStatus.MainHand;
                draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.MainHand;
                player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex);
                StaticEventHandler.CallWeaponSwitchedEventForBook();
                break;
            case ItemSwapPos.DragMainSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet - 1][0] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                targetWeapon.weaponStats.weaponBelongingToWhichOffHandSet = 0;
                targetWeapon.weaponStats.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet;
                targetWeapon.ItemSlotStatus = ItemSlotStatus.MainHand;
                draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet = 0;
                draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.OffHand;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex);
                }
                else
                {
                    StaticEventHandler.CallWeaponSwitchedEventForBook();
                }
                break;
            case ItemSwapPos.DragOffSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet - 1][1] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                targetWeapon.weaponStats.weaponBelongingToWhichMainHandSet = 0;
                targetWeapon.weaponStats.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet;
                targetWeapon.ItemSlotStatus = ItemSlotStatus.OffHand;
                draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet = 0;
                draggableItemWeapon.weaponStats.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.MainHand;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex);
                }
                else
                {
                    StaticEventHandler.CallWeaponSwitchedEventForBook();
                }

                break;
            case ItemSwapPos.DragOffSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet - 1][1] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                targetWeapon.weaponStats.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet;
                draggableItemWeapon.weaponStats.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.ItemSlotStatus = ItemSlotStatus.OffHand;
                targetWeapon.ItemSlotStatus = ItemSlotStatus.OffHand;
                player.playerControl.SetWeaponSetByIndex(false, player.currentWeaponSlotSetIndex);

                break;
            case ItemSwapPos.DragInventorySlotInventory:
                index = draggableItem.belongingSlot.inventoryIndexNumber;

                // Swap generic items into inventory array
                if (!isMultiplayer)
                {
                    player.playerInventory.inventoryArray[index] = targetItem.itemGeneric;
                    player.playerInventory.inventoryArray[inventoryIndexNumber] = draggableItem.itemGeneric;
                }
                else
                {
                    player.playerInventory.inventoryArray[index] = targetItem.itemGeneric;
                    player.playerInventory.inventoryArray[inventoryIndexNumber] = draggableItem.itemGeneric;
                }

                // Book update
                StaticEventHandler.CallGenericItemsSwappedInInventory(draggableItem, targetItem, index, inventoryIndexNumber);

                // Swap belonging slots for drag-drop references
                SwapBelongingSlots(draggableItem, targetItem);

                break;
            default:
                break;
        }
    }

    ItemSwapPos ResolveSwapPosition(Slot fromSlot, Slot toSlot, ItemGeneric item)
    {
        // Inventory -> Inventory
        if (fromSlot.slotType == SlotType.Inventory && toSlot.slotType == SlotType.Inventory) return ItemSwapPos.DragInventorySlotInventory;

        // Inventory -> Main/Off
        if (fromSlot.slotType == SlotType.Inventory && toSlot.slotType == SlotType.WeaponMainHand) return ItemSwapPos.DragInventorySlotMain;
        if (fromSlot.slotType == SlotType.Inventory && toSlot.slotType == SlotType.WeaponOffHand) return ItemSwapPos.DragInventorySlotOff;

        // Main/Off -> Inventory
        if (fromSlot.slotType == SlotType.WeaponMainHand && toSlot.slotType == SlotType.Inventory) return ItemSwapPos.DragMainSlotInventory;
        if (fromSlot.slotType == SlotType.WeaponOffHand && toSlot.slotType == SlotType.Inventory) return ItemSwapPos.DragOffSlotInventory;

        // Main <-> Off
        if (fromSlot.slotType == SlotType.WeaponMainHand && toSlot.slotType == SlotType.WeaponMainHand) return ItemSwapPos.DragMainSlotMain;
        if (fromSlot.slotType == SlotType.WeaponMainHand && toSlot.slotType == SlotType.WeaponOffHand) return ItemSwapPos.DragMainSlotOff;
        if (fromSlot.slotType == SlotType.WeaponOffHand && toSlot.slotType == SlotType.WeaponMainHand) return ItemSwapPos.DragOffSlotMain;
        if (fromSlot.slotType == SlotType.WeaponOffHand && toSlot.slotType == SlotType.WeaponOffHand) return ItemSwapPos.DragOffSlotOff;

        if (item is PassiveItem)
        {
            if (fromSlot.slotType == SlotType.Inventory && toSlot.slotType == SlotType.Passive) return ItemSwapPos.DragPassiveInventorySlotPassive;
            if (fromSlot.slotType == SlotType.Passive && toSlot.slotType == SlotType.Inventory) return ItemSwapPos.DragPassiveSlotPassiveInventory;
        }

        return ItemSwapPos.None;
    }

    void UpdateWeaponSlotStatus(Weapon weapon, int setIndex, ItemSlotStatus status)
    {
        weapon.ItemSlotStatus = status;
        if (status == ItemSlotStatus.MainHand) weapon.weaponStats.weaponBelongingToWhichMainHandSet = setIndex + 1;
        else if (status == ItemSlotStatus.OffHand) weapon.weaponStats.weaponBelongingToWhichOffHandSet = setIndex + 1;
        else
        {
            weapon.weaponStats.weaponBelongingToWhichMainHandSet = 0;
            weapon.weaponStats.weaponBelongingToWhichOffHandSet = 0;
        }
    }

    void SwapBelongingSlots(DraggableItem a, DraggableItem b)
    {
        var temp = a.belongingSlot;
        a.belongingSlot = b.belongingSlot;
        b.belongingSlot = temp;
    }

    public void DropSelectedItem()
    {
        if (selectedSlot != null && selectedSlot.selectedSlotDraggableItem != null)
        {
            bool dropFailed = DropProcess(false, selectedSlot.selectedSlotDraggableItem);

            if (!dropFailed)
            {
                // Clean up
                selectedSlot.selectedSlotDraggableItem = null;
                selectedSlot = null;
            }
        }
    }

    public void UpgradeSelectedItem()
    {
        if (selectedSlot != null && selectedSlot.selectedSlotDraggableItem != null)
        {
            bool upgradeFailed = UpgradeProcess(selectedSlot.selectedSlotDraggableItem);

            if (!upgradeFailed)
            {
                // Clean up
                selectedSlot.selectedSlotDraggableItem = null;
                selectedSlot = null;
            }
        }
    }

    public void DismantleSelectedItem()
    {
        if (selectedSlot != null && selectedSlot.selectedSlotDraggableItem != null)
        {
            int shardGain;

            bool dissambleFailed = DismantleProcess(selectedSlot.selectedSlotDraggableItem, out shardGain);

            if (!dissambleFailed)
            {
                // Clean up
                selectedSlot.selectedSlotDraggableItem = null;
                selectedSlot = null;
            }
        }
    }

    private bool DropProcess(bool dropFailed, DraggableItem draggableItem)
    {
        if (draggableItem != null)
        {
            if (draggableItem.itemGeneric is Weapon weapon)
            {
                bool dropOffhand = draggableItem.belongingSlot.slotType == SlotType.WeaponOffHand ? true : false;
                int setIndex = 0;

                if (draggableItem.belongingSlot.slotType == SlotType.WeaponMainHand)
                {
                    setIndex = player.currentWeaponSlotSetIndex;
                }
                else if (draggableItem.belongingSlot.slotType == SlotType.WeaponOffHand)
                {
                    setIndex = player.currentWeaponSlotSetIndex;
                }
                else if (draggableItem.belongingSlot.slotType == SlotType.Inventory)
                {
                    setIndex = -1;
                }

                player.playerControl.DropProcess(ItemType.Weapon, weapon.weaponStats, default, weapon.Rarity, isServer: false, weapon.ItemSlotStatus,
                    draggableItem.belongingSlot.inventoryIndexNumber, setIndex, dropButton: true);
            }
            else if (draggableItem.itemGeneric is PassiveItem passiveItem)
            {
                player.playerControl.DropProcess(ItemType.PassiveItem, default, passiveItem.passiveStats, passiveItem.Rarity, isServer: false, passiveItem.ItemSlotStatus,
                    draggableItem.belongingSlot.inventoryIndexNumber, setIndex: -1, dropButton: true);

                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

                // Book update
                if (passiveItem.ItemSlotStatus == ItemSlotStatus.Inventory)
                {

                    StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(draggableItem.belongingSlot.inventoryIndexNumber);
                }
                else
                {
                    StaticEventHandler.CallItemRemovedFromPassiveItemSlot(passiveItem.passiveStats.passiveItemSlotName);
                }
            }
        }

        return dropFailed;
    }

    private bool UpgradeProcess(DraggableItem draggableItem)
    {
        if (draggableItem == null) return false;

        // Only allow upgrading items that are in the inventory
        if (slotType != SlotType.Upgrade || draggableItem.itemGeneric.ItemSlotStatus != ItemSlotStatus.Inventory) return false;

        ItemGeneric item = draggableItem.itemGeneric;

        if (item == null) return false;

        // Already maxed?
        if (item.Rarity == Rarity.Legendary) return false;

        // Determine shard cost and next rarity
        int shardCost = draggableItem.itemGeneric.Rarity switch
        {
            Rarity.Basic => 100, // Uprade cost to enchanted
            Rarity.Enchanted => 350, // Upgrade cost to mythic,
            Rarity.Mythic => int.MaxValue, // Currently max level is mythic; legendary is not accessed
            Rarity.Legendary => int.MaxValue,
            _ => int.MaxValue
        };

        // Enough shards?
        if (draggableItem.itemGeneric.Rarity >= Rarity.Mythic)
        {
            GameManager.Instance.OpenPopUpLog(PopUpReason.ReachedMaxUpgradeLevel);
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
            return false;
        }
        else if (player.coinsAndShards.GetCurrentShard() < shardCost)
        {
            GameManager.Instance.OpenPopUpLog(PopUpReason.NotEnoughShardsForUpgrade);
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
            return false;
        }

        // Try the upgrade
        bool success = UpgradeItem(draggableItem, inventoryIndexNumber);

        if (success) GameManager.Instance.OpenPopUpLog(PopUpReason.UpgradeCompletedLog);
        else GameManager.Instance.OpenPopUpLog(PopUpReason.NotEnoughShardsForUpgrade);

        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);

        if (!success) return false;

        // Commit the shard spend only on success
        player.coinsAndShards.RemoveShard(shardCost);
        return true;
    }

    private bool UpgradeItem(DraggableItem draggableItem, int inventoryIndex)
    {
        if (draggableItem == null || draggableItem.itemGeneric == null) return false;

        ItemGeneric item = draggableItem.itemGeneric;

        // Generate seed
        int seed = Random.Range(int.MinValue, int.MaxValue);
        WartheonRNG rng = new WartheonRNG(seed);

        // Promote rarity
        Rarity next = item.Rarity switch
        {
            Rarity.Basic => Rarity.Enchanted,
            Rarity.Enchanted => Rarity.Mythic,
            Rarity.Mythic => Rarity.Legendary,
            _ => item.Rarity
        };

        if (next == item.Rarity) return false; // nothing to do

        // Mutate in-place by type
        if (item is Weapon upgWeapon)
        {
            // Ensure we have details
            if (upgWeapon.weaponStats.weaponTitle == WeaponTitle.None) return false;

            WeaponDetailsSO upgWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(upgWeapon.weaponStats.weaponTitle);

            upgWeapon.Rarity = next;
            upgWeapon.weaponStats.baseTypeRolled = upgWeaponDetails.baseTypeModifier;
            upgWeapon.weaponStats.baseUniqueRolled = upgWeaponDetails.baseUniqueModifier;

            // Add new rolls when crossing thresholds
            List<BoostType> pool = upgWeaponDetails.additionalModifierPoolForType;

            if (pool != null && pool.Count > 0)
            {
                if (next >= Rarity.Enchanted && upgWeapon.weaponStats.enchantedBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType>
                {
                    upgWeaponDetails.baseUniqueModifier,
                    upgWeaponDetails.baseTypeModifier

                });
                    upgWeapon.weaponStats.enchantedBoostType = rolled;
                    WeaponDropGenerator.SetWeaponModifier(ref upgWeapon, rolled, upgWeaponDetails, rng);
                    //ApplyWeaponBoost(upgWeapon, rolled, upgWeapon.weaponDetails);
                }

                if (next >= Rarity.Mythic && upgWeapon.weaponStats.mythicBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> {
                    upgWeaponDetails.baseUniqueModifier,
                    upgWeaponDetails.baseTypeModifier,
                    upgWeapon.weaponStats.enchantedBoostType
                });
                    upgWeapon.weaponStats.mythicBoostType = rolled;
                    WeaponDropGenerator.SetWeaponModifier(ref upgWeapon, rolled, upgWeaponDetails, rng);
                    //ApplyWeaponBoost(upgWeapon, rolled, upgWeapon.weaponDetails);
                }
            }

            StaticEventHandler.CallInventoryWeaponUpgradedEventForBook(upgWeapon, draggableItem.belongingSlot.inventoryIndexNumber);
            return true;
        }
        else if (item is PassiveItem upgPassive)
        {
            if (upgPassive.passiveStats.passiveItemType != PassiveItemType.None) return false;

            PassiveItemDetailsSO upgPassiveDetails = WartheonDatabase.Instance.GetPassiveItemDetails(upgPassive.passiveStats.passiveItemType);

            upgPassive.Rarity = next;
            upgPassive.passiveStats.baseUniqueRolled = upgPassiveDetails.baseUniqueModifier;

            List<BoostType> pool = upgPassiveDetails.additionalModifierPoolForType;

            if (pool != null && pool.Count > 0)
            {
                if (next >= Rarity.Enchanted && upgPassive.passiveStats.enchantedBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> { upgPassiveDetails.baseUniqueModifier });
                    upgPassive.passiveStats.enchantedBoostType = rolled;
                    PassiveDropGenerator.SetPassiveItemModifier(ref upgPassive, rolled, upgPassiveDetails, rng);
                    //ApplyPassiveBoost(upgPassive, rolled);
                }

                if (next >= Rarity.Mythic && upgPassive.passiveStats.mythicBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> { upgPassiveDetails.baseUniqueModifier, upgPassive.passiveStats.enchantedBoostType });
                    upgPassive.passiveStats.mythicBoostType = rolled;
                    PassiveDropGenerator.SetPassiveItemModifier(ref upgPassive, rolled, upgPassiveDetails, rng);
                    //ApplyPassiveBoost(upgPassive, rolled);
                }
            }

            StaticEventHandler.CallInventoryPassiveUpgradedEventForBook(upgPassive, inventoryIndexNumber);
            return true;
        }

        return false;
    }

    BoostType RollOne(List<BoostType> pool, HashSet<BoostType> exclude)
    {
        List<BoostType> candidates = new List<BoostType>();

        foreach (var b in pool)
            if (!exclude.Contains(b)) candidates.Add(b);

        if (candidates.Count == 0) return BoostType.None;

        int idx = Random.Range(0, candidates.Count);
        return candidates[idx];
    }

    private bool DismantleProcess(DraggableItem draggableItem, out int shardGain)
    {
        if (draggableItem != null)
        {
            shardGain = 0;

            // Inventory Slot Check
            if (slotType == SlotType.Dismantle && draggableItem.itemGeneric.ItemSlotStatus == ItemSlotStatus.Inventory)
            {
                switch (draggableItem.itemGeneric.Rarity)
                {
                    case Rarity.Basic: shardGain = 10; break;
                    case Rarity.Enchanted: shardGain = 35; break;
                    case Rarity.Mythic: shardGain = 100; break;
                    case Rarity.Legendary: shardGain = 250; break;
                    default:
                        break;
                }

                player.coinsAndShards.AddShard(shardGain);

                if (!NetworkServer.active && !NetworkClient.active) player.playerInventory.EmptyItemFromInventory(draggableItem.belongingSlot.inventoryIndexNumber);
                else player.playerInventory.EmptyItemFromInventory(draggableItem.belongingSlot.inventoryIndexNumber);

                // Book update
                if (draggableItem.itemGeneric is Weapon)
                {
                    StaticEventHandler.CallInventoryWeaponDroppedEventForBook(draggableItem.belongingSlot.inventoryIndexNumber);
                }
                else if (draggableItem.itemGeneric is PassiveItem)
                {
                    StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(draggableItem.belongingSlot.inventoryIndexNumber);
                }
            }

            return false;
        }

        shardGain = 0;

        return true;
    }

    private void ApplyWeaponBoost(Weapon weapon, BoostType boost, WeaponDetailsSO weaponDetails)
    {
        // Determine base damage safely (shields -> 0)
        int baseMin = 0, baseMax = 0;

        // Prefer an explicit flag if you have one; otherwise derive from class
        bool isShield = (weaponDetails.weaponClass == WeaponClass.Shield);
        // If you have a bool on the SO: isShield |= d.isShield;

        if (!isShield)
        {
            if (weaponDetails.isMeleeWeapon)
            {
                baseMin = weaponDetails.physicalDamageMin;
                baseMax = weaponDetails.physicalDamageMax;
            }
            else
            {
                var proj = weaponDetails.weaponCurrentProjectile;
                baseMin = proj != null ? proj.projectilePhyDamageMin : 0;
                baseMax = proj != null ? proj.projectilePhyDamageMax : 0;
            }
        }

        // Split by forge as before
        int margin = baseMax - baseMin;

        float rng = Random.Range(1f, 1.5f);
        float rng2;

        switch (boost)
        {
            case BoostType.AttackCooldown:
                rng2 = Random.Range(0.05f, 0.12f);
                weapon.weaponStats.attackCooldownModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                // Affect PHYSICAL slice
                rng2 = Random.Range(1, 16);
                weapon.weaponStats.physicalAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                weapon.weaponStats.attackRatingIncrease = (float)Math.Round(rng - 1, 2);
                break;
            case BoostType.MagicDamage:
                // Affect MAGIC slice
                rng2 = Random.Range(1, 11);
                weapon.weaponStats.magicAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.weaponStats.criticalHitChanceIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritDamage:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.weaponStats.criticalHitDamageIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = Random.Range(1, 11);
                weapon.weaponStats.lifeStealAmount = Mathf.RoundToInt(rng2);
                break;
            case BoostType.BlockChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.weaponStats.blockChanceIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.weaponStats.dodgeChanceIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = Random.Range(-1f, 1f);
                weapon.weaponStats.increasedMaxHealth = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = Random.Range(-1f, 1f);
                weapon.weaponStats.increasedMaxMana = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.weaponStats.statusResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = Random.Range(1, 11);
                weapon.weaponStats.damageVsLowHealthEnemies = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.weaponStats.criticalResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.weaponStats.armorIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.weaponStats.magicResistance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = Random.Range(0.5f, 2f);
                weapon.weaponStats.speedIncreaseModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = Random.Range(0.05f, 0.15f);
                weapon.weaponStats.damageReductionRate = Mathf.Round(rng2 * 100f) / 100f;
                break;
            case BoostType.ArmorPenetration:
                rng2 = Random.Range(0.05f, 0.15f);
                weapon.weaponStats.armorPenetration = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackRange:
                rng2 = Random.Range(0.05f, 0.30f);
                if (weaponDetails.weaponCurrentProjectile != null)
                    weapon.weaponStats.attackRange = Mathf.Round(weaponDetails.weaponCurrentProjectile.projectileRange * rng2 * 100f) / 100f;
                break;
            case BoostType.SkillCooldown:
                rng2 = Random.Range(0.05f, 0.13f);
                weapon.weaponStats.skillCooldown = (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillDuration:
                rng2 = Random.Range(0.05f, 0.4f);
                weapon.weaponStats.skillDuration = (float)Math.Round(rng2, 2);
                break;

            default:
                break;
        }
    }

    private void ApplyPassiveBoost(PassiveItem passiveItem, BoostType boost)
    {
        float rng = Random.Range(1f, 1.5f);
        float rng2;

        switch (boost)
        {
            case BoostType.AttackCooldown:
                rng2 = Random.Range(0.05f, 0.12f);
                passiveItem.passiveStats.attackCooldown = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                // Affect PHYSICAL slice
                rng2 = Random.Range(1, 16);
                passiveItem.passiveStats.physicalAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                passiveItem.passiveStats.attackRating = (float)Math.Round(rng - 1, 2);
                break;
            case BoostType.MagicDamage:
                // Affect MAGIC slice
                rng2 = Random.Range(1, 16);
                passiveItem.passiveStats.magicAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.passiveStats.criticalHitChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = Random.Range(1, 8);
                passiveItem.passiveStats.lifeStealAmount = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritDamage:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.passiveStats.criticalHitDamage = (float)Math.Round(rng2, 2);
                break;
            case BoostType.BlockChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.passiveStats.blockChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.passiveStats.dodgeChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = Random.Range(-1f, 1f);
                passiveItem.passiveStats.increasedMaxHealth = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = Random.Range(-1f, 1f);
                passiveItem.passiveStats.increasedMaxMana = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.passiveStats.statusResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = Random.Range(1, 8);
                passiveItem.passiveStats.damageVsLowHealthEnemies = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.passiveStats.criticalResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.passiveStats.armorIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.passiveStats.magicResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = Random.Range(0.5f, 2f);
                passiveItem.passiveStats.speedIncreaseModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = Random.Range(0.05f, 0.15f);
                passiveItem.passiveStats.damageReductionRate = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorPenetration:
                rng2 = Random.Range(0.05f, 0.15f);
                passiveItem.passiveStats.armorPenetration = (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillCooldown:
                rng2 = Random.Range(0.05f, 0.13f);
                passiveItem.passiveStats.skillCooldown = (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillDuration:
                rng2 = Random.Range(0.05f, 0.4f);
                passiveItem.passiveStats.skillDuration = (float)Math.Round(rng2, 2);
                break;
            default:
                break;
        }
    }

    private void BoostTypeColorUpdate(Weapon weapon)
    {
        switch (weapon.Rarity)
        {
            case Rarity.Basic:
                headerText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                levelText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                break;
            case Rarity.Enchanted:
                headerText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                levelText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                break;
            case Rarity.Mythic:
                headerText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                levelText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                break;
            case Rarity.Legendary:
                headerText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                levelText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                break;
            default:
                break;
        }
    }

    private void BoostTypeColorUpdate(PassiveItem passiveItem)
    {
        switch (passiveItem.Rarity)
        {
            case Rarity.Basic:
                headerText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                levelText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                break;
            case Rarity.Enchanted:
                headerText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                levelText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                break;
            case Rarity.Mythic:
                headerText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                levelText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                break;
            case Rarity.Legendary:
                headerText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                levelText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                break;
            default:
                break;
        }
    }

    private void BoostForWeapon(Weapon weapon, BoostType boostType, BoostPhase boostPhase)
    {
        if (boostType != BoostType.None)
        {
            if (boostPhase == BoostPhase.Unique)
            {
                switch (boostType)
                {
                    case BoostType.AttackCooldown:
                        bonusText.text = $"Attack Speed: + {weapon.weaponStats.attackCooldownModifier * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text = "Phy. Attack Dmg.: + " + weapon.weaponStats.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text = $"Attack Rating: + {weapon.weaponStats.attackRatingIncrease * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text = "Magic Attack Dmg.: + " + weapon.weaponStats.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text = $"Cr. Hit Chance: + {weapon.weaponStats.criticalHitChanceIncrease * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text = $"Cr. Hit Damage: + {weapon.weaponStats.criticalHitDamageIncrease * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text = $"Life Steal: + {weapon.weaponStats.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text = $"Block Chance: + {weapon.weaponStats.blockChanceIncrease * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text = $"Dodge Chance: + {weapon.weaponStats.dodgeChanceIncrease * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text = $"Health: + {weapon.weaponStats.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text = $"Mana: + {weapon.weaponStats.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text = $"Status Resistance: + {weapon.weaponStats.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text = $"Damage vs Low Health: + {weapon.weaponStats.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text = $"Cr. Resistance: + {weapon.weaponStats.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text = $"Armor: + {weapon.weaponStats.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text = $"Magic Resistance: + {weapon.weaponStats.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text = $"Move Speed: + {weapon.weaponStats.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text = $"Damage Reduction: + {weapon.weaponStats.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text = $"Armor Penetration: + {weapon.weaponStats.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text = $"Skill Cooldown: + {weapon.weaponStats.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text = $"Skill Duration: + {weapon.weaponStats.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (weapon.weaponStats.additionalPoisonChance > 0) bonusText.text = $"Poison Chance: + {weapon.weaponStats.additionalPoisonChance * 100}%";
                        else if (weapon.weaponStats.additionalBleedChance > 0) bonusText.text = $"Bleed Chance: + {weapon.weaponStats.additionalBleedChance * 100}%";
                        else if (weapon.weaponStats.additionalRootChance > 0) bonusText.text = $"Root Chance: + {weapon.weaponStats.additionalRootChance * 100}%";
                        else if (weapon.weaponStats.additionalStunChance > 0) bonusText.text = $"Stun Chance: + {weapon.weaponStats.additionalStunChance * 100}%";
                        else if (weapon.weaponStats.additionalCurseChance > 0) bonusText.text = $"Curse Chance: + {weapon.weaponStats.additionalCurseChance * 100}%";
                        else if (weapon.weaponStats.additionalFearChance > 0) bonusText.text = $"Fear Chance: + {weapon.weaponStats.additionalFearChance * 100}%";
                        else if (weapon.weaponStats.additionalRevealChance > 0) bonusText.text = $"Reveal Chance: + {weapon.weaponStats.additionalRevealChance * 100}%";
                        else if (weapon.weaponStats.additionalParalyzeChance > 0) bonusText.text = $"Paralyze Chance: + {weapon.weaponStats.additionalParalyzeChance * 100}%";
                        else if (weapon.weaponStats.additionalBurnChance > 0) bonusText.text = $"Burn Chance: + {weapon.weaponStats.additionalBurnChance * 100}%";
                        else if (weapon.weaponStats.additionalFreezeChance > 0) bonusText.text = $"Freeze Chance: + {weapon.weaponStats.additionalFreezeChance * 100}%";
                        else if (weapon.weaponStats.additionalBlindChance > 0) bonusText.text = $"Blind Chance: + {weapon.weaponStats.additionalBlindChance * 100}%";
                        else if (weapon.weaponStats.additionalSlowChance > 0) bonusText.text = $"Slow Chance: + {weapon.weaponStats.additionalSlowChance * 100}%";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (boostType)
                {
                    case BoostType.AttackCooldown:
                        bonusText.text += $"\nAttack Speed: + {weapon.weaponStats.attackCooldownModifier * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text += "\nPhy. Attack Dmg.: + " + weapon.weaponStats.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text += $"\nAttack Rating: + {weapon.weaponStats.attackRatingIncrease * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text += "\nMagic Attack Dmg.: + " + weapon.weaponStats.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text += $"\nCr. Hit Chance: + {weapon.weaponStats.criticalHitChanceIncrease * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text += $"\nCr. Hit Damage: + {weapon.weaponStats.criticalHitDamageIncrease * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text += $"\nLife Steal: + {weapon.weaponStats.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text += $"\nBlock Chance: + {weapon.weaponStats.blockChanceIncrease * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text += $"\nDodge Chance: + {weapon.weaponStats.dodgeChanceIncrease * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text += $"\nHealth: + {weapon.weaponStats.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text += $"\nMana: + {weapon.weaponStats.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text += $"\nStatus Resistance: + {weapon.weaponStats.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text += $"\nDamage vs Low Health: + {weapon.weaponStats.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text += $"\nCr. Resistance: + {weapon.weaponStats.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text += $"\nArmor: + {weapon.weaponStats.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text += $"\nMagic Resistance: + {weapon.weaponStats.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text += $"\nMove Speed: + {weapon.weaponStats.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text += $"\nDamage Reduction: + {weapon.weaponStats.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text += $"\nArmor Penetration: + {weapon.weaponStats.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text += $"\nSkill Cooldown: + {weapon.weaponStats.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text += $"\nSkill Duration: + {weapon.weaponStats.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (weapon.weaponStats.additionalPoisonChance > 0) bonusText.text += $"\nPoison Chance: + {weapon.weaponStats.additionalPoisonChance * 100}%";
                        else if (weapon.weaponStats.additionalBleedChance > 0) bonusText.text += $"\nBleed Chance: + {weapon.weaponStats.additionalBleedChance * 100}%";
                        else if (weapon.weaponStats.additionalRootChance > 0) bonusText.text += $"\nRoot Chance: + {weapon.weaponStats.additionalRootChance * 100}%";
                        else if (weapon.weaponStats.additionalStunChance > 0) bonusText.text += $"\nStun Chance: + {weapon.weaponStats.additionalStunChance * 100}%";
                        else if (weapon.weaponStats.additionalCurseChance > 0) bonusText.text += $"\nCurse Chance: + {weapon.weaponStats.additionalCurseChance * 100}%";
                        else if (weapon.weaponStats.additionalFearChance > 0) bonusText.text += $"\nFear Chance: + {weapon.weaponStats.additionalFearChance * 100}%";
                        else if (weapon.weaponStats.additionalRevealChance > 0) bonusText.text += $"\nReveal Chance: + {weapon.weaponStats.additionalRevealChance * 100}%";
                        else if (weapon.weaponStats.additionalParalyzeChance > 0) bonusText.text += $"\nParalyze Chance: + {weapon.weaponStats.additionalParalyzeChance * 100}%";
                        else if (weapon.weaponStats.additionalBurnChance > 0) bonusText.text += $"\nBurn Chance: + {weapon.weaponStats.additionalBurnChance * 100}%";
                        else if (weapon.weaponStats.additionalFreezeChance > 0) bonusText.text += $"\nFreeze Chance: + {weapon.weaponStats.additionalFreezeChance * 100}%";
                        else if (weapon.weaponStats.additionalBlindChance > 0) bonusText.text += $"\nBlind Chance: + {weapon.weaponStats.additionalBlindChance * 100}%";
                        else if (weapon.weaponStats.additionalSlowChance > 0) bonusText.text += $"\nSlow Chance: + {weapon.weaponStats.additionalSlowChance * 100}%";
                        break;
                    default:
                        break;
                }
            }
        }
    }

    private void BoostForPassiveItem(PassiveItem passiveItem, BoostType boostType, BoostPhase boostPhase)
    {
        if (boostType != BoostType.None)
        {
            if (boostPhase == BoostPhase.Unique)
            {
                switch (boostType)
                {
                    case BoostType.AttackCooldown:
                        bonusText.text = $"Attack Speed: + {passiveItem.passiveStats.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text = "Phy. Attack Dmg.: + " + passiveItem.passiveStats.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text = $"Attack Rating: + {passiveItem.passiveStats.attackRating * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text = "Magic Attack Dmg.: + " + passiveItem.passiveStats.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text = $"Cr. Hit Chance: + {passiveItem.passiveStats.criticalHitChance * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text = $"Cr. Hit Damage: + {passiveItem.passiveStats.criticalHitDamage * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text = $"Life Steal: + {passiveItem.passiveStats.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text = $"Block Chance: + {passiveItem.passiveStats.blockChance * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text = $"Dodge Chance: + {passiveItem.passiveStats.dodgeChance * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text = $"Health: + {passiveItem.passiveStats.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text = $"Mana: + {passiveItem.passiveStats.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text = $"Status Resistance: + {passiveItem.passiveStats.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text = $"Damage vs Low Health: + {passiveItem.passiveStats.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text = $"Cr. Resistance: + {passiveItem.passiveStats.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text = $"Armor: + {passiveItem.passiveStats.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text = $"Magic Resistance: + {passiveItem.passiveStats.magicResistanceModifier * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text = $"Move Speed: + {passiveItem.passiveStats.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text = $"Damage Reduction: + {passiveItem.passiveStats.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text = $"Armor Penetration: + {passiveItem.passiveStats.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text += $"\nSkill Cooldown: + {passiveItem.passiveStats.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text += $"\nSkill Duration: + {passiveItem.passiveStats.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (passiveItem.passiveStats.additionalPoisonChance > 0) bonusText.text = $"Poison Chance: + {passiveItem.passiveStats.additionalPoisonChance * 100}%";
                        else if (passiveItem.passiveStats.additionalBleedChance > 0) bonusText.text = $"Bleed Chance: + {passiveItem.passiveStats.additionalBleedChance * 100}%";
                        else if (passiveItem.passiveStats.additionalRootChance > 0) bonusText.text = $"Root Chance: + {passiveItem.passiveStats.additionalRootChance * 100}%";
                        else if (passiveItem.passiveStats.additionalStunChance > 0) bonusText.text = $"Stun Chance: + {passiveItem.passiveStats.additionalStunChance * 100}%";
                        else if (passiveItem.passiveStats.additionalCurseChance > 0) bonusText.text = $"Curse Chance: + {passiveItem.passiveStats.additionalCurseChance * 100}%";
                        else if (passiveItem.passiveStats.additionalFearChance > 0) bonusText.text = $"Fear Chance: + {passiveItem.passiveStats.additionalFearChance * 100}%";
                        else if (passiveItem.passiveStats.additionalRevealChance > 0) bonusText.text = $"Reveal Chance: + {passiveItem.passiveStats.additionalRevealChance * 100}%";
                        else if (passiveItem.passiveStats.additionalParalyzeChance > 0) bonusText.text = $"Paralyze Chance: + {passiveItem.passiveStats.additionalParalyzeChance * 100}%";
                        else if (passiveItem.passiveStats.additionalBurnChance > 0) bonusText.text = $"Burn Chance: + {passiveItem.passiveStats.additionalBurnChance * 100}%";
                        else if (passiveItem.passiveStats.additionalFreezeChance > 0) bonusText.text = $"Freeze Chance: + {passiveItem.passiveStats.additionalFreezeChance * 100}%";
                        else if (passiveItem.passiveStats.additionalBlindChance > 0) bonusText.text = $"Blind Chance: + {passiveItem.passiveStats.additionalBlindChance * 100}%";
                        else if (passiveItem.passiveStats.additionalSlowChance > 0) bonusText.text = $"Slow Chance: + {passiveItem.passiveStats.additionalSlowChance * 100}%";
                        break;
                    default:
                        break;
                }
            }
            else
            {
                switch (boostType)
                {
                    case BoostType.AttackCooldown:
                        bonusText.text += $"\nAttack Speed: + {passiveItem.passiveStats.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text += "\nPhy. Attack Dmg.: + " + passiveItem.passiveStats.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text += $"\nAttack Rating: + {passiveItem.passiveStats.attackRating * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text += "\nMagic Attack Dmg.: + " + passiveItem.passiveStats.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text += $"\nCr. Hit Chance: + {passiveItem.passiveStats.criticalHitChance * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text += $"\nCr. Hit Damage: + {passiveItem.passiveStats.criticalHitDamage * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text += $"\nLife Steal: + {passiveItem.passiveStats.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text += $"\nBlock Chance: + {passiveItem.passiveStats.blockChance * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text += $"\nDodge Chance: + {passiveItem.passiveStats.dodgeChance * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text += $"\nHealth: + {passiveItem.passiveStats.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text += $"\nMana: + {passiveItem.passiveStats.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text += $"\nStatus Resistance: + {passiveItem.passiveStats.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text += $"\nDamage vs Low Health: + {passiveItem.passiveStats.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text += $"\nCr. Resistance: + {passiveItem.passiveStats.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text += $"\nArmor: + {passiveItem.passiveStats.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text += $"\nMagic Resistance: + {passiveItem.passiveStats.magicResistanceModifier * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text += $"\nMove Speed: + {passiveItem.passiveStats.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text += $"\nDamage Reduction: + {passiveItem.passiveStats.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text += $"\nArmor Penetration: + {passiveItem.passiveStats.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text += $"\nSkill Cooldown: + {passiveItem.passiveStats.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text += $"\nSkill Duration: + {passiveItem.passiveStats.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (passiveItem.passiveStats.additionalPoisonChance > 0) bonusText.text += $"\nPoison Chance: + {passiveItem.passiveStats.additionalPoisonChance * 100}%";
                        else if (passiveItem.passiveStats.additionalBleedChance > 0) bonusText.text += $"\nBleed Chance: + {passiveItem.passiveStats.additionalBleedChance * 100}%";
                        else if (passiveItem.passiveStats.additionalRootChance > 0) bonusText.text += $"\nRoot Chance: + {passiveItem.passiveStats.additionalRootChance * 100}%";
                        else if (passiveItem.passiveStats.additionalStunChance > 0) bonusText.text += $"\nStun Chance: + {passiveItem.passiveStats.additionalStunChance * 100}%";
                        else if (passiveItem.passiveStats.additionalCurseChance > 0) bonusText.text += $"\nCurse Chance: + {passiveItem.passiveStats.additionalCurseChance * 100}%";
                        else if (passiveItem.passiveStats.additionalFearChance > 0) bonusText.text += $"\nFear Chance: + {passiveItem.passiveStats.additionalFearChance * 100}%";
                        else if (passiveItem.passiveStats.additionalRevealChance > 0) bonusText.text += $"\nReveal Chance: + {passiveItem.passiveStats.additionalRevealChance * 100}%";
                        else if (passiveItem.passiveStats.additionalParalyzeChance > 0) bonusText.text += $"\nParalyze Chance: + {passiveItem.passiveStats.additionalParalyzeChance * 100}%";
                        else if (passiveItem.passiveStats.additionalBurnChance > 0) bonusText.text += $"\nBurn Chance: + {passiveItem.passiveStats.additionalBurnChance * 100}%";
                        else if (passiveItem.passiveStats.additionalFreezeChance > 0) bonusText.text += $"\nFreeze Chance: + {passiveItem.passiveStats.additionalFreezeChance * 100}%";
                        else if (passiveItem.passiveStats.additionalBlindChance > 0) bonusText.text += $"\nBlind Chance: + {passiveItem.passiveStats.additionalBlindChance * 100}%";
                        else if (passiveItem.passiveStats.additionalSlowChance > 0) bonusText.text += $"\nSlow Chance: + {passiveItem.passiveStats.additionalSlowChance * 100}%";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}