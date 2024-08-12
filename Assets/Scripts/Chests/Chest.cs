using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Chest : MonoBehaviour, IUsable
{
    [HideInInspector] public bool dropCompleted = false;
    [HideInInspector] public ChestState chestState = ChestState.closed;
    [HideInInspector] public bool bobbyPinTried = false;
    [HideInInspector] public bool bobbyPinTrySuccessful = false;
    [HideInInspector] public Coroutine chestLockSoundRoutine;

    #region Tooltip
    [Tooltip("Populate withItemSpawnPoint transform")]
    #endregion Tooltip
    [SerializeField] private Transform itemSpawnPoint;
    int healthPercent;
    WeaponDetailsSO weaponDetails;
    int ammoPercent;
    Animator animator;
    SpriteRenderer spriteRenderer;
    bool isEnabled = false;

    GameObject chestItemGameObject;
    ChestItem chestItem;
    TextMeshPro messageTextTMP;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        messageTextTMP = GetComponentInChildren<TextMeshPro>();
    }

    /// <summary>
    /// Initialize Chest and either make it visible immediately or materialize it
    /// </summary>
    public void Initialize(WeaponDetailsSO weaponDetails)
    {
        this.weaponDetails = weaponDetails;

        EnableChest();
    }

    /// <summary>
    /// Enable the chest
    /// </summary>
    private void EnableChest()
    {
        // Set use to enabled
        isEnabled = true;
    }

    /// <summary>
    /// Use the chest - action will vary depending on the chest state
    /// </summary>
    public void StartChestProcess()
    {
        if (!isEnabled) return;

        switch (chestState)
        {
            case ChestState.closed:
                if (GameManager.Instance.GetPlayer().keyCount > 0 || bobbyPinTrySuccessful)
                {
                    OpenChest();
                    StartCoroutine(MoveItemDown(chestItem.transform, 1.5f));
                }
                else
                {
                    if (chestLockSoundRoutine == null)
                    {
                        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestLock);
                    }
                }

                break;

            case ChestState.weaponItem:
                //CollectWeaponItem();
                break;

            case ChestState.empty:
                return;

            default:
                return;
        }
    }

    /// <summary>
    /// Open the chest on first use
    /// </summary>
    private void OpenChest()
    {
        if (!bobbyPinTrySuccessful)
        {
            GameManager.Instance.GetPlayer().keyCount--;
        }
        animator.SetBool(Settings.use, true);

        // chest open sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

        // Check if player already has the weapon - if so set weapon to null
        if (weaponDetails != null)
        {
            if (GameManager.Instance.GetPlayer().weaponRightHandList.Contains(weaponDetails.GetWeapon()))
                weaponDetails = null;
        }

        UpdateChestState();
       
    }

    /// <summary>
    /// Create items based on what should be spawned and the chest state
    /// </summary>
    private void UpdateChestState()
    {
        if (weaponDetails != null)
        {
            chestState = ChestState.weaponItem;
            InstantiateWeaponItem();
        }
        else
        {
            chestState = ChestState.empty;
        }
    }

    /// <summary>
    /// Instantiate a chest item
    /// </summary>
    private void InstantiateItem()
    {
        chestItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);

        chestItem = chestItemGameObject.GetComponent<ChestItem>();
    }

    /// <summary>
    /// Instantiate a health item for the player to collect
    /// </summary>
    private void InstantiateHealthItem()
    {
        InstantiateItem();

        chestItem.Initialize(null, null, null, GameResources.Instance.heartIcon, healthPercent.ToString() + "%", itemSpawnPoint.position);
    }

    /// <summary>
    /// Collect the health item and add it to the players health
    /// </summary>
    private void CollectHealthItem()
    {
        // Check item exists and has been materialized
        if (chestItem == null) return;

        // Add health to player
        GameManager.Instance.GetPlayer().health.AddHealth(healthPercent);

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.healthPickup);

        healthPercent = 0;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    /// <summary>
    /// Instantiate a ammo item for the player to collect
    /// </summary>
    private void InstantiateAmmoItem()
    {
        InstantiateItem();

        chestItem.Initialize(null, null, null, GameResources.Instance.ammoDropIcon, ammoPercent.ToString() + "%", itemSpawnPoint.position);
    }

    /// <summary>
    /// Collect an ammo item and add it to the ammo in the players current weapon
    /// </summary>
    private void CollectAmmoItem()
    {
        // Check item exists and has been materialized
        if (chestItem == null) return;

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.ammoPickup);

        ammoPercent = 0;

        Destroy(chestItemGameObject);

        UpdateChestState();
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem()
    {
        InstantiateItem();

        chestItemGameObject.GetComponent<ChestItem>().Initialize(weaponDetails, null, null, weaponDetails.weaponFrontSprite, weaponDetails.weaponName, 
            itemSpawnPoint.position);
    }

    public void PlayLock()
    {
        if (chestLockSoundRoutine == null)
        {
            chestLockSoundRoutine = StartCoroutine(PlayLockRoutine());
        }
    }

    /// <summary>
    /// Play lock routine
    /// </summary>
    IEnumerator PlayLockRoutine()
    {
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestLock);

        yield return new WaitForSeconds(2f);

        chestLockSoundRoutine = null;
    }

    /// <summary>
    /// Slow motion item drop from chest
    /// </summary>
    private IEnumerator MoveItemDown(Transform itemTransform, float distance)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = itemTransform.position;
        Vector3 targetPosition = initialPosition - new Vector3(0f, distance, 0f);

        while (elapsedTime < 2f)
        {
            elapsedTime += Time.deltaTime; // Increment time based on frame rate
            itemTransform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime);
            yield return null; // Wait for the next frame
        }

        // Ensure the item reaches the target position
        itemTransform.position = targetPosition;

        // Make sure drop completed
        dropCompleted = true;
    }

    /// <summary>
    /// Collect the weapon and add it to the players weapons list
    /// </summary>
    private void CollectWeaponItem()
    {
        // Check item exists and has been materialized
        if (chestItem == null) return;

        // Add weapon to player
        GameManager.Instance.GetPlayer().UpdateWieldedWeapons(weaponDetails, true, false);

        // Play pickup sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.weaponPickup);

        weaponDetails = null;
        Destroy(chestItemGameObject);
        UpdateChestState();
    }

    /// <summary>
    /// Display message above chest
    /// </summary>
    private IEnumerator DisplayMessage(string messageText, float messageDisplayTime)
    {
        messageTextTMP.text = messageText;

        yield return new WaitForSeconds(messageDisplayTime);

        messageTextTMP.text = "";
    }
}
