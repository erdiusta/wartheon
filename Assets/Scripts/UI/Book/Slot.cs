using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;
using System;
using Random = UnityEngine.Random;

public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler, ISelectHandler, IDeselectHandler
{
    public static Slot selectedSlot;
    public static GameObject currentOpenTooltip;

    [HideInInspector] public DraggableItem selectedSlotDraggableItem;
    [HideInInspector] public Transform equippedTransform;

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

    private void Awake()
    {
        slotRect = GetComponent<RectTransform>();
    }

    private void OnEnable()
    {
        player = GameManager.Instance.GetPlayer();

        if (slotType != SlotType.Drop && slotType != SlotType.Upgrade && slotType != SlotType.Dismantle)
        {
            equippedTransform = transform.GetChild(1);

            UpdateTooltipPanelInfo();
        }
    }

    private void OnDisable()
    {
        if (TooltipManager.Instance != null) TooltipManager.Instance.Hide();
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
        if (TooltipManager.Instance == null) return;

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
        if (TooltipManager.Instance == null) return;

        TooltipManager.Instance.Hide();
        isOnHoverProcess = false;
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
        if (TooltipManager.Instance == null) return;

        //if (!BookUI.IsBookOpen) return;

        if (equippedTransform == null) return;

        tooltipRect = TooltipManager.Instance.TooltipRect;
        tooltipParent = TooltipManager.Instance.TooltipParent;

        if (tooltipRect == null || tooltipParent == null) return;

        // Slot Empty Check
        if(equippedTransform.childCount == 0)
        {
            TooltipManager.Instance.Hide();
            return;
        }

        Transform currentChild = equippedTransform.GetChild(0);

        DraggableItem draggableItem = currentChild.GetComponent<DraggableItem>();

        if (draggableItem == null || draggableItem.isLockIcon || draggableItem.itemGeneric == null)
        {
            TooltipManager.Instance.Hide();
            return;
        }

        headerText = TooltipManager.Instance.HeaderText;
        levelText = TooltipManager.Instance.LevelText;
        contentText = TooltipManager.Instance.ContentText;
        bonusText = TooltipManager.Instance.BonusText;

        // Inventory Slot Validation
        if (inventoryIndexNumber >= 0)
        {
            if (InventoryManager.Instance.inventoryArray[inventoryIndexNumber] == null)
            {
                TooltipManager.Instance.Hide();
                return;
            }
        }

        // Passive Item
        if (draggableItem.itemGeneric is PassiveItem)
        {
            PassiveItem passiveItem = draggableItem.GetDraggedPassiveItem();

            if (passiveItem == null)
            {
                TooltipManager.Instance.Hide();
                return;
            }

            ReshapeTooltip(false);
            ClearTooltipTexts();
            PositionTooltip();
            WriteTooltipTextForPassiveItem(passiveItem);

            TooltipManager.Instance.Show();
            return;
        }

        // Weapon
        if (draggableItem.itemGeneric is Weapon weapon)
        {
            // Off-hand lock check
            if (slotType == SlotType.WeaponOffHand && player.activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.wieldType == WieldType.TwoHanded)
            {
                TooltipManager.Instance.Hide();
                return;
            }

            ReshapeTooltip(isWeapon: true);
            ClearTooltipTexts();
            PositionTooltip();
            WriteToolTipTextForWeapon(weapon);

            TooltipManager.Instance.Show();
            return;
        }

        // Fallback
        TooltipManager.Instance.Hide();
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

        headerText.text = passiveItem.passiveItemDetails.passiveItemName;
        levelText.text = $"({passiveItem.rarity.ToString()})";

        switch (passiveItem.rarity)
        {
            case Rarity.Basic:
                BoostForPassiveItem(passiveItem, passiveItem.baseUniqueRolled, BoostPhase.Unique);
                BoostForPassiveItem(passiveItem, passiveItem.baseTypeRolled, BoostPhase.Type);
                break;
            case Rarity.Enchanted:
                BoostForPassiveItem(passiveItem, passiveItem.baseUniqueRolled, BoostPhase.Unique);
                BoostForPassiveItem(passiveItem, passiveItem.baseTypeRolled, BoostPhase.Type);
                BoostForPassiveItem(passiveItem, passiveItem.enchantedBoostType, BoostPhase.Enchanted);
                break;
            case Rarity.Mythic:
                BoostForPassiveItem(passiveItem, passiveItem.baseUniqueRolled, BoostPhase.Unique);
                BoostForPassiveItem(passiveItem, passiveItem.baseTypeRolled, BoostPhase.Type);
                BoostForPassiveItem(passiveItem, passiveItem.enchantedBoostType, BoostPhase.Enchanted);
                BoostForPassiveItem(passiveItem, passiveItem.mythicBoostType, BoostPhase.Mythic);
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

        headerText.text = weapon.weaponDetails.weaponName;
        levelText.text = $"({weapon.rarity.ToString()})";
        contentText.text = $"Class: {weapon.weaponDetails.weaponClass.ToString()}\n";

        if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
        {
            contentText.text = $"\nWield Type: {weapon.weaponDetails.wieldType.ToString()}\n";
            contentText.text += $"Block Rate: {(weapon.weaponDetails.blockChance + weapon.blockChanceIncrease) * 100}%\n";
        }
        else
        {
            float fireRate = (float)Math.Round(1 / (weapon.weaponDetails.weaponCooldownDuration - (weapon.attackCooldownModifier / 2)), 2);
            contentText.text += $"Attack Speed: {fireRate}\n";
            contentText.text += $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}\n";

            contentText.text += $"Phy. Damage: {weapon.weaponDetails.physicalDamageMin + weapon.physicalAttackDamageIncrease}-" +
                $"{weapon.weaponDetails.physicalDamageMax + weapon.physicalAttackDamageIncrease}\n";

            contentText.text += $"Magic Damage: {weapon.weaponDetails.magicDamageMin + weapon.magicAttackDamageIncrease}-" +
                $"{weapon.weaponDetails.magicDamageMax + weapon.magicAttackDamageIncrease}\n";

            float updatedAttackRating = (float)Math.Round(weapon.weaponDetails.weaponAttackRating * weapon.attackRatingIncrease, 2);
            contentText.text += $"Attack Rating: {updatedAttackRating * 100}%\n";

            contentText.text += $"Cr. Hit Chance: {weapon.criticalHitChanceIncrease * 100}%\n";
            contentText.text += $"Cr. Hit Damage: {weapon.criticalHitDamageIncrease * 100}%\n";
        }

        switch (weapon.rarity)
        {
            case Rarity.Basic:
                BoostForWeapon(weapon, weapon.baseUniqueRolled, BoostPhase.Unique);
                BoostForWeapon(weapon, weapon.baseTypeRolled, BoostPhase.Type);
                break;
            case Rarity.Enchanted:
                BoostForWeapon(weapon, weapon.baseUniqueRolled, BoostPhase.Unique);
                BoostForWeapon(weapon, weapon.baseTypeRolled, BoostPhase.Type);
                BoostForWeapon(weapon, weapon.enchantedBoostType, BoostPhase.Enchanted);
                break;
            case Rarity.Mythic:
                BoostForWeapon(weapon, weapon.baseUniqueRolled, BoostPhase.Unique);
                BoostForWeapon(weapon, weapon.baseTypeRolled, BoostPhase.Type);
                BoostForWeapon(weapon, weapon.enchantedBoostType, BoostPhase.Enchanted);
                BoostForWeapon(weapon, weapon.mythicBoostType, BoostPhase.Mythic);
                break;
            case Rarity.Legendary:
                break;
            default:
                break;
        }
    }

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

                    if (slotType == SlotType.WeaponMainHand || slotType == SlotType.WeaponOffHand || slotType == SlotType.Passive)
                    {
                        // BookUIWrapper
                        BookUIRefreshHelper.RefreshBookUIAfterItemPlacement(selectedSlot.selectedSlotDraggableItem.itemGeneric, selectedSlot, this);
                    }

                    // Clear old visual
                    if (selectedSlot.equippedTransform.childCount > 0)
                    {
                        Transform oldChild = selectedSlot.equippedTransform.GetChild(0);
                        Destroy(oldChild.gameObject);
                    }
                }

                tooltipRect.gameObject.SetActive(false);
                selectedSlot.selectedSlotDraggableItem = null;
                selectedSlot = null;
            }
        }
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

            if (!upgradeFailed && draggableItem.itemGeneric.rarity != Rarity.Legendary && draggableItem.itemGeneric.rarity != Rarity.Mythic)
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
            if (inventoryIndexNumber >= 0)
            {

            }
            else
            {
                // Only now is selectedSlot guaranteed to be non-null
                if (!SlotPlacementRules.IsPlacementAllowed(draggableItem.itemGeneric, slotType, player.activeWeapon.GetCurrentMainHandWeapon(), player.currentWeaponSlotSetIndex))
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
                SwapItems(draggableItem, currentSlotsDraggableItem, player.activeWeapon.GetCurrentMainHandWeapon(), player.activeWeapon.GetCurrentOffHandWeapon());
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

            if (playerMainHand.weaponBelongingToWhichMainHandSet != playerOffHand?.weaponBelongingToWhichOffHandSet)
            {
                targetItemWeaponIfItIs = (Weapon)targetItem.itemGeneric;

                // If it's a weapon, peek into the appropriate offhand slot
                peekedWeaponSetsOffHandWeapon = null;
                if (targetItemWeaponIfItIs != null)
                {
                    int index = targetItemWeaponIfItIs.weaponBelongingToWhichOffHandSet - 1;
                    peekedWeaponSetsOffHandWeapon = player.weaponSlotSetArray[index][1];
                }
            }
        }

    jump:

        if (peekedWeaponSetsOffHandWeapon is Weapon validWeapon)
        {
            if (SlotPlacementRules.IsSwapAllowed(draggableItem, targetItem, draggableItem.itemGeneric, targetItem.itemGeneric, playerMainHand, playerOffHand,
                validWeapon, out itemSwapPos))
            {
                SwapProcess(draggableItem, targetItem, itemSwapPos);
            }
            else
            {
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
            }
        }
        else
        {
            // If it's null or invalid, call IsSwapAllowed with null
            if (SlotPlacementRules.IsSwapAllowed(draggableItem, targetItem, draggableItem.itemGeneric, targetItem.itemGeneric,
                playerMainHand, playerOffHand, null, out itemSwapPos))
            {
                SwapProcess(draggableItem, targetItem, itemSwapPos);
            }
            else
            {
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
            }
        }

        #region Bullshit
        //Weapon draggableItemWeapon = (Weapon)draggableItem.itemGeneric;
        //Weapon targetWeapon = (Weapon)targetItem.itemGeneric;

        //// Move slot's weapon to draggable item's previous slot
        //// Draggable item is on main hand
        //if (draggableItemWeapon.onMainHand)
        //{
        //    if (targetWeapon == null)
        //    {
        //        // Draggable item is two-handed weapon, so swap is canceled

        //        draggableItem.swapCancelled = true;
        //        return;
        //    }
        //    // Slot item is on inventory slot
        //    else if (targetWeapon.onInventorySlot)
        //    {

        //    }
        //    // Slot and draggable items are both main hands
        //    else if (targetWeapon.onMainHand)
        //    {
        //        // Draggable item doesn't have an off-hand weapon
        //        if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
        //        {
        //            // Slot's current set doesn't have an off-hand weapon
        //            if (player.weaponSlotSetArray[targetWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
        //            {
        //                // Both set's off-hand slots are empty so swap is successful
        //                SwapProcess(draggableItem, targetItem, ItemSwapPos.DragMainSlotMain);
        //            }
        //            else // Slot's current set has an off-hand weapon
        //            {
        //                // Draggable item is two-handed weapon
        //                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
        //                {
        //                    // Draggable item is two-handed weapon, so swap is canceled
        //                    draggableItem.swapCancelled = true;
        //                    return;
        //                }
        //                else
        //                {
        //                    // Draggable item is not a two-handed weapon, so swap is successful
        //                    SwapProcess(draggableItem, targetItem, ItemSwapPos.DragMainSlotMain);
        //                }
        //            }
        //        }
        //        // Slot item has an off-hand weapon
        //        else
        //        {
        //            // Slot's current set doesn't have an off-hand weapon
        //            if (player.weaponSlotSetArray[targetWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
        //            {
        //                // Slot's current weapon is a two-handed weapon
        //                if (player.weaponSlotSetArray[targetWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
        //                {
        //                    // Draggable item has an off-hand and slot item is a two-handed weapon, so swap is canceled
        //                    draggableItem.swapCancelled = true;
        //                    return;
        //                }
        //                else
        //                {
        //                    // Draggable item has an off-hand and but slot item is a one-handed weapon, so swap is succesful
        //                    SwapProcess(draggableItem, targetItem, ItemSwapPos.DragMainSlotMain);
        //                }
        //            }
        //            // Slot's current set has an off-hand weapon
        //            else
        //            {
        //                // Both draggable and slot item sets have an off-hand weapon. This means both items are one-handed, so swap is succesfful
        //                SwapProcess(draggableItem, targetItem, ItemSwapPos.DragMainSlotMain);
        //            }
        //        }
        //    }
        //    // Draggable item is at main-hand and slot item is at off-hand
        //    else
        //    {
        //        // Slot item is a shield
        //        if (player.weaponSlotSetArray[targetWeapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
        //        {
        //            // Slot item is a shield, so swap is canceled
        //            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
        //            draggableItem.swapCancelled = true;
        //            return;
        //        }
        //        // Draggable item is a two-handed weapon
        //        else if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
        //        {
        //            // Draggable item is two-handed weapon, so swap is canceled
        //            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandCantBeAddedToTwoHanded);
        //            draggableItem.swapCancelled = true;
        //            return;
        //        }
        //        else
        //        {
        //            // Neither slot item is a shield nor draggable item is a two-handed weapon, so swap is succesfful
        //            SwapProcess(draggableItem, targetItem, ItemSwapPos.DragMainSlotOff);
        //        }
        //    }
        //}
        //// Draggable item is on off-hand
        //else
        //{
        //    // Draggable item is on off-hand and slot item is on main hand
        //    if (targetWeapon.onMainHand)
        //    {
        //        // Draggable item is a shield
        //        if (draggableItemWeapon.weaponDetails.weaponClass == WeaponClass.Shield)
        //        {
        //            // Draggable item is a shield, so swap is canceled
        //            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
        //            draggableItem.swapCancelled = true;
        //            return;
        //        }
        //        // Slot item is a two-handed weapon
        //        else if (player.weaponSlotSetArray[targetWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
        //        {
        //            // Slot item is two-handed weapon, so swap is canceled
        //            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandCantBeAddedToTwoHanded);
        //            draggableItem.swapCancelled = true;
        //            return;
        //        }
        //        else
        //        {
        //            // Neither draggable item is a shield nor slot item is a two-handed weapon, so swap is succesfful
        //            SwapProcess(draggableItem,  targetItem, ItemSwapPos.DragOffSlotMain);
        //        }
        //    }
        //    // Both draggable and slot items are off-hand
        //    else
        //    {
        //        // Both draggable and slot items are off-hand; it means they are either one-handed or a shield, so swap is successful
        //        SwapProcess(draggableItem, targetItem, ItemSwapPos.DragOffSlotOff);
        //    }
        //}
        #endregion
    }

    private bool IsInventorySwap(DraggableItem draggedItem, DraggableItem targetItem) =>
        draggedItem.itemGeneric.itemSlotStatus == ItemSlotStatus.Inventory || targetItem.itemGeneric.itemSlotStatus == ItemSlotStatus.Inventory;

    private void MoveItemToSlot(DraggableItem draggableItem, bool clickTransport = false)
    {
        ItemGeneric draggableItemGeneric = draggableItem.itemGeneric;

        if (draggableItemGeneric.itemSlotStatus == ItemSlotStatus.Inventory) // If weapon moved from inventory slot
        {
            // Remove draggable item from inventory
            InventoryManager.Instance.EmptyItemFromInventory(draggableItem.belongingSlot.inventoryIndexNumber);

            if (draggableItemGeneric is Weapon)
            {
                Weapon draggableInventoryWeapon = (Weapon)draggableItemGeneric;

                if (slotType == SlotType.WeaponMainHand)
                {
                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableInventoryWeapon;
                    draggableInventoryWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    draggableInventoryWeapon.itemSlotStatus = ItemSlotStatus.MainHand;
                    player.mainHandSlotFilled = false;

                    // Activation
                    player.playerControl.SetWeaponSetByIndex(true, false, true);

                    // Book update
                    StaticEventHandler.CallInventoryWeaponDroppedEventForBook(draggableItem.belongingSlot.inventoryIndexNumber);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else if (slotType == SlotType.WeaponOffHand)
                {
                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableInventoryWeapon;
                    draggableInventoryWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    draggableInventoryWeapon.itemSlotStatus = ItemSlotStatus.OffHand;
                    player.offHandSlotFilled = false;

                    // Activation
                    player.playerControl.SetWeaponSetByIndex(true, false, true);

                    // Book update
                    StaticEventHandler.CallInventoryWeaponDroppedEventForBook(draggableItem.belongingSlot.inventoryIndexNumber);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else if (slotType == SlotType.Inventory)
                {
                    InventoryManager.Instance.PlaceItemToInventoryIndexSlot(draggableItem.itemGeneric, placeToLowestIndex: false, inventoryIndexNumber);

                    // Book update
                    StaticEventHandler.CallGenericItemPlacedToEmptyInInventory(draggableItem.itemGeneric, draggableItem.belongingSlot.inventoryIndexNumber, 
                        inventoryIndexNumber, draggableItem.image.sprite);
                }
            }
            // Passive item in the inventory moves to passive item slot
            else if (draggableItemGeneric is PassiveItem)
            {
                if (slotType == SlotType.Inventory)
                {
                    InventoryManager.Instance.PlaceItemToInventoryIndexSlot(draggableItem.itemGeneric, placeToLowestIndex: false, inventoryIndexNumber);

                    // Book update
                    StaticEventHandler.CallGenericItemPlacedToEmptyInInventory(draggableItem.itemGeneric, draggableItem.belongingSlot.inventoryIndexNumber,
                        inventoryIndexNumber, draggableItem.image.sprite);
                }
                else
                {
                    PassiveItem draggableInventoryPassiveItem = (PassiveItem)draggableItemGeneric;

                    // Equip event and update stats
                    player.setPassiveItemEvent.CallEquipPassiveItem(draggableInventoryPassiveItem, draggableInventoryPassiveItem.passiveItemDetails.passiveItemSlotName);
                    draggableInventoryPassiveItem.itemSlotStatus = ItemSlotStatus.None;

                    // Book update for passive slot addition and inventory slot drop
                    StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(draggableItem.belongingSlot.inventoryIndexNumber);
                    StaticEventHandler.CallItemAddedToPassiveItemSlot(draggableInventoryPassiveItem, draggableInventoryPassiveItem.passiveItemDetails.passiveItemSlotName);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }

            return; // This is dragged from inventory so don't go further
        }

        if (draggableItem.itemGeneric is Weapon)
        {
            Weapon draggableItemWeapon = draggableItem.itemGeneric as Weapon;

            if (draggableItemWeapon.itemSlotStatus == ItemSlotStatus.MainHand)
            {
                if (inventoryIndexNumber >= 0 && !InventoryManager.Instance.IsInventoryFull()) // IT MEANS, DRAGGED SLOT IS AN INVENTORY SLOT AND INVENTORY IS NOT FULL
                {
                    // Empty weapon on hand
                    player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;

                    // Place weapon into inventory
                    InventoryManager.Instance.PlaceItemToInventoryIndexSlot(draggableItemWeapon, placeToLowestIndex: false, inventoryIndexNumber);

                    draggableItemWeapon.itemSlotStatus = ItemSlotStatus.Inventory;
                    draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;

                    player.playerControl.SetWeaponSetByIndex(true, false, false, true);

                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(draggableItemWeapon, inventoryIndexNumber);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else if (slotType == SlotType.WeaponMainHand)
                {
                    player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;

                    draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    player.playerControl.SetWeaponSetByIndex(true, false);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else
                {
                    if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] != null)
                    {
                        GameManager.Instance.OpenPopUpLog(PopUpReason.EmptyOffHandFirst);
                        draggableItem.swapCancelled = true;
                        return;
                    }
                    else
                    {
                        if (draggableItemWeapon.weaponBelongingToWhichMainHandSet == player.currentWeaponSlotSetIndex)
                        {
                            GameManager.Instance.OpenPopUpLog(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                            draggableItem.swapCancelled = true;
                            return;
                        }
                    }

                    if (draggableItem.transactionOnTheSameSet)
                    {
                        GameManager.Instance.OpenPopUpLog(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                        draggableItem.swapCancelled = true;
                        return;
                    }

                    player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;
                    player.mainHandSlotFilled = false; // Change flag so this empty slot can be used for future pick-ups

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                    draggableItemWeapon.itemSlotStatus = ItemSlotStatus.OffHand;

                    draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                    draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    draggableItem.dragMainSlotOff = true;
                    player.playerControl.SetWeaponSetByIndex(true, false);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }
            else
            {
                if (inventoryIndexNumber >= 0 && !InventoryManager.Instance.IsInventoryFull())
                {
                    // Empty weapon on off-hand
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = null;

                    // Place weapon into inventory
                    InventoryManager.Instance.PlaceItemToInventoryIndexSlot(draggableItemWeapon, placeToLowestIndex: false, inventoryIndexNumber);

                    draggableItemWeapon.itemSlotStatus = ItemSlotStatus.Inventory;
                    draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;

                    player.playerControl.SetWeaponSetByIndex(true, false, false, true);

                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(draggableItemWeapon, inventoryIndexNumber);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                }
                else if (slotType == SlotType.WeaponMainHand)
                {
                    if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
                    {
                        GameManager.Instance.OpenPopUpLog(PopUpReason.ShieldCantBePutOnMainHand);
                        draggableItem.swapCancelled = true;
                        return;
                    }

                    player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = null;

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                    draggableItemWeapon.itemSlotStatus = ItemSlotStatus.MainHand;

                    draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                    draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                    player.playerControl.SetWeaponSetByIndex(true, false);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
                else
                {
                    if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] == null)
                    {
                        GameManager.Instance.OpenPopUpLog(PopUpReason.EquipMainHandFirst);
                        draggableItem.swapCancelled = true;
                        return;
                    }

                    player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = null;
                    player.offHandSlotFilled = false; // Change flag so this empty slot can be used for future pick-ups

                    // Put draggable item to current slot
                    player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;

                    draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                    player.playerControl.SetWeaponSetByIndex(true, false);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();
                }
            }
        }
        else if (draggableItem.itemGeneric is PassiveItem)
        {
            if (!InventoryManager.Instance.IsInventoryFull())
            {
                // Passive item in the slot moves to inventory slot
                PassiveItem draggableInventoryPassiveItem = (PassiveItem)draggableItemGeneric;

                player.setPassiveItemEvent.CallRemovePassiveItem(draggableInventoryPassiveItem, draggableInventoryPassiveItem.passiveItemDetails.passiveItemSlotName);

                InventoryManager.Instance.PlaceItemToInventoryIndexSlot(draggableInventoryPassiveItem, placeToLowestIndex: false, inventoryIndexNumber);

                // Book update for passive slot inventory addition and passive slot drop
                StaticEventHandler.CallItemRemovedFromPassiveItemSlot(draggableInventoryPassiveItem.passiveItemDetails.passiveItemSlotName);
                StaticEventHandler.CallPassiveItemAddedToInventorySlot(draggableInventoryPassiveItem, inventoryIndexNumber); // This is placed slot's index number

                StaticEventHandler.CallStatsChangedOnTheBookEvent();
            }
        }
    }

    private void SwapProcess(DraggableItem draggableItem, DraggableItem targetItem, ItemSwapPos itemSwapPos)
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
                player.setPassiveItemEvent.CallRemovePassiveItem(draggablePassiveItem, draggablePassiveItem.passiveItemDetails.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(targetPassiveItem, targetPassiveItem.passiveItemDetails.passiveItemSlotName, true);
                draggableItem.itemGeneric.itemSlotStatus = ItemSlotStatus.None;

                // Target item to inventory slot - Stat Update
                player.setPassiveItemEvent.CallRemovePassiveItem(targetPassiveItem, targetPassiveItem.passiveItemDetails.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(draggablePassiveItem, draggablePassiveItem.passiveItemDetails.passiveItemSlotName, true);
                targetItem.itemGeneric.itemSlotStatus = ItemSlotStatus.Inventory;

                // Inventory update
                InventoryManager.Instance.inventoryArray[draggableItem.belongingSlot.inventoryIndexNumber] = targetPassiveItem;

                // Book update
                StaticEventHandler.CallPassiveItemsSwappedEvent(targetPassiveItem, draggablePassiveItem, draggableItem.belongingSlot.inventoryIndexNumber);

                // Swap slots
                SwapBelongingSlots(draggableItem, targetItem);

                break;

            case ItemSwapPos.DragPassiveSlotPassiveInventory:
                // Dragged item to inventory slot - Stat Update
                player.setPassiveItemEvent.CallRemovePassiveItem(targetPassiveItem, targetPassiveItem.passiveItemDetails.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(draggablePassiveItem, draggablePassiveItem.passiveItemDetails.passiveItemSlotName, true);
                draggableItem.itemGeneric.itemSlotStatus = ItemSlotStatus.Inventory;

                // Target item to equipped passive item slot - Stat Update
                player.setPassiveItemEvent.CallRemovePassiveItem(draggablePassiveItem, draggablePassiveItem.passiveItemDetails.passiveItemSlotName, true);
                player.setPassiveItemEvent.CallEquipPassiveItem(targetPassiveItem, targetPassiveItem.passiveItemDetails.passiveItemSlotName, true);
                targetItem.itemGeneric.itemSlotStatus = ItemSlotStatus.None;

                // Inventory update
                InventoryManager.Instance.inventoryArray[targetItem.belongingSlot.inventoryIndexNumber] = draggablePassiveItem;

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
                InventoryManager.Instance.inventoryArray[index] = draggableItemWeapon;

                // Inventory weapon to main hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.Inventory);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.MainHand);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(true, false, false, false, true);

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
                InventoryManager.Instance.inventoryArray[index] = draggableItemWeapon;

                // Inventory weapon to off-hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.Inventory);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.OffHand);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(true, false, false, false, true);

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
                InventoryManager.Instance.inventoryArray[index] = targetWeapon;

                // Inventory weapon to main hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.MainHand);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.Inventory);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(true, false, false, false, true);

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
                InventoryManager.Instance.inventoryArray[index] = targetWeapon;

                // Inventory weapon to main hand slot
                UpdateWeaponSlotStatus(draggableItemWeapon, setIndex, ItemSlotStatus.OffHand);
                UpdateWeaponSlotStatus(targetWeapon, setIndex, ItemSlotStatus.Inventory);

                // Activate weapon changes
                player.playerControl.SetWeaponSetByIndex(true, false, false, false, true);

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
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                targetWeapon.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponBelongingToWhichMainHandSet;
                targetWeapon.itemSlotStatus = ItemSlotStatus.MainHand;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.itemSlotStatus = ItemSlotStatus.MainHand;
                player.playerControl.SetWeaponSetByIndex(true, false);
                StaticEventHandler.CallWeaponSwitchedEventForBook();
                break;
            case ItemSwapPos.DragMainSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                targetWeapon.weaponBelongingToWhichOffHandSet = 0;
                targetWeapon.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponBelongingToWhichMainHandSet;
                targetWeapon.itemSlotStatus = ItemSlotStatus.MainHand;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.itemSlotStatus = ItemSlotStatus.OffHand;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(true, false);
                }
                else
                {
                    StaticEventHandler.CallWeaponSwitchedEventForBook();
                }
                break;
            case ItemSwapPos.DragOffSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                targetWeapon.weaponBelongingToWhichMainHandSet = 0;
                targetWeapon.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponBelongingToWhichOffHandSet;
                targetWeapon.itemSlotStatus = ItemSlotStatus.OffHand;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.itemSlotStatus = ItemSlotStatus.MainHand;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(true, false);
                }
                else
                {
                    StaticEventHandler.CallWeaponSwitchedEventForBook();
                }

                break;
            case ItemSwapPos.DragOffSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = targetWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                targetWeapon.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponBelongingToWhichOffHandSet;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.itemSlotStatus = ItemSlotStatus.OffHand;
                targetWeapon.itemSlotStatus = ItemSlotStatus.OffHand;
                player.playerControl.SetWeaponSetByIndex(true, false);

                break;
            case ItemSwapPos.DragInventorySlotInventory:
                index = draggableItem.belongingSlot.inventoryIndexNumber;

                // Swap generic items into inventory array
                InventoryManager.Instance.inventoryArray[index] = targetItem.itemGeneric;
                InventoryManager.Instance.inventoryArray[inventoryIndexNumber] = draggableItem.itemGeneric;

                // Book update
                StaticEventHandler.CallGenericItemsSwappedInInventory(draggableItem, targetItem, index, inventoryIndexNumber);

                // Swap belonging slots for drag-drop references
                SwapBelongingSlots(draggableItem, targetItem);

                break;
            default:
                break;
        }
    }

    void UpdateWeaponSlotStatus(Weapon weapon, int setIndex, ItemSlotStatus status)
    {
        weapon.itemSlotStatus = status;
        if (status == ItemSlotStatus.MainHand) weapon.weaponBelongingToWhichMainHandSet = setIndex;
        else if (status == ItemSlotStatus.OffHand) weapon.weaponBelongingToWhichOffHandSet = setIndex;
        else
        {
            weapon.weaponBelongingToWhichMainHandSet = 0;
            weapon.weaponBelongingToWhichOffHandSet = 0;
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
            if (draggableItem.itemGeneric is Weapon)
            {
                Weapon weapon = draggableItem.itemGeneric as Weapon;

                bool dropOffhand = draggableItem.belongingSlot.slotType == SlotType.WeaponOffHand ? true : false;

                player.playerControl.DropProcess(DropType.Weapon, weapon, toBeSwappedWeapon: null, dropOffhand, weapon.itemSlotStatus, 
                    draggableItem.belongingSlot.inventoryIndexNumber, true);
            }
            else if (draggableItem.itemGeneric is PassiveItem)
            {
                PassiveItem passiveItem = draggableItem.itemGeneric as PassiveItem;

                player.playerControl.DropProcess(DropType.PassiveItem, passiveItem, toBeSwappedWeapon: null, false, passiveItem.itemSlotStatus, 
                    draggableItem.belongingSlot.inventoryIndexNumber, true);
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

                // Book update
                if (passiveItem.itemSlotStatus == ItemSlotStatus.Inventory)
                {
 
                    StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(draggableItem.belongingSlot.inventoryIndexNumber);
                }
                else
                {
                    StaticEventHandler.CallItemRemovedFromPassiveItemSlot(passiveItem.passiveItemDetails.passiveItemSlotName);
                }
            }
        }

        return dropFailed;
    }

    private bool UpgradeProcess(DraggableItem draggableItem)
    {
        if (draggableItem == null) return false;

        // Only allow upgrading items that are in the inventory
        if (slotType != SlotType.Upgrade || draggableItem.itemGeneric.itemSlotStatus != ItemSlotStatus.Inventory) return false;

        ItemGeneric item = draggableItem.itemGeneric;

        if (item == null) return false;

        // Already maxed?
        if (item.rarity == Rarity.Legendary) return false;

        // Determine shard cost and next rarity
        int shardCost = draggableItem.itemGeneric.rarity switch
        {
            Rarity.Basic => 100, // Uprade cost to enchanted
            Rarity.Enchanted => 350, // Upgrade cost to mythic,
            Rarity.Mythic => int.MaxValue, // Currently max level is mythic; legendary is not accessed
            Rarity.Legendary => int.MaxValue,
            _ => int.MaxValue
        };

        // Enough shards?
        if (draggableItem.itemGeneric.rarity >= Rarity.Mythic)
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

        // Promote rarity
        Rarity next = item.rarity switch
        {
            Rarity.Basic => Rarity.Enchanted,
            Rarity.Enchanted => Rarity.Mythic,
            Rarity.Mythic => Rarity.Legendary,
            _ => item.rarity
        };

        if (next == item.rarity) return false; // nothing to do

        // Mutate in-place by type
        if (item is Weapon upgWeapon)
        {
            // Ensure we have details
            if (upgWeapon.weaponDetails == null) return false;

            upgWeapon.rarity = next;
            upgWeapon.baseTypeRolled = upgWeapon.weaponDetails.baseTypeModifier;
            upgWeapon.baseUniqueRolled = upgWeapon.weaponDetails.baseUniqueModifier;

            // Add new rolls when crossing thresholds
            List<BoostType> pool = upgWeapon.weaponDetails.additionalModifierPoolForType;

            if (pool != null && pool.Count > 0)
            {
                if (next >= Rarity.Enchanted && upgWeapon.enchantedBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType>
                {
                    upgWeapon.weaponDetails.baseUniqueModifier,
                    upgWeapon.weaponDetails.baseTypeModifier

                });
                    upgWeapon.enchantedBoostType = rolled;
                    WeaponDropGenerator.SetWeaponModifier(ref upgWeapon, rolled, upgWeapon.weaponDetails);
                    //ApplyWeaponBoost(upgWeapon, rolled, upgWeapon.weaponDetails);
                }

                if (next >= Rarity.Mythic && upgWeapon.mythicBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> {
                    upgWeapon.weaponDetails.baseUniqueModifier,
                    upgWeapon.weaponDetails.baseTypeModifier,
                    upgWeapon.enchantedBoostType
                });
                    upgWeapon.mythicBoostType = rolled;
                    WeaponDropGenerator.SetWeaponModifier(ref upgWeapon, rolled, upgWeapon.weaponDetails);
                    //ApplyWeaponBoost(upgWeapon, rolled, upgWeapon.weaponDetails);
                }
            }

            StaticEventHandler.CallInventoryWeaponUpgradedEventForBook(upgWeapon, draggableItem.belongingSlot.inventoryIndexNumber);
            return true;
        }
        else if (item is PassiveItem upgPassive)
        {
            if (upgPassive.passiveItemDetails == null) return false;

            upgPassive.rarity = next;
            upgPassive.baseUniqueRolled = upgPassive.passiveItemDetails.baseUniqueModifier;

            List<BoostType> pool = upgPassive.passiveItemDetails.additionalModifierPoolForType;

            if (pool != null && pool.Count > 0)
            {
                if (next >= Rarity.Enchanted && upgPassive.enchantedBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> { upgPassive.passiveItemDetails.baseUniqueModifier });
                    upgPassive.enchantedBoostType = rolled;
                    PassiveDropGenerator.SetPassiveItemModifier(ref upgPassive, rolled, upgPassive.passiveItemDetails);
                    //ApplyPassiveBoost(upgPassive, rolled);
                }

                if (next >= Rarity.Mythic && upgPassive.mythicBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> { upgPassive.passiveItemDetails.baseUniqueModifier, upgPassive.enchantedBoostType });
                    upgPassive.mythicBoostType = rolled;
                    PassiveDropGenerator.SetPassiveItemModifier(ref upgPassive, rolled, upgPassive.passiveItemDetails);
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
            if (slotType == SlotType.Dismantle && draggableItem.itemGeneric.itemSlotStatus == ItemSlotStatus.Inventory) 
            {
                switch (draggableItem.itemGeneric.rarity)
                {
                    case Rarity.Basic: shardGain = 10; break;
                    case Rarity.Enchanted: shardGain = 35; break;
                    case Rarity.Mythic: shardGain = 100; break;
                    case Rarity.Legendary: shardGain = 250; break;
                    default:
                        break;
                }

                player.coinsAndShards.AddShard(shardGain);
                InventoryManager.Instance.EmptyItemFromInventory(draggableItem.belongingSlot.inventoryIndexNumber);

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
                weapon.attackCooldownModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                // Affect PHYSICAL slice
                rng2 = Random.Range(1, 16);
                weapon.physicalAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                weapon.attackRatingIncrease = (float)Math.Round(rng - 1, 2);
                break;
            case BoostType.MagicDamage:
                // Affect MAGIC slice
                rng2 = Random.Range(1, 11);
                weapon.magicAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.criticalHitChanceIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritDamage:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.criticalHitDamageIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = Random.Range(1, 11);
                weapon.lifeStealAmount = Mathf.RoundToInt(rng2);
                break;
            case BoostType.BlockChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.blockChanceIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.dodgeChanceIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = Random.Range(-1f, 1f);
                weapon.increasedMaxHealth = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = Random.Range(-1f, 1f);
                weapon.increasedMaxMana = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.statusResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = Random.Range(1, 11);
                weapon.damageVsLowHealthEnemies = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.criticalResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.armorIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.magicResistance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = Random.Range(0.5f, 2f);
                weapon.speedIncreaseModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = Random.Range(0.05f, 0.15f);
                weapon.damageReductionRate = Mathf.Round(rng2 * 100f) / 100f;
                break;
            case BoostType.ArmorPenetration:
                rng2 = Random.Range(0.05f, 0.15f);
                weapon.armorPenetration = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackRange:
                rng2 = Random.Range(0.05f, 0.30f);
                if (weaponDetails.weaponCurrentProjectile != null)
                    weapon.attackRange = Mathf.Round(weaponDetails.weaponCurrentProjectile.projectileRange * rng2 * 100f) / 100f;
                break;
            case BoostType.SkillCooldown:
                rng2 = Random.Range(0.05f, 0.13f);
                weapon.skillCooldown = (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillDuration:
                rng2 = Random.Range(0.05f, 0.4f);
                weapon.skillDuration = (float)Math.Round(rng2, 2);
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
                passiveItem.attackCooldown = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackDamage:
                // Affect PHYSICAL slice
                rng2 = Random.Range(1, 16);
                passiveItem.physicalAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.AttackRating:
                passiveItem.attackRating = (float)Math.Round(rng - 1, 2);
                break;
            case BoostType.MagicDamage:
                // Affect MAGIC slice
                rng2 = Random.Range(1, 16);
                passiveItem.magicAttackDamageIncrease = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.criticalHitChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.LifeSteal:
                rng2 = Random.Range(1, 8);
                passiveItem.lifeStealAmount = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritDamage:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.criticalHitDamage = (float)Math.Round(rng2, 2);
                break;
            case BoostType.BlockChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.blockChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DodgeChance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.dodgeChance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.HealthIncrease:
                rng2 = Random.Range(-1f, 1f);
                passiveItem.increasedMaxHealth = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.ManaIncrease:
                rng2 = Random.Range(-1f, 1f);
                passiveItem.increasedMaxMana = Mathf.RoundToInt(100 * rng * (1 + rng2));
                break;
            case BoostType.StatusResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.statusResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.AttackVsLowHealthEnemies:
                rng2 = Random.Range(1, 8);
                passiveItem.damageVsLowHealthEnemies = Mathf.RoundToInt(rng2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.criticalResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.armorIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.magicResistanceModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MoveSpeed:
                rng2 = Random.Range(0.5f, 2f);
                passiveItem.speedIncreaseModifier = (float)Math.Round(rng2, 2);
                break;
            case BoostType.DamageReduction:
                rng2 = Random.Range(0.05f, 0.15f);
                passiveItem.damageReductionRate = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorPenetration:
                rng2 = Random.Range(0.05f, 0.15f);
                passiveItem.armorPenetration = (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillCooldown:
                rng2 = Random.Range(0.05f, 0.13f);
                passiveItem.skillCooldown = (float)Math.Round(rng2, 2);
                break;
            case BoostType.SkillDuration:
                rng2 = Random.Range(0.05f, 0.4f);
                passiveItem.skillDuration = (float)Math.Round(rng2, 2);
                break;
            default: 
                break;
        }
    }

    private void BoostTypeColorUpdate(Weapon weapon)
    {
        switch (weapon.rarity)
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
        switch (passiveItem.rarity)
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
                        bonusText.text = $"Attack Speed: + {weapon.attackCooldownModifier * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text = "Phy. Attack Dmg.: + " + weapon.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text = $"Attack Rating: + {weapon.attackRatingIncrease * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text = "Magic Attack Dmg.: + " + weapon.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text = $"Cr. Hit Chance: + {weapon.criticalHitChanceIncrease * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text = $"Cr. Hit Damage: + {weapon.criticalHitDamageIncrease * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text = $"Life Steal: + {weapon.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text = $"Block Chance: + {weapon.blockChanceIncrease * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text = $"Dodge Chance: + {weapon.dodgeChanceIncrease * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text = $"Health: + {weapon.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text = $"Mana: + {weapon.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text = $"Status Resistance: + {weapon.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text = $"Damage vs Low Health: + {weapon.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text = $"Cr. Resistance: + {weapon.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text = $"Armor: + {weapon.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text = $"Magic Resistance: + {weapon.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text = $"Move Speed: + {weapon.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text = $"Damage Reduction: + {weapon.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text = $"Armor Penetration: + {weapon.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text = $"Skill Cooldown: + {weapon.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text = $"Skill Duration: + {weapon.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (weapon.additionalPoisonChance > 0) bonusText.text = $"Poison Chance: + {weapon.additionalPoisonChance * 100}%";
                        else if (weapon.additionalBleedChance > 0) bonusText.text = $"Bleed Chance: + {weapon.additionalBleedChance * 100}%";
                        else if (weapon.additionalRootChance > 0) bonusText.text = $"Root Chance: + {weapon.additionalRootChance * 100}%";
                        else if (weapon.additionalStunChance > 0) bonusText.text = $"Stun Chance: + {weapon.additionalStunChance * 100}%";
                        else if (weapon.additionalCurseChance > 0) bonusText.text = $"Curse Chance: + {weapon.additionalCurseChance * 100}%";
                        else if (weapon.additionalFearChance > 0) bonusText.text = $"Fear Chance: + {weapon.additionalFearChance * 100}%";
                        else if (weapon.additionalRevealChance > 0) bonusText.text = $"Reveal Chance: + {weapon.additionalRevealChance * 100}%";
                        else if (weapon.additionalParalyzeChance > 0) bonusText.text = $"Paralyze Chance: + {weapon.additionalParalyzeChance * 100}%";
                        else if (weapon.additionalBurnChance > 0) bonusText.text = $"Burn Chance: + {weapon.additionalBurnChance * 100}%";
                        else if (weapon.additionalFreezeChance > 0) bonusText.text = $"Freeze Chance: + {weapon.additionalFreezeChance * 100}%";
                        else if (weapon.additionalBlindChance > 0) bonusText.text = $"Blind Chance: + {weapon.additionalBlindChance * 100}%";
                        else if (weapon.additionalSlowChance > 0) bonusText.text = $"Slow Chance: + {weapon.additionalSlowChance * 100}%";
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
                        bonusText.text += $"\nAttack Speed: + {weapon.attackCooldownModifier * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text += "\nPhy. Attack Dmg.: + " + weapon.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text += $"\nAttack Rating: + {weapon.attackRatingIncrease * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text += "\nMagic Attack Dmg.: + " + weapon.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text += $"\nCr. Hit Chance: + {weapon.criticalHitChanceIncrease * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text += $"\nCr. Hit Damage: + {weapon.criticalHitDamageIncrease * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text += $"\nLife Steal: + {weapon.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text += $"\nBlock Chance: + {weapon.blockChanceIncrease * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text += $"\nDodge Chance: + {weapon.dodgeChanceIncrease * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text += $"\nHealth: + {weapon.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text += $"\nMana: + {weapon.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text += $"\nStatus Resistance: + {weapon.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text += $"\nDamage vs Low Health: + {weapon.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text += $"\nCr. Resistance: + {weapon.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text += $"\nArmor: + {weapon.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text += $"\nMagic Resistance: + {weapon.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text += $"\nMove Speed: + {weapon.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text += $"\nDamage Reduction: + {weapon.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text += $"\nArmor Penetration: + {weapon.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text += $"\nSkill Cooldown: + {weapon.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text += $"\nSkill Duration: + {weapon.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (weapon.additionalPoisonChance > 0) bonusText.text += $"\nPoison Chance: + {weapon.additionalPoisonChance * 100}%";
                        else if (weapon.additionalBleedChance > 0) bonusText.text += $"\nBleed Chance: + {weapon.additionalBleedChance * 100}%";
                        else if (weapon.additionalRootChance > 0) bonusText.text += $"\nRoot Chance: + {weapon.additionalRootChance * 100}%";
                        else if (weapon.additionalStunChance > 0) bonusText.text += $"\nStun Chance: + {weapon.additionalStunChance * 100}%";
                        else if (weapon.additionalCurseChance > 0) bonusText.text += $"\nCurse Chance: + {weapon.additionalCurseChance * 100}%";
                        else if (weapon.additionalFearChance > 0) bonusText.text += $"\nFear Chance: + {weapon.additionalFearChance * 100}%";
                        else if (weapon.additionalRevealChance > 0) bonusText.text += $"\nReveal Chance: + {weapon.additionalRevealChance * 100}%";
                        else if (weapon.additionalParalyzeChance > 0) bonusText.text += $"\nParalyze Chance: + {weapon.additionalParalyzeChance * 100}%";
                        else if (weapon.additionalBurnChance > 0) bonusText.text += $"\nBurn Chance: + {weapon.additionalBurnChance * 100}%";
                        else if (weapon.additionalFreezeChance > 0) bonusText.text += $"\nFreeze Chance: + {weapon.additionalFreezeChance * 100}%";
                        else if (weapon.additionalBlindChance > 0) bonusText.text += $"\nBlind Chance: + {weapon.additionalBlindChance * 100}%";
                        else if (weapon.additionalSlowChance > 0) bonusText.text += $"\nSlow Chance: + {weapon.additionalSlowChance * 100}%";
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
                        bonusText.text = $"Attack Speed: + {passiveItem.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text = "Phy. Attack Dmg.: + " + passiveItem.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text = $"Attack Rating: + {passiveItem.attackRating * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text = "Magic Attack Dmg.: + " + passiveItem.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text = $"Cr. Hit Chance: + {passiveItem.criticalHitChance * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text = $"Cr. Hit Damage: + {passiveItem.criticalHitDamage * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text = $"Life Steal: + {passiveItem.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text = $"Block Chance: + {passiveItem.blockChance * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text = $"Dodge Chance: + {passiveItem.dodgeChance * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text = $"Health: + {passiveItem.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text = $"Mana: + {passiveItem.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text = $"Status Resistance: + {passiveItem.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text = $"Damage vs Low Health: + {passiveItem.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text = $"Cr. Resistance: + {passiveItem.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text = $"Armor: + {passiveItem.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text = $"Magic Resistance: + {passiveItem.magicResistanceModifier * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text = $"Move Speed: + {passiveItem.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text = $"Damage Reduction: + {passiveItem.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text = $"Armor Penetration: + {passiveItem.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text += $"\nSkill Cooldown: + {passiveItem.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text += $"\nSkill Duration: + {passiveItem.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (passiveItem.additionalPoisonChance > 0) bonusText.text = $"Poison Chance: + {passiveItem.additionalPoisonChance * 100}%";
                        else if (passiveItem.additionalBleedChance > 0) bonusText.text = $"Bleed Chance: + {passiveItem.additionalBleedChance * 100}%";
                        else if (passiveItem.additionalRootChance > 0) bonusText.text = $"Root Chance: + {passiveItem.additionalRootChance * 100}%";
                        else if (passiveItem.additionalStunChance > 0) bonusText.text = $"Stun Chance: + {passiveItem.additionalStunChance * 100}%";
                        else if (passiveItem.additionalCurseChance > 0) bonusText.text = $"Curse Chance: + {passiveItem.additionalCurseChance * 100}%";
                        else if (passiveItem.additionalFearChance > 0) bonusText.text = $"Fear Chance: + {passiveItem.additionalFearChance * 100}%";
                        else if (passiveItem.additionalRevealChance > 0) bonusText.text = $"Reveal Chance: + {passiveItem.additionalRevealChance * 100}%";
                        else if (passiveItem.additionalParalyzeChance > 0) bonusText.text = $"Paralyze Chance: + {passiveItem.additionalParalyzeChance * 100}%";
                        else if (passiveItem.additionalBurnChance > 0) bonusText.text = $"Burn Chance: + {passiveItem.additionalBurnChance * 100}%";
                        else if (passiveItem.additionalFreezeChance > 0) bonusText.text = $"Freeze Chance: + {passiveItem.additionalFreezeChance * 100}%";
                        else if (passiveItem.additionalBlindChance > 0) bonusText.text = $"Blind Chance: + {passiveItem.additionalBlindChance * 100}%";
                        else if (passiveItem.additionalSlowChance > 0) bonusText.text = $"Slow Chance: + {passiveItem.additionalSlowChance * 100}%";
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
                        bonusText.text += $"\nAttack Speed: + {passiveItem.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text += "\nPhy. Attack Dmg.: + " + passiveItem.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text += $"\nAttack Rating: + {passiveItem.attackRating * 100}";
                        break;
                    case BoostType.MagicDamage:
                        bonusText.text += "\nMagic Attack Dmg.: + " + passiveItem.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        bonusText.text += $"\nCr. Hit Chance: + {passiveItem.criticalHitChance * 100}%";
                        break;
                    case BoostType.CritDamage:
                        bonusText.text += $"\nCr. Hit Damage: + {passiveItem.criticalHitDamage * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        bonusText.text += $"\nLife Steal: + {passiveItem.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        bonusText.text += $"\nBlock Chance: + {passiveItem.blockChance * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        bonusText.text += $"\nDodge Chance: + {passiveItem.dodgeChance * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        bonusText.text += $"\nHealth: + {passiveItem.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        bonusText.text += $"\nMana: + {passiveItem.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        bonusText.text += $"\nStatus Resistance: + {passiveItem.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        bonusText.text += $"\nDamage vs Low Health: + {passiveItem.damageVsLowHealthEnemies}";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text += $"\nCr. Resistance: + {passiveItem.criticalResistanceModifier * 100}%";
                        break;
                    case BoostType.ArmorIncrease:
                        bonusText.text += $"\nArmor: + {passiveItem.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        bonusText.text += $"\nMagic Resistance: + {passiveItem.magicResistanceModifier * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        bonusText.text += $"\nMove Speed: + {passiveItem.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        bonusText.text += $"\nDamage Reduction: + {passiveItem.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        bonusText.text += $"\nArmor Penetration: + {passiveItem.armorPenetration * 100}%";
                        break;
                    case BoostType.SkillCooldown:
                        bonusText.text += $"\nSkill Cooldown: + {passiveItem.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text += $"\nSkill Duration: + {passiveItem.skillDuration * 100}%";
                        break;
                    case BoostType.StatusInflict:
                        if (passiveItem.additionalPoisonChance > 0) bonusText.text += $"\nPoison Chance: + {passiveItem.additionalPoisonChance * 100}%";
                        else if (passiveItem.additionalBleedChance > 0) bonusText.text += $"\nBleed Chance: + {passiveItem.additionalBleedChance * 100}%";
                        else if (passiveItem.additionalRootChance > 0) bonusText.text += $"\nRoot Chance: + {passiveItem.additionalRootChance * 100}%";
                        else if (passiveItem.additionalStunChance > 0) bonusText.text += $"\nStun Chance: + {passiveItem.additionalStunChance * 100}%";
                        else if (passiveItem.additionalCurseChance > 0) bonusText.text += $"\nCurse Chance: + {passiveItem.additionalCurseChance * 100}%";
                        else if (passiveItem.additionalFearChance > 0) bonusText.text += $"\nFear Chance: + {passiveItem.additionalFearChance * 100}%";
                        else if (passiveItem.additionalRevealChance > 0) bonusText.text += $"\nReveal Chance: + {passiveItem.additionalRevealChance * 100}%";
                        else if (passiveItem.additionalParalyzeChance > 0) bonusText.text += $"\nParalyze Chance: + {passiveItem.additionalParalyzeChance * 100}%";
                        else if (passiveItem.additionalBurnChance > 0) bonusText.text += $"\nBurn Chance: + {passiveItem.additionalBurnChance * 100}%";
                        else if (passiveItem.additionalFreezeChance > 0) bonusText.text += $"\nFreeze Chance: + {passiveItem.additionalFreezeChance * 100}%";
                        else if (passiveItem.additionalBlindChance > 0) bonusText.text += $"\nBlind Chance: + {passiveItem.additionalBlindChance * 100}%";
                        else if (passiveItem.additionalSlowChance > 0) bonusText.text += $"\nSlow Chance: + {passiveItem.additionalSlowChance * 100}%";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}