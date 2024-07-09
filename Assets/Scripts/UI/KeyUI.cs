using TMPro;
using UnityEngine;

[DisallowMultipleComponent]
public class KeyUI : MonoBehaviour
{
    [SerializeField] TextMeshProUGUI keyCountDisplayText;

    private void Update()
    {
        keyCountDisplayText.text = GameManager.Instance.GetPlayer().keyCount.ToString();
    }
}
