using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class Slot : MonoBehaviour, IDropHandler, IPointerEnterHandler, IPointerExitHandler
{
    public SlotType slotType;
    public PassiveItemSlotName passiveItemSlotName;
    public RectTransform tooltipPanel;

    Player player;
    Transform backgroundTransform;
    Transform equippedTransform;

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
        backgroundTransform = transform.GetChild(0);
        equippedTransform = transform.GetChild(1);

        tooltipPanel.transform.localPosition = new Vector3(60f, 20f, 0f);
        UpdateTooltipPanelInfo();
    }

    private void UpdateTooltipPanelInfo()
    {
        Transform currentChild = null;

        if (slotType == SlotType.Passive) 
        {
            headerText.colorGradient = new VertexGradient(GameManager.Instance.passiveItemColor, GameManager.Instance.passiveItemColor,
                GameManager.Instance.passiveItemColor, GameManager.Instance.passiveItemColor);
            headerText.text = string.Empty;
            levelText.colorGradient = new VertexGradient(GameManager.Instance.passiveItemColor, GameManager.Instance.passiveItemColor,
                GameManager.Instance.passiveItemColor, GameManager.Instance.passiveItemColor);
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

            switch (passiveItemSlotName)
            {
                case PassiveItemSlotName.None:
                    break;
                case PassiveItemSlotName.Head:
                    if (player.selectedPassiveItem.headPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case PassiveItemSlotName.Chest:
                    if (player.selectedPassiveItem.chestPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case PassiveItemSlotName.Neck:
                    if (player.selectedPassiveItem.neckPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case PassiveItemSlotName.Finger:
                    if (player.selectedPassiveItem.fingerPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case PassiveItemSlotName.Back:
                    if (player.selectedPassiveItem.backPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case PassiveItemSlotName.Waist:
                    if (player.selectedPassiveItem.waistPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case PassiveItemSlotName.Arm:
                    if (player.selectedPassiveItem.armPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                case PassiveItemSlotName.Leg:
                    if (player.selectedPassiveItem.legPassiveItem == null)
                    {
                        tooltipPanel.gameObject.SetActive(false);
                        return;
                    }
                    break;
                default:
                    break;
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
                        weaponWieldText.text = "Dual-Wield Dagger or Claw Equipped";
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
        else if (slotType == SlotType.Active)
        {
            headerText.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
            headerText.text = string.Empty;
            levelText.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
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

            if (player.selectedActiveItem.GetCurrentActiveItem() == null)
            {
                tooltipPanel.gameObject.SetActive(false);
                return;
            }

            // Check if the slot is occupied
            if (equippedTransform.childCount > 0)
            {
                currentChild = equippedTransform.GetChild(0);

                // Retrieve draggable item and weapon from current child
                DraggableItem slotDragggableItem = currentChild.GetComponent<DraggableItem>();
                ActiveItem activeItem = slotDragggableItem.GetDraggedActiveItem();

                if (activeItem != null)
                {
                    headerText.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
                    headerText.text = activeItem.activeItemDetails.activeItemName;
                    levelText.text = $"(Active Item)";

                    if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Dummy)
                    {
                        weaponClassText.text = "Distracts Enemies Until Being";
                        hitSpeedText.text = "Destroyed";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Potion)
                    {
                        weaponClassText.text = "Slowly Regenerates Health";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Bomb)
                    {
                        weaponClassText.text = "Explodes and Gives";
                        hitSpeedText.text = "AoE Damage";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Compass)
                    {
                        weaponClassText.text = "Locates Boss Room's";
                        hitSpeedText.text = "Direction";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Boomerang)
                    {
                        weaponClassText.text = "Strikes And Return, Useful";
                        hitSpeedText.text = "AoE DamageFor Stunning Enemies";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Hourglass)
                    {
                        weaponClassText.text = "Slows the Time Flow By";
                        hitSpeedText.text = "Half to Act More Precisely";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Shiruken)
                    {
                        weaponClassText.text = "Several Quick Throwable";
                        hitSpeedText.text = "Star Projectiles";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Trap)
                    {
                        weaponClassText.text = "Trap for Enemies To Step On";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Summoner)
                    {
                        weaponClassText.text = "Summoning Ally Mobs as Companion";
                        hitSpeedText.text = "For a Short Time";
                    }
                    else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.BobbyPin)
                    {
                        weaponClassText.text = "Chance to Crack The";
                        hitSpeedText.text = "Chest Without a Key";
                        weaponWieldText.text = "Only One Attempt Permitted";
                    }
                }

                //levelText.text = $"({weapon.weaponDetails.weaponLevel.ToString()})";
                //weaponClassText.text = $"Class: {weapon.weaponDetails.weaponClass.ToString()}";

                //if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                //{
                //    weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";
                //    damageText.text = $"Deflect Rate: {weapon.weaponDetails.projectileDeflectRatio * 100}%";
                //}
                //else
                //{
                //    hitSpeedText.text = $"Speed: {weapon.weaponDetails.weaponHitSpeed.ToString()}";
                //    weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";
                //    damageText.text = $"Damage: {weapon.weaponDetails.meleeDamageMin}-{weapon.weaponDetails.meleeDamageMax}";
                //}

                //baseHandlingText.text = $"Base Handling: {weapon.weaponDetails.weaponBaseHandling * 100}%";
                //crHitChanceText.text = $"Base Cr. Hit Chance: {weapon.weaponDetails.criticalHitChance * 100}%";
                //crHitDamageText.text = $"Base Cr. Hit Damage: {weapon.weaponDetails.criticalHitDamageMultiplier * 100}%";
                //elementalBiasText.text = "Elemental Bias";
            }
        }
        else if (slotType == SlotType.WeaponMainHand || slotType == SlotType.WeaponOffHand)
        {
            if (slotType == SlotType.WeaponMainHand)
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
                        damageText.text = $"Deflect Rate: {weapon.weaponDetails.projectileDeflectRatio * 100}%";
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
    }

    /// <summary>
    /// Open tooltip panel when hovering over the related item or weapon
    /// </summary>
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipPanel.gameObject.SetActive(true);
        UpdateTooltipPanelInfo();
    }

    /// <summary>
    /// Close tooltip panel when stop hovering over the related item or weapon
    /// </summary>
    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.gameObject.SetActive(false);
    }

    public void OnDrop(PointerEventData eventData)
    {
        GameObject draggedItem = eventData.pointerDrag;
        DraggableItem draggableItem = draggedItem.GetComponent<DraggableItem>();

        if (draggableItem != null)
        {
            // If dragged item is not a weapon, cancel the swap or move
            if (draggableItem.receivable is not Weapon) return;

            if (draggableItem.receivable is Weapon && (slotType == SlotType.Active || slotType == SlotType.Passive)) return;

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
                // If slot is occupied, swap items
                SwapItems(draggableItem, currentChild);
            }
            else
            {
                draggableItem.justMoveNotSwap = true;

                // If slot is not occupied, just relocate selected item
                MoveItemToSlot(draggableItem);
            }
        }
    }

    private void SwapItems(DraggableItem draggableItem, Transform currentChild)
    {
        DraggableItem currentSlotsDraggableItem = currentChild.GetComponent<DraggableItem>();

        if (currentSlotsDraggableItem.isLockIcon) return;

        Weapon draggableItemWeapon = (Weapon)draggableItem.receivable;
        Weapon currentSlotsDraggableItemWeapon = (Weapon)currentSlotsDraggableItem.receivable;

        // Move slot's weapon to draggable item's previous slot
        // Draggable item is on main hand
        if (draggableItemWeapon.onMainHand)
        {
            if (currentSlotsDraggableItemWeapon == null)
            {
                // Draggable item is two-handed weapon, so swap is canceled

                draggableItem.swapCanceled = true;
                return;
            }
            // Slot and draggable items are both main hands
            else if (currentSlotsDraggableItemWeapon.onMainHand)
            {
                // Draggable item doesn't have an off-hand weapon
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                {
                    // Slot's current set doesn't have an off-hand weapon
                    if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                    {
                        // Both set's off-hand slots are empty so swap is successful
                        SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                    }
                    else // Slot's current set has an off-hand weapon
                    {
                        // Draggable item is two-handed weapon
                        if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                        {
                            // Draggable item is two-handed weapon, so swap is canceled
                            draggableItem.swapCanceled = true;
                            return;
                        }
                        else
                        {
                            // Draggable item is not a two-handed weapon, so swap is successful
                            SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                        }
                    }
                }
                // Slot item has an off-hand weapon
                else
                {
                    // Slot's current set doesn't have an off-hand weapon
                    if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] == null)
                    {
                        // Slot's current weapon is a two-handed weapon
                        if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                        {
                            // Draggable item has an off-hand and slot item is a two-handed weapon, so swap is canceled
                            draggableItem.swapCanceled = true;
                            return;
                        }
                        else
                        {
                            // Draggable item has an off-hand and but slot item is a one-handed weapon, so swap is succesful
                            SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                        }
                    }
                    // Slot's current set has an off-hand weapon
                    else
                    {
                        // Both draggable and slot item sets have an off-hand weapon. This means both items are one-handed, so swap is succesfful
                        SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotMain);
                    }
                }
            }
            // Draggable item is at main-hand and slot item is at off-hand
            else
            {
                // Slot item is a shield
                if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    // Slot item is a shield, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    draggableItem.swapCanceled = true;
                    return;
                }
                // Draggable item is a two-handed weapon
                else if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    // Draggable item is two-handed weapon, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandCantBeAddedToTwoHanded);
                    draggableItem.swapCanceled = true;
                    return;
                }
                else
                {
                    // Neither slot item is a shield nor draggable item is a two-handed weapon, so swap is succesfful
                    SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragMainSlotOff);
                }
            }
        }
        // Draggable item is on off-hand
        else
        {
            // Draggable item is on off-hand and slot item is on main hand
            if (currentSlotsDraggableItemWeapon.onMainHand)
            {
                // Draggable item is a shield
                if (draggableItemWeapon.weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    // Draggable item is a shield, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    draggableItem.swapCanceled = true;
                    return;
                }
                // Slot item is a two-handed weapon
                else if (player.weaponSlotSetArray[currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    // Slot item is two-handed weapon, so swap is canceled
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandCantBeAddedToTwoHanded);
                    draggableItem.swapCanceled = true;
                    return;
                }
                else
                {
                    // Neither draggable item is a shield nor slot item is a two-handed weapon, so swap is succesfful
                    SwapProcess(draggableItem,  currentSlotsDraggableItem, ItemSwapPos.DragOffSlotMain);
                }
            }
            // Both draggable and slot items are off-hand
            else
            {
                // Both draggable and slot items are off-hand; it means they are either one-handed or a shield, so swap is successful
                SwapProcess(draggableItem, currentSlotsDraggableItem, ItemSwapPos.DragOffSlotOff);
            }
        }
    }

    private void MoveItemToSlot(DraggableItem draggableItem)
    {
        BackgroundAndEquippedSlotTransactions(draggableItem);
    }

    private void SwapProcess(DraggableItem draggableItem, DraggableItem currentSlotsDraggableItem, ItemSwapPos itemSwapPos)
    {
        Weapon draggableItemWeapon = (Weapon)draggableItem.receivable;
        Weapon currentSlotsDraggableItemWeapon = (Weapon)currentSlotsDraggableItem.receivable;

        switch (itemSwapPos)
        {
            case ItemSwapPos.None:
                break;
            case ItemSwapPos.DragMainSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponBelongingToWhichMainHandSet;
                currentSlotsDraggableItemWeapon.onMainHand = true;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMainHand = true;
                player.playerControl.SetWeaponSetByIndex(true);
                break;
            case ItemSwapPos.DragMainSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet = draggableItemWeapon.weaponBelongingToWhichMainHandSet;
                currentSlotsDraggableItemWeapon.onMainHand = true;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMainHand = false;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(true);
                }
                else
                {
                    Destroy(equippedTransform.GetChild(0).gameObject);
                    StaticEventHandler.CallWeaponAddedToOffHandBook(draggableItemWeapon);
                }
                break;
            case ItemSwapPos.DragOffSlotMain:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponBelongingToWhichOffHandSet;
                currentSlotsDraggableItemWeapon.onMainHand = false;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMainHand = true;

                if (draggableItem.transactionOnTheSameSet)
                {
                    player.playerControl.SetWeaponSetByIndex(true);
                }
                else
                {
                    Destroy(equippedTransform.GetChild(0).gameObject);
                    StaticEventHandler.CallWeaponAddedToMainHandBook(draggableItemWeapon, false);
                }

                break;
            case ItemSwapPos.DragOffSlotOff:
                // Put current slots child to draggable item slot
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = currentSlotsDraggableItemWeapon;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                currentSlotsDraggableItemWeapon.weaponBelongingToWhichOffHandSet = draggableItemWeapon.weaponBelongingToWhichOffHandSet;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                draggableItemWeapon.onMainHand = false;
                currentSlotsDraggableItemWeapon.onMainHand = false;
                player.playerControl.SetWeaponSetByIndex(true);
                break;
            default:
                break;
        }
    }

    private void BackgroundAndEquippedSlotTransactions(DraggableItem draggableItem)
    {
        Weapon draggableItemWeapon = (Weapon)draggableItem.receivable;

        if (draggableItemWeapon.onMainHand)
        {
            if (slotType == SlotType.WeaponMainHand)
            {
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] != null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EmptyOffHandFirst);
                    draggableItem.swapCanceled = true;
                    return;
                }

                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;

                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                //player.ActivateWeapon(draggableItemWeapon, false, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true);
            }
            else
            {
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][1] != null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EmptyOffHandFirst);
                    draggableItem.swapCanceled = true;
                    return;
                }
                else
                {
                    if (draggableItemWeapon.weaponBelongingToWhichMainHandSet == player.currentWeaponSlotSetIndex)
                    {
                        GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                        draggableItem.swapCanceled = true;
                        return;
                    }
                }

                if (draggableItem.transactionOnTheSameSet)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.CantMoveYourMainHandWithEmptyOffHand);
                    draggableItem.swapCanceled = true;
                    return;
                }

                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichMainHandSet - 1][0] = null;
                player.mainHandSlotFilled = false; // Change flag so this empty slot can be used for future pick-ups

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;
                draggableItemWeapon.onMainHand = false; 

                draggableItemWeapon.weaponBelongingToWhichMainHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                //player.ActivateWeapon(draggableItemWeapon, true, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true, true);
                draggableItem.dragMainSlotOff = true;
            }
        }
        else
        {
            if (slotType == SlotType.WeaponMainHand)
            {
                if (player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1].weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                    draggableItem.swapCanceled = true;
                    return;
                }
                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = null;

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] = draggableItemWeapon;
                draggableItemWeapon.onMainHand = true;

                draggableItemWeapon.weaponBelongingToWhichOffHandSet = 0;
                draggableItemWeapon.weaponBelongingToWhichMainHandSet = player.currentWeaponSlotSetIndex;
                //player.ActivateWeapon(draggableItemWeapon, true, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true);
            }
            else
            {
                if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] == null)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.EquipMainHandFirst);
                    draggableItem.swapCanceled = true;
                    return;
                }

                player.weaponSlotSetArray[draggableItemWeapon.weaponBelongingToWhichOffHandSet - 1][1] = null;
                player.offHandSlotFilled = false; // Change flag so this empty slot can be used for future pick-ups

                // Put draggable item to current slot
                player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] = draggableItemWeapon;

                draggableItemWeapon.weaponBelongingToWhichOffHandSet = player.currentWeaponSlotSetIndex;
                //player.ActivateWeapon(draggableItemWeapon, true, player.currentWeaponSlotSetIndex);
                player.playerControl.SetWeaponSetByIndex(true);
            }
        }
    }
}
