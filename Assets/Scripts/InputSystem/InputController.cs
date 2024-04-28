using System;
using Photon.Pun;
using UnityEngine;

namespace InputSystem
{
    public class InputController : MonoBehaviour
    {
        private InputInstructions _inputInstructions;
        private PhotonView _photonView;
        public Vector2 Move { get; private set; }
        public Vector2 Look { get; private set; }
        public bool Jump { get; private set; }
        public bool Sprint { get; private set; }
        
        [Header("Movement Settings")] 
        public bool analogMovement;

        [Header("Mouse Cursor Settings")] 
        private bool _cursorLocked = true;
        private  bool _cursorInputForLook = true;

        public event Action OnOpenHideTauntMenu;
        public event Action OnInteract;
        public event Action OnMouseClick;


        private void Awake()
        {
            _photonView = GetComponent<PhotonView>();
            if(!_photonView.IsMine)
                return;
                
            _inputInstructions = new InputInstructions();
            InputCheck();
        }

        private void InputCheck()
        {
            _inputInstructions.Enable();
            _inputInstructions.Player.Jump.performed += ctx => Jump = ctx.ReadValueAsButton();
            _inputInstructions.Player.Jump.canceled += ctx => Jump = ctx.ReadValueAsButton();
            _inputInstructions.Player.Sprint.performed += ctx => Sprint = ctx.ReadValueAsButton();
            _inputInstructions.Player.Sprint.canceled += ctx => Sprint = ctx.ReadValueAsButton();
            _inputInstructions.Player.Move.performed += ctx => Move = ctx.ReadValue<Vector2>();
            _inputInstructions.Player.Move.canceled += ctx => Move = Vector2.zero;
            _inputInstructions.Player.Look.performed += ctx => OnLook(ctx.ReadValue<Vector2>());
            _inputInstructions.Player.Look.canceled += ctx => Look = Vector2.zero;
            _inputInstructions.Player.OpenTauntMenu.performed += ctx => OnOpenHideTauntMenu?.Invoke();
            _inputInstructions.Player.Interact.performed += ctx => OnInteract?.Invoke();
            _inputInstructions.Player.MouseClick.performed += ctx => OnMouseClick?.Invoke();
        }


        private void OnLook(Vector2 value)
        {
            if (_cursorInputForLook)
            {
                Look = value;
            }
        }


        private void OnApplicationFocus(bool hasFocus)
        {
            SetCursorState(_cursorLocked);
        }

        public void SetCursorState(bool newState)
        {
            _cursorLocked = newState;
            Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
        }


        private void OnDisable()
        {
            _inputInstructions.Disable();
        }
    }
}