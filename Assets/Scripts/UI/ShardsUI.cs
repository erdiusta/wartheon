using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ShardsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI shardsDisplayText;

    private void OnEnable()
    {
        // BOOK SHARD AMOUNT
        StaticEventHandler.OnShardAmountChanged += StaticEventHandler_OnShardAmountChanged;
    }

    private void OnDisable()
    {
        // BOOK SHARD AMOUNT
        StaticEventHandler.OnShardAmountChanged -= StaticEventHandler_OnShardAmountChanged;
    }

    private void StaticEventHandler_OnShardAmountChanged(CoinAndShardArgs coinAndShardArgs)
    {
        shardsDisplayText.text = coinAndShardArgs.updatedShardAmount.ToString();
    }
}
