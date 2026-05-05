using TMPro;
using UnityEngine;
using System.Collections;

[DisallowMultipleComponent]
public class KeyUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI keyCountDisplayText;

    Player player;

    private void OnEnable()
    {
        StartCoroutine(WaitForPlayerInitialization());
    }

    private void OnDisable()
    {
        player.consumableEvent.OnKeyCountChanged -= ConsumableEvent_OnKeyCountChanged;
    }

    IEnumerator WaitForPlayerInitialization()
    {
        while (player == null || !player.IsLocal)
        {
            player = GameManager.Instance.GetLocalPlayer();
            yield return null;
        }

        keyCountDisplayText.text = "x" + player.keyCount;
        player.consumableEvent.OnKeyCountChanged += ConsumableEvent_OnKeyCountChanged;
    }

    private void ConsumableEvent_OnKeyCountChanged(ConsumableEvent arg1, ConsumableEventArgs arg)
    {
        keyCountDisplayText.text = "x" + arg.keyAmount.ToString();
    }
}
