using UnityEngine;

public class CoinsAndShards : MonoBehaviour
{
    public int coinAmount;
    public int shardAmount;

    private void Start()
    {
        coinAmount = 0;
        shardAmount = 0;

        StaticEventHandler.CallCoinAmountChanged(coinAmount);
        StaticEventHandler.CallShardAmountChanged(shardAmount);
    }

    public void AddCoin(int count)
    {
        coinAmount = Mathf.Clamp(coinAmount + count, 0, coinAmount + count);
        StaticEventHandler.CallCoinAmountChanged(coinAmount);
    }

    public void AddShard(int count)
    {
        shardAmount = Mathf.Clamp(shardAmount + count, 0, shardAmount + count);
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
