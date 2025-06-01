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
    [HideInInspector] public bool droppedByPlayer = false;
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

        if (hasPrimaryPassiveDrop && trackPlayer && !isPickedUp)
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

            // Trigger pickup if close enough
            if (Vector2.Distance(transform.position, targetPos) < SNAP_UNIT * 1.5f)
            {
                isPickedUp = true;
                CollectPassiveItem(player);
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
        GameManager.Instance.UpdateTooltipPanelInfo(genericItem, hasWeaponDrop, hasActiveDrop, hasSecondaryPassiveDrop, tooltipSource);
        isPointerOver = true;
    }

    private void CloseTooltip()
    {
        animator.SetBool(Settings.hovered, false);
        GameManager.Instance.CloseTooltipPanel();
        GameManager.Instance.CloseTooltipEquippedPanel();
        isPointerOver = false;
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag(Settings.playerTag) || collision.CompareTag(Settings.playerWeapon))
        {
            if (!droptItemsInRange.Contains(this)) droptItemsInRange.Add(this);
        }
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

                            StaticEventHandler.CallNPCInteractionStartedEvent(npcType);
                        }

                        if (!tooltipVisibleFromProximity)
                        {
                            tooltipVisibleFromProximity = true;

                            // Show tooltip from proximity
                            if (!hasPrimaryPassiveDrop && !isGambleDropItem)
                            {
                                GameManager.Instance.UpdateTooltipPanelInfo(genericItem, hasWeaponDrop, hasActiveDrop, hasSecondaryPassiveDrop, TooltipSource.Proximity);
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
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                if (counter != null)
                                {
                                    NpcType npcType = GameManager.Instance.GetCurrentRoom().instantiatedRoom.GetComponentInChildren<NPC>().npcType;

                                    StaticEventHandler.CallNPCInteractionStartedEvent(npcType);

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
                                                ChestItemWeaponDropPickUpProcess(player);
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
                                    ChestItemWeaponDropPickUpProcess(player);
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
                                if (counter != null)
                                {
                                    if (passiveItemDetails != null)
                                    {
                                        int currentPrice = (int)(passiveItemDetails.price * (1 + player.additinalNPCCostModifier));

                                        if (GameManager.Instance.GetPlayer().coins.coinAmount >= currentPrice && !isPurchasing)
                                        {
                                            isPurchasing = true;
                                            StaticDialogueHandler.CallTradeCompletedEvent();
                                            ChestItemPassiveItemDropPickUpProcess(player);
                                        }
                                        else
                                        {
                                            StaticDialogueHandler.CallInsufficientFundsEvent();
                                        }
                                    }
                                }
                                else
                                {
                                    ChestItemPassiveItemDropPickUpProcess(player);
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
                            if (InputManager.Instance.interaction.action.IsPressed())
                            {
                                ChestItemWeaponDropPickUpProcess(player);
                            }
                            else
                            {
                                InputManager.Instance.isPressedPreviousFrame = false;
                            }
                        }
                        else if (hasActiveDrop)
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
                                chest.chestState = ChestState.empty;
                            }
                        }
                        else if (hasSecondaryPassiveDrop)
                        {
                            // Drop process
                            switch (passiveItemDetails.passiveItemSlotName)
                            {
                                case PassiveItemSlotName.Head:
                                    if (player.selectedPassiveItem.GetCurrentHeadPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentHeadPassiveItem());
                                        }
                                    }
                                    break;
                                case PassiveItemSlotName.Chest:
                                    if (player.selectedPassiveItem.GetCurrentChestPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentChestPassiveItem());
                                        }
                                    }
                                    break;
                                case PassiveItemSlotName.Neck:
                                    if (player.selectedPassiveItem.GetCurrentNeckPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentNeckPassiveItem());
                                        }
                                    }
                                    break;
                                case PassiveItemSlotName.Finger:
                                    if (player.selectedPassiveItem.GetCurrentFingerPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentFingerPassiveItem());
                                        }
                                    }
                                    break;
                                case PassiveItemSlotName.Back:
                                    if (player.selectedPassiveItem.GetCurrentBackPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentBackPassiveItem());
                                        }
                                    }
                                    break;
                                case PassiveItemSlotName.Waist:
                                    if (player.selectedPassiveItem.GetCurrentWaistPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentWaistPassiveItem());
                                        }
                                    }
                                    break;
                                case PassiveItemSlotName.Arm:
                                    if (player.selectedPassiveItem.GetCurrentArmPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentArmPassiveItem());
                                        }
                                    }
                                    break;
                                case PassiveItemSlotName.Leg:
                                    if (player.selectedPassiveItem.GetCurrentLegPassiveItem() != null)
                                    {
                                        if (InventoryManager.Instance.IsInventoryFull())
                                        {
                                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentLegPassiveItem());
                                        }
                                    }
                                    break;
                                default:
                                    break;
                            }

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

    private void ChestItemPassiveItemDropPickUpProcess(Player player)
    {
        if (!InputManager.Instance.isPressedPreviousFrame)
        {
            if (!isPickedUp)
            {
                // Drop process
                switch (passiveItemDetails.passiveItemSlotName)
                {
                    case PassiveItemSlotName.Head:
                        if (player.selectedPassiveItem.GetCurrentHeadPassiveItem() != null)
                        {                        
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentHeadPassiveItem());
                        }
                        break;
                    case PassiveItemSlotName.Chest:
                        if (player.selectedPassiveItem.GetCurrentChestPassiveItem() != null)
                        {
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentChestPassiveItem());
                        }
                        break;
                    case PassiveItemSlotName.Neck:
                        if (player.selectedPassiveItem.GetCurrentNeckPassiveItem() != null)
                        {
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentNeckPassiveItem());
                        }
                        break;
                    case PassiveItemSlotName.Finger:
                        if (player.selectedPassiveItem.GetCurrentFingerPassiveItem() != null)
                        {
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentFingerPassiveItem());
                        }
                        break;
                    case PassiveItemSlotName.Back:
                        if (player.selectedPassiveItem.GetCurrentBackPassiveItem() != null)
                        {
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentBackPassiveItem());
                        }
                        break;
                    case PassiveItemSlotName.Waist:
                        if (player.selectedPassiveItem.GetCurrentWaistPassiveItem() != null)
                        {
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentWaistPassiveItem());
                        }
                        break;
                    case PassiveItemSlotName.Arm:
                        if (player.selectedPassiveItem.GetCurrentArmPassiveItem() != null)
                        {
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentArmPassiveItem());
                        }
                        break;
                    case PassiveItemSlotName.Leg:
                        if (player.selectedPassiveItem.GetCurrentLegPassiveItem() != null)
                        {
                            player.playerControl.DropProcess(DropType.PassiveItem, player.selectedPassiveItem.GetCurrentLegPassiveItem());
                        }
                        break;
                    default:
                        break;
                }

                // Pick up process
                isColliding = false;
                CollectPassiveItem(player);
            }
        }
    }

    private void ChestItemWeaponDropPickUpProcess(Player player)
    {
        if (!InputManager.Instance.isPressedPreviousFrame)
        {
            if (!weaponDetails.requiredPrimaryStats.MeetsRequirements(player))
            {
                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.DontMeetRequiredPrimaryStats);
                return;
            }

            // Drop process if inventory is full
            if (InventoryManager.Instance.IsInventoryFull() && player.activeWeapon.GetCurrentMainHandWeapon() != null && !isPickedUp)
            {
                if (weaponDetails.weaponClass == WeaponClass.Shield)
                {
                    goto shieldContinue; // Skip drop process because you equip one-handed weapon and chest contains a shield
                }

                // Drop off-hand weapon if pick-up item is two-handed
                if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
                {
                    if (weaponDetails.wieldType == WieldType.TwoHanded)
                    {
                        player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentOffHandWeapon());
                    }
                }

                if (player.activeWeapon.GetCurrentMainHandWeapon() != null)
                {
                    // If pick-up weapon is two-handed, off-hand weapon is dropped and off-hand active weapon is null
                    if (weaponDetails.wieldType == WieldType.TwoHanded)
                    {
                        player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentMainHandWeapon(), true);
                    }
                    else
                    {
                        if (player.activeWeapon.GetCurrentOffHandWeapon() != null)
                        {
                            // CURRENT ACTIVE MAIN HAND WEAPON BECOMES NULL HERE AND DROP HAPPENS
                            player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentMainHandWeapon(), true);
                        }
                        else
                        {
                            if (weaponDetails.weaponClass == WeaponClass.Shield)
                            {
                                GameManager.Instance.OpenWarningPopUpMenu(PopUpReason.ShieldCantBePutOnMainHand);
                            }
                            else
                            {
                                player.playerControl.DropProcess(DropType.Weapon, player.activeWeapon.GetCurrentMainHandWeapon(), true);
                            }
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
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.meteor ||
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
                GameManager.Instance.CloseTooltipPanel();
                GameManager.Instance.CloseTooltipEquippedPanel();
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
    public void Initialize(ItemGeneric itemGeneric, Sprite sprite, Vector3 spawnPosition)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;
        this.genericItem = itemGeneric;

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

        if (isColliding) return;

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

        if (isColliding) return;

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

                // Play pickup sound effect
                SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
            }

            if (passiveItem.passiveItemDetails.primaryPassiveItemName == PrimaryPassiveItemName.Medicine)
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

        //// Introduction pop-up
        //StaticEventHandler.CallIntroductionPopUpEvent(DropType.PassiveItem, passiveItem);

        StaticEventHandler.CallPrimaryStatsChangedEvent();
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
    private void CollectActiveItem(Player player, DropItem chestItem)
    {
        if (!hasActiveDrop) return;

        if (isColliding) return;

        ActiveItem activeItem = new ActiveItem();
        activeItem.activeItemDetails = activeItemDetails;
        activeItem.activeItemMaxCharge = activeItemDetails.activeItemMaxCharge + player.additionalActiveItemCharge;

        if (activeItemDetails != null)
        {
            chestItem.remainingItemCharge = droppedByPlayer ? chestItem.remainingItemCharge : activeItem.activeItemMaxCharge;
        }

        if (!droppedByPlayer)
        {
            activeItem.activeItemRemainingCharge = activeItem.activeItemMaxCharge;
        }

        player.AddActiveItemToPlayer(activeItemDetails, this, chestItem.remainingItemCharge);

        pickUpAnimator.SetTrigger("pickUp");
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

        isColliding = true;
        isPickedUp = true;

        transform.SetParent(player.transform);
    }
}
