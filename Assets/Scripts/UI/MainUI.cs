using TMPro;
using UnityEngine;
using System;
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
    public TMP_Text contentText;
    public TMP_Text bonusText;

    [Space(10)]
    // Tooltip panel equipped
    public GameObject tooltipPanelEquipped;
    public TMP_Text headerTextEquipped;
    public TMP_Text levelTextEquipped;
    public TMP_Text equippedText;
    public TMP_Text contentTextEquipped;
    public TMP_Text bonusTextEquipped;


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

                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.EmeraldPendant)
                {

                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.TopazPendant)
                {

                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.SapphirePendant)
                {

                }
                // CHEST
                // FINGER
                // ARM
                // BACK
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.ShadowCloak)
                {
                }
                else if (passiveItem.passiveItemDetails.passiveItemType == PassiveItemType.MantleOfStars)
                {

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
                levelTextEquipped.text = $"({equippedWeapon.rarity.ToString()})\n";
                contentTextEquipped.text = $"Class: {equippedWeapon.weaponDetails.weaponClass.ToString()}\n";

                float equippedFireRate = (float)Math.Round(1 / (equippedWeapon.weaponDetails.weaponCooldownDuration - equippedWeapon.attackCooldownModifier), 2);
                contentTextEquipped.text += $"Attack Speed: {equippedFireRate}\n";

                contentTextEquipped.text += $"Wield Type: {equippedWeapon.weaponDetails.wieldType.ToString()}\n";

                if (weapon.weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    contentText.text += $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}\n";
                    contentText.text += $"Block Rate: {weapon.weaponDetails.blockChance * 100}%\n";
                }
                else
                {
                    float fireRate = (float)Math.Round(1 / (weapon.weaponDetails.weaponCooldownDuration - weapon.attackCooldownModifier), 2);
                    contentText.text += $"Attack Speed: {fireRate}\n";

                    contentText.text += $"Wield Type: {weapon.weaponDetails.wieldType.ToString()}\n";

                    contentText.text += $"Phy. Damage: {weapon.weaponDetails.physicalDamageMin + weapon.physicalAttackDamageIncrease}-" +
                        $"{weapon.weaponDetails.physicalDamageMax + weapon.physicalAttackDamageIncrease}\n";

                    contentText.text += $"Magic Damage: {weapon.weaponDetails.magicDamageMin + weapon.magicAttackDamageIncrease}-" +
                        $"{weapon.weaponDetails.magicDamageMax + weapon.magicAttackDamageIncrease}\n";
                }

                contentTextEquipped.text += $"Phy. Damage: {equippedWeapon.weaponDetails.physicalDamageMin + equippedWeapon.physicalAttackDamageIncrease}-" +
                    $"{equippedWeapon.weaponDetails.physicalDamageMax + equippedWeapon.physicalAttackDamageIncrease}\n";

                contentTextEquipped.text += $"Phy. Damage: {equippedWeapon.weaponDetails.magicDamageMin + equippedWeapon.magicAttackDamageIncrease}-" +
                    $"{equippedWeapon.weaponDetails.magicDamageMax + equippedWeapon.magicAttackDamageIncrease}\n";

                contentTextEquipped.text += $"Attack Rating: {equippedWeapon.attackRatingIncrease * 100}%\n";
                contentTextEquipped.text += $"Cr. Hit Chance: {equippedWeapon.criticalHitChanceIncrease * 100}%\n";
                contentTextEquipped.text += $"Cr. Hit Damage: {equippedWeapon.criticalHitDamageIncrease * 100}%\n";
            }

            headerText.text = weaponDetails.weaponName;
            levelText.text = $"({weapon.rarity.ToString()})";

            contentText.text = $"Class: {weaponDetails.weaponClass.ToString()}";

            if (weaponDetails.weaponClass == WeaponClass.Shield)
            {
                contentText.text += $"Wield Type: {weaponDetails.wieldType.ToString()}\n";
                contentText.text += $"Block Rate: {(weapon.weaponDetails.blockChance + weapon.blockChanceIncrease) * 100}%\n";
            }
            else
            {
                contentText.text += $"Speed: {weapon.attackCooldownModifier.ToString()}\n";
                contentText.text += $"Wield Type: {weaponDetails.wieldType.ToString()}\n";

                contentText.text += $"Phy. Damage: {weapon.weaponDetails.physicalDamageMin + weapon.physicalAttackDamageIncrease}-" +
                    $"{weapon.weaponDetails.physicalDamageMax + weapon.physicalAttackDamageIncrease}\n";

                contentText.text += $"Magic Damage: {weapon.weaponDetails.magicDamageMin + weapon.magicAttackDamageIncrease}-" +
                    $"{weapon.weaponDetails.magicDamageMax + weapon.magicAttackDamageIncrease}\n";
            }

            float updatedAttackRating = (float)Math.Round(weapon.weaponDetails.weaponAttackRating * weapon.attackRatingIncrease, 2);
            contentText.text += $"Attack Rating: {updatedAttackRating * 100}%\n";

            contentText.text += $"Cr. Hit Chance: {weapon.criticalHitChanceIncrease * 100}%\n";
            contentText.text += $"Cr. Hit Damage: {weapon.criticalHitDamageIncrease * 100}%\n";

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
                        bonusText.text = $"Attack Speed: + {weapon.attackCooldownModifier * 100}%";
                        break;
                    case BoostType.AttackDamage:
                        bonusText.text = "Phy. Attack Dmg.: + " + weapon.physicalAttackDamageIncrease;
                        break;
                    case BoostType.AttackRating:
                        bonusText.text = $"Attack Rating: + {weapon.attackRatingIncrease * 100}%";
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
                        bonusText.text = $"Cr. Resistance: + {weapon.criticalResistanceModifier}%";
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
                        if(weapon.additionalPoisonChance > 0) bonusText.text = $"Poison Chance: + {weapon.additionalPoisonChance * 100}%";
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
                        bonusText.text += $"\nAttack Rating: + {weapon.attackRatingIncrease * 100}%";
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
                        bonusText.text += $"\nCr. Resistance: + {weapon.criticalResistanceModifier}%";
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
                        bonusText.text = $"Attack Rating: + {passiveItem.attackRating * 100}%";
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
                        bonusText.text = $"Damage vs Low Health: + {passiveItem.damageVsLowHealthEnemies * 100}%";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text = $"Cr. Resistance: + {passiveItem.criticalResistanceModifier}%";
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
                        bonusText.text = $"Skill Cooldown: + {passiveItem.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text = $"Skill Duration: + {passiveItem.skillDuration * 100}%";
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
                        bonusText.text += $"\nAttack Rating: + {passiveItem.attackRating * 100}%";
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
                        bonusText.text += $"\nDamage vs Low Health: + {passiveItem.damageVsLowHealthEnemies * 100}%";
                        break;
                    case BoostType.CritResistance:
                        bonusText.text += $"\nCr. Resistance: + {passiveItem.criticalResistanceModifier}%";
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
