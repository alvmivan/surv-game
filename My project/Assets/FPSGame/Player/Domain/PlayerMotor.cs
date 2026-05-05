using UnityEngine;

namespace FPSGame.Player
{
    public class PlayerMotor : IMovable, IJumpable
    {
        private readonly PlayerConfig _config;
        private CharacterController _controller;
        private Vector3 _velocity;
        private bool _isGrounded;
        private float _coyoteTimeCounter;
        private float _jumpBufferCounter;
        private bool _isCrouching;
        private float _currentSpeed;

        public bool IsGrounded => _isGrounded;
        public float CurrentSpeed => _currentSpeed;
        public bool CanJump => _coyoteTimeCounter > 0f;

        public PlayerMotor(PlayerConfig config, CharacterController controller)
        {
            _config = config ?? throw new System.ArgumentNullException(nameof(config));
            _controller = controller ?? throw new System.ArgumentNullException(nameof(controller));
        }

        public void Move(Vector2 input)
        {
            Vector3 moveDirection = new Vector3(input.x, 0f, input.y);
            moveDirection = _controller.transform.TransformDirection(moveDirection);

            float speedMultiplier = 1f;
            if (_isCrouching) speedMultiplier = _config.CrouchSpeedMultiplier;
            else if (input.magnitude > 0.9f) speedMultiplier = _config.RunSpeedMultiplier;

            _currentSpeed = _config.WalkSpeed * speedMultiplier;
            _controller.Move(moveDirection * _currentSpeed * Time.fixedDeltaTime);
        }

        public void Jump()
        {
            if (CanJump)
            {
                _velocity.y = Mathf.Sqrt(_config.JumpForce * -2f * _config.Gravity);
                _coyoteTimeCounter = 0f;
                _jumpBufferCounter = 0f;
            }
        }

        public void UpdateMovement(bool jumpPressed)
        {
            _isGrounded = _controller.isGrounded;

            if (_isGrounded)
            {
                _coyoteTimeCounter = _config.CoyoteTime;
                if (_velocity.y < 0) _velocity.y = -2f;
            }
            else
            {
                _coyoteTimeCounter -= Time.fixedDeltaTime;
            }

            if (jumpPressed)
            {
                _jumpBufferCounter = _config.JumpBuffer;
            }
            else
            {
                _jumpBufferCounter -= Time.fixedDeltaTime;
            }

            if (_jumpBufferCounter > 0f && _coyoteTimeCounter > 0f)
            {
                Jump();
            }

            _velocity.y += _config.Gravity * Time.fixedDeltaTime;
            _controller.Move(_velocity * Time.fixedDeltaTime);

            if (_controller.transform.position.y < _config.FallLimitY)
            {
                UnityEngine.Debug.LogWarning("Player fell out of world");
            }
        }

        public void SetCrouch(bool crouching)
        {
            _isCrouching = crouching;
            _controller.height = crouching ? 1f : 2f;
            _controller.center = crouching ? new Vector3(0f, 0.5f, 0f) : new Vector3(0f, 1f, 0f);
        }
    }
}
