using UnityEngine;

public class ScreenCursor : MonoBehaviour
{
    public Texture2D cursorTexture; // Your custom cursor texture
    public Vector2 cursorHotspot; // The hotspot of the cursor

    void Start()
    {
        // Set the custom cursor with the specified hotspot
        Cursor.SetCursor(cursorTexture, cursorHotspot, CursorMode.Auto);
    }
}