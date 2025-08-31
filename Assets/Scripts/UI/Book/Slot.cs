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
    public RectTransform tooltipPanel;
    public int inventoryIndexNumber;

    Player player;

    // Tooltip Panel Weapon Texts
    [Header("TOOLTIP PANEL FOR WEAPONS")]
    [Space(10)]
    [SerializeField] TMP_Text headerText;
    [SerializeField] TMP_Text levelText;
    [SerializeField] TMP_Text weaponClassText;
    [SerializeField] TMP_Text hitSpeedText;
    [SerializeField] TMP_Text weaponWieldText;
    [SerializeField] TMP_Text physicalDamageText;
    [SerializeField] TMP_Text magicDamageText;
    [SerializeField] TMP_Text attackRatingText;
    [SerializeField] TMP_Text crHitChanceText;
    [SerializeField] TMP_Text crHitDamageText;
    [SerializeField] TMP_Text enchantedBoostText;

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

    // Weapon Level 
    [Header("ELEMENTAL COLORS")]
    [Space(10)]
    Color noneElementalColor1 = new Color(1, 1, 1);
    Color noneElementalColor2 = new Color(1, 1, 1);
    Color fireColor1 = new Color(0.9686275f, 1, 0.2980392f);
    Color fireColor2 = new Color(1f, 0.1921569f, 0.2431373f);
    Color waterColor1 = new Color(0.8431373f, 0.9647059f, 1);
    Color waterColor2 = new Color(0, 0.5882353f, 1);
    Color airColor1 = new Color(1, 1, 1);
    Color airColor2 = new Color(0.4235294f, 0.4235294f, 0.4235294f);
    Color earthColor1 = new Color(0.5647059f, 1f, 0.2509804f);
    Color earthColor2 = new Color(0.02352941f, 0.4352941f, 0.03529412f);
    Color lightColor1 = new Color(1, 1, 1);
    Color lightColor2 = new Color(0.9716981f, 0.8067644f, 0f);
    Color darkColor1 = new Color(0.627451f, 0, 1);
    Color darkColor2 = new Color(0.6784314f, 0.01568628f, 0.5607843f);

    private void OnEnable()
    {
        player = GameManager.Instance.GetPlayer();

        if (slotType != SlotType.Drop && slotType != SlotType.Upgrade && slotType != SlotType.Dissamble)
        {
            equippedTransform = transform.GetChild(1);

            tooltipPanel.transform.localPosition = new Vector3(60f, 20f, 0f);
            UpdateTooltipPanelInfo();
        }
    }

    /// <summary>
    /// Open tooltip panel when hovering over the related item or weapon
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (equippedTransform != null && equippedTransform.childCount > 0)
        {
            DraggableItem draggableItem = equippedTransform.GetChild(0).GetComponent<DraggableItem>();

            if (draggableItem != null && !tooltipPanel.gameObject.activeSelf)
            {
                tooltipPanel.gameObject.SetActive(true);
                UpdateTooltipPanelInfo();
            }
        }
    }

    /// <summary>
    /// Close tooltip panel when stop hovering over the related item or weapon
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        if (equippedTransform != null && tooltipPanel.gameObject.activeSelf)
        {
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    public void OnSelect(BaseEventData eventData)
    {
        ShowTooltip();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        HideTooltip();
    }

    private void ShowTooltip()
    {
        if (tooltipPanel == null) return;

        if (slotType != SlotType.Drop) return;

        // Hide previous one
        if (currentOpenTooltip != null && currentOpenTooltip != tooltipPanel)
        currentOpenTooltip.SetActive(false);

        tooltipPanel.gameObject.SetActive(true);
        UpdateTooltipPanelInfo(); // This should be your own method
        currentOpenTooltip = tooltipPanel.gameObject;
    }

    private void HideTooltip()
    {
        if (tooltipPanel != null && tooltipPanel.gameObject.activeSelf)
        {
            tooltipPanel.gameObject.SetActive(false);

            if (currentOpenTooltip == tooltipPanel) currentOpenTooltip = null;
        }
    }

    public void UpdateTooltipPanelInfo()
    {
        Transform currentChild = null;

        if (slotType == SlotType.Passive) 
        {
            headerText.text = string.Empty;
            levelText.text = string.Empty;
            weaponClassText.text = string.Empty;
            hitSpeedText.text = string.Empty;
            weaponWieldText.text = string.Empty;
            physicalDamageText.text = string.Empty;
            magicDamageText.text = string.Empty;
            attackRatingText.text = string.Empty;
            crHitChanceText.text = string.Empty;
            crHitDamageText.text = string.Empty;
            enchantedBoostText.text = string.Empty;

            if(inventoryIndexNumber >= 0) // Inventory slot check
            {
                for (int i = 0; i < InventoryManager.Instance.inventoryArray.Length; i++)
                {
                    if (inventoryIndexNumber == i && InventoryManager.Instance.inventoryArray[i] == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                }
            }
            else
            {
                if (player.equippedPassiveItems != null &&
                    player.equippedPassiveItems.TryGetValue(passiveItemSlotName, out var passiveItem))
                {
                    if (passiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                    }
                }
                else
                {
                    // Key doesn't exist, treat as empty slot (safe fail)
                    tooltipPanel.gameObject.SetActive(false);
                }
            }

            // Check if the slot is occupied
            if (equippedTransform.childCount > 0)
            {
                currentChild = equippedTransform.GetChild(0);

                // Retrieve draggable item and weapon from current child
                DraggableItem slotDragggableItem = currentChild?.GetComponent<DraggableItem>();
                PassiveItem passiveItem = slotDragggableItem?.GetDraggedPassiveItem();

                if (passiveItem != null)
                {
                    // Populate text field based on the related weapon info
                    BoostTypeColorUpdate(passiveItem);

                    headerText.text = passiveItem.passiveItemDetails.passiveItemName;
                    levelText.text = $"(Passive Item)";

                    // HEAD
                    // NECK
                    if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
                    {
                        weaponClassText.text = "+20% Fire Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                    {
                        weaponClassText.text = "+20% Earth Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                    {
                        weaponClassText.text = "+20% Air Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                    {
                        weaponClassText.text = "+20% Water Resistance";
                    }
                    // CHEST
                    // FINGER
                    // BACK
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                    {
                        weaponClassText.text = "+5% Cr. Hit Chance";
                        hitSpeedText.text = "+10% Cr. Hit Chance When";
                        weaponWieldText.text = "Dual-Wield Dagger or";
                        physicalDamageText.text = "Claw Equipped";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                    {
                        weaponClassText.text = "+5% Elemental Damage";
                        hitSpeedText.text = "+15% Elemental Resistance";
                    }
                    // ARM
                    // LEG

                    switch (passiveItem.rarity)
                    {
                        case Rarity.Basic:
                            enchantedBoostText.gameObject.SetActive(false);
                            BoostForPassiveItem(passiveItem, passiveItem.baseUniqueRolled, BoostPhase.Unique);
                            BoostForPassiveItem(passiveItem, passiveItem.baseTypeRolled, BoostPhase.Type);
                            break;
                        case Rarity.Enchanted:
                            enchantedBoostText.gameObject.SetActive(true);
                            BoostForPassiveItem(passiveItem, passiveItem.baseUniqueRolled, BoostPhase.Unique);
                            BoostForPassiveItem(passiveItem, passiveItem.baseTypeRolled, BoostPhase.Type);
                            BoostForPassiveItem(passiveItem, passiveItem.enchantedBoostType, BoostPhase.Enchanted);
                            break;
                        case Rarity.Mythic:
                            enchantedBoostText.gameObject.SetActive(true);
                            BoostForPassiveItem(passiveItem, passiveItem.baseUniqueRolled, BoostPhase.Unique);
                            BoostForPassiveItem(passiveItem, passiveItem.baseTypeRolled, BoostPhase.Type);
                            BoostForPassiveItem(passiveItem, passiveItem.enchantedBoostType, BoostPhase.Enchanted);
                            BoostForPassiveItem(passiveItem, passiveItem.mythicBoostType, BoostPhase.Mythic);
                            break;
                        case Rarity.Legendary:
                            enchantedBoostText.gameObject.SetActive(true);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        else if (slotType == SlotType.WeaponMainHand || slotType == SlotType.WeaponOffHand)
        {
            if (inventoryIndexNumber >= 0) // Inventory slot check
            {
                for (int i = 0; i < InventoryManager.Instance.inventoryArray.Length; i++)
                {
                    if (inventoryIndexNumber == i && InventoryManager.Instance.inventoryArray[i] == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                }
            }
            else if (slotType == SlotType.WeaponMainHand)
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon() == null)
                {
                    tooltipPanel.gameObject.SetActive(false);
                    return;
                }
            }
            else if (slotType == SlotType.WeaponOffHand)
            {
                // Cancel tooltip panel transaction if hovered image is lock image at off-hand
                if (player.activeWeapon.GetCurrentMainHandWeapon()?.weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    tooltipPanel.gameObject.SetActive(false);
                    return;
                }

                if (player.activeWeapon.GetCurrentOffHandWeapon() == null)
                {
                    tooltipPanel.gameObject.SetActive(false);
                    return;
                }
            }

            // Check if the slot is occupied
            if (equippedTransform.childCount > 0)
            {
                currentChild = equippedTransform.GetChild(0);

                // Retrieve draggable item and weapon from current child
                DraggableItem slotDragggableItem = currentChild.GetComponent<DraggableItem>();
                Weapon weapon = slotDragggableItem.GetDraggedWeapon();

                if (weapon != null)
                {
                    // Populate text field based on the related weapon info
                    BoostTypeColorUpdate(weapon);

                    headerText.text = weapon.weaponDetails.weaponName;
                    levelText.text = $"({weapon.rarity.ToString()})";
                    weaponClassText.text = $"Class: {weapon.weaponDetails.weaponClass.ToString()}";

                    if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                    {
                        weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";
                        physicalDamageText.text = $"Block Rate: {weapon.blockChanceIncrease * 100}%";
                    }
                    else
                    {
                        hitSpeedText.text = $"Speed: {weapon.attackCooldown.ToString()}";
                        weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";

                        physicalDamageText.text = $"Phy. Damage: {weapon.weaponDetails.physicalDamageMin + weapon.physicalAttackDamageIncrease}-" +
                            $"{weapon.weaponDetails.physicalDamageMax + weapon.physicalAttackDamageIncrease}";

                        magicDamageText.text = $"Magic Damage: {weapon.weaponDetails.magicDamageMin + weapon.magicAttackDamageIncrease}-" +
                            $"{weapon.weaponDetails.magicDamageMax + weapon.magicAttackDamageIncrease}";
                    }

                    attackRatingText.text = $"Base Attack Rating: {weapon.attackRatingIncrease * 100}%";
                    crHitChanceText.text = $"Base Cr. Hit Chance: {weapon.criticalHitChanceIncrease * 100}%";
                    crHitDamageText.text = $"Base Cr. Hit Damage: {weapon.criticalHitDamageIncrease * 100}%";

                    switch (weapon.rarity)
                    {
                        case Rarity.Basic:
                            enchantedBoostText.gameObject.SetActive(false);
                            BoostForWeapon(weapon, weapon.baseUniqueRolled, BoostPhase.Unique);
                            BoostForWeapon(weapon, weapon.baseTypeRolled, BoostPhase.Type);
                            break;
                        case Rarity.Enchanted:
                            enchantedBoostText.gameObject.SetActive(true);

                            enchantedBoostText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                            BoostForWeapon(weapon, weapon.baseUniqueRolled, BoostPhase.Unique);
                            BoostForWeapon(weapon, weapon.baseTypeRolled, BoostPhase.Type);
                            BoostForWeapon(weapon, weapon.enchantedBoostType, BoostPhase.Enchanted);
                            break;
                        case Rarity.Mythic:
                            enchantedBoostText.gameObject.SetActive(true);

                            enchantedBoostText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                            BoostForWeapon(weapon, weapon.baseUniqueRolled, BoostPhase.Unique);
                            BoostForWeapon(weapon, weapon.baseTypeRolled, BoostPhase.Type);
                            BoostForWeapon(weapon, weapon.enchantedBoostType, BoostPhase.Enchanted);
                            BoostForWeapon(weapon, weapon.mythicBoostType, BoostPhase.Mythic);
                            break;
                        case Rarity.Legendary:
                            enchantedBoostText.gameObject.SetActive(true);
                            break;
                        default:
                            break;
                    }
                }
            }
        }
        else
        {
            // Close tooltip bar for empty inventory slots
            tooltipPanel.gameObject.SetActive(false);
        }
    }

    public void OnClick()
    {
        if (selectedSlot == null)
        {
            if (equippedTransform.childCount > 0)
            {
                DraggableItem draggableItem = equippedTransform.GetChild(0).GetComponent<DraggableItem>();

                if (slotType == SlotType.WeaponMainHand || slotType == SlotType.WeaponOffHand || slotType == SlotType.Passive || slotType == SlotType.Active)
                {
                    selectedSlot = this;
                    selectedSlot.selectedSlotDraggableItem = draggableItem;
                }
            }
        }
        else if (selectedSlot == this)
        {
            if (slotType != SlotType.Drop) tooltipPanel.gameObject.SetActive(false); // Close tooltip

            // Clicked same slot again - deselect
            tooltipPanel.gameObject.SetActive(false); // Ensure this runs unconditionally
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
                MoveItemToSlot(selectedSlot.selectedSlotDraggableItem, true);

                // BookUIWrapper
                BookUIRefreshHelper.RefreshBookUIAfterItemPlacement(selectedSlot.selectedSlotDraggableItem.itemGeneric, selectedSlot, this);

                // Clear old visual
                if (selectedSlot.equippedTransform.childCount > 0)
                {
                    Transform oldChild = selectedSlot.equippedTransform.GetChild(0);
                    Destroy(oldChild.gameObject);
                }
            }

            tooltipPanel.gameObject.SetActive(false);
            selectedSlot.selectedSlotDraggableItem = null;
            selectedSlot = null;
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
        else if (slotType == SlotType.Dissamble)
        {
            // If this was a click-based drop (not drag), ignore
            if (selectedSlot != null && eventData.pointerDrag == null) return;

            bool dissambleFailed = false;

            if (eventData.pointerDrag != null)
            {
                draggableItem = eventData.pointerDrag?.GetComponentInParent<DraggableItem>() ?? eventData.pointerDrag?.GetComponent<DraggableItem>();

                dissambleFailed = DissambleProcess(draggableItem);
            }

            return;
        }
        else if (slotType == SlotType.Upgrade)
        {
            // If this was a click-based drop (not drag), ignore
            if (selectedSlot != null && eventData.pointerDrag == null) return;

            bool upgradeFailed = false;

            draggableItem = eventData.pointerDrag?.GetComponentInParent<DraggableItem>() ?? eventData.pointerDrag?.GetComponent<DraggableItem>();

            upgradeFailed = UpgradeProcess(draggableItem);

            if (!upgradeFailed && draggableItem.itemGeneric.rarity != Rarity.Legendary && draggableItem.itemGeneric.rarity != Rarity.Mythic)
            {
                // Clean up
                selectedSlot.selectedSlotDraggableItem = null;
                selectedSlot = null;
            }

            return;
        }

       draggableItem = draggedItem.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            // Only now is selectedSlot guaranteed to be non-null
            if (!SlotPlacementRules.IsPlacementAllowed(draggableItem.itemGeneric, slotType, player.activeWeapon.GetCurrentMainHandWeapon(), player.currentWeaponSlotSetIndex))
            {
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.invalidActionSoundEffect);
                return;
            }

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
        if (targetItem.isLockIcon) return;

        ItemSwapPos itemSwapPos = ItemSwapPos.None;

        // Safe cast - returns null if itemGeneric is not a Weapon
        Weapon targetItemWeaponIfItIs = null;
        Weapon peekedWeaponSetsOffHandWeapon = null;

        if (!IsInventorySwap(draggableItem, targetItem))
        {
            if (playerMainHand.weaponBelongingToWhichMainHandSet != playerOffHand?.weaponBelongingToWhichOffHandSet)
            {
                targetItemWeaponIfItIs = targetItem.itemGeneric as Weapon;

                // If it's a weapon, peek into the appropriate offhand slot
                peekedWeaponSetsOffHandWeapon = null;
                if (targetItemWeaponIfItIs != null)
                {
                    int index = targetItemWeaponIfItIs.weaponBelongingToWhichOffHandSet - 1;
                    peekedWeaponSetsOffHandWeapon = player.weaponSlotSetArray[index][1];
                }
            }
        }

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
            }
            // Passive item in the inventory moves to passive item slot
            else if (draggableItemGeneric is PassiveItem)
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

            return; // This is dragged from inventory so don't go further
        }

        if (draggableItem.itemGeneric is Weapon)
        {
            Weapon draggableItemWeapon = draggableItem.itemGeneric as Weapon;

            if (draggableItemWeapon.itemSlotStatus == ItemSlotStatus.MainHand)
            {
                if (inventoryIndexNumber >= 0 && !InventoryManager.Instance.IsInventoryFull())
                {
                    // Empty weapon on hand
                    player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;

                    // Place weapon into inventory
                    int inventoryIndex = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(draggableItemWeapon);

                    draggableItemWeapon.itemSlotStatus = ItemSlotStatus.Inventory;
                    draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;

                    player.playerControl.SetWeaponSetByIndex(true, false, false, true);

                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(draggableItemWeapon, inventoryIndex);

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
                        GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EmptyOffHandFirst);
                        draggableItem.swapCancelled = true;
                        return;
                    }
                    else
                    {
                        if (draggableItemWeapon.weaponBelongingToWhichMainHandSet == player.currentWeaponSlotSetIndex)
                        {
                            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                            draggableItem.swapCancelled = true;
                            return;
                        }
                    }

                    if (draggableItem.transactionOnTheSameSet)
                    {
                        GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
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
                    int inventoryIndex = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(draggableItemWeapon);

                    draggableItemWeapon.itemSlotStatus = ItemSlotStatus.Inventory;
                    draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;

                    player.playerControl.SetWeaponSetByIndex(true, false, false, true);

                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(draggableItemWeapon, inventoryIndex);

                    StaticEventHandler.CallStatsChangedOnTheBookEvent();

                }
                else if (slotType == SlotType.WeaponMainHand)
                {
                    if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
                    {
                        GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
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
                        GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EquipMainHandFirst);
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
                draggableInventoryPassiveItem.itemSlotStatus = ItemSlotStatus.Inventory;

                int inventoryItemIndex = InventoryManager.Instance.FindIndexOfItem(draggableInventoryPassiveItem);

                // Book update for passive slot inventory addition and passive slot drop
                StaticEventHandler.CallItemRemovedFromPassiveItemSlot(draggableInventoryPassiveItem.passiveItemDetails.passiveItemSlotName);
                StaticEventHandler.CallPassiveItemAddedToInventorySlot(draggableInventoryPassiveItem, inventoryItemIndex);

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

        EquipResult equipResult = new EquipResult();
        Slot temporarySlot = new Slot();
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

                if (draggablePassiveItem == null || targetPassiveItem == null)
                {
                    Debug.LogWarning("SwapProcess aborted: One or both passive items were null.");
                    return;
                }
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

                if (draggablePassiveItem == null || targetPassiveItem == null)
                {
                    Debug.LogWarning("SwapProcess aborted: One or both passive items were null.");
                    return;
                }
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

                if (draggableItemWeapon == null || targetWeapon == null)
                {
                    Debug.LogWarning("SwapProcess aborted: One or both weapons were null.");
                    return;
                }

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

                if (draggableItemWeapon == null || targetWeapon == null)
                {
                    Debug.LogWarning("SwapProcess aborted: One or both weapons were null.");
                    return;
                }
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

                if (draggableItemWeapon == null || targetWeapon == null)
                {
                    Debug.LogWarning("SwapProcess aborted: One or both weapons were null.");
                    return;
                }
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

    public void DissambleSelectedItem()
    {
        if (selectedSlot != null && selectedSlot.selectedSlotDraggableItem != null)
        {
            bool dissambleFailed = DissambleProcess(selectedSlot.selectedSlotDraggableItem);

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
        if (player.coinsAndShards.GetCurrentShard() < shardCost) return false;

        // Try the upgrade
        bool success = UpgradeItem(draggableItem, inventoryIndexNumber);

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
                    ApplyWeaponBoost(upgWeapon, rolled, upgWeapon.weaponDetails);
                }

                if (next >= Rarity.Mythic && upgWeapon.mythicBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> {
                    upgWeapon.weaponDetails.baseUniqueModifier,
                    upgWeapon.weaponDetails.baseTypeModifier,
                    upgWeapon.enchantedBoostType
                });
                    upgWeapon.mythicBoostType = rolled;
                    ApplyWeaponBoost(upgWeapon, rolled, upgWeapon.weaponDetails);
                }
            }

            StaticEventHandler.CallInventoryWeaponUpgradedEventForBook(upgWeapon, inventoryIndexNumber);
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
                    ApplyPassiveBoost(upgPassive, rolled);
                }

                if (next >= Rarity.Mythic && upgPassive.mythicBoostType == BoostType.None)
                {
                    BoostType rolled = RollOne(pool, new HashSet<BoostType> { upgPassive.passiveItemDetails.baseUniqueModifier, upgPassive.enchantedBoostType });
                    upgPassive.mythicBoostType = rolled;
                    ApplyPassiveBoost(upgPassive, rolled);
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

    private bool DissambleProcess(DraggableItem draggableItem)
    {
        if (draggableItem != null)
        {
            // Inventory Slot Check
            if (slotType == SlotType.Dissamble && draggableItem.itemGeneric.itemSlotStatus == ItemSlotStatus.Inventory) 
            {
                int shardGain = 0;

                switch (draggableItem.itemGeneric.rarity)
                {
                    case Rarity.Basic: return true; // Dissamble failed
                    case Rarity.Enchanted: shardGain = 20; break;
                    case Rarity.Mythic: shardGain = 70; break;
                    case Rarity.Legendary: shardGain = 200; break;
                    default:
                        break;
                }

                player.coinsAndShards.AddShard(shardGain);
                InventoryManager.Instance.EmptyItemFromInventory(inventoryIndexNumber);

                // Book update
                if (draggableItem.itemGeneric is Weapon)
                {
                    StaticEventHandler.CallInventoryWeaponDroppedEventForBook(inventoryIndexNumber);
                }
                else if (draggableItem.itemGeneric is PassiveItem)
                {
                    StaticEventHandler.CallInventoryPassiveItemDroppedEventForBook(inventoryIndexNumber);
                }
            }

            return false;
        }

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
                rng2 = Random.Range(0.05f, 0.25f);
                weapon.attackCooldown = (float)Math.Round(rng2, 2);
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
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.attackRateVsLowHealthEnemies = (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                weapon.criticalResistance = (float)Math.Round(rng2, 2);
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

            default: 
                break;
        }
    }

    private void ApplyPassiveBoost(PassiveItem passiveItem, BoostType boost)
    {
        float rng = Random.Range(1f, 1.5f);
        float rng2;
        int margin = 0;

        switch (boost)
        {
            case BoostType.AttackCooldown:
                rng2 = Random.Range(0.05f, 0.25f);
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
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.lifeStealAmount = Mathf.RoundToInt(passiveItem.physicalAttackDamageIncrease * rng2);
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
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.attackRateVsLowHealthEnemies = (float)Math.Round(rng2, 2);
                break;
            case BoostType.CritResistance:
                rng2 = Random.Range(0.1f, 0.25f);
                passiveItem.criticalResistance = (float)Math.Round(rng2, 2);
                break;
            case BoostType.ArmorIncrease:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.armorIncrease = (float)Math.Round(rng2, 2);
                break;
            case BoostType.MagicResistance:
                rng2 = Random.Range(0.05f, 0.25f);
                passiveItem.magicResistance = (float)Math.Round(rng2, 2);
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
            default: break;
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
                        enchantedBoostText.text = $"Attack Speed: + {weapon.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        enchantedBoostText.text = "Phy. Attack Dmg.: + " + weapon.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        enchantedBoostText.text = $"Attack Rating: + {weapon.attackRatingIncrease * 100}%";
                        break;
                    case BoostType.MagicDamage:
                        enchantedBoostText.text = "Magic Attack Dmg.: + " + weapon.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        enchantedBoostText.text = $"Cr. Hit Chance: + {weapon.criticalHitChanceIncrease * 100}%";
                        break;
                    case BoostType.CritDamage:
                        enchantedBoostText.text = $"Cr. Hit Damage: + {weapon.criticalHitDamageIncrease * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        enchantedBoostText.text = $"Life Steal: + {weapon.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        enchantedBoostText.text = $"Block Chance: + {weapon.blockChanceIncrease * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        enchantedBoostText.text = $"Dodge Chance: + {weapon.dodgeChanceIncrease * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        enchantedBoostText.text = $"Health: + {weapon.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        enchantedBoostText.text = $"Mana: + {weapon.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        enchantedBoostText.text = $"Status Resistance: + {weapon.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        enchantedBoostText.text = $"Attack vs Low Health Enemies: + {weapon.attackRateVsLowHealthEnemies * 100}%";
                        break;
                    case BoostType.CritResistance:
                        enchantedBoostText.text = $"Cr. Resistance: + {weapon.criticalResistance}%";
                        break;
                    case BoostType.ArmorIncrease:
                        enchantedBoostText.text = $"Armor: + {weapon.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        enchantedBoostText.text = $"Magic Resistance: + {weapon.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        enchantedBoostText.text = $"Magic Resistance: + {weapon.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        enchantedBoostText.text = $"Damage Reduction: + {weapon.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        enchantedBoostText.text = $"Armor Penetration: + {weapon.armorPenetration * 100}%";
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
                        enchantedBoostText.text += $"\nAttack Speed: + {weapon.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        enchantedBoostText.text += "\nPhy. Attack Dmg.: + " + weapon.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        enchantedBoostText.text += $"\nAttack Rating: + {weapon.attackRatingIncrease * 100}%";
                        break;
                    case BoostType.MagicDamage:
                        enchantedBoostText.text += "\nMagic Attack Dmg.: + " + weapon.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        enchantedBoostText.text += $"\nCr. Hit Chance: + {weapon.criticalHitChanceIncrease * 100}%";
                        break;
                    case BoostType.CritDamage:
                        enchantedBoostText.text += $"\nCr. Hit Damage: + {weapon.criticalHitDamageIncrease * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        enchantedBoostText.text += $"\nLife Steal: + {weapon.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        enchantedBoostText.text += $"\nBlock Chance: + {weapon.blockChanceIncrease * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        enchantedBoostText.text += $"\nDodge Chance: + {weapon.dodgeChanceIncrease * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        enchantedBoostText.text += $"\nHealth: + {weapon.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        enchantedBoostText.text += $"\nMana: + {weapon.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        enchantedBoostText.text += $"\nStatus Resistance: + {weapon.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        enchantedBoostText.text += $"\nAttack vs Low Health Enemies: + {weapon.attackRateVsLowHealthEnemies * 100}%";
                        break;
                    case BoostType.CritResistance:
                        enchantedBoostText.text += $"\nCr. Resistance: + {weapon.criticalResistance}%";
                        break;
                    case BoostType.ArmorIncrease:
                        enchantedBoostText.text += $"\nArmor: + {weapon.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        enchantedBoostText.text += $"\nMagic Resistance: + {weapon.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        enchantedBoostText.text += $"\nMagic Resistance: + {weapon.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        enchantedBoostText.text += $"\nDamage Reduction: + {weapon.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        enchantedBoostText.text += $"\nArmor Penetration: + {weapon.armorPenetration * 100}%";
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
                        hitSpeedText.text = $"Attack Speed: + {passiveItem.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        hitSpeedText.text = "Phy. Attack Dmg.: + " + passiveItem.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        hitSpeedText.text = $"Attack Rating: + {passiveItem.attackRating * 100}%";
                        break;
                    case BoostType.MagicDamage:
                        hitSpeedText.text = "Magic Attack Dmg.: + " + passiveItem.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        hitSpeedText.text = $"Cr. Hit Chance: + {passiveItem.criticalHitChance * 100}%";
                        break;
                    case BoostType.CritDamage:
                        hitSpeedText.text = $"Cr. Hit Damage: + {passiveItem.criticalHitDamage * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        hitSpeedText.text = $"Life Steal: + {passiveItem.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        hitSpeedText.text = $"Block Chance: + {passiveItem.blockChance * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        hitSpeedText.text = $"Dodge Chance: + {passiveItem.dodgeChance * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        hitSpeedText.text = $"Health: + {passiveItem.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        hitSpeedText.text = $"Mana: + {passiveItem.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        hitSpeedText.text = $"Status Resistance: + {passiveItem.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        hitSpeedText.text = $"Attack vs Low Health Enemies: + {passiveItem.attackRateVsLowHealthEnemies * 100}%";
                        break;
                    case BoostType.CritResistance:
                        hitSpeedText.text = $"Cr. Resistance: + {passiveItem.criticalResistance}%";
                        break;
                    case BoostType.ArmorIncrease:
                        hitSpeedText.text = $"Armor: + {passiveItem.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        hitSpeedText.text = $"Magic Resistance: + {passiveItem.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        hitSpeedText.text = $"Magic Resistance: + {passiveItem.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        hitSpeedText.text = $"Damage Reduction: + {passiveItem.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        hitSpeedText.text = $"Armor Penetration: + {passiveItem.armorPenetration * 100}%";
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
                        hitSpeedText.text += $"\nAttack Speed: + {passiveItem.attackCooldown * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        hitSpeedText.text += "\nPhy. Attack Dmg.: + " + passiveItem.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        hitSpeedText.text += $"\nAttack Rating: + {passiveItem.attackRating * 100}%";
                        break;
                    case BoostType.MagicDamage:
                        hitSpeedText.text += "\nMagic Attack Dmg.: + " + passiveItem.magicAttackDamageIncrease;
                        break;
                    case BoostType.CritChance:
                        hitSpeedText.text += $"\nCr. Hit Chance: + {passiveItem.criticalHitChance * 100}%";
                        break;
                    case BoostType.CritDamage:
                        hitSpeedText.text += $"\nCr. Hit Damage: + {passiveItem.criticalHitDamage * 100}%";
                        break;
                    case BoostType.LifeSteal:
                        hitSpeedText.text += $"\nLife Steal: + {passiveItem.lifeStealAmount}";
                        break;
                    case BoostType.BlockChance:
                        hitSpeedText.text += $"\nBlock Chance: + {passiveItem.blockChance * 100}%";
                        break;
                    case BoostType.DodgeChance:
                        hitSpeedText.text += $"\nDodge Chance: + {passiveItem.dodgeChance * 100}%";
                        break;
                    case BoostType.HealthIncrease:
                        hitSpeedText.text += $"\nHealth: + {passiveItem.increasedMaxHealth}";
                        break;
                    case BoostType.ManaIncrease:
                        hitSpeedText.text += $"\nMana: + {passiveItem.increasedMaxMana}";
                        break;
                    case BoostType.StatusResistance:
                        hitSpeedText.text += $"\nStatus Resistance: + {passiveItem.statusResistanceModifier * 100}%";
                        break;
                    case BoostType.AttackVsLowHealthEnemies:
                        hitSpeedText.text += $"\nAttack vs Low Health Enemies: + {passiveItem.attackRateVsLowHealthEnemies * 100}%";
                        break;
                    case BoostType.CritResistance:
                        hitSpeedText.text += $"\nCr. Resistance: + {passiveItem.criticalResistance}%";
                        break;
                    case BoostType.ArmorIncrease:
                        hitSpeedText.text += $"\nArmor: + {passiveItem.armorIncrease * 100}%";
                        break;
                    case BoostType.MagicResistance:
                        hitSpeedText.text += $"\nMagic Resistance: + {passiveItem.magicResistance * 100}%";
                        break;
                    case BoostType.MoveSpeed:
                        hitSpeedText.text += $"\nMagic Resistance: + {passiveItem.speedIncreaseModifier}";
                        break;
                    case BoostType.DamageReduction:
                        hitSpeedText.text += $"\nDamage Reduction: + {passiveItem.damageReductionRate * 100}%";
                        break;
                    case BoostType.ArmorPenetration:
                        hitSpeedText.text += $"\nArmor Penetration: + {passiveItem.armorPenetration * 100}%";
                        break;
                    default:
                        break;
                }
            }
        }
    }
}