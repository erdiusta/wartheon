using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;

public class CheatCodeListener : SingletonMonobehaviour<CheatCodeListener>
{
    string kukuliCheatCode = "KUKULI";
    string ustasoftCheatCode = "USTASOFT";
    string inputBuffer = string.Empty;

    string[] cheats;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        cheats = new string[] { kukuliCheatCode, ustasoftCheatCode };
    }

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

                    // Trim buffer to the maximum cheat length
                    int maxCheatLength = cheats.Max(c => c.Length);
                    if (inputBuffer.Length > maxCheatLength)
                        inputBuffer = inputBuffer.Substring(inputBuffer.Length - maxCheatLength);

                    for (int i = 0; i < cheats.Length; i++)
                    {
                        if (inputBuffer.EndsWith(cheats[i]))
                        {
                            if (cheats[i] == kukuliCheatCode)
                            {
                                if (SceneManager.GetActiveScene().name == "MainMenuScene")
                                {
                                    StaticEventHandler.CallCheatActivatedEvent();
                                    GameManager.isDemo = false;
                                }
                                else
                                {
                                    Debug.Log("SORRY MAN TOO LATE");
                                }
                            }
                            else if (cheats[i] == ustasoftCheatCode)
                            {
                                StaticEventHandler.CallCheatActivatedEvent();
                            }
                        }
                    }
                }
            }
        }
    }
}
