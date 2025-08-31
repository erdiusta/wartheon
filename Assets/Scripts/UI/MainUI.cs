using TMPro;
using UnityEngine;

public class MainUI : SingletonMonobehaviour<MainUI>
{
    Player player;

    [Header("TOOLTIP PANEL")]
    [Space(10)]
    // Tooltip panel
    public GameObject tooltipPanel;
    public TMP_Text headerText;
    public TMP_Text levelText;
    public TMP_Text requirementText;
    public TMP_Text weaponClassText;
    public TMP_Text hitSpeedText;
    public TMP_Text weaponWieldText;
    public TMP_Text physicalDamageText;
    public TMP_Text magicDamageText;
    public TMP_Text attackRatingText;
    public TMP_Text crHitChanceText;
    public TMP_Text crHitDamageText;
    public TMP_Text enchantedBoostText;

    [Space(10)]
    // Tooltip panel equipped
    public GameObject tooltipPanelEquipped;
    public TMP_Text headerTextEquipped;
    public TMP_Text levelTextEquipped;
    public TMP_Text equippedText;
    public TMP_Text weaponClassTextEquipped;
    public TMP_Text hitSpeedTextEquipped;
    public TMP_Text weaponWieldTextEquipped;
    public TMP_Text physicalDamageTextEquipped;
    public TMP_Text magicDamageTextEquipped;
    public TMP_Text attackRatingTextEquipped;
    public TMP_Text crHitChanceTextEquipped;
    public TMP_Text crHitDamageTextEquipped;
    public TMP_Text enchantedBoostTextEquipped;


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

