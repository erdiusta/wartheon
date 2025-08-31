using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CoinsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinDisplayText;

    private void OnEnable()
    {
        // BOOK COIN AMOUNT
        StaticEventHandler.OnCoinAmountChanged += StaticEventHandler_OnCoinAmountChanged;
    }

    private void OnDisable()
    {
        // BOOK COIN AMOUNT
        StaticEventHandler.OnCoinAmountChanged -= StaticEventHandler_OnCoinAmountChanged;
    }

    private void StaticEventHandler_OnCoinAmountChanged(CoinAndShardArgs coinAndShardArgs)
    {
        coinDisplayText.text = coinAndShardArgs.updatedCoinAmount.ToString();
    }
}
