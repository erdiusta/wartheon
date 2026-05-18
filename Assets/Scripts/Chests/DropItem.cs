using Mirror;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItem : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static List<DropItem> droptItemsInRange = new List<DropItem>();
    private const float SNAP_UNIT = 0.03125f;

    [SerializeField] Transform tooltipPanel;

    public Transform priceContainer;

    [HideInInspector] public DropSourceType dropSourceType;
    [HideInInspector] public bool hasWeaponDrop = false;
    [HideInInspector] public bool hasPrimaryPassiveDrop = false;
    [HideInInspector] public bool hasSecondaryPassiveDrop = false;

    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;
    [HideInInspector] public BoxCollider2D boxCollider2D;

    [HideInInspector] public bool isPickedUp = false;
    [HideInInspector] public bool isColliding;
    [HideInInspector] public ItemGeneric itemGeneric;

    // GAMBLE
    [HideInInspector] public bool isGambleDropItem;
    [HideInInspector] public int gambleValue;
    [HideInInspector] public bool isRevealed;

    [HideInInspector] public static Weapon droppedThrowingAxe = null;
    [HideInInspector] public bool isInitialized = false;
    [HideInInspector] public bool dropCompleted;
    [HideInInspector] public Counter owningCounter;
    [HideInInspector] public bool isPressedPreviousFrame;

    bool isPurchased;
    bool trackPlayer;
    bool isNearestDropItem;

    public WeaponDetailsSO weaponDetails;
    public PassiveItemDetailsSO passiveItemDetails;
    Player player;

    public Animator pickUpAnimator;
    bool isPointerOver = false;

    // Tooltip interaction for 
    bool tooltipVisibleFromProximity = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        pickUpAnimator = transform.GetChild(1).GetComponent<Animator>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
    {
        player = GameManager.Instance.GetLocalPlayer();
    }

    private void OnEnable()
    {

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

        //if (trackPlayer && !isPickedUp)
        //{
        //    Vector3 targetPos = player.transform.position + new Vector3(0f, 0.5f, 0f);

        //    // Calculcate direction
        //    Vector3 direction = (targetPos - transform.position).normalized;

        //    // Move by one snapped step in that direction
        //    Vector3 step = direction * SNAP_UNIT * 2;

        //    // Only move if not overshooting the target
        //    if ((targetPos - transform.position).sqrMagnitude > step.sqrMagnitude)
        //    {
        //        transform.position = SnapPosition(transform.position + step);
        //    }
        //    else
        //    {
        //        transform.position = SnapPosition(targetPos);
        //    }
        //}
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
        DropItem dropItem = eventData.pointerEnter.GetComponent<DropItem>();
        Player relatedPlayer = eventData.pointerEnter.GetComponentInParent<Player>();
        Enemy enemy = eventData.pointerEnter.GetComponentInParent<Enemy>();

        if (dropItem == this && relatedPlayer == null && enemy == null && isInitialized)
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

        Weapon weapon = itemGeneric as Weapon;
        PassiveItem passiveItem = itemGeneric as PassiveItem;
        WeaponStats weaponStats = default;
        PassiveItemStats passiveItemStats = default;
        WeaponTitle weaponTitle = WeaponTitle.None;
        PassiveItemType passiveItemType = PassiveItemType.None;
        Rarity rarity = Rarity.Basic;

        if (weapon != null)
        {
            weaponStats = weapon.weaponStats;
            passiveItemStats = default;
            weaponTitle = weapon.weaponStats.weaponTitle;
            passiveItemType = default;
            rarity = weapon.Rarity;
        }
        else if (passiveItem != null)
        {
            weaponStats = default;
            passiveItemStats = passiveItem.passiveStats;
            weaponTitle = default;
            passiveItemType = passiveItem.passiveStats.passiveItemType;
            rarity = passiveItem.Rarity;
        }

        MainUI.Instance.UpdateTooltipPanelInfo(weaponStats, passiveItemStats, weaponTitle, passiveItemType, rarity, hasWeaponDrop, hasSecondaryPassiveDrop, tooltipSource);
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
        if (!dropCompleted) return;

        // Ignore irrelevant collisions
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.aoeSkill ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile || collision.tag == Settings.playerWeapon) return;

        Player player = collision.GetComponent<Player>();
        if (player == null || !player.IsLocal) return;

        // 1 - NEAREST ITEM FIXED
        NearestItemCheck(player);

        // 2 - HOVER VISUAL
        animator.SetBool(Settings.hovered, isNearestDropItem);
        if (!isNearestDropItem) return;

        // 3 - TOOLTIP
        TooltipCheck();

        // 4 - NPC / COUNTER LOGIC
        NpcCounterCheck();

        // 5 - PRIMARY PASSIVE
        if (IsPrimaryPassive(player)) return;

        // 6 - INPUT HANDLING
        if (!IsPressed()) return;

        // 7 - GAMBLE
        if (IsGamble(player)) return;

        // 8 - WEAPON PICK UP
        if (IsWeapon(player)) return;

        // 9 - SECONDARY PASSIVE
        if (IsSecondaryPassive(player)) return;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == Settings.chestItemTag || collision.tag == Settings.enemyProjectile || collision.tag == Settings.aoeSkill ||
            collision.tag == Settings.enemyTag || collision.tag == Settings.playerProjectile || collision.tag == Settings.playerWeapon) return;

        if (collision.CompareTag(Settings.playerTag) || collision.CompareTag(Settings.playerWeapon)) droptItemsInRange.Remove(this);

        if (!collision.CompareTag(Settings.playerTag)) return;

        Player player = collision.GetComponent<Player>();
        if (player == null || !player.IsLocal) return;

        if (owningCounter != null)
        {
            StaticEventHandler.CallNPCInteractionEndedEvent();
        }

        if (MainUI.Instance != null && tooltipVisibleFromProximity)
        {
            tooltipVisibleFromProximity = false;
            MainUI.Instance.CloseTooltipPanel();
            MainUI.Instance.CloseTooltipEquippedPanel();
        }

        animator.SetBool(Settings.hovered, false);

        // Reset nearestChestItem when the player exits the trigger
        if (GameManager.Instance != null && GameManager.Instance.nearestDropItem == this)
        {
            GameManager.Instance.nearestDropItem = null;
            isNearestDropItem = false;
        }

        isPressedPreviousFrame = false;
    }

    private void NearestItemCheck(Player player)
    {
        float distance = Vector2.Distance(player.transform.position, transform.position);

        var current = GameManager.Instance.nearestDropItem;

        if (current == null)
        {
            GameManager.Instance.nearestDropItem = this;
            isNearestDropItem = true;
            return;
        }

        float currentDist = Vector2.Distance(player.transform.position, current.transform.position);

        if (this == current)
        {
            isNearestDropItem = true;
            return;
        }

        if (distance < currentDist)
        {
            current.isNearestDropItem = false;
            GameManager.Instance.nearestDropItem = this;
            isNearestDropItem = true;
        }
        else
        {
            isNearestDropItem = false;
        }
    }

    private void TooltipCheck()
    {
        if (!tooltipVisibleFromProximity)
        {
            tooltipVisibleFromProximity = true;

            WeaponStats weaponStats = itemGeneric is Weapon w ? w.weaponStats : default;
            PassiveItemStats passiveStats = itemGeneric is PassiveItem p ? p.passiveStats : default;

            if (!hasPrimaryPassiveDrop && !isGambleDropItem)
            {
                MainUI.Instance.UpdateTooltipPanelInfo(weaponStats, passiveStats, weaponStats.weaponTitle, passiveStats.passiveItemType, itemGeneric.Rarity, hasWeaponDrop, hasSecondaryPassiveDrop, TooltipSource.Proximity);
            }
        }
    }

    private void NpcCounterCheck()
    {
        bool isMultiplayer = NetworkServer.active || NetworkClient.active;

        if (owningCounter != null)
        {
            InstantiatedRoom ir = isMultiplayer ? DungeonRuntime.GetInstantiatedRoom(GameSessionManager.Instance.GetCurrentRoomNetData().roomId) : GameManager.Instance.GetCurrentRoom().instantiatedRoom;
            NPC npc = ir.GetComponentInChildren<NPC>();

            if (npc != null && npc.npcType == NpcType.Gambler)
            {
                StaticEventHandler.CallNPCInteractionStartedEvent(npc.npcType);
            }
        }
    }

    private bool IsPressed()
    {
        bool pressed = InputManager.Instance.interaction.action.IsPressed();

        if (!pressed)
        {
            isPressedPreviousFrame = false;
            return false;
        }

        if (InputManager.interactionDisabled) return false;

        // Prevent spam
        if (isPressedPreviousFrame) return false;

        isPressedPreviousFrame = true;
        return true;
    }

    private bool IsGamble(Player player)
    {
        if (!isGambleDropItem || owningCounter == null) return false;
        if (owningCounter.isGambleLocked) return true;

        int gambleBlind = 10;

        if (player.coinsAndShards.coinAmount < gambleBlind)
        {
            StaticDialogueHandler.CallInsufficientFundsEvent();
            return true;
        }

        // Pay the blind
        player.coinsAndShards.AddCoin(-gambleBlind);

        // Get the gamble results
        player.coinsAndShards.AddCoin(gambleValue + gambleBlind);

        InstantiatedRoom ir;
        NPC gambleNpc;

        if (!NetworkServer.active && !NetworkClient.active) ir = GameManager.Instance.GetCurrentRoom().instantiatedRoom;
        else ir = DungeonRuntime.GetInstantiatedRoom(GameSessionManager.Instance.GetCurrentRoomNetData().roomId);

        gambleNpc = ir.GetComponentInChildren<NPC>();

        if (gambleValue < 0)
        {
            SoundEffectManager.Instance.PlaySoundEffect(gambleNpc.gambleLostSoundEffect);
            StaticDialogueHandler.CallGambleLostEvent();
        }
        else
        {
            SoundEffectManager.Instance.PlaySoundEffect(gambleNpc.gambleWinSoundEffect);
            StaticDialogueHandler.CallGambleWonEvent();
        }

        StaticEventHandler.CallGambleCompletedEvent(player);

        return true;
    }

    private bool IsWeapon(Player player)
    {
        if (!hasWeaponDrop) return false;

        if (owningCounter != null)
        {
            if (weaponDetails != null)
            {
                int price = Mathf.RoundToInt(weaponDetails.price * (1 + player.additionalNPCCostModifier));

                if (!isPurchased)
                {
                    if (player.coinsAndShards.coinAmount < price)
                    {
                        StaticDialogueHandler.CallInsufficientFundsEvent();
                        return true;
                    }
                    else
                    {
                        player.coinsAndShards.AddCoin(-price);
                        isPurchased = true;
                        StaticDialogueHandler.CallTradeCompletedEvent();
                    }
                }
            }
        }

        player.playerInventory.PickUpProcess(player.playerDetails.playerCharacterIndex, this);

        //DropWeaponPickUpProcess(player, weaponDetails);

        return true;
    }

    private bool IsSecondaryPassive(Player player)
    {
        if (!hasSecondaryPassiveDrop) return false;

        if (owningCounter != null)
        {
            int price = (int)(passiveItemDetails.price * (1 + player.additionalNPCCostModifier));

            if (!isPurchased)
            {
                if (player.coinsAndShards.coinAmount < price)
                {
                    StaticDialogueHandler.CallInsufficientFundsEvent();
                    return true;
                }
                else
                {
                    player.coinsAndShards.AddCoin(-price);
                    isPurchased = true;
                    StaticDialogueHandler.CallTradeCompletedEvent();
                }
            }
        }

        player.playerInventory.PickUpProcess(player.playerDetails.playerCharacterIndex, this);

        //DropPassiveItemPickUpProcess(player);

        return true;
    }

    private bool IsPrimaryPassive(Player player)
    {
        if (!hasPrimaryPassiveDrop) return false;

        player.playerInventory.PickUpProcess(player.playerDetails.playerCharacterIndex, this, isPrimaryPassive: true);
        return true;
    }

    public int ShardGainProcess(Player player)
    {
        int shardGain = 0;

        switch (itemGeneric.Rarity)
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

    /// <summary>
    /// Initialize for enemy drops
    /// </summary>
    public void Initialize(ItemGeneric itemGeneric, Sprite sprite, Vector3 spawnPosition, Counter counter, bool isTutorial = false)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;
        this.itemGeneric = itemGeneric;
        owningCounter = counter;

        if (isTutorial)
        {
            Transform tutorialArrowContainer = transform.GetChild(4);
            tutorialArrowContainer.gameObject.SetActive(true);
        }

        // Check for animation - Passive Items
        if (hasPrimaryPassiveDrop || hasSecondaryPassiveDrop)
        {
            PassiveItem passiveItem = (PassiveItem)itemGeneric;

            Debug.Log("Passive item type is " + passiveItem.passiveStats.passiveItemType);

            passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItem.passiveStats.passiveItemType);

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

            weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weapon.weaponStats.weaponTitle);

            animator.runtimeAnimatorController = weaponDetails?.weaponHoverAnimatorController ?? animator.runtimeAnimatorController;

            isInitialized = true;
        }

        if (owningCounter != null)
        {
            priceContainer.gameObject.SetActive(true);

            TextMeshPro priceText = priceContainer.GetChild(1).GetComponent<TextMeshPro>();
            float priceModifier = 1 + GameManager.Instance.GetLocalPlayer().additionalNPCCostModifier;

            if (itemGeneric is Weapon w)
            {
                priceText.text = $"x {w.weaponStats.activePrice * priceModifier}";
            }
            else if (itemGeneric is PassiveItem p)
            {
                priceText.text = $"x {p.passiveStats.activePrice * priceModifier}";
            }
        }
    }

    public void InitializeGamble(Counter counter, DropItem drop)
    {
        drop.spriteRenderer.sprite = null;
        drop.animator.runtimeAnimatorController = GameResources.Instance.gambleDiceAnimatorController;

        drop.owningCounter = counter;

        drop.dropCompleted = true; // Initialize starts here
    }

    public void PickUpPrimaryPassive(Player player)
    {
        if (isPickedUp) return;

        CollectPrimaryPassiveItem(player);

        isPickedUp = true;
        isColliding = true;
        isPurchased = false;

        Destroy(gameObject, 1f);
    }

    private void CollectPrimaryPassiveItem(Player player)
    {
        if (!hasPrimaryPassiveDrop) return;

        PassiveItem passiveItem = (PassiveItem)itemGeneric;

        if (passiveItem.passiveStats.passiveItemType == PassiveItemType.Key)
        {
            player.consumableEvent.CallKeyCountChangedEvent(++player.keyCount);

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.itemPickup);
        }

        if (passiveItem.passiveStats.passiveItemType == PassiveItemType.SilverCoin)
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

        if (passiveItem.passiveStats.passiveItemType == PassiveItemType.GoldCoin)
        {
            player.coinsAndShards.AddCoin(5);

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.coinPickup);
        }

        if (passiveItem.passiveStats.passiveItemType == PassiveItemType.Health)
        {
            player.UpdatePlayerHealth(20, false, false);

            if (InputManager.TutorialEnabled && TutorialInteraction.Instance.currentTutorialPhase == TutorialPhase.PickUpPrimaryPassiveHealth)
            {
                TutorialInteraction.Instance.currentTutorialProcess = TutorialProcess.QuestPassed;
            }

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
        }

        if (passiveItem.passiveStats.passiveItemType == PassiveItemType.Mana)
        {
            player.UpdatePlayerMana(20, false, false);

            // Play pickup sound effect
            SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);
        }

        if (passiveItem.passiveStats.passiveItemType == PassiveItemType.Cure)
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

        StaticEventHandler.CallStatsChangedOnTheBookEvent();

        pickUpAnimator.SetTrigger("pickUp");
        passiveItemDetails = null;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;

        if (GameManager.Instance.nearestDropItemNetwork == this)
        {
            GameManager.Instance.nearestDropItemNetwork = null;
            isNearestDropItem = false;

            Debug.Log("It is not the nearest item because object is about to be destroyed.");
        }
    }
    public bool MeetsRequirements()
    {
        switch (player.playerDetails.playerCharacterIndex)
        {
            case Character.Caelion:
                if (weaponDetails.weaponClass == WeaponClass.Sword || weaponDetails.weaponClass == WeaponClass.Shield) return true; break;
            case Character.Morven:
                if (weaponDetails.weaponClass == WeaponClass.Dagger) return true; break;
            case Character.Nyveran:
                if (weaponDetails.weaponClass == WeaponClass.Dagger|| weaponDetails.weaponClass == WeaponClass.Bow ||
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