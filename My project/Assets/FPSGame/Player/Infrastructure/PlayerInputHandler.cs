using UnityEngine;

namespace FPSGame.Player
{
    public class PlayerInputHandler : IPlayerInput
    {
        private Vector2 _moveInput;
        private Vector2 _lookInput;
        private bool _jumpPressed;
        private bool _runHeld;
        private bool _crouchToggled;
        private bool _jumpConsumed;

        public Vector2 MoveInput => _moveInput;
        public Vector2 LookInput => _lookInput;
        public bool JumpPressed => _jumpPressed && !_jumpConsumed;
        public bool RunHeld => _runHeld;
        public bool CrouchToggled => _crouchToggled;

        public void SetMoveInput(Vector2 input) => _moveInput = input;
        public void SetLookInput(Vector2 input) => _lookInput = input;
        public void SetJumpPressed(bool pressed) { _jumpPressed = pressed; if (!pressed) _jumpConsumed = false; }
        public void SetRunHeld(bool held) => _runHeld = held;
        public void ToggleCrouch() => _crouchToggled = !_crouchToggled;

        public void UpdateInput()
        {
            _jumpConsumed = false;
        }

        public void ConsumeJump()
        {
            _jumpConsumed = true;
        }
    }
}
