using System.Collections;
using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class ShardsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI shardsDisplayText;

    Player player;

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        player.consumableEvent.OnShardCountChanged -= UpdateShardCount;
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        player.consumableEvent.OnShardCountChanged += UpdateShardCount;
    }

    private void UpdateShardCount(ConsumableEvent arg1, ConsumableEventArgs arg)
    {
        shardsDisplayText.text = "x" + arg.shardAmount.ToString();
    }
}
