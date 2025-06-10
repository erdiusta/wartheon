using TMPro;
using UnityEngine;

public class MainUI : SingletonMonobehaviour<MainUI>
{
    Player player;

    [Space(10)]
    [Header("TOOLTIP PANEL")]
    [Space(10)]
    // Tooltip panel
    public GameObject tooltipPanel;
    public TextMeshProUGUI headerText;
    public TextMeshProUGUI levelText;
    public TextMeshProUGUI requirementText;
    public TextMeshProUGUI weaponClassText;
    public TextMeshProUGUI hitSpeedText;
    public TextMeshProUGUI weaponWieldText;
    public TextMeshProUGUI damageText;
    public TextMeshProUGUI baseHandlingText;
    public TextMeshProUGUI crHitChanceText;
    public TextMeshProUGUI crHitDamageText;
    public TextMeshProUGUI elementalBiasText;
    public TextMeshProUGUI elementText;
    public TextMeshProUGUI elementalForgeRateText;
    public TextMeshProUGUI masteryText1;
    public TextMeshProUGUI masteryText2;
    public TextMeshProUGUI masteryText3;

    [Space(10)]
    // Tooltip panel equipped
    public GameObject tooltipPanelEquipped;
    public TextMeshProUGUI headerTextEquipped;
    public TextMeshProUGUI levelTextEquipped;
    public TextMeshProUGUI equippedText;
    public TextMeshProUGUI weaponClassTextEquipped;
    public TextMeshProUGUI hitSpeedTextEquipped;
    public TextMeshProUGUI weaponWieldTextEquipped;
    public TextMeshProUGUI damageTextEquipped;
    public TextMeshProUGUI baseHandlingTextEquipped;
    public TextMeshProUGUI crHitChanceTextEquipped;
    public TextMeshProUGUI crHitDamageTextEquipped;
    public TextMeshProUGUI elementalBiasTextEquipped;
    public TextMeshProUGUI elementTextEquipped;
    public TextMeshProUGUI elementalForgeRateTextEquipped;
    public TextMeshProUGUI masteryText1Equipped;
    public TextMeshProUGUI masteryText2Equipped;
    public TextMeshProUGUI masteryText3Equipped;

    // COLORS
    // Weapon Level 
    [HideInInspector] public Color basicLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color basicLevelColor2 = new Color(0.2196078f, 0.172549f, 0.172549f);
    [HideInInspector] public Color enchantedLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color enchantedLevelColor2 = new Color(0f, 0.4588235f, 1f);
    [HideInInspector] public Color mythicLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color mythicLevelColor2 = new Color(0.6745098f, 0, 1);
    [HideInInspector] public Color legendaryLevelColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color legendaryLevelColor2 = new Color(1f, 0.09411765f, 0f);
    // Weapon Level 
    [HideInInspector] public Color noneElementalColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color noneElementalColor2 = new Color(1, 1, 1);
    [HideInInspector] public Color fireColor1 = new Color(0.9686275f, 1, 0.2980392f);
    [HideInInspector] public Color fireColor2 = new Color(1f, 0.1921569f, 0.2431373f);
    [HideInInspector] public Color waterColor1 = new Color(0.8431373f, 0.9647059f, 1);
    [HideInInspector] public Color waterColor2 = new Color(0, 0.5882353f, 1);
    [HideInInspector] public Color airColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color airColor2 = new Color(0.4235294f, 0.4235294f, 0.4235294f);
    [HideInInspector] public Color earthColor1 = new Color(0.5647059f, 1f, 0.2509804f);
    [HideInInspector] public Color earthColor2 = new Color(0.02352941f, 0.4352941f, 0.03529412f);
    [HideInInspector] public Color lightColor1 = new Color(1, 1, 1);
    [HideInInspector] public Color lightColor2 = new Color(0.9716981f, 0.8067644f, 0f);
    [HideInInspector] public Color darkColor1 = new Color(0.627451f, 0, 1);
    [HideInInspector] public Color darkColor2 = new Color(0.6784314f, 0.01568628f, 0.5607843f);

    [HideInInspector] public Color passiveItemColor = new Color(0f, 0.7f, 1f);

    TooltipSource currentTooltipSource;

