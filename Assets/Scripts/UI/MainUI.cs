using TMPro;
using UnityEngine;
using System;
using System.Collections;
using Mirror;

public class MainUI : SingletonMonobehaviour<MainUI>
{
    Player player;

    [Header("TOOLTIP PANEL")]
    // Tooltip panel
    public GameObject tooltipPanel;
    public TMP_Text headerText;
    public TMP_Text levelText;
    public TMP_Text requirementText;
    public TMP_Text contentText;
    public TMP_Text bonusText;

    [Space(10)]
    [Header("TOOLTIP PANEL - FOR COMPARING WITH EQUIPPED ONE")]
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
    [SerializeField] GameObject tutorialPanel;

    [Space(10)]
    [Header("MINIMAP PANEL")]
    [SerializeField] GameObject minimapPanel;

    protected override void Awake()
    {
        base.Awake();
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        StaticEventHandler.OnRoomChanged -= StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnOverviewCameraToggled -= StaticEventHandler_OnOverviewCameraToggled;
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || player?.specialMoveEvent == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        StaticEventHandler.OnRoomChanged += StaticEventHandler_OnRoomChanged;
        StaticEventHandler.OnOverviewCameraToggled += StaticEventHandler_OnOverviewCameraToggled;
    }

    private void StaticEventHandler_OnRoomChanged(RoomChangedEventArgs args)
    {
        tooltipPanel.SetActive(false);
    }

    private void StaticEventHandler_OnOverviewCameraToggled(OverviewCameraFollowArgs args)
    {
        minimapPanel.SetActive(!args.isOn);
    }

