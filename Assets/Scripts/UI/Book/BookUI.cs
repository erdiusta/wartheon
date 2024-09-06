using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BookUI : MonoBehaviour
{
    [Header("PAGE HEADERS")]
    [Space(10)]
    public Transform statsPage;
    public Transform weaponsPage;
    public Transform itemsPage;
    public Transform beastiaryPage;
    public Transform bossesPage;

    [SerializeField] TextMeshProUGUI characterName;
    [SerializeField] Image characterImage;

    [SerializeField] TextMeshProUGUI healthText;
    [SerializeField] TextMeshProUGUI weaponText;
    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] TextMeshProUGUI specialMoveText;

    [SerializeField] Animator bookAnimator;

    Transform mainHandWeaponSlot;
    Transform offHandWeaponSlot;

    Transform activeItemContainer;

    [Header("Passive Item Slots")]
    Transform passiveItemHeadSlot;
    Transform passiveItemChestSlot;
    Transform passiveItemNeckSlot;
    Transform passiveItemArmSlot;
    Transform passiveItemBackSlot;
    Transform passiveItemLegSlot;
    Transform passiveItemWaistSlot;
    Transform passiveItemFingerSlot1;
    Transform passiveItemFingerSlot2;
    Transform passiveItemAccessorySlot1;
    Transform passiveItemAccessorySlot2;

    private void Awake()
    {
        // Main Hand Slot
        mainHandWeaponSlot = transform.GetChild(1).GetChild(1).GetChild(3).GetChild(1);

        // Off-hand Slot
        offHandWeaponSlot = transform.GetChild(1).GetChild(1).GetChild(4).GetChild(1);

        // Active Item Slot
        activeItemContainer = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(0);

        // Passive Item Slots
        passiveItemHeadSlot = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(0);
        passiveItemChestSlot = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(1);
        passiveItemNeckSlot = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(2);
        passiveItemArmSlot = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(3);
        passiveItemBackSlot = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(4);
        passiveItemLegSlot = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(5);
        passiveItemWaistSlot = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(6);
        passiveItemFingerSlot1 = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(7);
        passiveItemFingerSlot2 = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(8);
        passiveItemAccessorySlot1 = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(9);
        passiveItemAccessorySlot2 = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(10);

        Player player = GameManager.Instance.GetPlayer();

        characterName.text = player.playerDetails.playerCharacterName;
        healthText.text = $"Health : {player.health.GetCurrentHealth()} / {player.health.GetStartingHealth()}"; 
        weaponText.text = $"Pr.Weapon : {player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponName}";
        speedText.text = $"Speed : {player.movementByVelocity.GetCurrentMoveSpeed()}";
      
        switch (player.playerDetails.playerCharacterName)
        {
            case Settings.astraeus:
            case Settings.erebus:
                characterImage.sprite = player.playerDetails.playerBookSprite;

                // MAIN HAND WEAPON EQUIP AT START - SLOT
                Transform mainWeaponBackground = mainHandWeaponSlot.GetChild(0);
                Transform mainWeaponEquipped = mainHandWeaponSlot.GetChild(1);
                mainWeaponBackground.gameObject.SetActive(false);
                mainWeaponEquipped.gameObject.SetActive(true);
                GameObject mainHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, mainWeaponEquipped);
                mainHandWeaponAtSlot.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[0].weaponFrontSprite;

                // OFF HAND WEAPON EQUIP AT START - SLOT
                Transform offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
                Transform offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);
                offHandWeaponBackground.gameObject.SetActive(false);
                offHandWeaponEquipped.gameObject.SetActive(true);
                GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
                offHandWeaponAtSlot.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[1].weaponFrontSprite;
                break;

            case Settings.orion:
            case Settings.lyrisa:
                characterImage.sprite = player.playerDetails.playerBookSprite;

                // WEAPON EQUIP AT START - SLOT
                mainWeaponBackground = mainHandWeaponSlot.GetChild(0);
                mainWeaponEquipped = mainHandWeaponSlot.GetChild(1);
                mainWeaponBackground.gameObject.SetActive(false);
                mainWeaponEquipped.gameObject.SetActive(true);
                mainHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, mainWeaponEquipped);
                mainHandWeaponAtSlot.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[0].weaponFrontSprite;

                // OFF HAND WEAPON EQUIP AT START - SLOT
                offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
                offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);
                offHandWeaponBackground.gameObject.SetActive(false);
                offHandWeaponEquipped.gameObject.SetActive(true);
                offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
                offHandWeaponAtSlot.GetComponent<Image>().sprite = GameResources.Instance.lockSlotIcon;
                break;

            default:
                break;
        }

        // ACTIVE ITEM EQUIP AT START
        Transform activeBackground = activeItemContainer.GetChild(0);
        Transform activeEquipped = activeItemContainer.GetChild(1);
        activeBackground.gameObject.SetActive(false);
        activeEquipped.gameObject.SetActive(true);
        GameObject activeItem = Instantiate(GameResources.Instance.bookWeaponSlot, activeEquipped);
        activeItem.GetComponent<Image>().sprite = player.playerDetails.activeItemsList[0].activeItemSprite;

        // PASSIVE EQUIPS AT START
        GameObject passiveItem;
        Transform passiveBackground;
        Transform passiveEquipped;

        switch (player.playerDetails.playerCharacterName)
        {
            case Settings.astraeus:
                passiveBackground = passiveItemFingerSlot1.GetChild(0);
                passiveEquipped = passiveItemFingerSlot1.GetChild(1);
                passiveBackground.gameObject.SetActive(false);
                passiveEquipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, passiveEquipped);
                passiveItem.GetComponent<Image>().sprite = player.playerDetails.passiveItemsList[0].passiveItemSprite;
                break;

            case Settings.erebus:
                passiveBackground = passiveItemBackSlot.GetChild(0);
                passiveEquipped = passiveItemBackSlot.GetChild(1);
                passiveBackground.gameObject.SetActive(false);
                passiveEquipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, passiveEquipped);
                passiveItem.GetComponent<Image>().sprite = player.playerDetails.passiveItemsList[0].passiveItemSprite;
                break;

            case Settings.orion:
                passiveBackground = passiveItemHeadSlot.GetChild(0);
                passiveEquipped = passiveItemHeadSlot.GetChild(1);
                passiveBackground.gameObject.SetActive(false);
                passiveEquipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, passiveEquipped);
                passiveItem.GetComponent<Image>().sprite = player.playerDetails.passiveItemsList[0].passiveItemSprite;
                break;

            case Settings.lyrisa:
                passiveBackground = passiveItemWaistSlot.GetChild(0);
                passiveEquipped = passiveItemWaistSlot.GetChild(1);
                passiveBackground.gameObject.SetActive(false);
                passiveEquipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, passiveEquipped);
                passiveItem.GetComponent<Image>().sprite = player.playerDetails.passiveItemsList[0].passiveItemSprite;
                break;

            default:
                break;
        }
    }

    private void OnEnable()
    {
        StaticEventHandler.OnWeaponAddedToMainHandBook += StaticEventHandler_OnWeaponAddedToMainHandBook;
        StaticEventHandler.OnWeaponSwappedAtMainHand += StaticEventHandler_OnWeaponSwappedAtMainHand;
        StaticEventHandler.OnWeaponRemovedFromMainHandBook += StaticEventHandler_OnWeaponRemovedFromMainHandBook;
        StaticEventHandler.OnWeaponAddedToOffHandBook += StaticEventHandler_OnWeaponAddedToOffHandBook;
        StaticEventHandler.OnWeaponSwappedAtOffHand += StaticEventHandler_OnWeaponSwappedAtOffHand;
        StaticEventHandler.OnWeaponRemovedFromOffHandBook += StaticEventHandler_OnWeaponRemovedFromOffHandBook;
        StaticEventHandler.OnBookHealthChanged += StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnItemAddedToActiveItemSlot += StaticEventHandler_OnItemAddedToActiveItemSlot;
        StaticEventHandler.OnItemRemovedFromActiveItemSlot += StaticEventHandler_OnItemRemovedFromActiveItemSlot;
        StaticEventHandler.OnItemAddedToPassiveItemSlot += StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot += StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnWeaponAddedToMainHandBook -= StaticEventHandler_OnWeaponAddedToMainHandBook;
        StaticEventHandler.OnWeaponSwappedAtMainHand -= StaticEventHandler_OnWeaponSwappedAtMainHand;
        StaticEventHandler.OnWeaponRemovedFromMainHandBook -= StaticEventHandler_OnWeaponRemovedFromMainHandBook;
        StaticEventHandler.OnWeaponAddedToOffHandBook -= StaticEventHandler_OnWeaponAddedToOffHandBook;
        StaticEventHandler.OnWeaponSwappedAtOffHand -= StaticEventHandler_OnWeaponSwappedAtOffHand;
        StaticEventHandler.OnWeaponRemovedFromOffHandBook -= StaticEventHandler_OnWeaponRemovedFromOffHandBook;
        StaticEventHandler.OnBookHealthChanged -= StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnItemAddedToActiveItemSlot -= StaticEventHandler_OnItemAddedToActiveItemSlot;
        StaticEventHandler.OnItemRemovedFromActiveItemSlot -= StaticEventHandler_OnItemRemovedFromActiveItemSlot;
        StaticEventHandler.OnItemAddedToPassiveItemSlot -= StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot -= StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
    }

    private void StaticEventHandler_OnWeaponAddedToMainHandBook(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        // OFF-HAND WEAPON EQUIP AT START - SLOT
        if (weaponAddedToBookArgs.weapon.weaponDetails.wieldType == WieldType.TwoHanded)
        {
            Transform offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
            Transform offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);
            offHandWeaponBackground.gameObject.SetActive(false);
            offHandWeaponEquipped.gameObject.SetActive(true);
            GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
            offHandWeaponAtSlot.GetComponent<Image>().sprite = GameResources.Instance.lockSlotIcon;
            offHandWeaponAtSlot.GetComponent<DraggableItem>().isLockIcon = true;
        }
        else
        {
            // Destroy lock icon at off-hand if changing main weapon from two-handed to one-handed
            if (!weaponAddedToBookArgs.onStart)
            {
                Transform offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
                Transform offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);

                for (int i = offHandWeaponEquipped.childCount - 1; i >= 0; i--)
                {
                    GameObject offHandWeaponAtSlot = offHandWeaponEquipped.GetChild(i).gameObject;
                    Destroy(offHandWeaponAtSlot);
                }

                offHandWeaponBackground.gameObject.SetActive(true);
                offHandWeaponEquipped.gameObject.SetActive(false);

            }
        }

        // Destroy previous main hand slot before new weapon replaces it
        Transform mainHandWeaponBackground = mainHandWeaponSlot.GetChild(0);
        Transform mainHandWeaponEquipped = mainHandWeaponSlot.GetChild(1);
        mainHandWeaponBackground.gameObject.SetActive(true);
        mainHandWeaponEquipped.gameObject.SetActive(false);

        for (int i = mainHandWeaponEquipped.childCount - 1; i >= 0; i--)
        {
            GameObject mainHandWeaponAtSlot = mainHandWeaponEquipped.GetChild(i).gameObject;
            Destroy(mainHandWeaponAtSlot);
        }

        // Place new weapon icon to the slot
        mainHandWeaponBackground.gameObject.SetActive(false);
        mainHandWeaponEquipped.gameObject.SetActive(true);
        GameObject newMainHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponEquipped);
        newMainHandWeaponAtSlot.GetComponent<Image>().sprite = weaponAddedToBookArgs.weapon.weaponDetails.weaponFrontSprite;
    }

    private void StaticEventHandler_OnWeaponSwappedAtMainHand(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        Transform mainHandWeaponBackground = mainHandWeaponSlot.GetChild(0);
        Transform mainHandWeaponEquipped = mainHandWeaponSlot.GetChild(1);
        mainHandWeaponBackground.gameObject.SetActive(false);
        mainHandWeaponEquipped.gameObject.SetActive(true);
        GameObject newMainHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponEquipped);
        newMainHandWeaponAtSlot.GetComponent<Image>().sprite = weaponAddedToBookArgs.weapon.weaponDetails.weaponFrontSprite;
    }

    private void StaticEventHandler_OnWeaponRemovedFromMainHandBook()
    {
        Transform mainHandWeaponBackground = mainHandWeaponSlot.GetChild(0);
        Transform mainHandWeaponEquipped = mainHandWeaponSlot.GetChild(1);
        mainHandWeaponBackground.gameObject.SetActive(true);
        mainHandWeaponEquipped.gameObject.SetActive(false);

        for (int i = mainHandWeaponEquipped.childCount - 1; i >= 0; i--)
        {
            GameObject HandWeaponAtSlot = mainHandWeaponEquipped.GetChild(i).gameObject;
            Destroy(mainHandWeaponEquipped);
        }

        if (mainHandWeaponEquipped.childCount == 0)
        {
            mainHandWeaponBackground.gameObject.SetActive(true);
            mainHandWeaponEquipped.gameObject.SetActive(false);
        }
    }

    private void StaticEventHandler_OnWeaponAddedToOffHandBook(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        // OFF-HAND WEAPON EQUIP AT START - SLOT
        Transform offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
        Transform offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);
        offHandWeaponBackground.gameObject.SetActive(false);
        offHandWeaponEquipped.gameObject.SetActive(true);
        GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
        offHandWeaponAtSlot.GetComponent<Image>().sprite = weaponAddedToBookArgs.weapon.weaponDetails.weaponFrontSprite;
    }

    private void StaticEventHandler_OnWeaponSwappedAtOffHand(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        Transform offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
        Transform offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);
        offHandWeaponBackground.gameObject.SetActive(false);
        offHandWeaponEquipped.gameObject.SetActive(true);
        GameObject newOffHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
        newOffHandWeaponAtSlot.GetComponent<Image>().sprite = weaponAddedToBookArgs.weapon.weaponDetails.weaponFrontSprite;
    }

    private void StaticEventHandler_OnWeaponRemovedFromOffHandBook()
    {
        Transform offHandWeaponBackground = offHandWeaponSlot.GetChild(0);
        Transform offHandWeaponEquipped = offHandWeaponSlot.GetChild(1);

        // Loop through all child objects and destroy them
        for (int i = offHandWeaponEquipped.childCount - 1; i >= 0; i--)
        {
            GameObject offHandWeaponAtSlot = offHandWeaponEquipped.GetChild(i).gameObject;
            Destroy(offHandWeaponAtSlot);
        }

        offHandWeaponBackground.gameObject.SetActive(true);
        offHandWeaponEquipped.gameObject.SetActive(false);

        // OFF-HAND WEAPON EQUIP AT START - SLOT
        if (GameManager.Instance.GetPlayer().activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
        {
            offHandWeaponBackground.gameObject.SetActive(false);
            offHandWeaponEquipped.gameObject.SetActive(true);
            GameObject offHandWeaponAtSlot = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponEquipped);
            offHandWeaponAtSlot.GetComponent<Image>().sprite = GameResources.Instance.lockSlotIcon;
            offHandWeaponAtSlot.GetComponent<DraggableItem>().isLockIcon = true;
        }
    }

    private void StaticEventHandler_OnBookHealthChanged(HealthChangedArgs healthChangedArgs)
    {
        Player player = GameManager.Instance.GetPlayer();
        healthText.text = $"Health : {healthChangedArgs.currentHealth} / {player.health.GetStartingHealth()}";
    }

    private void StaticEventHandler_OnItemAddedToActiveItemSlot(ItemAddedToBookArgs itemAddedToBookArgs)
    {
        GameObject activeItem = Instantiate(GameResources.Instance.bookWeaponSlot, activeItemContainer);
        activeItem.GetComponent<Image>().sprite = itemAddedToBookArgs.itemSprite;
    }

    private void StaticEventHandler_OnItemRemovedFromActiveItemSlot()
    {
        GameObject activeItemImageObject = activeItemContainer.GetChild(activeItemContainer.childCount - 1).gameObject;
        Destroy(activeItemImageObject);
    }

    private void StaticEventHandler_OnItemAddedToPassiveItemSlot(ItemAddedToBookArgs itemAddedToBookArgs)
    {
        GameObject passiveItem = new GameObject();
        Transform background;
        Transform equipped;

        switch (itemAddedToBookArgs.itemSlotName)
        {
            case ItemSlotName.None:
                break;
            case ItemSlotName.Head:
                background = passiveItemHeadSlot.GetChild(0);
                equipped = passiveItemHeadSlot.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Chest:
                background = passiveItemChestSlot.GetChild(0);
                equipped = passiveItemChestSlot.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Neck:
                background = passiveItemNeckSlot.GetChild(0);
                equipped = passiveItemNeckSlot.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Finger:
                background = passiveItemFingerSlot1.GetChild(0);
                equipped = passiveItemFingerSlot1.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Back:
                background = passiveItemBackSlot.GetChild(0);
                equipped = passiveItemBackSlot.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Waist:
                background = passiveItemWaistSlot.GetChild(0);
                equipped = passiveItemWaistSlot.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Arm:
                background = passiveItemArmSlot.GetChild(0);
                equipped = passiveItemArmSlot.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Leg:
                background = passiveItemLegSlot.GetChild(0);
                equipped = passiveItemLegSlot.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            case ItemSlotName.Accessory:
                background = passiveItemAccessorySlot1.GetChild(0);
                equipped = passiveItemAccessorySlot1.GetChild(1);
                background.gameObject.SetActive(false);
                equipped.gameObject.SetActive(true);
                passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, equipped);
                break;
            default:
                break;
        }

        passiveItem.GetComponent<Image>().sprite = itemAddedToBookArgs.itemSprite;
    }

    private void StaticEventHandler_OnItemRemovedFromPassiveItemSlot(ItemRemovedFromBookArgs itemRemovedFromBookArgs)
    {
        GameObject passiveItem = new GameObject();
        Transform background;
        Transform equipped;

        switch (itemRemovedFromBookArgs.itemSlotName)
        {
            case ItemSlotName.None:
                break;
            case ItemSlotName.Head:
                background = passiveItemHeadSlot.GetChild(0);
                equipped = passiveItemHeadSlot.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Chest:
                background = passiveItemChestSlot.GetChild(0);
                equipped = passiveItemChestSlot.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Neck:
                background = passiveItemNeckSlot.GetChild(0);
                equipped = passiveItemNeckSlot.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Finger:
                background = passiveItemFingerSlot1.GetChild(0);
                equipped = passiveItemFingerSlot1.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Back:
                background = passiveItemBackSlot.GetChild(0);
                equipped = passiveItemBackSlot.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Waist:
                background = passiveItemWaistSlot.GetChild(0);
                equipped = passiveItemWaistSlot.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Arm:
                background = passiveItemArmSlot.GetChild(0);
                equipped = passiveItemArmSlot.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Leg:
                background = passiveItemLegSlot.GetChild(0);
                equipped = passiveItemLegSlot.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            case ItemSlotName.Accessory:
                background = passiveItemAccessorySlot1.GetChild(0);
                equipped = passiveItemAccessorySlot1.GetChild(1);
                passiveItem = equipped.GetChild(0).gameObject;
                background.gameObject.SetActive(true);
                equipped.gameObject.SetActive(false);
                break;
            default:
                break;
        }

        Destroy(passiveItem);
    }

    public void OpenStatsPage()
    {
        if (statsPage.GetChild(0).gameObject.activeSelf) return;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Stats));
    }

    public void OpenWeaponsPage()
    {
        if (weaponsPage.GetChild(0).gameObject.activeSelf) return;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Weapons));
    }

    public void OpenItemPage()
    {
        if (itemsPage.GetChild(0).gameObject.activeSelf) return;

        StopAllCoroutines();
        StartCoroutine(CompleteTurnPageThenDisplay(BookPage.Items));
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
                case BookPage.Weapons:
                    weaponsPage.GetChild(0).gameObject.SetActive(true);
                    break;
                case BookPage.Items:
                    itemsPage.GetChild(0).gameObject.SetActive(true);
                    break;
                case BookPage.Beastiary:
                    beastiaryPage.GetChild(0).gameObject.SetActive(true);
                    break;
                case BookPage.Bosses:
                    bossesPage.GetChild(0).gameObject.SetActive(true);
                    break;
                default:
                    break;
            }
        }
    }

    private void TurnThePage()
    {
        if (statsPage.GetChild(0).gameObject.activeSelf) { ClearStatPage(); }
        else if (weaponsPage.GetChild(0).gameObject.activeSelf) { weaponsPage.GetChild(0).gameObject.SetActive(false); }
        else if (itemsPage.GetChild(0).gameObject.activeSelf) { itemsPage.GetChild(0).gameObject.SetActive(false); }
        else if (beastiaryPage.GetChild(0).gameObject.activeSelf) { beastiaryPage.GetChild(0).gameObject.SetActive(false); }
        else if (bossesPage.GetChild(0).gameObject.activeSelf) { bossesPage.GetChild(0).gameObject.SetActive(false); }

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
            child.gameObject.SetActive(true);
        }
    }
}
