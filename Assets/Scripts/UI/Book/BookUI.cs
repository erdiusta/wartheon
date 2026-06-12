using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Localization.Settings;
using UnityEngine.UI;

public class BookUI : SingletonMonobehaviour<BookUI>, ISelectHandler, IDeselectHandler
{
    public static bool IsBookOpen { get; set; }

    [Header("PAGE HEADERS")]
    [Space(10)]
    public Transform skillPage;
    public Transform statsPage;
    public Transform beastiaryPage;
    public Transform bossesPage;

    [Space(10)]
    public Button weaponSetOneButton;
    public Button weaponSetTwoButton;
    public Button weaponSetThreeButton;
    public Button dropButton;

    [SerializeField] TMP_Text characterName;
    [SerializeField] Image characterImage;
    Image characterSeparatorImage;

    [Space(10)]
    [Header("PRIMARY STATS")]
    [SerializeField] TMP_Text strengthValue;
    [SerializeField] TMP_Text constitutionValue;
    [SerializeField] TMP_Text dexterityValue;
    [SerializeField] TMP_Text intelligenceValue;
    [SerializeField] TMP_Text willpowerValue;
    [SerializeField] TMP_Text agilityValue;
    [SerializeField] TMP_Text resolveValue;
    [SerializeField] TMP_Text ferocityValue;

    [Space(10)]
    [Header("PRIMARY STATS BUTTONS")]
    public Transform primaryStatsButtonContainer;
    [SerializeField] TMP_Text currentAvailableStatPoints;

    [Space(10)]
    [Header("COINS AND SHARDS")]
    [SerializeField] TMP_Text coinsText;
    [SerializeField] TMP_Text shardText;

    [Space(10)]
    [Header("SECONDARY STATS")]
    [SerializeField] TMP_Text damageValue;
    [SerializeField] TMP_Text healthValue;
    [SerializeField] TMP_Text manaValue;
    [SerializeField] TMP_Text dodgeRateValue;
    [SerializeField] TMP_Text blockRateValue;
    [SerializeField] TMP_Text speedValue;
    [SerializeField] TMP_Text criticalHitChanceDamageValue;
    [SerializeField] TMP_Text criticalHitDamageAmountValue;

    [Space(10)]
    [Header("AUXILLARY STATS")]
    [SerializeField] TMP_Text attackCooldownValue;
    [SerializeField] TMP_Text attackRatingValue;
    [SerializeField] TMP_Text skillCooldownModifierValue;
    [SerializeField] TMP_Text skillDurationModifierValue;
    [SerializeField] TMP_Text lifeStealValue;
    [SerializeField] TMP_Text damageReductionValue;
    [SerializeField] TMP_Text damageVsLowHealthValue;
    [SerializeField] TMP_Text armorPenetrationValue;

    [Space(10)]
    [Header("RESISTANCE STATS")]
    [SerializeField] TMP_Text armorText;
    [SerializeField] TMP_Text magicResistanceText;
    [SerializeField] TMP_Text statusResistanceText;
    [SerializeField] TMP_Text criticalResistanceText;

    [Space(10)]
    [SerializeField] Animator bookAnimator;
    [SerializeField] Transform mainHandWeaponSlot;
    [SerializeField] Transform offHandWeaponSlot;

    [Header("PASSIVE ITEM SLOTS")]
    Transform passiveItemHeadSlot;
    Transform passiveItemChestSlot;
    Transform passiveItemNeckSlot;
    Transform passiveItemArmSlot;
    Transform passiveItemBackSlot;
    Transform passiveItemLegSlot;
    Transform passiveItemFingerSlot;

    [Header("INVENTORY ITEM SLOTS")]
    [SerializeField] Transform inventoryParent;

    [Space(10)]
    [Header("SKILLS&INNER PATH")]
    [SerializeField] Transform innerPathContainer;
    [SerializeField] Transform skillDescriptonInnerPanel;
    [SerializeField] Transform skillPointsTransform;
    [SerializeField] Transform innerPathTextContainer;
    [SerializeField] TMP_Text skillDescriptionText;
    [SerializeField] TMP_Text skillDetailsText;

    // SLOT TRANSFORMS
    Transform mainHandWeaponBackground;
    Transform mainHandWeaponEquipped;
    Transform offHandWeaponBackground;
    Transform offHandWeaponEquipped;

    // BEASTIARY
    [Space(10)]
    [SerializeField] TMP_Text mobTitleText;
    [SerializeField] TMP_Text mobDetailsText;
    [SerializeField] Transform beastiaryImageContainer;

    // BOSSES
    [SerializeField] TMP_Text bossTitleText;
    [SerializeField] TMP_Text bossDetailsText;
    [SerializeField] Transform bossImageContainer;

    // TOOLTIP PANEL
    [Space(10)]
    public TooltipManager tooltipManager;

    Player player;

