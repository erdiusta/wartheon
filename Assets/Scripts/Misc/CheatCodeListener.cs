using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class CheatCodeListener : SingletonMonobehaviour<CheatCodeListener>
{
    string cheatCode = "KUKULI";
    string inputBuffer = string.Empty;

    private void Update()
    {
        if (Keyboard.current == null) return;

        foreach (KeyControl key in Keyboard.current.allKeys)
        {
            if (key.wasPressedThisFrame)
            {
                string keyChar = key.displayName.ToUpper();

                // Only allow letters
                if (keyChar.Length == 1 && char.IsLetter(keyChar[0]))
                {
                    inputBuffer += keyChar;

                    // Trim buffer if too long
                    if (inputBuffer.Length > cheatCode.Length)
                        inputBuffer = inputBuffer.Substring(inputBuffer.Length - cheatCode.Length);

                    if (inputBuffer == cheatCode)
                    {
                        StaticEventHandler.CallCheatActivatedEvent();
                        GameManager.isDemo = false;
                        Debug.Log("Cheat code activated! Full game unlocked.");
                    }
                }
            }
        }
    }
}