    [Space(10)]
    [Header("TUTORIAL PANEL")]
    [Space(10)]
    [SerializeField] GameObject tutorialPanel;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs roomChangedEventArgs)
    {
        tooltipPanel.SetActive(false);
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();

        tutorialPanel.SetActive(InputManager.TutorialEnabled);
    }

    private void Update()
    {
        if (InputManager.TutorialEnabled)
        {
            if (TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.MinimapCheck)
            {
                Transform minimapPromptArrow = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1);
                minimapPromptArrow.gameObject.SetActive(true);
            }
            else
            {
                Transform minimapPromptArrow = transform.GetChild(0).GetChild(0).GetChild(0).GetChild(1);
                minimapPromptArrow.gameObject.SetActive(false);
            }
        }
    }

    public void UpdateTooltipPanelInfo(ItemGeneric itemGeneric, bool hasWeaponDrop, bool hasActiveDrop, bool hasSecondaryPassiveDrop, TooltipSource source)
    {
        if (currentTooltipSource == source) return;

        currentTooltipSource = source;

        tooltipPanel.SetActive(true);
        if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
        {
            if (itemGeneric is Weapon)
            {
                Weapon weapon = (Weapon)itemGeneric;

                if (weapon.weaponDetails.weaponClass != WeaponClass.Shield)
                {
                    tooltipPanelEquipped.SetActive(true);
                }
            }
        }

        ClearTooltipPanel();
        ClearTooltipEquippedPanel();

        if (hasSecondaryPassiveDrop)
        {
            if (itemGeneric is PassiveItem)
            {
                headerText.colorGradient = new VertexGradient(passiveItemColor, passiveItemColor, passiveItemColor, passiveItemColor);
                levelText.colorGradient = new VertexGradient(passiveItemColor, passiveItemColor, passiveItemColor, passiveItemColor);
                PassiveItem passiveItem = (PassiveItem)itemGeneric;
                PassiveItemDetailsSO passiveItemDetails = passiveItem.passiveItemDetails;

                headerText.text = passiveItemDetails.passiveItemName;
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
                //NECK
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
                    weaponWieldText.text = "Absorbs +30% Physical Damage";
                    damageText.text = "When Healt is below 50%";
                    baseHandlingText.text = "";
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
                    weaponClassText.text = "+1 Strength";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfVitality)
                {
                    weaponClassText.text = "+1 Constitution";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.RingOfSagacity)
                {
                    weaponClassText.text = "+1 Intelligence";
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
        if (hasActiveDrop)
        {
            if (itemGeneric is ActiveItem)
            {
                headerText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                levelText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                ActiveItem activeItem = (ActiveItem)itemGeneric;
                ActiveItemDetailsSO activeItemDetails = activeItem.activeItemDetails;

                headerText.text = activeItemDetails.activeItemName;
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
                    weaponClassText.text = "Explodes and Gives AoE Damage";
                    //hitSpeedText.text = "AoE Damage";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Compass)
                {
                    weaponClassText.text = "Locates Boss Room's";
                    hitSpeedText.text = "Direction";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Boomerang)
                {
                    weaponClassText.text = "Strikes And Return, Useful";
                    hitSpeedText.text = "For Stunning Enemies";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Hourglass)
                {
                    weaponClassText.text = "Slows the Time Flow By";
                    hitSpeedText.text = "Half to Act More Precisely";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Shiruken)
                {
                    weaponClassText.text = "Several Quick Throwable Star Projectiles";
                }
                else if (activeItem.activeItemDetails.activeItemType == ActiveItemType.Pentagram)
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
        }

        if (hasWeaponDrop)
        {
            if (itemGeneric is Weapon)
            {
                Weapon weapon = (Weapon)itemGeneric;
                WeaponDetailsSO weaponDetails = weapon.weaponDetails;

                // Populate text field based on the related weapon info
                switch (weaponDetails.weaponLevel)
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

                Weapon equippedWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

                // Equipped
                if (equippedWeapon != null && weapon.weaponDetails.weaponClass != WeaponClass.Shield)
                {
                    switch (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponLevel)
                    {
                        case WeaponLevel.Basic:
                            headerTextEquipped.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                            break;
                        case WeaponLevel.Enchanted:
                            headerTextEquipped.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                            break;
                        case WeaponLevel.Mythic:
                            headerTextEquipped.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                            break;
                        case WeaponLevel.Legendary:
                            headerTextEquipped.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                            levelTextEquipped.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                            break;
                        default:
                            break;
                    }

                    equippedText.text = "Equipped";
                    headerTextEquipped.text = equippedWeapon.weaponDetails.weaponName;
                    levelTextEquipped.text = $"({equippedWeapon.weaponDetails.weaponLevel.ToString()})";
                    weaponClassTextEquipped.text = $"Class: {equippedWeapon.weaponDetails.weaponClass.ToString()}";
                    hitSpeedTextEquipped.text = $"Speed: {equippedWeapon.weaponDetails.weaponHitSpeed.ToString()}";
                    weaponWieldTextEquipped.text = $"Wield Type: {equippedWeapon.weaponDetails.wieldType.ToString()}";

                    if (equippedWeapon.weaponDetails.isMeleeWeapon)
                    {
                        damageTextEquipped.text = $"Damage: {equippedWeapon.weaponDetails.meleeDamageMin}-{equippedWeapon.weaponDetails.meleeDamageMax}";
                    }
                    else
                    {
                        damageTextEquipped.text = $"Damage: {equippedWeapon.weaponDetails.weaponCurrentProjectile.projectileDamageMin}-" +
                            $"{equippedWeapon.weaponDetails.weaponCurrentProjectile.projectileDamageMax}";
                    }

                    int dropWeaponDamageMax = weaponDetails.isMeleeWeapon ? weaponDetails.meleeDamageMax : weaponDetails.weaponCurrentProjectile.projectileDamageMax;
                    int equippedWeaponDamageMax = equippedWeapon.weaponDetails.isMeleeWeapon ? equippedWeapon.weaponDetails.meleeDamageMax :
                        equippedWeapon.weaponDetails.weaponCurrentProjectile.projectileDamageMax;

                    if (equippedWeaponDamageMax > dropWeaponDamageMax)
                    {
                        damageTextEquipped.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                        damageText.colorGradient = new VertexGradient(Color.red, Color.red, Color.red, Color.red);
                    }
                    else if (equippedWeaponDamageMax == dropWeaponDamageMax)
                    {
                        damageTextEquipped.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
                        damageText.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
                    }
                    else
                    {
                        damageTextEquipped.colorGradient = new VertexGradient(Color.red, Color.red, Color.red, Color.red);
                        damageText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                    }

                    baseHandlingTextEquipped.text = $"Base Handling: {equippedWeapon.weaponDetails.weaponBaseHandling * 100}%";
                    crHitChanceTextEquipped.text = $"Base Cr. Hit Chance: {equippedWeapon.weaponDetails.criticalHitChance * 100}%";

                    if (equippedWeapon.weaponDetails.isMeleeWeapon)
                    {
                        crHitDamageTextEquipped.text = $"Base Cr. Hit Damage: {(equippedWeapon.weaponDetails.criticalHitDamageMultiplier + player.additionalCriticalMeleeDamageModifier) * 100}%";
                    }
                    else
                    {
                        crHitDamageTextEquipped.text = $"Base Cr. Hit Damage: {equippedWeapon.weaponDetails.criticalHitDamageMultiplier * 100}%";
                    }

                    elementalBiasTextEquipped.text = "Elemental Bias:";

                    // Populate text field based on the related elemental info
                    switch (equippedWeapon.weaponDetails.elementalBias)
                    {
                        case ElementalBias.None:
                            elementTextEquipped.colorGradient = new VertexGradient(noneElementalColor1, noneElementalColor1, noneElementalColor2, noneElementalColor2);
                            break;
                        case ElementalBias.Fire:
                            elementTextEquipped.colorGradient = new VertexGradient(fireColor1, fireColor1, fireColor2, fireColor2);
                            break;
                        case ElementalBias.Water:
                            elementTextEquipped.colorGradient = new VertexGradient(waterColor1, waterColor1, waterColor2, waterColor2);
                            break;
                        case ElementalBias.Earth:
                            elementTextEquipped.colorGradient = new VertexGradient(earthColor1, earthColor1, earthColor2, earthColor2);
                            break;
                        case ElementalBias.Air:
                            elementTextEquipped.colorGradient = new VertexGradient(airColor1, airColor1, airColor2, airColor2);
                            break;
                        case ElementalBias.Dark:
                            elementTextEquipped.colorGradient = new VertexGradient(darkColor1, darkColor1, darkColor2, darkColor2);
                            break;
                        case ElementalBias.Light:
                            elementTextEquipped.colorGradient = new VertexGradient(lightColor1, lightColor1, lightColor2, lightColor2);
                            break;
                        default:
                            break;
                    }

                    elementTextEquipped.text = equippedWeapon.weaponDetails.elementalBias.ToString();
                    elementalForgeRateTextEquipped.text = $"El. Forge Rate: {equippedWeapon.weaponDetails.elementalForgeRate * 100}%";

                    switch (equippedWeapon.weaponDetails.weaponLevel)
                    {
                        case WeaponLevel.Basic:
                            masteryText1Equipped.gameObject.SetActive(false);
                            masteryText2Equipped.gameObject.SetActive(false);
                            masteryText3Equipped.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Enchanted:
                            masteryText1Equipped.gameObject.SetActive(true);
                            masteryText1Equipped.text = "Enchanted Mastery: Locked";
                            masteryText2Equipped.gameObject.SetActive(false);
                            masteryText3Equipped.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Mythic:
                            masteryText1Equipped.gameObject.SetActive(true);
                            masteryText1Equipped.text = "Enchanted Mastery: Locked";
                            masteryText2Equipped.gameObject.SetActive(true);
                            masteryText2Equipped.text = "Mythic Mastery: Locked";
                            masteryText3Equipped.gameObject.SetActive(false);
                            break;
                        case WeaponLevel.Legendary:
                            masteryText1Equipped.gameObject.SetActive(true);
                            masteryText1Equipped.text = "Enchanted Mastery: Locked";
                            masteryText2Equipped.gameObject.SetActive(true);
                            masteryText2Equipped.text = "Mythic Mastery: Locked";
                            masteryText3Equipped.gameObject.SetActive(true);
                            masteryText3Equipped.text = "Legendary Mastery: Locked";
                            break;
                        default:
                            break;

                    }
                }

                headerText.text = weaponDetails.weaponName;
                levelText.text = $"({weaponDetails.weaponLevel.ToString()})";

                requirementText.text = UpdateRequirementText(weaponDetails);
                weaponClassText.text = $"Class: {weaponDetails.weaponClass.ToString()}";

                if (weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    weaponWieldText.text = $"Wield Type: {weaponDetails.wieldType.ToString()}";
                    damageText.text = $"Deflect Rate: {weaponDetails.projectileDeflectRatio * 100}%";
                }
                else
                {
                    hitSpeedText.text = $"Speed: {weaponDetails.weaponHitSpeed.ToString()}";
                    weaponWieldText.text = $"Wield Type: {weaponDetails.wieldType.ToString()}";

                    if (weaponDetails.isMeleeWeapon)
                    {
                        damageText.text = $"Damage: {weaponDetails.meleeDamageMin}-{weaponDetails.meleeDamageMax}";
                    }
                    else
                    {
                        damageText.text = $"Damage: {weaponDetails.weaponCurrentProjectile.projectileDamageMin}-{weaponDetails.weaponCurrentProjectile.projectileDamageMax}";
                    }
                }

                baseHandlingText.text = $"Base Handling: {weaponDetails.weaponBaseHandling * 100}%";
                crHitChanceText.text = $"Base Cr. Hit Chance: {weaponDetails.criticalHitChance * 100}%";

                if (weaponDetails.isMeleeWeapon)
                {
                    crHitDamageText.text = $"Base Cr. Hit Damage: {(weaponDetails.criticalHitDamageMultiplier + player.additionalCriticalMeleeDamageModifier) * 100}%";
                }
                else
                {
                    crHitDamageText.text = $"Base Cr. Hit Damage: {weaponDetails.criticalHitDamageMultiplier * 100}%";
                }

                elementalBiasText.text = "Elemental Bias:";


                // Populate text field based on the related elemental info
                switch (weaponDetails.elementalBias)
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

                elementText.text = weaponDetails.elementalBias.ToString();
                elementalForgeRateText.text = $"El. Forge Rate: {weaponDetails.elementalForgeRate * 100}%";

                switch (weaponDetails.weaponLevel)
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

    /// <summary>
    /// Updates the requirement text based on the weapon's required stats.
    /// </summary>
    /// <param name="weaponDetails">The weapon details ScriptableObject.</param>
    public string UpdateRequirementText(WeaponDetailsSO weaponDetails)
    {
        PrimaryStats requiredStats = weaponDetails.requiredPrimaryStats;

        string requirementString = "Requirement: ";

        if (requiredStats.strength > 0)
        {
            requirementString += $"STR: {requiredStats.strength} ";
        }
        ;

        if (requiredStats.dexterity > 0) requirementString += $"DEX: {requiredStats.dexterity} ";

        if (requiredStats.constitution > 0) requirementString += $"CON: {requiredStats.constitution} ";

        if (requiredStats.intelligence > 0) requirementString += $"INT: {requiredStats.intelligence} ";

        if (requiredStats.agility > 0) requirementString += $"AGI: {requiredStats.agility} ";

        if ((requiredStats.strength > 0 && player.CurrentStrengthValue < requiredStats.strength) ||
            (requiredStats.dexterity > 0 && player.CurrentDexterityValue < requiredStats.dexterity) ||
            (requiredStats.constitution > 0 && player.CurrentConstitutionValue < requiredStats.constitution) ||
            (requiredStats.intelligence > 0 && player.CurrentIntelligenceValue < requiredStats.intelligence) ||
            (requiredStats.agility > 0 && player.CurrentAgilityValue < requiredStats.agility))
        {
            requirementText.color = Color.red;
        }
        else
        {
            requirementText.color = Color.green;
        }

        return requirementString;
    }

    private void ClearTooltipPanel()
    {
        currentTooltipSource = TooltipSource.None;

        foreach (Transform child in tooltipPanel.transform)
        {
            child.GetComponent<TextMeshProUGUI>().text = string.Empty;
        }
    }

    private void ClearTooltipEquippedPanel()
    {
        foreach (Transform child in tooltipPanelEquipped.transform)
        {
            child.GetComponent<TextMeshProUGUI>().text = string.Empty;
        }
    }

    public void CloseTooltipPanel()
    {
        tooltipPanel.SetActive(false);
    }

    public void CloseTooltipEquippedPanel()
    {
        tooltipPanelEquipped.SetActive(false);
    }
}