    public void UpdateTooltipPanelInfo(ItemGeneric itemGeneric, bool hasWeaponDrop, bool hasSecondaryPassiveDrop, TooltipSource source)
    {
        if (currentTooltipSource == source) return;

        currentTooltipSource = source;

        tooltipPanel.SetActive(true);

        Weapon equippedWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

        if (itemGeneric != null)
        {
            if (equippedWeapon != null)
            {
                if (itemGeneric is Weapon w && w.weaponDetails.weaponClass != WeaponClass.Shield)
                    tooltipPanelEquipped.SetActive(true);
            }
        }

        ClearTooltipPanel();
        ClearTooltipEquippedPanel();

        //// NEW: also clear modifier text blocks
        //ClearModifierTexts();

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
                //NECK
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
                // ARM
                // BACK
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                    weaponClassText.text = "+5% Cr. Hit Chance";
                    hitSpeedText.text = "+10% Cr. Hit Chance When";
                    weaponWieldText.text = "Dual-Wield Dagger or Claw Equipped";
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                {
                    weaponClassText.text = "+5% Elemental Damage";
                    hitSpeedText.text = "+15% Elemental Resistance";
                }
                // LEG

                BoostForPassiveItem(passiveItem, passiveItem.baseUniqueRolled, BoostPhase.Unique);
                BoostForPassiveItem(passiveItem, passiveItem.baseTypeRolled, BoostPhase.Type);
                BoostForPassiveItem(passiveItem, passiveItem.enchantedBoostType, BoostPhase.Enchanted);
                BoostForPassiveItem(passiveItem, passiveItem.mythicBoostType, BoostPhase.Mythic);
            }
        }
        if (hasWeaponDrop && itemGeneric is Weapon weapon)
        {
            WeaponDetailsSO weaponDetails = weapon.weaponDetails;

            // Populate text field based on the related weapon info
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

            // Equipped
            if (equippedWeapon != null && weapon.weaponDetails.weaponClass != WeaponClass.Shield)
            {
                switch (equippedWeapon.rarity)
                {
                    case Rarity.Basic:
                        headerTextEquipped.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                        levelTextEquipped.colorGradient = new VertexGradient(basicLevelColor1, basicLevelColor1, basicLevelColor2, basicLevelColor2);
                        break;
                    case Rarity.Enchanted:
                        headerTextEquipped.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                        levelTextEquipped.colorGradient = new VertexGradient(enchantedLevelColor1, enchantedLevelColor1, enchantedLevelColor2, enchantedLevelColor2);
                        break;
                    case Rarity.Mythic:
                        headerTextEquipped.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                        levelTextEquipped.colorGradient = new VertexGradient(mythicLevelColor1, mythicLevelColor1, mythicLevelColor2, mythicLevelColor2);
                        break;
                    case Rarity.Legendary:
                        headerTextEquipped.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                        levelTextEquipped.colorGradient = new VertexGradient(legendaryLevelColor1, legendaryLevelColor1, legendaryLevelColor2, legendaryLevelColor2);
                        break;
                    default:
                        break;
                }

                equippedText.text = "Equipped";
                headerTextEquipped.text = equippedWeapon.weaponDetails.weaponName;
                levelTextEquipped.text = $"({equippedWeapon.rarity.ToString()})";
                weaponClassTextEquipped.text = $"Class: {equippedWeapon.weaponDetails.weaponClass.ToString()}";
                hitSpeedTextEquipped.text = $"Speed: {equippedWeapon.attackCooldown.ToString()}";
                weaponWieldTextEquipped.text = $"Wield Type: {equippedWeapon.weaponDetails.wieldType.ToString()}";

                if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";
                    physicalDamageText.text = $"Block Rate: {weapon.weaponDetails.blockChance * 100}%";
                }
                else
                {
                    hitSpeedText.text = $"Speed: {weapon.weaponDetails.weaponHitSpeed.ToString()}";
                    weaponWieldText.text = $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}";

                    physicalDamageText.text = $"Phy. Damage: {weapon.weaponDetails.physicalDamageMin + weapon.physicalAttackDamageIncrease}-" +
                        $"{weapon.weaponDetails.physicalDamageMax + weapon.physicalAttackDamageIncrease}";

                    magicDamageText.text = $"Magic Damage: {weapon.weaponDetails.magicDamageMin + weapon.magicAttackDamageIncrease}-" +
                        $"{weapon.weaponDetails.magicDamageMax + weapon.magicAttackDamageIncrease}";
                }

                physicalDamageTextEquipped.text = $"Phy. Damage: {equippedWeapon.weaponDetails.physicalDamageMin + equippedWeapon.physicalAttackDamageIncrease}-" +
                    $"{equippedWeapon.weaponDetails.physicalDamageMax + equippedWeapon.physicalAttackDamageIncrease}";

                magicDamageTextEquipped.text = $"Phy. Damage: {equippedWeapon.weaponDetails.magicDamageMin + equippedWeapon.magicAttackDamageIncrease}-" +
                    $"{equippedWeapon.weaponDetails.magicDamageMax + equippedWeapon.magicAttackDamageIncrease}";

                int dropWeaponDamageMax = weaponDetails.isMeleeWeapon ? weaponDetails.physicalDamageMax : weaponDetails.weaponCurrentProjectile.projectilePhyDamageMax;
                int equippedWeaponDamageMax = equippedWeapon.weaponDetails.isMeleeWeapon ? equippedWeapon.weaponDetails.physicalDamageMax :
                    equippedWeapon.weaponDetails.weaponCurrentProjectile.projectilePhyDamageMax;

                if (equippedWeaponDamageMax > dropWeaponDamageMax)
                {
                    physicalDamageTextEquipped.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                    physicalDamageText.colorGradient = new VertexGradient(Color.red, Color.red, Color.red, Color.red);
                }
                else if (equippedWeaponDamageMax == dropWeaponDamageMax)
                {
                    physicalDamageTextEquipped.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
                    physicalDamageText.colorGradient = new VertexGradient(Color.yellow, Color.yellow, Color.yellow, Color.yellow);
                }
                else
                {
                    physicalDamageTextEquipped.colorGradient = new VertexGradient(Color.red, Color.red, Color.red, Color.red);
                    physicalDamageText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
                }

                attackRatingTextEquipped.text = $"Base Attack Rating: {equippedWeapon.attackRatingIncrease * 100}%";
                crHitChanceTextEquipped.text = $"Base Cr. Hit Chance: {equippedWeapon.criticalHitChanceIncrease * 100}%";
                crHitDamageTextEquipped.text = $"Base Cr. Hit Damage: {equippedWeapon.criticalHitDamageIncrease * 100}%";
            }

            headerText.text = weaponDetails.weaponName;
            levelText.text = $"({weapon.rarity.ToString()})";

            requirementText.text = UpdateRequirementText(weaponDetails);
            weaponClassText.text = $"Class: {weaponDetails.weaponClass.ToString()}";

            if (weaponDetails.weaponClass == WeaponClass.Shield)
            {
                weaponWieldText.text = $"Wield Type: {weaponDetails.wieldType.ToString()}";
                physicalDamageText.text = $"Block Rate: {weapon.blockChanceIncrease * 100}%";
            }
            else
            {
                hitSpeedText.text = $"Speed: {weapon.attackCooldown.ToString()}";
                weaponWieldText.text = $"Wield Type: {weaponDetails.wieldType.ToString()}";

                physicalDamageText.text = $"Phy. Damage: {weapon.weaponDetails.physicalDamageMin + weapon.physicalAttackDamageIncrease}-" +
                    $"{weapon.weaponDetails.physicalDamageMax + weapon.physicalAttackDamageIncrease}";

                magicDamageText.text = $"Magic Damage: {weapon.weaponDetails.magicDamageMin + weapon.magicAttackDamageIncrease}-" +
                    $"{weapon.weaponDetails.magicDamageMax + weapon.magicAttackDamageIncrease}";
            }

            attackRatingText.text = $"Base Handling: {weapon.attackRatingIncrease * 100}%";
            crHitChanceText.text = $"Base Cr. Hit Chance: {weapon.criticalHitChanceIncrease * 100}%";
            crHitDamageText.text = $"Base Cr. Hit Damage: {weapon.criticalHitDamageIncrease * 100}%";

            BoostForWeapon(weapon, weapon.baseUniqueRolled, BoostPhase.Unique);
            BoostForWeapon(weapon, weapon.baseTypeRolled, BoostPhase.Type);
            BoostForWeapon(weapon, weapon.enchantedBoostType, BoostPhase.Enchanted);
            BoostForWeapon(weapon, weapon.mythicBoostType, BoostPhase.Mythic);

            //// NEW: Modifiers for the DROP item
            //SetModifierBlock(weapon, modifiersHeaderText, modifierBaseUniqueText, modifierBaseTypeText, modifierExtra1Text, modifierExtra2Text);

            ////  NEW: Modifiers for the EQUIPPED item (if visible and not a shield)
            //equippedWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
            //if (equippedWeapon != null && weaponDetails.weaponClass != WeaponClass.Shield)
            //{
            //    SetModifierBlock(equippedWeapon, modifiersHeaderTextEquipped, modifierBaseUniqueTextEquipped, modifierBaseTypeTextEquipped,
            //        modifierExtra1TextEquipped,modifierExtra2TextEquipped);
            //}
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


    /// <summary>
    /// Updates the requirement text based on the weapon's required stats.
    /// </summary>
    /// <param name="weaponDetails">The weapon details ScriptableObject.</param>
    public string UpdateRequirementText(WeaponDetailsSO weaponDetails)
    {
        PrimaryStats requiredStats = weaponDetails.requiredPrimaryStats;

        string requirementString = "Required Char: ";

        if (requiredStats.strength > 0)
        {
            requirementString += $"STR: {requiredStats.strength} ";
        };

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

    //// Clear helper
    //private void ClearModifierTexts()
    //{
    //    if (modifiersHeaderText != null)
    //    {
    //        modifiersHeaderText.gameObject.SetActive(false);
    //        modifierBaseUniqueText.gameObject.SetActive(false);
    //        modifierBaseTypeText.gameObject.SetActive(false);
    //        modifierExtra1Text.gameObject.SetActive(false);
    //        modifierExtra2Text.gameObject.SetActive(false);
    //    }

    //    if (modifiersHeaderTextEquipped != null)
    //    {
    //        modifiersHeaderTextEquipped.gameObject.SetActive(false);
    //        modifierBaseUniqueTextEquipped.gameObject.SetActive(false);
    //        modifierBaseTypeTextEquipped.gameObject.SetActive(false);
    //        modifierExtra1TextEquipped.gameObject.SetActive(false);
    //        modifierExtra2TextEquipped.gameObject.SetActive(false);
    //    }
    //}

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
