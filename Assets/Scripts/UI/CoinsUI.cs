using TMPro;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class CoinsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinDisplayText;

    Player player;

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        player.consumableEvent.OnCoinCountChanged -= ConsumableEvent_OnCoinCountChanged;
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetPlayer();
            yield return null;
        }

        coinDisplayText.text = "x" + player.coinsAndShards.coinAmount;

        player.consumableEvent.OnCoinCountChanged += ConsumableEvent_OnCoinCountChanged;
    }

    private void ConsumableEvent_OnCoinCountChanged(ConsumableEvent arg1, ConsumableEventArgs arg)
    {
        coinDisplayText.text = "x" + arg.coinAmount.ToString();
    }
}
