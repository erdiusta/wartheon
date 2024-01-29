using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
    Vector2 cursorPosition;

    private void Awake()
    {
        // Set hardware cursor off
        Cursor.visible = false;
    }

    private void Update()
    {
        cursorPosition = GameManager.Instance.pointerPosition.action.ReadValue<Vector2>();
        transform.position = new Vector3(cursorPosition.x, cursorPosition.y, 0f);
    }
}