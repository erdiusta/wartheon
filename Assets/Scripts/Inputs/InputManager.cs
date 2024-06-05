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
    public InputActionReference reload;
    public InputActionReference resetWeaponIndex;
    public InputActionReference nextLevel;
    public InputActionReference interaction;
    public InputActionReference specialMove;
    public InputActionReference bookView;
    public InputActionReference activeItem;
    public InputActionReference dropActiveItem;
    public InputActionReference pause;

    [HideInInspector] public bool isPressedPreviousFrame;
}
