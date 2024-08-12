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
    [HideInInspector] public ActiveItem toBeDroppedActiveItem;
    [HideInInspector] public PassiveItem toBeDroppedPassiveItem;
    
    bool isColliding;
    Chest chest;
    ParticleSystem collectParticleSystem;
    Enemy enemy;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    ActiveItemDetailsSO activeItemDetails;
    int ammoPercent;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        textTMP = GetComponentInChildren<TextMeshPro>();
        collectParticleSystem = GetComponentInChildren<ParticleSystem>();
        chest = GetComponentInParent<Chest>();
        boxCollider2D = GetComponent<BoxCollider2D>();
    }

    private void Start()
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
        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            Player player = collision.GetComponent<Player>();

            if (chest == null)
            {
                try
                {
                    animator.SetBool(Settings.hovered, true);

                    if (hasWeaponDrop)
                    {
                        if (InputManager.Instance.interaction.action.IsPressed())
                        {
                            if (GetComponentInParent<Counter>() != null)
                            {
                                if (GameManager.Instance.GetPlayer().coins.coinAmount >= weaponDetails.price)
                                {
                                    GameManager.Instance.GetPlayer().coins.coinAmount -= weaponDetails.price;
                                }
                                else
                                {
                                    StaticDialogueHandler.CallInsufficientFundsEvent();
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
                                    player.playerControl.DropProcess(GameManager.Instance.GetToBeDroppedChestItem());
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
                    animator.SetBool(Settings.hovered, true);

                    if (InputManager.Instance.interaction.action.IsPressed())
                    {
                        CollectWeaponItem(player);
                    }
                    else
                    {
                        CollectPassiveItem(player);
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
        if (collision.tag == Settings.playerTag || collision.tag == Settings.playerWeapon)
        {
            animator.SetBool(Settings.hovered, false);
        }
    }

    /// <summary>
    /// Initialize for enemy drops
    /// </summary>
    public void Initialize(WeaponDetailsSO weaponDetails, ActiveItemDetailsSO activeItemDetails, PassiveItemDetailsSO passiveItemDetails, Sprite sprite, 
        string text, Vector3 spawnPosition)
    {
        spriteRenderer.sprite = sprite;
        transform.position = spawnPosition;
        textTMP.text = text;

        // Check for animation - Active Item
        if (hasActiveDrop)
        {
            this.activeItemDetails = activeItemDetails;
            animator.runtimeAnimatorController = activeItemDetails.activeItemAnimatorController;
        }

        // Check for animation - Passive Items
        if (hasPrimaryPassiveDrop || hasSecondaryPassiveDrop)
        {
            this.passiveItemDetails = passiveItemDetails;
            animator.runtimeAnimatorController = passiveItemDetails.passiveItemAnimatorController;
        }

        // Check for animation - Weapons
        if (hasWeaponDrop)
        {
            this.weaponDetails = weaponDetails;
            animator.runtimeAnimatorController = weaponDetails.weaponHoverAnimatorController;
        }
    }

    /// <summary>
    /// Collect the weapon and add it to the players weapons list
    /// </summary>
    private void CollectWeaponItem(Player player)
    {
        if (!hasWeaponDrop) return;

        if (isColliding) return;

        if (weaponDetails != null)
        {
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

        collectParticleSystem.Play();

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

            if (textTMP.text == "Gold Coin")
            {
                player.GetComponent<Coins>().Add(5);
            }

            if (textTMP.text == "Health")
            {
                player.health.AddHealth((int)(20f / player.health.GetStartingHealth() * 100));
            }

            if (textTMP.text == "Status Cure")
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

            if (textTMP.text == "Silver Armor")
            {
                if (player.armorStatus == ArmorStatus.Acid)
                {
                    player.healthEvent.CallAcidCuredEvent();
                }

                player.armorStatus = ArmorStatus.SilverArmor;
                player.health.SetArmorValue();
                player.healthEvent.CallGetSilverArmorEvent();
                Debug.Log("Player's current armor value is " + player.health.currentArmorValue);
            }

            if (textTMP.text == "Ammo")
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

                ammoPercent = 0;
            }

        }
        else if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Secondary)
        {
            player.AddPassiveItemToPlayer(passiveItemDetails);

            isPickedUp = true;
        }

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

        isColliding = true;
        collectParticleSystem.Play();
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

        Debug.Log("Collecting item: " + activeItemDetails.activeItemName);

        chestItem.remainingItemCharge = droppedByPlayer ? chestItem.remainingItemCharge : activeItemDetails.activeItemMaxCharge;

        player.AddActiveItemToPlayer(activeItemDetails, this, chestItem.remainingItemCharge);

        collectParticleSystem.Play();
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        isColliding = true;
        isPickedUp = true;

        //animator.runtimeAnimatorController = null;
        //spriteRenderer.sprite = null;

        transform.SetParent(player.transform);
    }
}
