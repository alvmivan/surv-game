using UnityEngine;
using UnityEngine.InputSystem;

namespace FPSGame.Player
{
    public class PlayerPawn : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private PlayerConfig config;
        [SerializeField] private CharacterController characterController;
        [SerializeField] private Camera playerCamera;

        private PlayerMotor motor;
        private PlayerInputHandler inputHandler;
        private PlayerCameraController cameraController;
        private PlayerInput playerInput;

        public PlayerConfig Config => config;
        public CharacterController Controller => characterController;
        public Camera PlayerCamera => playerCamera;

        private void Awake()
        {
            if (characterController == null)
                characterController = GetComponent<CharacterController>();

            if (playerCamera == null)
                playerCamera = GetComponentInChildren<Camera>();

            motor = new PlayerMotor(config, characterController);
            inputHandler = new PlayerInputHandler();
            cameraController = GetComponentInChildren<PlayerCameraController>();
            playerInput = GetComponent<PlayerInput>();

            if (cameraController == null && playerCamera != null)
            {
                cameraController = playerCamera.gameObject.AddComponent<PlayerCameraController>();
            }
        }

        private void Update()
        {
            inputHandler.UpdateInput();

            if (playerInput != null)
            {
                var playerMap = playerInput.actions.FindActionMap("Player");
                if (playerMap != null)
                {
                    inputHandler.SetMoveInput(playerMap.FindAction("Move")?.ReadValue<Vector2>() ?? Vector2.zero);
                    inputHandler.SetLookInput(playerMap.FindAction("Look")?.ReadValue<Vector2>() ?? Vector2.zero);
                    inputHandler.SetJumpPressed(playerMap.FindAction("Jump")?.WasPressedThisFrame() ?? false);
                    inputHandler.SetRunHeld(playerMap.FindAction("Sprint")?.IsPressed() ?? false);
                    if (playerMap.FindAction("Crouch")?.WasPressedThisFrame() ?? false) inputHandler.ToggleCrouch();
                }
            }

            if (cameraController != null)
            {
                cameraController.HandleLook(inputHandler.LookInput);
            }
        }

        private void FixedUpdate()
        {
            motor.Move(inputHandler.MoveInput);
            motor.UpdateMovement(inputHandler.JumpPressed);

            if (inputHandler.CrouchToggled)
            {
                SetCrouch(!characterController.height.Equals(1f));
            }
        }

        public void SetCrouch(bool crouching)
        {
            motor.SetCrouch(crouching);
        }
    }
}
