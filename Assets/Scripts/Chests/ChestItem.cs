using System;
using TMPro;
using UnityEngine;

public class ChestItem : MonoBehaviour
{
    [HideInInspector] public bool hasWeaponDrop = false;
    [HideInInspector] public bool hasPassiveDrop = false;
    [HideInInspector] public bool hasAmmoDrop = false;

    SpriteRenderer spriteRenderer;
    TextMeshPro textTMP;
    Enemy enemy;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    int ammoPercent;
    bool isColliding = false;

    private void Awake()
    {
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();
        textTMP = GetComponentInChildren<TextMeshPro>();
    }

    private void Start()
    {
        if (transform.parent != null)
        {
            if (transform.parent.tag == "Enemy")
            {
                enemy = GetComponentInParent<Enemy>();
            }
        }
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Player")
        {
            Player player = collision.GetComponent<Player>();
            CollectItem(player);
        }
    }

    /// <summary>
    /// Initialize for chest drops
    /// </summary>
    public void Initialize(Sprite sprite, string text, Vector3 spawnPosition)
    {
        spriteRenderer.sprite = sprite;
        textTMP.text = text;
        transform.position = spawnPosition;
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

            player.UpdateWieldedWeapons(weaponDetails);
        }
        else
        {
            //// display message saying you already have the weapon
            //StartCoroutine(DisplayMessage("WEAPON\nALREADY\nEQUIPPED", 5f));
        }

        isColliding = true;
        weaponDetails = null;
        Destroy(gameObject);
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

        if (textTMP.text == "Coin")
        {
            player.GetComponent<Coins>().Add(1);
            isColliding = true;
        }

        if (textTMP.text == "Health")
        {
            player.health.AddHealth((int)(20f / player.health.GetStartingHealth() * 100));
            isColliding = true;
        }

        passiveItemDetails = null;
        Destroy(gameObject);
    }

    /// <summary>
    /// Collect an ammo item and add it to the ammo in the players current weapon
    /// </summary>
    private void CollectAmmoItem(Player player)
    {
        if (!hasAmmoDrop) return;

        if (isColliding) return;

        // Update ammo for current weapon
        if (!player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.hasInfiniteProjectile &&
            !player.activeWeapon.GetCurrentRightHandWeapon().weaponDetails.isMeleeWeapon)
        {
            player.reloadWeaponEvent.CallReloadWeaponEvent(player.activeWeapon.GetCurrentRightHandWeapon(), ammoPercent);
            isColliding = true;
        }

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        ammoPercent = 0;
        Destroy(gameObject);
    }

    private void CollectItem(Player player)
    {
        CollectWeaponItem(player);
        CollectPassiveItem(player);
        CollectAmmoItem(player);
    }
}
