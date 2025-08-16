using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

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
    [SerializeField] TextMeshProUGUI headerText;
    [SerializeField] TextMeshProUGUI levelText;
    [SerializeField] TextMeshProUGUI weaponClassText;
    [SerializeField] TextMeshProUGUI hitSpeedText;
    [SerializeField] TextMeshProUGUI weaponWieldText;
    [SerializeField] TextMeshProUGUI damageText;
    [SerializeField] TextMeshProUGUI baseHandlingText;
    [SerializeField] TextMeshProUGUI crHitChanceText;
    [SerializeField] TextMeshProUGUI crHitDamageText;
    [SerializeField] TextMeshProUGUI elementalBiasText;
    [SerializeField] TextMeshProUGUI elementText;
    [SerializeField] TextMeshProUGUI elementalForgeRateText;
    [SerializeField] TextMeshProUGUI masteryText1;
    [SerializeField] TextMeshProUGUI masteryText2;
    [SerializeField] TextMeshProUGUI masteryText3;

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

        if (slotType != SlotType.Drop)
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
            headerText.colorGradient = new VertexGradient(MainUI.Instance.passiveItemColor, MainUI.Instance.passiveItemColor,
                MainUI.Instance.passiveItemColor, MainUI.Instance.passiveItemColor);
            headerText.text = string.Empty;
            levelText.colorGradient = new VertexGradient(MainUI.Instance.passiveItemColor, MainUI.Instance.passiveItemColor,
                MainUI.Instance.passiveItemColor, MainUI.Instance.passiveItemColor);
            levelText.text = string.Empty;
            weaponClassText.text = string.Empty;
            hitSpeedText.text = string.Empty;
            weaponWieldText.text = string.Empty;
            damageText.text = string.Empty;
            baseHandlingText.text = string.Empty;
            crHitChanceText.text = string.Empty;
            crHitDamageText.text = string.Empty;
            elementalBiasText.text = string.Empty;
            elementText.text = string.Empty;
            elementalForgeRateText.text = string.Empty;
            masteryText1.text = string.Empty;
            masteryText2.text = string.Empty;
            masteryText3.text = string.Empty;

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
                    headerText.text = passiveItem.passiveItemDetails.passiveItemName;
                    levelText.text = $"(Passive Item)";

                    // HEAD
                    if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WardenOfForest)
                    {
                        weaponClassText.text = "+100% Accuracy for Bows";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HaloOfBlindingRadiance)
                    {
                        weaponClassText.text = "+10% Light Resistance";
                        hitSpeedText.text = "+10% Chance to Blind";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.HelmOfTheEternalVigil)
                    {
                        weaponClassText.text = "+1 Dexterity";
                        hitSpeedText.text = "+10% Physical Resistance";
                        weaponWieldText.text = "+10% Evasiveness";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EnchantersSpire)
                    {
                        weaponClassText.text = "+1 Intelligence";
                        hitSpeedText.text = "+5% All Elemental Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WhisperingHood)
                    {
                        weaponClassText.text = "+1 Dexterity";
                        hitSpeedText.text = "+15% Evasiveness";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GildedGuardian)
                    {
                        weaponClassText.text = "+1 Constitution";
                        hitSpeedText.text = "+20% Physical Resistance";
                    }
                    // NECK
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RubyPendant)
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
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ChestplateOfTheLastLight)
                    {
                        weaponClassText.text = "+1 Strength";
                        hitSpeedText.text = "+30% Physical Resistance";
                        weaponWieldText.text = "Absorbs +30% Physical";
                        damageText.text = "Damage When Healt is";
                        baseHandlingText.text = "Below 50%";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BlazingHeartplate)
                    {
                        weaponClassText.text = "+1 Strength";
                        hitSpeedText.text = "+20% Physical Resistance";
                        weaponWieldText.text = "+10% Fire Resistance";
                        damageText.text = "-5% Attack Cooldown";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.FrostboundChainmail)
                    {
                        weaponClassText.text = "+15% Physical Resistance";
                        hitSpeedText.text = "+10% Water Resistance";
                        weaponWieldText.text = "Immune to Frost";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomweaveVest)
                    {
                        weaponClassText.text = "+10% Physical Resistance";
                        hitSpeedText.text = "+10% Earth Resistance";
                        weaponWieldText.text = "Immune to Poison";
                    }
                    // WAIST
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BeltOfSorcery)
                    {
                        weaponClassText.text = "+1 Intelligence";
                        hitSpeedText.text = "-20% Cast Duration";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.InfernoSash)
                    {
                        weaponClassText.text = "+1 Constitution";
                        hitSpeedText.text = "+5% Physical Resistance";
                        weaponWieldText.text = "+15% Fire Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GirdleOfFirmament)
                    {
                        weaponClassText.text = "+10% Air Resistance";
                        hitSpeedText.text = "+10% Light Resistance";
                        weaponWieldText.text = "Immune to Blind";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BloodforgedGirdle)
                    {
                        weaponClassText.text = "+1 Strength";
                        hitSpeedText.text = "+1 Agility";
                        weaponWieldText.text = "-5% Melee Attack Cooldown";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SandweaversSash)
                    {
                        weaponClassText.text = "+1 Dexterity";
                        hitSpeedText.text = "+10% Evasiveness";
                        weaponWieldText.text = "+5% Cr.Hit Chance";
                    }
                    // FINGER
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfFortune)
                    {
                        weaponClassText.text = "+15% Drop Chance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfTempestStrikes)
                    {
                        weaponClassText.text = "-20% Attack Cooldown";
                        hitSpeedText.text = "-10% Physical Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfMight)
                    {
                        weaponClassText.text = "+1 Stength";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfVitality)
                    {
                        weaponClassText.text = "+1 Constitution";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfSagacity)
                    {
                        weaponClassText.text = "+1 Intelligence";
                    }
                    // BACK
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                    {
                        weaponClassText.text = "+5% Cr. Hit Chance";
                        hitSpeedText.text = "+10% Cr. Hit Chance When";
                        weaponWieldText.text = "Dual-Wield Dagger or";
                        damageText.text = "Claw Equipped";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RecantersCloak)
                    {
                        weaponClassText.text = "+1 Agility";
                        hitSpeedText.text = "+10% Evasiveness";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                    {
                        weaponClassText.text = "+5% Elemental Damage";
                        hitSpeedText.text = "+15% Elemental Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.CloakOfWindwalker)
                    {
                        weaponClassText.text = "+2 Agility";
                        hitSpeedText.text = "+30% Air Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.GoldenCloak)
                    {
                        weaponClassText.text = "+1 All Primary Stats";
                    }
                    // ARM
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.OminousGripOfThunder)
                    {
                        weaponClassText.text = "+5% Physical Resistance";
                        hitSpeedText.text = "+10% Air Resistance";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmbercladBracers)
                    {
                        weaponClassText.text = "+10% Physical Resistance";
                        hitSpeedText.text = "+8% Fire Resistance";
                        weaponWieldText.text = "-5% Attack Cooldown";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.VenomTouchedGloves)
                    {
                        weaponClassText.text = "+10% Earth Resistance";
                        hitSpeedText.text = "Immunity to Poison";
                    }
                    // LEG
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.WingedSandals)
                    {
                        weaponClassText.text = "+2 Agility";
                    }
                    else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.BootsOfInfernalMarch)
                    {
                        weaponClassText.text = "+1 Agility";
                        hitSpeedText.text = "+15% Fire Resistance";
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
                    switch (weapon.weaponDetails.weaponLevel)
                    {
                        case WeaponLevel.Basic:
                            headerText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                            levelText.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                            break;
                        case WeaponLevel.Enchanted:
                            headerText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                            levelText.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                            break;
                        case WeaponLevel.Mythic:
                            headerText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                            levelText.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                            break;
                        case WeaponLevel.Legendary:
                            headerText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                            levelText.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                            break;
                        default:
                            break;
                    }

                    headerText.text = weapon.weaponDetails.weaponName;
                    levelText.text = $"({weapon.weaponDetails.weaponLevel.ToString()})";
                    weaponClassText.text = $"Class: {weapon.weaponDetails.weaponClass.ToString()}";

                    if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                    {
                        weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";
                        damageText.text = $"Deflect Rate: {weapon.weaponDetails.blockRate * 100}%";
                    }
                    else
                    {
                        hitSpeedText.text = $"Speed: {weapon.weaponDetails.weaponHitSpeed.ToString()}";
                        weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";

                        if (weapon.weaponDetails.isMeleeWeapon)
                        {
                            damageText.text = $"Damage: {weapon.weaponDetails.meleeDamageMin}-{weapon.weaponDetails.meleeDamageMax}";
                        }
                        else
                        {
                            damageText.text = $"Damage: {weapon.weaponDetails.weaponCurrentProjectile.projectileDamageMin}-{weapon.weaponDetails.weaponCurrentProjectile.projectileDamageMax}";
                        }
                    }

                    baseHandlingText.text = $"Base Handling: {weapon.weaponDetails.weaponBaseHandling * 100}%";

                    crHitChanceText.text = $"Base Cr. Hit Chance: {weapon.weaponDetails.criticalHitChance * 100}%";

                    if (weapon.weaponDetails.isMeleeWeapon)
                    {
                        crHitDamageText.text = $"Base Cr. Hit Damage: {(weapon.weaponDetails.criticalHitDamageMultiplier + player.additionalCriticalMeleeDamageModifier) * 100}%";
                    }
                    else
                    {
                        crHitDamageText.text = $"Base Cr. Hit Damage: {weapon.weaponDetails.criticalHitDamageMultiplier * 100}%";
                    }

                    elementalBiasText.text = "Elemental Bias:";

                    // Populate text field based on the related elemental info
                    switch (weapon.weaponDetails.elementalBias)
                    {
                        case ElementalBias.None:
                            elementText.colorGradient = new VertexGradient(noneElementalColor1, noneElementalColor1, noneElementalColor2, noneElementalColor2);
                            break;
                        case ElementalBias.Fire:
                            elementText.colorGradient = new VertexGradient(fireColor1, fireColor1, fireColor2, fireColor2);
                            break;
                        case ElementalBias.Water:
                            elementText.colorGradient = new VertexGradient(waterColor1, waterColor1, waterColor2, waterColor2);
                            break;
                        case ElementalBias.Earth:
                            elementText.colorGradient = new VertexGradient(earthColor1, earthColor1, earthColor2, earthColor2);
                            break;
                        case ElementalBias.Air:
                            elementText.colorGradient = new VertexGradient(airColor1, airColor1, airColor2, airColor2);
                            break;
                        case ElementalBias.Dark:
                            elementText.colorGradient = new VertexGradient(darkColor1, darkColor1, darkColor2, darkColor2);
                            break;
                        case ElementalBias.Light:
                            elementText.colorGradient = new VertexGradient(lightColor1, lightColor1, lightColor2, lightColor2);
                            break;
                        default:
                            break;
                    }

                    elementText.text = weapon.weaponDetails.elementalBias.ToString();
                    elementalForgeRateText.text = $"El. Forge Rate: {weapon.weaponDetails.elementalForgeRate * 100}%";

                    switch (weapon.weaponDetails.weaponLevel)
                    {
                        case WeaponLevel.Basic:
                            masteryText1.gameObject.SetActive(false);
                            masteryText2.gameObject.SetActive(false);
                            masteryText3.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Enchanted:
                            masteryText1.gameObject.SetActive(true);
                            masteryText1.text = "Enchanted Mastery: Locked";
                            masteryText2.gameObject.SetActive(false);
                            masteryText3.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Mythic:
                            masteryText1.gameObject.SetActive(true);
                            masteryText1.text = "Enchanted Mastery: Locked";
                            masteryText2.gameObject.SetActive(true);
                            masteryText2.text = "Mythic Mastery: Locked";
                            masteryText3.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Legendary:
                            masteryText1.gameObject.SetActive(true);
                            masteryText1.text = "Enchanted Mastery: Locked";
                            masteryText2.gameObject.SetActive(true);
                            masteryText2.text = "Mythic Mastery: Locked";
                            masteryText3.gameObject.SetActive(true);
                            masteryText3.text = "Legendary Mastery: Locked";
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

                SwapItems(selectedSlot.selectedSlotDraggableItem, targetDraggableItem, player.activeWeapon.GetCurrentMainHandWeapon(), player.activeWeapon.GetCurrentOffHandWeapon());
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
            if (SlotPlacementRules.IsSwapAllowed(draggableItem, targetItem, draggableItem.itemGeneric, targetItem.itemGeneric, playerMainHand, playerOffHand, validWeapon, out itemSwapPos))
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
            if (SlotPlacementRules.IsSwapAllowed(draggableItem, targetItem, draggableItem.itemGeneric, targetItem.itemGeneric, playerMainHand, playerOffHand, null, out itemSwapPos))
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
            Weapon draggableItemWeapon = (Weapon)draggableItem.itemGeneric;

            if (draggableItemWeapon.itemSlotStatus == ItemSlotStatus.MainHand)
            {
                if (inventoryIndexNumber >= 0 && !InventoryManager.Instance.IsInventoryFull())
                {
                    // Empty weapon on hand
                    player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;

                    // Place weapon into inventory
                    int invetoryIndex = InventoryManager.Instance.PlaceItemToLowestPossibleIndexSlot(draggableItemWeapon);

                    draggableItemWeapon.itemSlotStatus = ItemSlotStatus.Inventory;
                    draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;

                    player.playerControl.SetWeaponSetByIndex(true, false, false, true);

                    StaticEventHandler.CallOnWeaponAddedToInventoryEventForBook(draggableItemWeapon, invetoryIndex);

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

    private bool DropProcess(bool dropFailed, DraggableItem draggableItem)
    {
        if (draggableItem != null)
        {
            if (draggableItem.itemGeneric is Weapon)
            {
                Weapon weapon = (Weapon)draggableItem.itemGeneric;

                bool dropOffhand = draggableItem.belongingSlot.slotType == SlotType.WeaponOffHand ? true : false;

                player.playerControl.DropProcess(DropType.Weapon, weapon, null, dropOffhand, weapon.itemSlotStatus, draggableItem.belongingSlot.inventoryIndexNumber, true);
            }
            else if (draggableItem.itemGeneric is ActiveItem)
            {
                ActiveItem activeItem = (ActiveItem)draggableItem.itemGeneric;

                player.playerControl.DropProcess(DropType.ActiveItem);
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
            }
            else if (draggableItem.itemGeneric is PassiveItem)
            {
                PassiveItem passiveItem = (PassiveItem)draggableItem.itemGeneric;

                player.playerControl.DropProcess(DropType.PassiveItem, passiveItem, null, false, passiveItem.itemSlotStatus, draggableItem.belongingSlot.inventoryIndexNumber, true);
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
}
