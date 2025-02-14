using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class CoinsUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI coinDisplayText;

    private void Update()
    {
        coinDisplayText.text = "x" + GameManager.Instance.GetPlayer().coins.GetCurrentCoin().ToString();
    }
}
