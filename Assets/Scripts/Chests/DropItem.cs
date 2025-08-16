using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static List<DropItem> droptItemsInRange = new List<DropItem>();
    private const float SNAP_UNIT = 0.03125f;

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
    [HideInInspector] public bool droppedByPlayer = false; // Used for actives to cache charge count
    [HideInInspector] public ActiveItem toBeDroppedActiveItem;
    [HideInInspector] public PassiveItem toBeDroppedPassiveItem;
    [HideInInspector] public bool isColliding;
    [HideInInspector] public bool hasMainHandWeapon;
    [HideInInspector] public bool hasOffHandWeapon;
    [HideInInspector] public ItemGeneric genericItem;
    [HideInInspector] public bool isGambleDropItem;
    [HideInInspector] public int gambleValue;
    [HideInInspector] public static DropItem toBeDroppedDropItem;
    [HideInInspector] public static DropItem nearestDropItem = null;
    [HideInInspector] public static WeaponDetailsSO droppedThrowingAxe = null;
    [HideInInspector] public bool isInitialized = false;

    bool isPurchasing;
    Chest chest;
    bool trackPlayer;

    public WeaponDetailsSO weaponDetails;
    public PassiveItemDetailsSO passiveItemDetails;
    public ActiveItemDetailsSO activeItemDetails;
    Player player;

    Animator pickUpAnimator;
    bool isPointerOver = false;

    // Tooltip interaction for 
    bool tooltipVisibleFromProximity = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        pickUpAnimator = transform.GetChild(1).GetComponent<Animator>();
        chest = GetComponentInParent<Chest>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        player = GameManager.Instance.GetPlayer();
    }

    private void OnEnable()
    {
        // DEBUG PURPOSE
        if (weaponDetails != null)
        {
            Weapon weapon = new Weapon();
            weapon.weaponDetails = weaponDetails;
            hasWeaponDrop = true;

            Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position);
        }
        else if (passiveItemDetails != null)
        {
            PassiveItem passiveItem = new PassiveItem();
            passiveItem.passiveItemDetails = passiveItemDetails;
            hasSecondaryPassiveDrop = true;
 
            Initialize(passiveItem, passiveItemDetails.passiveItemSprite, transform.position);
        }

        StaticEventHandler.OnEnemiesCleared += StaticEventHandler_OnEnemiesCleared;
    }

    private void OnDisable()
    {
        StaticEventHandler.OnEnemiesCleared -= StaticEventHandler_OnEnemiesCleared;
    }

    private void Update()
    {
        if (player == null) return;

        // Close tooltip
        if (isPointerOver && !EventSystem.current.IsPointerOverGameObject())
        {
            CloseTooltip(); // only if pointer no longer over *any* UI
        }

        if (isPointerOver && InputManager.Instance.AnyNonTooltipInputPressed())
        {
            CloseTooltip();
        }

        if (trackPlayer && !isPickedUp)
        {
            Vector3 targetPos = player.transform.position + new Vector3(0f, 0.5f, 0f);

            // Calculcate direction
            Vector3 direction = (targetPos - transform.position).normalized;

            // Move by one snapped step in that direction
            Vector3 step = direction * SNAP_UNIT * 2;

            // Only move if not overshooting the target
            if ((targetPos - transform.position).sqrMagnitude > step.sqrMagnitude)
            {
                transform.position = SnapPosition(transform.position + step);
            }
            else
            {
                transform.position = SnapPosition(targetPos);
            }
        }
    }

    private Vector3 SnapPosition(Vector3 pos)
    {
        return new Vector3(
            Mathf.Round(pos.x / SNAP_UNIT) * SNAP_UNIT,
            Mathf.Round(pos.y / SNAP_UNIT) * SNAP_UNIT,
            transform.position.z 
        );
    }

    private void StaticEventHandler_OnEnemiesCleared()
    {
        if (hasPrimaryPassiveDrop)
        {
            trackPlayer = true;
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter.GetComponent<DropItem>() == this && eventData.pointerEnter.GetComponentInParent<Player>() == null &&
              eventData.pointerEnter.GetComponentInParent<Enemy>() == null && isInitialized)
        {
            OpenTooltip(TooltipSource.Pointer);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CloseTooltip();
    }

    private void OpenTooltip(TooltipSource tooltipSource)
    {
        // If chest item contains primary passive drop or nothing, cancel the transaction
        if (!hasWeaponDrop && !hasActiveDrop && !hasSecondaryPassiveDrop) return;

        animator.SetBool(Settings.hovered, true);

        MainUI.Instance.UpdateTooltipPanelInfo(genericItem, hasWeaponDrop, hasActiveDrop, hasSecondaryPassiveDrop, tooltipSource);
        isPointerOver = true;
    }

    private void CloseTooltip()
    {
        animator.SetBool(Settings.hovered, false);
        MainUI.Instance.CloseTooltipPanel();
        MainUI.Instance.CloseTooltipEquippedPanel();
        isPointerOver = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (GetComponentInParent<Player>() != null) return;

        if (collision.CompareTag(Settings.playerTag) || collision.CompareTag(Settings.playerWeapon))
        {
            if (!droptItemsInRange.Contains(this)) droptItemsInRange.Add(this);
        }
    }

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.aoeSkill ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (GetComponentInParent<Player>() != null) return;

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
                    if (nearestDropItem == null || nearestDropItem == this || (nearestDropItem != null && Vector2.Distance(player.transform.position, 
                        nearestDropItem.transform.position) > distanceToPlayer))
                    {
                        nearestDropItem = this;
                    }

                    // Only allow the nearest chest item to be interacted with
                    if (nearestDropItem == this)
                    {
                        animator.SetBool(Settings.hovered, true);

                        Counter counter = GetComponentInParent<Counter>();

                        if (counter != null)
                        {
                            NpcType npcType = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<NPC>().npcType;

                            if (npcType == NpcType.Gambler)
                            {
                                StaticEventHandler.CallNPCInteractionStartedEvent(npcType); // Cloese-up camera for gambler
                            }
                        }

                        if (!tooltipVisibleFromProximity)
                        {
                            tooltipVisibleFromProximity = true;

                            // Show tooltip from proximity
                            if (!hasPrimaryPassiveDrop && !isGambleDropItem)
                            {
                                MainUI.Instance.UpdateTooltipPanelInfo(genericItem, hasWeaponDrop, hasActiveDrop, hasSecondaryPassiveDrop, TooltipSource.Proximity);
                            }
                        }

                        if (isGambleDropItem)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                if (counter != null)
                                {
                                    int gambleBlind = 10;

                                    NPC gambleNpc = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<NPC>();
                 
                                    if (!InputManager.Instance.isPressedPreviousFrame)
                                    {
                                        if (player.coins.coinAmount >= gambleBlind)
                                        {
                                            player.coins.coinAmount = Mathf.Clamp(player.coins.coinAmount + gambleValue, 0, player.coins.coinAmount + gambleValue);

                                            if (gambleValue < 0)
                                            {
                                                SoundEffectManager.Instance.PlaySoundEffect(gambleNpc.gambleLostSoundEffect);
                                                StaticDialogueHandler.CallGambleLostEvent(); // Gamble lost
                                            }
                                            else
                                            {
                                                SoundEffectManager.Instance.PlaySoundEffect(gambleNpc.gambleWinSoundEffect);
                                                StaticDialogueHandler.CallGambleWonEvent(); // Gamble won
                                            }

                                            isColliding = true;

                                            StaticEventHandler.CallGambleCompletedEvent(); // This event is for resetting gamble chest item
                                        }
                                        else
                                        {
                                            StaticDialogueHandler.CallInsufficientFundsEvent();
                                        }

                                    }

                                    InputManager.Instance.isPressedPreviousFrame = true;
                                }
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasWeaponDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed() && !InputManager.interactionDisabled)
                            {
                                if (counter != null)
                                {
                                    //NpcType npcType = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<NPC>().npcType;
                                    //StaticEventHandler.CallNPCInteractionStartedEvent(npcType);

                                    if (weaponDetails != null)
                                    {
                                        if (!player.mainHandSlotFilled)
                                        {
                                            if(!weaponDetails.requiredPrimaryStats.MeetsRequirements(player))
                                            {
                                                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.DontMeetRequiredPrimaryStats);
                                                return;
                                            }

                                            int currentPrice = (int)(weaponDetails.price * (1 + player.additinalNPCCostModifier));

                                            if (GameManager.Instance.GetPlayer().coins.coinAmount >= currentPrice && !isPurchasing)
                                            {
                                                isPurchasing = true;
                                                StaticDialogueHandler.CallTradeCompletedEvent();
                                                DropWeaponPickUpProcess(player, weaponDetails);
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
                                    DropWeaponPickUpProcess(player, weaponDetails);
                                }
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasSecondaryPassiveDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed() && !InputManager.interactionDisabled)
                            {
                                if (counter != null)
                                {
                                    if (passiveItemDetails != null)
                                    {
                                        int currentPrice = (int)(passiveItemDetails.price * (1 + player.additinalNPCCostModifier));

                                        if (GameManager.Instance.GetPlayer().coins.coinAmount >= currentPrice && !isPurchasing)
                                        {
                                            isPurchasing = true;
                                            StaticDialogueHandler.CallTradeCompletedEvent();
                                            DropPassiveItemPickUpProcess(player);
                                        }
                                        else
                                        {
                                            StaticDialogueHandler.CallInsufficientFundsEvent();
                                        }
                                    }
                                }
                                else
                                {
                                    DropPassiveItemPickUpProcess(player);
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
                    if (nearestDropItem == null || nearestDropItem == this || (nearestDropItem != null && Vector2.Distance(player.transform.position, nearestDropItem.transform.position)
                        > distanceToPlayer))
                    {
                        nearestDropItem = this;
                    }

                    // Only allow the nearest chest item to be interacted with
                    if (nearestDropItem == this)
                    {
                        animator.SetBool(Settings.hovered, true);

                        if (hasWeaponDrop)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed() && !InputManager.interactionDisabled)
                            {
                                DropWeaponPickUpProcess(player, weaponDetails);
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasSecondaryPassiveDrop)
                        {
                            // Drop process
                            player.playerControl.DropProcess(DropType.PassiveItem, player.equippedPassiveItems[passiveItemDetails.passiveItemSlotName]);

                            // Pick up process
                            isColliding = false;
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

    private void DropPassiveItemPickUpProcess(Player player)
    {
        if (!InputManager.Instance.isPressedPreviousFrame)
        {
            if (nearestDropItem != this) return;

            if (!isPickedUp)
            {
                Debug.Log("Drop interaction happened. Drop is " + gameObject.GetComponent<DropItem>().passiveItemDetails.passiveItemName);
                Debug.Log("Frame count " + Time.frameCount);

                // Drop process
                if (player.equippedPassiveItems[passiveItemDetails.passiveItemSlotName] != null)
                {
                    player.playerControl.DropProcess(DropType.PassiveItem, player.equippedPassiveItems[passiveItemDetails.passiveItemSlotName]);
                }

                // Pick up process
                isColliding = false;
                CollectPassiveItem(player);
            }
        }
    }

    private void DropWeaponPickUpProcess(Player player, WeaponDetailsSO toBeSwappedWeaponDetails,bool dropToInventory = false)
    {
        Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        if (!InputManager.Instance.isPressedPreviousFrame)
        {
            if (nearestDropItem != this) return;

            if (!weaponDetails.requiredPrimaryStats.MeetsRequirements(player))
            {
                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.DontMeetRequiredPrimaryStats);
                return;
            }

            if (weaponDetails.weaponClass == WeaponClass.Shield || (offHandWeapon == null && weaponDetails.wieldType == WieldType.OneHanded && 
                weaponDetails.weaponClass != WeaponClass.Spear && mainHandWeapon != null && mainHandWeapon.weaponDetails.wieldType == WieldType.OneHanded &&
                mainHandWeapon.weaponDetails.weaponClass != WeaponClass.Spear))
            {
                goto shieldContinue; // Skip drop process because you equip one-handed weapon and chest contains a shield
            }

            // Drop off-hand weapon if pick-up item is two-handed
            if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
            {
                if (weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentOffHandWeapon(), weaponDetails, true);
                }
            }

            if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
            {
                // If pick-up weapon is two-handed, off-hand weapon is dropped and off-hand active weapon is null
                if (weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentMainHandWeapon(), weaponDetails, false);
                }
                else
                {
                    if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
                    {
                        // CURRENT ACTIVE MAIN HAND WEAPON BECOMES NULL HERE AND DROP HAPPENS
                        player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentMainHandWeapon(), weaponDetails, false);
                    }
                    else
                    {
                        if (weaponDetails.weaponClass == WeaponClass.Shield)
                        {
                            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                        }
                        else
                        {
                            player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentMainHandWeapon(), weaponDetails, true);
                        }
                    }
                }
            }

        // AT THIS LINE active main hand weapon becomes null

        shieldContinue:
            // Pick up process
            // If weapon dropped for replace
            if (player.activeWeapon.GetCurrentMainHandWeapon() == null)
            {
                isColliding = false;

                if (weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                }
                else
                {
                    player.isShieldCalculated = false;
                    CollectWeaponItem(player);
                }
            }
            else
            {
                if (!InventoryManager.Instance.IsInventoryFull())
                {
                    CollectWeaponItem(player);
                }
                else
                {
                    // Drop equipped off-hand weapon if to-be-picked-up item is a shield
                    if (weaponDetails.weaponClass == WeaponClass.Shield)
                    {
                        if (player.activeWeapon?.GetCurrentOffHandWeapon() != null)
                        {
                            player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentOffHandWeapon());

                            CollectWeaponItem(player);
                        }
                        else if (player.activeWeapon.GetCurrentMainHandWeapon().weaponDetails.wieldType == WieldType.TwoHanded)
                        {
                            GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.OffHandCantBeAddedToTwoHanded);
                        }
                        else
                        {
                            CollectWeaponItem(player);
                        }
                    }
                }
            }
        }

        if (isPickedUp)
        {
            InputManager.Instance.isPressedPreviousFrame = true;
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.aoeSkill ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile) return;

        if (collision.CompareTag(Settings.playerTag) || collision.CompareTag(Settings.playerWeapon)) droptItemsInRange.Remove(this);

        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {

            Counter counter = GetComponentInParent<Counter>();

            if (counter != null)
            {
                StaticEventHandler.CallNPCInteractionEndedEvent();
            }

            if (tooltipVisibleFromProximity)
            {
                tooltipVisibleFromProximity = false;
                MainUI.Instance.CloseTooltipPanel();
                MainUI.Instance.CloseTooltipEquippedPanel();
                animator.SetBool(Settings.hovered, false);
            }

            // Reset nearestChestItem when the player exits the trigger
            if (nearestDropItem == this)
            {
                nearestDropItem = null;
            }
        }
    }

    /// <summary>
    /// Initialize for enemy drops
    /// </summary>
    public void Initialize(ItemGeneric itemGeneric, Sprite sprite, Vector3 spawnPosition, bool isTutorial = false)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;
        this.genericItem = itemGeneric;

        if (isTutorial)
        {
            Transform tutorialArrowContainer = transform.GetChild(4);
            tutorialArrowContainer.gameObject.SetActive(true);
        }

        // Check for animation - Active Item
        if (hasActiveDrop)
        {
            ActiveItem activeItem = (ActiveItem)itemGeneric;
            activeItemDetails = activeItem.activeItemDetails;
            animator.runtimeAnimatorController = activeItemDetails?.activeItemAnimatorController ?? animator.runtimeAnimatorController;

            isInitialized = true;
        }

        // Check for animation - Passive Items
        else if (hasPrimaryPassiveDrop || hasSecondaryPassiveDrop)
        {
            PassiveItem passiveItem = (PassiveItem)itemGeneric;
            passiveItemDetails = passiveItem.passiveItemDetails;
            animator.runtimeAnimatorController = passiveItemDetails?.passiveItemAnimatorController ?? animator.runtimeAnimatorController;

            if (hasSecondaryPassiveDrop)
            {
                isInitialized = true;
            }
        }

        // Check for animation - Weapons
        else if (hasWeaponDrop)
        {
            Weapon weapon = (Weapon)itemGeneric;
            weaponDetails = weapon.weaponDetails;
            animator.runtimeAnimatorController = weaponDetails?.weaponHoverAnimatorController ?? animator.runtimeAnimatorController;

            isInitialized = true;
        }
    }

    /// <summary>
    /// Collect the weapon and add it to the players weapons list
    /// </summary>
    private void CollectWeaponItem(Player player)
    {
        if (!hasWeaponDrop) return;

        if (isPickedUp || isColliding) return;

        Weapon weapon = new Weapon();
        weapon.weaponDetails = weaponDetails;

        if (player.mainHandSlotFilled && InventoryManager.Instance.IsInventoryFull())
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

        if (isPickedUp || isColliding) return;

        PassiveItem passiveItem = new PassiveItem();
        passiveItem.passiveItemDetails = passiveItemDetails;

        if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Primary)
        {
            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.Key)
            {
                player.keyCount++;

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.itemPickup);
            }

            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.SilverCoin)
            {
                int coinAmount = player.additionalCoinIncreaseActivated ? 1 : 1 + player.additionalCoinIncreaserModifier;

                player.GetComponent<Coins>().Add(coinAmount);

                if (InputManager.TutorialEnabled && TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.PickUpPrimaryPassiveCoin)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }

                player.additionalCoinIncreaseActivated = true;

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.coinPickup);
            }

            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.GoldCoin)
            {
                player.GetComponent<Coins>().Add(5);

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.coinPickup);
            }

            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.Health)
            {
                player.UpdatePlayerHealth(20, false, false);

                if (InputManager.TutorialEnabled && TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.PickUpPrimaryPassiveHealth)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
            }

            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.Mana)
            {
                player.UpdatePlayerMana(20, false, false);

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
            }

            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.Cure)
            {
                // HEALTH STATUS CHECKS
                if ((player.healthStatus & HealthStatus.Poisoned) != 0)
                {
                    player.healthEvent.CallPoisonCuredEvent();
                    player.healthStatus &= ~HealthStatus.Poisoned; // Remove poisoned status
                }
                if ((player.healthStatus & HealthStatus.Burned) != 0)
                {
                    player.healthEvent.CallBurnCuredEvent();
                    player.healthStatus &= ~HealthStatus.Burned; // Remove burned status
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

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
            }

            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.HolyWater)
            {
                if (player.isCursed)
                {
                    player.healthEvent.CallCurseCuredEvent();
                    player.isCursed = false;
                }

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
            }
        }
        else if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Secondary)
        {
            if (isPurchasing)
            {
                GameManager.Instance.GetPlayer().coins.coinAmount -= passiveItemDetails.price;
            }

            player.AddPassiveItemToPlayer(passiveItemDetails);

            isPickedUp = true;

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
        }

        StaticEventHandler.CallStatsChangedOnTheBookEvent();
        isColliding = true;
        pickUpAnimator.SetTrigger("pickUp");
        passiveItemDetails = null;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;
        Destroy(gameObject, 1f);
    }
}