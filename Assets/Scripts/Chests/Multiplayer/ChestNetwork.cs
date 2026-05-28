using Mirror;
using UnityEngine;

public class ChestNetwork : NetworkBehaviour
{
    [SyncVar] public int seed;
    [SyncVar] public int chestIndex;
    [SyncVar] public uint openerNetId;
    [SyncVar] public WeaponTitle weaponTitle;
    [SyncVar] public PassiveItemType passiveItemType;
    [SyncVar(hook = nameof(OnChestStateChanged))] 
    public ChestState chestState;

    Animator animator;

    private void Awake()
    {
        animator = GetComponent<Animator>();
    }

    public void InitializeServer(int seed, int chestIndex, WeaponTitle weaponTitle, PassiveItemType passiveItemType)
    {
        this.seed = seed;
        this.chestIndex = chestIndex;
        this.weaponTitle = weaponTitle;
        this.passiveItemType = passiveItemType;
    }

    private void OnChestStateChanged(ChestState oldState, ChestState newState)
    {
        if (newState != ChestState.weaponItem) return;

        Player player = GameManager.Instance.GetLocalPlayer();

        animator.SetBool(Settings.use, true);

        if (player == null || player.NetAuth.netId != openerNetId) return;

        player.consumableEvent.CallKeyCountChangedEvent(--player.keyCount);
    }

    public override void OnStartClient()
    {
        base.OnStartClient();

        InitializeClient();
    }

    private void InitializeClient()
    {
        WartheonRNG rng = new WartheonRNG(seed);

        Chest chest = GetComponent<Chest>();

        // Resolve via database
        if(weaponTitle != WeaponTitle.None)
        {
            var weapon = WartheonDatabase.Instance.GetWeaponDetails(weaponTitle);
            chest.Initialize(weapon, rng);
        }

        if (passiveItemType != PassiveItemType.None)
        {
            var passive = WartheonDatabase.Instance.GetPassiveItemDetails(passiveItemType);
            chest.Initialize(passive, rng);
        }
    }

    [Command]
    public void CmdSetChestState(ChestState chestState)
    {
        this.chestState = chestState;
    }
}
