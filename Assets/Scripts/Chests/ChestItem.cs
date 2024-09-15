using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChestItem : MonoBehaviour
{
    [HideInInspector] public bool hasWeaponDrop = false;
    [HideInInspector] public bool hasActiveDrop = false;
    [HideInInspector] public bool hasPrimaryPassiveDrop = false;
    [HideInInspector] public bool hasSecondaryPassiveDrop = false;
    [HideInInspector] public TextMeshPro textTMP;
    [HideInInspector] public WeaponAnimator weaponAnimator;
    [HideInInspector] public SpriteRenderer spriteRenderer;
    [HideInInspector] public bool isPickedUp = false;
    [HideInInspector] public Animator animator;
    [HideInInspector] public BoxCollider2D boxCollider2D;
    [HideInInspector] public int remainingItemCharge;
    [HideInInspector] public bool droppedByPlayer = false;
    [HideInInspector] public Weapon toBeDroppedWeapon;
    [HideInInspector] public ActiveItem toBeDroppedActiveItem;
    [HideInInspector] public PassiveItem toBeDroppedPassiveItem;
    [HideInInspector] public bool isColliding;
    [HideInInspector] public static ChestItem toBeDroppedChestItem;
    [HideInInspector] public static ChestItem nearestChestItem = null;

    Chest chest;
    Enemy enemy;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    ActiveItemDetailsSO activeItemDetails;
    int ammoPercent;
    bool isPurchasing;
    Animator pickUpAnimator;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = transform.GetChild(0).GetComponent<Animator>();
        pickUpAnimator = transform.GetChild(2).GetComponent<Animator>();
        textTMP = GetComponentInChildren<TextMeshPro>();
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
                                    CollectWeaponItem(player);
                                }
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
                                        Debug.Log("Dropping item from ChestItem.");
                                        player.playerControl.DropProcess(toBeDroppedChestItem, DropType.ActiveItem);
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
    public void Initialize(IReceivable receivable, Sprite sprite, string text, Vector3 spawnPosition)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;
        textTMP.text = text;

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
            if (textTMP.text == "Key")
            {
                player.keyCount++;
            }

            if (textTMP.text == "Silver Coin")
            {
                player.GetComponent<Coins>().Add(1);
            }

            if (textTMP.text == "Golden Coin")
            {
                player.GetComponent<Coins>().Add(5);
            }

            if (textTMP.text == "Health")
            {
                player.health.AddHealth((int)(20f / player.health.GetStartingHealth() * 100));
            }

            if (textTMP.text == "Medicine")
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

            if (textTMP.text == "Holy Water")
            {
                if (player.isCursed)
                {
                    player.healthEvent.CallCurseCuredEvent();
                    player.isCursed = false;
                }
            }

            if (textTMP.text == "Quiver")
            {
                ammoPercent = Random.Range(0, 101);

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
}
