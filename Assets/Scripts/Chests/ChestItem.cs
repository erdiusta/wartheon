using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class ChestItem : MonoBehaviour
{
    [HideInInspector] public bool hasWeaponDrop = false;
    [HideInInspector] public bool hasPassiveDrop = false;
    [HideInInspector] public bool hasAmmoDrop = false;
    [HideInInspector] public TextMeshPro textTMP;
    [HideInInspector] public WeaponAnimator weaponAnimator;

    Chest chest;
    SpriteRenderer spriteRenderer;
    Animator animator;
    ParticleSystem collectParticleSystem;
    Enemy enemy;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    int ammoPercent;
    bool isColliding = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        animator = GetComponentInChildren<Animator>();
        textTMP = GetComponentInChildren<TextMeshPro>();
        collectParticleSystem = GetComponentInChildren<ParticleSystem>();
        chest = GetComponentInParent<Chest>();
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
                            CollectWeaponItem(player);
                        }
                    }
                    if (hasAmmoDrop)
                    {
                        if (InputManager.Instance.interaction.action.IsPressed())
                        {
                            CollectAmmoItem(player);
                        }
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
    public void Initialize(WeaponDetailsSO weaponDetails, Sprite sprite, string text, Vector3 spawnPosition)
    {
        if (weaponDetails != null)
        {
            this.weaponDetails = weaponDetails;
        }

        spriteRenderer.sprite = sprite;
        textTMP.text = text;
        transform.position = spawnPosition;

        // Check for animation - Ammo
        if (hasAmmoDrop)
        {
            animator.runtimeAnimatorController = GameResources.Instance.ammoHoverAnimatorController;
        }

        // Check for animation - Weapons
        for (int i = 0; i < GameResources.Instance.weaponsHoverArray.Length; i++)
        {
            if (textTMP.text == GameResources.Instance.weaponsHoverArray[i].weaponName)
            {
                weaponAnimator = GameResources.Instance.weaponsHoverArray[i];
                animator.runtimeAnimatorController = weaponAnimator.weaponHoverAnimatorController;
                break;
            }
        }

        // Check for animation - Passive Items
        switch (textTMP.text)
        {
            case "Silver Coin":
                animator.runtimeAnimatorController = GameResources.Instance.silverCoinShineAnimatorController;
                break;
            case "Gold Coin":
                animator.runtimeAnimatorController = GameResources.Instance.goldCoinShineAnimatorController;
                break;
            case "Health":
                animator.runtimeAnimatorController = GameResources.Instance.heartShineAnimatorController;
                break;
            case "Status Cure":
                animator.runtimeAnimatorController = GameResources.Instance.cureShineAnimatorController;
                break;
            case "Silver Armor":
                animator.runtimeAnimatorController = GameResources.Instance.silverArmorShineAnimatorController;
                break;
            default:
                break;
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
            player.UpdateWieldedWeapons(weaponDetails, true);
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
        if (!hasPassiveDrop) return;

        if (isColliding) return;

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

        if (textTMP.text == "Silver Coin")
        {
            player.GetComponent<Coins>().Add(1);
            isColliding = true;
        }

        if (textTMP.text == "Gold Coin")
        {
            player.GetComponent<Coins>().Add(5);
            isColliding = true;
        }

        if (textTMP.text == "Health")
        {
            player.health.AddHealth((int)(20f / player.health.GetStartingHealth() * 100));
            isColliding = true;
        }

        if (textTMP.text == "Status Cure")
        {
            // HEALTH STATUS CHECKS
            if (player.healthStatus == HealthStatus.Bleeding)
            {
                player.healthEvent.CallBleedingCuredEvent();
            }
            if (player.healthStatus == HealthStatus.Poisoned)
            {
                player.healthEvent.CallPoisonCuredEvent();
            }

            player.healthStatus = HealthStatus.Normal;

            // MOVE STATUS CHECKS
            if (player.moveStatus == MoveStatus.Slow)
            {
                player.movementByVelocity.moveSpeed = Random.Range(player.movementByVelocity.playerStartingMinSpeed, 
                    player.movementByVelocity.playerStartingMaxSpeed);
                player.healthEvent.CallSlowCuredEvent();
            }

            player.moveStatus = MoveStatus.Idle;

            // ARMOR STATUS CHECKS
            if (player.armorStatus == ArmorStatus.Acid)
            {
                player.armorStatus = ArmorStatus.Normal;
                player.healthEvent.CallAcidCuredEvent();
            }

            isColliding = true;
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
            isColliding = true;
        }

        collectParticleSystem.Play();
        passiveItemDetails = null;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;
        Destroy(gameObject, 1f);
    }

    /// <summary>
    /// Collect an ammo item and add it to the ammo in the players current weapon
    /// </summary>
    private void CollectAmmoItem(Player player)
    {
        if (!hasAmmoDrop) return;

        if (isColliding) return;

        ammoPercent = Random.Range(0, 101);

        // Update ammo for current weapon
        if (!player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasInfiniteProjectile &&
            !player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon)
        {
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentRightHandWeapon(), ammoPercent);
            isColliding = true;
        }

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        collectParticleSystem.Play();
        ammoPercent = 0;
        animator.runtimeAnimatorController = null;
        spriteRenderer.sprite = null;
        Destroy(gameObject, 1f);
    }
}
