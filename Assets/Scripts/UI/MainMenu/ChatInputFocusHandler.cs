using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

public class ChatInputFocusHandler : MonoBehaviour, ISelectHandler, IDeselectHandler
{
    public TMP_InputField chatInput;

    public void OnSelect(BaseEventData eventData)
    {
        // Input field was selected (mouse, keyboard, gamepad)
        chatInput.ActivateInputField();
        MoveCaretToEnd();
    }

    public void OnDeselect(BaseEventData eventData)
    {
        // Optional: you can react when focus is lost
        // Debug.Log("Chat input lost focus");
    }

    private void MoveCaretToEnd()
    {
        int len = chatInput.text.Length;
        chatInput.caretPosition = len;
        chatInput.stringPosition = len;
    }
}
