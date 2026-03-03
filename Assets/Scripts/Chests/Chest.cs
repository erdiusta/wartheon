using System.Collections;
using UnityEngine;
using TMPro;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Chest : MonoBehaviour, IUsable
{
    [HideInInspector] public bool dropCompleted = false;
    [HideInInspector] public ChestState chestState = ChestState.closed;
    [HideInInspector] public Coroutine chestLockSoundRoutine;

    #region Tooltip
    [Tooltip("Populate withItemSpawnPoint transform")]
    #endregion Tooltip
    [SerializeField] private Transform itemSpawnPoint;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    Animator animator;
    bool isEnabled = false;

    GameObject chestItemGameObject;
    DropItem chestItem;

    private void Awake()
    {
        animator = GetComponent<Animator>();
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
    /// Initialize overload for passive item
    /// </summary>
    public void Initialize(PassiveItemDetailsSO passiveItemDetails)
    {
        this.passiveItemDetails = passiveItemDetails;
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
                if (GameManager.Instance.GetLocalPlayer().keyCount > 0)
                {
                    OpenChest();
                    StartCoroutine(MoveItemDown(chestItem.transform, 1.5f));
                }
                else
                {
                    if (chestLockSoundRoutine == null)
                    {
                        chestLockSoundRoutine = StartCoroutine(PlayLockRoutine());
                    }
                }
                break;

            case ChestState.weaponItem:
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
        Player player = GameManager.Instance.GetLocalPlayer();
        player.consumableEvent.CallKeyCountChangedEvent(--player.keyCount);

        animator.SetBool(Settings.use, true);

        // chest open sound effect
        SoundEffectManager.Instance.PlaySoundEffect(GameResources.Instance.chestOpen);

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
        else if (passiveItemDetails != null)
        {
            chestState = ChestState.weaponItem;
            InstantiatePassiveItem();
        }
        else
        {
            chestState = ChestState.empty;
        }
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem()
    {
        InstantiateItem();
        chestItem.hasWeaponDrop = true;
        chestItem.hasSecondaryPassiveDrop = false;

        // Create a weapon instance with rolled modifiers
        Weapon weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails);

        chestItem.Initialize(weapon, weaponDetails.weaponFrontSprite, itemSpawnPoint.position);
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem()
    {
        InstantiateItem();
        chestItem.hasWeaponDrop = false;
        chestItem.hasSecondaryPassiveDrop = true;

        // Create a passive item instance with rolled modifiers
        PassiveItem passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails);

        chestItem.Initialize(passiveItem, passiveItem.passiveItemDetails.passiveItemSprite, itemSpawnPoint.position);
    }

    /// <summary>
    /// Instantiate a chest item
    /// </summary>
    private void InstantiateItem()
    {
        chestItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
        chestItem = chestItemGameObject.GetComponent<DropItem>();
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
            if (itemTransform == null) break;

            elapsedTime += Time.deltaTime; // Increment time based on frame rate
            itemTransform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime);
            yield return null; // Wait for the next frame
        }

        // Ensure the item reaches the target position
        itemTransform.position = targetPosition;

        // Make sure drop completed
        dropCompleted = true;
    }
}
