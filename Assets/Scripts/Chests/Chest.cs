using System.Collections;
using UnityEngine;
using TMPro;
using Mirror;

[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(SpriteRenderer))]
public class Chest : MonoBehaviour, IUsable
{
    [HideInInspector] public ChestState chestState = ChestState.closed;
    [HideInInspector] public Coroutine chestLockSoundRoutine;
    [HideInInspector] public ChestNetwork chestNetwork;

    #region Tooltip
    [Tooltip("Populate withItemSpawnPoint transform")]
    #endregion Tooltip
    [SerializeField] private Transform itemSpawnPoint;
    WeaponDetailsSO weaponDetails;
    PassiveItemDetailsSO passiveItemDetails;
    Animator animator;
    bool isEnabled = false;

    GameObject dropItemGameObject;
    DropItem dropItem;
    DropItemNetwork dropItemNetwork;

    WartheonRNG rng;

    private void Awake()
    {
        animator = GetComponent<Animator>();
        chestNetwork = GetComponent<ChestNetwork>();
    }

    /// <summary>
    /// Initialize Chest and either make it visible immediately or materialize it
    /// </summary>
    public void Initialize(WeaponDetailsSO weaponDetails, WartheonRNG rng)
    {
        this.weaponDetails = weaponDetails;
        this.rng = rng;
        EnableChest();
    }

    /// <summary>
    /// Initialize overload for passive item
    /// </summary>
    public void Initialize(PassiveItemDetailsSO passiveItemDetails, WartheonRNG rng)
    {
        this.passiveItemDetails = passiveItemDetails;
        this.rng = rng;
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
                    OpenChest(5000, isMultiplayer: false);

                    StartCoroutine(MoveItemDown(dropItem.transform, 1.5f));
                }
                else
                {
                    if (chestLockSoundRoutine == null)
                    {
                        chestLockSoundRoutine = StartCoroutine(PlayLockRoutine(isMultiplayer: false));
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

    [Server]
    public void Server_StartChestProcess(uint playerNetId)
    {
        if (!isEnabled) return;

        switch (chestNetwork.chestState)
        {
            case ChestState.closed:
                if (GameManager.Instance.GetLocalPlayer().keyCount > 0)
                {
                    OpenChest(playerNetId, isMultiplayer: true);

                    // Spawn network item here
                    StartCoroutine(MoveItemDown(dropItemNetwork.transform, 1.5f, isMultiplayer: true));
                }
                else
                {
                    if (chestLockSoundRoutine == null)
                    {
                        chestLockSoundRoutine = StartCoroutine(PlayLockRoutine(isMultiplayer: true));
                    }
                }
                break;
            case ChestState.weaponItem:
                break;
            case ChestState.empty:
                break;
            default:
                break;
        }
    }

    /// <summary>
    /// Open the chest on first use
    /// </summary>
    private void OpenChest(uint playerNetId, bool isMultiplayer)
    {
        if (!isMultiplayer)
        {
            Player player = GameManager.Instance.GetLocalPlayer();
            player.consumableEvent.CallKeyCountChangedEvent(--player.keyCount);

            animator.SetBool(Settings.use, true);

            // chest open sound effect
            if (isMultiplayer) NetworkSoundManager.Instance.ServerPlaySound(SoundName.OpenChest, transform.position);
            else WorldSoundManager.Instance.PlayWorldSound(GameResources.Instance.chestOpen, transform.position);
        }

        UpdateChestState(playerNetId, isMultiplayer);       
    }

    /// <summary>
    /// Create items based on what should be spawned and the chest state
    /// </summary>
    private void UpdateChestState(uint playerNetId, bool isMultiplayer)
    {
        if (weaponDetails != null)
        {
            if (isMultiplayer)
            {
                NetworkSoundManager.Instance.ServerPlaySound(SoundName.OpenChest, transform.position);

                chestNetwork.openerNetId = playerNetId;
                chestNetwork.chestState = ChestState.weaponItem;
            }
            else chestState = ChestState.weaponItem;

            InstantiateWeaponItem(isMultiplayer);
        }
        else if (passiveItemDetails != null)
        {
            if (isMultiplayer)
            {
                NetworkSoundManager.Instance.ServerPlaySound(SoundName.OpenChest, transform.position);

                chestNetwork.openerNetId = playerNetId;
                chestNetwork.chestState = ChestState.weaponItem;
            }
            else chestState = ChestState.weaponItem;

            InstantiatePassiveItem(isMultiplayer);
        }
        else
        {
            if (isMultiplayer) chestNetwork.chestState = ChestState.empty;
            else chestState = ChestState.empty;
        }
    }

    /// <summary>
    /// Instantiate a weapon item for the player to collect
    /// </summary>
    private void InstantiateWeaponItem(bool isMultiplayer)
    {
        InstantiateDropItem(isMultiplayer);

        if (!isMultiplayer)
        {
            dropItem.hasWeaponDrop = true;
            dropItem.hasSecondaryPassiveDrop = false;

            // Create a weapon instance with rolled modifiers
            Weapon weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails, rng);

            dropItem.Initialize(weapon, weaponDetails.weaponFrontSprite, itemSpawnPoint.position, null);
        }
        else
        {
            Weapon weapon = new Weapon(Rarity.Basic);

            dropItemNetwork.currentLocation = DropItemLocation.World;
            dropItemNetwork.hasWeaponDrop = true;
            dropItemNetwork.dropSourceType = DropSourceType.Enemy;

            weapon = WeaponDropGenerator.CreateRolledInstance(weaponDetails, rng);

            NetworkTransformUnreliable nt = dropItemNetwork.GetComponent<NetworkTransformUnreliable>();
            nt.ServerTeleport(transform.position, Quaternion.identity);

            dropItemNetwork.weaponTitle = weapon.weaponStats.weaponTitle;
            dropItemNetwork.weaponClass = weapon.weaponStats.weaponClass;
            dropItemNetwork.weaponStats = weapon.weaponStats;
        }
    }

    /// <summary>
    /// Instantiate a passive item for the player to collect
    /// </summary>
    private void InstantiatePassiveItem(bool isMultiplayer)
    {
        InstantiateDropItem(isMultiplayer);

        if (!isMultiplayer)
        {
            dropItem.hasWeaponDrop = false;
            dropItem.hasSecondaryPassiveDrop = true;

            // Create a passive item instance with rolled modifiers
            PassiveItem passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng);
            passiveItem.passiveStats.passiveItemType = passiveItemDetails.passiveItemType;

            dropItem.Initialize(passiveItem, passiveItemDetails.passiveItemSprite, itemSpawnPoint.position, null);
        }
        else
        {
            PassiveItem passiveItem = new PassiveItem(Rarity.Basic);

            if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Primary)
            {
                dropItemNetwork.currentLocation = DropItemLocation.World;
                dropItemNetwork.hasPrimaryPassiveDrop = true;
                dropItemNetwork.dropSourceType = DropSourceType.Enemy;
                passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng, false, Rarity.Basic, isPrimaryPassive: true);
            }
            else if (passiveItemDetails.passiveItemCategory == PassiveItemCategory.Secondary)
            {
                dropItemNetwork.currentLocation = DropItemLocation.World;
                dropItemNetwork.hasSecondaryPassiveDrop = true;
                dropItemNetwork.dropSourceType = DropSourceType.Enemy;
                passiveItem = PassiveDropGenerator.CreateRolledInstance(passiveItemDetails, rng);
            }

