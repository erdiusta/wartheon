using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

public class InputManager : SingletonMonobehaviour<InputManager>
{
    static InputDevice currentDevice;
    public InputActionAsset actions;
    Vector2 lastMousePosition;

    #region INPUT ACTION REFERENCES
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
    public InputActionReference uiNavigate;
    public InputActionReference scroll;
    public InputActionReference tooltip;

    [HideInInspector] public bool isPressedPreviousFrame;

    protected override void Awake()
    {
        base.Awake();

        SetUpInputActions();
    }

    private void OnEnable()
    {
        tooltip.action.performed += OnShowTooltipPerformed;
    }

    private void OnDisable()
    {
        tooltip.action.performed -= OnShowTooltipPerformed;
    }

    private void Update()
    {
        // Update the last hovered UI element (under pointer)
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

    private void SetUpInputActions()
    {

    }

    public void OnShowTooltipPerformed(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (EventSystem.current.currentSelectedGameObject == null) return;

        Slot currentSlot = EventSystem.current.currentSelectedGameObject.GetComponent<Slot>();

        if (currentSlot != null && currentSlot.equippedTransform.childCount > 0)
        {
            if (currentSlot.tooltipPanel != null)
            {
                bool isActive = currentSlot.tooltipPanel.gameObject.activeSelf;

                // Toggle tooltip
                currentSlot.tooltipPanel.gameObject.SetActive(!isActive);

                if (!isActive)
                {
                    currentSlot.UpdateTooltipPanelInfo();
                    Slot.currentOpenTooltip = currentSlot.tooltipPanel.gameObject;
                }
                else
                {
                    if (Slot.currentOpenTooltip == currentSlot.tooltipPanel)
                        Slot.currentOpenTooltip = null;
                }
            }
        }
    }

    public static bool IsGamepad() => currentDevice is Gamepad;
    public static bool IsKeyboardMouse() => currentDevice is Keyboard || currentDevice is Mouse;

}
