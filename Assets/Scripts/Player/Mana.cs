using UnityEngine;

[RequireComponent(typeof(ManaEvent))]
[DisallowMultipleComponent]
public class Mana : MonoBehaviour
{
    [HideInInspector] public int currentMana;
    [HideInInspector] public int reservedMana;
    [HideInInspector] public int maximumMana;

    ManaEvent manaEvent;
    Player player;

    private void Awake()
    {
        manaEvent = GetComponent<ManaEvent>();
    }

    private void Start()
    {
        // Trigger a mana event for UI update
        if (player != null && !player.isInitialized) return;

        if (player == null)
        {
            manaEvent.CallManaChangedEvent(currentMana);
        }

        // Attempt to load components
        player = GetComponent<Player>();
    }

    private void Update()
    {
        if (player != null) if (!player.isInitialized) return;
    }

    /// <summary>
    /// Set mana - Player
    /// </summary>
    public void SetMaximumMana(int maximumMana, bool shouldManaFilled = true, bool onStart = false, bool manaReserved = false)
    {
        this.maximumMana = maximumMana;

        // If current health maximized together with increasing max health or not
        currentMana = onStart ? maximumMana : shouldManaFilled ? maximumMana : currentMana;

        // Trigger mana event
        if(player != null) manaEvent.CallManaChangedEvent(currentMana, manaReserved);
    }

    /// <summary>
    /// Get the maximum mana
    /// </summary>
    public int GetMaximumMana() => maximumMana;

    /// <summary>
    /// Get the reserved mana
    /// </summary>
    public int GetReservedMana() => reservedMana;

    /// <summary>
    /// Get current mana
    /// </summary>
    public int GetCurrentMana() => currentMana;

    /// <summary>
    /// Increase mana by specified amount
    /// </summary>
    public void AddMana(int manaIncrease)
    {
        currentMana = Mathf.Clamp(currentMana + manaIncrease, 0, maximumMana);
            
        // Trigger mana event
        manaEvent.CallManaChangedEvent(currentMana);

        // Trigger mana event
        manaEvent.CallManaChangedEvent(currentMana);
        StaticEventHandler.CallBookManaChangedEvent(currentMana);
    }

    /// <summary>
    /// Reduce mana by specified amount
    /// </summary>
    public void ConsumeMana(int manaDecrease, bool manaReserved = false)
    {
        if (manaReserved) reservedMana += manaDecrease;

        currentMana = Mathf.Clamp(currentMana - manaDecrease, 0, maximumMana);

        if (player != null && player.isSurgeTapGainActive && currentMana < 10) currentMana += 10; // Surge Tap Gain - Instant 10 mana if mana is below 10

        // Trigger mana event
        manaEvent.CallManaChangedEvent(currentMana, manaReserved);
        StaticEventHandler.CallBookManaChangedEvent(currentMana);
    }

    // Change reserved mana amount
    public void ResetReservedMana(int manaAmount)
    {
        currentMana = Mathf.Clamp(currentMana + manaAmount, 0, maximumMana); // Refill removed mana amount from reserve
        reservedMana = Mathf.Clamp(reservedMana - manaAmount, 0, reservedMana);

        // Trigger mana reserve reset event
        manaEvent.CallReservedManaResetEvent(manaAmount);
        StaticEventHandler.CallBookManaChangedEvent(currentMana);
    }
}
