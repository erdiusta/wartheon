using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : SingletonMonobehaviour<InputManager>
{
    #region INPUT ACTION REFERENCES
    [Space(10)]
    [Header("INPUT ACTION REFERENCES")]
    #endregion
    public InputActionReference pointerPosition;
    public InputActionReference movement;
    public InputActionReference attack;
    public InputActionReference attackOffHand;
    public InputActionReference switchWeapon;
    public InputActionReference overviewMapFullView;
    public InputActionReference nextLevel;
    public InputActionReference interaction;
    public InputActionReference specialMoveOne;
    public InputActionReference specialMoveTwo;
    public InputActionReference specialMoveThree;
    public InputActionReference bookView;
    public InputActionReference activeItem;
    public InputActionReference dropActiveItem;
    public InputActionReference pause;
    public InputActionReference OKButton;
    public InputActionReference jumpButton;

    public InputActionReference levelOneButton;
    public InputActionReference levelTwoButton;
    public InputActionReference levelThreeButton;
    public InputActionReference levelFourButton;
    public InputActionReference levelFiveButton;
    public InputActionReference levelSixButton;
    public InputActionReference levelSevenButton;
    public InputActionReference levelEightButton;

    [HideInInspector] public bool isPressedPreviousFrame;
}
