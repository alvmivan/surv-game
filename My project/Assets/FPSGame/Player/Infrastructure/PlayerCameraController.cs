using UnityEngine;

namespace FPSGame.Player
{
    public class PlayerCameraController : MonoBehaviour
    {
        [Header("Settings")]
        [SerializeField] private float sensitivityX = 2f;
        [SerializeField] private float sensitivityY = 2f;
        [SerializeField] private float minXRotation = -90f;
        [SerializeField] private float maxXRotation = 90f;

        [Header("References")]
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private Transform playerTransform;

        private float _xRotation;

        private void Awake()
        {
            if (!cameraTransform  )
                cameraTransform = Camera.main?.transform;

            if (!playerTransform  )
                playerTransform = transform.parent;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }

        public void HandleLook(Vector2 lookInput)
        {
            float mouseX = lookInput.x * sensitivityX;
            float mouseY = lookInput.y * sensitivityY;

            _xRotation -= mouseY;
            _xRotation = Mathf.Clamp(_xRotation, minXRotation, maxXRotation);

            cameraTransform.localRotation = Quaternion.Euler(_xRotation, 0f, 0f);
            playerTransform.Rotate(Vector3.up * mouseX);
        }
    }
}