    protected override void Awake()
    {
        base.Awake();

        characterSeparatorImage = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();

        // Passive Item Slots
        passiveItemHeadSlot = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(0);
        passiveItemChestSlot = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(1);
        passiveItemNeckSlot = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(2);
        passiveItemArmSlot = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(3);
        passiveItemFingerSlot = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(4);
        passiveItemBackSlot = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(5);
        passiveItemLegSlot = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(6);

        // Slot transforms
        mainHandWeaponBackground = mainHandWeaponSlot.GetChild(0);
        mainHandWeaponEquipped = mainHandWeaponSlot.GetChild(1);
        offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
        offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);
    }

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        Unsubscribe();
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || player?.specialMoveEvent == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        characterSeparatorImage.sprite = player.playerDetails.playerMiniMapIcon;
        characterImage.sprite = player.playerDetails.playerBookSprite;

        UpdatePlayerStatInfo(player);
        currentAvailableStatPoints.text = player.currentStatPoints.ToString();

        skillDescriptionText.text = string.Empty;
        skillDetailsText.text = string.Empty;

        Subscribe();
        SetupStartingWeapons(player);
    }

    private void SetupStartingWeapons(Player player)
    {
        DisableBackgroundEnableEquippedTransform();

        switch (player.playerDetails.playerCharacterIndex)
        {
            case Character.Caelion: case Character.Morven: case Character.Karnag: case Character.Nyxa:
                PlaceMainHand(player, 0);
                PlaceOffHand(player, 1);
                break;
            case Character.Nyveran: case Character.Mycara: case Character.Kynara: case Character.Nymara:
                PlaceMainHand(player, 0);
                PlaceLockIcon();
                break;
            default:
                break;
        }
    }

    private void PlaceMainHand(Player player, int index)
    {
        GameObject go = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponEquipped);
        go.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[index].weaponFrontSprite;
    }

    private void PlaceOffHand(Player player, int index)
    {
        DisableBackgroundEnableEquippedTransform(true);
        GameObject go = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
        go.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[index].weaponFrontSprite;
    }

    private void Subscribe()
    {
        StaticEventHandler.OnStatPageOpened += StaticEventHandler_OnStatPageOpened;
        StaticEventHandler.OnBuildPageOpened += StaticEventHandler_OnBuildPageOpened;

        // BOOK STAT POINTS
        StaticEventHandler.OnStatPointChanged += StaticEventHandler_OnStatPointChanged;

        // BOOK COIN&SHARD AMOUNT
        if (player != null && player.consumableEvent != null)
        {
            player.consumableEvent.OnCoinCountChanged += ConsumableEvent_OnCoinCountChanged;
            player.consumableEvent.OnShardCountChanged += ConsumableEvent_OnShardCountChanged;
        }

        player.consumableEvent.OnCoinCountChanged += ConsumableEvent_OnCoinCountChanged;
        player.consumableEvent.OnShardCountChanged += ConsumableEvent_OnShardCountChanged;

        // BOOK WEAPON EVENTS
        StaticEventHandler.OnWeaponPickedUp += StaticEventHandler_OnWeaponPickedUp;
        StaticEventHandler.OnWeaponSwitchedInventory += StaticEventHandler_OnWeaponSwitchedInventory;
        StaticEventHandler.OnWeaponSwitched += StaticEventHandler_OnWeaponSwitched;
        StaticEventHandler.OnWeaponDropped += StaticEventHandler_OnWeaponDropped;        

        // BOOK INVENTORY EVENTS
        StaticEventHandler.OnWeaponRemoved += StaticEventHandler_OnWeaponRemoved;
        StaticEventHandler.OnInventoryItemRemoved += StaticEventHandler_OnInventoryItemRemoved;

        StaticEventHandler.OnWeaponAddedToInventory += StaticEventHandler_OnWeaponAddedToInventory;
        StaticEventHandler.OnPassiveItemAddedToInventorySlot += StaticEventHandler_OnPassiveItemAddedToInventorySlot;
        StaticEventHandler.OnInventoryWeaponDropped += StaticEventHandler_OnInventoryWeaponDropped;
        StaticEventHandler.OnInventoryPassiveItemDropped += StaticEventHandler_OnInventoryPassiveItemDropped;
        StaticEventHandler.OnPassiveItemsSwapped += StaticEventHandler_OnPassiveItemsSwapped;
        StaticEventHandler.OnWeaponsSwappedWithInventory += StaticEventHandler_OnWeaponsSwappedWithInventory;
        StaticEventHandler.OnGenericItemsSwappedInInventory += StaticEventHandler_OnGenericItemsSwappedInInventory;
        StaticEventHandler.OnGenericItemPlacedToEmptyInventory += StaticEventHandler_OnGenericItemPlacedToEmptyInventory;

        // WEAPON&PASSIVE UPGRADED
        StaticEventHandler.OnInventoryWeaponUpgraded += StaticEventHandler_OnInventoryWeaponUpgraded;
        StaticEventHandler.OnInventoryPassiveUpgraded += StaticEventHandler_OnInventoryPassiveUpgraded;

        // UNIQUE SKILL AND INNER PATH EVENTS
        StaticEventHandler.OnUniqueSkillInfoHovered += StaticEventHandler_OnUniqueSkillInfoHovered;
        StaticEventHandler.OnUniqueSkillInfoUnhovered += StaticEventHandler_OnUniqueSkillInfoUnhovered;
        StaticEventHandler.OnInnerPathInfoHovered += StaticEventHandler_OnBuildInfoHovered;
        StaticEventHandler.OnInnerPathInfoUnhovered += StaticEventHandler_OnBuildInfoUnhovered;

        // BEASTIARY EVENTS
        StaticEventHandler.OnMobUnlocked += StaticEventHandler_OnMobUnlocked;
        StaticEventHandler.OnMobHovered += StaticEventHandler_OnMobHovered;
        StaticEventHandler.OnMobUnhovered += StaticEventHandler_OnMobUnhovered;

        StaticEventHandler.OnBookHealthChanged += StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnBookManaChanged += StaticEventHandler_OnBookManaChanged;
        StaticEventHandler.OnItemAddedToPassiveItemSlot += StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot += StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
        StaticEventHandler.OnSkillPointUsed += StaticEventHandler_OnInnerPathPointUsed;
        StaticEventHandler.OnSkillBoostUsed += StaticEventHandler_OnSkillBoostUsed;
        StaticEventHandler.OnLevelUp += StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnPrimaryStatsChanged += StaticEventHandler_OnPrimaryStatsChanged;
    }

    private void Unsubscribe()
    {
        StaticEventHandler.OnStatPageOpened -= StaticEventHandler_OnStatPageOpened;
        StaticEventHandler.OnBuildPageOpened -= StaticEventHandler_OnBuildPageOpened;

        // BOOK STAT POINTS
        StaticEventHandler.OnStatPointChanged -= StaticEventHandler_OnStatPointChanged;

        // BOOK COIN&SHARD AMOUNT
        if (player != null && player.consumableEvent != null)
        {
            player.consumableEvent.OnCoinCountChanged -= ConsumableEvent_OnCoinCountChanged;
            player.consumableEvent.OnShardCountChanged -= ConsumableEvent_OnShardCountChanged;
        }

        // BOOK WEAPON EVENTS
        StaticEventHandler.OnWeaponPickedUp -= StaticEventHandler_OnWeaponPickedUp;
        StaticEventHandler.OnWeaponSwitchedInventory -= StaticEventHandler_OnWeaponSwitchedInventory;
        StaticEventHandler.OnWeaponSwitched -= StaticEventHandler_OnWeaponSwitched;
        StaticEventHandler.OnWeaponDropped -= StaticEventHandler_OnWeaponDropped;

        // BOOK INVENTORY EVENTS
        StaticEventHandler.OnWeaponRemoved -= StaticEventHandler_OnWeaponRemoved;
        StaticEventHandler.OnInventoryItemRemoved -= StaticEventHandler_OnInventoryItemRemoved;

        StaticEventHandler.OnWeaponAddedToInventory -= StaticEventHandler_OnWeaponAddedToInventory;
        StaticEventHandler.OnPassiveItemAddedToInventorySlot -= StaticEventHandler_OnPassiveItemAddedToInventorySlot;
        StaticEventHandler.OnInventoryWeaponDropped -= StaticEventHandler_OnInventoryWeaponDropped;
        StaticEventHandler.OnInventoryPassiveItemDropped -= StaticEventHandler_OnInventoryPassiveItemDropped;
        StaticEventHandler.OnPassiveItemsSwapped -= StaticEventHandler_OnPassiveItemsSwapped;
        StaticEventHandler.OnWeaponsSwappedWithInventory -= StaticEventHandler_OnWeaponsSwappedWithInventory;
        StaticEventHandler.OnGenericItemsSwappedInInventory -= StaticEventHandler_OnGenericItemsSwappedInInventory;
        StaticEventHandler.OnGenericItemPlacedToEmptyInventory -= StaticEventHandler_OnGenericItemPlacedToEmptyInventory;

        // WEAPON&PASSIVE UPGRADED
        StaticEventHandler.OnInventoryWeaponUpgraded -= StaticEventHandler_OnInventoryWeaponUpgraded;
        StaticEventHandler.OnInventoryPassiveUpgraded -= StaticEventHandler_OnInventoryPassiveUpgraded;

        // UNIQUE SKILL AND INNER PATH EVENTS
        StaticEventHandler.OnUniqueSkillInfoHovered -= StaticEventHandler_OnUniqueSkillInfoHovered;
        StaticEventHandler.OnUniqueSkillInfoUnhovered -= StaticEventHandler_OnUniqueSkillInfoUnhovered;
        StaticEventHandler.OnInnerPathInfoHovered -= StaticEventHandler_OnBuildInfoHovered;
        StaticEventHandler.OnInnerPathInfoUnhovered -= StaticEventHandler_OnBuildInfoUnhovered;

        // BEASTIARY EVENTS
        StaticEventHandler.OnMobUnlocked -= StaticEventHandler_OnMobUnlocked;
        StaticEventHandler.OnMobHovered -= StaticEventHandler_OnMobHovered;
        StaticEventHandler.OnMobUnhovered -= StaticEventHandler_OnMobUnhovered;

        StaticEventHandler.OnBookHealthChanged -= StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnBookManaChanged -= StaticEventHandler_OnBookManaChanged;
        StaticEventHandler.OnItemAddedToPassiveItemSlot -= StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot -= StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
        StaticEventHandler.OnSkillPointUsed -= StaticEventHandler_OnInnerPathPointUsed;
        StaticEventHandler.OnSkillBoostUsed -= StaticEventHandler_OnSkillBoostUsed;
        StaticEventHandler.OnLevelUp -= StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnPrimaryStatsChanged -= StaticEventHandler_OnPrimaryStatsChanged;
    }

    private void Update()
    {
        if (player == null) return;

        if (player.currentStatPoints > 0) primaryStatsButtonContainer.gameObject.SetActive(true);
        else primaryStatsButtonContainer.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData) { }

    public void OnDeselect(BaseEventData eventData) { }

    private void StaticEventHandler_OnStatPageOpened()
    {
        OpenStatsPage();
    }

    private void StaticEventHandler_OnBuildPageOpened()
    {
        OpenBuildPage();
    }

    private void StaticEventHandler_OnBuildInfoHovered(SkillPointsArgs skillPointsArgs)
    {
        skillDescriptionText.text = skillPointsArgs.innerPathDetails.innerPathName;
        skillDetailsText.text = skillPointsArgs.innerPathDetails.innerPathDetails;
    }

    private void StaticEventHandler_OnBuildInfoUnhovered(SkillPointsArgs skillPointsArgs)
    {
        skillDescriptionText.text = string.Empty;
        skillDetailsText.text = string.Empty;
    }

    private void StaticEventHandler_OnUniqueSkillInfoHovered(SkillPointsArgs skillPointsArgs)
    {
        //skillDescriptionText.text = skillPointsArgs.activeUniqueSkillContainer.activeUniqueSkillName;

        RefreshLocalizedTexts(player, skillPointsArgs.activeUniqueSkillContainer.activeSkill, skillPointsArgs.activeUniqueSkillContainer.passiveSkill);

        skillDetailsText.text = skillPointsArgs.activeUniqueSkillContainer.levels[skillPointsArgs.activeUniqueSkillContainer.GetCurrentActiveLevel() - 1].
            activeUniqueSkillDetails;
    }

    private void StaticEventHandler_OnUniqueSkillInfoUnhovered(SkillPointsArgs skillPointsArgs)
    {
        skillDescriptionText.text = string.Empty;
        skillDetailsText.text = string.Empty;
    }

    private void StaticEventHandler_OnWeaponRemoved(WeaponAddedToBookArgs args)
    {
        if (args.weapon == null) return;

        StartCoroutine(WeaponRemoveRoutine(args.slotType));
    }

    IEnumerator WeaponRemoveRoutine(SlotType slotType)
    {
        yield return new WaitForEndOfFrame();

        if (slotType == SlotType.WeaponMainHand)
        {
            EmptyMainHandEquippedSlot();
            EnableBackgroundDisableEquippedTransform(offHand: false);
        }
        else if (slotType == SlotType.WeaponOffHand)
        {
            EmptyOffhandEquippedSlot();
            EnableBackgroundDisableEquippedTransform(offHand: true);
        }
    }

    private void StaticEventHandler_OnInventoryItemRemoved(WeaponAddedToBookArgs args)
    {
        StartCoroutine(InventoryItemRemoveRoutine(args.inventoryIndexNumber));
    }

    IEnumerator InventoryItemRemoveRoutine(int index)
    {
        yield return new WaitForEndOfFrame();

        Transform equippedInventorySlot = inventoryParent.GetChild(index);

        Transform inventoryItemBackground = equippedInventorySlot.GetChild(0);
        Transform inventoryItemEquipped = equippedInventorySlot.GetChild(1);

        for (int i = inventoryItemEquipped.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryItemEquipped.GetChild(i).gameObject);
        }

        inventoryItemBackground.gameObject.SetActive(true);
        inventoryItemEquipped.gameObject.SetActive(false);
    }

    private void StaticEventHandler_OnWeaponPickedUp(WeaponAddedToBookArgs args)
    {
        StartCoroutine(WeaponPickUpRoutine(args.pickedUpByOffHand, args.weapon));
    }

    IEnumerator WeaponPickUpRoutine(bool pickedUpByOffHand, Weapon weapon)
    {
        yield return new WaitForEndOfFrame();

        UpdatePlayerStatInfo(player);

        if (pickedUpByOffHand)
        {
            DisableBackgroundEnableEquippedTransform(true);
            EmptyOffhandEquippedSlot();
            PlaceWeaponIconToOffhand(weapon);
        }
        else
        {
            DisableBackgroundEnableEquippedTransform();
            EmptyMainHandEquippedSlot();
            PlaceWeaponIconToMainHand(weapon);

            if (weapon.weaponStats.wieldType == WieldType.TwoHanded)
            {
                PlaceLockIcon();
            }
        }
    }

    private void StaticEventHandler_OnWeaponAddedToInventory(WeaponAddedToBookArgs args)
    {
        StartCoroutine(InventoryWeaponAddRoutine(args.inventoryIndexNumber, args.weapon));
    }

    IEnumerator InventoryWeaponAddRoutine(int index, Weapon weapon)
    {
        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);
        Sprite weaponSprite = weaponDetails.weaponFrontSprite;

        yield return new WaitForEndOfFrame();

        Transform equippedInventorySlot = inventoryParent.GetChild(index);
        equippedInventorySlot.GetComponent<Slot>().slotType = SlotType.Inventory;

        Transform inventoryItemBackground = equippedInventorySlot.GetChild(0);
        Transform inventoryItemEquipped = equippedInventorySlot.GetChild(1);
        inventoryItemBackground.gameObject.SetActive(false);
        inventoryItemEquipped.gameObject.SetActive(true);

        GameObject inventoryItem = Instantiate(GameResources.Instance.bookWeaponSlot, inventoryItemEquipped);

        // Set Draggable Item
        DraggableItem inventoryDraggableItem = inventoryItem.GetComponent<DraggableItem>();

        inventoryDraggableItem.SetDraggableItem(weapon, equippedInventorySlot.GetComponent<Slot>(), weaponSprite, ItemSlotStatus.Inventory);
    }

    private void StaticEventHandler_OnInventoryWeaponDropped(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        StartCoroutine(InventoryWeaponDropRoutine(weaponAddedToBookArgs.inventoryIndexNumber));
    }

    IEnumerator InventoryWeaponDropRoutine(int index)
    {
        yield return new WaitForEndOfFrame();

        Transform equippedInventroySlot = inventoryParent.GetChild(index);
        equippedInventroySlot.GetComponent<Slot>().slotType = SlotType.Inventory; // Reset inventory slot

        Transform inventoryItemBackground = equippedInventroySlot.GetChild(0);
        Transform inventoryItemEquipped = equippedInventroySlot.GetChild(1);

        for (int i = inventoryItemEquipped.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryItemEquipped.GetChild(i).gameObject);
        }

        inventoryItemBackground.gameObject.SetActive(true);
        inventoryItemEquipped.gameObject.SetActive(false);
    }

    private void StaticEventHandler_OnPassiveItemAddedToInventorySlot(PassiveItemAddedToBookArgs passiveItemAddedToBookArgs)
    {
        StartCoroutine(InventoryPassiveItemAddRoutine(passiveItemAddedToBookArgs.passiveItem, passiveItemAddedToBookArgs.inventoryIndexNumber));
    }

    IEnumerator InventoryPassiveItemAddRoutine(PassiveItem passiveItem, int index)
    {
        PassiveItemDetailsSO passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);
        Sprite passiveItemSprite = passiveItemDetails.passiveItemSprite;

        yield return new WaitForEndOfFrame();

        if (index < 0) yield break;

        Transform equippedInventroySlot = inventoryParent.GetChild(index);
        equippedInventroySlot.GetComponent<Slot>().slotType = SlotType.Inventory;
        equippedInventroySlot.GetComponent<Slot>().passiveItemSlotName = passiveItem.passiveStats.passiveItemSlotName;

        Transform inventoryItemBackground = equippedInventroySlot.GetChild(0);
        Transform inventoryItemEquipped = equippedInventroySlot.GetChild(1);
        inventoryItemBackground.gameObject.SetActive(false);
        inventoryItemEquipped.gameObject.SetActive(true);

        GameObject inventoryItem = Instantiate(GameResources.Instance.bookWeaponSlot, inventoryItemEquipped);

        // Set Draggable Item
        DraggableItem inventoryDraggableItem = inventoryItem.GetComponent<DraggableItem>();
        inventoryDraggableItem.SetDraggableItem(passiveItem, equippedInventroySlot.GetComponent<Slot>(), passiveItemSprite, 
            ItemSlotStatus.Inventory);
    }

    private void StaticEventHandler_OnInventoryPassiveItemDropped(PassiveItemAddedToBookArgs passiveItemAddedToBookArgs)
    {
        StartCoroutine(InventoryPassiveItemDropRoutine(passiveItemAddedToBookArgs.inventoryIndexNumber));
    }

    IEnumerator InventoryPassiveItemDropRoutine(int index)
    {
        yield return new WaitForEndOfFrame();

        Transform equippedInventroySlot = inventoryParent.GetChild(index);
        equippedInventroySlot.GetComponent<Slot>().slotType = SlotType.Inventory; // Reset inventory slot
        equippedInventroySlot.GetComponent<Slot>().passiveItemSlotName = PassiveItemSlotName.None;

        Transform inventoryItemBackground = equippedInventroySlot.GetChild(0);
        Transform inventoryItemEquipped = equippedInventroySlot.GetChild(1);

        for (int i = inventoryItemEquipped.childCount - 1; i >= 0; i--)
        {
            Destroy(inventoryItemEquipped.GetChild(i).gameObject);
        }

        inventoryItemBackground.gameObject.SetActive(true);
        inventoryItemEquipped.gameObject.SetActive(false);
    }

    private void StaticEventHandler_OnWeaponsSwappedWithInventory(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        StartCoroutine(SlotAndInventoryWeaponSwapRoutine(weaponAddedToBookArgs.weapon, weaponAddedToBookArgs.intentoryWeapon, weaponAddedToBookArgs.inventoryIndexNumber,
            weaponAddedToBookArgs.weaponSetNumber, weaponAddedToBookArgs.onMainHand, weaponAddedToBookArgs.slotWeaponDraggableItem, weaponAddedToBookArgs.inventoryWeaponDraggableItem));
    }

    IEnumerator SlotAndInventoryWeaponSwapRoutine(Weapon slotWeapon, Weapon inventoryWeapon, int index, int weaponSetNumber, bool onMainHand, DraggableItem slotWeaponDraggableItem,
        DraggableItem inventoryWeaponDraggableItem)
    {
        WeaponDetailsSO slotWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(slotWeapon.weaponStats.weaponTitle);
        WeaponDetailsSO inventoryWeaponDetails = WartheonDatabase.Instance.GetWeaponDetails(inventoryWeapon.weaponStats.weaponTitle);

        Sprite slotWeaponSprite = slotWeaponDetails.weaponFrontSprite;
        Sprite inventoryWeaponSprite = inventoryWeaponDetails.weaponFrontSprite;

        yield return new WaitUntil(() => !DraggableItem.IsDragging);
        yield return new WaitForEndOfFrame();

        Transform inventorySlotTransform = inventoryParent.GetChild(index).GetChild(1);
        Transform weaponSlotTransform = (onMainHand ? mainHandWeaponSlot : offHandWeaponSlot).GetChild(1);

        // Clean up visuals from both slots
        foreach (Transform child in weaponSlotTransform) Destroy(child.gameObject);
        foreach (Transform child in inventorySlotTransform) Destroy(child.gameObject);

        // Create inventory-side draggable (slot weapon goes to inventory)
        if (slotWeapon != null)
        {
            GameObject inventoryItem = Instantiate(GameResources.Instance.bookWeaponSlot, inventorySlotTransform);
            DraggableItem newInventoryDraggable = inventoryItem.GetComponent<DraggableItem>();

            Slot inventorySlot = inventorySlotTransform.parent.GetComponent<Slot>();
            newInventoryDraggable.SetDraggableItem(slotWeapon, inventorySlot, slotWeaponSprite, ItemSlotStatus.Inventory);
        }

        ItemSlotStatus validItemSlotStatus = onMainHand ? ItemSlotStatus.MainHand : ItemSlotStatus.OffHand;

        // Create weapon slot draggable (inventory weapon goes to equipped slot)
        if (inventoryWeapon != null)
        {
            GameObject weaponItem = Instantiate(GameResources.Instance.bookWeaponSlot, weaponSlotTransform);
            DraggableItem newWeaponDraggable = weaponItem.GetComponent<DraggableItem>();

            Slot equippedSlot = weaponSlotTransform.parent.GetComponent<Slot>();
            newWeaponDraggable.SetDraggableItem(inventoryWeapon, equippedSlot, inventoryWeaponSprite, validItemSlotStatus);

            // Optional: If you're hovering or selecting, update selection references
            if (Slot.selectedSlot != null && Slot.selectedSlot.selectedSlotDraggableItem == inventoryWeaponDraggableItem)
            {
                Slot.selectedSlot.selectedSlotDraggableItem = newWeaponDraggable;
            }
        }

        UpdatePlayerStatInfo(GameManager.Instance.GetLocalPlayer());
    }

    private void StaticEventHandler_OnGenericItemPlacedToEmptyInventory(ItemGenericPlacedArgs args)
    {
        StartCoroutine(ItemGenericPlaceToEmptyInventorySlotRoutine(args.item, args.fromIndex, args.toIndex));
    }

    IEnumerator ItemGenericPlaceToEmptyInventorySlotRoutine(ItemGeneric item, int fromIndex, int targetSlotIndex)
    {
        Sprite sprite = null;

        if(item is Weapon w)
        {
            WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(w.weaponStats.weaponTitle);
            sprite = weaponDetails.weaponFrontSprite;
        }
        else if (item is PassiveItem p)
        {
            PassiveItemDetailsSO passiveDetails = WartheonDatabase.Instance.GetPassiveItemDetails(p.passiveStats.passiveItemType);
            sprite = passiveDetails.passiveItemSprite;
        }

        // Wait for drag to fully end
        yield return new WaitUntil(() => !DraggableItem.IsDragging);

        // Wait one frame so UI hierarchy settles
        yield return new WaitForEndOfFrame();

        // NOW it is safe to hard-reset
        Transform itemMovedFromBackground = inventoryParent.GetChild(fromIndex).GetChild(0);
        Transform itemMovedFromEquipped = inventoryParent.GetChild(fromIndex).GetChild(1);

        // EMPTY PREVIOUS SLOT
        for (int i = itemMovedFromEquipped.childCount - 1; i >= 0; i--)
        {
            Destroy(itemMovedFromEquipped.GetChild(i).gameObject);
        }

        itemMovedFromBackground.gameObject.SetActive(true);
        itemMovedFromEquipped.gameObject.SetActive(false);

        // SLOT PLACEMENT STARTS HERE
        // Rebuild visuals
        Transform targetSlot = inventoryParent.GetChild(targetSlotIndex);
        Transform targetBackground = targetSlot.GetChild(0);
        Transform targetItemEquipped = targetSlot.GetChild(1);

        targetBackground.gameObject.SetActive(false);
        targetItemEquipped.gameObject.SetActive(true);

        GameObject inventoryItem = Instantiate(GameResources.Instance.bookWeaponSlot, targetItemEquipped);

        DraggableItem placedDraggableItem = inventoryItem.GetComponent<DraggableItem>();

        placedDraggableItem.SetDraggableItem(item, targetSlot.GetComponent<Slot>(), sprite, ItemSlotStatus.Inventory);
    }

    private void StaticEventHandler_OnGenericItemsSwappedInInventory(ItemGenericSwappedArgs args)
    {
        StartCoroutine(ItemGenericSwapRoutine(args.draggableItem, args.targetItem, args.draggableItemInventoryIndex, args.targetItemInventoryIndex));
    }

    IEnumerator ItemGenericSwapRoutine(DraggableItem draggableItem, DraggableItem targetItem, int draggableItemInventoryIndex, int targetItemInventoryIndex)
    {
        yield return new WaitUntil(() => !DraggableItem.IsDragging);
        yield return new WaitForEndOfFrame(); // Also wait one frame for UI updates to settle

        Transform draggedItemsSlot = inventoryParent.GetChild(draggableItemInventoryIndex);
        Transform draggedInventoryItemEquipped = draggedItemsSlot.GetChild(1);
        DraggableItem inventoryPassiveDraggableItem = draggedInventoryItemEquipped.GetComponentInChildren<DraggableItem>();

        Transform targetSlot = inventoryParent.GetChild(targetItemInventoryIndex);
        Transform targetInventoryItemEquipped = targetSlot.GetChild(1);
        DraggableItem slotPassiveDraggableItem = targetInventoryItemEquipped.GetComponentInChildren<DraggableItem>();
  
        // Swap
        if (slotPassiveDraggableItem != null && inventoryPassiveDraggableItem != null)
        {
            SafeReparentDraggableItem(slotPassiveDraggableItem, draggedInventoryItemEquipped);
            SafeReparentDraggableItem(inventoryPassiveDraggableItem, targetInventoryItemEquipped);
        }
    }

    private void StaticEventHandler_OnPassiveItemsSwapped(PassiveItemAddedToBookArgs args)
    {
        StartCoroutine(PassiveItemSwapRoutine(args.passiveItem, args.inventoryPassiveItem, args.inventoryIndexNumber));
    }

    IEnumerator PassiveItemSwapRoutine(PassiveItem slotPassiveItem, PassiveItem inventoryPassiveItem, int index)
    {
        yield return new WaitUntil(() => !DraggableItem.IsDragging);
        yield return new WaitForEndOfFrame(); // Also wait one frame for UI updates to settle

        Transform equippedInventorySlot = inventoryParent.GetChild(index);
        Transform inventoryItemEquipped = equippedInventorySlot.GetChild(1);
        DraggableItem inventoryPassiveDraggableItem = inventoryItemEquipped.GetComponentInChildren<DraggableItem>();

        PassiveItemDetailsSO slotPassiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(slotPassiveItem.passiveStats.passiveItemType);

        Transform slot = GetEquippedSlot(slotPassiveItemDetails.passiveItemSlotName);
        Transform slotEquipped = slot.GetChild(1);
        DraggableItem slotPassiveDraggableItem = slotEquipped.GetComponentInChildren<DraggableItem>();

        // Swap
        if (slotPassiveDraggableItem != null && inventoryPassiveDraggableItem != null)
        {
            SafeReparentDraggableItem(slotPassiveDraggableItem, inventoryItemEquipped);
            SafeReparentDraggableItem(inventoryPassiveDraggableItem, slotEquipped);
        }

        UpdatePlayerStatInfo(GameManager.Instance.GetLocalPlayer());
    }

    private void StaticEventHandler_OnInventoryWeaponUpgraded(InventoryWeaponUpgradedArgs args)
    {
        StartCoroutine(InventoryWeaponUpgradeRoutine(args.inventoryIndexNumber, args.weapon));
    }

    private void StaticEventHandler_OnInventoryPassiveUpgraded(InventoryPassiveUpgradedArgs args)
    {
        StartCoroutine(InventoryPassiveUpgradeRoutine(args.inventoryIndexNumber, args.passiveItem));
    }

    IEnumerator InventoryWeaponUpgradeRoutine(int index, Weapon weapon)
    {
        yield return new WaitForEndOfFrame();
    }

    IEnumerator InventoryPassiveUpgradeRoutine(int index, PassiveItem item)
    {
        yield return new WaitForEndOfFrame();
    }

    private void SafeReparentDraggableItem(DraggableItem item, Transform newParent)
    {
        if (item.transform.parent != newParent)
        {
            item.transform.SetParent(newParent, false);
            item.rectTransform.anchoredPosition = Vector2.zero;

            if (item.TryGetComponent(out CanvasGroup group))
            {
                group.alpha = 1f;
                group.blocksRaycasts = true;
            }

            item.originalParent = newParent;

            item.belongingSlot = newParent.GetComponentInParent<Slot>();
        }
    }

    private void StaticEventHandler_OnWeaponSwitched(WeaponAddedToBookArgs args)
    {
        DisableBackgroundEnableEquippedTransform(true);
        DisableBackgroundEnableEquippedTransform();
        EmptyMainHandEquippedSlot();
        EmptyOffhandEquippedSlot();

        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
        {
            PlaceWeaponIconToMainHand(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0]);
        }
        else
        {
            EnableBackgroundDisableEquippedTransform();
        }

        // Off-hand full
        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] != null)
        {
            PlaceWeaponIconToOffhand(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1]);
        }
        // Off-hand empty
        else
        {
            // Off-hand empty and main hand full
            if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
            {
                // Off-hand empty and main hand is two-handed weapon
                if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0].weaponStats.wieldType == WieldType.TwoHanded)
                {
                    PlaceLockIcon();
                }
                // Off-hand empty and main hand is one-handed weapon
                else
                {
                    EnableBackgroundDisableEquippedTransform(true);
                }
            }
            // Both off-hand and main hands are empty
            else
            {
                EnableBackgroundDisableEquippedTransform(true);
            }
        }

        UpdatePlayerStatInfo(player);
    }

    private void StaticEventHandler_OnWeaponSwitchedInventory(WeaponAddedToBookArgs args)
    {
        DisableBackgroundEnableEquippedTransform(true);
        DisableBackgroundEnableEquippedTransform();
        EmptyMainHandEquippedSlot();
        EmptyOffhandEquippedSlot();

        EmptyInventorySlot();

        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
        {
            PlaceWeaponIconToMainHand(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0]);
        }
        else
        {
            EnableBackgroundDisableEquippedTransform();
        }

        // Off-hand full
        if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1] != null)
        {
            PlaceWeaponIconToOffhand(player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][1]);
        }
        // Off-hand empty
        else
        {
            // Off-hand empty and main hand full
            if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0] != null)
            {
                // Off-hand empty and main hand is two-handed weapon
                if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0].weaponStats.wieldType == WieldType.TwoHanded)
                {
                    PlaceLockIcon();
                }
                // Off-hand empty and main hand is one-handed weapon
                else
                {
                    EnableBackgroundDisableEquippedTransform(true);
                }
            }
            // Both off-hand and main hands are empty
            else
            {
                EnableBackgroundDisableEquippedTransform(true);
            }
        }

        //Inventory
        for (int i = 0; i < player.playerInventory.inventoryArray.Length; i++)
        {
            if (player.playerInventory.inventoryArray[i] != null)
            {
                PopulateItemIntoInventorySlot(player.playerInventory.inventoryArray[i], i);
            }
        }

        UpdatePlayerStatInfo(player);
    }

    private void StaticEventHandler_OnWeaponDropped(WeaponAddedToBookArgs args)
    {
        if (args.slotType == SlotType.WeaponOffHand)
        {
            DisableBackgroundEnableEquippedTransform(true);
            EmptyOffhandEquippedSlot();
            EnableBackgroundDisableEquippedTransform(true);
        }
        else
        {
            DisableBackgroundEnableEquippedTransform();
            EmptyMainHandEquippedSlot();
            EnableBackgroundDisableEquippedTransform();

            // Remove lock icon
            if (args.weapon != null && args.weapon.weaponStats.wieldType == WieldType.TwoHanded)
            {
                DisableBackgroundEnableEquippedTransform(true);
                EmptyOffhandEquippedSlot();
                EnableBackgroundDisableEquippedTransform(true);
            }
        }

        UpdatePlayerStatInfo(player);
    }

    private void PlaceWeaponIconToMainHand(Weapon weapon)
    {
        GameObject newMainHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponEquipped);

        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);
        newMainHandWeaponAtSlot.GetComponent<Image>().sprite = weaponDetails.weaponFrontSprite;

        DraggableItem placedDraggableItem = mainHandWeaponEquipped.GetChild(0).GetComponent<DraggableItem>();
        placedDraggableItem.SetDraggableItem(weapon, mainHandWeaponSlot.GetComponent<Slot>(), placedDraggableItem.GetComponent<Image>().sprite, ItemSlotStatus.MainHand);
    }

    private void PlaceWeaponIconToOffhand(Weapon weapon)
    {
        GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);

        WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);
        offHandWeaponAtSlot.GetComponent<Image>().sprite = weaponDetails.weaponFrontSprite;

        DraggableItem placedDraggableItem = offHandWeaponEquipped.GetChild(0).GetComponent<DraggableItem>();
        placedDraggableItem.SetDraggableItem(weapon, offHandWeaponSlot.GetComponent<Slot>(), placedDraggableItem.GetComponent<Image>().sprite, ItemSlotStatus.OffHand);
    }

    private void PlaceItemGenericToInventory(ItemGeneric item, Transform inventorySlot)
    {
        Transform inventoryEquippedSlot = inventorySlot.GetChild(1);
        GameObject itemGenericAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, inventoryEquippedSlot);

        if (item is Weapon weapon)
        {
            WeaponDetailsSO weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);
            inventoryEquippedSlot.GetComponent<Image>().sprite = weaponDetails.weaponFrontSprite;
        }
        else if (item is PassiveItem passive)
        {
            PassiveItemDetailsSO passiveDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passive.passiveStats.passiveItemType);
            inventoryEquippedSlot.GetComponent<Image>().sprite = passiveDetails.passiveItemSprite;
        }

        DraggableItem placedDraggableItem = itemGenericAtSlot.GetComponent<DraggableItem>();
        placedDraggableItem.SetDraggableItem(item, inventorySlot.GetComponent<Slot>(), inventoryEquippedSlot.GetComponent<Image>().sprite, ItemSlotStatus.Inventory);
    }

    private void EmptyMainHandEquippedSlot()
    {
        for (int i = mainHandWeaponEquipped.childCount - 1; i >= 0; i--)
        {
            Destroy(mainHandWeaponEquipped.GetChild(i).gameObject);
        }
    }

    private void EmptyOffhandEquippedSlot()
    {
        for (int i = offHandWeaponEquipped.childCount - 1; i >= 0; i--)
        {
            Destroy(offHandWeaponEquipped.GetChild(i).gameObject);
        }
    }

    private void EmptyInventorySlot()
    {
        foreach (Transform inventorySlot in inventoryParent)
        {
            Transform inventoryItemBackground = inventorySlot.GetChild(0);
            Transform inventoryItemEquipped = inventorySlot.GetChild(1);

            for (int i = inventoryItemEquipped.childCount - 1; i >= 0; i--)
            {
                Destroy(inventoryItemEquipped.GetChild(i).gameObject);

                Debug.Log("One slot is destroyed");
            }

            inventoryItemBackground.gameObject.SetActive(true);
            inventoryItemEquipped.gameObject.SetActive(false);
        }
    }

    private void PopulateItemIntoInventorySlot(ItemGeneric item, int inventoryIndex)
    {
        Transform inventoryItemBackground = inventoryParent.GetChild(inventoryIndex).GetChild(0);
        Transform inventoryItemEquipped = inventoryParent.GetChild(inventoryIndex).GetChild(1);

        inventoryItemBackground.gameObject.SetActive(false);
        inventoryItemEquipped.gameObject.SetActive(true);

        if (inventoryItemEquipped.childCount == 0)
        {
            PlaceItemGenericToInventory(item, inventoryParent.GetChild(inventoryIndex));
        }
    }

    private void PlaceLockIcon()
    {
        GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
        offHandWeaponAtSlot.GetComponent<Image>().sprite = GameResources.Instance.lockSlotIcon;
        offHandWeaponAtSlot.GetComponent<DraggableItem>().isLockIcon = true;
    }

    /// <summary>
    /// Enable background transform and disable equipped transform which hold weapon images
    /// </summary>
    private void EnableBackgroundDisableEquippedTransform(bool offHand = false)
    {
        if (offHand)
        {
            offHandWeaponBackground.gameObject.SetActive(true);
            offHandWeaponEquipped.gameObject.SetActive(false);
        }
        else
        {
            mainHandWeaponBackground.gameObject.SetActive(true);
            mainHandWeaponEquipped.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Disable background transform and enable equipped transform which hold weapon images
    /// </summary>
    private void DisableBackgroundEnableEquippedTransform(bool offHand = false)
    {
        if (offHand)
        {
            offHandWeaponBackground.gameObject.SetActive(false);
            offHandWeaponEquipped.gameObject.SetActive(true);
        }
        else
        {
            mainHandWeaponBackground.gameObject.SetActive(false);
            mainHandWeaponEquipped.gameObject.SetActive(true);
        }
    }

    private void StaticEventHandler_OnBookHealthChanged(HealthChangedArgs healthChangedArgs)
    {
        Player player = GameManager.Instance.GetLocalPlayer();
        healthValue.text = $"{player.health.GetCurrentHealth()} / {player.health.GetMaximumHealth()}";
    }

    private void StaticEventHandler_OnBookManaChanged(ManaChangedArgs manaChangedArgs)
    {
        Player player = GameManager.Instance.GetLocalPlayer();
        manaValue.text = $"{player.mana.GetCurrentMana()} / {player.mana.GetMaximumMana()}";
    }

    private void ConsumableEvent_OnCoinCountChanged(ConsumableEvent arg1, ConsumableEventArgs arg)
    {
        coinsText.text = arg.coinAmount.ToString();
    }

    private void ConsumableEvent_OnShardCountChanged(ConsumableEvent arg1, ConsumableEventArgs arg)
    {
        shardText.text = arg.shardAmount.ToString();
    }

    private void DestroyDraggableItems(Transform current)
    {
        // First, recursively process children
        foreach (Transform child in current)
        {
            DestroyDraggableItems(child);
        }

        // Then check the current transform
        DraggableItem draggable = current.GetComponent<DraggableItem>();
        if (draggable != null)
        {
            Destroy(draggable.gameObject);
        }
    }

    private void StaticEventHandler_OnItemAddedToPassiveItemSlot(PassiveItemAddedToBookArgs itemAddedToBookArgs)
    {
        StartCoroutine(PassiveItemAddRoutine(itemAddedToBookArgs.passiveItem, itemAddedToBookArgs.itemSlotName));
    }

    IEnumerator PassiveItemAddRoutine(PassiveItem passiveItem, PassiveItemSlotName itemSlotName)
    {
        yield return new WaitForEndOfFrame();

        Transform background;
        Transform equipped;
        GameObject passiveItemObject = new GameObject();

        Transform passiveItemSlot = GetEquippedSlot(itemSlotName);

        if(passiveItemSlot != null)
        {
            background = passiveItemSlot.GetChild(0);
            equipped = passiveItemSlot.GetChild(1);
            background.gameObject.SetActive(false);
            equipped.gameObject.SetActive(true);

            // Loop through all child objects and destroy them
            for (int i = equipped.childCount - 1; i >= 0; i--)
            {
                Destroy(equipped.GetChild(i).gameObject);
            }

            PassiveItemDetailsSO passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);

            passiveItemObject = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
            passiveItemObject.GetComponent<Image>().sprite = passiveItemDetails.passiveItemSprite;


            DraggableItem placedDraggableItem = equipped.GetComponent<DraggableItem>();
            placedDraggableItem.SetDraggableItem(passiveItem, passiveItemSlot.GetComponent<Slot>(), equipped.GetComponent<Image>().sprite, ItemSlotStatus.Passive);
        }
    }

    private void StaticEventHandler_OnItemRemovedFromPassiveItemSlot(PassiveItemRemovedFromBookArgs itemRemovedFromBookArgs)
    {
        StartCoroutine(PassiveItemRemoveRoutine(itemRemovedFromBookArgs.itemSlotName));
    }

    IEnumerator PassiveItemRemoveRoutine(PassiveItemSlotName passiveItemSlotName)
    {
        yield return new WaitForEndOfFrame();

        GameObject passiveItemAtSlot = new GameObject();
        Transform background;
        Transform equipped;

        Transform passiveItemSlot = GetEquippedSlot(passiveItemSlotName);
        background = passiveItemSlot.GetChild(0);
        equipped = passiveItemSlot.GetChild(1);

        // Loop through all child objects and destroy them
        for (int i = equipped.childCount - 1; i >= 0; i--)
        {
            passiveItemAtSlot = equipped.GetChild(i).gameObject;
            Destroy(passiveItemAtSlot);
        }

        background.gameObject.SetActive(true);
        equipped.gameObject.SetActive(false);

        UpdatePlayerStatInfo(GameManager.Instance.GetLocalPlayer());
    }

    Transform GetEquippedSlot(PassiveItemSlotName slotName)
    {
        return slotName switch
        {
            PassiveItemSlotName.Head => passiveItemHeadSlot,
            PassiveItemSlotName.Chest => passiveItemChestSlot,
            PassiveItemSlotName.Neck => passiveItemNeckSlot,
            PassiveItemSlotName.Finger => passiveItemFingerSlot,
            PassiveItemSlotName.Back => passiveItemBackSlot,
            PassiveItemSlotName.Arm => passiveItemArmSlot,
            PassiveItemSlotName.Leg => passiveItemLegSlot,
            _ => null,
        };
    }

    private void UpdatePlayerStatInfo(Player player)
    {
        // PRIMARY STATS
        characterName.text = player.playerDetails.playerCharacterName;
        strengthValue.text = player.CurrentStrengthValue.ToString();
        constitutionValue.text = player.CurrentConstitutionValue.ToString();
        dexterityValue.text = player.CurrentDexterityValue.ToString();
        intelligenceValue.text = player.CurrentIntelligenceValue.ToString();
        agilityValue.text = player.CurrentAgilityValue.ToString();
        willpowerValue.text = player.CurrentWillpowerValue.ToString();
        resolveValue.text = player.CurrentResolveValue.ToString();
        ferocityValue.text = player.CurrentFerocityValue.ToString();

        // SECONDARY STATS
        damageValue.text = $"{player.currentMainHandMinDamageValue}-{player.currentMainHandMaxDamageValue}({player.currentOffHandMinDamageValue}-" +
            $"{player.currentOffHandMaxDamageValue})";
        healthValue.text = $"{player.health.GetCurrentHealth()} / {player.health.GetMaximumHealth()}";
        manaValue.text = $"{player.mana.GetCurrentMana()} / {player.mana.GetMaximumMana()}";
        dodgeRateValue.text = $"{player.currentDodgeValue * 100}%";
        blockRateValue.text = $"{player.currentBlockValue * 100}%";
        speedValue.text = $"{player.movementByForce.moveSpeed}";
        criticalHitChanceDamageValue.text = $"{player.currentMainHandCriticalHitChance * 100}%({player.currentOffHandCriticalHitChance * 100}%)";

        float deductedMainHandCriticalDamage = (float)Math.Round((double)((player.currentMainHandCriticalHitDamage - 1) * 100), 2);
        float deductedOffHandCriticalDamage = (float)Math.Round((double)((player.currentOffHandCriticalHitDamage - 1) * 100), 2);

        criticalHitDamageAmountValue.text = $"+{Mathf.Max(0, deductedMainHandCriticalDamage)}%(+{Mathf.Max(0, deductedOffHandCriticalDamage)}%)";

        // AUXILLARY STATS
        attackCooldownValue.text = $"{player.additionalAttackCoolDownModifier * 100}%";
        attackRatingValue.text = $"{player.currentAttackRatingValue * 100}";
        skillCooldownModifierValue.text = $"{player.currentSkillCooldownReducer * 100}%";
        damageReductionValue.text = $"{player.currentDamageReductionValue * 100}%";
        skillDurationModifierValue.text = $"{player.currentSkillDurationModifier * 100}%";
        damageVsLowHealthValue.text = $"{player.currentDamageVsLowHealthModifierValue}";
        lifeStealValue.text = $"{player.currentLifeStealValue}";
        armorPenetrationValue.text = $"{player.currentArmorPenetrationValue * 100}%";

        // RESISTANCE STATS
        armorText.text = $"{player.currentArmorValue * 100}%";
        magicResistanceText.text = $"{player.currentMagicResistanceValue * 100}%";
        statusResistanceText.text = $"{player.currentStatusResistance * 100}%";
        criticalResistanceText.text = $"{player.currentCriticalResistanceValue * 100}%";
    }

    public void OpenBuildPage()
    {
        if (skillPage.GetChild(0).gameObject.activeSelf) return;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Skills));
    }

    public void OpenStatsPage()
    {
        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Stats));
    }

    public void OpenBestiaryPage()
    {
        if (beastiaryPage.GetChild(0).gameObject.activeSelf) return;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Beastiary));
    }

    public void OpenBossesPage()
    {
        if (bossesPage.GetChild(0).gameObject.activeSelf) return;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Bosses));
    }

    IEnumerator CompleteTurnPageThenDisplay(BookPage bookPage)
    {
        TurnThePage();

        while (!GameManager.Instance.turnPageCompleted)
        {
            yield return new WaitForEndOfFrame();
        }

        if (GameManager.Instance.turnPageCompleted)
        {
            GameManager.Instance.statsPageChanged = true;

            switch (bookPage)
            {
                case BookPage.Stats:
                    EnableStatPage();
                    break;
                case BookPage.Beastiary:
                    beastiaryPage.GetChild(0).gameObject.SetActive(true);
                    EnableBeastiaryPage();
                    UpdateBeastiaryPage();
                    break;
                case BookPage.Bosses:
                    bossesPage.GetChild(0).gameObject.SetActive(true);
                    EnableBossesPage();
                    UpdateBossesPage();
                    break;
                case BookPage.Skills:
                    EnableBuildsPage();
                    skillPage.GetChild(0).gameObject.SetActive(true);
                    skillDescriptonInnerPanel.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }

    private void TurnThePage()
    {
        if (statsPage.GetChild(0).gameObject.activeSelf) { ClearStatPage(); }
        else if (beastiaryPage.GetChild(0).gameObject.activeSelf) { ClearBeastiaryPage();  }
        else if (bossesPage.GetChild(0).gameObject.activeSelf) { ClearBossesPage(); }
        else if (skillPage.GetChild(0).gameObject.activeSelf) { ClearBuildsPage(); }

        bookAnimator.enabled = false;
        bookAnimator.enabled = true;
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.openBookSoundEffect);
        bookAnimator.SetBool(Settings.turnPage, true);
    }

    private void ClearStatPage()
    {
        foreach (Transform child in statsPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void EnableStatPage()
    {
        foreach (Transform child in statsPage)
        {
            if (child == tooltipManager.transform) continue;

            child.gameObject.SetActive(true);
        }
    }

    private void ClearBeastiaryPage()
    {
        foreach (Transform child in beastiaryPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void UpdateBeastiaryPage()
    {
        for (int i = 0; i < beastiaryImageContainer.childCount; i++)
        {
            MobSlot mobSlot = beastiaryImageContainer.GetChild(i).GetComponent<MobSlot>();

            if (mobSlot.mobUnlocked)
            {
                mobSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                mobSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void EnableBeastiaryPage()
    {
        foreach (Transform child in beastiaryPage)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void ClearBossesPage()
    {
        foreach (Transform child in bossesPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void UpdateBossesPage()
    {
        for (int i = 0; i < bossImageContainer.childCount; i++)
        {
            MobSlot mobSlot = bossImageContainer.GetChild(i).GetComponent<MobSlot>();

            if (mobSlot.mobUnlocked)
            {
                mobSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                mobSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void EnableBossesPage()
    {
        foreach (Transform child in bossesPage)
        {
            child.gameObject.SetActive(true);

            if (InputManager.TutorialEnabled && TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.OtherCollectionsPage)
            {
                TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
            }
        }
    }

    private void ClearBuildsPage()
    {
        foreach (Transform child in skillPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void EnableBuildsPage()
    {
        foreach (Transform child in skillPage)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void StaticEventHandler_OnMobUnlocked(MobUnlockArgs mobUnlockArgs)
    {
        if (beastiaryImageContainer == null) return;

        if (!mobUnlockArgs.isBoss)
        {
            for (int i = 0; i < beastiaryImageContainer.childCount; i++)
            {
                MobSlot mobSlot = beastiaryImageContainer.GetChild(i).GetComponent<MobSlot>();

                if (mobSlot.mobUnlocked == true) continue;

                if (mobSlot.mobDetails.enemyCategory == mobUnlockArgs.mobCategory)
                {
                    mobSlot.mobUnlocked = true;
                    mobSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    mobSlot.GetComponent<Button>().interactable = true;
                }
            }
        }
        else
        {
            for (int i = 0; i < bossImageContainer.childCount; i++)
            {
                MobSlot mobSlot = bossImageContainer.GetChild(i).GetComponent<MobSlot>();

                if (mobSlot.mobUnlocked == true) continue;

                if (mobSlot.mobDetails.enemyCategory == mobUnlockArgs.mobCategory)
                {
                    mobSlot.mobUnlocked = true;
                    mobSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                    mobSlot.GetComponent<Button>().interactable = true;
                }
            }
        }
    }

    private void StaticEventHandler_OnMobHovered(MobHoverArgs mobHoverArgs)
    {
        if (!mobHoverArgs.isBoss)
        {
            for (int i = 0; i < beastiaryImageContainer.childCount; i++)
            {
                MobSlot mobSlot = beastiaryImageContainer.GetChild(i).GetComponent<MobSlot>();

                if (mobSlot.mobUnlocked == true && mobSlot.mobDetails.enemyCategory == mobHoverArgs.mobCategory)
                {
                    mobTitleText.text = mobSlot.mobDetails.enemyName;
                    mobDetailsText.text = mobSlot.mobDetails.enemyDetails;
                }
            }
        }
        else
        {
            for (int i = 0; i < bossImageContainer.childCount; i++)
            {
                MobSlot mobSlot = bossImageContainer.GetChild(i).GetComponent<MobSlot>();

                if (mobSlot.mobUnlocked == true && mobSlot.mobDetails.enemyCategory == mobHoverArgs.mobCategory)
                {
                    bossTitleText.text = mobSlot.mobDetails.enemyName;
                    bossDetailsText.text = mobSlot.mobDetails.enemyDetails;
                }
            }
        }
    }

    private void StaticEventHandler_OnMobUnhovered(MobHoverArgs mobHoverArgs)
    {
        if (!mobHoverArgs.isBoss)
        {
            mobTitleText.text = string.Empty;
            mobDetailsText.text = string.Empty;
        }
        else
        {
            bossTitleText.text = string.Empty;
            bossDetailsText.text = string.Empty;
        }
    }

    private void StaticEventHandler_OnPrimaryStatsChanged()
    {
        UpdatePlayerStatInfo(player);
    }

    private void StaticEventHandler_OnInnerPathPointUsed(SkillPointsArgs buildPointsArgs)
    {
        skillPointsTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.currentSkillPoints.ToString();
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.buildActivationSoundEffect);
        UseInnerPath(buildPointsArgs);
    }

    private void UseInnerPath(SkillPointsArgs buildPointsArgs)
    {
        switch (buildPointsArgs.innerPathDetails.innerPathSelectionName)
        {
            case InnerPathName.KillersEdge:
                player.additionalCriticalDamageModifier += 0.1f;
                break;
            case InnerPathName.FocusedAggression:
                player.isFocusedAggressionActive = true;
                break;
            case InnerPathName.PunishersWill:
                player.isPunishersWillActive = true;
                break;
            case InnerPathName.ViciousMomentum:
                player.isViciousMomentumActive = true;
                break;
            case InnerPathName.SurgingElements:
                player.additionalMagicDamageModifier = 0.1f;
                break;
            case InnerPathName.CounterRiposte:
                player.isCounterRiposteActive = true;
                break;
            case InnerPathName.Armorbane:
                player.additionalArmorPenetrationModifier = 0.06f;
                break;
            case InnerPathName.TriadExecution:
                player.isTriadExecutionActive = true;
                break;
            case InnerPathName.UnyieldingGuard:
                player.additionalShieldArmorModifier = 0.1f;
                break;
            case InnerPathName.DieHard:
                player.isDieHardActive = true;
                break;
            case InnerPathName.StoneSkin:
                player.additionalArmorModifier = 0.1f;
                break;
            case InnerPathName.IronTenacity:
                player.additionalStatusResistanceModifier += 0.1f;
                break;
            case InnerPathName.FortifiedResolve:
                player.isFortifiedResolveActive = true;
                break;
            case InnerPathName.ArcaneFortitude:
                player.currentMagicResistanceValue += 0.1f;
                break;
            case InnerPathName.SecondBreath:
                player.isSecondBreathActive = true;
                break;
            case InnerPathName.BlockThemAll:
                player.additionalBlockModifier = 0.1f;
                break;
            case InnerPathName.QuickReflexes:
                player.additionalDodgeRateModifier = 0.05f;
                break;
            case InnerPathName.WeaversTempo:
                player.additionalSkillCoolDownModifier = 0.08f;
                break;
            case InnerPathName.EfficientMind:
                player.additionalManaReductionModifier = 0.1f;
                break;
            case InnerPathName.ShiftingStance:
                player.isShiftingStanceActive = true;
                break;
            case InnerPathName.CombatFocus:
                player.isCombatFocusActive = true;
                break;
            case InnerPathName.Resourceful:
                player.resourcefulActive = true;
                break;
            case InnerPathName.BattleReady:
                player.isBattleReadyActive = true;
                break;
            case InnerPathName.SurgeTapGain:
                player.isSurgeTapGainActive = true;
                break;
            default:
                break;
        }

        // Recalculate and update book UI after new build unlocked
        player.RecalculateSecondaryStats();
        UpdatePlayerStatInfo(player);
    }

    private void RefreshLocalizedTexts(Player player, ActiveSkill activeSkill, PassiveSkill passiveSkill)
    {
        switch (passiveSkill)
        {
            case PassiveSkill.None:
                break;
            case PassiveSkill.GraceOfTheUnscarred:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsCaelion", "UNIQUE_SKILL_P_NAME");
                break;
            case PassiveSkill.BloodEagle:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKarnag", "UNIQUE_SKILL_P_NAME");
                break;
            case PassiveSkill.PhoenixRising:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKynara", "UNIQUE_SKILL_P_NAME");
                break;
            case PassiveSkill.CloakedPrecision:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMorven", "UNIQUE_SKILL_P_NAME");
                break;
            case PassiveSkill.IceborneVitality:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMycara", "UNIQUE_SKILL_P_NAME");
                break;
            case PassiveSkill.ConductiveTouch:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNymara", "UNIQUE_SKILL_P_NAME");
                break;
            case PassiveSkill.DeadeyesQuiver:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyveran", "UNIQUE_SKILL_P_NAME");
                break;
            case PassiveSkill.NyxasReflex:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyxa", "UNIQUE_SKILL_P_NAME");
                break;
            default:
                break;
        }

        switch (activeSkill)
        {
            case ActiveSkill.SeismicSlam:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsCaelion", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.Valor:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsCaelion", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.ShieldBash:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsCaelion", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.BreakTheLine:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsCaelion", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.GuardedOath:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsCaelion", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.UmbralMist:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMorven", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.Stealth:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMorven", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.BloodDrain:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMorven", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.ShadowStep:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMorven", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.CullTheMeek:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMorven", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.Penetrate:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyveran", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.TripleThreat:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyveran", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.BindingArrow:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyveran", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.ArrowsOfTheSevenPlagues:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyveran", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.HuntersReach:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyveran", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.Blizzard:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMycara", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.MycarasSeal:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMycara", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.SheerCold:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMycara", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.Icebreaker:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMycara", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.AbsoluteZero:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsMycara", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.FireBlast:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKynara", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.MoltenRift:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKynara", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.FlameLotus:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKynara", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.KynarasEmbrace:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKynara", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.BlazingCyclone:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKynara", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.Rage:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKarnag", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.Shattercry:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKarnag", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.AxeThrow:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKarnag", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.Whirlrend:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKarnag", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.FeastOfWar:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsKarnag", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.MistOfDisruption:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNymara", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.NymarasWindveil:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNymara", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.ChainLightning:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNymara", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.EyeOfTheStorm:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNymara", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.IonicRejuvenation:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNymara", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.DontBlink:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyxa", "UNIQUE_SKILL_1_NAME");
                break;
            case ActiveSkill.VenomousIvy:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyxa", "UNIQUE_SKILL_2_NAME");
                break;
            case ActiveSkill.FadeAndFeed:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyxa", "UNIQUE_SKILL_3_NAME");
                break;
            case ActiveSkill.BladeDash:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyxa", "UNIQUE_SKILL_4_NAME");
                break;
            case ActiveSkill.Shiruken:
                skillDescriptionText.text = LocalizationSettings.StringDatabase.GetLocalizedString("UniqueSkillsNyxa", "UNIQUE_SKILL_5_NAME");
                break;
            case ActiveSkill.None:
                break;
            default:
                break;
        }
    }

    private void StaticEventHandler_OnStatPointChanged()
    {
        UpdatePlayerStatInfo(player);

        currentAvailableStatPoints.text = player.currentStatPoints.ToString();
    }

    private void StaticEventHandler_OnLevelUp()
    {
        skillPointsTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.currentSkillPoints.ToString();
    }

    private void StaticEventHandler_OnSkillBoostUsed(SkillBoostArgs skillBoostArgs)
    {
        skillPointsTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.currentSkillPoints.ToString();
    }

    public void SelectWeaponSetOne()
    {
        GameManager.Instance.GetLocalPlayer().playerControl.NextWeaponSet(false, false, 1);
    }

    public void SelectWeaponSetTwo()
    {
        GameManager.Instance.GetLocalPlayer().playerControl.NextWeaponSet(false, false, 2);
    }

    public void SelectWeaponSetThree()
    {
        GameManager.Instance.GetLocalPlayer().playerControl.NextWeaponSet(false, false, 3);
    }
}
