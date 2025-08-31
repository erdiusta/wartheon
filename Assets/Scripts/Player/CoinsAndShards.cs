using UnityEngine;

public class CoinsAndShards : MonoBehaviour
{
    public int coinAmount = 0;
    public int shardAmount = 0;

    private void Start()
    {
        shardAmount = 5000;
    }

    public void AddCoin(int count)
    {
        coinAmount += count;

        StaticEventHandler.CallCoinAmountChanged(coinAmount);
    }

    public void AddShard(int count)
    {
        shardAmount += count;

        StaticEventHandler.CallShardAmountChanged(shardAmount);
    }

    public void RemoveShard(int count)
    {
        shardAmount -= count;

        StaticEventHandler.CallShardAmountChanged(shardAmount);
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
