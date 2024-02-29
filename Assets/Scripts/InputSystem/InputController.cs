using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace InputSystem
{
    public class InputController : MonoBehaviour
    {
        private InputInstructions _inputInstructions;
        
        [Header("Character Input Values")] public Vector2 move;
        public Vector2 look;
        public bool jump;
        public bool sprint;
        public bool openTauntMenu;

        [Header("Movement Settings")] public bool analogMovement;

        [Header("Mouse Cursor Settings")] public bool cursorLocked = true;
        public bool cursorInputForLook = true;
        
        private void Awake()
        {
            _inputInstructions = new InputInstructions();
            InputCheck();
        }

        


#if ENABLE_INPUT_SYSTEM
        
        private void InputCheck()
        {
            _inputInstructions.Enable();
            _inputInstructions.Player.Jump.performed += ctx => jump = ctx.ReadValueAsButton();
            _inputInstructions.Player.Jump.canceled += ctx => jump = ctx.ReadValueAsButton();
            _inputInstructions.Player.Sprint.performed += ctx => sprint = ctx.ReadValueAsButton();
            _inputInstructions.Player.Sprint.canceled += ctx => sprint = ctx.ReadValueAsButton();
            _inputInstructions.Player.OpenTauntMenu.performed += ctx => openTauntMenu = !openTauntMenu;
            _inputInstructions.Player.Move.performed += ctx => move = ctx.ReadValue<Vector2>();
            _inputInstructions.Player.Move.canceled += ctx => move = Vector2.zero;
            _inputInstructions.Player.Look.performed += ctx => OnLook(ctx.ReadValue<Vector2>());
            _inputInstructions.Player.Look.canceled += ctx => look = Vector2.zero;
        }
        
        public void OnLook(Vector2 value)
        {
            if (cursorInputForLook)
            {
                look = value;
            }
        }
        

#endif

        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(cursorLocked);
        }

        public void SetCursorState(bool newState)
        {
            cursorLocked = newState;
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }
        
    }
}