            NetworkTransformUnreliable nt = dropItemNetwork.GetComponent<NetworkTransformUnreliable>();
            nt.ServerTeleport(transform.position, Quaternion.identity);

            dropItemNetwork.passiveItemType = passiveItem.passiveStats.passiveItemType;
            dropItemNetwork.passiveItemSlotName = passiveItem.passiveStats.passiveItemSlotName;
            dropItemNetwork.passiveStats = passiveItem.passiveStats; 
        }
    }

    /// <summary>
    /// Instantiate a chest item
    /// </summary>
    private void InstantiateDropItem(bool isMultiplayer)
    {
        if (!isMultiplayer)
        {
            dropItemGameObject = Instantiate(GameResources.Instance.chestItemPrefab, transform);
            dropItem = dropItemGameObject.GetComponent<DropItem>();
        }
        else
        {
            if (NetworkServer.active)
            {
                dropItemGameObject = Instantiate(GameResources.Instance.chestItemNetworkPrefab);
                NetworkServer.Spawn(dropItemGameObject);

                dropItemNetwork = dropItemGameObject.GetComponent<DropItemNetwork>();
                dropItemNetwork.dropSourceType = DropSourceType.Enemy;

                // Set collider to true
                dropItemGameObject.GetComponent<BoxCollider2D>().enabled = true;
            }
        }
    }

    /// <summary>
    /// Play lock routine
    /// </summary>
    IEnumerator PlayLockRoutine(bool isMultiplayer)
    {
        if (isMultiplayer) NetworkSoundManager.Instance.ServerPlaySound(SoundName.ChestLocked, transform.position);
        else WorldSoundManager.Instance.PlayWorldSound(GameResources.Instance.chestOpen, transform.position);

        yield return new WaitForSeconds(2f);

        chestLockSoundRoutine = null;
    }

    /// <summary>
    /// Slow motion item drop from chest
    /// </summary>
    private IEnumerator MoveItemDown(Transform itemTransform, float distance, bool isMultiplayer = false)
    {
        float elapsedTime = 0f;
        Vector3 initialPosition = itemTransform.position;
        Vector3 targetPosition = initialPosition - new Vector3(0f, distance, 0f);

        DropItemNetwork dropItemNetwork = itemTransform.GetComponent<DropItemNetwork>();
        DropItem dropItem = itemTransform.GetComponent<DropItem>();

        yield return null;

        if (isMultiplayer) dropItemNetwork.canInitialize = true;

        while (elapsedTime < 2f)
        {
            if (itemTransform == null || itemTransform.Equals(null)) break;

            elapsedTime += Time.deltaTime; // Increment time based on frame rate
            itemTransform.position = Vector3.Lerp(initialPosition, targetPosition, elapsedTime);
            yield return null; // Wait for the next frame
        }

        // Ensure the item reaches the target position
        if(itemTransform != null && !itemTransform.Equals(null)) itemTransform.position = targetPosition;



        // Make sure drop completed
        if (isMultiplayer) dropItemNetwork.dropCompleted = true;
        else dropItem.dropCompleted = true;
    }
}
