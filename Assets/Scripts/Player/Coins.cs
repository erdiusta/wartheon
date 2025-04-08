using UnityEngine;

public class Coins : MonoBehaviour
{
    public int coinAmount = 0;

    public void Add(int count)
    {
        coinAmount += count;
    }

    public int GetCurrentCoin()
    {
        return coinAmount;
    }
}
