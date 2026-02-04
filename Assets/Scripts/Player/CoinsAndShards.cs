using UnityEngine;

public class CoinsAndShards : MonoBehaviour
{
    public int coinAmount = 0;
    public int shardAmount = 0;

    ConsumableEvent consumableEvent;
    Player player;

    private void Awake()
    {
        consumableEvent = GetComponent<ConsumableEvent>();
        player = GetComponent<Player>(); // cache early
    }

    public void AddCoin(int count)
    {
        coinAmount = Mathf.Clamp(coinAmount + count, 0, coinAmount + count);
        player.consumableEvent.CallCoinCountChangedEvent(coinAmount);
    }

    public void AddShard(int count)
    {
        shardAmount = Mathf.Clamp(shardAmount + count, 0, shardAmount + count);
        player.consumableEvent.CallSharCountChangedEvent(shardAmount);
    }

    public void RemoveShard(int count)
    {
        shardAmount -= count;
        player.consumableEvent.CallSharCountChangedEvent(shardAmount);
    }

    public int GetCurrentCoin()
    {
        return coinAmount;
    }

    public int GetCurrentShard()
    {
        return shardAmount;
    }
}
