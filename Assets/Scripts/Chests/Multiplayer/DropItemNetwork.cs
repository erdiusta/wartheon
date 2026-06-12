using Mirror;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class DropItemNetwork : NetworkBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public static List<DropItemNetwork> droptItemsInRange = new List<DropItemNetwork>();

    private const float SNAP_UNIT = 0.03125f;

    [Header("Visuals")]
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public Animator animator;
    [HideInInspector] public Animator pickUpAnimator;
    [HideInInspector] public DropItemAnimationSync dropItemAnimationSync;

    [Header("Drop Flags")]
    [SyncVar] public DropSourceType dropSourceType;
    [SyncVar(hook = nameof(OnInitialize))] public bool canInitialize;
    [SyncVar] public bool hasWeaponDrop = false;
    [SyncVar] public bool hasPrimaryPassiveDrop = false;
    [SyncVar] public bool hasSecondaryPassiveDrop = false;
    [SyncVar] public bool isPickedUp = false;
    [SyncVar] public DropItemLocation currentLocation;

    [Header("Weapon Data")]
    [SyncVar] public WeaponTitle weaponTitle;
    [SyncVar] public WeaponClass weaponClass;
    [SyncVar] public WeaponStats weaponStats;
    [SyncVar] public Rarity rarity;

    [Header("Passive Data")]
    [SyncVar] public PassiveItemStats passiveStats;
    [SyncVar] public PassiveItemType passiveItemType;
    [SyncVar] public PassiveItemSlotName passiveItemSlotName;

    [Header("NPC Data")]
    public Transform priceContainer;

    [Header("Gamble Data")]
    [SyncVar] public bool isGambleDropItem;

    [Header("Input")]
    [SyncVar] public bool isPressedPreviousFrame;
    public bool isNearestDropItem;
    [SyncVar] public bool dropCompleted;

    public WeaponDetailsSO weaponDetails;
    public PassiveItemDetailsSO passiveItemDetails;

    [HideInInspector] public ItemGeneric itemGeneric;
    [HideInInspector] public static Weapon droppedThrowingAxe = null;
    [HideInInspector] public bool isInitialized;

    [HideInInspector] public bool isColliding;

    PassiveItem containingPassiveItem;

    bool isPurchased;

    Player localPlayer;
    bool isPointerOver;
    bool tooltipVisibleFromProximity;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        pickUpAnimator = transform.GetChild(1).GetComponent<Animator>();
        dropItemAnimationSync = pickUpAnimator.GetComponent<DropItemAnimationSync>();
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        localPlayer = GameManager.Instance.GetLocalPlayer();
    }

    private void OnInitialize(bool oldValue, bool newValue)
    {
        if (!newValue) return;

        StartCoroutine(DelayInitialize());
    }

    IEnumerator DelayInitialize()
    {
        yield return null;

        InitializeVisual();
    }

    public void InitializeVisual()
    {
        if (isInitialized && itemGeneric != null) return;

        if(weaponStats.weaponTitle != WeaponTitle.None)
        {
            weaponDetails = WartheonDatabase.Instance.GetWeaponDetails(weaponStats.weaponTitle);

            if (dropSourceType == DropSourceType.Enemy)
            {
                itemGeneric = WeaponDropGenerator.GetWeaponWithStats(weaponStats, rarity, ItemSlotStatus.None, -1);
            }
            else // Player drop
            {
                // Pure reconstruction - NO RNG
                itemGeneric = WeaponDropGenerator.GetWeaponWithStats(weaponStats, rarity, ItemSlotStatus.None, -1);
            }

            Initialize(weaponDetails.weaponFrontSprite, transform.position);
        }

        if (passiveStats.passiveItemType != PassiveItemType.None)
        {
            passiveItemDetails = WartheonDatabase.Instance.GetPassiveItemDetails(passiveStats.passiveItemType);

            if (dropSourceType == DropSourceType.Enemy)
            {
                itemGeneric = PassiveDropGenerator.GetPassiveWithStats(passiveStats, rarity, ItemSlotStatus.None, -1);
            }
            else
            {
                itemGeneric = PassiveDropGenerator.GetPassiveWithStats(passiveStats, rarity, ItemSlotStatus.None, -1);
            }

            if (passiveItemDetails == null) return;

            Initialize(passiveItemDetails.passiveItemSprite, transform.position);
        }
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
        if (hasPrimaryPassiveDrop) return;

        animator.SetBool(Settings.hovered, true);

        MainUI.Instance.UpdateTooltipPanelInfo(weaponStats, passiveStats, weaponTitle, passiveItemType, rarity, hasWeaponDrop, hasSecondaryPassiveDrop, tooltipSource);
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
        NpcCounterCheck(player);

        // 5 - PRIMARY PASSIVE
        if (IsPrimaryPassive(player)) return;

        // 6 - INPUT HANDLING
        if (!IsPressed()) return;

        // 7 - WEAPON PICK UP
        if (IsWeapon(player)) return;

        // 8 - SECONDARY PASSIVE
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

        if (currentLocation == DropItemLocation.Counter)
        {
            StaticEventHandler.CallNPCInteractionEndedEvent();
        }

        if (tooltipVisibleFromProximity)
        {
            tooltipVisibleFromProximity = false;
            MainUI.Instance.CloseTooltipPanel();
            MainUI.Instance.CloseTooltipEquippedPanel();
        }

        animator.SetBool(Settings.hovered, false);

        // Reset nearestChestItem when the player exits the trigger
        if (GameManager.Instance.nearestDropItemNetwork == this)
        {
            GameManager.Instance.nearestDropItemNetwork = null;
            isNearestDropItem = false;
        }

        isPressedPreviousFrame = false;
    }

    private void NearestItemCheck(Player player)
    {
        float distance = Vector2.Distance(player.transform.position, transform.position);

        var current = GameManager.Instance.nearestDropItemNetwork;

        if (current == null)
        {
            GameManager.Instance.nearestDropItemNetwork = this;
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
            GameManager.Instance.nearestDropItemNetwork = this;
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

            if (!hasPrimaryPassiveDrop && !isGambleDropItem)
            {
                MainUI.Instance.UpdateTooltipPanelInfo(weaponStats, passiveStats, weaponTitle, passiveItemType, rarity, hasWeaponDrop, hasSecondaryPassiveDrop, TooltipSource.Proximity);
            }
        }
    }

    private void NpcCounterCheck(Player player)
    {
        if (currentLocation == DropItemLocation.Counter)
        {
            InstantiatedRoom instantiatedRoom = DungeonRuntime.GetInstantiatedRoom(GameSessionManager.Instance.GetCurrentRoomNetData().roomId);

            NPC npc = instantiatedRoom.GetComponentInChildren<NPC>();

            if (npc != null && npc.npcType == NpcType.Gambler)
            {
                uint netID = (!NetworkServer.active && !NetworkClient.active) ? 0 : player.NetAuth.netId;
                StaticEventHandler.CallNPCInteractionStartedEvent(npc.npcType, netID);
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

    private bool IsWeapon(Player player)
    {
        if (!hasWeaponDrop) return false;

        if (currentLocation == DropItemLocation.Counter)
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

        player.playerInventoryNetwork.RequestPickUp(player.playerDetails.playerCharacterIndex, this);

        return true;
    }

    private bool IsSecondaryPassive(Player player)
    {
        if (!hasSecondaryPassiveDrop) return false;

        if (currentLocation == DropItemLocation.Counter)
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

        player.playerInventoryNetwork.RequestPickUp(player.playerDetails.playerCharacterIndex, this);

        return true;
    }

    private bool IsPrimaryPassive(Player player)
    {
        if (!hasPrimaryPassiveDrop) return false;

        player.playerInventoryNetwork.RequestPickUp(player.playerDetails.playerCharacterIndex, this, isPrimaryPassive: true);

        return true;
    }

    /// <summary>
    /// Initialize for enemy drops
    /// </summary>
    public void Initialize(Sprite sprite, Vector3 spawnPosition, bool isTutorial = false)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;

        if (isTutorial)
        {
            Transform tutorialArrowContainer = transform.GetChild(4);
            tutorialArrowContainer.gameObject.SetActive(true);
        }

        // Check for animation - Passive Items
        if (hasPrimaryPassiveDrop || hasSecondaryPassiveDrop)
        {
            animator.runtimeAnimatorController = passiveItemDetails?.passiveItemAnimatorController ?? animator.runtimeAnimatorController;

            if (hasSecondaryPassiveDrop)
            {
                isInitialized = true;
            }
        }

        // Check for animation - Weapons
        else if (hasWeaponDrop)
        {
            animator.runtimeAnimatorController = weaponDetails?.weaponHoverAnimatorController ?? animator.runtimeAnimatorController;

            isInitialized = true;
        }

        if (currentLocation == DropItemLocation.Counter)
        {
            SetPriceSettings(isGamble: false);
        }
    }

    private void SetPriceSettings(bool isGamble)
    {
        priceContainer.gameObject.SetActive(true);

        TextMeshPro priceText = priceContainer.GetChild(1).GetComponent<TextMeshPro>();

        if (isGamble)
        {
            priceText.text = $"x 10";
        }
        else
        {
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

    [Server]
    public void PickUpPrimaryPassive_Server(Player player)
    {
        if (isPickedUp) return;

        CollectPrimaryPassiveItem(player);

        isPickedUp = true;
        isColliding = true;
        isPurchased = false;

        pickUpAnimator.SetTrigger("pickUp");

        switch (passiveStats.passiveItemType)
        {
            case PassiveItemType.Health:
            case PassiveItemType.Cure:
            case PassiveItemType.Mana:
                NetworkSoundManager.Instance.ServerPlaySound(SoundName.HealthPickUp, transform.position);
                break;
            case PassiveItemType.SilverCoin:
            case PassiveItemType.GoldCoin:
                NetworkSoundManager.Instance.ServerPlaySound(SoundName.CoinPickUp, transform.position);
                break;
            case PassiveItemType.Key:
                NetworkSoundManager.Instance.ServerPlaySound(SoundName.ItemPickUp, transform.position);
                break;
            default:
                break;
        }

        RpcPickUpPrimaryPassive(player.NetAuth.netIdentity);

        StartCoroutine(DestroyRoutine(1f));
    }

    [ClientRpc]
    void RpcPickUpPrimaryPassive(NetworkIdentity playerNetId)
    {
        if (isServer) return;

        Player player = playerNetId.GetComponent<Player>();

        pickUpAnimator.SetTrigger("pickUp");

        if (player == null || !player.IsLocal) return;
        
        CollectPrimaryPassiveItem(player);
    }

    private void CollectPrimaryPassiveItem(Player player)
    {
        if (!hasPrimaryPassiveDrop) return;

        PassiveItem passiveItem = (PassiveItem)itemGeneric;

        if (passiveStats.passiveItemType == PassiveItemType.Key)
        {
            player.consumableEvent.CallKeyCountChangedEvent(++player.keyCount);

        }

        if (passiveStats.passiveItemType == PassiveItemType.SilverCoin)
        {
            int coinAmount = 1;

            player.coinsAndShards.AddCoin(coinAmount);
        }

        if (passiveStats.passiveItemType == PassiveItemType.GoldCoin)
        {
            int coinAmount = 5;

            player.coinsAndShards.AddCoin(coinAmount);
        }

        if (passiveStats.passiveItemType == PassiveItemType.Health)
        {
            player.UpdatePlayerHealth(20, false, false);
        }

        if (passiveStats.passiveItemType == PassiveItemType.Mana)
        {
            player.UpdatePlayerMana(20, false, false);
        }

        if (passiveStats.passiveItemType == PassiveItemType.Cure)
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
        }
    }

    public bool MeetsRequirements(Player player)
    {
        switch (player.playerDetails.playerCharacterIndex)
        {
            case Character.Caelion:
                if (weaponClass == WeaponClass.Sword || weaponClass == WeaponClass.Shield) return true; break;
            case Character.Morven:
                if (weaponClass == WeaponClass.Dagger) return true; break;
            case Character.Nyveran:
                if (weaponClass == WeaponClass.Bow || weaponClass == WeaponClass.Crossbow) return true; break;
            case Character.Karnag:
                if (weaponClass == WeaponClass.Axe) return true; break;
            case Character.Kynara:
            case Character.Mycara:
            case Character.Nymara:
                if (weaponClass == WeaponClass.Staff) return true; break;
            case Character.Nyxa:
                if (weaponClass == WeaponClass.Dagger || weaponClass == WeaponClass.Crossbow) return true; break;
            default: break;
        }

        return false;
    }

    IEnumerator DestroyRoutine(float duration)
    {
        yield return new WaitForSeconds(duration);

        NetworkServer.Destroy(gameObject);
    }
}
