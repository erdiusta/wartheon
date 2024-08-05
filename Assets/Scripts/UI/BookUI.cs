using System;
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

    Transform mainHandWeaponContainer;
    Transform offHandWeaponContainer;
    Transform passiveItemContainer;
    Transform activeItemContainer;

    private void Awake()
    {
        mainHandWeaponContainer = transform.GetChild(1).GetChild(1).GetChild(3).GetChild(0);
        offHandWeaponContainer = transform.GetChild(1).GetChild(1).GetChild(4).GetChild(0);
        passiveItemContainer = transform.GetChild(1).GetChild(1).GetChild(5).GetChild(0);
        activeItemContainer = transform.GetChild(1).GetChild(1).GetChild(6).GetChild(0);

        GameObject mainHandWeapon = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponContainer);

        Player player = GameManager.Instance.GetPlayer();

        characterName.text = player.playerDetails.playerCharacterName;
        healthText.text = $"Health : {player.health.GetCurrentHealth()} / {player.health.GetStartingHealth()}"; 
        weaponText.text = $"Pr.Weapon : {player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponName}";
        speedText.text = $"Speed : {player.movementByVelocity.GetCurrentMoveSpeed()}";
        specialMoveText.text = $"Sp.Move : {player.playerDetails.specialMoveName}";       

        switch (player.playerDetails.playerCharacterName)
        {
            case Settings.astraeus:
            case Settings.erebus:
                characterImage.sprite = player.playerDetails.playerBookSprite;

                mainHandWeapon.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[0].weaponFrontSprite;
                player.weaponBookMainHandHashSet.Add(player.playerDetails.startingWeaponList[0].weaponFrontSprite);

                GameObject offHandWeapon = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponContainer);
                offHandWeapon.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[1].weaponFrontSprite;
                player.weaponBookOffHandHashSet.Add(player.playerDetails.startingWeaponList[1].weaponFrontSprite);
                break;

            case Settings.orion:
            case Settings.lyrisa:
                characterImage.sprite = player.playerDetails.playerBookSprite;

                mainHandWeapon.GetComponent<Image>().sprite = player.playerDetails.startingWeaponList[0].weaponFrontSprite;
                player.weaponBookMainHandHashSet.Add(player.playerDetails.startingWeaponList[0].weaponFrontSprite);
                break;

            default:
                break;
        }

        GameObject activeItem = Instantiate(GameResources.Instance.bookWeaponSlot, activeItemContainer);
        activeItem.GetComponent<Image>().sprite = player.playerDetails.activeItemsList[0].activeItemSprite;

        GameObject passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, passiveItemContainer);
        passiveItem.GetComponent<Image>().sprite = player.playerDetails.passiveItemsList[0].passiveItemSprite;
    }

    private void OnEnable()
    {
        StaticEventHandler.OnWeaponAddedToMainHandBook += StaticEventHandler_OnWeaponAddedToMainHandBook;
        StaticEventHandler.OnWeaponAddedToOffHandBook += StaticEventHandler_OnWeaponAddedToOffHandBook;
        StaticEventHandler.OnBookHealthChanged += StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnItemAddedToActiveItemSlot += StaticEventHandler_OnItemAddedToActiveItemSlot;
        StaticEventHandler.OnItemRemovedFromActiveItemSlot += StaticEventHandler_OnItemRemovedFromActiveItemSlot;
        StaticEventHandler.OnItemAddedToPassiveItemSlot += StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot += StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnWeaponAddedToMainHandBook -= StaticEventHandler_OnWeaponAddedToMainHandBook;
        StaticEventHandler.OnWeaponAddedToOffHandBook -= StaticEventHandler_OnWeaponAddedToOffHandBook;
        StaticEventHandler.OnBookHealthChanged -= StaticEventHandler_OnBookHealthChanged;
        StaticEventHandler.OnItemAddedToActiveItemSlot -= StaticEventHandler_OnItemAddedToActiveItemSlot;
        StaticEventHandler.OnItemRemovedFromActiveItemSlot -= StaticEventHandler_OnItemRemovedFromActiveItemSlot;
        StaticEventHandler.OnItemAddedToPassiveItemSlot -= StaticEventHandler_OnItemAddedToPassiveItemSlot;
        StaticEventHandler.OnItemRemovedFromPassiveItemSlot -= StaticEventHandler_OnItemRemovedFromPassiveItemSlot;
    }

    private void StaticEventHandler_OnWeaponAddedToMainHandBook(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        GameObject mainHandWeapon = Instantiate(GameResources.Instance.bookWeaponSlot, mainHandWeaponContainer);

        if (mainHandWeapon.GetComponent<Image>().sprite == null)
        {
            mainHandWeapon.GetComponent<Image>().sprite = weaponAddedToBookArgs.weaponSprite;
        }
    }

    private void StaticEventHandler_OnWeaponAddedToOffHandBook(WeaponAddedToBookArgs weaponAddedToBookArgs)
    {
        GameObject offHandWeapon = Instantiate(GameResources.Instance.bookWeaponSlot, offHandWeaponContainer);
        offHandWeapon.GetComponent<Image>().sprite = weaponAddedToBookArgs.weaponSprite;
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
        GameObject passiveItem = Instantiate(GameResources.Instance.bookWeaponSlot, passiveItemContainer);
        passiveItem.GetComponent<Image>().sprite = itemAddedToBookArgs.itemSprite;
    }

    private void StaticEventHandler_OnItemRemovedFromPassiveItemSlot(ItemRemovedFromBookArgs itemRemovedFromBookArgs)
    {
        GameObject passiveItemGameObject = passiveItemContainer.GetChild(passiveItemContainer.childCount - 1).gameObject;
        Destroy(passiveItemGameObject);
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