    private void Start()
    {
        if (!NetworkServer.active && !NetworkClient.active)
        {
            tutorialPanel.SetActive(InputManager.TutorialEnabled);
        }
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

    public void UpdateTooltipPanelInfo(WeaponStats weaponStats, PassiveItemStats passiveStats, WeaponTitle weaponTitle, PassiveItemType passiveItemType, Rarity rarity, 
        bool hasWeaponDrop, bool hasSecondaryPassiveDrop, TooltipSource source)
    {
        if (player == null || !player.IsLocal) return;

        if (currentTooltipSource == source) return;

        currentTooltipSource = source;

        tooltipPanel.SetActive(true);

        Weapon equippedWeapon = player.activeWeapon.GetCurrentMainHandWeapon();

        if (hasWeaponDrop && equippedWeapon != null)
        {
            if (weaponStats.weaponClass != WeaponClass.Shield) tooltipPanelEquipped.SetActive(true);
        }

        ClearTooltipPanel();
        ClearTooltipEquippedPanel();

        //// NEW: also clear modifier text blocks
        //ClearModifierTexts();

        if (hasSecondaryPassiveDrop && passiveItemType != PassiveItemType.None)
        {
            ReshapeTooltip(isWeapon: false);

            BoostTypeColorUpdate(rarity);

            PassiveItem passiveItem = PassiveDropGenerator.GetPassiveWithStats(passiveStats, rarity, ItemSlotStatus.None, -1);

            headerText.text = passiveStats.passiveItemType.ToString();
            levelText.text = $"({passiveItem.Rarity.ToString()})";

            // HEAD
            //NECK
            // CHEST
            // FINGER
            // ARM
            // BACK
            // LEG

            BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseUniqueRolled, BoostPhase.Unique, bonusText);
            BoostForPassiveItem(passiveItem, passiveItem.passiveStats.baseTypeRolled, BoostPhase.Type, bonusText);
            BoostForPassiveItem(passiveItem, passiveItem.passiveStats.enchantedBoostType, BoostPhase.Enchanted, bonusText);
            BoostForPassiveItem(passiveItem, passiveItem.passiveStats.mythicBoostType, BoostPhase.Mythic, bonusText);
        }
        if (hasWeaponDrop && weaponTitle != WeaponTitle.None)
        {
            ReshapeTooltip(isWeapon: true);

            BoostTypeColorUpdate(rarity);

            Weapon weapon = WeaponDropGenerator.GetWeaponWithStats(weaponStats, rarity, ItemSlotStatus.None, -1);

            // Populate text field based on the related weapon info
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

            // Equipped
            if (equippedWeapon != null && weapon.weaponStats.weaponClass != WeaponClass.Shield)
            {
                switch (equippedWeapon.Rarity)
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
                headerTextEquipped.text = equippedWeapon.weaponStats.weaponTitle.ToString();
                levelTextEquipped.text = $"({equippedWeapon.Rarity.ToString()})\n";
                contentTextEquipped.text = $"Class: {equippedWeapon.weaponStats.weaponClass.ToString()}\n";

                float equippedFireRate = (float)Math.Round(1 / (equippedWeapon.weaponStats.weaponCooldownDuration - (equippedWeapon.weaponStats.attackCooldownModifier / 2)), 2);

                contentTextEquipped.text += $"Attack Speed: {equippedFireRate}\n";
                contentTextEquipped.text += $"Wield Type: {equippedWeapon.weaponStats.wieldType.ToString()}\n";
            }

            headerText.text = weapon.weaponStats.weaponTitle.ToString();
            levelText.text = $"({weapon.Rarity.ToString()})";
            contentText.text = $"Class: {weapon.weaponStats.weaponClass.ToString()}\n";

            if (weapon.weaponStats.weaponClass == WeaponClass.Shield)
            {
                contentText.text += $"Wield Type: {weapon.weaponStats.wieldType.ToString()}\n";
                contentText.text += $"Block Rate: {weapon.weaponStats.blockChance * 100}%\n";
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

                contentText.text += $"Attack Rating: {weapon.weaponStats.attackRatingIncrease * 100}\n";
                contentText.text += $"Cr. Hit Chance: {weapon.weaponStats.criticalHitChanceIncrease * 100}%\n";
                contentText.text += $"Cr. Hit Damage: {weapon.weaponStats.criticalHitDamageIncrease * 100}%\n";
            }

            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                contentTextEquipped.text += $"Phy. Damage: {equippedWeapon.weaponStats.physicalDamageMin + equippedWeapon.weaponStats.physicalAttackDamageIncrease}-" +
                    $"{equippedWeapon.weaponStats.physicalDamageMax + equippedWeapon.weaponStats.physicalAttackDamageIncrease}\n";

                contentTextEquipped.text += $"Magic Damage: {equippedWeapon.weaponStats.magicDamageMin + equippedWeapon.weaponStats.magicAttackDamageIncrease}-" +
                    $"{equippedWeapon.weaponStats.magicDamageMax + equippedWeapon.weaponStats.magicAttackDamageIncrease}\n";

                contentTextEquipped.text += $"Attack Rating: {equippedWeapon.weaponStats.attackRatingIncrease * 100}%\n";
                contentTextEquipped.text += $"Cr. Hit Chance: {equippedWeapon.weaponStats.criticalHitChanceIncrease * 100}%\n";
                contentTextEquipped.text += $"Cr. Hit Damage: {equippedWeapon.weaponStats.criticalHitDamageIncrease * 100}%\n";
            }

            BoostForWeapon(weapon, weapon.weaponStats.baseUniqueRolled, BoostPhase.Unique, bonusText);
            BoostForWeapon(weapon, weapon.weaponStats.baseTypeRolled, BoostPhase.Type, bonusText);
            BoostForWeapon(weapon, weapon.weaponStats.enchantedBoostType, BoostPhase.Enchanted, bonusText);
            BoostForWeapon(weapon, weapon.weaponStats.mythicBoostType, BoostPhase.Mythic, bonusText);

            if (equippedWeapon != null)
            {
                BoostForWeapon(equippedWeapon, equippedWeapon.weaponStats.baseUniqueRolled, BoostPhase.Unique, bonusTextEquipped);
                BoostForWeapon(equippedWeapon, equippedWeapon.weaponStats.baseTypeRolled, BoostPhase.Type, bonusTextEquipped);
                BoostForWeapon(equippedWeapon, equippedWeapon.weaponStats.enchantedBoostType, BoostPhase.Enchanted, bonusTextEquipped);
                BoostForWeapon(equippedWeapon, equippedWeapon.weaponStats.mythicBoostType, BoostPhase.Mythic, bonusTextEquipped);
            }
        }
    }

    private void ReshapeTooltip(bool isWeapon)
    {
        RectTransform tooltipRect = tooltipPanel.GetComponent<RectTransform>();

        if (isWeapon) tooltipRect.sizeDelta = new Vector2(tooltipRect.sizeDelta.x, 120);
        else tooltipRect.sizeDelta = new Vector2(tooltipRect.sizeDelta.x, 60);
    }

    private void BoostForWeapon(Weapon weapon, BoostType boostType, BoostPhase boostPhase, TMP_Text bonusText)
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
                        if(weapon.weaponStats.additionalPoisonChance > 0) bonusText.text = $"Poison Chance: + {weapon.weaponStats.additionalPoisonChance * 100}%";
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
                        bonusText.text += $"\nAttack Rating: + {weapon.weaponStats.attackRatingIncrease * 100}%";
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


    private void BoostForPassiveItem(PassiveItem passiveItem, BoostType boostType, BoostPhase boostPhase, TMP_Text bonusText)
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
                        bonusText.text = $"Attack Rating: + {passiveItem.passiveStats.attackRating * 100}%";
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
                        bonusText.text = $"Damage vs Low Health: + {passiveItem.passiveStats.damageVsLowHealthEnemies * 100}%";
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
                        bonusText.text = $"Skill Cooldown: + {passiveItem.passiveStats.skillCooldown * 100}%";
                        break;
                    case BoostType.SkillDuration:
                        bonusText.text = $"Skill Duration: + {passiveItem.passiveStats.skillDuration * 100}%";
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
                        bonusText.text += $"\nAttack Rating: + {passiveItem.passiveStats.attackRating * 100}%";
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
                        bonusText.text += $"\nDamage vs Low Health: + {passiveItem.passiveStats.damageVsLowHealthEnemies * 100}%";
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

    private void BoostTypeColorUpdate(Rarity rarity)
    {
        switch (rarity)
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

    private void ClearTooltipPanel()
    {
        currentTooltipSource = TooltipSource.None;

        foreach (Transform child in tooltipPanel.transform)
        {
            child.GetComponent<TMP_Text>().text = string.Empty;
        }
    }

    private void ClearTooltipEquippedPanel()
    {
        foreach (Transform child in tooltipPanelEquipped.transform)
        {
            child.GetComponent<TMP_Text>().text = string.Empty;
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
