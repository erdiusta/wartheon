using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class CheatCodeListener : SingletonMonobehaviour<CheatCodeListener>
{
    string kukuliCheatCode = "KUKULI";
    string multiCheatCode = "IHUHUHU";
    string ustasoftCheatCode2 = "USTASOFT2";
    string ustasoftCheatCode3 = "USTASOFT3";
    string ustasoftCheatCode4 = "USTASOFT4";
    string ustasoftCheatCode5 = "USTASOFT5";
    string ustasoftCheatCode6 = "USTASOFT6";
    string ustasoftCheatCode7 = "USTASOFT7";
    string ustasoftCheatCode8 = "USTASOFT8";
    string inputBuffer = string.Empty;

    string[] cheats;

    public Button multiButton;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Start()
    {
        cheats = new string[] { kukuliCheatCode, multiCheatCode, ustasoftCheatCode2, ustasoftCheatCode3, ustasoftCheatCode4, ustasoftCheatCode5,
            ustasoftCheatCode6, ustasoftCheatCode7, ustasoftCheatCode8};
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        foreach (KeyControl key in Keyboard.current.allKeys)
        {
            if (key == null) continue;

            if (key.wasPressedThisFrame)
            {
                string keyChar = key.displayName.ToUpper();

                // Only allow letters
                if (keyChar.Length == 1 && char.IsLetterOrDigit(keyChar[0]))
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
                            else if (cheats[i] == multiCheatCode)
                            {
                                multiButton.enabled = true;
                                multiButton.GetComponent<Image>().color = new Color(1f, 1f, 1f);
                            }
                            else if (cheats[i] == ustasoftCheatCode2)
                            {
                                InputManager.cachedLevelIndex = 2;
                            }
                            else if (cheats[i] == ustasoftCheatCode3)
                            {
                                InputManager.cachedLevelIndex = 3;
                            }
                            else if (cheats[i] == ustasoftCheatCode4)
                            {
                                InputManager.cachedLevelIndex = 4;
                            }
                            else if (cheats[i] == ustasoftCheatCode5)
                            {
                                InputManager.cachedLevelIndex = 5;
                            }
                            else if (cheats[i] == ustasoftCheatCode6)
                            {
                                InputManager.cachedLevelIndex = 6;
                            }
                            else if (cheats[i] == ustasoftCheatCode7)
                            {
                                InputManager.cachedLevelIndex = 7;
                            }
                            else if (cheats[i] == ustasoftCheatCode8)
                            {
                                InputManager.cachedLevelIndex = 8;
                            }

                            StaticEventHandler.CallCheatActivatedEvent();
                        }
                    }
                }
            }
        }
    }
}
