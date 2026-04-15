using Mirror;
using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItemNetwork : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static List<DropItemNetwork> droptItemsInRange = new List<DropItemNetwork>();
    private const float SNAP_UNIT = 0.03125f;

    [SerializeField] Transform tooltipPanel;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public bool isPickedUp = false;
    [HideInInspector] public Animator animator;
    [HideInInspector] public BoxCollider2D boxCollider2D;
    [HideInInspector] public int remainingItemCharge;
    [HideInInspector] public bool droppedByPlayer = false;
    [HideInInspector] public PassiveItem toBeDroppedPassiveItem;
    [HideInInspector] public bool hasMainHandWeapon;
    [HideInInspector] public bool hasOffHandWeapon;
    [HideInInspector] public ItemGeneric itemGeneric;
    [HideInInspector] public static DropItemNetwork toBeDroppedDropItem;
    [HideInInspector] public static DropItemNetwork nearestDropItem = null;
    [HideInInspector] public static Weapon droppedThrowingAxe = null;
    [HideInInspector] public bool isInitialized = false;

    [SyncVar] public Rarity rarity;

    [SyncVar(hook = nameof(OnPassiveItemStatsChanged))]
    public PassiveItemStats passiveStats;
    [SyncVar(hook = nameof(OnWeaponStatsChanged))]
    public WeaponStats weaponStats;

    [SyncVar] public PassiveItemSlotName passiveItemSlotName;

    [SyncVar] public PassiveItemType passiveItemType;
    [SyncVar] public WeaponTitle weaponTitle;
    [SyncVar] public bool isColliding;
    [SyncVar] public bool hasWeaponDrop = false;
    [SyncVar] public bool hasPrimaryPassiveDrop = false;
    [SyncVar] public bool hasSecondaryPassiveDrop = false;
    [SyncVar] public bool isGambleDropItem;
    [SyncVar] public int gambleValue;

    bool isPurchasing;
    Chest chest;
    bool shardGained = false;

    public WeaponDetailsSO weaponDetails;
    public PassiveItemDetailsSO passiveItemDetails;

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

    private void OnWeaponStatsChanged(WeaponStats oldValue, WeaponStats newValue)
    {
        InitializeVisual();
    }

    private void OnPassiveItemStatsChanged(PassiveItemStats oldValue, PassiveItemStats newValue)
    {
        InitializeVisual();
    }

    private void InitializeVisual()
    {
        if(weaponStats.weaponTitle != WeaponTitle.None)
        {
            weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weaponStats.weaponTitle);
            Weapon weapon = WeaponDropGenerator.GetWeaponWithStats(weaponStats, rarity);

            weapon.weaponDetails = weaponDetails;
            hasWeaponDrop = true;

            Initialize(weapon, weaponDetails.weaponFrontSprite, transform.position);
        }

        if (passiveStats.passiveItemType != PassiveItemType.None)
        {
            passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveStats.passiveItemType);

            PassiveItem passiveItem = PassiveDropGenerator.GetPassiveWithStats(passiveStats, rarity);
            passiveItem.passiveItemDetails = passiveItemDetails;

            if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Primary) hasPrimaryPassiveDrop = true;
            else hasSecondaryPassiveDrop = true;

            Initialize(passiveItem, passiveItemDetails.passiveItemSprite, transform.position);
        }
    }

    [Command(requiresAuthority = false)]
    private void CmdDropPassive(NetworkIdentity playerIdentity, PassiveItemCategory passiveItemCategory, PassiveItemType passiveItemType)
    {
        
    }

    [Command(requiresAuthority = false)]
    private void CmdDropWeapon(NetworkIdentity playerIdentity)
    {

    }


    [Command(requiresAuthority = false)]
    private void CmdPickupPassive(NetworkIdentity playerIdentity, PassiveItemCategory passiveItemCategory, PassiveItemType passiveItemType)
    {
        if (isPickedUp) return;

        isPickedUp = true;

        Player player = playerIdentity.GetComponent<Player>();

        CollectPassiveItem(player, passiveItemCategory, passiveItemType);
    }

    [Command(requiresAuthority = false)]
    private void CmdPickupWeapon(NetworkIdentity playerIdentity)
    {
        if (isPickedUp) return;

        isPickedUp = true;

        Player player = playerIdentity.GetComponent<Player>();

        CollectWeaponItem(player);
    }

    private void Update()
    {
        // Close tooltip
        if (isPointerOver && !EventSystem.current.IsPointerOverGameObject())
        {
            CloseTooltip(); // only if pointer no longer over *any* UI
        }

        if (isPointerOver && InputManager.Instance.AnyNonTooltipInputPressed())
        {
            CloseTooltip();
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (eventData.pointerEnter.GetComponent<DropItemNetwork>() == this && eventData.pointerEnter.GetComponentInParent<Player>() == null &&
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
        if (!hasWeaponDrop && !hasSecondaryPassiveDrop) return;

        animator.SetBool(Settings.hovered, true);

        MainUI.Instance.UpdateTooltipPanelInfo(itemGeneric, hasWeaponDrop, hasSecondaryPassiveDrop, tooltipSource);
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
                    if (isColliding) return;

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
                                StaticEventHandler.CallNPCInteractionStartedEvent(npcType); // Close-up camera for gambler
                            }
                        }

                        if (!tooltipVisibleFromProximity)
                        {
                            tooltipVisibleFromProximity = true;

                            // Show tooltip from proximity
                            if (!hasPrimaryPassiveDrop && !isGambleDropItem)
                            {
                                MainUI.Instance.UpdateTooltipPanelInfo(itemGeneric, hasWeaponDrop, hasSecondaryPassiveDrop, TooltipSource.Proximity);
                            }
                        }

                        if (isGambleDropItem)
                        {
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                if (counter != null)
                                {
                                    int gambleBlind = 40;

                                    NPC gambleNpc = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<NPC>();

                                    if (!InputManager.Instance.isPressedPreviousFrame)
                                    {
                                        if (player.coinsAndShards.coinAmount >= gambleBlind)
                                        {
                                            player.coinsAndShards.AddCoin(gambleValue);

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
                                            int currentPrice = Mathf.RoundToInt(weaponDetails.price * (1 + player.additinalNPCCostModifier));

                                            if (player.coinsAndShards.coinAmount >= currentPrice && !isPurchasing)
                                            {
                                                if (!MeetsRequirements(player))
                                                {
                                                    if (!shardGained)
                                                    {
                                                        int shardGain = ShardGainProcess(player);
                                                        GameManager.Instance.OpenPopUpLog(PopUpReason.DontMeetRequiredCharacter, shardGain);
                                                        isPurchasing = true;
                                                        StaticDialogueHandler.CallTradeCompletedEvent();
                                                        shardGained = true;

                                                        // Play pickup sound effect
                                                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
                                                    }

                                                    CloseTooltip();
                                                    StartCoroutine(DestroyRoutine(0.2f));
                                                    return;
                                                }

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

                                        if (GameManager.Instance.GetLocalPlayer().coinsAndShards.coinAmount >= currentPrice && !isPurchasing)
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
                            CmdPickupPassive(player.NetAuth.netIdentity, PassiveItemCategory.Primary, passiveItemType);
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
                try
                {
                    // Calculate the distance between the chest item and the player
                    float distanceToPlayer = Vector2.Distance(player.transform.position, transform.position);

                    // Check if there is currently a nearest chest item and if it is valid
                    if (nearestDropItem == null || nearestDropItem == this || (nearestDropItem != null && Vector2.Distance(player.transform.position,
                        nearestDropItem.transform.position) > distanceToPlayer))
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
                            //Drop process
                            //player.playerControl.DropProcess(DropType.PassiveItem, player.equippedPassiveItems[passiveItemDetails.passiveItemSlotName], toBeSwappedWeaponDetails: null, false, ItemSlotStatus.None, -1, false, isChest: true);

                            if (InputManager.Instance.interaction.action.IsPressed() && !InputManager.interactionDisabled)
                            {
                                DropPassiveItemPickUpProcess(player);

                                // Pick up process
                                isColliding = false;

                                CmdPickupPassive(player.NetAuth.netIdentity, PassiveItemCategory.Secondary, passiveItemDetails.passiveItemType);

                                chest.chestState = ChestState.empty;
                            }
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
                // Drop process
                if (player.equippedPassiveItems[passiveItemDetails.passiveItemSlotName] != null)
                {
                    player.playerControl.DropProcess(DropType.PassiveItem, player.equippedPassiveItems[passiveItemDetails.passiveItemSlotName], toBeSwappedWeaponDetails: null);
                }

                // Pick up process
                isColliding = false;

                CmdPickupPassive(player.NetAuth.netIdentity, passiveItemDetails.passiveItemCategory, passiveItemDetails.passiveItemType);
            }
        }
    }

    private void DropWeaponPickUpProcess(Player player, WeaponDetailsSO toBeSwappedWeaponDetails, bool dropToInventory = false)
    {
        Weapon mainHandWeapon = player.activeWeapon.GetCurrentMainHandWeapon();
        Weapon offHandWeapon = player.activeWeapon.GetCurrentOffHandWeapon();

        if (!InputManager.Instance.isPressedPreviousFrame)
        {
            if (nearestDropItem != this) return;

            if (!MeetsRequirements(player))
            {
                if (!shardGained)
                {
                    int shardGain = ShardGainProcess(player);
                    GameManager.Instance.OpenPopUpLog(PopUpReason.DontMeetRequiredCharacter, shardGain);
                    shardGained = true;

                    // Play pickup sound effect
                    SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
                }

                StartCoroutine(DestroyRoutine(0.2f));
                CloseTooltip();

                return;
            }

            if (weaponDetails.weaponClass == WeaponClass.Shield || (offHandWeapon == null && weaponDetails.wieldType == WieldType.OneHanded &&
                weaponDetails.weaponClass != WeaponClass.Spear && mainHandWeapon != null && mainHandWeapon.weaponDetails.wieldType == WieldType.OneHanded &&
                mainHandWeapon.weaponDetails.weaponClass != WeaponClass.Spear))
            {
                goto shieldContinue; // Skip drop process because you equip one-handed weapon and chest contains a shield
            }

            // Drop off-hand weapon if pick-up item is two-handed
            if (offHandWeapon != null)
            {
                if (weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    player.playerControl.DropProcess(DropType.Weapon, offHandWeapon, weaponDetails, true);
                }
            }

            if (mainHandWeapon != null)
            {
                // If pick-up weapon is two-handed, off-hand weapon is dropped and off-hand active weapon is null
                if (weaponDetails.wieldType == WieldType.TwoHanded)
                {
                    player.playerControl.DropProcess(DropType.Weapon, mainHandWeapon, weaponDetails, false);
                }
                else
                {
                    if (offHandWeapon != null)
                    {
                        // CURRENT ACTIVE MAIN HAND WEAPON BECOMES NULL HERE AND DROP HAPPENS
                        player.playerControl.DropProcess(DropType.Weapon, mainHandWeapon, weaponDetails, false);
                    }
                    else
                    {
                        if (weaponDetails.weaponClass == WeaponClass.Shield)
                        {
                            GameManager.Instance.OpenPopUpLog(PopUpReason.ShieldCantBePutOnMainHand);
                        }
                        else
                        {
                            player.playerControl.DropProcess(DropType.Weapon, mainHandWeapon, weaponDetails, true);
                        }
                    }
                }
            }

        // AT THIS LINE active main hand weapon becomes null

        shieldContinue:
            // Pick up process
            // If weapon dropped for replace
            if (mainHandWeapon == null)
            {
                isColliding = false;

                if (weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    GameManager.Instance.OpenPopUpLog(PopUpReason.ShieldCantBePutOnMainHand);
                }
                else
                {
                    player.isShieldCalculated = false;

                    CmdPickupWeapon(player.NetAuth.netIdentity);
                }
            }
            else
            {
                if (!InventoryManager.Instance.IsInventoryFull())
                {
                    CmdPickupWeapon(player.NetAuth.netIdentity);
                }
                else
                {
                    // Drop equipped off-hand weapon if to-be-picked-up item is a shield
                    if (weaponDetails.weaponClass == WeaponClass.Shield)
                    {
                        if (offHandWeapon != null)
                        {
                            player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentOffHandWeapon(),
                                toBeSwappedWeaponDetails: null);

                            CmdPickupWeapon(player.NetAuth.netIdentity);
                        }
                        else if (mainHandWeapon.weaponDetails.wieldType == WieldType.TwoHanded)
                        {
                            GameManager.Instance.OpenPopUpLog(PopUpReason.OffHandCantBeAddedToTwoHanded);
                        }
                        else
                        {
                            CmdPickupWeapon(player.NetAuth.netIdentity);
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

    private int ShardGainProcess(Player player)
    {
        int shardGain = 0;

        switch (itemGeneric.rarity)
        {
            case Rarity.Basic: shardGain = 10; break;
            case Rarity.Enchanted: shardGain = 35; break;
            case Rarity.Mythic: shardGain = 100; break;
            case Rarity.Legendary: shardGain = 250; break;
            default:
                break;
        }

        player.coinsAndShards.AddShard(shardGain);
        return shardGain;
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
        this.itemGeneric = itemGeneric;

        if (isTutorial)
        {
            Transform tutorialArrowContainer = transform.GetChild(4);
            tutorialArrowContainer.gameObject.SetActive(true);
        }

        // Check for animation - Passive Items
        if (hasPrimaryPassiveDrop || hasSecondaryPassiveDrop)
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

        if (isColliding) return;

        // Use the instance created at drop time
        Weapon weapon = (Weapon)itemGeneric;

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
                                    GameManager.Instance.GetLocalPlayer().coinsAndShards.coinAmount -= weaponDetails.price;
                                }
                                goto shieldContinue;
                            }
                        }
                    }
                }
            }

            GameManager.Instance.OpenPopUpLog(PopUpReason.YourHandsFull);
            isPurchasing = false;
            return;
        }

    shieldContinue:
        if (weaponDetails != null)
        {
            if (isPurchasing)
            {
                GameManager.Instance.GetLocalPlayer().coinsAndShards.coinAmount -= weaponDetails.price;
            }

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

            // Pick up item and update equipped weapon list
            player.UpdateWieldedWeapons(ref weapon, true, false, weaponDetails);
        }
        else
        {
            //// display message saying you already have the weapon
            //StartCoroutine(DisplayMessage("WEAPON\nALREADY\nEQUIPPED", 5f));
        }

        pickUpAnimator.SetTrigger("pickUp");

        isColliding = true;
        weaponDetails = null;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;

        StartCoroutine(DestroyRoutine(1f));
    }

    /// <summary>
    /// Collect the passive item
    /// </summary>
    private void CollectPassiveItem(Player player, PassiveItemCategory passiveItemCategory, PassiveItemType passiveItemType)
    {
        if (!hasPrimaryPassiveDrop && !hasSecondaryPassiveDrop) return;

        if (isColliding) return;

        if (passiveItemCategory == PassiveItemCategory.Primary)
        {
             PassiveItem passiveItem = (PassiveItem)itemGeneric;

            if (passiveItemType == PassiveItemType.Key)
            {
                player.consumableEvent.CallKeyCountChangedEvent(++player.keyCount);

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.itemPickup);
            }

            if (passiveItemType == PassiveItemType.SilverCoin)
            {
                int coinAmount = 1;

                player.coinsAndShards.AddCoin(coinAmount);

                if (InputManager.TutorialEnabled && TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.PickUpPrimaryPassiveCoin)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.coinPickup);
            }

            if (passiveItemType == PassiveItemType.GoldCoin)
            {
                player.coinsAndShards.AddCoin(5);

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.coinPickup);
            }

            if (passiveItemType == PassiveItemType.Health)
            {
                player.UpdatePlayerHealth(20, false, false);

                if (InputManager.TutorialEnabled && TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.PickUpPrimaryPassiveHealth)
                {
                    TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
                }

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
            }

            if (passiveItemType == PassiveItemType.Mana)
            {
                player.UpdatePlayerMana(20, false, false);

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
            }

            if (passiveItemType == PassiveItemType.Cure)
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
        }
        else if (passiveItemCategory == PassiveItemCategory.Secondary)
        {
            PassiveItem passiveItem = (PassiveItem)itemGeneric;

            if (isPurchasing)
            {
                player.coinsAndShards.AddCoin(-passiveItem.passiveStats.activePrice);
            }

            passiveItem = (PassiveItem)itemGeneric;
            player.AddPassiveItemToPlayer(ref passiveItem, passiveItemSlotName);

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);
        }

        StaticEventHandler.CallStatsChangedOnTheBookEvent();
        isColliding = true;

        pickUpAnimator.SetTrigger("pickUp");

        passiveItemDetails = null;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;

        StartCoroutine(DestroyRoutine(1f));
    }

    IEnumerator DestroyRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        NetworkServer.Destroy(gameObject);
    }

    private bool MeetsRequirements(Player player)
    {
        switch (player.playerDetails.playerCharacterIndex)
        {
            case Character.Caelion:
                if (weaponDetails.weaponClass == WeaponClass.Sword || weaponDetails.weaponClass == WeaponClass.Shield) return true; break;
            case Character.Morven:
                if (weaponDetails.weaponClass == WeaponClass.Dagger) return true; break;
            case Character.Nyveran:
                if ((weaponDetails.weaponClass == WeaponClass.Dagger && player.activeWeapon) || weaponDetails.weaponClass == WeaponClass.Bow ||
                    weaponDetails.weaponClass == WeaponClass.Crossbow) return true; break;
            case Character.Karnag:
                if (weaponDetails.weaponClass == WeaponClass.Axe) return true; break;
            case Character.Kynara:
            case Character.Mycara:
            case Character.Nymara:
                if (weaponDetails.weaponClass == WeaponClass.Staff) return true; break;
            case Character.Nyxa:
                if (weaponDetails.weaponClass == WeaponClass.Dagger || weaponDetails.weaponClass == WeaponClass.Crossbow) return true; break;
            default: break;
        }

        return false;
    }
}
