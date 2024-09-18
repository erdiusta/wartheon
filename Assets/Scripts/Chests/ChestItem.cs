using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using Random = UnityEngine.Random;

public class ChestItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Transform tooltipPanel;

    [HideInInspector] public bool hasWeaponDrop = false;
    [HideInInspector] public bool hasActiveDrop = false;
    [HideInInspector] public bool hasPrimaryPassiveDrop = false;
    [HideInInspector] public bool hasSecondaryPassiveDrop = false;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public bool isPickedUp = false;
    [HideInInspector] public Animator animator;
    [HideInInspector] public BoxCollider2D boxCollider2D;
    [HideInInspector] public int remainingItemCharge;
    [HideInInspector] public bool droppedByPlayer = false;
    [HideInInspector] public ActiveItem toBeDroppedActiveItem;
    [HideInInspector] public PassiveItem toBeDroppedPassiveItem;
    [HideInInspector] public bool isColliding;
    [HideInInspector] public bool hasMainHandWeapon;
    [HideInInspector] public bool hasOffHandWeapon;
    [HideInInspector] public static ChestItem toBeDroppedChestItem;
    [HideInInspector] public static ChestItem nearestChestItem = null;

    Chest chest;
    Enemy enemy;
    WeaponDetailsSO toBeDroppedMainWeaponDetails;
    WeaponDetailsSO toBeDroppedOffWeaponDetails;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    ActiveItemDetailsSO activeItemDetails;
    int ammoPercent;
    bool isPurchasing;
    Animator pickUpAnimator;

    // Tooltip Panel Weapon Texts
    [Header("TOOLTIP PANEL FOR WEAPONS")]
    [Space(10)]
    [SerializeField] TextMeshPro headerText;
    [SerializeField] TextMeshPro levelText;
    [SerializeField] TextMeshPro weaponClassText;
    [SerializeField] TextMeshPro hitSpeedText;
    [SerializeField] TextMeshPro weaponWieldText;
    [SerializeField] TextMeshPro damageText;
    [SerializeField] TextMeshPro baseHandlingText;
    [SerializeField] TextMeshPro crHitChanceText;
    [SerializeField] TextMeshPro crHitDamageText;
    [SerializeField] TextMeshPro elementalBiasText;
    [SerializeField] TextMeshPro elementText;
    [SerializeField] TextMeshPro elementalForgeRateText;
    [SerializeField] TextMeshPro masteryText1;
    [SerializeField] TextMeshPro masteryText2;
    [SerializeField] TextMeshPro masteryText3;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        pickUpAnimator = transform.GetChild(1).GetComponent<Animator>();
        chest = GetComponentInParent<Chest>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void OnEnable()
    {
        if (transform.parent != null)
        {
            if (transform.parent.tag == Settings.enemyTag)
            {
                enemy = GetComponentInParent<Enemy>();
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter.GetComponent<ChestItem>() == this && eventData.pointerEnter.GetComponentInParent<Player>() == null)
        {
            tooltipPanel.gameObject.SetActive(true);
            UpdateTooltipPanelInfo();
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tooltipPanel.gameObject.SetActive(false);
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.meteor ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            Player player = collision.GetComponent<Player>();

            if (chest == null)
            {
                try
                {
                    if(isColliding) return;

                    // Calculate the distance between the chest item and the player
                    float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

                    // Check if there's currently a nearest chest item and if it's valid
                    if (nearestChestItem == null || nearestChestItem == this || (nearestChestItem != null && Vector2.Distance(player.transform.position, nearestChestItem.transform.position) 
                        > distanceToPlayer))
                    {
                        nearestChestItem = this;
                    }

                    // Only allow the nearest chest item to be interacted with
                    if (nearestChestItem == this)
                    {
                        animator.SetBool(Settings.hovered, true);

                        if (hasWeaponDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                Counter counter = GetComponentInParent<Counter>();

                                if (counter != null)
                                {
                                    if (weaponDetails != null)
                                    {
                                        if (!player.mainHandSlotFilled)
                                        {
                                            if (GameManager.Instance.GetPlayer().coins.coinAmount >= weaponDetails.price && !isPurchasing)
                                            {
                                                isPurchasing = true;
                                                CollectWeaponItem(player);
                                            }
                                            else
                                            {
                                                StaticDialogueHandler.CallInsufficientFundsEvent();
                                            }
                                        }
                                    }
                                }
                                else
                                {
                                    if (!InputManager.Instance.isPressedPreviousFrame)
                                    {
                                        // Drop process
                                        if (player.activeWeapon.GetCurrentMainHandWeapon() != null && !isPickedUp)
                                        {
                                            if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.OneHanded &&
                                                weaponDetails.weaponClass == WeaponClass.Shield)
                                            {
                                                goto shieldContinue; // Skip drop process because you equip one-handed weapon and chest contains a shield
                                            }

                                            if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
                                            {
                                                toBeDroppedOffWeaponDetails = weaponDetails;
                                                player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentOffHandWeapon());
                                            }

                                            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                                            {

                                                toBeDroppedMainWeaponDetails = player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails;
                                                player.playerControl.DropProcess(DropType.Weapon,player.activeWeapon.GetCurrentMainHandWeapon());
                                            }

                                        }

                                        shieldContinue:
                                        // Pick up process
                                        if (player.activeWeapon.GetCurrentMainHandWeapon() == null)
                                        {
                                            isColliding = false;

                                            if (weaponDetails.weaponClass == WeaponClass.Shield)
                                            {
                                                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                                            }
                                            else
                                            {
                                                CollectWeaponItem(player);
                                            }
                                        }
                                        else
                                        {
                                            CollectWeaponItem(player);
                                        }
                                    }

                                    if (isPickedUp)
                                    {
                                        InputManager.Instance.isPressedPreviousFrame = true;
                                    }
                                }
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasActiveDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {

                                if (!InputManager.Instance.isPressedPreviousFrame)
                                {
                                    // Drop process
                                    if (player.selectedActiveItem.GetCurrentActiveItem() != null && !isPickedUp)
                                    {
                                        player.playerControl.DropProcess(DropType.ActiveItem);
                                    }

                                    // Pick up process
                                    if (player.selectedActiveItem.GetCurrentActiveItem() == null)
                                    {
                                        isColliding = false;
                                        CollectActiveItem(player, this);
                                    }
                                }

                                if (isPickedUp)
                                {
                                    InputManager.Instance.isPressedPreviousFrame = true;
                                }
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasSecondaryPassiveDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                if (!InputManager.Instance.isPressedPreviousFrame)
                                {
                                    if (!isPickedUp)
                                    {
                                        CollectPassiveItem(player);
                                    }
                                }

                                if (isPickedUp)
                                {
                                    InputManager.Instance.isPressedPreviousFrame = true;
                                }
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasPrimaryPassiveDrop)
                        {
                            CollectPassiveItem(player);
                        }
                    }
                    else
                    {
                        animator.SetBool(Settings.hovered, false);
                    }
                }
                catch (InvalidOperationException)
                {
                    Destroy(gameObject);
                }
            }
            else if (chest != null && chest.dropCompleted && chest.chestState == ChestState.weaponItem)
            {
                hasWeaponDrop = true;

                try
                {
                // Calculate the distance between the chest item and the player
                float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

                    // Check if there's currently a nearest chest item and if it's valid
                    if (nearestChestItem == null || nearestChestItem == this || (nearestChestItem != null && Vector2.Distance(player.transform.position, nearestChestItem.transform.position)
                        > distanceToPlayer))
                    {
                        nearestChestItem = this;
                    }

                    // Only allow the nearest chest item to be interacted with
                    if (nearestChestItem == this)
                    {
                        animator.SetBool(Settings.hovered, true);

                        if (hasWeaponDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                CollectWeaponItem(player);
                                chest.chestState = ChestState.empty;
                            }
                        }
                        else if (hasActiveDrop)
                        {
                            CollectActiveItem(player, this);
                            chest.chestState = ChestState.empty;
                        }
                        else if (hasSecondaryPassiveDrop)
                        {
                            CollectPassiveItem(player);
                            chest.chestState = ChestState.empty;
                        }
                    }
                    else
                    {
                        animator.SetBool(Settings.hovered, false);
                    }
                }
                catch (InvalidOperationException)
                {
                    Destroy(gameObject);
                }
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.meteor ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            animator.SetBool(Settings.hovered, false);

            // Reset nearestChestItem when the player exits the trigger
            if (nearestChestItem == this)
            {
                nearestChestItem = null;
            }
        }
    }

    /// <summary>
    /// Initialize for enemy drops
    /// </summary>
    public void Initialize(IReceivable receivable, Sprite sprite, Vector3 spawnPosition)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;

        // Check for animation - Active Item
        if (hasActiveDrop)
        {
            ActiveItem activeItem = (ActiveItem)receivable;
            this.activeItemDetails = activeItem.activeItemDetails;
            animator.runtimeAnimatorController = activeItemDetails?.activeItemAnimatorController ?? animator.runtimeAnimatorController;
        }

        // Check for animation - Passive Items
        else if (hasPrimaryPassiveDrop || hasSecondaryPassiveDrop)
        {
            PassiveItem passiveItem = (PassiveItem)receivable;
            this.passiveItemDetails = passiveItem.passiveItemDetails;
            animator.runtimeAnimatorController = passiveItemDetails?.passiveItemAnimatorController ?? animator.runtimeAnimatorController;
        }

        // Check for animation - Weapons
        else if (hasWeaponDrop)
        {
            Weapon weapon = (Weapon)receivable;
            this.weaponDetails = weapon.weaponDetails;
            animator.runtimeAnimatorController = weaponDetails?.weaponHoverAnimatorController ?? animator.runtimeAnimatorController;
        }
    }

    /// <summary>
    /// Collect the weapon and add it to the players weapons list
    /// </summary>
    private void CollectWeaponItem(Player player)
    {
        if (!hasWeaponDrop) return;

        if (isColliding) return;

        Weapon weapon = new Weapon();
        weapon.weaponDetails = weaponDetails;

        if (player.mainHandSlotFilled)
        {
            if (!player.offHandSlotFilled)
            {
                for (int i = 3; i > 0; i--)
                {
                    int index = player.currentWeaponSlotSetIndex - i >= 0 ? player.currentWeaponSlotSetIndex - i : player.currentWeaponSlotSetIndex - i + 3;

                    if (player.weaponSlotSetArray[index][1] != null)
                    {
                        continue;
                    }
                    else if (player.weaponSlotSetArray[index][1] == null)
                    {
                        if (weaponDetails != null)
                        {
                            if (weaponDetails.weaponClass == WeaponClass.Shield)
                            {
                                if (isPurchasing)
                                {
                                    GameManager.Instance.GetPlayer().coins.coinAmount -= weaponDetails.price;
                                }
                                goto shieldContinue;
                            }
                        }
                    }
                }
            }

            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.YourHandsFull);
            isPurchasing = false;
            return;
        }

        shieldContinue:
        if (weaponDetails != null)
        {
            if (isPurchasing)
            {
                GameManager.Instance.GetPlayer().coins.coinAmount -= weaponDetails.price;
            }

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

            // Pick up item and update equipped weapon list
            player.UpdateWieldedWeapons(weaponDetails, true, false);
        }
        else
        {
            //// display message saying you already have the weapon
            //StartCoroutine(DisplayMessage("WEAPON\nALREADY\nEQUIPPED", 5f));
        }

        pickUpAnimator.SetTrigger("pickUp");

        // Introduction pop-up
        StaticEventHandler.CallIntroductionPopUpEvent(DropType.Weapon, weapon);

        isPickedUp = true;
        isColliding = true;
        weaponDetails = null;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// Collect the passive item
    /// </summary>
    private void CollectPassiveItem(Player player)
    {
        if (!hasPrimaryPassiveDrop && !hasSecondaryPassiveDrop) return;

        if (isColliding) return;

        PassiveItem passiveItem = new PassiveItem();
        passiveItem.passiveItemDetails = passiveItemDetails;

        if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Primary)
        {
            if (passiveItem.passiveItemDetails.passiveItemName == "Key")
            {
                player.keyCount++;
            }

            if (passiveItem.passiveItemDetails.passiveItemName == "Silver Coin")
            {
                player.GetComponent<Coins>().Add(1);
            }

            if (passiveItem.passiveItemDetails.passiveItemName == "Golden Coin")
            {
                player.GetComponent<Coins>().Add(5);
            }

            if (passiveItem.passiveItemDetails.passiveItemName == "Health")
            {
                player.health.AddHealth((int)(20f / player.health.GetStartingHealth() * 100));
            }

            if (passiveItem.passiveItemDetails.passiveItemName == "Medicine")
            {
                // HEALTH STATUS CHECKS
                if (player.healthStatus == HealthStatus.Poisoned)
                {
                    player.healthEvent.CallPoisonCuredEvent();
                }

                player.healthStatus = HealthStatus.Normal;

                // MOVE STATUS CHECKS
                player.moveStatus = MoveStatus.Idle;

                // ARMOR STATUS CHECKS
                if (player.armorStatus == ArmorStatus.Acid)
                {
                    player.armorStatus = ArmorStatus.Normal;
                    player.healthEvent.CallAcidCuredEvent();
                }
            }

            if (passiveItem.passiveItemDetails.passiveItemName == "Holy Water")
            {
                if (player.isCursed)
                {
                    player.healthEvent.CallCurseCuredEvent();
                    player.isCursed = false;
                }
            }

            if (passiveItem.passiveItemDetails.passiveItemName == "Quiver")
            {
                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    // Update ammo for current weapon
                    if (!player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.hasInfiniteProjectile &&
                        !player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.isMeleeWeapon)
                    {
                        player.activeWeapon.GetCurrentMainHandWeapon().weaponRemainingProjectile =
                            player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.weaponProjectileCapacity;

                        player.weaponFiredEvent.CallWeaponFiredEvent(player.activeWeapon.GetCurrentMainHandWeapon(), true);
                    }

                    // Play pickup sound effect
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);
                }
            }
        }
        else if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Secondary)
        {
            player.AddPassiveItemToPlayer(passiveItemDetails);

            isPickedUp = true;
        }

        // Introduction pop-up
        StaticEventHandler.CallIntroductionPopUpEvent(DropType.PassiveItem, passiveItem);

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

        isColliding = true;
        pickUpAnimator.SetTrigger("pickUp");
        passiveItemDetails = null;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// Collect an active item and add it to the player's current active item
    /// </summary>
    private void CollectActiveItem(Player player, ChestItem chestItem)
    {
        if (!hasActiveDrop) return;

        if (isColliding) return;

        ActiveItem activeItem = new ActiveItem();
        activeItem.activeItemDetails = activeItemDetails;

        if (activeItemDetails != null)
        {
            chestItem.remainingItemCharge = droppedByPlayer ? chestItem.remainingItemCharge : activeItemDetails.activeItemMaxCharge;
        }

        player.AddActiveItemToPlayer(activeItemDetails, this, chestItem.remainingItemCharge);

        pickUpAnimator.SetTrigger("pickUp");
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        // Introduction pop-up
        StaticEventHandler.CallIntroductionPopUpEvent(DropType.ActiveItem, activeItem);

        isColliding = true;
        isPickedUp = true;

        //animator.runtimeAnimatorController = null;
        //spriteRenderer.sprite = null;

        transform.SetParent(player.transform);
    }

    private void UpdateTooltipPanelInfo()
    {
        if (passiveItemDetails != null)
        {
            if (hasSecondaryPassiveDrop)
            {
                headerText.colorGradient = new VertexGradient(Color.blue, Color.blue, Color.blue, Color.blue);
                levelText.colorGradient = new VertexGradient(Color.blue, Color.blue, Color.blue, Color.blue);
                headerText.text = passiveItemDetails.passiveItemName;
                levelText.text = $"(Passive Item)";
            }
        }

        if (activeItemDetails != null)
        {
            headerText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
            levelText.colorGradient = new VertexGradient(Color.green, Color.green, Color.green, Color.green);
            headerText.text = activeItemDetails.activeItemName;
            levelText.text = $"(Active Item)";
        }

        if (weaponDetails != null)
        {
            // Populate text field based on the related weapon info
            switch (weaponDetails.weaponLevel)
            {
                case WeaponLevel.Basic:
                    headerText.colorGradient = new VertexGradient(GameManager.Instance.basicLevelColor1, GameManager.Instance.basicLevelColor1,
                        GameManager.Instance.basicLevelColor2, GameManager.Instance.basicLevelColor2);
                    levelText.colorGradient = new VertexGradient(GameManager.Instance.basicLevelColor1, GameManager.Instance.basicLevelColor1,
                        GameManager.Instance.basicLevelColor2, GameManager.Instance.basicLevelColor2);
                    break;
                case WeaponLevel.Enchanted:
                    headerText.colorGradient = new VertexGradient(GameManager.Instance.enchantedLevelColor1, GameManager.Instance.enchantedLevelColor1,
                        GameManager.Instance.enchantedLevelColor2, GameManager.Instance.enchantedLevelColor2);
                    levelText.colorGradient = new VertexGradient(GameManager.Instance.enchantedLevelColor1, GameManager.Instance.enchantedLevelColor1,
                        GameManager.Instance.enchantedLevelColor2, GameManager.Instance.enchantedLevelColor2);
                    break;
                case WeaponLevel.Mythic:
                    headerText.colorGradient = new VertexGradient(GameManager.Instance.mythicLevelColor1, GameManager.Instance.mythicLevelColor1,
                        GameManager.Instance.mythicLevelColor2, GameManager.Instance.mythicLevelColor2);
                    levelText.colorGradient = new VertexGradient(GameManager.Instance.mythicLevelColor1, GameManager.Instance.mythicLevelColor1,
                        GameManager.Instance.mythicLevelColor2, GameManager.Instance.mythicLevelColor2);
                    break;
                case WeaponLevel.Legendary:
                    headerText.colorGradient = new VertexGradient(GameManager.Instance.legendaryLevelColor1, GameManager.Instance.legendaryLevelColor1,
                        GameManager.Instance.legendaryLevelColor2, GameManager.Instance.legendaryLevelColor2);
                    levelText.colorGradient = new VertexGradient(GameManager.Instance.legendaryLevelColor1, GameManager.Instance.legendaryLevelColor1,
                        GameManager.Instance.legendaryLevelColor2, GameManager.Instance.legendaryLevelColor2);
                    break;
                default:
                    break;
            }

            headerText.text = weaponDetails.weaponName;
            levelText.text = $"({weaponDetails.weaponLevel.ToString()})";
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
            crHitDamageText.text = $"Base Cr. Hit Damage: {weaponDetails.criticalHitDamageMultiplier * 100}%";
            elementalBiasText.text = "Elemental Bias:";
        

            // Populate text field based on the related elemental info
            switch (weaponDetails.elementalBias)
            {
                case ElementalBias.None:
                    elementText.colorGradient = new VertexGradient(GameManager.Instance.noneElementalColor1, GameManager.Instance.noneElementalColor1,
                        GameManager.Instance.noneElementalColor2, GameManager.Instance.noneElementalColor2);
                    break;
                case ElementalBias.Fire:
                    elementText.colorGradient = new VertexGradient(GameManager.Instance.fireColor1, GameManager.Instance.fireColor1,
                        GameManager.Instance.fireColor2, GameManager.Instance.fireColor2);
                    break;
                case ElementalBias.Water:
                    elementText.colorGradient = new VertexGradient(GameManager.Instance.waterColor1, GameManager.Instance.waterColor1,
                        GameManager.Instance.waterColor2, GameManager.Instance.waterColor2);
                    break;
                case ElementalBias.Earth:
                    elementText.colorGradient = new VertexGradient(GameManager.Instance.earthColor1, GameManager.Instance.earthColor1,
                        GameManager.Instance.earthColor2, GameManager.Instance.earthColor2);
                    break;
                case ElementalBias.Air:
                    elementText.colorGradient = new VertexGradient(GameManager.Instance.airColor1, GameManager.Instance.airColor1,
                        GameManager.Instance.airColor2, GameManager.Instance.airColor2);
                    break;
                case ElementalBias.Dark:
                    elementText.colorGradient = new VertexGradient(GameManager.Instance.darkColor1, GameManager.Instance.darkColor1,
                        GameManager.Instance.darkColor2, GameManager.Instance.darkColor2);
                    break;
                case ElementalBias.Light:
                    elementText.colorGradient = new VertexGradient(GameManager.Instance.lightColor1, GameManager.Instance.lightColor1,
                        GameManager.Instance.lightColor2, GameManager.Instance.lightColor2);
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
