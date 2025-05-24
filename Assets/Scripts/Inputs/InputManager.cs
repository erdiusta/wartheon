using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.InputSystem.LowLevel;

public class InputManager : SingletonMonobehaviour<InputManager>
{
    #region INPUT ACTION REFERENCES
    static InputDevice currentDevice;

    Vector2 lastMousePosition;

    [Space(10)]
    [Header("INPUT ACTION REFERENCES")]
    #endregion
    [Header("Gameplay")]
    public InputActionReference pointerPosition;
    public InputActionReference gamepadAim;
    public InputActionReference movement;
    public InputActionReference attack;
    public InputActionReference switchWeaponByWheel;
    public InputActionReference switchWeaponByButton;
    public InputActionReference overviewMapFullView;
    public InputActionReference nextLevel;
    public InputActionReference interaction;
    public InputActionReference specialMoveOne;
    public InputActionReference specialMoveTwo;
    public InputActionReference specialMoveThree;
    public InputActionReference bookView;
    public InputActionReference activeItem;
    public InputActionReference pause;
    public InputActionReference jumpButton;
    public InputActionReference invisibleButton;
    public InputActionReference parryButton;
    public InputActionReference levelOneButton;
    public InputActionReference levelTwoButton;
    public InputActionReference levelThreeButton;
    public InputActionReference levelFourButton;
    public InputActionReference levelFiveButton;
    public InputActionReference levelSixButton;
    public InputActionReference levelSevenButton;
    public InputActionReference levelEightButton;

    [Header("UI")]
    public InputActionReference OKButton;
    public InputActionReference escapeButton;
    public InputActionReference uiInteraction;
    public InputActionReference scroll;
    public InputActionReference cancelButton;

    [HideInInspector] public bool isPressedPreviousFrame;

    protected override void Awake()
    {
        base.Awake();
    }

    private void Update()
    {
        if (Mouse.current != null)
        {
            Vector2 currentMousePosition = HelperUtilities.GetMouseWorldPosition();

            if ((currentMousePosition - lastMousePosition).sqrMagnitude > 0.05f)
            {
                // Update current device based on mouse movement
                currentDevice = Mouse.current;
            }

            lastMousePosition = currentMousePosition;
        }

        if (Keyboard.current != null)
        {
            foreach (var key in Keyboard.current.allKeys)
            {
                if (key.wasPressedThisFrame)
                {
                    currentDevice = Keyboard.current;
                    break;
                }
            }
        }

        // --- Detect Gamepad Button Press or Stick Move ---
        if (Gamepad.current != null)
        {
            bool gamepadUsed = false;

            foreach (var control in Gamepad.current.allControls)
            {
                if (control is ButtonControl button && button.wasPressedThisFrame)
                {
                    gamepadUsed = true;
                    break;
                }
            }

            // Include stick movement as well
            if (Gamepad.current.leftStick.ReadValue().sqrMagnitude > 0.01f ||
                Gamepad.current.rightStick.ReadValue().sqrMagnitude > 0.01f)
            {
                gamepadUsed = true;
            }

            if (gamepadUsed)
            {
                currentDevice = Gamepad.current;
            }
        }
    }

    public static bool IsGamepad() => currentDevice is Gamepad;
    public static bool IsKeyboardMouse() => currentDevice is Keyboard || currentDevice is Mouse;

}
