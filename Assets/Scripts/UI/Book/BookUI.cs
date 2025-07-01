using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class BookUI : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    [Header("PAGE HEADERS")]
    [Space(10)]
    public Transform buildPage;
    public Transform statsPage;
    public Transform weaponsPage;
    public Transform passivesPage;
    public Transform activesPage;
    public Transform beastiaryPage;
    public Transform bossesPage;

    [Space(10)]
    public Button weaponSetOneButton;
    public Button weaponSetTwoButton;
    public Button weaponSetThreeButton;

    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] Image characterImage;
    Image characterSeparatorImage;

    [Space(10)]
    [Header("PRIMARY STATS")]
    [SerializeField] TextMeshProUGUI strengthValue;
    [SerializeField] TextMeshProUGUI constitutionValue;
    [SerializeField] TextMeshProUGUI dexterityValue;
    [SerializeField] TextMeshProUGUI intelligenceValue;
    [SerializeField] TextMeshProUGUI willpowerValue;
    [SerializeField] TextMeshProUGUI agilityValue;
    [SerializeField] TextMeshProUGUI resolveValue;
    [SerializeField] TextMeshProUGUI ferocityValue;

    [Space(10)]
    [Header("PRIMARY STATS BUTTONS")]
    public Transform primaryStatsButtonContainer;
    [SerializeField] TextMeshProUGUI currentAvailableStatPoints;

    [Space(10)]
    [Header("SECONDARY STATS")]
    [SerializeField] TextMeshProUGUI damageValue;
    [SerializeField] TextMeshProUGUI healthValue;
    [SerializeField] TextMeshProUGUI criticalHitChanceDamageValue;
    [SerializeField] TextMeshProUGUI elementalDamageModifierValue;
    [SerializeField] TextMeshProUGUI manaValue;
    [SerializeField] TextMeshProUGUI speedValue;
    [SerializeField] TextMeshProUGUI globalResistanceValue;
    [SerializeField] TextMeshProUGUI criticalHitDamageAmountValue;

    [Space(10)]
    [Header("AUXILLARY STATS")]
    [SerializeField] TextMeshProUGUI attackCooldownValue;
    [SerializeField] TextMeshProUGUI chanceToHitValue;
    [SerializeField] TextMeshProUGUI blockRateValue;
    [SerializeField] TextMeshProUGUI debuffDurationModifierValue;
    [SerializeField] TextMeshProUGUI buffDurationModifierValue;
    [SerializeField] TextMeshProUGUI manaRegenerationValue;
    [SerializeField] TextMeshProUGUI eveasivenessRateValue;
    [SerializeField] TextMeshProUGUI dotEffectsValue;

    [Space(10)]
    [Header("RESISTANCE STATS")]
    [SerializeField] TextMeshProUGUI resistanceText;
    [SerializeField] TextMeshProUGUI physicalResistanceText;
    [SerializeField] TextMeshProUGUI fireResistanceText;
    [SerializeField] TextMeshProUGUI waterResistanceText;
    [SerializeField] TextMeshProUGUI airResistanceText;
    [SerializeField] TextMeshProUGUI earthResistanceText;
    [SerializeField] TextMeshProUGUI lightResistanceText;
    [SerializeField] TextMeshProUGUI darkResistanceText;

    [Space(10)]
    [SerializeField] Animator bookAnimator;
    [SerializeField] Transform mainHandWeaponSlot;
    [SerializeField] Transform offHandWeaponSlot;

    [SerializeField] Transform activeItemSlot;

    [Header("Passive Item Slots")]
    Transform passiveItemHeadSlot;
    Transform passiveItemChestSlot;
    Transform passiveItemNeckSlot;
    Transform passiveItemArmSlot;
    Transform passiveItemBackSlot;
    Transform passiveItemLegSlot;
    Transform passiveItemWaistSlot;
    Transform passiveItemFingerSlot;

    [Header("Inventory Item Slots")]
    Transform inventoryParent;
    Transform inventoryItemSlot;

    // SLOT TRANSFORMS
    Transform mainHandWeaponBackground;
    Transform mainHandWeaponEquipped;
    Transform offHandWeaponBackground;
    Transform offHandWeaponEquipped;

    Transform activeUniqueSkillsContainer;
    Transform innerPathContainer;
    Transform buildDescriptonInnerPanel;
    Transform buildPointsTransform;
    Transform buildTextContainer;
    TextMeshProUGUI buildTitleText;
    TextMeshProUGUI buildDetailsText;
    Player player;

    // WEAPONS
    [SerializeField] TextMeshProUGUI weaponTitleText;
    [SerializeField] TextMeshProUGUI weaponDetailsText;
    [SerializeField] Transform weaponImageContainer;

    // PASSIVES
    [SerializeField] TextMeshProUGUI passivesTitleText;
    [SerializeField] TextMeshProUGUI passivesDetailsText;
    [SerializeField] Transform passivesImageContainer;

    // ACTIVES
    [SerializeField] TextMeshProUGUI activesTitleText;
    [SerializeField] TextMeshProUGUI activesDetailsText;
    [SerializeField] Transform activesImageContainer;

    // BEASTIARY
    [SerializeField] TextMeshProUGUI mobTitleText;
    [SerializeField] TextMeshProUGUI mobDetailsText;
    [SerializeField] Transform beastiaryImageContainer;

    // BOSSES
    [SerializeField] TextMeshProUGUI bossTitleText;
    [SerializeField] TextMeshProUGUI bossDetailsText;
    [SerializeField] Transform bossImageContainer;

    bool bookStatPageOpen;

    private void Awake()
    {
        characterSeparatorImage = transform.GetChild(1).GetChild(0).GetChild(0).GetChild(0).GetComponent<Image>();

        // Passive Item Slots
        passiveItemHeadSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(0);
        passiveItemChestSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(1);
        passiveItemNeckSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(2);
        passiveItemArmSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(3);
        passiveItemFingerSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(4);
        passiveItemWaistSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(5);
        passiveItemBackSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(6);
        passiveItemLegSlot = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(7);

        player = GameManager.Instance.GetPlayer();
        characterSeparatorImage.sprite = player.playerDetails.playerMiniMapIcon;

        UpdatePlayerStatInfo(player);

        // Populate Slot Transforms
        mainHandWeaponBackground = mainHandWeaponSlot.GetChild(0);
        mainHandWeaponEquipped = mainHandWeaponSlot.GetChild(1);
        offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
        offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);

        switch (player.playerDetails.playerCharacterIndex)
        {
            case Character.Caelion:
            case Character.Morven:
                characterImage.sprite = player.playerDetails.playerBookSprite;

                // MAIN HAND WEAPON EQUIP AT START - SLOT
                DisableBackgroundEnableEquippedTransform();
                GameObject mainHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponEquipped);
                mainHandWeaponAtSlot.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[0].weaponFrontSprite;

                // OFF HAND WEAPON EQUIP AT START - SLOT
                DisableBackgroundEnableEquippedTransform(true);
                GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
                offHandWeaponAtSlot.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[1].weaponFrontSprite;
                break;

            case Character.Nyveran:
            case Character.Lyrisa:
                characterImage.sprite = player.playerDetails.playerBookSprite;

                // WEAPON EQUIP AT START - SLOT
                DisableBackgroundEnableEquippedTransform();
                mainHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponEquipped);
                mainHandWeaponAtSlot.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[0].weaponFrontSprite;

                // OFF HAND WEAPON EQUIP AT START - SLOT
                DisableBackgroundEnableEquippedTransform(true);
                offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
                offHandWeaponAtSlot.GetComponent<Image>().sprite = GameResources.Instance.lockSlotIcon;
                break;

            default:
                break;
        }

        // ACTIVE ITEM EQUIP AT START
        if (!InputManager.TutorialEnabled)
        {
            Transform activeBackground = activeItemSlot.GetChild(0);
            Transform activeEquipped = activeItemSlot.GetChild(1);
            activeBackground.gameObject.SetActive(false);
            activeEquipped.gameObject.SetActive(true);
            GameObject activeItem = Instantiate(GameResources.Instance.bookWeaponSlot, activeEquipped);
            activeItem.GetComponent<Image>().sprite = player.playerDetails.activeItemsList[0].activeItemSprite;
        }

        // Inventory parent
        inventoryParent = transform.GetChild(1).GetChild(1).GetChild(8).GetChild(0).transform;
    }

    private void OnEnable()
    {
        StaticEventHandler.OnBuildPageOpened += StaticEventHandler_OnBuildPageOpened;

        // BOOK STAT POINTS
        StaticEventHandler.OnStatPointChanged += StaticEventHandler_OnStatPointChanged;

        // BOOK WEAPON EVENTS
        StaticEventHandler.OnWeaponPickedUp += StaticEventHandler_OnWeaponPickedUp;
        StaticEventHandler.OnWeaponSwitched += StaticEventHandler_OnWeaponSwitched;
        StaticEventHandler.OnWeaponDropped += StaticEventHandler_OnWeaponDropped;

        // BOOK INVENTORY EVENTS
        StaticEventHandler.OnWeaponAddedToInventory += StaticEventHandler_OnWeaponAddedToInventory;
        StaticEventHandler.OnPassiveItemAddedToInventorySlot += StaticEventHandler_OnPassiveItemAddedToInventorySlot;
        StaticEventHandler.OnInventoryWeaponDropped += StaticEventHandler_OnInventoryWeaponDropped;
        StaticEventHandler.OnInventoryPassiveItemDropped += StaticEventHandler_OnInventoryPassiveItemDropped;
        StaticEventHandler.OnPassiveItemsSwapped += StaticEventHandler_OnPassiveItemsSwapped;
        StaticEventHandler.OnWeaponsSwappedWithInventory += StaticEventHandler_OnWeaponsSwappedWithInventory;

        // BUILD EVENTS
        StaticEventHandler.OnBuildInfoHovered += StaticEventHandler_OnBuildInfoHovered;
        StaticEventHandler.OnBuildInfoUnhovered += StaticEventHandler_OnBuildInfoUnhovered;

        // BEASTIARY EVENTS
        StaticEventHandler.OnMobUnlocked += StaticEventHandler_OnMobUnlocked;
        StaticEventHandler.OnMobHovered += StaticEventHandler_OnMobHovered;
        StaticEventHandler.OnMobUnhovered += StaticEventHandler_OnMobUnhovered;

        // WEAPON EVENTS
        StaticEventHandler.OnWeaponUnlocked += StaticEventHandler_OnWeaponUnlocked;
        StaticEventHandler.OnWeaponHovered += StaticEventHandler_OnWeaponHovered;
        StaticEventHandler.OnWeaponUnhovered += StaticEventHandler_OnWeaponUnhovered;

        // PASSIVE EVENTS
        StaticEventHandler.OnPassiveUnlocked += StaticEventHandler_OnPassiveUnlocked;
        StaticEventHandler.OnPassiveHovered += StaticEventHandler_OnPassiveHovered;
        StaticEventHandler.OnPassiveUnhovered += StaticEventHandler_OnPassiveUnhovered;

        // ACTIVE EVENTS
        StaticEventHandler.OnActiveUnlocked += StaticEventHandler_OnActiveUnlocked;
        StaticEventHandler.OnActiveHovered += StaticEventHandler_OnActiveHovered;
        StaticEventHandler.OnActiveUnhovered += StaticEventHandler_OnActiveUnhovered;

        StaticEventHandler.OnBookHealthChanged += StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnBookManaChanged += StaticEventHandler_OnBookManaChanged;
        StaticEventHandler.OnItemAddedToActiveItemSlot += StaticEventHandler_OnItemAddedToActiveItemSlot;
        StaticEventHandler.OnItemRemovedFromActiveItemSlot += StaticEventHandler_OnItemRemovedFromActiveItemSlot;
        StaticEventHandler.OnItemAddedToPassiveItemSlot += StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot += StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
        StaticEventHandler.OnBuildPointUsed += StaticEventHandler_OnBuildPointUsed;
        StaticEventHandler.OnLevelUp += StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnPrimaryStatsChanged += StaticEventHandler_OnPrimaryStatsChanged;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnBuildPageOpened -= StaticEventHandler_OnBuildPageOpened;

        // BOOK STAT POINTS
        StaticEventHandler.OnStatPointChanged -= StaticEventHandler_OnStatPointChanged;

        // BOOK WEAPON EVENTS
        StaticEventHandler.OnWeaponPickedUp -= StaticEventHandler_OnWeaponPickedUp;
        StaticEventHandler.OnWeaponSwitched -= StaticEventHandler_OnWeaponSwitched;
        StaticEventHandler.OnWeaponDropped -= StaticEventHandler_OnWeaponDropped;

        // BOOK INVENTORY EVENTS
        StaticEventHandler.OnWeaponAddedToInventory -= StaticEventHandler_OnWeaponAddedToInventory;
        StaticEventHandler.OnPassiveItemAddedToInventorySlot -= StaticEventHandler_OnPassiveItemAddedToInventorySlot;
        StaticEventHandler.OnInventoryWeaponDropped -= StaticEventHandler_OnInventoryWeaponDropped;
        StaticEventHandler.OnInventoryPassiveItemDropped -= StaticEventHandler_OnInventoryPassiveItemDropped;
        StaticEventHandler.OnPassiveItemsSwapped -= StaticEventHandler_OnPassiveItemsSwapped;
        StaticEventHandler.OnWeaponsSwappedWithInventory -= StaticEventHandler_OnWeaponsSwappedWithInventory;

        // BEASTIARY EVENTS
        StaticEventHandler.OnMobUnlocked -= StaticEventHandler_OnMobUnlocked;
        StaticEventHandler.OnMobHovered -= StaticEventHandler_OnMobHovered;
        StaticEventHandler.OnMobUnhovered -= StaticEventHandler_OnMobUnhovered;

        // WEAPON EVENTS
        StaticEventHandler.OnWeaponUnlocked -= StaticEventHandler_OnWeaponUnlocked;
        StaticEventHandler.OnWeaponHovered -= StaticEventHandler_OnWeaponHovered;
        StaticEventHandler.OnWeaponUnhovered -= StaticEventHandler_OnWeaponUnhovered;

        // PASSIVE EVENTS
        StaticEventHandler.OnPassiveUnlocked -= StaticEventHandler_OnPassiveUnlocked;
        StaticEventHandler.OnPassiveHovered -= StaticEventHandler_OnPassiveHovered;
        StaticEventHandler.OnPassiveUnhovered -= StaticEventHandler_OnPassiveUnhovered;

        // ACTIVE EVENTS
        StaticEventHandler.OnActiveUnlocked -= StaticEventHandler_OnActiveUnlocked;
        StaticEventHandler.OnActiveHovered -= StaticEventHandler_OnActiveHovered;
        StaticEventHandler.OnActiveUnhovered -= StaticEventHandler_OnActiveUnhovered;

        StaticEventHandler.OnBookHealthChanged -= StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnBookManaChanged -= StaticEventHandler_OnBookManaChanged;
        StaticEventHandler.OnItemAddedToActiveItemSlot -= StaticEventHandler_OnItemAddedToActiveItemSlot;
        StaticEventHandler.OnItemRemovedFromActiveItemSlot -= StaticEventHandler_OnItemRemovedFromActiveItemSlot;
        StaticEventHandler.OnItemAddedToPassiveItemSlot -= StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot -= StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
        StaticEventHandler.OnBuildPointUsed -= StaticEventHandler_OnBuildPointUsed;
        StaticEventHandler.OnLevelUp -= StaticEventHandler_OnLevelUp;
        StaticEventHandler.OnPrimaryStatsChanged -= StaticEventHandler_OnPrimaryStatsChanged;
    }

    private void Start()
    {
        activeUniqueSkillsContainer = buildPage.GetChild(0).GetChild(4);
        innerPathContainer = buildPage.GetChild(0).GetChild(5);
        buildDescriptonInnerPanel = buildPage.GetChild(0).GetChild(2);
        buildTextContainer = buildPage.GetChild(0).GetChild(9);
        buildPointsTransform = buildPage.GetChild(0).GetChild(10);
        buildTitleText = buildTextContainer.GetChild(0).GetComponent<TextMeshProUGUI>();
        buildDetailsText = buildTextContainer.GetChild(1).GetComponent<TextMeshProUGUI>();

        buildTitleText.text = string.Empty;
        buildDetailsText.text = string.Empty;

        currentAvailableStatPoints.text = player.currentStatPoints.ToString();

        PopulateCharactersBuildDetails();
    }

    private void Update()
    {
        if (player.currentStatPoints > 0) primaryStatsButtonContainer.gameObject.SetActive(true);
        else primaryStatsButtonContainer.gameObject.SetActive(false);
    }

    public void OnSelect(BaseEventData eventData)
    {
        if (weaponsPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (passivesPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (activesPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (beastiaryPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (bossesPage.GetChild(0).gameObject.activeSelf)
        {

        }
    }

    public void OnDeselect(BaseEventData eventData)
    {
        if (buildPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (weaponsPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (passivesPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (activesPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (beastiaryPage.GetChild(0).gameObject.activeSelf)
        {

        }
        else if (bossesPage.GetChild(0).gameObject.activeSelf)
        {

        }
    }

    private void StaticEventHandler_OnBuildPageOpened()
    {
        OpenBuildPage();
    }

    private void StaticEventHandler_OnBuildInfoHovered(BuildPointsArgs buildPointsArgs)
    {
        buildTitleText.text = player.playerDetails.charBuildDetails[buildPointsArgs.buildIndex].characterBuildName;
        buildDetailsText.text = player.playerDetails.charBuildDetails[buildPointsArgs.buildIndex].characterBuildDetails;
    }

    private void StaticEventHandler_OnBuildInfoUnhovered(BuildPointsArgs buildPointsArgs)
    {
        buildTitleText.text = string.Empty;
        buildDetailsText.text = string.Empty;
    }

    private void StaticEventHandler_OnWeaponPickedUp(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        StartCoroutine(WeaponPickUpRoutine(weaponAddedToBookArgs.pickedUpByOffHand, weaponAddedToBookArgs.weapon));
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

            if (weapon.weaponDetails.wieldType == WieldType.TwoHanded)
            {
                PlaceLockIcon();
            }
        }
    }

    private void StaticEventHandler_OnWeaponAddedToInventory(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        StartCoroutine(InventoryWeaponAddRoutine(weaponAddedToBookArgs.inventoryIndexNumber, weaponAddedToBookArgs.weapon));
    }

    IEnumerator InventoryWeaponAddRoutine(int index, Weapon weapon)
    {
        yield return new WaitForEndOfFrame();

        Transform equippedInventroySlot = inventoryParent.GetChild(index);
        equippedInventroySlot.GetComponent<Slot>().slotType = SlotType.WeaponMainHand;

        Transform inventoryItemBackground = equippedInventroySlot.GetChild(0);
        Transform inventoryItemEquipped = equippedInventroySlot.GetChild(1);
        inventoryItemBackground.gameObject.SetActive(false);
        inventoryItemEquipped.gameObject.SetActive(true);

        GameObject inventoryItem = Instantiate(GameResources.Instance.bookWeaponSlot, inventoryItemEquipped);

        // Set Draggable Item
        DraggableItem inventoryDraggableItem = inventoryItem.GetComponent<DraggableItem>();
        inventoryDraggableItem.SetDraggableItem(weapon, equippedInventroySlot.GetComponent<Slot>(), weapon.weaponDetails.weaponFrontSprite, ItemSlotStatus.Inventory);
    }

    private void StaticEventHandler_OnInventoryWeaponDropped(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        StartCoroutine(InventoryWeaponDropRoutine(weaponAddedToBookArgs.inventoryIndexNumber));
    }

    IEnumerator InventoryWeaponDropRoutine(int index)
    {
        yield return new WaitForEndOfFrame();

        Transform equippedInventroySlot = inventoryParent.GetChild(index);
        equippedInventroySlot.GetComponent<Slot>().slotType = SlotType.None; // Reset inventory slot

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
        yield return new WaitForEndOfFrame();

        Transform equippedInventroySlot = inventoryParent.GetChild(index);
        equippedInventroySlot.GetComponent<Slot>().slotType = SlotType.Passive;
        equippedInventroySlot.GetComponent<Slot>().passiveItemSlotName = passiveItem.passiveItemDetails.passiveItemSlotName;

        Transform inventoryItemBackground = equippedInventroySlot.GetChild(0);
        Transform inventoryItemEquipped = equippedInventroySlot.GetChild(1);
        inventoryItemBackground.gameObject.SetActive(false);
        inventoryItemEquipped.gameObject.SetActive(true);

        GameObject inventoryItem = Instantiate(GameResources.Instance.bookWeaponSlot, inventoryItemEquipped);

        // Set Draggable Item
        DraggableItem inventoryDraggableItem = inventoryItem.GetComponent<DraggableItem>();
        inventoryDraggableItem.SetDraggableItem(passiveItem, equippedInventroySlot.GetComponent<Slot>(), passiveItem.passiveItemDetails.passiveItemSprite, ItemSlotStatus.Inventory);
    }

    private void StaticEventHandler_OnInventoryPassiveItemDropped(PassiveItemAddedToBookArgs passiveItemAddedToBookArgs)
    {
        StartCoroutine(InventoryPassiveItemDropRoutine(passiveItemAddedToBookArgs.inventoryIndexNumber));
    }

    IEnumerator InventoryPassiveItemDropRoutine(int index)
    {
        yield return new WaitForEndOfFrame();

        Transform equippedInventroySlot = inventoryParent.GetChild(index);
        equippedInventroySlot.GetComponent<Slot>().slotType = SlotType.None; // Reset inventory slot
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
        Sprite slotWeaponSprite = slotWeapon.weaponDetails.weaponFrontSprite;
        Sprite inventoryWeaponSprite = inventoryWeapon.weaponDetails.weaponFrontSprite;

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

        UpdatePlayerStatInfo(GameManager.Instance.GetPlayer());
    }


    private void StaticEventHandler_OnPassiveItemsSwapped(PassiveItemAddedToBookArgs passiveItemAddedToBookArgs)
    {
        StartCoroutine(PassiveItemSwapRoutine(passiveItemAddedToBookArgs.passiveItem, passiveItemAddedToBookArgs.inventoryPassiveItem, passiveItemAddedToBookArgs.inventoryIndexNumber));
    }

    IEnumerator PassiveItemSwapRoutine(PassiveItem slotPassiveItem, PassiveItem inventoryPassiveItem, int index)
    {
        yield return new WaitUntil(() => !DraggableItem.IsDragging);
        yield return new WaitForEndOfFrame(); // Also wait one frame for UI updates to settle

        Transform equippedInventorySlot = inventoryParent.GetChild(index);
        Transform inventoryItemEquipped = equippedInventorySlot.GetChild(1);
        DraggableItem inventoryPassiveDraggableItem = inventoryItemEquipped.GetComponentInChildren<DraggableItem>();

        Transform slot = GetEquippedSlot(slotPassiveItem.passiveItemDetails.passiveItemSlotName);
        Transform slotEquipped = slot.GetChild(1);
        DraggableItem slotPassiveDraggableItem = slotEquipped.GetComponentInChildren<DraggableItem>();

        // Swap
        if (slotPassiveDraggableItem != null && inventoryPassiveDraggableItem != null)
        {
            SafeReparentDraggableItem(slotPassiveDraggableItem, inventoryItemEquipped);
            SafeReparentDraggableItem(inventoryPassiveDraggableItem, slotEquipped);
        }

        UpdatePlayerStatInfo(GameManager.Instance.GetPlayer());
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
        }
    }

    private void StaticEventHandler_OnWeaponSwitched()
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
                if (player.weaponSlotSetArray[player.currentWeaponSlotSetIndex - 1][0].weaponDetails.wieldType == WieldType.TwoHanded)
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

    private void StaticEventHandler_OnWeaponDropped(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        if (weaponAddedToBookArgs.slotType == SlotType.WeaponOffHand)
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
            if (weaponAddedToBookArgs.weapon != null && weaponAddedToBookArgs.weapon.weaponDetails.wieldType == WieldType.TwoHanded)
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
        newMainHandWeaponAtSlot.GetComponent<Image>().sprite = weapon.weaponDetails.weaponFrontSprite;
    }

    private void PlaceWeaponIconToOffhand(Weapon weapon)
    {
        GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
        offHandWeaponAtSlot.GetComponent<Image>().sprite = weapon.weaponDetails.weaponFrontSprite;
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
        Player player = GameManager.Instance.GetPlayer();
        healthValue.text = $"{player.health.GetCurrentHealth()} / {player.health.GetMaximumHealth()}";
    }

    private void StaticEventHandler_OnBookManaChanged(ManaChangedArgs manaChangedArgs)
    {
        Player player = GameManager.Instance.GetPlayer();
        manaValue.text = $"{player.mana.GetCurrentMana()} / {player.mana.GetMaximumMana()}";
    }

    private void StaticEventHandler_OnItemAddedToActiveItemSlot(SetSelectedActiveItemArgs itemAddedToBookArgs)
    {
        StartCoroutine(DelayedPickUpActiveItemSlot(itemAddedToBookArgs.activeItem));
    }

    IEnumerator DelayedPickUpActiveItemSlot(ActiveItem activeItem)
    {
        // Wait for end of frame
        yield return new WaitForEndOfFrame();

        Transform activeItemBackground = activeItemSlot.GetChild(0);
        Transform activeItemEquipped = activeItemSlot.GetChild(1);
        activeItemBackground.gameObject.SetActive(false);
        activeItemEquipped.gameObject.SetActive(true);
        GameObject activeItemGameObject = Instantiate(GameResources.Instance.bookWeaponSlot, activeItemEquipped);
        activeItemGameObject.GetComponent<Image>().sprite = activeItem.activeItemDetails.activeItemSprite;
    }

    private void StaticEventHandler_OnItemRemovedFromActiveItemSlot()
    {
        StartCoroutine(DelayedClearActiveItemSlot());
    }

    IEnumerator DelayedClearActiveItemSlot()
    {
        // Wait for end of frame so OnEndDrag completes
        yield return new WaitForEndOfFrame();

        Transform activeItemBackground = activeItemSlot.GetChild(0);
        Transform activeItemEquipped = activeItemSlot.GetChild(1);

        foreach (Transform item in activeItemSlot)
        {
            DestroyDraggableItems(item);
        }

        activeItemBackground.gameObject.SetActive(true);
        activeItemEquipped.gameObject.SetActive(false);
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
        background = passiveItemSlot.GetChild(0);
        equipped = passiveItemSlot.GetChild(1);
        background.gameObject.SetActive(false);
        equipped.gameObject.SetActive(true);

        // Loop through all child objects and destroy them
        for (int i = equipped.childCount - 1; i >= 0; i--)
        {
            Destroy(equipped.GetChild(i).gameObject);
        }

        passiveItemObject = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
        passiveItemObject.GetComponent<Image>().sprite = passiveItem.passiveItemDetails.passiveItemSprite;

        UpdatePlayerStatInfo(GameManager.Instance.GetPlayer());
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

        UpdatePlayerStatInfo(GameManager.Instance.GetPlayer());
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
            PassiveItemSlotName.Waist => passiveItemWaistSlot,
            PassiveItemSlotName.Arm => passiveItemArmSlot,
            PassiveItemSlotName.Leg => passiveItemLegSlot,
            _ => null,
        };
    }

    private void UpdatePlayerStatInfo(Player player)
    {
        float handling = player.currentWeaponHandlingValue ?? 0f;

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
        healthValue.text = $"{player.health.GetCurrentHealth()} / {player.health.GetMaximumHealth()}";
        manaValue.text = $"{player.mana.GetCurrentMana()} / {player.mana.GetMaximumMana()}";
        damageValue.text = $"{player.currentMainHandMinDamageValue}-{player.currentMainHandMaxDamageValue}({player.currentOffHandMinDamageValue}-" +
            $"{player.currentOffHandMaxDamageValue})";
        criticalHitChanceDamageValue.text = $"{player.currentMainHandCriticalHitChance * 100}%({player.currentOffHandCriticalHitChance * 100}%)";
        speedValue.text = $"{player.movementByVelocity.moveSpeed}";
        criticalHitDamageAmountValue.text = $"{player.currentMainHandCriticalHitDamage * 100}%({player.currentOffHandCriticalHitDamage * 100}%)";

        // AUXILLARY STATS
        chanceToHitValue.text = $"{handling * 100}%";
        blockRateValue.text = $"{player.currentBlockValue * 100}%";
        eveasivenessRateValue.text = $"{player.currentEvasivenessValue * 100}%";

        // RESISTANCE STATS
        physicalResistanceText.text = $"Armor : {player.currentArmorValue * 100} %";
        fireResistanceText.text = $"Fire : {player.currentFireResistanceValue * 100} %";
        waterResistanceText.text = $"Water : {player.currentWaterResistanceValue * 100} %";
        airResistanceText.text = $"Air : {player.currentAirResistanceValue * 100} %";
        earthResistanceText.text = $"Earth : {player.currentEarthResistanceValue * 100} %";
        lightResistanceText.text = $"Light : {player.currentLightResistanceValue * 100} %";
        darkResistanceText.text = $"Dark : {player.currentDarkResistanceValue * 100} %";
    }

    public void OpenBuildPage()
    {
        if (buildPage.GetChild(0).gameObject.activeSelf) return;

        bookStatPageOpen = false;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Build));
    }

    public void OpenStatsPage()
    {
        if (statsPage.GetChild(0).gameObject.activeSelf) return;

        bookStatPageOpen = false;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Stats));
    }

    public void OpenWeaponsPage()
    {
        if (weaponsPage.GetChild(0).gameObject.activeSelf) return;

        bookStatPageOpen = false;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Weapons));
    }

    public void OpenPassivesPage()
    {
        if (passivesPage.GetChild(0).gameObject.activeSelf) return;

        bookStatPageOpen = false;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Passives));
    }

    public void OpenActivesPage()
    {
        if (activesPage.GetChild(0).gameObject.activeSelf) return;

        bookStatPageOpen = false;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Actives));
    }

    public void OpenBestiaryPage()
    {
        if (beastiaryPage.GetChild(0).gameObject.activeSelf) return;

        bookStatPageOpen = false;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Beastiary));
    }

    public void OpenBossesPage()
    {
        if (bossesPage.GetChild(0).gameObject.activeSelf) return;

        bookStatPageOpen = false;

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
                case BookPage.Weapons:
                    weaponsPage.GetChild(0).gameObject.SetActive(true);
                    EnableWeaponsPage();
                    UpdateWeaponsPage();
                    break;
                case BookPage.Passives:
                    passivesPage.GetChild(0).gameObject.SetActive(true);
                    EnablePassivesPage();
                    UpdatePassivesPage();
                    break;
                case BookPage.Actives:
                    activesPage.GetChild(0).gameObject.SetActive(true);
                    EnableActivesPage();
                    UpdateActivesPage();
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
                case BookPage.Build:
                    EnableBuildsPage();
                    buildPage.GetChild(0).gameObject.SetActive(true);
                    buildDescriptonInnerPanel.gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }

    private void TurnThePage()
    {
        if (statsPage.GetChild(0).gameObject.activeSelf) { ClearStatPage(); }
        else if (weaponsPage.GetChild(0).gameObject.activeSelf) { ClearWeaponsPage(); }
        else if (passivesPage.GetChild(0).gameObject.activeSelf) { ClearPassivesPage(); }
        else if (activesPage.GetChild(0).gameObject.activeSelf) { ClearActivesPage(); }
        else if (beastiaryPage.GetChild(0).gameObject.activeSelf) { ClearBeastiaryPage();  }
        else if (bossesPage.GetChild(0).gameObject.activeSelf) { ClearBossesPage(); }
        else if (buildPage.GetChild(0).gameObject.activeSelf) { ClearBuildsPage(); }

        bookAnimator.enabled = false;
        bookAnimator.enabled = true;
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.openBookSoundEffect);
        bookAnimator.SetBool(Settings.turnPage, true);
    }

    private void PopulateCharactersBuildDetails()
    {
        for (int i = 0; i < innerPathContainer.childCount; i++)
        {
            //// Manipulate build title 
            //buildTreeFrame.GetChild(i).GetChild(buildTreeFrame.GetChild(i).childCount - 1).GetChild(0).GetComponent<TextMeshProUGUI>().text =
            //    player.playerDetails.charBuildDetails[i].characterBuildName;
            //// Manipulate detailed info title
            //buildTreeFrame.GetChild(i).GetChild(buildTreeFrame.GetChild(i).childCount - 1).GetChild(1).GetComponent<TextMeshProUGUI>().text =
            //    player.playerDetails.charBuildDetails[i].characterBuildDetails;
            // Manipulate build icon
            //innerPathContainer.GetChild(i).GetComponent<Image>().sprite = player.playerDetails.charBuildDetails[i].characterBuildImage;
        }
    }

    private void ClearStatPage()
    {
        foreach (Transform child in statsPage)
        {
            child.gameObject.SetActive(false);
        }

        bookStatPageOpen = false;
    }

    private void EnableStatPage()
    {
        foreach (Transform child in statsPage)
        {
            child.gameObject.SetActive(true);
        }

        bookStatPageOpen = true;
    }


    private void ClearWeaponsPage()
    {
        foreach (Transform child in weaponsPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void UpdateWeaponsPage()
    {
        for (int i = 0; i < weaponImageContainer.childCount; i++)
        {
            WeaponSlot weaponSlot = weaponImageContainer.GetChild(i).GetComponent<WeaponSlot>();

            if (weaponSlot.weaponUnlocked)
            {
                weaponSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                weaponSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void EnableWeaponsPage()
    {
        foreach (Transform child in weaponsPage)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void ClearPassivesPage()
    {
        foreach (Transform child in passivesPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void UpdatePassivesPage()
    {
        for (int i = 0; i < passivesImageContainer.childCount; i++)
        {
            PassiveSlot passiveSlot = passivesImageContainer.GetChild(i).GetComponent<PassiveSlot>();

            if (passiveSlot.passiveUnlocked)
            {
                passiveSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                passiveSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void EnablePassivesPage()
    {
        foreach (Transform child in passivesPage)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void ClearActivesPage()
    {
        foreach (Transform child in activesPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void UpdateActivesPage()
    {
        for (int i = 0; i < activesImageContainer.childCount; i++)
        {
            ActiveSlot activeSlot = activesImageContainer.GetChild(i).GetComponent<ActiveSlot>();

            if (activeSlot.activeUnlocked)
            {
                activeSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                activeSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void EnableActivesPage()
    {
        foreach (Transform child in activesPage)
        {
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
        foreach (Transform child in buildPage)
        {
            child.gameObject.SetActive(false);
        }
    }

    private void EnableBuildsPage()
    {
        foreach (Transform child in buildPage)
        {
            child.gameObject.SetActive(true);
        }
    }

    private void StaticEventHandler_OnMobUnlocked(MobUnlockArgs mobUnlockArgs)
    {
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

    private void StaticEventHandler_OnWeaponUnlocked(WeaponUnlockArgs weaponUnlockArgs)
    {
        for (int i = 0; i < weaponImageContainer.childCount; i++)
        {
            WeaponSlot weaponSlot = weaponImageContainer.GetChild(i).GetComponent<WeaponSlot>();

            if (weaponSlot.weaponUnlocked == true) continue;

            if (weaponSlot.weaponDetails.weaponTitle == weaponUnlockArgs.weaponTitle)
            {
                weaponSlot.weaponUnlocked = true;
                weaponSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                weaponSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void StaticEventHandler_OnWeaponHovered(WeaponHoverArgs weaponHoverArgs)
    {
        for (int i = 0; i < weaponImageContainer.childCount; i++)
        {
            WeaponSlot weaponSlot = weaponImageContainer.GetChild(i).GetComponent<WeaponSlot>();

            if (weaponSlot.weaponUnlocked == true && weaponSlot.weaponDetails.weaponTitle == weaponHoverArgs.weaponTitle)
            {
                weaponTitleText.text = weaponSlot.weaponDetails.weaponName;
                weaponDetailsText.text = "";
            }
        }
    }

    private void StaticEventHandler_OnWeaponUnhovered()
    {
        weaponTitleText.text = string.Empty;
        weaponDetailsText.text = string.Empty;
    }

    private void StaticEventHandler_OnPassiveUnlocked(PassiveUnlockArgs passiveUnlockArgs)
    {
        for (int i = 0; i < passivesImageContainer.childCount; i++)
        {
            PassiveSlot passiveSlot = passivesImageContainer.GetChild(i).GetComponent<PassiveSlot>();

            if (passiveSlot.passiveUnlocked == true) continue;

            if (passiveSlot.passiveItemDetails.passiveItemType == passiveUnlockArgs.passiveItemType)
            {
                passiveSlot.passiveUnlocked = true;
                passiveSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                passiveSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void StaticEventHandler_OnPassiveHovered(PassiveHoverArgs passiveHoverArgs)
    {
        for (int i = 0; i < passivesImageContainer.childCount; i++)
        {
            PassiveSlot passiveSlot = passivesImageContainer.GetChild(i).GetComponent<PassiveSlot>();

            if (passiveSlot.passiveUnlocked == true && passiveSlot.passiveItemDetails.passiveItemType == passiveHoverArgs.passiveItemType)
            {
                passivesTitleText.text = passiveSlot.passiveItemDetails.passiveItemName;
                passivesDetailsText.text = "";
            }
        }
    }

    private void StaticEventHandler_OnPassiveUnhovered()
    {
        passivesTitleText.text = string.Empty;
        passivesDetailsText.text = string.Empty;
    }

    private void StaticEventHandler_OnActiveUnlocked(ActiveUnlockArgs activeUnlockArgs)
    {
        for (int i = 0; i < activesImageContainer.childCount; i++)
        {
            ActiveSlot activeSlot = activesImageContainer.GetChild(i).GetComponent<ActiveSlot>();

            if (activeSlot.activeUnlocked == true) continue;

            if (activeSlot.activeItemDetails.activeItemType == activeUnlockArgs.activeItemType)
            {
                activeSlot.activeUnlocked = true;
                activeSlot.GetComponent<Image>().color = new Color(1f, 1f, 1f, 1f);
                activeSlot.GetComponent<Button>().interactable = true;
            }
        }
    }

    private void StaticEventHandler_OnActiveHovered(ActiveHoverArgs activeHoverArgs)
    {
        for (int i = 0; i < activesImageContainer.childCount; i++)
        {
            ActiveSlot activeSlot = activesImageContainer.GetChild(i).GetComponent<ActiveSlot>();

            if (activeSlot.activeUnlocked == true && activeSlot.activeItemDetails.activeItemType == activeHoverArgs.activeItemType)
            {
                activesTitleText.text = activeSlot.activeItemDetails.activeItemName;
                activesDetailsText.text = activeSlot.activeItemDetails.activeItemDetails;
            }
        }
    }

    private void StaticEventHandler_OnActiveUnhovered()
    {
        activesTitleText.text = string.Empty;
        activesDetailsText.text = string.Empty;
    }

    private void StaticEventHandler_OnPrimaryStatsChanged()
    {
        UpdatePlayerStatInfo(player);
    }

    private void StaticEventHandler_OnBuildPointUsed(BuildPointsArgs buildPointsArgs)
    {
        buildPointsTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.currentSkillPoints.ToString();
        UnlockBelowBuildIcon(buildPointsArgs.buildIndex);
        SoundEffectManager.Instance.PlaySoundEffect(player.playerDetails.buildActivationSoundEffect);
        ActivateBuild(buildPointsArgs);
    }

    private void ActivateBuild(BuildPointsArgs buildPointsArgs)
    {
        for (int i = 0; i < innerPathContainer.childCount; i++)
        {
            if (i == buildPointsArgs.buildIndex)
            {
                if (i == 0)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Ironheart Endurance
                            player.CurrentConstitutionValue++;
                            player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
                            break;
                        case Character.Morven:
                            // Shadow Endurance
                            player.CurrentConstitutionValue++;
                            player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
                            break;
                        case Character.Nyveran:
                            // Windrunner's Agility
                            player.CurrentAgilityValue++;
                            break;
                        case Character.Lyrisa:
                            // Granite Resolve
                            player.currentArmorValue += 0.1f;
                            break;
                    }
                }
                else if (i == 1)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Colossal Might
                            player.CurrentStrengthValue++; 
                            break;
                        case Character.Morven:
                            // Phantom Reflexes
                            player.CurrentDexterityValue++;
                            break;
                        case Character.Nyveran:
                            // Sharpshooter's Reflexes
                            player.CurrentDexterityValue++;
                            break;
                        case Character.Lyrisa:
                            // Mystic Insight
                            player.CurrentIntelligenceValue++;
                            break;
                    }
                }
                else if (i == 2)
                {
                    // Treasure Seeker
                    player.additionalCoinIncreaserModifier++;
                }
                else if (i == 3)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Adamant Bulwark
                            player.currentArmorValue += 0.1f;
                            break;
                        case Character.Morven:
                            // Blood Reaper
                            player.bloodDrainSkillAdditionalDamagePercentageModifier += 0.1f;
                            break;
                        case Character.Nyveran:
                            // Ranger Stride
                            player.additionalLightfeetSkillDurationModifier += 0.5f;
                            break;
                        case Character.Lyrisa:
                            // Aegis Mastery
                            player.barrierSkillAdditionalDurationModifier += 0.5f;
                            break;
                    }
                }
                else if (i == 4)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Earthshatter Slam
                            player.seismicSlamDamage = (int)(player.seismicSlamDamage * 1.5f);
                            break;
                        case Character.Morven:
                            // Assassin's Precision
                            player.additionalCriticalDamageOnCloakedPrecision++;
                            break;
                        case Character.Nyveran:
                            // Dead-eye Precision
                            player.additionalHeadShotDamageModifier += 0.5f;
                            break;
                        case Character.Lyrisa:
                            // Temporal Focus
                            player.additionalCastDurationModifier -= 0.15f;
                            break;
                    }
                }
                else if (i == 5)
                {
                    // Seasoned Explorer
                    player.expGainModifier *= 1.2f;
                }
                else if (i == 6)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Gemstone Skin
                            player.gemSkinBoostGainedDuringGemSkinActive = player.isGemSkinActive ? true : false;
                            //player.gemStoneSkillAdditionalModifier += 0.1f;
                            break;
                        case Character.Morven:
                            // Phantom Strength
                            player.CurrentStrengthValue++;
                            break;
                        case Character.Nyveran:
                            // Steady Resolve
                            player.CurrentConstitutionValue++;
                            player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
                            break;
                        case Character.Lyrisa:
                            // Ethereal Resilience
                            player.CurrentConstitutionValue++;
                            player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
                            break;
                    }
                }
                else if (i == 7)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Titan's Strength
                            player.CurrentStrengthValue++;
                            break;
                        case Character.Morven:
                            // Shadow Step
                            player.CurrentAgilityValue++;
                            break;
                        case Character.Nyveran:
                            // Rapid Execution
                            player.additionalBowAttackCoolDownModifier -= 0.1f;
                            break;
                        case Character.Lyrisa:
                            // Elemental Catacylsm
                            player.additionalCataclysmElementalDamageModifier += 0.3f;
                            break;
                    }
                }
                else if (i == 8)
                {
                    // Relic Hoarder
                    player.additionalActiveItemCharge++;
                    player.selectedActiveItem.GetCurrentActiveItem().activeItemMaxCharge += player.additionalActiveItemCharge;
                    player.selectedActiveItem.GetCurrentActiveItem().activeItemRemainingCharge += player.additionalActiveItemCharge;
                }
                else if (i == 9)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Defensive Stance
                            //player.blockSkillAdditionalDurationModifier += 0.5f;
                            break;
                        case Character.Morven:
                            // Lockpicking
                            player.additionalLockpickingModifier += 0.15f;
                            break;
                        case Character.Nyveran:
                            // Gale Dancer
                            player.additionalEvasivenessModifier += 0.1f;
                            player.UpdateBlockAndEvasivenessValues();
                            break;
                        case Character.Lyrisa:
                            // Voidstep Shield
                            player.threeSecInvincilibityAfterTeleportEnabled = true;
                            break;
                    }
                }
                else if (i == 10)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Berserker's Wrath
                            player.additionalCriticalMeleeDamageModifier += 0.3f;
                            break;
                        case Character.Morven:
                            // Deathmark Edge
                            player.additionalMeleeCriticalHitChanceModifier += 0.1f;
                            player.UpdateWeaponHandlingAndCriticalValues();
                            break;
                        case Character.Nyveran:
                            // Piercing Shot
                            player.additionalPenetrationSkillDamageModifier += 0.5f;
                            break;
                        case Character.Lyrisa:
                            // Runic Mastery
                            player.CurrentIntelligenceValue++;
                            break;
                    }
                }
                else if (i == 11)
                {
                    // Rare Instinct
                    ChestSpawner.rareChestLocateModifier += 0.1f;
                    ChestSpawner.legendaryChestLocateModifier += 0.05f;
                }
                else if (i == 12)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Unyielding Will
                            player.CurrentConstitutionValue++;
                            player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
                            break;
                        case Character.Morven:
                            // Shadow Clone Mastery
                            player.tripleTeamEnabled = true;
                            break;
                        case Character.Nyveran:
                            // Enduring Marksman
                            player.CurrentStrengthValue++;
                            player.currentArmorValue += 0.05f;
                            player.currentAirResistanceValue += 0.05f;
                            player.currentEarthResistanceValue += 0.05f;
                            player.currentFireResistanceValue += 0.05f;
                            player.currentWaterResistanceValue += 0.05f;
                            player.currentDarkResistanceValue += 0.05f;
                            player.currentLightResistanceValue += 0.05f;
                            break;
                        case Character.Lyrisa:
                            // Mindbender's Persuasion
                            player.additinalNPCCostModifier -= 0.2f;
                            break;
                    }
                }
                else if (i == 13)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Relentless Fury
                            player.additionalMeleeAttackCoolDownModifier -= 0.1f;
                            break;
                        case Character.Morven:
                            // Spectral Dexterity
                            player.CurrentDexterityValue++;
                            break;
                        case Character.Nyveran:
                            // Falcon's Grace
                            player.CurrentDexterityValue++;
                            break;
                        case Character.Lyrisa:
                            // Elemental Affinity
                            player.additionalStaffElementalDamageModifier += 0.1f;
                            player.UpdateDamageValues();
                            break;
                    }
                }
                else if (i == 14)
                {
                    // Fortune's Favor
                    player.additionalDropChanceModifier += 0.1f;
                }
                else if (i == 15)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Granite Resolve
                            player.currentArmorValue += 0.05f;
                            player.currentAirResistanceValue += 0.05f;
                            player.currentEarthResistanceValue += 0.05f;
                            player.currentFireResistanceValue += 0.05f;
                            player.currentWaterResistanceValue += 0.05f;
                            player.currentDarkResistanceValue += 0.05f;
                            player.currentLightResistanceValue += 0.05f;
                            break;
                        case Character.Morven:
                            // Elusive Phantom
                            player.additionalEvasivenessModifier += 0.1f;
                            player.UpdateBlockAndEvasivenessValues();
                            break;
                        case Character.Nyveran:
                            // Survival Instinct
                            player.additionalEvasivenessModifier += 0.1f;
                            player.UpdateBlockAndEvasivenessValues();
                            break;
                        case Character.Lyrisa:
                            // Hex Purification
                            player.additionalNegativeStatusEffectNegatorModifier += 0.1f;
                            break;
                    }
                }
                else if (i == 16)
                {
                    switch (player.playerDetails.playerCharacterIndex)
                    {
                        case Character.Caelion:
                            // Seismic Impact
                            player.seismicSlamCircleRadius *= 1.25f;
                            break;
                        case Character.Morven:
                            // Assassin's Wrath
                            player.additionalCriticalMeleeDamageModifier += 0.3f;
                            break;
                        case Character.Nyveran:
                            // Hawk's Focus
                            player.additionalBowAccuracyModifier += 0.4f;
                            break;
                        case Character.Lyrisa:
                            // Chrono Amplification
                            player.additionalCastDurationModifier -= 0.15f;
                            break;
                    }
                }
                else if (i == 17)
                {
                    // Legacy
                    player.CurrentStrengthValue++;
                    player.CurrentDexterityValue++;
                    player.CurrentConstitutionValue++;
                    player.CurrentIntelligenceValue++;
                    player.CurrentAgilityValue++;
                    player.healthEvent.CallHealthChangedEvent(player.health.currentHealth, 0, MeleeHand.None);
                }
            }
        }

        // Update book UI after new build unlocked
        UpdatePlayerStatInfo(player);
    }

    private void StaticEventHandler_OnStatPointChanged(StatChangedArgs statChangedArgs)
    {
        strengthValue.text = player.CurrentStrengthValue.ToString();
        constitutionValue.text = player.CurrentConstitutionValue.ToString();
        dexterityValue.text = player.CurrentDexterityValue.ToString();
        intelligenceValue.text = player.CurrentIntelligenceValue.ToString();
        agilityValue.text = player.CurrentAgilityValue.ToString();
        willpowerValue.text = player.CurrentWillpowerValue.ToString();
        resolveValue.text = player.CurrentResolveValue.ToString();
        ferocityValue.text = player.CurrentFerocityValue.ToString();

        currentAvailableStatPoints.text = player.currentStatPoints.ToString();
    }

    private void StaticEventHandler_OnLevelUp()
    {
        buildPointsTransform.GetChild(1).GetComponent<TextMeshProUGUI>().text = player.currentSkillPoints.ToString();
    }

    private void UnlockBelowBuildIcon(int indexNumber)
    {
        if (indexNumber < innerPathContainer.childCount - 3)
        {
            Image buildImage = innerPathContainer.GetChild(indexNumber + 3).GetComponent<Image>();
            buildImage.color = new Color(1f, 1f, 1f, 1f);

            //buildTreeFrame.GetChild(indexNumber + 3).GetChild(1).gameObject.SetActive(false);

            innerPathContainer.GetChild(indexNumber + 3).GetComponent<BuildSlot>().isLocked = false;
        }
    }

    public void SelectWeaponSetOne()
    {
        GameManager.Instance.GetPlayer().playerControl.NextWeaponSet(true, false, false, 1);
    }

    public void SelectWeaponSetTwo()
    {
        GameManager.Instance.GetPlayer().playerControl.NextWeaponSet(true, false, false, 2);
    }

    public void SelectWeaponSetThree()
    {
        GameManager.Instance.GetPlayer().playerControl.NextWeaponSet(true, false, false, 3);
    }
}
