using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Mirror.Discovery;

public class SessionEntryUI : MonoBehaviour
{
    [SerializeField] Button button;
    [SerializeField] TextMeshProUGUI titleText;
    [SerializeField] TextMeshProUGUI infoText;
    [SerializeField] SoundEffectSO clickSound;

    public bool isJoinable { get; private set; }
    public ServerResponse BoundResponse { get; private set; }
    public bool HasResponse { get; private set; }

    public void Initialize(string title, string info, bool joinable)
    {
        isJoinable = joinable;
        titleText.text = title;
        infoText.text = info;
        button.interactable = joinable;

        UpdateVisualState();
    }

    public void BindResponse(ServerResponse response)
    {
        BoundResponse = response;
        HasResponse = true;
    }

    public void OnClick()
    {
        // Always allow selection
        MultiplayerEntryUI.Instance.OnSessionSelected(this);

        // Feedback only if blocked
        if (!isJoinable)
        {
            // Optional feedback (sound, shake, tooltip later)
            SoundEffectManager.Instance.PlaySoundEffect(clickSound);
        }
    }

    public void SetSelected(bool selected)
    {
        // highlight background / outline
    }

    private void UpdateVisualState()
    {
        // Optional: fade if not joinable
        var img = button.targetGraphic;
        if (img != null)
            img.color = isJoinable ? Color.white : new Color(1f, 1f, 1f, 0.4f);
    }
}
